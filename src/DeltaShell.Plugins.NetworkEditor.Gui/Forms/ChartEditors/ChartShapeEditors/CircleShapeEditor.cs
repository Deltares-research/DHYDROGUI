using System;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors
{
    public class CircleShapeEditor : ShapeFeatureEditor, IShapeFeatureEditor
    {
        private int collapsed = 1;


        public CircleShapeEditor(IShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService, ShapeEditMode shapeEditMode)
            : base(shapeFeature, chartCoordinateService, shapeEditMode)
        {
            Initialize();
        }

        private void Initialize()
        {
            points.Clear();
            if (((ShapeEditMode & ShapeEditMode.ShapeMove) != ShapeEditMode.ShapeMove) &&
                ((ShapeEditMode & ShapeEditMode.ShapeResize) != ShapeEditMode.ShapeResize))
            {
                // only add Trackers if there something to do.
                return;
            }

            var circleShapeFeature = (CircleShapeFeature)ShapeFeature;
            // bottom
            points.Add(new Point(
                circleShapeFeature.Center.X,
                circleShapeFeature.Center.Y + circleShapeFeature.YRadius / 2));
            // right
            points.Add(new Point(
                circleShapeFeature.Center.X + circleShapeFeature.XRadius / 2,
                circleShapeFeature.Center.Y));
            // top
            points.Add(new Point(
                circleShapeFeature.Center.X,
                circleShapeFeature.Center.Y - circleShapeFeature.YRadius / 2));
            // left
            points.Add(new Point(
                circleShapeFeature.Center.X - circleShapeFeature.XRadius / 2,
                circleShapeFeature.Center.Y));
            CenterTracker = new Point(0, 0);
        }

        #region IShapeFeatureEditor Members


        public bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY)
        {
            CircleShapeFeature circleShapeFeature = (CircleShapeFeature)ShapeFeature;
            if (trackerFeature == points[0])
            {
                // bottom
                circleShapeFeature.YRadius += deltaY * 2 * collapsed;
                GeometryHelper.MoveCoordinate(points[0], 0, 0, deltaY);
                points[0].GeometryChangedAction();
                // also update top
                GeometryHelper.MoveCoordinate(points[2], 0, 0, -deltaY);
                points[2].GeometryChangedAction();
            }
            else if (trackerFeature == points[1])
            {
                // right
                circleShapeFeature.XRadius += deltaX * 2 * collapsed;
                GeometryHelper.MoveCoordinate(points[1], 0, deltaX, 0);
                points[1].GeometryChangedAction();
                // also update left
                GeometryHelper.MoveCoordinate(points[3], 0, -deltaX, 0);
                points[3].GeometryChangedAction();
            }
            else if (trackerFeature == points[2])
            {
                // top
                circleShapeFeature.YRadius -= deltaY * 2 * collapsed;
                GeometryHelper.MoveCoordinate(points[2], 0, 0, deltaY);
                points[2].GeometryChangedAction();
                // also update bottom
                GeometryHelper.MoveCoordinate(points[0], 0, 0, -deltaY);
                points[0].GeometryChangedAction();
            }
            else if (trackerFeature == points[3])
            {
                // left
                circleShapeFeature.XRadius -= deltaX * 2 * collapsed;
                GeometryHelper.MoveCoordinate(points[3], 0, deltaX, 0);
                points[3].GeometryChangedAction();
                // also update right
                GeometryHelper.MoveCoordinate(points[1], 0, -deltaX, 0);
                points[1].GeometryChangedAction();
            }
            else
            {
                // center
                circleShapeFeature.Center.X += deltaX;
                circleShapeFeature.Center.Y += deltaY;
                // update all Trackers
                for (int i = 0; i < points.Count; i++)
                {
                    GeometryHelper.MoveCoordinate(points[i], 0, deltaX, deltaY);
                    points[i].GeometryChangedAction();
                }
            }
            if (circleShapeFeature.XRadius < 0)
            {
                circleShapeFeature.XRadius = Math.Abs(circleShapeFeature.XRadius);
                collapsed *= -1;
            }
            if (circleShapeFeature.YRadius < 0)
            {
                circleShapeFeature.YRadius = Math.Abs(circleShapeFeature.YRadius);
                collapsed *= -1;
            }
            return true;
        }

        public override Cursor GetCursor(IPoint trackerFeature)
        {
            if ((trackerFeature == points[0]) || (trackerFeature == points[2]))
                return Cursors.SizeNS;
            if ((trackerFeature == points[1]) || (trackerFeature == points[3]))
                return Cursors.SizeWE;
            return base.GetCursor(trackerFeature);
        }

        #endregion
    }
}