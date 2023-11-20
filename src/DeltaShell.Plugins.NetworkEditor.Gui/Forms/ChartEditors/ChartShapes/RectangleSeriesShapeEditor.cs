using System;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using SharpMap.Styles;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes
{
    /// <summary>
    /// Implements a ShapeFeatureEditor to modify a RectangleSeriesShapeFeature
    /// 
    /// </summary>
    public class RectangleSeriesShapeEditor : ShapeFeatureEditor
    {
        private RectangleSeriesShapeFeature RectangleSeriesShapeFeature { get; set; }

        public RectangleSeriesShapeEditor(IShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService, ShapeEditMode shapeEditMode) 
            : base(shapeFeature, chartCoordinateService, shapeEditMode)
        {
            RectangleSeriesShapeFeature = (RectangleSeriesShapeFeature) shapeFeature;
            foreach (FixedRectangleShapeFeature borderShape in RectangleSeriesShapeFeature.BorderShapes)
            {
                Rectangle rectangle = borderShape.GetBounds();
                double worldHeight = chartCoordinateService.ToWorldHeight(rectangle.Height);
                points.Add(new NetTopologySuite.Geometries.Point(borderShape.X, RectangleSeriesShapeFeature.Y + worldHeight / 2));
            }
        }

        public override Cursor GetCursor(IPoint trackerFeature)
        {
            return points.Contains(trackerFeature) ? Cursors.VSplit : base.GetCursor(trackerFeature);
        }

        /// <summary>
        /// do not use points.IndexOf(trackerFeature); it uses internally Equals ands will always return the first tracker in the
        /// list that is in a identical position
        /// see also GeometryHelper.IndexOfGeometry()
        /// </summary>
        /// <param name="tracker"></param>
        /// <returns></returns>
        private int GetTrackerIndex(IPoint tracker)
        {
            for (int i=0; i < points.Count; i++)
            {
                if (points[i] == tracker)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Moves tracker trackerFeature to new position worldPosition. 
        /// see additional comments for extra restrictions
        /// </summary>
        /// <param name="trackerFeature"></param>
        /// <param name="worldPosition"></param>
        /// <param name="deltaX"></param>
        /// <param name="deltaY"></param>
        /// <returns></returns>
        public override bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY)
        {
            var current = GetTrackerIndex(trackerFeature);
            GetTrackerIndex(trackerFeature);
            double min = RectangleSeriesShapeFeature.X;
            double max = RectangleSeriesShapeFeature.X + RectangleSeriesShapeFeature.Width;
            if (current > 0)
            {
                // the new minimum value is the X value of the previous rectangle
                min = points[current - 1].X;
            }
            if (current < points.Count - 1)
            {
                // the new maximum value is the X value of the next rectangle
                max = points[current + 1].X;
            }
            double newX = worldPosition.X;
            newX = Math.Min(max, newX);            
            newX = Math.Max(min, newX);
            double limitedDelateX = newX - points[current].X;
            GeometryHelper.MoveCoordinate(points[current], 0, limitedDelateX, 0);
            points[current].GeometryChangedAction();
            RectangleSeriesShapeFeature.SetBorder(current, points[current].X);
            return true;
        }

        /// <summary>
        /// Gets tracker at position x, y. Internally Trackers are represented as IPoint. For the RectangleSeriesShapeFeature
        /// the Trackers are in fact horizontal/vertical line.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="xMarge"></param>
        /// <param name="yMarge"></param>
        /// <returns></returns>
        public override IPoint GetTrackerAt(double x, double y, double xMarge, double yMarge)
        {
            if (y > RectangleSeriesShapeFeature.Top)
            {
                // mouse is outside composite; ignore.
                return null;
            }
            if (y < (RectangleSeriesShapeFeature.Y - ChartCoordinateService.ToWorldHeight((int)RectangleSeriesShapeFeature.Height)))
            {
                // mouse is outside composite; ignore.
                return null;
            }
            // only check for x : vertical line
            for (int i = 0; i <= points.Count - 1; i++)
            {
                // could be generalized with polygon geometry and contains but this is simpler
                Coordinate coordinate = points[i].Coordinates[0];
                if (((x >= (coordinate.X - xMarge)) && (x <= (coordinate.X + xMarge))))
                {
                    return points[i];
                }
            }
            return ShapeFeature.Contains(x, y) ? CenterTracker : null;
        }

        public override void Paint(IChart chart, ChartGraphics g)
        {
            if (points.Count == 0)
            {
                return;
            }
            VectorStyle style = new VectorStyle
                                    {
                                        // style is not used; refactor ChartDrawingContext
                                        Fill = new SolidBrush(Color.FromArgb(150, Color.Magenta)),
                                        Line = new Pen(Color.Black)
                                    };
            IChartDrawingContext chartDrawingContext = new ChartDrawingContext(g, style);

            for (int i = 0; i < points.Count; i++)
            {
                g.BackColor = CurrentTracker == points[i] ? Color.FromArgb(255, Color.DarkMagenta) : Color.FromArgb(50, Color.DarkMagenta);
                g.PenColor = CurrentTracker == points[i] ? Color.FromArgb(255, Color.DarkMagenta) : Color.FromArgb(50, Color.DarkMagenta);
                // draw tracker not as a small circle (see base class) but as a rectangle/line.
                g.Rectangle(RectangleSeriesShapeFeature.BorderShapes[i].GetBounds());
            }
            chartDrawingContext.Reset();
        }
    }
}