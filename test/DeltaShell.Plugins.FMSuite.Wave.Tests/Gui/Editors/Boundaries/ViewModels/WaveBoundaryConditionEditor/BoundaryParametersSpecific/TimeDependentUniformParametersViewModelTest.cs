using System;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    [TestFixture(typeof(PowerDefinedSpreading))]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    public class TimeDependentUniformParametersViewModelTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var underlyingFunction = Substitute.For<IFunction>();
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            waveEnergyFunction.UnderlyingFunction.Returns(underlyingFunction);

            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);

            // Call
            var viewModel = new TimeDependentUniformParametersViewModel<TSpreading>(parameters);

            // Assert
            Assert.That(viewModel.ObservedParameters, Is.SameAs(parameters));
        }

        [Test]
        public void Constructor_ParametersNull_ThrowsArgumentNullException()
        {
            void Call() => new TimeDependentUniformParametersViewModel<TSpreading>(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }
    }
}