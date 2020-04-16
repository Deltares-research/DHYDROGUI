using System;
using System.Windows.Forms;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;
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
            var viewModel = new TimeDependentUniformParametersViewModel<TSpreading>(Substitute.For<IGenerateSeries>(),
                                                                                    parameters);

            // Assert
            Assert.That(viewModel.ObservedParameters, Is.SameAs(parameters));
            Assert.That(viewModel.TimeDependentParametersFunctions, Is.EquivalentTo(new[] {underlyingFunction}));
        }

        [Test]
        public void Constructor_GenerateSeriesNull_ThrowsArgumentNullException()
        {
            // Setup
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);

            // Call | Assert
            void Call() => new TimeDependentUniformParametersViewModel<TSpreading>( null, parameters);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("generateSeries"));
        }

        [Test]
        public void Constructor_ParametersNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => new TimeDependentUniformParametersViewModel<TSpreading>(Substitute.For<IGenerateSeries>(), null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }

        [Test]
        public void GenerateSeriesCommand_ExpectedResult()
        {
            // Setup
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();

            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);

            var generateSeries = Substitute.For<IGenerateSeries>();
            var viewModel = new TimeDependentUniformParametersViewModel<TSpreading>(generateSeries,
                                                                                    parameters);

            var window = Substitute.For<IWin32Window>();

            // Call
            viewModel.GenerateTimeSeriesCommand.Execute(window);
            
            // Assert
            generateSeries.Received(1).Execute(window, waveEnergyFunction, null);
        }
    }
}