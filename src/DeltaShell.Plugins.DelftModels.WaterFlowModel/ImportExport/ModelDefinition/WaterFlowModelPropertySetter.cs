using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelPropertySetter provides the methods to set different
    /// model wide aspects based upon a data access model of the md1d file.
    /// </summary>
    public static class WaterFlowModelPropertySetter
    {
        /// <summary>
        /// Set the model time properties as specified in the ModelDefinitionsRegion.TimeHeader
        /// of the <paramref name="modelSettingsCategories"/>
        /// </summary>
        /// <param name="modelSettingsCategories"> A set of DelftIniCategories describing a md1d file. </param>
        /// <param name="model"> The model whose time variables should be changed. </param>
        /// <remarks>
        /// Pre-condition: modelSettingsCategories != null && model != null
        /// Pre-condition: ModelDefinitionsRegion.TimeHeader In modelSettingsCategories
        /// </remarks>
        public static void SetTimeProperties(IEnumerable<DelftIniCategory> modelSettingsCategories, WaterFlowModel1D model)
        {
            var timeCategory = modelSettingsCategories?.FirstOrDefault(c => c.Name == ModelDefinitionsRegion.TimeHeader);
            if (timeCategory == null) return;

            var startTime = timeCategory.ReadProperty<DateTime>(ModelDefinitionsRegion.StartTime.Key);
            var stopTime = timeCategory.ReadProperty<DateTime>(ModelDefinitionsRegion.StopTime.Key);
            var timeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.TimeStep.Key);
            var gridPointsOutputTimeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.OutTimeStepGridPoints.Key);
            var structuresOutputTimeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.OutTimeStepStructures.Key);

            model.StartTime = startTime;
            model.StopTime = stopTime;
            model.TimeStep = TimeSpan.FromSeconds(timeStep);

            var modelOutputSettings = model.OutputSettings;
            modelOutputSettings.GridOutputTimeStep = TimeSpan.FromSeconds(gridPointsOutputTimeStep);
            modelOutputSettings.StructureOutputTimeStep = TimeSpan.FromSeconds(structuresOutputTimeStep);
        }

        /// <summary>
        /// Set the WaterFlowModel1DOutputSettingData engine parameters to the
        /// values specified in <paramref name="modelSettingsCategories"/>.
        /// </summary>
        /// <param name="modelSettingsCategories">A set of DelftIniCategories describing a md1d file. </param>
        /// <param name="outputSettings"> reference to WaterFlowModel1DOutputSettingData of some WaterFlow1DModel.</param>
        /// <remarks>
        /// Pre-condition: modelSettingsCategories != null && outputSettings != null
        /// </remarks>
        public static void SetOutputProperties(IEnumerable<DelftIniCategory> modelSettingsCategories,
                                                                             WaterFlowModel1DOutputSettingData outputSettings)
        {
            foreach (var cat in modelSettingsCategories)
            {
                // Determine element set
                ElementSet elementSet;
                switch (cat.Name)
                {
                    case ModelDefinitionsRegion.ResultsNodesHeader:
                        elementSet = ElementSet.GridpointsOnBranches;
                        break;
                    case ModelDefinitionsRegion.ResultsBranchesHeader:
                        elementSet = ElementSet.ReachSegElmSet;
                        break;
                    case ModelDefinitionsRegion.ResultsStructuresHeader:
                        elementSet = ElementSet.Structures;
                        break;
                    case ModelDefinitionsRegion.ResultsPumpsHeader:
                        elementSet = ElementSet.Pumps;
                        break;
                    case ModelDefinitionsRegion.ResultsWaterBalanceHeader:
                        elementSet = ElementSet.ModelWide;
                        break;
                    case ModelDefinitionsRegion.ResultsObservationsPointsHeader:
                        elementSet = ElementSet.Observations;
                        break;
                    case ModelDefinitionsRegion.ResultsLateralsHeader:
                        elementSet = ElementSet.Laterals;
                        break;
                    case ModelDefinitionsRegion.ResultsRetentionsHeader:
                        elementSet = ElementSet.Retentions;
                        break;
                    default:
                        continue;
                }

                foreach (var prop in cat.Properties)
                {
                    // Determine quantity type
                    QuantityType qType;
                    try
                    {
                        qType = (QuantityType) Enum.Parse(typeof(QuantityType), prop.Name);
                    }
                    catch (ArgumentException _)
                    {
                        continue;
                    }

                    var engineParameter = outputSettings.GetEngineParameter(qType, elementSet, DataItemRole.Output);

                    if (engineParameter == null) continue;

                    // determine AggregateOption
                    AggregationOptions aggregateOption;
                    try
                    {
                        aggregateOption = (AggregationOptions) Enum.Parse(typeof(AggregationOptions), prop.Value);
                    }
                    catch (ArgumentException _)
                    {
                        continue;
                    }
                    engineParameter.AggregationOptions = aggregateOption;
                }
            }
        }
    }
}
