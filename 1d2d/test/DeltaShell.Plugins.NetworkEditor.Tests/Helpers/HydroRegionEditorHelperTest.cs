using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMap.Api.Layers;
using SharpMap.Converters.WellKnownText;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Helpers
{
    /// <summary>
    /// TODO : All these test use wkt to create network. Would be nicer to use HNH to create the network and then create a layer for it
    /// </summary>
    [TestFixture]
    public class HydroRegionEditorHelperTest
    {
        private static MapControl MapControl;
        private static IHydroNetwork Network;
        private static HydroRegionMapLayer hydroNetworkMapLayer;
        private ILayer channelLayer;
        private ILayer nodeLayer;
        private ILayer lateralSourceLayer;
        private ILayer weirLayer;
        private ILayer crossSectionLayer;
        private ILayer compositeStructureLayer;
        private ILayer observationPointLayer;
        private ILayer pumpLayer;

        [SetUp]
        public void Initialize()
        {
            MapControl = new MapControl { Map = { Size = new Size(1000, 1000) } };
            MapControl.Resize += delegate { MapControl.Refresh(); };
            MapControl.ActivateTool(MapControl.SelectTool);

            Network = new HydroNetwork();
            hydroNetworkMapLayer = (HydroRegionMapLayer) MapLayerProviderHelper.CreateLayersRecursive(Network, null, new List<IMapLayerProvider> {new NetworkEditorMapLayerProvider()});
            nodeLayer = hydroNetworkMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(HydroNode));
            channelLayer = hydroNetworkMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(Channel));
            lateralSourceLayer = hydroNetworkMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(LateralSource));
            weirLayer = hydroNetworkMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(Weir));
            crossSectionLayer = hydroNetworkMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(CrossSection));
            compositeStructureLayer = hydroNetworkMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(CompositeBranchStructure));
            observationPointLayer = hydroNetworkMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(ObservationPoint));
            pumpLayer = hydroNetworkMapLayer.Layers.First(l => l.DataSource != null && l.DataSource.FeatureType == typeof(Pump));

            MapControl.Map.Layers.Add(hydroNetworkMapLayer);
            HydroRegionEditorHelper.AddHydroRegionEditorMapTool(MapControl);
        }

        [Test]
        public void MoveNodeTest()
        {
            // First add branch and implictly 2 nodes
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            Assert.AreEqual(1, channelLayer.DataSource.Features.Count);
            Assert.AreEqual(2, nodeLayer.DataSource.Features.Count);
            var node = (INode)nodeLayer.DataSource.Features[1];
            Assert.AreEqual(100, node.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, node.Geometry.Coordinates[0].Y, 1.0e-6);
            HydroRegionEditorHelper.MoveNodeTo(node, 200, -200);
            Assert.AreEqual(200, node.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(-200, node.Geometry.Coordinates[0].Y, 1.0e-6);
        }

        [Test]
        public void MoveStartNode2SameLocationTest()
        {
            // First add branch and implictly 2 nodes
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            var node = (INode)nodeLayer.DataSource.Features[0];
            Assert.Throws<ArgumentException>(() => HydroRegionEditorHelper.MoveNodeTo(node, 100, 0));

        }

        [Test]
        public void MoveEndNode2SameLocationTest()
        {
            // First add branch and implictly 2 nodes
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            var node = (INode)nodeLayer.DataSource.Features[1];
            Assert.Throws<ArgumentException>(() =>
            {
                HydroRegionEditorHelper.MoveNodeTo(node, 0, 0);
            });
        }

        [Test]
        public void MoveLateralTest()
        {
            // First add branch and implictly 2 nodes
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            Assert.AreEqual(1, channelLayer.DataSource.Features.Count);
            Assert.AreEqual(2, nodeLayer.DataSource.Features.Count);

            lateralSourceLayer.DataSource.Add(GeometryFromWKT.Parse("Point (20 0)"));
            var lateralSource = (ILateralSource)lateralSourceLayer.DataSource.Features[0];
            Assert.AreEqual(20, lateralSource.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, lateralSource.Geometry.Coordinates[0].Y, 1.0e-6);
            HydroRegionEditorHelper.MoveBranchFeatureTo(lateralSource, 40);
            Assert.AreEqual(40, lateralSource.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, lateralSource.Geometry.Coordinates[0].Y, 1.0e-6);
        }

        [Test]
        public void MoveDiffuseLateralTest()
        {
            // First add branch and implictly 2 nodes
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            Assert.AreEqual(1, channelLayer.DataSource.Features.Count);
            Assert.AreEqual(2, nodeLayer.DataSource.Features.Count);

            lateralSourceLayer.DataSource.Add(GeometryFromWKT.Parse("Point (20 0)"));
            var lateralSource = (ILateralSource)lateralSourceLayer.DataSource.Features[0];
            HydroRegionEditorHelper.UpdateBranchFeatureGeometry(lateralSource, 5);

            Assert.AreEqual(20, lateralSource.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, lateralSource.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(25, lateralSource.Geometry.Coordinates[1].X, 1.0e-6);
            Assert.AreEqual(0, lateralSource.Geometry.Coordinates[1].Y, 1.0e-6);
            Assert.AreEqual(20, lateralSource.Chainage);

            HydroRegionEditorHelper.MoveBranchFeatureTo(lateralSource, 40);

            Assert.AreEqual(40, lateralSource.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, lateralSource.Geometry.Coordinates[0].Y, 1.0e-6);
            Assert.AreEqual(45, lateralSource.Geometry.Coordinates[1].X, 1.0e-6);
            Assert.AreEqual(0, lateralSource.Geometry.Coordinates[1].Y, 1.0e-6);
            Assert.AreEqual(40, lateralSource.Chainage);
        }

        [Test, Category(TestCategory.Integration)]
        public void CheckLateralSourceGeometryAfterChangingLength()
        {
            var network = new HydroNetwork();
            var branch1 = new Channel(new HydroNode("n1"), new HydroNode("n1"))
                              {
                                  Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")
                              };

            var lateralSource = new LateralSource { Name = "Source1", Chainage = 10 };
            
            network.Branches.Add(branch1);
            branch1.BranchFeatures.Add(lateralSource);
            lateralSource.Branch = branch1;

            HydroRegionEditorHelper.UpdateBranchFeatureGeometry(lateralSource, 40);
            
            var geometry = lateralSource.Geometry;

            Assert.IsTrue(geometry is ILineString);
            Assert.AreEqual(40.0, lateralSource.Length);

            HydroRegionEditorHelper.UpdateBranchFeatureGeometry(lateralSource, 0);
            Assert.IsTrue(lateralSource.Geometry is IPoint);
            Assert.AreEqual(0, lateralSource.Length);
        }

        [Test]
        public void AddChannelUsingGeometryAddsOneBranchAndTwoNodes()
        {
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            
            Assert.AreEqual(1, channelLayer.DataSource.Features.Count);
            Assert.AreEqual(2, nodeLayer.DataSource.Features.Count);
        }

        [Test]
        public void AddLateralSourceUsingGeometryWhichOverlapsWithBranchGeometry()
        {
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));

            lateralSourceLayer.DataSource.Add(GeometryFromWKT.Parse("Point (20 0)"));
            var lateralSource = (ILateralSource)lateralSourceLayer.DataSource.Features[0];
            
            Assert.AreEqual(20, lateralSource.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, lateralSource.Geometry.Coordinates[0].Y, 1.0e-6);
        }

        [Test]
        [TestCase(-100, 0)]
        [TestCase(200, 100)]
        public void MoveLateralOutOfBranchLengthSnapsToBranchLength(double newChainage, double expectedX)
        {
            // preparation
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            lateralSourceLayer.DataSource.Add(GeometryFromWKT.Parse("Point (20 0)"));
            var lateralSource = (ILateralSource)lateralSourceLayer.DataSource.Features[0];

            // action
            HydroRegionEditorHelper.MoveBranchFeatureTo(lateralSource, newChainage);

            // asserts
            Assert.AreEqual(expectedX, lateralSource.Geometry.Coordinates[0].X, 1.0e-6, "snaps to branch length");
            Assert.AreEqual(0, lateralSource.Geometry.Coordinates[0].Y, 1.0e-6);
        }

        [Test]
        public void MoveLateralOutOfRangeNetworkRemainsUncorrupted()
        {
            int numExceptions = 0;
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            lateralSourceLayer.DataSource.Add(GeometryFromWKT.Parse("Point (20 0)"));
            var lateralSource = (ILateralSource)lateralSourceLayer.DataSource.Features[0];

            HydroRegionEditorHelper.MoveBranchFeatureTo(lateralSource, -100);
            Assert.AreEqual(0, lateralSource.Chainage);

            HydroRegionEditorHelper.MoveBranchFeatureTo(lateralSource, 200);
            Assert.AreEqual(lateralSource.Branch.Length, lateralSource.Chainage);

            HydroRegionEditorHelper.MoveBranchFeatureTo(lateralSource, 90);
            Assert.AreEqual(90, lateralSource.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(0, lateralSource.Geometry.Coordinates[0].Y, 1.0e-6);
        }

        [Test]
        public void MoveCrossSectionTest()
        {
            // First add branch and implictly 2 nodes
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            crossSectionLayer.FeatureEditor.CreateNewFeature = (l => CrossSection.CreateDefault());
            crossSectionLayer.DataSource.Add(GeometryFromWKT.Parse("Point (20 0)"));
            var crossSection = (ICrossSection)crossSectionLayer.DataSource.Features[0];

            HydroRegionEditorHelper.MoveBranchFeatureTo(crossSection, 40);

            Assert.AreEqual(40, crossSection.Geometry.Coordinates[0].X, 1.0e-6);
        }

        [Test]
        public void MoveCompoundStructureTest()
        {
            // First add branch and implictly 2 nodes
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            weirLayer.DataSource.Add(GeometryFromWKT.Parse("Point (30 0)"));
            weirLayer.DataSource.Add(GeometryFromWKT.Parse("Point (30 0)"));

            var compositeBranchStructure = (ICompositeBranchStructure)compositeStructureLayer.DataSource.Features[0];
            var weir0 = (IWeir)weirLayer.DataSource.Features[0];
            var weir1 = (IWeir)weirLayer.DataSource.Features[1];

            Assert.AreEqual(2, compositeBranchStructure.Structures.Count);

            Assert.AreEqual(30, compositeBranchStructure.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(30, weir0.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(30, weir1.Geometry.Coordinates[0].X, 1.0e-6);

            HydroRegionEditorHelper.MoveBranchFeatureTo(compositeBranchStructure, 70);

            Assert.AreEqual(70, compositeBranchStructure.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(70, weir0.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(70, weir1.Geometry.Coordinates[0].X, 1.0e-6);
        }

        [Test]
        public void MoveWeirFromCompoundStructureTest()
        {
            // First add branch and implictly 2 nodes
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            weirLayer.DataSource.Add(GeometryFromWKT.Parse("Point (30 0)"));
            weirLayer.DataSource.Add(GeometryFromWKT.Parse("Point (30 0)"));

            var compositeBranchStructure = (ICompositeBranchStructure)compositeStructureLayer.DataSource.Features[0];
            var weir0 = (IWeir)weirLayer.DataSource.Features[0];
            var weir1 = (IWeir)weirLayer.DataSource.Features[1];

            Assert.AreEqual(2, compositeBranchStructure.Structures.Count);

            Assert.AreEqual(30, compositeBranchStructure.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(30, weir0.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(30, weir1.Geometry.Coordinates[0].X, 1.0e-6);

            HydroRegionEditorHelper.MoveBranchFeatureTo(weir0, 70);
            Assert.AreEqual(30, compositeBranchStructure.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(30, weir1.Geometry.Coordinates[0].X, 1.0e-6);

            // fiirst is added to new compound
            Assert.AreEqual(2, compositeStructureLayer.DataSource.Features.Count);
            var compositeBranchStructure1 = (ICompositeBranchStructure)compositeStructureLayer.DataSource.Features[1];
            Assert.AreEqual(70, compositeBranchStructure1.Geometry.Coordinates[0].X, 1.0e-6);
            Assert.AreEqual(70, weir0.Geometry.Coordinates[0].X, 1.0e-6);
        }

        [Test]
        public void MapChainage()
        {
            // First add branch and implictly 2 nodes
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            lateralSourceLayer.DataSource.Add(GeometryFromWKT.Parse("Point (20 0)"));
            var branch = (IBranch)channelLayer.DataSource.Features[0];
            var lateralSource = (ILateralSource)lateralSourceLayer.DataSource.Features[0];

            branch.IsLengthCustom = true;
            branch.Length = 1000.0;
            Assert.AreEqual(200.0, lateralSource.Chainage);
            Assert.AreEqual(20.0, NetworkHelper.MapChainage(lateralSource));
        }

        [Test]
        public void MovePumpToChainageZeroTest()
        {
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (100 0, 100 100)"));
            
            pumpLayer.DataSource.Add(GeometryFromWKT.Parse("Point (100 50)"));
            var pump = (Pump)pumpLayer.DataSource.Features[0];

            var branch2 = ((IHydroNetwork)hydroNetworkMapLayer.Region).Branches.ElementAt(1);
            Assert.AreEqual(branch2,pump.Branch);
            HydroRegionEditorHelper.MoveBranchFeatureTo(pump, 0);
            //branch is same
            Assert.AreEqual(branch2, pump.Branch);
            Assert.AreEqual(0,0, pump.Chainage);
        }

        [Test]
        public void MoveObservationPointToChainageZeroTest()
        {
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"));
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (100 0, 100 100)"));

            observationPointLayer.DataSource.Add(GeometryFromWKT.Parse("Point (100 50)"));
            var obs = (ObservationPoint)observationPointLayer.DataSource.Features[0];

            var branch2 = ((IHydroNetwork)hydroNetworkMapLayer.Region).Branches.ElementAt(1);
            Assert.AreEqual(branch2, obs.Branch);
            HydroRegionEditorHelper.MoveBranchFeatureTo(obs, 0);
            //branch is same
            Assert.AreEqual(branch2, obs.Branch);
            Assert.AreEqual(0, 0, obs.Chainage);
        }

        [Test]
        public void ChangingChainageObservationPointToIntegerValueShouldRemainIntegerValue()
        {
            // Following values came directly from some clicking in GUI, for which the problem reported in TOOLS-4465 appeared
            // Problem does not occur with network branches that are specified by integer values for X and Y.
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 987.36543219870009 183.27964499999996)"));
            observationPointLayer.DataSource.Add(GeometryFromWKT.Parse("Point (294.961350997529 54.7521818534439)"));
            var obs = (ObservationPoint)observationPointLayer.DataSource.Features[0];

            HydroRegionEditorHelper.MoveBranchFeatureTo(obs, 200);
            Assert.AreEqual(200,obs.Chainage,1E-15); //needs very small acceptance resolution as Properties pane can easily show 12 decimals
        }

        [Test]
        public void ChangingChainageWeirToIntegerValueShouldRemainIntegerValue()
        {
            // Following values came directly from some clicking in GUI, for which the problem reported in TOOLS-4465 appeared
            // Problem does not occur with network branches that are specified by integer values for X and Y.
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 987.36543219870009 183.27964499999996)"));
            weirLayer.DataSource.Add(GeometryFromWKT.Parse("Point (294.961350997529 54.7521818534439)"));
            var weir = (IWeir)weirLayer.DataSource.Features[0];

            HydroRegionEditorHelper.MoveBranchFeatureTo(weir, 200);
            Assert.AreEqual(200, weir.Chainage, 1E-15); //needs very small acceptance resolution as Properties pane can easily show 12 decimals
            Assert.AreEqual(200, weir.ParentStructure.Chainage, 1E-15);//needs very small acceptance resolution as Properties pane can easily show 12 decimals
        }

        [Test]
        public void ChangingChainageCrossSectionToIntegerValueShouldRemainIntegerValue()
        {
            // Following values came directly from some clicking in GUI, for which the problem reported in TOOLS-4465 appeared
            // Problem described in TOOLS-4465 did not occur for CrossSection features, only for non-CrossSection features.
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 987.36543219870009 183.27964499999996)"));
            crossSectionLayer.DataSource.Add(GeometryFromWKT.Parse("Point (294.961350997529 54.7521818534439)"));

            crossSectionLayer.FeatureEditor.CreateNewFeature = (l => CrossSection.CreateDefault());

            var crossSection = (ICrossSection)crossSectionLayer.DataSource.Features[0];

            HydroRegionEditorHelper.MoveBranchFeatureTo(crossSection, 200);
            Assert.AreEqual(200, crossSection.Chainage, 1E-15); //needs very small acceptance resolution as Properties pane can easily show 12 decimals
        }

        [Test]
        public void ChangingChainageCompoundStructureToIntegerValueShouldRemainIntegerValue()
        {
            // Following values came directly from some clicking in GUI, for which the problem reported in TOOLS-4465 appeared
            // Problem does not occur with network branches that are specified by integer values for X and Y.
            channelLayer.DataSource.Add(GeometryFromWKT.Parse("LINESTRING (0 0, 987.36543219870009 183.27964499999996)"));
            weirLayer.DataSource.Add(GeometryFromWKT.Parse("Point (294.961350997529 54.7521818534439)"));
            weirLayer.DataSource.Add(GeometryFromWKT.Parse("Point (294.961350997529 54.7521818534439)"));

            var compositeBranchStructure = (ICompositeBranchStructure)compositeStructureLayer.DataSource.Features[0];
            var weir0 = (IWeir)weirLayer.DataSource.Features[0];
            var weir1 = (IWeir)weirLayer.DataSource.Features[1];

            HydroRegionEditorHelper.MoveBranchFeatureTo(compositeBranchStructure, 200);

            Assert.AreEqual(200, compositeBranchStructure.Chainage, 1E-15); //needs very small acceptance resolution as Properties pane can easily show 12 decimals
            Assert.AreEqual(200, weir0.Chainage, 1E-15); //needs very small acceptance resolution as Properties pane can easily show 12 decimals
            Assert.AreEqual(200, weir1.Chainage, 1E-15); //needs very small acceptance resolution as Properties pane can easily show 12 decimals
        }
    }
}