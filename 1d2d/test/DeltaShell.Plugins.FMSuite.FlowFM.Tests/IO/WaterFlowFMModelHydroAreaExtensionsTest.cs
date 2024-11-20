using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class WaterFlowFMModelHydroAreaExtensionsTest
    {
        private TemporaryDirectory modelDir;
        private TemporaryDirectory featuresDir;
        private WaterFlowFMModel model;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string testDataDir = TestHelper.GetTestFilePath(@"HydroAreaCollection\MduFileProjects\MduFileWithoutFeatureFileReferences\FlowFM");

            modelDir = new TemporaryDirectory();
            modelDir.CopyDirectoryToTempDirectory(testDataDir);

            featuresDir = new TemporaryDirectory();
            featuresDir.CopyDirectoryToTempDirectory(Path.Combine(testDataDir, "FeatureFiles"));

            string mduFilePath = Path.Combine(modelDir.Path, @"FlowFM\MDU\FlowFM.mdu");

            model = new WaterFlowFMModel(mduFilePath);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            modelDir.Dispose();
            featuresDir.Dispose();
        }

        [Test]
        public void GivenNullValue_WhenRemovingDuplicateFeatures_ThenNoExceptionIsThrown()
        {
            WaterFlowFMModelHydroAreaExtensions.RemoveDuplicateFeatures(Arg<object>.Is.Anything, null, Arg<string>.Is.Anything);
        }

        [Test]
        public void GivenAddedFeatureThatIsADuplicateOfAFeatureInAList_WhenRemovingDuplicateFeatures_ThenAddedFeatureIsRemovedAgainAndUserReceivesWarning()
        {
            CheckIfRemoveDuplicateFeaturesWorks<LandBoundary2D>();
            CheckIfRemoveDuplicateFeaturesWorks<GroupableFeature2DPolygon>();
            CheckIfRemoveDuplicateFeaturesWorks<ThinDam2D>();
            CheckIfRemoveDuplicateFeaturesWorks<FixedWeir>();
            CheckIfRemoveDuplicateFeaturesWorks<GroupableFeature2DPoint>();
            CheckIfRemoveDuplicateFeaturesWorks<ObservationCrossSection2D>();
            CheckIfRemoveDuplicateFeaturesWorks<Pump2D>();
            CheckIfRemoveDuplicateFeaturesWorks<Weir2D>();
            CheckIfRemoveDuplicateFeaturesWorks<Gate2D>();
            CheckIfRemoveDuplicateFeaturesWorks<BridgePillar>();
        }

        [Test]
        public void GivenUniqueFeatures_WhenRemovingDuplicateFeatures_ThenNoFeaturesAreRemoved()
        {
            RemoveDuplicateFeaturesDoesNotRemoveUniqueFeatures<LandBoundary2D>();
            RemoveDuplicateFeaturesDoesNotRemoveUniqueFeatures<GroupableFeature2DPolygon>();
            RemoveDuplicateFeaturesDoesNotRemoveUniqueFeatures<ThinDam2D>();
            RemoveDuplicateFeaturesDoesNotRemoveUniqueFeatures<FixedWeir>();
            RemoveDuplicateFeaturesDoesNotRemoveUniqueFeatures<GroupableFeature2DPoint>();
            RemoveDuplicateFeaturesDoesNotRemoveUniqueFeatures<ObservationCrossSection2D>();
            RemoveDuplicateFeaturesDoesNotRemoveUniqueFeatures<Pump2D>();
            RemoveDuplicateFeaturesDoesNotRemoveUniqueFeatures<Weir2D>();
            RemoveDuplicateFeaturesDoesNotRemoveUniqueFeatures<Gate2D>();
            RemoveDuplicateFeaturesDoesNotRemoveUniqueFeatures<BridgePillar>();
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("myName", "myName")]
        [TestCase("mySubFolder/myName", "mySubFolder/myName")]
        [TestCase("mySubFolder/myName.ext", "mySubFolder/myName.ext")]
        [TestCase("mySubFolder/myName.ini", "mySubFolder/myName.ini")]
        public void GivenFeatureWithUnRootedGroupName_WhenUpdatingGroupName_ThenGroupNameWillNotChange(string groupName, string expectedGroupName)
        {
            // structures
            AssertUpdatedGroupName<Gate2D>(groupName, expectedGroupName);
            AssertUpdatedGroupName<Weir2D>(groupName, expectedGroupName);
            AssertUpdatedGroupName<Pump2D>(groupName, expectedGroupName);

            // other features
            AssertUpdatedGroupName<LandBoundary2D>(groupName, expectedGroupName);
            AssertUpdatedGroupName<GroupableFeature2DPolygon>(groupName, expectedGroupName);
            AssertUpdatedGroupName<ThinDam2D>(groupName, expectedGroupName);
            AssertUpdatedGroupName<FixedWeir>(groupName, expectedGroupName);
            AssertUpdatedGroupName<GroupableFeature2DPoint>(groupName, expectedGroupName);
            AssertUpdatedGroupName<ObservationCrossSection2D>(groupName, expectedGroupName);
            AssertUpdatedGroupName<BridgePillar>(groupName, expectedGroupName);
        }

        [Test]
        [TestCase("myFile.pli", "FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate01.pli", "FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate02.pli", "FlowFM_structures.ini")]
        public void GivenStructureWithGroupNameThatIsOutsideModelDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheStructureFileName(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForStructures(Path.Combine(featuresDir.Path, fileName), expectedGroupName);
        }

        [Test]
        [TestCase("myFile.ext", "myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsOutsideModelDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheFileName(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForHydroAreaFeatures(Path.Combine(featuresDir.Path, fileName), expectedGroupName);
        }

        [Test]
        [TestCase("myFile.pli", "../FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate01.pli", "../FeatureFiles/FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate02.pli", "../FeatureFiles/FlowFM_structures.ini")]
        public void GivenStructureWithGroupNameThatIsAboveMduDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheRelativePath(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForStructures(Path.Combine(model.GetModelDirectory(), fileName), expectedGroupName);
        }

        [Test]
        [TestCase("myFile.ext", "../myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "../FeatureFiles/myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsAboveMduDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheRelativePath(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForHydroAreaFeatures(Path.Combine(model.GetModelDirectory(), fileName), expectedGroupName);
        }

        [Test]
        [TestCase("myFile.pli", "FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate01.pli", "FeatureFiles/FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate02.pli", "FeatureFiles/FlowFM_structures.ini")]
        public void GivenStructureWithGroupNameThatIsInMduSubDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheRelativePath(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForStructures(Path.Combine(model.GetMduDirectory(), fileName), expectedGroupName);
        }

        [Test]
        [TestCase("myFile.ext", "myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "FeatureFiles/myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsInMduSubDir_WhenUpdatingGroupName_ThenGroupNameWillBeTheRelativePath(string fileName, string expectedGroupName)
        {
            AssertUpdatedGroupNameForHydroAreaFeatures(Path.Combine(model.GetMduDirectory(), fileName), expectedGroupName);
        }

        private void AssertUpdatedGroupNameForStructures(string newGroupName, string expectedGroupName)
        {
            AssertUpdatedGroupName<Gate2D>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<Weir2D>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<Pump2D>(newGroupName, expectedGroupName);
        }

        private void AssertUpdatedGroupNameForHydroAreaFeatures(string newGroupName, string expectedGroupName)
        {
            AssertUpdatedGroupName<LandBoundary2D>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<GroupableFeature2DPolygon>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<ThinDam2D>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<FixedWeir>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<GroupableFeature2DPoint>(newGroupName, expectedGroupName);
            AssertUpdatedGroupName<ObservationCrossSection2D>(newGroupName, expectedGroupName);
        }

        private void AssertUpdatedGroupName<TFeature>(string newGroupName, string expectedGroupName)
            where TFeature : IGroupableFeature, new()
        {
            var structure = new TFeature { GroupName = newGroupName };
            structure.UpdateGroupName(model);

            Assert.That(structure.GroupName, Is.EqualTo(expectedGroupName));
        }

        private static void CheckIfRemoveDuplicateFeaturesWorks<T>() where T : IGroupableFeature, INameable, new()
        {
            var features = new EventedList<T>();
            var feature1 = new T
            {
                GroupName = "MyGroup",
                Name = "MyName"
            };
            var feature2 = new T
            {
                GroupName = "MyGroup",
                Name = "MyName"
            };
            features.Add(feature1);
            features.Add(feature2);
            Assert.That(features.Count, Is.EqualTo(2));

            TestHelper.AssertAtLeastOneLogMessagesContains(
                () => WaterFlowFMModelHydroAreaExtensions.RemoveDuplicateFeatures(features, feature2, "MyModelName"),
                "', because a feature with the same properties already exists.");
            Assert.That(features.Count, Is.EqualTo(1));
        }

        private static void RemoveDuplicateFeaturesDoesNotRemoveUniqueFeatures<T>() where T : IGroupableFeature, INameable, new()
        {
            var features = new EventedList<T>();
            var feature1 = new T
            {
                GroupName = "MyGroup1",
                Name = "MyName"
            };
            var feature2 = new T
            {
                GroupName = "MyGroup2",
                Name = "MyName"
            };
            var feature3 = new T
            {
                GroupName = "MyGroup1",
                Name = "MyName1"
            };
            features.Add(feature1);
            features.Add(feature2);
            features.Add(feature3);
            Assert.That(features.Count, Is.EqualTo(3));

            WaterFlowFMModelHydroAreaExtensions.RemoveDuplicateFeatures(features, feature1, "MyModelName");
            Assert.That(features.Count, Is.EqualTo(3));
            WaterFlowFMModelHydroAreaExtensions.RemoveDuplicateFeatures(features, feature2, "MyModelName");
            Assert.That(features.Count, Is.EqualTo(3));
            WaterFlowFMModelHydroAreaExtensions.RemoveDuplicateFeatures(features, feature3, "MyModelName");
            Assert.That(features.Count, Is.EqualTo(3));
        }
    }
}