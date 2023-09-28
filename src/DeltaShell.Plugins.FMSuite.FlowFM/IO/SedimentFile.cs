using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class SedimentFile
    {
        public const string GeneralHeader = "SedimentFileInformation";
        public const string OverallHeader = "SedimentOverall";
        public const string Header = "Sediment";
        internal const string SedimentUnknownProperty = "SedimentUnknownProperty";

        public static readonly ConfigurationSetting Name = new ConfigurationSetting(key: "Name", description: "Name of sediment fraction");
        public static readonly ConfigurationSetting SedimentType = new ConfigurationSetting(key: "SedTyp", description: "Must be \"sand\", \"mud\" or \"bedload\"");

        public static readonly string FileCreatedBy = "FileCreatedBy";
        public static readonly string FileCreationDate = "FileCreationDate";
        public static readonly string FileVersion = "FileVersion";

        private static readonly ILog Log = LogManager.GetLogger(typeof(SedimentFile));
        private static SedMorIniWriter writer;

        public static SedMorIniWriter Writer
        {
            get
            {
                if (writer == null)
                {
                    writer = new SedMorIniWriter();
                }

                return writer;
            }
        }

        #region Write logic

        public static void Save(string sedPath, WaterFlowFMModelDefinition modelDefinition, ISedimentModelData sedimentModelData)
        {
            try
            {
                var sedIniSections = WriteHeaders(modelDefinition, sedimentModelData);
                WriteSpatiallyVaryingSedimentPropertySubFiles(sedimentModelData, sedPath);

                foreach (var sedimentFraction in sedimentModelData.SedimentFractions)
                {
                    AddSedimentIniSectionToIniFile(modelDefinition, sedimentFraction, sedIniSections);
                }

                Writer.WriteIniFile(sedIniSections.ToList(), sedPath);
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Could not write sediment file because : {0}", exception.Message);
            }
        }

        private static void AddSedimentIniSectionToIniFile(WaterFlowFMModelDefinition modelDefinition,
            ISedimentFraction sedimentFraction, List<IniSection> sedIniSections)
        {
            var sedimentIniSection = new IniSection(Header);
            sedimentIniSection.AddSedimentProperty(Name.Key, string.Format("#{0}#", sedimentFraction.Name), string.Empty,
                Name.Description);
            sedimentIniSection.AddSedimentProperty(SedimentType.Key, sedimentFraction.CurrentSedimentType.Key, string.Empty,
                SedimentType.Description);

            AddSedimentTypeProperties(sedimentFraction, sedimentIniSection);
            AddFormulaTypeProperties(sedimentFraction, sedimentIniSection);

            /*Add custom properties*/
            AddPropertiesToIniSection(modelDefinition, sedimentIniSection, sedimentFraction.Name);

            /*Add everything to the ini file*/
            sedIniSections.Add(sedimentIniSection);
        }

        private static List<IniSection> WriteHeaders(WaterFlowFMModelDefinition modelDefinition, ISedimentModelData sedimentModelData)
        {
            var sedIniSections = new List<IniSection>()
            {
                MorphologySedimentIniFileGenerator.GenerateSedimentGeneralRegion()
            };

            var overalIniSection =
                MorphologySedimentIniFileGenerator.GenerateOverallRegion(sedimentModelData.SedimentOverallProperties);
            AddPropertiesToIniSection(modelDefinition, overalIniSection, OverallHeader);
            sedIniSections.Add(overalIniSection);
            return sedIniSections;
        }

        private static void WriteSpatiallyVaryingSedimentPropertySubFiles(ISedimentModelData sedimentModelData, string sedPath)
        {
            var sedimentDataItem = sedimentModelData.GetSedimentDataItem();

            sedimentDataItem.SpacialVariableNames = sedimentModelData.SedimentFractions.SelectMany(s => s.GetAllActiveSpatiallyVaryingPropertyNames()).Where( n => !n.EndsWith("SedConc")).ToList();

            foreach (var coverageGrouping in sedimentDataItem.Coverages)
            {
                Coordinate[] coordinates = null;

                foreach (var coverage in coverageGrouping)
                {
                    if (coverage.IsTimeDependent)
                        throw new NotSupportedException(
                            "Converting time dependent spatial data to samples is not supported");

                    var component = coverage.Components[0] as IVariable<double>;
                    if (component == null)
                    {
                        throw new NotSupportedException(
                            "Converting a non-double valued coverage component to a point cloud is not supported");
                    }

                    var values = component.Values;
                    double? noDataValue = (double?)component.NoDataValue;

                    var pointCloud = new PointCloud();
                    var i = 0;
                    foreach (double v in values) // using enumerable next is faster than using index (for loop)
                    {
                        if (noDataValue.HasValue && v == noDataValue.Value)
                        {
                            i++;
                            continue;
                        }

                        if (coordinates == null)
                        {
                            coordinates = coverage.Coordinates.ToArray();

                            if (coordinates.Length != values.Count)
                                throw new InvalidOperationException(
                                    "Spatial data is not consistent: number of coordinate does not match number of values");
                        }

                        var coord = coordinates[i];
                        pointCloud.PointValues.Add(new PointValue { X = coord.X, Y = coord.Y, Value = v });
                        i++;
                    }

                    if (pointCloud.PointValues.Count == 0)
                    {
                        continue;
                    }

                    var pointCloudFeatureProvider = new PointCloudFeatureProvider
                    {
                        PointCloud = pointCloud
                    };

                    var newOperation = new AddSamplesOperation(false) { Name = coverage.Name };
                    newOperation.SetInputData(AddSamplesOperation.SamplesInputName, pointCloudFeatureProvider);

                    sedimentDataItem.SpatialOperation.Add(sedimentDataItem.DataItemNameLookup[coverage], new[] { newOperation });
                }
            }

            foreach (var operations in sedimentDataItem.SpatialOperation)
            {
                foreach (var spatialOperation in operations.Value)
                {
                    WriteXyzIfDirectoryExist(sedimentModelData, sedPath, spatialOperation, operations);
                }
            }

            var spatialOperationNames = sedimentDataItem.SpatialOperation.SelectMany(sp => sp.Value.Select(spv => spv.Name));
            foreach (var spaceVarName in sedimentDataItem.SpacialVariableNames.Where( spN => ! spatialOperationNames.Contains(spN)))
            {
                //Give a warning for all those space varying properties which have NO operations.
                Log.WarnFormat(Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_No_spatial_operations_of_type_Import__Add_or_Value_found_for_spatially_varying_property__0___Remember_to_interpolate_them_to_generate_the_xyz_file__Otherwise_the_model_might_not_run_as_expected_, spaceVarName);
            }
        }

        private static void WriteXyzIfDirectoryExist(ISedimentModelData sedimentModelData, string sedPath,
            ISpatialOperation spatialOperation, KeyValuePair<string, IList<ISpatialOperation>> operations)
        {
            var samplesOperation = spatialOperation as ImportSamplesOperationImportData;
            if (samplesOperation != null)
            {
                WriteXYZIfDirectoryExists(sedimentModelData, sedPath, spatialOperation, samplesOperation.GetPoints());
                return;
            }

            var addSamplesOperation = spatialOperation as AddSamplesOperation;
            if (addSamplesOperation != null)
            {
                WriteXYZIfDirectoryExists(sedimentModelData, sedPath, spatialOperation, addSamplesOperation.GetPoints());
                return;
            }

            var valueOperation = spatialOperation as ValueOperationBase;
            if (valueOperation != null)
            {
                Log.WarnFormat(
                    Resources
                        .SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or,
                    operations.Key);
            }
        }

        private static void WriteXYZIfDirectoryExists(ISedimentModelData sedimentModelData, string sedPath, ISpatialOperation spatialOperation,
            IEnumerable<IPointValue> xyValuePoints)
        {
            /* If we don't point to the give sedPath then it won't be saved correctly 
             * when exporting (or cloning) as the MduPath parameter is the last thing to be changed.*/
            var directoryName = Path.GetDirectoryName(sedPath);
            if (directoryName != null)
            {
                var xyzFilePath = Path.Combine(directoryName,
                    spatialOperation.Name + "." + XyzFile.Extension);

                XyzFile.Write(xyzFilePath, xyValuePoints);
            }
            else
            {
                throw new ArgumentException(String.Format(Resources.SedimentFile_WriteXYZIfDirectoryExists_Could_not_get_directory_name_from_file_path__0_, sedimentModelData.MduFilePath));
            }
        }

        private static void AddPropertiesToIniSection(WaterFlowFMModelDefinition modelDefinition, IniSection overalCat, string category)
        {
            var ovProperties = modelDefinition.Properties
                .Where(p => p.PropertyDefinition.FileSectionName != "GUIOnly")
                .Where(p => p.PropertyDefinition.Category.Equals(category)
                            && p.PropertyDefinition.FileSectionName.Equals(SedimentUnknownProperty));

            foreach (var property in ovProperties)
            {
                overalCat.AddPropertyWithOptionalComment(property.PropertyDefinition.FilePropertyKey, property.GetValueAsString());
            }
        }

        private static void AddUnknownSedimentProperty(IniProperty readProp, WaterFlowFMModelDefinition definition, string category)
        {
            var propDef = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(SedimentUnknownProperty, readProp.Key, readProp.Comment);
            propDef.Category = category;
            var newSedProp = new WaterFlowFMProperty(propDef, readProp.Value);
            definition.AddProperty(newSedProp);

            if (!string.IsNullOrEmpty(readProp.Value))
            {
                newSedProp.SetValueAsString(readProp.Value);
            }
        }

        private static void AddFormulaTypeProperties(ISedimentFraction sedimentFraction, IniSection sedimentIniSection)
        {
            if (sedimentFraction.CurrentFormulaType == null) return;

            foreach (var sedimentProperty in sedimentFraction.CurrentFormulaType.Properties)
            {
                sedimentProperty.SedimentPropertyWrite(sedimentIniSection);
            }
        }

        private static void AddSedimentTypeProperties(ISedimentFraction sedimentFraction, IniSection sedimentIniSection)
        {
            foreach (var sedimentProperty in sedimentFraction.CurrentSedimentType.Properties.Where(n => !n.Name.EndsWith("SedConc")))
            {
                sedimentProperty.SedimentPropertyWrite(sedimentIniSection);
            }
        }

        #endregion

        #region Read logic

        private static readonly Dictionary<string, Action<IniSection, string, WaterFlowFMModel>> SectionLoaders = new Dictionary
            <string, Action<IniSection, string, WaterFlowFMModel>>
            {
                {Header, SedimentSectionLoader},
                {OverallHeader, SedimentOverallSectionLoader}
            };

        private static void SedimentOverallSectionLoader(IniSection iniSection, string path, WaterFlowFMModel model)
        {
            foreach (var sedimentProperty in model.SedimentOverallProperties)
            {
                sedimentProperty.SedimentPropertyLoad(iniSection);
            }
        }

        private static void SedimentSectionLoader(IniSection iniSection, string path, WaterFlowFMModel model)
        {
            var name = iniSection.GetPropertyValueWithOptionalDefaultValue(Name.Key);

            var validationIssue = WaterFlowFMSedimentMorphologyValidator.ValidateSedimentName(name);
            if (validationIssue!=null && validationIssue.Severity == ValidationSeverity.Error)
            {
                throw new ArgumentNullException(string.Format("Sediment name {0} in sediment file {1} is invalid to deltashell", name, path));
            }

            var fraction = new SedimentFraction()
            {
                Name = name
            };

            var sedimentTypeKey = iniSection.GetPropertyValueWithOptionalDefaultValue(SedimentType.Key);
            var sedimentType = fraction.AvailableSedimentTypes.FirstOrDefault(st => st.Key == sedimentTypeKey);
            if (sedimentType == null)
                throw new ArgumentNullException(string.Format("Sediment Type {0} in sediment file {1} is unknown to deltashell", sedimentTypeKey, path));
            foreach (var sedimentProperty in sedimentType.Properties)
            {
                /* Custom properties will not get loaded here.*/
                sedimentProperty.SedimentPropertyLoad(iniSection);
                LoadSpatiallyVaryingOperationForProperty(sedimentProperty, model, path);
            }
            fraction.CurrentSedimentType = sedimentType;
            int traFrm;
            if (int.TryParse(iniSection.GetPropertyValueWithOptionalDefaultValue("TraFrm"), out traFrm))
            {
                var sedimentFormula = fraction.SupportedFormulaTypes.FirstOrDefault(ft => ft.TraFrm == traFrm);
                if (sedimentFormula != null)
                {
                    foreach (var sedimentFormulaProperty in sedimentFormula.Properties)
                    {
                        /* Custom properties will not get loaded here.*/
                        sedimentFormulaProperty.SedimentPropertyLoad(iniSection);
                        LoadSpatiallyVaryingOperationForProperty(sedimentFormulaProperty, model, path);
                    }
                    fraction.CurrentFormulaType = sedimentFormula;
                }
            }
            
            model.SedimentFractions.Add(fraction);
        }

        /* DELFT3DFM-1112
         * This method is necessary specially for Import, 
         * as when loading a .dsproj the DB restores to the previous state.
         * But when importing there is no other way but to do it like this as
         * the ExtForceFile.cs only loads the SedConc spatially varying operations.
         */
        private static void LoadSpatiallyVaryingOperationForProperty(ISedimentProperty property, WaterFlowFMModel model, string path)
        {
            var varyingProp = property as ISpatiallyVaryingSedimentProperty;
            if (varyingProp == null || !varyingProp.IsSpatiallyVarying) return;

            var dataItemName = varyingProp.SpatiallyVaryingName;
            if (dataItemName == null) return;

            var xyzFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(path, dataItemName + ".xyz");
            if (!File.Exists(xyzFilePath)) return;

            var modelDefinition = model.ModelDefinition;
            var operation = new ImportSamplesOperationImportData
            {
                Name = dataItemName,
                FilePath = xyzFilePath,
                InterpolationMethod = SpatialInterpolationMethod.Triangulation //Type 5,
            };

            var spatialOperations = modelDefinition.GetSpatialOperations(dataItemName);

            bool createOperationSet = spatialOperations == null;
            if (createOperationSet)
            {
                spatialOperations = new List<ISpatialOperation>();
                modelDefinition.SpatialOperations[dataItemName] = spatialOperations;
            }

            spatialOperations.Clear();
            spatialOperations.Add(operation);
        }

        public static void LoadSediments(string path, WaterFlowFMModel model)
        {
            try
            {
                var definition = model.ModelDefinition;
                var sedIniSections = new SedMorIniReader().ReadIniFile(path);
                foreach (var iniSection in sedIniSections)
                {
                    Action<IniSection, string, WaterFlowFMModel> Loader;
                 
                    /*Load paramaters related to the model*/
                    if (SectionLoaders.TryGetValue(iniSection.Name, out Loader))
                    {
                        Loader(iniSection, path, model);
                    }

                    StoreUnknownParametersForOverallProperties(model, iniSection, definition);

                    StoreUnknownParamtersForSedimentFractions(model, iniSection, definition);
                }
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Could not read sediment file because : {0}", exception.Message);
            }
        }

        private static void StoreUnknownParametersForOverallProperties(WaterFlowFMModel model, IniSection iniSection,
            WaterFlowFMModelDefinition definition)
        {
            var overallProps = model.SedimentOverallProperties;
            if (iniSection.Name.Equals(OverallHeader) && overallProps != null)
            {
                foreach (var readProp in iniSection.Properties)
                {
                    if (!overallProps.Any(p => p.Name.Equals(readProp.Key)))
                    {
                        AddUnknownSedimentProperty(readProp, definition, OverallHeader);
                    }
                }
            }
        }

        private static void StoreUnknownParamtersForSedimentFractions(WaterFlowFMModel model, IniSection iniSection,
            WaterFlowFMModelDefinition definition)
        {
            if (!iniSection.Name.Equals(Header)) return;

            var sedimentProperty = iniSection.Properties.FirstOrDefault(p => p.Key.Equals(Name.Key));
            var selectedSedimentFraction = sedimentProperty != null
                ? model.SedimentFractions.FirstOrDefault(sf => sf.Name.Equals(sedimentProperty.Value))
                : null;

            if (selectedSedimentFraction == null) return;

            var allFTProps = selectedSedimentFraction.CurrentFormulaType != null
                ? selectedSedimentFraction.CurrentFormulaType.Properties
                : new EventedList<ISedimentProperty>();

            var allsedimentPropertyNames = selectedSedimentFraction.CurrentSedimentType.Properties
                .Concat(allFTProps)
                .Select(p => p.Name)
                .ToList();

            allsedimentPropertyNames.Add(Name.Key);
            allsedimentPropertyNames.Add(SedimentType.Key);

            iniSection.Properties
                .Where(p => !allsedimentPropertyNames.Contains(p.Key))
                .ForEach(p => AddUnknownSedimentProperty(p, definition, sedimentProperty.Value));
        }

        #endregion
    }
}