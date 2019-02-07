using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using ArrayExtensions = DelftTools.Utils.ArrayExtensions;

namespace DeltaShell.NGHS.IO.Store1D
{
    public abstract class NetCdfFunctionStore1DBase<T, U> : IFunctionStore, IFileBased where T : ILocationMetaData, new() where U : ITimeDependentVariableMetaDataBase, new()
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NetCdfFunctionStore1DBase<T, U>));
        private IEventedList<IFunction> functions;
        private readonly IDictionary<string, double> minValues = new Dictionary<string, double>();
        private readonly IDictionary<string, double> maxValues = new Dictionary<string, double>();
        private readonly Dictionary<IVariable, IMultiDimensionalArray> argumentVariableCache = new Dictionary<IVariable, IMultiDimensionalArray>();
        private string path;
        protected string fileName;
        protected FeatureTypeConverter featureTypeConverter = new FeatureTypeConverter();
        protected NetworkLocationTypeConverter networkLocationTypeConverter = new NetworkLocationTypeConverter();
        private OutputFile1DMetaData<T, U> metaData;
        private bool disableCaching;
        private IOutput1DFileReader<T, U> outputFileReader;
        protected int sobekStartIndex = 1;// minus one because fortran is 1 based...

        protected OutputFile1DMetaData<T, U> MetaData
        {
            get
            {
                if (metaData != null) return metaData;

                if(!File.Exists(Path)) return new OutputFile1DMetaData<T, U>();

                try
                {
                    metaData = OutputFileReader.ReadMetaData(Path);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Error reading MetaData for file: {0}{1}{2}", Path, Environment.NewLine, ex.Message);
                    metaData = new OutputFile1DMetaData<T, U>();
                }
                return metaData;
            }
        }

        protected IOutput1DFileReader<T, U> OutputFileReader
        {
            get { return outputFileReader; }
            set { outputFileReader = value; }
        }

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
                path = value;
                fileName = File.Exists(path) ? new FileInfo(path).Name : null;
                metaData = null;

                // clear caches for argument variables and min/max
                argumentVariableCache.Clear(); 
                minValues.Clear();
                maxValues.Clear();
            }
        }

        public IEnumerable<string> Paths { get { return new[] { Path }; } }
        public bool IsOpen { get { return false; } }

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
                Log.WarnFormat("Unable to get variable values for function: {0}, path = {1}", variable.Name, Path);
                var genericType = typeof(MultiDimensionalArray<>).MakeGenericType(variable.ValueType);
                return (IMultiDimensionalArray) Activator.CreateInstance(genericType);
            }

            if (variable.IsIndependent) // argument
            {
                if (variable.ValueType == typeof(INetworkLocation))
                {
                    return GetResultsFromCache(variable, () => GetNetworkLocationsForLocations(variable, Enumerable.Range(0, MetaData.NumLocations).ToList()));
                }

                if (variable.ValueType == typeof(IBranchFeature))
                {
                    return GetResultsFromCache(variable, () => GetBranchFeaturesForLocations(variable, Enumerable.Range(0, MetaData.NumLocations).ToList()));
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
                var ncVariableName = GetNetCdfVariableName(coverage);
                if (ncVariableName == null)
                {
                    Log.WarnFormat("Could not find mapping for output coverage: {0}", coverage.Name);
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
                    Log.Error(e.Message);
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
            if (typeof(T) == typeof(INetworkLocation))
            {
                var maxValue = Enumerable.Last<INetworkLocation>(GetNetworkLocationsForLocations(variable, new List<int> { MetaData.NumLocations - 1 }));
                return (T)maxValue;
            }

            if (typeof(T) == typeof(IBranchFeature))
            {
                var maxValue = Enumerable.Last<IBranchFeature>(GetBranchFeaturesForLocations(variable, new List<int> { MetaData.NumLocations -1 }));
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
            Path = newPath;
        }

        public virtual void Delete()
        {
            FileUtils.DeleteIfExists(path);
        }

        private IMultiDimensionalArray GetValuesForTimeSeriesAtAllLocations(string ncVariableName)
        {
            var variableData = OutputFileReader.GetAllVariableData(path, ncVariableName, MetaData);
            var variableDataShape = ArrayExtensions.GetShape(variableData);
            return new MultiDimensionalArray<double>(variableData, variableDataShape);
        }

        private IList<double> GetValuesForTimeSeriesAtSingleLocation(string ncVariableName, IVariableValueFilter branchFeatureFilter, out int[] shape)
        {
            var locationIndex = GetLocationIndex((IBranchFeature)branchFeatureFilter.Values[0]);

            var origin = new[] { 0, locationIndex };
            shape = new[] { MetaData.NumTimes, 1 };

            return GetSelectionOfVariableData(ncVariableName, origin, ref shape);
        }

        private IList<double> GetValuesForTimeStepAtAllLocations(string ncVariableName, int timeStepIndex, out int[] shape)
        {
            var origin = new[] { timeStepIndex, 0 };
            shape = new[] { 1, MetaData.NumLocations };

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
            var locationIndex = GetLocationIndex((IBranchFeature)branchFeatureFilter.Values[0]);

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
                Log.ErrorFormat("Error retrieving data for variable {0}: {1}", ncVariableName, ex.Message);
                shape = new[] { 0, 0 };
                return new List<double>();
            }
        }

        private IMultiDimensionalArray<INetworkLocation> GetNetworkLocationsForLocations(IVariable function, ICollection<int> locations)
        {
            UpdateTypeConverters(function);
            var convertedList = (List<INetworkLocation>)TypeUtils.CreateGeneric(typeof(List<>), networkLocationTypeConverter.ConvertedType);

            foreach (var location in locations)
            {
                var branchId = MetaData.Locations[location].BranchId - sobekStartIndex; 
                var chainage = MetaData.Locations[location].Chainage;
                var networkLocation = networkLocationTypeConverter.ConvertFromStore(new object[] { branchId, chainage });
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

        private int GetLocationIndex(IBranchFeature branchFeature)
        {
            T location;
            if (branchFeature is INetworkLocation)
            {
                var branchIndex = branchFeature.Network.Branches.IndexOf(branchFeature.Branch);
                location = MetaData.Locations.FirstOrDefault(l => l.BranchId - sobekStartIndex == branchIndex && Math.Abs(l.Chainage - branchFeature.Chainage) < double.Epsilon);
            }
            else if (branchFeature is IStructure1D)
            {
                var structure = (IStructure1D)branchFeature;

                var compositePrefix = structure.ParentStructure?.Structures.Count > 1
                    ? structure.ParentStructure.Name + "_"
                    : string.Empty;

                var structureName = compositePrefix + branchFeature.Name;

                location = MetaData.Locations.FirstOrDefault(l => l.Id == structureName);
            }
            else
            {
                location = MetaData.Locations.FirstOrDefault(l => l.Id == branchFeature.Name);
            }

            if (location == null)
            {
                throw new ArgumentException(string.Format((string)"Values for {0} feature type {1} could not be found.", branchFeature.Name, branchFeature.GetType().Name));
            }

            return MetaData.Locations.IndexOf(location);
        }

        protected virtual string GetNetCdfVariableName(ICoverage coverage)
        {
            return string.Empty; 
        }

        private ICoverage GetCoverage(IVariable variable)
        {
            return functions.OfType<ICoverage>().FirstOrDefault(f => f.Arguments.Concat(f.Components).Contains(variable));
        }

        private void UpdateTypeConverters(IVariable function)
        {
            if (functions.Any(f => f is INetworkCoverage))
            {
                var networkCoverage = functions.OfType<INetworkCoverage>().First(f => f.Arguments.Contains(function));
                networkLocationTypeConverter.Network = networkCoverage.Network;
                networkLocationTypeConverter.Coverage = networkCoverage;
            }

            if (functions.Any(f => f is IFeatureCoverage))
            {
                var featureCoverage = functions.OfType<IFeatureCoverage>().First(f => f.Arguments.Contains(function));
                featureTypeConverter.FeatureCoverage = featureCoverage;
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

        public abstract object Clone();
    }
}