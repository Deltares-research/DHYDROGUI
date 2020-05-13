using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.DataAccessObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;

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

            return modelDefinition.SourcesAndSinks.Where(ss => ss.Feature.Name != null).
                                   ToDictionary(sourceAndSink => sourceAndSink, sourceAndSink => GetSourceAndSinkItem(sourceAndSink, polyLineForceFileItems));
        }

        private static ExtForceFileItem GetSourceAndSinkItem(SourceAndSink sourceAndSink, IDictionary<IFeatureData, ExtForceFileItem> polyLineForceFileItems)
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
                extForceFileItem.ModelData["AREA"] = sourceAndSink.Area;
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
    }
}