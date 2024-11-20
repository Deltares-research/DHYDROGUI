using System.Drawing;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Forms
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class UnstructuredGridLayerTest
    {
        /// <summary>
        /// It is expected that there is a thin dam in the grid that shows that
        /// there are a couple of thin dams that split cells.
        /// You can see these cells with another color.
        /// </summary>
        [Test]
        public void ShowWaqGeomGeneratedGridLayer()
        {
            string filePath = TestHelper.GetTestFilePath(@"IO\real\uni3d_flowgeom.nc");
            UnstructuredGrid grid = NetFileImporter.ImportModelGrid(filePath);

            var gridLayer = new UnstructuredGridLayer
            {
                Grid = grid,
                Renderer = new GridEdgeRenderer(Color.Blue) {GridEdgeRenderMode = GridEdgeRenderMode.EdgesWithBlockedFlowLinks}
            };

            var map = new Map
            {
                Layers = {gridLayer},
                Size = new Size
                {
                    Width = 800,
                    Height = 800
                }
            };
            map.ZoomToExtents();

            var mapControl = new MapControl
            {
                Map = map,
                Dock = DockStyle.Fill
            };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }
    }
}