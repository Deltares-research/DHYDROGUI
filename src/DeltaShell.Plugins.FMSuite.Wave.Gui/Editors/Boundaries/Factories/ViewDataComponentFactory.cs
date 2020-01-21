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
    }
}