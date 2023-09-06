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
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.IniReaders;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.IniWriters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using SharpMap;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public static class SedimentFile
    {
        public const string GeneralHeader = "SedimentFileInformation";
        public const string OverallHeader = "SedimentOverall";
        public const string Header = "Sediment";

        public static readonly ConfigurationSetting
            Name = new ConfigurationSetting("Name", "Name of sediment fraction");

        public static readonly ConfigurationSetting SedimentType =
            new ConfigurationSetting("SedTyp", "Must be \"sand\", \"mud\" or \"bedload\"");

        public static readonly string FileCreatedBy = "FileCreatedBy";
        public static readonly string FileCreationDate = "FileCreationDate";
        public static readonly string FileVersion = "FileVersion";

        private static readonly ILog Log = LogManager.GetLogger(typeof(SedimentFile));
        private static SedMorIniWriter writer;

        private static readonly IList<string> knownSections = new List<string>
        {
            Header,
            GeneralHeader,
            OverallHeader
        };

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

        public static void Save(string sedFilePath, WaterFlowFMModelDefinition modelDefinition,
                                ISedimentModelData sedimentModelData)
        {
            try
            {
                var iniData = new IniData();
                iniData.AddMultipleSections(WriteHeaders(modelDefinition, sedimentModelData));

                WriteSpatiallyVaryingSedimentPropertySubFiles(sedimentModelData, sedFilePath);

                foreach (ISedimentFraction sedimentFraction in sedimentModelData.SedimentFractions)
                {
                    AddSedimentSectionToIniFile(modelDefinition, sedimentFraction, iniData);
                }

                iniData.AddMultipleSections(CreateUnknownSections(modelDefinition));

                Writer.WriteIniFile(iniData, sedFilePath);
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Could not write sediment file because : {0}", exception.Message);
            }
        }

        private static void AddSedimentSectionToIniFile(WaterFlowFMModelDefinition modelDefinition,
                                                        ISedimentFraction sedimentFraction,
                                                        IniData iniData)
        {
            var sedimentSection = new IniSection(Header);

            sedimentSection.AddSedimentProperty(
                Name.Key,
                string.Format($"#{sedimentFraction.Name}#"),
                string.Empty,
                Name.Description);

            sedimentSection.AddSedimentProperty(
                SedimentType.Key,
                sedimentFraction.CurrentSedimentType.Key,
                string.Empty,
                SedimentType.Description);

            AddSedimentTypeProperties(sedimentFraction, sedimentSection);
            AddFormulaTypeProperties(sedimentFraction, sedimentSection);

            /*Add custom properties to known ini section*/
            AddUnknownPropertiesToSection(modelDefinition, sedimentSection, sedimentFraction.Name);

            /*Add everything to the ini file*/
            iniData.AddSection(sedimentSection);
        }

        private static IEnumerable<IniSection> CreateUnknownSections(
            WaterFlowFMModelDefinition modelDefinition)
        {
            IEnumerable<WaterFlowFMProperty> customPropertiesOfCustomGroups =
                modelDefinition.Properties.Where(p => p.PropertyDefinition.UnknownPropertySource
                                                       .Equals(PropertySource.SedimentFile)
                                                      && !knownSections.Contains(p.PropertyDefinition.FileCategoryName));

            return MorphologySedimentIniFileHelper.CreateSectionsFromModelProperties(
                customPropertiesOfCustomGroups);
        }

        private static List<IniSection> WriteHeaders(WaterFlowFMModelDefinition modelDefinition,
                                                           ISedimentModelData sedimentModelData)
        {
            var sedimentSections = new List<IniSection> {MorphologySedimentIniFileHelper.CreateSedimentGeneralSection()};

            IniSection sedimentOverallSection =
                MorphologySedimentIniFileHelper.CreateSedimentOverallSection(
                    sedimentModelData.SedimentOverallProperties);
            AddUnknownPropertiesToSection(modelDefinition, sedimentOverallSection, OverallHeader);
            sedimentSections.Add(sedimentOverallSection);

            return sedimentSections;
        }

        private static void WriteSpatiallyVaryingSedimentPropertySubFiles(
            ISedimentModelData sedimentModelData, string sedFilePath)
        {
            SedimentModelDataItem sedimentDataItem = sedimentModelData.GetSedimentDataItem();

            sedimentDataItem.SpacialVariableNames = sedimentModelData
                                                    .SedimentFractions
                                                    .SelectMany(s => s.GetAllActiveSpatiallyVaryingPropertyNames())
                                                    .Where(n => !n.EndsWith("SedConc")).ToList();

            foreach (IGrouping<Type, UnstructuredGridCoverage> coverageGrouping in sedimentDataItem.Coverages)
            {
                Coordinate[] coordinates = null;

                foreach (UnstructuredGridCoverage coverage in coverageGrouping)
                {
                    if (coverage.IsTimeDependent)
                    {
                        throw new NotSupportedException(
                            "Converting time dependent spatial data to samples is not supported");
                    }

                    var component = coverage.Components[0] as IVariable<double>;
                    if (component == null)
                    {
                        throw new NotSupportedException(
                            "Converting a non-double valued coverage component to a point cloud is not supported");
                    }

                    IMultiDimensionalArray<double> values = component.Values;
                    var noDataValue = (double?) component.NoDataValue;

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
                            {
                                throw new InvalidOperationException(
                                    "Spatial data is not consistent: number of coordinate does not match number of values");
                            }
                        }

                        Coordinate coord = coordinates[i];
                        pointCloud.PointValues.Add(new PointValue
                        {
                            X = coord.X,
                            Y = coord.Y,
                            Value = v
                        });
                        i++;
                    }

                    if (pointCloud.PointValues.Count == 0)
                    {
                        continue;
                    }

                    var pointCloudFeatureProvider = new PointCloudFeatureProvider {PointCloud = pointCloud};

                    var newOperation = new AddSamplesOperation(false) {Name = coverage.Name};
                    newOperation.SetInputData(AddSamplesOperation.SamplesInputName, pointCloudFeatureProvider);

                    sedimentDataItem.SpatialOperation.Add(sedimentDataItem.DataItemNameLookup[coverage], new[]
                    {
                        newOperation
                    });
                }
            }

            foreach (KeyValuePair<string, IList<ISpatialOperation>> operations in sedimentDataItem.SpatialOperation)
            {
                foreach (ISpatialOperation spatialOperation in operations.Value)
                {
                    WriteXyzIfDirectoryExist(sedimentModelData, sedFilePath, spatialOperation, operations);
                }
            }

            IEnumerable<string> spatialOperationNames =
                sedimentDataItem.SpatialOperation.SelectMany(sp => sp.Value.Select(spv => spv.Name));
            foreach (string spaceVarName in sedimentDataItem.SpacialVariableNames.Where(
                spN => !spatialOperationNames.Contains(spN)))
            {
                //Give a warning for all those space varying properties which have NO operations.
                Log.WarnFormat(
                    Resources
                        .SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_No_spatial_operations_of_type_Import__Add_or_Value_found_for_spatially_varying_property__0___Remember_to_interpolate_them_to_generate_the_xyz_file__Otherwise_the_model_might_not_run_as_expected_,
                    spaceVarName);
            }
        }

        private static void WriteXyzIfDirectoryExist(ISedimentModelData sedimentModelData, string sedFilePath,
                                                     ISpatialOperation spatialOperation,
                                                     KeyValuePair<string, IList<ISpatialOperation>> operations)
        {
            var samplesOperation = spatialOperation as ImportSamplesSpatialOperation;
            if (samplesOperation != null)
            {
                WriteXYZIfDirectoryExists(sedimentModelData, sedFilePath, spatialOperation,
                                          samplesOperation.GetPoints());
                return;
            }

            var addSamplesOperation = spatialOperation as AddSamplesOperation;
            if (addSamplesOperation != null)
            {
                WriteXYZIfDirectoryExists(sedimentModelData, sedFilePath, spatialOperation,
                                          addSamplesOperation.GetPoints());
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

        private static void WriteXYZIfDirectoryExists(ISedimentModelData sedimentModelData, string sedPath,
                                                      ISpatialOperation spatialOperation,
                                                      IEnumerable<IPointValue> xyValuePoints)
        {
            /* If we don't point to the give sedPath then it won't be saved correctly 
             * when exporting (or cloning) as the MduPath parameter is the last thing to be changed.*/
            string directoryName = Path.GetDirectoryName(sedPath);
            if (directoryName != null)
            {
                string xyzFilePath = Path.Combine(directoryName,
                                                  spatialOperation.Name + "." + XyzFile.Extension);

                XyzFile.Write(xyzFilePath, xyValuePoints);
            }
            else
            {
                throw new ArgumentException(string.Format(
                                                Resources
                                                    .SedimentFile_WriteXYZIfDirectoryExists_Could_not_get_directory_name_from_file_path__0_,
                                                sedimentModelData.MduFilePath));
            }
        }

        private static void AddUnknownPropertiesToSection(WaterFlowFMModelDefinition modelDefinition,
                                                          IniSection section, string categoryName)
        {
            IEnumerable<WaterFlowFMProperty> properties =
                modelDefinition.Properties
                               .Where(p => IsUnknownSedimentPropertyForCategory(categoryName, p));

            foreach (WaterFlowFMProperty property in properties)
            {
                section.AddProperty(property.PropertyDefinition.FilePropertyName, property.GetValueAsString());
            }
        }

        private static bool IsUnknownSedimentPropertyForCategory(string category, WaterFlowFMProperty p)
        {
            return p.PropertyDefinition.FileCategoryName != "GUIOnly"
                   && p.PropertyDefinition.Category.Equals(category)
                   && p.PropertyDefinition.UnknownPropertySource.Equals(PropertySource.SedimentFile);
        }

        private static void AddUnknownSedimentProperty(IniProperty property,
                                                       WaterFlowFMModelDefinition definition, string categoryName,
                                                       ILogHandler logHandler,
                                                       string sedimentFractionName = null)
        {
            WaterFlowFMPropertyDefinition propertyDefinition =
                WaterFlowFMPropertyDefinitionCreator.CreateForCustomProperty(categoryName,
                                                                             property.Key,
                                                                             property.Comment,
                                                                             PropertySource.SedimentFile);

            propertyDefinition.Category = sedimentFractionName ?? categoryName;

            var newProperty = new WaterFlowFMProperty(propertyDefinition, property.Value);
            definition.AddProperty(newProperty);

            string propertyValue = property.Value;
            if (!string.IsNullOrEmpty(propertyValue))
            {
                newProperty.SetValueAsString(propertyValue);
            }

            logHandler.ReportWarningFormat(Resources.MorphologySediment_ReadCategoryProperties_Unsupported_keyword___0___at_line___1___detected_and_will_be_passed_to_the_computational_core__Note_that_some_data_or_the_connection_to_linked_files_may_be_lost_,
                                           property.Key, property.LineNumber);
        }

        private static void AddFormulaTypeProperties(ISedimentFraction sedimentFraction,
                                                     IniSection sedimentSection)
        {
            if (sedimentFraction.CurrentFormulaType == null)
            {
                return;
            }

            foreach (ISedimentProperty sedimentProperty in sedimentFraction.CurrentFormulaType.Properties)
            {
                sedimentProperty.SedimentPropertyWrite(sedimentSection);
            }
        }

        private static void AddSedimentTypeProperties(ISedimentFraction sedimentFraction,
                                                      IniSection sedimentSection)
        {
            foreach (ISedimentProperty sedimentProperty in sedimentFraction.CurrentSedimentType.Properties.Where(
                n => !n.Name.EndsWith("SedConc")))
            {
                sedimentProperty.SedimentPropertyWrite(sedimentSection);
            }
        }

        #endregion

        #region Read logic

        private static readonly Dictionary<string, Action<IniSection, string, WaterFlowFMModel>> SectionLoaders =
            new Dictionary
                <string, Action<IniSection, string, WaterFlowFMModel>>
                {
                    {Header, SedimentSectionLoader},
                    {OverallHeader, SedimentOverallSectionLoader}
                };

        private static void SedimentOverallSectionLoader(IniSection section, string path,
                                                         WaterFlowFMModel model)
        {
            foreach (ISedimentProperty sedimentProperty in model.SedimentOverallProperties)
            {
                sedimentProperty.SedimentPropertyLoad(section);
            }
        }

        private static void SedimentSectionLoader(IniSection section, string path, WaterFlowFMModel model)
        {
            string name = section.GetPropertyValueOrDefault(Name.Key);

            ValidationIssue validationIssue = WaterFlowFMSedimentMorphologyValidator.ValidateSedimentName(name);
            if (validationIssue != null && validationIssue.Severity == ValidationSeverity.Error)
            {
                throw new ArgumentNullException($"Sediment name {name} in sediment file {path} is invalid to deltashell");
            }

            var fraction = new SedimentFraction {Name = name};

            string sedimentTypeKey = section.GetPropertyValueOrDefault(SedimentType.Key);
            ISedimentType sedimentType =
                fraction.AvailableSedimentTypes.FirstOrDefault(st => st.Key == sedimentTypeKey);
            if (sedimentType == null)
            {
                throw new ArgumentNullException(
                    $"Sediment Type {sedimentTypeKey} in sediment file {path} is unknown to deltashell");
            }

            foreach (ISedimentProperty sedimentProperty in sedimentType.Properties)
            {
                /* Custom properties will not get loaded here.*/
                sedimentProperty.SedimentPropertyLoad(section);
                LoadSpatiallyVaryingOperationForProperty(sedimentProperty, model, path);
            }

            fraction.CurrentSedimentType = sedimentType;
            int traFrm;
            if (int.TryParse(section.GetPropertyValueOrDefault("TraFrm"), out traFrm))
            {
                ISedimentFormulaType sedimentFormula =
                    fraction.SupportedFormulaTypes.FirstOrDefault(ft => ft.TraFrm == traFrm);
                if (sedimentFormula != null)
                {
                    foreach (ISedimentProperty sedimentFormulaProperty in sedimentFormula.Properties)
                    {
                        /* Custom properties will not get loaded here.*/
                        sedimentFormulaProperty.SedimentPropertyLoad(section);
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
        private static void LoadSpatiallyVaryingOperationForProperty(ISedimentProperty property, WaterFlowFMModel model,
                                                                     string path)
        {
            var varyingProp = property as ISpatiallyVaryingSedimentProperty;
            if (varyingProp == null || !varyingProp.IsSpatiallyVarying)
            {
                return;
            }

            string dataItemName = varyingProp.SpatiallyVaryingName;
            if (dataItemName == null)
            {
                return;
            }

            string xyzFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(path, dataItemName + FileConstants.XyzFileExtension);
            if (!File.Exists(xyzFilePath))
            {
                return;
            }

            WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
            var operation = new ImportSamplesSpatialOperation
            {
                Name = dataItemName,
                FilePath = xyzFilePath,
                InterpolationMethod = SpatialInterpolationMethod.Triangulation //Type 5,
            };

            IList<ISpatialOperation> spatialOperations = modelDefinition.GetSpatialOperations(dataItemName);

            bool createOperationSet = spatialOperations == null;
            if (createOperationSet)
            {
                spatialOperations = new List<ISpatialOperation>();
                modelDefinition.SpatialOperations[dataItemName] = spatialOperations;
            }

            spatialOperations.Clear();
            spatialOperations.Add(operation);
        }

        /// <summary>
        /// Load the sediment data from the sed file at <paramref name="path"/> to the specified <paramref name="model"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="model">The model.</param>
        /// <param name="logHandler">An optional log handler.</param>
        /// <remarks>
        /// If no <paramref name="logHandler"/> is specified or if it specified as null, then a default
        /// log handler will be created and used.
        /// </remarks>
        public static void LoadSediments(string path, WaterFlowFMModel model, ILogHandler logHandler = null)
        {
            try
            {
                logHandler = logHandler ?? new LogHandler("reading the sediment file");

                WaterFlowFMModelDefinition definition = model.ModelDefinition;

                IniData iniData;
                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    iniData = new SedMorIniReader().ReadIniFile(fileStream, path);
                }

                foreach (IniSection section in iniData.Sections)
                {
                    string sectionName = section.Name;

                    if (sectionName == GeneralHeader)
                    {
                        continue;
                    }

                    /*Load parameters related to the model*/
                    if (SectionLoaders.TryGetValue(sectionName,
                                                   out Action<IniSection, string, WaterFlowFMModel> loader))
                    {
                        loader(section, path, model);
                    }

                    switch (sectionName)
                    {
                        case GeneralHeader:
                            continue;
                        case OverallHeader:
                            StoreUnknownPropertiesForOverallSection(model, section, definition, logHandler);
                            break;
                        case Header:
                            StoreUnknownPropertiesForSedimentFractionSection(model, section, definition, logHandler);
                            break;
                        default:
                            StoreUnknownPropertiesForUnknownSection(section, definition, logHandler);
                            break;
                    }
                }

                logHandler.LogReport();
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Could not read sediment file because : {0}", exception.Message);
            }
        }

        private static void StoreUnknownPropertiesForOverallSection(WaterFlowFMModel model,
                                                                    IniSection section,
                                                                    WaterFlowFMModelDefinition definition,
                                                                    ILogHandler logHandler)
        {
            if (model.SedimentOverallProperties == null)
            {
                return;
            }

            ISet<string> overallProps = new HashSet<string>(model.SedimentOverallProperties.Select(p => p.Name));

            foreach (IniProperty readProp in section.Properties)
            {
                if (!overallProps.Contains(readProp.Key))
                {
                    AddUnknownSedimentProperty(readProp, definition, OverallHeader, logHandler);
                }
            }
        }

        private static void StoreUnknownPropertiesForUnknownSection(IniSection section,
                                                                    WaterFlowFMModelDefinition definition,
                                                                    ILogHandler logHandler)
        {
            string sectionName = section.Name;

            foreach (IniProperty property in section.Properties)
            {
                AddUnknownSedimentProperty(property, definition, sectionName, logHandler);
            }
        }

        private static void StoreUnknownPropertiesForSedimentFractionSection(WaterFlowFMModel model,
                                                                             IniSection section,
                                                                             WaterFlowFMModelDefinition definition,
                                                                             ILogHandler logHandler)
        {
            IniProperty sedimentNameProperty = section.Properties.FirstOrDefault(p => p.Key.Equals(Name.Key));
            string sedimentFractionName = sedimentNameProperty?.Value;

            ISedimentFraction selectedSedimentFraction = sedimentFractionName != null
                                                             ? model.SedimentFractions.FirstOrDefault(
                                                                 sf => sf.Name.Equals(sedimentFractionName))
                                                             : null;

            if (selectedSedimentFraction == null)
            {
                return;
            }

            IEventedList<ISedimentProperty> allFTProps = selectedSedimentFraction.CurrentFormulaType != null
                                                             ? selectedSedimentFraction.CurrentFormulaType.Properties
                                                             : new EventedList<ISedimentProperty>();

            List<string> allsedimentPropertyNames = selectedSedimentFraction.CurrentSedimentType.Properties
                                                                            .Concat(allFTProps)
                                                                            .Select(p => p.Name)
                                                                            .ToList();

            allsedimentPropertyNames.Add(Name.Key);
            allsedimentPropertyNames.Add(SedimentType.Key);

            section.Properties
                    .Where(p => !allsedimentPropertyNames.Contains(p.Key))
                    .ForEach(p => AddUnknownSedimentProperty(p, definition, Header, logHandler, sedimentFractionName));
        }

        #endregion
    }
}