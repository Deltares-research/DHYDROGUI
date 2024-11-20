using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting.Tools;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    public class ShapeHoverTool : ShapeLayerTool
    {
        private IShapeFeature HoverShape { get; set; }
        private Bitmap backGround;

        public override void MouseEvent(ChartMouseEvent kind, MouseEventArgs e, Cursor c)
        {
            Point tmP = new Point(e.X, e.Y);
            switch (kind)
            {
                case ChartMouseEvent.Move:
                    if (e.Button == MouseButtons.None)
                    {

                        IShapeFeature hoverShape = ShapeModifyTool.Clicked(tmP.X, tmP.Y);
                        if (HoverShape != hoverShape)
                        {
                            HoverShape = hoverShape;
                            Draw();
                        }
                    }
                    break;
            }
        }

        private void Draw()
        {
            var c = ShapeModifyTool.Chart.ParentControl;
            if (null == c)
            {
                return;
            }
            if (null == backGround)
            {
                backGround = ShapeModifyTool.Chart.Bitmap();
            }

            var hoverImage = new Bitmap(c.Width, c.Height);
            Graphics graphics = Graphics.FromImage(hoverImage);
            graphics.SetClip(ShapeModifyTool.Chart.ChartBounds);
            graphics.DrawImage(backGround, 0, 0);

            PaintHovers(graphics);
            graphics.ResetClip();
            graphics.Dispose();

            graphics = c.CreateGraphics();
            graphics.DrawImage(hoverImage, 0, 0);
            hoverImage.Dispose();
            graphics.Dispose();
        }

        public void Clear()
        {
            HoverShape = null;
        }

        public void ClearBuffer()
        {
            if (null == backGround)
            {
                return;
            }
            backGround.Dispose();
            backGround = null;
        }

        private void PaintHovers(Graphics graphics)
        {
            var usedSpace = new List<Rectangle>();

            if (null == HoverShape)
            {
                return;
            }

            var hover = HoverShape as IHover;

            if (null != hover)
            {
                hover.Hover(usedSpace, null, graphics);
            }
        }
    }
}