using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
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
            UpdateFeaturesAndCheckResult(initialGroupName, true, defaultGroupName + MduFile.ObsExtension, true, MduFile.ObsExtension);
        }

        [Test]
        [TestCase(@"myDir\myGroupName", @"myDir/myGroupName")]
        [TestCase(@"my Dir/my Group Name", @"my_Dir/my_Group_Name")]
        public void GivenFeatureWithGroupNameContainingSpaceOrBackwardSlash_WhenUpdatingFeatures_ThenReturnUpdatedGroupNameWithUnderscoresAndForwardSlashes(string initialGroupName, string expectedUpdatedGroupName)
        {
            UpdateFeaturesAndCheckResult(initialGroupName, false, expectedUpdatedGroupName, false, MduFile.ObsExtension);
        }

        [Test]
        [TestCase(@"myDir/myGroupName", @"myDir/myGroupName")]
        [TestCase(@"myDir/myGroupName_obs.xyn", @"myDir/myGroupName_obs.xyn")]
        public void GivenFeatureWithCorrectGroupName_WhenUpdatingFeatures_ThenReturnUpdatedGroupName(string initialGroupName, string expectedUpdatedGroupName)
        {
            UpdateFeaturesAndCheckResult(initialGroupName, false, expectedUpdatedGroupName, false, MduFile.ObsExtension);
        }

        [Test]
        [TestCase("NotTheDefaultGroupName")]
        public void GivenFeatureWithGroupNameNotEqualToTheDefaultGroupNameButIsDefaultGroupFlagEqualToTrue_WhenUpdatingFeatures_ThenIsDefaultGroupFlagIsFalse(string initialGroupName)
        {
            UpdateFeaturesAndCheckResult(initialGroupName, true, initialGroupName, false, MduFile.ObsExtension);
        }

        #endregion

        #region GetUniqueFilePathsForWindows
        
        [Test]
        [TestCase("MyGroupName", MduFile.ObsExtension, "mygroupname", "MYGROUPNAME", "MyGroupName")]
        [TestCase("MyGroupName", ".xyn", "mygroupname", "MYGROUPNAME", "MyGroupName")]
        [TestCase("myDir/MyGroupName", MduFile.ObsExtension, "myDir/mygroupname", "mydir/MYGROUPNAME", "MYDIR/MyGroupName")]
        [TestCase("myDir/MyGroupName", ".xyn", "myDir/mygroupname", "mydir/MYGROUPNAME", "MYDIR/MyGroupName")]
        public void GivenAnExistingFileAndFeaturesWithTheSameGroupName_WhenGettingUniqueFilePaths_ThenReturnExistingFileName(string existingGroupName, string extension, params string[] featureGroupNames)
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var mduDir = Path.GetDirectoryName(mduFilePath);
            var existingFilePath = Path.Combine(mduDir, existingGroupName + extension);

            // create file with name
            Directory.CreateDirectory(Path.GetDirectoryName(existingFilePath));
            var fileStream = File.Create(existingFilePath);
            fileStream.Close();

            var name1 = featureGroupNames[0] + extension;
            var name2 = featureGroupNames[1] + extension;
            var name3 = featureGroupNames[2] + extension;
            var features = SetupFeatures(name1, name2, name3);

            string[] uniqueGroupNames = null;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => uniqueGroupNames = MduFileHelper.GetUniqueFilePathsForWindows(mduFilePath, features, MduFile.ObsExtension), "already exists in the project folder. Features in group");

            // Check results
            Assert.That(uniqueGroupNames.Length, Is.EqualTo(1));
            Assert.That(uniqueGroupNames.First(), Is.EqualTo(existingGroupName + MduFile.ObsExtension));

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
        [TestCase("mygroupname" + MduFile.ObsExtension, "MYGROUPNAME", "MyGroupName")]
        [TestCase(@"myDir/mygroupname" + MduFile.ObsExtension, @"myDir/MYGROUPNAME", @"myDir/MyGroupName")]
        [TestCase(@"myDir/mygroupname", @"myDir/MYGROUPNAME" + MduFile.ObsExtension, @"myDir/MyGroupName")]
        [TestCase(@"myDir/mygroupname", @"myDir/MYGROUPNAME", @"myDir/MyGroupName" + MduFile.ObsExtension)]
        [TestCase(@"MYDIR/mygroupname", @"MyDir/MYGROUPNAME", @"mydir/MyGroupName")]
        [TestCase(@"MYDIR\mygroupname", @"MyDir/MYGROUPNAME", @"mydir/MyGroupName")]
        public void GivenFeaturesWithSameGroupName_WhenGettingUniqueFilePaths_ThenReturnOneFilePath(string firstName, string secondName, string thirdName)
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var features = SetupFeatures(firstName, secondName, thirdName);

            string[] uniqueGroupNames = null;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => uniqueGroupNames = MduFileHelper.GetUniqueFilePathsForWindows(mduFilePath, features, MduFile.ObsExtension), "Features with group name");

            // Check results
            Assert.That(uniqueGroupNames.Length, Is.EqualTo(1));
            Assert.That(uniqueGroupNames.First(), Is.EqualTo(firstName.Replace(@"\", "/") + (firstName.EndsWith(MduFile.ObsExtension) ? string.Empty : MduFile.ObsExtension)));

            // Check that the group names are not changed
            Assert.That(features.Count(f => f.GroupName == firstName), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == secondName), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == thirdName), Is.EqualTo(1));
        }

        [Test]
        public void GivenFeaturesWithDifferentGroupName_WhenGettingUniqueFilePaths_ThenReturnDifferentPaths()
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var features = SetupFeatures("name1", "name2", "name3");

            var uniqueGroupNames = MduFileHelper.GetUniqueFilePathsForWindows(mduFilePath, features, MduFile.ObsExtension);

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
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var features = SetupFeatures("name1", "name2", "name1");

            var uniqueGroupNames = MduFileHelper.GetUniqueFilePathsForWindows(mduFilePath, features, MduFile.ObsExtension);

            // Check results
            Assert.That(uniqueGroupNames.Length, Is.EqualTo(2));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains("name1")), Is.EqualTo(1));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains("name2")), Is.EqualTo(1));

            // Check that the group names are not changed
            Assert.That(features.Count(f => f.GroupName == "name1"), Is.EqualTo(2));
            Assert.That(features.Count(f => f.GroupName == "name2"), Is.EqualTo(1));
        }

        private const string MyCustomExtension = "_something.lala";
        [TestCase("name1" + MduFile.FixedWeirAlternativeExtension, "name2" + MduFile.FixedWeirAlternativeExtension, "name3" + MduFile.FixedWeirAlternativeExtension)]
        [TestCase("name1", "name2", "name3")]
        [TestCase("name1" + MduFile.FixedWeirAlternativeExtension, "name2", "name3" + MduFile.FixedWeirExtension)]
        [TestCase("name1" + MyCustomExtension, "name2" + MyCustomExtension, "name3" + MyCustomExtension)]
        public void GivenFeatureWithAlternativeExtensionInGroupName_WhenGettingUniqueFilePaths_ThenGroupNameDoesNotChange(string featureName1, string featureName2, string featureName3)
        {
            var mduFilePath = string.Concat(Path.GetTempFileName(), ".mdu");
            var features = SetupFeatures(featureName1, featureName2, featureName3);
            var uniqueGroupNames = MduFileHelper.GetUniqueFilePathsForWindows(mduFilePath, features, MduFile.FixedWeirExtension, MduFile.FixedWeirAlternativeExtension, MyCustomExtension);

            // Check results
            Assert.That(uniqueGroupNames.Length, Is.EqualTo(3));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains(featureName1.EndsWith(MduFile.FixedWeirExtension) || featureName1.EndsWith(MduFile.FixedWeirAlternativeExtension) || featureName1.EndsWith(MyCustomExtension) ? featureName1 : featureName1 + MduFile.FixedWeirExtension)), Is.EqualTo(1));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains(featureName2.EndsWith(MduFile.FixedWeirExtension) || featureName2.EndsWith(MduFile.FixedWeirAlternativeExtension) || featureName2.EndsWith(MyCustomExtension) ? featureName2 : featureName2 + MduFile.FixedWeirExtension)), Is.EqualTo(1));
            Assert.That(uniqueGroupNames.Count(gn => gn.Contains(featureName3.EndsWith(MduFile.FixedWeirExtension) || featureName3.EndsWith(MduFile.FixedWeirAlternativeExtension) || featureName3.EndsWith(MyCustomExtension) ? featureName3 : featureName3 + MduFile.FixedWeirExtension)), Is.EqualTo(1));

            // Check that the group names are not changed
            Assert.That(features.Count(f => f.GroupName == featureName1), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == featureName2), Is.EqualTo(1));
            Assert.That(features.Count(f => f.GroupName == featureName3), Is.EqualTo(1));
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
            return new List<IGroupableFeature>() {feature1, feature2, feature3};
        }

        private static void UpdateFeaturesAndCheckResult(string initialGroupName, bool initialIsDefaultGroupValue, string expectedUpdatedGroupName, bool expectedIsDefaultGroupValue, string extension)
        {
            var features = new List<GroupableFeature2DPoint>()
            {
                WaterFlowFMMduFileTestHelper.GetNewGroupableFeature2DPoint(initialGroupName, "MyName", initialIsDefaultGroupValue)
            };
            MduFileHelper.UpdateFeatures(features, extension, defaultGroupName);

            Assert.That(features.Count, Is.EqualTo(1));
            var updatedFeature = features.FirstOrDefault();
            Assert.That(updatedFeature.GroupName, Is.EqualTo(expectedUpdatedGroupName));
            Assert.That(updatedFeature.IsDefaultGroup, Is.EqualTo(expectedIsDefaultGroupValue));
        }

        #endregion
        
    }
}