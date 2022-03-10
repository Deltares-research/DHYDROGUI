using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Converters.WellKnownText;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Tools
{
    [TestFixture]
    public class MapToolTest // TODO: name is not consistent with the class name
    {
        private static MapControl mapControl;
        private static IHydroNetwork network;
        private static HydroRegionMapLayer networkMapLayer;
        private ILayer channelLayer;

        [SetUp]
        public void Initialize()
        {
            mapControl = new MapControl { Map = { Size = new Size(1000, 1000) } };
            mapControl.Resize += delegate { mapControl.Refresh(); };
            mapControl.ActivateTool(mapControl.SelectTool);

            network = new HydroNetwork();
            networkMapLayer = (HydroRegionMapLayer) MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> {new NetworkEditorMapLayerProvider()});
            channelLayer = networkMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(Channel));

            mapControl.Map.Layers.Add(networkMapLayer);
            HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapControl);
        }

        [TearDown]
        public void TearDown()
        {
            mapControl.Dispose();
        }

        /// <summary>
        /// This test should ideally be in DeltaShell.Plugins.SharpMap.UI but this is difficult 
        /// because in DeltaShell.Plugins.SharpMap.UI features are not available.
        /// </summary>
        [Test]
        public void GetNextFeatureTest()
        {
            // Create a simple network and add 2 segments
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 30 40, 70 40, 100 100)"));
            
            //var branch = network.Channels[0];

            //HydroNetworkHelper.GenerateDiscretization(branch, new[] { 0, branch.Geometry.Length / 2, branch.Geometry.Length });

            // At location 0 0 there are the following features 
            // node, branch (start), /*segment, segment boundary,*/ boundary
            var selectTool = mapControl.SelectTool;
            var selectedFeature = new Dictionary<IFeature, int>();

            IFeature nextFeature = null;
            nextFeature = selectTool.GetNextFeatureAtPosition(new Coordinate(0, 0), 1, out ILayer _, nextFeature, ol => ol.Visible);
            selectedFeature.Add(nextFeature, 1);
            nextFeature = selectTool.GetNextFeatureAtPosition(new Coordinate(0, 0), 1, out ILayer _, nextFeature, ol => ol.Visible);
            selectedFeature.Add(nextFeature, 1);
            //nextFeature = selectTool.GetNextFeatureAtPosition(new Coordinate(0, 0), 1, out outLayer, nextFeature, ol => ol.IsVisible);
            //selectedFeature.Add(nextFeature, 1);
            //nextFeature = selectTool.GetNextFeatureAtPosition(new Coordinate(0, 0), 1, out outLayer, nextFeature, ol => ol.IsVisible);
            //selectedFeature.Add(nextFeature, 1);
            // we are only interested in cyclic selection not the actual sequence
            Assert.AreEqual(2, selectedFeature.Count);
        }

        [Test]
        public void GetNextHiddenFeatureNotPossible()
        {
            // Create a simple network and add 2 segments
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 30 40, 70 40, 100 100)"));
            
            var selectTool = mapControl.SelectTool;

            IFeature nextFeature = null;

            nextFeature = selectTool.GetNextFeatureAtPosition(new Coordinate(0, 0), 1, out ILayer _, nextFeature, ol => ol.Visible);
            Assert.IsNotNull(nextFeature);

            networkMapLayer.Visible = false;
            
            nextFeature = selectTool.GetNextFeatureAtPosition(new Coordinate(0, 0), 1, out ILayer _, nextFeature, ol => ol.Visible);
            Assert.IsNull(nextFeature);
        }

        [Test]
        public void GetHiddenFeatureNotPossible()
        {
            // Create a simple network and add 2 segments
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 30 40, 70 40, 100 100)"));

            var selectTool = mapControl.SelectTool;

            var feature = selectTool.FindNearestFeature(new Coordinate(0, 0), 1f, out ILayer _, ol => ol.Visible);
            Assert.IsNotNull(feature);

            networkMapLayer.Visible = false;

            feature = selectTool.FindNearestFeature(new Coordinate(0, 0), 1f, out ILayer _, ol => ol.Visible);
            Assert.IsNull(feature);
        }
    }
}
