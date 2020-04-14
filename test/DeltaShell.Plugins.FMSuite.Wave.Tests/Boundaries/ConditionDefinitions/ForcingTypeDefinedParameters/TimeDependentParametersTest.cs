using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    [TestFixture]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class TimeDependentParametersTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();

            // Call
            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);

            // Assert
            Assert.That(parameters, Is.InstanceOf<IBoundaryConditionParameters>());
            Assert.That(parameters, Has.Property(nameof(TimeDependentParameters<TSpreading>.WaveEnergyFunction))
                                       .SameAs(waveEnergyFunction));
        }

        [Test]
        public void Constructor_WaveEnergyFunctionNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => new TimeDependentParameters<TSpreading>(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveEnergyFunction"));
        }
    }
}