using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
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

            // Call
            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<TimeDependentParametersSettingsViewModel>());
            Assert.That(viewModel.ActiveParametersViewModel, Is.Null, 
                        "Expected ActiveParametersViewModel to be null upon construction.");
            Assert.That(viewModel.GroupBoxTitle, Is.EqualTo("Spatially Varying Time Dependent Parameters"),
                        "Expected a different GroupBoxTitle:");
        }

        [Test]
        public void Constructor_SupportPointToParametersMappingNull_ThrowsArgumentNullException()
        {
            void Call() => new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPointToParametersMapping"));
        }

        private static SupportPoint GetDefaultSupportPoint() =>
            new SupportPoint(20.0, Substitute.For<IWaveBoundaryGeometricDefinition>());

        private static TimeDependentParameters<TSpreading> GetDefaultParameters() =>
            new TimeDependentParameters<TSpreading>(Substitute.For<IWaveEnergyFunction<TSpreading>>());


        [Test]
        public void UpdateActiveSupportPoint_ObservedParameters_ValueInDictionary()
        {
            // Setup
            SupportPoint supportPoint = GetDefaultSupportPoint();
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>()
            {
                {supportPoint, GetDefaultParameters()}
            };

            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary);

            Assert.That(viewModel.ActiveParametersViewModel, Is.Null, "Precondition violated.");

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

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
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>()
            {
                {supportPoint, GetDefaultParameters()}
            };

            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary);

            viewModel.UpdateActiveSupportPoint(supportPoint);
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null, "Precondition violated.");

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

            var otherSupportPoint = new SupportPoint(20.0, Substitute.For<IWaveBoundaryGeometricDefinition>());

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
        public void UpdateActiveSupportPoint_ObservedParameters_SupportPointNull()
        {
            // Setup
            SupportPoint supportPoint = GetDefaultSupportPoint();
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>()
            {
                {supportPoint, GetDefaultParameters()}
            };

            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary);

            viewModel.UpdateActiveSupportPoint(supportPoint);
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null, "Precondition violated.");

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

            // Call
            viewModel.UpdateActiveSupportPoint(null);

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
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>()
            {
                {supportPoint, GetDefaultParameters()}
            };

            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary);

            viewModel.UpdateActiveSupportPoint(supportPoint);
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null, "Precondition violated.");
            Assert.That(viewModel.ActiveParametersViewModel, Is.InstanceOf<TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>>());
            TimeDependentParameters<TSpreading> initialObservedParameters =
                ((TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>) viewModel.ActiveParametersViewModel).ObservedParameters;
            Assert.That(initialObservedParameters, 
                        Is.SameAs(dictionary[supportPoint]), "Precondition violated.");

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

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
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>();
            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary);

            Assert.That(viewModel.ActiveParametersViewModel, Is.Null, "Precondition violated.");

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;

            var supportPoint = new SupportPoint(20.0, Substitute.For<IWaveBoundaryGeometricDefinition>());

            // Call
            viewModel.UpdateActiveSupportPoint(supportPoint);

            // Assert
            Assert.That(viewModel.ActiveParametersViewModel, Is.Null);
            Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));
        }

        [Test]
        public void UpdateActiveSupportPoint_SupportPointNull_Sets()
        {
            // Setup
            var dictionary = new Dictionary<SupportPoint, TimeDependentParameters<TSpreading>>();
            var viewModel = new SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading>(dictionary);

            // Call
            viewModel.UpdateActiveSupportPoint(null);

            // Assert
            Assert.That(viewModel.ActiveParametersViewModel, Is.Null); 
        }
    }
}