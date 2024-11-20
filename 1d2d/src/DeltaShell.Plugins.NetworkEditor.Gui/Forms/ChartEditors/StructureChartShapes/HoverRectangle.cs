using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    public class HoverRectangle : IHoverFeature
    {
        public Color ForeColor { get; set; }
        public IShapeFeature ShapeFeature { get; set; }
        public VectorStyle Style { get; set; }

        public HoverRectangle(IShapeFeature shapeFeature, Color color)
        {
            ShapeFeature = shapeFeature;
            ForeColor = color;
        }

        public HoverRectangle(IShapeFeature shapeFeature, VectorStyle style)
        {
            ShapeFeature = shapeFeature;
            Style = style;
        }

        public void Render(List<Rectangle> usedSpace, IChart chart, Graphics graphics)
        {
            var bounds = ShapeFeature.GetBounds();
            if (Style != null)
            {
                graphics.DrawRectangle(Style.Outline, bounds);
            }
            else
            {
                var brush = new SolidBrush(ForeColor);
                graphics.FillRectangle(brush, bounds);
                brush.Dispose();
            }
        }

        public HoverType HoverType { get; set; }
    }
}