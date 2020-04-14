using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
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
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="dataComponent"/> is <c>null</c>.
        /// </exception>
        ForcingViewType GetForcingType(ISpatiallyDefinedDataComponent dataComponent);

        /// <summary>
        /// Gets the <see cref="SpatialDefinitionViewType"/> corresponding with
        /// the provided <paramref name="dataComponent"/>.
        /// </summary>
        /// <param name="dataComponent">The data component.</param>
        /// <returns>
        /// The <see cref="SpatialDefinitionViewType"/> corresponding with the provided
        /// <paramref name="dataComponent"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="dataComponent"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when <paramref name="dataComponent"/> is of an unsupported type.
        /// </exception>
        SpatialDefinitionViewType GetSpatialDefinition(ISpatiallyDefinedDataComponent dataComponent);

        /// <summary>
        /// Gets the <see cref="DirectionalSpreadingViewType"/> corresponding with
        /// the provided <paramref name="dataComponent"/>.
        /// </summary>
        /// <param name="dataComponent">The data component.</param>
        /// <returns>
        /// The <see cref="DirectionalSpreadingViewType"/> corresponding with the provided
        /// <paramref name="dataComponent"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="dataComponent"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when <paramref name="dataComponent"/> is of an unsupported type.
        /// </exception>
        DirectionalSpreadingViewType GetDirectionalSpreadingViewType(ISpatiallyDefinedDataComponent dataComponent);

        /// <summary>
        /// Gets whether the boundary wide parameters should be visible for the given <paramref name="dataComponent"/>.
        /// </summary>
        /// <param name="dataComponent">The current data component.</param>
        /// <returns>
        /// A boolean.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="dataComponent"/> is <c>null</c>.
        /// </exception>
        bool GetAreBoundaryWideParametersVisible(ISpatiallyDefinedDataComponent dataComponent);

        /// <summary>
        /// Constructs the <see cref="IParametersSettingsViewModel"/> corresponding
        /// with the provided <paramref name="dataComponent"/>.
        /// </summary>
        /// <param name="dataComponent">The data component.</param>
        /// <returns>
        /// The <see cref="IParametersSettingsViewModel"/> corresponding
        /// with the provided <paramref name="dataComponent"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="dataComponent"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when <paramref name="dataComponent"/> is of an unsupported type.
        /// </exception>
        IParametersSettingsViewModel ConstructParametersSettingsViewModel(ISpatiallyDefinedDataComponent dataComponent);

        /// <summary>
        /// Constructs the <see cref="ISpatiallyDefinedDataComponent"/> corresponding
        /// with the <paramref name="forcingType"/> and <paramref name="spatialDefinition"/>.
        /// </summary>
        /// <param name="forcingType">The <see cref="ForcingViewType"/>.</param>
        /// <param name="spatialDefinition">The <see cref="SpatialDefinitionViewType"/>.</param>
        /// <param name="spreadingType">The <see cref="DirectionalSpreadingViewType"/>.</param>
        /// <returns>
        /// The <see cref="ISpatiallyDefinedDataComponent"/> corresponding
        /// with the <paramref name="forcingType"/> and <paramref name="spatialDefinition"/>.
        /// </returns>
        ISpatiallyDefinedDataComponent ConstructBoundaryConditionDataComponent(ForcingViewType forcingType,
                                                                                SpatialDefinitionViewType spatialDefinition,
                                                                                DirectionalSpreadingViewType spreadingType);

        /// <summary>
        /// Converts the provided <paramref name="currentDataComponent"/> to a similar
        /// <see cref="ISpatiallyDefinedDataComponent"/> with the specified
        /// <paramref name="newSpreadingType"/>.
        /// </summary>
        /// <param name="currentDataComponent">The current data component.</param>
        /// <param name="newSpreadingType">New type of the spreading.</param>
        /// <returns>
        /// A <see cref="ISpatiallyDefinedDataComponent"/> equal to <paramref name="currentDataComponent"/>
        /// but with the specified <paramref name="newSpreadingType"/>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="currentDataComponent"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when <paramref name="currentDataComponent"/> is of an unsupported type.
        /// </exception>
        ISpatiallyDefinedDataComponent ConvertBoundaryConditionDataComponentSpreadingType(
            ISpatiallyDefinedDataComponent currentDataComponent,
            DirectionalSpreadingViewType newSpreadingType);
    }
}