using DelftTools.Hydro;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Extensions.Feature;
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
            FeatureGroupExtensions.TrySetGroupName(groupableFeature, filePath);

            Assert.That(groupableFeature.GroupName, Is.EqualTo(filePath));
        }
    }
}