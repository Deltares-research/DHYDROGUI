using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Netron.GraphLib;
using Netron.GraphLib.Attributes;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Shapes
{
    [Description("Math expression shape")]
    [NetronGraphShape(
        "Mathematical expression shape",
        "RTC Shape",
        "DeltaShell.Plugins.DelftModels.RTCShapes.Shapes.MathematicalExpressionShape",
        "Mathematical expression")]
    public class MathematicalExpressionShape : ShapeBase
    {
        public IEnumerable<Connection> GetTopConnectors()
        {
            foreach (Connection topNodeConnection in TopNode.Connections)
            {
                yield return topNodeConnection;
            }
        }

        public override PointF ConnectionPoint(Connector c)
        {
            if (c == TopNode)
            {
                return new PointF(Rectangle.Right, Rectangle.Top);
            }

            if (c == BottomNode)
            {
                return new PointF(Rectangle.Right, Rectangle.Bottom);
            }

            if (c == LeftNode)
            {
                return new PointF(Rectangle.Left, Rectangle.Top + (Rectangle.Height / 2));
            }

            if (c == RightNode)
            {
                return new PointF(Rectangle.Right, Rectangle.Top + (Rectangle.Height / 2));
            }

            return new PointF(0, 0);
        }

        public override void Paint(Graphics g)
        {
            Recalculate(g);

            float x = Rectangle.X;
            float y = Rectangle.Y;
            float width = Rectangle.Width;
            float height = Rectangle.Height;

            var points = new[]
            {
                // top right
                new PointF(x + width, y),

                // bottom right
                new PointF(x + width, y + height),

                // left
                new PointF(x, y + (height / 2))
            };

            Pen pen = Pens.Black;
            g.DrawPolygon(pen, points);

            var brush = new SolidBrush(Color.FromArgb(14, 187, 240));
            g.FillPolygon(brush, points);
            PreRender(g);
            base.Paint(g);
        }

        protected override void Initialize()
        {
            Rectangle = new RectangleF(0, 0, 25, 50);

            LeftNode = CreateConnector("Left", false, true);
            TopNode = CreateConnector("Top", false, true);
            BottomNode = CreateConnector("Bottom", true, false);

            Connectors.Add(LeftNode);
            Connectors.Add(TopNode);
            Connectors.Add(BottomNode);
        }

        private Connector CreateConnector(string name, bool allowFrom, bool allowTo)
        {
            return new Connector(this, name, true)
            {
                AllowNewConnectionsFrom = allowFrom,
                AllowNewConnectionsTo = allowTo,
                ShowLabel = true
            };
        }
    }
}
