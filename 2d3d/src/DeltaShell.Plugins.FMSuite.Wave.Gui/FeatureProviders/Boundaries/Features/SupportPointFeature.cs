using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Features
{
    /// <summary>
    /// <see cref="SupportPointFeature"/> describes a support point of a <see cref="IWaveBoundaryGeometricDefinition"/>
    /// as a <see cref="Feature2D"/>.
    /// </summary>
    public class SupportPointFeature : Feature2D
    {
        /// <summary>
        /// Gets or sets the geometry.
        /// </summary>
        public override IGeometry Geometry { get; set; }

        /// <summary>
        /// Gets or sets the observed support point.
        /// </summary>
        public SupportPoint ObservedSupportPoint { get; set; }
    }
}