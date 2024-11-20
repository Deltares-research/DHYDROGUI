using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData.Laterals
{
    [TestFixture]
    public class LateralDefinitionTest
    {
        [Test]
        public void Constructor_InitializesCorrectly()
        {
            // Call
            var lateralDefinition = new LateralDefinition();

            // Assert
            Assert.That(lateralDefinition.Discharge, Is.Not.Null);
        }
    }
}