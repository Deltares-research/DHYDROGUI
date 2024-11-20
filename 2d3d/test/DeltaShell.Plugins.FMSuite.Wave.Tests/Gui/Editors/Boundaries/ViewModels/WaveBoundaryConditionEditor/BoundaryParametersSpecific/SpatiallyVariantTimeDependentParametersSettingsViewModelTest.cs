using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class SpatiallyVariantTimeDependentParametersSettingsViewModelTest<TSpreading>
        where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>();
            var generateSeries = Substitute.For<IGenerateSeries>();

            // Call
            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary,
                                                                                                     generateSeries);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<TimeDependentParametersSettingsViewModel>());
            Assert.That(viewModel, Is.InstanceOf<ISpatiallyVariantParametersSettingsViewModel>());
            Assert.That(viewModel.ActiveParametersViewModel, Is.Null,
                        "Expected ActiveParametersViewModel to be null upon construction.");
            Assert.That(viewModel.GroupBoxTitle, Is.EqualTo("Spatially Varying Time Dependent Parameters"),
                        "Expected a different GroupBoxTitle:");
        }

        [Test]
        public void Constructor_SupportPointToParametersMappingNull_ThrowsArgumentNullException()
        {
            // Setup
            var generateSeries = Substitute.For<IGenerateSeries>();

            // Call | Assert
            void Call() => new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(null, generateSeries);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPointToParametersMapping"));
        }

        [Test]
        public void Constructor_GenerateSeriesNull_ThrowsArgumentNullException()
        {
            // Setup
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>();

            // Call | Assert
            void Call() => new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("generateSeries"));
        }

        [Test]
        public void UpdateActiveSupportPoint_ObservedParameters_ValueInDictionary()
        {
            // Setup
            SupportPoint supportPoint = GetDefaultSupportPoint();
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>() {{supportPoint, GetDefaultParameters()}};

            var generateSeries = Substitute.For<IGenerateSeries>();
            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary,
                                                                                                     generateSeries);

            Assert.That(viewModel.ActiveParametersViewModel, Is.Null, "Precondition violated.");

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
            viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

            // Call
            viewModel.UpdateActiveSupportPoint(supportPoint);

            // Assert
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null);
            Assert.That(viewModel.ActiveParametersViewModel, Is.InstanceOf<TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>>());
            TimeDependentParameters<TSpreading> observedParameters =
                ((TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>) viewModel.ActiveParametersViewModel).ObservedParameters;
            Assert.That(observedParameters, Is.SameAs(dictionary[supportPoint]));

            Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
            Assert.That(propertyChangedObserver.Senders.First(), Is.SameAs(viewModel));

            PropertyChangedEventArgs relevantEventArgs = propertyChangedObserver.EventArgses.First();
            Assert.That(relevantEventArgs.PropertyName,
                        Is.EqualTo(nameof(SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>.ActiveParametersViewModel)));
        }

        [Test]
        public void UpdateActiveSupportPoint_ObservedParameters_ValueNotInDictionary()
        {
            // Setup
            SupportPoint supportPoint = GetDefaultSupportPoint();
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>() {{supportPoint, GetDefaultParameters()}};

            var generateSeries = Substitute.For<IGenerateSeries>();
            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary,
                                                                                                     generateSeries);

            viewModel.UpdateActiveSupportPoint(supportPoint);
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null, "Precondition violated.");

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
            viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

            SupportPoint otherSupportPoint = GetDefaultSupportPoint();

            // Call
            viewModel.UpdateActiveSupportPoint(otherSupportPoint);

            // Assert
            Assert.That(viewModel.ActiveParametersViewModel, Is.Null);

            Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
            Assert.That(propertyChangedObserver.Senders.First(), Is.SameAs(viewModel));

            PropertyChangedEventArgs relevantEventArgs = propertyChangedObserver.EventArgses.First();
            Assert.That(relevantEventArgs.PropertyName,
                        Is.EqualTo(nameof(SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>.ActiveParametersViewModel)));
        }

        [Test]
        public void UpdateActiveSupportPoint_ObservedParameters_ValueAlreadySet()
        {
            // Setup
            SupportPoint supportPoint = GetDefaultSupportPoint();
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>() {{supportPoint, GetDefaultParameters()}};

            var generateSeries = Substitute.For<IGenerateSeries>();
            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary,
                                                                                                     generateSeries);

            viewModel.UpdateActiveSupportPoint(supportPoint);
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null, "Precondition violated.");
            Assert.That(viewModel.ActiveParametersViewModel, Is.InstanceOf<TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>>());
            TimeDependentParameters<TSpreading> initialObservedParameters =
                ((TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>) viewModel.ActiveParametersViewModel).ObservedParameters;
            Assert.That(initialObservedParameters,
                        Is.SameAs(dictionary[supportPoint]), "Precondition violated.");

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
            viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

            // Call
            viewModel.UpdateActiveSupportPoint(supportPoint);

            // Assert
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null);
            Assert.That(viewModel.ActiveParametersViewModel, Is.InstanceOf<TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>>());
            TimeDependentParameters<TSpreading> currentObservedParameters =
                ((TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>) viewModel.ActiveParametersViewModel).ObservedParameters;
            Assert.That(currentObservedParameters,
                        Is.SameAs(dictionary[supportPoint]));

            Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));
        }

        [Test]
        public void UpdateActiveSupportPoint_ObservedParameters_ActiveParametersAlreadySet()
        {
            // Setup
            var generateSeries = Substitute.For<IGenerateSeries>();
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>();
            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary,
                                                                                                     generateSeries);

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
            viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

            SupportPoint supportPoint = GetDefaultSupportPoint();

            // Preconditions
            Assert.That(viewModel.ActiveParametersViewModel, Is.Null, "Precondition violated.");

            // Call
            viewModel.UpdateActiveSupportPoint(supportPoint);

            // Assert
            Assert.That(viewModel.ActiveParametersViewModel, Is.Null);
            Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));
        }

        [Test]
        public void UpdateActiveSupportPoint_SupportPointNull_ThrowsArgumentNullException()
        {
            // Setup
            var generateSeries = Substitute.For<IGenerateSeries>();
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>();
            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary,
                                                                                                     generateSeries);

            // Call | Assert
            void Call() => viewModel.UpdateActiveSupportPoint(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPoint"));
        }

        private static SupportPoint GetDefaultSupportPoint() =>
            new SupportPoint(20.0, Substitute.For<IWaveBoundaryGeometricDefinition>());

        private static TimeDependentParameters<TSpreading> GetDefaultParameters() =>
            new TimeDependentParameters<TSpreading>(Substitute.For<IWaveEnergyFunction<TSpreading>>());
    }
}