using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DelftTools.Controls.Wpf.Commands;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Validation;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="SupportPointEditorViewModel" /> defines the view model for the support point editor view
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class SupportPointEditorViewModel : INotifyPropertyChanged
    {
        private readonly IWaveBoundaryGeometricDefinition geometricDefinition;
        private ObservableCollection<SupportPointViewModel> viewModels;
        private SupportPointViewModel selectedViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPointListViewModel" /> class.
        /// </summary>
        public SupportPointEditorViewModel(IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            Ensure.NotNull(geometricDefinition, nameof(geometricDefinition));
            this.geometricDefinition = geometricDefinition;

            RemoveSupportPointCommand = new RelayCommand(RemoveSupportPointAction);
            AddSupportPointCommand = new RelayCommand(AddSupportPointAction);

            InitializeViewModels();
            SelectedViewModel = ViewModels.FirstOrDefault();
        }

        /// <summary>
        /// Gets or sets the view models.
        /// </summary>
        /// <value>
        /// The view models.
        /// </value>
        public ObservableCollection<SupportPointViewModel> ViewModels
        {
            get => viewModels;
            private set
            {
                if (viewModels == value)
                {
                    return;
                }

                if (viewModels != null)
                {
                    viewModels.CollectionChanged -= OnViewModelCollectionChanged;
                }

                viewModels = value;

                if (viewModels != null)
                {
                    viewModels.CollectionChanged += OnViewModelCollectionChanged;
                }

                OnPropertyChanged();
            }
        }

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
        /// <remarks>This property is binded in the view.</remarks>
        public double NewDistance { get; set; }

        private void InitializeViewModels()
        {
            IOrderedEnumerable<SupportPoint> sortedSupportPoints = geometricDefinition.SupportPoints
                                                                                      .OrderBy(sp => sp.Distance);
            IEnumerable<SupportPointViewModel> sortedViewModels = sortedSupportPoints
                .Select(sp => new SupportPointViewModel(sp));

            ViewModels = new ObservableCollection<SupportPointViewModel>(sortedViewModels);
        }

        private void AddSupportPointAction(object value)
        {
            var distanceString = (string) value;
            if (!FieldValidator.IsPositiveDouble(distanceString, CultureInfo.CurrentCulture))
            {
                return;
            }

            double distance = double.Parse(distanceString, NumberStyles.Any, CultureInfo.CurrentCulture);
            if (ViewModels.Any(vm => Math.Abs(vm.Distance - distance) < 1E-15))
            {
                return;
            }

            var newSupportPoint = new SupportPoint(distance, geometricDefinition);
            ViewModels.Add(new SupportPointViewModel(newSupportPoint));
        }

        private void RemoveSupportPointAction(object viewModel)
        {
            ViewModels.Remove((SupportPointViewModel) viewModel);
        }

        private void OnViewModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    OnViewModelRemoved((SupportPointViewModel) e.OldItems[0]);
                    break;
                case NotifyCollectionChangedAction.Add:
                    OnViewModelAdded((SupportPointViewModel) e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException(nameof(e.Action));
                default:
                    throw new ArgumentOutOfRangeException(nameof(e.Action));
            }
        }

        private void OnViewModelAdded(SupportPointViewModel addedViewModel)
        {
            geometricDefinition.SupportPoints.Add(addedViewModel.SupportPoint);

            if (ViewModels.Count == 1)
            {
                SelectedViewModel = ViewModels[0];
                return;
            }

            ViewModels = new ObservableCollection<SupportPointViewModel>(ViewModels.OrderBy(vm => vm.Distance));
        }

        private void OnViewModelRemoved(SupportPointViewModel viewModel)
        {
            geometricDefinition.SupportPoints.Remove(viewModel.SupportPoint);

            if (SelectedViewModel == viewModel)
            {
                SelectedViewModel = ViewModels.FirstOrDefault();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}