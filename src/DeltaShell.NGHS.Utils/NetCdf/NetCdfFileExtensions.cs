using System;
using System.Text.RegularExpressions;
using DelftTools.Utils.Guards;
using DelftTools.Utils.NetCdf;

namespace DeltaShell.NGHS.Utils.NetCdf
{
    /// <summary>
    /// Extensions methods for <see cref="INetCdfFile"/>.
    /// </summary>
    public static class NetCdfFileExtensions
    {
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
    }
}