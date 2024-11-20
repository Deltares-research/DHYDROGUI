using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Hydro.Structures;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.Api.Editors;
using SharpMap.Data.Providers;
using SharpMap.Editors.Snapping;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.UI.Tools;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public class LeveeBreachMapTool: Feature2DLineTool
    {
        private bool movingBreachLocation = false;
        private IPoint newBreachLocationPoint;
        private IFeature newBreachLocationFeature;
        private VectorLayer newBreachLocationLayer;
        private readonly Collection<IGeometry> newPointFeatureGeometry = new Collection<IGeometry>();
        private VectorStyle pointFeatureStyle;
        private VectorStyle errorPointFeatureStyle;
        private SnapResult snapResult;
        private ILeveeBreach selectedLeveeBreach;
        private double hitAreaDistance = 25;

        public LeveeBreachMapTool(string targetLayerName, string name, Bitmap icon) : base(targetLayerName, name, icon)
        {
        }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if(IsOnBreachLocation(worldPosition))
            {
                StartMovingBreachLocation(worldPosition);
            }
            else
            {
                base.OnMouseDown(worldPosition, e);
            }

        }

        public override void OnMouseMove(Coordinate worldPosition, MouseEventArgs e)
        {
            if (movingBreachLocation)
            {
                MoveBreachLocation(worldPosition, e);
            }
            else
            {
                base.OnMouseMove(worldPosition, e);
            }
        }

        public override void OnMouseUp(Coordinate worldPosition, MouseEventArgs e)
        {
            if (movingBreachLocation)
            {
                StopMovingBreachLocation(worldPosition);
            }
            else
            {
                base.OnMouseUp(worldPosition, e);
            }
        }

        public override void StartDrawing()
        {
            base.StartDrawing();
            AddDrawingLayer();
        }

        public override void StopDrawing()
        {
            base.StopDrawing();
            RemoveDrawingLayer();
        }

        public override void Render(Graphics graphics)
        {
            if (movingBreachLocation) return;

            base.Render(graphics);
        }

        private bool IsOnBreachLocation(Coordinate worldPosition)
        {
            var c1 = new Coordinate(worldPosition.X - hitAreaDistance, worldPosition.Y - hitAreaDistance);
            var c2 = new Coordinate(worldPosition.X + hitAreaDistance, worldPosition.Y + hitAreaDistance);
            var hitArea = new Envelope(c1, c2);
            foreach (var feature in VectorLayer.DataSource.Features)
            {
                var leveeBreach = feature as ILeveeBreach;
                if (leveeBreach?.BreachLocation != null && hitArea.Intersects(leveeBreach.BreachLocation.Coordinate))
                {
                    selectedLeveeBreach = leveeBreach;
                    return true;
                }
            }
            return false;
        }


        private void StartMovingBreachLocation(Coordinate worldPosition)
        {
            movingBreachLocation = true;
            StartDrawing();
            newBreachLocationPoint = new NetTopologySuite.Geometries.Point(worldPosition);
            ((DataTableFeatureProvider)newBreachLocationLayer.DataSource).Clear();
            newBreachLocationLayer.DataSource.Add(newBreachLocationPoint);
            newBreachLocationFeature = newBreachLocationLayer.DataSource.GetFeature(0);

            SetMovingFeatureCoordinates(worldPosition);
        }

        private void SetMovingFeatureCoordinates(Coordinate worldPosition)
        {
            var snapRule = new SnapRule {Obligatory = true, SnapRole = SnapRole.FreeAtObject, PixelGravity = 8};

            snapResult = MapControl.SnapTool.ExecuteSnapRule(snapRule, newBreachLocationFeature, newBreachLocationPoint,
                new List<IFeature>() {selectedLeveeBreach}, worldPosition, -1);

            if (snapResult != null)
            {
                newBreachLocationPoint.Coordinates[0].X = snapResult.Location.X;
                newBreachLocationPoint.Coordinates[0].Y = snapResult.Location.Y;
            }

            if (newBreachLocationLayer != null)
                newBreachLocationLayer.Style = MapControl.SnapTool.Failed ? errorPointFeatureStyle : pointFeatureStyle;
        }

        private void MoveBreachLocation(Coordinate worldPosition, MouseEventArgs mouseEventArgs)
        {
            if (VectorLayer == null)
            {
                return;
            }

            //to avoid listening to the mousewheel in the mean time
            if (AdditionalButtonsBeingPressed(mouseEventArgs))
                return;

            StartDrawing();
            SetMovingFeatureCoordinates(worldPosition);
            DoDrawing(true);
            StopDrawing();
        }


        private void StopMovingBreachLocation(Coordinate worldPosition)
        {
            StopDrawing();
            SetMovingFeatureCoordinates(worldPosition);
            selectedLeveeBreach.BreachLocationX = newBreachLocationPoint.Coordinates[0].X;
            selectedLeveeBreach.BreachLocationY = newBreachLocationPoint.Coordinates[0].Y;
            MapControl.Refresh();
            movingBreachLocation = false;
        }

        private void AddDrawingLayer()
        {
            newBreachLocationLayer = new VectorLayer(VectorLayer) { Name = "newBreachLocation", Map = VectorLayer.Map };

            DataTableFeatureProvider trackingProvider = new DataTableFeatureProvider(newPointFeatureGeometry);
            newBreachLocationLayer.DataSource = trackingProvider;

            pointFeatureStyle = (VectorStyle)newBreachLocationLayer.Style.Clone();
            errorPointFeatureStyle = (VectorStyle)newBreachLocationLayer.Style.Clone();
            SharpMap.UI.Forms.MapControl.PimpStyle(pointFeatureStyle, true);
            SharpMap.UI.Forms.MapControl.PimpStyle(errorPointFeatureStyle, false);
            newBreachLocationLayer.Style = pointFeatureStyle;
        }

        private void RemoveDrawingLayer()
        {
            newPointFeatureGeometry.Clear();
            newBreachLocationLayer = null;
        }
    }
}
