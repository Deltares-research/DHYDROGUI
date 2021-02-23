using DelftTools.Hydro.GroupableFeatures;
using DelftTools.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace DelftTools.Hydro.Tests.GroupableFeatures
{
    [TestFixture]
    public class GroupableFeatureComparerTest
    {
        private readonly MockRepository mock = new MockRepository();

        [Test]
        [TestCase(null, null, null, null, true)]
        [TestCase("name", "groupName", "name", "groupName", true)]
        [TestCase("name", "groupName1", "name", "groupName2", false)]
        [TestCase("name1", "groupName", "name2", "groupName", false)]
        [TestCase("name", "groupName", "name", null, false)]
        [TestCase("name", "groupName", null, "groupName", false)]
        public void ReplaceGroupableFeatureTest(string firstName, string firstGroupName, string secondName, string secondGroupName, bool expectEqual)
        {
            var currentItem = mock.DynamicMock<ITestGroupableNameableFeature>();

            currentItem.Expect(c => c.Name).Return(firstName).Repeat.Any();
            currentItem.Expect(c => c.GroupName).Return(firstGroupName).Repeat.Any();

            var importedItem = mock.DynamicMock<ITestGroupableNameableFeature>();
            importedItem.Expect(c => c.Name).Return(secondName).Repeat.Any();
            importedItem.Expect(c => c.GroupName).Return(secondGroupName).Repeat.Any();

            mock.ReplayAll();

            var comparer = new GroupableFeatureComparer<ITestGroupableNameableFeature>();
            Assert.AreEqual(expectEqual, comparer.Equals(currentItem, importedItem));
            Assert.AreEqual(expectEqual, comparer.GetHashCode(currentItem) == comparer.GetHashCode(importedItem));
        }

        [Test]
        public void NotReplaceGroupableFeatureWithDifferentGroupNamesTest()
        {
            var currentItem = mock.DynamicMock<ITestGroupableNameableFeature>();

            currentItem.Expect(c => c.GroupName).Return("TheGreatestGroup").Repeat.Any();

            var importedItem = mock.DynamicMock<ITestGroupableNameableFeature>();
            importedItem.Expect(c => c.GroupName).Return("TheNotSoGreatGroup").Repeat.Any();

            mock.ReplayAll();

            var comparer = new GroupableFeatureComparer<ITestGroupableNameableFeature>();
            Assert.IsFalse(comparer.Equals(currentItem, importedItem));
            Assert.AreNotEqual(comparer.GetHashCode(currentItem), comparer.GetHashCode(importedItem));
        }

        [Test]
        public void NotReplaceWhileOneFeatureIsNullGroupableTest()
        {
            var importedItem = mock.DynamicMock<ITestGroupableNameableFeature>();

            mock.ReplayAll();

            bool shouldReplace = new GroupableFeatureComparer<ITestGroupableNameableFeature>().Equals(null, importedItem);
            Assert.IsFalse(shouldReplace);
        }

        [Test]
        public void NotReplaceWhileTwoFeaturesAreNullGroupableTest()
        {
            bool shouldReplace = new GroupableFeatureComparer<ITestGroupableNameableFeature>().Equals(null, null);
            Assert.IsTrue(shouldReplace);
        }

        public interface ITestGroupableNameableFeature : IGroupableFeature, INameable {}
    }
}