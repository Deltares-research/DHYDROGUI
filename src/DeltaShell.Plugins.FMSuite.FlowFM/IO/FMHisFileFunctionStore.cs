using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.NetCdf;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Features;

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
                                      IEnumerable<Feature2D> modelObsCrossSections = null)
            : base(hisPath) //loads the actual functions
        {
            CoordinateSystem = coordinateSystem;

            using (ReconnectToMapFile())
            {
                stationFeatures = InitializeStationFeatures(modelObsPoints ?? new Feature2D[0]);
                crossSectionFeatures = InitializeCrossSectionFeatures(modelObsCrossSections ?? new Feature2D[0]);
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
        }

        private IMultiDimensionalArray<IFeature> cachedStationsArray;
        private IMultiDimensionalArray<IFeature> cachedCrossSectionsArray;

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
                    default:
                        throw new ArgumentException(string.Format("Unexpected dimension name: {0}", dimensionName));
                }
            }
            return base.GetVariableValuesCore<T>(function, filters);
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

            var xs = netCdfFile.Read(netCdfFile.GetVariableByName("station_x_coordinate"));
            var ys = netCdfFile.Read(netCdfFile.GetVariableByName("station_y_coordinate"));
            
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

        private static string CharArrayToString(char[] chars)
        {
            return new string(chars).TrimEnd(new[] {'\0', ' '});
        }
    }
}