using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    public class RealTimeControlOutputFileFunctionStore : ReadOnlyNetCdfFunctionStoreBase, IFileBased
    {
        protected const string LongNameAttribute = "long_name";
        private const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss";
        private const string TimeDimensionName = "time";

        private readonly Dictionary<string, IMultiDimensionalArray<IFeature>> cachedFeatureArrays = new Dictionary<string, IMultiDimensionalArray<IFeature>>();
        private IList<IFeature> features;
        private ICoordinateSystem coordinateSystem;

        public RealTimeControlOutputFileFunctionStore()
        {
            dateTimeFormat = DateTimeFormat;
        }

        public ICoordinateSystem CoordinateSystem
        {
            get
            {
                return coordinateSystem;
            }
            set
            {
                coordinateSystem = value;
                Functions.OfType<IFeatureCoverage>().ForEach(c => c.CoordinateSystem = coordinateSystem);
            }
        }

        public IList<IFeature> Features
        {
            get
            {
                return features ?? new List<IFeature>();
            }
            set
            {
                features = value;

                List<FeatureCoverage> featureCoverages = Functions.OfType<FeatureCoverage>().ToList();
                if (featureCoverages.Any())
                {
                    featureCoverages.ForEach(c => SetFeatureCoverageFeatures(c));
                }
            }
        }

        protected override IList<string> TimeVariableNames
        {
            get
            {
                return new[]
                {
                    GetTimeVariableName(TimeDimensionName)
                };
            }
        }

        protected override IList<string> TimeDimensionNames
        {
            get
            {
                return new[]
                {
                    TimeDimensionName
                };
            }
        }

        protected override string GetTimeVariableName(string dimName)
        {
            return TimeDimensionName;
        }

        protected override string ReadReferenceDateFromFile(string timeVariableName)
        {
            var result = new DateTime(1970, 1, 1);
            return result.ToString(DateTimeFormatInfo.InvariantInfo.FullDateTimePattern, CultureInfo.InvariantCulture);
        }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            foreach (NetCdfVariableInfo timeVariable in dataVariables.Where(v => v.IsTimeDependent))
            {
                NetCdfVariable netcdfVariable = timeVariable.NetCdfDataVariable;
                string longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute);

                string coverageLongName;
                string outputVariableName;
                string featureName;
                ParseUserFriendlyVariableNamesFromLongName(longName, out coverageLongName, out outputVariableName, out featureName);

                IFunction function;
                IVariable<DateTime> functionTimeVariable;

                if (timeVariable.NumDimensions == 2)
                {
                    var coverage = new FeatureCoverage(coverageLongName)
                    {
                        IsEditable = false,
                        IsTimeDependent = true,
                        CoordinateSystem = CoordinateSystem
                    };

                    List<NetCdfDimension> dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();
                    var featureVariable = new Variable<IFeature>
                    {
                        IsEditable = false,
                        Name = featureName,
                        Attributes =
                        {
                            [NcNameAttribute] = netCdfFile.GetDimensionName(dimensions[1]),
                            [NcUseVariableSizeAttribute] = "false"
                        }
                    };

                    coverage.Arguments.Add(featureVariable);

                    functionTimeVariable = coverage.Time;
                    functionTimeVariable.InterpolationType = InterpolationType.Linear;

                    if (!SetFeatureCoverageFeatures(coverage))
                    {
                        yield break;
                    }

                    function = coverage;
                }

                else
                {
                    var timeSeries = new TimeSeries
                    {
                        Name = coverageLongName,
                        IsEditable = false
                    };

                    function = timeSeries;
                    functionTimeVariable = timeSeries.Time;
                }

                var outputVariable = new Variable<double>
                {
                    Name = outputVariableName,
                    IsEditable = false,
                    NoDataValue = MissingValue,
                    InterpolationType = InterpolationType.Linear
                };

                outputVariable.Attributes[NcNameAttribute] = netCdfFile.GetVariableName(netcdfVariable);
                outputVariable.Attributes[NcUseVariableSizeAttribute] = "true";

                function.Components.Add(outputVariable);

                functionTimeVariable.Name = "Time";
                functionTimeVariable.Attributes[NcNameAttribute] = TimeVariableNames[0];
                functionTimeVariable.Attributes[NcUseVariableSizeAttribute] = "true";
                functionTimeVariable.Attributes[NcRefDateAttribute] = timeVariable.ReferenceDate;
                functionTimeVariable.IsEditable = false;

                function.Store = this;

                yield return function;
            }
        }

        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(IVariable function, IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] == "false") // has no explicit variable
            {
                if (cachedFeatureArrays.ContainsKey(function.Name))
                {
                    return (MultiDimensionalArray<T>) cachedFeatureArrays[function.Name];
                }

                IMultiDimensionalArray<IFeature> featureArray = new MultiDimensionalArray<IFeature>(new List<IFeature>() {null}, new[]
                {
                    GetSize(function)
                });

                FeatureCoverage matchingFunction = Functions.OfType<FeatureCoverage>().FirstOrDefault(f => f.Arguments.Concat(f.Components).Contains(function));
                if (matchingFunction != null)
                {
                    featureArray = new MultiDimensionalArray<IFeature>(matchingFunction.Features, new[]
                    {
                        GetSize(function)
                    });
                    cachedFeatureArrays.Add(function.Name, featureArray);
                }

                return (MultiDimensionalArray<T>) featureArray;
            }

            return base.GetVariableValuesCore<T>(function, filters);
        }

        private bool SetFeatureCoverageFeatures(IFeatureCoverage coverage)
        {
            IFeature matchingFeature = Features.Where(f => f is INameable).FirstOrDefault(f => coverage.Name.Contains('_' + ((INameable) f).Name + '_'));

            coverage.Features = new EventedList<IFeature>();
            if (matchingFeature == null)
            {
                return false;
            }

            coverage.Features.Add(matchingFeature);
            return true;
        }

        private static void ParseUserFriendlyVariableNamesFromLongName(string longName, out string coverageLongName,
                                                                       out string outputVariableName, out string featureName)
        {
            /*
                 e.g. taken from sample RTC output file:
            
                 long_name = "Real-Time Control:output_Weir1_Crest level (s) -> Flow1D:weirs/Weir1/structure_crest_level"
                 long_name = "Flow1D:observations/Near pipe/water_level -> Real-Time Control:input_Near pipe_Water level (op)"; 
             */

            // Note: this parsing is done based on the sample output file provided... I expect it will have to change once the RTC kernel work is completed^

            coverageLongName = longName.Split(new[]
                                       {
                                           "->"
                                       }, StringSplitOptions.None)
                                       .FirstOrDefault(n => n.Contains("Real-Time Control:")) ?? string.Empty;

            coverageLongName = coverageLongName.Replace("Real-Time Control:", string.Empty).Trim();

            int index = coverageLongName.LastIndexOf('_');

            outputVariableName = index > 0 && index < coverageLongName.Length ? coverageLongName.Substring(index + 1, coverageLongName.Length - index - 1).Trim() : string.Empty;

            featureName = index > 0 && index < coverageLongName.Length ? coverageLongName.Substring(0, index).Replace("output_", string.Empty).Replace("input_", string.Empty).Trim() : string.Empty;
        }

        #region IFileBased implementation

        public void CreateNew(string path)
        {
            FileUtils.DeleteIfExists(path);
            Path = path;
        }

        public new void Close()
        {
            // Nothing to close.
        }

        public void Open(string path)
        {
            // Nothing to open.
        }

        public void CopyTo(string destinationPath)
        {
            if (!File.Exists(Path) || Equals(Path, destinationPath))
            {
                return;
            }

            string dir = new FileInfo(destinationPath).DirectoryName;
            FileUtils.CreateDirectoryIfNotExists(dir);
            FileUtils.CopyFile(Path, destinationPath);
        }

        public void SwitchTo(string newPath)
        {
            Path = newPath;
        }

        public void Delete()
        {
            FileUtils.DeleteIfExists(Path);
        }

        public IEnumerable<string> Paths
        {
            get
            {
                return new[]
                {
                    Path
                };
            }
        }

        public bool IsFileCritical
        {
            get
            {
                return false;
            }
        }

        public bool IsOpen
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Make a copy of the file if it is located in the DeltaShell working directory
        /// </summary>
        public bool CopyFromWorkingDirectory { get; }

        #endregion
    }
}