using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
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

        #region Write logic

        public static void Save(string sedPath, WaterFlowFMModel model)
        {
            try
            {
                /*Writing Headers*/
                var sedCategories = new List<DelftIniCategory>()
                {
                    MorphologySedimentIniFileGenerator.GenerateSedimentGeneralRegion()
                };

                var overalCat = MorphologySedimentIniFileGenerator.GenerateOverallRegion(model.SedimentOverallProperties);
                AddPropertiesToCategory(model, overalCat, OverallHeader);
                sedCategories.Add(overalCat);

                WriteSpatiallyVaryingSedimentPropertySubFiles(model, sedPath);

                foreach (var sedimentFraction in model.SedimentFractions)
                {
                    var sedimentCategory = new DelftIniCategory(Header);
                    sedimentCategory.AddSedimentProperty(Name.Key, string.Format("#{0}#", sedimentFraction.Name), string.Empty, Name.Description);
                    sedimentCategory.AddSedimentProperty(SedimentType.Key, sedimentFraction.CurrentSedimentType.Key, string.Empty, SedimentType.Description);

                    AddSedimentTypeProperties(sedimentFraction, sedimentCategory);
                    AddFormulaTypeProperties(sedimentFraction, sedimentCategory);

                    /*Add custom properties*/
                    AddPropertiesToCategory(model, sedimentCategory, sedimentFraction.Name);
                    
                    /*Add everything to the ini file*/
                    sedCategories.Add(sedimentCategory);
                }

                var writer = new SedMorDelftIniWriter();
                writer.WriteDelftIniFile(sedCategories.ToList(), sedPath);
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Could not write sediment file because : {0}", exception.Message);
            }
        }

        private static void WriteSpatiallyVaryingSedimentPropertySubFiles(IWaterFlowFMModel model, string sedPath)
        {
            var spaceVarNames = model.SedimentFractions.SelectMany(s => s.GetAllActiveSpatiallyVaryingPropertyNames()).Where( n => !n.EndsWith("SedConc")).ToList();

            var dataItemsFound = spaceVarNames.SelectMany(spaceVarName => model.DataItems.Where(di => di.Name.Equals(spaceVarName))).ToArray();
            var dataItemsWithConverter = dataItemsFound.Where(d => d.ValueConverter is SpatialOperationSetValueConverter).ToList();
            var dataItemsWithOutConverter = dataItemsFound.Except(dataItemsWithConverter).ToList();
            var spatialOperations = GetSpatialOperations(dataItemsWithConverter);

            var coverageByType = dataItemsWithOutConverter.Select(di => di.Value)
                                    .OfType<UnstructuredGridCoverage>()
                                    .GroupBy(c => c.GetType())
                                    .ToList();
            var dataItemNameLookup = dataItemsWithOutConverter.ToDictionary(di => di.Value, di => di.Name);

            foreach (var coverageGrouping in coverageByType)
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

                    spatialOperations.Add(dataItemNameLookup[coverage], new[] { newOperation });
                }
            }

            foreach (var operations in spatialOperations)
            {
                foreach (var spatialOperation in operations.Value)
                {
                    var samplesOperation = spatialOperation as ImportSamplesSpatialOperationExtension;
                    if (samplesOperation != null)
                    {
                        WriteXYZIfDirectoryExists(model, sedPath, spatialOperation, samplesOperation.GetPoints());
                        continue;
                    }

                    var addSamplesOperation = spatialOperation as AddSamplesOperation;
                    if (addSamplesOperation != null)
                    {
                        WriteXYZIfDirectoryExists(model, sedPath, spatialOperation, addSamplesOperation.GetPoints());
                        continue;
                    }
                    
                    var valueOperation = spatialOperation as ValueOperationBase;
                    if (valueOperation != null)
                    {
                        Log.WarnFormat(Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_Cannot_create_xyz_file_for_spatial_varying_initial_condition__0__because_it_is_a_value_spatial_operation__please_interpolate_the_operation_to_the_grid_or, operations.Key);
                    }
                }
            }

            var spatialOperationNames = spatialOperations.SelectMany(sp => sp.Value.Select(spv => spv.Name));
            foreach (var spaceVarName in spaceVarNames.Where( spN => ! spatialOperationNames.Contains(spN)))
            {
                //Give a warning for all those space varying properties which have NO operations.
                Log.WarnFormat(Resources.SedimentFile_WriteSpatiallyVaryingSedimentPropertySubFiles_No_spatial_operations_of_type_Import__Add_or_Value_found_for_spatially_varying_property__0___Remember_to_interpolate_them_to_generate_the_xyz_file__Otherwise_the_model_might_not_run_as_expected_, spaceVarName);
            }
        }

        private static void WriteXYZIfDirectoryExists(IWaterFlowFMModel model, string sedPath, ISpatialOperation spatialOperation,
            IEnumerable<IPointValue> xyValuePoints)
        {
            /* If we don't point to the give sedPath then it won't be saved correctly 
             * when exporting (or cloning) as the MduPath parameter is the last thing to be changed.*/
            var directoryName = Path.GetDirectoryName(sedPath);
            if (directoryName != null)
            {
                var xyzFilePath = Path.Combine(directoryName,
                    spatialOperation.Name + "." + XyzFile.Extension);

                var newFile = new XyzFile();
                newFile.Write(xyzFilePath, xyValuePoints);
            }
            else
            {
                throw new ArgumentException(String.Format(Resources.SedimentFile_WriteXYZIfDirectoryExists_Could_not_get_directory_name_from_file_path__0_, model.MduFilePath));
            }
        }

        private static void AddPropertiesToCategory(WaterFlowFMModel model, DelftIniCategory overalCat, string category)
        {
            var ovProperties = model.ModelDefinition.Properties
                .Where(p => p.PropertyDefinition.FileCategoryName != "GUIOnly")
                .Where(p => p.PropertyDefinition.Category.Equals(category)
                            && p.PropertyDefinition.FileCategoryName.Equals(SedimentUnknownProperty));

            foreach (var property in ovProperties)
            {
                overalCat.AddProperty(property.PropertyDefinition.FilePropertyName, property.GetValueAsString());
            }
        }

        private static void AddUnknownSedimentProperty(DelftIniProperty readProp, WaterFlowFMModelDefinition definition, string category)
        {
            var propDef = WaterFlowFMProperty.CreatePropertyDefinitionForUnknownProperty(SedimentUnknownProperty, readProp.Name, readProp.Comment);
            propDef.Category = category;
            var newSedProp = new WaterFlowFMProperty(propDef, readProp.Value);
            definition.AddProperty(newSedProp);

            if (!string.IsNullOrEmpty(readProp.Value))
            {
                newSedProp.SetValueAsString(readProp.Value);
            }
        }

        private static void AddFormulaTypeProperties(ISedimentFraction sedimentFraction, IDelftIniCategory sedimentCategory)
        {
            if (sedimentFraction.CurrentFormulaType == null) return;

            foreach (var sedimentProperty in sedimentFraction.CurrentFormulaType.Properties)
            {
                sedimentProperty.SedimentPropertyWrite(sedimentCategory);
            }
        }

        private static void AddSedimentTypeProperties(ISedimentFraction sedimentFraction, IDelftIniCategory sedimentCategory)
        {
            foreach (var sedimentProperty in sedimentFraction.CurrentSedimentType.Properties.Where(n => !n.Name.EndsWith("SedConc")))
            {
                sedimentProperty.SedimentPropertyWrite(sedimentCategory);
            }
        }

        #endregion

        #region Read logic

        private static readonly Dictionary<string, Action<IDelftIniCategory, string, WaterFlowFMModel>> SectionLoaders = new Dictionary
            <string, Action<IDelftIniCategory, string, WaterFlowFMModel>>
            {
                {Header, SedimentSectionLoader},
                {OverallHeader, SedimentOverallSectionLoader}
            };

        private static void SedimentOverallSectionLoader(IDelftIniCategory category, string path, WaterFlowFMModel model)
        {
            foreach (var sedimentProperty in model.SedimentOverallProperties)
            {
                sedimentProperty.SedimentPropertyLoad(category);
            }
        }

        private static void SedimentSectionLoader(IDelftIniCategory category, string path, WaterFlowFMModel model)
        {
            var name = category.GetPropertyValue(Name.Key);

            var validationIssue = WaterFlowFMSedimentMorphologyValidator.ValidateSedimentName(name);
            if (validationIssue!=null && validationIssue.Severity == ValidationSeverity.Error)
            {
                throw new ArgumentNullException(string.Format("Sediment name {0} in sediment file {1} is invalid to deltashell", name, path));
            }

            var fraction = new SedimentFraction()
            {
                Name = name
            };

            var sedimentTypeKey = category.GetPropertyValue(SedimentType.Key);
            var sedimentType = fraction.AvailableSedimentTypes.FirstOrDefault(st => st.Key == sedimentTypeKey);
            if (sedimentType == null)
                throw new ArgumentNullException(string.Format("Sediment Type {0} in sediment file {1} is unknown to deltashell", sedimentTypeKey, path));
            foreach (var sedimentProperty in sedimentType.Properties)
            {
                /* Custom properties will not get loaded here.*/
                sedimentProperty.SedimentPropertyLoad(category);
                LoadSpatiallyVaryingOperationForProperty(sedimentProperty, model, path);
            }
            fraction.CurrentSedimentType = sedimentType;
            int traFrm;
            if (int.TryParse(category.GetPropertyValue("TraFrm"), out traFrm))
            {
                var sedimentFormula = fraction.SupportedFormulaTypes.FirstOrDefault(ft => ft.TraFrm == traFrm);
                if (sedimentFormula != null)
                {
                    foreach (var sedimentFormulaProperty in sedimentFormula.Properties)
                    {
                        /* Custom properties will not get loaded here.*/
                        sedimentFormulaProperty.SedimentPropertyLoad(category);
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
            var operation = new ImportSamplesSpatialOperationExtension
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
                var sedCategories = new SedMorDelftIniReader().ReadDelftIniFile(path);
                foreach (var category in sedCategories)
                {
                    Action<IDelftIniCategory, string, WaterFlowFMModel> Loader;
                    /*Load paramaters related to the model*/
                    if (SectionLoaders.TryGetValue(category.Name, out Loader))
                    {
                        Loader(category, path, model);
                    }

                    /*Store unknown parameters for the overall properties*/
                    var overallProps = model.SedimentOverallProperties;
                    if (category.Name.Equals(OverallHeader) && overallProps != null)
                    {
                        foreach ( var readProp in category.Properties)
                        {
                            if (!overallProps.Any(p => p.Name.Equals(readProp.Name)))
                            {
                                AddUnknownSedimentProperty(readProp, definition, OverallHeader);
                            }
                        }
                    }
                    
                    /*Only Store unknown parameters for the Sediment fractions*/
                    if (!category.Name.Equals(Header)) continue;
                    
                    var sedimentProperty = category.Properties.FirstOrDefault(p => p.Name.Equals(Name.Key));
                    var selectedSedimentFraction = sedimentProperty != null
                        ? model.SedimentFractions.FirstOrDefault(sf => sf.Name.Equals(sedimentProperty.Value))
                        : null;

                    if (selectedSedimentFraction == null) continue;

                    var allFTProps = selectedSedimentFraction.CurrentFormulaType != null
                        ? selectedSedimentFraction.CurrentFormulaType.Properties
                        : new EventedList<ISedimentProperty>();

                    var allsedimentPropertyNames = selectedSedimentFraction.CurrentSedimentType.Properties
                        .Concat(allFTProps)
                        .Select(p => p.Name)
                        .ToList();

                    allsedimentPropertyNames.Add(Name.Key);
                    allsedimentPropertyNames.Add(SedimentType.Key);

                    category.Properties
                        .Where(p => !allsedimentPropertyNames.Contains(p.Name))
                        .ForEach(p => AddUnknownSedimentProperty(p, definition, sedimentProperty.Value));                     
                }
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Could not read sediment file because : {0}", exception.Message);
            }
        }

        #endregion

        public static Dictionary<string, IList<ISpatialOperation>> GetSpatialOperations(List<IDataItem> dataItemsWithConverter)
        {
            var spatialOperations = new Dictionary<string, IList<ISpatialOperation>>();
            foreach (var dataItem in dataItemsWithConverter)
            {
                var spatialOperationValueConverter = (SpatialOperationSetValueConverter) dataItem.ValueConverter;
                if (
                    spatialOperationValueConverter.SpatialOperationSet.Operations.All(
                        WaterFlowFMModelDefinition.SupportedByExtForceFile))
                {
                    // put in everything except spatial operation sets,
                    // because we only use interpolate commands that will grab the importsamplesoperation via the input parameters.
                    var spatialOperation = spatialOperationValueConverter.SpatialOperationSet.GetOperationsRecursive()
                        .Where(s => !(s is ISpatialOperationSet))
                        .Select(WaterFlowFMModelDefinition.ConvertSpatialOperation)
                        .ToList();

                    //spatialOperations.AddRange(spatialOperation);
                    spatialOperations.Add(dataItem.Name, spatialOperation);
                }
                // null check to see if it has a final coverage. It could be that there are only point clouds in the set.
                else if (spatialOperationValueConverter.SpatialOperationSet.Output.Provider != null)
                {
                    // unsupported operations are converted to sample operations that are saved with an xyz file via the model definition.
                    var coverage =
                        spatialOperationValueConverter.SpatialOperationSet.Output.Provider.Features[0] as
                            UnstructuredGridCoverage;

                    // In the event that the coverage is comprised entirely of non-data values, ignore it and continue
                    // (This can happen when exporting spatial operations that comprise of added points but no interpolation
                    // - we're not interested in these for the mdu, they will be saved as dataitems to the dsproj)
                    if (coverage == null || (coverage.Components[0].NoDataValues != null &&
                                             coverage.GetValues<double>()
                                                 .All(v => coverage.Components[0].NoDataValues.Contains(v))))
                    {
                        continue;
                    }

                    var newOperation = new AddSamplesOperation(false)
                    {
                        Name = spatialOperationValueConverter.SpatialOperationSet.Name
                    };
                    newOperation.SetInputData(AddSamplesOperation.SamplesInputName,
                        new PointCloudFeatureProvider
                        {
                            PointCloud = coverage.ToPointCloud(0, true),
                        });

                    spatialOperations.Add(dataItem.Name, new[] { newOperation } );
                }
            }
            return spatialOperations;
        }
    }
}