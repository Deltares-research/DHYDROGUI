using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using GeoAPI.Extensions.CoordinateSystems;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveModelMapLayerProviderTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowLayersForWaveModel()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary\obw.mdw");
            var model = new WaveModel(mdwPath);
            ShowModelLayers(model);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CreateLayer_WithDiscreteGridPointCoverage_ThenCurvilinearVertexCoverageLayerWithExpectedValuesIsReturned(bool isEditable)
        {
            // Setup
            var mapLayerProvider = new WaveModelMapLayerProvider();

            var outputCoverage = new DiscreteGridPointCoverage
            {
                Name = "Name",
                IsEditable = isEditable
            };

            var waveModel = MockRepository.GenerateStub<IWaveModel>();
            var coordinateSystem = MockRepository.GenerateStub<ICoordinateSystem>();
            waveModel.CoordinateSystem = coordinateSystem;

            // Call
            var layer = (CurvilinearVertexCoverageLayer) mapLayerProvider.CreateLayer(outputCoverage, waveModel);

            // Assert
            Assert.AreEqual("Name", layer.Name, "The name of the coverage is not correctly set in the layer");
            Assert.AreSame(outputCoverage, layer.Coverage, "The coverage is not correctly set in the layer");
            Assert.IsFalse(layer.Visible, "The visibility of the layer should be false");
            Assert.IsFalse(layer.OptimizeRendering, "The optimize rendering setting of the layer should be false");
            Assert.AreEqual(!isEditable, layer.ReadOnly, "The read only setting is not correctly set in the layer");

            var dataSource = (WaveGridBasedDataSource) layer.DataSource;
            Assert.AreSame(waveModel.CoordinateSystem, dataSource.CoordinateSystem, "The coordinate system in the data source of the layer is not the same as the coordinate system of the coverage");
        }

        private static void ShowModelLayers(WaveModel model)
        {
            var provider = new WaveModelMapLayerProvider();

            var layer = (IGroupLayer)MapLayerProviderHelper.CreateLayersRecursive(model, null, new[] { provider });

            layer.Layers.ForEach(l => l.Visible = true);

            var map = new Map { Layers = { layer }, Size = new Size { Width = 800, Height = 800 } };
            map.ZoomToExtents();

            var mapControl = new MapControl { Map = map, Dock = DockStyle.Fill };

            WindowsFormsTestHelper.ShowModal(mapControl);
        }
    }
}
