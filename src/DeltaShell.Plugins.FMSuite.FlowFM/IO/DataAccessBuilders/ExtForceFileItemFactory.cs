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
using SharpMap.Api.SpatialOperations;
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

        private static ExtForceFileItem GetExistingItem(ISpatialOperation spatialOperation, IDictionary<ExtForceFileItem, object> existingForceFileItems)
        {
            return existingForceFileItems.Where(item => Equals(item.Value, spatialOperation))
                                         .Select(item => item.Key)
                                         .FirstOrDefault();
        }
    }
}