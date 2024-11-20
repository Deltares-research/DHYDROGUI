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
        public IShapeFeature ShapeFeature { get; set; }
        protected readonly List<IPoint> points = new List<IPoint>();
        protected IPoint CenterTracker { get; set; }
        protected IChartCoordinateService ChartCoordinateService { get; set; }
        public ShapeEditMode ShapeEditMode { get; set; }

        private IPoint currentTracker;
        public IPoint CurrentTracker 
        { 
            get {return currentTracker;}
            set
            {
                currentTracker = value;
                ShapeFeature.Invalidate();
            } 
        }

        public ShapeFeatureEditor(IShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService, ShapeEditMode shapeEditMode)
        {
            ShapeFeature = shapeFeature;
            ChartCoordinateService = chartCoordinateService;
            ShapeEditMode = shapeEditMode;
        }

        public virtual IEnumerable<IPoint> GetTrackers()
        {
            for (int i = 0; i < points.Count; i++)
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
            for (int i = 0; i <= points.Count - 1; i++)
            {
                // could be generalized with polygon geometry and contains but this is simpler
                Coordinate coordinate = points[i].Coordinates[0];
                if (((x >= (coordinate.X - xMarge)) && (x <= (coordinate.X + xMarge))) &&
                    ((y >= (coordinate.Y - yMarge)) && (y <= (coordinate.Y + yMarge))))
                {
                    return points[i];
                }
            }
            return ShapeFeature.Contains(x, y) ? CenterTracker : null;
        }

        public virtual Cursor GetCursor(IPoint trackerFeature)
        {
            return (null == trackerFeature) ? Cursors.Default : Cursors.SizeAll;
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

            for (int i = 0; i < points.Count; i++)
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

        public virtual void InsertCoordinate(Coordinate worldPosition, double width, double height)
        {
        }

        public virtual void DeleteTracker(IPoint trackerFeature)
        {
        }

        public virtual bool CanDeleteTracker(IPoint trackerFeature)
        {
            return false;
        }

        public virtual void Start()
        {
        }

        public virtual void Stop()
        {
        }

        public bool CanResize
        {
            get { return (ShapeEditMode.ShapeResize == (ShapeEditMode & ShapeEditMode.ShapeResize)); }
        }
        public bool CanMove
        {
            get { return (ShapeEditMode.ShapeMove == (ShapeEditMode & ShapeEditMode.ShapeMove)); }
        }
        public bool CanSelect
        {
            get { return (ShapeEditMode.ShapeSelect == (ShapeEditMode & ShapeEditMode.ShapeSelect)); }
        }

        protected int GetTrackerIndex(IShapeFeatureEditor shapeFeatureEditor, IPoint trackerFeature)
        {
            var v = shapeFeatureEditor.GetTrackers();
            int index = -1;
            foreach (var point in v)
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
            var v = shapeFeatureEditor.GetTrackers();
            int localIndex = -1;
            foreach (var point in v)
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