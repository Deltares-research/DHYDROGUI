using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData.Laterals
{
    [TestFixture]
    public class LateralDischargeTest
    {
        [Test]
        public void Constructor_InitializesCorrectly()
        {
            // Call
            var lateralDischarge = new LateralDischarge();

            // Assert
            Assert.That(lateralDischarge.Type, Is.EqualTo(LateralDischargeType.Constant));
            Assert.That(lateralDischarge.Constant, Is.Zero);
            Assert.That(lateralDischarge.TimeSeries, Is.Not.Null);
        }
    }
}