using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Units.Generics;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public static class WaterFlowModel1DImporterHelper
    {
        private const string DispersionF1Component = "Dispersion F1 coefficient";
        private const string DispersionF3Component = "Dispersion F3 coefficient";
        private const string UseThatcherHarlemanTag = "usethatcherharleman";
        private const string PreviousDischargeAtLateralOutputDataItemTag = "Discharge (l)";

        public static void RemovePreviousVersionOfDischargeAtLateralsCoverage(WaterFlowModel1D waterFlowModel1D)
        {
            if (waterFlowModel1D == null) return;

            // SOBEK3-115: Output for Laterals has now changed from 'Discharge' to 'Actual Discharge', 'Defined Discharge', and 'Lateral Difference'
            var previousDischargeAtLateralOutputDataItem = waterFlowModel1D.DataItems
                .FirstOrDefault(di => di.Role == DataItemRole.Output && di.Tag == PreviousDischargeAtLateralOutputDataItemTag);

            if (previousDischargeAtLateralOutputDataItem != null)
                waterFlowModel1D.DataItems.Remove(previousDischargeAtLateralOutputDataItem);
        }

        public static void AdaptExistingDispersionCoverageToNewDispersionCoverages(WaterFlowModel1D waterFlowModel1D)
        {
            // get existing dispersion coverage
            var dispersionCoverageDataItem = waterFlowModel1D.DataItems.FirstOrDefault(di => di.Tag == WaterFlowModel1DDataSet.InputDispersionCoverageTag);
            if (dispersionCoverageDataItem == null) return;

            var dispersionCoverage = dispersionCoverageDataItem.Value as INetworkCoverage;
            if (dispersionCoverage == null) return; // should not happen

            // update coverage name
            dispersionCoverage.Name = DispersionF1Component;

            // update f1 component name
            var f1Component = dispersionCoverage.Components.FirstOrDefault(c => c.Name == "Dispersion Coefficient" || c.Name == "F1");
            if (f1Component != null) f1Component.Name = DispersionF1Component;

            // check if there is an F3 component in the F1 coverage
            var f3Component = dispersionCoverage.Components.FirstOrDefault(c => c.Name == "F3");
            if (f3Component == null) return;

            // remove F3 component from F1 coverage
            dispersionCoverage.Components.Remove(f3Component);

            if (waterFlowModel1D.DispersionFormulationType == DispersionFormulationType.Constant) return;

            // add new F3 and F4 coverages
            waterFlowModel1D.EnableThatcherHarlemanCoverages();

            // get existing F3 coverage
            var dispersionF3CoverageDataItem = waterFlowModel1D.DataItems.FirstOrDefault(di => di.Tag == WaterFlowModel1DDataSet.InputDispersionF3CoverageTag);
            if (dispersionF3CoverageDataItem == null) return; // should not happen

            var dispersionF3Coverage = dispersionF3CoverageDataItem.Value as INetworkCoverage;
            if (dispersionF3Coverage == null) return; // should not happen

            // update Argument and Component values
            foreach (var argument in dispersionCoverage.Arguments)
            {
                var matchingArgument = dispersionF3Coverage.Arguments
                    .FirstOrDefault(a => a.ValueType == argument.ValueType && a.Name == argument.Name);

                if (matchingArgument == null) continue;

                matchingArgument.Values = (IMultiDimensionalArray)argument.Values.Clone();
            }

            var matchingComponent = dispersionF3Coverage.Components
                .FirstOrDefault(c => c.ValueType == f3Component.ValueType && c.Name == DispersionF3Component);

            if (matchingComponent != null)
            {
                matchingComponent.Values = (IMultiDimensionalArray)f3Component.Values.Clone();
            }
        }

        public static void AdaptExistingUseThatcherHarlemanPropertyToNewDispersionFormulationTypeProperty(WaterFlowModel1D waterFlowModel1D)
        {
            // retrieve legacy dataItem
            var legacyThatcherHarlemanDataItem = waterFlowModel1D.DataItems.FirstOrDefault(di => di.Tag == UseThatcherHarlemanTag);
            if (legacyThatcherHarlemanDataItem == null) return;

            // calculate new value
            var newValue = DispersionFormulationType.Constant.ToString();

            // remove legacy dataitem
            waterFlowModel1D.DataItems.Remove(legacyThatcherHarlemanDataItem);

            var parameter = new Parameter<string>(WaterFlowModel1DDataSet.DispersionFormulationTypeTag) { Value = newValue };

            var newDispersionFormulationDataItem = waterFlowModel1D.GetDataItemByTag(WaterFlowModel1DDataSet.DispersionFormulationTypeTag);
            if (newDispersionFormulationDataItem != null)
            {
                // update DataItem value
                newDispersionFormulationDataItem.Value = parameter;
                return;
            }

            waterFlowModel1D.DataItems.Add(new DataItem(parameter, DataItemRole.Input, WaterFlowModel1DDataSet.DispersionFormulationTypeTag));
        }
    }
}