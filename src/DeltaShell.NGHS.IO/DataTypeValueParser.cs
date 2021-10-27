using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.NGHS.Utils;
using log4net;

namespace DeltaShell.NGHS.IO
{
    public static class DataTypeValueParser
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (DataTypeValueParser));

        /// <summary>
        /// Returns the C# value <see cref="Type"/> for a property in the schema csv file.
        /// </summary>
        /// <param name="propertyKeyName">The name of property as it appears in the csv schema file.</param>
        /// <param name="typeField">The value type as it appears in the csv schema file.</param>
        /// <param name="captionField">The caption of how <paramref name="propertyKeyName"/> should be shown in the UI.</param>
        /// <param name="schemaFileName">The schema file name.</param>
        /// <param name="lineNumber">The line being evaluated in the schema file.</param>
        /// <returns>The C# value <see cref="Type"/> of the variable.</returns>
        /// <exception cref="FormatException">When there is a syntax error in the schema file.</exception>
        /// <exception cref="ArgumentException">When <paramref name="typeField"/> cannot be translated to a type.</exception>
        public static Type GetClrType(string propertyKeyName, string typeField, ref string captionField, string schemaFileName, int lineNumber)
        {
            Type dataType;

            var typeFieldLower = typeField.ToLower();

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
            else if (typeFieldLower.Equals("string") || typeFieldLower.Equals("filename"))
            {
                dataType = typeof(string);
            }
            else if (typeFieldLower.Equals("multipleentriesfilename")) /* Still only one string per entry, but multiple occurrences of the property name in the file*/
            {
                dataType = typeof(IList<string>);
            }
            else if (typeFieldLower.Equals("steerable"))
            {
                dataType = typeof (Steerable);
            }
            else if (typeFieldLower.Contains("|"))
            {
                if ((typeField.Equals("0|1") ||
                     typeField.Equals("1|0")) && !captionField.Contains("|") ||
                     typeField.Equals("true|false"))
                {
                    dataType = typeof (bool);
                }
                else
                {
                    var captionFields = captionField.Split(':');
                    if (captionFields.Length != 2)
                    {
                        throw new FormatException(
                            String.Format("Invalid caption field {0} on line {1} of file {2}",
                                          captionField, lineNumber, schemaFileName));
                    }
                    captionField = captionFields[0];
                    var captionChoices = captionFields[1].Split('|');
                    var typeChoices = typeField.Split('|');
                    if (captionChoices.Length != typeChoices.Length)
                    {
                        throw new FormatException(
                            String.Format(
                                "Inconsistent caption and type field for {0} on line {1} of file {2}",
                                captionField, lineNumber, schemaFileName));
                    }

                    // if int enum, cast to int, otherwise keep as string
                    int tryOutValue;
                    int[] typeChoicesInts = int.TryParse(typeChoices[0], out tryOutValue)
                                                ? typeChoices.Select(int.Parse).ToArray()
                                                : Enumerable.Range(0, typeChoices.Length).ToArray();

                    lock (EnumPropertyTypes)
                    {
                        if (!EnumPropertyTypes.TryGetValue(propertyKeyName, out var type))
                        {
                            dataType = DynamicTypeUtils.CreateDynamicEnum(propertyKeyName, typeChoicesInts, captionChoices, typeChoices);
                            EnumPropertyTypes[propertyKeyName] = dataType;
                        }
                        else
                        {
                            dataType = type;
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException(String.Format("Invalid type field on line {0} of file {1}", lineNumber, schemaFileName));
            }
            return dataType;
        }
        private static Dictionary<string, Type> EnumPropertyTypes { get; } = new Dictionary<string, Type>();
        public static string ToString(object obj, Type dataType)
        {
            if (dataType == typeof(string))
            {
                return (string)obj;
            }
            if (dataType == typeof(double))
            {
                return ((double)obj).ToString(CultureInfo.InvariantCulture);
            }
            if (dataType == typeof(int))
            {
                return ((int)obj).ToString(CultureInfo.InvariantCulture);
            }
            if (dataType == typeof(bool))
            {
                return ((bool)obj) ? "1" : "0";
            }
            if (dataType == typeof(DateTime))
            {
                return ((DateTime)obj).ToString("yyyyMMdd");
            }
            if (dataType == typeof (TimeSpan))
            {
                if (obj is TimeSpan && ((TimeSpan) obj).Ticks != 0)
                {
                    var nrOfTicks = 1.0*((TimeSpan) obj).Ticks;
                    return (nrOfTicks/TimeSpan.TicksPerSecond).ToString(CultureInfo.InvariantCulture);
                }
                return "";
            }
            if (dataType == typeof(IList<Double>))
            {
                return string.Join(" ", ((IList<Double>)obj).Select(d => d.ToString(CultureInfo.InvariantCulture)));
            }
            if (dataType == typeof(IList<string>))
            {
                return string.Join(" ", (IList<string>)obj);
            }
            if (dataType.IsEnum)
            {
                return ((Enum)obj).GetDisplayName();
            }
            if (dataType == typeof (Steerable))
            {
                var steerable = (Steerable) obj;
                switch (steerable.Mode)
                {
                    case SteerableMode.ConstantValue:
                        return ToString(steerable.ConstantValue, typeof (double));
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
        /// <param name="str">String to be parsed.</param>
        /// <param name="dataType">Expected data type represented by <paramref name="str"/>.</param>
        /// <returns>The parsed object.</returns>
        /// <seealso cref="FromString{T}(string)"/>
        /// <exception cref="ArgumentNullException">Can occur for some <paramref name="dataType"/> when <paramref name="str"/> is null.</exception>
        /// <exception cref="FormatException">Can occur for some <paramref name="dataType"/> when <paramref name="str"/> is in invalid format.</exception>
        /// <exception cref="OverflowException">Can occur for some <paramref name="dataType"/> when <paramref name="str"/> represent a value that is too big or too small.</exception>
        public static object FromString(string str, Type dataType)
        {
            if (dataType == typeof(string))
            {
                return str;
            }
            if (dataType == typeof(IList<string>))
            {
                if (string.IsNullOrEmpty(str)) return new List<string>();

                return str.SplitOnEmptySpace().ToList();
            }
            if (dataType == typeof(double))
            {
                return ParseDoubleValue(str);
            }
            if (dataType == typeof(int))
            {
                return Int32.Parse(str);
            }
            if (dataType == typeof(bool))
            {
                return str.Trim().Equals("1") || str.Trim().Equals("true"); //simple version
            }
            if (dataType == typeof(DateTime))
            {
                return ParseFMDateTime(str);
            }
            if (dataType == typeof(TimeSpan))
            {
                if (!String.IsNullOrWhiteSpace(str))
                {
                    var valueAsDouble = ParseDoubleValue(str);
                    var ticks = (long) (TimeSpan.TicksPerSecond*valueAsDouble);
                    return new TimeSpan(ticks);
                }
                return new TimeSpan(0);
            }
            if (dataType == typeof(IList<Double>))
            {
                if(str == null) throw new ArgumentNullException("str");

                var trimmedString = str.Replace('d', 'e').Trim();
                if (string.IsNullOrEmpty(trimmedString)) return new List<double>(0);

                var numbers = trimmedString.SplitOnEmptySpace();
                var resultList = new List<Double>();
                foreach (var number in numbers)
                {
                    try
                    {
                        resultList.Add(ParseDoubleValue(number));
                    }
                    // ArgumentNullException won't occur in this scenario
                    catch (FormatException e)
                    {
                        Log.WarnFormat("Value '{0}' in collection cannot be read (Cause: {1}) and is skipped. ", number, e.Message);
                        resultList.Add(double.NaN);
                    }
                    catch (OverflowException e)
                    {
                        Log.WarnFormat("Value '{0}' in collection cannot be read (Cause: {1}) and is skipped. ", number, e.Message);
                        resultList.Add(double.NaN);
                    }
                }
                return resultList;
            }
            if (dataType.IsEnum)
            {
                var enumValue = EnumUtils.GetEnumValueFromDisplayName(str, dataType);
                if (enumValue == null) throw new FormatException(String.Format("Value of '{0}' not valid.", str));
                return enumValue;
            }
            if (dataType == typeof (Steerable))
            {
                throw new NotImplementedException("Try to parse the value of a Steerable as double, and when that fails as string.");
            }
            throw new NotImplementedException("Unsupported data type " + dataType);
        }

        /// <summary>
        /// Parses a string for a given value type.
        /// </summary>
        /// <typeparam name="T">Expected data type represented by <paramref name="str"/>.</typeparam>
        /// <param name="str">String to be parsed.</param>
        /// <returns>The parsed object.</returns>
        /// <remarks>This method cannot be used for enumerations</remarks>
        /// <exception cref="ArgumentNullException">Can occur for some <typeparamref name="T"/> when <paramref name="str"/> is null.</exception>
        /// <exception cref="FormatException">Can occur for some <typeparamref name="T"/> when <paramref name="str"/> is in invalid format.</exception>
        /// <exception cref="OverflowException">Can occur for some <typeparamref name="T"/> when <paramref name="str"/> represent a value that is too big or too small.</exception>
        /// <seealso cref="FromString(string,Type)"/>
        public static T FromString<T>(string str)
        {
            var type = typeof (T);
            if (type.IsEnum) throw new NotImplementedException("Use FromString(string str, Type dataType) instead!");

            return (T) FromString(str, type);
        }

        /// <summary>
        /// Parses a string to <see cref="DateTime"/> expecting 'yyyyMMdd' or 'yyyyMMddHHmmss' format.
        /// </summary>
        /// <param name="valueAsString">Value to be parsed.</param>
        /// <returns>Parsed date-time, or <see cref="DateTime.Now"/> when <paramref name="valueAsString"/> empty or null.</returns>
        /// <exception cref="FormatException">When <paramref name="valueAsString"/> does not represent a supported date-time format.</exception>
        public static DateTime ParseFMDateTime(string valueAsString)
        {
            if (String.IsNullOrEmpty(valueAsString)) return DateTime.Now;

            if (!DateTime.TryParseExact(valueAsString, "yyyyMMdd", null, DateTimeStyles.None, out var value) && 
                !DateTime.TryParseExact(valueAsString, "yyyyMMddHHmmss", null, DateTimeStyles.None, out value) && 
                !DateTime.TryParseExact(valueAsString, "yyyy-MM-dd", null, DateTimeStyles.None, out value))
            {
                throw new FormatException("Unexpected date time format");
            }
            return value;
        }

        /// <summary>
        /// Parses a string as double, expecting <see cref="CultureInfo.InvariantCulture"/> format.
        /// </summary>
        /// <param name="valuesAsString">Value to be parsed.</param>
        /// <returns>Double value represented by <paramref name="valuesAsString"/></returns>
        /// <exception cref="ArgumentNullException">When <paramref name="valuesAsString"/> is null.</exception>
        /// <exception cref="FormatException">When <paramref name="valuesAsString"/> does not represent a number in a valid format.</exception>
        /// <exception cref="OverflowException">When <paramref name="valuesAsString"/> represents a number that is less than 
        ///     <see cref="double.MinValue"/> or greater than <see cref="double.MaxValue"/>.</exception>
        private static double ParseDoubleValue(string valuesAsString)
        {
            var actualString = valuesAsString.Replace("d-", "e-").Replace("d+", "e+");
            actualString = actualString.Replace("d", "e+");
            return Double.Parse(actualString, CultureInfo.InvariantCulture);
        }
    }
}
