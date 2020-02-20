using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.SharpMapGis.ImportExport;

namespace DeltaShell.NGHS.IO.Store1D
{
    public abstract class Output1DFileReader<U> : IOutput1DFileReader<U> where U: ITimeDependentVariableMetaDataBase, new()
    {
        protected string timeVariableNameInNetCDFFile;
        protected string timeDimensionNameInNetCdfFile;
        protected string unitsAttributeKeyNameInNetCdfFile;
        protected string timeVariableUnitValuePrefixInNetCdfFile;
        protected string dateTimeFormat;
        protected string longNameAttributeKeyNameInNetCdfFile;
        
        public virtual OutputFile1DMetaData<U> ReadMetaData(string path, bool doValidation = true)
        {
            var times = ReadTimesFromNetCdfFile(path);
            var timeDependentVariableMetaData = ReadTimeDependentVariableMetaDataFromNetCdfFile(path);
            var locationMetaData = ReadLocationMetaDataFromNetCdfFile(timeDependentVariableMetaData, path);

            return new OutputFile1DMetaData<U>(times, locationMetaData, timeDependentVariableMetaData);
        }

        public double[,] GetAllVariableData(string path, string variableName, OutputFile1DMetaData<U> metaData)
        {
            using (var netCdfFileWrapper = new NetCdfFileWrapper(path))
            {
                return netCdfFileWrapper.GetValues2D<double>(variableName) ?? new double[0, 0];
            }
        }
        
        public IList<double> GetSelectionOfVariableData(string path, string variableName, int[] origin, int[] shape)
        {
            return DoWithNetCdfFile(path, (outputFile, timeDependentVariables) =>
            {
                var fileVariable = outputFile.GetVariableByName(variableName);

                var locationData = outputFile.Read(fileVariable, origin, shape);
                return locationData.OfType<object>().Select(Convert.ToDouble).ToList();
            });
        }
        
        private V DoWithNetCdfFile<V>(string path, Func<NetCdfFile, IList<U>, V> function, IList<U> timeDependentVariableMetaData = null)
        {
            NetCdfFile outputFile = null;
            try
            {
                outputFile = NetCdfFile.OpenExisting(path);
                return function(outputFile, timeDependentVariableMetaData);
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
            return DoWithNetCdfFile(path,  (outputFile, timeDependentVariables) =>
            {
                var timeVariable = outputFile.GetVariableByName(timeVariableNameInNetCDFFile);
                if (timeVariable == null) return new List<DateTime>();

                var t0 = ParseReferenceTime(outputFile, timeVariable);
                return ParseTimeVariable(path, t0);
            });
        }

        private IDictionary<U, IList<LocationMetaData>> ReadLocationMetaDataFromNetCdfFile(IList<U> timeDependentVariableMetaData, string path)
        {
            return DoWithNetCdfFile(path, (outputFile, timeDependentVariableMetaDatas) =>
            {
                var result = new Dictionary<U, IList<LocationMetaData>>();
                
                foreach (var timeDependentVariableMetaDataBase in timeDependentVariableMetaData)
                {
                    var variable = outputFile.GetVariableByName(timeDependentVariableMetaDataBase.Name);
                    
                    var netCdfDimensions = outputFile.GetDimensions(variable).ToList();
                    if (netCdfDimensions.Count != 2) continue;

                    var secondDimension = netCdfDimensions.LastOrDefault();
                    if (secondDimension == null) continue;

                    var secondDimensionName = outputFile.GetDimensionName(secondDimension);

                    var variableNames = new LocationVariableNames(secondDimensionName);

                    var locationIds = GetLocationData(path,variableNames.id, variableNames.OnNodes);
                    var branchIds = Parse1DNetCdfVariable<int>(path, variableNames.BranchId);
                    var chainages = Parse1DNetCdfVariable<double>(path, variableNames.Chainage);
                    var xCoordinates = Parse1DNetCdfVariable<double>(path, variableNames.XNodeCoordinate);
                    var yCoordinates = Parse1DNetCdfVariable<double>(path, variableNames.YNodeCoordinate);

                    result[timeDependentVariableMetaDataBase] = ParseLocationMetaData(locationIds, branchIds, chainages, xCoordinates, yCoordinates);
                }

                return result;
            });
        }

        private IList<string> GetLocationData(string path, string variableName, bool onNodes)
        {
            return onNodes
                ? ParseLocationIdVariable(path, variableName)
                : GenerateEdgeIds(path, variableName);
        }

        private List<string> GenerateEdgeIds(string path, string variableName)
        {
            var list = Parse1DNetCdfVariable<int>(path, variableName).ToList();
            var edgeNodesNameIds = new List<string>();
            for (int i = 0; i < list.Count; i += 2)
            {
                edgeNodesNameIds.Add(string.Format("{0}_{1}", list[i].ToString(),
                    list[i + 1].ToString()));
            }

            return edgeNodesNameIds;
        }

        private IList<LocationMetaData> ParseLocationMetaData(IList<string> locationIds, IList<int> branchIds, IList<double> chainages,
            IList<double> xCoordinates, IList<double> yCoordinates)
        {
            if (locationIds == null || branchIds == null || chainages == null || xCoordinates == null ||
                yCoordinates == null) return new List<LocationMetaData>();

            return locationIds.Where((s, i) => branchIds[i] != int.MinValue + 1).Select((id, index) =>
                    new LocationMetaData
                    {
                        Id = id,
                        BranchId = branchIds == null ? 0 : branchIds[index],
                        Chainage = chainages == null ? 0.0 : chainages[index],
                        XCoordinate = xCoordinates == null ? 0.0 : xCoordinates[index],
                        YCoordinate = yCoordinates == null ? 0.0 : yCoordinates[index]
                    })
                .ToList();
        }

        private IList<U> ReadTimeDependentVariableMetaDataFromNetCdfFile(string path)
        {
            return DoWithNetCdfFile(path, (outputFile, timeDependentVariables) =>
            {
                IList<U> timeDependentVariableMetaData = new List<U>();

                var dependentVariablesOnTimeDimension = outputFile.GetVariables()
                    .Where(v => outputFile.GetVariableDimensionNames(v)
                        .Contains(timeDimensionNameInNetCdfFile));

                foreach (var netCdfVariable in dependentVariablesOnTimeDimension)
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
        protected virtual U ParseVariableMetaData(string variableName, Dictionary<string, object> attributes)
        {
            var longName = attributes.FirstOrDefault(a => a.Key == longNameAttributeKeyNameInNetCdfFile).Value;
            var longNameString = longName == null ? string.Empty : longName.ToString();

            var unit = attributes.FirstOrDefault(a => a.Key == unitsAttributeKeyNameInNetCdfFile).Value;
            var unitString = unit == null ? string.Empty : unit.ToString();

            return new U { Name = variableName, LongName  = longNameString, Unit = unitString};
        }

        private class LocationVariableNames
        {
            public LocationVariableNames(string dimensionName)
            {
                OnNodes = dimensionName == "mesh1d_nNodes";
            }

            public bool OnNodes { get; }
            
            public string id
            {
                get { return OnNodes ? "mesh1d_node_id" : "mesh1d_edge_nodes"; }
            }

            public string BranchId
            {
                get { return OnNodes ? "mesh1d_node_branch" : "mesh1d_edge_branch"; }
            }

            public string Chainage
            {
                get { return OnNodes ? "mesh1d_node_offset" : "mesh1d_edge_offset"; }
            }
            
            public string XNodeCoordinate
            {
                get { return OnNodes ? "mesh1d_node_x" : "mesh1d_edge_x"; }
            }
            
            public string YNodeCoordinate
            {
                get { return OnNodes ? "mesh1d_node_y" : "mesh1d_edge_y"; }
            }
        }
    }
}