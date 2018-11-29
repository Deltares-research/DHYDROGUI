using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions.Generic;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
    /// <summary>
    /// BcConverterHelper class contains several static functions which are shared among the different
    /// Converters used to extract Boundary and lateral discharge data from the BoundaryCondition and
    /// BoundaryLocation files.
    /// </summary>
    public static class BcConverterHelper
    {
        /// <summary>
        /// Parse the double values stored as a string in the column as a IEnumerable of actual doubles.
        /// </summary>
        /// <param name="column">The column from which the double values are extracted.</param>
        /// <pre-condition>For all strings in Column Values it is true that: double.TryParse(val) </pre-condition>
        /// <returns>
        /// An enumerable containing the values (in order) as specified in the <paramref name="column"/>.
        /// </returns>
        public static IEnumerable<double> ParseDoubleValuesFromTableColumn(IDelftBcQuantityData column)
        {
            return column.Values.Select(e => double.Parse(e, NumberStyles.AllowExponent |
                                                             NumberStyles.AllowDecimalPoint |
                                                             NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Parse the DateTime values stored as string values in the column as a IEnumerable of actual DateTimes.
        /// </summary>
        /// <param name="column">The column from which the date time values are extracted.</param>
        /// <precondition>
        /// For all string in Column Values it is true that: double.TryParse(val)
        /// For the unit string it is true that it is formatted as (unit) since (reference date)
        /// </precondition>
        /// <returns>
        /// An enumerable containing the values (in order) as specified in the <paramref name="column"/>.
        /// </returns>
        public static IEnumerable<DateTime> ParseDateTimesValuesFromTableColumn(IDelftBcQuantityData column)
        {
            var dateTimeData = ParseDoubleValuesFromTableColumn(column);

            // Format of the unit for time as specified by the reference manual: <unit> since <reference date>
            var data = column.Unit.Value.Split(new[] {" since "}, StringSplitOptions.None);

            // Determine factor
            double factor;
            var stepUnit = data[0].Trim();
            if (stepUnit.Equals("seconds"))
                factor = 1.0;
            else if (stepUnit.Equals("minutes"))
                factor = 60.0;
            else // stepUnit.Equals("hours")
                factor = 3600.0;

            var referenceDateTime = DateTime.ParseExact(
                data[1].Trim(), // Reference date
                BoundaryRegion.UnitStrings.TimeFormat,
                CultureInfo.InvariantCulture);

            return dateTimeData.Select(e => referenceDateTime.AddSeconds(e * factor));
        }

        /// <summary>
        /// Validate whether the specified property exists and is unique within properties.
        /// </summary>
        /// <param name="properties">A list of properties to be validated. </param>
        /// <param name="propertyKey">The key which should exist and be unique within <paramref name="properties"/></param>
        /// <param name="isOptional">Whether the specified key is optional</param>
        /// <param name="propertyVal">The value associated with the key</param>
        /// <pre-condition>
        /// properties != null && propertyKey != null
        /// </pre-condition>
        /// <returns>
        /// True and the propertyVal if it exists and is unique
        /// True and null if it does not exist and is optional, False otherwise
        /// </returns>
        public static bool ValidateUniqueProperty(IList<DelftIniProperty> properties,
            string propertyKey,
            bool isOptional,
            out string propertyVal)
        {
            propertyVal = null;
            var nPropertyEntries = properties.Count(p => p.Name.Equals(propertyKey));
            if (!(nPropertyEntries == 1 || (isOptional && nPropertyEntries == 0)))
                return false;

            propertyVal = properties.FirstOrDefault(p => p.Name.Equals(propertyKey))?.Value;
            return true;
        }

        /// <summary>
        /// Validate the Name property within <paramref name="properties"/>.
        /// </summary>
        /// <param name="properties">A list of properties to be validated. </param>
        /// <param name="name">The Name if validated. </param>
        /// <pre-condition>properties != null </pre-condition>
        /// <returns>
        /// True if Name is validated correctly, false otherwise.
        /// If True then name contains the found Name.
        /// </returns>
        public static bool ValidateNameProperty(IList<DelftIniProperty> properties,
            out string name)
        {
            return BcConverterHelper.ValidateUniqueProperty(properties,
                       BoundaryRegion.Name.Key,
                       false,
                       out name) &&
                   name.Length > 0;
        }

        /// <summary>
        /// Validate the Function property within <paramref name="properties"/>.
        /// </summary>
        /// <param name="properties">A list of properties to be validated. </param>
        /// <param name="functionType">The FunctionType if validated.</param>
        /// <returns>
        /// True if the Function is validated correctly, false otherwise.
        /// If True then functionType contains the found FunctionType.
        /// </returns>
        public static bool ValidateFunctionProperty(IList<DelftIniProperty> properties,
            out FunctionType functionType)
        {
            functionType = FunctionType.Constant;

            string functionStr;
            if (!ValidateUniqueProperty(properties,
                BoundaryRegion.Function.Key,
                false,
                out functionStr))
                return false;

            switch (functionStr)
            {
                case BoundaryRegion.FunctionStrings.Constant:
                    functionType = FunctionType.Constant;
                    break;
                case BoundaryRegion.FunctionStrings.QhTable:
                    functionType = FunctionType.QhTable;
                    break;
                case BoundaryRegion.FunctionStrings.TimeSeries:
                    functionType = FunctionType.TimeSeries;
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validate the Interpolation property within <paramref name="properties"/>.
        /// </summary>
        /// <param name="properties">A list of properties to be validated. </param>
        /// <param name="type"> The type of interpolation if validated. </param>
        /// <pre-condition>properties != null </pre-condition>
        /// <returns>
        /// True if Interpolation is validated correctly, false otherwise.
        /// If True then type contains the found InterpolationType.
        /// </returns>
        public static bool ValidateInterpolation(IList<DelftIniProperty> properties,
            out InterpolationType type)
        {
            type = InterpolationType.None;

            string interpolationTypeString;
            if (!ValidateUniqueProperty(properties,
                BoundaryRegion.Interpolation.Key,
                false,
                out interpolationTypeString))
                return false;

            switch (interpolationTypeString)
            {
                case BoundaryRegion.TimeInterpolationStrings.LinearAndExtrapolate:
                    type = InterpolationType.Linear;
                    break;
                case BoundaryRegion.TimeInterpolationStrings.BlockFrom:
                    type = InterpolationType.Constant;
                    break;
                case BoundaryRegion.TimeInterpolationStrings.BlockTo:
                    type = InterpolationType.Constant;
                    break;
                default:
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validate the Periodicity property within <paramref name="properties"/>
        /// </summary>
        /// <param name="properties">A list of properties to be validated. </param>
        /// <param name="hasPeriodicity">Whether the periodicity property is set to true if validated</param>
        /// <returns>
        /// True if Periodicity is validated correctly, false otherwise.
        /// If True then hasPeriodicity contains the found Periodicity.
        /// </returns>
        public static bool ValidatePeriodicity(IList<DelftIniProperty> properties,
            out bool hasPeriodicity)
        {
            hasPeriodicity = false;
            string periodicityStr;
            if (!ValidateUniqueProperty(properties,
                BoundaryRegion.Periodic.Key,
                true,
                out periodicityStr))
            {
                return false;
            }

            if (periodicityStr == null || periodicityStr.Equals("0") || periodicityStr.ToLower().Equals("false"))
                return true;

            if (periodicityStr.Equals("1") || periodicityStr.ToLower().Equals("true"))
            {
                hasPeriodicity = true;
                return true;
            }

            return false;
        }
    }

    /// <summary> Possible BoundaryCondition FunctionTypes </summary>
    public enum FunctionType
    {
        Constant,
        QhTable,
        TimeSeries
    }
}