using System;
using System.ComponentModel;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.DataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions
{
    /// <summary>
    /// <see cref="WaveBoundaryConditionDefinition"/> implements the condition 
    /// attributes of a <see cref="IWaveBoundary"/>.
    /// </summary>
    /// <seealso cref="IWaveBoundaryConditionDefinition" />
    public class WaveBoundaryConditionDefinition : IWaveBoundaryConditionDefinition
    {
        /// <summary>
        /// Creates a new instance of the <see cref="WaveBoundaryConditionDefinition"/>.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="periodType">Type of the period.</param>
        /// <param name="directionalSpreadingType">Type of the directional spreading.</param>
        /// <param name="dataComponent">The data component.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="shape"/> or <paramref name="dataComponent"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when <paramref name="periodType"/> or <paramref name="directionalSpreadingType"/>
        /// is not defined.
        /// </exception>
        public WaveBoundaryConditionDefinition(IBoundaryConditionShape shape,
                                               BoundaryConditionPeriodType periodType,
                                               BoundaryConditionDirectionalSpreadingType directionalSpreadingType,
                                               IBoundaryConditionDataComponent dataComponent)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            PeriodType = periodType;
            DirectionalSpreadingType = directionalSpreadingType;
            DataComponent = dataComponent ?? throw new ArgumentNullException(nameof(dataComponent));
        }


        public IBoundaryConditionShape Shape 
        { 
            get => shape; 
            set => shape = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IBoundaryConditionShape shape;

        public BoundaryConditionPeriodType PeriodType
        {
            get => periodType;
            set => periodType = Enum.IsDefined(typeof(BoundaryConditionPeriodType), value)
                                    ? value
                                    : throw new InvalidEnumArgumentException($"Undefined enum value: {value}"); 
        }

        private BoundaryConditionPeriodType periodType;

        public BoundaryConditionDirectionalSpreadingType DirectionalSpreadingType
        {
            get => spreadingType; 
            set => spreadingType = Enum.IsDefined(typeof(BoundaryConditionDirectionalSpreadingType), value)
                                       ? value
                                       : throw new InvalidEnumArgumentException($"Undefined enum value: {value}"); 
        }

        private BoundaryConditionDirectionalSpreadingType spreadingType;

        public IBoundaryConditionDataComponent DataComponent { get; set; }
    }
}