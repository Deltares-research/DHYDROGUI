using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.FeatureProviders.Boundaries.Factories
{
    /// <summary>
    /// <see cref="WaveBoundaryGeometryFactory"/> implements the methods to construct
    /// geometry from <see cref="IWaveBoundary"/>.
    /// </summary>
    /// <seealso cref="IWaveBoundaryGeometryFactory"/>
    public sealed class WaveBoundaryGeometryFactory : IWaveBoundaryGeometryFactory
    {
        private readonly IGridBoundaryProvider gridBoundaryProvider;
        private readonly IBoundarySnappingCalculatorProvider snappingCalculatorProvider;

        /// <summary>
        /// Creates a new of the <see cref="WaveBoundaryGeometryFactory"/>.
        /// </summary>
        /// <param name="gridBoundaryProvider">The grid boundary provider.</param>
        /// <param name="snappingCalculatorProvider">The snapping calculator provider.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public WaveBoundaryGeometryFactory(IGridBoundaryProvider gridBoundaryProvider,
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
            int nElements = (geomDefinition.EndingIndex - geomDefinition.StartingIndex) + 1;

            IEnumerable<Coordinate> relevantCoordinates =
                gridBoundary[geomDefinition.GridSide]
                    .Skip(waveBoundary.GeometricDefinition.StartingIndex)
                    .Take(nElements)
                    .Select(x => gridBoundary.GetWorldCoordinateFromBoundaryCoordinate(x));

            return new LineString(relevantCoordinates.ToArray());
        }

        public IPoint ConstructBoundaryStartPoint(IWaveBoundary waveBoundary)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            return ConstructGridPoint(waveBoundary.GeometricDefinition.StartingIndex,
                                      waveBoundary.GeometricDefinition.GridSide);
        }

        public IPoint ConstructBoundaryEndPoint(IWaveBoundary waveBoundary)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            return ConstructGridPoint(waveBoundary.GeometricDefinition.EndingIndex,
                                      waveBoundary.GeometricDefinition.GridSide);
        }

        public IPoint ConstructBoundarySupportPoint(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));

            IBoundarySnappingCalculator calculator = snappingCalculatorProvider.GetBoundarySnappingCalculator();

            if (calculator == null)
            {
                return null;
            }

            Coordinate coordinate = calculator.CalculateCoordinateFromSupportPoint(supportPoint);

            return new Point(coordinate);
        }

        private IPoint ConstructGridPoint(int index, GridSide gridSide)
        {
            IGridBoundary gridBoundary = gridBoundaryProvider.GetGridBoundary();

            if (gridBoundary == null)
            {
                return null;
            }

            Coordinate coordinate = GetCoordinate(index, gridSide, gridBoundary);
            return new Point(coordinate);
        }

        private static Coordinate GetCoordinate(int index, GridSide side, IGridBoundary boundary)
        {
            GridBoundaryCoordinate gridBoundaryCoordinate = boundary[side].Skip(index).First();
            return boundary.GetWorldCoordinateFromBoundaryCoordinate(gridBoundaryCoordinate);
        }
    }
}