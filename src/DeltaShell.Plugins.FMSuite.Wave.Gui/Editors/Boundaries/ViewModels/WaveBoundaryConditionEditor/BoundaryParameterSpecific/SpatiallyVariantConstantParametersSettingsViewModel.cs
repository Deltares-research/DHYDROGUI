using System.Collections.Generic;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="SpatiallyVariantConstantParametersSettingsViewModel{TSpreading}"/> defines the view model for the
    /// ParametersSettingsView given spatially varying constant data.
    /// </summary>
    /// <seealso cref="ConstantParameters{TSpreading}"/>
    /// <seealso cref="ISpatiallyVariantParametersSettingsViewModel"/>
    public sealed class SpatiallyVariantConstantParametersSettingsViewModel<TSpreading> : ConstantParametersSettingsViewModel, ISpatiallyVariantParametersSettingsViewModel
        where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        private readonly IReadOnlyDictionary<SupportPoint, ConstantParameters<TSpreading>> supportPointToParametersMapping;

        /// <summary>
        /// Creates a new <see cref="SpatiallyVariantConstantParametersSettingsViewModel{TSpreading}"/>.
        /// </summary>
        /// <param name="supportPointToParametersMapping">
        /// The mapping of support points to their corresponding <see cref="ConstantParameters{TSpreading}"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="supportPointToParametersMapping"/> is <c>null</c>.
        /// </exception>
        public SpatiallyVariantConstantParametersSettingsViewModel(IReadOnlyDictionary<SupportPoint, ConstantParameters<TSpreading>> supportPointToParametersMapping)
        {
            Ensure.NotNull(supportPointToParametersMapping, nameof(supportPointToParametersMapping));
            this.supportPointToParametersMapping = supportPointToParametersMapping;

            GroupBoxTitle = Resources.SpatiallyVariantConstantParametersSettingsViewModel_GroupBoxTitle;
        }

        public void UpdateActiveSupportPoint(SupportPoint supportPoint)
        {
            Ensure.NotNull(supportPoint, nameof(supportPoint));
            ConstantParameters<TSpreading> correspondingParameters =
                supportPointToParametersMapping.TryGetValue(supportPoint, out ConstantParameters<TSpreading> value)
                    ? value
                    : null;

            if (correspondingParameters == (ActiveParametersViewModel as ConstantParametersViewModelGeneric<TSpreading>)?.ObservedParameters)
            {
                return;
            }

            ActiveParametersViewModel = correspondingParameters != null
                                            ? new ConstantParametersViewModelGeneric<TSpreading>(correspondingParameters)
                                            : null;
        }
    }
}