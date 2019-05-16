using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.WaterFlowFMModel
{
    public interface IWaterFlowFMModel : ITimeDependentModel
    {
        UnstructuredGrid Grid { get; set; }
        bool DisableFlowNodeRenumbering { get; set; }
        WaterFlowFMModelDefinition ModelDefinition { get; }
    }
}