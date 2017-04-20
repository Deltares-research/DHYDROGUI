using System.Collections.Generic;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapes;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Operation.Distance;
using SharpMap.Converters.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors
{
    public class PolygonEditor : ShapeFeatureEditor
    {
        public PolygonEditor(IShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService, ShapeEditMode shapeEditMode)
            : base(shapeFeature, chartCoordinateService, shapeEditMode)
            // do not call base constructor
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            points.Clear();
            if (((ShapeEditMode & ShapeEditMode.ShapeMove) != ShapeEditMode.ShapeMove) &&
                ((ShapeEditMode & ShapeEditMode.ShapeResize) != ShapeEditMode.ShapeResize))
            {
                // only add Trackers if there something to do.
                return;
            }
            for (int i = 0; i < ShapeFeature.Geometry.Coordinates.Length - 1; i++)
            {
                Coordinate coordinate = ShapeFeature.Geometry.Coordinates[i];
                points.Add(GeometryFactory.CreatePoint(coordinate.X, coordinate.Y));
            }
            CenterTracker = GeometryFactory.CreatePoint(0, 0);
        }

        #region IShapeFeatureEditor Members

        public override bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY)
        {
            int index = points.IndexOf(trackerFeature);
            if (-1 != index)
            {
                GeometryHelper.MoveCoordinate(ShapeFeature.Geometry, index, deltaX, deltaY);
                ShapeFeature.Geometry.GeometryChangedAction();
                if (0 == index)
                {
                    int lastIndex = ShapeFeature.Geometry.Coordinates.Length - 1;
                    GeometryHelper.MoveCoordinate(ShapeFeature.Geometry, lastIndex, deltaX, deltaY);
                }
                // update the tracker
                GeometryHelper.MoveCoordinate(trackerFeature, 0, deltaX, deltaY);
                trackerFeature.GeometryChangedAction();
            }
            else
            {
                if (trackerFeature == CenterTracker)
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        GeometryHelper.MoveCoordinate(ShapeFeature.Geometry, i, deltaX, deltaY);
                        // update the tracker
                        GeometryHelper.MoveCoordinate(points[i], 0, deltaX, deltaY);
                        points[i].GeometryChangedAction();
                    }
                    // update the last coordinate because it is not linked to a tracker
                    GeometryHelper.MoveCoordinate(ShapeFeature.Geometry, ShapeFeature.Geometry.Coordinates.Length - 1, deltaX, deltaY);
                    ShapeFeature.Geometry.GeometryChangedAction();
                }
            }
            return true;
        }

        public override void Stop()
        {
            ShapeFeature.Geometry = (IGeometry)ShapeFeature.Geometry.Clone();
        }

        #endregion

        /// <summary>
        /// Insert a coordinate in the linesegment nearest to worldPosition
        /// </summary>
        /// <param name="worldPosition"></param>
        public override void InsertCoordinate(Coordinate worldPosition, double width, double height)
        {
            /// To avoid odd effects due to an odd aspect ratio first translate to screen coordinates and then the visual 
            /// nearest point is also the nearest point returned by
            List<Coordinate> deviceVertices = new List<Coordinate>();
            List<Coordinate> vertices = new List<Coordinate>();
            for (int i = 0; i < ShapeFeature.Geometry.Coordinates.Length; i++)
            {
                Coordinate coordinate = ShapeFeature.Geometry.Coordinates[i];
                vertices.Add((Coordinate)coordinate.Clone());
                deviceVertices.Add(new Coordinate(ChartCoordinateService.ToDeviceX(coordinate.X), 
                                                                    ChartCoordinateService.ToDeviceY(coordinate.Y)));
            }

            ILineString lineString = GeometryFactory.CreateLineString(deviceVertices.ToArray());
            Coordinate deviceCoordinate = new Coordinate(
                ChartCoordinateService.ToDeviceX(worldPosition.X),
                ChartCoordinateService.ToDeviceY(worldPosition.Y));
             
            //DistanceOp distanceOp = new DistanceOp(ShapeFeature.Geometry, GeometryFactory.CreatePoint(worldPosition));
            DistanceOp distanceOp = new DistanceOp(lineString, GeometryFactory.CreatePoint(deviceCoordinate));
            GeometryLocation[] closestLocations = distanceOp.ClosestLocations();
            if (-1 == closestLocations[0].SegmentIndex) 
                return;
            vertices.Insert(closestLocations[0].SegmentIndex + 1, worldPosition);

            ILinearRing linearRing = GeometryFactory.CreateLinearRing(vertices.ToArray());
            IPolygon polygon = GeometryFactory.CreatePolygon(linearRing, null);
            ShapeFeature.Geometry = polygon;
            Initialize();
            CurrentTracker = points[closestLocations[0].SegmentIndex + 1];
            ShapeFeature.Invalidate();
        }

        public virtual void KeyEvent(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && CurrentTracker != null)
            {
                if (points.Count < 4)
                {
                    return; // smallest valid polygon has 4 vertices. 0 1 2 3=0
                }
                int index = points.IndexOf(CurrentTracker);
                if (-1 == index)
                    return; // centertracker
                List<Coordinate> vertices = new List<Coordinate>();
                for (int i = 0; i < ShapeFeature.Geometry.Coordinates.Length; i++)
                {
                    Coordinate coordinate = ShapeFeature.Geometry.Coordinates[i];
                    vertices.Add((Coordinate)coordinate.Clone());
                }
                if (0 == index) // note that first and last vertex share tracker
                {
                    vertices.RemoveAt(0);
                    vertices.RemoveAt(vertices.Count - 1);
                    vertices.Add((Coordinate)vertices[0].Clone());
                }
                else
                {
                    vertices.RemoveAt(index);
                }
                ILinearRing linearRing = GeometryFactory.CreateLinearRing(vertices.ToArray());
                IPolygon polygon = GeometryFactory.CreatePolygon(linearRing, null);
                ShapeFeature.Geometry = polygon;
                Initialize();
                if (index > (points.Count - 1))
                    CurrentTracker = points[0];
                else
                    CurrentTracker = points[index];
                ShapeFeature.Invalidate();
            }
        }   
    }
}