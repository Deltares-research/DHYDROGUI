using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using DeltaShell.Plugins.DelftModels.RTCShapes.Properties;
using Netron.GraphLib;
using Netron.GraphLib.Attributes;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Shapes
{
    /// <summary>
    /// A rectangular shape for a rule
    /// a rule has a
    /// - TopNode input of input entity
    /// - LeftNode input of conditions
    /// - RightNode output of output entity
    /// </summary>
    [Description("Rule shape")]
    [NetronGraphShape("Rule shape",
                      "BD485837-D65C-4aa6-B791-C9F208581E4E",
                      "RTC Shape",
                      "DeltaShell.Plugins.DelftModels.RTCShapes.Shapes.RuleShape",
                      "Rule is UI representation of derived RuleBase.")]
    public class RuleShape : ShapeBase
    {
        public override Bitmap GetThumbnail()
        {
            return Resources.Rule;
        }

        public override void Paint(Graphics g)
        {
            Recalculate(g);

            // a rule has two possible input left and top
            // if left is empty the rule is considered a start point and will be drawn with a bold line
            Pen pen = LeftNode.Connections.Count == 0 ? new Pen(Color.Black, 2) : Pens.Black;
            var linearGradientBrush = new LinearGradientBrush(Rectangle, Color.Coral, Color.White, 45.0F);

            g.FillRectangle(linearGradientBrush, Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
            g.DrawRectangle(pen, Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
            PreRender(g);
            base.Paint(g);

            if (LeftNode.Connections.Count == 0)
            {
                pen.Dispose();
            }

            linearGradientBrush.Dispose();
        }

        protected override void Initialize()
        {
            Rectangle = new RectangleF(0, 0, 60, 40);

            LeftNode = new Connector(this, "Left", true)
            {
                ConnectorLocation = ConnectorLocation.West,
                AllowNewConnectionsFrom = false
            };
            Connectors.Add(LeftNode);

            TopNode = new Connector(this, "Top", true)
            {
                ConnectorLocation = ConnectorLocation.North,
                AllowNewConnectionsFrom = false
            };
            Connectors.Add(TopNode);

            RightNode = new Connector(this, "Right", true)
            {
                ConnectorLocation = ConnectorLocation.East,
                AllowNewConnectionsTo = false
            };
            Connectors.Add(RightNode);

            BottomNode = new Connector(this, "Bottom", true)
            {
                ConnectorLocation = ConnectorLocation.South,
                AllowNewConnectionsFrom = false
            };
            Connectors.Add(BottomNode);
        }
    }
}