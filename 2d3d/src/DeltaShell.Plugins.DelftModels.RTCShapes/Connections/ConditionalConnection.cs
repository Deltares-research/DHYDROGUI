using System.Drawing;
using System.Linq;
using Netron.GraphLib;
using Netron.GraphLib.Attributes;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Connections
{
    /// <summary>
    /// A multi-line connection painter, this code is part of an online tutorial: see the Netron site for more information
    /// http://netron.sf.net
    /// </summary>
    [NetronGraphConnection("ConditionalConnection", "0CCF8556-3824-4a32-A5D7-64E017398EEA",
                           "DeltaShell.Plugins.DelftModels.RTCShapes.Connections.ConditionalConnection")]
    public class ConditionalConnection : DefaultPainter
    {
        private readonly Brush labelBrush = new SolidBrush(Color.Black);
        private readonly Brush labelBoxBrush = new SolidBrush(Color.FromArgb(255, 255, 231));
        private readonly Pen labelBoxPen = new Pen(Color.Black, 1);

        private readonly string[] labelNeedsCorrection =
        {
            "Right",
            "Bottom"
        };

        public ConditionalConnection(Connection connection) : base(connection) {}

        public override void Paint(Graphics g)
        {
            base.Paint(g);
            PaintLabel(g);
        }

        private void PaintLabel(Graphics g)
        {
            RectangleF startPosition = Connection.From.ConnectionGrip();
            RectangleF endPosition = Connection.To.ConnectionGrip();

            float xPos = startPosition.X;
            float yPos = startPosition.Y;

            // draw label further from connection grip
            if (labelNeedsCorrection.Contains(Connection.From.Name))
            {
                xPos += GetCorrection(endPosition.X, xPos);
                yPos += GetCorrection(endPosition.Y, yPos);
            }

            if (Connection.BoxedLabel)
            {
                PaintLabelBox(g, xPos, yPos);
            }

            PaintLabel(g, xPos, yPos);
        }

        private void PaintLabel(Graphics g, float xPos, float yPos)
        {
            g.DrawString(Connection.Text, Connection.Font, labelBrush, xPos, yPos);
        }

        private void PaintLabelBox(Graphics g, float xPos, float yPos)
        {
            var size = g.MeasureString(Connection.Text, Connection.Font).ToSize();
            var boxRectangle = new RectangleF(xPos, yPos, size.Width, size.Height + 1);
            boxRectangle.Inflate(+3, +2);

            g.FillRectangle(labelBoxBrush, boxRectangle);
            g.DrawRectangle(labelBoxPen, Rectangle.Round(boxRectangle));
        }

        private static float GetCorrection(float end, float start) => (end - start) / 8;
    }
}