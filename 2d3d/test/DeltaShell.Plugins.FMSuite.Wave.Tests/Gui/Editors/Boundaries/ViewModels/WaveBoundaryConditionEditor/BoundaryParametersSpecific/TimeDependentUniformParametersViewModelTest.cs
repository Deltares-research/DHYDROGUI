using System;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.NGHS.TestUtils;
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
            Assert.That(viewModel.TimeDependentParametersFunctions, Is.EquivalentTo(new[]
            {
                underlyingFunction
            }));
        }

        [Test]
        public void Constructor_GenerateSeriesNull_ThrowsArgumentNullException()
        {
            // Setup
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);

            // Call | Assert
            void Call() => new TimeDependentUniformParametersViewModel<TSpreading>(null, parameters);

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

            // Call
            viewModel.GenerateTimeSeriesCommand.Execute(null);

            // Assert
            generateSeries.Received(1).Execute(waveEnergyFunction, null);
        }

        [Test]
        public void GenerateSeriesCommand_TemporarilySwitchesBackingFunction()
        {
            // Setup
            var waveEnergyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            var parameters = new TimeDependentParameters<TSpreading>(waveEnergyFunction);
            var generateSeries = Substitute.For<IGenerateSeries>();
            var viewModel = new TimeDependentUniformParametersViewModel<TSpreading>(generateSeries,
                                                                                    parameters);

            var observer = new EventTestObserver<PropertyChangedEventArgs>();
            viewModel.PropertyChanged += observer.OnEventFired;

            var stateIsValid = false;

            void VerifyStateAtGenerateSeries(object _) =>
                stateIsValid = !viewModel.TimeDependentParametersFunctions
                                         .Contains(waveEnergyFunction.UnderlyingFunction);

            generateSeries.When(x => x.Execute(waveEnergyFunction))
                          .Do(VerifyStateAtGenerateSeries);

            // Call
            viewModel.GenerateTimeSeriesCommand.Execute(null);

            // Assert
            Assert.That(stateIsValid);

            IFunction[] expectedEnergyFunctions =
            {
                waveEnergyFunction.UnderlyingFunction
            };
            Assert.That(viewModel.TimeDependentParametersFunctions, Is.EqualTo(expectedEnergyFunctions));
            Assert.That(observer.NCalls, Is.EqualTo(2));

            Assert.That(observer.EventArgses[0].PropertyName,
                        Is.EqualTo(nameof(viewModel.TimeDependentParametersFunctions)));
            Assert.That(observer.EventArgses[1].PropertyName,
                        Is.EqualTo(nameof(viewModel.TimeDependentParametersFunctions)));

            Assert.That(observer.Senders[0], Is.EqualTo(viewModel));
            Assert.That(observer.Senders[1], Is.EqualTo(viewModel));
        }
    }
}