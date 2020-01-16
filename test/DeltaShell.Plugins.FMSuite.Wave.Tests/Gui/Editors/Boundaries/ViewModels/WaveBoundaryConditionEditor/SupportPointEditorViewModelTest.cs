using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    [TestFixture]
    public class SupportPointEditorViewModelTest
    {
        private readonly Random random = new Random();
        private SupportPointEditorViewModel viewModel;
        private IWaveBoundaryGeometricDefinition geometricDefinition;

        private IEventedList<SupportPoint> SupportPoints => geometricDefinition.SupportPoints;
        private ObservableCollection<SupportPointViewModel> SubViewModels => viewModel.ViewModels;

        [SetUp]
        public void SetUp()
        {
            geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>());
            viewModel = new SupportPointEditorViewModel(geometricDefinition);
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

        [TestCaseSource(nameof(SelectedViewModelCases))]
        public void SetSelectedViewModel_WithOtherValue_PropertyChangedFiredOnce(SupportPointViewModel originalValue,
                                                                                 SupportPointViewModel setValue,
                                                                                 int expectedPropertyChangedCount)
        {
            // Setup
            viewModel.SelectedViewModel = originalValue;

            // Call
            void Call() => viewModel.SelectedViewModel = setValue;

            // Assert
            viewModel.AssertPropertyChangedFired(Call, expectedPropertyChangedCount,
                                                 nameof(viewModel.SelectedViewModel));
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
            Assert.That(SubViewModels.Count, Is.EqualTo(1));
            Assert.That(SubViewModels[0].Distance, Is.EqualTo(value).Within(1E-15));
        }

        [TestCase("-1.0")]
        [TestCase("NaN")]
        [TestCase("One")]
        public void ExecuteAddSupportPointCommand_WithInvalidValue_NoViewModelIsAdded(object invalidValue)
        {
            // Call
            viewModel.AddSupportPointCommand.Execute(invalidValue);

            // Assert
            Assert.That(SubViewModels.Count, Is.EqualTo(0));
        }

        [Test]
        public void ExecuteAddSupportPointCommand_WhenViewModelWithSameDistanceAlreadyExists_NoViewModelIsAdded()
        {
            // Setup
            double value = random.NextDouble();
            viewModel.ViewModels.Add(GetSupportPointViewModel(value));
            viewModel.NewDistance = value;

            // Call
            viewModel.AddSupportPointCommand.Execute(value.ToString(CultureInfo.CurrentCulture));

            // Assert
            Assert.That(SubViewModels.Count, Is.EqualTo(1));
            Assert.That(SubViewModels[0].Distance, Is.EqualTo(value).Within(1E-15));
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
            IEnumerable<SupportPointViewModel> existingViewModels = new[]
                {
                    existingDistanceA,
                    existingDistanceB
                }.OrderBy(d => d)
                 .Select(GetSupportPointViewModel)
                 .ToArray();
            viewModel.ViewModels.AddRange(existingViewModels);

            viewModel.NewDistance = newDistance;

            // Call
            viewModel.AddSupportPointCommand.Execute(newDistance.ToString(CultureInfo.CurrentCulture));

            // Assert
            double[] orderedDistances = {0, 1, 2};
            orderedDistances.ForEach((d, i) => Assert.That(SubViewModels[i].Distance, Is.EqualTo(d)));

            Assert.That(SupportPoints, Has.Count.EqualTo(3));
            SubViewModels.ForEach(vm => Assert.That(SupportPoints, Contains.Item(vm.SupportPoint)));
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
            double[] orderedDistances = distances.OrderBy(d => d).ToArray();
            orderedDistances.ForEach((d, i) => Assert.That(SubViewModels[i].Distance, Is.EqualTo(d)));

            Assert.That(SupportPoints, Has.Count.EqualTo(5));
            SubViewModels.ForEach(vm => Assert.That(SupportPoints, Contains.Item(vm.SupportPoint)));
        }

        [Test]
        public void ExecuteRemoveSupportPointCommand_RemovesViewModel()
        {
            // Setup
            SupportPointViewModel subViewModel = GetSupportPointViewModel();
            viewModel.ViewModels.Add(subViewModel);

            // Call
            viewModel.RemoveSupportPointCommand.Execute(subViewModel);

            // Assert
            Assert.That(SubViewModels.Contains(subViewModel), Is.False);
        }

        [Test]
        public void AddFirstViewModel_SupportPointIsAddedToGeometricDefinitionAndSelectionIsCorrect()
        {
            SupportPointViewModel newSubViewModel = GetSupportPointViewModel();

            // Call
            viewModel.ViewModels.Add(newSubViewModel);

            // Assert

            Assert.That(SupportPoints, Has.Count.EqualTo(1));
            Assert.That(SupportPoints, Contains.Item(newSubViewModel.SupportPoint));
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(newSubViewModel));
        }

        [Test]
        public void AddSecondViewModel_SelectedViewModelShouldNotChange()
        {
            // Setup
            SupportPointViewModel firstSubViewModel = GetSupportPointViewModel();
            viewModel.ViewModels.Add(firstSubViewModel);

            // Precondition
            Assert.That(viewModel.SelectedViewModel, Is.EqualTo(firstSubViewModel));

            SupportPointViewModel newSubViewModel = GetSupportPointViewModel();

            // Call
            viewModel.ViewModels.Add(newSubViewModel);

            // Assert
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(firstSubViewModel));

            Assert.That(SupportPoints, Has.Count.EqualTo(2));
            Assert.That(SupportPoints, Contains.Item(firstSubViewModel.SupportPoint));
            Assert.That(SupportPoints, Contains.Item(newSubViewModel.SupportPoint));
        }

        [Test]
        public void RemoveOnlyViewModel_SupportPointIsRemovedFromGeometricDefinitionAndSelectionIsCorrect()
        {
            // Setup
            SupportPointViewModel subViewModel = GetSupportPointViewModel();
            viewModel.ViewModels.Add(subViewModel);

            // Call
            viewModel.ViewModels.Remove(subViewModel);

            // Assert
            Assert.That(SupportPoints, Has.Count.EqualTo(0));
            Assert.That(viewModel.SelectedViewModel, Is.Null);
        }

        [Test]
        public void RemoveSelectedViewModel_FirstViewModelShouldBeSelected()
        {
            // Setup
            SupportPointViewModel firstSubViewModel = GetSupportPointViewModel();
            viewModel.ViewModels.Add(firstSubViewModel);
            SupportPointViewModel subViewModel = GetSupportPointViewModel();
            viewModel.ViewModels.Add(subViewModel);

            viewModel.SelectedViewModel = subViewModel;

            // Call
            viewModel.ViewModels.Remove(subViewModel);

            // Assert
            Assert.That(SupportPoints, Has.Count.EqualTo(1));
            Assert.That(SupportPoints.Contains(firstSubViewModel.SupportPoint));
            Assert.That(viewModel.SelectedViewModel, Is.SameAs(firstSubViewModel));
        }

        private SupportPointViewModel GetSupportPointViewModel(double distance)
        {
            return new SupportPointViewModel(GetSupportPoint(distance));
        }

        private SupportPointViewModel GetSupportPointViewModel()
        {
            return new SupportPointViewModel(GetSupportPoint(random.NextDouble()));
        }

        private SupportPoint GetSupportPoint(double distance)
        {
            return new SupportPoint(distance, geometricDefinition);
        }

        private IEnumerable<TestCaseData> SelectedViewModelCases()
        {
            SupportPointViewModel subViewModel = GetSupportPointViewModel();

            yield return new TestCaseData(subViewModel, subViewModel, 0);
            yield return new TestCaseData(subViewModel, GetSupportPointViewModel(), 1);
        }
    }
}