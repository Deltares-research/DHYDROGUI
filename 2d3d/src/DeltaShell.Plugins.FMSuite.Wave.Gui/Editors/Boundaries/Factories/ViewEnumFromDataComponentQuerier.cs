using System;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories
{
    /// <summary>
    /// <see cref="IViewEnumFromDataComponentQuerier"/> implements the interface to convert
    /// a <see cref="ISpatiallyDefinedDataComponent"/> to its corresponding view enums.
    /// </summary>
    public class ViewEnumFromDataComponentQuerier : IViewEnumFromDataComponentQuerier
    {
        public ForcingViewType GetForcingType(ISpatiallyDefinedDataComponent dataComponent)
        {
            Ensure.NotNull(dataComponent, nameof(dataComponent));

            switch (dataComponent)
            {
                case UniformDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                case UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> _:
                    return ForcingViewType.Constant;
                case UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                case UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> _:
                    return ForcingViewType.TimeSeries;
                case UniformDataComponent<FileBasedParameters> _:
                case SpatiallyVaryingDataComponent<FileBasedParameters> _:
                    return ForcingViewType.FileBased;
                default:
                    throw new NotSupportedException($"The provided {nameof(dataComponent)} is not supported.");
            }
        }

        public SpatialDefinitionViewType GetSpatialDefinition(ISpatiallyDefinedDataComponent dataComponent)
        {
            Ensure.NotNull(dataComponent, nameof(dataComponent));

            switch (dataComponent)
            {
                case UniformDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                case UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>> _:
                case UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                case UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> _:
                case UniformDataComponent<FileBasedParameters> _:
                    return SpatialDefinitionViewType.Uniform;
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<FileBasedParameters> _:
                    return SpatialDefinitionViewType.SpatiallyVarying;
                default:
                    throw new NotSupportedException("The type of the specified dataComponent does not correspond with a supported type");
            }
        }

        public DirectionalSpreadingViewType GetDirectionalSpreadingViewType(ISpatiallyDefinedDataComponent dataComponent)
        {
            Ensure.NotNull(dataComponent, nameof(dataComponent));

            switch (dataComponent)
            {
                case UniformDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                case UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                case UniformDataComponent<FileBasedParameters> _:
                case SpatiallyVaryingDataComponent<FileBasedParameters> _:
                    return DirectionalSpreadingViewType.Power;
                case UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> _:
                case UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> _:
                    return DirectionalSpreadingViewType.Degrees;
                default:
                    throw new NotSupportedException("The type of the specified dataComponent does not correspond with a supported type");
            }
        }
    }
}