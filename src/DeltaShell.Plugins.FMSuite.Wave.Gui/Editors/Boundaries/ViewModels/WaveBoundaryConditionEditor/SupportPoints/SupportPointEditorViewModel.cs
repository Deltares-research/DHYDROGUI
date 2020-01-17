using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Validation;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.SupportPoints
{
    /// <summary>
    /// <see cref="SupportPointEditorViewModel" /> defines the view model for the support point editor view
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class SupportPointEditorViewModel : INotifyPropertyChanged
    {
        private readonly IWaveBoundaryGeometricDefinition geometricDefinition;
        private SupportPointViewModel selectedViewModel;
        private readonly double maxDistance;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPointListViewModel" /> class.
        /// </summary>
        public SupportPointEditorViewModel(IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            Ensure.NotNull(geometricDefinition, nameof(geometricDefinition));
            this.geometricDefinition = geometricDefinition;
            maxDistance = geometricDefinition.Length;

            RemoveSupportPointCommand = new RelayCommand(RemoveSupportPointAction);
            AddSupportPointCommand = new RelayCommand(AddSupportPointAction);

            ViewModels = GetSortedViewModels();

            Subscribe();

            SelectedViewModel = ViewModels.FirstOrDefault();
        }

        private void Subscribe()
        {
            ViewModels.ForEach(SubscribeViewModel);
        }

        private void SubscribeViewModel(SupportPointViewModel viewModel)
        {
            viewModel.PropertyChanged += OnSupportPointModelPropertyChanged;
        }

        private void UnsubscribeViewModel(SupportPointViewModel viewModel)
        {
            viewModel.PropertyChanged -= OnSupportPointModelPropertyChanged;
        }

        /// <summary>
        /// Gets the view models.
        /// </summary>
        /// <value>
        /// The view models.
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

        private ObservableCollection<SupportPointViewModel> GetSortedViewModels()
        {
            IOrderedEnumerable<SupportPoint> sortedSupportPoints = geometricDefinition.SupportPoints
                                                                                      .OrderBy(sp => sp.Distance);
            IEnumerable<SupportPointViewModel> sortedViewModels = sortedSupportPoints
                .Select(sp => new SupportPointViewModel(sp));

            return new ObservableCollection<SupportPointViewModel>(sortedViewModels);
        }

        private void AddSupportPointAction(object value)
        {
            ValidationResult result =
                new PositiveDoubleValidationRule().Validate(value, CultureInfo.CurrentCulture);

            if (!result.IsValid
                || !IsInRange(NewDistance)
                || DistanceExists(NewDistance))
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

        private SupportPointViewModel CreateSupportPointViewModel(double distance)
        {
            var newSupportPoint = new SupportPoint(distance, geometricDefinition);
            return new SupportPointViewModel(newSupportPoint);
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

            geometricDefinition.SupportPoints.Add(viewModel.SupportPoint);

            SubscribeViewModel(viewModel);
        }

        private void RemoveSupportPointAction(object viewModel)
        {
            var supportPointViewModel = (SupportPointViewModel) viewModel;

            if (IsEndPoint(supportPointViewModel.Distance))
            {
                return;
            }

            RemoveViewModel(supportPointViewModel);

            if (SelectedViewModel == null)
            {
                SelectedViewModel = ViewModels.FirstOrDefault();
            }
        }

        private void RemoveViewModel(SupportPointViewModel viewModel)
        {
            ViewModels.Remove(viewModel);

            geometricDefinition.SupportPoints.Remove(viewModel.SupportPoint);

            UnsubscribeViewModel(viewModel);
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
            if (!(sender is SupportPointViewModel supportPointViewModel)
                || e.PropertyName != nameof(SupportPointViewModel.Distance)
                || !(e is PropertyChangedExtendedEventArgs eExtended))
            {
                return;
            }

            OnViewModelDistanceChanged(supportPointViewModel, (double) eExtended.OriginalValue);
        }

        private void OnViewModelDistanceChanged(SupportPointViewModel supportPointViewModel, double originalDistance)
        {
            double newDistance = supportPointViewModel.Distance;

            IEnumerable<SupportPointViewModel> viewModelsToCheck = ViewModels.Except(new[]
            {
                supportPointViewModel
            });

            if (IsEndPoint(originalDistance) || !IsInRange(newDistance) || DistanceExists(viewModelsToCheck, newDistance))
            {
                UnsubscribeViewModel(supportPointViewModel);

                supportPointViewModel.Distance = originalDistance;

                SubscribeViewModel(supportPointViewModel);
            }
            else
            {
                ReplaceViewModel(supportPointViewModel);
            }
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

        private bool IsInRange(double distance)
        {
            return 0 <= distance && distance <= maxDistance;
        }

        private bool IsEndPoint(double distance)
        {
            return Math.Abs(distance) < 1E-15 || Math.Abs(distance - maxDistance) < 1E-15;
        }

        private bool DistanceExists(double distance)
        {
            return DistanceExists(ViewModels, distance);
        }

        private static bool DistanceExists(IEnumerable<SupportPointViewModel> viewModels, double distance)
        {
            return viewModels.Any(vm => Math.Abs(vm.Distance - distance) < 1E-15);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}