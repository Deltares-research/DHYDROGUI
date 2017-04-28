using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Layers.Renderers;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid.ModelGrid;
using SharpMap.Api;
using SharpMap.Rendering.Thematics;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Layers.Renderers
{
    public class UnstructuredGridFlowLinkPointCoverageRenderer : IUnstructuredGridCoverageRenderer
    {
        public void Render(IPrimitivesRenderer renderer, UnstructuredBaseLayer layer, UnstructuredModelGrid grid, ITheme theme, double[] values)
        {
            grid = layer.GetQuadTreeOptimizedModelGrid(grid, UnstructuredBaseLayer.ElementType.FlowLink, ref values);

            for (var i = 0; i < grid.FlowLinks.Count; i++)
            {
                var flowLink = grid.FlowLinks[i];
                var center = layer.PerformProjection(flowLink.GetCenter(grid));
                renderer.SetFillColor(theme.GetFillColor(values[i]));
                renderer.FillCircle(new PointF((float) center.X, (float) center.Y), 12f);
            }
        }
    }
}