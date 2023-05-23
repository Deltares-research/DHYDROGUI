using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.Api.ECModule;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Coverages
{
    public static class UnstructuredGridCoverageExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UnstructuredGridCoverageExtensions));

        public static IPointCloud ToPointCloud(this UnstructuredGridCoverage coverage, int componentIndex = 0,
            bool skipMissingValues = false)
        {
            var pointCloud = new PointCloud();

            if (coverage.IsTimeDependent)
                throw new NotSupportedException(Resources.UnstructuredGridCoverageExtensions_ToPointCloud_Converting_time_dependent_spatial_data_to_samples_is_not_supported);

            var component = coverage.Components[componentIndex] as IVariable<double>;
            if (component == null)
            {
                throw new NotSupportedException(
                    Resources.UnstructuredGridCoverageExtensions_ToPointCloud_Converting_a_non_double_valued_coverage_component_to_a_point_cloud_is_not_supported);
            }

            var coordinates = coverage.Coordinates.ToList();
            var values = component.Values;
            var noDataValue = (double) component.NoDataValue;

            if (coordinates.Count != values.Count)
                throw new InvalidOperationException(
                    Resources.UnstructuredGridCoverageExtensions_ToPointCloud_Spatial_data_is_not_consistent__number_of_coordinate_does_not_match_number_of_values);

            for (var i = 0; i < coordinates.Count; i++)
            {
                if (skipMissingValues && values[i] == noDataValue)
                {
                    continue;
                }
                var coord = coordinates[i];
                var point = new PointValue {X = coord.X, Y = coord.Y, Value = values[i]};
                pointCloud.PointValues.Add(point);
            }

            return pointCloud;
        }

        public static void LoadBathymetry(this UnstructuredGridVertexCoverage coverage, UnstructuredGrid grid, double? noDataValue)
        {
            coverage.BeginEdit(new DefaultEditAction("Starting import of bed levels"));
            var count = grid.Vertices.Count();
            var locationIndexVariable = coverage.Arguments.Last();
            locationIndexVariable.Values.Clear();
            if (count > 0)
            {
                FunctionHelper.SetValuesRaw(locationIndexVariable, Enumerable.Range(0, count));
            }
            var component = coverage.Components[0];
            component.Values.Clear();
            component.NoDataValue = noDataValue ?? -999.0d;
            if (count > 0)
            {
                FunctionHelper.SetValuesRaw(component, grid.Vertices.Select(v => v.Z));
            }
            if (!coverage.Grid.Equals(grid))
                coverage.Grid = grid;
            coverage.EndEdit();
        }
        
        public static void LoadGrid(this UnstructuredGridCoverage coverage, UnstructuredGrid grid, bool reInterpolate = false)
        {
            coverage.BeginEdit(new DefaultEditAction("Inserting new grid in coverage"));
            var newLocations = coverage.GetCoordinatesForGrid(grid).ToList();
            var count = newLocations.Count();
            var locationIndexVariable = coverage.Arguments.Last();

            if (!reInterpolate)
            {
                locationIndexVariable.Values.Clear();
                if (count > 0)
                {
                    FunctionHelper.SetValuesRaw(locationIndexVariable, Enumerable.Range(0, count));
                }
                foreach (var component in coverage.Components.OfType<IVariable<double>>())
                {
                    component.Values.Clear();
                    if (count > 0)
                    {
                        FunctionHelper.SetValuesRaw(component,
                            Enumerable.Repeat(component.NoDataValue, count).Cast<double>());
                    }
                }
            }
            else
            {
                var pointClouds = coverage.GetPointClouds();

                coverage.SetPointCloudsOnCoverageGrid(pointClouds, grid);
            }

            coverage.Grid = grid;
            coverage.EndEdit();
        }

        public static void SetPointCloudsOnCoverageGrid(this UnstructuredGridCoverage coverage, IDictionary<IPointCloud, bool> pointClouds, UnstructuredGrid grid = null)
        {
            grid = grid ?? coverage.Grid;

            var newLocations = coverage.GetCoordinatesForGrid(grid).ToList();
            var count = newLocations.Count();
            var locationIndexVariable = coverage.Arguments.Last();
            locationIndexVariable.ClearWithoutEventing();
            if (count > 0)
            {
                coverage.BeginEdit(new DefaultEditAction("setting location index variables on coverage"));
                FunctionHelper.SetValuesRaw(locationIndexVariable, Enumerable.Range(0, count));
                coverage.EndEdit();
            }

            for (int i = 0; i < pointClouds.Count; ++i)
            {
                var keyValuePair = pointClouds.ElementAt(i);

                var points = keyValuePair.Key;
                var interpolating = keyValuePair.Value;
                if (interpolating && points.PointValues.Count >0)
                {
                    using(var api = new RemoteECModuleApi())
                    {
                        using (var mesh = new DisposableMeshGeometry(grid))
                        {
                            var projectionType = grid.CoordinateSystem == null ||
                                                 !grid.CoordinateSystem.IsGeographic
                                ? ProjectionType.Cartesian
                                : ProjectionType.Spherical;
                            double[] targetZ;
                            try
                            {
                                targetZ = api.Triangulation(points.PointValues, mesh, coverage.GetLocationTypeForCoverage(), projectionType);


                                var unstructuredGridFlowLinkCoverage = coverage as UnstructuredGridFlowLinkCoverage;
                                if (unstructuredGridFlowLinkCoverage != null)
                                {
                                    targetZ = grid.FlowLinks.Count != 0
                                        ? grid.ReOrderResultsForFlowLinks(targetZ)
                                        : new double[0];
                                }
                                coverage.BeginEdit(new DefaultEditAction("Interpolating values on coverage"));
                                coverage.Components[i].ClearWithoutEventing();
                                FunctionHelper.SetValuesRaw<double>(coverage.Components[i], targetZ);
                                coverage.EndEdit();
                            }
                            catch
                            {
                                //gulp
                                //should never happen
                                Log.Fatal($"ECModuleApi could to triangulate the old point values to the new grid on coverage : {coverage.Name}" );
                            }
                        }

                    }
                }
                else
                {
                    double value;
                    if (points.PointValues.Count > 0)
                    {
                        value = points.PointValues[0].Value;
                    }
                    else
                    {
                        if (!double.TryParse(coverage.Components[i].NoDataValue.ToString(),out value))
                            value = -999.0d;
                    }
                    coverage.BeginEdit(new DefaultEditAction("Setting empty value on converage"));
                    coverage.Components[i].ClearWithoutEventing();
                    FunctionHelper.SetValuesRaw(coverage.Components[i], Enumerable.Repeat(value, count));
                    coverage.EndEdit();
                }
            }
        }

        public static Dictionary<IPointCloud, bool> GetPointClouds(this UnstructuredGridCoverage coverage)
        {
            var pointClouds = new Dictionary<IPointCloud, bool>();
            foreach (var component in coverage.Components.Cast<IVariable<double>>())
            {
                double value;
                if (SingleValue(component.Values.ToList(), out value))
                {
                    var pointCloud = new PointCloud();
                    pointCloud.PointValues.Add(new PointValue {X = 0, Y = 0, Value = value});
                    pointClouds.Add(pointCloud, false);
                }
                else
                {
                    var i = coverage.Components.IndexOf(component);
                    pointClouds.Add(coverage.ToPointCloud(i, true), true);
                }
            }
            return pointClouds;
        }

        private static bool SingleValue(IList<double> values, out double value)
        {
            if (values.Count == 0)
            {
                value = double.NaN;
                return false;
            }
            value = values[0];
            for (int i = 1; i < values.Count; ++i)
            {
                if (values[i] != value) return false;
            }
            return true;
        }
    }
}