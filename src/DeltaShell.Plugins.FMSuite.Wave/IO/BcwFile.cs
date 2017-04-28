using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Units;
using DelftTools.Utils.Collections;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.FMSuite.Common.IO;
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
        /// Returns functions with components waveheight,period,direction,spreading and argument time.
        /// The keys are the names of the boundary conditions, and should be matched to those in the mdw file.
        /// </summary>
        /// <param name="timeSeriesFile"></param>
        /// <returns></returns>
        public IDictionary<string, List<IFunction>> Read(string timeSeriesFile)
        {
            var bcwData = new Dictionary<string, List<IFunction>>();

            try
            {
                OpenInputFile(timeSeriesFile);

                BcwHeaderData header = null;
                IList<BcwParameter> parameterData = null;
                while (GetNextLine() != null)
                {
                    if (IsNewBoundaryDataBlock(CurrentLine))
                    {
                        if (parameterData != null)
                        {
                            FillBoundaryData(bcwData, header, parameterData);
                        }

                        header = ReadBcwHeaderData();
                        GetNextLine();
                        parameterData = ReadParameterMetaData();
                        bcwData.Add(header.BoundaryName, new List<IFunction>());
                    }

                    if (header == null)
                    {
                        Log.ErrorFormat("Invalid header in file {0}", InputFilePath);
                        return null;
                    }

                    if (parameterData == null)
                    {
                        Log.ErrorFormat("No valid parameter definition in file {0}", InputFilePath);
                        return null;
                    }

                    var values = new List<double>();
                    var stringValues = CurrentLine.Trim().Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    values.AddRange(
                        stringValues.Select(s => double.Parse(s, NumberStyles.Any, CultureInfo.InvariantCulture))
                                    .ToArray());

                    if (values.Count != parameterData.Count)
                    {
                        Log.ErrorFormat(
                            "Invalid parameter data in file {0}, line number {1}. Expecting {2} parameter values.",
                            InputFilePath, LineNumber, parameterData.Count);
                        return null;
                    }

                    parameterData.ForEach((pd, i) => pd.Values.Add(values[i]));
                }

                // store last block
                if (parameterData != null)
                {
                    FillBoundaryData(bcwData, header, parameterData);
                }

            }
            catch (Exception e)
            {
                Log.ErrorFormat("Could not parse line nr. {0} in file {1}", LineNumber, InputFilePath);
            }
            finally
            {
                CloseInputFile();
            }

            return bcwData;
        }

        private void FillBoundaryData(Dictionary<string, List<IFunction>> bcwData, BcwHeaderData header,
                                      IList<BcwParameter> parameterData)
        {
            bcwData[header.BoundaryName] = new List<IFunction>();
            bcwData[header.BoundaryName].AddRange(CreateFunctionsFromData(parameterData, header));
        }

        private IList<BcwParameter> ReadParameterMetaData()
        {
            var parameterData = new List<BcwParameter>();
            var line = CurrentLine;

            while (IsNewParameter(line))
            {
                var bcwParameter = new BcwParameter {Values = new List<double>()};
                var matches = RegularExpression.GetFirstMatch(ParameterPattern, line);
                bcwParameter.Name = matches.Groups["parname"].Value.Trim();
                bcwParameter.Unit = matches.Groups["unit"].Value.Trim();
                parameterData.Add(bcwParameter);

                line = GetNextLine();
            }
            return parameterData;
        }

        private BcwHeaderData ReadBcwHeaderData()
        {
            var header = new BcwHeaderData();

            var line = CurrentLine;
            header.BoundaryName = RegularExpression.GetFirstMatch(BoundaryNamePattern, line).Groups["value"].Value;
            line = GetNextLine();
            header.TimeFunction = RegularExpression.GetFirstMatch(TimeFunctionPattern, line).Groups["value"].Value;
            var refDateString =
                RegularExpression.GetFirstMatch(ReferenceDatePattern, GetNextLine()).Groups["value"].Value;
            header.ReferenceDateString = refDateString;
            line = GetNextLine();
            header.TimeUnit = RegularExpression.GetFirstMatch(TimeUnitPattern, line).Groups["value"].Value;
            line = GetNextLine();
            header.InterpolationType = RegularExpression.GetFirstMatch(InterpolPattern, line).Groups["value"].Value;
            return header;
        }

        private IList<IFunction> CreateFunctionsFromData(IList<BcwParameter> parameterData, BcwHeaderData header)
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
                throw new NotImplementedException(
                    string.Format("Reference date \"from model\" in bcw file {0} not (yet) supported", InputFilePath));
            }

            var timeParameter = parameterData.FirstOrDefault(pd => pd.Name == "time");
            if (timeParameter == null)
            {
                throw new Exception(string.Format("Missing time parameter in timeseries file {0}", InputFilePath));
            }

            var dateTimes =
                timeParameter.Values.Select(v => ConvertToDateTime(v, header.TimeUnit, referenceDate)).ToList();
            var waveheights = parameterData.Where(pd => pd.Name == "WaveHeight").ToList();
            var periods = parameterData.Where(pd => pd.Name == "Period").ToList();
            var directions = parameterData.Where(pd => pd.Name == "Direction").ToList();
            var spreadings = parameterData.Where(pd => pd.Name == "DirSpreading").ToList();

            for (int i = 0; i < waveheights.Count; ++i)
            {
                var func = WaveBoundaryCondition.CreateEmptyWaveEnergyFunction();

                func.Arguments[0].SetValues(dateTimes);
                func.Arguments[0].Unit = null;
                func.Components[0].SetValues(waveheights[i].Values);
                func.Components[0].Unit = GetUnitFromString(waveheights[i].Unit);
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

        public void Write(IDictionary<string, List<IFunction>> bcwData, string filePath)
        {
            OpenOutputFile(filePath);
            try
            {
                foreach (var boundary in bcwData)
                {
                    var boundaryName = boundary.Key;
                    var functions = boundary.Value;
                    var header = CreateHeaderFromFunctions(boundaryName, functions);
                    var parameters = CreateParametersFromFunctions(functions);

                    if (header == null || parameters == null)
                    {
                        Log.ErrorFormat("Could not write boundary condition data for boundary {0} to file {1}",
                                        boundaryName, OutputFilePath);
                    }

                    WriteBoundaryData(header, parameters);
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }
        
        private void WriteBoundaryData(BcwHeaderData header, IList<BcwParameter> sortedParameters)
        {
            // header
            WriteLine(string.Format("{0,-21}{1,-21}", "location", "\'" + header.BoundaryName + "\'").TrimEnd());
            WriteLine(string.Format("{0,-21}{1,-21}", "time-function", "\'" + header.TimeFunction + "\'").TrimEnd());
            WriteLine(string.Format("{0,-21}{1,-21}", "reference-time", header.ReferenceDateString).TrimEnd());
            WriteLine(string.Format("{0,-21}{1,-21}", "time-unit", "\'" + header.TimeUnit + "\'").TrimEnd());
            WriteLine(string.Format("{0,-21}{1,-21}", "interpolation", "\'" + header.InterpolationType + "\'").TrimEnd());

            // parameters
            foreach (var parameter in sortedParameters)
            {
                WriteLine(
                    string.Format("{0,-21}{1,-21}{2,-21}", "parameter", "\'" + parameter.Name + "\'",
                                  "unit \'" + parameter.Unit + "\'").TrimEnd());
            }

            // data
            var timeParameter = sortedParameters.First(p => p.Name == "time");
            for (int i = 0; i < timeParameter.Values.Count; ++i)
            {
                var time = timeParameter.Values[i].ToString("F2", CultureInfo.InvariantCulture);
                var line = string.Format("{0,8}", time);
                for (int j = 1; j < sortedParameters.Count; ++j)
                {
                    line += string.Format(" {0,8}", sortedParameters[j].Values[i].ToString("F4", CultureInfo.InvariantCulture));
                }
                WriteLine(line.TrimEnd());
            }
        }
        
        private BcwHeaderData CreateHeaderFromFunctions(string boundaryName, IList<IFunction> functions)
        {
            var func = functions.FirstOrDefault();
            if (func == null) return null;

            var header = new BcwHeaderData
                {
                    BoundaryName = boundaryName,
                    TimeFunction = func.Attributes[TimeFunctionAttributeName],
                    ReferenceDateString = func.Attributes[RefDateAttributeName],
                    TimeUnit = func.Attributes[TimeUnitAttributeName],
                    InterpolationType = "linear"
                };
            return header;
        }

        private IList<BcwParameter> CreateParametersFromFunctions(IList<IFunction> functions)
        {
            var func = functions.FirstOrDefault();
            if (func == null) return null;

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
                
                var waveheight = CreateBcwParameter(f.Components[0], "WaveHeight");
                var period = CreateBcwParameter(f.Components[1], "Period");
                var direction = CreateBcwParameter(f.Components[2], "Direction");
                var spreading = CreateBcwParameter(f.Components[3], "DirSpreading");
                
                parameters.Add(waveheight);
                parameters.Add(period);
                parameters.Add(direction);
                parameters.Add(spreading);
            }

            var sortedParameters = new List<BcwParameter>();
            sortedParameters.Add(parameters.First());
            sortedParameters.AddRange(parameters.Where(p => p.Name == "WaveHeight"));
            sortedParameters.AddRange(parameters.Where(p => p.Name == "Period"));
            sortedParameters.AddRange(parameters.Where(p => p.Name == "Direction"));
            sortedParameters.AddRange(parameters.Where(p => p.Name == "DirSpreading"));

            return sortedParameters;
        }

        private static Unit GetUnitFromString(string unitString)
        {
            if (unitString == "[m]")
                return new Unit("meter", "m");
            else if (unitString == "[s]")
                return new Unit("second", "s");
            else if (unitString == "[N^o]")
                return new Unit("degree", "deg");
            else
                return new Unit("", "-");
        }

        private static string GetStringFromUnit(IUnit unit)
        {
            if (unit.Name == "meter")
                return "[m]";
            if (unit.Name == "second")
                return "[s]";
            if (unit.Name == "degree")
                return "[N^o]";
            return "[-]";
        }

        private DateTime ConvertToDateTime(double value, string unit, DateTime referenceDate)
        {
            if (unit == "days")
            {
                return referenceDate.AddDays(value);
            }
            if (unit == "hours")
            {
                return referenceDate.AddHours(value);
            }
            if (unit == "minutes")
            {
                return referenceDate.AddMinutes(value);
            }
            if (unit == "seconds")
            {
                return referenceDate.AddSeconds(value);
            }
            throw new NotImplementedException(string.Format("Unit {0} for bcw file is not (yet) implemented", unit));
        }

        private double ConvertToBcwTime(DateTime dateTime, DateTime refDate, string unit)
        {
            if (unit == "days")
            {
                return (dateTime - refDate).TotalDays;
            }
            if (unit == "hours")
            {
                return (dateTime - refDate).TotalHours;
            }
            if (unit == "minutes")
            {
                return (dateTime - refDate).TotalMinutes;
            }
            if (unit == "seconds")
            {
                return (dateTime - refDate).TotalSeconds;
            }
            throw new NotImplementedException(string.Format("Unit {0} for bcw file is not (yet) implemented", unit));
        }

        private BcwParameter CreateBcwParameter(IVariable component, string bcwName)
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
            public string BoundaryName;
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