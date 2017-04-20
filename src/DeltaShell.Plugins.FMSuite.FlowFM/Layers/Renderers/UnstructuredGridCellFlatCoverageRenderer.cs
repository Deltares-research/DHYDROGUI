using System.Collections.Generic;
using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Layers.Renderers;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid.ModelGrid;
using SharpMap.Api;
using SharpMap.Rendering.Thematics;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Layers.Renderers
{
    public class UnstructuredGridCellFlatCoverageRenderer : IUnstructuredGridCoverageRenderer
    {
        public void Render(IPrimitivesRenderer renderer, UnstructuredBaseLayer layer, UnstructuredModelGrid grid, ITheme theme, double[] values)
        {
            grid = layer.GetQuadTreeOptimizedModelGrid(grid, UnstructuredBaseLayer.ElementType.Cells, ref values);

            for (int i = 0; i < grid.Cells.Count; i++)
            {
                var cell = grid.Cells[i];
                var points = new List<PointF>();

                foreach (var vertexIndex in cell.VertexIndices)
                {
                    var vertex = layer.PerformProjection(grid.Vertices[vertexIndex]);
                    points.Add(new PointF((float)vertex.X, (float)vertex.Y));
                }

                renderer.SetFillColor(theme.GetFillColor(values[i]));
                renderer.FillPolygon(points.ToArray());
            }
        }
    }
}