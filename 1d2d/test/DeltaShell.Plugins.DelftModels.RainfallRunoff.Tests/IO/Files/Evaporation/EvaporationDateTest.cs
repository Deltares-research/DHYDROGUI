using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.Files.Evaporation
{
    [TestFixture]
    public class EvaporationDateTest
    {
        [Test]
        public void Constructor_WithYear_InitializesInstanceCorrectly()
        {
            // Call
            var date = new EvaporationDate(2022, 8, 24);

            // Assert
            Assert.That(date.Year, Is.EqualTo(2022));
            Assert.That(date.Month, Is.EqualTo(8));
            Assert.That(date.Day, Is.EqualTo(24));
        }

        [Test]
        public void Constructor_WithoutYear_InitializesInstanceCorrectly()
        {
            // Call
            var date = new EvaporationDate(8, 24);

            // Assert
            Assert.That(date.Year, Is.EqualTo(0));
            Assert.That(date.Month, Is.EqualTo(8));
            Assert.That(date.Day, Is.EqualTo(24));
        }
    }
}