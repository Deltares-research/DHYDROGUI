using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions
{
    /// <summary>
    /// Represents a support point defined on a <see cref="IWaveBoundary"/>.
    /// </summary>
    public class SupportPoint
    {
        public double Distance { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPoint"/> class.
        /// </summary>
        /// <param name="distance">The distance from the start index of the <see cref="IWaveBoundaryGeometricDefinition"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="distance"/> is smaller than 0.
        /// </exception>
        public SupportPoint(double distance)
        {
            if (distance < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(distance));
            }

            Distance = distance;
        }
    }
}
