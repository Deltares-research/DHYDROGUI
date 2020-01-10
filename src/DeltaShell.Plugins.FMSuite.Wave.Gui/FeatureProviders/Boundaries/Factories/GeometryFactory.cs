using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Common;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories
{
    /// <summary>
    /// <see cref="GeometryFactory"/> implements the methods to construct
    /// geometry from <see cref="IWaveBoundary"/>.
    /// </summary>
    /// <seealso cref="IGeometryFactory"/>
    public sealed class GeometryFactory : IGeometryFactory
    {
        private readonly IGridBoundaryProvider gridBoundaryProvider;
        private readonly IBoundarySnappingCalculatorProvider snappingCalculatorProvider;

        /// <summary>
        /// Creates a new of the <see cref="GeometryFactory"/>.
        /// </summary>
        /// <param name="gridBoundaryProvider">The grid boundary provider.</param>
        /// <param name="snappingCalculatorProvider">The snapping calculator provider.</param>
        /// <exception cref="ArgumentNullException">
        /// Throw when any parameter is <c>null</c>.
        /// </exception>
        public GeometryFactory(IGridBoundaryProvider gridBoundaryProvider,
                               IBoundarySnappingCalculatorProvider snappingCalculatorProvider)
        {
            Ensure.NotNull(gridBoundaryProvider, nameof(gridBoundaryProvider));
            Ensure.NotNull(snappingCalculatorProvider, nameof(snappingCalculatorProvider));

            this.gridBoundaryProvider = gridBoundaryProvider;
            this.snappingCalculatorProvider = snappingCalculatorProvider;
        }

        public ILineString ConstructBoundaryLineGeometry(IWaveBoundary waveBoundary)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));

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

        public IEnumerable<IPoint> ConstructBoundaryEndPoints(IWaveBoundary waveBoundary)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));

            IGridBoundary gridBoundary = gridBoundaryProvider.GetGridBoundary();

            if (gridBoundary == null)
            {
                return Enumerable.Empty<IPoint>();
            }

            Coordinate firstCoordinate = GetCoordinate(waveBoundary.GeometricDefinition.StartingIndex,
                                                       waveBoundary.GeometricDefinition.GridSide,
                                                       gridBoundary);

            Coordinate lastCoordinate = GetCoordinate(waveBoundary.GeometricDefinition.EndingIndex,
                                                      waveBoundary.GeometricDefinition.GridSide,
                                                      gridBoundary);

            return new[]
            {
                new Point(firstCoordinate),
                new Point(lastCoordinate)
            };
        }

        public IPoint ConstructBoundarySupportPoint(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));

            IBoundarySnappingCalculator calculator = snappingCalculatorProvider.GetBoundarySnappingCalculator();
            Coordinate coordinate = calculator.CalculateCoordinateFromSupportPoint(supportPoint);

            return new Point(coordinate);
        }

        private static Coordinate GetCoordinate(int index, GridSide side, IGridBoundary boundary)
        {
            // TODO: (MWT) Fix this ToArray
            GridBoundaryCoordinate gridBoundaryCoordinate = boundary[side].ToArray()[index];
            return boundary.GetWorldCoordinateFromBoundaryCoordinate(gridBoundaryCoordinate);
        }
    }
}