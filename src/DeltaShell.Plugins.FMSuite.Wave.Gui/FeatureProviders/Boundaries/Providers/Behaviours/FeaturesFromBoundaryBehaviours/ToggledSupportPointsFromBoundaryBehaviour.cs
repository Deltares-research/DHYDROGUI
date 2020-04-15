using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
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
    public class ToggledSupportPointsFromBoundaryBehaviour : IFeaturesFromBoundaryBehaviour
    {
        private readonly bool shouldHaveBoundaryConditionData;
        private readonly SupportPointDataComponentViewModel supportPointDataComponentViewModel;
        private readonly IWaveBoundaryGeometryFactory geometryFactory;

        /// <summary>
        /// Creates a new <see cref="ToggledSupportPointsFromBoundaryBehaviour"/>.
        /// </summary>
        /// <param name="shouldHaveBoundaryConditionData">if set to <c>true</c> [should have boundary condition data].</param>
        /// <param name="supportPointDataComponentViewModel">The support point data component view model.</param>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPointDataComponentViewModel"/> or
        /// <paramref name="geometryFactory"/> is <c>null</c>.
        /// </exception>
        public ToggledSupportPointsFromBoundaryBehaviour(bool shouldHaveBoundaryConditionData,
                                                         SupportPointDataComponentViewModel supportPointDataComponentViewModel,
                                                         IWaveBoundaryGeometryFactory geometryFactory)
        {
            Ensure.NotNull(supportPointDataComponentViewModel, nameof(supportPointDataComponentViewModel));
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));

            this.shouldHaveBoundaryConditionData = shouldHaveBoundaryConditionData;
            this.supportPointDataComponentViewModel = supportPointDataComponentViewModel;
            this.geometryFactory = geometryFactory;
        }

        /// <summary>
        /// Create a collection of new support points that either have or do not
        /// have data associated with them, as set by the shouldHaveBoundaryConditionData
        /// flag in the constructor.
        /// </summary>
        /// <param name="boundary">The boundary.</param>
        /// <returns>
        /// A collection containing the active or inactive support points, if the support
        /// points are enabled, and geometry could be constructed; else an empty
        /// collection.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundary"/> is <c>null</c>.
        /// </exception>
        public IEnumerable<IFeature> Execute(IWaveBoundary boundary)
        {
            Ensure.NotNull(boundary, nameof(boundary));

            if (!supportPointDataComponentViewModel.IsEnabled())
            {
                return Enumerable.Empty<IFeature>();
            }

            return boundary.GeometricDefinition.SupportPoints
                           .Select(ConstructGeometry)
                           .Where(x => x != null);
        }

        private IFeature ConstructGeometry(SupportPoint sp)
        {
            if (supportPointDataComponentViewModel.IsEnabledSupportPoint(sp) != shouldHaveBoundaryConditionData)
            {
                return null;
            }

            IPoint p = geometryFactory.ConstructBoundarySupportPoint(sp);
            return p != null ? new Feature2DPoint { Geometry = p } : null;
        }
    }
}