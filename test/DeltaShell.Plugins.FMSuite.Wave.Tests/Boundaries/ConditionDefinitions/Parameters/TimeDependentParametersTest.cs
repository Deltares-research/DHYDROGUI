using System;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.Parameters
{
    [TestFixture]
    public class TimeDependentParametersTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var waveEnergyFunction = Substitute.For<IFunction>();

            // Call
            var parameters = new TimeDependentParameters(waveEnergyFunction);

            // Assert
            Assert.That(parameters, Is.InstanceOf<IBoundaryConditionParameters>());
            Assert.That(parameters, Has.Property(nameof(TimeDependentParameters.WaveEnergyFunction))
                                       .SameAs(waveEnergyFunction));
        }

        [Test]
        public void Constructor_WaveEnergyFunctionNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => new TimeDependentParameters(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveEnergyFunction"));
        }
    }
}