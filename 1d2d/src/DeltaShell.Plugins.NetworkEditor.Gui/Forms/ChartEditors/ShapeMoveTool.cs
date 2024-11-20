using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Tools;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    public class ShapeMoveTool : ShapeLayerTool
    {
        ShapeSelectTool ShapeSelectTool { get; set; }
        public event ShapeChangedEvendHandler ShapeChanged;

        public IShapeFeatureEditor ShapeFeatureEditor
        {
            get
            {
                return ShapeSelectTool.ShapeFeatureEditor;
            }
            set
            {
                ShapeSelectTool.ShapeFeatureEditor = value;
            }
        }

        public ShapeMoveTool(ShapeSelectTool shapeSelectTool)
        {
            ShapeSelectTool = shapeSelectTool;
        }

        Point Down { get; set; }


        public override void MouseEvent(ChartMouseEvent kind, MouseEventArgs e, Cursor c)
        {
            Point tmP = new Point(e.X, e.Y);
            if (null != ShapeSelectTool.ShapeFeatureEditor)
            {
                IPoint tracker = ShapeSelectTool.GetTrackerAt(ShapeSelectTool.ShapeFeatureEditor, tmP);
                c = ShapeSelectTool.ShapeFeatureEditor.GetCursor(tracker);
                ShapeModifyTool.Chart.CancelMouseEvents = true; // prevent other tools to reset the mouse.
            }
            else
            {
                c = Cursors.Default;
            }
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            switch (kind)
            {
                case ChartMouseEvent.Down:
                    Down = tmP;
                    ShapeSelectTool.MouseEvent(kind, e, c);
                    if (ShapeSelectTool.ShapeFeatureEditor != null)
                    {
                        ShapeSelectTool.ShapeFeatureEditor.Start();
                        IsBusy = true;
                    }
                    break;
                case ChartMouseEvent.Move:
                    {
                        if (!IsBusy)
                        {
                            return;
                        }
                        if ((null != ShapeSelectTool.ShapeFeatureEditor) && (null != ShapeSelectTool.ShapeFeatureEditor.CurrentTracker))
                        {
                            Coordinate coordinate = new Coordinate(
                                ChartCoordinateService.ToWorldX(ShapeModifyTool.Chart, tmP.X), ChartCoordinateService.ToWorldY(ShapeModifyTool.Chart, tmP.Y));
                            double deltaX = ChartCoordinateService.ToWorldX(ShapeModifyTool.Chart, tmP.X) - ChartCoordinateService.ToWorldX(ShapeModifyTool.Chart, Down.X);
                            double deltaY = ChartCoordinateService.ToWorldY(ShapeModifyTool.Chart, tmP.Y) - ChartCoordinateService.ToWorldY(ShapeModifyTool.Chart, Down.Y);
                            if ((Math.Abs(deltaX) < 1.0e-6) && (Math.Abs(deltaY) < 1.0e-6))
                            {
                                return;
                            }
                            ShapeFeatureEditor.MoveTracker(ShapeFeatureEditor.CurrentTracker, coordinate, deltaX, deltaY);
                            ShapeModifyTool.Chart.CancelMouseEvents = true;
                            ((ShapeFeatureEditor)ShapeFeatureEditor).ShapeFeature.Invalidate();
                        }
                        Down = tmP;
                    }
                    break;
                case ChartMouseEvent.Up:
                    if (null != ShapeFeatureEditor)
                    {
                        ShapeFeatureEditor.Stop();
                    }
                    if (IsBusy)
                    {
                        ShapeModifyTool.Chart.CancelMouseEvents = true;
                        IsBusy = false;
                        if (null != ShapeChanged)
                        {
                            ShapeChanged(this, new ShapeEventArgs(ShapeModifyTool.SelectedShape));
                        }
                    }
                    break;
            }
        }
    }
}