using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO.HydFileElement
{
    public static class HydFileStringValueParser
    {
        /// <summary>
        /// Describes the pattern "[some text][optional layer type post fix that is whitespace separated]".
        /// </summary>
        private const string geometryDefinitionRegex = @"(?<geometryDefinition>\S+)(\s+(?<layerType>\S+))?";

        private static readonly IDictionary<string, HydroDynamicModelType> HydFileGeometryDefinitionMapping =
            new Dictionary
                <string, HydroDynamicModelType>
                {
                    {"unstructured", HydroDynamicModelType.Unstructured},
                    {"curvilinear-grid", HydroDynamicModelType.Curvilinear},
                    {"finite-elements", HydroDynamicModelType.FiniteElements},
                    {"network", HydroDynamicModelType.HydroNetwork}
                };

        private static readonly IDictionary<string, LayerType> HydFileLayerTypeMapping = new Dictionary
            <string, LayerType>
            {
                {"", LayerType.Sigma},
                {"sigma", LayerType.Sigma},
                {"z-layers", LayerType.ZLayer}
            };

        public static T Parse<T>(string textToParse, CultureInfo culture = null)
        {
            culture = culture ?? CultureInfo.InvariantCulture;

            object result = null;

            if (typeof(T) == typeof(int))
            {
                result = ParseInt(textToParse, culture);
            }

            if (typeof(T) == typeof(double))
            {
                result = ParseDouble(textToParse, culture);
            }

            if (typeof(T) == typeof(string))
            {
                result = ParseString(textToParse);
            }

            if (typeof(T) == typeof(int[]))
            {
                result = ParseArray<int>(textToParse, culture);
            }

            if (typeof(T) == typeof(double[]))
            {
                result = ParseArray<double>(textToParse, culture);
            }

            if (typeof(T) == typeof(DateTime))
            {
                result = ParseDateTime(textToParse, culture);
            }

            if (typeof(T) == typeof(TimeSpan))
            {
                result = ParseTimeSpan(textToParse);
            }

            if (typeof(T) == typeof(HydroDynamicModelType))
            {
                result = ParseHydFileGeometryDefinition(textToParse);
            }

            if (typeof(T) == typeof(LayerType))
            {
                result = ParseHydFileLayerType(textToParse);
            }

            return (T) result;
        }

        private static LayerType ParseHydFileLayerType(string textToParse)
        {
            if (string.IsNullOrWhiteSpace(textToParse))
            {
                return LayerType.Undefined;
            }

            Match match = Regex.Match(textToParse.ToLower(), geometryDefinitionRegex);
            if (match.Success)
            {
                string lowerTextToParse = match.Groups["layerType"].Value;
                if (HydFileLayerTypeMapping.ContainsKey(lowerTextToParse))
                {
                    return HydFileLayerTypeMapping[lowerTextToParse];
                }
            }

            throw new FormatException(string.Format("The text '{0}' is not a valid geometry definition string.",
                                                    textToParse));
        }

        private static HydroDynamicModelType ParseHydFileGeometryDefinition(string textToParse)
        {
            if (string.IsNullOrWhiteSpace(textToParse))
            {
                return HydroDynamicModelType.Undefined;
            }

            Match match = Regex.Match(textToParse.ToLower(), geometryDefinitionRegex);
            if (match.Success)
            {
                string lowerTextToParse = match.Groups["geometryDefinition"].Value;
                if (HydFileGeometryDefinitionMapping.ContainsKey(lowerTextToParse))
                {
                    return HydFileGeometryDefinitionMapping[lowerTextToParse];
                }
            }

            throw new FormatException(string.Format("The text '{0}' is not a valid geometry definition string.",
                                                    textToParse));
        }

        private static TimeSpan ParseTimeSpan(string textToParse)
        {
            if (string.IsNullOrWhiteSpace(textToParse))
            {
                return new TimeSpan();
            }

            // Regex using capture groups to match to the pattern 'ddddddddhhmmss':
            Match match = Regex.Match(textToParse, @"'(?<day>\d{8})(?<hour>\d{2})(?<minute>\d{2})(?<second>\d{2})'");
            if (!match.Success)
            {
                throw new FormatException(
                    string.Format("Timespan ({0}) is not given in expected format of 'ddddddddhhmmss'.", textToParse));
            }

            int day = int.Parse(match.Groups["day"].Value);
            int hour = int.Parse(match.Groups["hour"].Value);
            int minute = int.Parse(match.Groups["minute"].Value);
            int second = int.Parse(match.Groups["second"].Value);

            try
            {
                return new TimeSpan(day, hour, minute, second);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new FormatException("Timespan must be smaller or equal to value '10675199024805'.", exception);
            }
        }

        private static DateTime ParseDateTime(string textToParse, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(textToParse))
            {
                return DateTime.MinValue;
            }

            DateTime result;
            if (DateTime.TryParseExact(textToParse.Trim('\''), new[]
            {
                "yyyyMMddHHmmss",
                "yyyyMMdd HHmmss",
                "HH:mm:ss,dd-MM-yyyy",
                "HH:mm:ss, dd-MM-yyyy"
            }, culture, DateTimeStyles.None, out result))
            {
                return result;
            }

            throw new FormatException(string.Format("'{0}' could not be converted to a valid DateTime", textToParse));
        }

        private static T[] ParseArray<T>(string textToParse, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(textToParse))
            {
                return new T[0];
            }

            string[] items = Regex.Split(textToParse, @"\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            var array = new T[items.Length];
            for (var i = 0; i < items.Length; i++)
            {
                array[i] = Parse<T>(items[i], culture);
            }

            return array;
        }

        private static string ParseString(string textToParse)
        {
            if (string.IsNullOrWhiteSpace(textToParse) || textToParse.Equals("none"))
            {
                return string.Empty;
            }

            Match match = Regex.Match(textToParse, "'(?<fileName>.+)'");
            if (match.Success)
            {
                return match.Groups["fileName"].Value.Trim();
            }

            throw new FormatException("No filename between \' characters is given.");
        }

        private static double ParseDouble(string textToParse, CultureInfo culture)
        {
            double result;

            if (double.TryParse(textToParse, NumberStyles.Any, culture, out result))
            {
                return result;
            }

            string message = string.Format("Value ({0}) must fall within the range [{1}, {2}].", textToParse,
                                           double.MinValue, double.MaxValue);
            throw new FormatException(message);
        }

        private static int ParseInt(string textToParse, CultureInfo culture)
        {
            int result;
            if (string.IsNullOrWhiteSpace(textToParse))
            {
                return 0;
            }

            if (int.TryParse(textToParse, NumberStyles.Any, culture, out result))
            {
                return result;
            }

            string message = string.Format("Value ({0}) must fall within the range [{1}, {2}].", textToParse,
                                           int.MinValue, int.MaxValue);
            throw new FormatException(message);
        }
    }
}