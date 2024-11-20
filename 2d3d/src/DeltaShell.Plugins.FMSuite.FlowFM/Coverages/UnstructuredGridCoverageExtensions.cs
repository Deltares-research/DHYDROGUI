using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api;
using SharpMap.Api.ECModule;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Coverages
{
    public static class UnstructuredGridCoverageExtensions
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UnstructuredGridCoverageExtensions));

        public static IPointCloud ToPointCloud(this UnstructuredGridCoverage coverage, int componentIndex = 0,
                                               bool skipMissingValues = false)
        {
            var pointCloud = new PointCloud();

            if (coverage.IsTimeDependent)
            {
                throw new NotSupportedException(
                    Resources
                        .UnstructuredGridCoverageExtensions_ToPointCloud_Converting_time_dependent_spatial_data_to_samples_is_not_supported);
            }

            var component = coverage.Components[componentIndex] as IVariable<double>;
            if (component == null)
            {
                throw new NotSupportedException(
                    Resources
                        .UnstructuredGridCoverageExtensions_ToPointCloud_Converting_a_non_double_valued_coverage_component_to_a_point_cloud_is_not_supported);
            }

            Coordinate[] coordinates = coverage.Coordinates.ToArray();
            IMultiDimensionalArray<double> values = component.Values;
            var noDataValue = (double)component.NoDataValue;

            if (coordinates.Length != values.Count)
            {
                throw new InvalidOperationException(
                    Resources
                        .UnstructuredGridCoverageExtensions_ToPointCloud_Spatial_data_is_not_consistent__number_of_coordinate_does_not_match_number_of_values);
            }

            for (var i = 0; i < coordinates.Length; i++)
            {
                if (skipMissingValues && values[i] == noDataValue)
                {
                    continue;
                }

                Coordinate coord = coordinates[i];
                var point = new PointValue
                {
                    X = coord.X,
                    Y = coord.Y,
                    Value = values[i]
                };
                pointCloud.PointValues.Add(point);
            }

            return pointCloud;
        }

        /// <summary>
        /// Load the bathymetry on this <see cref="UnstructuredGridVertexCoverage"/>
        /// from the specified <paramref name="grid"/>.
        /// </summary>
        /// <param name="coverage">The coverage to load the bathymetry data on.</param>
        /// <param name="grid">The grid from which to obtain the data and set as the grid of the coverage.</param>
        /// <param name="noDataValue">The no data value used to indicate no data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="coverage"/> or <paramref name="grid"/> is <c>null</c>.
        /// </exception>
        public static void LoadBathymetry(this UnstructuredGridVertexCoverage coverage,
                                          UnstructuredGrid grid,
                                          double noDataValue = -999.0)
        {
            Ensure.NotNull(coverage, nameof(coverage));
            Ensure.NotNull(grid, nameof(grid));

            IEnumerable<double> GetZValues() => grid.Vertices.Select(v => v.Z);

            LoadBathymetry(coverage, grid, noDataValue, GetZValues);
        }

        /// <summary>
        /// Load the bathymetry on this <see cref="UnstructuredGridCellCoverage"/>
        /// from the specified grid and nc file at <paramref name="netFilePath"/>.
        /// </summary>
        /// <param name="coverage">The coverage to load the bathymetry data on.</param>
        /// <param name="grid">The grid from which to obtain the data and set as the grid of the coverage.</param>
        /// <param name="netFilePath">The nc file from which to obtain the z-values</param>
        /// <param name="noDataValue">The no data value used to indicate no data.</param>
        /// <remarks>
        /// For some reason when we construct our <see cref="UnstructuredGridCellCoverage"/>
        /// when we have our bathymetry data on the faces, these values are not properly read.
        /// As such we need to retrieve them from the net file itself.
        ///
        /// We assume that the <paramref name="netFilePath"/> exists and corresponds with
        /// the provided <paramref name="grid"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="coverage"/> or <paramref name="grid"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="netFilePath"/> is <c>null</c> or empty.
        /// </exception>
        public static void LoadBathymetry(this UnstructuredGridCellCoverage coverage,
                                          UnstructuredGrid grid,
                                          string netFilePath,
                                          double noDataValue = -999.0)
        {
            Ensure.NotNull(coverage, nameof(coverage));
            Ensure.NotNull(grid, nameof(grid));
            Ensure.NotNullOrEmpty(netFilePath, nameof(netFilePath));

            // Note that at the time of writing, there is no distinction between reading z-values
            // with Faces or FacesMeanLevFromNodes, so we default to Faces.
            IEnumerable<double> GetZValues() =>
                UnstructuredGridFileHelper.ReadZValues(netFilePath, UnstructuredGridFileHelper.BedLevelLocation.Faces);

            LoadBathymetry(coverage, grid, noDataValue, GetZValues);
        }

        private static void LoadBathymetry(UnstructuredGridCoverage coverage,
                                           UnstructuredGrid grid,
                                           double noDataValue,
                                           Func<IEnumerable<double>> getZValues)
        {
            coverage.BeginEdit("Starting import of bed levels");

            int count = coverage.GetCoordinatesForGrid(grid).Count();

            IVariable locationIndexVariable = coverage.Arguments.Last();
            locationIndexVariable.Values.Clear();

            IVariable component = coverage.Components[0];
            component.Values.Clear();
            component.NoDataValue = noDataValue;

            if (count > 0)
            {
                FunctionHelper.SetValuesRaw(locationIndexVariable, Enumerable.Range(0, count));
                IEnumerable<double> zValues = getZValues().ToArray();

                if (zValues.Count() != count)
                {
                    log.WarnFormat(Resources.UnstructuredGridCoverageExtensions_LoadBathymetry_No_bathymetry_data_was_found__the_default_D_FlowFM___0___will_be_used_instead_, noDataValue);
                    zValues = Enumerable.Repeat(noDataValue, count);
                }

                FunctionHelper.SetValuesRaw(component, zValues);
            }

            coverage.Grid = grid;
            coverage.EndEdit();
        }

        /// <summary>
        /// Loads the specified <paramref name="grid"/> onto this <see cref="UnstructuredGridCoverage"/>.
        /// </summary>
        /// <param name="coverage">The coverage.</param>
        /// <param name="grid">The grid.</param>
        /// <param name="reInterpolate">if set to <c>true</c> [re interpolate].</param>
        public static void LoadGrid(this UnstructuredGridCoverage coverage,
                                    UnstructuredGrid grid,
                                    bool reInterpolate = false)
        {
            if (coverage.Grid == grid)
            {
                return;
            }

            coverage.BeginEdit("Inserting new grid in coverage");
            List<Coordinate> newLocations = coverage.GetCoordinatesForGrid(grid).ToList();

            int count = newLocations.Count;

            IVariable locationIndexVariable = coverage.Arguments.Last();

            if (!reInterpolate)
            {
                locationIndexVariable.Values.Clear();
                if (count > 0)
                {
                    FunctionHelper.SetValuesRaw(locationIndexVariable, Enumerable.Range(0, count));
                }

                foreach (IVariable<double> component in coverage.Components.OfType<IVariable<double>>())
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
                var pointClouds = new Dictionary<IPointCloud, bool>();
                foreach (IVariable<double> component in coverage.Components.Cast<IVariable<double>>())
                {
                    double value;
                    if (SingleValue(component.Values.ToList(), out value))
                    {
                        var pointCloud = new PointCloud();
                        pointCloud.PointValues.Add(new PointValue
                        {
                            X = 0,
                            Y = 0,
                            Value = value
                        });
                        pointClouds.Add(pointCloud, false);
                    }
                    else
                    {
                        int i = coverage.Components.IndexOf(component);
                        pointClouds.Add(coverage.ToPointCloud(i, true), true);
                    }
                }

                locationIndexVariable.Clear();
                if (count > 0)
                {
                    FunctionHelper.SetValuesRaw(locationIndexVariable, Enumerable.Range(0, count));
                }

                for (var i = 0; i < pointClouds.Count; ++i)
                {
                    KeyValuePair<IPointCloud, bool> keyValuePair = pointClouds.ElementAt(i);

                    IPointCloud points = keyValuePair.Key;
                    bool interpolating = keyValuePair.Value;
                    if (interpolating)
                    {
                        using (var api = new RemoteECModuleApi())
                        using (var mesh = new DisposableMeshGeometry(grid))
                        {
                            ProjectionType projectionType = grid.CoordinateSystem == null ||
                                                            !grid.CoordinateSystem.IsGeographic
                                                                ? ProjectionType.Cartesian
                                                                : ProjectionType.Spherical;

                            double[] targetZ = api.Triangulation(points.PointValues, mesh,
                                                                 coverage.GetLocationTypeForCoverage(), projectionType);

                            var unstructuredGridFlowLinkCoverage = coverage as UnstructuredGridFlowLinkCoverage;
                            if (unstructuredGridFlowLinkCoverage != null)
                            {
                                targetZ = grid.FlowLinks.Count != 0
                                              ? grid.ReOrderResultsForFlowLinks(targetZ)
                                              : new double[0];
                            }

                            coverage.Components[i].Values.Clear();
                            FunctionHelper.SetValuesRaw<double>(coverage.Components[i], targetZ);
                        }
                    }
                    else
                    {
                        double value = points.PointValues[0].Value;

                        coverage.Components[i].Values.Clear();
                        FunctionHelper.SetValuesRaw(coverage.Components[i], Enumerable.Repeat(value, count));
                    }
                }
            }

            coverage.Grid = grid;
            coverage.EndEdit();
        }

        /// <summary>
        /// Replaces the no data values for this <see cref="UnstructuredGridCoverage"/> with default values.
        /// </summary>
        /// <param name="coverage"> The coverage for which to replace the values for. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="coverage"/> is <c>null</c>.
        /// </exception>
        public static void ReplaceMissingValuesWithDefaultValues(this UnstructuredGridCoverage coverage)
        {
            Ensure.NotNull(coverage, nameof(coverage));

            var variable = (IVariable<double>)coverage.Components[0];
            if (Equals(variable.NoDataValue, variable.DefaultValue))
            {
                return;
            }

            coverage.BeginEdit($"Replacing missing values for coverage {coverage.Name}");
            List<double> values = GenerateCollectionWithReplacedValues(variable).ToList();
            variable.Values.Clear();

            FunctionHelper.SetValuesRaw(variable, values);

            coverage.EndEdit();
        }

        private static IEnumerable<T> GenerateCollectionWithReplacedValues<T>(IVariable<T> variable)
        {
            foreach (T value in variable.Components[0].Values)
            {
                yield return value.Equals(variable.NoDataValue)
                                 ? variable.DefaultValue
                                 : value;
            }
        }

        private static bool SingleValue(IList<double> values, out double value)
        {
            if (values.Count == 0)
            {
                value = double.NaN;
                return false;
            }

            value = values[0];
            for (var i = 1; i < values.Count; ++i)
            {
                if (values[i] != value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}