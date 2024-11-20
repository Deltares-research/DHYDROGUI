using System.Drawing;
using GeoAPI.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    public static class NetworkLayerFactory
    {
        public static VectorStyle CreatePointStyle(Bitmap bitmap)
        {
            return new VectorStyle
            {
                GeometryType = typeof (IPoint),
                Symbol = bitmap
            };
        }
    }
}