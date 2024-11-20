using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Spreading
{
    [TestFixture]
    public class WaveSpreadingConstantsTest
    {
        [Test]
        public void DegreesDefaultSpreading_ExpectedResults()
        {
            Assert.That(WaveSpreadingConstants.DegreesDefaultSpreading, Is.EqualTo(20.0));
        }

        [Test]
        public void PowerDefaultSpreading_ExpectedResults()
        {
            Assert.That(WaveSpreadingConstants.PowerDefaultSpreading, Is.EqualTo(4.0));
        }
    }
}