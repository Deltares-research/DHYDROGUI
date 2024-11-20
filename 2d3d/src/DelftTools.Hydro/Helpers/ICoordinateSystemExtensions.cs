using System;
using GeoAPI.Extensions.CoordinateSystems;

namespace DelftTools.Hydro.Helpers
{
    public static class ICoordinateSystemExtensions
    {
        /// Checks whether the values of this instance is equal to the values of another instance.
        /// Only parameters used for coordinate system are used for comparison.
        public static bool EqualsTo(this ICoordinateSystem sourceCoordinateSystem,
                                    ICoordinateSystem destinationCoordinateSystem)
        {
            if (sourceCoordinateSystem == null && destinationCoordinateSystem == null)
            {
                return true;
            }

            if (destinationCoordinateSystem == null)
            {
                return false;
            }

            if (sourceCoordinateSystem == null)
            {
                return false;
            }

            if (sourceCoordinateSystem.Name != destinationCoordinateSystem.Name)
            {
                return false;
            }

            if (sourceCoordinateSystem.Abbreviation != destinationCoordinateSystem.Abbreviation)
            {
                return false;
            }

            if (sourceCoordinateSystem.Authority != destinationCoordinateSystem.Authority)
            {
                return false;
            }

            if (sourceCoordinateSystem.AuthorityCode != destinationCoordinateSystem.AuthorityCode)
            {
                return false;
            }

            if (sourceCoordinateSystem.PROJ4 != destinationCoordinateSystem.PROJ4)
            {
                return false;
            }

            if (sourceCoordinateSystem.Remarks != destinationCoordinateSystem.Remarks)
            {
                return false;
            }

            if (sourceCoordinateSystem.WKT != destinationCoordinateSystem.WKT)
            {
                return false;
            }

            if (Math.Abs(sourceCoordinateSystem.GetSemiMajor() - destinationCoordinateSystem.GetSemiMajor()) >
                double.Epsilon)
            {
                return false;
            }

            if (Math.Abs(sourceCoordinateSystem.GetSemiMinor() - destinationCoordinateSystem.GetSemiMinor()) >
                double.Epsilon)
            {
                return false;
            }

            if (sourceCoordinateSystem.Dimension != destinationCoordinateSystem.Dimension)
            {
                return false;
            }

            if (sourceCoordinateSystem.DefaultEnvelope != destinationCoordinateSystem.DefaultEnvelope)
            {
                return false;
            }

            return true;
        }
    }
}