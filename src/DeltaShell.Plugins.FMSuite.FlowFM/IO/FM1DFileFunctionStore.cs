using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.NetCdf;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class FM1DFileFunctionStore : FMNetCdfFileFunctionStore
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FM1DFileFunctionStore));
        public static readonly string LocationAttributeName = "Location";

        private IHydroNetwork outputNetwork = new HydroNetwork();
        private IHydroNetwork inputNetwork;
        private readonly IDiscretization outputDiscretization;

        private readonly IDictionary<string, IList<INetworkLocation>> locationsByNetworkDataType = new Dictionary<string, IList<INetworkLocation>>();
        private readonly Dictionary<IVariable, IMultiDimensionalArray> argumentVariableCache = new Dictionary<IVariable, IMultiDimensionalArray>();
        private readonly Dictionary<IVariable, IMultiDimensionalArray> networkLocationsForThisFunctionCache = new Dictionary<IVariable, IMultiDimensionalArray>();
        
        private OutputFile1DMetaData metaData;
        private int sobekStartIndex = 1;// minus one because fortran is 1 based...

        private const string StandardNameAttribute = "standard_name";
        private const string LongNameAttribute = "long_name";
        private const string UnitAttribute = "units";
        private const string FillValueAttribute = "_FillValue";

        public FM1DFileFunctionStore(IHydroNetwork network)
        {
            OutputFileReader = new FmMapFile1DOutputFileReader();
            outputDiscretization = new Discretization();
            outputDiscretization.Locations.IsAutoSorted = false;
            sobekStartIndex = 0;
            inputNetwork = network;
            DisableCaching = true;
        }
        public IHydroNetwork OutputNetwork
        {
            get { return outputNetwork; }
        }

        public IDiscretization OutputDiscretization
        {
            get { return outputDiscretization; }
        }
        
        private void UpdateNetworkAndDiscretisationAfterPathSet()
        {
            var netFilePath = Path;
            if (!File.Exists(netFilePath)) return;

            int numberOfNetworks = UGridFileHelper.GetNumberOfNetworks(netFilePath);
            if (numberOfNetworks != 1) return;

            int numberOfNetworkDiscretisations = UGridFileHelper.GetNumberOfNetworkDiscretizations(netFilePath);
            if (numberOfNetworkDiscretisations != 1) return;

            using (ReconnectToMapFile())
            {
                if (!UGridFileHelper.IsUGridFile(netCdfFile.Path)) return;

                var branchData = NetworkPropertiesHelper.ReadPropertiesPerBranchFromFile(netFilePath);
                var compartmentData = NetworkPropertiesHelper.ReadPropertiesPerNodeFromFile(netFilePath);
                outputNetwork.Nodes.Clear();
                outputNetwork.Branches.Clear();
                outputDiscretization.Clear();

                UGridFileHelper.ReadNetworkAndDiscretisation(netFilePath, outputDiscretization, outputNetwork, compartmentData, branchData);

                foreach (var hydroObject in outputNetwork.AllHydroObjects)
                {
                    hydroObject.Name = hydroObject.Name + "_output";
                }
                foreach (var outputNetworkLocation in outputDiscretization.Locations.AllValues)
                {
                    outputNetworkLocation.Name = outputNetworkLocation.Name + "_output";
                }

            }
        }
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
            if (variable.IsIndependent && typeof(T) == typeof(INetworkLocation))
            {
                var featureFilter = filters.FirstOrDefault(f => f.Variable.ValueType == typeof(INetworkLocation));
                if (variable.Attributes != null && variable.Attributes.ContainsKey(NcNameAttribute))
                {
                    var location = variable.Attributes[NcNameAttribute];
                    var networkLocations = locationsByNetworkDataType[location];
                    if (filters.Length == 0 || featureFilter == null)
                    {
                        if (!networkLocationsForThisFunctionCache.TryGetValue(variable, out var networkLocationsForThisFunction))
                        {
                            networkLocationsForThisFunction = new MultiDimensionalArray<T>((IList<T>)networkLocations);
                            networkLocationsForThisFunctionCache[variable] = networkLocationsForThisFunction;
                        }
                        return (MultiDimensionalArray<T>)networkLocationsForThisFunction;
                    }

                    if (featureFilter is VariableIndexFilter indexFilter)
                    {
                        if (!networkLocationsForThisFunctionCache.TryGetValue(variable, out var networkLocationsForThisFunction))
                        {
                            networkLocationsForThisFunction = new MultiDimensionalArray<T>((IList<T>)networkLocations);
                            networkLocationsForThisFunctionCache[variable] = networkLocationsForThisFunction;
                        }
                        // ik weet niet helemaal zeker of dit nou moet... maar hier zit volgens mij de conversie naar de output
                        var indexesOfLocationsInOutput = indexFilter.Indices.Select(i => locationsByNetworkDataType[location].IndexOf(networkLocations[i])).ToArray();
                        return new MultiDimensionalArray<T>((IList<T>)((MultiDimensionalArray<T>)networkLocationsForThisFunction).Select(1, indexesOfLocationsInOutput), new[] { MetaData?.Times.Count ?? 1, indexesOfLocationsInOutput.Length } );
                    }
                }
                return new MultiDimensionalArray<T>();
            }

            if (variable.ValueType == typeof(DateTime) && variable.Name.Equals("Time", StringComparison.InvariantCultureIgnoreCase) && filters!= null && filters.Length ==0) //waarom double, gewoon omdat het kan (alle values in de map dile zijn direct doubles in de output, nooit int of ander type
            {
                if (!argumentVariableCache.TryGetValue(variable, out var timeArgument))
                {
                    timeArgument = new MultiDimensionalArray<T>((IList<T>) MetaData.Times);
                    argumentVariableCache[variable] = timeArgument;
                }
                return (MultiDimensionalArray<T>) timeArgument;
            }

            if (variable.ValueType == typeof(double) && !variable.IsIndependent)//waarom double, gewoon omdat het kan (alle values in de map dile zijn direct doubles in de output, nooit int of ander type)
            {
                var coverage = GetCoverage(variable);
                if (coverage == null) return new MultiDimensionalArray<T>(new List<T>(), new[] {0, 0});
                var coverageComponent = coverage.Components[0];
                var ncVariableName = coverageComponent.Attributes.ContainsKey(NcNameAttribute) 
                    ? coverageComponent.Attributes[NcNameAttribute] 
                    : null;

                if (ncVariableName == null)
                {
                    return new MultiDimensionalArray<T>(new List<T>(), new[] { 0, 0 });
                }

                if (filters == null  || filters.Length == 0)
                {
                    return GetValuesForTimeSeriesAtAllLocations<T>(variable, ncVariableName); ;
                }

                var dateTimeFilter = filters.OfType<VariableValueFilter<DateTime>>().FirstOrDefault(f => f.Variable == coverage.Time);

                var featureVariable = coverage.Arguments.FirstOrDefault(a => a != coverage.Time && a.ValueType.Implements(typeof(IBranchFeature)));
                var branchFeatureFilter = filters.OfType<IVariableValueFilter>().FirstOrDefault(f => f.Variable == featureVariable);
                var branchRangeFilter = filters.OfType<VariableIndexRangesFilter>().FirstOrDefault(f => f.Variable == featureVariable);

                var hasBranchRangeFilter = branchRangeFilter != null && branchRangeFilter.IndexRanges.Count == 1;
                var hasBranchFilter = branchFeatureFilter != null && branchFeatureFilter.Values.Count == 1;
                var hasTimeFilter = dateTimeFilter != null && dateTimeFilter.Values.Count == 1;

                int[] shape = null;
                IList<T> timeSeriesData = null;
                try
                {
                    if (hasTimeFilter)
                    {
                        var timeStepIndex = MetaData.Times.IndexOf(dateTimeFilter.Values[0]);
                        if (hasBranchFilter)
                        {
                            
                            timeSeriesData = GetValueForTimeStepAtSingleLocation<T>(variable, ncVariableName, branchFeatureFilter, timeStepIndex, out shape);
                            
                        }
                        else if (hasBranchRangeFilter)
                        {
                            timeSeriesData = GetValuesForTimeStepAtRangeOfLocations<T>(variable, ncVariableName, branchRangeFilter, timeStepIndex, out shape);
                        }
                        else
                        {
                            timeSeriesData = GetValuesForTimeStepAtAllLocations<T>(variable, ncVariableName, timeStepIndex, out shape);
                        }
                    }
                    else
                    {
                        if (hasBranchFilter)
                        {
                            timeSeriesData = GetValuesForTimeSeriesAtSingleLocation<T>(variable, ncVariableName, branchFeatureFilter, out shape);
                        }
                    }
                }
                catch (Exception e)
                {
                    return new MultiDimensionalArray<T>(new List<T>(), new[] { 0, 0 });
                }


                if (shape == null || timeSeriesData == null)
                {
                    throw new NotImplementedException();
                }
                
                UpdateMinMaxCache(timeSeriesData.Cast<double>(), variable);
                return new MultiDimensionalArray<T>(timeSeriesData, shape);
            }
            return base.GetVariableValues<T>(variable, filters);
        }
        #region private GetValue helper methods

        private IMultiDimensionalArray<T> GetValuesForTimeSeriesAtAllLocations<T>(IVariable function, string ncVariableName)
        {
            if (argumentVariableCache.TryGetValue(function, out var timeSeriesAtAllLocations))
            {
                return (IMultiDimensionalArray<T>)timeSeriesAtAllLocations;
            }
            
            Func<IMultiDimensionalArray<T>> realGetFunction = () =>
                    {
                        var variableData = GetAllVariableData<T>(ncVariableName);
                        var variableDataShape = variableData.GetShape();
                        UpdateMinMaxCache(variableData.Cast<double>(), function);
                        return new MultiDimensionalArray<T>(variableData, variableDataShape);
                    };
            timeSeriesAtAllLocations = new LazyMultiDimensionalArray<T>(realGetFunction, () => (MetaData?.Times.Count ?? 1) * (MetaData?.NumLocationsForFunctionId(ncVariableName) ?? 1));
            argumentVariableCache[function] = timeSeriesAtAllLocations;
            return (IMultiDimensionalArray<T>) timeSeriesAtAllLocations;
        }

        private T[,] GetAllVariableData<T>(string variableName)
        {
            using (ReconnectToMapFile())
            {
                var variable = netCdfFile.GetVariableByName(variableName);
                if (variable == null) return new T[0, 0];
                return (T[,]) netCdfFile.Read(variable);
            }
        }

        private IList<T> GetValuesForTimeSeriesAtSingleLocation<T>(IVariable function, string ncVariableName, IVariableValueFilter branchFeatureFilter, out int[] shape)
        {
            var locationIndex = GetLocationIndex((IBranchFeature)branchFeatureFilter.Values[0], ncVariableName);

            var origin = new[] { 0, locationIndex };
            shape = new[] { MetaData.Times.Count, 1 };
            if (argumentVariableCache.TryGetValue(function, out var timeSeriesAtAllLocations))
            {
                return ((IMultiDimensionalArray<T>)timeSeriesAtAllLocations).Select(1, new [] {locationIndex} );
            }
        
            return GetSelectionOfVariableData<T>(ncVariableName, origin, ref shape);
        }

        private IList<T> GetValuesForTimeStepAtAllLocations<T>(IVariable function, string ncVariableName, int timeStepIndex, out int[] shape)
        {
            var origin = new[] { timeStepIndex, 0 };
            var timeDependentVariableMetaDataBaseKeyForThisLocation = MetaData.Locations.Keys.FirstOrDefault(tdv => tdv.Name.Equals(ncVariableName));
            if (timeDependentVariableMetaDataBaseKeyForThisLocation == null)
                throw new ArgumentNullException(nameof(timeDependentVariableMetaDataBaseKeyForThisLocation));
            
            shape = new[] { 1, MetaData.Locations[timeDependentVariableMetaDataBaseKeyForThisLocation].Count };
            
            if (argumentVariableCache.TryGetValue(function, out var timeSeriesAtAllLocations))
            {
                return ((IMultiDimensionalArray<T>) timeSeriesAtAllLocations).Select(0, new[] {timeStepIndex});

            }

            return GetSelectionOfVariableData<T>(ncVariableName, origin, ref shape);
        }

        private IList<T> GetValuesForTimeStepAtRangeOfLocations<T>(IVariable function, string ncVariableName, VariableIndexRangesFilter branchRangeFilter, int timeStepIndex, out int[] shape)
        {
            var endIndex = branchRangeFilter.IndexRanges[0].Second;
            var beginIndex = branchRangeFilter.IndexRanges[0].First;

            var origin = new[] { timeStepIndex, beginIndex };
            shape = new[] { 1, endIndex - beginIndex + 1 };
            if (argumentVariableCache.TryGetValue(function, out var timeSeriesAtAllLocations))
            {
                return ((IMultiDimensionalArray<T>)timeSeriesAtAllLocations).Select(new[] { timeStepIndex, beginIndex}, new[] { timeStepIndex, endIndex});

            }
            return GetSelectionOfVariableData<T>(ncVariableName, origin, ref shape);
        }

        private IList<T> GetValueForTimeStepAtSingleLocation<T>(IVariable function, string ncVariableName,
            IVariableValueFilter branchFeatureFilter, int timeStepIndex, out int[] shape)
        {
            var locationIndex = GetLocationIndex((IBranchFeature)branchFeatureFilter.Values[0], ncVariableName);
            shape = new[] { 1, 1 };
            if (argumentVariableCache.TryGetValue(function, out var timeSeriesAtAllLocations))
            {
                return ((IMultiDimensionalArray<T>)timeSeriesAtAllLocations).Select(new[] {timeStepIndex, locationIndex}, new[] {timeStepIndex, locationIndex});

            }
            var origin = new[] { timeStepIndex, locationIndex };
            
            return GetSelectionOfVariableData<T>(ncVariableName, origin, ref shape);
        }

        private IList<T> GetSelectionOfVariableData<T>(string ncVariableName, int[] origin, ref int[] shape)
        {
            try
            {
                using (ReconnectToMapFile())
                {
                    var fileVariable = netCdfFile.GetVariableByName(ncVariableName);

                    var locationData = netCdfFile.Read(fileVariable, origin, shape);
                    return (IList<T>)locationData.OfType<object>().Select(Convert.ToDouble).ToList();
                }
            }
            catch (Exception ex)
            {
                shape = new[] { 0, 0 };
                return new List<T>();
            }
        }

        #endregion

        private ICoverage GetCoverage(IVariable variable)
        {
            return Functions.OfType<ICoverage>().FirstOrDefault(f => f.Arguments.Concat(f.Components).Contains(variable));
        }
        private int GetLocationIndex(IBranchFeature branchFeature, string netCdfVariableName)
        {
            var timeDependentVariableMetaDataBaseKeyForThisLocation = MetaData.Locations.Keys.FirstOrDefault(tdv => tdv.Name.Equals(netCdfVariableName));
            if (timeDependentVariableMetaDataBaseKeyForThisLocation == null)
                throw new ArgumentNullException(nameof(timeDependentVariableMetaDataBaseKeyForThisLocation));
            LocationMetaData location;
            if (branchFeature is INetworkLocation)
            {
                var branchIndex = branchFeature.Network.Branches.IndexOf(branchFeature.Branch);
                location = MetaData.Locations[timeDependentVariableMetaDataBaseKeyForThisLocation]
                               .FirstOrDefault(l => l.BranchId == branchIndex 
                                        && Math.Abs(l.Chainage - branchFeature.Chainage) < double.Epsilon) 
                           ??  MetaData.Locations[timeDependentVariableMetaDataBaseKeyForThisLocation]
                               .FirstOrDefault(l => l.Id.Equals(branchFeature.Name, StringComparison.InvariantCultureIgnoreCase))
                           ??  MetaData.Locations[timeDependentVariableMetaDataBaseKeyForThisLocation]
                               .FirstOrDefault(l => Math.Abs(l.XCoordinate - branchFeature.Geometry.Coordinate.X) < double.Epsilon 
                                                 && Math.Abs(l.YCoordinate - branchFeature.Geometry.Coordinate.Y) < double.Epsilon);
            }
            else if (branchFeature is IStructure1D)
            {
                var structure = (IStructure1D)branchFeature;

                var compositePrefix = structure.ParentStructure?.Structures.Count > 1
                    ? structure.ParentStructure.Name + "_"
                    : string.Empty;

                var structureName = compositePrefix + branchFeature.Name;

                location = MetaData.Locations[timeDependentVariableMetaDataBaseKeyForThisLocation].FirstOrDefault(l => l.Id == structureName);
            }
            else
            {
                location = MetaData.Locations[timeDependentVariableMetaDataBaseKeyForThisLocation].FirstOrDefault(l => l.Id == branchFeature.Name);
            }

            if (location == null)
            {
                throw new ArgumentException($"Values for {branchFeature.Name} feature type {branchFeature.GetType().Name} could not be found.");
            }

            return MetaData.Locations[timeDependentVariableMetaDataBaseKeyForThisLocation].IndexOf(location);
        }
        protected override void UpdateFunctionsAfterPathSet()
        {
            ClearCaches();
            UpdateNetworkAndDiscretisationAfterPathSet();
            if (outputNetwork.Branches.Count != inputNetwork.Branches.Count)
            {
                log.ErrorFormat($"Could not load output from \"{Path}\", output network differs from input network");
                return;
            }

            if (CoordinateSystem == null) CoordinateSystem = UGridFileHelper.ReadCoordinateSystem(Path);
            // clear caches for argument variables and networkLocations
            Functions.Clear();
            if (File.Exists(Path))
            {
                using (ReconnectToMapFile())
                {
                    if (! UGridFileHelper.IsUGridFile(netCdfFile.Path)) return;
                    Functions.AddRange(ConstructFunctions(GetVariableInfos()));
                }
            }
        }
        private void ClearCaches()
        {
            locationsByNetworkDataType.Clear();
            networkLocationsForThisFunctionCache.Clear();
            argumentVariableCache.Clear();
            metaData?.Locations?.Clear();
            metaData?.TimeDependentVariables?.Clear();
            metaData?.Times?.Clear();
            metaData = null;
        }


        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            var netCdfVariables = netCdfFile.GetVariables().ToList();
            var mesh1DNameNetCdfVariable = netCdfVariables.FirstOrDefault(dv =>
            {
                var attributes = netCdfFile.GetAttributes(dv);
                object dimension;
                if (attributes.TryGetValue("topology_dimension", out dimension) && attributes.ContainsKey("coordinate_space"))
                {
                    if (int.Parse(dimension.ToString()) == 1)
                    {
                        return true;
                    }
                }
                return false;
            });
            var mesh1DName = mesh1DNameNetCdfVariable == null ? string.Empty : netCdfFile.GetVariableName(mesh1DNameNetCdfVariable);
            var isUgridConvention = true;

            return Get1DFunctions(dataVariables, isUgridConvention, mesh1DName);
        }
        private IEnumerable<INetworkCoverage> Get1DFunctions(IEnumerable<NetCdfVariableInfo> dataVariables, bool isUgridConvention, string mesh1DName)
        {
            var timeDepVarSelectionCriteria = isUgridConvention
                ? (Func<NetCdfVariableInfo, bool>) (v =>
                {
                    var attributes = netCdfFile.GetAttributes(v.NetCdfDataVariable);
                    if (!attributes.TryGetValue("mesh", out var meshName) ||
                        !attributes.TryGetValue("location", out var locationName))
                    {
                        return false;
                    }

                    var location = GetNetworkLocation(locationName.ToString());

                    return v.IsTimeDependent && 
                           v.NumDimensions > 1 && 
                           meshName.ToString() == mesh1DName &&
                           (location == NetworkDataLocation.Edge ||
                            location == NetworkDataLocation.Node );
                })
                : v => v.IsTimeDependent && v.NumDimensions > 1 && v.NumDimensions <= 2;

            var timeDepVariables = dataVariables.Where(timeDepVarSelectionCriteria).ToList();
            var functions = timeDepVariables.SelectMany(ProcessTimeDependent1DVariable).Where(c => c != null).ToList();

            return functions;
        }

        private IEnumerable<NetworkCoverage> ProcessTimeDependent1DVariable(NetCdfVariableInfo timeDependentVariable)
        {
            NetworkCoverage coverage = null;
            var netcdfVariable = timeDependentVariable.NetCdfDataVariable;

            var netCdfVariableName = netCdfFile.GetVariableName(netcdfVariable);
            
            var netCdfVariableType = netCdfFile.GetVariableDataType(netcdfVariable);
            if (netCdfVariableType != NetCdfDataType.NcDoublePrecision)
            {
                yield break;
            }

            var longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute) ??
                           netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttribute);

            var coverageLongName = (longName != null)
                ? string.Format("{0} ({1})", longName, netCdfVariableName)
                : netCdfVariableName;

            var location = netCdfFile.GetDimensionName(netCdfFile.GetDimensions(netcdfVariable).ToArray()[1]);
            var timeDependentVariableMetaDataBaseKeyForThisLocation = MetaData.Locations.Keys.FirstOrDefault(tdv => tdv.Name.Equals(netCdfVariableName) );
            if (!locationsByNetworkDataType.ContainsKey(location) &&
                timeDependentVariableMetaDataBaseKeyForThisLocation != null)
            {
                locationsByNetworkDataType[location] = MetaData
                    .Locations[timeDependentVariableMetaDataBaseKeyForThisLocation]
                    .Select(l => new NetworkLocation(inputNetwork.Branches[l.BranchId - sobekStartIndex], l.Chainage)
                        {Geometry = new Point(l.XCoordinate, l.YCoordinate), Name = l.Id, Attributes = new DictionaryFeatureAttributeCollection(){{LocationAttributeName, location}}}).Cast<INetworkLocation>().ToList();
            }
            var unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);
            var noDataValue = double.Parse(netCdfFile.GetAttributeValue(netcdfVariable, FillValueAttribute));

            coverage = CreateNetworkCoverage(coverageLongName, unitSymbol, netCdfVariableName, location, timeDependentVariable.ReferenceDate, noDataValue:noDataValue);

            yield return coverage;
        }

        private static NetworkDataLocation GetNetworkLocation(string locationName)
        {
            switch (locationName)
            {
                case "edge":
                    return NetworkDataLocation.Edge;
                case "node":
                    return NetworkDataLocation.Node;
                default:
                    return NetworkDataLocation.UnKnown;
            }
        }
        
        public ICoordinateSystem CoordinateSystem { get; set; }
        
        private NetworkCoverage CreateNetworkCoverage(string coverageLongName, string unitSymbol,
            string netCdfVariableName, string location, string refDate, int number = -1, double noDataValue = -999.0d)
        {
            var suffix = number < 0 ? string.Empty : string.Format(" ({0})", number);
            var coverageName = coverageLongName + suffix;
            var networkCoverage = new NetworkCoverage(coverageName, true, coverageName, unitSymbol) { Network = inputNetwork, CoordinateSystem = CoordinateSystem};
            networkCoverage.Store = this;

            networkCoverage.Components[0].NoDataValue = noDataValue;
            
            networkCoverage.Locations.FixedSize = 0;
            networkCoverage.Locations.InterpolationType = InterpolationType.Constant;
            networkCoverage.Locations.ExtrapolationType = ExtrapolationType.Constant;
            
            networkCoverage.IsEditable = false;

            var timeDimension = networkCoverage.Arguments[0];
            timeDimension.Name = "Time";
            timeDimension.Attributes[NcNameAttribute] = TimeVariableNames[0];
            timeDimension.Attributes[NcUseVariableSizeAttribute] = "true";
            timeDimension.Attributes[NcRefDateAttribute] = refDate;
            timeDimension.IsEditable = false;

            networkCoverage.Arguments[1].Attributes[NcUseVariableSizeAttribute] = "false";
            networkCoverage.Arguments[1].Attributes[NcNameAttribute] = location;
            
            var coverageComponent = networkCoverage.Components[0];
            coverageComponent.Name = netCdfVariableName;
            coverageComponent.Attributes[NcNameAttribute] = netCdfVariableName;
            coverageComponent.Attributes[NcUseVariableSizeAttribute] = "true";
            
            coverageComponent.NoDataValue = MissingValue;
            coverageComponent.IsEditable = false;
            coverageComponent.Unit = new Unit(unitSymbol, unitSymbol);

            networkCoverage.Attributes.Add("NetCdfVariableName", netCdfVariableName);
            networkCoverage.Attributes.Add(LocationAttributeName, GetNetworkLocation(location).ToString());

            networkCoverage.IsEditable = false;

            return networkCoverage;
        }

        private IEnumerable<NetCdfVariableInfo> GetVariableInfos()
        {
            using (ReconnectToMapFile())
            {
                foreach (NetCdfVariable variable in netCdfFile.GetVariables())
                {
                    List<NetCdfDimension> dimensions = netCdfFile.GetDimensions(variable).ToList();
                    string firstDimensionName = dimensions.Select(d => netCdfFile.GetDimensionName(d)).FirstOrDefault();

                    if (dimensions.Count == 0)
                        continue;

                    if (TimeVariableNames.Contains(netCdfFile.GetVariableName(variable)))
                        continue;

                    // maybe add some relationship checks here: now we're also returning some arguments & components seperately.
                    if (firstDimensionName != null && TimeDimensionNames.Contains(firstDimensionName))
                    {
                        // time dependent variable
                        string timeVariableName = GetTimeVariableName(firstDimensionName);
                        string refDate = ReadReferenceDateFromFile(timeVariableName);
                        yield return new NetCdfVariableInfo(variable, dimensions.Count, timeVariableName, refDate);
                    }
                    else
                    {
                        // non time dependent variable
                        yield return new NetCdfVariableInfo(variable, dimensions.Count);
                    }
                }
            }
        }
        private OutputFile1DMetaData MetaData
        {
            get
            {
                if (metaData != null) return metaData;

                if(!File.Exists(Path)) return new OutputFile1DMetaData();

                try
                {
                    metaData = OutputFileReader.ReadMetaData(Path);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error reading MetaData for file: {0}{1}{2}", Path, Environment.NewLine, ex.Message);
                    metaData = new OutputFile1DMetaData();
                }
                return metaData;
            }
        }
        private FmMapFile1DOutputFileReader OutputFileReader { get; set; }

        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(IVariable function,
            IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] == "false") // has no explicit variable (for example nFlowElem, which is only a dimension)
            {
                int size = GetSize(function);
                return new MultiDimensionalArray<T>(Enumerable.Range(0, size).Cast<T>().ToList(), new[]
                {
                    size
                });
            }
            try
            {
                return base.GetVariableValuesCore<T>(function, filters);
            }
            catch (Exception e) when (e.Message.Contains("NetCDF error code"))
            {
                log.Error(string.Format("While reading variable {0} from the file {1} an error was encountered: {2}", function.Name, System.IO.Path.GetFileName(Path), e.Message));
                int functionSize = GetSize(function);
                return new MultiDimensionalArray<T>(new List<T>(new T[functionSize]), functionSize);
            }
        }
    }
}