using DelftTools.Utils;
using NUnit.Framework;
using Rhino.Mocks;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class NameableFeatureComparerTest
    {
        private readonly MockRepository mock = new MockRepository();

        [Test]
        public void ReplaceNameableFeatureWithoutNamesTest()
        {
            var currentItem = mock.DynamicMock<INameable>();

            currentItem.Expect(c => c.Name).Return(null).Repeat.Once();

            var importedItem = mock.DynamicMock<INameable>();
            importedItem.Expect(c => c.Name).Return(null).Repeat.Once();

            mock.ReplayAll();

            bool shouldReplace = new NameableFeatureComparer<INameable>().Equals(currentItem, importedItem);
            Assert.IsTrue(shouldReplace);
        }

        [Test]
        public void ReplaceNameableFeatureWithEqualNamesTest()
        {
            var currentItem = mock.DynamicMock<INameable>();

            currentItem.Expect(c => c.Name).Return("Name_A").Repeat.Once();

            var importedItem = mock.DynamicMock<INameable>();
            importedItem.Expect(c => c.Name).Return("Name_A").Repeat.Once();

            mock.ReplayAll();

            bool shouldReplace = new NameableFeatureComparer<INameable>().Equals(currentItem, importedItem);
            Assert.IsTrue(shouldReplace);
        }

        [Test]
        public void NotReplaceNameableFeatureWithDifferentNamesTest()
        {
            var currentItem = mock.DynamicMock<INameable>();

            currentItem.Expect(c => c.Name).Return("Name_A").Repeat.Once();

            var importedItem = mock.DynamicMock<INameable>();
            importedItem.Expect(c => c.Name).Return("Name_B").Repeat.Once();

            mock.ReplayAll();

            bool shouldReplace = new NameableFeatureComparer<INameable>().Equals(currentItem, importedItem);
            Assert.IsFalse(shouldReplace);
        }

        [Test]
        public void NotReplaceWhileOneFeatureIsNullTest()
        {
            var importedItem = mock.DynamicMock<INameable>();

            mock.ReplayAll();

            bool shouldReplace = new NameableFeatureComparer<INameable>().Equals(null, importedItem);
            Assert.IsFalse(shouldReplace);
        }

        [Test]
        public void NotReplaceWhileTwoFeaturesAreNullTest()
        {
            bool shouldReplace = new NameableFeatureComparer<INameable>().Equals(null, null);
            Assert.IsTrue(shouldReplace);
        }
    }
}