using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.Helpers;
using TimeUnits = DeltaShell.NGHS.IO.FileWriters.Boundary.BoundaryRegion.UnitStrings;

namespace DeltaShell.NGHS.IO.FileReaders
{
    /// <summary>
    /// Represents a boundary conditions category specific for lateral source discharge data from the boundary conditions file.
    /// </summary>
    public class LateralSourceBcCategory : ILateralSourceBcCategory
    {
        private readonly ILogHandler logHandler;
        private readonly int lineNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="LateralSourceBcCategory"/> class.
        /// </summary>
        /// <param name="category"> The bc category to parse from. </param>
        /// <param name="logHandler"> Optional parameter; the log handler to report errors. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the <paramref name="category"/> is not a lateral category.
        /// This means that the category should have name 'forcing' or 'LateralDischarge'.
        /// </exception>
        /// <remarks>
        /// The category is expected to have the following properties:
        /// - 'name'
        /// - 'function'
        /// If a property is missing, an error will be logged.
        /// Moreover, depending on the discharge data type, the quantities are expected to be in the correct order:
        /// First the argument, then the component.
        /// </remarks>
        public LateralSourceBcCategory(IDelftBcCategory category, ILogHandler logHandler = null)
        {
            Ensure.NotNull(category, nameof(category));
            EnsureLateralCategory(category, nameof(category));

            this.logHandler = logHandler;
            lineNumber = category.LineNumber;

            var name = category.ReadProperty<string>(BoundaryRegion.Name.Key);
            var function = category.ReadProperty<string>(BoundaryRegion.Function.Key);
            var periodic = category.ReadProperty<string>(BoundaryRegion.Periodic.Key, true);

            Name = name;
            DataType = ToDataType(function);
            SetDischarge(category.Table, periodic);
        }

        /// <summary>
        /// The name of the lateral source.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The discharge data type of the lateral source.
        /// </summary>
        public Model1DLateralDataType DataType { get; }

        /// <summary>
        /// The constant discharge value of the lateral source.
        /// </summary>
        public double Discharge { get; private set; }

        /// <summary>
        /// The variable discharge function of the lateral source.
        /// </summary>
        public IFunction DischargeFunction { get; private set; }

        private static void EnsureLateralCategory(IDelftIniCategory category, string paramName)
        {
            if (!category.Name.EqualsCaseInsensitive(BoundaryRegion.BcLateralHeader) &&
                !category.Name.EqualsCaseInsensitive(BoundaryRegion.BcForcingHeader))
            {
                throw new ArgumentException("The category is not a lateral category.", paramName);
            }
        }

        private void SetDischarge(IList<IDelftBcQuantityData> table, string periodic)
        {
            switch (DataType)
            {
                case Model1DLateralDataType.FlowWaterLevelTable:
                    DischargeFunction = CreateFlowWaterLevelTable(table, periodic);
                    return;
                case Model1DLateralDataType.FlowTimeSeries:
                    DischargeFunction = CreateTimeSeries(table, periodic);
                    return;
                case Model1DLateralDataType.FlowConstant:
                    Discharge = CreateConstant(table);
                    return;
            }
        }

        private double CreateConstant(IList<IDelftBcQuantityData> table)
        {
            if (TryParseDouble(table[0].Values[0], out double value))
            {
                return value;
            }

            return 0;
        }

        private IFunction CreateTimeSeries(IList<IDelftBcQuantityData> table, string periodic)
        {
            if (!TryParseDateTimes(table[0].Values, out IEnumerable<DateTime> argumentValues, table[0].Unit.Value))
            {
                return null;
            }

            if (!TryParseDoubles(table[1].Values, out IEnumerable<double> functionValues))
            {
                return null;
            }

            TimeSeries function = HydroTimeSeriesFactory.CreateFlowTimeSeries();

            CompleteFunction(function, argumentValues, functionValues, periodic);

            return function;
        }

        private IFunction CreateFlowWaterLevelTable(IList<IDelftBcQuantityData> table, string periodic)
        {
            if (!TryParseDoubles(table[0].Values, out IEnumerable<double> argumentValues))
            {
                return null;
            }

            if (!TryParseDoubles(table[1].Values, out IEnumerable<double> functionValues))
            {
                return null;
            }

            var function = new FlowWaterLevelTable();
            CompleteFunction(function, argumentValues, functionValues, periodic);
            return function;
        }

        private static void CompleteFunction<T>(IFunction function, IEnumerable<T> argumentValues, IEnumerable<double> values, string periodic)
        {
            function.Clear();
            IVariable argument = function.Arguments[0];

            bool isAutoSorted = argument.IsAutoSorted;
            argument.IsAutoSorted = false;
            argument.SetValues(argumentValues);
            function.SetValues(values);
            argument.IsAutoSorted = isAutoSorted;

            argument.ExtrapolationType = periodic.EqualsCaseInsensitive("true")
                                             ? ExtrapolationType.Periodic
                                             : ExtrapolationType.Linear;
        }

        private bool TryParseDateTimes(IEnumerable<string> values, out IEnumerable<DateTime> dateTimes, string unitValue)
        {
            dateTimes = null;

            if (!TryParseDoubles(values, out IEnumerable<double> doubles) ||
                !TryGetReferenceTime(unitValue, out DateTime referenceTime))
            {
                return false;
            }

            if (unitValue.Contains(TimeUnits.TimeSeconds))
            {
                dateTimes = doubles.Select(referenceTime.AddSeconds);
                return true;
            }

            if (unitValue.Contains(TimeUnits.TimeMinutes))
            {
                dateTimes = doubles.Select(referenceTime.AddMinutes);
                return true;
            }

            if (unitValue.Contains(TimeUnits.TimeHours))
            {
                dateTimes = doubles.Select(referenceTime.AddHours);
                return true;
            }

            logHandler?.ReportError($"Cannot interpret '{unitValue}', see category on line {lineNumber}.");
            return false;
        }

        private bool TryGetReferenceTime(string unitValue, out DateTime referenceTime)
        {
            const string since = "since";
            int sinceIndex = unitValue.IndexOf(since, StringComparison.InvariantCultureIgnoreCase);
            string dateTimeString = unitValue.Substring(sinceIndex + since.Length).Trim();

            try
            {
                referenceTime = DateTime.ParseExact(dateTimeString,
                                                    TimeUnits.TimeFormat,
                                                    CultureInfo.InvariantCulture);
                return true;
            }
            catch (FormatException)
            {
                logHandler?.ReportError($"Cannot parse '{dateTimeString}' to a date time, see category on line {lineNumber}.");
                referenceTime = DateTime.MinValue;
                return false;
            }
        }

        private bool TryParseDoubles(IEnumerable<string> stringValues, out IEnumerable<double> doubles)
        {
            var doubleValues = new List<double>();

            foreach (string stringValue in stringValues)
            {
                if (!TryParseDouble(stringValue, out double doubleValue))
                {
                    doubles = null;
                    return false;
                }

                doubleValues.Add(doubleValue);
            }

            doubles = doubleValues;
            return true;
        }

        private bool TryParseDouble(string doubleString, out double doubleVal)
        {
            const NumberStyles numberStyle = NumberStyles.AllowLeadingWhite |
                                             NumberStyles.AllowTrailingWhite |
                                             NumberStyles.AllowLeadingSign |
                                             NumberStyles.AllowDecimalPoint |
                                             NumberStyles.AllowThousands |
                                             NumberStyles.AllowExponent;

            if (double.TryParse(doubleString, numberStyle, CultureInfo.InvariantCulture, out double doubleVal2))
            {
                doubleVal = doubleVal2;
                return true;
            }

            logHandler?.ReportError($"Cannot parse '{doubleString}' to a double, see category on line {lineNumber}.");
            doubleVal = doubleVal2;
            return false;
        }

        private static Model1DLateralDataType ToDataType(string function)
        {
            if (function.EqualsCaseInsensitive(BoundaryRegion.FunctionStrings.Constant))
            {
                return Model1DLateralDataType.FlowConstant;
            }

            if (function.EqualsCaseInsensitive(BoundaryRegion.FunctionStrings.QhTable))
            {
                return Model1DLateralDataType.FlowWaterLevelTable;
            }

            if (function.EqualsCaseInsensitive(BoundaryRegion.FunctionStrings.TimeSeries))
            {
                return Model1DLateralDataType.FlowTimeSeries;
            }

            return Model1DLateralDataType.FlowRealTime;
        }
    }
}