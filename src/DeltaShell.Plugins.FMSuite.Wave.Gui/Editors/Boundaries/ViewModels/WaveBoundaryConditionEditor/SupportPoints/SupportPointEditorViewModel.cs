using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.Eventing;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Validation;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints
{
    /// <summary>
    /// <see cref="SupportPointEditorViewModel" /> defines the view model for the support point editor view.
    /// </summary>
    public sealed class SupportPointEditorViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IWaveBoundary waveBoundary;
        private SupportPointViewModel selectedViewModel;

        private double MaxDistance => waveBoundary.GeometricDefinition.Length;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPointEditorViewModel" /> class.
        /// </summary>
        /// <param name="waveBoundary">The observed <see cref="IWaveBoundary"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="waveBoundary"/> is <c>null</c>.
        /// </exception>
        public SupportPointEditorViewModel(IWaveBoundary waveBoundary)
        {
            Ensure.NotNull(waveBoundary, nameof(waveBoundary));
            this.waveBoundary = waveBoundary;

            RemoveSupportPointCommand = new RelayCommand(RemoveSupportPointAction);
            AddSupportPointCommand = new RelayCommand(AddSupportPointAction);

            ViewModels = GetSortedViewModels();

            Subscribe();

            SelectedViewModel = ViewModels.FirstOrDefault();

            IsEnabled = ShouldBeEnabled();
        }

        /// <summary>
        /// Gets the view models of the support points.
        /// </summary>
        /// <value>
        /// The view models of the support points.
        /// </value>
        public ObservableCollection<SupportPointViewModel> ViewModels { get; }

        /// <summary>
        /// Gets or sets the selected view model.
        /// </summary>
        /// <value>
        /// The selected view model.
        /// </value>
        public SupportPointViewModel SelectedViewModel
        {
            get => selectedViewModel;
            set
            {
                if (selectedViewModel == value)
                {
                    return;
                }

                selectedViewModel = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (value == isEnabled)
                {
                    return;
                }

                isEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool isEnabled;

        // TODO: Verify whether we want to move this to the ViewDataComponentFactory, 
        // or move to separate function. Preferably it should only be defined in one place.
        private bool ShouldBeEnabled()
        {
            return waveBoundary.ConditionDefinition.DataComponent is SpatiallyVaryingDataComponent<ConstantParameters>;
        }

        public void ReceiveDataComponentChanged()
        {
            IsEnabled = ShouldBeEnabled();
        }

        /// <summary>
        /// Gets the add support point command.
        /// </summary>
        /// <value>
        /// The add support point command.
        /// </value>
        public ICommand AddSupportPointCommand { get; }

        /// <summary>
        /// Gets the remove support point command.
        /// </summary>
        /// <value>
        /// The remove support point command.
        /// </value>
        public ICommand RemoveSupportPointCommand { get; }

        /// <summary>
        /// Gets or sets the new distance.
        /// </summary>
        /// <value>
        /// The new distance.
        /// </value>
        public double NewDistance { get; set; }

        private void AddSupportPointAction(object value)
        {
            var validationRule = new PositiveDoubleValidationRule();
            ValidationResult result = validationRule.Validate(value, CultureInfo.CurrentCulture);

            if (!result.IsValid ||
                IsOutsideRange(NewDistance) ||
                DistanceExists(NewDistance))
            {
                return;
            }

            SupportPointViewModel newViewModel = CreateSupportPointViewModel(NewDistance);
            AddViewModel(newViewModel);

            if (ViewModels.HasExactlyOneValue())
            {
                SelectedViewModel = ViewModels[0];
            }
        }

        private void AddViewModel(SupportPointViewModel viewModel)
        {
            if (TryFindInsertIndex(viewModel.Distance, out int index))
            {
                ViewModels.Insert(index, viewModel);
            }
            else
            {
                ViewModels.Add(viewModel);
            }

            waveBoundary.GeometricDefinition.SupportPoints.Add(viewModel.SupportPoint);

            SubscribeViewModel(viewModel);
        }

        private void RemoveSupportPointAction(object viewModel)
        {
            var supportPointViewModel = (SupportPointViewModel)viewModel;

            if (IsEndPoint(supportPointViewModel.Distance))
            {
                return;
            }

            RemoveViewModel(supportPointViewModel);

            if (SelectedViewModel == null || SelectedViewModel == supportPointViewModel)
            {
                SelectedViewModel = ViewModels.FirstOrDefault();
            }
        }

        private void RemoveViewModel(SupportPointViewModel viewModel)
        {
            ViewModels.Remove(viewModel);

            waveBoundary.GeometricDefinition.SupportPoints.Remove(viewModel.SupportPoint);

            UnsubscribeViewModel(viewModel);
        }

        private void ReplaceViewModel(SupportPointViewModel oldViewModel)
        {
            bool isSelected = SelectedViewModel == oldViewModel;

            RemoveViewModel(oldViewModel);

            SupportPointViewModel newViewModel = CreateSupportPointViewModel(oldViewModel.Distance);

            AddViewModel(newViewModel);

            if (isSelected)
            {
                SelectedViewModel = newViewModel;
            }
        }

        private ObservableCollection<SupportPointViewModel> GetSortedViewModels()
        {
            IOrderedEnumerable<SupportPoint> sortedSupportPoints =
                waveBoundary.GeometricDefinition.SupportPoints.OrderBy(sp => sp.Distance);
            IEnumerable<SupportPointViewModel> sortedViewModels = sortedSupportPoints
                .Select(sp => new SupportPointViewModel(sp));

            return new ObservableCollection<SupportPointViewModel>(sortedViewModels);
        }

        private SupportPointViewModel CreateSupportPointViewModel(double distance)
        {
            var newSupportPoint = new SupportPoint(distance, waveBoundary.GeometricDefinition);
            return new SupportPointViewModel(newSupportPoint);
        }

        private bool TryFindInsertIndex(double distance, out int index)
        {
            index = -1;

            if (!ViewModels.Any())
            {
                return false;
            }

            if (distance < ViewModels[0].Distance)
            {
                index = 0;
                return true;
            }

            if (distance > ViewModels.Last().Distance)
            {
                return false;
            }

            for (var i = 0; i + 1 < ViewModels.Count; i++)
            {
                if (ViewModels[i].Distance < distance && distance < ViewModels[i + 1].Distance)
                {
                    index = i + 1;
                    return true;
                }
            }

            return false;
        }

        private void OnSupportPointModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SupportPointViewModel.Distance) &&
                sender is SupportPointViewModel supportPointViewModel &&
                e is PropertyChangedExtendedEventArgs eExtended)
            {
                OnViewModelDistanceChanged(supportPointViewModel, (double) eExtended.OriginalValue);
            }
        }

        private void OnViewModelDistanceChanged(SupportPointViewModel supportPointViewModel, double originalDistance)
        {
            double newDistance = supportPointViewModel.Distance;

            IEnumerable<SupportPointViewModel> viewModelsToCheck = ViewModels.Except(new[]
            {
                supportPointViewModel
            });

            if (IsEndPoint(originalDistance) ||
                IsOutsideRange(newDistance) ||
                DistanceExists(viewModelsToCheck, newDistance))
            {
                UnsubscribeViewModel(supportPointViewModel);

                supportPointViewModel.Distance = originalDistance;

                SubscribeViewModel(supportPointViewModel);
            }
            else
            {
                // The view model needs to be replaced to trigger the refreshing of the map.
                ReplaceViewModel(supportPointViewModel);
            }
        }

        private bool IsOutsideRange(double distance)
        {
            return distance < 0 || distance > MaxDistance;
        }

        private bool IsEndPoint(double distance)
        {
            const double tolerance = 1E-15;
            return Math.Abs(distance) < tolerance || Math.Abs(distance - MaxDistance) < tolerance;
        }

        private bool DistanceExists(double distance)
        {
            return DistanceExists(ViewModels, distance);
        }

        private static bool DistanceExists(IEnumerable<SupportPointViewModel> viewModels, double distance)
        {
            const double tolerance = 1E-15;
            return viewModels.Any(vm => Math.Abs(vm.Distance - distance) < tolerance);
        }

        private void Subscribe()
        {
            ViewModels.ForEach(SubscribeViewModel);
        }

        private void Unsubscribe()
        {
            ViewModels.ForEach(UnsubscribeViewModel);
        }

        private void SubscribeViewModel(SupportPointViewModel viewModel)
        {
            viewModel.PropertyChanged += OnSupportPointModelPropertyChanged;
        }

        private void UnsubscribeViewModel(SupportPointViewModel viewModel)
        {
            viewModel.PropertyChanged -= OnSupportPointModelPropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            Unsubscribe();
        }
    }
}