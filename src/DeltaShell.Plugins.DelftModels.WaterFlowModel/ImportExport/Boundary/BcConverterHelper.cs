using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary
{
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
            var data = column.Unit.Value.Split(new [] { " since " }, StringSplitOptions.None); 

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
    }
}
