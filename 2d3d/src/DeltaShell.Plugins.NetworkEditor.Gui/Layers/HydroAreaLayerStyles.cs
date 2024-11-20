using System.Drawing;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Layers
{
    public static class HydroAreaLayerStyles
    {
        public static VectorStyle LandBoundaryStyle =>
            new VectorStyle
            {
                Line = new Pen(Color.Black, 1f),
                GeometryType = typeof(ILineString)
            };

        public static VectorStyle ThinDamStyle =>
            new VectorStyle
            {
                Line = new Pen(Color.Red, 3f),
                GeometryType = typeof(ILineString)
            };

        public static VectorStyle FixedWeirStyle =>
            new VectorStyle
            {
                Line = new Pen(Color.Purple, 3f),
                GeometryType = typeof(ILineString)
            };

        public static VectorStyle ObservationPointStyle =>
            new VectorStyle
            {
                GeometryType = typeof(IPoint),
                Symbol = Resources.Observation
            };

        public static VectorStyle ObsCrossSectionStyle =>
            new VectorStyle
            {
                Line = new Pen(Color.DeepPink, 3f),
                GeometryType = typeof(ILineString)
            };

        public static VectorStyle PumpStyle =>
            new VectorStyle
            {
                Line = new Pen(Color.Aquamarine, 3f),
                GeometryType = typeof(ILineString)
            };

        public static VectorStyle WeirStyle =>
            new VectorStyle
            {
                Line = new Pen(Color.LightSteelBlue, 3f),
                GeometryType = typeof(ILineString)
            };

        public static VectorStyle BoundariesStyle =>
            new VectorStyle
            {
                Line = new Pen(Color.DarkBlue, 3f),
                GeometryType = typeof(ILineString)
            };

        public static VectorStyle BoundariesWaterLevelPointsStyle =>
            new VectorStyle
            {
                Line = new Pen(Color.LightBlue, 3f),
                Fill = new SolidBrush(Color.DarkBlue),
                GeometryType = typeof(IPoint),
                ShapeSize = 8
            };

        public static VectorStyle BoundariesVelocityPointsStyle =>
            new VectorStyle
            {
                Line = new Pen(Color.IndianRed, 3f),
                Fill = new SolidBrush(Color.Red),
                GeometryType = typeof(IPoint),
                ShapeSize = 8
            };

        public static VectorStyle DryPointStyle =>
            new VectorStyle
            {
                GeometryType = typeof(IPoint),
                Symbol = Resources.dry_point
            };

        public static VectorStyle DryAreaStyle =>
            new VectorStyle
            {
                GeometryType = typeof(IPolygon),
                Fill = new SolidBrush(Color.FromArgb(50, Color.SandyBrown)),
                Outline = new Pen(Color.FromArgb(100, Color.SaddleBrown), 2f)
            };

        public static VectorStyle SourcesAndSinksStyle =>
            new VectorStyle
            {
                Line = new Pen(Color.Tomato, 3f),
                GeometryType = typeof(ILineString),
                Symbol = Resources.LateralSourceMap
            };

        public static VectorStyle SnappedSourcesAndSinksStyle =>
            new VectorStyle
            {
                GeometryType = typeof(IMultiPoint),
                Fill = new SolidBrush(Color.Tomato),
                ShapeSize = 8
            };

        public static VectorStyle EnclosureStyle =>
            new VectorStyle
            {
                GeometryType = typeof(IPolygon),
                Fill = new SolidBrush(Color.Transparent),
                Outline = new Pen(Color.FromArgb(100, Color.CornflowerBlue), 2f)
            };

        public static VectorStyle BridgePillarStyle =>
            new VectorStyle
            {
                Line = new Pen(Color.LightSeaGreen, 3f),
                GeometryType = typeof(ILineString)
            };
    }
}