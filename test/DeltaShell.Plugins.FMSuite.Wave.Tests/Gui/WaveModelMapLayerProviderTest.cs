using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.UI.Forms;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

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

        [Test]
        public void Test_GetWaveModelsFunction_ShouldGetCorrectModels()
        {
            using (var gui = new DeltaShellGui())
            {
                gui.Plugins.Add(new SharpMapGisGuiPlugin { Gui = gui });
                var waveGuiPlugin = new WaveGuiPlugin() { Gui = gui };         
                gui.Plugins.Add(waveGuiPlugin);

                var modelOne = new WaveModel();
                var modelTwo = new WaveModel();

                gui.Application = new DeltaShellApplication { Project = new Project() };
                var app = gui.Application;
                app.Project.RootFolder.Add(modelOne);
                app.Project.RootFolder.Add(modelTwo);

                var mapLayerProvider = waveGuiPlugin.MapLayerProvider as WaveModelMapLayerProvider;

                Assert.NotNull(mapLayerProvider);

                var models = mapLayerProvider.GetWaveModels.Invoke().ToList();

                Assert.AreEqual(2, models.Count);
                Assert.IsTrue(models.Contains(modelOne));
                Assert.IsTrue(models.Contains(modelTwo));
            }
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
