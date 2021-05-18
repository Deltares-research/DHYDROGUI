using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.SpatialOperations;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// File reader for initialFields.ini.
    /// </summary>
    public static class InitialConditionInitialFieldsFileReader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InitialConditionInitialFieldsFileReader));

        /// <summary>
        /// Reads an initialFields.ini file.
        /// </summary>
        /// <param name="filePath">Path to the file to read.</param>
        /// <param name="modelDefinition">A <see cref="WaterFlowFMModelDefinition"/>.</param>
        /// <exception cref="FileReadingException">When an error occurs during reading of the file.</exception>
        /// <returns></returns>
        public static (InitialConditionQuantity, string) ReadFile(string filePath, WaterFlowFMModelDefinition modelDefinition)
        {
            if (!File.Exists(filePath)) throw new FileReadingException(string.Format(Properties.Resources.ReadFile_Could_not_read_file__0__properly__it_doesn_t_exist, filePath));

            var categories = new DelftIniReader().ReadDelftIniFile(filePath);
            if (categories.Count == 0) throw new FileReadingException(string.Format(Properties.Resources.ReadFile_Could_not_read_file__0__properly__it_seems_empty, filePath));
            
            ReadSpatialOperation(Path.GetDirectoryName(filePath), categories, ExtForceQuantNames.FrictCoef, WaterFlowFMModelDefinition.RoughnessDataItemName, modelDefinition);
            if (categories.Any(c => c.Name.Equals(InitialConditionRegion.InitialConditionIniHeader, StringComparison.InvariantCultureIgnoreCase) &&
                                    c.ReadProperty<string>(InitialConditionRegion.LocationType.Key, true, "all")
                                        .Equals("2d", StringComparison.InvariantCultureIgnoreCase) &&
                                    c.ReadProperty<string>(InitialConditionRegion.Quantity.Key)
                                        .Equals(ExtForceQuantNames.WaterLevel, StringComparison.InvariantCultureIgnoreCase)))
            {
                modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D, ((int)InitialConditionQuantity.WaterLevel).ToString());
                ReadSpatialOperation(Path.GetDirectoryName(filePath), categories, ExtForceQuantNames.WaterLevel, WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, modelDefinition);
            }
            else if (categories.Any(c => c.Name.Equals(InitialConditionRegion.InitialConditionIniHeader, StringComparison.InvariantCultureIgnoreCase) &&
                                    c.ReadProperty<string>(InitialConditionRegion.LocationType.Key, true, "all")
                                        .Equals("2d", StringComparison.InvariantCultureIgnoreCase) &&
                                    c.ReadProperty<string>(InitialConditionRegion.Quantity.Key)
                                        .Equals(ExtForceQuantNames.WaterDepth, StringComparison.InvariantCultureIgnoreCase)))
            {
                modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D, ((int)InitialConditionQuantity.WaterDepth).ToString());
                ReadSpatialOperation(Path.GetDirectoryName(filePath), categories, ExtForceQuantNames.WaterDepth, WaterFlowFMModelDefinition.InitialWaterDepthDataItemName, modelDefinition);
            }


            // [Initial 1d]
            

            var initialConditionCategories = categories.Where(category => category.Name.Equals(InitialConditionRegion.InitialConditionIniHeader, StringComparison.CurrentCultureIgnoreCase) &&
                                                                        (category.ReadProperty<string>(InitialConditionRegion.LocationType.Key, true, "all").Equals("1d", StringComparison.InvariantCultureIgnoreCase) ||
                                                                         category.ReadProperty<string>(InitialConditionRegion.DataFileType.Key).Equals("1dField", StringComparison.InvariantCultureIgnoreCase))).ToArray();
            
            if (initialConditionCategories.Length > 1)
            {
                Log.Warn(Properties.Resources.Initial_Condition_Warning_Only_one_quantity_type_is_currently_supported_reading_the_first_and_ignoring_all_others);
            }
            return initialConditionCategories.Any() 
                ? ReadInitialConditionCategory(modelDefinition, initialConditionCategories.First())
                : (InitialConditionQuantity.WaterLevel, "");

        }

        private static void ReadSpatialOperation(
            string filePath, 
            IList<DelftIniCategory> categories, 
            string quantity, 
            string dataItemName, 
            WaterFlowFMModelDefinition modelDefinition)
        {
            var parameterCategories = GetParameterCategoriesAndFilterByFrictionType(categories, modelDefinition);
            ReadSpatialOperationData(filePath, parameterCategories, modelDefinition, quantity, dataItemName);
        }

        private static void ReadSpatialOperationData(string filePath, IEnumerable<DelftIniCategory> parameterCategories,
            WaterFlowFMModelDefinition modelDefinition, string quantity, string dataItemName)
        {
            var parameterItems = parameterCategories
                .Where(c => (c.Name.Equals(InitialConditionRegion.ParameterIniHeader, StringComparison.InvariantCultureIgnoreCase) ||
                             c.Name.Equals(InitialConditionRegion.InitialConditionIniHeader, StringComparison.InvariantCultureIgnoreCase)) &&
                            c.ReadProperty<string>(InitialConditionRegion.Quantity.Key, true, string.Empty).Equals(quantity, StringComparison.InvariantCultureIgnoreCase) && 
                            c.ReadProperty<string>(InitialConditionRegion.LocationType.Key,true,"all" ).Equals("2d", StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (!parameterItems.Any()) return;

            var spatialOperations = modelDefinition.GetSpatialOperations(dataItemName);

            bool createOperationSet = spatialOperations == null;

            if (createOperationSet)
            {
                spatialOperations = new List<ISpatialOperation>();
                modelDefinition.SpatialOperations[dataItemName] = spatialOperations;
            }

            spatialOperations.Clear();
            foreach (var parameterItem in parameterItems)
            {
                var spatialOperation = CreateSpatialOperation(parameterItem, filePath);
                if (spatialOperation != null)
                {
                    spatialOperations.Add(spatialOperation);
                }
            }
        }

        private static ISpatialOperation CreateSpatialOperation(DelftIniCategory parameterItem, string path)
        {
            var dataFile = parameterItem.ReadProperty<string>(InitialConditionRegion.DataFile.Key);
            var dataFileType = parameterItem.ReadProperty<string>(InitialConditionRegion.DataFileType.Key).ToLower();
            switch (dataFileType)
            {
                case "geotiff":
                case "arcinfo":
                    return CreateSamplesOperation<ImportRasterSamplesSpatialOperationExtension>(parameterItem,Path.Combine(path, dataFile));
                case "sample":
                    return CreateSamplesOperation<ImportSamplesSpatialOperationExtension>(parameterItem,Path.Combine(path, dataFile));
                case "polygon":
                    return CreatePolygonOperation(parameterItem, Path.Combine(path, dataFile));
                default:
                    throw new ArgumentException(
                        string.Format("Cannot construct spatial operation for file {0} with file type {1}",
                            dataFile, dataFileType));
            }
        }
        

        private static ISpatialOperation CreatePolygonOperation(DelftIniCategory parameterItem, string polFile)
        {
            var features = new PolFile<Feature2DPolygon>().Read(polFile).Select(f => new Feature { Geometry = f.Geometry, Attributes = f.Attributes });

            var operationName = Path.GetFileNameWithoutExtension(polFile);
            var value = parameterItem.ReadProperty<double>(InitialConditionRegion.Value.Key);
            var operand = parameterItem.ReadProperty<string>(InitialConditionRegion.Operand.Key);
            var operation = new SetValueOperation
            {
                Value = value,
                OperationType = ExtForceQuantNames.ParseOperationType(operand),
                Name = operationName,
            };
            operation.Mask.Provider = new FeatureCollection(features.ToList(), typeof(Feature));

            return operation;
        }

        private static T CreateSamplesOperation<T>(DelftIniCategory parameterItem, string sampleFile) where T: ImportSamplesSpatialOperationExtension, new()
        {
            var operationName = Path.GetFileNameWithoutExtension(sampleFile);

            var operation = new T
            {
                Name = operationName,
                FilePath = sampleFile
            };

            var operand = parameterItem.ReadProperty<string>(InitialConditionRegion.Operand.Key, true);
            if (operand != null)
            {
                operation.Operand = ExtForceQuantNames.ParseOperationType(operand);
            }
            
            var averagingType = parameterItem.ReadProperty<string>(InitialConditionRegion.AveragingType.Key, true);
            if (averagingType != null)
            {
                operation.AveragingMethod = GetAveragingType(averagingType);
            }
            var relSearchCellSize = parameterItem.ReadProperty<double>(InitialConditionRegion.AveragingRelSize.Key, true,double.NaN);
            if (!double.IsNaN(relSearchCellSize))
            {
                operation.RelativeSearchCellSize = relSearchCellSize;
            }

            var minSamplePoints = parameterItem.ReadProperty<int>(InitialConditionRegion.AveragingNumMin.Key, true, 1);
            if (minSamplePoints >= 0)
            {
                operation.MinSamplePoints = minSamplePoints;
            }

            var interpolationMethod = parameterItem.ReadProperty<string>(InitialConditionRegion.InterpolationMethod.Key)?.ToLower();
            switch (interpolationMethod)
            {
                case "triangulation" :
                    operation.InterpolationMethod = SpatialInterpolationMethod.Triangulation;
                    break;
                case "averaging":
                    operation.InterpolationMethod = SpatialInterpolationMethod.Averaging;
                    break;
                default:
                    throw new Exception(string.Format("Invalid interpolation method {0} for file {1}",
                        interpolationMethod, Path.GetFileName(sampleFile)));
            }
            return operation;
        }

        private static GridCellAveragingMethod GetAveragingType(string averagingType)
        {
            switch (averagingType)
            {
                case "nearestNb": return GridCellAveragingMethod.ClosestPoint; //nearest neighbour value
                case "max": return GridCellAveragingMethod.MaximumValue; //highest
                case "min": return GridCellAveragingMethod.MinimumValue; //lowest
                case "invDist": return GridCellAveragingMethod.InverseWeightedDistance; //inverse-weighted distance average
                case "minAbs": return GridCellAveragingMethod.MinAbs; //smallest absolute value
                //case "median": return GridCellAveragingMethod.Median; //median value, does not exist yet
            }
            return GridCellAveragingMethod.SimpleAveraging;
        }

        private static IEnumerable<DelftIniCategory> GetParameterCategoriesAndFilterByFrictionType(IEnumerable<DelftIniCategory> categories, WaterFlowFMModelDefinition modelDefinition)
        {
            var frictionTypeProperty =
                modelDefinition.Properties.FirstOrDefault(
                    p => p.PropertyDefinition.MduPropertyName == KnownProperties.FrictionType);


            if(!int.TryParse(frictionTypeProperty?.GetValueAsString(), out var modelFrictionType))
                modelFrictionType = 1;

            foreach (var iniCategory in categories.Where(c => c.Name.Equals(InitialConditionRegion.ParameterIniHeader, StringComparison.InvariantCultureIgnoreCase) || 
                                                              c.Name.Equals(InitialConditionRegion.InitialConditionIniHeader, StringComparison.InvariantCultureIgnoreCase) &&
                                                              c.ReadProperty<string>(InitialConditionRegion.LocationType.Key,true,"all").Equals("2d", StringComparison.CurrentCultureIgnoreCase)))
            {
                var quantity = iniCategory.ReadProperty<string>(InitialConditionRegion.Quantity.Key);
                if (quantity != ExtForceQuantNames.FrictCoef)
                {
                    yield return iniCategory;
                    continue;
                }

                //we don't write this... how do i do this (the writing part)?
                var frictionType = iniCategory.ReadProperty<int>(ExtForceFile.FricTypeKey,true,defaultValue:modelFrictionType);

                if (frictionType != modelFrictionType)
                {
                    Log.WarnFormat(
                        "Ignoring roughness operation with friction {0} type unequal to uniform model friction type {1}",
                        frictionType, modelFrictionType);
                }
                else
                {
                    yield return iniCategory;
                }
            }
        }
        private static (InitialConditionQuantity, string) ReadInitialConditionCategory(WaterFlowFMModelDefinition modelDefinition, DelftIniCategory initialConditionCategory)
        {
            var quantityString = initialConditionCategory.ReadProperty<string>(InitialConditionRegion.Quantity.Key);
            var quantity = InitialConditionQuantityTypeConverter.ConvertStringToInitialConditionQuantity(quantityString);
            var dataFile = initialConditionCategory.ReadProperty<string>(InitialConditionRegion.DataFile.Key);

            return (quantity, dataFile);
        }
    }
}