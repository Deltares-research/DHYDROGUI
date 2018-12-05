using System;
using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    public class WaterFlowModelOutputSetter : IWaterFlowModelCategoryPropertySetter
    {
        private Dictionary<string, ElementSet> headerMapping = new Dictionary<string, ElementSet>
        {
            [ModelDefinitionsRegion.ResultsNodesHeader] = ElementSet.GridpointsOnBranches,
            [ModelDefinitionsRegion.ResultsBranchesHeader] = ElementSet.ReachSegElmSet,
            [ModelDefinitionsRegion.ResultsStructuresHeader] = ElementSet.Structures,
            [ModelDefinitionsRegion.ResultsPumpsHeader] = ElementSet.Pumps,
            [ModelDefinitionsRegion.ResultsObservationsPointsHeader] = ElementSet.Observations,
            [ModelDefinitionsRegion.ResultsRetentionsHeader] = ElementSet.Retentions,
            [ModelDefinitionsRegion.ResultsLateralsHeader] = ElementSet.Laterals,
            [ModelDefinitionsRegion.ResultsWaterBalanceHeader] = ElementSet.ModelWide,
            [ModelDefinitionsRegion.FiniteVolumeGridOnGridPoints] = ElementSet.FiniteVolumeGridOnGridPoints
        };

        public void SetProperties(DelftIniCategory category, WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            // Determine element set
            ElementSet elementSet;
            if (!headerMapping.TryGetValue(category.Name, out elementSet)) return;

            foreach (var property in category.Properties)
            {
                var quantityType = GetQuantityType(property);
                if(quantityType == QuantityType.UndeterminedValue) continue;

                // Kernel expects Dispersion in ResultsBranchesHeader. GUI puts it
                // in ElementSet.GridpointsOnBranches, as such we need to 
                // explicitly correct this. 
                var engineParameter = model.OutputSettings.GetEngineParameter(quantityType,
                    quantityType == QuantityType.Dispersion && elementSet == ElementSet.ReachSegElmSet
                        ? ElementSet.GridpointsOnBranches
                        : elementSet,
                    DataItemRole.Output);

                if (engineParameter == null) continue;

                // determine AggregateOption
                AggregationOptions aggregateOption;
                try
                {
                    aggregateOption = (AggregationOptions)Enum.Parse(typeof(AggregationOptions), property.Value);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                engineParameter.AggregationOptions = aggregateOption;
            }
        }

        private static QuantityType GetQuantityType(IDelftIniProperty property)
        {
            QuantityType quantityType;
            try
            {
                quantityType = (QuantityType)Enum.Parse(typeof(QuantityType), property.Name);
            }
            catch (ArgumentException)
            {
                // Kernel expects the property Lateral1D2D. GUI defines this as
                // QTotal_1d2d. As such we need to explicitly test for this.
                quantityType = property.Name == "Lateral1D2D" 
                    ? QuantityType.QTotal_1d2d 
                    : QuantityType.UndeterminedValue;
            }

            return quantityType;
        }
    }
}
