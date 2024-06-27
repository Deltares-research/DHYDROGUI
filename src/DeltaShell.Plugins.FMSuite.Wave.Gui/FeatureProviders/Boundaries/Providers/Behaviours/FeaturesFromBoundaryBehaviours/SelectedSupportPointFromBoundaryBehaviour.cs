using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Providers.Behaviours.FeaturesFromBoundaryBehaviours
{
    /// <summary>
    /// <see cref="SelectedSupportPointFromBoundaryBehaviour"/> defines the
    /// behaviour to construct the selected support point feature.
    /// </summary>
    /// <seealso cref="IFeaturesFromBoundaryBehaviour"/>
    public sealed class SelectedSupportPointFromBoundaryBehaviour : IFeaturesFromBoundaryBehaviour
    {
        private readonly SupportPointDataComponentViewModel supportPointDataComponentViewModel;
        private readonly IWaveBoundaryGeometryFactory geometryFactory;

        /// <summary>
        /// Creates a new <see cref="SelectedSupportPointFromBoundaryBehaviour"/>.
        /// </summary>
        /// <param name="supportPointDataComponentViewModel">The support point data component view model.</param>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPointDataComponentViewModel"/> or
        /// <paramref name="geometryFactory"/> is <c>null</c>.
        /// </exception>
        public SelectedSupportPointFromBoundaryBehaviour(SupportPointDataComponentViewModel supportPointDataComponentViewModel,
                                                         IWaveBoundaryGeometryFactory geometryFactory)
        {
            Ensure.NotNull(supportPointDataComponentViewModel, nameof(supportPointDataComponentViewModel));
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));

            this.supportPointDataComponentViewModel = supportPointDataComponentViewModel;
            this.geometryFactory = geometryFactory;
        }

        /// <summary>
        /// Create a new selected support point from the <see cref="SupportPointEditorViewModel"/>
        /// provided at construction time.
        /// </summary>
        /// <param name="waveBoundary">The boundary.</param>
        /// <returns>
        /// A collection containing the selected support point, if the support
        /// points are enabled, and geometry could be constructed; else an empty
        /// collection.
        /// </returns>
        public IEnumerable<IFeature> Execute(IWaveBoundary waveBoundary)
        {
            if (!supportPointDataComponentViewModel.IsEnabled())
            {
                yield break;
            }

            IPoint geom = geometryFactory.ConstructBoundarySupportPoint(
                supportPointDataComponentViewModel.SelectedSupportPoint);

            if (geom == null)
            {
                yield break;
            }

            yield return new Feature2DPoint {Geometry = geom};
        }
    }
}