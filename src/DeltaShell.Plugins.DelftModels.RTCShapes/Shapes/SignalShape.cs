using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using Netron.GraphLib;
using Netron.GraphLib.Attributes;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Shapes
{
    [Serializable]
    [Description("Signal shape")]
    [NetronGraphShape("Signal shape",
                      "EC4C70E2-EAC8-442A-B5FF-76E553B5458E",
                      "RTC Shape",
                      "DeltaShell.Plugins.DelftModels.RTCShapes.Shapes.SignalShape",
                      "Signal for Rules.")]
    public class SignalShape : ShapeBase
    {
        //public Color GradientStartColor { get; set; }
        //public Color GradientEndColor { get; set; }

        //public SignalShape()
        //{
        //    GradientStartColor = Color.LemonChiffon;
        //    GradientEndColor = Color.White;
        //}

        protected override void Initialize()
        {
            Rectangle = new RectangleF(0, 0, 60, 40);

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

        }

        public override Bitmap GetThumbnail()
        {
            return RTCShapes.Properties.Resources.Signal;
        }

        public override void Paint(Graphics g)
        {
            Recalculate(g);

            var linearGradientBrush = new LinearGradientBrush(Rectangle, Color.Khaki, Color.White, 45.0F);
            g.FillRectangle(linearGradientBrush, Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);
            g.DrawRectangle(Pens.Black, Rectangle.X, Rectangle.Y, Rectangle.Width, Rectangle.Height);

            PreRender(g);
            base.Paint(g);
        }

    }
}
