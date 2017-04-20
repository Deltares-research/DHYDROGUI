using System.Drawing;
using GeoAPI.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.FMSuite.Wave.Layers
{
    public static class WaveModelLayerStyles
    {
        public static readonly VectorStyle BoundaryStyle = new VectorStyle
            {
                Line = new Pen(Color.Blue, 3f),
                GeometryType = typeof (ILineString)
            };
    }
}