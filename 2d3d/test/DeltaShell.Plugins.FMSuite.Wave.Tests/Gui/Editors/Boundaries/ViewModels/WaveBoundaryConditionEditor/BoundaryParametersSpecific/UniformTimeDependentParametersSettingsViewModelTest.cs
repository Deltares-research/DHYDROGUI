using System;
using System.Linq;
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
    [TestFixture]
    [TestFixture(typeof(PowerDefinedSpreading))]
    [TestFixture(typeof(DegreesDefinedSpreading))]
    public class UniformTimeDependentParametersSettingsViewModelTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var underlyingFunction = Substitute.For<IFunction>();
            var energyFunction = Substitute.For<IWaveEnergyFunction<TSpreading>>();
            energyFunction.UnderlyingFunction.Returns(underlyingFunction);

            var parameters = new TimeDependentParameters<TSpreading>(energyFunction);
            var generateSeries = Substitute.For<IGenerateSeries>();

            // Call
            var viewModel = new UniformTimeDependentParametersSettingsViewModel<TSpreading>(parameters, generateSeries);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<TimeDependentParametersSettingsViewModel>());
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null,
                        "Expected the ActiveParametersViewModel to not be null:");
            Assert.That(viewModel.ActiveParametersViewModel.TimeDependentParametersFunctions.Count(), Is.EqualTo(1));
            Assert.That(viewModel.ActiveParametersViewModel.TimeDependentParametersFunctions.First, Is.SameAs(underlyingFunction));

            Assert.That(viewModel.GroupBoxTitle, Is.EqualTo("Uniform Time Dependent Parameters"),
                        "Expected the GroupBoxTitle to be different:");
        }

        [Test]
        public void Constructor_ActiveParametersViewModelNull_ThrowsArgumentNullException()
        {
            // Setup
            var generateSeries = Substitute.For<IGenerateSeries>();

            // Call | Assert
            void Call() => new UniformTimeDependentParametersSettingsViewModel<TSpreading>(null, generateSeries);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("parameters"));
        }

        [Test]
        public void Constructor_GenerateSeriesNull_ThrowsArgumentNullException()
        {
            // Setup
            var parameters = new TimeDependentParameters<TSpreading>(Substitute.For<IWaveEnergyFunction<TSpreading>>());

            // Call | Assert
            void Call() => new UniformTimeDependentParametersSettingsViewModel<TSpreading>(parameters, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("generateSeries"));
        }
    }
}