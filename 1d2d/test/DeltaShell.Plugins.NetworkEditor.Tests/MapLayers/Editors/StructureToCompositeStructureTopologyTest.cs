using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.Editors;
using SharpMap.Api.Layers;
using SharpMap.Converters.WellKnownText;
using SharpMap.Layers;
using SharpMap.Styles;
using SharpMap.UI.Forms;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors
{
    [TestFixture]
    public class StructureToCompositeStructureTopologyTest
    {
        private HydroRegionMapLayer hydroNetworkLayer;
        private MapControl mapControl;

        private HydroRegionMapLayer InitializeNetworkEditor(IHydroNetwork network)
        {
            mapControl = new MapControl {Map = {Size = new Size(1000, 1000)}}; // enable coordinate conversions
            var nwLayer = (HydroRegionMapLayer)MapLayerProviderHelper.CreateLayersRecursive(network, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider() });
            mapControl.Map.Layers.Add(nwLayer);
            HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapControl);
            return nwLayer;
        }

        private ILayer GetChannelLayer(HydroRegionMapLayer regionMapLayer)
        {
            return regionMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof (Channel));
        }

        private ILayer GetPumpLayer(HydroRegionMapLayer regionMapLayer)
        {
            return regionMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(Pump));
        }

        private static ILayer GetChildLayerOfType<T>(HydroRegionMapLayer regionMapLayer)
        {
            return regionMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(T));
        }

        [TearDown]
        public void TearDown()
        {
            if(mapControl != null && !mapControl.IsDisposed)
            {
                mapControl.Dispose();
            }
        }

        [Test]
        public void TestAddStructureToNewCompositeStructure_GeneratesUniqueNamesForCompositeBranchStructures()
        {
            var network = new HydroNetwork();
            hydroNetworkLayer = InitializeNetworkEditor(network);
            AddGeometry(GetChannelLayer(hydroNetworkLayer), GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));

            AddGeometry(GetChildLayerOfType<Weir>(hydroNetworkLayer), GeometryFromWKT.Parse("Point (25 0)"));
            AddGeometry(GetChildLayerOfType<Weir>(hydroNetworkLayer), GeometryFromWKT.Parse("Point (25 0)"));
            AddGeometry(GetChildLayerOfType<Weir>(hydroNetworkLayer), GeometryFromWKT.Parse("Point (50 0)"));
            AddGeometry(GetChildLayerOfType<Weir>(hydroNetworkLayer), GeometryFromWKT.Parse("Point (75 0)"));

            Assert.AreEqual(3, network.CompositeBranchStructures.Count());
            Assert.IsTrue(network.CompositeBranchStructures.Select(cbs=>cbs.Name).HasUniqueValues());
        }

        [Test]
        public void AddPump()
        {
            var network = new HydroNetwork();
            
            hydroNetworkLayer = InitializeNetworkEditor(network);
            AddGeometry(GetChannelLayer(hydroNetworkLayer), GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            AddGeometry(GetPumpLayer(hydroNetworkLayer), GeometryFromWKT.Parse("Point (50 0)"));

            var pump = network.Pumps.First();
            var compositeStructure = network.CompositeBranchStructures.First();

            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(1, network.Pumps.Count());
            Assert.AreEqual(1, compositeStructure.Structures.Count);

            Assert.AreEqual(50, pump.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, pump.Geometry.Coordinates[0].Y);
            Assert.AreEqual(50, compositeStructure.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, compositeStructure.Geometry.Coordinates[0].Y);

            Assert.AreEqual(50, network.Pumps.First().Chainage);
            Assert.AreEqual(50, network.CompositeBranchStructures.First().Chainage);
        }

        // todo move this functionality to layer or provider
        private IFeature AddGeometry(ILayer layer, IGeometry geometry)
        {
            return layer.DataSource.Add(geometry);
            //IFeature feature = layer.DataSource.GetFeature(layer.DataSource.GetFeatureCount() - 1);
            //IFeatureInteractor FeatureInteractor = mapControl.SelectTool.GetFeatureInteractor(layer, feature);
            //if (null == FeatureInteractor) 
            //    return;
            //FeatureInteractor.Start();
            //FeatureInteractor.Stop();
        }

        [Test]
        public void AddStructures()
        {
            var network = new HydroNetwork();

            hydroNetworkLayer = InitializeNetworkEditor(network);
            AddGeometry(GetChannelLayer(hydroNetworkLayer), GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            AddGeometry(GetPumpLayer(hydroNetworkLayer), GeometryFromWKT.Parse("Point (50 0)"));
            AddGeometry(GetChildLayerOfType<Culvert>(hydroNetworkLayer), GeometryFromWKT.Parse("Point (50 0)"));
            AddGeometry(GetChildLayerOfType<Weir>(hydroNetworkLayer), GeometryFromWKT.Parse("Point (50 0)"));
            AddGeometry(GetChildLayerOfType<Bridge>(hydroNetworkLayer), GeometryFromWKT.Parse("Point (50 0)"));

            Assert.AreEqual(1, network.CompositeBranchStructures.Count());
            Assert.AreEqual(1, network.Pumps.Count());
            Assert.AreEqual(1, network.Culverts.Count());
            Assert.AreEqual(1, network.Weirs.Count());
            Assert.AreEqual(1, network.Bridges.Count());
        }

        [Test]
        public void AddPumps()
        {
            var network = new HydroNetwork();
            
            hydroNetworkLayer = InitializeNetworkEditor(network);
            AddGeometry(GetChannelLayer(hydroNetworkLayer), GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            AddGeometry(GetPumpLayer(hydroNetworkLayer), GeometryFromWKT.Parse("Point (50 0)"));
            AddGeometry(GetPumpLayer(hydroNetworkLayer), GeometryFromWKT.Parse("Point (50 0)"));

            Assert.AreEqual(2, network.Pumps.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());
            Assert.AreEqual(3, network.BranchFeatures.Count());
            
            var compositeStructure = network.CompositeBranchStructures.First();
            Assert.AreEqual(2, compositeStructure.Structures.Count());
            Assert.AreEqual(network.Pumps.First(), compositeStructure.Structures[0]);
            Assert.AreEqual(network.Pumps.Skip(1).First(), compositeStructure.Structures[1]);
        }


        [Test]
        public void MovePump()
        {
            var network = new HydroNetwork();
            
            hydroNetworkLayer = InitializeNetworkEditor(network);
            var pumpLayer = GetPumpLayer(hydroNetworkLayer);

            var channel = AddGeometry(GetChannelLayer(hydroNetworkLayer), GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            var pump = AddGeometry(pumpLayer, GeometryFromWKT.Parse("Point (50 0)")) as IPump;
            
            Assert.AreEqual(50, pump.Chainage);
            
            var featureInteractor = mapControl.SelectTool.GetFeatureInteractor(pumpLayer, pump);
            featureInteractor.Start();
            featureInteractor.MoveTracker(featureInteractor.Trackers[0], 20, 0);
            featureInteractor.Stop(new SnapResult(featureInteractor.TargetFeature.Geometry.Coordinate, channel, pumpLayer, channel.Geometry,0,0));

            Assert.AreEqual(1, network.Pumps.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());
            Assert.AreEqual(2, network.BranchFeatures.Count());

            Assert.AreEqual(70, network.Pumps.First().Chainage);
            Assert.AreEqual(70, network.CompositeBranchStructures.First().Chainage, "offset is updated in accordance to geometry");
        }

        [Test]
        public void MovePumpFromCompositeToComposite()
        {
            var network = new HydroNetwork();
            hydroNetworkLayer = InitializeNetworkEditor(network);
            var channel = AddGeometry(GetChannelLayer(hydroNetworkLayer), GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            var pumpLayer = GetPumpLayer(hydroNetworkLayer);

            AddGeometry(pumpLayer, GeometryFromWKT.Parse("Point (30 0)"));
            AddGeometry(pumpLayer, GeometryFromWKT.Parse("Point (30 0)"));
            AddGeometry(pumpLayer, GeometryFromWKT.Parse("Point (30 0)"));
            AddGeometry(pumpLayer, GeometryFromWKT.Parse("Point (70 0)"));
            AddGeometry(pumpLayer, GeometryFromWKT.Parse("Point (70 0)"));

            Assert.AreEqual(5, network.Pumps.Count());
            Assert.AreEqual(2, network.CompositeBranchStructures.Count());
            Assert.AreEqual(7, network.BranchFeatures.Count());
            Assert.AreEqual(3, network.CompositeBranchStructures.First().Structures.Count);
            Assert.AreEqual(2, network.CompositeBranchStructures.Skip(1).First().Structures.Count);
            
            var pump = (IPump) network.CompositeBranchStructures.First().Structures[0];
            IFeatureInteractor featureInteractor = mapControl.SelectTool.GetFeatureInteractor(pumpLayer, pump);

            featureInteractor.Start();
            featureInteractor.MoveTracker(featureInteractor.Trackers[0], 40, 0);
            featureInteractor.Stop(new SnapResult(featureInteractor.TargetFeature.Geometry.Coordinate, channel, pumpLayer, channel.Geometry, 0, 0));

            Assert.AreEqual(5, network.Pumps.Count());
            Assert.AreEqual(2, network.CompositeBranchStructures.Count());
            Assert.AreEqual(7, network.BranchFeatures.Count());
            Assert.AreEqual(2, network.CompositeBranchStructures.First().Structures.Count);
            Assert.AreEqual(3, network.CompositeBranchStructures.Skip(1).First().Structures.Count);
        }

        [Test]
        public void MovePumpWithTool()
        {
            var network = new HydroNetwork();
            hydroNetworkLayer = InitializeNetworkEditor(network);
            var channel = AddGeometry(GetChannelLayer(hydroNetworkLayer), GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            var pumpLayer = GetPumpLayer(hydroNetworkLayer);

            var pump = AddGeometry(pumpLayer, GeometryFromWKT.Parse("Point (50 0)")) as IPump;

            Assert.AreEqual(1, network.Pumps.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());

            Assert.AreEqual(50, network.Pumps.First().Chainage, 1.0e-6);
            
            var pumpEditor = new StructureInteractor<Pump>(new VectorLayer { Map = mapControl.Map }, pump,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, network);
            
            const double deltaX = 5;
            const double deltaY = 0;

            pumpEditor.Start();
            pumpEditor.MoveTracker(pumpEditor.Trackers[0], deltaX, deltaY);

            // result are not yet stored
            Assert.AreEqual(50, pump.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, pump.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(50, pump.Chainage, 1.0e-6);

            pumpEditor.Stop(new SnapResult(pumpEditor.TargetFeature.Geometry.Coordinate, channel, pumpLayer, channel.Geometry, 0, 0));

            Assert.AreEqual(55, pump.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, pump.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(55, pump.Chainage, 1.0e-6);

            Assert.AreEqual(1, network.Pumps.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());

            ICompositeBranchStructure CompositeBranchStructure = network.CompositeBranchStructures.First();
            Assert.AreEqual(55, CompositeBranchStructure.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, CompositeBranchStructure.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(55, CompositeBranchStructure.Chainage, 1.0e-6);

            Assert.AreEqual(1, CompositeBranchStructure.Structures.Count);
        }

        [Test]
        public void MovePumpWithToolToOtherBranch()
        {
            var network = new HydroNetwork();
            hydroNetworkLayer = InitializeNetworkEditor(network);
            var channel1 = AddGeometry(GetChannelLayer(hydroNetworkLayer), GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            var channel2 = AddGeometry(GetChannelLayer(hydroNetworkLayer), GeometryFromWKT.Parse("LINESTRING (100 0, 200 0)"));
            var pumpLayer = GetPumpLayer(hydroNetworkLayer);

            AddGeometry(pumpLayer, GeometryFromWKT.Parse("Point (50 0)"));

            Assert.AreEqual(1, network.Pumps.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());

            Assert.AreEqual(50, network.Pumps.First().Chainage, 1.0e-6);
            var pump = network.Pumps.First();

            var pumpEditor = new StructureInteractor<Pump>(hydroNetworkLayer, pump,
                                                       new VectorStyle { Symbol = new Bitmap(16, 16) }, network);
            const double deltaX = 110;
            const double deltaY = 0;

            pumpEditor.Start();
            pumpEditor.MoveTracker(pumpEditor.Trackers[0], deltaX, deltaY);

            // result are not yet stored
            Assert.AreEqual(50, pump.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, pump.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(50, pump.Chainage, 1.0e-6);

            pumpEditor.Stop(new SnapResult(pumpEditor.TargetFeature.Geometry.Coordinate, channel2, pumpLayer, channel2.Geometry, 0, 0));

            Assert.AreEqual(160, pump.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, pump.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(60, pump.Chainage, 1.0e-6);

            Assert.AreEqual(1, network.Pumps.Count());
            Assert.AreEqual(1, network.CompositeBranchStructures.Count());

            ICompositeBranchStructure CompositeBranchStructure = network.CompositeBranchStructures.First();
            Assert.AreEqual(160, CompositeBranchStructure.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, CompositeBranchStructure.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(60, CompositeBranchStructure.Chainage, 1.0e-6);

            Assert.AreEqual(1, CompositeBranchStructure.Structures.Count);
            Assert.AreEqual(network.Branches[1], pump.Branch);
            Assert.AreEqual( pump.Branch, CompositeBranchStructure.Branch);
        }


        [Test]
        public void MoveCompositeToOtherBranch()
        {
            HydroNetwork hydroNetwork = new HydroNetwork();
            hydroNetworkLayer = InitializeNetworkEditor(hydroNetwork);

            hydroNetwork.Branches.Add(new Channel { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(100, 0) }) });
            hydroNetwork.Branches.Add(new Channel { Geometry = new LineString(new[] { new Coordinate(100, 0), new Coordinate(200, 0) }) });
            CompositeBranchStructure compositeBranchStructure = new CompositeBranchStructure { Geometry = new Point(30, 0), Chainage = 30 };
            NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, hydroNetwork.Branches[0], compositeBranchStructure.Chainage);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, new Pump { Geometry = new Point(30, 0) });
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, new Pump { Geometry = new Point(30, 0) });
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, new Pump { Geometry = new Point(30, 0) });

            Assert.AreEqual(3, hydroNetwork.Pumps.Count());
            Assert.AreEqual(1, hydroNetwork.CompositeBranchStructures.Count());
            Assert.AreEqual(4, hydroNetwork.BranchFeatures.Count());
            Assert.AreEqual(3, hydroNetwork.CompositeBranchStructures.First().Structures.Count);

            IChannel channel1 = (IChannel) hydroNetwork.Branches[0];
            IChannel channel2 = (IChannel)hydroNetwork.Branches[1];

            ICompositeBranchStructure CompositeBranchStructure = hydroNetwork.CompositeBranchStructures.First();

            var compositeStructureEditor = new CompositeStructureInteractor(new VectorLayer { Map = mapControl.Map }, CompositeBranchStructure,
                                                                        new VectorStyle
                                                                            {
                                                                                Symbol = new Bitmap(16, 16)
                                                                            }, hydroNetwork){Network = hydroNetwork};
            const double deltaX = 100;
            const double deltaY = 0;

            compositeStructureEditor.Start();
            compositeStructureEditor.MoveTracker(compositeStructureEditor.Trackers[0], deltaX, deltaY);

            // result are not yet stored
            Assert.AreEqual(30, CompositeBranchStructure.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, CompositeBranchStructure.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(30, CompositeBranchStructure.Chainage, 1.0e-6);

            compositeStructureEditor.Stop(new SnapResult(compositeStructureEditor.TargetFeature.Geometry.Coordinate, channel2, compositeStructureEditor.Layer, channel2.Geometry, 0,0));

            Assert.AreEqual(130, CompositeBranchStructure.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, CompositeBranchStructure.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(30, CompositeBranchStructure.Chainage, 1.0e-6);

            Assert.AreEqual(3, hydroNetwork.Pumps.Count());
            Assert.AreEqual(1, hydroNetwork.CompositeBranchStructures.Count());
            Assert.AreEqual(4, hydroNetwork.BranchFeatures.Count());
            Assert.AreEqual(3, hydroNetwork.CompositeBranchStructures.First().Structures.Count);
            Assert.AreEqual(0, channel1.BranchFeatures.Count);
            Assert.AreEqual(4, channel2.BranchFeatures.Count);
        }
    }
}