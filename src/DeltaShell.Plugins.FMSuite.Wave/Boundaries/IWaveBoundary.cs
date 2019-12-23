using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries
{
    /// <summary>
    /// <see cref="IWaveBoundary"/> defines the data of a single wave boundary.
    /// </summary>
    public interface IWaveBoundary
    {
        /// <summary>
        /// Gets or sets the name of this <see cref="IWaveBoundary"/>.
        /// </summary>
        /// <value>
        /// The name of this <see cref="IWaveBoundary"/>
        /// </value>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is null or empty.
        /// </exception>
        string Name { get; set; }

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

        /// <summary>
        /// Gets the condition definition.
        /// </summary>
        /// <value>
        /// The condition definition.
        /// </value>
        IWaveBoundaryConditionDefinition ConditionDefinition { get; }
    }
}