using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using Deltares.Infrastructure.API.Guards;
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
            int cellOne = -1;
            int cellTwo = -1;
            var moreThanTwo = false;

            foreach (int fromCellIndex in vertexFromCellIndices)
            {
                foreach (int toCellIndex in vertexToCellIndices)
                {
                    if (fromCellIndex != toCellIndex)
                    {
                        continue;
                    }

                    if (cellOne == -1)
                    {
                        cellOne = fromCellIndex;
                        continue;
                    }

                    if (cellTwo == -1)
                    {
                        cellTwo = toCellIndex;
                        continue;
                    }

                    moreThanTwo = true;
                }
            }

            if (!moreThanTwo && cellOne != -1 && cellTwo != -1)
            {
                yield return new FlowLink(cellOne, cellTwo, gridEdge);
            }
        }
    }
}