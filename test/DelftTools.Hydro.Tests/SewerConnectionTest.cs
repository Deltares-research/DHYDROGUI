using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class SewerConnectionTest
    {
        [Test]
        public void CreateSewerConnection()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection);
        }

        [Test]
        public void CreateSewerConnectionWithGivenManholesGetsValidGeometry()
        {
            var sourcePoint = new Point(1, 3);
            var targetPoint = new Point(10, 7);

            var compartmentOne = new Compartment("compartmentOne");
            var sourceManhole = new Manhole("sourceManhole")
            {
                Compartments = new EventedList<Compartment> {compartmentOne},
                Geometry = sourcePoint
            };

            var compartmentTwo = new Compartment("compartmentTwo");
            var targetManhole = new Manhole("targetManhole")
            {
                Compartments = new EventedList<Compartment> { compartmentTwo },
                Geometry = targetPoint
            };

            var sewerConnection = new SewerConnection(sourceManhole, targetManhole);
            Assert.IsNotNull(sewerConnection);

            Assert.IsNotNull(sewerConnection.Geometry);
            Assert.IsTrue(sewerConnection.Geometry.IsValid);
            Assert.IsTrue(sewerConnection.Geometry.Coordinates.Any());
            Assert.IsTrue(sewerConnection.Geometry.Coordinates.Contains(sourcePoint.Coordinate));
            Assert.IsTrue(sewerConnection.Geometry.Coordinates.Contains(targetPoint.Coordinate));
        }

        [Test]
        public void CreateSewerConnectionWithEmptyManholesGetsGeometry()
        {
            //This test relies on maholes getting a default geometry when being created.
            var sewerConnection = new SewerConnection(new Manhole("sourceManhole"), new Manhole("targetManhole"));
            Assert.IsNotNull(sewerConnection);

            Assert.IsNotNull(sewerConnection.Geometry);
            Assert.IsTrue(sewerConnection.Geometry.Coordinates.Any());
        }

        [Test]
        public void SewerConnectionGeometryGetsRefreshedWhenManholesGeometryChanges()
        {
            //This test relies on maholes getting a default geometry when being created.
            var sourcePoint = new Point(1, 3);
            var targetPoint = new Point(10, 7);

            var sourceManhole = new Manhole("sourceManhole"){Geometry = sourcePoint};
            var targetManhole = new Manhole("targetManhole"){Geometry = targetPoint};

            var sewerConnection = new SewerConnection(sourceManhole, targetManhole);
            Assert.IsNotNull(sewerConnection);

            var connectionGeom = sewerConnection.Geometry;
            var firstLength = connectionGeom.Length;
            Assert.IsNotNull(connectionGeom);
            Assert.IsTrue(connectionGeom.IsValid);
            Assert.IsTrue(connectionGeom.Coordinates.Any());
            Assert.IsTrue(connectionGeom.Coordinates.Contains(sourcePoint.Coordinate));
            Assert.IsTrue(connectionGeom.Coordinates.Contains(targetPoint.Coordinate));

            //Change geometry now.
            var newSourceCoordinate = new Coordinate(30, 30);
            GeometryHelper.MoveCoordinate(sourceManhole.Geometry, 0, newSourceCoordinate.X - sourcePoint.X, newSourceCoordinate.Y - sourcePoint.Y);

            Assert.IsNotNull(connectionGeom);
            Assert.IsTrue(connectionGeom.IsValid);
            Assert.IsTrue(connectionGeom.Coordinates.Any());

            Assert.IsTrue(connectionGeom.Coordinates.Contains(targetPoint.Coordinate));
            Assert.IsTrue(connectionGeom.Coordinates.Contains(newSourceCoordinate));

            Assert.AreNotEqual(firstLength, connectionGeom.Length);
        }

        [Test]
        public void CreateSewerConnectionGivingName()
        {
            var nameSewer = "TestSewer";
            var sewerConnection = new SewerConnection(nameSewer);
            Assert.IsNotNull(sewerConnection);
            Assert.AreEqual(nameSewer, sewerConnection.Name);
        }

        [Test]
        public void CreateSewerConnectionGivingSourceManholeTargetManholeAndLength()
        {
            var sourceManhole = new Manhole("manholSource");
            var targetManhole = new Manhole("manholTarget");

            var length = 0.0;
            var sewerConnection = new SewerConnection(sourceManhole, targetManhole, length);
            Assert.IsNotNull(sewerConnection);

            Assert.AreEqual(sourceManhole, sewerConnection.Source);
            Assert.AreEqual(targetManhole, sewerConnection.Target);
            Assert.AreEqual(length, sewerConnection.Length);
        }

        [Test]
        public void CreateSewerConnectionGivingNameSourceManholeTargetManholeAndLength()
        {
            var nameSewer = "TestSewer";

            var sourceManhole = new Manhole("manholSource");
            var targetManhole = new Manhole("manholTarget");

            var length = 0.0;
            var sewerConnection = new SewerConnection(nameSewer, sourceManhole, targetManhole, length);

            Assert.IsNotNull(sewerConnection);
            Assert.AreEqual(nameSewer, sewerConnection.Name);
            Assert.AreEqual(sourceManhole, sewerConnection.Source);
            Assert.AreEqual(targetManhole, sewerConnection.Target);
            Assert.AreEqual(length, sewerConnection.Length);
        }

        [Test]
        public void CreateSimpleSewerConnectionCreatesEmptyEnumerableBranchFeaturesList()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection.BranchFeatures);
            Assert.IsFalse(sewerConnection.BranchFeatures.Any());
        }

        [Test]
        public void SimpleSewerConnectionIsNotPipe()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection.BranchFeatures);
            Assert.IsFalse(sewerConnection.IsPipe());
        }

        [Test]
        public void ChangingSourceCompartmentChangesSourceTest()
        {
            var manholeTest = new Manhole("manholeTest");
            var compartmentTest = new Compartment("compartmentTest");
            manholeTest.Compartments.Add(compartmentTest);

            var sewerConnection = new SewerConnection();
            
            Assert.IsNull(sewerConnection.Source);
            Assert.IsNull(sewerConnection.SourceCompartment);

            sewerConnection.SourceCompartment = compartmentTest;

            Assert.AreEqual(compartmentTest, sewerConnection.SourceCompartment);
            Assert.AreEqual(manholeTest, sewerConnection.Source);
        }

        [Test]
        public void ChangingTargetCompartmentChangesTargetTest()
        {
            var manholeTest = new Manhole("manholeTest");
            var compartmentTest = new Compartment("compartmentTest");
            manholeTest.Compartments.Add(compartmentTest);

            var sewerConnection = new SewerConnection();

            Assert.IsNull(sewerConnection.Target);
            Assert.IsNull(sewerConnection.TargetCompartment);

            sewerConnection.TargetCompartment = compartmentTest;

            Assert.AreEqual(compartmentTest, sewerConnection.TargetCompartment);
            Assert.AreEqual(manholeTest, sewerConnection.Target);
        }

        [Test]
        public void SewerConnectionWaterTypeDefaultValueIsNotNull()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection.WaterType);
            //This default value might change, but at least we have here a trigger.
            Assert.AreEqual(SewerConnectionWaterType.None, sewerConnection.WaterType);
        }

        [Test]
        public void SewerConnectionBranchFeaturesShouldBePresentInNetworkBranchFeatures()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            NetworkHelper.AddChannelToHydroNetwork(network, sewerConnection);

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);
            #endregion

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);

            //Add one feature.
            var compositeStructure = sewerConnection.AddStructureToBranch(featureOne);

            Assert.IsTrue(network.BranchFeatures.Contains(compositeStructure));
            Assert.IsTrue(network.BranchFeatures.Contains(featureOne));
        }

        [Test]
        public void SewerConnectionBranchFeaturesDoesNotAcceptMoreThanOneFeature()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);

            var featureThree = new Pump();
            Assert.IsNotNull(featureThree);
            #endregion

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);

            //Add one feature.
            sewerConnection.BranchFeatures.Add(featureOne);
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureOne, sewerConnection.BranchFeatures.First());

            //Try to add a second one, but the feature should still be the first one.
            var expectedLogMessage = string.Format("Sewer connection {0} does not accept more than one branch feature", sewerConnection.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.BranchFeatures.Add(featureTwo), expectedLogMessage);
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureOne, sewerConnection.BranchFeatures.First());
        }

        [Test]
        public void SewerConnectionBranchFeaturesAcceptsReplacementOfFeature()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);

            var featureThree = new Pump();
            Assert.IsNotNull(featureThree);
            #endregion

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);

            //Add one feature.
            sewerConnection.BranchFeatures.Add(featureOne);
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureOne, sewerConnection.BranchFeatures.First());

            //Try to replace the existent feature, it should be possible
            sewerConnection.BranchFeatures[0] = featureTwo;
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureTwo, sewerConnection.BranchFeatures.First());
        }

        [Test]
        public void SewerConnectionBranchFeaturesAcceptsNewFeautureIfPreviousIsRemoved()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);

            var featureThree = new Pump();
            Assert.IsNotNull(featureThree);
            #endregion

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);

            //Add one feature.
            sewerConnection.BranchFeatures.Add(featureOne);
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureOne, sewerConnection.BranchFeatures.First());

            //Try removing said feature and adding a new one.
            sewerConnection.BranchFeatures.Clear();
            Assert.IsFalse(sewerConnection.BranchFeatures.Any());

            sewerConnection.BranchFeatures.Add(featureThree);
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureThree, sewerConnection.BranchFeatures.First());
        }

        [Test]
        public void SewerConnectionBranchFeaturesAcceptsCompositeStructureWithOneStructureAsFeatureBranch()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            #endregion

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);

            //Add one composite feature.
            var compositeStructure = sewerConnection.AddStructureToBranch(featureOne);
            Assert.AreEqual(2 /*CompositeStructure, and Structure on it*/, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(compositeStructure, sewerConnection.BranchFeatures.First());
            Assert.AreEqual(featureOne, sewerConnection.BranchFeatures.Last());

            var foundComposite = sewerConnection.BranchFeatures.First() as CompositeBranchStructure;
            Assert.IsNotNull(foundComposite);
            Assert.IsTrue(foundComposite.Structures.Any());
            Assert.IsTrue(foundComposite.Structures.Contains(featureOne));

            Assert.AreEqual(featureOne, sewerConnection.GetStructuresFromBranchFeatures<Pump>().First());
        }

        [Test]
        public void SewerConnectionBranchFeaturesDoesNotAcceptToAddMoreThanFeatureToFeatureBranches()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);
            #endregion

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);
            var expectedLogMessage = string.Format("Sewer connection {0} does not accept more than one branch feature", sewerConnection.Name);

            //Add one composite feature.
            var compositeStructure = sewerConnection.AddStructureToBranch(featureOne);

            //Try to add an extra feature to the branch feature itself.
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.BranchFeatures.Add(featureTwo), expectedLogMessage);
            Assert.AreEqual(2/*CompositeStructure, and Structure on it*/, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(compositeStructure, sewerConnection.BranchFeatures.First());
            Assert.AreEqual(featureOne, sewerConnection.BranchFeatures.Last());

            var foundComposite = sewerConnection.BranchFeatures.First() as CompositeBranchStructure;
            Assert.IsNotNull(foundComposite);
            Assert.IsTrue(foundComposite.Structures.Count.Equals(1));
            Assert.IsTrue(foundComposite.Structures.Contains(featureOne));

            Assert.AreEqual(featureOne, sewerConnection.GetStructuresFromBranchFeatures<Pump>().First());
        }

        [Test]
        public void SewerConnectionBranchFeaturesDoesNotAcceptAFeatureBranchCompositeStructureWithMoreThanOneStructure()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureThree = new Pump();
            Assert.IsNotNull(featureThree);
            #endregion

            Assert.AreEqual(0, sewerConnection.BranchFeatures.Count);
            var expectedLogMessage = string.Format("Sewer connection {0} does not accept more than one branch feature", sewerConnection.Name);

            //Add one composite feature.
            var compositeStructure = sewerConnection.AddStructureToBranch(featureOne);

            //Try to add a feature to the composite instead, it should still fail.
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.AddStructureToBranch(featureThree), expectedLogMessage);
            Assert.AreEqual(2/*CompositeStructure, and Structure on it*/, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(compositeStructure, sewerConnection.BranchFeatures.First());
            Assert.AreEqual(featureOne, sewerConnection.BranchFeatures.Last());

            var foundComposite = sewerConnection.BranchFeatures.First() as CompositeBranchStructure;
            Assert.IsNotNull(foundComposite);
            Assert.IsTrue(foundComposite.Structures.Count.Equals(1));
            Assert.IsTrue(foundComposite.Structures.Contains(featureOne));

            Assert.AreEqual(featureOne, sewerConnection.GetStructuresFromBranchFeatures<Pump>().First());
        }

        [Test]
        public void ReplacingSewerConnectionFeatureBranchesWithOneStructureReturnsLogMessage()
        {
            var sewerConnection = new SewerConnection();
            Assert.IsNotNull(sewerConnection);

            #region Features
            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);

            var featureThree = new Pump();
            Assert.IsNotNull(featureThree);

            var featureList = new EventedList<IBranchFeature>() {featureOne, featureTwo, featureThree};
            Assert.IsNotNull(featureList);
            Assert.IsTrue(featureList.Any());
            #endregion

            var expectedLogMessage = string.Format("Sewer connection {0} does not accept more than one branch feature", sewerConnection.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.BranchFeatures = featureList, expectedLogMessage);
        }

        [Test]
        public void ReplacingSewerConnectionFeatureBranchesWithOneCompositeBranchFeatureWithMultipleStructuresReturnsLogMessage()
        {
            var sewerConnection = GetSewerConnectionWithSourceAndTarget();
            Assert.IsNotNull(sewerConnection);

            var network = new HydroNetwork();
            sewerConnection.Network = network;

            var featureOne = new Pump();
            Assert.IsNotNull(featureOne);

            var featureTwo = new Pump();
            Assert.IsNotNull(featureTwo);
            var featureThree = new Pump();
            Assert.IsNotNull(featureThree);
            var compositeStructureTwo = new CompositeBranchStructure();
            compositeStructureTwo.Structures.AddRange(new []{featureTwo, featureThree});
            var featureReplacementList = new EventedList<IBranchFeature>() {compositeStructureTwo, featureTwo, featureThree};

            //No problem adding the first structure.
            var compositeStructureOne = sewerConnection.AddStructureToBranch(featureOne);
            Assert.IsTrue(sewerConnection.BranchFeatures.Any());
            Assert.IsTrue(sewerConnection.BranchFeatures.Count.Equals(2));
            Assert.IsTrue(sewerConnection.BranchFeatures.Contains(compositeStructureOne));
            Assert.IsTrue(sewerConnection.BranchFeatures.Contains(featureOne));

            //Try to replace the branch features directly with compositeStructureTwo, it should not be possible because there are more than one structures.
            var expectedLogMessage = string.Format("Sewer connection {0} does not accept more than one branch feature", sewerConnection.Name);
            TestHelper.AssertAtLeastOneLogMessagesContains(() => sewerConnection.BranchFeatures = featureReplacementList, expectedLogMessage);

            //Check the branch features remain the same
            Assert.IsTrue(sewerConnection.BranchFeatures.Any());
            Assert.IsTrue(sewerConnection.BranchFeatures.Count.Equals(2));
            Assert.IsTrue(sewerConnection.BranchFeatures.Contains(compositeStructureOne));
            Assert.IsTrue(sewerConnection.BranchFeatures.Contains(featureOne));
        }

        #region Helpers

        private SewerConnection GetSewerConnectionWithSourceAndTarget()
        {
            var compartmentOne = new Compartment("compartmentOne");
            var sourceManhole = new Manhole("sourceManhole")
            {
                Compartments = new EventedList<Compartment> {compartmentOne},
                Geometry = new Point(1, 3)
            };

            var compartmentTwo = new Compartment("compartmentTwo");
            var targetManhole = new Manhole("targetManhole")
            {
                Compartments = new EventedList<Compartment> {compartmentTwo},
                Geometry = new Point(10, 7)
            };

            var sewerConnection = new SewerConnection();
            sewerConnection.SourceCompartment = compartmentOne;
            sewerConnection.TargetCompartment = compartmentTwo;

            Assert.AreEqual(compartmentOne, sewerConnection.SourceCompartment);
            Assert.AreEqual(compartmentTwo, sewerConnection.TargetCompartment);
            Assert.AreEqual(sourceManhole, sewerConnection.Source);
            Assert.AreEqual(targetManhole, sewerConnection.Target);

            sewerConnection.Geometry = new LineString(new []{sourceManhole.Geometry.Coordinate, targetManhole.Geometry.Coordinate});
            Assert.IsNotNull(sewerConnection.Geometry);

            return sewerConnection;
        }

        #endregion
    }
}