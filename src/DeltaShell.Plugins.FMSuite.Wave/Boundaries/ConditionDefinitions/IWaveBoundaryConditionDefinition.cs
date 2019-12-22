using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions
{
    /// <summary>
    /// <see cref="IWaveBoundaryConditionDefinition"/> defines the condition 
    /// attributes of a <see cref="IWaveBoundary"/>.
    /// </summary>
    public interface IWaveBoundaryConditionDefinition
    {
        /// <summary>
        /// Gets or sets the condition shape.
        /// </summary>
        /// <value>
        /// The shape
        /// </value>
        IBoundaryConditionShape Shape { get; set; }

        /// <summary>
        /// Gets or sets the type of the period.
        /// </summary>
        /// <value>
        /// The type of the period.
        /// </value>
        BoundaryConditionPeriodType PeriodType { get; set; }

        /// <summary>
        /// Gets or sets the type of the directional spreading.
        /// </summary>
        /// <value>
        /// The type of the directional spreading.
        /// </value>
        BoundaryConditionDirectionalSpreadingType DirectionalSpreadingType { get; set; }
    }
}