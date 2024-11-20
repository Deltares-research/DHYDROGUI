using System.Drawing;
using System.Drawing.Drawing2D;
using GeoAPI.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.MapLayers
{
    public static class AreaLayerStyles
    {
        public static VectorStyle LandBoundaryStyle
        {
            get
            {
                return new VectorStyle
                           {
                               Line = new Pen(Color.Black, 1f),
                               GeometryType = typeof (ILineString)
                           };
            }
        }

        public static VectorStyle ThinDamStyle
        {
            get
            {
                return new VectorStyle
                           {
                               Line = new Pen(Color.Red, 3f),
                               GeometryType = typeof (ILineString)
                           };
            }
        }

        public static VectorStyle FixedWeirStyle
        {
            get
            {
                return new VectorStyle
                           {
                               Line = new Pen(Color.Purple, 3f),
                               GeometryType = typeof (ILineString)
                           };
            }
        }

        public static VectorStyle ObservationPointStyle
        {
            get
            {
                return new VectorStyle
                           {
                               GeometryType = typeof (IPoint),
                               Symbol = Properties.Resources.Observation
                           };
            }
        }

        public static VectorStyle ObsCrossSectionStyle
        {
            get
            {
                return new VectorStyle
                           {
                               Line = new Pen(Color.DeepPink, 3f),
                               GeometryType = typeof (ILineString)
                           };
            }
        }

        public static VectorStyle PumpStyle
        {
            get
            {
                return new VectorStyle
                           {
                               Line = new Pen(Color.Aquamarine, 3f),
                               GeometryType = typeof (ILineString)
                           };
            }
        }

        public static VectorStyle WeirStyle
        {
            get
            {
                return new VectorStyle
                           {
                               Line = new Pen(Color.LightSteelBlue, 3f),
                               GeometryType = typeof (ILineString)
                           };
            }
        }

        public static VectorStyle GateStyle
        {
            get
            {
                return new VectorStyle
                           {
                               Line = new Pen(Color.SteelBlue, 3f),
                               GeometryType = typeof (ILineString)
                           };
            }
        }

        public static VectorStyle BoundariesStyle
        {
            get
            {
                return new VectorStyle
                           {
                               Line = new Pen(Color.DarkBlue, 3f),
                               GeometryType = typeof (ILineString),
                           };
            }
        }

        public static VectorStyle BoundariesWaterLevelPointsStyle
        {
            get
            {
                return new VectorStyle
                    {
                        Line = new Pen(Color.LightBlue, 3f),
                        Fill = new SolidBrush(Color.DarkBlue),
                        GeometryType = typeof (IPoint),
                        ShapeSize = 8,
                    };
            }
        }

        public static VectorStyle BoundariesVelocityPointsStyle
        {
            get
            {
                return new VectorStyle
                    {
                        Line = new Pen(Color.IndianRed, 3f),
                        Fill = new SolidBrush(Color.Red),
                        GeometryType = typeof (IPoint),
                        ShapeSize = 8,
                    };
            }
        }

        public static VectorStyle DryPointStyle
        {
            get
            {
                return new VectorStyle
                {
                    GeometryType = typeof (IPoint),
                    Symbol = Properties.Resources.dry_point
                };
            }
        }

        public static VectorStyle DryAreaStyle
        {
            get
            {
                return new VectorStyle
                {
                    GeometryType = typeof (IPolygon),
                    Fill = new SolidBrush(Color.FromArgb(50, Color.SandyBrown)),
                    Outline = new Pen(Color.FromArgb(100, Color.SaddleBrown), 2f),
                };
            }
        }

        public static VectorStyle SourcesAndSinksStyle
        {
            get
            {
                return new VectorStyle
                {
                    Line = new Pen(Color.Tomato, 3f),
                    GeometryType = typeof(ILineString),
                    Symbol = Properties.Resources.LateralSourceMap
                };
            }
        }

        public static VectorStyle SnappedSourcesAndSinksStyle
        {
            get
            {
                return new VectorStyle
                {
                    GeometryType = typeof(IMultiPoint),
                    Fill = new SolidBrush(Color.Tomato),
                    ShapeSize = 8,
                };
            }
        }

        public static VectorStyle EmbankmentStyle
        {
            get
            {
                return new VectorStyle
                {
                    Line = new Pen(Color.SandyBrown, 1f),
                    GeometryType = typeof(ILineString)
                };
            }
        }

        public static VectorStyle EnclosureStyle
        {
            get
            {
                return new VectorStyle
                {
                    GeometryType = typeof(IPolygon),
                    Fill = new SolidBrush(Color.Transparent),
                    Outline = new Pen(Color.FromArgb(100, Color.CornflowerBlue), 2f),
                };
            }
        }

        public static VectorStyle BridgePillarStyle
        {
            get
            {
                return new VectorStyle
                {
                    Line = new Pen(Color.LightSeaGreen, 3f),
                    GeometryType = typeof(ILineString)
                };
            }
        }

        public static VectorStyle RoofAreaStyle
        {
            get
            {
                return new VectorStyle
                {
                    GeometryType = typeof(IPolygon),
                    Line = new Pen(Color.Firebrick, 3f),
                    Fill = new SolidBrush(Color.FromArgb(50, Color.Tomato)),
                    Outline = new Pen(Color.FromArgb(50, Color.Tomato), 2f),
                };
            }
        }

        public static VectorStyle Gulliestyle
        {
            get
            {
                return new VectorStyle
                {
                    GeometryType = typeof(IPoint),
                    Symbol = Properties.Resources.Gully
                };
            }
        }

        public static VectorStyle LeveeStyle
        {
            get
            {
                return new VectorStyle
                {
                    Line = new Pen(Color.LawnGreen, 3f),
                    GeometryType = typeof(ILineString)
                };
            }
        }

        public static VectorStyle BreachStyle
        {
            get
            {
                return new VectorStyle
                {
                    Line = new Pen(Color.DeepPink, 2f),
                    Fill = new SolidBrush(Color.DeepPink),
                    GeometryType = typeof(IPoint)
                };
            }
        }

        public static VectorStyle LeveeSnappedStyle
        {
            get
            {
                return new VectorStyle
                {
                    Line = new Pen(Color.FromArgb(128, Color.LawnGreen), 3f) ,
                    GeometryType = typeof(ILineString)
                };
            }
        }
        public static VectorStyle WaterLevelStreamSnappedStyle
        {
            get
            {
                return new VectorStyle
                {
                    Line = new Pen(Color.FromArgb(128, Color.DarkRed), 2f){StartCap = LineCap.RoundAnchor, EndCap = LineCap.ArrowAnchor, DashStyle = DashStyle.Dot},
                    GeometryType = typeof(ILineString)
                };
            }
        }

        public static VectorStyle BreachSnappedStyle
        {
            get
            {
                return new VectorStyle
                {
                    Line = new Pen(Color.FromArgb(64,Color.DeepPink), 2f),
                    Fill = new SolidBrush(Color.FromArgb(64,Color.DeepPink)),
                    GeometryType = typeof(IPoint)
                };
            }
        }

        public static VectorStyle BreachWidthLineStyle
        {
            get
            {
                return new VectorStyle
                {
                    Line = new Pen(Color.DeepPink, 3f),
                    GeometryType = typeof(ILineString)
                };
            }
        }

        public static VectorStyle BreachWidthPointStyle
        {
            get
            {
                return new VectorStyle
                {
                    Fill = new SolidBrush(Color.DeepPink),
                    GeometryType = typeof(IPoint),
                    SymbolScale = 0.75f,
                };
            }
        }
    }
}