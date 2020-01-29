using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints
{
    [TestFixture]
    public class SupportPointEditorViewModelTest
    {
        private readonly Random random = new Random();
        private const double maxDistance = 10;

        private SupportPointEditorViewModel viewModel;
        private IWaveBoundary waveBoundary;
        private IWaveBoundaryGeometricDefinition geometricDefinition;

        private SupportPointDataComponentViewModel supportPointDataComponentViewModel;

        private IEventedList<SupportPoint> SupportPoints => geometricDefinition.SupportPoints;
        private ObservableCollection<SupportPointViewModel> SubViewModels => viewModel.ViewModels;

        [SetUp]
        public void SetUp()
        {
            geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(maxDistance);
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>()
            {
                new SupportPoint(0, geometricDefinition),
                new SupportPoint(maxDistance, geometricDefinition),
            });

            waveBoundary = Substitute.For<IWaveBoundary>();
            waveBoundary.GeometricDefinition.Returns(geometricDefinition);

            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent = 
                new SpatiallyVaryingDataComponent<ConstantParameters>();

            supportPointDataComponentViewModel = 
                new SupportPointDataComponentViewModel(conditionDefinition,
                                                       new BoundaryParametersFactory());

            viewModel = new SupportPointEditorViewModel(geometricDefinition, 
                                                        supportPointDataComponentViewModel);
        }

        [TearDown]
        public void TearDown()
        {
            viewModel.Dispose();
        }

        [Test]
        public void Constructor_GeometricDefinitionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SupportPointEditorViewModel(null, supportPointDataComponentViewModel);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("geometricDefinition"));
        }

        [Test]
        public void Constructor_SupportPointDataComponentViewModel_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SupportPointEditorViewModel(geometricDefinition, 
                                                           null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("supportPointDataComponentViewModel"));
        }

        [Test]
        public void Constructor_SetsCorrectValues()
        {
            // Assert
            Assert.That(SubViewModels, Is.Not.Null);
            Assert.That(SubViewModels, Has.Count.EqualTo(2));
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(SubViewModels[0]));

            Assert.That(viewModel.AddSupportPointCommand, Is.Not.Null);
            Assert.That(viewModel.RemoveSupportPointCommand, Is.Not.Null);

            Assert.That(viewModel, Is.InstanceOf<IRefreshIsEnabledOnDataComponentChanged>());
            Assert.That(viewModel.IsEnabled, Is.True);
;        }

        [Test]
        public void Constructor_WithGeometricDefinitionWithSupportPoints_SetsCorrectValues()
        {
            // Setup
            IList<double> distances = new List<double>()
            {
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble()
            };
            distances.ForEach(d => geometricDefinition.SupportPoints.Add(GetSupportPoint(d)));

            // Call
            viewModel = new SupportPointEditorViewModel(geometricDefinition,
                                                        supportPointDataComponentViewModel);

            // Assert
            Assert.That(SubViewModels, Is.Not.Null);
            Assert.That(SubViewModels, Has.Count.EqualTo(7));

            distances.Add(0);
            distances.Add(maxDistance);

            double[] orderedDistances = distances.OrderBy(d => d).ToArray();
            orderedDistances.ForEach((d, i) => Assert.That(SubViewModels[i].Distance, Is.EqualTo(d)));

            Assert.That(viewModel.SelectedViewModel, Is.SameAs(SubViewModels[0]));

            Assert.That(viewModel.AddSupportPointCommand, Is.Not.Null);
            Assert.That(viewModel.RemoveSupportPointCommand, Is.Not.Null);
        }

        [Test]
        public void SetSelectedViewModel_WithOtherValue_PropertyChangedFiredOnce()
        {
            // Setup
            viewModel.SelectedViewModel = GetSupportPointViewModel();

            // Call
            void Call() => viewModel.SelectedViewModel = GetSupportPointViewModel();

            // Assert
            viewModel.AssertPropertyChangedFired(Call, 1, nameof(viewModel.SelectedViewModel));
        }

        [Test]
        public void SetSelectedViewModel_WithSameValue_PropertyChangedNotFired()
        {
            // Setup
            SupportPointViewModel originalValue = GetSupportPointViewModel();
            viewModel.SelectedViewModel = originalValue;

            // Call
            void Call() => viewModel.SelectedViewModel = originalValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, 0, nameof(viewModel.SelectedViewModel));
        }

        [Test]
        public void ExecuteAddSupportPointCommand_CorrectViewModelIsAdded()
        {
            // Setup
            double value = random.NextDouble();
            viewModel.NewDistance = value;

            // Call
            viewModel.AddSupportPointCommand.Execute(value.ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(3));
            Assert.That(SupportPoints, Has.Count.EqualTo(3));
            SupportPointViewModel subViewModel = SubViewModels[1];
            Assert.That(subViewModel.Distance, Is.EqualTo(value).Within(1E-15));
            Assert.That(SupportPoints[2], Is.SameAs(subViewModel.SupportPoint));
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(SubViewModels[0]));
        }

        [TestCase("-1.0")]
        [TestCase("NaN")]
        [TestCase("One")]
        public void ExecuteAddSupportPointCommand_WithInvalidValue_NoViewModelIsAdded(object invalidValue)
        {
            viewModel.NewDistance = random.NextDouble();

            // Call
            viewModel.AddSupportPointCommand.Execute(invalidValue);

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(2));
            Assert.That(SupportPoints, Has.Count.EqualTo(2));
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(SubViewModels[0]));
        }

        [Test]
        public void ExecuteAddSupportPointCommand_WithNewDistanceExceedingUpperLimit_NoViewModelIsAdded()
        {
            // Setup
            viewModel.NewDistance = maxDistance + random.NextDouble();

            // Call
            viewModel.AddSupportPointCommand.Execute(random.NextDouble().ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(2));
            Assert.That(SupportPoints, Has.Count.EqualTo(2));
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(SubViewModels[0]));
        }

        [Test]
        public void ExecuteAddSupportPointCommand_WithNewDistanceExceedingLowerLimit_NoViewModelIsAdded()
        {
            // Setup
            viewModel.NewDistance = -random.NextDouble();

            // Call
            viewModel.AddSupportPointCommand.Execute(random.NextDouble().ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(2));
            Assert.That(SupportPoints, Has.Count.EqualTo(2));
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(SubViewModels[0]));
        }

        [Test]
        public void ExecuteAddSupportPointCommand_DistanceAlreadyExists_NoViewModelIsAdded()
        {
            // Setup
            double value = random.NextDouble();
            SupportPointViewModel existingSubViewModel = GetExistingSupportPointViewModel(value);

            viewModel.NewDistance = value;

            // Call
            viewModel.AddSupportPointCommand.Execute(value.ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(3));
            Assert.That(SupportPoints, Has.Count.EqualTo(3));

            SupportPointViewModel subViewModel = SubViewModels[1];
            Assert.That(subViewModel.Distance, Is.EqualTo(value).Within(1E-15));
            Assert.That(subViewModel, Is.SameAs(existingSubViewModel));
            Assert.That(SupportPoints[2], Is.SameAs(existingSubViewModel.SupportPoint));
        }

        [Test]
        public void ExecuteAddSupportPointCommand_IndexForDistanceCannotBeFound_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>
            {
                new SupportPoint(1, geometricDefinition),
                new SupportPoint(2, geometricDefinition),
            });
            viewModel = new SupportPointEditorViewModel(geometricDefinition,
                                                        supportPointDataComponentViewModel);

            double value = random.NextDouble();
            viewModel.NewDistance = value;

            // Call
            void Call() => viewModel.AddSupportPointCommand.Execute(value.ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.Throws<ArgumentOutOfRangeException>(Call);
        }

        [TestCase(1, 2, 3)]
        [TestCase(1, 3, 2)]
        [TestCase(2, 1, 3)]
        [TestCase(2, 3, 1)]
        [TestCase(3, 1, 2)]
        [TestCase(3, 2, 1)]
        public void ExecuteAddSupportPointCommand_NewViewModelIsInsertedAtCorrectIndexAndSelectionDoesNotChange(double existingDistanceA,
                                                                                                                double existingDistanceB,
                                                                                                                double newDistance)
        {
            // Setup
            double smallestDistance = Math.Min(existingDistanceA, existingDistanceB);
            GetExistingSupportPointViewModel(smallestDistance, 1);

            double largestDistance = Math.Max(existingDistanceA, existingDistanceB);
            SupportPointViewModel selectedViewModel = GetExistingSupportPointViewModel(largestDistance, 2);

            viewModel.SelectedViewModel = selectedViewModel;
            viewModel.NewDistance = newDistance;

            // Call
            viewModel.AddSupportPointCommand.Execute(newDistance.ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(5));
            Assert.That(SupportPoints, Has.Count.EqualTo(5));

            double[] orderedDistances = {0, 1, 2, 3, maxDistance};

            SubViewModels.ForEach((vm, i) =>
            {
                Assert.That(vm.Distance, Is.EqualTo(orderedDistances[i]));
                Assert.That(SupportPoints, Contains.Item(vm.SupportPoint));
            });

            Assert.That(viewModel.SelectedViewModel, Is.SameAs(selectedViewModel));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ExecuteAddSupportPointCommand_MultipleTimes_AllViewModelsAreSortedOnDistance()
        {
            // Setup
            IList<double> distances = new List<double>()
            {
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble(),
            };

            // Calls
            distances.ForEach(d =>
            {
                viewModel.NewDistance = d;
                viewModel.AddSupportPointCommand.Execute(d.ToString(CultureInfo.CurrentCulture));
            });

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(7));
            Assert.That(SupportPoints, Has.Count.EqualTo(7));

            distances.Add(0);
            distances.Add(maxDistance);

            double[] orderedDistances = distances.OrderBy(d => d).ToArray();

            SubViewModels.ForEach((vm, i) =>
            {
                Assert.That(vm.Distance, Is.EqualTo(orderedDistances[i]));
                Assert.That(SupportPoints, Contains.Item(vm.SupportPoint));
            });
        }

        [Test]
        public void ExecuteRemoveSupportPointCommand_RemovesViewModel()
        {
            // Setup
            SupportPointViewModel subViewModel = GetExistingSupportPointViewModel();

            // Call
            viewModel.RemoveSupportPointCommand.Execute(subViewModel);

            // Assert
            Assert.That(SubViewModels.Contains(subViewModel), Is.False);
            Assert.That(SupportPoints.Contains(subViewModel.SupportPoint), Is.False);
        }

        [Test]
        public void ExecuteRemoveSupportPointCommand_OtherViewModelSelected_SelectedViewModelIsCorrect()
        {
            // Setup
            SupportPointViewModel subViewModel = GetExistingSupportPointViewModel();
            SupportPointViewModel selectedSubViewModel = SubViewModels.Last();

            viewModel.SelectedViewModel = selectedSubViewModel;

            // Call
            viewModel.RemoveSupportPointCommand.Execute(subViewModel);

            // Assert
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(selectedSubViewModel));
            Assert.That(SubViewModels, Has.Count.EqualTo(2));
            Assert.That(SupportPoints, Has.Count.EqualTo(2));
        }

        [Test]
        public void ExecuteRemoveSupportPointCommand_OnSelectedViewModel_FirstViewModelShouldBeSelected()
        {
            // Setup
            SupportPointViewModel subViewModel = GetExistingSupportPointViewModel();
            viewModel.SelectedViewModel = subViewModel;

            // Call
            viewModel.RemoveSupportPointCommand.Execute(subViewModel);

            // Assert
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(SubViewModels[0]));
            Assert.That(SubViewModels, Has.Count.EqualTo(2));
            Assert.That(SupportPoints, Has.Count.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [Category(TestCategory.Integration)]
        public void GivenASupportPointViewModel_WhenDistanceIsChanged_NewValueIsValid_ThenViewModelIsReplaced(int selectionIndex)
        {
            // Setup
            double existingDistance = random.NextDouble();
            viewModel.NewDistance = existingDistance;
            viewModel.AddSupportPointCommand.Execute(existingDistance.ToString(CultureInfo.CurrentCulture));

            double nextValue = random.NextDouble();
            SupportPointViewModel originalSubViewModel = SubViewModels[1];

            viewModel.SelectedViewModel = SubViewModels[selectionIndex];

            // Call
            originalSubViewModel.Distance = nextValue;

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(3));
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(SubViewModels[selectionIndex]));

            SupportPointViewModel subViewModel = SubViewModels[1];
            Assert.That(subViewModel, Is.Not.SameAs(originalSubViewModel));
            Assert.That(subViewModel.Distance, Is.EqualTo(nextValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenASupportPointViewModel_WhenDistanceIsChanged_NewValueAlreadyExists_ThenViewModelIsNotReplacedAndHasOriginalValue()
        {
            // Setup
            double existingDistance = random.NextDouble();
            GetExistingSupportPointViewModel(existingDistance);

            SupportPointViewModel[] existingSubViewModels = SubViewModels.ToArray();

            double originalValue = random.NextDouble();
            viewModel.NewDistance = originalValue;
            viewModel.AddSupportPointCommand.Execute(originalValue.ToString(CultureInfo.CurrentCulture));

            SupportPointViewModel originalSubViewModel = SubViewModels.Single(m => !existingSubViewModels.Contains(m));

            // Call
            originalSubViewModel.Distance = existingDistance;

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(4));

            SupportPointViewModel subViewModel = SubViewModels.Single(m => !existingSubViewModels.Contains(m));
            Assert.That(subViewModel, Is.SameAs(originalSubViewModel));
            Assert.That(subViewModel.Distance, Is.EqualTo(originalValue));
        }

        [Test]
        public void IsEnabled_SetDifferentValue_TriggersINotifyPropertyChange()
        {
            // Setup
            var notifyPropertyChangedObserver = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += notifyPropertyChangedObserver.OnPropertyChanged;

            bool expectedValue = !viewModel.IsEnabled;

            // Call
            viewModel.IsEnabled = expectedValue;

            // Assert
            Assert.That(viewModel.IsEnabled, Is.EqualTo(expectedValue));
            Assert.That(notifyPropertyChangedObserver.NCalls, Is.EqualTo(1));
            Assert.That(notifyPropertyChangedObserver.Senders.First(), Is.EqualTo(viewModel));
            Assert.That(notifyPropertyChangedObserver.EventArgses.First().PropertyName, 
                        Is.EqualTo(nameof(viewModel.IsEnabled)));
        }

        [Test]
        public void IsEnabled_SetSameValue_DoesNotTriggerINotifyPropertyChange()
        {
            // Setup
            var notifyPropertyChangedObserver = new NotifyPropertyChangedTestObserver();
            viewModel.PropertyChanged += notifyPropertyChangedObserver.OnPropertyChanged;

            bool expectedValue = viewModel.IsEnabled;

            // Call
            viewModel.IsEnabled = expectedValue;

            // Assert
            Assert.That(viewModel.IsEnabled, Is.EqualTo(expectedValue));
            Assert.That(notifyPropertyChangedObserver.NCalls, Is.EqualTo(0));
        }

        private static IEnumerable<TestCaseData> GetRefreshIsEnabledData()
        {
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters>(), true);
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters>(new ConstantParameters(0, 0, 0, 0)), false);
        }

        [Test]
        [TestCaseSource(nameof(GetRefreshIsEnabledData))]
        public void RefreshIsEnabled_SetsCorrectValue(IBoundaryConditionDataComponent dataComponent,
                                                      bool expectedValue)
        {
            // Setup
            waveBoundary.ConditionDefinition.DataComponent = dataComponent;

            // Call
            viewModel.RefreshIsEnabled();

            // Assert
            Assert.That(viewModel.IsEnabled, Is.EqualTo(expectedValue));
        }

        private SupportPointViewModel GetExistingSupportPointViewModel()
        {
            return GetExistingSupportPointViewModel(random.NextDouble());
        }

        private SupportPointViewModel GetExistingSupportPointViewModel(double distance, int index = 1)
        {
            SupportPoint supportPoint = GetSupportPoint(distance);
            var subViewModel = new SupportPointViewModel(supportPoint, supportPointDataComponentViewModel);

            viewModel.ViewModels.Insert(index, subViewModel);
            geometricDefinition.SupportPoints.Add(supportPoint);

            return subViewModel;
        }

        private SupportPointViewModel GetSupportPointViewModel()
        {
            return new SupportPointViewModel(GetSupportPoint(random.NextDouble()), supportPointDataComponentViewModel);
        }

        private SupportPoint GetSupportPoint(double distance)
        {
            return new SupportPoint(distance, geometricDefinition);
        }
    }
}