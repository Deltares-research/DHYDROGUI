using System.Drawing;
using System.Drawing.Drawing2D;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes
{
    class ArchesShapeFeature : RoundCrestShapeFeature
    {
        /// <summary>
        ///   ----       ----       ----       ----
        ///  /    \     /    \     /    \     /    \
        /// /      \   /      \   /      \   /      \
        /// |      |   |      |   |      |   |      |  ^
        /// |      |   |      |   |      |   |      |  |  offset
        /// |      |   |      |   |      |   |      |  v
        /// --------   --------   --------   -------- 
        ///         <-> pillarwidth
        /// </summary>
        public int NumberOfPillars { get; set; }
        public double PillarWidth { get; set; }

        public ArchesShapeFeature(IChart chart, double left, double top, double right, double bottom, double crestOffset,
                                  int numberOfPillars, double pillarWidth)
            : base(chart, left, top, right, bottom, crestOffset)

        {
            NumberOfPillars = numberOfPillars;
            PillarWidth = pillarWidth;
        }
        public override void Paint(IChartDrawingContext chartDrawingContext)
        {
            var g = (ChartGraphics)chartDrawingContext.Graphics;
            //g.BackColor = Selected ? Color.FromArgb(100, Color.Red) : Color.FromArgb(100, Color.Blue);
            g.BackColor = ((SolidBrush)chartDrawingContext.Style.Fill).Color;
            double archWidth = (Width - (PillarWidth * NumberOfPillars)) / (NumberOfPillars + 1);
            double left = X - Width/2;
            double right = left + archWidth;
            for (int i=0; i<=NumberOfPillars; i++)
            {
                double top = Y;
                double bottom = Y+Height;
                double offset = CrestOffset;
                GraphicsPath graphicsPath = GenerateArch(Chart, left, top, right, bottom, offset);
                if (null != graphicsPath)
                {
                    g.DrawPath(new Pen(g.PenColor), graphicsPath);
                    // hack: Graphics3D does not support FillPath but DrawPath does not
/*                    if (g is Graphics3DGdiPlus)
                    {
                        ((Graphics3DGdiPlus)g).Graphics.FillPath(new SolidBrush(g.BackColor), graphicsPath);
                    }*/
                }
                left += archWidth + PillarWidth;
                right = left + archWidth;
            }
        }

        public override IShapeFeatureEditor CreateShapeFeatureEditor(ShapeEditMode shapeEditMode)
        {
            return new ArchShapeEditor(this, new ChartCoordinateService(Chart), shapeEditMode);
        }
    }
}