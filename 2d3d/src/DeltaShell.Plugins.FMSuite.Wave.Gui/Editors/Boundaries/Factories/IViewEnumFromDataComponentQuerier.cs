using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Enums;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Factories
{
    /// <summary>
    /// <see cref="IViewEnumFromDataComponentQuerier"/> defines the interface to convert
    /// a <see cref="ISpatiallyDefinedDataComponent"/> to its corresponding view enums.
    /// </summary>
    public interface IViewEnumFromDataComponentQuerier
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
    }
}