using System;
using DelftTools.Functions;
using DeltaShell.NGHS.IO.Store1D;
using GeoAPI.Extensions.Coverages;

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
