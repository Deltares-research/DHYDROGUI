using System;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.Store1D;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class WaterFlowModel1DNetCdfFunctionStore : NetCdfFunctionStore1DBase<LocationMetaData, WaterFlow1DTimeDependentVariableMetaData>
    {
        public WaterFlowModel1DNetCdfFunctionStore()
        {
            OutputFileReader = new WaterFlowModel1DOutputFileReader();
        }
        #region IFunctionStore fields and Properties

        public override event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanged;

        #endregion

        #region IFileBased Properties
        public override object Clone()
        {
            var clonedStore = new WaterFlowModel1DNetCdfFunctionStore() { Path = this.Path, OutputFileReader = new WaterFlowModel1DOutputFileReader() };

            foreach (var existingNetworkCoverage in Functions.OfType<INetworkCoverage>())
            {
                var newNetworkCoverage = new NetworkCoverage(existingNetworkCoverage.Name, true)
                {
                    Network = existingNetworkCoverage.Network,
                    Store = clonedStore
                };

                clonedStore.Functions.AddRange(newNetworkCoverage.Arguments);
                clonedStore.Functions.AddRange(newNetworkCoverage.Components);
                clonedStore.Functions.Add(newNetworkCoverage);
            }

            foreach (var existingFeatureCoverage in Functions.OfType<IFeatureCoverage>())
            {
                var newFeatureCoverage = new FeatureCoverage(existingFeatureCoverage.Name)
                {
                    Features = existingFeatureCoverage.Features,
                    Store = clonedStore
                };

                clonedStore.Functions.AddRange(newFeatureCoverage.Arguments);
                clonedStore.Functions.AddRange(newFeatureCoverage.Components);
                clonedStore.Functions.Add(newFeatureCoverage);
            }

            foreach (var function in Functions.Where(f => !(f is ICoverage)))
            {
                var matchingFunction = (IVariable)Enumerable.FirstOrDefault<IFunction>(clonedStore.Functions, f => f.Name == function.Name
                                                                                                                   && f.GetType() == function.GetType());

                if (matchingFunction != null) matchingFunction.CopyFrom(function);
            }

            return clonedStore;
        }
        #endregion

        #region IFunctionStore method implementations

        #endregion

        #region IFileBased method implementations

        #endregion

        #region private GetValue helper methods

        #endregion

        #region private other helper methods

        #endregion

        protected override string GetNetCdfVariableName(ICoverage coverage)
        {
            return WaterFlowModel1DOutputCoverageMappings.GetMappingForCoverage(fileName, coverage.Name);
        }
    }
}
