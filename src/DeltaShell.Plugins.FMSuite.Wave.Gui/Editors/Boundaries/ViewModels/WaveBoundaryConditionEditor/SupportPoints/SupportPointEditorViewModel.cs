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
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Validation;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints
{
    /// <summary>
    /// <see cref="SupportPointEditorViewModel" /> defines the view model for the support point editor view.
    /// </summary>
    /// <remarks>
    /// The assumption is made that end points cannot
    /// be edited or removed through the view
    /// </remarks>
    public sealed class SupportPointEditorViewModel : INotifyPropertyChanged, IDisposable, IRefreshIsEnabledOnDataComponentChanged
    {
        private readonly IWaveBoundaryGeometricDefinition geometricDefinition;
        private readonly SupportPointDataComponentViewModel supportPointDataComponentViewModel;

        private SupportPointViewModel selectedViewModel;

        private const double comparisonTolerance = 1E-15;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPointEditorViewModel" /> class.
        /// </summary>
        /// <param name="geometricDefinition">The observed <see cref="IWaveBoundaryGeometricDefinition"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="geometricDefinition"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// The assumption is made that the specified <paramref name="geometricDefinition"/>
        /// contains at least two support points.
        /// </remarks>
        public SupportPointEditorViewModel(IWaveBoundaryGeometricDefinition geometricDefinition,
                                           SupportPointDataComponentViewModel supportPointDataComponentViewModel)
        {
            Ensure.NotNull(geometricDefinition, nameof(geometricDefinition));
            Ensure.NotNull(supportPointDataComponentViewModel, nameof(supportPointDataComponentViewModel));

            this.geometricDefinition = geometricDefinition;
            this.supportPointDataComponentViewModel = supportPointDataComponentViewModel;

            RemoveSupportPointCommand = new RelayCommand(RemoveSupportPointAction);
            AddSupportPointCommand = new RelayCommand(AddSupportPointAction);

            ViewModels = new ObservableCollection<SupportPointViewModel>(GetSortedViewModels());

            Subscribe();

            SelectedViewModel = ViewModels.First();

            IsEnabled = this.supportPointDataComponentViewModel.IsEnabled();
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

        public void RefreshIsEnabled()
        {
            IsEnabled = supportPointDataComponentViewModel.IsEnabled();
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
        }

        private void AddViewModel(SupportPointViewModel viewModel)
        {
            int insertIndex = FindInsertIndex(viewModel.Distance);
            ViewModels.Insert(insertIndex, viewModel);

            geometricDefinition.SupportPoints.Add(viewModel.SupportPoint);

            SubscribeViewModel(viewModel);
        }

        private void RemoveSupportPointAction(object viewModel)
        {
            var supportPointViewModel = (SupportPointViewModel) viewModel;

            RemoveViewModel(supportPointViewModel);

            if (SelectedViewModel == null || SelectedViewModel == supportPointViewModel)
            {
                SelectedViewModel = ViewModels[0];
            }
        }

        private void RemoveViewModel(SupportPointViewModel viewModel)
        {
            ViewModels.Remove(viewModel);

            geometricDefinition.SupportPoints.Remove(viewModel.SupportPoint);

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

        private IEnumerable<SupportPointViewModel> GetSortedViewModels()
        {
            SupportPoint[] sortedSupportPoints = 
                geometricDefinition.SupportPoints.OrderBy(sp => sp.Distance).ToArray();

            int nSupportPoints = sortedSupportPoints.Length;
            for (var i = 0; i < nSupportPoints; i++)
            {
                bool isFirstOrLast = i == 0 || i == nSupportPoints - 1;
                yield return new SupportPointViewModel(sortedSupportPoints[i], 
                                                       supportPointDataComponentViewModel,
                                                       !isFirstOrLast);
            }
        }

        private SupportPointViewModel CreateSupportPointViewModel(double distance)
        {
            var newSupportPoint = new SupportPoint(distance, geometricDefinition);
            return new SupportPointViewModel(newSupportPoint, supportPointDataComponentViewModel);
        }

        private int FindInsertIndex(double distance)
        {
            for (var i = 0; i + 1 < ViewModels.Count; i++)
            {
                if (ViewModels[i].Distance < distance &&
                    distance < ViewModels[i + 1].Distance)
                {
                    return i + 1;
                }
            }

            return -1;
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

            if (IsOutsideRange(newDistance) ||
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
            return distance < 0 ||
                   distance > ViewModels.Last().Distance;
        }

        private bool DistanceExists(double distance)
        {
            return DistanceExists(ViewModels, distance);
        }

        private static bool DistanceExists(IEnumerable<SupportPointViewModel> viewModels, double distance)
        {
            return viewModels.Any(vm => Math.Abs(vm.Distance - distance) < comparisonTolerance);
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