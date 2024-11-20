using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData.Laterals
{
    [TestFixture]
    public class LateralDischargeFunctionTest
    {
        [Test]
        public void Constructor_InitializesCorrectly()
        {
            // Call
            var lateralDischargeFunction = new LateralDischargeFunction();

            // Assert
            Assert.That(lateralDischargeFunction.Name, Is.EqualTo("Discharge"));
            Assert.That(lateralDischargeFunction.DischargeComponent, Is.SameAs(lateralDischargeFunction.Components.Single()));
            Assert.That(lateralDischargeFunction.DischargeComponent.Name, Is.EqualTo("Discharge"));
            Assert.That(lateralDischargeFunction.DischargeComponent.Unit.Name, Is.EqualTo("cubic meters per second"));
            Assert.That(lateralDischargeFunction.DischargeComponent.Unit.Symbol, Is.EqualTo("m3/s"));
        }
    }
}