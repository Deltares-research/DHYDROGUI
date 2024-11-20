using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using DeltaShell.Plugins.DelftModels.RTCShapes.Properties;
using Netron.GraphLib;
using Netron.GraphLib.Attributes;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Shapes
{
    [Description("Input item shape")]
    [NetronGraphShape("Output item shape",
                      "B8CA9CB8-6198-449e-80DA-0E05E9162F3E",
                      "A RTC Shapes",
                      "DeltaShell.Plugins.DelftModels.RTCShapes.Shapes.OutputItemShape",
                      "Location where reult is written.")]
    public class OutputItemShape : ShapeBase
    {
        public OutputItemShape()
        {
            GradientStartColor = Color.Gray;
            GradientEndColor = Color.White;
        }

        public Color GradientStartColor { get; set; }
        public Color GradientEndColor { get; set; }

        public override Bitmap GetThumbnail()
        {
            return Resources.Output;
        }

        public override void Paint(Graphics g)
        {
            Recalculate(g);
            var linearGradientBrush = new LinearGradientBrush(Rectangle, GradientStartColor, GradientEndColor, 45.0F);
            g.FillEllipse(linearGradientBrush, Rectangle);
            g.DrawEllipse(Pens.Black, Rectangle);
            PreRender(g);
            base.Paint(g);
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
        }

        protected override void UpdateColor(bool isLinked)
        {
            GradientStartColor = isLinked ? Color.DodgerBlue : Color.LightYellow;
        }
    }
}