using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Coverages
{
    /// <summary>
    /// Factory class to creating various types of an <see cref="UnstructuredGridCoverage"/>.
    /// </summary>
    public static class UnstructuredGridCoverageFactory
    {
        private const double noDataValue = -999d;

        /// <summary>
        /// Creates an <see cref="UnstructuredGridVertexCoverage"/>.
        /// </summary>
        /// <param name="name"> The name of the coverage. </param>
        /// <param name="grid"> The unstructured grid. </param>
        /// <param name="componentValues"> Optional parameter; the component values to set the coverage with. </param>
        /// <returns> The created <see cref="UnstructuredGridVertexCoverage"/>. </returns>
        public static UnstructuredGridVertexCoverage CreateVertexCoverage(
            string name, UnstructuredGrid grid, IEnumerable<double> componentValues)
        {
            var coverage = new UnstructuredGridVertexCoverage(new UnstructuredGrid(), false);
            ConfigureUnstructuredGridCoverage(coverage, grid, name, GetArgumentValues(grid.Vertices.Count), componentValues, noDataValue);
            return coverage;
        }

        /// <summary>
        /// Creates an <see cref="UnstructuredGridCellCoverage"/>.
        /// </summary>
        /// <param name="name"> The name of the coverage. </param>
        /// <param name="grid"> The unstructured grid. </param>
        /// <param name="componentValues"> Optional parameter; the component values to set the coverage with. </param>
        /// <param name="defaultValue"> Optional parameter; the default component value to set the coverage with. </param>
        /// <returns> The created <see cref="UnstructuredGridCellCoverage"/>. </returns>
        public static UnstructuredGridCellCoverage CreateCellCoverage(
            string name, UnstructuredGrid grid, IEnumerable<double> componentValues = null, double defaultValue = noDataValue)
        {
            var coverage = new UnstructuredGridCellCoverage(new UnstructuredGrid(), false);
            ConfigureUnstructuredGridCoverage(coverage, grid, name, GetArgumentValues(grid.Cells.Count), componentValues, defaultValue);
            return coverage;
        }

        /// <summary>
        /// Creates an <see cref="UnstructuredGridFlowLinkCoverage"/>.
        /// </summary>
        /// <param name="name"> The name of the coverage. </param>
        /// <param name="grid"> The unstructured grid. </param>
        /// <returns> The created <see cref="UnstructuredGridFlowLinkCoverage"/>. </returns>
        public static UnstructuredGridFlowLinkCoverage CreateFlowLinkCoverage(
            string name, UnstructuredGrid grid)
        {
            var coverage = new UnstructuredGridFlowLinkCoverage(new UnstructuredGrid(), false);
            ConfigureUnstructuredGridCoverage(coverage, grid, name, GetArgumentValues(grid.FlowLinks.Count), null, noDataValue);
            return coverage;
        }

        private static void ConfigureUnstructuredGridCoverage(UnstructuredGridCoverage coverage,
                                                              UnstructuredGrid grid, string name,
                                                              IEnumerable<int> argumentValues,
                                                              IEnumerable<double> componentValues,
                                                              double defaultValue)
        {
            coverage.Name = name;
            coverage.Grid = grid;

            coverage.Components[0].NoDataValue = noDataValue;
            coverage.Components[0].DefaultValue = defaultValue;

            FunctionHelper.SetValuesRaw(coverage.Arguments[0], argumentValues);

            IEnumerable<double> values = componentValues ?? Enumerable.Repeat(defaultValue, coverage.Arguments[0].Values.Count);
            FunctionHelper.SetValuesRaw(coverage.Components[0], values);
        }

        private static IEnumerable<int> GetArgumentValues(int count) => Enumerable.Range(0, count);
    }
}