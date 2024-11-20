using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls.Swf.Charting;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.ChartShapeEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapes;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;
using SharpMap.Api.Editors;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors.StructureChartShapeEditors
{
    public class FreeFormatWeirEditor : PolygonEditor
    {
        FreeFormatWeirShapeFeature FreeFormatWeirShapeFeature { get; set; }

        public FreeFormatWeirEditor(FreeFormatWeirShapeFeature shapeFeature, IChartCoordinateService chartCoordinateService, ShapeEditMode shapeEditMode) 
            : base(shapeFeature, chartCoordinateService, shapeEditMode)
        {
            FreeFormatWeirShapeFeature = shapeFeature;
        }

        protected override void Initialize()
        {
            CreateTrackers();
        }

        private void CreateTrackers()
        {
            points.Clear();
            IGeometry geometry = ((FreeFormatWeirShapeFeature)ShapeFeature).PolygonShapeFeature.Geometry;
            for (int i = 2; i < geometry.Coordinates.Length - 1; i++)
            {
                Coordinate coordinate = geometry.Coordinates[i];
                points.Add(new Point(coordinate.X, coordinate.Y));
            }
            CenterTracker = new Point(0, 0);
        }

        private void ChangeValue(double oldValue, double newValue, double value)
        {
            var sel = FreeFormatWeirShapeFeature.CrestShape.Where(c => c.X == oldValue).FirstOrDefault();
            sel.X = newValue;
            sel.Y = value;
        }

        public override bool MoveTracker(IPoint trackerFeature, Coordinate worldPosition, double deltaX, double deltaY)
        {
            int index = points.IndexOf(trackerFeature);
            IGeometry geometry = FreeFormatWeirShapeFeature.PolygonShapeFeature.Geometry;
            if (-1 != index)
            {
                int moveIndex = index + 2;
                // prevent to move before or after predecessor of successor
                if (moveIndex > 2)
                {
                    double maxDelta = geometry.Coordinates[moveIndex - 1].X -
                                      geometry.Coordinates[moveIndex].X + 1.0e-6;
                    deltaX = Math.Max(deltaX, maxDelta);
                }
                if (moveIndex < geometry.Coordinates.Length - 2)
                {
                    double maxDelta = geometry.Coordinates[moveIndex + 1].X -
                                      geometry.Coordinates[moveIndex].X - 1.0e-6;
                    deltaX = Math.Min(deltaX, maxDelta);
                }

                GeometryHelper.MoveCoordinate(geometry, moveIndex, deltaX, deltaY);
                geometry.GeometryChangedAction();

                // Note: the startpoint/ endpoint is not directly editable
                FreeFormatWeirShapeFeature.ChangeValue(moveIndex,
                    trackerFeature.X + deltaX - FreeFormatWeirShapeFeature.Weir.OffsetY,
                    trackerFeature.Y + deltaY);
                GeometryHelper.MoveCoordinate(trackerFeature, 0, deltaX, deltaY);
                trackerFeature.GeometryChangedAction();

                if (index == 0)
                {
                    // synchronize with left bottom
                    GeometryHelper.MoveCoordinate(geometry, 1, deltaX, 0);
                }
                else if (index == points.Count - 1)
                {
                    // synchronize with rigth bottom = start and endpoint
                    GeometryHelper.MoveCoordinate(geometry, 0, deltaX, 0);
                    GeometryHelper.MoveCoordinate(geometry, geometry.Coordinates.Length - 1, deltaX, 0);
                }
                UpdateEnvelopeInternal(geometry);

                ChangeValue(trackerFeature.X - FreeFormatWeirShapeFeature.Weir.OffsetY,
                    trackerFeature.X + deltaX - FreeFormatWeirShapeFeature.Weir.OffsetY,
                    trackerFeature.Y + deltaY);

                return true;
            }

            if (trackerFeature == CenterTracker)
            {
                // Move all
                int length = geometry.Coordinates.Length;
                for (int i = 0; i < length; i++)
                {
                    GeometryHelper.MoveCoordinate(geometry, i, deltaX, deltaY);
                    // update the tracker
                    if ((i > 1) && (i < (length - 1)))
                    {
                        GeometryHelper.MoveCoordinate(points[i - 2], 0, deltaX, deltaY);
                        points[i - 2].GeometryChangedAction();
                    }
                }
            }
            CreateTrackers();
            UpdateEnvelopeInternal(geometry);
            //ShapeFeature.Geometry.GeometryChangedAction();
            return true;           
        }

        ///// <summary>
        ///// Hack: Updates the internal envelope after a move operation.
        ///// </summary>
        ///// <param name="geometry"></param>
        private static void UpdateEnvelopeInternal(IGeometry geometry)
        {
            Coordinate[] coordinates = geometry.Coordinates;

            if (coordinates.Length > 1)
            {
                for (int i = 0; i < coordinates.Length; i++)
                {
                    if (0 == i)
                    {
                        geometry.EnvelopeInternal.Init(coordinates[i]);
                    }
                    else
                    {
                        geometry.EnvelopeInternal.ExpandToInclude(coordinates[i]);
                    }
                }
            }
        }

        public override SnapResult Snap(Coordinate worldPosition, double width, double height)
        {
            // do not allow snapping outside the curent width of the weir
            IGeometry geometry = FreeFormatWeirShapeFeature.PolygonShapeFeature.Geometry;
            if (worldPosition.X <= (points[0].X + 1.0e-6))
            {
                return null;
            }
            if (worldPosition.X >= (points[points.Count - 1].X - 1.0e-6))
            {
                return null;
            }
            IPoint tracker = GetTrackerAt(worldPosition.X, worldPosition.Y, width, height);
            if ((tracker == CenterTracker) || (null == tracker))
            {
                int index = points.IndexOf(points.Where(p => p.X > worldPosition.X).FirstOrDefault());
                SnapResult snapResult = new SnapResult(worldPosition, null, null, geometry, index - 1, index);

                IList<Coordinate> coordinates = new List<Coordinate>
                                                     {
                                                         (Coordinate) points[index - 1].Coordinates[0].Clone(),
                                                         (Coordinate) worldPosition.Clone(),
                                                         (Coordinate) points[index].Coordinates[0].Clone()
                                                     };
                snapResult.VisibleSnaps.Add(new LineString(coordinates.ToArray()));
                return snapResult;
            }
            return null;
        }

        /// <summary>
        /// Insert a coordinate in the linesegment nearest to worldPosition
        /// </summary>
        /// <param name="worldPosition"></param>
        public override void InsertCoordinate(Coordinate worldPosition, double width, double height)
        {
            IGeometry geometry = FreeFormatWeirShapeFeature.PolygonShapeFeature.Geometry;
            // To avoid odd effects due to an odd aspect ratio first translate to screen coordinates and then the visual 
            // nearest point is also the nearest point returned by
            if (!(geometry is IPolygon))
            {
                throw new ArgumentException("Geometry is not a polygon");
            }

            SnapResult snapResult = Snap(worldPosition, width, height);
            if (null == snapResult)
                return;
            IList<Coordinate> vertices = new List<Coordinate>();
            for (int i = 0; i < FreeFormatWeirShapeFeature.CrestShape.Count; i++)
            {
                Coordinate clone = (Coordinate) FreeFormatWeirShapeFeature.CrestShape[i].Clone();
                vertices.Add(clone);
            }
            vertices.Add(new Coordinate(worldPosition.X - FreeFormatWeirShapeFeature.Weir.OffsetY,
                                        worldPosition.Y));


            FreeFormatWeirShapeFeature.CrestShape = vertices.Select(s => s).OrderBy(s => s.X).ToList();
            FreeFormatWeirShapeFeature.UpdateGeometry();
            CreateTrackers();
            CurrentTracker = points[snapResult.SnapIndexNext];
            ShapeFeature.Invalidate();
        }

        public override void DeleteTracker(IPoint trackerFeature)
        {
            if (CanDeleteTracker(trackerFeature))
            {
                IGeometry geometry = FreeFormatWeirShapeFeature.PolygonShapeFeature.Geometry;

                if (!(geometry is IPolygon))
                {
                    throw new ArgumentException("Geometry is not a polygon");
                }

                double xValue = trackerFeature.X - FreeFormatWeirShapeFeature.Weir.OffsetY;
                FreeFormatWeirShapeFeature.CrestShape =
                    FreeFormatWeirShapeFeature.CrestShape.Where(c => c.X != xValue).ToArray();
                FreeFormatWeirShapeFeature.UpdateGeometry();
                CreateTrackers();
                ShapeFeature.Invalidate();
            }
        }

        public override bool CanDeleteTracker(IPoint trackerFeature)
        {
            // check if tracker belongs to this editor?
            if (trackerFeature == CenterTracker)
            {
                return false;
            }
            if (trackerFeature == points[0])
            {
                return false;
            }
            if (trackerFeature == points[points.Count - 1])
            {
                return false;
            }
            return true;
        }

        public override void Start()
        {
            // No start logic needed
        }

        public override void Stop()
        {
            // No stop logic needed
        }
    }
}