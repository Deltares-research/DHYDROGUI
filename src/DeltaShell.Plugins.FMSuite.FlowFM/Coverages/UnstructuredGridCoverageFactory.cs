using System;
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
        /// <summary>
        /// Creates an <see cref="UnstructuredGridVertexCoverage"/>.
        /// </summary>
        /// <param name="name"> The name of the coverage. </param>
        /// <param name="grid"> The unstructured grid. </param>
        /// <param name="componentValues"> Optional parameter; the component values to set the coverage with. </param>
        /// <returns> The created <see cref="UnstructuredGridVertexCoverage"/>. </returns>
        public static UnstructuredGridVertexCoverage CreateVertexCoverage(
            string name, UnstructuredGrid grid, IEnumerable<double> componentValues = null)
        {
            return CreateUnstructuredGridCoverage(name, grid,
                                                  () => new UnstructuredGridVertexCoverage(new UnstructuredGrid(), false),
                                                  () => Enumerable.Range(0, grid.Vertices.Count),
                                                  componentValues);
        }

        /// <summary>
        /// Creates an <see cref="UnstructuredGridCellCoverage"/>.
        /// </summary>
        /// <param name="name"> The name of the coverage. </param>
        /// <param name="grid"> The unstructured grid. </param>
        /// <param name="componentValues"> Optional parameter; the component values to set the coverage with. </param>
        /// <returns> The created <see cref="UnstructuredGridCellCoverage"/>. </returns>
        public static UnstructuredGridCellCoverage CreateCellCoverage(
            string name, UnstructuredGrid grid, IEnumerable<double> componentValues = null)
        {
            return CreateUnstructuredGridCoverage(name, grid,
                                                  () => new UnstructuredGridCellCoverage(new UnstructuredGrid(), false),
                                                  () => Enumerable.Range(0, grid.Cells.Count),
                                                  componentValues);
        }

        /// <summary>
        /// Creates an <see cref="UnstructuredGridFlowLinkCoverage"/>.
        /// </summary>
        /// <param name="name"> The name of the coverage. </param>
        /// <param name="grid"> The unstructured grid. </param>
        /// <param name="componentValues"> Optional parameter; the component values to set the coverage with. </param>
        /// <returns> The created <see cref="UnstructuredGridFlowLinkCoverage"/>. </returns>
        public static UnstructuredGridFlowLinkCoverage CreateFlowLinkCoverage(
            string name, UnstructuredGrid grid, IEnumerable<double> componentValues = null)
        {
            return CreateUnstructuredGridCoverage(name, grid,
                                                  () => new UnstructuredGridFlowLinkCoverage(new UnstructuredGrid(), false),
                                                  () => Enumerable.Range(0, grid.FlowLinks.Count),
                                                  componentValues);
        }

        public static T CreateUnstructuredGridCoverage<T>(string name, UnstructuredGrid grid, Func<T> createCoverage,
                                                          Func<IEnumerable<int>> argumentValues,
                                                          IEnumerable<double> componentValues = null)
            where T : UnstructuredGridCoverage
        {
            T result = createCoverage();

            result.Name = name;
            result.Grid = grid;

            result.Components[0].NoDataValue = -999d;
            result.Components[0].DefaultValue = -999d;

            FunctionHelper.SetValuesRaw(result.Arguments[0], argumentValues());

            IEnumerable<double> values = componentValues ?? Enumerable.Repeat(-999d, result.Arguments[0].Values.Count);
            FunctionHelper.SetValuesRaw(result.Components[0], values);

            return result;
        }
    }
}