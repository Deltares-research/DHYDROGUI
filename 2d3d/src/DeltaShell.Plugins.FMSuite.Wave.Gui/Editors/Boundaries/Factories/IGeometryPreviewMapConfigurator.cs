using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories
{
    /// <summary>
    /// <see cref="IGeometryPreviewMapConfigurator"/> defines the interface
    /// with which to configure the <see cref="IMap"/> of a geometry preview.
    /// </summary>
    public interface IGeometryPreviewMapConfigurator
    {
        /// <summary>
        /// Configures the provided <paramref name="map"/> to display the GeometryPreview layers.
        /// </summary>
        /// <param name="map">The map to configure.</param>
        /// <param name="boundaryProvider">The boundary provider.</param>
        /// <param name="supportPointDataComponentViewModel">The support point data component view model.</param>
        /// <param name="refreshGeometryView">The map refresh interface.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        void ConfigureMap(IMap map,
                          IBoundaryProvider boundaryProvider,
                          SupportPointDataComponentViewModel supportPointDataComponentViewModel,
                          IRefreshGeometryView refreshGeometryView);
    }
}