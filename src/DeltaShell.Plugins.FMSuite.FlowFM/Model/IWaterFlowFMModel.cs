using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public interface IWaterFlowFMModel : ITimeDependentModel
    {
        UnstructuredGrid Grid { get; set; }
        bool DisableFlowNodeRenumbering { get; set; }

        /// <summary>
        /// Gets the spatial data of this model.
        /// </summary>
        ISpatialData SpatialData { get; }

        WaterFlowFMModelDefinition ModelDefinition { get; }
    }
}