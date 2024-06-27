using System;
using System.ComponentModel;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions
{
    /// <summary>
    /// <see cref="WaveBoundaryConditionDefinition"/> implements the condition
    /// attributes of a <see cref="IWaveBoundary"/>.
    /// </summary>
    /// <seealso cref="IWaveBoundaryConditionDefinition"/>
    public class WaveBoundaryConditionDefinition : IWaveBoundaryConditionDefinition
    {
        private IBoundaryConditionShape shape;

        private BoundaryConditionPeriodType periodType;

        private ISpatiallyDefinedDataComponent dataComponent;

        /// <summary>
        /// Creates a new instance of the <see cref="WaveBoundaryConditionDefinition"/>.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="periodType">Type of the period.</param>
        /// <param name="dataComponent">The data component.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="shape"/> or <paramref name="dataComponent"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// Thrown when <paramref name="periodType"/> is not defined.
        /// </exception>
        public WaveBoundaryConditionDefinition(IBoundaryConditionShape shape,
                                               BoundaryConditionPeriodType periodType,
                                               ISpatiallyDefinedDataComponent dataComponent)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            PeriodType = periodType;
            DataComponent = dataComponent ?? throw new ArgumentNullException(nameof(dataComponent));
        }

        public IBoundaryConditionShape Shape
        {
            get => shape;
            set => shape = value ?? throw new ArgumentNullException(nameof(value));
        }

        public BoundaryConditionPeriodType PeriodType
        {
            get => periodType;
            set => periodType = Enum.IsDefined(typeof(BoundaryConditionPeriodType), value)
                                    ? value
                                    : throw new InvalidEnumArgumentException($"Undefined enum value: {value}");
        }

        public ISpatiallyDefinedDataComponent DataComponent
        {
            get => dataComponent;
            set => dataComponent = value ?? throw new ArgumentNullException(nameof(value));
        }

        public void AcceptVisitor(IBoundaryConditionVisitor visitor)
        {
            Ensure.NotNull(visitor, nameof(visitor));
            visitor.Visit(this);
        }
    }
}