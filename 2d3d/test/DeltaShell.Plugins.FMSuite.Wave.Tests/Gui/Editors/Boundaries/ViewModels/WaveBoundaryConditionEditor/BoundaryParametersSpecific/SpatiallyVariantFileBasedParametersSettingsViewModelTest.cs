using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParametersSpecific
{
    public class SpatiallyVariantFileBasedParametersSettingsViewModelTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var dictionary = new Dictionary<SupportPoint, FileBasedParameters>();

            // Call
            var viewModel = new SpatiallyVariantFileBasedParametersSettingsViewModel(dictionary);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<FileBasedParametersSettingsViewModel>());
            Assert.That(viewModel, Is.InstanceOf<ISpatiallyVariantParametersSettingsViewModel>());
            Assert.That(viewModel.ActiveParametersViewModel, Is.Null,
                        "Expected ActiveParametersViewModel to be null upon construction.");
            Assert.That(viewModel.GroupBoxTitle, Is.EqualTo("Spatially Varying File Based Parameters"),
                        "Expected a different GroupBoxTitle:");
        }

        [Test]
        public void Constructor_SupportPointToParametersMappingNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SpatiallyVariantFileBasedParametersSettingsViewModel(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPointToParametersMapping"));
        }

        [Test]
        public void UpdateActiveSupportPoint_ObservedParameters_ValueInDictionary()
        {
            // Setup
            var supportPoint = new SupportPoint(20.0, Substitute.For<IWaveBoundaryGeometricDefinition>());
            var dictionary = new Dictionary<SupportPoint, FileBasedParameters> {{supportPoint, new FileBasedParameters("path")}};

            var viewModel = new SpatiallyVariantFileBasedParametersSettingsViewModel(dictionary);

            Assert.That(viewModel.ActiveParametersViewModel, Is.Null, "Precondition violated.");

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
            viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

            // Call
            viewModel.UpdateActiveSupportPoint(supportPoint);

            // Assert
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null);
            Assert.That(viewModel.ActiveParametersViewModel, Is.InstanceOf<FileBasedParametersViewModel>());
            FileBasedParameters observedParameters =
                viewModel.ActiveParametersViewModel.ObservedParameters;
            Assert.That(observedParameters, Is.SameAs(dictionary[supportPoint]));

            Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
            Assert.That(propertyChangedObserver.Senders.First(), Is.SameAs(viewModel));

            PropertyChangedEventArgs relevantEventArgs = propertyChangedObserver.EventArgses.First();
            Assert.That(relevantEventArgs.PropertyName,
                        Is.EqualTo(nameof(SpatiallyVariantFileBasedParametersSettingsViewModel.ActiveParametersViewModel)));
        }

        [Test]
        public void UpdateActiveSupportPoint_ObservedParameters_ValueNotInDictionary()
        {
            // Setup
            var supportPoint = new SupportPoint(20.0, Substitute.For<IWaveBoundaryGeometricDefinition>());
            var dictionary = new Dictionary<SupportPoint, FileBasedParameters> {{supportPoint, new FileBasedParameters("path")}};

            var viewModel = new SpatiallyVariantFileBasedParametersSettingsViewModel(dictionary);

            viewModel.UpdateActiveSupportPoint(supportPoint);
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null, "Precondition violated.");

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
            viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

            var otherSupportPoint = new SupportPoint(20.0, Substitute.For<IWaveBoundaryGeometricDefinition>());

            // Call
            viewModel.UpdateActiveSupportPoint(otherSupportPoint);

            // Assert
            Assert.That(viewModel.ActiveParametersViewModel, Is.Null);

            Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
            Assert.That(propertyChangedObserver.Senders.First(), Is.SameAs(viewModel));

            PropertyChangedEventArgs relevantEventArgs = propertyChangedObserver.EventArgses.First();
            Assert.That(relevantEventArgs.PropertyName,
                        Is.EqualTo(nameof(SpatiallyVariantFileBasedParametersSettingsViewModel.ActiveParametersViewModel)));
        }

        [Test]
        public void UpdateActiveSupportPoint_ObservedParameters_ValueAlreadySet()
        {
            // Setup
            var supportPoint = new SupportPoint(20.0, Substitute.For<IWaveBoundaryGeometricDefinition>());
            var dictionary = new Dictionary<SupportPoint, FileBasedParameters> {{supportPoint, new FileBasedParameters("path")}};

            var viewModel = new SpatiallyVariantFileBasedParametersSettingsViewModel(dictionary);

            viewModel.UpdateActiveSupportPoint(supportPoint);
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null, "Precondition violated.");
            Assert.That(viewModel.ActiveParametersViewModel, Is.InstanceOf<FileBasedParametersViewModel>());
            FileBasedParameters initialObservedParameters =
                viewModel.ActiveParametersViewModel.ObservedParameters;
            Assert.That(initialObservedParameters,
                        Is.SameAs(dictionary[supportPoint]), "Precondition violated.");

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
            viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

            // Call
            viewModel.UpdateActiveSupportPoint(supportPoint);

            // Assert
            Assert.That(viewModel.ActiveParametersViewModel, Is.Not.Null);
            Assert.That(viewModel.ActiveParametersViewModel, Is.InstanceOf<FileBasedParametersViewModel>());
            FileBasedParameters currentObservedParameters =
                viewModel.ActiveParametersViewModel.ObservedParameters;
            Assert.That(currentObservedParameters,
                        Is.SameAs(dictionary[supportPoint]));

            Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));
        }

        [Test]
        public void UpdateActiveSupportPoint_ObservedParameters_ActiveParametersAlreadySet()
        {
            // Setup
            var dictionary = new Dictionary<SupportPoint, FileBasedParameters>();
            var viewModel = new SpatiallyVariantFileBasedParametersSettingsViewModel(dictionary);

            Assert.That(viewModel.ActiveParametersViewModel, Is.Null, "Precondition violated.");

            var propertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
            viewModel.PropertyChanged += propertyChangedObserver.OnEventFired;

            var supportPoint = new SupportPoint(20.0, Substitute.For<IWaveBoundaryGeometricDefinition>());

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
            var dictionary = new Dictionary<SupportPoint, FileBasedParameters>();
            var viewModel = new SpatiallyVariantFileBasedParametersSettingsViewModel(dictionary);

            // Call | Assert
            void Call() => viewModel.UpdateActiveSupportPoint(null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPoint"));
        }
    }
}