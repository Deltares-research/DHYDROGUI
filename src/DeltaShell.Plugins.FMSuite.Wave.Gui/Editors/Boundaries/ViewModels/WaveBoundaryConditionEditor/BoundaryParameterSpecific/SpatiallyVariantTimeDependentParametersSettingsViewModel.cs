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
    /// <seealso cref="ConstantParameters{TSpreading}" />
    public sealed class SpatiallyVariantTimeDependentParametersSettingsViewModel<TSpreading> : TimeDependentParametersSettingsViewModel
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

        private TimeDependentParametersViewModel activeParametersViewModel;

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

        private string groupBoxTitle;


        /// <summary>
        /// Updates the currently selected <see cref="TimeDependentParameters{TSpreading}"/>
        /// with the newly selected <paramref name="supportPoint"/>.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="supportPoint"/> is null.
        /// </exception>
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
                                            ? new TimeDependentSpatiallyVaryingParametersViewModel<TSpreading>(generateSeries,  
                                                                                                               correspondingParameters, 
                                                                                                               supportPointToParametersMapping)
                                            : null;
        }
    }
}
