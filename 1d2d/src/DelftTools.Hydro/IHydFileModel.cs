using DelftTools.Shell.Core.Workflow;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Model that produces a Hyd file (containing hydrodynamic data)
    /// </summary>
    public interface IHydFileModel : IModel
    {
        /// <summary>
        /// Path to the produced hyd file
        /// </summary>
        string HydFilePath { get; }

        /// <summary>
        /// Enables hyd file output to be created
        /// </summary>
        bool HydFileOutput { get; set; }
    }
}