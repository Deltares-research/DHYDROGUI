using System;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    [TestFixture]
    public class BoundaryDescriptionViewModelTest
    {
        private static IWaveBoundary GetConfiguredWaveBoundary(string name)
        {
            var boundary = Substitute.For<IWaveBoundary>();
            boundary.Name = name;

            var dataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();
            boundary.ConditionDefinition.DataComponent = dataComponent;

            return boundary;
        }

        private static IViewDataComponentFactory GetConfiguredFactory(ISpatiallyDefinedDataComponent dataComponent,
                                                                      ForcingViewType forcingType, 
                                                                      SpatialDefinitionViewType spatialDefinition)
        {
            var factory = Substitute.For<IViewDataComponentFactory>();

            factory.GetForcingType(dataComponent).Returns(forcingType);
            factory.GetSpatialDefinition(dataComponent).Returns(spatialDefinition);

            return factory;
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            const string expectedName = "aBoundaryName";
            const ForcingViewType expectedForcingType = ForcingViewType.Constant;
            const SpatialDefinitionViewType expectedSpatialDefinition = SpatialDefinitionViewType.SpatiallyVarying;

            IWaveBoundary boundary = GetConfiguredWaveBoundary(expectedName);

            IViewDataComponentFactory factory = GetConfiguredFactory(boundary.ConditionDefinition.DataComponent,
                                                                     expectedForcingType, 
                                                                     expectedSpatialDefinition);

            // Call
            var viewModel = new BoundaryDescriptionViewModel(boundary, factory);

            // Assert
            Assert.That(viewModel.Name, Is.EqualTo(expectedName), 
                        "Expected a different name:");
            Assert.That(viewModel.SpatialDefinition, Is.EqualTo(expectedSpatialDefinition),
                "Expected a different SpatialDefinition:");
            Assert.That(viewModel.ForcingType, Is.EqualTo(expectedForcingType),
                        "Expected a different ForcingType");
        }

        [Test]
        public void Constructor_ObservedBoundaryNull_ThrowsArgumentNullException()
        {
            // Setup
            var factory = Substitute.For<IViewDataComponentFactory>();

            // Call
            void Call() => new BoundaryDescriptionViewModel(null, factory);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("observedBoundary"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void Constructor_DataComponentFactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundary = Substitute.For<IWaveBoundary>();

            // Call
            void Call() => new BoundaryDescriptionViewModel(boundary, null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("dataComponentFactory"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void SetMediator_AnnounceDataComponentChangedNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundary = Substitute.For<IWaveBoundary>();
            var factory = Substitute.For<IViewDataComponentFactory>();
            var viewModel = new BoundaryDescriptionViewModel(boundary, factory);

            // Call
            void Call() => viewModel.SetMediator(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("mediator"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void GivenABoundaryDescriptionViewModelWithABoundary_WhenNameIsSet_ThenTheNameInTheBoundaryIsAdjusted()
        {
            // Setup
            const string expectedName = "aBoundaryName";
            
            IWaveBoundary boundary = GetConfiguredWaveBoundary("someOtherName");
            IViewDataComponentFactory factory = GetConfiguredFactory(boundary.ConditionDefinition.DataComponent,
                                                                     ForcingViewType.Constant,
                                                                     SpatialDefinitionViewType.SpatiallyVarying);

            var viewModel = new BoundaryDescriptionViewModel(boundary, factory);
            var observer = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += observer.OnPropertyChanged;

            // Call
            viewModel.Name = expectedName;

            // Assert
            boundary.Received(1).Name = expectedName;
            Assert.That(observer.NCalls, Is.EqualTo(1));
            Assert.That(observer.Senders[0], Is.SameAs(viewModel));
            Assert.That(observer.EventArgses[0].PropertyName, Is.EqualTo("Name"));
        }

        [Test]
        public void GivenABoundaryDescriptionViewModelWithABoundary_WhenNameIsSetToSame_ThenNothingHappens()
        {
            // Setup
            const string expectedName = "aBoundaryName";
            
            IWaveBoundary boundary = GetConfiguredWaveBoundary(expectedName);
            IViewDataComponentFactory factory = GetConfiguredFactory(boundary.ConditionDefinition.DataComponent,
                                                                     ForcingViewType.Constant,
                                                                     SpatialDefinitionViewType.SpatiallyVarying);

            var viewModel = new BoundaryDescriptionViewModel(boundary, factory);
            var observer = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += observer.OnPropertyChanged;

            boundary.ClearReceivedCalls();

            // Call
            viewModel.Name = expectedName;

            // Assert
            boundary.DidNotReceiveWithAnyArgs().Name = expectedName;
            Assert.That(observer.NCalls, Is.EqualTo(0));
        }

        [Test]
        public void SetForcingType_ValueChanged_ExpectedResults()
        {
            // Setup
            IWaveBoundary boundary = GetConfiguredWaveBoundary("someName");
            IViewDataComponentFactory factory = GetConfiguredFactory(boundary.ConditionDefinition.DataComponent,
                                                                     ForcingViewType.Constant,
                                                                     SpatialDefinitionViewType.SpatiallyVarying);

            const DirectionalSpreadingViewType spreadingType = DirectionalSpreadingViewType.Degrees;
            factory.GetDirectionalSpreadingViewType(boundary.ConditionDefinition.DataComponent)
                   .Returns(spreadingType);

            var newDataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();
            factory.ConstructBoundaryConditionDataComponent(ForcingViewType.TimeSeries,
                                                            SpatialDefinitionViewType.SpatiallyVarying,
                                                            spreadingType)
                   .Returns(newDataComponent);

            var announceDataComponentChanged = Substitute.For<IAnnounceDataComponentChanged>();

            var viewModel = new BoundaryDescriptionViewModel(boundary, factory);
            viewModel.SetMediator(announceDataComponentChanged);

            var observer = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += observer.OnPropertyChanged;


            boundary.ConditionDefinition.ClearReceivedCalls();
            // Call
            viewModel.ForcingType = ForcingViewType.TimeSeries;

            // Assert
            Assert.That(observer.NCalls, Is.EqualTo(1));
            Assert.That(observer.Senders[0], Is.SameAs(viewModel));
            Assert.That(observer.EventArgses[0].PropertyName, Is.EqualTo("ForcingType"));

            boundary.ConditionDefinition.Received(1).DataComponent = newDataComponent;
            announceDataComponentChanged.Received(1).AnnounceDataComponentChanged();
        }

        [Test]
        public void SetForcingType_ValueSame_NothingChanged()
        {
            // Setup
            IWaveBoundary boundary = GetConfiguredWaveBoundary("someName");
            IViewDataComponentFactory factory = GetConfiguredFactory(boundary.ConditionDefinition.DataComponent,
                                                                     ForcingViewType.Constant,
                                                                     SpatialDefinitionViewType.SpatiallyVarying);
            var announceDataComponentChanged = Substitute.For<IAnnounceDataComponentChanged>();

            var viewModel = new BoundaryDescriptionViewModel(boundary, factory);
            viewModel.SetMediator(announceDataComponentChanged);
            
            var observer = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += observer.OnPropertyChanged;

            // Call
            viewModel.ForcingType = ForcingViewType.Constant;

            // Assert
            Assert.That(observer.NCalls, Is.EqualTo(0));
            announceDataComponentChanged.DidNotReceive().AnnounceDataComponentChanged();
        }

        [Test]
        public void SetSpatialDefinitionType_ValueChanged_ExpectedResults()
        {
            // Setup
            IWaveBoundary boundary = GetConfiguredWaveBoundary("someName");
            IViewDataComponentFactory factory = GetConfiguredFactory(boundary.ConditionDefinition.DataComponent,
                                                                     ForcingViewType.Constant,
                                                                     SpatialDefinitionViewType.SpatiallyVarying);

            const DirectionalSpreadingViewType spreadingType = DirectionalSpreadingViewType.Degrees;
            factory.GetDirectionalSpreadingViewType(boundary.ConditionDefinition.DataComponent)
                   .Returns(spreadingType);

            var newDataComponent = Substitute.For<ISpatiallyDefinedDataComponent>();
            factory.ConstructBoundaryConditionDataComponent(ForcingViewType.Constant,
                                                            SpatialDefinitionViewType.Uniform,
                                                            spreadingType)
                   .Returns(newDataComponent);

            var announceDataComponentChanged = Substitute.For<IAnnounceDataComponentChanged>();

            var viewModel = new BoundaryDescriptionViewModel(boundary, factory);
            viewModel.SetMediator(announceDataComponentChanged);
            
            var observer = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += observer.OnPropertyChanged;


            boundary.ConditionDefinition.ClearReceivedCalls();
            // Call
            viewModel.SpatialDefinition = SpatialDefinitionViewType.Uniform;

            // Assert
            Assert.That(observer.NCalls, Is.EqualTo(1));
            Assert.That(observer.Senders[0], Is.SameAs(viewModel));
            Assert.That(observer.EventArgses[0].PropertyName, Is.EqualTo("SpatialDefinition"));

            boundary.ConditionDefinition.Received(1).DataComponent = newDataComponent;
            announceDataComponentChanged.Received(1).AnnounceDataComponentChanged();
        }

        [Test]
        public void SetSpatialDefinitionType_ValueSame_NothingChanged()
        {
            // Setup
            IWaveBoundary boundary = GetConfiguredWaveBoundary("someName");
            IViewDataComponentFactory factory = GetConfiguredFactory(boundary.ConditionDefinition.DataComponent,
                                                                     ForcingViewType.Constant,
                                                                     SpatialDefinitionViewType.SpatiallyVarying);
            var announceDataComponentChanged = Substitute.For<IAnnounceDataComponentChanged>();

            var viewModel = new BoundaryDescriptionViewModel(boundary, factory);
            viewModel.SetMediator(announceDataComponentChanged);
            
            var observer = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += observer.OnPropertyChanged;

            // Call
            viewModel.SpatialDefinition = SpatialDefinitionViewType.SpatiallyVarying;

            // Assert
            Assert.That(observer.NCalls, Is.EqualTo(0));
            announceDataComponentChanged.DidNotReceive().AnnounceDataComponentChanged();
        }
    }
}