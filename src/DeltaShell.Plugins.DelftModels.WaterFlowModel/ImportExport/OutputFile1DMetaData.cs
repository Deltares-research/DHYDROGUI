using DeltaShell.NGHS.IO.Store1D;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class WaterFlow1DTimeDependentVariableMetaData : TimeDependentVariableMetaDataBase
    {
        public AggregationOptions AggregationOption { get; set; }

        public WaterFlow1DTimeDependentVariableMetaData()
        {
            
        }
        public WaterFlow1DTimeDependentVariableMetaData(string name, string longName, string unit, AggregationOptions aggregationOption) : base(name, longName, unit)
        {
            AggregationOption = aggregationOption;
        }
    }

}
