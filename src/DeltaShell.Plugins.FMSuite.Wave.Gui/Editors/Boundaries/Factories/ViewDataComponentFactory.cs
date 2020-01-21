using System;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories
{
    /// <summary>
    /// <see cref="ViewDataComponentFactory"/> implements the interface with
    /// which to create the ViewModels necessary for the
    /// BoundaryParameterSpecific view.
    /// </summary>
    /// <seealso cref="IViewDataComponentFactory" />
    public class ViewDataComponentFactory : IViewDataComponentFactory
    {
        private readonly IBoundaryConditionDataComponentFactory dataComponentFactory;

        /// <summary>
        /// Creates a new <see cref="ViewDataComponentFactory"/>.
        /// </summary>
        /// <param name="dataComponentFactory">The <see cref="IBoundaryConditionDataComponentFactory"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dataComponentFactory"/> is <c>null</c>.
        /// </exception>
        public ViewDataComponentFactory(IBoundaryConditionDataComponentFactory dataComponentFactory)
        {
            Ensure.NotNull(dataComponentFactory, nameof(dataComponentFactory));
            this.dataComponentFactory = dataComponentFactory;
        }

        public ForcingViewType GetForcingType(IBoundaryConditionDataComponent dataComponent)
        {
            Ensure.NotNull(dataComponent, nameof(dataComponent));
            return ForcingViewType.Constant;
        }

        public SpatialDefinitionViewType GetSpatialDefinition(IBoundaryConditionDataComponent dataComponent)
        {
            Ensure.NotNull(dataComponent, nameof(dataComponent));

            switch (dataComponent)
            {
                case UniformDataComponent<ConstantParameters> _:
                    return SpatialDefinitionViewType.Uniform;
                case SpatiallyVaryingDataComponent<ConstantParameters> _:
                    return SpatialDefinitionViewType.SpatiallyVarying;
                default:
                    throw new NotSupportedException("The type of the specified dataComponent does not correspond with a supported type");
            }
        }

        public IParametersSettingsViewModel ConstructParametersSettingsViewModel(IBoundaryConditionDataComponent dataComponent)
        {
            Ensure.NotNull(dataComponent, nameof(dataComponent));

            switch (dataComponent)
            {
                case UniformDataComponent<ConstantParameters> uniformDataComponent:
                    return new UniformConstantParametersSettingsViewModel(uniformDataComponent.Data);
                case SpatiallyVaryingDataComponent<ConstantParameters> spatiallyVaryingDataComponent:
                    return new SpatiallyVariantConstantParametersSettingsViewModel(spatiallyVaryingDataComponent.Data);
                default:
                    throw new NotSupportedException("The type of the specified dataComponent does not correspond with a supported type");
            }
        }

        public IBoundaryConditionDataComponent ConstructBoundaryConditionDataComponent(ForcingViewType forcingType,
                                                                                       SpatialDefinitionViewType spatialDefinition)
        {
            switch (forcingType) {
                case ForcingViewType.Constant when spatialDefinition == SpatialDefinitionViewType.Uniform:
                    return dataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters>>();
                case ForcingViewType.Constant when spatialDefinition == SpatialDefinitionViewType.SpatiallyVarying:
                    return dataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters>>();
                case ForcingViewType.TimeSeries:
                case ForcingViewType.FileBased:
                default:
                    throw new NotSupportedException($"The combination of {forcingType} and {spatialDefinition} is currently not supported.");
            }
        }
    }
}