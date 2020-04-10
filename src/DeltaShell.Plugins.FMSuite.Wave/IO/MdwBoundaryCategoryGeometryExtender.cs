using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Calculators;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Static class containing static method to start extending the boundary
    /// category with geometry properties.
    /// </summary>
    public static class MdwBoundaryCategoryGeometryExtender
    {
        /// <summary>
        /// Static method for retrieving boundary geometry properties of each boundary
        /// and add them to the existing category.
        /// </summary>
        /// <param name="boundaryCategory"> The category that needs to be extended</param>
        /// <param name="boundaryContainer"> The boundary container of the model</param>
        /// <param name="supportPoints"> The support points in the geometric definition</param>
        public static void AddNewProperties(DelftIniCategory boundaryCategory, IBoundaryContainer boundaryContainer,
                                            IEnumerable<SupportPoint> supportPoints)
        {
            Ensure.NotNull(boundaryCategory, nameof(boundaryCategory));
            Ensure.NotNull(boundaryContainer, nameof(boundaryContainer));
            Ensure.NotNull(supportPoints, nameof(supportPoints));

            boundaryCategory.AddProperty(KnownWaveProperties.Definition, "xy-coordinates");

            SupportPoint[] sortedSupportPoints = supportPoints.OrderBy(sp => sp.Distance).ToArray();

            IBoundarySnappingCalculator calculator = boundaryContainer.GetBoundarySnappingCalculator();

            Coordinate startCoordinate = calculator.CalculateCoordinateFromSupportPoint(sortedSupportPoints.First());
            Coordinate endCoordinate = calculator.CalculateCoordinateFromSupportPoint(sortedSupportPoints.Last());

            boundaryCategory.AddProperty(KnownWaveProperties.StartCoordinateX, startCoordinate.X);
            boundaryCategory.AddProperty(KnownWaveProperties.EndCoordinateX, endCoordinate.X);
            boundaryCategory.AddProperty(KnownWaveProperties.StartCoordinateY, startCoordinate.Y);
            boundaryCategory.AddProperty(KnownWaveProperties.EndCoordinateY, endCoordinate.Y);
        }
    }
}