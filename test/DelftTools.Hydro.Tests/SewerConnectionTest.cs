using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Networks;
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
        public void SewerConnectionBranchFeaturesDoesNotAcceptMoreThanOneFeature()
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

            //Try to replace the existent feature, it should be possible
            sewerConnection.BranchFeatures[0] = featureTwo;
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureTwo, sewerConnection.BranchFeatures.First());

            //Now try removing said feature and adding a new one.
            sewerConnection.BranchFeatures.Clear();
            Assert.IsFalse(sewerConnection.BranchFeatures.Any());

            sewerConnection.BranchFeatures.Add(featureThree);
            Assert.AreEqual(1, sewerConnection.BranchFeatures.Count);
            Assert.AreEqual(featureThree, sewerConnection.BranchFeatures.First());
        }

        [Test]
        public void ReplacingSewerConnectionFeatureBranchesReturnsLogMessage()
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
    }
}