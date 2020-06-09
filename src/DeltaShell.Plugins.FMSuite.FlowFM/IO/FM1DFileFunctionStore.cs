using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using ArrayExtensions = DelftTools.Utils.ArrayExtensions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class FM1DFileFunctionStore : IFunctionStore, IFileBased
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FM1DFileFunctionStore));
        public static readonly string LocationAttributeName = "Location";

        private readonly object readLock = new object();
        private NetCdfFile netCdfFile;
        private const string TimeDimensionName = "time";
        private string dateTimeFormat = "yyyy-MM-dd hh:mm:ss"; // default
        private IHydroNetwork outputNetwork = new HydroNetwork();
        private IHydroNetwork inputNetwork;
        private IDiscretization outputDiscretization = new Discretization();
        
        private IEventedList<IFunction> functions;
        private readonly IDictionary<string, double> minValues = new Dictionary<string, double>();
        private readonly IDictionary<string, double> maxValues = new Dictionary<string, double>();
        private readonly Dictionary<IVariable, IMultiDimensionalArray> argumentVariableCache = new Dictionary<IVariable, IMultiDimensionalArray>();
        private string path;
        private string fileName;
        private FeatureTypeConverter featureTypeConverter = new FeatureTypeConverter();
        private NetworkLocationTypeConverter networkLocationTypeConverter = new NetworkLocationTypeConverter();
        private OutputFile1DMetaData metaData;
        private bool disableCaching;
        private int sobekStartIndex = 1;// minus one because fortran is 1 based...

        private const string StandardNameAttribute = "standard_name";
        private const string LongNameAttribute = "long_name";
        private const string UnitAttribute = "units";

        public FM1DFileFunctionStore(IHydroNetwork network)
        {
            OutputFileReader = new FmMapFile1DOutputFileReader();
            sobekStartIndex = 0;
            inputNetwork = network;
        }
        public void Delete()
        {
            //lets not delete! I only do this for the stipid prototype.... the _map.nc file is also used by fmfilefunctionstore.... can't delete it!!
        }

        public object Clone()
        {
            var clonedStore = new FM1DFileFunctionStore((IHydroNetwork)inputNetwork.Clone()) { Path = this.Path, OutputFileReader = new FmMapFile1DOutputFileReader(), CoordinateSystem = CoordinateSystem};

            foreach (var existingNetworkCoverage in Functions.OfType<INetworkCoverage>())
            {
                var newNetworkCoverage = new NetworkCoverage(existingNetworkCoverage.Name, true)
                {
                    Network = existingNetworkCoverage.Network,
                    Store = clonedStore,
                    CoordinateSystem = CoordinateSystem
                };

                clonedStore.Functions.AddRange(newNetworkCoverage.Arguments);
                clonedStore.Functions.AddRange(newNetworkCoverage.Components);
                clonedStore.Functions.Add(newNetworkCoverage);
            }

            foreach (var existingFeatureCoverage in Functions.OfType<IFeatureCoverage>())
            {
                var newFeatureCoverage = new FeatureCoverage(existingFeatureCoverage.Name)
                {
                    Features = existingFeatureCoverage.Features,
                    Store = clonedStore,
                    CoordinateSystem = CoordinateSystem
                };

                clonedStore.Functions.AddRange(newFeatureCoverage.Arguments);
                clonedStore.Functions.AddRange(newFeatureCoverage.Components);
                clonedStore.Functions.Add(newFeatureCoverage);
            }

            foreach (var function in Functions.Where(f => !(f is ICoverage)))
            {
                var matchingFunction = (IVariable)Enumerable.FirstOrDefault<IFunction>(clonedStore.Functions, f => f.Name == function.Name
                                                                                                                   && f.GetType() == function.GetType());

                if (matchingFunction != null) matchingFunction.CopyFrom(function);
            }

            return clonedStore;
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

        protected virtual void UpdateFunctionsAfterPathSet()
        {
            if(CoordinateSystem == null) CoordinateSystem = UGridFileHelper.ReadCoordinateSystem(Path);
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

        protected IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
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

            var location = netCdfFile.GetAttributeValue(netcdfVariable, UGridConstants.Naming.LocationAttributeName);
            var unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);

            coverage = CreateNetworkCoverage(coverageLongName, unitSymbol);
            coverage.Attributes.Add("NetCdfVariableName",netCdfVariableName);
            coverage.Attributes.Add(LocationAttributeName, GetNetworkLocation(location).ToString());
            coverage.Store = this;
            var times = MetaData.Times;
            AddNetworkLocationsToNetworkCoverage(outputDiscretization, times, coverage);
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

        private string GetNetCdfVariableName(ICoverage coverage)
        {
            var networkCoverage = coverage as NetworkCoverage;
            if (networkCoverage?.Attributes == null || !networkCoverage.Attributes.ContainsKey("NetCdfVariableName")) return string.Empty;
            return networkCoverage.Attributes["NetCdfVariableName"];
        }

        private static void AddNetworkLocationsToNetworkCoverage(IDiscretization discretization, ICollection<DateTime> times, INetworkCoverage networkCoverage)
        {
            if (networkCoverage.Store is FM1DFileFunctionStore) return; // temporary until modelApi is removed

            var networkLocations = discretization.Locations.Values.OrderBy(l => l).ToArray();

            networkCoverage.Clear();

            networkCoverage.Time.FixedSize = times.Count;
            networkCoverage.Locations.FixedSize = networkLocations.Length;

            if (times.Count != 0) networkCoverage.Time.SetValues(times);
            if (networkLocations.Length != 0) networkCoverage.SetLocations(networkLocations);
        }
        public ICoordinateSystem CoordinateSystem { get; set; }
        private NetworkCoverage CreateNetworkCoverage(string coverageLongName, string unitSymbol, int number = -1)
        {
            var suffix = number < 0 ? string.Empty : string.Format(" ({0})", number);
            var coverageName = coverageLongName + suffix;
            var networkCoverage = new NetworkCoverage(coverageName, true, coverageName, unitSymbol) { Network = inputNetwork, CoordinateSystem = CoordinateSystem };
            networkCoverage.Components[0].NoDataValue = double.NaN;
            
            networkCoverage.Locations.FixedSize = 0;
            networkCoverage.Locations.InterpolationType = InterpolationType.Constant;
            networkCoverage.Locations.ExtrapolationType = ExtrapolationType.Constant;
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
        protected virtual string ReadReferenceDateFromFile(string timeVariableName)
        {
            NetCdfVariable timeVariable = netCdfFile.GetVariableByName(timeVariableName);
            string timeReference = netCdfFile.GetAttributeValue(timeVariable, "units");

            const string secondsSinceStr = "seconds since ";

            var dateTime = new DateTime(1970, 1, 1); // assume epoch otherwise
            if (timeReference.StartsWith(secondsSinceStr))
            {
                string timeStr = timeReference.Substring(secondsSinceStr.Length);

                if (!DateTime.TryParseExact(timeStr, dateTimeFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTime))
                    throw new ArgumentException("Could not parse time reference");
            }
            return dateTime.ToString(DateTimeFormatInfo.InvariantInfo.FullDateTimePattern, CultureInfo.InvariantCulture);
        }

        private IDisposable ReconnectToMapFile()
        {
            return new NetCdfFileConnection(this);
        }
        private class NetCdfFileConnection : IDisposable
        {
            private readonly bool fileWasAlreadyOpen = true;
            private FM1DFileFunctionStore store;

            public NetCdfFileConnection(FM1DFileFunctionStore store)
            {
                Monitor.Enter(store.readLock);

                this.store = store;

                if (store.netCdfFile != null)
                    return;

                store.netCdfFile = NetCdfFile.OpenExisting(store.Path);
                fileWasAlreadyOpen = false;
            }

            public void Dispose()
            {
                try
                {
                    // there might be nesting:
                    if (fileWasAlreadyOpen)
                        return;

                    store.netCdfFile.Close();
                    store.netCdfFile = null;
                }
                finally
                {
                    Monitor.Exit(store.readLock);
                    store = null;
                }
            }
        }
        protected class NetCdfVariableInfo
        {
            public NetCdfVariableInfo(NetCdfVariable dataVariable, int numDimensions)
            {
                NetCdfDataVariable = dataVariable;
                NumDimensions = numDimensions;
                IsTimeDependent = false;
            }

            public NetCdfVariableInfo(NetCdfVariable dataVariable, int numDimensions, string timeVariableName,
                string referenceDate)
                : this(dataVariable, numDimensions)
            {
                IsTimeDependent = true;
                TimeVariableName = timeVariableName;
                ReferenceDate = referenceDate;
            }

            public NetCdfVariable NetCdfDataVariable { get; private set; }
            public int NumDimensions { get; private set; }

            public bool IsTimeDependent { get; private set; }
            public string TimeVariableName { get; private set; }
            public string ReferenceDate { get; private set; }
        }

        private class VariableSizeInfo
        {
            public object Max;
            public object Min;

            public VariableSizeInfo(object min, object max)
            {
                Min = min;
                Max = max;
            }
        }

        private IList<string> TimeVariableNames
        {
            get { return new[] { GetTimeVariableName(TimeDimensionName) }; }
        }

        private IList<string> TimeDimensionNames
        {
            get { return new[] { TimeDimensionName }; }
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

        public long Id { get; set; }
        public bool SkipChildItemEventBubbling { get; set; }

        public IEventedList<IFunction> Functions
        {
            get { return functions ?? (functions = new EventedList<IFunction>()); }
            set { functions = value; }
        }

        public bool SupportsPartialRemove { get { return false; } }
        public IList<ITypeConverter> TypeConverters { get { return null; } }
        public bool FireEvents { get; set; }

        public bool DisableCaching
        {
            get { return disableCaching; }
            set
            {
                disableCaching = value;
                if (disableCaching)
                {
                    argumentVariableCache.Clear();
                }
            }
        }

        public bool IsMultiValueFilteringSupported { get { return false; } }
        public bool IsFileCritical { get { return false; } }

        public virtual string Path
        {
            get { return path; }
            set
            {
                var previousPath = path;
                path = value;

                if (previousPath == path) return;
                
                fileName = File.Exists(path) ? new FileInfo(path).Name : null;
                metaData = null;

                // clear caches for argument variables and min/max
                argumentVariableCache.Clear(); 
                minValues.Clear();
                maxValues.Clear();

                UpdateNetworkAndDiscretisationAfterPathSet();
                UpdateFunctionsAfterPathSet();
            }
        }

        public IEnumerable<string> Paths { get { return new[] { Path }; } }
        public bool IsOpen { get { return false; } }
        public bool CopyFromWorkingDirectory { get; } = false;

        private string GetTimeVariableName(string dimName)
        {
            return "time";
        }

        public event NotifyCollectionChangingEventHandler CollectionChanging;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public virtual event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanged;
        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanging;

        public Type GetEntityType()
        {
            return GetType();
        }

        public void SetVariableValues<T>(IVariable function, IEnumerable<T> values, params IVariableFilter[] filters)
        {
            throw new NotSupportedException("Function store is readonly");
        }

        public void RemoveFunctionValues(IFunction function, params IVariableValueFilter[] filters)
        {
            Path = null;
        }

        public void AddIndependentVariableValues<T>(IVariable variable, IEnumerable<T> values)
        {
            throw new NotSupportedException("Function store is readonly");
        }

        public IMultiDimensionalArray<T> GetVariableValues<T>(IVariable function, params IVariableFilter[] filters)
        {
            return (IMultiDimensionalArray<T>) GetVariableValues(function, filters);
        }

        public IMultiDimensionalArray GetVariableValues(IVariable variable, params IVariableFilter[] filters)
        {
            if (Path == null || !File.Exists(path))
            {
                log.WarnFormat("Unable to get variable values for function: {0}, path = {1}", variable.Name, Path);
                var genericType = typeof(MultiDimensionalArray<>).MakeGenericType(variable.ValueType);
                return (IMultiDimensionalArray) Activator.CreateInstance(genericType);
            }

            if (variable.IsIndependent) // argument
            {
                if (variable.ValueType == typeof(INetworkLocation))
                {
                    return GetResultsFromCache(variable, () => GetNetworkLocationsForLocations(variable, Enumerable.Range(0, MetaData.NumLocationsForFunctionId(GetNetCdfVariableName(GetCoverage(variable)))).ToList()));
                }

                if (variable.ValueType == typeof(IBranchFeature))
                {
                    return GetResultsFromCache(variable, () => GetBranchFeaturesForLocations(variable, Enumerable.Range(0, MetaData.NumLocationsForFunctionId(GetNetCdfVariableName(GetCoverage(variable)))).ToList()));
                }

                if (variable.ValueType == typeof(DateTime))
                {
                    if (!filters.Any())
                    {
                        return GetResultsFromCache(variable, () => new MultiDimensionalArray<DateTime>(MetaData.Times));
                    }

                    if (filters.Length == 1 && filters[0] is IVariableValueFilter)
                    {
                        var dateTimes = ((IVariableValueFilter)filters[0]).Values.OfType<DateTime>().ToArray();
                        return new MultiDimensionalArray<DateTime>(dateTimes);
                    }
                }
            }

            if (variable.ValueType == typeof(double) && !variable.IsIndependent)
            {
                var coverage = GetCoverage(variable);
                if (coverage == null)
                {
                    //Log.WarnFormat("Could not find output coverage: {0}", coverage.Name);
                    return new MultiDimensionalArray<double>(new List<double>(), new[] {0, 0});
                }

                var ncVariableName = GetNetCdfVariableName(coverage);
                if (ncVariableName == null)
                {
                    log.WarnFormat("Could not find mapping for output coverage: {0}", coverage.Name);
                    return new MultiDimensionalArray<double>(new List<double>(), new [] {0, 0});
                }

                if (filters.Length == 0)
                {
                    return GetValuesForTimeSeriesAtAllLocations(ncVariableName);
                }

                var dateTimeFilter = filters.OfType<VariableValueFilter<DateTime>>().FirstOrDefault(f => f.Variable == coverage.Time);

                var featureVariable = Enumerable.FirstOrDefault<IVariable>(coverage.Arguments, a => a != coverage.Time && a.ValueType.Implements(typeof(IBranchFeature)));
                var branchFeatureFilter = filters.OfType<IVariableValueFilter>().FirstOrDefault(f => f.Variable == featureVariable);
                var branchRangeFilter = filters.OfType<VariableIndexRangesFilter>().FirstOrDefault(f => f.Variable == featureVariable);

                var hasBranchRangeFilter = branchRangeFilter != null && branchRangeFilter.IndexRanges.Count == 1;
                var hasBranchFilter = branchFeatureFilter != null && branchFeatureFilter.Values.Count == 1;
                var hasTimeFilter = dateTimeFilter != null && dateTimeFilter.Values.Count == 1;

                int[] shape = null;
                IList<double> timeSeriesData = null;
                try
                {
                    if (hasTimeFilter)
                    {
                        var timeStepIndex = MetaData.Times.IndexOf(dateTimeFilter.Values[0]);
                        if (hasBranchFilter)
                        {
                            timeSeriesData = GetValueForTimeStepAtSingleLocation(ncVariableName, branchFeatureFilter, timeStepIndex, out shape);
                        }
                        else if (hasBranchRangeFilter)
                        {
                            timeSeriesData = GetValuesForTimeStepAtRangeOfLocations(ncVariableName, branchRangeFilter, timeStepIndex, out shape);
                        }
                        else
                        {
                            timeSeriesData = GetValuesForTimeStepAtAllLocations(ncVariableName, timeStepIndex, out shape);
                        }
                    }
                    else
                    {
                        if (hasBranchFilter)
                        {
                            timeSeriesData = GetValuesForTimeSeriesAtSingleLocation(ncVariableName, branchFeatureFilter, out shape);
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                    return new MultiDimensionalArray<double>();
                }
                

                if (shape == null || timeSeriesData == null)
                {
                    throw new NotImplementedException();
                }

                UpdateMinMax(timeSeriesData, variable);

                return new MultiDimensionalArray<double>(timeSeriesData, shape);
            }

            throw new NotImplementedException();
        }

        private IMultiDimensionalArray GetResultsFromCache(IVariable variable, Func<IMultiDimensionalArray> getResult)
        {
            if (DisableCaching)
            {
                return getResult();
            }

            if (!argumentVariableCache.ContainsKey(variable))
            {
                argumentVariableCache[variable] = getResult();
            }

            return argumentVariableCache[variable];
        }

        public void UpdateVariableSize(IVariable variable)
        {
            throw new NotSupportedException("Function store is readonly");
        }

        public T GetMaxValue<T>(IVariable variable)
        {
            var key = GetNetCdfVariableName(GetCoverage(variable));
            if (typeof(T) == typeof(INetworkLocation))
            {
                var maxValue = Enumerable.Last<INetworkLocation>(GetNetworkLocationsForLocations(variable, new List<int> { MetaData.NumLocationsForFunctionId(key) - 1 }));
                return (T)maxValue;
            }

            if (typeof(T) == typeof(IBranchFeature))
            {
                var maxValue = Enumerable.Last<IBranchFeature>(GetBranchFeaturesForLocations(variable, new List<int> { MetaData.NumLocationsForFunctionId(key) - 1 }));
                return (T)maxValue;
            }

            if (typeof(T) == typeof(double))
            {
                var maxValue = maxValues.ContainsKey(variable.Name) ? maxValues[variable.Name] : 1.0;
                return (T)Convert.ChangeType(maxValue, typeof(T));
            }

            if (typeof(T) == typeof(DateTime))
            {
                var maxDateTime = MetaData.Times.Any() ? MetaData.Times.Last() : DateTime.MaxValue;
                return (T)Convert.ChangeType(maxDateTime, typeof(T));
            }

            throw new NotSupportedException("File only contains doubles or datetime values");
        }

        public T GetMinValue<T>(IVariable variable)
        {
            if (typeof(T) == typeof(INetworkLocation))
            {
                var minValue = Enumerable.First<INetworkLocation>(GetNetworkLocationsForLocations(variable, new List<int> { 0 }));
                return (T)minValue;
            }

            if (typeof(T) == typeof(IBranchFeature))
            {
                var minValue = Enumerable.First<IBranchFeature>(GetBranchFeaturesForLocations(variable, new List<int> { 0 }));
                return (T) minValue;
            }
            
            if (typeof(T) == typeof(double))
            {
                var minValue = minValues.ContainsKey(variable.Name) ? minValues[variable.Name] : 0.0;
                return (T)Convert.ChangeType(minValue, typeof(T));
            }

            if (typeof(T) == typeof(DateTime))
            {
                var minDateTime = MetaData.Times.Any() ? MetaData.Times.First() : DateTime.MinValue;
                return (T)Convert.ChangeType(minDateTime, typeof(T));
            }

            throw new NotSupportedException("File only contains doubles or datetime values");
        }

        public void CacheVariable(IVariable variable)
        {
            throw new NotSupportedException("Function store will cache argument values if DisableCache is false");
        }

        public void CreateNew(string path)
        {
            FileUtils.DeleteIfExists(path);
            Path = path;
        }

        public void Close()
        {
            
        }

        public void Open(string path)
        {
            
        }

        public void CopyTo(string destinationPath)
        {
            if (!File.Exists(path) || Equals(Path, destinationPath))
            {
                return;
            }
            
            var dir = new FileInfo(destinationPath).DirectoryName;
            FileUtils.CreateDirectoryIfNotExists(dir);
            FileUtils.CopyFile(path, destinationPath);
        }

        public void SwitchTo(string newPath)
        {
            path = newPath;
        }

        private IMultiDimensionalArray GetValuesForTimeSeriesAtAllLocations(string ncVariableName)
        {
            var variableData = OutputFileReader.GetAllVariableData(path, ncVariableName, MetaData);
            var variableDataShape = ArrayExtensions.GetShape(variableData);
            return new MultiDimensionalArray<double>(variableData, variableDataShape);
        }

        private IList<double> GetValuesForTimeSeriesAtSingleLocation(string ncVariableName, IVariableValueFilter branchFeatureFilter, out int[] shape)
        {
            var locationIndex = GetLocationIndex(ncVariableName, (IBranchFeature)branchFeatureFilter.Values[0]);

            var origin = new[] { 0, locationIndex };
            shape = new[] { MetaData.NumTimes, 1 };

            return GetSelectionOfVariableData(ncVariableName, origin, ref shape);
        }

        private IList<double> GetValuesForTimeStepAtAllLocations(string ncVariableName, int timeStepIndex, out int[] shape)
        {
            var origin = new[] { timeStepIndex, 0 };
            shape = new[] { 1, MetaData.NumLocationsForFunctionId(ncVariableName) };

            return GetSelectionOfVariableData(ncVariableName, origin, ref shape);
        }

        private IList<double> GetValuesForTimeStepAtRangeOfLocations(string ncVariableName, VariableIndexRangesFilter branchRangeFilter, int timeStepIndex, out int[] shape)
        {
            var endIndex = branchRangeFilter.IndexRanges[0].Second;
            var beginIndex = branchRangeFilter.IndexRanges[0].First;

            var origin = new[] { timeStepIndex, beginIndex };
            shape = new[] { 1, endIndex - beginIndex + 1 };

            return GetSelectionOfVariableData(ncVariableName, origin, ref shape);
        }

        private IList<double> GetValueForTimeStepAtSingleLocation(string ncVariableName, IVariableValueFilter branchFeatureFilter, int timeStepIndex, out int[] shape)
        {
            var locationIndex = GetLocationIndex(ncVariableName, (IBranchFeature)branchFeatureFilter.Values[0]);

            var origin = new[] { timeStepIndex, locationIndex };
            shape = new[] { 1, 1 };

            return GetSelectionOfVariableData(ncVariableName, origin, ref shape);
        }

        private IList<double> GetSelectionOfVariableData(string ncVariableName, int[] origin, ref int[] shape)
        {
            try
            {
                return OutputFileReader.GetSelectionOfVariableData(path, ncVariableName, origin, shape);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error retrieving data for variable {0}: {1}", ncVariableName, ex.Message);
                shape = new[] { 0, 0 };
                return new List<double>();
            }
        }

        private IMultiDimensionalArray<INetworkLocation> GetNetworkLocationsForLocations(IVariable function, ICollection<int> locations)
        {
            UpdateTypeConverters(function);
            var convertedList = (List<INetworkLocation>)TypeUtils.CreateGeneric(typeof(List<>), networkLocationTypeConverter.ConvertedType);
            var key = GetNetCdfVariableName(GetCoverage(function));
            var MetaLocationInfo = MetaData.Locations.FirstOrDefault(l => l.Key.Name == key);

            foreach (var location in locations)
            {
                var branchId = MetaLocationInfo.Value[location].BranchId - sobekStartIndex; 
                var chainage = MetaLocationInfo.Value[location].Chainage;
                var networkLocation = networkLocationTypeConverter.ConvertFromStore(new object[] { branchId, chainage });
                networkLocation.Chainage = networkLocation.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(networkLocation.Chainage);
                convertedList.Add(networkLocation);
            }

            var shape = new[] { locations.Count };
            return new MultiDimensionalArray<INetworkLocation>(convertedList, shape);
        }

        private IMultiDimensionalArray<IBranchFeature> GetBranchFeaturesForLocations(IVariable function, ICollection<int> locations)
        {
            UpdateTypeConverters(function);
            var convertedList = (List<IBranchFeature>)TypeUtils.CreateGeneric(typeof(List<>), featureTypeConverter.ConvertedType);

            convertedList.AddRange(locations
                .Select(location => featureTypeConverter.ConvertFromStore(new object[] { location }))
                .OfType<IBranchFeature>());

            var shape = new[] { locations.Count };
            return new MultiDimensionalArray<IBranchFeature>(convertedList, shape);
        }

        private int GetLocationIndex(string ncVariableName, IBranchFeature branchFeature)
        {
            LocationMetaData location;
            var metaDataLocations = MetaData.Locations.FirstOrDefault(l => l.Key.Name == ncVariableName);
            if (branchFeature is INetworkLocation)
            {
                var branchIndex = branchFeature.Network.Branches.IndexOf(branchFeature.Branch);
                location = metaDataLocations.Value.FirstOrDefault(l =>
                {
                    // euuh klopt dit voor metaLocationChainage?
                    var metaLocationChainage = branchFeature.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(l.Chainage);
                    var branchFeatureChainage = branchFeature.Branch.CorrectlyRoundOffChainageIfChainageIsOnEndOfBranch(branchFeature.Chainage);
                    return l.BranchId - sobekStartIndex == branchIndex &&
                           Math.Abs(metaLocationChainage - branchFeatureChainage) < double.Epsilon;
                });
            }
            else if (branchFeature is IStructure1D)
            {
                var structure = (IStructure1D)branchFeature;

                var compositePrefix = structure.ParentStructure?.Structures.Count > 1
                    ? structure.ParentStructure.Name + "_"
                    : string.Empty;

                var structureName = compositePrefix + branchFeature.Name;

                location = metaDataLocations.Value.FirstOrDefault(l => l.Id == structureName);
            }
            else
            {
                location = metaDataLocations.Value.FirstOrDefault(l => l.Id == branchFeature.Name);
            }

            if (location == null)
            {
                throw new ArgumentException(string.Format((string)"Values for {0} feature type {1} could not be found.", branchFeature.Name, branchFeature.GetType().Name));
            }

            return metaDataLocations.Value.IndexOf(location);
        }

        private ICoverage GetCoverage(IVariable variable)
        {
            return functions.OfType<ICoverage>().FirstOrDefault(f => f.Arguments.Concat(f.Components).Contains(variable));
        }

        private void UpdateTypeConverters(IVariable function)
        {
            if (functions.Any(f => f is INetworkCoverage))
            {
                var networkCoverage = functions.OfType<INetworkCoverage>().FirstOrDefault(f => f.Arguments.Contains(function));
                if (networkCoverage != null)
                {
                    networkLocationTypeConverter.Network = networkCoverage.Network;
                    networkLocationTypeConverter.Coverage = networkCoverage;
                }
            }

            if (functions.Any(f => f is IFeatureCoverage))
            {
                var featureCoverage = functions.OfType<IFeatureCoverage>().FirstOrDefault(f => f.Arguments.Contains(function));
                if (featureTypeConverter != null) featureTypeConverter.FeatureCoverage = featureCoverage;
            }
        }

        private void UpdateMinMax(IEnumerable<double> timeStepData, IVariable function)
        {
            double? min = null;
            double? max = null;

            foreach (var value in timeStepData)
            {
                if (Equals(value, function.NoDataValue)) continue;

                if (min == null || min.Value > value)
                {
                    min = value;
                }

                if (max == null || max.Value < value)
                {
                    max = value;
                }
            }

            var name = function.Name;
            var minMaxChanged = false;
            if (min != null && (!minValues.ContainsKey(name) || minValues[name] > min.Value))
            {
                minValues[name] = min.Value;
                minMaxChanged = true;
            }

            if (max != null && (!maxValues.ContainsKey(name) || maxValues[name] < max.Value))
            {
                maxValues[name] = max.Value;
                minMaxChanged = true;
            }

            if (!minMaxChanged) return;

            FireFunctionValuesChanged(this, new FunctionValuesChangingEventArgs { Function = function });
        }

        private void FireFunctionValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            if (FunctionValuesChanged == null) return;
            FunctionValuesChanged(sender, e);
        }
    }
}