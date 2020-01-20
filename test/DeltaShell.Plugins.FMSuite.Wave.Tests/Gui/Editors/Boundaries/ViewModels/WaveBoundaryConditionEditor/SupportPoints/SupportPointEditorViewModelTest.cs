using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
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
        private IWaveBoundaryGeometricDefinition geometricDefinition;

        private IEventedList<SupportPoint> SupportPoints => geometricDefinition.SupportPoints;
        private ObservableCollection<SupportPointViewModel> SubViewModels => viewModel.ViewModels;

        [SetUp]
        public void SetUp()
        {
            geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.Length.Returns(maxDistance);
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>());
            viewModel = new SupportPointEditorViewModel(geometricDefinition);
        }

        [TearDown]
        public void TearDown()
        {
            viewModel.Dispose();
        }

        [Test]
        public void Constructor_GeometricDefinitionNull_ThrownArgumentNullException()
        {
            // Call
            void Call() => new SupportPointEditorViewModel(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("geometricDefinition"));
        }

        [Test]
        public void Constructor_SetsCorrectValues()
        {
            // Assert
            Assert.That(SubViewModels, Is.Not.Null);
            Assert.That(SubViewModels, Is.Empty);
            Assert.That(viewModel.SelectedViewModel, Is.Null);

            Assert.That(viewModel.AddSupportPointCommand, Is.Not.Null);
            Assert.That(viewModel.RemoveSupportPointCommand, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithGeometricDefinitionWithSupportPoints_SetsCorrectValues()
        {
            // Setup
            double[] distances =
            {
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble(),
                random.NextDouble()
            };
            distances.ForEach(d => geometricDefinition.SupportPoints.Add(GetSupportPoint(d)));

            // Call
            viewModel = new SupportPointEditorViewModel(geometricDefinition);

            // Assert
            Assert.That(SubViewModels, Is.Not.Null);
            Assert.That(SubViewModels, Has.Count.EqualTo(5));

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
            Assert.That(SubViewModels, Has.Count.EqualTo(1));
            Assert.That(SupportPoints, Has.Count.EqualTo(1));
            SupportPointViewModel subViewModel = SubViewModels[0];
            Assert.That(subViewModel.Distance, Is.EqualTo(value).Within(1E-15));
            Assert.That(SupportPoints[0], Is.SameAs(subViewModel.SupportPoint));
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(subViewModel));
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
            Assert.That(SubViewModels, Has.Count.EqualTo(0));
            Assert.That(SupportPoints, Has.Count.EqualTo(0));
            Assert.That(viewModel.SelectedViewModel, Is.Null);
        }

        [Test]
        public void ExecuteAddSupportPointCommand_WithNewDistanceExceedingUpperLimit_NoViewModelIsAdded()
        {
            // Setup
            viewModel.NewDistance = maxDistance + random.NextDouble();

            // Call
            viewModel.AddSupportPointCommand.Execute(random.NextDouble().ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(0));
            Assert.That(SupportPoints, Has.Count.EqualTo(0));
            Assert.That(viewModel.SelectedViewModel, Is.Null);
        }

        [Test]
        public void ExecuteAddSupportPointCommand_WithNewDistanceExceedingLowerLimit_NoViewModelIsAdded()
        {
            // Setup
            viewModel.NewDistance = -random.NextDouble();

            // Call
            viewModel.AddSupportPointCommand.Execute(random.NextDouble().ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(0));
            Assert.That(SupportPoints, Has.Count.EqualTo(0));
            Assert.That(viewModel.SelectedViewModel, Is.Null);
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
            Assert.That(SubViewModels, Has.Count.EqualTo(1));
            Assert.That(SupportPoints, Has.Count.EqualTo(1));

            SupportPointViewModel subViewModel = SubViewModels[0];
            Assert.That(subViewModel.Distance, Is.EqualTo(value).Within(1E-15));
            Assert.That(subViewModel, Is.SameAs(existingSubViewModel));
            Assert.That(SupportPoints[0], Is.SameAs(existingSubViewModel.SupportPoint));
        }

        [TestCase(0, 1, 2)]
        [TestCase(0, 2, 1)]
        [TestCase(1, 0, 2)]
        [TestCase(1, 2, 0)]
        [TestCase(2, 0, 1)]
        [TestCase(2, 1, 0)]
        public void ExecuteAddSupportPointCommand_NewViewModelIsInsertedAtCorrectIndex(double existingDistanceA,
                                                                                       double existingDistanceB,
                                                                                       double newDistance)
        {
            // Setup
            double smallestDistance = Math.Min(existingDistanceA, existingDistanceB);
            GetExistingSupportPointViewModel(smallestDistance);

            double largestDistance = Math.Max(existingDistanceA, existingDistanceB);
            GetExistingSupportPointViewModel(largestDistance);

            viewModel.NewDistance = newDistance;

            // Call
            viewModel.AddSupportPointCommand.Execute(newDistance.ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(3));
            Assert.That(SupportPoints, Has.Count.EqualTo(3));

            double[] orderedDistances = {0, 1, 2};

            SubViewModels.ForEach((vm, i) =>
            {
                Assert.That(vm.Distance, Is.EqualTo(orderedDistances[i]));
                Assert.That(SupportPoints, Contains.Item(vm.SupportPoint));
            });
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ExecuteAddSupportPointCommand_MultipleTimes_AllViewModelsAreSortedOnDistance()
        {
            // Setup
            double[] distances =
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
            Assert.That(SubViewModels, Has.Count.EqualTo(5));
            Assert.That(SupportPoints, Has.Count.EqualTo(5));

            double[] orderedDistances = distances.OrderBy(d => d).ToArray();

            SubViewModels.ForEach((vm, i) =>
            {
                Assert.That(vm.Distance, Is.EqualTo(orderedDistances[i]));
                Assert.That(SupportPoints, Contains.Item(vm.SupportPoint));
            });
        }

        #region Remove command

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
        public void ExecuteRemoveSupportPointCommand_OnOnlyViewModel_SelectedViewModelIsCorrect()
        {
            // Setup
            SupportPointViewModel subViewModel = GetExistingSupportPointViewModel();

            // Call
            viewModel.RemoveSupportPointCommand.Execute(subViewModel);

            // Assert
            Assert.That(viewModel.SelectedViewModel, Is.Null);
            Assert.That(SubViewModels, Has.Count.EqualTo(0));
            Assert.That(SupportPoints, Has.Count.EqualTo(0));
        }

        [Test]
        public void ExecuteRemoveSupportPointCommand_OnSelectedViewModel_FirstViewModelShouldBeSelected()
        {
            // Setup
            SupportPointViewModel firstSubViewModel = GetExistingSupportPointViewModel();
            GetExistingSupportPointViewModel();

            SupportPointViewModel subViewModel = GetExistingSupportPointViewModel();
            viewModel.SelectedViewModel = subViewModel;

            // Call
            viewModel.RemoveSupportPointCommand.Execute(subViewModel);

            // Assert
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(firstSubViewModel));
            Assert.That(SubViewModels, Has.Count.EqualTo(2));
            Assert.That(SupportPoints, Has.Count.EqualTo(2));
        }

        [TestCase(0)]
        [TestCase(maxDistance)]
        public void ExecuteRemoveSupportPointCommand_OnEndPointViewModel_ViewModelShouldNotBeDeleted(double distance)
        {
            // Setup
            SupportPointViewModel subViewModel = GetExistingSupportPointViewModel(distance);

            // Call
            viewModel.RemoveSupportPointCommand.Execute(subViewModel);

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(1));
            Assert.That(SupportPoints, Has.Count.EqualTo(1));
            Assert.That(SubViewModels[0], Is.SameAs(subViewModel));
            Assert.That(SupportPoints[0], Is.SameAs(subViewModel.SupportPoint));
        }

        #endregion Remove command

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenASupportPointViewModel_WhenDistanceIsChanged_NewValueIsValid_ThenViewModelIsReplaced()
        {
            // Setup
            double existingDistance = random.NextDouble();
            viewModel.NewDistance = existingDistance;
            viewModel.AddSupportPointCommand.Execute(existingDistance.ToString(CultureInfo.CurrentCulture));

            double nextValue = random.NextDouble();
            SupportPointViewModel originalSubViewModel = SubViewModels[0];

            // Call
            originalSubViewModel.Distance = nextValue;

            // Assert
            Assert.That(SubViewModels.HasExactlyOneValue());

            SupportPointViewModel subViewModel = SubViewModels[0];
            Assert.That(subViewModel, Is.Not.SameAs(originalSubViewModel));
            Assert.That(subViewModel.Distance, Is.EqualTo(nextValue));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenASupportPointViewModel_WhenDistanceIsChanged_NewValueAlreadyExists_ThenViewModelIsNotReplacedAndHasOriginalValue()
        {
            // Setup
            double existingDistance = random.NextDouble();
            viewModel.NewDistance = existingDistance;
            viewModel.AddSupportPointCommand.Execute(existingDistance.ToString(CultureInfo.CurrentCulture));

            SupportPointViewModel existingSubViewModel = SubViewModels[0];

            double originalValue = random.NextDouble();
            viewModel.NewDistance = originalValue;
            viewModel.AddSupportPointCommand.Execute(originalValue.ToString(CultureInfo.CurrentCulture));

            SupportPointViewModel originalSubViewModel = SubViewModels.Single(m => m != existingSubViewModel);

            // Call
            originalSubViewModel.Distance = existingDistance;

            // Assert
            Assert.That(SubViewModels, Has.Count.EqualTo(2));

            SupportPointViewModel subViewModel = SubViewModels.Single(m => m != existingSubViewModel);
            Assert.That(subViewModel, Is.SameAs(originalSubViewModel));
            Assert.That(subViewModel.Distance, Is.EqualTo(originalValue));
        }

        private SupportPointViewModel GetExistingSupportPointViewModel()
        {
            return GetExistingSupportPointViewModel(random.NextDouble());
        }

        private SupportPointViewModel GetExistingSupportPointViewModel(double distance)
        {
            SupportPoint supportPoint = GetSupportPoint(distance);
            var subViewModel = new SupportPointViewModel(supportPoint);

            viewModel.ViewModels.Add(subViewModel);
            geometricDefinition.SupportPoints.Add(supportPoint);

            return subViewModel;
        }

        private SupportPointViewModel GetSupportPointViewModel()
        {
            return new SupportPointViewModel(GetSupportPoint(random.NextDouble()));
        }

        private SupportPoint GetSupportPoint(double distance)
        {
            return new SupportPoint(distance, geometricDefinition);
        }
    }
}