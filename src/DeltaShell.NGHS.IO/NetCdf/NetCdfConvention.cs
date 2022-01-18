using System;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.IO.NetCdf
{
    /// <summary>
    /// The <see cref="NetCdfConvention"/> in a NetCDF file as described by the "Conventions" attribute.
    /// </summary>
    public class NetCdfConvention
    {
        private const string cfConventionStr = "CF";
        private const string uGridConventionStr = "UGRID";
        private const string deltaresConventionStr = "Deltares";

        /// <summary>
        /// Initializes a new instance of the <see cref="NetCdfConvention"/> class.
        /// </summary>
        /// <param name="cf"> The CF convention. </param>
        /// <param name="uGrid"> The UGRID convention. </param>
        /// <param name="deltares"> The Deltares convention. </param>
        public NetCdfConvention(Version cf = null, Version uGrid = null, Version deltares = null)
        {
            Cf = cf;
            UGrid = uGrid;
            Deltares = deltares;
        }

        /// <summary>
        /// The CF (Climate and Forecast) convention.
        /// </summary>
        public Version Cf { get; }

        /// <summary>
        /// The UGRID (Unstructured Grid) convention.
        /// </summary>
        public Version UGrid { get; }

        /// <summary>
        /// The Deltares convention.
        /// </summary>
        public Version Deltares { get; }

        /// <summary>
        /// Whether or not the NetCDF convention satisfied the provided required convention.
        /// </summary>
        /// <param name="requiredConvention"> The required NetCDF convention. </param>
        /// <returns> True if the CF, UGRID and Deltares conventions with the required conventions; otherwise, false. </returns>
        public bool Satisfies(NetCdfConvention requiredConvention)
        {
            Ensure.NotNull(requiredConvention, nameof(requiredConvention));

            return Satisfies(Cf, requiredConvention.Cf) &&
                   Satisfies(UGrid, requiredConvention.UGrid) &&
                   Satisfies(Deltares, requiredConvention.Deltares);
        }

        public override string ToString()
        {
            return (ToString(cfConventionStr, Cf) +
                    ToString(uGridConventionStr, UGrid) +
                    ToString(deltaresConventionStr, Deltares)).TrimEnd();
        }

        private static bool Satisfies(Version version, Version requiredVersion)
        {
            if (requiredVersion == null)
            {
                return true;
            }

            return version != null && version >= requiredVersion;
        }

        private static string ToString(string convention, Version version)
        {
            return version != null ? $"{convention}-{version} " : string.Empty;
        }
    }
}