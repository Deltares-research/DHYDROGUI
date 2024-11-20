using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using DeltaShell.Plugins.DelftModels.RTCShapes.Properties;
using Netron.GraphLib;
using Netron.GraphLib.Attributes;

namespace DeltaShell.Plugins.DelftModels.RTCShapes.Shapes
{
    [Description("Input item shape")]
    [NetronGraphShape("Input item shape",
                      "8A70C0A3-DBDD-4a6a-BEFB-5EC933D50622",
                      "A RTC Shapes",
                      "DeltaShell.Plugins.DelftModels.RTCShapes.Shapes.InputItemShape",
                      "Location where input is taken from.")]
    public class InputItemShape : ShapeBase
    {
        public InputItemShape()
        {
            GradientStartColor = Color.LemonChiffon;
            GradientEndColor = Color.White;
        }

        public Color GradientStartColor { get; set; }
        public Color GradientEndColor { get; set; }

        public override Bitmap GetThumbnail()
        {
            return Resources.Input;
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

            BottomNode = new Connector(this, "Bottom", true)
            {
                ConnectorLocation = ConnectorLocation.South,
                AllowNewConnectionsTo = false
            };
            Connectors.Add(BottomNode);
        }

        protected override void UpdateColor(bool isLinked)
        {
            GradientStartColor = isLinked ? Color.LightGreen : Color.LightYellow;
        }
    }
}