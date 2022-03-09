using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DeltaShell.NGHS.IO;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    public class TimFile : NGHSFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TimFile));

        // if modelStartTime is null, we write absolute time values. 
        public void Write(string timFilePath, IFunction timeSeries, DateTime? modelReferenceDate)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                OpenOutputFile(timFilePath);
                try
                {
                    IVariable<DateTime> timeArgument =
                        timeSeries.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
                    if (timeArgument == null)
                    {
                        throw new ArgumentException("Incorrect function type: can only write time series to tim files");
                    }

                    DateTime[] timeValues = timeArgument.Values.ToArray();

                    List<IVariable<double>> components = timeSeries.Components.OfType<IVariable<double>>().ToList();

                    for (var i = 0; i < timeValues.Length; i++)
                    {
                        string timeString = modelReferenceDate == null
                                                ? string.Format("{0:yyyyMMddhhmm}", timeValues[i])
                                                : string.Format("{0:0.0000000e+00}",
                                                                (timeValues[i] - modelReferenceDate.Value)
                                                                .TotalMinutes);

                        List<string> valueStrings = components.Select(c =>
                                                                          string.Format(
                                                                              "{0:0.0000000e+00}", c.Values[i]))
                                                              .ToList();

                        WriteLine(string.Join(" ", new[]
                        {
                            timeString
                        }.Concat(valueStrings)));
                    }
                }
                finally
                {
                    CloseOutputFile();
                }
            }
        }

        public void Read(string timFilePath, IFunction function, DateTime? refDate)
        {
            if (function == null ||
                !(function.Arguments.Count == 1 && function.Arguments.First() is IVariable<DateTime>))
            {
                throw new ArgumentException(
                    string.Format("Cannot import time series data from {0} onto non-timeseries function {1}",
                                  timFilePath, function == null ? string.Empty : function.Name));
            }

            var minutes = new List<double>();
            var componentValues = new List<List<double>>();
            for (var i = 0; i < function.Components.Count; ++i)
            {
                componentValues.Add(new List<double>());
            }

            Read(timFilePath, minutes, componentValues);

            function.BeginEdit(new DefaultEditAction("Inserting time series from tim-file"));
            function.Clear();
            FunctionHelper.SetValuesRaw(function.Arguments[0], minutes.Select(m => GetDateTime(m, refDate)));
            for (var i = 0; i < function.Components.Count; ++i)
            {
                FunctionHelper.SetValuesRaw<double>(function.Components[i], componentValues[i]);
            }

            function.EndEdit();
        }

        public TimeSeries Read(string timFilePath, DateTime modelReferenceDate)
        {
            OpenInputFile(timFilePath);
            try
            {
                var dateTimes = new List<DateTime?>();
                var values = new List<List<double>>();

                var timeSeries = new TimeSeries();

                string line = GetNextLine();
                if (line == null)
                {
                    return timeSeries;
                }

                int componentColumns = SplitLine(line).Count() - 1;
                for (var i = 0; i < componentColumns; i++)
                {
                    values.Add(new List<double>());
                }

                while (line != null)
                {
                    var lineFields = (IList<string>) SplitLine(line).ToList();

                    if (lineFields.Count < componentColumns + 1)
                    {
                        throw new FormatException(string.Format("Invalid time/value row on line {0} in file {1}",
                                                                LineNumber, timFilePath));
                    }

                    dateTimes.Add(GetDateTime(lineFields[0], modelReferenceDate, "time"));

                    for (var i = 0; i < componentColumns; ++i)
                    {
                        values[i].Add(GetDouble(lineFields[i + 1], "value"));
                    }

                    line = GetNextLine();
                }

                for (var i = 0; i < componentColumns; ++i)
                {
                    timeSeries.Components.Add(new Variable<double>());
                }

                FunctionHelper.SetValuesRaw<DateTime?>(timeSeries.Time, dateTimes);
                for (var i = 0; i < componentColumns; ++i)
                {
                    FunctionHelper.SetValuesRaw<double>(timeSeries.Components[i], values[i]);
                }

                return timeSeries;
            }
            finally
            {
                CloseInputFile();
            }
        }

        public DateTime? GetDateTime(string lineField, DateTime? reference, string errorMessageKey = null)
        {
            double value = GetDouble(lineField, errorMessageKey);
            return GetDateTime(value, reference);
        }

        private void Read(string timFilePath, IList<double> minutes, IList<List<double>> values)
        {
            OpenInputFile(timFilePath);
            using (CultureUtils.SwitchToInvariantCulture())
            {
                try
                {
                    string line = GetNextLine();
                    var additionalValuesDetected = false;

                    while (line != null)
                    {
                        var lineFields = (IList<string>) SplitLine(line).ToList();
                        minutes.Add(GetDouble(lineFields[0], "time"));

                        int actualNumberOfValueColumns = lineFields.Count - 1;
                        int expectedNumberOfValueColumns = values.Count;

                        if (expectedNumberOfValueColumns < actualNumberOfValueColumns)
                        {
                            additionalValuesDetected = true;
                        }

                        int numberOfValuesRead = Math.Min(actualNumberOfValueColumns, expectedNumberOfValueColumns);

                        for (var i = 0; i < numberOfValuesRead; i++)
                        {
                            values[i].Add(GetDouble(lineFields[i + 1], "value"));
                        }

                        int missingValueColumns = expectedNumberOfValueColumns - actualNumberOfValueColumns;
                        for (var i = 0; i < missingValueColumns; i++)
                        {
                            values[actualNumberOfValueColumns + i].Add(0.0);
                        }

                        line = GetNextLine();
                    }

                    if (additionalValuesDetected)
                    {
                        Log.WarnFormat("Additional values detected when reading file: {0}." +
                                       "{1}Expected number of value columns is {2}, all additional values have been ignored.",
                                       timFilePath, Environment.NewLine, values.Count);
                    }
                }
                finally
                {
                    CloseInputFile();
                }
            }
        }

        private static DateTime? GetDateTime(double value, DateTime? reference)
        {
            // 31-12-1899 23:59
            const double minimalAbsoluteDate = 189912312359.0d;
            if (value > minimalAbsoluteDate)
            {
                // parse as absolute time
                var remainder = (long) value;
                long years = remainder / 100000000;
                remainder -= years * 100000000;
                long months = remainder / 1000000;
                remainder -= months * 1000000;
                long days = remainder / 10000;
                remainder -= days * 10000;
                long hours = remainder / 100;
                remainder -= hours * 100;
                return new DateTime((int) years, (int) months, (int) days, (int) hours, (int) remainder, 0);
            }

            if (value >= 999999999) //assume the actual value is irrelevant
            {
                return reference + new TimeSpan(0, 999999999, 0);
            }

            long ticks = (long) (value * TimeSpan.TicksPerSecond) * 60; // tim-file is always in minutes!
            return reference + new TimeSpan(ticks);
        }
    }
}