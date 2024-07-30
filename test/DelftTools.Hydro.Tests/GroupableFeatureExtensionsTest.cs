using System.Linq;
using DelftTools.Utils.Collections.Generic;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class GroupableFeatureExtensionsTest
    {
        private const int numberOfItemsInGroupA = 5;
        private const string nameGroupA = "Group_A";
        private const int numberOfItemsInGroupB = 5;
        private const string nameGroupB = "Group_B";
        private const int numberOfUngroupedItems = 5;
        
        [Test]
        public void MakeGroupNameRelative_GroupNameIsRelativePath_GroupNameNotChanged()
        {
            const string groupName = "TestGroup.pli";
            const string rootDir = @"c:\models\model1";
            const string baseDir = @"c:\models\model1\input";

            var feature = Substitute.For<IGroupableFeature>();
            feature.GroupName = groupName;

            feature.MakeGroupNameRelative(rootDir, baseDir);

            Assert.That(feature.GroupName, Is.EqualTo(groupName));
        }

        [Test]
        public void MakeGroupNameRelative_GroupNameIsAbsolutePath_GroupNameChangedToRelativePath()
        {
            const string groupName = @"c:\models\model1\polylines\TestGroup.pli";
            const string rootDir = @"c:\models\model1";
            const string baseDir = @"c:\models\model1\input";

            var feature = Substitute.For<IGroupableFeature>();
            feature.GroupName = groupName;

            feature.MakeGroupNameRelative(rootDir, baseDir);

            Assert.That(feature.GroupName, Is.EqualTo(@"..\polylines\TestGroup.pli"));
        }
        
        [Test]
        public void MakeGroupNameRelative_GroupNameIsRelativePathOutsideRootDir_GroupNameChangedToFileName()
        {
            const string groupName = @"c:\temp\TestGroup.pli";
            const string rootDir = @"c:\models\model1";
            const string baseDir = @"c:\models\model1\input";

            var feature = Substitute.For<IGroupableFeature>();
            feature.GroupName = groupName;

            feature.MakeGroupNameRelative(rootDir, baseDir);

            Assert.That(feature.GroupName, Is.EqualTo("TestGroup.pli"));
        }

        [Test]
        public void GroupableItemRemoveGroupTest()
        {
            IEventedList<IGroupableFeature> eventedList = new EventedList<IGroupableFeature>();
            
            PopulateListWithGroupableFeatures(eventedList);

            // remove all items with group B groupName
            eventedList.RemoveGroup(nameGroupB);
            Assert.AreEqual(numberOfItemsInGroupA, eventedList.Count);
            Assert.IsFalse(eventedList.Any(i => i.GroupName == nameGroupB));

            // remove all items with group A groupName
            eventedList.RemoveGroup(nameGroupA);
            Assert.IsFalse(eventedList.Any(i => i.GroupName == nameGroupA));
            Assert.AreEqual(0, eventedList.Count);
        }

        [Test]
        public void GroupableItemsRemoveUngroupedTest()
        {
            IEventedList<IGroupableFeature> eventedList = new EventedList<IGroupableFeature>();
            
            PopulateListWithGroupableFeatures(eventedList, true);
            
            // remove all ungrouped items
            eventedList.RemoveUngroupedItems();

            Assert.AreEqual(numberOfItemsInGroupA + numberOfItemsInGroupB, eventedList.Count);
            Assert.IsFalse(eventedList.Any(item => string.IsNullOrWhiteSpace(item.GroupName)));
        }

        private void PopulateListWithGroupableFeatures(IEventedList<IGroupableFeature> eventedList, bool addUngroupedItems = false)
        {
            // populate list
            for (int i = 0; i < numberOfItemsInGroupA; ++i)
            {
                var groupableFeature = Substitute.For<IGroupableFeature>();
                groupableFeature.GroupName = nameGroupA;
                eventedList.Add(groupableFeature);
            }

            for (int i = 0; i < numberOfItemsInGroupB; ++i)
            {
                var groupableFeature = Substitute.For<IGroupableFeature>();
                groupableFeature.GroupName = nameGroupB;
                eventedList.Add(groupableFeature);
            }

            if (addUngroupedItems)
            {
                for (int i = 0; i < numberOfUngroupedItems; ++i)
                {
                    var groupableFeature = Substitute.For<IGroupableFeature>();
                    eventedList.Add(groupableFeature);
                }
            }
        }
    }
}