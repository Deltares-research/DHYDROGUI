using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public static class WaterFlowFMModelCoordinateConversion
    {
        public static bool CanAssignCoordinateSystem(WaterFlowFMModel model, ICoordinateSystem coordinateSystem)
        {
            var gridEnvelope = model.GridExtent;
            if (!CoordinateSystemValidator.CanAssignCoordinateSystem(gridEnvelope, coordinateSystem))
            {
                return false;
            }
            var modelCoordinates = GetAllModelFeatures(model).Select(f => f.Geometry);
            return CoordinateSystemValidator.CanAssignCoordinateSystem(modelCoordinates, coordinateSystem);
        }

        private static bool CanConvertModel(WaterFlowFMModel model, ICoordinateTransformation transformation)
        {
            var gridEnvelope = model.GridExtent;
            if (!CoordinateSystemValidator.CanConvertByTransformation(gridEnvelope, transformation))
            {
                return false;
            }
            if (SpatialOperationPointClouds(model)
                    .Any(
                        pointCloud =>
                            !CoordinateSystemValidator.CanConvertByTransformation(pointCloud.GetExtents(),
                                transformation)))
            {
                return false;
            }
            return CoordinateSystemValidator.CanConvertByTransformation(
                GetAllModelFeatures(model).Select(f => f.Geometry), transformation);
        }

        private static IEnumerable<IFeatureProvider> SpatialOperationPointClouds(IModel model)
        {
            return model.DataItems.Select(di => di.ValueConverter)
                .OfType<SpatialOperationSetValueConverter>()
                .Select(vc => vc.SpatialOperationSet)
                .SelectMany(sos => sos.GetAllFeatureProviders().OfType<PointCloudFeatureProvider>())
                .ToList();
        }

        public static void ConvertModel(WaterFlowFMModel model, ICoordinateTransformation transformation)
        {
            if (!CanConvertModel(model, transformation))
                throw new CoordinateTransformException(model.Name, transformation.SourceCS, transformation.TargetCS);

            ConvertGrid(model.Grid, transformation);

            model.RefreshGridExtents();

            foreach (var feature in GetAllModelFeatures(model))
            {
                feature.Geometry = GeometryTransform.TransformGeometry(feature.Geometry,
                                                                       transformation.MathTransform);
            }

            foreach (var dataItem in model.DataItems.Where(di => di.ValueConverter is SpatialOperationSetValueConverter))
            {
                ((SpatialOperationSetValueConverter)dataItem.ValueConverter).Transform(transformation);
            }

            model.CoordinateSystem = (ICoordinateSystem) transformation.TargetCS;

            // we immediately rewrite the grid file
            if (!model.Grid.IsEmpty)
            {
                using (var ugridFile = new UGridFile(model.NetFilePath))
                {
                    ugridFile.RewriteGridCoordinates(model.Grid);
                }
            }

            model.InvalidateSnapping();
        }

        public static void ConvertGrid(UnstructuredGrid grid, ICoordinateTransformation transformation)
        {
            foreach (var vertex in grid.Vertices)
            {
                var newVertex = transformation.MathTransform.Transform(new[] {vertex.X, vertex.Y});
                vertex.X = newVertex[0];
                vertex.Y = newVertex[1];
            }

            foreach (var cell in grid.Cells)
            {
                var newCellCenter =
                    transformation.MathTransform.Transform(new[] {(double) cell.CenterX, (double) cell.CenterY});
                cell.CenterX = (float) newCellCenter[0];
                cell.CenterY = (float) newCellCenter[1];
            }
        }

        private static IEnumerable<IFeature> GetAllModelFeatures(WaterFlowFMModel model)
        {
            var area = model.Area;
            return area.ObservationCrossSections.OfType<IFeature>()
                .Concat(area.ObservationPoints)
                .Concat(area.DredgingLocations)
                .Concat(area.DumpingLocations)
                .Concat(area.LandBoundaries)
                .Concat(area.DryPoints)
                .Concat(area.DryAreas)
                .Concat(area.Pumps)
                .Concat(area.Weirs)
                .Concat(area.Gates)
                .Concat(area.ThinDams)
                .Concat(area.FixedWeirs)
                .Concat(area.Enclosures)
                .Concat(model.Boundaries)
                .Concat(model.Pipes)
                .Concat(model.BoundaryConditions1D)
                .Concat(model.LateralSourcesData);
        }
    }
}