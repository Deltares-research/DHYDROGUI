using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    [TestFixture]
    public class BoundaryWideParametersViewModelTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var modelShape = new GaussShape();
            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();

            boundaryCondition.Shape = modelShape;
            boundaryCondition.PeriodType = BoundaryConditionPeriodType.Mean;
            var dataComponent = Substitute.For<IBoundaryConditionDataComponent>();
            boundaryCondition.DataComponent = dataComponent;

            var viewShape = new GaussViewShape(modelShape);

            var shapeFactory = Substitute.For<IViewShapeFactory>();
            shapeFactory.ConstructFromShape(modelShape).Returns(viewShape);

            var dataComponentFactory = Substitute.For<IViewDataComponentFactory>();
            dataComponentFactory.GetDirectionalSpreadingViewType(dataComponent)
                                .Returns(DirectionalSpreadingViewType.Degrees);

            // Call
            var viewModel = new BoundaryWideParametersViewModel(boundaryCondition,
                                                                shapeFactory,
                                                                dataComponentFactory);

            // Assert
            shapeFactory.Received(1).ConstructFromShape(modelShape);

            Assert.That(viewModel, Is.InstanceOf<INotifyPropertyChanged>());
            Assert.That(viewModel, Is.InstanceOf<IRefreshViewModel>());
            Assert.That(viewModel.ShapeType, Is.EqualTo(typeof(GaussViewShape)));
            Assert.That(viewModel.Shape, Is.SameAs(viewShape));
            Assert.That(viewModel.PeriodType, Is.EqualTo(PeriodViewType.Mean));
            Assert.That(viewModel.DirectionalSpreadingType, Is.EqualTo(DirectionalSpreadingViewType.Degrees));
        }

        [Test]
        public void Constructor_ObservedBoundaryConditionNull_ThrowsArgumentNullException()
        {
            // Setup
            var shapeFactory = Substitute.For<IViewShapeFactory>();
            var dataComponentFactory = Substitute.For<IViewDataComponentFactory>();

            // Call
            void Call() => new BoundaryWideParametersViewModel(null,
                                                               shapeFactory,
                                                               dataComponentFactory);

            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("observedBoundaryCondition"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void Constructor_ShapeFactoryNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponentFactory = Substitute.For<IViewDataComponentFactory>();

            // Call
            void Call() => new BoundaryWideParametersViewModel(boundaryCondition,
                                                               null,
                                                               dataComponentFactory);

            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("shapeFactory"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void SetMediator_AnnounceDataComponentChangedNull_ThrowsArgumentNullException()
        {
            // Setup
            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponentFactory = Substitute.For<IViewDataComponentFactory>();
            var shapeFactory = Substitute.For<IViewShapeFactory>();

            var viewModel = new BoundaryWideParametersViewModel(boundaryCondition,
                                                                shapeFactory,
                                                                dataComponentFactory);

            // Call
            void Call() => viewModel.SetMediator(null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("mediator"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void ShapeTypeList_ExpectedValues()
        {
            // Setup
            var shapeFactory = Substitute.For<IBoundaryConditionShapeFactory>();
            var viewShapeFactory = new ViewShapeFactory(shapeFactory);
            
            var modelShape = new GaussShape();

            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();
            boundaryCondition.Shape = modelShape;
            boundaryCondition.PeriodType = BoundaryConditionPeriodType.Mean;

            ParametersTestConfig testConfig = new ParametersTestConfig().WithBoundaryCondition(boundaryCondition)
                                                                        .WithShapeFactory(viewShapeFactory)
                                                                        .WithDefaultDataComponentFactory()
                                                                        .WithDefaultAnnounceDataComponentChanged()
                                                                        .ConstructViewModel();
            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            // Call
            IReadOnlyList<Type> shapeTypes = viewModel.ShapeTypeList;

            // Assert
            Assert.That(shapeTypes, Is.Not.Null);

            var expectedTypes = new List<Type>
            {
                typeof(GaussViewShape),
                typeof(JonswapViewShape),
                typeof(PiersonMoskowitzViewShape)
            };

            Assert.That(shapeTypes, Is.EquivalentTo(expectedTypes));
        }

        [Test]
        [TestCaseSource(nameof(GetShapeTypeSetValueData))]
        public void ShapeType_SetValue_NotEqual(Type expectedType, ViewShapeType expectedViewShapeType, IViewShape expectedShape)
        {
            // Setup
            ParametersTestConfig testConfig = new ParametersTestConfig().WithDefaultBoundaryCondition()
                                                                        .WithDefaultShapeFactory()
                                                                        .WithShapeFactoryAction(f => f.ConstructFromType(expectedViewShapeType).Returns(expectedShape))
                                                                        .WithDefaultDataComponentFactory()
                                                                        .WithDefaultAnnounceDataComponentChanged()
                                                                        .ConstructViewModel();

            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            // Call
            viewModel.ShapeType = expectedType;

            // Assert
            Assert.That(viewModel.ShapeType, Is.EqualTo(expectedType));

            testConfig.ShapeFactory.Received(1).ConstructFromType(expectedViewShapeType);
            Assert.That(viewModel.Shape, Is.SameAs(expectedShape));

            Assert.That(testConfig.NPropertyChangedCalls, Is.EqualTo(2));

            var expectedSenders = new List<object>
            {
                viewModel,
                viewModel
            };

            Assert.That(testConfig.Senders, Is.EquivalentTo(expectedSenders));
            Assert.That(testConfig.EventArgses.Any(x => x.PropertyName == nameof(BoundaryWideParametersViewModel.ShapeType)));
            Assert.That(testConfig.EventArgses.Any(x => x.PropertyName == nameof(BoundaryWideParametersViewModel.Shape)));
        }

        [Test]
        public void ShapeType_SetValueUnsupportedType_ThrowsNotSupportedException()
        {
            // Setup
            ParametersTestConfig testConfig = new ParametersTestConfig().WithDefaultBoundaryCondition()
                                                                        .WithDefaultShapeFactory()
                                                                        .WithDefaultDataComponentFactory()
                                                                        .WithDefaultAnnounceDataComponentChanged()
                                                                        .ConstructViewModel();

            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            // Call | Assert
            void Call() => viewModel.ShapeType = typeof(object);
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void ShapeType_SetValue_Equal()
        {
            // Setup
            var modelShape = new GaussShape();
            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();

            boundaryCondition.Shape = modelShape;
            boundaryCondition.PeriodType = BoundaryConditionPeriodType.Mean;

            var viewShape = new GaussViewShape(modelShape);

            var shapeFactory = Substitute.For<IViewShapeFactory>();
            shapeFactory.ConstructFromShape(modelShape).Returns(viewShape);

            Type expectedType = typeof(GaussViewShape);
            const ViewShapeType expectedViewShapeType = ViewShapeType.Gauss;
            ParametersTestConfig testConfig = new ParametersTestConfig().WithBoundaryCondition(boundaryCondition)
                                                                        .WithShapeFactory(shapeFactory)
                                                                        .WithDefaultDataComponentFactory()
                                                                        .WithDefaultAnnounceDataComponentChanged()
                                                                        .ConstructViewModel();

            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            // Call
            viewModel.ShapeType = expectedType;

            // Assert
            Assert.That(viewModel.ShapeType, Is.EqualTo(expectedType));

            testConfig.ShapeFactory.DidNotReceiveWithAnyArgs().ConstructFromType(expectedViewShapeType);
            Assert.That(viewModel.Shape, Is.SameAs(viewShape));

            Assert.That(testConfig.NPropertyChangedCalls, Is.EqualTo(0));
        }

        [Test]
        public void PeriodType_SetValue_NotEqual()
        {
            // Setup
            var modelShape = new PiersonMoskowitzShape();
            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();
            boundaryCondition.Shape = modelShape;
            boundaryCondition.PeriodType = BoundaryConditionPeriodType.Mean;

            ParametersTestConfig testConfig = new ParametersTestConfig().WithBoundaryCondition(boundaryCondition)
                                                                        .WithDefaultShapeFactory()
                                                                        .WithDefaultDataComponentFactory()
                                                                        .WithDefaultAnnounceDataComponentChanged()
                                                                        .ConstructViewModel();

            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            // Call
            viewModel.PeriodType = PeriodViewType.Peak;

            // Assert
            Assert.That(viewModel.PeriodType, Is.EqualTo(PeriodViewType.Peak));
            boundaryCondition.Received(1).PeriodType = BoundaryConditionPeriodType.Peak;

            Assert.That(testConfig.NPropertyChangedCalls, Is.EqualTo(1));
            Assert.That(testConfig.Senders[0], Is.SameAs(viewModel));
            Assert.That(testConfig.EventArgses[0].PropertyName, Is.EqualTo(nameof(BoundaryWideParametersViewModel.PeriodType)));
        }

        [Test]
        public void PeriodType_SetValue_Equal()
        {
            // Setup
            var modelShape = new PiersonMoskowitzShape();
            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();
            boundaryCondition.Shape = modelShape;
            boundaryCondition.PeriodType = BoundaryConditionPeriodType.Mean;

            ParametersTestConfig testConfig = new ParametersTestConfig().WithBoundaryCondition(boundaryCondition)
                                                                        .WithDefaultShapeFactory()
                                                                        .WithDefaultDataComponentFactory()
                                                                        .WithDefaultAnnounceDataComponentChanged()
                                                                        .ConstructViewModel();

            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            // Call
            viewModel.PeriodType = PeriodViewType.Mean;

            // Assert
            Assert.That(viewModel.PeriodType, Is.EqualTo(PeriodViewType.Mean));
            boundaryCondition.ReceivedWithAnyArgs(1).PeriodType = BoundaryConditionPeriodType.Mean; // Only set up call gets counted.

            Assert.That(testConfig.NPropertyChangedCalls, Is.EqualTo(0));
        }

        [Test]
        public void DirectionalSpreadingType_SetValue_NotEqual()
        {
            // Setup
            var modelShape = new PiersonMoskowitzShape();
            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();
            boundaryCondition.Shape = modelShape;

            ParametersTestConfig testConfig = new ParametersTestConfig().WithBoundaryCondition(boundaryCondition)
                                                                        .WithDefaultShapeFactory()
                                                                        .WithDefaultDataComponentFactory()
                                                                        .WithDefaultAnnounceDataComponentChanged()
                                                                        .ConstructViewModel();
            var dataComponentDegrees = Substitute.For<IBoundaryConditionDataComponent>();
            testConfig.DataComponentFactory
                      .GetDirectionalSpreadingViewType(dataComponentDegrees)
                      .Returns(DirectionalSpreadingViewType.Degrees);

            boundaryCondition.DataComponent = dataComponentDegrees;
            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            var dataComponentPower = Substitute.For<IBoundaryConditionDataComponent>();
            testConfig.DataComponentFactory
                      .GetDirectionalSpreadingViewType(dataComponentPower)
                      .Returns(DirectionalSpreadingViewType.Power);
            testConfig.DataComponentFactory
                      .ConvertBoundaryConditionDataComponentSpreadingType(dataComponentDegrees,
                                                                          DirectionalSpreadingViewType.Power)
                      .Returns(dataComponentPower);

            // Call
            viewModel.DirectionalSpreadingType = DirectionalSpreadingViewType.Power;

            // Assert
            Assert.That(viewModel.DirectionalSpreadingType, Is.EqualTo(DirectionalSpreadingViewType.Power));
            Assert.That(testConfig.NPropertyChangedCalls, Is.EqualTo(1));
            Assert.That(testConfig.Senders[0], Is.SameAs(viewModel));
            Assert.That(testConfig.EventArgses[0].PropertyName, Is.EqualTo(nameof(BoundaryWideParametersViewModel.DirectionalSpreadingType)));

            testConfig.AnnounceDataComponentChanged.Received(1).AnnounceDataComponentChanged();
        }

        [Test]
        public void DirectionalSpreadingType_SetValue_Equal()
        {
            // Setup
            var modelShape = new PiersonMoskowitzShape();
            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();
            boundaryCondition.Shape = modelShape;

            ParametersTestConfig testConfig = new ParametersTestConfig().WithBoundaryCondition(boundaryCondition)
                                                                        .WithDefaultShapeFactory()
                                                                        .WithDefaultDataComponentFactory()
                                                                        .WithDefaultAnnounceDataComponentChanged()
                                                                        .ConstructViewModel();
            var dataComponentDegrees = Substitute.For<IBoundaryConditionDataComponent>();
            testConfig.DataComponentFactory
                      .GetDirectionalSpreadingViewType(dataComponentDegrees)
                      .Returns(DirectionalSpreadingViewType.Degrees);

            boundaryCondition.DataComponent = dataComponentDegrees;
            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            // Call
            viewModel.DirectionalSpreadingType = DirectionalSpreadingViewType.Degrees;

            // Assert
            Assert.That(viewModel.DirectionalSpreadingType, Is.EqualTo(DirectionalSpreadingViewType.Degrees));
            Assert.That(testConfig.NPropertyChangedCalls, Is.EqualTo(0));
        }

        [Test]
        public void RaisePropertyChanged_PropertyChangedEventRaised()
        {
            // Setup
            ParametersTestConfig testConfig = new ParametersTestConfig().WithBoundaryCondition(Substitute.For<IWaveBoundaryConditionDefinition>())
                                                                        .WithDefaultShapeFactory()
                                                                        .WithDefaultDataComponentFactory()
                                                                        .WithDefaultAnnounceDataComponentChanged()
                                                                        .ConstructViewModel();
            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            var propertyChangedRaised = false;
            string propertyNameChanged = null;
            viewModel.PropertyChanged += (sender, args) =>
            {
                propertyChangedRaised = true;
                propertyNameChanged = args.PropertyName;
            };

            // Call
            viewModel.RefreshViewModel();

            // Assert
            Assert.That(propertyChangedRaised, Is.True);
            Assert.That(propertyNameChanged, Is.EqualTo(string.Empty));
        }

        /// <summary>
        /// Helper class to ease setup and verification of <see cref="BoundaryWideParametersViewModel"/>.
        /// </summary>
        private class ParametersTestConfig
        {
            public IWaveBoundaryConditionDefinition BoundaryCondition { get; private set; }
            public IViewDataComponentFactory DataComponentFactory { get; private set; }
            public IAnnounceDataComponentChanged AnnounceDataComponentChanged { get; private set; }
            public IViewShapeFactory ShapeFactory { get; private set; }
            public BoundaryWideParametersViewModel ViewModel { get; private set; }

            public int NPropertyChangedCalls { get; private set; }

            public IList<object> Senders { get; } = new List<object>();

            public IList<PropertyChangedEventArgs> EventArgses { get; } = new List<PropertyChangedEventArgs>();

            public ParametersTestConfig WithBoundaryCondition(IWaveBoundaryConditionDefinition boundaryCondition)
            {
                BoundaryCondition = boundaryCondition;
                return this;
            }

            public ParametersTestConfig WithDefaultBoundaryCondition()
            {
                var modelShape = Substitute.For<IBoundaryConditionShape>();
                BoundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();

                BoundaryCondition.Shape = modelShape;
                BoundaryCondition.PeriodType = BoundaryConditionPeriodType.Mean;

                return this;
            }

            public ParametersTestConfig WithDefaultDataComponentFactory()
            {
                DataComponentFactory = Substitute.For<IViewDataComponentFactory>();
                return this;
            }

            public ParametersTestConfig WithDefaultAnnounceDataComponentChanged()
            {
                AnnounceDataComponentChanged = Substitute.For<IAnnounceDataComponentChanged>();
                return this;
            }

            public ParametersTestConfig WithShapeFactory(IViewShapeFactory shapeFactory)
            {
                ShapeFactory = shapeFactory;
                return this;
            }

            public ParametersTestConfig WithShapeFactoryAction(Action<IViewShapeFactory> action)
            {
                action(ShapeFactory);
                return this;
            }

            public ParametersTestConfig WithDefaultShapeFactory()
            {
                Assert.That(BoundaryCondition, Is.Not.Null, "Create BoundaryCondition before ShapeFactory.");

                var viewShape = Substitute.For<IViewShape>();

                ShapeFactory = Substitute.For<IViewShapeFactory>();
                ShapeFactory.ConstructFromShape(BoundaryCondition.Shape).Returns(viewShape);

                return this;
            }

            public ParametersTestConfig ConstructViewModel()
            {
                Assert.That(BoundaryCondition, Is.Not.Null);
                Assert.That(ShapeFactory, Is.Not.Null);
                Assert.That(DataComponentFactory, Is.Not.Null);
                Assert.That(AnnounceDataComponentChanged, Is.Not.Null);

                ViewModel = new BoundaryWideParametersViewModel(BoundaryCondition,
                                                                ShapeFactory,
                                                                DataComponentFactory);
                ViewModel.SetMediator(AnnounceDataComponentChanged);
                ViewModel.PropertyChanged += OnPropertyChanged;

                return this;
            }

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                NPropertyChangedCalls += 1;
                Senders.Add(sender);
                EventArgses.Add(e);
            }
        }

        private static IEnumerable<TestCaseData> GetShapeTypeSetValueData()
        {
            yield return new TestCaseData(typeof(GaussViewShape), ViewShapeType.Gauss, new GaussViewShape(new GaussShape()));
            yield return new TestCaseData(typeof(JonswapViewShape), ViewShapeType.Jonswap, new JonswapViewShape(new JonswapShape()));
            yield return new TestCaseData(typeof(PiersonMoskowitzViewShape), ViewShapeType.PiersonMoskowitz, new PiersonMoskowitzViewShape(new PiersonMoskowitzShape()));
        }
    }
}