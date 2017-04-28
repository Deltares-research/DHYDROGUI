using DelftTools.Hydro;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public interface IWaterFlowFMModel
    {
        UnstructuredGrid Grid { get; set; }
        IHydroNetwork Network { get; set; }
        bool UseNetCDFMapFormat { get; set; }
        bool DisableFlowNodeRenumbering { get; set; }
    }
}