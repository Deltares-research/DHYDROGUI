using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Reads an Unstruc HIS file and acts as the backing store. The his files contains timeseries on stations and cross sections.
    /// These correspond to observation points and obs. cross sections in the model. These features can either be generated from 
    /// the netcdf file (in case you import the HIS file standalone), or be inserted from the model, to ensure the instances are
    /// exactly the same. This is required to use functionality like 'Query timeseries' etc.
    /// </summary>
    public class FMHisFileFunctionStore : FMNetCdfFileFunctionStore
    {
        private readonly IList<IFeature> stationFeatures;
        private readonly IList<IFeature> crossSectionFeatures;
        private readonly IList<IFeature> generalStructuresFeatures;
        private readonly IList<IFeature> leveeBreachFeatures;
        private const string leveeBreachesName = "dambreaks";
        private const string leveeBreachName = "dambreak";
        protected const string StandardNameAttribute = "standard_name";
        protected const string LongNameAttribute = "long_name";
        protected const string UnitAttribute = "units";

        private static readonly IList<string> DiscardedFeatures = new[] {"weirgens", "gategens", "pumps"};

        // nhib
        protected FMHisFileFunctionStore()
        {
        }

        public FMHisFileFunctionStore(string hisPath, ICoordinateSystem coordinateSystem=null,
                                      IEnumerable<Feature2D> modelObsPoints = null,
                                      IEnumerable<Feature2D> modelObsCrossSections = null,
                                      IEnumerable<Weir2D> modelGeneralStructures = null,
                                      IEnumerable<LeveeBreach> modelLeveeBreaches = null)
            : base(hisPath) //loads the actual functions
        {
            CoordinateSystem = coordinateSystem;

            using (ReconnectToMapFile())
            {
                stationFeatures = InitializeStationFeatures(modelObsPoints ?? new Feature2D[0]);
                crossSectionFeatures = InitializeCrossSectionFeatures(modelObsCrossSections ?? new Feature2D[0]);
                generalStructuresFeatures = InitializeGeneralStructuresFeatures(modelGeneralStructures ?? new Weir2D[0]);
                leveeBreachFeatures = InitializeLeveeBreachFeatures(modelLeveeBreaches ?? new LeveeBreach[0]);
            }

            // initialize 'Features' collection of each coverage
            foreach (var featureCoverage in Functions.OfType<IFeatureCoverage>())
            {
                InsertFeaturesInCoverage(featureCoverage);
            }
        }

        public ICoordinateSystem CoordinateSystem { get; set; }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            // add special velocity timeseries?
            foreach (var timeVariable in dataVariables.Where(v => v.IsTimeDependent))
            {
                var netcdfVariable = timeVariable.NetCdfDataVariable;

                var dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();

                var variableName = netCdfFile.GetVariableName(netcdfVariable);
                var longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute) ??
                               netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttribute);
                var coverageLongName = (longName != null)
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

                    var secondDimensionName = netCdfFile.GetDimensionName(dimensions[1]);

                    if(DiscardedFeatures.Contains(secondDimensionName)) continue;

                    var featureVariable = new Variable<IFeature> { IsEditable = false, Name = secondDimensionName };

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
                    var timeSeries = new TimeSeries { Name = coverageLongName, IsEditable = false };

                    function = timeSeries;
                    functionTimeVariable = timeSeries.Time;
                }

                var unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);
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
            var featureName = coverage.FeatureVariable.Attributes[NcNameAttribute];
            if (featureName == "stations" && stationFeatures != null)
            {
                coverage.Features = new EventedList<IFeature>(stationFeatures);
            }
            if (featureName == "cross_section" && crossSectionFeatures != null)
            {
                coverage.Features = new EventedList<IFeature>(crossSectionFeatures);
            }
            if (featureName == "general_structures" && generalStructuresFeatures != null)
            {
                coverage.Features = new EventedList<IFeature>(generalStructuresFeatures);
            }
            if (featureName == leveeBreachesName && leveeBreachFeatures != null)
            {
                coverage.Features = new EventedList<IFeature>(leveeBreachFeatures);
            }
        }

        private IMultiDimensionalArray<IFeature> cachedStationsArray;
        private IMultiDimensionalArray<IFeature> cachedCrossSectionsArray;
        private IMultiDimensionalArray<IFeature> cachedGeneralStructures;
        private IMultiDimensionalArray<IFeature> cachedLeveeBreach;

        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(IVariable function, DelftTools.Functions.Filters.IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] == "false") // has no explicit variable: stations / cross sections, composited from multiple vars
            {
                var dimensionName = function.Attributes[NcNameAttribute];
                switch (dimensionName)
                {
                    case "stations":
                        if (cachedStationsArray == null)
                        {
                            cachedStationsArray = new MultiDimensionalArray<IFeature>(stationFeatures,
                                                                                      new[] {GetSize(function)});
                        }
                        return (MultiDimensionalArray<T>) cachedStationsArray;
                    case "cross_section":
                        if (cachedCrossSectionsArray == null)
                        {
                            cachedCrossSectionsArray = new MultiDimensionalArray<IFeature>(crossSectionFeatures,
                                                                                           new[] {GetSize(function)});
                        }
                        return (MultiDimensionalArray<T>) cachedCrossSectionsArray;
                    case "general_structures":
                        if (cachedGeneralStructures == null)
                        {
                            cachedGeneralStructures = new MultiDimensionalArray<IFeature>(generalStructuresFeatures,
                                new[] { GetSize(function) });
                        }
                        return (MultiDimensionalArray<T>)cachedGeneralStructures;
                    case leveeBreachesName:
                        if (cachedLeveeBreach == null)
                        {
                            cachedLeveeBreach = new MultiDimensionalArray<IFeature>(leveeBreachFeatures,
                                new[] { GetSize(function) });
                        }
                        return (MultiDimensionalArray<T>)cachedLeveeBreach;
                    default:
                        throw new ArgumentException(string.Format("Unexpected dimension name: {0}", dimensionName));
                }
            }
            return base.GetVariableValuesCore<T>(function, filters);
        }

        private IList<IFeature> InitializeGeneralStructuresFeatures(IEnumerable<IFeature> modelGeneralStructures)
        {
            var results = new List<IFeature>();

            var generalStructureNameVariable = netCdfFile.GetVariableByName("general_structure_name");
            if (generalStructureNameVariable == null)
                return results;

            var names = netCdfFile.Read(generalStructureNameVariable).Cast<char[]>().Select(CharArrayToString).ToArray();
            for (int i = 0; i < names.Length; i++)
            {
                // first try to find the right one in the model features, otherwise we skip it for now. We are not ready to fill in all the data just yet.
                results.Add(modelGeneralStructures.FirstOrDefault(m => (m as Weir2D) != null && (m as Weir2D).Name == names[i]) ??
                            CreateGeneralStructureFromNetCdf(i, names));
            }
            return results;
        }

        private IList<IFeature> InitializeLeveeBreachFeatures(IEnumerable<IFeature> modelLeveeFeatures)
        {
            var results = new List<IFeature>();

            var leveeBreachNameVariable = netCdfFile.GetVariableByName(leveeBreachName + "_name");
            if (leveeBreachNameVariable == null)
                return results;

            var names = netCdfFile.Read(leveeBreachNameVariable).Cast<char[]>().Select(CharArrayToString).ToArray();

            var leveeFeatures = modelLeveeFeatures as IFeature[] ?? modelLeveeFeatures.ToArray();
            for (var i = 0; i < names.Length; i++)
            {
                results.Add(leveeFeatures.FirstOrDefault(m => 
                                (m as ILeveeBreach) != null && (m as INameable)?.Name == names[i]) ?? CreateLeveeBreachFromNetCdf(i, names));
            }
            return results;
        }

        private IList<IFeature> InitializeCrossSectionFeatures(IEnumerable<Feature2D> modelObsCrossSections)
        {
            var results = new List<IFeature>();

            var crossSectionNameVariable = netCdfFile.GetVariableByName("cross_section_name");
            if (crossSectionNameVariable == null)
                return results;

            var names = netCdfFile.Read(crossSectionNameVariable)
                                  .Cast<char[]>().Select(CharArrayToString).ToArray();
            var xs = netCdfFile.Read(netCdfFile.GetVariableByName("cross_section_x_coordinate"));
            var ys = netCdfFile.Read(netCdfFile.GetVariableByName("cross_section_y_coordinate"));

            for (int i = 0; i < xs.GetLength(0); i++)
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

            var stationIdVariable = netCdfFile.GetVariableByName("station_id");
            if (stationIdVariable == null) 
                return results;

            var ids = netCdfFile.Read(stationIdVariable)
                                .Cast<char[]>().Select(CharArrayToString).ToArray();

            // TODO: xs and yx are now time dependent, evetually we will need to re-think this... for now, just take the 1st dimension

            var xs = netCdfFile.Read(netCdfFile.GetVariableByName("station_x_coordinate"))
                                .Cast<double>().ToArray();
            var ys = netCdfFile.Read(netCdfFile.GetVariableByName("station_y_coordinate"))
                                .Cast<double>().ToArray();


            for (int i = 0; i < ids.Length; i++)
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
            for (int j = 0; j < xs.GetLength(1); j++)
            {
                var x = (double)xs.GetValue(i, j);
                var y = (double)ys.GetValue(i, j);

                if (x < NetCdfConstants.FillValues.NcFillFloat) // use default fill value here..
                    coordinates.Add(new Coordinate(x, y));
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

        private static Weir2D CreateGeneralStructureFromNetCdf(int i, string[] names)
        {
            return new Weir2D
            {
                Name = names[i],
                WeirFormula = new GeneralStructureWeirFormula()
            };
        }

        private static IFeature CreateLeveeBreachFromNetCdf(int i, string[] names)
        {
            return new LeveeBreach
            {
                Name = names[i]
            };
        }

        private static string CharArrayToString(char[] chars)
        {
            return new string(chars).TrimEnd(new[] {'\0', ' '});
        }
    }
}