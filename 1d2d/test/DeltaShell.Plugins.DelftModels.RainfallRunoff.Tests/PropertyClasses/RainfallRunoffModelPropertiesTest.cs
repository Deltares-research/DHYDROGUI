using System;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.PropertyClasses
{
    [TestFixture]
    public class RainfallRunoffModelPropertiesTest
    {
        [Test]
        [TestCase(2005, true)]
        [TestCase(1990, false)]
        [TestCase(1950, true)]
        public void GivenRainfallRunoffModelProperties_SettingGreenHouseYear_ShouldCheckRange(short year, bool expectError)
        {
            //Arrange
            var model = new RainfallRunoffModel();
            var properties = new RainfallRunoffModelProperties { Data = model };

            // Act & Assert
            if (expectError)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => properties.GreenhouseYear = year);
                Assert.AreEqual(1994, properties.GreenhouseYear);
            }
            else
            {
                properties.GreenhouseYear = year;
                Assert.AreEqual(year, properties.GreenhouseYear);
            }
        }
    }
}