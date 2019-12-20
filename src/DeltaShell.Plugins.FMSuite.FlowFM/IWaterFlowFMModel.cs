using System.Collections.Generic;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public interface IWaterFlowFMModel : ITimeDependentModel, IModelWithNetwork
    {
        UnstructuredGrid Grid { get; set; }
        WaterFlowFMModelDefinition ModelDefinition { get; }

        bool DisableFlowNodeRenumbering { get; set; }
        IEventedList<ISedimentProperty> SedimentOverallProperties { get; }
        IEventedList<ISedimentFraction> SedimentFractions { get; }
        string MduFilePath { get; }
        IEventedList<Model1DBoundaryNodeData> BoundaryConditions1D { get; }
        IEventedList<Model1DLateralSourceData> LateralSourcesData { get; }
    }
}