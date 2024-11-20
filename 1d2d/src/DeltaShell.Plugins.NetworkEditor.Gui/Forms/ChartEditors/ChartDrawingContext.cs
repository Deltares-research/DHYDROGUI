using System.Drawing;
using System.Drawing.Drawing2D;
using DelftTools.Controls.Swf.Charting;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    class ChartDrawingContext : IChartDrawingContext
    {
        private ChartGraphics g;
        private readonly Color oldBackColor;
        private readonly int oldPenWidth;
        private readonly DashStyle oldPenStyle;
        private readonly Color oldPenColor;

        public VectorStyle Style { get; set; }
        public object Graphics { get { return g;} }

        internal ChartDrawingContext(ChartGraphics g, VectorStyle style)
        {
            oldBackColor = g.BackColor;
            oldPenColor = g.PenColor;
            oldPenWidth = g.PenWidth;
            oldPenStyle = g.PenStyle;

            this.g = g;

            Style = style;
            
            g.BackColor = style.Fill is SolidBrush
                              ? ((SolidBrush) style.Fill).Color
                              : Color.Transparent;
            
            g.PenColor = style.Line.Color;
            g.PenWidth = (int) style.Line.Width;
            g.PenStyle = style.Line.DashStyle;
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