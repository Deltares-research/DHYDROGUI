using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessBuilders
{
    public static class ExtForceFileItemFactory
    {
        public static IDictionary<FlowBoundaryCondition, ExtForceFileItem> GetBoundaryConditionsItems(
            WaterFlowFMModelDefinition modelDefinition,
            IDictionary<IFeatureData, ExtForceFileItem> polyLineForceFileItems)
        {
            if (modelDefinition == null)
            {
                throw new ArgumentNullException(nameof(modelDefinition));
            }

            if (polyLineForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(polyLineForceFileItems));
            }

            var boundaryConditionsItems = new Dictionary<FlowBoundaryCondition, ExtForceFileItem>();

            foreach (BoundaryConditionSet boundaryConditionSet in
                modelDefinition.BoundaryConditionSets.Where(bcs => bcs.Feature.Name != null))
            {
                FlowBoundaryCondition[] flowBoundaryConditions =
                    boundaryConditionSet.BoundaryConditions.OfType<FlowBoundaryCondition>().ToArray();

                foreach (FlowBoundaryCondition flowBoundaryCondition in flowBoundaryConditions)
                {
                    if (!polyLineForceFileItems.TryGetValue(flowBoundaryCondition, out ExtForceFileItem matchingItem))
                    {
                        continue; //new boundary conditions shall be written by BndExtForceFile.
                    }

                    boundaryConditionsItems.Add(flowBoundaryCondition,
                                                GetFlowBoundaryConditionsItem(flowBoundaryCondition, matchingItem));
                }
            }

            return boundaryConditionsItems;
        }

        public static IDictionary<SourceAndSink, ExtForceFileItem> GetSourceAndSinkItems(
            WaterFlowFMModelDefinition modelDefinition,
            IDictionary<IFeatureData, ExtForceFileItem> polyLineForceFileItems)
        {
            if (modelDefinition == null)
            {
                throw new ArgumentNullException(nameof(modelDefinition));
            }

            if (polyLineForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(polyLineForceFileItems));
            }

            return modelDefinition.SourcesAndSinks.Where(ss => ss.Feature.Name != null).ToDictionary(
                sourceAndSink => sourceAndSink,
                sourceAndSink => GetSourceAndSinkItem(sourceAndSink, polyLineForceFileItems));
        }

        public static ExtForceFileItem GetInitialConditionsSamplesItem(
            ImportSamplesSpatialOperation spatialOperation, string extForceFileQuantityName, string prefix,
            IDictionary<ExtForceFileItem, object> existingForceFileItems, string targetDirectory)
        {
            if (spatialOperation == null)
            {
                throw new ArgumentNullException(nameof(spatialOperation));
            }

            if (existingForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(existingForceFileItems));
            }

            ExtForceFileItem existingItem = GetExistingItem(spatialOperation, existingForceFileItems);

            string quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;
            ExtForceFileItem extForceFileItem = existingItem ?? new ExtForceFileItem(quantityName)
            {
                FileName =
                    targetDirectory != null
                        ? spatialOperation.FilePath.Replace(targetDirectory + "\\", "")
                        : spatialOperation.FilePath,
                FileType = 7,
                Method = GetImportSamplesSpatialOperationMethod(spatialOperation)
            };
            if (spatialOperation.InterpolationMethod == SpatialInterpolationMethod.Averaging)
            {
                extForceFileItem.ModelData[ExtForceFileConstants.AveragingTypeKey] =
                    (int) spatialOperation.AveragingMethod;
                extForceFileItem.ModelData[ExtForceFileConstants.RelSearchCellSizeKey] =
                    spatialOperation.RelativeSearchCellSize;
            }

            extForceFileItem.Enabled = spatialOperation.Enabled;
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite];

            return extForceFileItem;
        }

        public static ExtForceFileItem GetInitialConditionsPolygonItem(SetValueOperation spatialOperation, string extForceFileQuantityName, string prefix,
                                                                       IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            if (spatialOperation == null)
            {
                throw new ArgumentNullException(nameof(spatialOperation));
            }

            if (existingForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(existingForceFileItems));
            }

            ExtForceFileItem existingItem = GetExistingItem(spatialOperation, existingForceFileItems);

            string quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;
            ExtForceFileItem extForceFileItem = existingItem ?? new ExtForceFileItem(quantityName)
            {
                FileName =
                    $"{extForceFileQuantityName}_{spatialOperation.Name.Replace(" ", "_").Replace("\t", "_")}{FileConstants.PolylineFileExtension}",
                FileType = ExtForceQuantNames.FileTypes.InsidePolygon,
                Method = 4
            };

            ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            Operator op = ExtForceQuantNames.OperatorMapping[spatialOperation.OperationType];

            extForceFileItem.Value = spatialOperation.Value;
            extForceFileItem.Enabled = spatialOperation.Enabled;
            extForceFileItem.Operand = ExtForceQuantNames.OperatorToStringMapping[op];

            return extForceFileItem;
        }

        public static ExtForceFileItem GetInitialConditionsUnsupportedItem(SampleSpatialOperation spatialOperation,
                                                                           string extForceFileQuantityName, string prefix)
        {
            if (spatialOperation == null)
            {
                throw new ArgumentNullException(nameof(spatialOperation));
            }
            
            string quantityName = prefix != null ? prefix + extForceFileQuantityName : extForceFileQuantityName;

            return new ExtForceFileItem(quantityName)
            {
                FileName = MakeXyzFileName(extForceFileQuantityName),
                FileType = ExtForceQuantNames.FileTypes.Triangulation,
                Method = 6,
                Enabled = spatialOperation.Enabled,
                Operand = ExtForceQuantNames.OperatorToStringMapping[Operator.Overwrite],
                ModelData =
                {
                    [ExtForceFileConstants.AveragingTypeKey] = (int) GridCellAveragingMethod.ClosestPoint,
                    [ExtForceFileConstants.RelSearchCellSizeKey] = 1.0
                },
            };
        }

        public static ExtForceFileItem GetWindFieldExtForceFileItem(IWindField windField, string fileName,
                                                                       IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            return GetExistingItem(windField, existingForceFileItems) ??
                   new ExtForceFileItem(ExtForceQuantNames.WindQuantityNames[windField.Quantity])
                   {
                       FileName = fileName,
                       FileType = GetFileType(windField),
                       Method = GetMethod(windField),
                       Operand = "+"
                   };
        }

        public static ExtForceFileItem GetHeatFluxModelItem(HeatFluxModel heatFluxModel, string modelName,
                                                            IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            if (heatFluxModel == null)
            {
                throw new ArgumentNullException(nameof(heatFluxModel));
            }

            if (existingForceFileItems == null)
            {
                throw new ArgumentNullException(nameof(existingForceFileItems));
            }

            HeatFluxModelType heatFluxModelType = heatFluxModel.Type;
            if (heatFluxModelType != HeatFluxModelType.Composite)
            {
                return null;
            }

            ExtForceFileItem item;
            if (heatFluxModel.GriddedHeatFluxFilePath != null)
            {
                item = GetExistingItem(heatFluxModelType, existingForceFileItems);
            }
            else
            {
                item = GetExistingItem(heatFluxModel.MeteoData, existingForceFileItems) ??
                       new ExtForceFileItem(
                           heatFluxModel.ContainsSolarRadiation
                               ? ExtForceQuantNames.MeteoDataWithRadiation
                               : ExtForceQuantNames.MeteoData)
                       {
                           FileName = modelName + FileConstants.MeteoFileExtension,
                           FileType = ExtForceQuantNames.FileTypes.Uniform,
                           Method = 1,
                           Operand = ExtForceQuantNames.OperatorToStringMapping[
                               Operator.Overwrite]
                       };
            }

            return item;
        }

        public static IDictionary<IUnsupportedFileBasedExtForceFileItem, ExtForceFileItem> GetUnknownQuantitiesItems(WaterFlowFMModelDefinition modelDefinition)
        {
            return modelDefinition.UnsupportedFileBasedExtForceFileItems
                                  .ToDictionary(i => i, i => i.UnsupportedExtForceFileItem);
        }

        private static ExtForceFileItem GetSourceAndSinkItem(SourceAndSink sourceAndSink,
                                                             IDictionary<IFeatureData, ExtForceFileItem> polyLineForceFileItems)
        {
            polyLineForceFileItems.TryGetValue(sourceAndSink, out ExtForceFileItem existingItem);

            ExtForceFileItem extForceFileItem = existingItem ?? new ExtForceFileItem(ExtForceQuantNames.SourceAndSink)
            {
                FileName = ExtForceFileHelper.GetPliFileName(sourceAndSink),
                FileType = ExtForceQuantNames.FileTypes.PolyTim,
                Method = 1,
                Operand = ExtForceQuantNames.OperatorToStringMapping[
                    Operator.Overwrite]
            };

            if (sourceAndSink.Area > 0)
            {
                extForceFileItem.ModelData[ExtForceFileConstants.AreaKey] = sourceAndSink.Area;
            }

            ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(extForceFileItem);

            return extForceFileItem;
        }

        private static ExtForceFileItem GetFlowBoundaryConditionsItem(FlowBoundaryCondition flowBoundaryCondition,
                                                                      ExtForceFileItem existingItem)
        {
            existingItem.Quantity = ExtForceQuantNames.GetQuantityString(flowBoundaryCondition);
            existingItem.Offset = Math.Abs(flowBoundaryCondition.Offset) < 1e-6 ? double.NaN : flowBoundaryCondition.Offset;
            existingItem.Factor = Math.Abs(flowBoundaryCondition.Factor - 1) < 1e-6 ? double.NaN : flowBoundaryCondition.Factor;

            ExtForceFileHelper.AddSuffixInCaseOfDuplicateFile(existingItem);

            return existingItem;
        }

        private static int GetImportSamplesSpatialOperationMethod(ImportSamplesSpatialOperation operation)
        {
            switch (operation.InterpolationMethod)
            {
                case SpatialInterpolationMethod.Triangulation:
                    return 5;
                case SpatialInterpolationMethod.Averaging:
                    return 6;
                default:
                    return -1;
            }
        }

        private static ExtForceFileItem GetExistingItem(object value, IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            return existingForceFileItems.Where(item => Equals(item.Value, value))
                                         .Select(item => item.Key)
                                         .FirstOrDefault();
        }

        private static string MakeXyzFileName(string quantity)
        {
            return string.Join(".", quantity.Replace(" ", "_").Replace("\t", "_"), ExtForceQuantNames.XyzFileExtension);
        }

        private static int GetFileType(IWindField windField)
        {
            if (windField is UniformWindField uniformWindField)
            {
                return uniformWindField.Components.Contains(WindComponent.Magnitude)
                           ? ExtForceQuantNames.FileTypes.UniMagDir
                           : ExtForceQuantNames.FileTypes.Uniform;
            }

            if (windField is GriddedWindField)
            {
                return windField.Quantity == WindQuantity.VelocityVectorAirPressure
                           ? ExtForceQuantNames.FileTypes.Curvi
                           : ExtForceQuantNames.FileTypes.ArcInfo;
            }

            if (windField is SpiderWebWindField)
            {
                return ExtForceQuantNames.FileTypes.SpiderWeb;
            }

            return -1;
        }

        private static int GetMethod(IWindField windField)
        {
            if (windField is UniformWindField)
            {
                return 1;
            }

            if (windField is GriddedWindField)
            {
                return windField.Quantity == WindQuantity.VelocityVectorAirPressure ? 3 : 2;
            }

            if (windField is SpiderWebWindField)
            {
                return 1;
            }

            return -1;
        }
    }
}