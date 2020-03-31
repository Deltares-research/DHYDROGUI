using System.Linq;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public static class MdwBoundaryGeometryPropertiesCreator
    {
        public static void AddNewProperties(DelftIniCategory boundaryCategory, IBoundaryContainer boundaryContainer,
                                            IEventedList<SupportPoint> supportPoints)
        {
            boundaryCategory.AddProperty(KnownWaveProperties.Definition, "xy-coordinates");

            SupportPoint[] sortedSupportPoints = supportPoints.OrderBy(sp => sp.Distance).ToArray();

            boundaryCategory.AddProperty(KnownWaveProperties.StartCoordinateX,
                                         boundaryContainer
                                             .GetBoundarySnappingCalculator()
                                             .CalculateCoordinateFromSupportPoint(sortedSupportPoints.First()).X);
            boundaryCategory.AddProperty(KnownWaveProperties.EndCoordinateX,
                                         boundaryContainer
                                             .GetBoundarySnappingCalculator()
                                             .CalculateCoordinateFromSupportPoint(sortedSupportPoints.Last()).X);
            boundaryCategory.AddProperty(KnownWaveProperties.StartCoordinateY,
                                         boundaryContainer
                                             .GetBoundarySnappingCalculator()
                                             .CalculateCoordinateFromSupportPoint(sortedSupportPoints.First()).Y);
            boundaryCategory.AddProperty(KnownWaveProperties.EndCoordinateY,
                                         boundaryContainer
                                             .GetBoundarySnappingCalculator()
                                             .CalculateCoordinateFromSupportPoint(sortedSupportPoints.Last()).Y);
        }
    }
}