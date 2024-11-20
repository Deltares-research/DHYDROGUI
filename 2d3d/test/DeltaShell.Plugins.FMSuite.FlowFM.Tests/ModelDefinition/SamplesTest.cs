using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.ModelDefinition
{
    [TestFixture]
    public class SamplesTest
    {
        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void Constructor_NameNullOrWhiteSpace_ThrowsException(string name)
        {
            // Call
            void Call() => new Samples(name);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }
    }
}