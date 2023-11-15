using DelftTools.Shell.Core.Workflow;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Hydro model is any model which simulates processes in the hydro region.
    /// </summary>
    public interface IHydroModel : IModel
    {
        /// <summary>
        /// Hydrographic region being simulated by this hydro model.
        /// </summary>
        IHydroRegion Region { get; }

        /// <summary>
        /// Interface for Coupling <see cref="IHydroModel"/>.
        /// </summary>
        IHydroCoupling HydroCoupling { get; }
    }
}