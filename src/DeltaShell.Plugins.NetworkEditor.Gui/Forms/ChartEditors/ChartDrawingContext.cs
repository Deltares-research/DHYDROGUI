using System.Drawing;
using System.Drawing.Drawing2D;
using DelftTools.Controls.Swf.Charting;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    internal class ChartDrawingContext : IChartDrawingContext
    {
        private readonly Color oldBackColor;
        private readonly int oldPenWidth;
        private readonly DashStyle oldPenStyle;
        private readonly Color oldPenColor;
        private ChartGraphics g;

        internal ChartDrawingContext(ChartGraphics g, VectorStyle style)
        {
            oldBackColor = g.BackColor;
            oldPenColor = g.PenColor;
            oldPenWidth = g.PenWidth;
            oldPenStyle = g.PenStyle;

            this.g = g;

            Style = style;

            g.BackColor = (style.Fill as SolidBrush)?.Color ?? Color.Transparent;

            g.PenColor = style.Line.Color;
            g.PenWidth = (int) style.Line.Width;
            g.PenStyle = style.Line.DashStyle;
        }

        public VectorStyle Style { get; set; }

        public object Graphics
        {
            get
            {
                return g;
            }
        }

        public void Reset()
        {
            g.BackColor = oldBackColor;
            g.PenColor = oldPenColor;
            g.PenWidth = oldPenWidth;
            g.PenStyle = oldPenStyle;
        }
    }
}