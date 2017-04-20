using System.Drawing;
using DeltaShell.Plugins.FMSuite.Common.Layers.Renderers;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid.ModelGrid;
using SharpMap.Api;
using SharpMap.Rendering.Thematics;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Layers.Renderers
{
    public class UnstructuredGridCellPointCoverageRenderer : IUnstructuredGridCoverageRenderer
    {
        public void Render(IPrimitivesRenderer renderer, UnstructuredBaseLayer layer, UnstructuredModelGrid grid, ITheme theme, double[] values)
        {
            grid = layer.GetQuadTreeOptimizedModelGrid(grid, UnstructuredBaseLayer.ElementType.Cells, ref values);

            for (var i = 0; i < grid.Cells.Count; i++)
            {
                var cell = grid.Cells[i];
                var center = layer.PerformProjection(cell.Center);
                renderer.SetFillColor(theme.GetFillColor(values[i]));
                renderer.FillCircle(new PointF((float)center.X, (float)center.Y), 12f);
            }
        }
    }
}