using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors
{
    public class RectangleShapeEditor : ShapeFeatureEditor
    {
        public RectangleShapeEditor(IShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService, ShapeEditMode shapeEditMode)
            : base(shapeFeature, chartCoordinateService, shapeEditMode)
        {
            if (CanResize)
            {
                // bottom
                points.Add(new Point(0, 0));
                // right
                points.Add(new Point(0, 0));
                // top
                points.Add(new Point(0, 0));
                // left
                points.Add(new Point(0, 0));
            }

            UpdateRectangleTrackers();
            if (CanMove)
            {
                CenterTracker = new Point(0, 0);
            }
        }

        private void UpdateRectangleTrackers()
        {
            RectangleShapeFeature rectangleShapeFeature = (RectangleShapeFeature)ShapeFeature;
            if (!CanResize) 
                return;
            // bottom
            points[0].X = rectangleShapeFeature.Left + rectangleShapeFeature.Width / 2;
            points[0].Y = rectangleShapeFeature.Bottom;
            // right
            points[1].X = rectangleShapeFeature.Right;
            points[1].Y = rectangleShapeFeature.Bottom + rectangleShapeFeature.Height / 2;
            // top
            points[2].X = rectangleShapeFeature.Left + rectangleShapeFeature.Width / 2;
            points[2].Y = rectangleShapeFeature.Top;
            // left
            points[3].X = rectangleShapeFeature.Left;
            points[3].Y = rectangleShapeFeature.Bottom + rectangleShapeFeature.Height / 2;

            points[0].GeometryChangedAction();
            points[1].GeometryChangedAction();
            points[2].GeometryChangedAction();
            points[3].GeometryChangedAction();
        }



        protected virtual void UpdateTrackers()
        {
            UpdateRectangleTrackers();
        }

        public override bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY)
        {
            RectangleShapeFeature rectangleShapeFeature = (RectangleShapeFeature)ShapeFeature;
            if (CanResize)
            {
                if (trackerFeature == points[0])
                {
                    // bottom
                    rectangleShapeFeature.Bottom += deltaY;
                    GeometryHelper.MoveCoordinate(points[0], 0, 0, deltaY);
                    points[0].GeometryChangedAction();
                    UpdateTrackers();
                    return true;
                }
                if (trackerFeature == points[1])
                {
                    // right
                    rectangleShapeFeature.Right += deltaX;
                    GeometryHelper.MoveCoordinate(points[1], 0, deltaX, 0);
                    points[1].GeometryChangedAction();
                    UpdateTrackers();
                    return true;
                }
                if (trackerFeature == points[2])
                {
                    // top
                    rectangleShapeFeature.Top += deltaY;
                    GeometryHelper.MoveCoordinate(points[2], 0, 0, deltaY);
                    points[2].GeometryChangedAction();
                    UpdateTrackers();
                    return true;
                }
                if (trackerFeature == points[3])
                {
                    // left
                    rectangleShapeFeature.Left += deltaX;
                    GeometryHelper.MoveCoordinate(points[3], 0, deltaX, 0);
                    points[3].GeometryChangedAction();
                    UpdateTrackers();
                    return true;
                }
            }
            if ((CanMove) && (trackerFeature == CenterTracker))
            {
                // center
                rectangleShapeFeature.Left += deltaX;
                rectangleShapeFeature.Top += deltaY;
                rectangleShapeFeature.Right += deltaX;
                rectangleShapeFeature.Bottom += deltaY;
                UpdateTrackers();
                return true;
            }
            return false;
        }

        public override Cursor GetCursor(IPoint trackerFeature)
        {
            if (CanResize)
            {
                if ((trackerFeature == points[0]) || (trackerFeature == points[2]))
                    return Cursors.SizeNS;
                if ((trackerFeature == points[1]) || (trackerFeature == points[3]))
                    return Cursors.SizeWE;
            }
            return (trackerFeature == CenterTracker) ? base.GetCursor(trackerFeature) : Cursors.Default;
        }
    }
}