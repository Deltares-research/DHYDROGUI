namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Helpers
{
    /// <summary>
    /// <see cref="IUniqueBoundaryNameProvider"/> defines a method to obtain a unique
    /// boundary name.
    /// </summary>
    public interface IUniqueBoundaryNameProvider
    {
        /// <summary>
        /// Gets an unique <see cref="Wave.Boundaries.IWaveBoundary"/> name.
        /// </summary>
        /// <returns>
        /// A unique <see cref="Wave.Boundaries.IWaveBoundary"/> name.
        /// </returns>
        string GetUniqueName();
    }
}