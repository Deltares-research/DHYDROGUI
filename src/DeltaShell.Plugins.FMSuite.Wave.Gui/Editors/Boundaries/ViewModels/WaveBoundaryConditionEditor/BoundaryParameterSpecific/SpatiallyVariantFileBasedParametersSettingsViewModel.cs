using System;
using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="SpatiallyVariantFileBasedParametersSettingsViewModel"/> defines the view model for the
    /// ParametersSettingsView given spatially varying file based data.
    /// </summary>
    /// <seealso cref="FileBasedParametersSettingsViewModel" />
    /// <seealso cref="ISpatiallyVariantParametersSettingsViewModel" />
    public sealed class SpatiallyVariantFileBasedParametersSettingsViewModel : FileBasedParametersSettingsViewModel, ISpatiallyVariantParametersSettingsViewModel
    {
        private readonly IReadOnlyDictionary<SupportPoint, FileBasedParameters> supportPointToParametersMapping;

        private FileBasedParametersViewModel activeParametersViewModel;

        private string groupBoxTitle;

        /// <summary>
        /// Creates a new <see cref="SpatiallyVariantConstantParametersSettingsViewModel{TSpreading}"/>.
        /// </summary>
        /// <param name="supportPointToParametersMapping">
        /// The mapping of support points to their corresponding <see cref="ConstantParameters{TSpreading}"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="supportPointToParametersMapping"/> is <c>null</c>.
        /// </exception>
        public SpatiallyVariantFileBasedParametersSettingsViewModel(IReadOnlyDictionary<SupportPoint, FileBasedParameters> supportPointToParametersMapping)
        {
            Ensure.NotNull(supportPointToParametersMapping, nameof(supportPointToParametersMapping));
            this.supportPointToParametersMapping = supportPointToParametersMapping;

            GroupBoxTitle = "Spatially Varying File Based Parameters";
        }

        public override FileBasedParametersViewModel ActiveParametersViewModel
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

        public override string GroupBoxTitle
        {
            get => groupBoxTitle;
            protected set
            {
                if (value == GroupBoxTitle)
                {
                    return;
                }

                groupBoxTitle = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Updates the currently selected <see cref="FileBasedParameters"/>
        /// with the newly selected <paramref name="supportPoint"/>.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="supportPoint"/> is null.
        /// </exception>
        public void UpdateActiveSupportPoint(SupportPoint supportPoint)
        {
            FileBasedParameters correspondingParameters =
                supportPoint != null &&
                supportPointToParametersMapping.TryGetValue(supportPoint, out FileBasedParameters value)
                    ? value
                    : null;

            if (correspondingParameters == ActiveParametersViewModel?.ObservedParameters)
            {
                return;
            }

            ActiveParametersViewModel = correspondingParameters != null
                                            ? new FileBasedParametersViewModel(correspondingParameters)
                                            : null;
        }
    }
}