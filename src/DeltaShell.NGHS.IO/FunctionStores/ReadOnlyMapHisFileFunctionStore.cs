using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.FunctionStores
{
    public class ReadOnlyMapHisFileFunctionStore : IFunctionStore, IFileBased
    {
        private IEventedList<IFunction> functions = new EventedList<IFunction>();
        private MapHisFileMetaData metaData;
        private readonly IDictionary<string, double> minValues = new Dictionary<string, double>();
        private readonly IDictionary<string, double> maxValues = new Dictionary<string, double>();
        private readonly IDictionary<string, object> locationLookup = new Dictionary<string, object>();

        private string path;
        private Func<string, object> locationsFromStringToObject;

        public long Id { get; set; }

        public IEventedList<IFunction> Functions
        {
            get { return functions; }
            set { functions = value; }
        }

        public bool FireEvents { get; set; }

        public string Path
        {
            get { return path; }
            set
            {
                path = value;

                metaData = null;
                minValues.Clear();
                maxValues.Clear();
                locationLookup.Clear();
            }
        }

        public IEnumerable<string> Paths
        {
            get { return new[] {Path}; }
        }

        public bool IsFileCritical
        {
            get { return false; }
        }

        public bool IsOpen
        {
            get { return false; }
        }

        public bool CopyFromWorkingDirectory { get; } = false;

        #region Unsupported properties

        public bool SkipChildItemEventBubbling { get; set; }

        public bool SupportsPartialRemove
        {
            get { return false; }
        }

        public IList<ITypeConverter> TypeConverters
        {
            get { return null; }
        }

        public bool DisableCaching { get; set; }

        public bool IsMultiValueFilteringSupported
        {
            get { return true; }
        }

        #endregion

        #region Unsupported functions

        public void SetVariableValues<T>(IVariable function, IEnumerable<T> values, params IVariableFilter[] filters)
        {
            throw new NotSupportedException("Function store is readonly");
        }

        public void RemoveFunctionValues(IFunction function, params IVariableValueFilter[] filters)
        {
            throw new NotSupportedException("Function store is readonly");
        }

        public void AddIndependentVariableValues<T>(IVariable variable, IEnumerable<T> values)
        {
            throw new NotSupportedException("Function store is readonly");
        }

        public void UpdateVariableSize(IVariable variable)
        {
            throw new NotSupportedException("Function store is readonly");
        }

        public void CacheVariable(IVariable variable)
        {
            throw new NotSupportedException("Function store has no caching");
        }

        #endregion

        public Type GetEntityType()
        {
            return GetType();
        }

        public object Clone()
        {
            var clonedStore = new ReadOnlyMapHisFileFunctionStore(){ Path = this.Path };

            foreach (var existingFeatureCoverage in Functions.OfType<IFeatureCoverage>())
            {
                var newFeatureCoverage = new FeatureCoverage(existingFeatureCoverage.Name)
                {
                    Features = existingFeatureCoverage.Features,
                    Store = clonedStore
                };

                clonedStore.Functions.AddRange(newFeatureCoverage.Arguments);
                clonedStore.Functions.AddRange(newFeatureCoverage.Components);
                clonedStore.Functions.Add(newFeatureCoverage);
            }

            return clonedStore;
        }

        public Func<object, string> LocationFromObjectToString { get; set; }

        public Func<string, object> LocationsFromStringToObject
        {
            get { return locationsFromStringToObject; }
            set
            {
                locationsFromStringToObject = value;
                locationLookup.Clear();
            }
        }

        public Func<string, string> GetParameterName { get; set; }

        private object GetObjectForLocationName(string locationName)
        {
            object objectToFind;
            if (!locationLookup.TryGetValue(locationName, out objectToFind))
            {
                objectToFind = LocationsFromStringToObject(locationName);
                locationLookup[locationName] = objectToFind;
            }

            return objectToFind;
        }

        public IMultiDimensionalArray GetVariableValues(IVariable function, params IVariableFilter[] filters)
        {
            if (!HasValidFile)
            {
                return CreateEmptyArrayForType(function.ValueType);
            }

            if (function.IsIndependent) 
            {
                // is argument
                return GetArgumentValues(function, filters);
            }

            var timeVariable = function.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime));

            // should be time dependent component with single value filters
            if (function.ValueType != typeof(double) || function.IsIndependent || timeVariable == null || filters.OfType<IVariableValueFilter>().Any(f => f.Values.Count > 1))
                throw new NotImplementedException();

            var parameterName = GetParameterName != null ? GetParameterName(function.Name) : function.Name;
            if (string.IsNullOrEmpty(parameterName))
            {
                return CreateEmptyArrayForType(function.ValueType);
            }

            List<double> data = null;
            int[] shape = new int[]{1};

            var timeFilter = filters.Where(f => f.Variable == timeVariable).OfType<VariableValueFilter<DateTime>>().FirstOrDefault();

            // UnstructuredCoverage
            var locationIndexVariable = function.Arguments.FirstOrDefault(a => a.ValueType == typeof(int));
            if (locationIndexVariable != null)
            {
                var locationFilter = filters.Where(f => f.Variable == locationIndexVariable).OfType<VariableValueFilter<int>>().FirstOrDefault();

                if (locationFilter == null && timeFilter == null)
                    throw new NotImplementedException();
                
                var locationIndex = locationFilter != null ? locationFilter.Values[0] : -1;
                var timeIndex = timeFilter != null ? MetaData.Times.IndexOf(timeFilter.Values[0]) : -1;

                data = timeIndex != -1
                    ? MapHisFileReader.GetTimeStepData(path, MetaData, timeIndex, parameterName, locationIndex)
                    : MapHisFileReader.GetTimeSeriesData(path, MetaData, parameterName, locationIndex);

                if (data != null)
                {
                    shape = timeIndex != -1
                        ? new[] {1, data.Count}
                        : new[] {MetaData.NumberOfTimeSteps, 1};
                }
            }

            // FeatureCoverage
            var featureVariable = function.Arguments.FirstOrDefault(a => a.ValueType.Implements(typeof(IFeature)));
            if (featureVariable != null)
            {
                if (!filters.Any())
                {
                    Func<IMultiDimensionalArray<double>> realGetFunction = () =>
                    {
                        var values = Enumerable.Range(0, MetaData.Times.Count)
                            .SelectMany(i => MapHisFileReader.GetTimeStepData(path, MetaData, i, parameterName))
                            .ToList();
                        UpdateMinMax(values, parameterName, function);
                        return new MultiDimensionalArray<double>(values, MetaData.NumberOfTimeSteps, MetaData.NumberOfLocations);
                    };
                    return new LazyMultiDimensionalArray<double>(realGetFunction, () => MetaData.NumberOfTimeSteps * MetaData.NumberOfLocations);
                }

                var featureVariableFilters = filters.Where(f => f.Variable == featureVariable).OfType<VariableValueFilter<IFeature>>().ToList();
                var locationIndex = featureVariableFilters.Count == 1
                    ? MetaData.Locations.IndexOf(LocationFromObjectToString(featureVariableFilters[0].Values[0]))
                    : -1;
                var timeIndex = timeFilter != null ? MetaData.Times.IndexOf(timeFilter.Values[0]) : -1;

                data = timeIndex != -1
                    ? MapHisFileReader.GetTimeStepData(path, MetaData, timeIndex, parameterName, locationIndex)
                    : MapHisFileReader.GetTimeSeriesData(path, MetaData, parameterName, locationIndex);

                if (data != null)
                {
                    shape = timeIndex != -1
                        ? new[] { 1, data.Count }
                        : new[] { MetaData.NumberOfTimeSteps, 1 };
                }
            }

            // TimeSeries
            if (function.Arguments.Count == 1 && !filters.Any() && MetaData.NumberOfLocations == 1)
            {
                data = MapHisFileReader.GetTimeSeriesData(path, MetaData, parameterName, 0);
                shape = new[] {MetaData.NumberOfTimeSteps};
            }

            if (data == null)
                return new MultiDimensionalArray<double>(new List<double>(), new[] { 0, 0 });

            UpdateMinMax(data, parameterName, function);
            return new MultiDimensionalArray<double>(data, shape);
        }

        private IMultiDimensionalArray GetArgumentValues(IVariable function, IVariableFilter[] filters)
        {
            var argumentTimeFilter = filters.OfType<VariableValueFilter<DateTime>>().FirstOrDefault();
            if (function.ValueType == typeof(DateTime))
            {
                return new MultiDimensionalArray<DateTime>(argumentTimeFilter == null? MetaData.Times : argumentTimeFilter.Values);
            }

            if (function.ValueType.Implements(typeof(IFeature)))
            {
                var features = MetaData.Locations.Select(GetObjectForLocationName).OfType<IFeature>().ToList();

                if (filters.Length == 1 && argumentTimeFilter != null)
                {
                    return new MultiDimensionalArray<IFeature>(features, new[] {1, features.Count});
                }
                if (!filters.Any())
                {
                    return new MultiDimensionalArray<IFeature>(features);
                }
            }

            if (function.ValueType == typeof(int) && !filters.Any())
            {
                return new MultiDimensionalArray<int>(MetaData.Locations.Select(l => Convert.ToInt32(l)).ToList());
            }

            throw new NotImplementedException();
        }

        public IMultiDimensionalArray<T> GetVariableValues<T>(IVariable function, params IVariableFilter[] filters)
        {
            return (IMultiDimensionalArray<T>) GetVariableValues(function, filters);
        }

        private void UpdateMinMaxWithOneTimeStep(IVariable variable)
        {
            if (MetaData.NumberOfTimeSteps == 0) return;

            var dateTimeVariable = variable.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime));
            GetVariableValues(variable, new VariableValueFilter<DateTime>(dateTimeVariable, MetaData.Times[0]));
        }
            
        public T GetMaxValue<T>(IVariable variable)
        {
            if (typeof(T) == typeof(double) && !variable.IsIndependent)
            {
                var parameterName = GetParameterName != null ? GetParameterName(variable.Name) : variable.Name;
                double maxValue;
                if (maxValues.ContainsKey(parameterName))
                {
                    maxValue = maxValues[parameterName];
                }
                else
                {
                    UpdateMinMaxWithOneTimeStep(variable);
                    maxValue = maxValues.ContainsKey(parameterName) ? maxValues[parameterName] : 1;
                }

                return (T) Convert.ChangeType(maxValue, typeof(T));
            }

            if (typeof(T) == typeof(DateTime))
            {
                var maxDateTime = MetaData.Times.Count == 0 ? MetaData.Times[0] : MetaData.Times.Last();
                return (T) Convert.ChangeType(maxDateTime, typeof(T));
            }

            throw new NotSupportedException("Map or His file only contains doubles or datetime values");
        }

        public T GetMinValue<T>(IVariable variable)
        {
            if (typeof(T) == typeof(double) && !variable.IsIndependent)
            {
                double minValue;
                var parameterName = GetParameterName != null ? GetParameterName(variable.Name) : variable.Name;
                if (minValues.ContainsKey(parameterName))
                {
                    minValue = minValues[parameterName];
                }
                else
                {
                    UpdateMinMaxWithOneTimeStep(variable);
                    minValue = minValues.ContainsKey(parameterName) ? minValues[parameterName] : 0;
                }

                return (T) Convert.ChangeType(minValue, typeof(T));
            }
            if (typeof(T) == typeof(DateTime))
            {
                return (T) Convert.ChangeType(MetaData.Times[0], typeof(T));
            }

            throw new NotSupportedException("Map or His file only contains doubles or datetime values");
        }

        public void CreateNew(string path)
        {
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

            // create directory if not exists
            var destinationFileInfo = new FileInfo(destinationPath);
            FileUtils.CreateDirectoryIfNotExists(destinationFileInfo.DirectoryName);

            FileUtils.CopyFile(path, destinationPath);
        }

        public void SwitchTo(string newPath)
        {
            path = newPath;
        }

        public void Delete()
        {
            //FileUtils.DeleteIfExists(path);
        }

        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanged;

        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanging;

        public event NotifyCollectionChangingEventHandler CollectionChanging;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private bool HasValidFile
        {
            get { return !string.IsNullOrEmpty(path) && File.Exists(path) && MetaData != null; }
        }

        private MapHisFileMetaData MetaData
        {
            get
            {
                if (metaData == null)
                {
                    metaData = MapHisFileReader.ReadMetaData(path);
                }
                return metaData;
            }
        }

        private static IMultiDimensionalArray CreateEmptyArrayForType(Type type)
        {
            var listType = typeof(List<>).MakeGenericType(type);
            var mda = typeof(MultiDimensionalArray<>).MakeGenericType(type);
            return (IMultiDimensionalArray) Activator.CreateInstance(mda, Activator.CreateInstance(listType));
        }

        private void UpdateMinMax(IEnumerable<double> timeStepData, string name, IVariable function)
        {
            double? min = null;
            double? max = null;

            double noDataValue = (double)(function.NoDataValue ?? 0.0);

            foreach (var value in timeStepData)
            {
                if (Equals(value, noDataValue))
                    continue;

                if (min == null || min.Value > value)
                {
                    min = value;
                }

                if (max == null || max.Value < value)
                {
                    max = value;
                }
            }

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

            FireFunctionValuesChanged(this, new FunctionValuesChangingEventArgs{ Function = function });
        }

        private void FireFunctionValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            if (FunctionValuesChanged == null)
                return;
            FunctionValuesChanged(sender, e);
        }

    }
};