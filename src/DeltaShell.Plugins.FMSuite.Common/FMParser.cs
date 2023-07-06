using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common
{
    public static class FMParser
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FMParser));

        private static readonly string[] stringDateFormatsFrom =
        {
            "yyyyMMddHHmmss",
            "yyyyMMdd",
            "yyyy-MM-dd"
        };

        /// <summary>
        /// Returns the C# value <see cref="Type"/> for a property in the schema csv file.
        /// </summary>
        /// <param name="propertyKeyName"> The name of property as it appears in the csv schema file. </param>
        /// <param name="typeField"> The value type as it appears in the csv schema file. </param>
        /// <param name="captionField"> The caption of how <paramref name="propertyKeyName"/> should be shown in the UI. </param>
        /// <param name="schemaFileName"> The schema file name. </param>
        /// <param name="lineNumber"> The line being evaluated in the schema file. </param>
        /// <returns> The C# value <see cref="Type"/> of the variable. </returns>
        /// <exception cref="FormatException"> When there is a syntax error in the schema file. </exception>
        /// <exception cref="ArgumentException"> When <paramref name="typeField"/> cannot be translated to a type. </exception>
        public static Type GetClrType(string propertyKeyName, string typeField, ref string captionField,
                                      string schemaFileName, int lineNumber)
        {
            Type dataType;

            string typeFieldLower = typeField.ToLower();

            if (typeFieldLower.Equals("integer"))
            {
                dataType = typeof(int);
            }
            else if (typeFieldLower.Equals("double"))
            {
                dataType = typeof(double);
            }
            else if (typeFieldLower.Equals("doublearray"))
            {
                dataType = typeof(IList<double>);
            }
            else if (typeFieldLower.Equals("timeframe"))
            {
                dataType = typeof(IList<double>);
            }
            else if (typeFieldLower.Equals("interval"))
            {
                dataType = typeof(TimeSpan);
            }
            else if (typeFieldLower.Equals("datetime"))
            {
                dataType = typeof(DateTime);
            }
            else if (typeFieldLower.Equals("dateonly"))
            {
                dataType = typeof(DateOnly);
            }
            else if (typeFieldLower.Equals("string") || typeFieldLower.Equals("filename"))
            {
                dataType = typeof(string);
            }
            else if (typeFieldLower.Equals("multipleentriesfilename")
            ) /* Still only one string per entry, but multiple occurrences of the property name in the file*/
            {
                dataType = typeof(IList<string>);
            }
            else if (typeFieldLower.Equals("steerable"))
            {
                dataType = typeof(Steerable);
            }
            else if (typeFieldLower.Contains("|"))
            {
                if ((typeField.Equals("0|1") ||
                     typeField.Equals("1|0")) && !captionField.Contains("|") ||
                    typeField.Equals("true|false"))
                {
                    dataType = typeof(bool);
                }
                else
                {
                    string[] captionFields = captionField.Split(':');
                    if (captionFields.Length != 2)
                    {
                        throw new FormatException(
                            string.Format("Invalid caption field {0} on line {1} of file {2}",
                                          captionField, lineNumber, schemaFileName));
                    }

                    captionField = captionFields[0];
                    string[] captionChoices = captionFields[1].Split('|');
                    string[] typeChoices = typeField.Split('|');
                    if (captionChoices.Length != typeChoices.Length)
                    {
                        throw new FormatException(
                            string.Format(
                                "Inconsistent caption and type field for {0} on line {1} of file {2}",
                                captionField, lineNumber, schemaFileName));
                    }

                    // if int enum, cast to int, otherwise keep as string
                    int[] typeChoicesInts = int.TryParse(typeChoices[0], out int _)
                                                ? typeChoices.Select(int.Parse).ToArray()
                                                : Enumerable.Range(0, typeChoices.Length).ToArray();

                    dataType = DynamicTypeUtils.CreateDynamicEnum(propertyKeyName, typeChoicesInts, captionChoices,
                                                                  typeChoices);
                }
            }
            else
            {
                throw new ArgumentException(string.Format("Invalid type field on line {0} of file {1}", lineNumber,
                                                          schemaFileName));
            }

            return dataType;
        }

        public static string ToString(object obj, Type dataType)
        {
            if (dataType == typeof(string))
            {
                return (string) obj;
            }

            if (dataType == typeof(double))
            {
                return ((double) obj).ToString(CultureInfo.InvariantCulture);
            }

            if (dataType == typeof(int))
            {
                return ((int) obj).ToString(CultureInfo.InvariantCulture);
            }

            if (dataType == typeof(bool))
            {
                return (bool) obj ? "1" : "0";
            }

            if (dataType == typeof(DateTime))
            {
                return ((DateTime) obj).ToString("yyyyMMddHHmmss");
            }
            if (dataType == typeof(DateOnly))
            {
                return ((DateOnly)obj).ToString("yyyyMMdd");
            }

            if (dataType == typeof(TimeSpan))
            {
                if (obj is TimeSpan && ((TimeSpan) obj).Ticks != 0)
                {
                    double nrOfTicks = 1.0 * ((TimeSpan) obj).Ticks;
                    return (nrOfTicks / TimeSpan.TicksPerSecond).ToString(CultureInfo.InvariantCulture);
                }

                return "";
            }

            if (dataType == typeof(IList<double>))
            {
                return string.Join(" ", ((IList<double>) obj).Select(d => d.ToString(CultureInfo.InvariantCulture)));
            }

            if (dataType == typeof(IList<string>))
            {
                return string.Join(" ", (IList<string>) obj);
            }

            if (dataType.IsEnum)
            {
                return ((Enum) obj).GetDisplayName();
            }

            if (dataType == typeof(Steerable))
            {
                var steerable = (Steerable) obj;
                switch (steerable.Mode)
                {
                    case SteerableMode.ConstantValue:
                        return ToString(steerable.ConstantValue, typeof(double));
                    case SteerableMode.TimeSeries:
                        return steerable.TimeSeriesFilename;
                    case SteerableMode.External:
                        return "REALTIME";
                }
            }

            throw new NotImplementedException("Unsupported data type: " + dataType);
        }

        /// <summary>
        /// Parses a string for a given value type.
        /// </summary>
        /// <param name="str"> String to be parsed. </param>
        /// <param name="dataType"> Expected data type represented by <paramref name="str"/>. </param>
        /// <returns> The parsed object. </returns>
        /// <seealso cref="FromString{T}(string)"/>
        /// <exception cref="ArgumentNullException">
        /// Can occur for some <paramref name="dataType"/> when <paramref name="str"/>
        /// is null.
        /// </exception>
        /// <exception cref="FormatException">
        /// Can occur for some <paramref name="dataType"/> when <paramref name="str"/> is in
        /// invalid format.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Can occur for some <paramref name="dataType"/> when <paramref name="str"/>
        /// represent a value that is too big or too small.
        /// </exception>
        public static object FromString(string str, Type dataType)
        {
            if (dataType == typeof(string))
            {
                return str;
            }

            if (dataType == typeof(IList<string>))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return new List<string>();
                }

                List<string> strings = str.Split(new[]
                {
                    ' ',
                    '\t'
                }, StringSplitOptions.RemoveEmptyEntries).ToList();
                return strings;
            }

            if (dataType == typeof(double))
            {
                return ParseDoubleValue(str);
            }

            if (dataType == typeof(int))
            {
                return int.Parse(str);
            }

            if (dataType == typeof(bool))
            {
                string strTrim = str.Trim();
                return strTrim.Equals("1") ||
                       strTrim.Equals("true", StringComparison.InvariantCultureIgnoreCase); //simple version
            }

            if (dataType == typeof(DateTime))
            {
                return ParseFMDateTime(str);
            }

            if (dataType == typeof(DateOnly))
            {
                return ParseFMDateOnly(str);
            }

            if (dataType == typeof(TimeSpan))
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    double valueAsDouble = ParseDoubleValue(str);
                    var ticks = (long) (TimeSpan.TicksPerSecond * valueAsDouble);
                    return new TimeSpan(ticks);
                }

                return new TimeSpan(0);
            }

            if (dataType == typeof(IList<double>))
            {
                if (str == null)
                {
                    throw new ArgumentNullException(nameof(str));
                }

                string trimmedString = str.Replace('d', 'e').Trim();
                if (string.IsNullOrEmpty(trimmedString))
                {
                    return new List<double>(0);
                }

                string[] numbers = trimmedString.Split(new[]
                {
                    ' ',
                    '\t'
                }, StringSplitOptions.RemoveEmptyEntries);
                var resultList = new List<double>();
                foreach (string number in numbers)
                {
                    try
                    {
                        resultList.Add(ParseDoubleValue(number));
                    }
                    // ArgumentNullException won't occur in this scenario
                    catch (FormatException e)
                    {
                        Log.WarnFormat("Value '{0}' in collection cannot be read (Cause: {1}) and is skipped. ", number,
                                       e.Message);
                        resultList.Add(double.NaN);
                    }
                    catch (OverflowException e)
                    {
                        Log.WarnFormat("Value '{0}' in collection cannot be read (Cause: {1}) and is skipped. ", number,
                                       e.Message);
                        resultList.Add(double.NaN);
                    }
                }

                return resultList;
            }

            if (dataType.IsEnum)
            {
                object enumValue = GetEnumValueFromDisplayName(dataType, str);
                if (enumValue == null)
                {
                    throw new FormatException($"Value of '{str}' not valid.");
                }

                return enumValue;
            }

            if (dataType == typeof(Steerable))
            {
                throw new NotImplementedException(
                    "Try to parse the value of a Steerable as double, and when that fails as string.");
            }

            throw new NotImplementedException("Unsupported data type " + dataType);
        }

        /// <summary>
        /// Parses a string for a given value type.
        /// </summary>
        /// <typeparam name="T"> Expected data type represented by <paramref name="str"/>. </typeparam>
        /// <param name="str"> String to be parsed. </param>
        /// <returns> The parsed object. </returns>
        /// <remarks> This method cannot be used for enumerations </remarks>
        /// <exception cref="ArgumentNullException">
        /// Can occur for some <typeparamref name="T"/> when <paramref name="str"/> is
        /// null.
        /// </exception>
        /// <exception cref="FormatException">
        /// Can occur for some <typeparamref name="T"/> when <paramref name="str"/> is in
        /// invalid format.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Can occur for some <typeparamref name="T"/> when <paramref name="str"/>
        /// represent a value that is too big or too small.
        /// </exception>
        /// <seealso cref="FromString(string,Type)"/>
        public static T FromString<T>(string str)
        {
            Type type = typeof(T);
            if (type.IsEnum)
            {
                throw new NotImplementedException("Use FromString(string str, Type dataType) instead!");
            }

            return (T) FromString(str, type);
        }

        /// <summary>
        /// Parses a string to <see cref="DateTime"/> expecting 'yyyyMMdd', 'yyyy-MM-dd' or 'yyyyMMddHHmmss' format.
        /// </summary>
        /// <param name="valueAsString"> Value to be parsed. </param>
        /// <returns> Parsed date-time, or <see cref="DateTime.Now"/> when <paramref name="valueAsString"/> empty or null. </returns>
        /// <exception cref="FormatException">
        /// When <paramref name="valueAsString"/> does not represent a supported date-time
        /// format.
        /// </exception>
        private static DateTime ParseFMDateTime(string valueAsString)
        {
            if (string.IsNullOrEmpty(valueAsString))
            {
                return DateTime.Now;
            }

            if (!DateTime.TryParseExact(valueAsString, stringDateFormatsFrom, null, DateTimeStyles.None, out DateTime value))
            {
                throw new FormatException("Unexpected date time format");
            }

            return value;
        }

        /// <summary>
        /// Parses a string to <see cref="DateOnly"/> expecting 'yyyyMMdd', 'yyyy-MM-dd' or 'yyyyMMddHHmmss' format.
        /// </summary>
        /// <param name="valueAsString">Value to be parsed.</param>
        /// <returns>Parsed date-time, or the date part of <see cref="DateTime.Now"/> when <paramref name="valueAsString"/> empty or null.</returns>
        /// <exception cref="FormatException">When <paramref name="valueAsString"/> does not represent a supported date-only format.</exception>
        private static DateOnly ParseFMDateOnly(string valueAsString)
        {
            if (String.IsNullOrEmpty(valueAsString))
            {
                var now = DateTime.Now;
                return new DateOnly(now.Year, now.Month, now.Day);
            }

            if (!DateOnly.TryParseExact(valueAsString, stringDateFormatsFrom, null, DateTimeStyles.None, out var value))
            {
                throw new FormatException("Unexpected date format");
            }
            return value;
        }
        
        /// <summary>
        /// Parses a string as double, expecting <see cref="CultureInfo.InvariantCulture"/> format.
        /// </summary>
        /// <param name="valuesAsString"> Value to be parsed. </param>
        /// <returns> Double value represented by <paramref name="valuesAsString"/> </returns>
        /// <exception cref="ArgumentNullException"> When <paramref name="valuesAsString"/> is null. </exception>
        /// <exception cref="FormatException">
        /// When <paramref name="valuesAsString"/> does not represent a number in a valid
        /// format.
        /// </exception>
        /// <exception cref="OverflowException">
        /// When <paramref name="valuesAsString"/> represents a number that is less than
        /// <see cref="double.MinValue"/> or greater than <see cref="double.MaxValue"/>.
        /// </exception>
        private static double ParseDoubleValue(string valuesAsString)
        {
            string actualString = valuesAsString.Replace("d-", "e-").Replace("d+", "e+");
            actualString = actualString.Replace("d", "e+");
            return double.Parse(actualString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// <remarks> This method is copied over from the framework where it was removed from. see issue D3DFMIQ-722 </remarks>
        /// Gets the display name of the enum value
        /// </summary>
        /// <param name="value"> The value you want the display name for </param>
        /// <returns> The display name, if any, else it's .ToString() </returns>
        private static string GetEnumDisplayName(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            var attributes =
                (DisplayNameAttribute[]) fi.GetCustomAttributes(
                    typeof(DisplayNameAttribute), false);
            return attributes.Length > 0 ? attributes[0].DisplayName : value.ToString();
        }

        /// <summary>
        ///     <remarks> This method is copied over from the framework where it was removed from. see issue D3DFMIQ-722 </remarks>
        /// </summary>
        /// <param name="enumType"> </param>
        /// <param name="displayNameString"> </param>
        /// <returns>
        /// Null if no match was found in <paramref name="enumType"/> with <paramref name="displayNameString"/>; Value
        /// otherwise.
        /// </returns>
        private static object GetEnumValueFromDisplayName(Type enumType, string displayNameString)
        {
            return Enum.GetValues(enumType)
                       .Cast<Enum>()
                       .FirstOrDefault(value => GetEnumDisplayName(value) == displayNameString);
        }
    }
}