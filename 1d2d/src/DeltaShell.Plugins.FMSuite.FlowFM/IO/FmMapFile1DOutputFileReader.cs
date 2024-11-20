using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.Plugins.SharpMapGis.ImportExport;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public sealed class FmMapFile1DOutputFileReader
    {
        private const string timeVariableNameInNetCdfFile = "time";
        private const string timeDimensionNameInNetCdfFile = "time";
        private const string unitsAttributeKeyNameInNetCdfFile = "units";
        private const string timeVariableUnitValuePrefixInNetCdfFile = "seconds since";
        private const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private const string longNameAttributeKeyNameInNetCdfFile = "long_name";
        private readonly string dateTimeFormatWithZone = $"{dateTimeFormat} zzz";

        public OutputFile1DMetaData ReadMetaData(string path)
        {
            IList<DateTime> times = ReadTimesFromNetCdfFile(path);
            IList<TimeDependentVariableMetaDataBase> timeDependentVariableMetaData = ReadTimeDependentVariableMetaDataFromNetCdfFile(path);
            IDictionary<TimeDependentVariableMetaDataBase, IList<LocationMetaData>> locationMetaData = ReadLocationMetaDataFromNetCdfFile(timeDependentVariableMetaData, path);

            return new OutputFile1DMetaData(times, locationMetaData, timeDependentVariableMetaData);
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
                string errorMessage = string.Format((string) "Error reading NetCdf file {0}", path);
                throw new FileReadingException(errorMessage, ex);
            }
            finally
            {
                if (outputFile != null)
                {
                    outputFile.Close();
                }
            }
        }

        private IList<DateTime> ReadTimesFromNetCdfFile(string path)
        {
            return DoWithNetCdfFile(path, (outputFile) =>
            {
                NetCdfVariable timeVariable = outputFile.GetVariableByName(timeVariableNameInNetCdfFile);
                if (timeVariable == null)
                {
                    return new List<DateTime>();
                }

                DateTime t0 = ParseReferenceTime(outputFile, timeVariable);
                return ParseTimeVariable(path, t0);
            });
        }

        private IDictionary<TimeDependentVariableMetaDataBase, IList<LocationMetaData>> ReadLocationMetaDataFromNetCdfFile(IList<TimeDependentVariableMetaDataBase> timeDependentVariableMetaData, string path)
        {
            return DoWithNetCdfFile(path, (outputFile) =>
            {
                var result = new Dictionary<TimeDependentVariableMetaDataBase, IList<LocationMetaData>>();

                foreach (TimeDependentVariableMetaDataBase timeDependentVariableMetaDataBase in timeDependentVariableMetaData)
                {
                    NetCdfVariable variable = outputFile.GetVariableByName(timeDependentVariableMetaDataBase.Name);

                    List<NetCdfDimension> netCdfDimensions = outputFile.GetDimensions(variable).ToList();
                    if (netCdfDimensions.Count != 2)
                    {
                        continue;
                    }

                    NetCdfDimension secondDimension = netCdfDimensions.LastOrDefault();
                    if (secondDimension == null)
                    {
                        continue;
                    }

                    string secondDimensionName = outputFile.GetDimensionName(secondDimension);

                    var variableNames = new LocationVariableNames(secondDimensionName);

                    IList<string> locationIds = GetLocationData(path, variableNames.id, variableNames.OnNodes);
                    IList<int> branchIds = Parse1DNetCdfVariable<int>(path, variableNames.BranchId);
                    IList<double> chainages = Parse1DNetCdfVariable<double>(path, variableNames.Chainage);
                    IList<double> xCoordinates = Parse1DNetCdfVariable<double>(path, variableNames.XNodeCoordinate);
                    IList<double> yCoordinates = Parse1DNetCdfVariable<double>(path, variableNames.YNodeCoordinate);

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
            List<int> list = Enumerable.ToList<int>(Parse1DNetCdfVariable<int>(path, variableName));
            var edgeNodesNameIds = new List<string>();
            for (var i = 0; i < list.Count; i += 2)
            {
                edgeNodesNameIds.Add(string.Format("{0}_{1}", list[i].ToString(),
                                                   list[i + 1].ToString()));
            }

            return edgeNodesNameIds;
        }

        private IList<LocationMetaData> ParseLocationMetaData(IList<string> locationIds, IList<int> branchIds, IList<double> chainages,
                                                              IList<double> xCoordinates, IList<double> yCoordinates)
        {
            if (locationIds == null || !locationIds.Any()
                                    || branchIds == null || !branchIds.Any()
                                    || chainages == null || !chainages.Any()
                                    || xCoordinates == null || !xCoordinates.Any()
                                    || yCoordinates == null || !yCoordinates.Any())
            {
                return new List<LocationMetaData>();
            }

            return locationIds.Where((s, i) => branchIds[i] != int.MinValue + 1).Select((id, index) =>
                                                                                            new LocationMetaData
                                                                                            (
                                                                                                id,
                                                                                                branchIds[index],
                                                                                                chainages[index],
                                                                                                xCoordinates[index],
                                                                                                yCoordinates[index]
                                                                                            ))
                              .ToList();
        }

        private IList<TimeDependentVariableMetaDataBase> ReadTimeDependentVariableMetaDataFromNetCdfFile(string path)
        {
            return DoWithNetCdfFile(path, (outputFile) =>
            {
                IList<TimeDependentVariableMetaDataBase> timeDependentVariableMetaData = new List<TimeDependentVariableMetaDataBase>();

                IEnumerable<NetCdfVariable> dependentVariablesOnTimeDimension = outputFile.GetVariables()
                                                                                          .Where(v => outputFile.GetVariableDimensionNames(v)
                                                                                                                .Contains(timeDimensionNameInNetCdfFile));

                foreach (NetCdfVariable netCdfVariable in dependentVariablesOnTimeDimension)
                {
                    // necessary to loop through since we don't know what the time-dependent variables will be called
                    string variableName = outputFile.GetVariableName(netCdfVariable);
                    if (variableName == timeVariableNameInNetCdfFile)
                    {
                        continue;
                    }

                    var longName = outputFile.GetAttributeValue(netCdfVariable, longNameAttributeKeyNameInNetCdfFile) ?? string.Empty;
                    var unit = outputFile.GetAttributeValue(netCdfVariable, unitsAttributeKeyNameInNetCdfFile) ?? string.Empty;
                    timeDependentVariableMetaData.Add(new TimeDependentVariableMetaDataBase(variableName, longName, unit));
                }

                return timeDependentVariableMetaData;
            });
        }

        private DateTime ParseReferenceTime(NetCdfFile outputFile, NetCdfVariable timeVariable)
        {
            string unit = outputFile.GetAttributeValue(timeVariable, unitsAttributeKeyNameInNetCdfFile);
            string unitString = unit == null
                                    ? string.Empty
                                    : unit.Replace(timeVariableUnitValuePrefixInNetCdfFile, "").Trim();

            if (DateTime.TryParseExact(unitString,
                                       new[]
                                       {
                                           dateTimeFormat,
                                           dateTimeFormatWithZone
                                       },
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.AdjustToUniversal,
                                       out DateTime referenceTime))
            {
                return referenceTime;
            }

            var errorMessage = $"Unable to parse DateTime {unitString} from file {outputFile.Path}";

            throw new FileReadingException(errorMessage);
        }

        private IList<DateTime> ParseTimeVariable(string path, DateTime referenceTime)
        {
            using (var netCdfFileWrapper = new NetCdfFileWrapper(path))
            {
                IList<double> times = netCdfFileWrapper.GetValues1D<double>(timeVariableNameInNetCdfFile) ?? new List<double>();
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
        
        private class LocationVariableNames
        {
            private readonly string meshName;
            private readonly string location;

            public LocationVariableNames(string dimensionName)
            {
                OnNodes = dimensionName.EndsWith("_nNodes");

                location = OnNodes ? "node" : "edge";

                string suffix = OnNodes ? "_nNodes" : "_nEdges";
                meshName = dimensionName.Replace(suffix, "");
            }

            public bool OnNodes { get; }

            public string id => OnNodes ? $"{meshName}_{location}_id" : $"{meshName}_{location}_nodes";

            public string BranchId => $"{meshName}_{location}_branch";

            public string Chainage => $"{meshName}_{location}_offset";

            public string XNodeCoordinate => $"{meshName}_{location}_x";

            public string YNodeCoordinate => $"{meshName}_{location}_y";
        }
    }
}