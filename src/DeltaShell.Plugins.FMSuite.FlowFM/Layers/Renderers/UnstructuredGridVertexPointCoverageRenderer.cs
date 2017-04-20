using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Layers.Renderers;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid.ModelGrid;
using SharpMap.Api;
using SharpMap.Rendering.Thematics;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Layers.Renderers
{
    public class UnstructuredGridVertexPointCoverageRenderer : IUnstructuredGridCoverageRenderer
    {
        public void Render(IPrimitivesRenderer renderer, UnstructuredBaseLayer layer, UnstructuredModelGrid grid, ITheme theme, double[] values)
        {
            grid = layer.GetQuadTreeOptimizedModelGrid(grid, UnstructuredBaseLayer.ElementType.Vertices, ref values);

            for (int i = 0; i < grid.Vertices.Count; i++)
            {
                var vertex = layer.PerformProjection(grid.Vertices[i]);
                renderer.SetFillColor(theme.GetFillColor(values[i]));
                renderer.FillCircle(new PointF((float) vertex.X, (float) vertex.Y), 12);
            }
        }
    }
}