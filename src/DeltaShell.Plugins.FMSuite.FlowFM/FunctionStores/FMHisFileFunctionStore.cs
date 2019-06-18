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
        private readonly IList<IFeature> stationFeatures;
        private readonly IList<IFeature> crossSectionFeatures;
        private readonly IList<IFeature> generalStructuresFeatures;
        protected const string StandardNameAttribute = "standard_name";
        protected const string LongNameAttribute = "long_name";
        protected const string UnitAttribute = "units";
        private const string featureName_Stations = "stations";
        private const string featureName_CrossSection = "cross_section";
        private const string featureName_GeneralStructures = "general_structures";

//        private static readonly IList<string> DiscardedFeatures = new[]
//        {
////            "weirgens",
////            "gategens",
////            "pumps"
//        };

        // nhib
        protected FMHisFileFunctionStore() {}

        public FMHisFileFunctionStore(string hisPath, ICoordinateSystem coordinateSystem = null, HydroArea area = null)
            : base(hisPath) //loads the actual functions
        {
            CoordinateSystem = coordinateSystem;

            using (ReconnectToMapFile())
            {
                stationFeatures = InitializeStationFeatures((area?.ObservationPoints as IEnumerable<Feature2D>) ?? new Feature2D[0]);
                crossSectionFeatures = InitializeCrossSectionFeatures((area?.ObservationCrossSections as IEnumerable<Feature2D>) ?? new Feature2D[0]);
                generalStructuresFeatures = InitializeGeneralStructuresFeatures((area?.Weirs as IEnumerable<Weir2D>) ?? new Weir2D[0]).ToList();
            }

            // initialize 'Features' collection of each coverage
            foreach (IFeatureCoverage featureCoverage in Functions.OfType<IFeatureCoverage>())
            {
                InsertFeaturesInCoverage(featureCoverage);
            }
        }

        public ICoordinateSystem CoordinateSystem { get; set; }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            // add special velocity timeseries?
            foreach (NetCdfVariableInfo timeVariable in dataVariables.Where(v => v.IsTimeDependent))
            {
                NetCdfVariable netcdfVariable = timeVariable.NetCdfDataVariable;

                List<NetCdfDimension> dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();

                string variableName = netCdfFile.GetVariableName(netcdfVariable);
                string longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute) ??
                                  netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttribute);
                string coverageLongName = longName != null
                                              ? string.Format("{0} ({1})", longName, variableName)
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

                    string secondDimensionName = netCdfFile.GetDimensionName(dimensions[1]);

//                    if (DiscardedFeatures.Contains(secondDimensionName))
//                    {
//                        continue;
//                    }

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

        private void InsertFeaturesInCoverage(IFeatureCoverage coverage)
        {
            string featureName = coverage.FeatureVariable.Attributes[NcNameAttribute];
            if (featureName == featureName_Stations && stationFeatures != null)
            {
                coverage.Features = new EventedList<IFeature>(stationFeatures);
            }

            if (featureName == featureName_CrossSection && crossSectionFeatures != null)
            {
                coverage.Features = new EventedList<IFeature>(crossSectionFeatures);
            }

            if (featureName == featureName_GeneralStructures && generalStructuresFeatures != null)
            {
                coverage.Features = new EventedList<IFeature>(generalStructuresFeatures);
            }
        }

        private IMultiDimensionalArray<IFeature> cachedStationsArray;
        private IMultiDimensionalArray<IFeature> cachedCrossSectionsArray;
        private IMultiDimensionalArray<IFeature> cachedGeneralStructures;

        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(
            IVariable function, IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] == "false"
            ) // has no explicit variable: stations / cross sections, composited from multiple vars
            {
                string dimensionName = function.Attributes[NcNameAttribute];
                switch (dimensionName)
                {
                    case featureName_Stations:
                        if (cachedStationsArray == null)
                        {
                            cachedStationsArray = new MultiDimensionalArray<IFeature>(stationFeatures,
                                                                                      new[]
                                                                                      {
                                                                                          GetSize(function)
                                                                                      });
                        }

                        return (MultiDimensionalArray<T>) cachedStationsArray;
                    case featureName_CrossSection:
                        if (cachedCrossSectionsArray == null)
                        {
                            cachedCrossSectionsArray = new MultiDimensionalArray<IFeature>(crossSectionFeatures,
                                                                                           new[]
                                                                                           {
                                                                                               GetSize(function)
                                                                                           });
                        }

                        return (MultiDimensionalArray<T>) cachedCrossSectionsArray;
                    case featureName_GeneralStructures:
                        if (cachedGeneralStructures == null)
                        {
                            cachedGeneralStructures = new MultiDimensionalArray<IFeature>(generalStructuresFeatures,
                                                                                          new[]
                                                                                          {
                                                                                              GetSize(function)
                                                                                          });
                        }

                        return (MultiDimensionalArray<T>) cachedGeneralStructures;
                    default:
                        throw new ArgumentException(string.Format("Unexpected dimension name: {0}", dimensionName));
                }
            }

            return base.GetVariableValuesCore<T>(function, filters);
        }

        //        private string GetNetCdfName(IWeirFormula weirFormula)
        //        {
        //            var name = "general_structure_name";
        //
        //            if (weirFormula as SimpleWeirFormula != null)
        //                return "weirgen_name";
        //
        //            if (weirFormula as GatedWeirFormula != null)
        //                return "gategen_weir_name";
        //
        //            return name;
        //        }

        private IList<string> GeneralStuctures = new List<string>()
        {
            "general_structure_name",
            "weirgen_name",
            "gategen_name"
        };

        private IList<IFeature> InitializeGeneralStructuresFeatures(IEnumerable<IFeature> modelGeneralStructures)
        {
            var results = new List<IFeature>();

            var variables = GeneralStuctures.Select(gs => netCdfFile.GetVariableByName(gs));
            foreach (var netCdfVariable in variables)
            {
                if (netCdfVariable == null)
                    continue;
                var names = netCdfFile.Read(netCdfVariable)
                                      .Cast<char[]>()
                                      .Select(CharArrayToString);
                foreach (var name in names)
                {
                    var feature = GetValidFeature(modelGeneralStructures, name);
                    results.Add(feature);
                }
            }

            return results;
        }

        private static IFeature GetValidFeature(IEnumerable<IFeature> modelGeneralStructures, string name)
        {
            return modelGeneralStructures.FirstOrDefault(m => m as IWeir != null && (m as IWeir).Name == name) 
                   ?? CreateGeneralStructureFromNetCdf(name);
        }

        private IList<IFeature> InitializeCrossSectionFeatures(IEnumerable<Feature2D> modelObsCrossSections)
        {
            var results = new List<IFeature>();

            NetCdfVariable crossSectionNameVariable = netCdfFile.GetVariableByName("cross_section_name");
            if (crossSectionNameVariable == null)
            {
                return results;
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

            return results;
        }

        private IList<IFeature> InitializeStationFeatures(IEnumerable<Feature2D> modelObsPoints)
        {
            var results = new List<IFeature>();

            NetCdfVariable stationIdVariable = netCdfFile.GetVariableByName("station_id");
            if (stationIdVariable == null)
            {
                return results;
            }

            string[] ids = netCdfFile.Read(stationIdVariable)
                                     .Cast<char[]>().Select(CharArrayToString).ToArray();

            // TODO: xs and yx are now time dependent, evetually we will need to re-think this... for now, just take the 1st dimension

            double[] xs = netCdfFile.Read(netCdfFile.GetVariableByName("station_x_coordinate"))
                                    .Cast<double>().ToArray();
            double[] ys = netCdfFile.Read(netCdfFile.GetVariableByName("station_y_coordinate"))
                                    .Cast<double>().ToArray();

            for (var i = 0; i < ids.Length; i++)
            {
                // first try to find the right one in the model features, otherwise create our own feature
                results.Add(modelObsPoints.FirstOrDefault(m => m.Name == ids[i]) ??
                            CreateStationFromNetCdf(i, ids, xs, ys));
            }

            return results;
        }

        private static Feature2D CreateCrossSectionFromNetCdf(int i, string[] names, Array xs, Array ys)
        {
            var coordinates = new List<Coordinate>();
            for (var j = 0; j < xs.GetLength(1); j++)
            {
                var x = (double) xs.GetValue(i, j);
                var y = (double) ys.GetValue(i, j);

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
                Geometry = new Point((double) xs.GetValue(i), (double) ys.GetValue(i))
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