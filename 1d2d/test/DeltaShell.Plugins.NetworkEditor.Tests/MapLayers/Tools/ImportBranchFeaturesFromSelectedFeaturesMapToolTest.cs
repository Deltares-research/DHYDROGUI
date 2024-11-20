using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Api.Layers;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using SharpTestsEx;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Tools
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ImportBranchFeaturesFromSelectedFeaturesMapToolTest
    {
        private IHydroNetwork network;
        private MapControl mapControl;
        private Map map;
        private HydroRegionMapLayer networkLayer;
        private HydroRegionEditorMapTool hydroRegionEditorMapTool;
        private ILayer weirLayer;

        [SetUp]
        public void SetUp()
        {
            network = new HydroNetwork();
            var node1 = new HydroNode { Name = "node1", Geometry = new Point(10, 10)};
            var node2 = new HydroNode { Name = "node2", Geometry = new Point(90, 90) };
            var branch1 = new Channel
            {
                Name = "branch1",
                Source = node1,
                Target = node2,
                Geometry = new LineString(new [] { new Coordinate(10, 10), new Coordinate(90, 90) })
            };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(branch1);

            networkLayer = (HydroRegionMapLayer) MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> {new NetworkEditorMapLayerProvider()});
            weirLayer = networkLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(Weir));

            map = new Map {Size = new Size(100, 100)};
            map.Layers.Add(networkLayer);

            mapControl = new MapControl { Map = map };

            // HACK: why do we need to initialize it? API does not look intuitive, clean enough
            hydroRegionEditorMapTool = HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapControl);

            hydroRegionEditorMapTool.Tolerance = 10;
        }

        [TearDown]
        public void TearDown()
        {
            mapControl.Dispose();
            map.ClearImage();
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ExecuteAndShowDialog()
        {
            var tool = new ImportBranchFeaturesFromSelectedFeaturesMapTool(l => l.Equals(networkLayer));
            mapControl.Tools.Add(tool);

            // add layer with 3 features and select 1st feature
            var geometries = new Collection<IGeometry> { new Point(20, 20), new Point(20, 25), new Point(20, 40) };
            var featureTable = new DataTableFeatureProvider(geometries);
            var vectorLayer = new VectorLayer { DataSource = featureTable };
            map.Layers.Add(vectorLayer);

            mapControl.SelectTool.AddSelection(vectorLayer, featureTable.GetFeature(0));
            tool.ImportSelectedFeaturesAsBranchFeatures(weirLayer);

            // add controls and show
            var executeToolButton = new Button();
            executeToolButton.Click += delegate { tool.Execute(); };

            mapControl.Controls.Add(executeToolButton);

            WindowsFormsTestHelper.ShowModal(mapControl);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportSelectedFeaturesAsPumps()
        {
            var tool = new ImportBranchFeaturesFromSelectedFeaturesMapTool(l => l.Equals(networkLayer)) { Tolerance = 10 };
            mapControl.Tools.Add(tool);

            // add layer with 3 features
            var geometries = new Collection<IGeometry> { new Point(20, 20), new Point(20, 25), new Point(20, 40) };
            var featureTable = new DataTableFeatureProvider(geometries);
            
            var attributesTable = featureTable.AttributesTable;
            
            attributesTable.Columns.Add("Name", typeof (string));
            attributesTable.Rows[0]["Name"] = "feature1";
            attributesTable.Rows[1]["Name"] = "feature2";
            attributesTable.Rows[2]["Name"] = "feature3";

            var vectorLayer = new VectorLayer { DataSource = featureTable };
            map.Layers.Add(vectorLayer);

            // select all 3 features
            var feature1 = featureTable.GetFeature(0);
            var feature2 = featureTable.GetFeature(1);
            var feature3 = featureTable.GetFeature(2);

            mapControl.SelectTool.AddSelection(vectorLayer, feature1);
            mapControl.SelectTool.AddSelection(vectorLayer, feature2);
            mapControl.SelectTool.AddSelection(vectorLayer, feature3);
            
            // imports
            tool.ImportSelectedFeaturesAsBranchFeatures(weirLayer);

            network.Weirs.Count()
                .Should().Be.EqualTo(2);
            
            network.Weirs.First().Name
                .Should().Be.EqualTo("feature1");

            network.Weirs.ElementAt(1).Name
                .Should().Be.EqualTo("feature2");
        }
    }
}