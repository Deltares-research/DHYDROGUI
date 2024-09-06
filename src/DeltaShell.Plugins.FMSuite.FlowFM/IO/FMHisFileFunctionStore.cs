using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.NetCdf;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// Static class with user-friendly names defined.
    /// Names are used for visualization in the GUI.
    /// </summary>
    internal static class UserFriendlyNames
    {
        public const string Default = "Observation points";
        public const string Pumps = "Pumps";
        public const string ObservationCrossSections = "Observation cross sections";
        public const string WeirsAndGeneralStructures = "Weirs + general structures";
        public const string Orifices = "Orifices";
        public const string Culverts = "Culverts";
        public const string CompoundStructures = "Compound structures";
        public const string Bridges = "Bridges";
        public const string SourcesAndSinks = "Sources and sinks";
        public const string Laterals = "Laterals";
    }
    
    /// <summary>
    /// Reads an Unstruc HIS file and acts as the backing store. The his files contains timeseries on stations and cross sections.
    /// These correspond to observation points and obs. cross sections in the model. These features can either be generated from 
    /// the netcdf file (in case you import the HIS file standalone), or be inserted from the model, to ensure the instances are
    /// exactly the same. This is required to use functionality like 'Query timeseries' etc.
    /// </summary>
    public class FMHisFileFunctionStore : FMNetCdfFileFunctionStore
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FMHisFileFunctionStore));

        public IHydroNetwork Network { get; } = null;
        public HydroArea Area { get; } = null;

        private IDictionary<string, IEnumerable<IFeature>> FeaturesByCoverage = new Dictionary<string, IEnumerable<IFeature>>();
        
        protected const string StandardNameAttribute = "standard_name";
        protected const string LongNameAttribute = "long_name";
        protected const string UnitAttribute = "units";
        protected const string CoordinatesAttribute = "coordinates";
        
        private const string CF_ROLE = "cf_role";
        private const string TIMESERIES_ID = "timeseries_id";
        private const string PROJECTION_X_COORDINATE = "projection_x_coordinate";
        private const string PROJECTION_Y_COORDINATE = "projection_y_coordinate";
        

        private static ReadOnlyDictionary<string, string> UserFriendlyCategoryNames = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            { "cross_section", UserFriendlyNames.ObservationCrossSections },
            { "weirgens", UserFriendlyNames.WeirsAndGeneralStructures },
            { "weirgen", UserFriendlyNames.WeirsAndGeneralStructures },
            { "orifice", UserFriendlyNames.Orifices },
            { "culvert", UserFriendlyNames.Culverts },
            { "pumps", UserFriendlyNames.Pumps },
            { "pump", UserFriendlyNames.Pumps },
            { "compoundStructures", UserFriendlyNames.CompoundStructures },
            { "cmpstru", UserFriendlyNames.CompoundStructures },
            { "bridge", UserFriendlyNames.Bridges },
            { "source_sink", UserFriendlyNames.SourcesAndSinks },
            { "lateral", UserFriendlyNames.Laterals },
        });

        // nhib
        protected FMHisFileFunctionStore()
        {
        }

        public FMHisFileFunctionStore(IHydroNetwork network, HydroArea area)
        {
            Network = network;
            Area = area;
        }
        public FMHisFileFunctionStore(string hisFilePath) // modelwide reader...
        {
            Path = hisFilePath;
        }

        public ICoordinateSystem CoordinateSystem { get; set; }

        protected override void UpdateFunctionsAfterPathSet()
        {
            if (CoordinateSystem == null)
            {
                CoordinateSystem = ReadCoordinateSystem();
            }

            Close();
            base.UpdateFunctionsAfterPathSet();
        }

        private ICoordinateSystem ReadCoordinateSystem()
        {
            NetCdfFile file = null;

            try
            {
                file = NetCdfFile.OpenExisting(Path);
                return file.GetCoordinateSystem(new OgrCoordinateSystemFactory());
            }
            finally
            {
                file?.Close();
            }
        }
        /// <summary>
        /// Dynamically groups functions constructed in <see cref="ConstructFunctions"/> based on second dimension name
        /// of the HIS functions read from the file. These functions are based on time and 'structure' category for example:
        /// <example>
        /// double weirgen_discharge(time, weirgens) ;
        /// </example>
        /// double means the value type of the discharge at a certain point in time at a certain type of structure.
        /// so in this case it will be categorized with all other weirgens output coverages but using the user friendly name retrieved from <see cref="UserFriendlyCategoryNames"/>
        /// <example>
        /// double orifice_discharge(time, orifice) ;
        /// </example>
        /// double means the value type of the discharge at a certain point in time at a certain type of structure.
        /// so in this case it will be categorized with all other orifices output coverages but using the user friendly name retrieved from <see cref="UserFriendlyCategoryNames"/>
        /// 
        /// This is placed in the feature variable (Function.Arguments[1]) Attributes dictionary (dictionary key <see cref="ReadOnlyNetCdfFunctionStoreBase.NcNameAttribute"/>)
        /// The converted user friendly variable is also placed in the feature variable Attributes dictionary with key <see cref="UserFriendlyCategoryNameAttribute"/>
        /// which converts the dynamically retrieved variable in to a user readable variable <see cref="UserFriendlyCategoryNames"/>
        /// Similar action is done at <seealso cref="FMMapFileFunctionStore.GetFunctionGrouping"/> and in the kernels.
        /// </summary>
        /// <returns>Grouped functions by a customized user friendly category name <see cref="UserFriendlyCategoryNames"/>.</returns>
        public IEnumerable<IGrouping<string, IFunction>> GetFunctionGrouping()
        {
            return Functions.Where(f => f.Arguments.Count > 1).GroupBy(f => f.Arguments[1].Attributes[UserFriendlyCategoryNameAttribute]);
        }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            if (!ValidateTimes())
            {
                yield break;
            }
            
            FeaturesByCoverage.Clear();

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
                    var secondDimensionName = netCdfFile.GetDimensionName(dimensions[1]);
                    var featureVariable = new Variable<IFeature> { IsEditable = false, Name = secondDimensionName };

                    featureVariable.Attributes[NcNameAttribute] = secondDimensionName;
                    if (!UserFriendlyCategoryNames.TryGetValue(secondDimensionName, out string userFriendlyCategoryName))
                    {
                        userFriendlyCategoryName = UserFriendlyNames.Default;
                    }

                    featureVariable.Attributes[UserFriendlyCategoryNameAttribute] = userFriendlyCategoryName;
                    featureVariable.Attributes[NcUseVariableSizeAttribute] = "false";
                    var coverage = new FeatureCoverage(coverageLongName)
                    {
                        IsEditable = false,
                        IsTimeDependent = true,
                        CoordinateSystem = CoordinateSystem
                    };

                    coverage.Arguments.Add(featureVariable);

                    string[] ids = null;
                    double[] xCoordinates = null;
                    double[] yCoordinates = null;

                    var idCollectionViaCoordinateAttribute = netCdfFile.GetAttributeValue(netcdfVariable, CoordinatesAttribute)?.SplitOnEmptySpace();
                    if (idCollectionViaCoordinateAttribute == null) continue;

                    FindInputFeatureIdsOrOutputCoordinatesToPlaceTimeseriesOn(idCollectionViaCoordinateAttribute, ref ids, ref xCoordinates, ref yCoordinates);
                    InsertFeaturesInCoverage(coverage, ids, xCoordinates, yCoordinates);
                    function = coverage;

                    functionTimeVariable = coverage.Time;
                    functionTimeVariable.InterpolationType = InterpolationType.Linear;

                }
                else
                {
                    var timeSeries = new TimeSeries { Name = coverageLongName, IsEditable = false };

                    function = timeSeries;
                    functionTimeVariable = timeSeries.Time;
                }

                var unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);
                var netCdfDataType = netCdfFile.GetVariableDataType(netcdfVariable);

                switch (netCdfDataType)
                {
                    case NetCdfDataType.NcInteger:
                    {
                        var outputVariable = GenerateOutputVariable<int>(variableName, unitSymbol);
                        outputVariable.Attributes[NcNameAttribute] = variableName;
                        outputVariable.Attributes[NcUseVariableSizeAttribute] = "true";
                        function.Components.Add(outputVariable);
                        break;
                    }
                    case NetCdfDataType.NcDoublePrecision:
                    {
                        var outputVariable = GenerateOutputVariable<double>(variableName, unitSymbol);

                        outputVariable.Attributes[NcNameAttribute] = variableName;
                        outputVariable.Attributes[NcUseVariableSizeAttribute] = "true";

                        function.Components.Add(outputVariable);

                        break;
                    }
                    default:
                        break;
                }
                functionTimeVariable.Name = "Time";
                functionTimeVariable.Attributes[NcNameAttribute] = TimeVariableNames[0];
                functionTimeVariable.Attributes[NcUseVariableSizeAttribute] = "true";
                functionTimeVariable.Attributes[NcRefDateAttribute] = timeVariable.ReferenceDate;
                functionTimeVariable.IsEditable = false;

                function.Store = this;

                yield return function;
            }
        }

        private void FindInputFeatureIdsOrOutputCoordinatesToPlaceTimeseriesOn(string[] idCollectionViaCoordinateAttribute, ref string[] ids, ref double[] xCoordinates, ref double[] yCoordinates)
        {
            foreach (var idCollection in idCollectionViaCoordinateAttribute)
            {
                var idCollectionNetCdfVariable = netCdfFile.GetVariableByName(idCollection);
                if (idCollectionNetCdfVariable == null) continue;

                var idCollectionNetCdfVariableHasCfRoleAttributeValue =
                    netCdfFile.GetAttributeValue(idCollectionNetCdfVariable, CF_ROLE);
                if (idCollectionNetCdfVariableHasCfRoleAttributeValue != null
                    && idCollectionNetCdfVariableHasCfRoleAttributeValue.Equals(TIMESERIES_ID,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    ids = netCdfFile.Read(idCollectionNetCdfVariable)
                        ?.Cast<char[]>()
                        .Select(FMHisFileFunctionStoreHelper.CharArrayToString)
                        .ToArray();
                }

                var idCollectionNetCdfVariableHasStandardNameProjectionXCoordinateAttributeValue =
                    netCdfFile.GetAttributeValue(idCollectionNetCdfVariable, StandardNameAttribute);
                if (idCollectionNetCdfVariableHasStandardNameProjectionXCoordinateAttributeValue != null
                    && idCollectionNetCdfVariableHasStandardNameProjectionXCoordinateAttributeValue.Equals(
                        PROJECTION_X_COORDINATE, StringComparison.InvariantCultureIgnoreCase))
                {
                    xCoordinates = netCdfFile.Read(idCollectionNetCdfVariable).Cast<double>().ToArray();
                }

                var idCollectionNetCdfVariableHasStandardNameProjectionYCoordinateAttributeValue =
                    netCdfFile.GetAttributeValue(idCollectionNetCdfVariable, StandardNameAttribute);
                if (idCollectionNetCdfVariableHasStandardNameProjectionYCoordinateAttributeValue != null
                    && idCollectionNetCdfVariableHasStandardNameProjectionYCoordinateAttributeValue.Equals(
                        PROJECTION_Y_COORDINATE, StringComparison.InvariantCultureIgnoreCase))
                {
                    yCoordinates = netCdfFile.Read(idCollectionNetCdfVariable).Cast<double>().ToArray();
                }
            }
        }

        private Variable<T> GenerateOutputVariable<T>(string variableName, string unitSymbol)
        {
            return new Variable<T>
            {
                Name = variableName,
                IsEditable = false,
                Unit = new Unit(unitSymbol, unitSymbol),
                NoDataValue = MissingValue,
                InterpolationType = InterpolationType.Linear
            };
        }

        private void InsertFeaturesInCoverage(IFeatureCoverage coverage, string[] ids, double[] xCoordinates, double[] yCoordinates)
        {
            var featureName = coverage?.FeatureVariable.Attributes[NcNameAttribute];
            if (featureName == null) return;
            var maxNumberOfCoordinatesTheGeometryOfTheObjectConsistOf = 1;

            if (ids == null && xCoordinates == null && yCoordinates == null)
            {
                FMHisFileFunctionStoreHelper.CheckAndResolveInputBecauseKernelIsNotGeneratingOutputCorrectly(netCdfFile, ref ids, ref maxNumberOfCoordinatesTheGeometryOfTheObjectConsistOf, ref xCoordinates, ref yCoordinates, featureName);
            }

            if (!FeaturesByCoverage.ContainsKey(featureName))
            {
                IFeature[] features = null;
                if (Network != null && Area != null && ids != null)
                {
                    //baseTypeChecking?
                    var allFeatures = Network.BranchFeatures.OfType<INameable>()
                                             .Concat(Area.AllHydroObjects)
                                             .Concat(Area.ObservationPoints)
                                             .Concat(Area.ObservationCrossSections)
                                             .Except(Network.Branches)
                                             .Except(Network.Retentions)
                                             .GroupBy(n => n.Name, StringComparer.InvariantCultureIgnoreCase)
                                             .ToDictionary(g => g.Key, StringComparer.InvariantCultureIgnoreCase);

                    features = new IFeature[ids.Length];
                    for (int i = 0; i < ids.Length; i++)
                    {
                        if (allFeatures.TryGetValue(ids[i], out var grouping))
                        {
                            features[i] = grouping.OfType<IFeature>().FirstOrDefault();
                        }
                    }
                }
                else if ((ids != null || xCoordinates != null && yCoordinates != null) && FMHisFileFunctionStoreHelper.OutputStructuresGenerators.ContainsKey(featureName))
                {
                    features = FMHisFileFunctionStoreHelper.OutputStructuresGenerators[featureName](ids, maxNumberOfCoordinatesTheGeometryOfTheObjectConsistOf, xCoordinates, yCoordinates).ToArray();
                }

                if (features != null && features.Length != 0)
                {
                    if (features.All(f => f != null))
                    {
                        FeaturesByCoverage[featureName] = features;
                    }
                    else if (ids != null)
                    {
                        var missingIds = ids
                                  .Zip(features, (id, feature) => new {id, feature})
                                  .Where(t => t.feature == null)
                                  .Select(t => t.id);
                        log.Error($"Could not find the referenced feature(s) \"{string.Join(",", missingIds)}\" for {coverage.Name}");
                    }
                }
            }

            coverage.Features = new EventedList<IFeature>(FeaturesByCoverage.ContainsKey(featureName) ? FeaturesByCoverage[featureName] : new IFeature[0]);
        }

        private readonly IDictionary<string, IMultiDimensionalArray<IFeature>> cachedFeatures = new Dictionary<string, IMultiDimensionalArray<IFeature>>();
        private bool HasValidFile
        {
            get { return !string.IsNullOrEmpty(Path) && File.Exists(Path); }
        }
        private static IMultiDimensionalArray CreateEmptyArrayForType(Type type)
        {
            var listType = typeof(List<>).MakeGenericType(type);
            var mda = typeof(MultiDimensionalArray<>).MakeGenericType(type);
            return (IMultiDimensionalArray)Activator.CreateInstance(mda, Activator.CreateInstance(listType));
        }
        protected override int GetVariableValuesCount(IVariable variable, IVariableFilter[] filters)
        {
            if (!HasValidFile)
            {
                return 0;
            }
            return base.GetVariableValuesCount(variable, filters);
        }
        public override IMultiDimensionalArray<T> GetVariableValues<T>(IVariable variable, params IVariableFilter[] filters)
        {
            if (!HasValidFile)
            {
                return (IMultiDimensionalArray<T>)CreateEmptyArrayForType(variable.ValueType);
            }
            return base.GetVariableValues<T>(variable, filters);
        }

        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(IVariable function, IVariableFilter[] filters)
        {
            var functionName = function.Attributes[NcNameAttribute];
            if (function.Attributes[NcUseVariableSizeAttribute] == "false" 
                && !string.IsNullOrEmpty(functionName))
            {
                if (FeaturesByCoverage.ContainsKey(functionName))
                {
                    if (!cachedFeatures.ContainsKey(functionName))
                    {
                        cachedFeatures[functionName] = new MultiDimensionalArray<IFeature>(
                            FeaturesByCoverage[functionName].ToArray(),
                            new[] {GetSize(function)});
                    }

                    return (MultiDimensionalArray<T>) cachedFeatures[functionName];
                }
                return new MultiDimensionalArray<T>(new List<T>(), new[] { 0, 0 });
            }
            return base.GetVariableValuesCore<T>(function, filters);
        }
    }
}