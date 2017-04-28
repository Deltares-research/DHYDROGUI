using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    class ThalWegInStructureView : FixedRectangleShapeFeature
    {
        private VectorStyle ThalWegStyle { get; set; }

        public ThalWegInStructureView(IChart chart)
            : base(chart)
        {
            ThalWegStyle = new VectorStyle
                               {
                                   //Fill = new HatchBrush(HatchStyle.Horizontal, Color.LightGray, Color.DodgerBlue),
                                   Fill = Brushes.DodgerBlue,
                                   Line = Pens.Transparent
                               };
        }

        /// <summary>
        /// Custom paint method since x of level lines is dependend of zoom-level
        /// </summary>
        /// <param name="vectorStyle"></param>
        public override void Paint(VectorStyle vectorStyle)
        {
            GetShape().Paint(ThalWegStyle);
        }

        private IShapeFeature GetShape()
        {
            double height = Chart.LeftAxis.Maximum - Chart.LeftAxis.Minimum;
            double minZValue = Chart.LeftAxis.Minimum - height / 10;
            double maxZValue = Chart.LeftAxis.Maximum + height / 10;
            var thalWegShape = new FixedRectangleShapeFeature(Chart,
                          0,
                          maxZValue,
                          4,
                          ChartCoordinateService.ToDeviceHeight(Chart, maxZValue - minZValue),
                          false,
                          false);
            thalWegShape.AddHover(new HoverText("Thalweg", null, thalWegShape, Color.DodgerBlue, HoverPosition.Left, ArrowHeadPosition.None)
                                      {ShowLine = false});
            return thalWegShape;
        }

        public override bool Contains(int x, int y)
        {
            return GetShape().Contains(x, y);
        }

        public override Rectangle GetBounds()
        {
            return GetShape().GetBounds();
        }

        public override void Hover(List<Rectangle> usedSpace, VectorStyle style, Graphics graphics)
        {
            var shapeFeature = GetShape();
            var hover = shapeFeature as IHover;
            if (hover != null)
            {
                hover.Hover(usedSpace, style, graphics);
            }
        }
    }
}
