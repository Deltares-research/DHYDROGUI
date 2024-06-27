using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// Static class containing static method to start extending the boundary
    /// section with geometry properties.
    /// </summary>
    public static class MdwBoundarySectionGeometryExtender
    {
        /// <summary>
        /// Static method for retrieving boundary geometry properties of each boundary
        /// and add them to the existing section.
        /// </summary>
        /// <param name="boundarySection"> The section that needs to be extended</param>
        /// <param name="boundaryContainer"> The boundary container of the model</param>
        /// <param name="supportPoints"> The support points in the geometric definition</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="boundarySection"/>, <paramref name="boundaryContainer"/>
        /// or <paramref name="supportPoints"/> is <c>null</c>.
        /// </exception>
        public static void AddNewProperties(IniSection boundarySection, IBoundaryContainer boundaryContainer,
                                            IEnumerable<SupportPoint> supportPoints)
        {
            Ensure.NotNull(boundarySection, nameof(boundarySection));
            Ensure.NotNull(boundaryContainer, nameof(boundaryContainer));
            Ensure.NotNull(supportPoints, nameof(supportPoints));

            boundarySection.AddProperty(KnownWaveProperties.Definition, "xy-coordinates");

            SupportPoint[] sortedSupportPoints = supportPoints.OrderBy(sp => sp.Distance).ToArray();

            IBoundarySnappingCalculator calculator = boundaryContainer.GetBoundarySnappingCalculator();

            Coordinate startCoordinate = calculator.CalculateCoordinateFromSupportPoint(sortedSupportPoints.First());
            Coordinate endCoordinate = calculator.CalculateCoordinateFromSupportPoint(sortedSupportPoints.Last());

            boundarySection.AddSpatialProperty(KnownWaveProperties.StartCoordinateX, startCoordinate.X);
            boundarySection.AddSpatialProperty(KnownWaveProperties.EndCoordinateX, endCoordinate.X);
            boundarySection.AddSpatialProperty(KnownWaveProperties.StartCoordinateY, startCoordinate.Y);
            boundarySection.AddSpatialProperty(KnownWaveProperties.EndCoordinateY, endCoordinate.Y);
        }
    }
}