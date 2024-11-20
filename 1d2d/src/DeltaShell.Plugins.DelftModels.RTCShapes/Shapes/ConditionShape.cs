using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using DeltaShell.Plugins.DelftModels.RTCShapes.Properties;
using Netron.GraphLib;
using Netron.GraphLib.Attributes;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Shapes
{
    /// <summary>
    /// A diamond shape for a condition
    /// a condition has a
    /// - TopNode input of input entity
    /// - LeftNode input of other conditions
    /// - RightNode output for true path
    /// - BottomNode output for false path
    /// </summary>
    [Description("Condition shape")]
    [NetronGraphShape("Condition shape",
                      "A RTC Shapes",
                      "DeltaShell.Plugins.DelftModels.RTCShapes.Shapes.ConditionShape",
                      "Condition triggers rule.")]
    public class ConditionShape : ShapeBase
    {
        public Bitmap Image { get; set; }

        public Func<string> GetDescriptionDelegate { get; set; }

        public void DisableInputConnections()
        {
            LeftNode.AllowNewConnectionsTo = false;
            TopNode.AllowNewConnectionsTo = false;
        }

        public void EnableInputConnections()
        {
            LeftNode.AllowNewConnectionsTo = true;
            TopNode.AllowNewConnectionsTo = true;
        }

        public override Bitmap GetThumbnail()
        {
            return Resources.Condition;
        }

        public override void Paint(Graphics g)
        {
            Recalculate(g);
            var linearGradientBrush = new LinearGradientBrush(Rectangle, Color.DarkCyan, Color.White, 45.0F);
            var points = new List<PointF>
            {
                new PointF(Rectangle.X, Rectangle.Y + (Rectangle.Height / 2)),
                new PointF(Rectangle.X + (Rectangle.Width / 2), Rectangle.Y),
                new PointF(Rectangle.X + Rectangle.Width, Rectangle.Y + (Rectangle.Height / 2)),
                new PointF(Rectangle.X + (Rectangle.Width / 2), Rectangle.Y + Rectangle.Height),
                new PointF(Rectangle.X, Rectangle.Y + (Rectangle.Height / 2))
            };
            g.FillPolygon(linearGradientBrush, points.ToArray());

            // a condition has two possible input left and top
            // if left is empty the condition is considered a start point and will be drawn with a bold line

            Pen pen = LeftNode.Connections.Count == 0 ? new Pen(Color.Black, 2) : Pens.Black;

            g.DrawPolygon(pen, points.ToArray());
            PreRender(g);
            RenderDescription(g);
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
                AllowNewConnectionsTo = false
            };
            Connectors.Add(BottomNode);
        }

        protected override void Recalculate(Graphics g)
        {
            if (!RecalculateSize)
            {
                return;
            }

            SizeF s = g.MeasureString(Title, Font);
            Rectangle = new RectangleF(Rectangle.X, Rectangle.Y, s.Width + 40, Math.Max(s.Height * 3, Rectangle.Height));
        }

        private void RenderDescription(Graphics g)
        {
            var margin = 1;

            if (Image != null)
            {
                float imageX = Rectangle.X + margin;
                float imageY = Rectangle.Bottom - (margin + Image.Height);

                g.DrawImage(Image, imageX, imageY);
            }

            if (GetDescriptionDelegate != null)
            {
                string description = GetDescriptionDelegate();

                float fontSize = Font.Size - 2;
                var fontToMeasure = new Font(Font.FontFamily, fontSize, Font.Style);
                SizeF strRect = g.MeasureString(description, fontToMeasure);
                var origin = new PointF(Rectangle.Right - strRect.Width - margin, Rectangle.Bottom - strRect.Height - margin);

                g.DrawString(description, fontToMeasure, TextBrush, origin.X, origin.Y);
            }
        }
    }
}