using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions
{
    /// <summary>
    /// <see cref="IWaveBoundaryConditionDefinition"/> defines the condition
    /// attributes of a <see cref="IWaveBoundary"/>.
    /// </summary>
    public interface IWaveBoundaryConditionDefinition : IVisitableWaveBoundaryConditionDefinition
    {
        /// <summary>
        /// Gets or sets the condition shape.
        /// </summary>
        /// <value>
        /// The shape
        /// </value>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        IBoundaryConditionShape Shape { get; set; }

        /// <summary>
        /// Gets or sets the type of the period.
        /// </summary>
        /// <value>
        /// The type of the period.
        /// </value>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="value"/> is not defined.
        /// </exception>
        BoundaryConditionPeriodType PeriodType { get; set; }

        /// <summary>
        /// Gets or sets the data component.
        /// </summary>
        /// <value>
        /// The <see cref="ISpatiallyDefinedDataComponent"/>.
        /// </value>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        ISpatiallyDefinedDataComponent DataComponent { get; set; }
    }
}