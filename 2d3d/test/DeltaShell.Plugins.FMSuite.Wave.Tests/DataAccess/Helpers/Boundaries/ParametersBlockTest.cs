using System;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.Boundaries
{
    [TestFixture]
    public class ParametersBlockTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            double waveHeight = random.NextDouble();
            double period = random.NextDouble();
            double direction = random.NextDouble();
            double directionalSpreading = random.NextDouble();

            // Call
            var block = new ParametersBlock(waveHeight, period, direction, directionalSpreading);

            // Assert
            Assert.That(block.WaveHeight, Is.EqualTo(waveHeight));
            Assert.That(block.Period, Is.EqualTo(period));
            Assert.That(block.Direction, Is.EqualTo(direction));
            Assert.That(block.DirectionalSpreading, Is.EqualTo(directionalSpreading));
        }
    }
}