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
        private const string standardNameAttribute = "standard_name";
        private const string longNameAttribute = "long_name";
        private const string unitAttribute = "units";
        private static readonly ILog log = LogManager.GetLogger(typeof(FMHisFileFunctionStore));

        public FMHisFileFunctionStore(string hisPath, ICoordinateSystem coordinateSystem = null, HydroArea area = null)
            : base(hisPath) //loads the actual functions
        {
            CoordinateSystem = coordinateSystem;

            using (ReconnectToMapFile())
            {
                InitializeFeatures(area?.GetDirectChildren().Where(c => c is INameable).OfType<IFeature>().ToArray());
            }

            // initialize 'Features' collection of each coverage
            Functions?.OfType<IFeatureCoverage>().ForEach(InsertFeaturesInCoverage);
        }

        protected FMHisFileFunctionStore() {}

        public ICoordinateSystem CoordinateSystem { get; set; }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            NetCdfVariableInfo[] dataVariablesArray = dataVariables.ToArray();

            LoadHisFileVariableNamesByDimensionToMap(dataVariablesArray);

            LoadHisFileVariableGeometriesByDimension();

            const string coordinatesAttribute = "coordinates";

            foreach (NetCdfVariableInfo timeVariable in dataVariablesArray.Where(v => v.IsTimeDependent))
            {
                NetCdfVariable netCdfVariable = timeVariable.NetCdfDataVariable;
                NetCdfDataType type = netCdfFile.GetVariableDataType(netCdfVariable);
                NetCdfDimension[] dimensions = netCdfFile.GetDimensions(netCdfVariable).ToArray();
                string variableName = netCdfFile.GetVariableName(netCdfVariable);

                string longName = netCdfFile.GetAttributeValue(netCdfVariable, longNameAttribute) ??
                                  netCdfFile.GetAttributeValue(netCdfVariable, standardNameAttribute);
                string coverageLongName = longName != null
                                              ? string.Format($"{longName} ({variableName})")
                                              : variableName;

                IFunction function;
                IVariable<DateTime> functionTimeVariable;

                if (timeVariable.NumDimensions >= 2)
                {
                    string secondDimensionName = netCdfFile.GetDimensionName(dimensions[1]);
                    string nodeCoordinatesVariableNames = netCdfFile.GetAttributeValue(netCdfVariable, coordinatesAttribute);
                    //check if this variable can be mapped to an input or created feature
                    if (!CanBeMappedToFeatureName(dimensions, nodeCoordinatesVariableNames))
                    {
                        log.Warn($"Cannot map dimension {secondDimensionName} to input or generated features, this maybe an old formatted his file. Using backward compatibility to read file");
                        LoadHisFileVariableNamesByDimensionToMapUsingBackWardsCompatibility(secondDimensionName, netCdfVariable);
                    }

                    //check if this variable can be mapped to an input or created feature geometry
                    if (!CanBeMappedToFeatureGeometry(dimensions))
                    {
                        log.Warn($"Cannot map dimension {secondDimensionName} to input or generated feature geometry, this maybe an old formatted his file. Using backward compatibility to read file");
                        LoadHisFileVariableGeometriesByDimensionToMapUsingBackWardsCompatibility(secondDimensionName, netCdfVariable);
                    }

                    var coverage = new FileBasedFeatureCoverage(coverageLongName)
                    {
                        IsEditable = false,
                        IsTimeDependent = true,
                        CoordinateSystem = CoordinateSystem
                    };

                    var featureVariable = new Variable<IFeature>
                    {
                        IsEditable = false,
                        Name = secondDimensionName,
                        Attributes =
                        {
                            [NcNameAttribute] = secondDimensionName,
                            [NcUseVariableSizeAttribute] = "false"
                        }
                    };

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

                string unitSymbol = netCdfFile.GetAttributeValue(netCdfVariable, unitAttribute);
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

            throw new ArgumentException($"Unexpected dimension name: {dimensionName}");
        }

        private void LoadHisFileVariableNamesByDimensionToMapUsingBackWardsCompatibility(string dimensionNameForThisTimeSeries, NetCdfVariable netCdfVariable)
        {
            if (mapDimensionToFeatureNamesForBackWardsCompatibilityFunctionsDictionary.ContainsKey(dimensionNameForThisTimeSeries))
            {
                string featureNamesVariableName = mapDimensionToFeatureNamesForBackWardsCompatibilityFunctionsDictionary[dimensionNameForThisTimeSeries](netCdfVariable, netCdfFile);
                timeSeriesIdsByDimensionNameDictionary[dimensionNameForThisTimeSeries] = GetNetCdfFeatureVariableNames(featureNamesVariableName);
                coordinateKeyByDimensionNameDictionary[dimensionNameForThisTimeSeries] = featureNamesVariableName;
            }

            if (mapDimensionToFeatureGeometriesForBackWardsCompatibilityFunctionsDictionary.ContainsKey(dimensionNameForThisTimeSeries)
                && timeSeriesIdsByDimensionNameDictionary.ContainsKey(dimensionNameForThisTimeSeries))
            {
                (string xCoordinateValuesName, string yCoordinateValuesName) = mapDimensionToFeatureGeometriesForBackWardsCompatibilityFunctionsDictionary[dimensionNameForThisTimeSeries](netCdfVariable, netCdfFile);
                Array xs = GetNetCdfVariableArray(xCoordinateValuesName);
                Array ys = GetNetCdfVariableArray(yCoordinateValuesName);
                var geometries = new List<IGeometry>();
                for (var i = 0; i < timeSeriesIdsByDimensionNameDictionary[dimensionNameForThisTimeSeries].Count(); i++)
                {
                    geometries.Add(xs.Rank == 1
                                       ? CreatePoint(i, xs, ys)
                                       : CreateLineString(i, xs, ys));
                }

                geometryByDimensionNameDictionary[dimensionNameForThisTimeSeries] = geometries;
            }
        }

        private void LoadHisFileVariableGeometriesByDimensionToMapUsingBackWardsCompatibility(string dimensionNameForThisTimeSeries, NetCdfVariable netCdfVariable)
        {
            if (mapDimensionToFeatureGeometriesForBackWardsCompatibilityFunctionsDictionary.ContainsKey(dimensionNameForThisTimeSeries)
                && timeSeriesIdsByDimensionNameDictionary.ContainsKey(dimensionNameForThisTimeSeries))
            {
                (string xCoordinateValuesName, string yCoordinateValuesName) = mapDimensionToFeatureGeometriesForBackWardsCompatibilityFunctionsDictionary[dimensionNameForThisTimeSeries](netCdfVariable, netCdfFile);
                Array xs = GetNetCdfVariableArray(xCoordinateValuesName);
                Array ys = GetNetCdfVariableArray(yCoordinateValuesName);
                var geometries = new List<IGeometry>();
                for (var i = 0; i < timeSeriesIdsByDimensionNameDictionary[dimensionNameForThisTimeSeries].Count(); i++)
                {
                    geometries.Add(xs.Rank == 1
                                       ? CreatePoint(i, xs, ys)
                                       : CreateLineString(i, xs, ys));
                }

                geometryByDimensionNameDictionary[dimensionNameForThisTimeSeries] = geometries;
            }
        }

        private bool CanBeMappedToFeatureName(NetCdfDimension[] dimensions, string nodeCoordinatesVariableNames)
        {
            var separators = new[]
            {
                " "
            };
            return nodeCoordinatesVariableNames != null
                   && dimensions != null
                   && dimensions.Any()
                   && dimensions
                      .Select(netCdfFile.GetDimensionName)
                      .Where(coordinateKeyByDimensionNameDictionary.ContainsKey)
                      .Any(dimensionName =>
                               nodeCoordinatesVariableNames
                                   .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                   .Any(coordinateVariableName => coordinateKeyByDimensionNameDictionary[dimensionName]
                                            .Equals(coordinateVariableName, StringComparison.InvariantCultureIgnoreCase)));
        }

        private bool CanBeMappedToFeatureGeometry(NetCdfDimension[] dimensions)
        {
            return dimensions != null
                   && dimensions.Any()
                   && dimensions
                      .Select(netCdfFile.GetDimensionName)
                      .Any(geometryByDimensionNameDictionary.ContainsKey);
        }

        private Variable<T> GenerateOutputVariable<T>(string variableName, string unitSymbol)
        {
            return new Variable<T>
            {
                Name = variableName,
                IsEditable = false,
                Unit = new Unit(unitSymbol, unitSymbol),
                NoDataValue = MissingValue,
                InterpolationType = InterpolationType.Linear,
                Attributes =
                {
                    [NcNameAttribute] = variableName,
                    [NcUseVariableSizeAttribute] = "true"
                }
            };
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

            NetCdfVariable[] netCdfVariables = netCdfFile.GetVariables().ToArray();
            foreach (NetCdfVariable netCdfVariable in netCdfVariables.Where(variable => netCdfFile.GetDimensions(variable).ToArray().Length == 0))
            {
                try
                {
                    NetCdfDataType type = netCdfFile.GetVariableDataType(netCdfVariable);
                    if (type != NetCdfDataType.NcInteger)
                    {
                        continue;
                    }

                    string geometryType = netCdfFile.GetAttributeValue(netCdfVariable, geometryTypeAttribute);

                    if (string.IsNullOrEmpty(geometryType))
                    {
                        continue;
                    }

                    (string geometryDimension, Array nodeCountValues) = LoadNodeCountValuesAndGeometryDimensionName(netCdfVariable);

                    if (string.IsNullOrEmpty(geometryDimension) || nodeCountValues.Length == 0)
                    {
                        continue;
                    }

                    string nodeCoordinatesVariableNames = netCdfFile.GetAttributeValue(netCdfVariable, nodeCoordinatesAttribute);

                    Array nodeXCoordinatesValues = LoadCoordinatesValues(nodeCoordinatesVariableNames, nodeCoordinatesXSubStringSearchValue);
                    if (nodeXCoordinatesValues.Length == 0)
                    {
                        continue;
                    }

                    Array nodeYCoordinatesValues = LoadCoordinatesValues(nodeCoordinatesVariableNames, nodeCoordinatesYSubStringSearchValue);
                    if (nodeYCoordinatesValues.Length == 0)
                    {
                        continue;
                    }

                    geometryByDimensionNameDictionary[geometryDimension] = GenerateGeometriesFromHisFile(geometryType, nodeCountValues, nodeXCoordinatesValues, nodeYCoordinatesValues);
                    geometryByDimensionNameDictionary[geometryDimension] = GenerateGeometriesFromHisFile(geometryType, nodeCountValues, nodeXCoordinatesValues, nodeYCoordinatesValues);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private (string, Array) LoadNodeCountValuesAndGeometryDimensionName(NetCdfVariable netCdfVariable)
        {
            const string nodeCountAttribute = "node_count";
            string nodeCountVariableName = netCdfFile.GetAttributeValue(netCdfVariable, nodeCountAttribute);
            NetCdfVariable nodeCountVariable = netCdfFile.GetVariableByName(nodeCountVariableName);
            NetCdfDataType type = netCdfFile.GetVariableDataType(nodeCountVariable);

            if (type != NetCdfDataType.NcInteger)
            {
                return (string.Empty, Array.Empty<int>());
            }

            NetCdfDimension[] nodeCountVariableDimensions = netCdfFile.GetDimensions(nodeCountVariable).ToArray();

            return nodeCountVariableDimensions.Length == 1
                       ? (netCdfFile.GetDimensionName(nodeCountVariableDimensions[0]), GetNetCdfVariableArray(nodeCountVariableName))
                       : (string.Empty, Array.Empty<int>());
        }

        private static IEnumerable<IGeometry> GenerateGeometriesFromHisFile(string geometryType, Array nodeCountValues, Array nodeXCoordinatesValues, Array nodeYCoordinatesValues)
        {
            for (var i = 0; i < nodeCountValues.Length; i++)
            {
                switch (geometryType.ToLowerInvariant())
                {
                    case "point":
                        yield return new Point((double) nodeXCoordinatesValues.GetValue(i), (double) nodeYCoordinatesValues.GetValue(i));
                        break;
                    case "line":
                        var coordinates = new List<Coordinate>();
                        for (var j = 0; j < (int) nodeCountValues.GetValue(i); j++)
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
            string[] separators =
            {
                " "
            };
            string coordinatesVariableName = nodeCoordinatesVariableNames
                                             .Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                             .SingleOrDefault(
                                                 name =>
                                                     name.IndexOf(nodeCoordinatesSubStringSearchValue, StringComparison.InvariantCultureIgnoreCase) >= 0);

            if (coordinatesVariableName == null)
            {
                return Array.Empty<double>();
            }

            NetCdfVariable coordinatesVariable = netCdfFile.GetVariableByName(coordinatesVariableName);
            if (coordinatesVariable == null)
            {
                return Array.Empty<double>();
            }

            NetCdfDataType type = netCdfFile.GetVariableDataType(coordinatesVariable);
            if (type != NetCdfDataType.NcDoublePrecision)
            {
                return Array.Empty<double>();
            }

            NetCdfDimension[] coordinatesVariableDimensions = netCdfFile.GetDimensions(coordinatesVariable)?.ToArray();
            return coordinatesVariableDimensions?.Length != 1 ? Array.Empty<double>() : GetNetCdfVariableArray(coordinatesVariableName);
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
                if (type != NetCdfDataType.NcCharacter)
                {
                    continue;
                }

                string attributeValue = netCdfFile.GetAttributeValue(netcdfVariable, cfRoleAttribute);
                if (string.IsNullOrEmpty(attributeValue)
                    || !attributeValue.Equals(timeSeriesIdAttribute, StringComparison.InvariantCultureIgnoreCase)
                    || variable.NumDimensions != 2)
                {
                    continue;
                }

                NetCdfDimension[] dimensions = netCdfFile.GetDimensions(netcdfVariable).ToArray();
                string dimensionNameForThisTimeSeries = netCdfFile.GetDimensionName(dimensions[0]);
                string variableName = netCdfFile.GetVariableName(netcdfVariable);
                timeSeriesIdsByDimensionNameDictionary[dimensionNameForThisTimeSeries] = GetNetCdfFeatureVariableNames(variableName);
                coordinateKeyByDimensionNameDictionary[dimensionNameForThisTimeSeries] = variableName;
            }
        }

        private void InsertFeaturesInCoverage(IFeatureCoverage coverage)
        {
            string featureName = coverage.FeatureVariable.Attributes[NcNameAttribute];
            if (featuresDictionary.TryGetValue(featureName, out IEnumerable<IFeature> features) && features != null)
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
        private readonly IDictionary<Type, Func<string, IGeometry, IWeirFormula, IFeature>> mapTypeGenerateDictionary =
            new Dictionary<Type, Func<string, IGeometry, IWeirFormula, IFeature>>()
            {
                {typeof(GroupableFeature2DPoint), (name, geometry, _) => CreateFeature2D(name, geometry)},
                {typeof(ObservationCrossSection2D), (name, geometry, _) => CreateFeature2D(name, geometry)},
                {typeof(IWeir), CreateGeneralStructureFromNetCdf},
                {typeof(IPump), (name, geometry, _) => new Pump2D(name) {Geometry = geometry}}
            };

        // Mapping dictionary used to read feature names for backwards compatibility.
        private readonly IDictionary<string, Func<NetCdfVariable, NetCdfFile, string>> mapDimensionToFeatureNamesForBackWardsCompatibilityFunctionsDictionary =
            new Dictionary<string, Func<NetCdfVariable, NetCdfFile, string>>()
            {
                {
                    featureNameCrossSection, (netCdfVariable, netCdfFile) => GetFeatureNamesVariableForBackWardsCompatibilityName(
                        netCdfFile,
                        netCdfVariable,
                        new[]
                        {
                            "cross_section_name"
                        })
                },
                {
                    featureNamePumps, (netCdfVariable, netCdfFile) => GetFeatureNamesVariableForBackWardsCompatibilityName(
                        netCdfFile,
                        netCdfVariable,
                        new[]
                        {
                            "pump_name",
                            "pump_id"
                        })
                },
                {
                    featureNameStations, (netCdfVariable, netCdfFile) =>
                    {
                        return GetFeatureNamesVariableForBackWardsCompatibilityName(
                            netCdfFile,
                            netCdfVariable,
                            new[]
                            {
                                "station_id",
                                "station_name"
                            });
                    }
                },
                {
                    featureNameGeneralStructures, (netCdfVariable, netCdfFile) => GetFeatureNamesVariableForBackWardsCompatibilityName(
                        netCdfFile,
                        netCdfVariable,
                        new[]
                        {
                            "general_structure_name",
                            "general_structure_id"
                        })
                },
                {
                    featureNameWeirgens, (netCdfVariable, netCdfFile) => GetFeatureNamesVariableForBackWardsCompatibilityName(
                        netCdfFile,
                        netCdfVariable,
                        new[]
                        {
                            "weirgen_name",
                            "weirgen_id"
                        })
                },
                {
                    featureNameGategens, (netCdfVariable, netCdfFile) => GetFeatureNamesVariableForBackWardsCompatibilityName(
                        netCdfFile,
                        netCdfVariable,
                        new[]
                        {
                            "gategen_name"
                        })
                }
            };

        private static string GetFeatureNamesVariableForBackWardsCompatibilityName(NetCdfFile netCdfFile, NetCdfVariable netcdfVariable, string[] defaultNames)
        {
            string featureNamesVariableNames = netCdfFile.GetAttributeValue(netcdfVariable, "coordinates");
            if (featureNamesVariableNames == null)
            {
                if (defaultNames.Length == 1)
                {
                    return defaultNames[0];
                }

                foreach (string defaultName in defaultNames)
                {
                    if (netCdfFile.GetVariableByName(defaultName) != null)
                    {
                        return defaultName;
                    }
                }

                return string.Empty;
            }

            var separator = new[]
            {
                " "
            };
            string featureNamesVariableName = featureNamesVariableNames
                                              .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                                              .SingleOrDefault(s =>
                                                                   s.IndexOf("x", StringComparison.InvariantCultureIgnoreCase) < 0 &&
                                                                   s.IndexOf("y", StringComparison.InvariantCultureIgnoreCase) < 0);
            if (featureNamesVariableName == null)
            {
                if (defaultNames.Length == 1)
                {
                    return defaultNames[0];
                }

                foreach (string defaultName in defaultNames)
                {
                    if (netCdfFile.GetVariableByName(defaultName) != null)
                    {
                        return defaultName;
                    }
                }
            }

            return featureNamesVariableName;
        }

        // Mapping dictionary used to read feature geometry for backwards compatibility.
        private readonly IDictionary<string, Func<NetCdfVariable, NetCdfFile, (string, string)>> mapDimensionToFeatureGeometriesForBackWardsCompatibilityFunctionsDictionary =
            new Dictionary<string, Func<NetCdfVariable, NetCdfFile, (string, string)>>()
            {
                {featureNameCrossSection, (netCdfVariable, netCdfFile) => GetFeatureGeometryXAndYVariableForBackWardCompatibilityNames(netCdfFile, netCdfVariable, "cross_section_x_coordinate", "cross_section_y_coordinate")},
                {featureNameStations, (netCdfVariable, netCdfFile) => GetFeatureGeometryXAndYVariableForBackWardCompatibilityNames(netCdfFile, netCdfVariable, "station_x_coordinate", "station_y_coordinate")},
                {featureNamePumps, (netCdfVariable, netCdfFile) => GetFeatureGeometryXAndYVariableForBackWardCompatibilityNames(netCdfFile, netCdfVariable, "pump_xmid", "pump_ymid")}
            };

        private static (string, string) GetFeatureGeometryXAndYVariableForBackWardCompatibilityNames(NetCdfFile netCdfFile, NetCdfVariable netCdfVariable, string defaultXCoordinateName, string defaultYCoordinateName)
        {
            string featureNamesVariableNames = netCdfFile.GetAttributeValue(netCdfVariable, "coordinates");
            if (featureNamesVariableNames == null)
            {
                return (defaultXCoordinateName, defaultYCoordinateName);
            }

            var separator = new[]
            {
                " "
            };
            string featureNamesXVariableName = featureNamesVariableNames
                                               .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                                               .SingleOrDefault(s =>
                                                                    s.IndexOf("x", StringComparison.InvariantCultureIgnoreCase) >= 0);
            string featureNamesYVariableName = featureNamesVariableNames
                                               .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                                               .SingleOrDefault(s =>
                                                                    s.IndexOf("y", StringComparison.InvariantCultureIgnoreCase) >= 0);
            return (featureNamesXVariableName ?? defaultXCoordinateName, featureNamesYVariableName ?? defaultYCoordinateName);
        }

        #endregion

        #region Feature Initialization

        private void InitializeFeatures(IFeature[] features)
        {
            foreach (string dimensionName in timeSeriesIdsByDimensionNameDictionary.Keys.Where(dn => !mapTypeDictionary.ContainsKey(dn)))
            {
                log.Warn($"could not find type for this dimension name {dimensionName}, can not safely map the dimension features to input or generate a structure of this type. Skipping reading");
            }

            foreach (string dimensionName in timeSeriesIdsByDimensionNameDictionary.Keys.Where(mapTypeDictionary.ContainsKey))
            {
                AddFeaturesToDictionary(dimensionName, GetOrCreateFeatures(features, dimensionName, timeSeriesIdsByDimensionNameDictionary[dimensionName].ToArray()).ToArray());
            }
        }

        private IEnumerable<IFeature> GetOrCreateFeatures(IFeature[] features, string dimensionName, string[] timeSeriesIds)
        {
            for (var i = 0; i < timeSeriesIds.Length; i++)
            {
                string name = timeSeriesIds[i];
                Type type = mapTypeDictionary[dimensionName];
                yield return features?
                                 .FirstOrDefault(m =>
                                                     m.GetType().Implements(type)
                                                     && m is INameable feature
                                                     && feature.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                             ?? CreateFeatureFromNetCdf(
                                 name,
                                 type,
                                 geometryByDimensionNameDictionary.ContainsKey(dimensionName)
                                     ? geometryByDimensionNameDictionary[dimensionName].ElementAt(i)
                                     : null,
                                 weirFormulaByDimensionName.ContainsKey(dimensionName)
                                     ? weirFormulaByDimensionName[dimensionName]()
                                     : null);
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

        private static IGeometry CreateLineString(int i, Array xs, Array ys)
        {
            if (xs is null || ys is null)
            {
                return null;
            }

            var coordinates = new List<Coordinate>();
            int arrayLength = xs.GetLength(1);
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
            {
                return null;
            }

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