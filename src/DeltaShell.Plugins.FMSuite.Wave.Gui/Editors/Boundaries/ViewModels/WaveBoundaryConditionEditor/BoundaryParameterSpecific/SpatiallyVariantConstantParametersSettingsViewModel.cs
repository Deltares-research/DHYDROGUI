using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="SpatiallyVariantConstantParametersSettingsViewModel"/> defines the view model for the
    /// ConstantParametersSettingsView given spatially varying data.
    /// </summary>
    /// <seealso cref="ConstantParametersSettingsViewModel" />
    /// <seealso cref="INotifyPropertyChanged" />
    public sealed class SpatiallyVariantConstantParametersSettingsViewModel : ConstantParametersSettingsViewModel, INotifyPropertyChanged
    {
        private readonly IReadOnlyDictionary<SupportPoint, ConstantParameters> supportPointToParametersMapping;

        /// <summary>
        /// Creates a new <see cref="SpatiallyVariantConstantParametersSettingsViewModel"/>.
        /// </summary>
        /// <param name="supportPointToParametersMapping">
        /// The mapping of support points to their corresponding <see cref="ConstantParameters"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPointToParametersMapping"/> is <c>null</c>.
        /// </exception>
         public SpatiallyVariantConstantParametersSettingsViewModel(IReadOnlyDictionary<SupportPoint, ConstantParameters> supportPointToParametersMapping)
        {
            Ensure.NotNull(supportPointToParametersMapping, nameof(supportPointToParametersMapping));
            this.supportPointToParametersMapping = supportPointToParametersMapping;
        }

        public override ConstantParametersViewModel ActiveParametersViewModel
        {
            get => activeParametersViewModel;
            protected set
            {
                if (value == ActiveParametersViewModel)
                {
                    return;
                }

                activeParametersViewModel = value;
                OnPropertyChanged();
            }
        }

        private ConstantParametersViewModel activeParametersViewModel;

        /// <summary>
        /// Updates the currently selected <see cref="ConstantParameters"/>
        /// with the newly selected <paramref name="supportPoint"/>.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="supportPoint"/> is null.
        /// </exception>
        public void UpdateActiveSupportPoint(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));

            ConstantParameters correspondingParameters =
                supportPointToParametersMapping.TryGetValue(supportPoint, out ConstantParameters value) 
                    ? value 
                    : null;

            if (correspondingParameters == ActiveParametersViewModel?.ObservedParameters)
            {
                return;
            }

            ActiveParametersViewModel = correspondingParameters != null
                                            ? new ConstantParametersViewModel(correspondingParameters)
                                            : null;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}