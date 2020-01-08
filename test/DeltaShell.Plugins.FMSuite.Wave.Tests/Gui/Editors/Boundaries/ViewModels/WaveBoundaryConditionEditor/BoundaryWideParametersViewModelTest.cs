using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    [TestFixture]
    public class BoundaryWideParametersViewModelTest
    {
        /// <summary>
        /// Helper class to ease setup and verification of <see cref="BoundaryWideParametersViewModel"/>.
        /// </summary>
        private class ParametersTestConfig
        {
            public IWaveBoundaryConditionDefinition BoundaryCondition { get; private set; } = null;

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
                BoundaryCondition.DirectionalSpreadingType = BoundaryConditionDirectionalSpreadingType.Degrees;

                return this;
            }

            public ParametersTestConfig WithBoundaryConditionAction(Action<IWaveBoundaryConditionDefinition> action)
            {
                action(BoundaryCondition);
                return this;
            }

            public IViewShapeFactory ShapeFactory { get; private set; } = null;

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

            public BoundaryWideParametersViewModel ViewModel { get; private set; } = null;

            public ParametersTestConfig ConstructViewModel()
            {
                Assert.That(BoundaryCondition, Is.Not.Null);
                Assert.That(ShapeFactory, Is.Not.Null);

                ViewModel = new BoundaryWideParametersViewModel(BoundaryCondition, ShapeFactory);
                ViewModel.PropertyChanged += OnPropertyChanged; 

                return this;
            }

            public int NPropertyChangedCalls { get; private set; } = 0;

            public IList<object> Senders { get; } = new List<object>();

            public IList<PropertyChangedEventArgs> EventArgses { get; } = new List<PropertyChangedEventArgs>();

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                NPropertyChangedCalls += 1;
                Senders.Add(sender);
                EventArgses.Add(e);
            }
        }

        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var modelShape = new GaussShape();
            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();
            
            boundaryCondition.Shape = modelShape;
            boundaryCondition.PeriodType = BoundaryConditionPeriodType.Mean;
            boundaryCondition.DirectionalSpreadingType = BoundaryConditionDirectionalSpreadingType.Degrees;

            var viewShape = new GaussViewShape(modelShape);

            var shapeFactory = Substitute.For<IViewShapeFactory>();
            shapeFactory.ConstructFromShape(modelShape).Returns(viewShape);

            // Call
            var viewModel = new BoundaryWideParametersViewModel(boundaryCondition, shapeFactory);

            // Assert
            shapeFactory.Received(1).ConstructFromShape(modelShape);

            Assert.That(viewModel.ShapeType, Is.EqualTo(typeof(GaussViewShape)));
            Assert.That(viewModel.Shape, Is.SameAs(viewShape));
            Assert.That(viewModel.PeriodType, Is.EqualTo(PeriodViewType.Mean));
            Assert.That(viewModel.DirectionalSpreadingType, Is.EqualTo(DirectionalSpreadingViewType.Degrees));
        }

        private static void ConfigureBoundaryCondition(IWaveBoundaryConditionDefinition boundaryCondition,
                                                       IBoundaryConditionShape shape,
                                                       BoundaryConditionPeriodType periodType,
                                                       BoundaryConditionDirectionalSpreadingType spreadingType)
        {
            boundaryCondition.Shape = shape;
            boundaryCondition.PeriodType = periodType;
            boundaryCondition.DirectionalSpreadingType = spreadingType;
        }

        [Test]
        public void Constructor_ObservedBoundaryConditionNull_ThrowsArgumentNullException()
        {
            // Setup
            var shapeFactory = Substitute.For<IViewShapeFactory>();

            // Call
            void Call() => new BoundaryWideParametersViewModel(null, shapeFactory);
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

            // Call
            void Call() => new BoundaryWideParametersViewModel(boundaryCondition, null);
            var exception = Assert.Throws<ArgumentNullException>(Call);

            // Assert
            Assert.That(exception.ParamName, Is.EqualTo("shapeFactory"), 
                        "Expected a different ParamName:");
        }

        [Test]
        public void ShapeTypeList_ExpectedValues()
        {
            // Setup
            ParametersTestConfig testConfig = new ParametersTestConfig().WithDefaultBoundaryCondition()
                                                                        .WithDefaultShapeFactory()
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
                typeof(PiersonMoskowitzViewShape),
            };

            Assert.That(shapeTypes, Is.EquivalentTo(expectedTypes));
        }

        [Test]
        public void ShapeType_SetValue_NotEqual()
        {
            // Setup
            Type expectedType = typeof(PiersonMoskowitzViewShape);
            var expectedShape = new PiersonMoskowitzViewShape(new PiersonMoskowitzShape());

            ParametersTestConfig testConfig = new ParametersTestConfig().WithDefaultBoundaryCondition()
                                                                        .WithDefaultShapeFactory()
                                                                        .WithShapeFactoryAction(f => f.ConstructFromType(expectedType).Returns(expectedShape))
                                                                        .ConstructViewModel();

            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            // Call
            viewModel.ShapeType = expectedType;

            // Assert
            Assert.That(viewModel.ShapeType, Is.EqualTo(expectedType));

            testConfig.ShapeFactory.Received(1).ConstructFromType(expectedType);
            Assert.That(viewModel.Shape, Is.SameAs(expectedShape));

            Assert.That(testConfig.NPropertyChangedCalls, Is.EqualTo(2));

            var expectedSenders = new List<object>
            {
                viewModel,
                viewModel,
            };

            Assert.That(testConfig.Senders, Is.EquivalentTo(expectedSenders));
            Assert.That(testConfig.EventArgses.Any(x => x.PropertyName == nameof(BoundaryWideParametersViewModel.ShapeType)));
            Assert.That(testConfig.EventArgses.Any(x => x.PropertyName == nameof(BoundaryWideParametersViewModel.Shape)));
        }

        [Test]
        public void ShapeType_SetValue_Equal()
        {
            // Setup
            var modelShape = new GaussShape();
            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();
            
            boundaryCondition.Shape = modelShape;
            boundaryCondition.PeriodType = BoundaryConditionPeriodType.Mean;
            boundaryCondition.DirectionalSpreadingType = BoundaryConditionDirectionalSpreadingType.Degrees;

            var viewShape = new GaussViewShape(modelShape);

            var shapeFactory = Substitute.For<IViewShapeFactory>();
            shapeFactory.ConstructFromShape(modelShape).Returns(viewShape);

            Type expectedType = typeof(GaussViewShape);
            ParametersTestConfig testConfig = new ParametersTestConfig().WithBoundaryCondition(boundaryCondition)
                                                                        .WithShapeFactory(shapeFactory)
                                                                        .ConstructViewModel();

            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            // Call
            viewModel.ShapeType = expectedType;

            // Assert
            Assert.That(viewModel.ShapeType, Is.EqualTo(expectedType));

            testConfig.ShapeFactory.DidNotReceiveWithAnyArgs().ConstructFromType(expectedType);
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
            boundaryCondition.DirectionalSpreadingType = BoundaryConditionDirectionalSpreadingType.Degrees;

            ParametersTestConfig testConfig = new ParametersTestConfig().WithBoundaryCondition(boundaryCondition)
                                                                        .WithDefaultShapeFactory()
                                                                        .ConstructViewModel();

            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            // Call
            viewModel.DirectionalSpreadingType = DirectionalSpreadingViewType.Power;

            // Assert
            Assert.That(viewModel.DirectionalSpreadingType, Is.EqualTo(DirectionalSpreadingViewType.Power));
            boundaryCondition.Received(1).DirectionalSpreadingType = BoundaryConditionDirectionalSpreadingType.Power;
            
            Assert.That(testConfig.NPropertyChangedCalls, Is.EqualTo(1));
            Assert.That(testConfig.Senders[0], Is.SameAs(viewModel));
            Assert.That(testConfig.EventArgses[0].PropertyName, Is.EqualTo(nameof(BoundaryWideParametersViewModel.DirectionalSpreadingType)));
        }

        [Test]
        public void DirectionalSpreadingType_SetValue_Equal()
        {
            // Setup
            var modelShape = new PiersonMoskowitzShape();
            var boundaryCondition = Substitute.For<IWaveBoundaryConditionDefinition>();
            boundaryCondition.Shape = modelShape;
            boundaryCondition.DirectionalSpreadingType = BoundaryConditionDirectionalSpreadingType.Degrees;

            ParametersTestConfig testConfig = new ParametersTestConfig().WithBoundaryCondition(boundaryCondition)
                                                                        .WithDefaultShapeFactory()
                                                                        .ConstructViewModel();

            BoundaryWideParametersViewModel viewModel = testConfig.ViewModel;

            // Call
            viewModel.DirectionalSpreadingType = DirectionalSpreadingViewType.Degrees;

            // Assert
            Assert.That(viewModel.DirectionalSpreadingType, Is.EqualTo(DirectionalSpreadingViewType.Degrees));
            boundaryCondition.ReceivedWithAnyArgs(1).DirectionalSpreadingType = BoundaryConditionDirectionalSpreadingType.Degrees; // Only set up call gets counted.
            
            Assert.That(testConfig.NPropertyChangedCalls, Is.EqualTo(0));
        }
    }
}