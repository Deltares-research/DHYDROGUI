using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="IWaveBoundary"/> defines the data of a single wave boundary.
    /// </summary>
    public interface IWaveBoundary
    {
        /// <summary>
        /// Get the geometric definition.
        /// </summary>
        /// <value>
        /// The geometric definition.
        /// </value>
        /// <remarks>
        /// GeometricDefinition is never null.
        /// </remarks>
        IWaveBoundaryGeometricDefinition GeometricDefinition { get; }
    }
}