using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
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
        private const double maxDistance = 10;
        private const double doublePrecision = 1E-7;
        private static readonly Random random = new Random();

        private SupportPointEditorViewModel viewModel;
        private IWaveBoundary waveBoundary;
        private IWaveBoundaryGeometricDefinition geometricDefinition;
        private IWaveBoundaryConditionDefinition conditionDefinition;

        private SupportPointDataComponentViewModel supportPointDataComponentViewModel;

        private IEventedList<SupportPoint> SupportPoints => geometricDefinition.SupportPoints;
        private ObservableCollection<SupportPointViewModel> SubViewModels => viewModel.SupportPointViewModels;

        [SetUp]
        public void SetUp()
        {
            geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(maxDistance);
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>()
            {
                new SupportPoint(0, geometricDefinition),
                new SupportPoint(maxDistance, geometricDefinition)
            });

            conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();

            waveBoundary = Substitute.For<IWaveBoundary>();
            waveBoundary.GeometricDefinition.Returns(geometricDefinition);
            waveBoundary.ConditionDefinition.Returns(conditionDefinition);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            supportPointDataComponentViewModel =
                new SupportPointDataComponentViewModel(conditionDefinition,
                                                       new ForcingTypeDefinedParametersFactory(),
                                                       mediator);

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
            Assert.That(viewModel.SelectedSupportPointViewModel, Is.SameAs(SubViewModels[0]));

            Assert.That(viewModel.AddSupportPointCommand, Is.Not.Null);
            Assert.That(viewModel.RemoveSupportPointCommand, Is.Not.Null);

            Assert.That(viewModel.IsEnabled, Is.True);
            Assert.That(viewModel, Is.InstanceOf<IRefreshIsEnabledOnDataComponentChanged>());
            Assert.That(viewModel, Is.InstanceOf<INotifyPropertyChanged>());
            Assert.That(viewModel, Is.InstanceOf<IDisposable>());
            ;
        }

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
            orderedDistances.ForEach((d, i) => Assert.That(SubViewModels[i].Distance, Is.EqualTo(d).Within(doublePrecision)));

            Assert.That(viewModel.SelectedSupportPointViewModel, Is.SameAs(SubViewModels[0]));

            Assert.That(viewModel.AddSupportPointCommand, Is.Not.Null);
            Assert.That(viewModel.RemoveSupportPointCommand, Is.Not.Null);
        }

        [Test]
        public void SetSelectedViewModel_WithOtherValue_PropertyChangedFiredOnce()
        {
            // Setup
            viewModel.SelectedSupportPointViewModel = GetSupportPointViewModel();

            // Call
            void Call() => viewModel.SelectedSupportPointViewModel = GetSupportPointViewModel();

            // Assert
            viewModel.AssertPropertyChangedFired(Call, 1, nameof(viewModel.SelectedSupportPointViewModel));
        }

        [Test]
        public void SetSelectedViewModel_WithSameValue_PropertyChangedNotFired()
        {
            // Setup
            SupportPointViewModel originalValue = GetSupportPointViewModel();
            viewModel.SelectedSupportPointViewModel = originalValue;

            // Call
            void Call() => viewModel.SelectedSupportPointViewModel = originalValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, 0, nameof(viewModel.SelectedSupportPointViewModel));
        }

        [Test]
        public void GivenAnEnabledSupportPointEditorViewModel_WhenSelectedViewModelIsChanged_ThenTheSupportPointDataComponentViewModelIsUpdated()
        {
            // Setup
            SupportPointViewModel newViewModel = GetExistingSupportPointViewModel();

            // Pre-condition
            Assert.That(supportPointDataComponentViewModel.SelectedSupportPoint,
                        Is.Not.SameAs(newViewModel.SupportPoint));

            // Call
            viewModel.SelectedSupportPointViewModel = newViewModel;

            // Assert
            Assert.That(supportPointDataComponentViewModel.SelectedSupportPoint,
                        Is.SameAs(newViewModel.SupportPoint));
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
            Assert.That(subViewModel.Distance, Is.EqualTo(value).Within(doublePrecision));
            Assert.That(SupportPoints[2], Is.SameAs(subViewModel.SupportPoint));
            Assert.That(viewModel.SelectedSupportPointViewModel, Is.SameAs(SubViewModels[0]));
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
            Assert.That(viewModel.SelectedSupportPointViewModel, Is.SameAs(SubViewModels[0]));
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
            Assert.That(viewModel.SelectedSupportPointViewModel, Is.SameAs(SubViewModels[0]));
        }

        [Test]
        public void ExecuteAddSupportPointCommand_IndexForDistanceCannotBeFound_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>
            {
                new SupportPoint(1, geometricDefinition),
                new SupportPoint(2, geometricDefinition)
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

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void ExecuteAddSupportPointCommand_MultipleTimes_AllViewModelsAreSortedOnDistance()
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
                Assert.That(vm.Distance, Is.EqualTo(orderedDistances[i]).Within(doublePrecision));
                Assert.That(SupportPoints, Contains.Item(vm.SupportPoint));
            });
        }

        [Test]
        public void ExecuteRemoveSupportPointCommand_RemovesViewModel()
        {
            // Setup
            SupportPointViewModel subViewModel = GetExistingSupportPointViewModel();
            subViewModel.IsEnabled = true;

            var dataComponent = (SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>) conditionDefinition.DataComponent;

            // Pre-condition
            Assert.That(dataComponent.Data.ContainsKey(subViewModel.SupportPoint), Is.True,
                        "DataComponent should contain data associated with the SupportPoint");

            // Call
            viewModel.RemoveSupportPointCommand.Execute(subViewModel);

            // Assert
            Assert.That(SubViewModels.Contains(subViewModel), Is.False);
            Assert.That(SupportPoints.Contains(subViewModel.SupportPoint), Is.False);

            Assert.That(dataComponent.Data.ContainsKey(subViewModel.SupportPoint), Is.False,
                        "DataComponent should not contain data associated with the removed SupportPoint");
        }

        [Test]
        public void ExecuteRemoveSupportPointCommand_OtherViewModelSelected_SelectedViewModelIsCorrect()
        {
            // Setup
            SupportPointViewModel subViewModel = GetExistingSupportPointViewModel();
            SupportPointViewModel selectedSubViewModel = SubViewModels.Last();

            viewModel.SelectedSupportPointViewModel = selectedSubViewModel;

            // Call
            viewModel.RemoveSupportPointCommand.Execute(subViewModel);

            // Assert
            Assert.That(viewModel.SelectedSupportPointViewModel, Is.SameAs(selectedSubViewModel));
            Assert.That(SubViewModels, Has.Count.EqualTo(2));
            Assert.That(SupportPoints, Has.Count.EqualTo(2));
        }

        [Test]
        public void ExecuteRemoveSupportPointCommand_OnSelectedViewModel_FirstViewModelShouldBeSelected()
        {
            // Setup
            SupportPointViewModel subViewModel = GetExistingSupportPointViewModel();
            viewModel.SelectedSupportPointViewModel = subViewModel;

            // Call
            viewModel.RemoveSupportPointCommand.Execute(subViewModel);

            // Assert
            Assert.That(viewModel.SelectedSupportPointViewModel, Is.SameAs(SubViewModels[0]));
            Assert.That(SubViewModels, Has.Count.EqualTo(2));
            Assert.That(SupportPoints, Has.Count.EqualTo(2));
        }

        [Test]
        public void SetNewDistance_NewDistanceIsSetCorrectly()
        {
            // Setup
            double newDistance = random.NextDouble();

            // Call
            viewModel.NewDistance = newDistance;

            // Assert
            Assert.That(viewModel.NewDistance, Is.EqualTo(Math.Round(newDistance, 7, MidpointRounding.AwayFromZero)));
        }

        [Test]
        public void GivenASupportPointViewModelWithAssociatedConditionData_WhenDistanceIsChanged_NewValueIsValidThenConditionDataIsReplaced()
        {
            // Setup
            double existingDistance = random.NextDouble();
            viewModel.NewDistance = existingDistance;
            viewModel.AddSupportPointCommand.Execute(existingDistance.ToString(CultureInfo.CurrentCulture));

            double nextValue = random.NextDouble();
            SupportPointViewModel originalSubViewModel = SubViewModels[1];
            originalSubViewModel.IsEnabled = true;

            // Pre-condition
            var dataComponent = (SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>) conditionDefinition.DataComponent;

            Assert.That(dataComponent.Data.ContainsKey(originalSubViewModel.SupportPoint), Is.True,
                        "Precondition violated: no data associated with the expected support point.");
            ConstantParameters<PowerDefinedSpreading> originalParameters = dataComponent.Data[originalSubViewModel.SupportPoint];

            // Call
            originalSubViewModel.Distance = nextValue;

            // Assert
            SupportPointViewModel newSubViewModel = SubViewModels[1];
            Assert.That(dataComponent.Data.ContainsKey(originalSubViewModel.SupportPoint), Is.False,
                        "Expected the replaced support point to not be present in the DataComponent");
            Assert.That(dataComponent.Data.ContainsKey(newSubViewModel.SupportPoint), Is.True,
                        "Expected the new support point to be present in the DataComponent");
            Assert.That(dataComponent.Data[newSubViewModel.SupportPoint], Is.SameAs(originalParameters));
        }

        [Test]
        public void IsEnabled_SetDifferentValue_TriggersINotifyPropertyChange()
        {
            // Setup
            var notifyPropertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
            viewModel.PropertyChanged += notifyPropertyChangedObserver.OnEventFired;

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
            var notifyPropertyChangedObserver = new EventTestObserver<PropertyChangedEventArgs>();
            viewModel.PropertyChanged += notifyPropertyChangedObserver.OnEventFired;

            bool expectedValue = viewModel.IsEnabled;

            // Call
            viewModel.IsEnabled = expectedValue;

            // Assert
            Assert.That(viewModel.IsEnabled, Is.EqualTo(expectedValue));
            Assert.That(notifyPropertyChangedObserver.NCalls, Is.EqualTo(0));
        }

        [Test]
        [TestCaseSource(nameof(GetRefreshIsEnabledData))]
        public void RefreshIsEnabled_SetsCorrectValue(ISpatiallyDefinedDataComponent dataComponent,
                                                      bool expectedValue)
        {
            // Setup
            waveBoundary.ConditionDefinition.DataComponent = dataComponent;

            // Call
            viewModel.RefreshIsEnabled();

            // Assert
            Assert.That(viewModel.IsEnabled, Is.EqualTo(expectedValue));
        }

        [Test]
        public void RefreshIsEnabled_ToDisabledState_DisablesSupportPoints()
        {
            // Setup
            viewModel.SupportPointViewModels.ForEach(x => x.IsEnabled = true);

            var dataComponent =
                new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0, 0, 0, new PowerDefinedSpreading()));
            waveBoundary.ConditionDefinition.DataComponent = dataComponent;

            // Call
            viewModel.RefreshIsEnabled();

            // Assert
            Assert.That(viewModel.SupportPointViewModels.Any(x => x.IsEnabled), Is.False,
                        "Expected all support point view models to be disabled.");
        }

        [Test]
        public void RefreshIsEnabled_ToEnabledState_SetsSelectedSupportPoint()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent =
                new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0, 0, 0, new PowerDefinedSpreading()));

            var waveBoundary = Substitute.For<IWaveBoundary>();
            waveBoundary.GeometricDefinition.Returns(geometricDefinition);
            waveBoundary.ConditionDefinition.Returns(conditionDefinition);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            supportPointDataComponentViewModel =
                new SupportPointDataComponentViewModel(conditionDefinition,
                                                       new ForcingTypeDefinedParametersFactory(),
                                                       mediator);

            var viewModel = new SupportPointEditorViewModel(geometricDefinition,
                                                            supportPointDataComponentViewModel);
            viewModel.SelectedSupportPointViewModel = GetExistingSupportPointViewModel();

            // Precondition
            Assert.That(supportPointDataComponentViewModel.SelectedSupportPoint,
                        Is.Not.SameAs(viewModel.SelectedSupportPointViewModel.SupportPoint));

            // Call
            waveBoundary.ConditionDefinition.DataComponent = new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();
            viewModel.RefreshIsEnabled();

            // Assert
            Assert.That(supportPointDataComponentViewModel.SelectedSupportPoint,
                        Is.SameAs(viewModel.SelectedSupportPointViewModel.SupportPoint));
        }

        [Test]
        public void RefreshIsEnabled_DatComponentChanged_RefreshesIsEnabledOnSupportPoints()
        {
            // Setup
            var conditionDefinition = Substitute.For<IWaveBoundaryConditionDefinition>();
            conditionDefinition.DataComponent =
                new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>();

            var waveBoundary = Substitute.For<IWaveBoundary>();
            waveBoundary.GeometricDefinition.Returns(geometricDefinition);
            waveBoundary.ConditionDefinition.Returns(conditionDefinition);

            var mediator = Substitute.For<IAnnounceSupportPointDataChanged>();
            supportPointDataComponentViewModel =
                new SupportPointDataComponentViewModel(conditionDefinition,
                                                       new ForcingTypeDefinedParametersFactory(),
                                                       mediator);

            var viewModel = new SupportPointEditorViewModel(geometricDefinition,
                                                            supportPointDataComponentViewModel);

            viewModel.SelectedSupportPointViewModel = GetExistingSupportPointViewModel();
            GetExistingSupportPointViewModel(random.Next() * 100.0, 2);

            foreach (SupportPointViewModel supportPoint in viewModel.SupportPointViewModels)
            {
                supportPoint.IsEnabled = true;
            }

            // Call
            conditionDefinition.DataComponent =
                new SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>();
            viewModel.RefreshIsEnabled();

            // Assert
            foreach (SupportPointViewModel supportPoint in viewModel.SupportPointViewModels)
            {
                Assert.That(supportPoint.IsEnabled, Is.False);
            }
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
            Assert.That(viewModel.SelectedSupportPointViewModel, Is.SameAs(SubViewModels[0]));
        }

        [TestCaseSource(nameof(GetEqualDistances))]
        public void ExecuteAddSupportPointCommand_DistanceAlreadyExists_NoViewModelIsAdded(double existingValue, double newValue)
        {
            // Setup
            SupportPointViewModel existingSubViewModel = GetExistingSupportPointViewModel(existingValue);

            viewModel.NewDistance = newValue;

            // Call
            viewModel.AddSupportPointCommand.Execute(newValue.ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(3));
            Assert.That(SupportPoints, Has.Count.EqualTo(3));

            SupportPointViewModel subViewModel = SubViewModels[1];
            Assert.That(subViewModel.Distance, Is.EqualTo(existingValue).Within(doublePrecision));
            Assert.That(subViewModel, Is.SameAs(existingSubViewModel));
            Assert.That(SupportPoints[2], Is.SameAs(existingSubViewModel.SupportPoint));
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

            viewModel.SelectedSupportPointViewModel = selectedViewModel;
            viewModel.NewDistance = newDistance;

            // Call
            viewModel.AddSupportPointCommand.Execute(newDistance.ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(5));
            Assert.That(SupportPoints, Has.Count.EqualTo(5));

            double[] orderedDistances =
            {
                0,
                1,
                2,
                3,
                maxDistance
            };

            SubViewModels.ForEach((vm, i) =>
            {
                Assert.That(vm.Distance, Is.EqualTo(orderedDistances[i]));
                Assert.That(SupportPoints, Contains.Item(vm.SupportPoint));
            });

            Assert.That(viewModel.SelectedSupportPointViewModel, Is.SameAs(selectedViewModel));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenASupportPointViewModel_WhenDistanceIsChanged_NewValueIsValid_ThenViewModelIsReplaced(int selectionIndex)
        {
            // Setup
            double existingDistance = random.NextDouble();
            viewModel.NewDistance = existingDistance;
            viewModel.AddSupportPointCommand.Execute(existingDistance.ToString(CultureInfo.CurrentCulture));

            double nextValue = random.NextDouble();
            SupportPointViewModel originalSubViewModel = SubViewModels[1];

            viewModel.SelectedSupportPointViewModel = SubViewModels[selectionIndex];

            // Call
            originalSubViewModel.Distance = nextValue;

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(3));
            Assert.That(viewModel.SelectedSupportPointViewModel, Is.SameAs(SubViewModels[selectionIndex]));

            SupportPointViewModel subViewModel = SubViewModels[1];
            Assert.That(subViewModel, Is.Not.SameAs(originalSubViewModel));
            Assert.That(subViewModel.Distance, Is.EqualTo(nextValue).Within(doublePrecision));
        }

        [TestCaseSource(nameof(GetEqualDistances))]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenASupportPointViewModel_WhenDistanceIsChanged_NewValueAlreadyExists_ThenViewModelIsNotReplacedAndHasOriginalValue(
            double existingDistance, double newDistance)
        {
            // Setup
            GetExistingSupportPointViewModel(existingDistance);

            SupportPointViewModel[] existingSubViewModels = SubViewModels.ToArray();

            double originalValue = random.NextDouble();
            viewModel.NewDistance = originalValue;
            viewModel.AddSupportPointCommand.Execute(originalValue.ToString(CultureInfo.CurrentCulture));

            SupportPointViewModel originalSubViewModel = SubViewModels.Single(m => !existingSubViewModels.Contains(m));

            // Call
            originalSubViewModel.Distance = newDistance;

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(4));

            SupportPointViewModel subViewModel = SubViewModels.Single(m => !existingSubViewModels.Contains(m));
            Assert.That(subViewModel, Is.SameAs(originalSubViewModel));
            Assert.That(subViewModel.Distance, Is.EqualTo(originalValue).Within(doublePrecision));
        }

        private static IEnumerable<TestCaseData> GetRefreshIsEnabledData()
        {
            yield return new TestCaseData(new SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>(), true);
            yield return new TestCaseData(new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(new ConstantParameters<PowerDefinedSpreading>(0, 0, 0, new PowerDefinedSpreading())), false);
        }

        private SupportPointViewModel GetExistingSupportPointViewModel()
        {
            return GetExistingSupportPointViewModel(random.NextDouble());
        }

        private SupportPointViewModel GetExistingSupportPointViewModel(double distance, int index = 1)
        {
            SupportPoint supportPoint = GetSupportPoint(distance);
            var subViewModel = new SupportPointViewModel(supportPoint, supportPointDataComponentViewModel);

            viewModel.SupportPointViewModels.Insert(index, subViewModel);
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

        private static IEnumerable<double[]> GetEqualDistances()
        {
            double value = random.NextDouble();
            yield return new[]
            {
                value,
                value
            };
            yield return new[]
            {
                value,
                Math.Round(value, 7, MidpointRounding.AwayFromZero)
            };
            yield return new[]
            {
                0.1234567,
                0.12345665001
            };
            yield return new[]
            {
                0.1234567,
                0.12345674999
            };
        }
    }
}