using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes
{
    /// <summary>
    /// Implements a compososite shapefeature that represents a series of adjoining rectangles.
    /// The rectangels in the series are separarted by borders.
    /// Current version only supports horizontal series
    /// </summary>
    public class RectangleSeriesShapeFeature : CompositeShapeFeature
    {
        private readonly IChart chart;
        public readonly List<double> Borders = new List<double>();
        public readonly List<IShapeFeature> BorderShapes = new List<IShapeFeature>();

        public RectangleSeriesShapeFeature(IChart chart, double x, double y, double width, double height, double top, bool widthIsWorld, bool heightIsWorld) :base(chart)
        {
            this.chart = chart;

            HorizontalShapeAlignment = HorizontalShapeAlignment.Left;
            VerticalShapeAlignment = VerticalShapeAlignment.Top;
            Top = top;
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
        public double X { get; set; }

        /// <summary>
        /// The y coordinate of the shape. 
        /// if VerticalShapeAlignment is VerticalShapeAlignment.Top Y is the top coordinate of the bounding box
        /// if VerticalShapeAlignment is VerticalShapeAlignment.Center Y is the vertical center coordinate of the bounding box
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Width of the shape either in world coordinates (WidthIsWorld == true) of device coordinates (WidthIsWorld == false) 
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Height of the shape either in world coordinates (HeightIsWorld == true) of device coordinates (HeightIsWorld == false) 
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Is Width in world coordinates (true) or device coordinates (false)
        /// </summary>
        public bool WidthIsWorld { get; set; }

        /// <summary>
        /// Is Height in world coordinates (true) or device coordinates (false)
        /// </summary>
        public bool HeightIsWorld { get; set; }

        /// <summary>
        /// X and Y are in World coordinates
        /// </summary>
        public bool StickToBottom { get; set; }

        /// <summary>
        /// Horizontal alignment of the shape
        /// </summary>
        public HorizontalShapeAlignment HorizontalShapeAlignment { get; set; }

        /// <summary>
        /// Vertical alignment of the shape
        /// </summary>
        public VerticalShapeAlignment VerticalShapeAlignment { get; set; }

        public double Top { get; set; }

        /// <summary>
        /// Set the new border value for border at index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="borderValue"></param>
        public void SetBorder(int index, double borderValue)
        {
            if (index >= (Borders.Count - 1))
            {
                return;
            }
            Borders[index] = borderValue;
            // adjust the following shapes
            // previous shapefeature of the border : width
            ((FixedRectangleShapeFeature)ShapeFeatures[index]).Width = borderValue - ((FixedRectangleShapeFeature)ShapeFeatures[index]).X;
            // next shapefeature of the border : X ans width
            ((FixedRectangleShapeFeature)ShapeFeatures[index + 1]).X = borderValue;
            double nextBorder = (index < Borders.Count - 2) ? ((FixedRectangleShapeFeature)ShapeFeatures[index + 2]).X : X + Width;
            ((FixedRectangleShapeFeature)ShapeFeatures[index + 1]).Width = nextBorder - borderValue;
            // The shape used to represent the border
            ((FixedRectangleShapeFeature) BorderShapes[index]).X = borderValue;
            Invalidate();
        }

        public void AddRectangle(object tag, string label, double right, VectorStyle normalStyle, VectorStyle selectedStyle)
        {
            if (Borders.Count > 0)
            {
                VectorStyle vectorStyle = new VectorStyle
                                              {
                                                  Fill = new SolidBrush(Color.FromArgb(150, Color.Orange)),
                                                  Line = new Pen(Color.FromArgb(50, Color.Orange), 1)
                                                             {
                                                                 DashStyle = System.Drawing.Drawing2D.DashStyle.Dot
                                                             }
                                              };
                FixedRectangleShapeFeature border = new FixedRectangleShapeFeature(chart, 
                                                                                    Borders[Borders.Count - 1], 
                                                                                    Top,
                                                                                    2, 
                                                                                    Math.Abs(Y - Top),
                                                                                    false, 
                                                                                    true)
                            { 
                                HorizontalShapeAlignment = HorizontalShapeAlignment.Center,
                                NormalStyle = vectorStyle,
                                SelectedStyle = vectorStyle,
                                Tag = tag,
                                StickToBottom = StickToBottom
                            };
                BorderShapes.Add(border);
            }
            double left = X;
            if (Borders.Count > 0)
            {
                left = Borders[Borders.Count - 1];
            }
            FixedRectangleShapeFeature feature = new FixedRectangleShapeFeature(chart, left, Y, right - left, Height,
                                                                                WidthIsWorld, HeightIsWorld)
                                                     {
                                                         NormalStyle = normalStyle,
                                                         VerticalShapeAlignment = VerticalShapeAlignment,
                                                         SelectedStyle = selectedStyle,
                                                         Label = label,
                                                         Tag = tag,
                                                         TopMargin = 5,
                                                         StickToBottom = StickToBottom
                                                     };
            ShapeFeatures.Add(feature);
            Borders.Add(right);
        }

        public override IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            return new RectangleSeriesShapeEditor(this, new ChartCoordinateService(Chart), shapeEditMode);
        }

        public override void Paint(VectorStyle style)
        {
            ShapeFeatures.ForEach(cs => cs.Paint(style));
            BorderShapes.ForEach(cs => cs.Paint(style));
        }

        public override bool Contains(int x, int y)
        {
            bool contains = false;
            BorderShapes.ForEach(cs => contains |= cs.Contains(x, y));
            return !contains ? base.Contains(x, y) : contains;
        }
    }
}