using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;

namespace DeltaShell.Plugins.FMSuite.Wave
{
    public static class WaveModelCoordinateConversion
    {
        public static bool IsSaneCoordinateSystemForModel(WaveModel model, ICoordinateSystem potentialCoordinateSystem)
        {
            if (model.OuterDomain != null && model.OuterDomain.Grid != null &&
                model.OuterDomain.Grid.Size1 > 0 && model.OuterDomain.Grid.Size2 > 0)
            {
                var coordinates = new List<Coordinate>();
                var minX = model.OuterDomain.Grid.X.Values.Where(v => !double.IsNaN(v)).DefaultIfEmpty().Min();
                var maxX = model.OuterDomain.Grid.X.Values.Where(v => !double.IsNaN(v)).DefaultIfEmpty().Max();
                var minY = model.OuterDomain.Grid.Y.Values.Where(v => !double.IsNaN(v)).DefaultIfEmpty().Min();
                var maxY = model.OuterDomain.Grid.Y.Values.Where(v => !double.IsNaN(v)).DefaultIfEmpty().Max();
                coordinates.Add(new Coordinate(minX, minY));
                coordinates.Add(new Coordinate(minX, maxY));
                coordinates.Add(new Coordinate(maxX, minY));
                coordinates.Add(new Coordinate(maxX, maxY));
                return CoordinateSystemValidator.CanAssignCoordinateSystem(coordinates, potentialCoordinateSystem);
            }
            return true;
        }

        public static void Transform(WaveModel model, ICoordinateTransformation transformation)
        {
            if (!CanConvertModel(model, transformation))
                throw new CoordinateTransformException(model.Name, transformation.SourceCS, transformation.TargetCS);
            
            foreach (var dataItem in model.DataItems.Where(di => di.ValueConverter is SpatialOperationSetValueConverter))
            {
                ((SpatialOperationSetValueConverter)dataItem.ValueConverter).Transform(transformation);
            }

            // convert grid(s)
            foreach (var domain in WaveDomainHelper.GetAllDomains(model.OuterDomain))
            {
                var grid = domain.Grid;
                var xCoordinates = new List<double>(grid.X.Values.Count);
                var yCoordinates = new List<double>(grid.Y.Values.Count);

                for (int i = 0; i < grid.X.Values.Count; ++i)
                {
                    var x = grid.X.Values[i];
                    var y = grid.Y.Values[i];

                    // dry points will be dry points..
                    if (!WaveDomainHelper.IsDryPoint(x, y))
                    {
                        var transformedCoordinate = transformation.MathTransform.Transform(new[] {x, y});
                        x = transformedCoordinate[0];
                        y = transformedCoordinate[1];
                    }

                    xCoordinates.Add(x);
                    yCoordinates.Add(y);
                }

                domain.Grid.BeginEdit(new DefaultEditAction(string.Format("Transform grid {0}...", domain.Grid.Name)));
                domain.Grid.Resize(grid.Size1, grid.Size2, xCoordinates, yCoordinates);
                domain.Grid.EndEdit();

                domain.Bathymetry.BeginEdit(new DefaultEditAction(string.Format("Transform bathymetry {0}...", domain.Bathymetry.Name)));
                domain.Bathymetry.Resize(grid.Size1, grid.Size2, grid.X.Values, grid.Y.Values);
                domain.Bathymetry.EndEdit();
            }

            // convert features
            foreach (var feature in GetAllModelFeatures(model))
            {
                feature.Geometry = GeometryTransform.TransformGeometry(feature.Geometry,
                                                                       transformation.MathTransform);
            }
        }

        private static bool CanConvertModel(WaveModel model, ICoordinateTransformation transformation)
        {
            var features = GetAllModelFeatures(model).ToList();
            return CoordinateSystemValidator.CanConvertByTransformation(
                features.Select(f => f.Geometry), transformation);
        }

        public static IEnumerable<IFeature> GetAllModelFeatures(WaveModel model)
        {
            return model.ObservationPoints.OfType<IFeature>()
                .Concat(model.ObservationCrossSections)
                .Concat(model.Obstacles)
                .Concat(model.Boundaries)
                .Concat(model.Sp2Boundaries);
        }
    }
}