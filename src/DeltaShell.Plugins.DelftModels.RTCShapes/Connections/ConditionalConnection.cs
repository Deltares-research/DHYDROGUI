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
            var size = g.MeasureString(Connection.Text, Connection.Font).ToSize();
            // simplied labelpainting; not in center but at start connection

            if (labelNeedsCorrection.Contains(Connection.From.Name))
            {
                RectangleF endPosition = Connection.To.ConnectionGrip();
                float xCorrection = (endPosition.X - startPosition.X) / 8;
                float yCorrection = (endPosition.Y - startPosition.Y) / 8;
                startPosition.X += xCorrection;
                startPosition.Y += yCorrection;
            }

            var labelRect = new RectangleF(startPosition.X, startPosition.Y, size.Width, size.Height + 1);
            if (Connection.BoxedLabel)
            {
                var boxRectangle = new RectangleF(labelRect.X, labelRect.Y, labelRect.Width, labelRect.Height);
                boxRectangle.Inflate(+3, +2);
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 231)), boxRectangle);
                g.DrawRectangle(new Pen(Color.Black, 1), Rectangle.Round(boxRectangle));
            }

            g.DrawString(Connection.Text, Connection.Font, new SolidBrush(Color.Black), labelRect.X, labelRect.Y);
        }
    }
}