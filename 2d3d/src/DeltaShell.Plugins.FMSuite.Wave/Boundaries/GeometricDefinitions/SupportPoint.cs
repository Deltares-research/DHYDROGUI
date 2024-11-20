using System;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// Represents a support point defined in a <see cref="IWaveBoundaryGeometricDefinition"/>.
    /// </summary>
    public class SupportPoint
    {
        private double distance;

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPoint"/> class.
        /// </summary>
        /// <param name="distance">The distance from the start index of the <see cref="IWaveBoundaryGeometricDefinition"/>.</param>
        /// <param name="geometricDefinition">The geometric definition.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="distance"/> is smaller than 0.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="geometricDefinition"/> is <c>null</c>.
        /// </exception>
        public SupportPoint(double distance, IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            if (distance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }

            Distance = distance;

            Ensure.NotNull(geometricDefinition, nameof(geometricDefinition));

            GeometricDefinition = geometricDefinition;
        }

        /// <summary>
        /// Gets the distance.
        /// </summary>
        /// <value>
        /// The distance from the start index of the <see cref="IWaveBoundaryGeometricDefinition"/>.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when setting with <paramref name="value"/> smaller than 0.
        /// </exception>
        public double Distance
        {
            get => distance;
            set
            {
                ValidateDistance(value);
                distance = value;
            }
        }

        /// <summary>
        /// Gets the geometric definition.
        /// </summary>
        /// <value>
        /// The geometric definition.
        /// </value>
        public IWaveBoundaryGeometricDefinition GeometricDefinition { get; }

        private static void ValidateDistance(double distance)
        {
            if (distance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }
        }
    }
}