using System.Linq;
using DelftTools.Utils.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class GroupableFeatureExtensionsTest
    {
        private const int NumberOfItemsInGroupA = 5;
        private const string NameGroupA = "Group_A";
        private const int NumberOfItemsInGroupB = 5;
        private const string NameGroupB = "Group_B";
        private const int NumberOfUngroupedItems = 5;
        private readonly MockRepository mock = new MockRepository();

        [Test]
        public void GroupableItemRemoveGroupTest()
        {
            IEventedList<IGroupableFeature> eventedList = new EventedList<IGroupableFeature>();

            PopulateListWithGroupableFeatures(eventedList);

            // remove all items with group B groupName
            eventedList.RemoveGroup(NameGroupB);
            Assert.AreEqual(NumberOfItemsInGroupA, eventedList.Count);
            Assert.IsFalse(eventedList.Any(i => i.GroupName == NameGroupB));

            // remove all items with group A groupName
            eventedList.RemoveGroup(NameGroupA);
            Assert.IsFalse(eventedList.Any(i => i.GroupName == NameGroupA));
            Assert.AreEqual(0, eventedList.Count);
        }

        [Test]
        public void GroupableItemsRemoveUngroupedTest()
        {
            IEventedList<IGroupableFeature> eventedList = new EventedList<IGroupableFeature>();

            PopulateListWithGroupableFeatures(eventedList, true);

            // remove all ungrouped items
            eventedList.RemoveUngroupedItems();

            Assert.AreEqual(NumberOfItemsInGroupA + NumberOfItemsInGroupB, eventedList.Count);
            Assert.IsFalse(eventedList.Any(item => string.IsNullOrWhiteSpace(item.GroupName)));
        }

        private void PopulateListWithGroupableFeatures(IEventedList<IGroupableFeature> eventedList, bool addUngroupedItems = false)
        {
            // populate list
            for (var i = 0; i < NumberOfItemsInGroupA; ++i)
            {
                var groupableFeature = mock.Stub<IGroupableFeature>();
                groupableFeature.GroupName = NameGroupA;
                eventedList.Add(groupableFeature);
            }

            for (var i = 0; i < NumberOfItemsInGroupB; ++i)
            {
                var groupableFeature = mock.Stub<IGroupableFeature>();
                groupableFeature.GroupName = NameGroupB;
                eventedList.Add(groupableFeature);
            }

            if (addUngroupedItems)
            {
                for (var i = 0; i < NumberOfUngroupedItems; ++i)
                {
                    var groupableFeature = mock.Stub<IGroupableFeature>();
                    eventedList.Add(groupableFeature);
                }
            }
        }
    }
}