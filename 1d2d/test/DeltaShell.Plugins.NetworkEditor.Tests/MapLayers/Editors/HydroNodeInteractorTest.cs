using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.MapLayers.Editors.Interactors;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.MapLayers.Editors
{
    [TestFixture]
    public class HydroNodeInteractorTest
    {
        private static IHydroNetwork network;
        private static IChannel branch1;
        private static IHydroNode node1;
        private static IHydroNode node2;

        [SetUp]
        public void NetworkSetup()
        {
            network = new HydroNetwork();

            branch1 = new Channel { Geometry = new LineString(new [] { new Coordinate(0, 0), new Coordinate(20, 0) }) };
            node1 = new HydroNode { Geometry = new Point(0, 0)};
            node2 = new HydroNode { Geometry = new Point(20, 0) };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            branch1.Source = node1;
            branch1.Target = node2;

            network.Branches.Add(branch1);
        }

        [Test]
        public void PropertyTest()
        {
            var interactor = new HydroNodeInteractor(null, node1, null, null);
            // Do not change these settings!!!!!!!! or consult with 1dflow team
            Assert.AreEqual(false, interactor.AllowDeletion());
            Assert.AreEqual(true, interactor.AllowMove());
            Assert.AreEqual(true, interactor.AllowSingleClickAndMove());
        }

        [Test]
        public void Move1NodeTest()
        {
            var interactor = new HydroNodeInteractor(null, node1, null, null);
            const double deltaX = 0;
            const double deltaY = 5;
            //nodeEditor.Move(node1, deltaX, deltaY);
            // start
            int count = 0;
            interactor.WorkerFeatureCreated += delegate { count++; };
            interactor.Start();
            Assert.AreEqual(1, count);
            interactor.MoveTracker(interactor.Trackers[0], deltaX, deltaY);

            // result are not yet stored
            Assert.AreEqual(0, node1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].Y);

            interactor.Stop();
            // end
            //nodeEditor.Move(deltaX, deltaY);
            Assert.AreEqual(5, node1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].X);
            //
            Assert.AreEqual(5, branch1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(20, branch1.Geometry.Coordinates[branch1.Geometry.Coordinates.Length - 1].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[branch1.Geometry.Coordinates.Length - 1].Y);
        }

        [Test]
        public void Move2NodeTest()
        {
            var interactor = new HydroNodeInteractor(null, node1, null, null);
            const double deltaX = 0;
            const double deltaY = 5;
            //nodeEditor.Move(node1, deltaX, deltaY);
            // start
            interactor.Start();
            interactor.MoveTracker(interactor.Trackers[0], deltaX, deltaY);
            interactor.MoveTracker(interactor.Trackers[0], deltaX, deltaY);
            interactor.Stop();
            // end
            //nodeEditor.Move(deltaX, deltaY);
            Assert.AreEqual(10, node1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].X);
            //
            Assert.AreEqual(10, branch1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(20, branch1.Geometry.Coordinates[branch1.Geometry.Coordinates.Length - 1].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[branch1.Geometry.Coordinates.Length - 1].Y);
        }

        [Test]
        public void MoveTargetNodeOntoSourceNodeOfSameBranchShouldNotResultInAMerge()
        {
            var interactor = new HydroNodeInteractor(null, node2, null, null);
            const double deltaX = -20;
            const double deltaY = 0;

            // start
            interactor.Start();
            interactor.MoveTracker(interactor.Trackers[0], deltaX, deltaY);
            interactor.Stop();
            // end

            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(2, network.Nodes.Count);

            Assert.AreEqual(0, network.Nodes[0].Geometry.Coordinate.X);
            Assert.AreEqual(0, network.Nodes[0].Geometry.Coordinate.Y);
            Assert.AreEqual(0, network.Nodes[1].Geometry.Coordinate.X);
            Assert.AreEqual(0, network.Nodes[1].Geometry.Coordinate.Y);
        }

        [Test]
        public void MoveSourceNodeOntoTargetNodeOfSameBranchShouldNotResultInAMerge()
        {
            var interactor = new HydroNodeInteractor(null, node1, null, null);
            const double deltaX = 20;
            const double deltaY = 0;

            // start
            interactor.Start();
            interactor.MoveTracker(interactor.Trackers[0], deltaX, deltaY);
            interactor.Stop();
            // end

            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(2, network.Nodes.Count);

            Assert.AreEqual(20, network.Nodes[0].Geometry.Coordinate.X);
            Assert.AreEqual(0, network.Nodes[0].Geometry.Coordinate.Y);
            Assert.AreEqual(20, network.Nodes[1].Geometry.Coordinate.X);
            Assert.AreEqual(0, network.Nodes[1].Geometry.Coordinate.Y);
        }

        [Test]
        public void MoveNodeOntoNodeOfOtherBranchShouldResultInAMerge()
        {
            var branch2 = new Channel { Geometry = new LineString(new[] { new Coordinate(40, 0), new Coordinate(60, 0) }) };
            var node3 = new HydroNode { Geometry = new Point(40, 0) };
            var node4 = new HydroNode { Geometry = new Point(60, 0) };

            network.Nodes.Add(node3);
            network.Nodes.Add(node4);

            branch2.Source = node3;
            branch2.Target = node4;

            network.Branches.Add(branch2);

            var interactor = new HydroNodeInteractor(null, node2, null, null);
            const double deltaX = 20;
            const double deltaY = 0;

            Assert.AreEqual(2, network.Branches.Count);
            Assert.AreEqual(4, network.Nodes.Count);

            // start
            interactor.Start();
            interactor.MoveTracker(interactor.Trackers[0], deltaX, deltaY);
            interactor.Stop();
            // end

            Assert.AreEqual(2, network.Branches.Count);
            Assert.AreEqual(3, network.Nodes.Count);

            Assert.AreEqual(0, network.Nodes[0].Geometry.Coordinate.X);
            Assert.AreEqual(0, network.Nodes[0].Geometry.Coordinate.Y);
            Assert.AreEqual(40, network.Nodes[1].Geometry.Coordinate.X);
            Assert.AreEqual(0, network.Nodes[1].Geometry.Coordinate.Y);
            Assert.AreEqual(60, network.Nodes[2].Geometry.Coordinate.X);
            Assert.AreEqual(0, network.Nodes[2].Geometry.Coordinate.Y);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void MoveNodeWithCrossSectionTest()
        {
            var crossSectionDef = new CrossSectionDefinitionYZ();

            var crossSection = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, crossSectionDef, 10);

            crossSectionDef.YZDataTable.AddCrossSectionYZRow(0, 0);
            crossSectionDef.YZDataTable.AddCrossSectionYZRow(5, -5);
            crossSectionDef.YZDataTable.AddCrossSectionYZRow(10, 0);
            
            var interactor = new HydroNodeInteractor(null, node2, null, null);
            const double deltaX = 20;
            const double deltaY = 0;
            
            // start
            interactor.Start();
            interactor.MoveTracker(interactor.Trackers[0], deltaX, deltaY);
            interactor.Stop();
            // end

            Assert.AreEqual(40, node2.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(40, branch1.Geometry.Coordinates[branch1.Geometry.Coordinates.Length - 1].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[branch1.Geometry.Coordinates.Length - 1].Y);

            Assert.AreEqual(20, crossSection.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, crossSection.Geometry.Coordinates[0].Y);

        }
        [Test]
        [Category(TestCategory.Integration)]
        public void MoveNodeWithStuctureFeatureTest()
        {
            var compositeStructure = new CompositeBranchStructure
                                                    {
                                                        Geometry = new Point(new Coordinate(10, 0))
                                                    };

            branch1.BranchFeatures.Add(compositeStructure);
            // NB structureFeature.Branch will be automatically set
            compositeStructure.Chainage = 10;

            var interactor = new HydroNodeInteractor(null, node2, null, null);
            const double deltaX = 20;
            const double deltaY = 0;
            //nodeEditor.Move(node1, deltaX, deltaY);
            // start
            interactor.Start();
            interactor.MoveTracker(interactor.Trackers[0], deltaX, deltaY);
            interactor.Stop();
            // end
            //nodeEditor.Move(deltaX, deltaY);
            Assert.AreEqual(40, node2.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[0].Y);
            Assert.AreEqual(40, branch1.Geometry.Coordinates[branch1.Geometry.Coordinates.Length - 1].X);
            Assert.AreEqual(0, branch1.Geometry.Coordinates[branch1.Geometry.Coordinates.Length - 1].Y);

            Assert.AreEqual(20, compositeStructure.Geometry.Coordinates[0].X);
            Assert.AreEqual(0, compositeStructure.Geometry.Coordinates[0].Y);
        }
    }
}
