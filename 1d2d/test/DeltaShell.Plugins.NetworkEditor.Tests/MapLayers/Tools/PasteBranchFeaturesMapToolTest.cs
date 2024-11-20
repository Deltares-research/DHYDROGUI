using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Tools
{
    [TestFixture]
    public class PasteBranchFeaturesMapToolTest
    {
        private ClipboardMock clipboard;

        [SetUp]
        public void SetUp()
        {
            if (!GuiTestHelper.IsBuildServer) return;
            clipboard = new ClipboardMock();
            clipboard.GetText_Returns_SetText();
            clipboard.GetData_Returns_SetData();
        }

        [TearDown]
        public void TearDown()
        {
            if (!GuiTestHelper.IsBuildServer) return;
            clipboard.Dispose();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CopyAndPasteBridgeInSameNetwork()
        {
            var network = new HydroNetwork();
            var channel = new Channel();
            var bridge = new Bridge { Geometry = new LineString(new[] { new Coordinate(10, 0), new Coordinate(10, 0) }) };

            network.Branches.Add(channel);
            NetworkHelper.AddBranchFeatureToBranch(bridge, channel, 0);

            var map = new Map { Size = new Size(100, 100) };
            using (var mapControl = new MapControl { Map = map })
            {
                var networkMapLayer = MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> {new NetworkEditorMapLayerProvider()});

                map.Layers.Add(networkMapLayer);
                mapControl.Resize += delegate { mapControl.Refresh(); };
                mapControl.ActivateTool(mapControl.SelectTool);
                HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapControl);

                mapControl.SelectTool.Select(bridge);

                var selectedFeature = mapControl.SelectedFeatures.First();
                var branchFeature = selectedFeature as IBranchFeature;
                if (branchFeature != null)
                {
                    HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(branchFeature);
                }

                var pasteTool = new PasteBranchFeaturesMapTool(l => l.Equals(networkMapLayer)) { MapControl = mapControl };
                pasteTool.Execute();

                var mapTool = mapControl.Tools.First(tool => tool.Name == HydroRegionEditorMapTool.AddBridgeToolName);
                Assert.IsTrue(mapTool.IsActive);
            }
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void CopyAndPasteBridgeInDifferentNetworks()
        {
            var network1 = new HydroNetwork();
            var network2 = new HydroNetwork();
            var channel = new Channel();
            var bridge = new Bridge { Geometry = new LineString(new[] { new Coordinate(10, 0), new Coordinate(10, 0) }) };

            network1.Branches.Add(channel);
            NetworkHelper.AddBranchFeatureToBranch(bridge, channel, 0);

            var map1 = new Map { Size = new Size(100, 100) };
            using (var mapControl1 = new MapControl { Map = map1 })
            {
                var networkMapLayer1 = MapLayerProviderHelper.CreateLayersRecursive(network1, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider() });

                map1.Layers.Add(networkMapLayer1);
                mapControl1.Resize += delegate { mapControl1.Refresh(); };
                mapControl1.ActivateTool(mapControl1.SelectTool);
                HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapControl1);

                var map2 = new Map { Size = new Size(100, 100) };
                var mapControl2 = new MapControl { Map = map2 };
                var networkMapLayer2 = MapLayerProviderHelper.CreateLayersRecursive(network2, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider() });

                map2.Layers.Add(networkMapLayer2);
                mapControl2.Resize += delegate { mapControl2.Refresh(); };
                mapControl2.ActivateTool(mapControl2.SelectTool);
                HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapControl2);

                mapControl1.SelectTool.Select(bridge);

                var selectedFeature = mapControl1.SelectedFeatures.First();
                var branchFeature = selectedFeature as IBranchFeature;
                if (branchFeature != null)
                {
                    HydroNetworkCopyAndPasteHelper.SetNetworkFeatureToClipBoard(branchFeature);
                }

                var pasteTool = new PasteBranchFeaturesMapTool(l => l.Equals(networkMapLayer2)) { MapControl = mapControl2 };
                pasteTool.Execute();

                var mapTool = mapControl2.Tools.First(tool => tool.Name == HydroRegionEditorMapTool.AddBridgeToolName);
                Assert.IsTrue(mapTool.IsActive);
            }
        }
    }
}
