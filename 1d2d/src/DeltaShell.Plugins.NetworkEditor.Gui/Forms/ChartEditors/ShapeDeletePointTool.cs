using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting.Tools;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    public class ShapeDeletePointTool : ShapeLayerTool
    {
        public override void MouseEvent(ChartMouseEvent kind, MouseEventArgs e, Cursor c)
        {
            Point tmP = new Point(e.X, e.Y);

            switch (kind)
            {
                case ChartMouseEvent.Down:
                    {
                        if (e.Button != MouseButtons.Left)
                        {
                            return;
                        }
                        if (null != ShapeModifyTool.ShapeFeatureEditor)
                        {
                            IPoint tracker = ShapeModifyTool.ShapeSelectTool.GetTrackerAt(ShapeModifyTool.ShapeFeatureEditor, tmP);
                            if (null != tracker && ShapeModifyTool.ShapeFeatureEditor.CanDeleteTracker(tracker))
                            {
                                ShapeModifyTool.ShapeFeatureEditor.DeleteTracker(tracker);
                                ((ShapeFeatureEditor)ShapeModifyTool.ShapeFeatureEditor).ShapeFeature.Invalidate();
                                return;
                            }
                        }
                        ShapeModifyTool.ShapeSelectTool.MouseEvent(kind, e, c);
                    }
                    break;
                case ChartMouseEvent.Move:
                    if (null != ShapeModifyTool.ShapeFeatureEditor)
                    {
                        IPoint tracker = ShapeModifyTool.ShapeSelectTool.GetTrackerAt(ShapeModifyTool.ShapeFeatureEditor, tmP);
                        if (null != tracker && ShapeModifyTool.ShapeFeatureEditor.CanDeleteTracker(tracker))
                        {
                            c = ShapeModifyTool.ShapeFeatureEditor.GetCursor(tracker);
                            return;
                        }
                        c = Cursors.Default;
                    }
                    break;
            }
        }
    }
}