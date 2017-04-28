using System.Drawing;
using System.Windows.Forms;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api.Layers;
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
