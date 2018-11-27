using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.SharpMapGis.ImportExport;

namespace DeltaShell.NGHS.IO.Store1D
{
    public abstract class Output1DFileReader<T> : IOutput1DFileReader<T> where T: TimeDependentVariableMetaDataBase, new()
    {
        protected string timeVariableNameInNetCDFFile;
        protected string timeDimensionNameInNetCdfFile;
        protected string cfRoleAttributeNameInNetCdfFile;
        protected string cfRoleAttributeValueInNetCdfFile;
        protected string branchidVariableNameInNetCDFFile;
        protected string chainageVariableNameInNetCDFFile;
        protected string xCoordinateVariableNameInNetCDFFile;
        protected string yCoordinateVariableNameInNetCDFFile;
        protected string unitsAttributeKeyNameInNetCdfFile;
        protected string timeVariableUnitValuePrefixInNetCdfFile;
        protected string dateTimeFormat;
        protected string longNameAttributeKeyNameInNetCdfFile;
        
        public virtual OutputFile1DMetaData<LocationMetaData, T> ReadMetaData(string path, bool doValidation = true)
        {
            var times = ReadTimesFromNetCdfFile(path);
            var locationMetaData = ReadLocationMetaDataFromNetCdfFile(path);
            var timeDependentVariableMetaData = ReadTimeDependentVariableMetaDataFromNetCdfFile(path);
            
            return new OutputFile1DMetaData<LocationMetaData,T>(times, locationMetaData, timeDependentVariableMetaData);
        }

        public double[,] GetAllVariableData(string path, string variableName, OutputFile1DMetaData<LocationMetaData, T> metaData)
        {
            using (var netCdfFileWrapper = new NetCdfFileWrapper(path))
            {
                return netCdfFileWrapper.GetValues2D<double>(variableName) ?? new double[0, 0];
            }
        }
        
        public IList<double> GetSelectionOfVariableData(string path, string variableName, int[] origin, int[] shape)
        {
            return DoWithNetCdfFile(path, outputFile =>
            {
                var fileVariable = outputFile.GetVariableByName(variableName);

                var locationData = outputFile.Read(fileVariable, origin, shape);
                return locationData.OfType<object>().Select(Convert.ToDouble).ToList();
            });
        }
        
        private T DoWithNetCdfFile<T>(string path, Func<NetCdfFile, T> function)
        {
            NetCdfFile outputFile = null;
            try
            {
                outputFile = NetCdfFile.OpenExisting(path);
                return function(outputFile);
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format((string)"Error reading NetCdf file {0}", path);
                throw new FileReadingException(errorMessage, ex);
            }
            finally
            {
                if (outputFile != null)
                    outputFile.Close();
            }
        }

        private IList<DateTime> ReadTimesFromNetCdfFile(string path)
        {
            return DoWithNetCdfFile(path, outputFile =>
            {
                var timeVariable = outputFile.GetVariableByName(timeVariableNameInNetCDFFile);
                if (timeVariable == null) return new List<DateTime>();

                var t0 = ParseReferenceTime(outputFile, timeVariable);
                return ParseTimeVariable(path, t0);
            });
        }

        private IList<LocationMetaData> ReadLocationMetaDataFromNetCdfFile(string path)
        {
            return DoWithNetCdfFile(path, outputFile =>
            {
                IList<string> locationIds = null;
                IList<int> branchIds = null;
                IList<double> chainages = null;
                IList<double> xCoordinates = null;
                IList<double> yCoordinates = null;

                var locationSpecificVariables = outputFile.GetVariables()
                    .Where(v => !outputFile.GetVariableDimensionNames(v)
                        .Contains(timeDimensionNameInNetCdfFile));

                foreach (var netCdfVariable in locationSpecificVariables)
                {
                    // it's necessary to loop through since we don't know what the 'location'_id variable will be called
                    var variableName = outputFile.GetVariableName(netCdfVariable);

                    var attributes = outputFile.GetAttributes(netCdfVariable);
                    if (attributes.Any(a =>
                    {
                        return a.Key == cfRoleAttributeNameInNetCdfFile &&
                               a.Value.ToString() == cfRoleAttributeValueInNetCdfFile;
                    }))
                    {  
                        // 'location'_id variable identified by CfRole attribute
                        locationIds = ParseLocationIdVariable(path, variableName);
                        continue;
                    }

                    
                    if(variableName == branchidVariableNameInNetCDFFile)
                        branchIds = Parse1DNetCdfVariable<int>(path, variableName);
                    
                    if (variableName == chainageVariableNameInNetCDFFile)
                        chainages = Parse1DNetCdfVariable<double>(path, variableName);

                    
                    if (variableName == xCoordinateVariableNameInNetCDFFile)
                        xCoordinates = Parse1DNetCdfVariable<double>(path, variableName);

                    if (variableName == yCoordinateVariableNameInNetCDFFile)
                        yCoordinates = Parse1DNetCdfVariable<double>(path, variableName);
                }

                return ParseLocationMetaData(locationIds, branchIds, chainages, xCoordinates, yCoordinates);
            });
        }

        private IList<T> ReadTimeDependentVariableMetaDataFromNetCdfFile(string path)
        {
            return DoWithNetCdfFile(path, outputFile =>
            {
                IList<T> timeDependentVariableMetaData = new List<T>();

                var timeDependentVariables = outputFile.GetVariables()
                    .Where(v => outputFile.GetVariableDimensionNames(v)
                        .Contains(timeDimensionNameInNetCdfFile));

                foreach (var netCdfVariable in timeDependentVariables)
                {
                    // necessary to loop through since we don't know what the time-dependent variables will be called
                    var variableName = outputFile.GetVariableName(netCdfVariable);
                    if(variableName == timeVariableNameInNetCDFFile) continue;

                    var attributes = outputFile.GetAttributes(netCdfVariable);

                    timeDependentVariableMetaData.Add(ParseVariableMetaData(variableName, attributes));
                }

                return timeDependentVariableMetaData;
            });
        }

        private DateTime ParseReferenceTime(NetCdfFile outputFile, NetCdfVariable timeVariable)
        {
            var attributes = outputFile.GetAttributes(timeVariable);

            var unit = attributes.FirstOrDefault(a => a.Key == unitsAttributeKeyNameInNetCdfFile).Value;
            var unitString = unit == null 
                ? string.Empty 
                : unit.ToString().Replace(timeVariableUnitValuePrefixInNetCdfFile, "").Trim();
            
            DateTime referenceTime;
            if (!DateTime.TryParseExact(unitString, dateTimeFormat, 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out referenceTime))
            {
                var errorMessage = string.Format(
                    (string)"Unable to parse DateTime {0} from file {1}",
                    unitString, outputFile.Path);

                throw new FileReadingException(errorMessage);
            }

            return referenceTime;
        }

        private IList<DateTime> ParseTimeVariable(string path, DateTime referenceTime)
        {
            using (var netCdfFileWrapper = new NetCdfFileWrapper(path))
            {
                var times = netCdfFileWrapper.GetValues1D<double>(timeVariableNameInNetCDFFile) ?? new List<double>();
                return times.Select(referenceTime.AddSeconds).ToList();
            }  
        }
        
        private IList<string> ParseLocationIdVariable(string path, string variableName)
        {
            using (var netCdfFileWrapper = new NetCdfFileWrapper(path))
            {
                return (netCdfFileWrapper.GetValues1D<char[]>(variableName) ?? new List<char[]>())
                    .Select(idString => new string(idString).Trim())
                    .ToList();
            }
        }

        private IList<T> Parse1DNetCdfVariable<T>(string path, string variableName)
        {
            using (var netCdfFileWrapper = new NetCdfFileWrapper(path))
            {
                return netCdfFileWrapper.GetValues1D<T>(variableName) ?? new List<T>();
            }
        }
        
        private IList<LocationMetaData> ParseLocationMetaData(IList<string> locationIds, IList<int> branchIds, IList<double> chainages, 
            IList<double> xCoordinates, IList<double> yCoordinates)
        {
            if(locationIds == null) return new List<LocationMetaData>();

            return locationIds.Where((s, i) => branchIds[i] != int.MinValue+1).Select((id, index) => new LocationMetaData(id,
                    branchIds == null ? 0 : branchIds[index],
                    chainages == null ? 0.0 : chainages[index],
                    xCoordinates == null ? 0.0 : xCoordinates[index],
                    yCoordinates == null ? 0.0 : yCoordinates[index]))
                .ToList();
        }

        protected virtual T ParseVariableMetaData(string variableName, Dictionary<string, object> attributes)
        {
            var longName = attributes.FirstOrDefault(a => a.Key == longNameAttributeKeyNameInNetCdfFile).Value;
            var longNameString = longName == null ? string.Empty : longName.ToString();

            var unit = attributes.FirstOrDefault(a => a.Key == unitsAttributeKeyNameInNetCdfFile).Value;
            var unitString = unit == null ? string.Empty : unit.ToString();
            /*
            var aggregationOption = attributes.FirstOrDefault(a => a.Key == aggregationOptionAttributeKeyNameInNetCdfFile).Value;

            AggregationOptions option = AggregationOptions.Current;// default to Current
            if (aggregationOption != null)
            {
                var aggregationOptionString = aggregationOption.ToString();
                aggregationOptionString = aggregationOptionString.First().ToString().ToUpper() + string.Join("", aggregationOptionString.Skip(1));
                if (!Enum.TryParse(aggregationOptionString, true, out option))
                {
                    option = AggregationOptions.Current;// default to Current
                }
            }*/
            
            return new T { Name = variableName, LongName  = longNameString, Unit = unitString};
        }
    }
}