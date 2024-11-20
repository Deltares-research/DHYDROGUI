using System;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    /// <summary>
    /// Implements a simple (rectangular) weir shape and a water column
    /// </summary>
    public class WeirShapeFeature : CompositeShapeFeature
    {
        public RectangleShapeFeature WeirShape { get; set; }
        public RectangleShapeFeature WaterShape { get; set; }

        public VectorStyle WaterStyle
        {
            set
            {
                VectorStyle transparentStyle = (VectorStyle)value.Clone();
                transparentStyle.Fill = Brushes.Transparent;

                WaterShape.NormalStyle = value;
                WaterShape.DisabledStyle = transparentStyle;
                WaterShape.SelectedStyle = value;
            }
        }

        public WeirShapeFeature(IChart chart, double left, double top, double right, double minY, double maxY) : base(chart)
        {
            var waterTop = Math.Max(maxY, top);
            var waterBottom = top;
            WaterShape = new RectangleShapeFeature(Chart, left, waterTop, right, waterBottom);
            ShapeFeatures.Add(WaterShape);

            WeirShape = new RectangleShapeFeature(chart, left, top, right, minY);
            ShapeFeatures.Add(WeirShape);
        }
    }
}