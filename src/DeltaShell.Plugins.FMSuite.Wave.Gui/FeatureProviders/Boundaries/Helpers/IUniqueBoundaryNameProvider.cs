using DeltaShell.Plugins.FMSuite.Wave.Boundaries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers
{
    /// <summary>
    /// <see cref="IUniqueBoundaryNameProvider"/> defines a method to obtain a unique
    /// boundary name.
    /// </summary>
    public interface IUniqueBoundaryNameProvider
    {
        /// <summary>
        /// Gets an unique <see cref="IWaveBoundary"/> name.
        /// </summary>
        /// <returns>
        /// A unique <see cref="IWaveBoundary"/> name.
        /// </returns>
        string GetUniqueName();
    }
}