using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// Interface for parameter settings view models
    /// that contain spatially variant data.
    /// </summary>
    /// <seealso cref="IParametersSettingsViewModel"/>
    public interface ISpatiallyVariantParametersSettingsViewModel : IParametersSettingsViewModel
    {
        /// <summary>
        /// Updates the currently selected parameters
        /// with the newly selected <paramref name="supportPoint"/>.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPoint"/> is <c>null</c>.
        /// </exception>
        void UpdateActiveSupportPoint(SupportPoint supportPoint);
    }
}