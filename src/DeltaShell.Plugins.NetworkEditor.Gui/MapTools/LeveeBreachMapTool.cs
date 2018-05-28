using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Tools;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.Gui.MapTools
{
    public class LeveeBreachMapTool: Feature2DLineTool
    {
        NewPointFeatureTool breachLocationTool;
        private bool movingBreachLocation = false;
        public LeveeBreachMapTool(string targetLayerName, string name, Bitmap icon) : base(targetLayerName, name, icon)
        {
            
            breachLocationTool = new NewPointFeatureTool(GetLeveesLayer(),"BreachLocation");
        }

        public override void Execute()
        {
            base.Execute();

            var featureCount = VectorLayer.DataSource.GetFeatureCount();

            var feature = featureCount > 0
                ? VectorLayer.DataSource.GetFeature(featureCount - 1)
                : null;
        }

        public override void OnMouseDown(Coordinate worldPosition, MouseEventArgs e)
        {
            if(IsOnBreachLocation(worldPosition))
            {
                movingBreachLocation = true;
            }
            else
            {
                base.OnMouseDown(worldPosition, e);
            }

        }

        public override void OnMouseMove(Coordinate worldPosition, MouseEventArgs e)
        {
            if (!movingBreachLocation)
            {
                base.OnMouseMove(worldPosition, e);
            }
        }

        public override void OnMouseUp(Coordinate worldPosition, MouseEventArgs e)
        {
            if (movingBreachLocation)
            {
                StopDrawing();
                MapControl.Refresh();
//                if (breachLocationTool.SnapResult == null)
//                {
//                    MapControl.SelectTool.Clear();
//                }
//                var point = GetNewFeatureGeometry(layer);
                movingBreachLocation = false;
            }
            else
            {
                base.OnMouseUp(worldPosition, e);
            }
        }

        public override void Render(Graphics graphics, Map mapBox)
        {
            if (movingBreachLocation)
            {
                breachLocationTool.Render(graphics, mapBox);
            }
            else
            {
                base.Render(graphics, mapBox);
            }
        }

        private IPoint GetNewFeatureGeometry(ILayer layer)
        {
            //            var nearestTargetLineString = SnapResult.NearestTarget as ILineString;//check if the snap result is on a linesegment
            //            if (layer.CoordinateTransformation == null || nearestTargetLineString == null)
            //            {
            //                var point = (IPoint)GeometryHelper.SetCoordinate(newPointFeature, 0, SnapResult.Location);
            //                return layer.CoordinateTransformation != null
            //                    ? GeometryTransform.TransformPoint(point, layer.CoordinateTransformation.MathTransform.Inverse())
            //                    : point;
            //            }
            //
            //            var previousCoordinate = nearestTargetLineString.Coordinates[SnapResult.SnapIndexPrevious];
            //            var nextCoorinate = nearestTargetLineString.Coordinates[SnapResult.SnapIndexNext];
            //
            //            var distanceToPrevious = previousCoordinate.Distance(SnapResult.Location);
            //            var percentageFromPrevious = distanceToPrevious / previousCoordinate.Distance(nextCoorinate);
            //
            //            var mathTransform = layer.CoordinateTransformation.MathTransform.Inverse();
            //            var c1 = TransformCoordinate(previousCoordinate, mathTransform);
            //            var c2 = TransformCoordinate(nextCoorinate, mathTransform);
            //
            //            var targetLineString = new LineString(new[] { c1, c2 });
            //
            //            var coordinate = GeometryHelper.LineStringCoordinate(targetLineString,
            //                targetLineString.Length * percentageFromPrevious);
            //            return (IPoint)GeometryHelper.SetCoordinate(newPointFeature, 0, coordinate);
            return (IPoint)GeometryHelper.SetCoordinate(new Point(0.0,0.0), 0, new Coordinate());
        }


        private Func<ILayer, bool> GetLeveesLayer()
        {
            throw new NotImplementedException();
        }

        private bool IsOnBreachLocation(Coordinate worldPosition)
        {
            throw new NotImplementedException();
        }
    }
}
