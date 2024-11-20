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
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Eventing;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.Utilities;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Validation;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints
{
    /// <summary>
    /// <see cref="SupportPointEditorViewModel"/> defines the view model for the support point editor view.
    /// </summary>
    /// <remarks>
    /// The assumption is made that end points cannot
    /// be edited or removed through the view
    /// </remarks>
    public sealed class SupportPointEditorViewModel : INotifyPropertyChanged, IDisposable, IRefreshIsEnabledOnDataComponentChanged
    {
        private readonly IWaveBoundaryGeometricDefinition geometricDefinition;
        private readonly SupportPointDataComponentViewModel supportPointDataComponentViewModel;

        private SupportPointViewModel selectedSupportPointViewModel;
        private double newDistance;

        private bool isEnabled;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPointEditorViewModel"/> class.
        /// </summary>
        /// <param name="geometricDefinition">The observed <see cref="IWaveBoundaryGeometricDefinition"/>.</param>
        /// <param name="supportPointDataComponentViewModel">The observed <see cref="supportPointDataComponentViewModel"/>.</param>
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

            SupportPointViewModels = new ObservableCollection<SupportPointViewModel>(GetSortedViewModels());

            Subscribe();

            IsEnabled = this.supportPointDataComponentViewModel.IsEnabled();
            SelectedSupportPointViewModel = SupportPointViewModels.First();
        }

        /// <summary>
        /// Gets the view models of the support points.
        /// </summary>
        /// <value>
        /// The view models of the support points.
        /// </value>
        public ObservableCollection<SupportPointViewModel> SupportPointViewModels { get; }

        /// <summary>
        /// Gets or sets the selected view model.
        /// </summary>
        /// <value>
        /// The selected view model.
        /// </value>
        public SupportPointViewModel SelectedSupportPointViewModel
        {
            get => selectedSupportPointViewModel;
            set
            {
                if (selectedSupportPointViewModel == value)
                {
                    return;
                }

                selectedSupportPointViewModel = value;

                if (IsEnabled)
                {
                    supportPointDataComponentViewModel.SelectedSupportPoint =
                        SelectedSupportPointViewModel?.SupportPoint;
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether this <see cref="SupportPointEditorViewModel"/> is enabled.
        /// </summary>
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
        public double NewDistance
        {
            get => newDistance;
            set => newDistance = SpatialDouble.Round(value);
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        public void RefreshIsEnabled()
        {
            IsEnabled = supportPointDataComponentViewModel.IsEnabled();

            if (IsEnabled)
            {
                supportPointDataComponentViewModel.SelectedSupportPoint = SelectedSupportPointViewModel.SupportPoint;
                SupportPointViewModels.ForEach(RefreshIsEnabledSupportPoint);
            }
            else
            {
                SupportPointViewModels.ForEach(x => x.IsEnabled = false);
            }
        }

        private void RefreshIsEnabledSupportPoint(SupportPointViewModel vm) =>
            vm.IsEnabled = supportPointDataComponentViewModel.IsEnabledSupportPoint(vm.SupportPoint);

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
            SupportPointViewModels.Insert(insertIndex, viewModel);

            geometricDefinition.SupportPoints.Add(viewModel.SupportPoint);

            SubscribeViewModel(viewModel);
        }

        private void RemoveSupportPointAction(object viewModel)
        {
            var supportPointViewModel = (SupportPointViewModel) viewModel;

            RemoveViewModel(supportPointViewModel);

            if (SelectedSupportPointViewModel == null || SelectedSupportPointViewModel == supportPointViewModel)
            {
                SelectedSupportPointViewModel = SupportPointViewModels[0];
            }
        }

        private void RemoveViewModel(SupportPointViewModel viewModel)
        {
            viewModel.IsEnabled = false;
            SupportPointViewModels.Remove(viewModel);

            geometricDefinition.SupportPoints.Remove(viewModel.SupportPoint);

            UnsubscribeViewModel(viewModel);
        }

        private void ReplaceViewModel(SupportPointViewModel oldViewModel)
        {
            bool isSelected = SelectedSupportPointViewModel == oldViewModel;

            var newSupportPoint = new SupportPoint(oldViewModel.Distance, geometricDefinition);
            ReplaceConditionData(oldViewModel.SupportPoint,
                                 newSupportPoint);

            RemoveViewModel(oldViewModel);

            var newViewModel = new SupportPointViewModel(newSupportPoint, supportPointDataComponentViewModel);
            AddViewModel(newViewModel);

            if (isSelected)
            {
                SelectedSupportPointViewModel = newViewModel;
            }
        }

        private void ReplaceConditionData(SupportPoint oldSupportPoint, SupportPoint newSupportPoint)
        {
            if (supportPointDataComponentViewModel.IsEnabledSupportPoint(oldSupportPoint))
            {
                supportPointDataComponentViewModel.ReplaceSupportPoint(oldSupportPoint,
                                                                       newSupportPoint);
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
            for (var i = 0; i + 1 < SupportPointViewModels.Count; i++)
            {
                if (SupportPointViewModels[i].Distance < distance &&
                    distance < SupportPointViewModels[i + 1].Distance)
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
            double currentDistance = supportPointViewModel.Distance;

            IEnumerable<SupportPointViewModel> viewModelsToCheck = SupportPointViewModels.Except(new[]
            {
                supportPointViewModel
            });

            if (IsOutsideRange(currentDistance) ||
                DistanceExists(viewModelsToCheck, currentDistance))
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
                   distance > SupportPointViewModels.Last().Distance;
        }

        private bool DistanceExists(double distance)
        {
            return DistanceExists(SupportPointViewModels, distance);
        }

        private static bool DistanceExists(IEnumerable<SupportPointViewModel> viewModels, double distance)
        {
            return viewModels.Any(vm => SpatialDouble.AreEqual(vm.Distance, distance));
        }

        private void Subscribe()
        {
            SupportPointViewModels.ForEach(SubscribeViewModel);
        }

        private void Unsubscribe()
        {
            SupportPointViewModels.ForEach(UnsubscribeViewModel);
        }

        private void SubscribeViewModel(SupportPointViewModel viewModel)
        {
            viewModel.PropertyChanged += OnSupportPointModelPropertyChanged;
        }

        private void UnsubscribeViewModel(SupportPointViewModel viewModel)
        {
            viewModel.PropertyChanged -= OnSupportPointModelPropertyChanged;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}