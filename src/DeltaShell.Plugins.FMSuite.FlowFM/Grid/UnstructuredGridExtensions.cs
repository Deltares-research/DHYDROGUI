using System.Collections.Generic;
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

            // optimized for performance
            var flowLinks = new List<FlowLink>();

            foreach (Edge gridEdge in grid.Edges)
            {
                grid.VertexToCellIndices.TryGetValue(gridEdge.VertexFromIndex, out IList<int> gridVertexToCellIndex);
                if (gridVertexToCellIndex == null)
                {
                    continue;
                }

                grid.VertexToCellIndices.TryGetValue(gridEdge.VertexToIndex, out IList<int> vertexToCellIndex);
                if (vertexToCellIndex == null)
                {
                    continue;
                }

                int cellOne = -1;
                int cellTwo = -1;
                var moreThanTwo = false;
                foreach (int value1 in gridVertexToCellIndex)
                {
                    foreach (int value2 in vertexToCellIndex)
                    {
                        if (value1 != value2)
                        {
                            continue;
                        }

                        if (cellOne == -1)
                        {
                            cellOne = value1;
                            continue;
                        }

                        if (cellTwo == -1)
                        {
                            cellTwo = value2;
                            continue;
                        }

                        moreThanTwo = true;
                    }
                }

                if (!moreThanTwo && cellOne != -1 && cellTwo != -1)
                {
                    flowLinks.Add(new FlowLink(cellOne, cellTwo, gridEdge));
                }
            }

            grid.FlowLinks.AddRange(flowLinks);
        }
    }
}