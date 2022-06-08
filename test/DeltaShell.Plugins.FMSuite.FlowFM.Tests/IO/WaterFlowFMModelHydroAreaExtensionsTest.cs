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
        private MockRepository mocks;
        private WaterFlowFMModel fmModel;
        private string mduFilePath;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            const string filePath = @"MduFileWithoutFeatureFileReferences/FlowFM.mdu";
            var testWorkingFolder = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection/MduFileProjects"));
            
            mduFilePath = Path.Combine(testWorkingFolder, filePath);
            fmModel = new WaterFlowFMModel(mduFilePath);
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
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
            CheckIfUpdateGroupNameGivesTheDesiredResult<Gate2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<Weir2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<Pump2D>(groupName, expectedGroupName);

            // other features
            CheckIfUpdateGroupNameGivesTheDesiredResult<LandBoundary2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<GroupableFeature2DPolygon>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<ThinDam2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<FixedWeir>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<GroupableFeature2DPoint>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<ObservationCrossSection2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<BridgePillar>(groupName, expectedGroupName);
        }

        [Test]
        [TestCase("myFile.pli", "FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate01.pli", "MyFile_structures.ini")]
        [TestCase("FeatureFiles/gate02.pli", "FlowFM_structures.ini")]
        [Category("Quarantine")]
        public void GivenStructureWithGroupNameThatIsNotInSubfolderOfMduFolder_WhenUpdatingGroupName_ThenGroupNameWillBeTheStructureFileName(string fileName, string expectedGroupName)
        {
            var parentDir = Directory.GetParent(Directory.GetParent(mduFilePath).FullName).FullName;
            CheckUpdatingNamesForStructures(fileName, expectedGroupName, parentDir);
        }

        [Test]
        [TestCase("myFile.ext", "myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsNotInSubfolderOfMduFolder_WhenUpdatingGroupName_ThenGroupNameWillBeTheFileName(string fileName, string expectedGroupName)
        {
            var parentDir = Directory.GetParent(Directory.GetParent(mduFilePath).FullName).FullName;
            CheckUpdatingNamesForHydroAreaFeatures(fileName, expectedGroupName, parentDir);
        }

        [Test]
        [TestCase("myFile.pli", "FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate01.pli", "FeatureFiles/FlowFM_structures.ini")]
        [TestCase("FeatureFiles/gate02.pli", "FeatureFiles/FlowFM_structures.ini")]
        public void GivenStructureWithGroupNameThatIsInSubfolderOfMduFolder_WhenUpdatingGroupName_ThenGroupNameWillBeTheStructureFileName(string fileName, string expectedGroupName)
        {
            var parentDir = Directory.GetParent(mduFilePath).FullName;
            CheckUpdatingNamesForStructures(fileName, expectedGroupName, parentDir);
        }

        [Test]
        [TestCase("myFile.ext", "myFile.ext")]
        [TestCase("FeatureFiles/myFile.ext", "FeatureFiles/myFile.ext")]
        public void GivenHydroAreaFeatureWithGroupNameThatIsInSubfolderOfMduFolder_WhenUpdatingGroupName_ThenGroupNameWillBeTheRelativePath(string fileName, string expectedGroupName)
        {
            var parentDir = Directory.GetParent(mduFilePath).FullName;
            CheckUpdatingNamesForHydroAreaFeatures(fileName, expectedGroupName, parentDir);
        }

        #region Helper methods

        private void CheckUpdatingNamesForStructures(string fileName, string expectedGroupName, string parentDir)
        {
            var groupName = Path.Combine(parentDir, fileName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<Gate2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<Weir2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<Pump2D>(groupName, expectedGroupName);
        }

        private void CheckUpdatingNamesForHydroAreaFeatures(string fileName, string expectedGroupName, string parentDir)
        {
            var groupName = Path.Combine(parentDir, fileName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<LandBoundary2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<GroupableFeature2DPolygon>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<ThinDam2D>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<FixedWeir>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<GroupableFeature2DPoint>(groupName, expectedGroupName);
            CheckIfUpdateGroupNameGivesTheDesiredResult<ObservationCrossSection2D>(groupName, expectedGroupName);
        }

        private void CheckIfUpdateGroupNameGivesTheDesiredResult<T>(string groupName, string expectedGroupName) where T : IGroupableFeature, new()
        {
            var gate = new T();
            gate.GroupName = groupName;
            gate.UpdateGroupName(fmModel);

            Assert.That(gate.GroupName, Is.EqualTo(expectedGroupName));
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

        #endregion
    }
}