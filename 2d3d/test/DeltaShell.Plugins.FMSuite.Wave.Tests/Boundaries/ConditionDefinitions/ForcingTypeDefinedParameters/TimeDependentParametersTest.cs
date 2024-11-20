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
            Assert.That(parameters, Is.InstanceOf<IForcingTypeDefinedParameters>());
            Assert.That(parameters.WaveEnergyFunction, Is.SameAs(waveEnergyFunction));
        }

        [Test]
        public void Constructor_WaveEnergyFunctionNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => new TimeDependentParameters<TSpreading>(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveEnergyFunction"));
        }

        [Test]
        public void AcceptVisitor_VisitorNull_ThrowsArgumentNullExceptionForTimeDependentParameters()
        {
            // Setup
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);

            // Call
            void Call() => parameters.AcceptVisitor(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("visitor"));
        }

        [Test]
        public void AcceptVisitor_CallsCorrectVisitorMethodForTimeDependentParameters()
        {
            // Setup
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);
            var visitor = Substitute.For<IForcingTypeDefinedParametersVisitor>();

            // Call
            parameters.AcceptVisitor(visitor);

            // Assert
            visitor.Received(1).Visit(parameters);
        }
    }
}