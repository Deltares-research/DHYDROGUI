using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public interface IWaterFlowFMModel : ITimeDependentModel
    {
        UnstructuredGrid Grid { get; set; }
        IHydroNetwork Network { get; set; }
        IDiscretization NetworkDiscretization { get; set; }
        bool DisableFlowNodeRenumbering { get; set; }
        IEventedList<ISedimentProperty> SedimentOverallProperties { get; }
        IEventedList<ISedimentFraction> SedimentFractions { get; }
        string MduFilePath { get; }
    }
}