using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Layers;
using SharpTestsEx;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class NetworkEditorMapLayerProviderTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithEnclosure()
        {
            var area = new HydroArea();

            var enclosureFeature = new GroupableFeature2DPolygon()
            {
                Name = "Enclosure01",
                Geometry = new MultiLineString(new ILineString[]
                {
                    /* 
                           (2.0, 10.0) O----------O (10.0, 10.0)             
                                      /           |
                                     /            |
                         (0.0, 5.0) O             O (10.0, 5.0)
                                    |              \
                                    |               \
                         (0.0, 0.0) O----------------O (12.0, 0.0)
                    */

                    new LineString(new [] {new Coordinate(0.0, 0.0), new Coordinate(12.0, 0.0) }),
                    new LineString(new [] {new Coordinate(12.0, 0.0), new Coordinate(10.0, 5.0) }),
                    new LineString(new [] {new Coordinate(10.0, 5.0), new Coordinate(10.0, 10.0) }),
                    new LineString(new [] {new Coordinate(10.0, 10.0), new Coordinate(2.0, 10.0) }),
                    new LineString(new [] {new Coordinate(2.0, 10.0), new Coordinate(0.0, 5.0) }),
                    new LineString(new [] {new Coordinate(0.0, 5.0), new Coordinate(0.0, 0.0) }),
                })
            };

            area.Enclosures.Add(enclosureFeature);
            
            using (var mapView = new MapView())
            {
                var layer = MapLayerProviderHelper.CreateLayersRecursive(area.Enclosures, area, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider()});
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate
                {
                    mapView.Map.ZoomToExtents();
                });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithPump2D()
        {
            var area = new HydroArea();

            var pump2DFeature = new Pump2D()
            {
                Capacity = 2.0,
                StartDelivery = 0.0,
                StopDelivery = 0.0,
                StartSuction = 0.001,
                StopSuction = 0.0,
                DirectionIsPositive = true,
                Name = "pump2D01",
                Geometry = new LineString(new[] { new Coordinate(0.0, 0.0), new Coordinate(12.0, 0.0) })
            };

            area.Pumps.Add(pump2DFeature);

            using (var mapView = new MapView())
            {
                var layer = MapLayerProviderHelper.CreateLayersRecursive(area.Pumps, area, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider() });
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate
                {
                    mapView.Map.ZoomToExtents();
                });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithGate2D()
        {
            var area = new HydroArea();

            var gate2DFeature = new Gate2D()
            {
                Name = "Gate2D01",
                Geometry = new LineString(new[] { new Coordinate(0.0, 0.0), new Coordinate(12.0, 0.0) })
            };

            area.Gates.Add(gate2DFeature);

            using (var mapView = new MapView())
            {
                var layer = MapLayerProviderHelper.CreateLayersRecursive(area.Gates, area, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider() });
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate
                {
                    mapView.Map.ZoomToExtents();
                });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithWeir2D()
        {
            var area = new HydroArea();

            var weir2DFeature = new Weir2D()
            {
                Name = "weir2D01",
                Geometry = new LineString(new[] { new Coordinate(0.0, 0.0), new Coordinate(12.0, 0.0) })
            };

            area.Weirs.Add(weir2DFeature);

            using (var mapView = new MapView())
            {
                var layer = MapLayerProviderHelper.CreateLayersRecursive(area.Weirs, area, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider() });
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate
                {
                    mapView.Map.ZoomToExtents();
                });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithPump()
        {
            var network = new HydroNetwork();

            var node1 = new HydroNode { Name = "node1" };
            var node2 = new HydroNode { Name = "node2" };
            var branch1 = new Channel { Name = "branch1", Source = node1, Target = node2 };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(branch1);
            //add geometry to the stuff or we won't see it
            var wktReader = new WKTReader();
            branch1.Geometry = wktReader.Read("LINESTRING(0 0,10 0)");
            node1.Geometry = wktReader.Read("POINT(0 0)");
            node2.Geometry = wktReader.Read("POINT(10 0)");

            // Add a Pump
            var pump = new Pump { Capacity = 2.0, StartDelivery = 0.0, StopDelivery = 0.0, StartSuction = 0.001, StopSuction = 0.0, DirectionIsPositive = true };
            var channel = network.Channels.First();

            var compositeBranchStructure = new CompositeBranchStructure
            {
                Network = network,
                Geometry = new Point(5, 0),//point is used to render the item
                Chainage = 5,//offset is used by model
            };

            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, channel, compositeBranchStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, pump);

            using (var mapView = new MapView())
            {
                var layer = MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider>{new NetworkEditorMapLayerProvider(),new SharpMapLayerProvider()});
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate
                {
                    mapView.Map.ZoomToExtents();
                });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void LinkLayerIsCreatedForBasinButNotForNetwork()
        {
            var basinLayer = (GroupLayer) MapLayerProviderHelper.CreateLayersRecursive(new DrainageBasin(), null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider(), new SharpMapLayerProvider() });
            Assert.IsTrue(basinLayer.Layers.Any(l => l.Name.StartsWith("Links")));

            var networkLayer = (GroupLayer)MapLayerProviderHelper.CreateLayersRecursive(new HydroNetwork(), null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider(), new SharpMapLayerProvider() });
            Assert.IsFalse(networkLayer.Layers.Any(l => l.Name.StartsWith("Links")));
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithLateral()
        {
            var network = new HydroNetwork();

            var node1 = new HydroNode { Name = "node1" };
            var node2 = new HydroNode { Name = "node2" };
            var branch1 = new Channel { Name = "branch1", Source = node1, Target = node2 };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(branch1);
            //add geometry to the stuff or we won't see it
            var wktReader = new WKTReader();
            branch1.Geometry = wktReader.Read("LINESTRING(0 0,10 0)");
            node1.Geometry = wktReader.Read("POINT(0 0)");
            node2.Geometry = wktReader.Read("POINT(10 0)");

            var lateral = new LateralSource();
            lateral.Geometry = wktReader.Read("POINT(5 0)");
            NetworkHelper.AddBranchFeatureToBranch(lateral, branch1, 5);

            using (var mapView = new MapView())
            {
                var layer = MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider(), new SharpMapLayerProvider() });
                mapView.Map.Layers.Add(layer);

                WindowsFormsTestHelper.ShowModal(mapView, delegate
                {
                    mapView.Map.ZoomToExtents();
                });
            }
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMapLayerWithRoute()
        {
            var network = new HydroNetwork();

            var node1 = new HydroNode { Name = "node1" };
            var node2 = new HydroNode { Name = "node2" };
            var branch1 = new Channel { Name = "branch1", Source = node1, Target = node2 };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(branch1);
            //add geometry to the stuff or we won't see it
            var wktReader = new WKTReader();
            branch1.Geometry = wktReader.Read("LINESTRING(0 0,10 0)");
            node1.Geometry = wktReader.Read("POINT(0 0)");
            node2.Geometry = wktReader.Read("POINT(10 0)");

            var lateral = new LateralSource();
            lateral.Geometry = wktReader.Read("POINT(5 0)");
            NetworkHelper.AddBranchFeatureToBranch(lateral, branch1, 5);

            network.Routes.Add(RouteHelper.CreateRoute(new INetworkLocation[] { new NetworkLocation(branch1, 2), new NetworkLocation(branch1, 8) }));

            using (var mapView = new MapView())
            {
                var layer = MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider(), new SharpMapLayerProvider() });
                mapView.Map.Layers.Add(layer);
                
                WindowsFormsTestHelper.ShowModal(mapView, f => mapView.Map.ZoomToExtents());
            }
        }

        [Test]
        public void AddingFeaturesToBranchAddsToVariousHydroNetworkLayers()
        {
            var network = new HydroNetwork();
            using (var mapView = new MapView())
            {
                var layer = MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider(), new SharpMapLayerProvider() });
                mapView.Map.Layers.Add(layer);

                var node1 = new HydroNode { Name = "node1" };
                network.Nodes.Add(node1);

                var hydroNodeLayer = ((GroupLayer) layer).Layers.First(l => l.DataSource.FeatureType == typeof (HydroNode));
                Assert.IsTrue(hydroNodeLayer.DataSource.Contains(node1));

                var branch = new Channel();
                network.Branches.Add(branch);
                var crossSectionDef = new CrossSectionDefinitionXYZ();
                var crossSection = new CrossSection(crossSectionDef);

                branch.BranchFeatures.Add(crossSection);
                var crossSectionLayer = ((GroupLayer) layer).Layers.First(l => l.DataSource.FeatureType == typeof (CrossSection));
                Assert.IsTrue(crossSectionLayer.DataSource.Contains(crossSection));
            }
        }
 
        [Test]
        public void ChangingAPropertyCausesRenderRequired()
        {
            //since maplayer is not parent this bubbling causes unneeded events.
            var network = new HydroNetwork();
            var layer = MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider(), new SharpMapLayerProvider() });
            var branch = new Channel();
            network.Branches.Add(branch);

            layer.RenderRequired = false;

            network.BeginEdit(new DefaultEditAction("edit name"));
            branch.Name = "new name";
            network.EndEdit();

            Assert.IsTrue(layer.RenderRequired);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.WindowsForms)]
        public void ShowHydroRegionWithDrainageBasinAndNetworkAndCatchmentLinkedToLateral()
        {
            var wktReader = new WKTReader();
            var node1 = new HydroNode { Name = "node1", Geometry = wktReader.Read("POINT(10 0)") };
            var node2 = new HydroNode { Name = "node2", Geometry = wktReader.Read("POINT(0 0)") };
            var branch1 = new Channel { Name = "branch1", Source = node1, Target = node2, Geometry = wktReader.Read("LINESTRING(0 0,10 0)") };
            var lateral = new LateralSource() {Name = "lateral1", Branch = branch1, Chainage = 5.0, Geometry = wktReader.Read("POINT(5 0)")};
            branch1.BranchFeatures.Add(lateral);
            var network = new HydroNetwork { Branches = { branch1 }, Nodes = { node1, node2 } };

            var catchment = Catchment.CreateDefault();
            var basin = new DrainageBasin { Catchments = { catchment } };

            var region = new HydroRegion { SubRegions = { network, basin } };

            using (var mapView = new MapView())
            {
                var layer = MapLayerProviderHelper.CreateLayersRecursive(region, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider(), new SharpMapLayerProvider() });
                mapView.Map.Layers.Add(layer);

                // add link using drainage basin map layer (link between features of the drainage basin map layer)
                var linkLayer = ((GroupLayer) layer).Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof (HydroLink));
                linkLayer.DataSource.Add(new LineString(new[] { catchment.Geometry.Centroid.Coordinate, lateral.Geometry.Coordinate }));

                // asserts
                region.Links.Count
                    .Should().Be.EqualTo(1);

                region.Links[0].Source
                    .Should().Be.SameInstanceAs(catchment);

                region.Links[0].Target
                    .Should().Be.SameInstanceAs(lateral);

                WindowsFormsTestHelper.ShowModal(mapView, delegate
                {
                    mapView.Map.ZoomToExtents();
                });
            }
        }

        [Test]
        [TestCase(typeof(HydroNetwork))]
        [TestCase(typeof(HydroArea))]
        public void GivenNetworkEditorMapLayerProvider_CreatingLayer_ShouldSetRenderOrder(Type type)
        {
            //Arrange
            var layerObject = Activator.CreateInstance(type);

            // Act
            var layer = (IGroupLayer) MapLayerProviderHelper.CreateLayersRecursive(layerObject, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider() }, new Dictionary<ILayer, object>());

            // Assert
            var layersWithoutOrder  = layer.Layers
                .Where(l => !(l is IGroupLayer))
                .Count(l => l.RenderOrder == 0);

            Assert.AreEqual(0, layersWithoutOrder);
        }
    }
}