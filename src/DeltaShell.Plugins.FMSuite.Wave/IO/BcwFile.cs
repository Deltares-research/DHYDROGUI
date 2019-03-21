using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Units;
using DelftTools.Utils.Collections;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    public class BcwFile : FMSuiteFileBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (BcwFile));

        private const string BoundaryNamePattern = @"(location\s+\'(?'value'.+)\')";
        private const string TimeFunctionPattern = @"(time-function\s+\'(?'value'.+)\')";
        private const string ReferenceDatePattern = @"(reference-time\s+(?'value'\d+))";
        private const string TimeUnitPattern = @"(time-unit\s+\'(?'value'.+)\')";
        private const string InterpolPattern = @"(interpolation\s+\'(?'value'.+)\')";

        private const string ParameterPattern = @"(parameter\s+\'(?'parname'.+)\'\s+unit\s+\'(?'unit'.+)\')";

        public const string TimeFunctionAttributeName = "time_function";
        public const string RefDateAttributeName = "reference_date";
        public const string TimeUnitAttributeName = "time_unit";

        public const string DateFormatString = "yyyyMMdd";

        /// <summary>
        /// Reads the .bcw file.
        /// </summary>
        /// <param name="bcwFilePath">Full file path</param>
        /// <returns>
        /// Returns a dictionary with the boundary condition names, matching those in the mdw file,
        /// and their functions with components wave height, period, direction, spreading and argument time.
        /// </returns>
        public IDictionary<string, List<IFunction>> Read(string bcwFilePath)
        {
            var bcwData = new Dictionary<string, List<IFunction>>();

            try
            {
                OpenInputFile(bcwFilePath);

                string boundaryName = null;
                BcwHeaderData header = null;
                IList<BcwParameter> parameterData = null;

                while (GetNextLine() != null)
                {
                    if (IsNewBoundaryDataBlock(CurrentLine))
                    {
                        if (parameterData != null)
                        {
                            FillBoundaryData(bcwData, boundaryName, header, parameterData);
                            parameterData = null;
                        }

                        while (CurrentLine != null && IsNewBoundaryDataBlock(CurrentLine))
                        {
                            boundaryName = ReadBoundaryName();
                            bcwData.Add(boundaryName, new List<IFunction>());
                        }

                        if (CurrentLine == null) break;

                        header = ReadBcwHeaderData();
                        parameterData = ReadParameterMetaData();
                    }

                    if (parameterData == null) continue;

                    var values = ReadParameterValues();
                    if (values.Count != parameterData.Count)
                    {
                        Log.ErrorFormat(
                            "Invalid parameter data in file {0}, line number {1}. Expecting {2} parameter values.",
                            InputFilePath, LineNumber, parameterData.Count);
                        return null;
                    }

                    AddValuesToParameters(parameterData, values);
                }

                // store values of the last block
                if (parameterData != null)
                {
                    FillBoundaryData(bcwData, boundaryName, header, parameterData);
                }
            }
            catch (Exception e) when (e is InvalidOperationException
                                      || e is OutOfMemoryException
                                      || e is IOException
                                      || e is NotSupportedException
                                      || e is FileFormatException
            )
            {
                LogErrorReading(e.Message);
            }
            finally
            {
                CloseInputFile();
            }

            return bcwData;
        }

        private void LogErrorReading(string exceptionMessage)
        {
            Log.Error($"There was an error reading the bcw file at line {LineNumber}: {exceptionMessage}");
        }

        private static void AddValuesToParameters(IEnumerable<BcwParameter> parameterData, IList<double> values)
        {
            parameterData.ForEach((pd, i) => pd.Values.Add(values[i]));
        }

        private IList<double> ReadParameterValues()
        {
            var stringValues = CurrentLine.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var values = stringValues.Select(s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture)).ToList();

            return values;
        }

        private string ReadBoundaryName()
        {
            var boundaryName = RegularExpression.GetFirstMatch(BoundaryNamePattern, CurrentLine).Groups["value"].Value;
            GetNextLine();

            return boundaryName;
        }

        private void FillBoundaryData(IDictionary<string, List<IFunction>> bcwData, string boundaryName, BcwHeaderData header,
            IList<BcwParameter> parameterData)
        {
            bcwData[boundaryName] = new List<IFunction>();
            bcwData[boundaryName].AddRange(CreateFunctionsFromData(parameterData, header));
        }

        private IList<BcwParameter> ReadParameterMetaData()
        {
            var parameterData = new List<BcwParameter>();

            while (IsNewParameter(CurrentLine))
            {
                var matches = RegularExpression.GetFirstMatch(ParameterPattern, CurrentLine);
                var bcwParameter = new BcwParameter
                {
                    Values = new List<double>(),
                    Name = matches.Groups["parname"].Value.Trim(),
                    Unit = matches.Groups["unit"].Value.Trim()
                };

                parameterData.Add(bcwParameter);

                GetNextLine();
            }

            return parameterData;
        }

        private BcwHeaderData ReadBcwHeaderData()
        {
            var header = new BcwHeaderData
            {
                TimeFunction = ReadParameterValue(TimeFunctionPattern, CurrentLine),
                ReferenceDateString = ReadParameterValue(ReferenceDatePattern, GetNextLine()),
                TimeUnit = ReadParameterValue(TimeUnitPattern, GetNextLine()),
                InterpolationType = ReadParameterValue(InterpolPattern, GetNextLine())
            };

            GetNextLine();

            return header;
        }

        private static string ReadParameterValue(string searchPattern, string line)
        {
            return RegularExpression.GetFirstMatch(searchPattern, line).Groups["value"].Value;
        }

        private IEnumerable<IFunction> CreateFunctionsFromData(IList<BcwParameter> parameterData, BcwHeaderData header)
        {
            var functions = new List<IFunction>();
            DateTime referenceDate;
            if (header.ReferenceDateString != "from model")
            {
                referenceDate = DateTime.ParseExact(header.ReferenceDateString, DateFormatString,
                                                    CultureInfo.InvariantCulture,
                                                    DateTimeStyles.None);
            }
            else
            {
                throw new NotSupportedException(
                    string.Format("Reference date \"from model\" in bcw file {0} not (yet) supported", InputFilePath));
            }

            var timeParameter = parameterData.FirstOrDefault(pd => pd.Name == "time");
            if (timeParameter == null)
            {
                throw new FileFormatException($"Missing time parameter in timeseries file {InputFilePath}");
            }

            var dateTimes =
                timeParameter.Values.Select(v => ConvertToDateTime(v, header.TimeUnit, referenceDate)).ToList();
            var waveHeights = parameterData.Where(pd => pd.Name == KnownWaveProperties.WaveHeight).ToList();
            var periods = parameterData.Where(pd => pd.Name == KnownWaveProperties.Period).ToList();
            var directions = parameterData.Where(pd => pd.Name == KnownWaveProperties.Direction).ToList();
            var spreadings = parameterData.Where(pd => pd.Name == KnownWaveProperties.DirectionalSpreadingValue).ToList();

            for (var i = 0; i < waveHeights.Count; ++i)
            {
                var func = WaveBoundaryCondition.CreateEmptyWaveEnergyFunction();

                func.Arguments[0].SetValues(dateTimes);
                func.Arguments[0].Unit = null;
                func.Components[0].SetValues(waveHeights[i].Values);
                func.Components[0].Unit = GetUnitFromString(waveHeights[i].Unit);
                func.Components[1].SetValues(periods[i].Values);
                func.Components[1].Unit = GetUnitFromString(periods[i].Unit);
                func.Components[2].SetValues(directions[i].Values);
                func.Components[2].Unit = GetUnitFromString(directions[i].Unit);
                func.Components[3].SetValues(spreadings[i].Values);
                func.Components[3].Unit = GetUnitFromString(spreadings[i].Unit);

                func.Attributes[TimeFunctionAttributeName] = header.TimeFunction;
                func.Attributes[RefDateAttributeName] = header.ReferenceDateString;
                func.Attributes[TimeUnitAttributeName] = header.TimeUnit;

                functions.Add(func);
            }

            return functions;
        }

        private static bool IsNewParameter(string line)
        {
            return line.Contains("parameter");
        }

        private static bool IsNewBoundaryDataBlock(string line)
        {
            return line.StartsWith("location");
        }

        /// <summary>
        /// Writes the specified boundary condition data.
        /// </summary>
        /// <param name="boundaryConditionToFunctionsMappings">A dictionary with the boundary condition names with their functions.</param>
        /// <param name="filePath">The file path.</param>
        /// <remarks>If wave boundary condition does not have any functions, only the boundary condition name is written to the file.</remarks>
        public void Write(IDictionary<string, List<IFunction>> boundaryConditionToFunctionsMappings, string filePath)
        {
            OpenOutputFile(filePath);
            try
            {
                foreach (var boundaryConditionToFunctionsMapping in boundaryConditionToFunctionsMappings)
                {
                    var boundaryName = boundaryConditionToFunctionsMapping.Key;
                    var functions = boundaryConditionToFunctionsMapping.Value;

                    if (functions.Any())
                    {
                        var header = CreateHeaderFromFunction(functions.First());
                        var parameters = CreateParametersFromFunctions(functions);

                        WriteBoundaryData(boundaryName, header, parameters);
                    }

                    else
                    {
                        WriteBoundaryName(boundaryName);
                    }
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        private void WriteBoundaryName(string boundaryName)
        {
            WriteLine(string.Format("{0,-21}{1,-21}", "location", "\'" + boundaryName + "\'").TrimEnd());
        }

        private void WriteBoundaryData(string boundaryName, BcwHeaderData header, IList<BcwParameter> sortedParameters)
        {
            WriteBoundaryName(boundaryName);

            WriteFormattedHeaderData("time-function", header.TimeFunction);
            WriteFormattedHeaderData("reference-time", header.ReferenceDateString, false);
            WriteFormattedHeaderData("time-unit", header.TimeUnit);
            WriteFormattedHeaderData("interpolation", header.InterpolationType);

            // parameters
            foreach (var parameter in sortedParameters)
            {
                WriteLine(
                    string.Format("{0,-21}{1,-21}{2,-21}", "parameter", "\'" + parameter.Name + "\'",
                        "unit \'" + parameter.Unit + "\'").TrimEnd());
            }

            // data
            var timeParameter = sortedParameters.First(p => p.Name == "time");
            for (var i = 0; i < timeParameter.Values.Count; ++i)
            {
                var time = timeParameter.Values[i].ToString("F2", CultureInfo.InvariantCulture);
                var line = string.Format("{0,8}", time);
                for (var j = 1; j < sortedParameters.Count; ++j)
                {
                    line += string.Format(" {0,8}", sortedParameters[j].Values[i].ToString("F4", CultureInfo.InvariantCulture));
                }

                WriteLine(line.TrimEnd());
            }
        }

        private void WriteFormattedHeaderData(string parameterName, string parameterValue, bool withApostrophes = true)
        {
            var apostrophe = withApostrophes ? "\'" : "";
            WriteLine($"{parameterName,-21}{apostrophe + parameterValue + apostrophe,-21}".TrimEnd());
        }

        private static BcwHeaderData CreateHeaderFromFunction(IFunction function)
        {
            var header = new BcwHeaderData
            {
                TimeFunction = function.Attributes[TimeFunctionAttributeName],
                ReferenceDateString = function.Attributes[RefDateAttributeName],
                TimeUnit = function.Attributes[TimeUnitAttributeName],
                InterpolationType = "linear"
            };
            return header;
        }

        private IList<BcwParameter> CreateParametersFromFunctions(IList<IFunction> functions)
        {
            var func = functions.First();

            var parameters = new List<BcwParameter>();

            var refDateString = func.Attributes[RefDateAttributeName];
            var refDate = DateTime.ParseExact(refDateString, DateFormatString, CultureInfo.InvariantCulture);

            // time
            var timeParameter = new BcwParameter
            {
                Name = "time",
                Unit = "[min]",
                Values = func.Arguments[0].GetValues<DateTime>()
                    .Select(d => ConvertToBcwTime(d, refDate, func.Attributes[TimeUnitAttributeName])).ToList()
            };
            parameters.Add(timeParameter);

            foreach (var f in functions)
            {
                var waveHeight = CreateBcwParameter(f.Components[0], KnownWaveProperties.WaveHeight);
                var period = CreateBcwParameter(f.Components[1], KnownWaveProperties.Period);
                var direction = CreateBcwParameter(f.Components[2], KnownWaveProperties.Direction);
                var spreading = CreateBcwParameter(f.Components[3], KnownWaveProperties.DirectionalSpreadingValue);

                parameters.Add(waveHeight);
                parameters.Add(period);
                parameters.Add(direction);
                parameters.Add(spreading);
            }

            var sortedParameters = new List<BcwParameter>();
            sortedParameters.Add(parameters.First());
            sortedParameters.AddRange(parameters.Where(p => p.Name == KnownWaveProperties.WaveHeight));
            sortedParameters.AddRange(parameters.Where(p => p.Name == KnownWaveProperties.Period));
            sortedParameters.AddRange(parameters.Where(p => p.Name == KnownWaveProperties.Direction));
            sortedParameters.AddRange(parameters.Where(p => p.Name == KnownWaveProperties.DirectionalSpreadingValue));

            return sortedParameters;
        }

        private static Unit GetUnitFromString(string unitString)
        {
            switch (unitString)
            {
                case "[m]":
                    return new Unit("meter", "m");
                case "[s]":
                    return new Unit("second", "s");
                case "[N^o]":
                    return new Unit("degree", "deg");
                default:
                    return new Unit("", "-");
            }
        }

        private static string GetStringFromUnit(IUnit unit)
        {
            switch (unit.Name)
            {
                case "meter":
                    return "[m]";
                case "second":
                    return "[s]";
                case "degree":
                    return "[N^o]";
                default:
                    return "[-]";
            }
        }

        private DateTime ConvertToDateTime(double value, string unit, DateTime referenceDate)
        {
            switch (unit)
            {
                case "days":
                    return referenceDate.AddDays(value);
                case "hours":
                    return referenceDate.AddHours(value);
                case "minutes":
                    return referenceDate.AddMinutes(value);
                case "seconds":
                    return referenceDate.AddSeconds(value);
                default:
                    throw new NotImplementedException(string.Format("Unit {0} for bcw file is not (yet) implemented", unit));
            }
        }

        private double ConvertToBcwTime(DateTime dateTime, DateTime refDate, string unit)
        {
            switch (unit)
            {
                case "days":
                    return (dateTime - refDate).TotalDays;
                case "hours":
                    return (dateTime - refDate).TotalHours;
                case "minutes":
                    return (dateTime - refDate).TotalMinutes;
                case "seconds":
                    return (dateTime - refDate).TotalSeconds;
                default:
                    throw new NotImplementedException(string.Format("Unit {0} for bcw file is not (yet) implemented", unit));
            }
        }

        private static BcwParameter CreateBcwParameter(IVariable component, string bcwName)
        {
            return new BcwParameter
            {
                Name = bcwName,
                Unit = GetStringFromUnit(component.Unit),
                Values = component.GetValues<double>().ToList()
            };
        }

        private class BcwHeaderData
        {
            public string TimeFunction;
            public string ReferenceDateString;
            public string TimeUnit;
            public string InterpolationType;
        }

        private class BcwParameter
        {
            public string Name;
            public string Unit;
            public List<double> Values;
        }
    }
}