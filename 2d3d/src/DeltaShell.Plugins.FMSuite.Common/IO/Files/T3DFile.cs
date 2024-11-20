using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    public class T3DFile : NGHSFileBase
    {
        private const string LayerTypeKey = "LAYER_TYPE";
        private const string LayersKey = "LAYERS";
        private const string TimeKey = "TIME";
        private readonly ILog log = LogManager.GetLogger(typeof(T3DFile));

        public TimeSeries Read(string filePath, out VerticalProfileDefinition verticalProfileDefinition)
        {
            OpenInputFile(filePath);
            try
            {
                var dateTimes = new List<DateTime>();
                var values = new List<List<double>>();

                string line = GetNextLine();
                VerticalProfileType verticalProfileType = ParseLayerDefinition(line);

                line = GetNextLine();
                verticalProfileDefinition = ParseLayers(line, verticalProfileType);
                int cols = Math.Max(1, verticalProfileDefinition.ProfilePoints);
                for (var i = 0; i < cols; i++)
                {
                    values.Add(new List<double>());
                }

                while (line != null)
                {
                    line = GetNextLine();
                    if (line == null)
                    {
                        break;
                    }

                    dateTimes.Add(ParseDateTimeLine(line));

                    line = GetNextLine();
                    if (line == null)
                    {
                        break;
                    }

                    List<double> valueRow =
                        line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => GetDouble(s, "value"))
                            .ToList();

                    if (valueRow.Count < cols)
                    {
                        throw new FormatException(string.Format("Too few values on line {0} of file {1}", LineNumber,
                                                                InputFilePath));
                    }

                    for (var i = 0; i < cols; i++)
                    {
                        values[i].Add(valueRow[i]);
                    }
                }

                var timeSeries = new TimeSeries();
                for (var i = 0; i < cols; ++i)
                {
                    timeSeries.Components.Add(new Variable<double>());
                }

                FunctionHelper.SetValuesRaw<DateTime>(timeSeries.Time, dateTimes);
                for (var i = 0; i < cols; ++i)
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

        public void Write(string filePath, IFunction data, VerticalProfileDefinition verticalProfileDefinition,
                          DateTime? modelReferenceDate)
        {
            if (data.Arguments.Count != 1)
            {
                throw new ArgumentException(string.Format("Cannot save multi-argument function {0} to t3d file",
                                                          data.Name));
            }

            if (data.Arguments[0].ValueType != typeof(DateTime))
            {
                throw new ArgumentException(string.Format("Cannot save non-timeseries function {0} to t3d file",
                                                          data.Name));
            }

            int cols = Math.Max(1, verticalProfileDefinition.ProfilePoints);
            if (data.Components.Count != cols)
            {
                throw new ArgumentException(string.Format("Cannot save multi-component function {0} to t3d file",
                                                          data.Name));
            }

            var timeArgument = (IVariable<DateTime>) data.Arguments[0];

            DateTime refTime = modelReferenceDate ?? timeArgument.Values[0];

            List<IVariable<double>> components = data.Components.OfType<IVariable<double>>().ToList();

            using (CultureUtils.SwitchToInvariantCulture())
            {
                OpenOutputFile(filePath);
                double factor = verticalProfileDefinition.Type == VerticalProfileType.PercentageFromBed ||
                                verticalProfileDefinition.Type == VerticalProfileType.PercentageFromSurface
                                    ? 0.01
                                    : 1.0;
                try
                {
                    string layerTypeString = GetLayerTypeString(verticalProfileDefinition.Type, data.Name);
                    if (layerTypeString == null)
                    {
                        return;
                    }

                    WriteLine(LayerTypeKey + " = " + layerTypeString);
                    WriteLine(LayersKey + " = " +
                              string.Join(" ", verticalProfileDefinition.SortedPointDepths.Select(x => factor * x)));

                    for (var i = 0; i < timeArgument.Values.Count; i++)
                    {
                        double seconds = (timeArgument.Values[i] - refTime).TotalSeconds;
                        var timeString = refTime.ToString("yyyy-MM-dd hh:mm:ss");
                        WriteLine(TimeKey + " = " + seconds + " seconds since " + timeString + " +00:00");
                        WriteLine(string.Join(" ", components.Select(c => c.Values[i])));
                    }
                }
                finally
                {
                    CloseOutputFile();
                }
            }
        }

        private DateTime ParseDateTimeLine(string line)
        {
            string field = GetValueForKey(line, TimeKey);
            if (field == null)
            {
                throw new FormatException(string.Format("Could not find TIME-qualifier in t3d-file {0} at line {1}.",
                                                        InputFilePath, LineNumber));
            }

            List<string> fields = field.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList();
            if (fields.Count < 5)
            {
                throw new FormatException(string.Format("Invalid time specification in t3d-file {0} at line {1}.",
                                                        InputFilePath, LineNumber));
            }

            int nUnits = GetInt(fields[0]);
            var span = new TimeSpan();

            switch (fields[1].ToLower())
            {
                case "seconds":
                    span = new TimeSpan(0, 0, 0, nUnits);
                    break;
                case "minutes":
                    span = new TimeSpan(0, 0, nUnits, 0);
                    break;
                case "hours":
                    span = new TimeSpan(0, nUnits, 0, 0);
                    break;
                case "days":
                    span = new TimeSpan(nUnits, 0, 0, 0);
                    break;
                default:
                    throw new FormatException(string.Format("Unknown time unit {0} in t3d-file {1} at line {2}",
                                                            fields[1], InputFilePath, LineNumber));
            }

            if (fields[2] != "since")
            {
                throw new FormatException(string.Format("Invalid time format encountered in file {0} at line {1}",
                                                        InputFilePath, LineNumber));
            }

            string refDateString = fields[3] + " " + fields[4];
            DateTime refDate = DateTime.ParseExact(refDateString, "yyyy-MM-dd hh:mm:ss",
                                                   CultureInfo.InvariantCulture.DateTimeFormat);
            return refDate + span;
        }

        private VerticalProfileType ParseLayerDefinition(string line)
        {
            string field = GetValueForKey(line, LayerTypeKey);
            switch (field)
            {
                case "SIGMA":
                    return VerticalProfileType.PercentageFromBed;
                case "Z":
                    return VerticalProfileType.ZFromBed;
                default:
                    return VerticalProfileType.Uniform;
            }
        }

        private string GetLayerTypeString(VerticalProfileType type, string data)
        {
            switch (type)
            {
                case VerticalProfileType.PercentageFromBed:
                    return "SIGMA";
                case VerticalProfileType.ZFromBed:
                    return "Z";
                default:
                    log.Error(string.Format(
                                  "Creating file for vertical profile of type {0} not for {1} boundary supported yet.",
                                  type, data));
                    return null;
            }
        }

        private VerticalProfileDefinition ParseLayers(string line, VerticalProfileType verticalProfileType)
        {
            string layerDepthString = GetValueForKey(line, LayersKey);
            if (string.IsNullOrEmpty(layerDepthString))
            {
                return new VerticalProfileDefinition(VerticalProfileType.Uniform);
            }

            List<double> layerDepths =
                layerDepthString.Split(new char[0], StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => GetDouble(s, "Layer depth"))
                                .ToList();
            int factor = verticalProfileType == VerticalProfileType.PercentageFromBed ||
                         verticalProfileType == VerticalProfileType.PercentageFromSurface
                             ? 100
                             : 1;
            return new VerticalProfileDefinition(verticalProfileType, layerDepths.Select(x => factor * x).ToArray());
        }

        private static string GetValueForKey(string line, string key)
        {
            if (line.ToUpper().StartsWith(key.ToUpper()))
            {
                int equalSignIndex = line.IndexOf('=');
                if (equalSignIndex != -1 && equalSignIndex < line.Length - 1)
                {
                    return line.Substring(equalSignIndex + 1).Trim();
                }
            }

            return null;
        }
    }
}