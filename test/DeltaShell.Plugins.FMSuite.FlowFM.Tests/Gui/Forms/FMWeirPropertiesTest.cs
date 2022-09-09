using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Forms
{
    [TestFixture]
    public class FMWeirPropertiesTest
    {
        [Test]
        public void CrestLevel_WeirIsUsingTimeSeriesForCrestLevel_ReturnsTimeSeriesString()
        {
            // Setup
            var weir = Substitute.For<IWeir>();
            weir.IsUsingTimeSeriesForCrestLevel().Returns(true);

            var fmWeirProperties = new FMWeirProperties() { Data = weir };

            // Call
            string result = fmWeirProperties.CrestLevel;
            
            // Assert
            const string expectedResult = "Time series";
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        
        [Test]
        public void CrestLevel_WeirIsNotUsingTimeSeriesForCrestLevel_ReturnsCrestLevelValueAsString()
        {
            // Setup
            const double randomCrestLevel = 123.456;
            
            var weir = Substitute.For<IWeir>();
            weir.IsUsingTimeSeriesForCrestLevel().Returns(false);
            weir.CrestLevel.Returns(randomCrestLevel);

            var fmWeirProperties = new FMWeirProperties() { Data = weir };

            // Call
            string result = fmWeirProperties.CrestLevel;
            
            // Assert
            Assert.That(result, Is.EqualTo(randomCrestLevel.ToString()));
        }
    }
}