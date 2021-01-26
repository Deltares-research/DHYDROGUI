using DelftTools.Shell.Core.Workflow;

namespace DelftTools.Hydro
{
    public interface IWorkDirectoryModel : IModel
    {
        string ExplicitWorkingDirectory { get; set; }
    }

    /// <summary>
    /// Hydro model is any model which simulates processes in the hydro region.
    /// </summary>
    public interface IHydroModel : IModel
    {
        /// <summary>
        /// Hydrographic region being simulated by this hydro model.
        /// </summary>
        IHydroRegion Region { get; }

        bool FileBasedModelIsLoaded { get; }
    }
}