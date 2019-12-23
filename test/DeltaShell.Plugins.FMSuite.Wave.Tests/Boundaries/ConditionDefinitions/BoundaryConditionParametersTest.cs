using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions
{
    [TestFixture]
    public class BoundaryConditionParametersTest
    {
        private readonly Random random = new Random(37);

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            double expectedHeight = random.NextDouble() * 1000.0;
            double expectedPeriod = random.NextDouble() * 1000.0;
            double expectedDirection = random.NextDouble() * 1000.0;
            double expectedSpreading = random.NextDouble() * 1000.0;

            // Call
            var boundaryConditionParameters = new BoundaryConditionParameters(expectedHeight, 
                                                                              expectedPeriod, 
                                                                              expectedDirection, 
                                                                              expectedSpreading);

            // Assert
            Assert.That(boundaryConditionParameters.Height, Is.EqualTo(expectedHeight),
                        "Expected a different Height:");
            Assert.That(boundaryConditionParameters.Period, Is.EqualTo(expectedPeriod),
                        "Expected a different Period:");
            Assert.That(boundaryConditionParameters.Direction, Is.EqualTo(expectedDirection),
                        "Expected a different Direction:");
            Assert.That(boundaryConditionParameters.Spreading, Is.EqualTo(expectedSpreading),
                        "Expected a different Spreading:");
        }
    }
}