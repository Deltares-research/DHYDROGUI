using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Units;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    /// <summary>
    /// Reads an Unstruc HIS file and acts as the backing store. The his files contains timeseries on stations and cross
    /// sections and general structures.
    /// These correspond to observation points and obs. cross sections in the model as well as general structures. These features can either be generated
    /// from
    /// the netcdf file (in case you import the HIS file standalone), or be inserted from the model, to ensure the instances
    /// are
    /// exactly the same. This is required to use functionality like 'Query timeseries' etc.
    /// </summary>
    public class FMHisFileFunctionStore : FMNetCdfFileFunctionStore
    {
        protected const string StandardNameAttribute = "standard_name";
        protected const string LongNameAttribute = "long_name";
        protected const string UnitAttribute = "units";

        #region Feature names and coverage helpers

        private const string featureNameStations = "stations";
        private const string featureNameCrossSection = "cross_section";
        private const string featureNameGeneralStructures = "general_structures";
        private const string featureNameWeirgens = "weirgens";
        private const string featureNameGategens = "gategens";
        private const string featureNamePumps = "pumps";

        private readonly Dictionary<string, IEnumerable<IFeature>> featuresDictionary =
            new Dictionary<string, IEnumerable<IFeature>>()
            {
                {featureNameStations, null },
                {featureNameCrossSection, null },
                {featureNameGeneralStructures, null },
                {featureNameWeirgens, null },
                {featureNameGategens, null },
                {featureNamePumps, null },
            };

        private readonly IDictionary<string, IMultiDimensionalArray<IFeature>> cachedFeatures =
            new Dictionary<string, IMultiDimensionalArray<IFeature>>()
            {
                {featureNameStations, null},
                {featureNameCrossSection, null},
                {featureNameGeneralStructures, null},
                {featureNameWeirgens, null},
                {featureNameGategens, null},
                {featureNamePumps, null},
            };

        private readonly IList<KeyValuePair<string, string>> generalStuctures = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("general_structure_name", featureNameGeneralStructures),
            new KeyValuePair<string, string>("weirgen_name", featureNameWeirgens),
            new KeyValuePair<string, string>("gategen_name", featureNameGategens)
        };

        #endregion

        public ICoordinateSystem CoordinateSystem { get; set; }
        protected FMHisFileFunctionStore() { }

        public FMHisFileFunctionStore(string hisPath, ICoordinateSystem coordinateSystem = null, HydroArea area = null)
            : base(hisPath) //loads the actual functions
        {
            CoordinateSystem = coordinateSystem;

            using (ReconnectToMapFile())
            {
                InitializeStationFeatures((area?.ObservationPoints as IEnumerable<Feature2D>) ?? new Feature2D[0]);
                InitializeCrossSectionFeatures((area?.ObservationCrossSections as IEnumerable<Feature2D>) ?? new Feature2D[0]);
                InitializePumpFeatures(area?.Pumps ?? Enumerable.Empty<Pump2D>());
                InitializeGeneralStructuresFeatures((area?.Weirs as IEnumerable<Weir2D>) ?? new Weir2D[0]);
            }

            // initialize 'Features' collection of each coverage
            Functions?.OfType<IFeatureCoverage>().ForEach(InsertFeaturesInCoverage);
        }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            // add special velocity timeseries?
            foreach (NetCdfVariableInfo timeVariable in dataVariables.Where(v => v.IsTimeDependent))
            {
                var netcdfVariable = timeVariable.NetCdfDataVariable;

                var dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();

                var variableName = netCdfFile.GetVariableName(netcdfVariable);
                var longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute) ??
                                  netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttribute);
                var coverageLongName = longName != null
                                              ? string.Format($"{longName} ({variableName})")
                                              : variableName;

                IFunction function;
                IVariable<DateTime> functionTimeVariable;

                if (timeVariable.NumDimensions == 2)
                {
                    var coverage = new FileBasedFeatureCoverage(coverageLongName)
                    {
                        IsEditable = false,
                        IsTimeDependent = true,
                        CoordinateSystem = CoordinateSystem
                    };

                    var secondDimensionName = netCdfFile.GetDimensionName(dimensions[1]);
                    var featureVariable = new Variable<IFeature>
                    {
                        IsEditable = false,
                        Name = secondDimensionName
                    };

                    featureVariable.Attributes[NcNameAttribute] = secondDimensionName;
                    featureVariable.Attributes[NcUseVariableSizeAttribute] = "false";

                    coverage.Arguments.Add(featureVariable);
                    functionTimeVariable = coverage.Time;
                    functionTimeVariable.InterpolationType = InterpolationType.Linear;

                    InsertFeaturesInCoverage(coverage);

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

                string unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);
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

        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(
            IVariable function, IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] != "false")
            {
                return base.GetVariableValuesCore<T>(function, filters);
            }

            var dimensionName = function.Attributes[NcNameAttribute];
            if (featuresDictionary.ContainsKey(dimensionName))
            {
                IMultiDimensionalArray<IFeature> cachedArray;
                if (!cachedFeatures.TryGetValue(dimensionName, out cachedArray) || cachedArray == null)
                {
                    var features = featuresDictionary[dimensionName].ToList();
                    var functionSize = GetSize(function);
                    cachedArray = new MultiDimensionalArray<IFeature>(features, new[]{ functionSize });
                    cachedFeatures[dimensionName] = cachedArray;
                }

                return (MultiDimensionalArray<T>)cachedArray;
            }

            throw new ArgumentException(string.Format("Unexpected dimension name: {0}", dimensionName));

        }

        private void InsertFeaturesInCoverage(IFeatureCoverage coverage)
        {
            var featureName = coverage.FeatureVariable.Attributes[NcNameAttribute];
            IEnumerable<IFeature> features;
            if (featuresDictionary.TryGetValue(featureName, out features) && features != null)
            {
                coverage.Features= new EventedList<IFeature>(features);
            }
        }

        #region Feature Initialization

        private void InitializePumpFeatures(IEnumerable<IFeature> pumpFeatures)
        {
            // Extract all possible pumps for later pairing with features
            var pumpNames = GetNetCdfFeatureVariableNames("pump_name");
            if (!pumpNames.Any()) return;

            var results = new List<IFeature>();
            foreach (var name in pumpNames)
            {
                var validFeature = pumpFeatures.FirstOrDefault(m => m is IPump && (m as IPump).Name == name);
                if (validFeature == null)
                {
                    validFeature = new Pump2D(name);
                }

                results.Add(validFeature);
            }

            AddFeaturesToDictionary(featureNamePumps, results);
        }

        private void InitializeGeneralStructuresFeatures(IEnumerable<IFeature> modelGeneralStructures)
        {
            // Iterate over all possible general structures
            foreach (var gsVariable in generalStuctures)
            {
                // Find whether they are present in the netcdf file
                var names = GetNetCdfFeatureVariableNames(gsVariable.Key);
                var results = new List<IFeature>();
                foreach (var name in names)
                {
                    var validFeature = modelGeneralStructures.OfType<IWeir>().FirstOrDefault(m => m.Name.Equals(name))
                        ?? CreateGeneralStructureFromNetCdf(name);
                    results.Add(validFeature);
                }

                if (results.Any())
                {
                    AddFeaturesToDictionary(gsVariable.Value, results);
                }
            }
        }

        private void InitializeCrossSectionFeatures(IEnumerable<Feature2D> modelObsCrossSections)
        {
            // Extract all possible cross section names for later pairing with features
            var crossSectionNames = GetNetCdfFeatureVariableNames("cross_section_name");
            if (!crossSectionNames.Any()) return;

            // Get all coordinates available.
            var xs = GetNetCdfVariableArray("cross_section_x_coordinate");
            var ys = GetNetCdfVariableArray("cross_section_y_coordinate");

            // Match all available features with occurrences found in the NetCdfFile
            var results = new List<IFeature>();
            foreach (var crossSectionName in crossSectionNames)
            {
                // first try to find the right one in the model features, otherwise create our own feature
                var validFeature = modelObsCrossSections.FirstOrDefault(m => m.Name.Equals(crossSectionName));
                if (validFeature == null)
                {
                    var idx = crossSectionNames.IndexOf(crossSectionName);
                    var geometry = CreateLineString(idx, xs, ys);
                    validFeature = CreateFeature2D(crossSectionName, geometry);
                }
                if (validFeature != null)
                {
                    results.Add(validFeature);
                }
            }

            AddFeaturesToDictionary(featureNameCrossSection, results);
        }

        private void InitializeStationFeatures(IEnumerable<Feature2D> modelObsPoints)
        {
            // Extract all possible station ids for later pairing with features
            var stationIds = GetNetCdfFeatureVariableNames("station_id");
            if (!stationIds.Any())
                return;
            
            // Get all coordinates available.
            // TODO: xs and yx are now time dependent, evetually we will need to re-think this... for now, just take the 1st dimension
            var xs = GetNetCdfVariableArray<double>("station_x_coordinate").ToArray();
            var ys = GetNetCdfVariableArray<double>("station_y_coordinate").ToArray();

            // Match all available features with occurrences found in the NetCdfFile
            var results = new List<IFeature>();
            foreach (var stationId in stationIds)
            {
                var validFeature = modelObsPoints.FirstOrDefault(m => m.Name.Equals(stationId)); 
                if(validFeature == null)
                {
                    var idx = stationIds.IndexOf(stationId);
                    var point = CreatePoint(idx, xs, ys);
                    validFeature = CreateFeature2D(stationId, point);
                };   
                if (validFeature != null)
                {
                    results.Add(validFeature);
                }
            }

            AddFeaturesToDictionary(featureNameStations, results);
        }

        #endregion

        #region Private methods

        private void AddFeaturesToDictionary(string featureName, IEnumerable<IFeature> results)
        {
            if (featuresDictionary.ContainsKey(featureName))
            {
                featuresDictionary[featureName] = results;
            }
            else
            {
                featuresDictionary.Add(featureName, results);
            }
        }

        private static string CharArrayToString(char[] chars)
        {
            return new string(chars).TrimEnd(new[]
            {
                '\0',
                ' '
            });
        }

        private IList<string> GetNetCdfFeatureVariableNames(string variableName)
        {
            var variableArray = GetNetCdfVariableArray<char[]>(variableName);
            var variableContent = variableArray?.Select(CharArrayToString).ToList();

            if (variableContent == null || !variableContent.Any())
                return new List<string>();

            return variableContent;
        }

        private Array GetNetCdfVariableArray(string variableName)
        {
            var variableByName = netCdfFile.GetVariableByName(variableName);
            return variableByName == null 
                ? null
                : netCdfFile.Read(variableByName);
        }

        private IEnumerable<T> GetNetCdfVariableArray<T>(string variableName)
        {
            var variableArray = GetNetCdfVariableArray(variableName);
            if (variableArray == null)
                return Enumerable.Empty<T>();
            var variableArrayT = variableArray.Cast<T>();
            return variableArrayT;
        }

        #endregion

        #region IFeature helpers

        private static IGeometry CreateLineString(int i, Array xs, Array ys)
        {
            if (xs is null || ys is null)
                return null;

            var coordinates = new List<Coordinate>();
            var arrayLength = xs.GetLength(1);
            for (var j = 0; j < arrayLength; j++)
            {
                var x = (double) xs.GetValue(i, j);
                var y = (double) ys.GetValue(i, j);

                if (x < NetCdfConstants.FillValues.NcFillFloat) // use default fill value here..
                {
                    coordinates.Add(new Coordinate(x, y));
                }
            }
            var geometry = new LineString(coordinates.ToArray());
            return geometry;
        }

        private static IGeometry CreatePoint(int i, Array xs, Array ys)
        {
            if (xs is null || ys is null)
                return null;

            var xValue = (double) xs.GetValue(i);
            var yValue = (double) ys.GetValue(i);
            return new Point(xValue, yValue);
        }

        private static Feature2D CreateFeature2D(string idName, IGeometry geometry)
        {
            return new Feature2D
            {
                Name = idName,
                Geometry = geometry
            };
        }

        private static Weir2D CreateGeneralStructureFromNetCdf(string name)
        {
            return new Weir2D
            {
                Name = name,
                WeirFormula = new GeneralStructureWeirFormula()
            };
        }

        #endregion
    }
}