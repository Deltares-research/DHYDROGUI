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
        /// Pre-condition: model != null
        /// </remarks>
        public static void SetTimeProperties(IEnumerable<DelftIniCategory> modelSettingsCategories, WaterFlowModel1D model)
        {
            var timeCategory = modelSettingsCategories?.FirstOrDefault(c => c.Name == ModelDefinitionsRegion.TimeHeader);
            if (timeCategory == null) return;

            var startTime = timeCategory.ReadProperty<DateTime>(ModelDefinitionsRegion.StartTime.Key, true);
            var stopTime = timeCategory.ReadProperty<DateTime>(ModelDefinitionsRegion.StopTime.Key, true);
            var timeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.TimeStep.Key, true);
            var gridPointsOutputTimeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.OutTimeStepGridPoints.Key, true);
            var structuresOutputTimeStep = timeCategory.ReadProperty<double>(ModelDefinitionsRegion.OutTimeStepStructures.Key, true);

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
            var headerMapping = new Dictionary<string, ElementSet>()
            {
                [ModelDefinitionsRegion.ResultsNodesHeader] = ElementSet.GridpointsOnBranches,
                [ModelDefinitionsRegion.ResultsBranchesHeader] = ElementSet.ReachSegElmSet,
                [ModelDefinitionsRegion.ResultsStructuresHeader] = ElementSet.Structures,
                [ModelDefinitionsRegion.ResultsPumpsHeader] = ElementSet.Pumps,
                [ModelDefinitionsRegion.ResultsObservationsPointsHeader] = ElementSet.Observations,
                [ModelDefinitionsRegion.ResultsRetentionsHeader] = ElementSet.Retentions,
                [ModelDefinitionsRegion.ResultsLateralsHeader] = ElementSet.Laterals,
                [ModelDefinitionsRegion.ResultsWaterBalanceHeader] = ElementSet.ModelWide,
            };

            foreach (var cat in modelSettingsCategories)
            {
                // Determine element set
                ElementSet elementSet;
                if (!headerMapping.TryGetValue(cat.Name, out elementSet)) continue;

                foreach (var prop in cat.Properties)
                {
                    // Determine quantity type
                    QuantityType qType;
                    try
                    {
                        qType = (QuantityType) Enum.Parse(typeof(QuantityType), prop.Name);
                    }
                    catch (ArgumentException)
                    {
                        // Kernel expects the property Lateral1D2D. GUI defines this as
                        // QTotal_1d2d. As such we need to explicitly test for this.
                        if (prop.Name == "Lateral1D2D")
                            qType = QuantityType.QTotal_1d2d;
                        else
                            continue;
                    }

                    // Kernel expects Dispersion in ResultsBranchesHeader. GUI puts it
                    // in ElementSet.GridpointsOnBranches, as such we need to 
                    // explicitly correct this. 
                    var engineParameter = outputSettings.GetEngineParameter(qType,
                        qType == QuantityType.Dispersion && elementSet == ElementSet.ReachSegElmSet
                            ? ElementSet.GridpointsOnBranches
                            : elementSet,
                        DataItemRole.Output);

                    if (engineParameter == null) continue;

                    // determine AggregateOption
                    AggregationOptions aggregateOption;
                    try
                    {
                        aggregateOption = (AggregationOptions) Enum.Parse(typeof(AggregationOptions), prop.Value);
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                    
                    engineParameter.AggregationOptions = aggregateOption;
                }
            }
        }
    }
}
