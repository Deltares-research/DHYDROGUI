using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using DelftTools.Utils.NetCdf;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public static class NetCdfFileConventionChecker
    {
        private const double supportedCfConvention = 1.6;
        private const double supportedUgridConvention = 1.0;
        private const string cfString = "CF-";
        private const string ugridString = "UGRID-";

        /// <summary>
        /// Determines whether the file at specified <paramref name="path"/> has the supported convention.
        /// </summary>
        /// <param name="path"> The path. </param>
        /// <returns>
        /// <c> true </c> if file has the supported convention; otherwise, <c> false </c>.
        /// </returns>
        /// <remarks>
        /// Supported UGrid convention is 1.0 and higher.
        /// Supported CF convention is 1.6 and higher.
        /// </remarks>
        /// <exception cref="FileNotFoundException"> Thrown when <paramref name="path"/> does not exist. </exception>
        public static bool HasSupportedConvention(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            return NetCdfFileReaderHelper.DoWithNetCdfFile(path, HasSupportedConvention);
        }

        private static bool HasSupportedConvention(NetCdfFile file)
        {
            NetCdfAttribute conventionAttribute = file.GetGlobalAttribute(NetCdfConventions.Attributes.Conventions);
            if (conventionAttribute == null)
            {
                return false;
            }

            var attributeValue = conventionAttribute.Value.ToString();

            if (TryGetConvention(attributeValue, cfString, out double cfConvention))
            {
                if (cfConvention < supportedCfConvention)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            if (TryGetConvention(attributeValue, ugridString, out double ugridConvention))
            {
                if (ugridConvention < supportedUgridConvention)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private static bool TryGetConvention(string attributeValue, string conventionPrefix, out double convention)
        {
            convention = 0.0;

            Match match = Regex.Match(attributeValue, conventionPrefix + @"\d\.\d");
            if (!match.Success)
            {
                return false;
            }

            string conventionString = match.Value.Substring(conventionPrefix.Length);

            return double.TryParse(conventionString,
                                   NumberStyles.AllowDecimalPoint,
                                   CultureInfo.InvariantCulture,
                                   out convention);
        }
    }
}