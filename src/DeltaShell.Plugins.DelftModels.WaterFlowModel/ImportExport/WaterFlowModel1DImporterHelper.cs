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

            if (waterFlowModel1D.DispersionFormulationType == DispersionFormulationType.Constant) return;

            // add new F3 and F4 coverages
            waterFlowModel1D.EnableThatcherHarlemanCoverages();

            // check if there is an F3 component in the F1 coverage
            var f3Component = dispersionCoverage.Components.FirstOrDefault(c => c.Name == "F3");
            if (f3Component == null) return;

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

            // remove F3 component from F1 coverage
            dispersionCoverage.Components.Remove(f3Component);
        }

        public static void AdaptExistingUseThatcherHarlemanPropertyToNewDispersionFormulationTypeProperty(WaterFlowModel1D waterFlowModel1D)
        {
            // retrieve legacy dataItem
            var legacyThatcherHarlemanDataItem = waterFlowModel1D.DataItems.FirstOrDefault(di => di.Tag == UseThatcherHarlemanTag);
            if (legacyThatcherHarlemanDataItem == null) return;

            var legacyValueParameter = legacyThatcherHarlemanDataItem.Value as Parameter<bool>;
            var legacyValue = legacyValueParameter != null && legacyValueParameter.Value;

            // calculate new value
            var newValue = legacyValue
                ? DispersionFormulationType.ThatcherHarleman.ToString()
                : DispersionFormulationType.Constant.ToString();

            // remove legacy dataitem
            waterFlowModel1D.DataItems.Remove(legacyThatcherHarlemanDataItem);

            var newDispersionFormulationDataItem = waterFlowModel1D.DataItems
                .FirstOrDefault(di => di.Tag == WaterFlowModel1DDataSet.DispersionFormulationTypeTag);

            // add or update new dataitem with new value
            if (newDispersionFormulationDataItem == null)
            {
                waterFlowModel1D.DataItems.Add(new DataItem(
                    new Parameter<string>(WaterFlowModel1DDataSet.DispersionFormulationTypeTag) { Value = newValue },
                    DataItemRole.Input,
                    WaterFlowModel1DDataSet.DispersionFormulationTypeTag));
            }
            else
            {
                newDispersionFormulationDataItem.Value = new Parameter<string>(WaterFlowModel1DDataSet.DispersionFormulationTypeTag) { Value = newValue };
            }
        }
    }
}