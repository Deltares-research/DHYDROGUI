using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using SharpMap.Api.SpatialOperations;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions
{
    public static class WaterQualityFunctionExtensions
    {
        /// <summary>
        /// Checks whether or not <paramref name="function"/> is a constant function (in the context of WaterQualityModel)
        /// </summary>
        /// <param name="function"> The function to check </param>
        public static bool IsConst(this IFunction function)
        {
            return function.Arguments.Any(v => v is Variable<double>);
        }

        /// <summary>
        /// Checks whether or not <paramref name="function"/> is a time series (in the context of WaterQualityModel)
        /// </summary>
        /// <param name="function"> The function to check </param>
        public static bool IsTimeSeries(this IFunction function)
        {
            return function.Arguments.Any(v => v is Variable<DateTime>);
        }

        /// <summary>
        /// Checks whether or not <paramref name="function"/> is a segment files (in the context of WaterQualityModel)
        /// </summary>
        /// <param name="function"> The function to check </param>
        public static bool IsSegmentFile(this IFunction function)
        {
            bool f = function is SegmentFileFunction;
            return f;
        }

        /// <summary>
        /// Checks whether or not <paramref name="function"/> is a network coverage
        /// </summary>
        /// <param name="function"> The function to check </param>
        public static bool IsNetworkCoverage(this IFunction function)
        {
            return function is NetworkCoverage;
        }

        /// <summary>
        /// Checks whether or not <paramref name="function"/> is a <see cref="UnstructuredGridCellCoverage"/>.
        /// </summary>
        /// <param name="function"> The function to check </param>
        public static bool IsUnstructuredGridCellCoverage(this IFunction function)
        {
            return function is UnstructuredGridCellCoverage;
        }

        /// <summary>
        /// Determines whether or not <paramref name="function"/> is a <see cref="FunctionFromHydroDynamics"/>.
        /// </summary>
        /// <param name="function"> The function to check. </param>
        public static bool IsFromHydroDynamics(this IFunction function)
        {
            return function is FunctionFromHydroDynamics;
        }

        /// <summary>
        /// Assigns a new Grid to a grid cell coverage.
        /// </summary>
        /// <param name="unstructuredGridCoverage"> The coverage to be updated. </param>
        /// <param name="grid"> The new grid. </param>
        /// <param name="clearCoverage">
        /// Optional: indicate if coverage should be cleared.
        /// Only set to false when the grid or cell indices haven't changed.
        /// Default value: true.
        /// </param>
        /// <remarks>
        ///     <para> Setting the new grid causes the coverage to be cleared. </para>
        ///     <para>
        ///     This method does not update <see cref="ISpatialOperation"/> instances
        ///     that might be applied to this particular coverage.
        ///     </para>
        /// </remarks>
        public static void AssignNewGridToCoverage(this UnstructuredGridCellCoverage unstructuredGridCoverage,
                                                   UnstructuredGrid grid, bool clearCoverage = true)
        {
            unstructuredGridCoverage.Grid = grid;
            if (clearCoverage)
            {
                unstructuredGridCoverage.ClearCoverage();
            }
        }

        /// <summary>
        /// Performs a clear on a grid cell coverage. If the coverage is not a time-dependent
        /// coverage, then the first component is set to <see cref="IVariable.NoDataValue"/>
        /// assigned to that component.
        /// </summary>
        /// <remarks>
        /// Events are disabled during the clear as performance optimization for
        /// <see cref="MemoryFunctionStore"/> based coverages.
        /// </remarks>
        public static void ClearCoverage(this UnstructuredGridCellCoverage coverage)
        {
            var functionStore = coverage.Store as LazyMapFileFunctionStore;
            if (functionStore != null)
            {
                functionStore.Path = null;
                return;
            }

            List<IVariable> variables = coverage.Arguments.Concat(coverage.Components).ToList();
            variables.ForEach(v => v.Values.FireEvents = false);
            variables.ForEach(v => v.Values.Clear());
            variables.ForEach(v => v.Values.FireEvents = true);

            int cellCount = coverage.Grid.Cells.Count;
            coverage.Arguments[coverage.IsTimeDependent ? 1 : 0].SetValues(Enumerable.Range(0, cellCount));

            if (!coverage.IsTimeDependent && cellCount > 0)
            {
                coverage.SetValues(Enumerable.Repeat(coverage.Components[0].NoDataValue, cellCount));
            }
        }
    }
}