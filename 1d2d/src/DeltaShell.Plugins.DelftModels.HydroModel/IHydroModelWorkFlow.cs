using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.DelftModels.HydroModel
{
    public interface IHydroModelWorkFlow : ICompositeActivity
    {
        /// <summary>
        /// Hydromodel to which the workflow belongs
        /// </summary>
        IHydroModel HydroModel { get; set; }

        /// <summary>
        /// Data of this workflow
        /// </summary>
        IHydroModelWorkFlowData Data { get; set; }
    }
}