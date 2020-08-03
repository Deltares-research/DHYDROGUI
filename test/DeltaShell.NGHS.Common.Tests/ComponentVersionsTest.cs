using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests
{
    [TestFixture]
    public class ComponentVersionsTest
    {
        [Test]
        public void Versions_Always_ReturnExpectedValues()
        {
            // Call
            const string fmSuiteVersion = ComponentVersions.FMSuiteVersion;

            // Assert
            Assert.That(fmSuiteVersion, Is.EqualTo("2020.05"));
        }
    }
}