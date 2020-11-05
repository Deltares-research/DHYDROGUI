using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Wave.OutputData
{
    /// <summary>
    /// <see cref="WavhFileFunctionStore"/> extends the <see cref="FMNetCdfFileFunctionStore"/>
    /// in order to support wave history files.
    /// </summary>
    /// <seealso cref="FMNetCdfFileFunctionStore" />
    public class WavhFileFunctionStore : FMNetCdfFileFunctionStore
    {
        private static class StationKeys
        {
            public const string name = "station_name";
            public const string id = "station_id";
            public const string xCoordinate = "station_x_coordinate";
            public const string yCoordinate = "station_y_coordinate";
        }

        private static class AttributeKeys
        {
            public const string standardName = "standard_name";
            public const string longName = "long_name";
            public const string unit = "units";
        }

        private const string featureNameStations = "stations";

        private readonly IReadOnlyDictionary<string, IList<IFeature>> featuresDictionary =
            new Dictionary<string, IList<IFeature>>()
            {
                {featureNameStations, new List<IFeature>()},
            };

        /// <summary>
        /// Creates a new <see cref="WavhFileFunctionStore"/>.
        /// </summary>
        /// <param name="ncPath">The path to the netcdf file to read.</param>
        public WavhFileFunctionStore(string ncPath) : base(ncPath)
        {
            DisableCaching = true;


            using (ReconnectToMapFile())
            {
                InitializeStationFeatures();
            }

            foreach (IFeatureCoverage coverage in Functions.OfType<IFeatureCoverage>())
            {
                InsertFeaturesInCoverage(coverage);
            }
        }

        private void InsertFeaturesInCoverage(IFeatureCoverage coverage)
        {
            string featureName = coverage.FeatureVariable.Attributes[NcNameAttribute];

            if (featuresDictionary.TryGetValue(featureName, out IList<IFeature> features))
            {
                coverage.Features = new EventedList<IFeature>(features);
            }
        }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables) =>
            dataVariables.Where(IsValidVariable)
                         .Select(ConstructFunction);

        private bool IsValidVariable(NetCdfVariableInfo variableInfo) =>
            variableInfo.IsTimeDependent && IsDoublePrecision(variableInfo);

        private bool IsDoublePrecision(NetCdfVariableInfo variableInfo)
        {
            NetCdfDataType variableType = 
                netCdfFile.GetVariableDataType(variableInfo.NetCdfDataVariable);
            return variableType == NetCdfDataType.NcDoublePrecision;
        }

        private IFunction ConstructFunction(NetCdfVariableInfo variableInfo)
        {
            NetCdfVariable variable = variableInfo.NetCdfDataVariable;

            IFunction function = variableInfo.NumDimensions == 2
                                     ? ConstructCoverage(variable, variableInfo.ReferenceDate)
                                     : ConstructTimeSeries(variable, variableInfo.ReferenceDate);

            function.Components.Add(ConstructOutputVariable(variable));
            function.Store = this;

            return function;
        }

        private IVariable ConstructOutputVariable(NetCdfVariable variable)
        {
            string variableName = netCdfFile.GetVariableName(variable);

            string unitSymbol = netCdfFile.GetAttributeValue(variable, AttributeKeys.unit);
            var outputVariable = new Variable<double>
            {
                Name = variableName,
                IsEditable = false,
                Unit = new Unit(unitSymbol, unitSymbol),
                NoDataValue = MissingValue,
                InterpolationType = InterpolationType.Linear
            };

            outputVariable.Attributes[NcNameAttribute] = variableName;
            outputVariable.Attributes[NcUseVariableSizeAttribute] = "true";

            return outputVariable;
        }

        private IFunction ConstructTimeSeries(NetCdfVariable variable,
                                              string referenceDate)
        {
            string timeSeriesName = GetVariableName(variable);
            var timeSeries = new TimeSeries
            {
                Name = timeSeriesName,
                IsEditable = false
            };

            ConfigureTimeVariable(timeSeries.Time, referenceDate);

            return timeSeries;
        }

        private IFunction ConstructCoverage(NetCdfVariable variable,
                                            string referenceDate)
        {
            string coverageName = GetVariableName(variable);
            var coverage = new FeatureCoverage(coverageName)
            {
                IsEditable = false,
                IsTimeDependent = true,
            };

            coverage.Arguments.Add(ConstructFeatureVariable(variable));
            coverage.Time.InterpolationType = InterpolationType.Linear;

            ConfigureTimeVariable(coverage.Time, referenceDate);

            return coverage;
        }

        private IVariable<IFeature> ConstructFeatureVariable(NetCdfVariable netCdfVariable)
        {
            NetCdfDimension[] dimensions = 
                netCdfFile.GetDimensions(netCdfVariable).ToArray();

            string secondDimensionName = netCdfFile.GetDimensionName(dimensions[1]);

            var featureVariable = new Variable<IFeature>
            {
                IsEditable = false,
                Name = secondDimensionName
            };

            featureVariable.Attributes[NcNameAttribute] = secondDimensionName;
            featureVariable.Attributes[NcUseVariableSizeAttribute] = "false";

            return featureVariable;
        }

        private void ConfigureTimeVariable(IVariable<DateTime> timeVariable,
                                           string referenceDate)
        {
            timeVariable.Name = "Time";
            timeVariable.Attributes[NcNameAttribute] = TimeVariableNames[0];
            timeVariable.Attributes[NcUseVariableSizeAttribute] = "true";
            timeVariable.Attributes[NcRefDateAttribute] = referenceDate;
            timeVariable.IsEditable = false;
        }

        private string GetVariableName(NetCdfVariable variable)
        {
            string variableName = netCdfFile.GetVariableName(variable);
            string longName = netCdfFile.GetAttributeValue(variable, AttributeKeys.longName) ??
                              netCdfFile.GetAttributeValue(variable, AttributeKeys.standardName);
            return longName != null ? $"{longName} ({variableName})" : variableName;
        }

        private void InitializeStationFeatures()
        {
            IList<string> stationIds = GetStationIds();

            if (!stationIds.Any())
            {
                return;
            }

            double[] xs = GetNetCdfVariableIEnumerable<double>(StationKeys.xCoordinate).ToArray();
            double[] ys = GetNetCdfVariableIEnumerable<double>(StationKeys.yCoordinate).ToArray();

            IEnumerable<Point> points = xs.Zip(ys, (x, y) => new Point(x, y));
            IEnumerable<IFeature> stations = stationIds.Zip(points, CreateFeature2D);

            featuresDictionary[featureNameStations].AddRange(stations);
        }

        private static Feature2D CreateFeature2D(string idName, IGeometry geometry) =>
            new Feature2D
            {
                Name = idName,
                Geometry = geometry
            };

        private IList<string> GetStationIds()
        {
            IList<string> stationIds = GetNetCdfFeatureVariableNames(StationKeys.name);
            return stationIds.Any() ? stationIds : GetNetCdfFeatureVariableNames(StationKeys.id);
        }

        private IList<string> GetNetCdfFeatureVariableNames(string variableName) =>
                GetNetCdfVariableIEnumerable<char[]>(variableName)
                    .Select(CharArrayToString)
                    .ToList();

        private static string CharArrayToString(char[] chars) =>
            new string(chars).TrimEnd('\0', ' ');

        private IEnumerable<T> GetNetCdfVariableIEnumerable<T>(string variableName)
        {
            NetCdfVariable variableByName = netCdfFile.GetVariableByName(variableName);
            return variableByName != null ? 
                       netCdfFile.Read(variableByName).Cast<T>() : 
                       Enumerable.Empty<T>();
        }

        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(
            IVariable function, IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] != "false")
            {
                return base.GetVariableValuesCore<T>(function, filters);
            }

            string dimensionName = function.Attributes[NcNameAttribute];
            if (featuresDictionary.ContainsKey(dimensionName))
            {
                IMultiDimensionalArray<IFeature> cachedArray;
                if (!cachedFeatures.TryGetValue(dimensionName, out cachedArray) || cachedArray == null)
                {
                    List<IFeature> features = featuresDictionary[dimensionName].ToList();
                    int functionSize = GetSize(function);
                    cachedArray = new MultiDimensionalArray<IFeature>(features, new[]
                    {
                        functionSize
                    });
                    cachedFeatures[dimensionName] = cachedArray;
                }

                return (MultiDimensionalArray<T>) cachedArray;
            }

            throw new ArgumentException($"Unexpected dimension name: {dimensionName}");
        }

        private readonly IDictionary<string, IMultiDimensionalArray<IFeature>> cachedFeatures =
            new Dictionary<string, IMultiDimensionalArray<IFeature>>()
            {
                {featureNameStations, null},
            };
    }
}