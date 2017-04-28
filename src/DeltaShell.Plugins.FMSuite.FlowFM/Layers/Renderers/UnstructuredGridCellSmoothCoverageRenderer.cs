using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.Layers.Renderers;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid.ModelGrid;
using SharpMap.Api;
using SharpMap.Rendering.Thematics;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Layers.Renderers
{
    public class UnstructuredGridCellSmoothCoverageRenderer : IUnstructuredGridCoverageRenderer
    {
        public void Render(IPrimitivesRenderer renderer, UnstructuredBaseLayer layer, UnstructuredModelGrid grid, ITheme theme, double[] values)
        {
            var vertexValues = new double[grid.Vertices.Count];

            var vertexToCellMapping = grid.VertexToCellIndices;

            for (var vi = 0; vi < grid.Vertices.Count; vi++)
            {
                IList<int> neighbouringCells;
                if (vertexToCellMapping.TryGetValue(vi, out neighbouringCells))
                {
                    // do in-place simplified interpolation
                    vertexValues[vi] = neighbouringCells.Select(ci => values[ci]).Average();
                }
                // else 0.0 (1d vertex?)
            }

            grid = layer.GetQuadTreeOptimizedModelGrid(grid, UnstructuredBaseLayer.ElementType.Cells);

            for (var i = 0; i < grid.Cells.Count; i++)
            {
                var cell = grid.Cells[i];
                var points = new List<PointF>();
                var colors = new List<Color>();
                
                foreach (var vertexIndex in cell.VertexIndices)
                {
                    var vertex = layer.PerformProjection(grid.Vertices[vertexIndex]);
                    points.Add(new PointF((float)vertex.X, (float)vertex.Y));
                    colors.Add(theme.GetFillColor(vertexValues[vertexIndex]));
                }

                renderer.FillPolygon(points.ToArray(), colors.ToArray());
            }
        }
    }
}