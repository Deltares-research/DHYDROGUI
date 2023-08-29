using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Guards;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Grid
{
    /// <summary>
    /// Provides extension methods for an <see cref="UnstructuredGrid"/>.
    /// </summary>
    public static class UnstructuredGridExtensions
    {
        /// <summary>
        /// Generate flow links to the grid based on the edges.
        /// </summary>
        /// <param name="grid"> The unstructured grid. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="grid"/> is <c>null</c>.
        /// </exception>
        public static void GenerateFlowLinks(this UnstructuredGrid grid)
        {
            Ensure.NotNull(grid, nameof(grid));

            grid.FlowLinks.AddRange(GetFlowLinks(grid));
        }

        private static IEnumerable<FlowLink> GetFlowLinks(UnstructuredGrid grid)
        {
            return grid.Edges.SelectMany(gridEdge => GetFlowLinksForEdge(grid, gridEdge));
        }

        private static IEnumerable<FlowLink> GetFlowLinksForEdge(UnstructuredGrid grid, Edge gridEdge)
        {
            if (!grid.VertexToCellIndices.TryGetValue(gridEdge.VertexFromIndex, out IList<int> vertexFromCellIndices))
            {
                yield break;
            }

            if (!grid.VertexToCellIndices.TryGetValue(gridEdge.VertexToIndex, out IList<int> vertexToCellIndices))
            {
                yield break;
            }

            foreach (FlowLink flowLink in GetFlowLinksForEdge(gridEdge, vertexFromCellIndices, vertexToCellIndices))
            {
                yield return flowLink;
            }
        }

        private static IEnumerable<FlowLink> GetFlowLinksForEdge(Edge gridEdge, IList<int> vertexFromCellIndices, IList<int> vertexToCellIndices)
        {
            List<int> cells = vertexFromCellIndices.Intersect(vertexToCellIndices).ToList();

            if (cells.Count == 2)
            {
                yield return new FlowLink(cells[0], cells[1], gridEdge);
            }
        }
    }
}