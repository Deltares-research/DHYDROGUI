using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="SpatiallyVariantTimeDependentParametersSettingsViewModel{TSpreading}"/> defines the view model for the
    /// ParametersSettingsView given spatially varying constant data.
    /// </summary>
    /// <seealso cref="ConstantParameters{TSpreading}"/>
    /// <seealso cref="ISpatiallyVariantParametersSettingsViewModel"/>
    public sealed class SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading> : TimeDependentParametersSettingsViewModel, ISpatiallyVariantParametersSettingsViewModel
        where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        private readonly IReadOnlyDictionary<SupportPoint, TimeDependentParameters<TSpreading>> supportPointToParametersMapping;

        private TimeDependentParametersViewModel activeParametersViewModel;

        private string groupBoxTitle;

        /// <summary>
        /// Creates a new <see cref="SpatiallyVariantTimeDependentParametersSettingsViewModel{TSpreading}"/>.
        /// </summary>
        /// <param name="supportPointToParametersMapping">
        /// The mapping of support points to their corresponding <see cref="TimeDependentParameters{TSpreading}"/>.
        /// </param>
        /// <param name="generateSeries">The <see cref="IGenerateSeries"/>. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPointToParametersMapping"/> or
        /// <paramref name="generateSeries"/> is <c>null</c>.
        /// </exception>
        public SpatiallyVariantTimeDependentParametersSettingsViewModel(IReadOnlyDictionary<SupportPoint, TimeDependentParameters<TSpreading>> supportPointToParametersMapping,
                                                                        IGenerateSeries generateSeries) : base(generateSeries)
        {
            Ensure.NotNull(supportPointToParametersMapping, nameof(supportPointToParametersMapping));

            this.supportPointToParametersMapping = supportPointToParametersMapping;

            GroupBoxTitle = "Spatially Varying Time Dependent Parameters";
        }

        public override TimeDependentParametersViewModel ActiveParametersViewModel
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

        public void UpdateActiveSupportPoint(SupportPoint supportPoint)
        {
            TimeDependentParameters<TSpreading> correspondingParameters =
                supportPoint != null &&
                supportPointToParametersMapping.TryGetValue(supportPoint, out TimeDependentParameters<TSpreading> value)
                    ? value
                    : null;

            if (correspondingParameters == (ActiveParametersViewModel as TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>)?.ObservedParameters)
            {
                return;
            }

            ActiveParametersViewModel = correspondingParameters != null
                                            ? new TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>(GenerateSeries,
                                                                                                               correspondingParameters,
                                                                                                               supportPointToParametersMapping)
                                            : null;
        }
    }
}