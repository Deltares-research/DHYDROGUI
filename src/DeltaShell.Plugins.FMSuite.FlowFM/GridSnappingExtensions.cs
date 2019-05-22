using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public static class GridSnappingExtensions
    {
        public static bool SnapsToFlowFmGrid(this IGeometry geometry, Envelope gridExtent)
        {
            if (gridExtent == null)
            {
                return true;
            }

            var extentsPlusMargin = new Envelope(0.9 * gridExtent.MinX, 1.1 * gridExtent.MaxX, 0.9 * gridExtent.MinY,
                                                 1.1 * gridExtent.MaxY);
            return extentsPlusMargin.Intersects(geometry.EnvelopeInternal);
        }
    }
}