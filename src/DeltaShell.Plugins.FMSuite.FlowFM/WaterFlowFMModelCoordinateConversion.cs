using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
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
            Envelope gridEnvelope = model.GridExtent;
            if (!CoordinateSystemValidator.CanAssignCoordinateSystem(gridEnvelope, coordinateSystem))
            {
                return false;
            }

            IEnumerable<IGeometry> modelCoordinates = GetAllModelFeatures(model).Select(f => f.Geometry);
            return CoordinateSystemValidator.CanAssignCoordinateSystem(modelCoordinates, coordinateSystem);
        }

        public static void ConvertModel(WaterFlowFMModel model, ICoordinateTransformation transformation)
        {
            if (!CanConvertModel(model, transformation))
            {
                throw new CoordinateTransformException(model.Name, transformation.SourceCS, transformation.TargetCS);
            }

            ConvertGrid(model.Grid, transformation);

            model.RefreshGridExtents();

            foreach (IFeature feature in GetAllModelFeatures(model))
            {
                feature.Geometry = GeometryTransform.TransformGeometry(feature.Geometry,
                                                                       transformation.MathTransform);
            }

            foreach (IDataItem dataItem in model.DataItems.Where(
                di => di.ValueConverter is SpatialOperationSetValueConverter))
            {
                ((SpatialOperationSetValueConverter) dataItem.ValueConverter).Transform(transformation);
            }

            model.CoordinateSystem = (ICoordinateSystem) transformation.TargetCS;

            // we immediately rewrite the grid file
            if (!model.Grid.IsEmpty)
            {
                UnstructuredGridFileHelper.RewriteGridCoordinates(model.NetFilePath, model.Grid);
            }

            model.InvalidateSnapping();
        }

        public static void ConvertGrid(UnstructuredGrid grid, ICoordinateTransformation transformation)
        {
            foreach (Coordinate vertex in grid.Vertices)
            {
                double[] newVertex = transformation.MathTransform.Transform(new[]
                {
                    vertex.X,
                    vertex.Y
                });
                vertex.X = newVertex[0];
                vertex.Y = newVertex[1];
            }

            foreach (Cell cell in grid.Cells)
            {
                double[] newCellCenter =
                    transformation.MathTransform.Transform(new[]
                    {
                        cell.CenterX,
                        cell.CenterY
                    });
                cell.CenterX = (float) newCellCenter[0];
                cell.CenterY = (float) newCellCenter[1];
            }
        }

        private static bool CanConvertModel(WaterFlowFMModel model, ICoordinateTransformation transformation)
        {
            Envelope gridEnvelope = model.GridExtent;
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

        private static IEnumerable<IFeature> GetAllModelFeatures(WaterFlowFMModel model)
        {
            HydroArea area = model.Area;
            return area.ObservationCrossSections.OfType<IFeature>()
                       .Concat(area.ObservationPoints)
                       .Concat(area.DredgingLocations)
                       .Concat(area.DumpingLocations)
                       .Concat(area.LandBoundaries)
                       .Concat(area.DryPoints)
                       .Concat(area.DryAreas)
                       .Concat(area.Pumps)
                       .Concat(area.Structures)
                       .Concat(area.ThinDams)
                       .Concat(area.FixedWeirs)
                       .Concat(area.Enclosures)
                       .Concat(model.Boundaries)
                       .Concat(model.Pipes)
                       .Concat(model.LateralFeatures);
        }
    }
}