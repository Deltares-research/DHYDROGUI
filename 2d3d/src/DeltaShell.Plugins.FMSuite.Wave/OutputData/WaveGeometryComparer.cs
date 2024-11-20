using System.Collections.Generic;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// This <see cref="WaveGeometryComparer"/> compares geometries within D-Waves.
    /// </summary>
    public sealed class WaveGeometryComparer : IEqualityComparer<IGeometry>
    {
        /// <summary>
        /// Compares the specified geometries with a tolerance of 1E-7.
        /// </summary>
        /// <param name="x">The first geometry.</param>
        /// <param name="y">The second geometry.</param>
        /// <returns>Whether or not the compared geometries are equal.</returns>
        public bool Equals(IGeometry x, IGeometry y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.EqualsExact(y, 1E-7);
        }

        public int GetHashCode(IGeometry obj) =>
            ((int) obj.EnvelopeInternal.Centre.X).GetHashCode() *
            ((int) obj.EnvelopeInternal.Centre.Y).GetHashCode();
    }
}