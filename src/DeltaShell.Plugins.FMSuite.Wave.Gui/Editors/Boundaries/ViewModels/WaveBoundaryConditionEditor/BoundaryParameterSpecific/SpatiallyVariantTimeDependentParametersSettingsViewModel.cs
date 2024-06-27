using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="SpatiallyVariantTimeDependentParametersSettingsViewModel{TSpreading}"/> defines the view model for the
    /// ParametersSettingsView given spatially varying time dependent data.
    /// </summary>
    /// <seealso cref="TimeDependentParameters{TSpreading}"/>
    /// <seealso cref="ISpatiallyVariantParametersSettingsViewModel"/>
    public sealed class SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading> : TimeDependentParametersSettingsViewModel, ISpatiallyVariantParametersSettingsViewModel
        where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        private readonly IReadOnlyDictionary<SupportPoint, TimeDependentParameters<TSpreading>> supportPointToParametersMapping;

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
                                                                        IGenerateSeries generateSeries) :
            base(generateSeries, Resources.SpatiallyVariantTimeDependentParametersSettingsViewModel_GroupBoxTitle)
        {
            Ensure.NotNull(supportPointToParametersMapping, nameof(supportPointToParametersMapping));

            this.supportPointToParametersMapping = supportPointToParametersMapping;
        }

        public void UpdateActiveSupportPoint(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));
            TimeDependentParameters<TSpreading> correspondingParameters =
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