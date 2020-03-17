using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="SpatiallyVariantConstantParametersSettingsViewModel{TSpreading}"/> defines the view model for the
    /// ConstantParametersSettingsView given spatially varying data.
    /// </summary>
    /// <seealso cref="ConstantParameters{TSpreading}" />
    public sealed class SpatiallyVariantConstantParametersSettingsViewModel<TSpreading> : ConstantParametersSettingsViewModel
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

            GroupBoxTitle = "Spatially Varying Constant Parameters";
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
        /// Updates the currently selected <see cref="ConstantParameters"/>
        /// with the newly selected <paramref name="supportPoint"/>.
        /// </summary>
        /// <param name="supportPoint">The support point.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when <paramref name="supportPoint"/> is null.
        /// </exception>
        public void UpdateActiveSupportPoint(SupportPoint supportPoint)
        {
            ConstantParameters<TSpreading> correspondingParameters = 
                supportPoint != null && 
                supportPointToParametersMapping.TryGetValue(supportPoint, out ConstantParameters<TSpreading> value) 
                    ? value 
                    : null;

            if (correspondingParameters == (ActiveParametersViewModel as ConstantParametersViewModel<TSpreading>)?.ObservedParameters)
            {
                return;
            }

            ActiveParametersViewModel = correspondingParameters != null
                                            ? new ConstantParametersViewModel<TSpreading>(correspondingParameters)
                                            : null;
        }
    }
}