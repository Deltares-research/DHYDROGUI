using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Tools;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors
{
    public class ShapeInsertPointTool : ShapeLayerTool
    {
        private bool isActive;
        public ShapeMoveTool ShapeMoveTool { get; set; }

        public override bool IsActive
        {
            get { return isActive; }
            set 
            { 
                isActive = value;
                SnapResult = null;
            }
        }

        private SnapResult SnapResult { get; set; }

        public override void Paint()
        {
            if (SnapResult != null)
            {
                VectorStyle vectorStyle = new VectorStyle
                                              {
                                                  Fill = new SolidBrush(Color.FromArgb(150, Color.DeepSkyBlue)),
                                                  Line = new Pen(Color.DarkGray, 100)
                                              };

                ChartDrawingContext vectorStyle2Graphics3D = new ChartDrawingContext(ShapeModifyTool.Chart.Graphics, vectorStyle);
                for (int i = 0; i < SnapResult.VisibleSnaps.Count; i++)
                {
                    IGeometry geometry = SnapResult.VisibleSnaps[i];
                    ShapeModifyTool.Chart.Graphics.MoveTo(
                        ChartCoordinateService.ToDeviceX(ShapeModifyTool.Chart, geometry.Coordinates[0].X),
                        ChartCoordinateService.ToDeviceY(ShapeModifyTool.Chart, geometry.Coordinates[0].Y));
                    for (int j = 1; j < geometry.Coordinates.Length; j++)
                    {
                        ShapeModifyTool.Chart.Graphics.LineTo(
                            ChartCoordinateService.ToDeviceX(ShapeModifyTool.Chart, geometry.Coordinates[j].X),
                            ChartCoordinateService.ToDeviceY(ShapeModifyTool.Chart, geometry.Coordinates[j].Y));
                    }
                }
                vectorStyle2Graphics3D.Reset();
            }
        }

        public override void MouseEvent(ChartMouseEvent kind, MouseEventArgs e, Cursor c)
        {
            if (null != ShapeModifyTool.ShapeFeatureEditor)
            {
                Point tmP = new Point(e.X, e.Y);
                const int tolerance = 4;
                double worldWidth = ChartCoordinateService.ToWorldWidth(ShapeModifyTool.Chart, tolerance);
                double worldHeight = ChartCoordinateService.ToWorldHeight(ShapeModifyTool.Chart, tolerance);
                Coordinate coordinate = new Coordinate(ChartCoordinateService.ToWorldX(ShapeModifyTool.Chart, tmP.X) ,ChartCoordinateService.ToWorldX(ShapeModifyTool.Chart, tmP.Y ));

                switch (kind)
                {
                    case ChartMouseEvent.Down:
                        {
                            if (e.Button != MouseButtons.Left)
                            {
                                return;
                            }
                            SnapResult snapResult = Snap(ShapeModifyTool.ShapeFeatureEditor, coordinate);
                            if (null != snapResult)
                            {
                                ShapeModifyTool.ShapeFeatureEditor.InsertCoordinate(coordinate, worldWidth, worldHeight);
                                return;
                            }
                            IPoint tracker = ShapeModifyTool.ShapeSelectTool.GetTrackerAt(ShapeModifyTool.ShapeFeatureEditor, tmP);
                            if (null != tracker)
                            {
                                // if a tracker is selected process as move command
                                ShapeModifyTool.ShapeMoveTool.MouseEvent(kind, e, c);
                            }
                            else
                            {
                                ShapeModifyTool.ShapeSelectTool.MouseEvent(kind, e, c);
                                if (null != ShapeModifyTool.ShapeFeatureEditor)
                                    ShapeModifyTool.ShapeFeatureEditor.Stop();
                            }
                        }
                        break;
                    case ChartMouseEvent.Move:
                        {
                            if (ShapeModifyTool.ShapeMoveTool.IsBusy)
                            {
                                ShapeModifyTool.ShapeMoveTool.MouseEvent(kind, e, c);
                                return;
                            }
                            SnapResult = Snap(ShapeModifyTool.ShapeFeatureEditor, coordinate);
                            ((ShapeFeatureEditor)ShapeModifyTool.ShapeFeatureEditor).ShapeFeature.Invalidate();
                            if (null == SnapResult)
                            {
                                ShapeModifyTool.ShapeMoveTool.MouseEvent(kind, e, c);
                            }
                        }
                        break;
                    case ChartMouseEvent.Up:
                        if (ShapeModifyTool.ShapeMoveTool.IsBusy)
                        {
                            ShapeModifyTool.ShapeMoveTool.MouseEvent(kind, e, c);
                            return;
                        }
                        break;
                }
            }
            else
            {
                ShapeModifyTool.ShapeSelectTool.MouseEvent(kind, e, c);
                if (kind == ChartMouseEvent.Down &&
                    ShapeModifyTool.ShapeSelectTool.ShapeFeatureEditor != null)
                {
                    ShapeModifyTool.ShapeFeatureEditor?.Start();
                }
            }
        }
    }
}