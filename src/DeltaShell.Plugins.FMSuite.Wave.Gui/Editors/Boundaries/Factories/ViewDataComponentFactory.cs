using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific.TimeSeriesGeneration;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

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
        private readonly IGenerateSeries generateSeries;

        /// <summary>
        /// Creates a new <see cref="ViewDataComponentFactory"/>.
        /// </summary>
        /// <param name="dataComponentFactory">The <see cref="IBoundaryConditionDataComponentFactory"/>.</param>
        /// <param name="referenceDateTimeProvider">The reference date time provider.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dataComponentFactory"/> is <c>null</c>.
        /// </exception>
        public ViewDataComponentFactory(IBoundaryConditionDataComponentFactory dataComponentFactory,
                                        IReferenceDateTimeProvider referenceDateTimeProvider)
        {
            Ensure.NotNull(dataComponentFactory, nameof(dataComponentFactory));
            Ensure.NotNull(referenceDateTimeProvider, nameof(referenceDateTimeProvider));

            this.dataComponentFactory = dataComponentFactory;
            generateSeries = new GenerateSeries(new GenerateSeriesDialogHelper(), 
                                                referenceDateTimeProvider);
        }

        public ForcingViewType GetForcingType(IBoundaryConditionDataComponent dataComponent)
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
                default:
                    throw new NotSupportedException($"The provided {nameof(dataComponent)} is not supported.");
            }
        }

        public SpatialDefinitionViewType GetSpatialDefinition(IBoundaryConditionDataComponent dataComponent)
        {
            Ensure.NotNull(dataComponent, nameof(dataComponent));

            switch (dataComponent)
            {
                case UniformDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                case UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>> _:
                case UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                case UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> _:
                    return SpatialDefinitionViewType.Uniform;
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> _:
                    return SpatialDefinitionViewType.SpatiallyVarying;
                default:
                    throw new NotSupportedException("The type of the specified dataComponent does not correspond with a supported type");
            }
        }

        public DirectionalSpreadingViewType GetDirectionalSpreadingViewType(IBoundaryConditionDataComponent dataComponent)
        {
            Ensure.NotNull(dataComponent, nameof(dataComponent));

            switch (dataComponent)
            {
                case UniformDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> _:
                case UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> _:
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

        public IParametersSettingsViewModel ConstructParametersSettingsViewModel(IBoundaryConditionDataComponent dataComponent)
        {
            Ensure.NotNull(dataComponent, nameof(dataComponent));

            switch (dataComponent)
            {
                case UniformDataComponent<ConstantParameters<PowerDefinedSpreading>> uniformDataComponent:
                    return new UniformConstantParametersSettingsViewModel<PowerDefinedSpreading>(uniformDataComponent.Data);

                case UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>> uniformDataComponent:
                    return new UniformConstantParametersSettingsViewModel<DegreesDefinedSpreading>(uniformDataComponent.Data);

                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> spatiallyVaryingDataComponent:
                    return new SpatiallyVariantConstantParametersSettingsViewModel<PowerDefinedSpreading>(spatiallyVaryingDataComponent.Data);

                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> spatiallyVaryingDataComponent:
                    return new SpatiallyVariantConstantParametersSettingsViewModel<DegreesDefinedSpreading>(spatiallyVaryingDataComponent.Data);

                case UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>> uniformDataComponent:
                    return new UniformTimeDependentParametersSettingsViewModel<PowerDefinedSpreading>(uniformDataComponent.Data, 
                                                                                                      generateSeries);

                case UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> uniformDataComponent:
                    return new UniformTimeDependentParametersSettingsViewModel<DegreesDefinedSpreading>(uniformDataComponent.Data, 
                                                                                                        generateSeries);

                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> spatiallyVaryingDataComponent:
                    return new SpatiallyVariantTimeDependentParametersSettingsViewModel<PowerDefinedSpreading>(spatiallyVaryingDataComponent.Data, 
                                                                                                               generateSeries);

                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> spatiallyVaryingDataComponent:
                    return new SpatiallyVariantTimeDependentParametersSettingsViewModel<DegreesDefinedSpreading>(spatiallyVaryingDataComponent.Data, 
                                                                                                                 generateSeries);

                default:
                    throw new NotSupportedException("The type of the specified dataComponent does not correspond with a supported type");
            }
        }

        public IBoundaryConditionDataComponent ConstructBoundaryConditionDataComponent(ForcingViewType forcingType,
                                                                                       SpatialDefinitionViewType spatialDefinition,
                                                                                       DirectionalSpreadingViewType spreadingType)
        {
            switch (forcingType) {
                case ForcingViewType.Constant when spatialDefinition == SpatialDefinitionViewType.Uniform && 
                                                   spreadingType == DirectionalSpreadingViewType.Power:
                    return dataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>>();
                case ForcingViewType.Constant when spatialDefinition == SpatialDefinitionViewType.Uniform && 
                                                   spreadingType == DirectionalSpreadingViewType.Degrees:
                    return dataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>>>();
                case ForcingViewType.Constant when spatialDefinition == SpatialDefinitionViewType.SpatiallyVarying &&
                                                   spreadingType == DirectionalSpreadingViewType.Power:
                    return dataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>>();
                case ForcingViewType.Constant when spatialDefinition == SpatialDefinitionViewType.SpatiallyVarying &&
                                                   spreadingType == DirectionalSpreadingViewType.Degrees:
                    return dataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>>>();
                case ForcingViewType.TimeSeries when spatialDefinition == SpatialDefinitionViewType.Uniform && 
                                                     spreadingType == DirectionalSpreadingViewType.Power:
                    return dataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>>();
                case ForcingViewType.TimeSeries when spatialDefinition == SpatialDefinitionViewType.Uniform && 
                                                     spreadingType == DirectionalSpreadingViewType.Degrees:
                    return dataComponentFactory.ConstructDefaultDataComponent<UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>>();
                case ForcingViewType.TimeSeries when spatialDefinition == SpatialDefinitionViewType.SpatiallyVarying &&
                                                     spreadingType == DirectionalSpreadingViewType.Power:
                    return dataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>>>();
                case ForcingViewType.TimeSeries when spatialDefinition == SpatialDefinitionViewType.SpatiallyVarying &&
                                                     spreadingType == DirectionalSpreadingViewType.Degrees:
                    return dataComponentFactory.ConstructDefaultDataComponent<SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>>();
                case ForcingViewType.FileBased:
                default:
                    throw new NotSupportedException($"The combination of {forcingType} and {spatialDefinition} is currently not supported.");
            }
        }

        public IBoundaryConditionDataComponent ConvertBoundaryConditionDataComponentSpreadingType(IBoundaryConditionDataComponent currentDataComponent, 
                                                                                                  DirectionalSpreadingViewType newSpreadingType)
        {
            Ensure.NotNull(currentDataComponent, nameof(currentDataComponent));

            switch (currentDataComponent)
            {
                case UniformDataComponent<ConstantParameters<PowerDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Power:
                    return dc;
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Power:
                    return dc;
                case UniformDataComponent<ConstantParameters<PowerDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Degrees:
                    return dataComponentFactory.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(dc);
                case SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Degrees:
                    return dataComponentFactory.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(dc);
                case UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Degrees:
                    return dc;
                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Degrees:
                    return dc;
                case UniformDataComponent<ConstantParameters<DegreesDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Power:
                    return dataComponentFactory.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(dc);
                case SpatiallyVaryingDataComponent<ConstantParameters<DegreesDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Power:
                    return dataComponentFactory.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(dc);

                case UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Power:
                    return dc;
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Power:
                    return dc;
                case UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Degrees:
                    return dataComponentFactory.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(dc);
                case SpatiallyVaryingDataComponent<TimeDependentParameters<PowerDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Degrees:
                    return dataComponentFactory.ConvertDataComponentSpreading<PowerDefinedSpreading, DegreesDefinedSpreading>(dc);
                case UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Degrees:
                    return dc;
                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Degrees:
                    return dc;
                case UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Power:
                    return dataComponentFactory.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(dc);
                case SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>> dc when newSpreadingType == DirectionalSpreadingViewType.Power:
                    return dataComponentFactory.ConvertDataComponentSpreading<DegreesDefinedSpreading, PowerDefinedSpreading>(dc);

                default:
                    throw new NotSupportedException("The provided data component and spreading type is not supported.");
            }
        }
    }
}