using System.Drawing;
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
        public ConditionalConnection(Connection connection) : base(connection) {}

        public override void Paint(Graphics g)
        {
            base.Paint(g);
            PaintLabel(g);
        }

        protected void PaintLabel(Graphics g)
        {
            RectangleF startPosition = Connection.From.ConnectionGrip();
            var size = g.MeasureString(Connection.Text, Connection.Font).ToSize();
            // simplied labelpainting; not in center but at start connection
            if (Connection.From.Name == "Right")
            {
                startPosition.X += size.Height;
                startPosition.Y -= size.Height;
            }

            if (Connection.From.Name == "Bottom")
            {
                startPosition.X += size.Height;
                startPosition.Y += size.Height;
            }

            var labelRect = new RectangleF(startPosition.X, startPosition.Y, size.Width, size.Height + 1);
            if (Connection.BoxedLabel)
            {
                var boxRexangle = new RectangleF(labelRect.X, labelRect.Y, labelRect.Width, labelRect.Height);
                boxRexangle.Inflate(+3, +2);
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 231)), boxRexangle);
                g.DrawRectangle(new Pen(Color.Black, 1), Rectangle.Round(boxRexangle));
            }

            g.DrawString(Connection.Text, Connection.Font, new SolidBrush(Color.Black), labelRect.X, labelRect.Y);
        }
    }
}