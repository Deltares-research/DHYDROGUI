using DeltaShell.Plugins.FMSuite.Common.Layers.Renderers;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid.ModelGrid;
using GisSharpBlog.NetTopologySuite.Geometries;
using SharpMap.Api;
using SharpMap.Rendering.Thematics;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Layers.Renderers
{
    public class UnstructuredGridVertexLineCoverageRenderer : IUnstructuredGridCoverageRenderer
    {
        public void Render(IPrimitivesRenderer renderer, UnstructuredBaseLayer layer, UnstructuredModelGrid grid, ITheme theme, double[] values)
        {
            grid = layer.GetQuadTreeOptimizedModelGrid(grid, UnstructuredBaseLayer.ElementType.Edges, ref values);

            for (var i = 0; i < grid.Edges.Count; i++)
            {
                var edge = grid.Edges[i];

                var v0 = layer.PerformProjection(grid.Vertices[edge.VertexFromIndex]);
                var v1 = layer.PerformProjection(grid.Vertices[edge.VertexToIndex]);

                var v0value = values[edge.VertexFromIndex];
                var v1value = values[edge.VertexToIndex];

                var edgeCenter = new Coordinate((v0.X + v1.X)/2.0, (v0.Y + v1.Y)/2.0);
                
                // poor mans gradient:
                renderer.SetFillColor(theme.GetFillColor(v0value));
                renderer.DrawLine((float) v0.X, (float) v0.Y, (float) edgeCenter.X, (float) edgeCenter.Y, 2f);

                renderer.SetFillColor(theme.GetFillColor(v1value));
                renderer.DrawLine((float)v1.X, (float)v1.Y, (float)edgeCenter.X, (float)edgeCenter.Y, 2f);
            }
        }
    }
}