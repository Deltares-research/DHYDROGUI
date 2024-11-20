using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Mediators;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor
{
    /// <summary>
    /// <see cref="BoundarySpecificParametersSettingsViewModel"/> defines the
    /// view model for the boundary-specific parameters settings view.
    /// </summary>
    public sealed class BoundarySpecificParametersSettingsViewModel : IRefreshDataComponentViewModel,
                                                                      INotifyPropertyChanged
    {
        private readonly IWaveBoundaryConditionDefinition conditionDefinition;
        private readonly IViewDataComponentFactory dataComponentFactory;

        private IParametersSettingsViewModel parametersSettingsViewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new <see cref="BoundarySpecificParametersSettingsViewModel"/>.
        /// </summary>
        /// <param name="conditionDefinition">The condition definition.</param>
        /// <param name="dataComponentFactory">The data component factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when any parameter is <c>null</c>.
        /// </exception>
        public BoundarySpecificParametersSettingsViewModel(IWaveBoundaryConditionDefinition conditionDefinition,
                                                           IViewDataComponentFactory dataComponentFactory)
        {
            Ensure.NotNull(conditionDefinition, nameof(conditionDefinition));
            Ensure.NotNull(dataComponentFactory, nameof(dataComponentFactory));

            this.conditionDefinition = conditionDefinition;
            this.dataComponentFactory = dataComponentFactory;

            RefreshDataComponentViewModel();
        }

        /// <summary>
        /// Gets or sets the parameters settings view model.
        /// </summary>
        public IParametersSettingsViewModel ParametersSettingsViewModel
        {
            get => parametersSettingsViewModel;
            set
            {
                if (ParametersSettingsViewModel == value)
                {
                    return;
                }

                parametersSettingsViewModel = value;
                OnPropertyChanged();
            }
        }

        public void RefreshDataComponentViewModel()
        {
            ParametersSettingsViewModel =
                dataComponentFactory.ConstructParametersSettingsViewModel(conditionDefinition.DataComponent);
        }

        public void UpdateSelectedActiveParameters(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));
            if (ParametersSettingsViewModel is ISpatiallyVariantParametersSettingsViewModel spatiallyVariantParametersSettingsViewModel)
            {
                spatiallyVariantParametersSettingsViewModel.UpdateActiveSupportPoint(supportPoint);
            }
            else
            {
                throw new InvalidOperationException(
                    "Cannot set the selected view point when the data is not spatially variant.");
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}