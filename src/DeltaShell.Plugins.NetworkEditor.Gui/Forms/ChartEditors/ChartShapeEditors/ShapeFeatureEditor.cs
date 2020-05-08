using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors
{
    public class ShapeFeatureEditor : IShapeFeatureEditor
    {
        protected readonly List<IPoint> points = new List<IPoint>();

        private IPoint currentTracker;

        public ShapeFeatureEditor(IShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService, ShapeEditMode shapeEditMode)
        {
            ShapeFeature = shapeFeature;
            ChartCoordinateService = chartCoordinateService;
            ShapeEditMode = shapeEditMode;
        }

        public ShapeEditMode ShapeEditMode { get; set; }

        public bool CanResize
        {
            get
            {
                return ShapeEditMode.ShapeResize == (ShapeEditMode & ShapeEditMode.ShapeResize);
            }
        }

        public bool CanMove
        {
            get
            {
                return ShapeEditMode.ShapeMove == (ShapeEditMode & ShapeEditMode.ShapeMove);
            }
        }

        public bool CanSelect
        {
            get
            {
                return ShapeEditMode.ShapeSelect == (ShapeEditMode & ShapeEditMode.ShapeSelect);
            }
        }

        public IShapeFeature ShapeFeature { get; set; }

        public IPoint CurrentTracker
        {
            get
            {
                return currentTracker;
            }
            set
            {
                currentTracker = value;
                //Debug.WriteLine(value);
                ShapeFeature.Invalidate();
            }
        }

        public virtual IEnumerable<IPoint> GetTrackers()
        {
            for (var i = 0; i < points.Count; i++)
            {
                yield return points[i];
            }

            yield return CenterTracker;
        }

        public virtual bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY)
        {
            return false;
        }

        public virtual IPoint GetTrackerAt(double x, double y, double xMarge, double yMarge)
        {
            for (var i = 0; i <= points.Count - 1; i++)
            {
                // could be generalized with polygon geometry and contains but this is simpler
                Coordinate coordinate = points[i].Coordinates[0];
                if (x >= coordinate.X - xMarge && x <= coordinate.X + xMarge && y >= coordinate.Y - yMarge && y <= coordinate.Y + yMarge)
                {
                    return points[i];
                }
            }

            return ShapeFeature.Contains(x, y) ? CenterTracker : null;
        }

        public virtual Cursor GetCursor(IPoint trackerFeature)
        {
            return null == trackerFeature ? Cursors.Default : Cursors.SizeAll;
        }

        public virtual void Paint(IChart chart, ChartGraphics g)
        {
            if (points.Count == 0)
            {
                return;
            }

            var style = new VectorStyle
            {
                // style is not used; refactor ChartDrawingContext
                Fill = new SolidBrush(Color.FromArgb(150, Color.Magenta)),
                Line = new Pen(Color.Black)
            };

            IChartDrawingContext chartDrawingContext = new ChartDrawingContext(g, style);

            for (var i = 0; i < points.Count; i++)
            {
                Coordinate coordinate = points[i].Coordinates[0];
                int x = ChartCoordinateService.ToDeviceX(coordinate.X);
                int y = ChartCoordinateService.ToDeviceY(coordinate.Y);
                var coordinateRect = new Rectangle(x - 3, y - 3, 6, 6);

                g.BackColor = CurrentTracker == points[i] ? Color.LightGreen : Color.DarkMagenta;
                g.Ellipse(coordinateRect);
            }

            chartDrawingContext.Reset();
        }

        public virtual SnapResult Snap(Coordinate worldPosition, double width, double height)
        {
            return null;
        }

        public virtual void InsertCoordinate(Coordinate worldPosition, double width, double height) {}

        public virtual void DeleteTracker(IPoint trackerFeature) {}

        public virtual bool CanDeleteTracker(IPoint trackerFeature)
        {
            return false;
        }

        public virtual void Start() {}

        public virtual void Stop() {}

        protected IPoint CenterTracker { get; set; }
        protected IChartCoordinateService ChartCoordinateService { get; set; }

        protected int GetTrackerIndex(IShapeFeatureEditor shapeFeatureEditor, IPoint trackerFeature)
        {
            IEnumerable<IPoint> v = shapeFeatureEditor.GetTrackers();
            int index = -1;
            foreach (IPoint point in v)
            {
                index++;
                if (point == trackerFeature)
                {
                    return index;
                }
            }

            return -1;
        }

        protected IPoint GetTracker(IShapeFeatureEditor shapeFeatureEditor, int index)
        {
            IEnumerable<IPoint> v = shapeFeatureEditor.GetTrackers();
            int localIndex = -1;
            foreach (IPoint point in v)
            {
                localIndex++;
                if (localIndex == index)
                {
                    return point;
                }
            }

            return null;
        }
    }
}