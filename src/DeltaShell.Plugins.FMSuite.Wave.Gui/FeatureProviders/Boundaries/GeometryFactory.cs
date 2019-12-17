using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries
{
    /// <summary>
    /// <see cref="GeometryFactory"/> implements the methods to construct
    /// geometry from <see cref="IWaveBoundary"/>.
    /// </summary>
    /// <seealso cref="IGeometryFactory"/>
    public class GeometryFactory : IGeometryFactory
    {
        private readonly IGridBoundaryProvider gridBoundaryProvider;

        /// <summary>
        /// Creates a new of the <see cref="GeometryFactory"/>.
        /// </summary>
        /// <param name="gridBoundaryProvider">The grid boundary provider.</param>
        /// <exception cref="ArgumentNullException">
        /// Throw when <paramref name="gridBoundaryProvider"/> is <c>null</c>.
        /// </exception>
        public GeometryFactory(IGridBoundaryProvider gridBoundaryProvider)
        {
            this.gridBoundaryProvider = gridBoundaryProvider ?? 
                                        throw new ArgumentNullException(nameof(gridBoundaryProvider));
        }

        public ILineString ConstructBoundaryLineGeometry(IWaveBoundary waveBoundary)
        {
            if (waveBoundary == null)
            {
                throw new ArgumentNullException(nameof(waveBoundary));
            }

            IGridBoundary gridBoundary = gridBoundaryProvider.GetGridBoundary();

            if (gridBoundary == null)
            {
                return null;
            }

            IWaveBoundaryGeometricDefinition geomDefinition = waveBoundary.GeometricDefinition;
            int nElements = geomDefinition.EndingIndex - geomDefinition.StartingIndex + 1;

            IEnumerable<Coordinate> relevantCoordinates = 
                gridBoundary[geomDefinition.GridSide]
                    .Skip(waveBoundary.GeometricDefinition.StartingIndex)
                    .Take(nElements)
                    .Select(x => gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(x));

            return new LineString(relevantCoordinates.ToArray());
        }
    }
}