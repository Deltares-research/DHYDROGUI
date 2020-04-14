using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Eventing;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints
{
    [TestFixture(typeof(DegreesDefinedSpreading))]
    [TestFixture(typeof(PowerDefinedSpreading))]
    public class SupportPointViewModelTest<TSpreading> where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        private readonly Random random = new Random();
        private SupportPointViewModel viewModel;
        private SupportPoint supportPoint;
        private SupportPointDataComponentViewModel supportPointDataComponentViewModel;

        [SetUp]
        public void SetUp()
        {
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = 
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();

            var mediator = Substitute.For<IAnnounceSelectedSupportPointDataChanged>();
            supportPointDataComponentViewModel = 
                new SupportPointDataComponentViewModel(conditionDefinition,
                                                       new BoundaryParametersFactory(), 
                                                       mediator);

            supportPoint = new SupportPoint(0, Substitute.For<IWaveBoundaryGeometricDefinition>());
            viewModel = new SupportPointViewModel(supportPoint, supportPointDataComponentViewModel);
        }

        [Test]
        public void Constructor_SupportPointNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SupportPointViewModel(null, supportPointDataComponentViewModel, random.NextBoolean());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPoint"));
        }

        [Test]
        public void Constructor_SupportPointNullAndDefaultIsEditable_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SupportPointViewModel(null, supportPointDataComponentViewModel);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPoint"));
        }

        [Test]
        public void Constructor_DataComponentViewModelNullAndDefaultIsEditable_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SupportPointViewModel(supportPoint, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataComponentViewModel"));
        }

        [Test]
        public void Constructor_DefaultIsEditable_SetsCorrectValues()
        {
            // Assert
            Assert.That(viewModel.SupportPoint, Is.SameAs(supportPoint));
            Assert.That(viewModel.IsEditable, Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Constructor_SetsCorrectValues(bool isEditable)
        {
            // Call
            viewModel = new SupportPointViewModel(supportPoint, 
                                                  supportPointDataComponentViewModel, 
                                                  isEditable);

            // Assert
            Assert.That(viewModel.SupportPoint, Is.SameAs(supportPoint));
            Assert.That(viewModel.IsEditable, Is.EqualTo(isEditable));
        }

        [TestCase(false, false, 0)]
        [TestCase(false, true, 1)]
        [TestCase(true, false, 1)]
        [TestCase(true, true, 0)]
        public void SetEnabled_PropertyChangedFiredOnce(bool originalValue, bool setValue, int expectedPropChangedCount)
        {
            // Setup
            viewModel.IsEnabled = originalValue;

            // Call
            void Call() => viewModel.IsEnabled = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropChangedCount, nameof(viewModel.IsEnabled));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenADisabledSupportPointViewModel_WhenIsEnabledIsSetToTrue_ThenAParameterIsAddedToTheModel()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            var dataComponent = 
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();

            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            parametersFactory.ConstructDefaultConstantParameters<TSpreading>().Returns(parameters);

            var mediator = Substitute.For<IAnnounceSelectedSupportPointDataChanged>();
            var supportPointDataComponentViewModel = 
                new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            var supportPoint = 
                new SupportPoint(0, Substitute.For<IWaveBoundaryGeometricDefinition>());
            viewModel = new SupportPointViewModel(supportPoint, supportPointDataComponentViewModel);

            // Call
            viewModel.IsEnabled = true;

            // Assert
            Assert.That(viewModel.IsEnabled, Is.True);
            Assert.That(dataComponent.Data.ContainsKey(supportPoint), 
                        "The data component should contain the newly added SupportPoint, but did not:");
            Assert.That(dataComponent.Data[supportPoint], Is.SameAs(parameters));
            parametersFactory.Received(1).ConstructDefaultConstantParameters<TSpreading>();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAnEnabledSupportPointViewModel_WhenIsEnabledIsSetToFalse_ThenTheCorrespondingParameterIsRemovedFromTheModel()
        {
            // Setup
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var dataComponent = 
                new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = dataComponent;

            var supportPoint = 
                new SupportPoint(0, Substitute.For<IWaveBoundaryGeometricDefinition>());

            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            dataComponent.AddParameters(supportPoint, parameters);

            var mediator = Substitute.For<IAnnounceSelectedSupportPointDataChanged>();
            var supportPointDataComponentViewModel = 
                new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            viewModel = new SupportPointViewModel(supportPoint, supportPointDataComponentViewModel);

            // Call
            viewModel.IsEnabled = false;
            
            // Assert
            Assert.That(viewModel.IsEnabled, Is.False);
            Assert.That(dataComponent.Data.ContainsKey(supportPoint), Is.False, 
                        "The data component should not contain the removed SupportPoint:");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAnEnabledSupportPointViewModelWithAnUniformDataComponent_WhenIsEnabledIsSetToFalse_ThenIsEnabledIsFalseAndOnNotifyPropertyChangeIsCalled()
        {
            // Setup
            // Setup initial statement
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var initialDataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<TSpreading>>();
            conditionDefinition.DataComponent = initialDataComponent;

            var supportPoint = 
                new SupportPoint(0, Substitute.For<IWaveBoundaryGeometricDefinition>());

            var parameters = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            initialDataComponent.AddParameters(supportPoint, parameters);

            var mediator = Substitute.For<IAnnounceSelectedSupportPointDataChanged>();
            var supportPointDataComponentViewModel = 
                new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            viewModel = new SupportPointViewModel(supportPoint, supportPointDataComponentViewModel);

            // Change data component
            var parametersNew = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            conditionDefinition.DataComponent = 
                new UniformDataComponent<ConstantParameters<TSpreading>>(parametersNew);

            Assert.That(viewModel.IsEnabled, Is.True, "Precondition violated:");

            var notifyChangedObserver = 
                new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += notifyChangedObserver.OnPropertyChanged;

            // Call
            viewModel.IsEnabled = false;
            
            // Assert
            Assert.That(viewModel.IsEnabled, Is.False);
            Assert.That(notifyChangedObserver.NCalls, Is.EqualTo(1));
            Assert.That(notifyChangedObserver.Senders.First(), Is.EqualTo(viewModel));
            Assert.That(notifyChangedObserver.EventArgses.First().PropertyName, 
                        Is.EqualTo("IsEnabled"));
        }

        [Test]
        public void GivenADisabledSupportPointViewModelWithAnUniformDataComponent_WhenIsEnabledIsSetToTrue_ThenAnInvalidOperationExceptionIsThrown()
        {
            // Setup
            var parametersFactory = Substitute.For<IBoundaryParametersFactory>();
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();

            var parametersNew = new ConstantParameters<TSpreading>(0, 0, 0, new TSpreading());
            conditionDefinition.DataComponent = 
                new UniformDataComponent<ConstantParameters<TSpreading>>(parametersNew);

            var supportPoint = 
                new SupportPoint(0, Substitute.For<IWaveBoundaryGeometricDefinition>());

            var mediator = Substitute.For<IAnnounceSelectedSupportPointDataChanged>();
            var supportPointDataComponentViewModel = 
                new SupportPointDataComponentViewModel(conditionDefinition, parametersFactory, mediator);

            viewModel = new SupportPointViewModel(supportPoint, supportPointDataComponentViewModel);

            Assert.That(viewModel.IsEnabled, Is.False, "Precondition violated:");

            // Call | Assert
            void Call() => viewModel.IsEnabled = true;
            Assert.Throws<InvalidOperationException>(Call);
        }

        [TestCase(0, 0)]
        [TestCase(1E-15, 0)]
        [TestCase(1E-14, 1)]
        [TestCase(1, 1)]
        public void SetDistance_CorrectDistanceIsSetOnModelAndPropertyChangedFiredOnce(double setValueDifference,
                                                                                       int expectedPropChangedCount)
        {
            // Setup
            double originalValue = random.NextDouble();
            double setValue = originalValue + setValueDifference;
            viewModel.Distance = originalValue;

            // Call
            void Call() => viewModel.Distance = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropChangedCount, nameof(viewModel.Distance), (e) =>
            {
                var extendedEventArgs = e as PropertyChangedExtendedEventArgs;
                Assert.That(extendedEventArgs, Is.Not.Null);
                Assert.That(extendedEventArgs.OriginalValue, Is.EqualTo(originalValue));
            });
            Assert.That(supportPoint.Distance, Is.EqualTo(setValue).Within(1E-15));
        }
    }
}