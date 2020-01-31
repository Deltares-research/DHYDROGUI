using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    [TestFixture]
    public class BoundarySpecificParametersSettingsViewModelTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var factory = Substitute.For<IViewDataComponentFactory>();
            var dataComponentViewModel = Substitute.For<IParametersSettingsViewModel>();

            factory.ConstructParametersSettingsViewModel(conditionDefinition.DataComponent)
                   .Returns(dataComponentViewModel);

            // Call
            var viewModel = new BoundarySpecificParametersSettingsViewModel(conditionDefinition, factory);

            // Assert
            Assert.That(viewModel, Is.InstanceOf<IRefreshDataComponentViewModel>());
            Assert.That(viewModel.ParametersSettingsViewModel, Is.SameAs(dataComponentViewModel));
            factory.Received(1).ConstructParametersSettingsViewModel(conditionDefinition.DataComponent);
        }

        [Test]
        public void Constructor_ConditionDefinitionNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = Substitute.For<IViewDataComponentFactory>();

            // Call | Assert
            void Call() => new BoundarySpecificParametersSettingsViewModel(null, factory);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("conditionDefinition"));
        }

        [Test]
        public void Constructor_DataComponentFactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            // Call | Assert
            void Call() => new BoundarySpecificParametersSettingsViewModel(conditionDefinition, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponentFactory"));
        }

        [Test]
        public void SetParametersSettingsViewModel_NewValue_ExpectedResults()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var factory = Substitute.For<IViewDataComponentFactory>();
            var initialDataComponentViewModel = Substitute.For<IParametersSettingsViewModel>();

            factory.ConstructParametersSettingsViewModel(conditionDefinition.DataComponent)
                   .Returns(initialDataComponentViewModel);

            var viewModel = new BoundarySpecificParametersSettingsViewModel(conditionDefinition, factory);
            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;
            
            var newDataComponentViewModel = Substitute.For<IParametersSettingsViewModel>();

            // Call
            viewModel.ParametersSettingsViewModel = newDataComponentViewModel;

            // Assert
            Assert.That(viewModel.ParametersSettingsViewModel, Is.SameAs(newDataComponentViewModel),
                        "Expected a different ParametersSettingsViewModel");
            Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
            Assert.That(propertyChangedObserver.Senders.First(), Is.SameAs(viewModel));
            Assert.That(propertyChangedObserver.EventArgses.First().PropertyName, Is.SameAs("ParametersSettingsViewModel"));
        }

        [Test]
        public void SetParametersSettingsViewModel_SameValue_DoesNothing()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var factory = Substitute.For<IViewDataComponentFactory>();
            var dataComponentViewModel = Substitute.For<IParametersSettingsViewModel>();

            factory.ConstructParametersSettingsViewModel(conditionDefinition.DataComponent)
                   .Returns(dataComponentViewModel);

            var viewModel = new BoundarySpecificParametersSettingsViewModel(conditionDefinition, factory);
            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;
                
            // Call
            viewModel.ParametersSettingsViewModel = dataComponentViewModel;

            // Assert
            Assert.That(viewModel.ParametersSettingsViewModel, Is.SameAs(dataComponentViewModel),
                        "Expected a different ParametersSettingsViewModel");
            Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(0));
        }

        [Test]
        public void GivenAViewModel_WhenRefreshDataComponentViewModelIsCalled_ThenTheFactoryConstructsANewDataComponentViewModelAndThisIsSetOnTheParametersSettingsViewModel()
        {
            // Setup
            var initialDataComponent = Substitute.For<IBoundaryConditionDataComponent>();
            var newDataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            var initialDataComponentViewModel = Substitute.For<IParametersSettingsViewModel>();
            var newDataComponentViewModel = Substitute.For<IParametersSettingsViewModel>();

            var factory = Substitute.For<IViewDataComponentFactory>();
            factory.ConstructParametersSettingsViewModel(initialDataComponent)
                   .Returns(initialDataComponentViewModel);
            factory.ConstructParametersSettingsViewModel(newDataComponent)
                   .Returns(newDataComponentViewModel);

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = initialDataComponent;

            var viewModel = new BoundarySpecificParametersSettingsViewModel(conditionDefinition, factory);

            var propertyChangedObserver = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += propertyChangedObserver.OnPropertyChanged;
            
            conditionDefinition.DataComponent = newDataComponent;

            // Call
            viewModel.RefreshDataComponentViewModel();

            // Assert
            Assert.That(viewModel.ParametersSettingsViewModel, Is.SameAs(newDataComponentViewModel),
                        "Expected a different ParametersSettingsViewModel");

            Assert.That(propertyChangedObserver.NCalls, Is.EqualTo(1));
            Assert.That(propertyChangedObserver.Senders.First(), Is.SameAs(viewModel));
            Assert.That(propertyChangedObserver.EventArgses.First().PropertyName, Is.SameAs("ParametersSettingsViewModel"));
            
            factory.Received(1).ConstructParametersSettingsViewModel(newDataComponent);
        }

        [Test]
        public void UpdateSelectedActiveParameters_SpatiallyVariantConstantParametersSettingsViewModel_UpdateActiveSupportPointCalled()
        {
            // Setup
            var initialDataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(20.0);
            var supportPoint = new SupportPoint(10.0, geometricDefinition);
            var parametersDictionary = new Dictionary<SupportPoint, ConstantParameters>()
            {
                { supportPoint, new ConstantParameters(0, 0, 0, 0) }
            };

            var initialDataComponentViewModel = new SpatiallyVariantConstantParametersSettingsViewModel(parametersDictionary);

            var factory = Substitute.For<IViewDataComponentFactory>();
            factory.ConstructParametersSettingsViewModel(initialDataComponent)
                   .Returns(initialDataComponentViewModel);

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = initialDataComponent;

            var viewModel = new BoundarySpecificParametersSettingsViewModel(conditionDefinition, factory);

            // Call
            viewModel.UpdateSelectedActiveParameters(supportPoint);

            // Assert
            Assert.That(initialDataComponentViewModel.ActiveParametersViewModel.ObservedParameters, 
                        Is.SameAs(parametersDictionary[supportPoint]));
        }

        [Test]
        public void UpdateSelectedActiveParameters_UnsupportedParametersSettingsViewModel_ThrowsInvalidOperationException()
        {
            // Setup
            var initialDataComponent = Substitute.For<IBoundaryConditionDataComponent>();

            var initialDataComponentViewModel = Substitute.For<IParametersSettingsViewModel>();

            var factory = Substitute.For<IViewDataComponentFactory>();
            factory.ConstructParametersSettingsViewModel(initialDataComponent)
                   .Returns(initialDataComponentViewModel);

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = initialDataComponent;

            var viewModel = new BoundarySpecificParametersSettingsViewModel(conditionDefinition, factory);

            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(20.0);
            var supportPoint = new SupportPoint(10.0, geometricDefinition);

            // Call | Assert
            void Call() => viewModel.UpdateSelectedActiveParameters(supportPoint);

            Assert.Throws<InvalidOperationException>(Call);
        }
    }
}