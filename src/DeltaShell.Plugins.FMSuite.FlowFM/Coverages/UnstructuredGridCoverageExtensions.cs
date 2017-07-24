using System;
﻿using System.Collections.Generic;
﻿using System.Linq;
﻿using DelftTools.Functions;
﻿using DelftTools.Functions.Generic;
﻿using DelftTools.Utils.Editing;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
﻿using NetTopologySuite.Extensions.Grids;
﻿using SharpMap.Api;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Coverages
{
    public static class UnstructuredGridCoverageExtensions
    {
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

        public static void LoadBathymetry(this UnstructuredGridVertexCoverage coverage, UnstructuredGrid grid, double noDataValue = -999.0)
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
            component.NoDataValue = noDataValue;
            if (count > 0)
            {
                FunctionHelper.SetValuesRaw(component, grid.Vertices.Select(v => v.Z));
            }
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
                var pointClouds = new Dictionary<IPointCloud, bool>();
                foreach (var component in coverage.Components.Cast<IVariable<double>>())
                {
                    double value;
                    if (SingleValue(component.Values.ToList(),out value))
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

                locationIndexVariable.Clear();
                if (count > 0)
                {
                    FunctionHelper.SetValuesRaw(locationIndexVariable, Enumerable.Range(0, count));
                }
                using (var api = new RemoteGeometryApi())
                {
                    var targetX = newLocations.Select(p => p.X).ToArray();
                    var targetY = newLocations.Select(p => p.Y).ToArray();
                    for (int i = 0; i < pointClouds.Count; ++i)
                    {
                        var keyValuePair = pointClouds.ElementAt(i);

                        var points = keyValuePair.Key;
                        var interpolating = keyValuePair.Value;
                        if (interpolating)
                        {
                            var sourceX = points.PointValues.Select(p => p.X).ToArray();
                            var sourceY = points.PointValues.Select(p => p.Y).ToArray();
                            var sourceZ = points.PointValues.Select(p => p.Value).ToArray();

                            var targetZ = api.Triangulate(sourceX, sourceY, sourceZ, targetX, targetY);

                            coverage.Components[i].Values.Clear();
                            FunctionHelper.SetValuesRaw<double>(coverage.Components[i], targetZ);
                        }
                        else
                        {
                            var value = points.PointValues[0].Value;
                            FunctionHelper.SetValuesRaw(coverage.Components[i], Enumerable.Repeat(value, count));
                        }
                    }
                }
            }
            coverage.Grid = grid;
            coverage.EndEdit();
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