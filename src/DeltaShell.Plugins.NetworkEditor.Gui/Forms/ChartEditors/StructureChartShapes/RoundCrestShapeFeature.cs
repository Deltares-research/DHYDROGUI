using System.Drawing;
using System.Drawing.Drawing2D;
using DelftTools.Controls.Swf.Charting;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes
{
    class RoundCrestShapeFeature : CrestShapeFeature
    {
        /// <summary>
        ///   -----
        ///  /     \
        /// /       \
        /// |   *   |  ^
        /// |       |  |  offset
        /// |       |  v
        /// ---------
        /// </summary>
        public RoundCrestShapeFeature(IChart chart, double x, double top, double pixelWidth, double bottom, double crestOffset)
            : base(chart, x, top, pixelWidth, bottom, crestOffset)

        {
        }

        static protected GraphicsPath GenerateArch(IChart chart, double x, double y, double pixelWidth, double bottom,
                                                   double offset)
        {
            int center = ChartCoordinateService.ToDeviceX(chart, x);
            int pixelLeft = (int) (center - pixelWidth/2.0);
            int pixelRight = (int)(center + pixelWidth / 2.0);

            double archHeight = y - bottom - offset;
            Rectangle rectangle = new Rectangle
                                      {
                                          X = pixelLeft,
                                          Y = ChartCoordinateService.ToDeviceY(chart, bottom + offset + archHeight),
                                          Width = (int)pixelWidth,
                                          Height = ChartCoordinateService.ToDeviceHeight(chart, 2 * archHeight)
                                      };
            if ((rectangle.Height <= 0) || (rectangle.Width <= 0))
            {
                return null;
            }
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddArc(rectangle, 180, 180);

            var pixelCrestOffset = ChartCoordinateService.ToDeviceY(chart, bottom + offset);
            rectangle.Y = pixelCrestOffset;
            rectangle.Height = ChartCoordinateService.ToDeviceHeight(chart, offset);
            var pixelBottom = ChartCoordinateService.ToDeviceY(chart, bottom);
            graphicsPath.AddLine(pixelRight, pixelCrestOffset,
                                 pixelRight, pixelBottom);
            graphicsPath.AddLine(pixelLeft, pixelBottom,
                                 pixelRight, pixelBottom);
            graphicsPath.AddLine(pixelLeft, pixelBottom,
                                 pixelLeft, pixelCrestOffset);
            return graphicsPath;
        }

        protected override void Paint(IChartDrawingContext chartDrawingContext)
        {
            var g = (ChartGraphics)chartDrawingContext.Graphics;

            g.BackColor = ((SolidBrush)chartDrawingContext.Style.Fill).Color;
            GraphicsPath graphicsPath = GenerateArch(Chart, X, Y, Width, Bottom, CrestOffset);
            if (null == graphicsPath) 
                return;
            g.DrawPath(new Pen(g.PenColor), graphicsPath);
        }
    }
}