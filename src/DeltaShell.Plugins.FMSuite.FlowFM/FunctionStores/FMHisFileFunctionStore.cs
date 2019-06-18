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
    /// sections.
    /// These correspond to observation points and obs. cross sections in the model. These features can either be generated
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

        private static string featureName_stations = "stations";
        private static string featureName_CrossSection = "cross_section";
        private static string featureName_GeneralStructures = "general_structures";
        private static string featureName_Weirgens = "weirgens";
        private static string featureName_Gategens = "gategens";
        private static string featureName_Pumps = "pumps";

//        private static readonly IList<string> FeatureNames = new[]
//        {
//            featureName_stations,
//            featureName_CrossSection,
//            featureName_GeneralStructures,
//            featureName_Weirgens,
//            featureName_Gategens,
//            featureName_Pumps
//        };

        private Dictionary<string, IEnumerable<IFeature>> FeatureCoveragesByNames =
            new Dictionary<string, IEnumerable<IFeature>>()
            {
                {featureName_stations, null },
                {featureName_CrossSection, null },
                {featureName_GeneralStructures, null },
                {featureName_Weirgens, null },
                {featureName_Gategens, null },
                {featureName_Pumps, null },
            };

        private IDictionary<string, IMultiDimensionalArray<IFeature>> cachedFeatures =
            new Dictionary<string, IMultiDimensionalArray<IFeature>>()
            {
                {featureName_stations, null},
                {featureName_CrossSection, null},
                {featureName_GeneralStructures, null},
                {featureName_Weirgens, null},
                {featureName_Gategens, null},
                {featureName_Pumps, null},
            };

        private IList<KeyValuePair<string, string>> GeneralStuctures = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("general_structure_name", featureName_GeneralStructures),
            new KeyValuePair<string, string>("weirgen_name", featureName_Weirgens),
            new KeyValuePair<string, string>("gategen_name", featureName_Gategens)
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
            if (FeatureCoveragesByNames.ContainsKey(dimensionName))
            {
                IMultiDimensionalArray<IFeature> cachedArray;
                if (!cachedFeatures.TryGetValue(dimensionName, out cachedArray) || cachedArray == null)
                {
                    var features = FeatureCoveragesByNames[dimensionName].ToList();
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
            if (FeatureCoveragesByNames.TryGetValue(featureName, out features) && features != null)
            {
                coverage.Features= new EventedList<IFeature>(features);
            }
        }

        #region Feature Initialization

        private void InitializePumpFeatures(IEnumerable<IFeature> pumpFeatures)
        {
            var pumpVariable = netCdfFile.GetVariableByName("pump_name");
            if (pumpVariable == null) return;
            var names = netCdfFile.Read(pumpVariable)
                                  .Cast<char[]>()
                                  .Select(CharArrayToString);
            var results = new List<IFeature>();
            foreach (var name in names)
            {
                var feature = pumpFeatures.FirstOrDefault(m => m is IPump && (m as IPump).Name == name);
                results.Add(feature);
            }

            AddFeaturesToDictionary(featureName_Pumps, results);
        }

        private void InitializeGeneralStructuresFeatures(IEnumerable<IFeature> modelGeneralStructures)
        {
            var gsVariableList = GeneralStuctures.Select(gs => new KeyValuePair<string, NetCdfVariable>(gs.Value, netCdfFile.GetVariableByName(gs.Key)));
            foreach (var gsVariable in gsVariableList)
            {
                var netCdfVariable = gsVariable.Value;
                if (netCdfVariable == null)
                    continue;
                var names = netCdfFile.Read(netCdfVariable)
                                      .Cast<char[]>()
                                      .Select(CharArrayToString);
                var results = new List<IFeature>();
                foreach (var name in names)
                {
                    var feature = GetValidFeature(modelGeneralStructures, name);
                    results.Add(feature);
                }

               AddFeaturesToDictionary(gsVariable.Key, results);
            }
        }

        private void InitializeCrossSectionFeatures(IEnumerable<Feature2D> modelObsCrossSections)
        {
            var results = new List<IFeature>();

            var crossSectionNameVariable = netCdfFile.GetVariableByName("cross_section_name");
            if (crossSectionNameVariable == null)
            {
                return;
            }

            string[] names = netCdfFile.Read(crossSectionNameVariable)
                                       .Cast<char[]>().Select(CharArrayToString).ToArray();
            Array xs = netCdfFile.Read(netCdfFile.GetVariableByName("cross_section_x_coordinate"));
            Array ys = netCdfFile.Read(netCdfFile.GetVariableByName("cross_section_y_coordinate"));

            for (var i = 0; i < xs.GetLength(0); i++)
            {
                // first try to find the right one in the model features, otherwise create our own feature
                results.Add(modelObsCrossSections.FirstOrDefault(m => m.Name == names[i]) ??
                            CreateCrossSectionFromNetCdf(i, names, xs, ys));
            }

            AddFeaturesToDictionary(featureName_CrossSection, results);
        }

        private void InitializeStationFeatures(IEnumerable<Feature2D> modelObsPoints)
        {
            var results = new List<IFeature>();

            var stationIdVariable = netCdfFile.GetVariableByName("station_id");
            if (stationIdVariable == null) return;

            var ids = netCdfFile.Read(stationIdVariable)
                                .Cast<char[]>()
                                .Select(CharArrayToString).ToArray();

            // TODO: xs and yx are now time dependent, evetually we will need to re-think this... for now, just take the 1st dimension

            var xs = netCdfFile.Read(netCdfFile.GetVariableByName("station_x_coordinate"))
                               .Cast<double>()
                               .ToArray();
            var ys = netCdfFile.Read(netCdfFile.GetVariableByName("station_y_coordinate"))
                               .Cast<double>()
                               .ToArray();

            for (var i = 0; i < ids.Length; i++)
            {
                // first try to find the right one in the model features, otherwise create our own feature
                results.Add(modelObsPoints.FirstOrDefault(m => m.Name == ids[i]) ??
                            CreateStationFromNetCdf(i, ids, xs, ys));
            }

            AddFeaturesToDictionary(featureName_stations, results);
        }

        #endregion

        private void AddFeaturesToDictionary(string featureName, IEnumerable<IFeature> results)
        {
            if (FeatureCoveragesByNames.ContainsKey(featureName))
            {
                FeatureCoveragesByNames[featureName] = results;
            }
            else
            {
                FeatureCoveragesByNames.Add(featureName, results);
            }
        }

        private static IFeature GetValidFeature(IEnumerable<IFeature> modelGeneralStructures, string name)
        {
            return modelGeneralStructures.FirstOrDefault(m => m as IWeir != null && (m as IWeir).Name == name)
                   ?? CreateGeneralStructureFromNetCdf(name);
        }

        #region IFeature helpers

        private static Feature2D CreateCrossSectionFromNetCdf(int i, string[] names, Array xs, Array ys)
        {
            var coordinates = new List<Coordinate>();
            for (var j = 0; j < xs.GetLength(1); j++)
            {
                var x = (double)xs.GetValue(i, j);
                var y = (double)ys.GetValue(i, j);

                if (x < NetCdfConstants.FillValues.NcFillFloat) // use default fill value here..
                {
                    coordinates.Add(new Coordinate(x, y));
                }
            }

            return new Feature2D
            {
                Name = names[i],
                Geometry = new LineString(coordinates.ToArray()),
            };
        }

        private static Feature2D CreateStationFromNetCdf(int i, string[] ids, Array xs, Array ys)
        {
            return new Feature2D
            {
                Name = ids[i],
                Geometry = new Point((double)xs.GetValue(i), (double)ys.GetValue(i))
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

        private static string CharArrayToString(char[] chars)
        {
            return new string(chars).TrimEnd(new[]
            {
                '\0',
                ' '
            });
        }
    }
}