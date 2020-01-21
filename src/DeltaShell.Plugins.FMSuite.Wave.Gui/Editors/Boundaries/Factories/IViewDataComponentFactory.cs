using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories
{
    /// <summary>
    /// <see cref="IViewDataComponentFactory"/> defines the interface with
    /// which to create the ViewModels necessary for the
    /// BoundaryParameterSpecific view.
    /// </summary>
    public interface IViewDataComponentFactory
    {
        /// <summary>
        /// Gets the <see cref="ForcingViewType"/> corresponding with the
        /// provided <paramref name="dataComponent"/>.
        /// </summary>
        /// <param name="dataComponent">The data component.</param>
        /// <returns>
        /// The <see cref="ForcingViewType"/> corresponding with the provided
        /// <paramref name="dataComponent"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dataComponent"/> is <c>null</c>.
        /// </exception>
        ForcingViewType GetForcingType(IBoundaryConditionDataComponent dataComponent);

        /// <summary>
        /// Gets the <see cref="SpatialDefinitionViewType"/> corresponding with
        /// the provided <paramref name="dataComponent"/>.
        /// </summary>
        /// <param name="dataComponent">The data component.</param>
        /// <returns>
        /// The <see cref="SpatialDefinitionViewType"/> corresponding with the provided
        /// <paramref name="dataComponent"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dataComponent"/> is <c>null</c>.
        /// </exception>
        SpatialDefinitionViewType GetSpatialDefinition(IBoundaryConditionDataComponent dataComponent);

        /// <summary>
        /// Constructs the <see cref="IParametersSettingsViewModel"/> corresponding
        /// with the provided <paramref name="dataComponent"/>.
        /// </summary>
        /// <param name="dataComponent">The data component.</param>
        /// <returns>
        /// The <see cref="IParametersSettingsViewModel"/> corresponding
        /// with the provided <paramref name="dataComponent"/>.
        /// </returns>
        IParametersSettingsViewModel ConstructParametersSettingsViewModel(IBoundaryConditionDataComponent dataComponent);

        /// <summary>
        /// Constructs the <see cref="IBoundaryConditionDataComponent"/> corresponding
        /// with the <paramref name="forcingType"/> and <paramref name="spatialDefinition"/>.
        /// </summary>
        /// <param name="forcingType">The <see cref="ForcingViewType"/>.</param>
        /// <param name="spatialDefinition">The <see cref="SpatialDefinitionViewType"/>.</param>
        /// <returns>
        /// The <see cref="IBoundaryConditionDataComponent"/> corresponding
        /// with the <paramref name="forcingType"/> and <paramref name="spatialDefinition"/>.
        /// </returns>
        IBoundaryConditionDataComponent ConstructBoundaryConditionDataComponent(ForcingViewType forcingType, 
                                                                                SpatialDefinitionViewType spatialDefinition);
    }
}