using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Layers.Renderers;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid.ModelGrid;
using SharpMap.Api;
using SharpMap.Rendering.Thematics;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Layers.Renderers
{
    public class UnstructuredGridVertexFillCoverageRenderer : IUnstructuredGridCoverageRenderer
    {
        public void Render(IPrimitivesRenderer renderer, UnstructuredBaseLayer layer, UnstructuredModelGrid grid, ITheme theme, double[] values)
        {
            grid = layer.GetQuadTreeOptimizedModelGrid(grid, UnstructuredBaseLayer.ElementType.Cells);

            for (int cellIndex = 0; cellIndex < grid.Cells.Count; cellIndex++)
            {
                var cell = grid.Cells[cellIndex];
                var numVerticesForCell = cell.VertexIndices.Length;

                var points = new PointF[numVerticesForCell];
                var colors = new Color[numVerticesForCell];

                for (int cellVertexIndex = 0; cellVertexIndex < numVerticesForCell; cellVertexIndex++)
                {
                    var gridVertexIndex = cell.VertexIndices[cellVertexIndex];
                    var vertex = layer.PerformProjection(grid.Vertices[gridVertexIndex]);
                    var color = theme.GetFillColor(values[gridVertexIndex]);

                    points[cellVertexIndex] = new PointF((float)vertex.X, (float)vertex.Y);
                    colors[cellVertexIndex] = color;
                }

                renderer.FillPolygon(points, colors);
            }
        }
    }
}