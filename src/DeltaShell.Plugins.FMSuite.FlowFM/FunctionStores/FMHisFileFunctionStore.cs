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
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.NetCdf;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    /// <summary>
    /// Reads an Unstruct HIS file and acts as the backing store. The his files contains timeseries.
    /// These correspond to hydro objects in the model.
    /// Some of these features can be generated from
    /// the netcdf file (in case you import the HIS file standalone),
    /// or be inserted from the model, to ensure the instances are
    /// exactly the same. This is required to use functionality like 'Query timeseries' etc.
    /// </summary>
    public class FMHisFileFunctionStore : FMNetCdfFileFunctionStore
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FMHisFileFunctionStore));
        private const string standardNameAttribute = "standard_name";
        private const string longNameAttribute = "long_name";
        private const string unitAttribute = "units";

        public FMHisFileFunctionStore(string hisPath, ICoordinateSystem coordinateSystem = null, HydroArea area = null)
            : base(hisPath) //loads the actual functions
        {
            CoordinateSystem = coordinateSystem;

            using (ReconnectToMapFile())
            {
                InitializeFeatures(area?.GetDirectChildren().Where(c=> c is INameable).OfType<IFeature>().ToArray());
            }

            // initialize 'Features' collection of each coverage
            Functions?.OfType<IFeatureCoverage>().ForEach(InsertFeaturesInCoverage);
        }

        protected FMHisFileFunctionStore() {}

        public ICoordinateSystem CoordinateSystem { get; set; }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            var dataVariablesArray = dataVariables.ToArray();

            LoadHisFileVariableNamesByDimensionToMap(dataVariablesArray);

            LoadHisFileVariableGeometriesByDimension();

            const string coordinatesAttribute = "coordinates";

            foreach (NetCdfVariableInfo timeVariable in dataVariablesArray.Where(v => v.IsTimeDependent))
            {
                NetCdfVariable netcdfVariable = timeVariable.NetCdfDataVariable;
                NetCdfDataType type = netCdfFile.GetVariableDataType(netcdfVariable);
                NetCdfDimension[] dimensions = netCdfFile.GetDimensions(netcdfVariable).ToArray();
                string variableName = netCdfFile.GetVariableName(netcdfVariable);
                
                string longName = netCdfFile.GetAttributeValue(netcdfVariable, longNameAttribute) ??
                                  netCdfFile.GetAttributeValue(netcdfVariable, standardNameAttribute);
                string coverageLongName = longName != null
                                              ? string.Format($"{longName} ({variableName})")
                                              : variableName;
                
                IFunction function;
                IVariable<DateTime> functionTimeVariable;

                if (timeVariable.NumDimensions >= 2)
                {
                    string secondDimensionName = netCdfFile.GetDimensionName(dimensions[1]);
                    string nodeCoordinatesVariableNames = netCdfFile.GetAttributeValue(netcdfVariable, coordinatesAttribute);
                    //check if this variable can be mapped to an input or created feature
                    if (!CanBeMappedToFeatureName(dimensions, nodeCoordinatesVariableNames)) 
                        log.Warn($"Cannot map dimension {secondDimensionName} to input or generated features, this maybe an old formatted his file. Using backward compatibility to read file");

                    var coverage = new FileBasedFeatureCoverage(coverageLongName)
                    {
                        IsEditable = false,
                        IsTimeDependent = true,
                        CoordinateSystem = CoordinateSystem
                    };

                    
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

                string unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, unitAttribute);
                IVariable outputVariable = null;
                if (type == NetCdfDataType.NcDoublePrecision)
                {
                    outputVariable = GenerateOutputVariable<double>(variableName, unitSymbol);
                }
                else if (type == NetCdfDataType.NcInteger)
                {
                    outputVariable = GenerateOutputVariable<int>(variableName, unitSymbol);
                }
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

        private bool CanBeMappedToFeatureName(NetCdfDimension[] dimensions, string nodeCoordinatesVariableNames)
        {
            var separators = new[] { " " };
            return dimensions
                   .Select(netCdfFile.GetDimensionName)
                   .Where(coordinateKeyByDimensionNameDictionary.ContainsKey)
                   .Any(dimensionName => 
                            nodeCoordinatesVariableNames
                                .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                .Any(coordinateVariableName => coordinateKeyByDimensionNameDictionary[dimensionName]
                                         .Equals(coordinateVariableName, StringComparison.InvariantCultureIgnoreCase)));
        }

        private Variable<T> GenerateOutputVariable<T>(string variableName, string unitSymbol)
        {
            var outputVariable = new Variable<T>
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
        private void LoadHisFileVariableGeometriesByDimension()
        {
            // initialize features geometry by reading their geometry and
            // match by first dimension name
            // which will be the dimension name of the feature
            // second dimension name will be string length

            const string geometryTypeAttribute = "geometry_type";
            const string nodeCoordinatesAttribute = "node_coordinates";
            const string nodeCoordinatesXSubStringSearchValue = "x";
            const string nodeCoordinatesYSubStringSearchValue = "y";

            var netCdfVariables = netCdfFile.GetVariables().ToArray();
            foreach (NetCdfVariable netCdfVariable in netCdfVariables.Where(variable => netCdfFile.GetDimensions(variable).ToArray().Length == 0))
            {
                try
                {
                    NetCdfDataType type = netCdfFile.GetVariableDataType(netCdfVariable);
                    if (type != NetCdfDataType.NcInteger) continue;
                    
                    string geometryType = netCdfFile.GetAttributeValue(netCdfVariable, geometryTypeAttribute);

                    if (string.IsNullOrEmpty(geometryType)) continue;

                    (string geometryDimension, Array nodeCountValues) = LoadNodeCountValuesAndGeometryDimensionName(netCdfVariable);

                    if (string.IsNullOrEmpty(geometryDimension) || nodeCountValues.Length == 0) continue;

                    var nodeCoordinatesVariableNames = netCdfFile.GetAttributeValue(netCdfVariable, nodeCoordinatesAttribute);

                    Array nodeXCoordinatesValues = LoadCoordinatesValues(nodeCoordinatesVariableNames, nodeCoordinatesXSubStringSearchValue);
                    if(nodeXCoordinatesValues.Length == 0) continue;

                    Array nodeYCoordinatesValues = LoadCoordinatesValues(nodeCoordinatesVariableNames, nodeCoordinatesYSubStringSearchValue);
                    if(nodeYCoordinatesValues.Length == 0) continue;

                    geometryByDimensionNameDictionary[geometryDimension] = GenerateGeometriesFromHisFile(geometryType, nodeCountValues, nodeXCoordinatesValues, nodeYCoordinatesValues);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private (string, Array) LoadNodeCountValuesAndGeometryDimensionName(NetCdfVariable netcdfVariable)
        {
            const string nodeCountAttribute = "node_count";
            string nodeCountVariableName = netCdfFile.GetAttributeValue(netcdfVariable, nodeCountAttribute);
            NetCdfVariable nodeCountVariable = netCdfFile.GetVariableByName(nodeCountVariableName);
            NetCdfDataType type = netCdfFile.GetVariableDataType(nodeCountVariable);

            if (type != NetCdfDataType.NcInteger) return (string.Empty, Array.Empty<int>());

            var nodeCountVariableDimensions = netCdfFile.GetDimensions(nodeCountVariable).ToArray();

            return nodeCountVariableDimensions.Length == 1 
                       ? (netCdfFile.GetDimensionName(nodeCountVariableDimensions[0]), GetNetCdfVariableArray(nodeCountVariableName)) 
                       : (string.Empty, Array.Empty<int>());
        }

        private static IEnumerable<IGeometry> GenerateGeometriesFromHisFile(string geometryType, Array nodeCountValues, Array nodeXCoordinatesValues, Array nodeYCoordinatesValues)
        {
            for (int i = 0; i < nodeCountValues.Length; i++)
            {
                switch (geometryType.ToLowerInvariant())
                {
                    case "point":
                        yield return new Point((double) nodeXCoordinatesValues.GetValue(i), (double) nodeYCoordinatesValues.GetValue(i));
                        break;
                    case "line":
                        var coordinates = new List<Coordinate>();
                        for (int j = 0; j < (int) nodeCountValues.GetValue(i); j++)
                        {
                            coordinates.Add(new Coordinate((double) nodeXCoordinatesValues.GetValue(i + j), (double) nodeYCoordinatesValues.GetValue(i + j)));
                        }

                        yield return coordinates.Count == 1
                                         ? (IGeometry) new Point(coordinates[0])
                                         : new LineString(coordinates.ToArray());
                        break;
                }
            }
        }

        private Array LoadCoordinatesValues(string nodeCoordinatesVariableNames, string nodeCoordinatesSubStringSearchValue)
        {
            string[] separators = { " " };
            NetCdfDataType type;
            var coordinatesVariableName = nodeCoordinatesVariableNames
                                               .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                               .SingleOrDefault(
                                                   name => 
                                                       name.IndexOf(nodeCoordinatesSubStringSearchValue, StringComparison.InvariantCultureIgnoreCase) >= 0);

            if (coordinatesVariableName == null) return Array.Empty<double>();
            NetCdfVariable coordinatesVariable = netCdfFile.GetVariableByName(coordinatesVariableName);
            type = netCdfFile.GetVariableDataType(coordinatesVariable);
            if (type != NetCdfDataType.NcDoublePrecision) return Array.Empty<double>();
            var coordinatesVariableDimensions = netCdfFile.GetDimensions(coordinatesVariable).ToArray();
            return coordinatesVariableDimensions.Length != 1 ? Array.Empty<double>() : GetNetCdfVariableArray(coordinatesVariableName);
        }

        private void LoadHisFileVariableNamesByDimensionToMap(NetCdfVariableInfo[] dataVariables)
        {
            // initialize features by reading their names and
            // match by first dimension name
            // which will be the dimension name of the feature
            // second dimension name will be string length
            const string cfRoleAttribute = "cf_role";
            const string timeSeriesIdAttribute = "timeseries_id";

            foreach (NetCdfVariableInfo variable in dataVariables)
            {
                NetCdfVariable netcdfVariable = variable.NetCdfDataVariable;
                NetCdfDataType type = netCdfFile.GetVariableDataType(netcdfVariable);
                if (type != NetCdfDataType.NcCharacter) continue;

                string attributeValue = netCdfFile.GetAttributeValue(netcdfVariable, cfRoleAttribute);
                if (string.IsNullOrEmpty(attributeValue)
                    || !attributeValue.Equals(timeSeriesIdAttribute, StringComparison.InvariantCultureIgnoreCase)
                    || variable.NumDimensions != 2) continue;

                NetCdfDimension[] dimensions = netCdfFile.GetDimensions(netcdfVariable).ToArray();
                string dimensionNameForThisTimeSeries = netCdfFile.GetDimensionName(dimensions[0]);
                string variableName = netCdfFile.GetVariableName(netcdfVariable);
                timeSeriesIdsByDimensionNameDictionary[dimensionNameForThisTimeSeries] = GetNetCdfFeatureVariableNames(variableName);
                coordinateKeyByDimensionNameDictionary[dimensionNameForThisTimeSeries] = variableName;
            }
        }

        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(
            IVariable function, IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] != "false")
            {
                return base.GetVariableValuesCore<T>(function, filters);
            }

            string dimensionName = function.Attributes[NcNameAttribute];
            if (timeSeriesIdsByDimensionNameDictionary.ContainsKey(dimensionName))
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

            throw new ArgumentException(string.Format("Unexpected dimension name: {0}", dimensionName));
        }

        private void InsertFeaturesInCoverage(IFeatureCoverage coverage)
        {
            string featureName = coverage.FeatureVariable.Attributes[NcNameAttribute];
            IEnumerable<IFeature> features;
            if (featuresDictionary.TryGetValue(featureName, out features) && features != null)
            {
                coverage.Features = new EventedList<IFeature>(features);
            }
        }

        #region Feature names and coverage helpers

        private const string featureNameStations = "stations";
        private const string featureNameCrossSection = "cross_section";
        private const string featureNameGeneralStructures = "general_structures";
        private const string featureNameWeirgens = "weirgens";
        private const string featureNameGategens = "gategens";
        private const string featureNamePumps = "pumps";
        
        // Mapping dictionary used to relate under which name is an IFeature stored in the NetCdfFile.
        private readonly IDictionary<string, IEnumerable<IFeature>> featuresDictionary = new Dictionary<string, IEnumerable<IFeature>>();

        // Mapping dictionary used to relate under which dimension name is IFeature names are stored in the NetCdfFile.
        private readonly IDictionary<string, IEnumerable<string>> timeSeriesIdsByDimensionNameDictionary = new Dictionary<string, IEnumerable<string>>();

        // Mapping dictionary used to relate under which dimension name is IFeature names are stored in the NetCdfFile.
        private readonly IDictionary<string, string> coordinateKeyByDimensionNameDictionary = new Dictionary<string, string>();

        // Mapping dictionary used to relate under which dimension name is IFeature geometries are stored in the NetCdfFile.
        private readonly IDictionary<string, IEnumerable<IGeometry>> geometryByDimensionNameDictionary = new Dictionary<string, IEnumerable<IGeometry>>();

        // Mapping dictionary used to relate under which name are Features stored in the Coverages.
        private readonly IDictionary<string, IMultiDimensionalArray<IFeature>> cachedFeatures = new Dictionary<string, IMultiDimensionalArray<IFeature>>();
       
        // Mapping dictionary used to relate under which name are FeatureTypes are mapped.
        private readonly IDictionary<string, Type> mapTypeDictionary = 
        new Dictionary<string, Type>()
        {
            {featureNameStations, typeof(GroupableFeature2DPoint)},
            {featureNameCrossSection, typeof(ObservationCrossSection2D)},
            {featureNameGeneralStructures, typeof(IWeir)},
            {featureNameWeirgens, typeof(IWeir)},
            {featureNameGategens, typeof(IWeir)},
            {featureNamePumps, typeof(IPump)}
        };
        // Mapping dictionary used to relate under which name are FeatureTypes are mapped.
        private readonly IDictionary<string, Func<IWeirFormula>> weirFormulaByDimensionName = 
        new Dictionary<string, Func<IWeirFormula>>()
        {
            {featureNameGeneralStructures, () => new GeneralStructureWeirFormula()},
            {featureNameWeirgens, () => new SimpleWeirFormula()},
            {featureNameGategens, () => new GatedWeirFormula()},
        };
        
        // Mapping dictionary used to create features under which name are FeatureTypes are mapped.
        private IDictionary<Type, Func<string, IGeometry, IWeirFormula, IFeature>> mapTypeGenerateDictionary = 
        new Dictionary<Type, Func<string, IGeometry, IWeirFormula, IFeature>>()
        {
            {typeof(GroupableFeature2DPoint), (name, geometry, _) => CreateFeature2D(name, geometry)},
            {typeof(ObservationCrossSection2D), (name, geometry, _) => CreateFeature2D(name, geometry)},
            {typeof(IWeir), CreateGeneralStructureFromNetCdf},
            {typeof(IPump),(name, geometry, _) => new Pump2D(name){Geometry = geometry}}
            
        };

        #endregion

        #region Feature Initialization

        private void InitializeFeatures(IFeature[] features)
        {
            foreach (string dimensionName in timeSeriesIdsByDimensionNameDictionary.Keys)
            {
                var results = new List<IFeature>();
                for (var i =0; i < timeSeriesIdsByDimensionNameDictionary[dimensionName].Count(); i++)
                {
                    var name = timeSeriesIdsByDimensionNameDictionary[dimensionName].ElementAt(i);
                    IFeature validFeature = null;
                    Type type = null;
                    if (mapTypeDictionary.ContainsKey(dimensionName))
                    {
                        type = mapTypeDictionary[dimensionName];
                        validFeature = features?.FirstOrDefault(m => m.GetType().Implements(type) && m is INameable feature && feature.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                    }
                    else
                    {
                        log.Warn($"could not find type for this dimension name {dimensionName}, can not safely map the dimension features to input or generate a structure of this type. Skipping reading");
                        continue;
                    }

                    if (validFeature == null)
                    {
                        validFeature = CreateFeatureFromNetCdf(name, type, geometryByDimensionNameDictionary.ContainsKey(dimensionName) ? geometryByDimensionNameDictionary[dimensionName].ElementAt(i) : null, weirFormulaByDimensionName.ContainsKey(dimensionName) ? weirFormulaByDimensionName[dimensionName]() : null);
                    }

                    results.Add(validFeature);

                }
                AddFeaturesToDictionary(dimensionName, results);
            }
        }

        private IFeature CreateFeatureFromNetCdf(string name, Type type, IGeometry geometry, IWeirFormula formula)
        {
            if (type != null && mapTypeGenerateDictionary.ContainsKey(type))
            {
                return mapTypeGenerateDictionary[type](name, geometry, formula);
            }

            return null;
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
            IEnumerable<char[]> variableArray = GetNetCdfVariableIEnumerable<char[]>(variableName);
            List<string> variableContent = variableArray?.Select(CharArrayToString).ToList();

            if (variableContent == null || !variableContent.Any())
            {
                return new List<string>();
            }

            return variableContent;
        }

        private Array GetNetCdfVariableArray(string variableName)
        {
            NetCdfVariable variableByName = netCdfFile.GetVariableByName(variableName);
            return variableByName == null
                       ? null
                       : netCdfFile.Read(variableByName);
        }

        private IEnumerable<T> GetNetCdfVariableIEnumerable<T>(string variableName)
        {
            Array variableArray = GetNetCdfVariableArray(variableName);
            if (variableArray == null)
            {
                return Enumerable.Empty<T>();
            }

            IEnumerable<T> variableArrayT = variableArray.Cast<T>();
            return variableArrayT;
        }

        #endregion

        #region IFeature helpers

        private static Feature2D CreateFeature2D(string idName, IGeometry geometry)
        {
            return new Feature2D
            {
                Name = idName,
                Geometry = geometry
            };
        }

        private static Weir2D CreateGeneralStructureFromNetCdf(string name, IGeometry geometry, IWeirFormula formula)
        {
            return new Weir2D
            {
                Name = name,
                WeirFormula = formula,
                Geometry = geometry
            };
        }

        #endregion
    }
}