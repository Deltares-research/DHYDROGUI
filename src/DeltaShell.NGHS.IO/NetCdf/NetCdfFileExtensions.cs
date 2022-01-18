using System;
using System.Text.RegularExpressions;
using DelftTools.Utils.Guards;
using DelftTools.Utils.NetCdf;
using GeoAPI.Extensions.CoordinateSystems;

namespace DeltaShell.NGHS.IO.NetCdf
{
    /// <summary>
    /// Extensions methods for <see cref="INetCdfFile"/>.
    /// </summary>
    public static class NetCdfFileExtensions
    {
        private const string projectedCoordinateSystemVariableName = "projected_coordinate_system";
        private const string wgsCoordinateSystemVariableName = "wgs84";
        private const string epsgAttributeName = "epsg";
        private const string conventionsAttributeName = "Conventions";
        private const string cfConventionStr = "CF";
        private const string uGridConventionStr = "UGRID";
        private const string deltaresConventionStr = "Deltares";

        private const string versionGroupName = "version";
        private const string versionRegex = @"(\d+\.)*\d+";

        /// <summary>
        /// Gets the <see cref="NetCdfConvention"/> for this <paramref name="netCdfFile"/>.
        /// </summary>
        /// <param name="netCdfFile"> The NetCDF file to get the convention from. </param>
        /// <returns>
        /// The <see cref="NetCdfConvention"/> if the "Conventions" attribute is available; otherwise, <c>null</c>.
        /// If a specific version is not provided or could not be determined, the corresponding property on the
        /// <see cref="NetCdfConvention"/> will be <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="netCdfFile"/> is <c>null</c>.
        /// </exception>
        public static NetCdfConvention GetConvention(this INetCdfFile netCdfFile)
        {
            Ensure.NotNull(netCdfFile, nameof(netCdfFile));

            NetCdfAttribute conventionAttribute = netCdfFile.GetGlobalAttribute(conventionsAttributeName);
            if (conventionAttribute == null)
            {
                return null;
            }

            var attributeValue = conventionAttribute.Value.ToString();

            Version cfConvention = GetVersion(attributeValue, cfConventionStr);
            Version uGridConvention = GetVersion(attributeValue, uGridConventionStr);
            Version deltaresConvention = GetVersion(attributeValue, deltaresConventionStr);

            return new NetCdfConvention(cfConvention, uGridConvention, deltaresConvention);
        }

        private static Version GetVersion(string attributeValue, string conventionStr)
        {
            if (!TryGetVersionString(attributeValue, conventionStr, out string versionStr))
            {
                return null;
            }

            return Version.TryParse(versionStr, out Version result) ? result : null;
        }

        private static bool TryGetVersionString(string sourceStr, string conventionStr, out string versionStr)
        {
            versionStr = null;

            var conventionRegex = $"{conventionStr}-(?<{versionGroupName}>{versionRegex})";

            Match match = Regex.Match(sourceStr, conventionRegex);
            if (!match.Success)
            {
                return false;
            }

            versionStr = match.Groups[versionGroupName].Value;
            return true;
        }

        /// <summary>
        /// Gets the <see cref="ICoordinateSystem"/> from the NetCDF file.
        /// The coordinate system is solely determined by the value of the 'epsg' attribute
        /// provided by the 'project_coordinate_system' or 'wgs84' variable in the NetCDF file.
        /// </summary>
        /// <param name="netCdfFile"> The NetCDF file to get the EPSG code from. </param>
        /// <param name="coordinateSystemFactory">
        /// The coordinate system factory that converts an EPSG code into a
        /// <see cref="ICoordinateSystem"/>.
        /// </param>
        /// <returns>
        /// The <see cref="ICoordinateSystem"/> if the EPSG code was successfully converted; otherwise, <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="netCdfFile"/> or <paramref name="coordinateSystemFactory"/> is <c>null</c>.
        /// </exception>
        public static ICoordinateSystem GetCoordinateSystem(this INetCdfFile netCdfFile, ICoordinateSystemFactory coordinateSystemFactory)
        {
            Ensure.NotNull(netCdfFile, nameof(netCdfFile));
            Ensure.NotNull(coordinateSystemFactory, nameof(coordinateSystemFactory));

            string epsgStr = netCdfFile.GetEpsg();

            return int.TryParse(epsgStr, out int epsg)
                       ? coordinateSystemFactory.CreateFromEPSG(epsg)
                       : null;
        }

        private static string GetEpsg(this INetCdfFile netCdfFile)
        {
            NetCdfVariable coordinateSystemVariable = netCdfFile.GetVariableByName(projectedCoordinateSystemVariableName) ??
                                                      netCdfFile.GetVariableByName(wgsCoordinateSystemVariableName);

            return coordinateSystemVariable != null
                       ? netCdfFile.GetAttributeValue(coordinateSystemVariable, epsgAttributeName)
                       : string.Empty;
        }
    }
}