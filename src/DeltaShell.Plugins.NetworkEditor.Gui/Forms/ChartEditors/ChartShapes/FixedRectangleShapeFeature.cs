using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using GeoAPI.Geometries;
using SharpMap.Converters.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes
{
    public enum HorizontalShapeAlignment
    {
        Left,
        Center,
        Right
    }

    public enum VerticalShapeAlignment
    {
        Top,
        Center,
        Bottom
    }

    /// <summary>
    /// FixedRectangleShapeFeature implements a rectangular shape that can either zoom with the chart (WidthIsWorld/HeightIsWorld 
    /// true) or retain its size (WidthIsWorld/HeightIsWorld false)
    /// </summary>
    public class FixedRectangleShapeFeature : ShapeFeatureBase
    {
        public FixedRectangleShapeFeature(IChart chart)
            : base(chart)
        {
            HorizontalShapeAlignment = HorizontalShapeAlignment.Left;
            VerticalShapeAlignment = VerticalShapeAlignment.Top;
        }


        public FixedRectangleShapeFeature(IChart chart, double x, double y, double width, double height, bool widthIsWorld, bool heightIsWorld)
            : base(chart)
        {
            HorizontalShapeAlignment = HorizontalShapeAlignment.Left;
            VerticalShapeAlignment = VerticalShapeAlignment.Top;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            WidthIsWorld = widthIsWorld;
            HeightIsWorld = heightIsWorld;
        }

        /// <summary>
        /// The x coordinate of the shape. 
        /// if HorizontalShapeAlignment is HorizontalShapeAlignment.Left X is the left coordinate of the bounding box
        /// if HorizontalShapeAlignment is HorizontalShapeAlignment.Center X is the horizontal center coordinate of the bounding box
        /// </summary>
        public virtual double X { get; set; }

        /// <summary>
        /// The y coordinate of the shape. 
        /// if VerticalShapeAlignment is VerticalShapeAlignment.Top Y is the top coordinate of the bounding box
        /// if VerticalShapeAlignment is VerticalShapeAlignment.Center Y is the vertical center coordinate of the bounding box
        /// </summary>
        public virtual double Y { get; set; }

        /// <summary>
        /// Width of the shape either in world coordinates (WidthIsWorld == true) of device coordinates (WidthIsWorld == false) 
        /// </summary>
        public virtual double Width { get; set; }

        /// <summary>
        /// Height of the shape either in world coordinates (HeightIsWorld == true) of device coordinates (HeightIsWorld == false) 
        /// </summary>
        public virtual double Height { get; set; }

        /// <summary>
        /// Is Width in world coordinates (true) or device coordinates (false)
        /// </summary>
        public bool WidthIsWorld { get; set; }

        /// <summary>
        /// Is Height in world coordinates (true) or device coordinates (false)
        /// </summary>
        public bool HeightIsWorld { get; set; }

        public bool StickToBottom { get; set; }

        /// <summary>
        /// Horizontal alignment of the shape
        /// </summary>
        public HorizontalShapeAlignment HorizontalShapeAlignment { get; set; }

        /// <summary>
        /// Vertical alignment of the shape
        /// </summary>
        public VerticalShapeAlignment VerticalShapeAlignment { get; set; }

        /// <summary>
        /// Add an extra margin when drawing the rectangle at the top. This enables to float a rectangle to 
        /// </summary>
        public int TopMargin { get; set; }

        /// <summary>
        /// Returns the geometry in world coordinates
        /// </summary>
        public override IGeometry Geometry
        {
            get
            {
                Rectangle rectangle = GetBounds();
                var left = ChartCoordinateService.ToWorldWidth(Chart, rectangle.Left);
                var right = ChartCoordinateService.ToWorldWidth(Chart, rectangle.Right);
                var bottom = ChartCoordinateService.ToWorldWidth(Chart, rectangle.Top);
                var top = ChartCoordinateService.ToWorldWidth(Chart, rectangle.Bottom);

                var vertices = new List<Coordinate>
                                   {
                                       new Coordinate(left, bottom),
                                       new Coordinate(right, bottom),
                                       new Coordinate(right, top),
                                       new Coordinate(left, top)
                                   };
                vertices.Add((Coordinate)vertices[0].Clone());
                ILinearRing newLinearRing = GeometryFactory.CreateLinearRing(vertices.ToArray());
                return GeometryFactory.CreatePolygon(newLinearRing, null);
            }
            set
            {
                base.Geometry = value;
            }
        }

        /// <summary>
        /// Returns the bounding rect in device coordinates. Used for drawing and hit testing.
        /// </summary>
        /// <returns></returns>
        public override Rectangle GetBounds()
        {
            var width = WidthIsWorld ? ChartCoordinateService.ToDeviceWidth(Chart, Width) : (int)Width;
            var height = HeightIsWorld ? ChartCoordinateService.ToDeviceHeight(Chart, Height) : (int)Height;
            int x, y;

            if (HorizontalShapeAlignment == HorizontalShapeAlignment.Left)
            {
                x = ChartCoordinateService.ToDeviceX(Chart, X);
            }
            else if (HorizontalShapeAlignment == HorizontalShapeAlignment.Center)
            {
                x = ChartCoordinateService.ToDeviceX(Chart, X) - width/2;
            }
            else
            {
                throw new NotImplementedException("HorizontalShapeAlignment.Right not implemented");
            }

            if (StickToBottom)
            {
                y = Chart.ChartBounds.Bottom - height;
            }
            else
            {
                if (VerticalShapeAlignment == VerticalShapeAlignment.Top)
                {
                    y = ChartCoordinateService.ToDeviceY(Chart, Y);
                }
                else if (VerticalShapeAlignment == VerticalShapeAlignment.Center)
                {
                    y = ChartCoordinateService.ToDeviceY(Chart, Y) - height/2;
                }
                else
                {
                    y = ChartCoordinateService.ToDeviceY(Chart, Y) - height;
                }
            }
            
            var rectangle = new Rectangle
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
            return rectangle;
        }

        public override bool Contains(double x, double y)
        {
            int xDevice = ChartCoordinateService.ToDeviceX(Chart, x);
            int yDevice = ChartCoordinateService.ToDeviceY(Chart, y);
            return GetBounds().Contains(xDevice, yDevice);
        }

        protected override void Paint(IChartDrawingContext chartDrawingContext)
        {
            var g = (ChartGraphics)chartDrawingContext.Graphics;
            var bounds = GetBounds();
            bounds.Y += TopMargin;
            g.Rectangle(bounds);
            if (string.IsNullOrEmpty(Label))
            {
                return;
            }
            var size = g.MeasureString(Label);
            int xpos = (int)(bounds.Left + bounds.Width / 2 - size.Width / 2);
            int ypos = (int)(bounds.Top + bounds.Height / 2 - size.Height / 2);
            g.TextOut(xpos, ypos, Label);
        }

        public override object Clone()
        {
            return new FixedRectangleShapeFeature(Chart, X, Y, Width, Height, WidthIsWorld, HeightIsWorld) { StickToBottom = StickToBottom };
        }

        public override IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            return null;
        }
    }
}
