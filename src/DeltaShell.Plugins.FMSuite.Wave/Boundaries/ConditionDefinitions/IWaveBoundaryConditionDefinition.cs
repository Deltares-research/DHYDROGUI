using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
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
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        IBoundaryConditionShape Shape { get; set; }

        /// <summary>
        /// Gets or sets the type of the period.
        /// </summary>
        /// <value>
        /// The type of the period.
        /// </value>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when <paramref name="value"/> is not defined.
        /// </exception>
        BoundaryConditionPeriodType PeriodType { get; set; }

        /// <summary>
        /// Gets or sets the type of the directional spreading.
        /// </summary>
        /// <value>
        /// The type of the directional spreading.
        /// </value>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when <paramref name="value"/> is not defined.
        /// </exception>
        BoundaryConditionDirectionalSpreadingType DirectionalSpreadingType { get; set; }

        /// <summary>
        /// Gets or sets the data component.
        /// </summary>
        /// <value>
        /// The <see cref="IBoundaryConditionDataComponent"/>.
        /// </value>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        IBoundaryConditionDataComponent DataComponent { get; set; }
    }
}