using DelftTools.Hydro;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class FeatureGroupExtensionsTest
    {
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

        [Test]
        public void GivenIGroupableFeature_WhenTryingToSetGroupName_ThenGroupNameIsEqualToFilePath()
        {
            var filePath = "subFolder/file.abc";

            var groupableFeature = mocks.DynamicMock<IGroupableFeature>();
            groupableFeature.Stub(gf => gf.GroupName).PropertyBehavior();
            mocks.ReplayAll();

            Assert.IsNull(groupableFeature.GroupName);
            GroupableFeatureExtensions.TrySetGroupName(groupableFeature, filePath);

            Assert.That(groupableFeature.GroupName, Is.EqualTo(filePath));
        }

        [Test]
        public void GivenIGroupableFeatureWithEmptyGroupName_ThenFeatureHasDefaultGroupName()
        {
            var groupableFeature = mocks.DynamicMock<IGroupableFeature>();
            groupableFeature.Expect(gf => gf.GroupName).Return(string.Empty).Repeat.Any();
            mocks.ReplayAll();

            Assert.IsTrue(groupableFeature.HasDefaultGroupName(Arg<string>.Is.Anything, Arg<string>.Is.Anything));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void GivenIGroupableFeatureWithDefaultGroupName_ThenFeatureHasDefaultGroupNameWhenRequested(bool featureGroupNameWithExtension)
        {
            var extension = ".ext";
            var defaultGroupName = "myDefaultGroupName";
            var featureGroupName = defaultGroupName + (featureGroupNameWithExtension ? extension : string.Empty);
            var groupableFeature = mocks.DynamicMock<IGroupableFeature>();
            groupableFeature.Expect(gf => gf.GroupName).Return(featureGroupName).Repeat.Any();
            mocks.ReplayAll();

            Assert.IsTrue(groupableFeature.HasDefaultGroupName(extension, defaultGroupName));
        }

        [Test]
        public void GivenIGroupableFeatureWithNonDefaultGroupName_ThenFeatureDoesNotHasDefaultGroupNameWhenRequested()
        {
            var extension = ".ext";
            var defaultGroupName = "myDefaultGroupName";
            var featureGroupName = "SomethingTotallyDifferent";
            var groupableFeature = mocks.DynamicMock<IGroupableFeature>();
            groupableFeature.Expect(gf => gf.GroupName).Return(featureGroupName).Repeat.Any();
            mocks.ReplayAll();

            Assert.IsFalse(groupableFeature.HasDefaultGroupName(extension, defaultGroupName));
        }
    }
}