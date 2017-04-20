using System.Drawing;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.Layers.Renderers;
using DeltaShell.Plugins.FMSuite.FlowFM.Grid.ModelGrid;
using GeoAPI.Geometries;
using SharpMap.Api;
using SharpMap.Rendering.Thematics;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Layers.Renderers
{
    public class UnstructuredGridFlowLinkDiamondCoverageRenderer : IUnstructuredGridCoverageRenderer
    {
        //draws wiebertjes (ruitjes, diamonds)
        public void Render(IPrimitivesRenderer renderer, UnstructuredBaseLayer layer, UnstructuredModelGrid grid, ITheme theme, double[] values)
        {
            grid = layer.GetQuadTreeOptimizedModelGrid(grid, UnstructuredBaseLayer.ElementType.FlowLink, ref values);

            for (var i = 0; i < grid.FlowLinks.Count; i++)
            {
                var flowLink = grid.FlowLinks[i];
                renderer.SetFillColor(theme.GetFillColor(values[i]));

                ICoordinate[] coordinates;
                if (flowLink.IsBoundaryLink)
                {
                    // custom rendering: we don't have a cell on each side
                    var v0 = grid.Cells[flowLink.CellToIndex].Center; //we know that CellToIndex is != -1
                    var v1 = grid.Vertices[flowLink.Edge.VertexFromIndex];
                    var v2 = grid.Vertices[flowLink.Edge.VertexToIndex];
                    coordinates = new[] { v0, v1, v2 };
                }
                else
                {
                    var v0 = grid.Cells[flowLink.CellFromIndex].Center;
                    var v1 = grid.Vertices[flowLink.Edge.VertexFromIndex];
                    var v2 = grid.Cells[flowLink.CellToIndex].Center;
                    var v3 = grid.Vertices[flowLink.Edge.VertexToIndex];
                    coordinates = new[] {v0, v1, v2, v3};
                }

                var points = coordinates.Select(layer.PerformProjection).Select(c => new PointF((float) c.X, (float) c.Y)).ToArray();
                renderer.FillPolygon(points);
            }
        }
    }
}