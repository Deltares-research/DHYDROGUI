using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class MduFileHelperTest
    {
        private const string defaultGroupName = "DefaultGroupName";
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        #region UpdateFeatures

        [Test]
        [TestCase("")]
        [TestCase(null)]
        [TestCase("DefaultGroupName")]
        public void GivenFeatureWithNullOrEmptyGroupName_WhenUpdatingFeatures_ThenReturnDefaultGroupFeature(string initialGroupName)
        {
            UpdateFeaturesAndCheckResult(initialGroupName, true, defaultGroupName + FileConstants.ObsPointFileExtension, true, FileConstants.ObsPointFileExtension);
        }

        [Test]
        [TestCase(@"myDir\myGroupName", @"myDir/myGroupName")]
        [TestCase(@"my Dir/my Group Name", @"my_Dir/my_Group_Name")]
        public void GivenFeatureWithGroupNameContainingSpaceOrBackwardSlash_WhenUpdatingFeatures_ThenReturnUpdatedGroupNameWithUnderscoresAndForwardSlashes(string initialGroupName, string expectedUpdatedGroupName)
        {
            UpdateFeaturesAndCheckResult(initialGroupName, false, expectedUpdatedGroupName, false, FileConstants.ObsPointFileExtension);
        }

        [Test]
        [TestCase(@"myDir/myGroupName", @"myDir/myGroupName")]
        [TestCase(@"myDir/myGroupName_obs.xyn", @"myDir/myGroupName_obs.xyn")]
        public void GivenFeatureWithCorrectGroupName_WhenUpdatingFeatures_ThenReturnUpdatedGroupName(string initialGroupName, string expectedUpdatedGroupName)
        {
            UpdateFeaturesAndCheckResult(initialGroupName, false, expectedUpdatedGroupName, false, FileConstants.ObsPointFileExtension);
        }

        [Test]
        [TestCase("NotTheDefaultGroupName")]
        public void GivenFeatureWithGroupNameNotEqualToTheDefaultGroupNameButIsDefaultGroupFlagEqualToTrue_WhenUpdatingFeatures_ThenIsDefaultGroupFlagIsFalse(string initialGroupName)
        {
            UpdateFeaturesAndCheckResult(initialGroupName, true, initialGroupName, false, FileConstants.ObsPointFileExtension);
        }

        #endregion

        #region GetUniqueFilePathsForWindows

        [Test]
        [TestCase("MyGroupName", FileConstants.ObsPointFileExtension, "mygroupname", "MYGROUPNAME", "MyGroupName")]
        [TestCase("MyGroupName", ".xyn", "mygroupname", "MYGROUPNAME", "MyGroupName")]
        [TestCase("myDir/MyGroupName", FileConstants.ObsPointFileExtension, "myDir/mygroupname", "mydir/MYGROUPNAME", "MYDIR/MyGroupName")]
        [TestCase("myDir/MyGroupName", ".xyn", "myDir/mygroupname", "mydir/MYGROUPNAME", "MYDIR/MyGroupName")]
        public void GivenAnExistingFileAndFeaturesWithTheSameGroupName_WhenGettingUniqueFilePaths_ThenReturnExistingFileName(string existingGroupName, string extension, params string[] featureGroupNames)
        {
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            string mduDir = Path.GetDirectoryName(mduFilePath);
            string existingFilePath = Path.Combine(mduDir, existingGroupName + extension);

            // create file with name
            Directory.CreateDirectory(Path.GetDirectoryName(existingFilePath));
            FileStream fileStream = File.Create(existingFilePath);
            fileStream.Close();

            string name1 = featureGroupNames[0] + extension;
            string name2 = featureGroupNames[1] + extension;
            string name3 = featureGroupNames[2] + extension;
            List<IGroupableFeature> features = SetupFeatures(name1, name2, name3);

            string[] uniqueGroupNames = null;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => uniqueGroupNames = MduFileHelper.GetUniqueFilePathsForWindows(mduFilePath, features, FileConstants.ObsPointFileExtension), "already exists in the project folder. Features in group");

            // Check results
            Assert.That(uniqueGroupNames.Length, Is.EqualTo(1));
            Assert.That(uniqueGroupNames.First(), Is.EqualTo(existingGroupName + FileConstants.ObsPointFileExtension));

            // Check that the group names are not changed
            Assert.That(features.Count(f => f.GroupName == name1), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == name2), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == name3), Is.EqualTo(1));

            File.Delete(existingFilePath);
        }

        [Test]
        [TestCase("mygroupname", "MYGROUPNAME", "MyGroupName")]
        [TestCase("mygroupname", "MyGroupName", "MYGROUPNAME")]
        [TestCase("MYGROUPNAME", "mygroupname", "MyGroupName")]
        [TestCase("MYGROUPNAME", "MyGroupName", "mygroupname")]
        [TestCase("MyGroupName", "MYGROUPNAME", "mygroupname")]
        [TestCase("MyGroupName", "mygroupname", "MYGROUPNAME")]
        [TestCase("mygroupname" + FileConstants.ObsPointFileExtension, "MYGROUPNAME", "MyGroupName")]
        [TestCase(@"myDir/mygroupname" + FileConstants.ObsPointFileExtension, @"myDir/MYGROUPNAME", @"myDir/MyGroupName")]
        [TestCase(@"myDir/mygroupname", @"myDir/MYGROUPNAME" + FileConstants.ObsPointFileExtension, @"myDir/MyGroupName")]
        [TestCase(@"myDir/mygroupname", @"myDir/MYGROUPNAME", @"myDir/MyGroupName" + FileConstants.ObsPointFileExtension)]
        [TestCase(@"MYDIR/mygroupname", @"MyDir/MYGROUPNAME", @"mydir/MyGroupName")]
        [TestCase(@"MYDIR\mygroupname", @"MyDir/MYGROUPNAME", @"mydir/MyGroupName")]
        public void GivenFeaturesWithSameGroupName_WhenGettingUniqueFilePaths_ThenReturnOneFilePath(string firstName, string secondName, string thirdName)
        {
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            List<IGroupableFeature> features = SetupFeatures(firstName, secondName, thirdName);

            string[] uniqueGroupNames = null;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => uniqueGroupNames = MduFileHelper.GetUniqueFilePathsForWindows(mduFilePath, features, FileConstants.ObsPointFileExtension), "Features with group name");

            // Check results
            Assert.That(uniqueGroupNames.Length, Is.EqualTo(1));
            Assert.That(uniqueGroupNames.First(), Is.EqualTo(firstName.Replace(@"\", "/") + (firstName.EndsWith(FileConstants.ObsPointFileExtension) ? string.Empty : FileConstants.ObsPointFileExtension)));

            // Check that the group names are not changed
            Assert.That(features.Count(f => f.GroupName == firstName), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == secondName), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == thirdName), Is.EqualTo(1));
        }

        [Test]
        public void GivenFeaturesWithDifferentGroupName_WhenGettingUniqueFilePaths_ThenReturnDifferentPaths()
        {
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            List<IGroupableFeature> features = SetupFeatures("name1", "name2", "name3");

            string[] uniqueGroupNames = MduFileHelper.GetUniqueFilePathsForWindows(mduFilePath, features, FileConstants.ObsPointFileExtension);

            // Check results
            Assert.That(uniqueGroupNames.Length, Is.EqualTo(3));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains("name1")), Is.EqualTo(1));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains("name2")), Is.EqualTo(1));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains("name3")), Is.EqualTo(1));

            // Check that the group names are not changed
            Assert.That(features.Count(f => f.GroupName == "name1"), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == "name2"), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == "name3"), Is.EqualTo(1));
        }

        [Test]
        public void GivenFeaturesWhereSomeHaveTheSameGroupName_WhenGettingUniqueFilePaths_ThenReturnTheAppropriateAmountOfFilePaths()
        {
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            List<IGroupableFeature> features = SetupFeatures("name1", "name2", "name1");

            string[] uniqueGroupNames = MduFileHelper.GetUniqueFilePathsForWindows(mduFilePath, features, FileConstants.ObsPointFileExtension);

            // Check results
            Assert.That(uniqueGroupNames.Length, Is.EqualTo(2));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains("name1")), Is.EqualTo(1));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains("name2")), Is.EqualTo(1));

            // Check that the group names are not changed
            Assert.That(features.Count(f => f.GroupName == "name1"), Is.EqualTo(2));
            Assert.That(features.Count(f => f.GroupName == "name2"), Is.EqualTo(1));
        }

        private const string MyCustomExtension = "_something.lala";

        [TestCase("name1" + FileConstants.FixedWeirPliFileExtension, "name2" + FileConstants.FixedWeirPliFileExtension, "name3" + FileConstants.FixedWeirPliFileExtension)]
        [TestCase("name1", "name2", "name3")]
        [TestCase("name1" + FileConstants.FixedWeirPliFileExtension, "name2", "name3" + FileConstants.FixedWeirPliFileExtension)]
        [TestCase("name1" + MyCustomExtension, "name2" + MyCustomExtension, "name3" + MyCustomExtension)]
        public void GivenFeatureWithAlternativeExtensionInGroupName_WhenGettingUniqueFilePaths_ThenGroupNameDoesNotChange(string featureName1, string featureName2, string featureName3)
        {
            string mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            List<IGroupableFeature> features = SetupFeatures(featureName1, featureName2, featureName3);
            string[] uniqueGroupNames = MduFileHelper.GetUniqueFilePathsForWindows(mduFilePath, features, FileConstants.FixedWeirPliFileExtension, FileConstants.FixedWeirPliFileExtension, MyCustomExtension);

            // Check results
            Assert.That(uniqueGroupNames.Length, Is.EqualTo(3));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains(featureName1.EndsWith(FileConstants.FixedWeirPliFileExtension) || featureName1.EndsWith(FileConstants.FixedWeirPliFileExtension) || featureName1.EndsWith(MyCustomExtension) ? featureName1 : featureName1 + FileConstants.FixedWeirPliFileExtension)), Is.EqualTo(1));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains(featureName2.EndsWith(FileConstants.FixedWeirPliFileExtension) || featureName2.EndsWith(FileConstants.FixedWeirPliFileExtension) || featureName2.EndsWith(MyCustomExtension) ? featureName2 : featureName2 + FileConstants.FixedWeirPliFileExtension)), Is.EqualTo(1));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains(featureName3.EndsWith(FileConstants.FixedWeirPliFileExtension) || featureName3.EndsWith(FileConstants.FixedWeirPliFileExtension) || featureName3.EndsWith(MyCustomExtension) ? featureName3 : featureName3 + FileConstants.FixedWeirPliFileExtension)), Is.EqualTo(1));

            // Check that the group names are not changed
            Assert.That(features.Count(f => f.GroupName == featureName1), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == featureName2), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == featureName3), Is.EqualTo(1));
        }

        private static WaterFlowFMModelDefinition CreateModelDefinitionWithProperty(string propertyKey, string propertyValue)
        {
            var propertyDefinition = new WaterFlowFMPropertyDefinition
            {
                DataType = typeof(string),
                MduPropertyName = propertyKey
            };
            var property = new WaterFlowFMProperty(propertyDefinition, propertyValue);
            var modelDefinition = new WaterFlowFMModelDefinition();

            modelDefinition.Properties.Add(property);

            return modelDefinition;
        }

        #endregion

        #region Helper methods

        private List<IGroupableFeature> SetupFeatures(string firstName, string secondName, string thirdName)
        {
            // create features
            var feature1 = mocks.Stub<IGroupableFeature>();
            var feature2 = mocks.Stub<IGroupableFeature>();
            var feature3 = mocks.Stub<IGroupableFeature>();
            feature1.GroupName = firstName;
            feature2.GroupName = secondName;
            feature3.GroupName = thirdName;
            mocks.ReplayAll();

            // Get unique file paths and check for logging message
            return new List<IGroupableFeature>()
            {
                feature1,
                feature2,
                feature3
            };
        }

        private static void UpdateFeaturesAndCheckResult(string initialGroupName, bool initialIsDefaultGroupValue, string expectedUpdatedGroupName, bool expectedIsDefaultGroupValue, string extension)
        {
            var features = new List<GroupableFeature2DPoint>() {WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(initialGroupName, "MyName", initialIsDefaultGroupValue)};
            MduFileHelper.UpdateFeatures(features, extension, defaultGroupName);

            Assert.That(features.Count, Is.EqualTo(1));
            GroupableFeature2DPoint updatedFeature = features.FirstOrDefault();
            Assert.That(updatedFeature.GroupName, Is.EqualTo(expectedUpdatedGroupName));
            Assert.That(updatedFeature.IsDefaultGroup, Is.EqualTo(expectedIsDefaultGroupValue));
        }

        #endregion
    }
}