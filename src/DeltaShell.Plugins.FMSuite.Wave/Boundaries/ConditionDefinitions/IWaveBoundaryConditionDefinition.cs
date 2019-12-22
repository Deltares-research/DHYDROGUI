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
        /// Gets or sets the boundary condition shape.
        /// </summary>
        /// <value>
        /// The BoundaryConditionShape.
        /// </value>
        IBoundaryConditionShape BoundaryConditionShape { get; set; }
        
    }
}