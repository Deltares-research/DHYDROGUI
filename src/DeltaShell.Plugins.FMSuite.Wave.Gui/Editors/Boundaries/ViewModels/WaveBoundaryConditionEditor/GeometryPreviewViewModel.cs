using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="GeometryPreviewViewModel"/> implements the view model for the geometry preview view.
    /// </summary>
    public class GeometryPreviewViewModel 
    {
        private readonly IWaveBoundary waveBoundary;
        private readonly IWaveBoundaryGeometryFactory geometryFactory;

        /// <summary>
        /// Creates a new <see cref="GeometryPreviewViewModel"/>.
        /// </summary>
        /// <param name="waveBoundary">The wave boundary.</param>
        /// <param name="geometryFactory">The geometry factory.</param>
        public GeometryPreviewViewModel(IWaveBoundary waveBoundary,
                                        IWaveBoundaryGeometryFactory geometryFactory)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            Ensure.NotNull(geometryFactory, nameof(geometryFactory));
            
            this.waveBoundary = waveBoundary;
            this.geometryFactory = geometryFactory;
            
            Feature.Geometry = BuildBoundaryGeometry();
        }

        /// <summary>
        /// Gets the feature displayed in this geometry preview.
        /// </summary>
        public IFeature Feature { get; } = new Feature2D();
        
        private IGeometry BuildBoundaryGeometry()
        {
            IEnumerable<IPoint> endPoints = geometryFactory.ConstructBoundaryEndPoints(waveBoundary);
            ILineString lineString = geometryFactory.ConstructBoundaryLineGeometry(waveBoundary);

            IEnumerable<IGeometry> geometries = endPoints.Concat<IGeometry>(new[] { lineString });

            var geometryCollection = new GeometryCollection(geometries.ToArray());
            return geometryCollection;
        }
    }
}