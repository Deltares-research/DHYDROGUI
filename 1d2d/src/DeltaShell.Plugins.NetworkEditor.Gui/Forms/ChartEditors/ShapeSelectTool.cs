using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Tools;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    public class ShapeSelectTool : ShapeLayerTool
    {
        public IShapeFeatureEditor ShapeFeatureEditor
        {
            get
            {
                return ShapeModifyTool.ShapeFeatureEditor;
            }
            set
            {
                ShapeModifyTool.ShapeFeatureEditor = value;
            }
        }

        public IPoint GetTrackerAt(IShapeFeatureEditor shapeFeatureEditor, Point point)
        {
            const int tolerance = 4;
            double worldX = ChartCoordinateService.ToWorldX(ShapeModifyTool.Chart, point.X);
            double worldY = ChartCoordinateService.ToWorldY(ShapeModifyTool.Chart, point.Y);
            double worldWidth = ChartCoordinateService.ToWorldWidth(ShapeModifyTool.Chart, tolerance);
            double worldHeight = ChartCoordinateService.ToWorldHeight(ShapeModifyTool.Chart, tolerance);
            return shapeFeatureEditor.GetTrackerAt(worldX, worldY, worldWidth, worldHeight);
        }


        public override void MouseEvent(ChartMouseEvent kind, MouseEventArgs e, Cursor c)
        {
            Point tmP = new Point(e.X, e.Y);
            switch (kind)
            {
                case ChartMouseEvent.Down:
                    if (e.Button == MouseButtons.Left)
                    {
                        if (null != ShapeFeatureEditor)
                        {
                            IPoint tracker = GetTrackerAt(ShapeFeatureEditor, tmP);
                            if (null != tracker)
                            {
                                ShapeModifyTool.Chart.CancelMouseEvents = true;
                                ShapeFeatureEditor.CurrentTracker = tracker;
                                return;
                            }
                            ShapeFeatureEditor = null;
                            ShapeModifyTool.SelectedShape = null;
                        }
                        IShapeFeature selectedShape = ShapeModifyTool.Clicked(tmP.X, tmP.Y);
                        SelectShape(selectedShape);
                        ShapeModifyTool.Invalidate();
                    }
                    break;
            }
        }

        public void SelectShape(IShapeFeature shapeFeature)
        {
            if (null == shapeFeature)
            {
                ShapeModifyTool.SelectedShape = null;
                ShapeFeatureEditor = null;
            }
            else
            {
                if (shapeFeature.Active)
                {
                    ShapeModifyTool.SelectedShape = shapeFeature;
                    shapeFeature.Selected = true;
                    ShapeFeatureEditor = shapeFeature.CreateShapeFeatureEditor(ShapeModifyTool.ShapeEditMode);
                }
            }
        }

        public void Clear()
        {
            ShapeFeatureEditor = null;
        }
    }
}