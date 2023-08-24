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
        private readonly IDictionary<string, double> minValues = new Dictionary<string, double>();
        private readonly IDictionary<string, double> maxValues = new Dictionary<string, double>();
        private readonly IDictionary<string, object> locationLookup = new Dictionary<string, object>();
        private MapHisFileMetaData metaData;

        private string path;
        private Func<string, object> locationsFromStringToObject;

        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanged;

        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanging;

        public event NotifyCollectionChangingEventHandler CollectionChanging;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public Func<object, string> LocationFromObjectToString { get; set; }

        public Func<string, object> LocationsFromStringToObject
        {
            get
            {
                return locationsFromStringToObject;
            }
            set
            {
                locationsFromStringToObject = value;
                locationLookup.Clear();
            }
        }

        public Func<string, string> GetParameterName { get; set; }

        public string Path
        {
            get
            {
                return path;
            }
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
            get
            {
                return new[]
                {
                    Path
                };
            }
        }

        public bool IsFileCritical
        {
            get
            {
                return false;
            }
        }

        public bool IsOpen
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Make a copy of the file if it is located in the DeltaShell working directory
        /// </summary>
        public bool CopyFromWorkingDirectory { get; }

        public long Id { get; set; }

        public IEventedList<IFunction> Functions { get; set; }

        public bool FireEvents { get; set; }

        public void CreateNew(string path)
        {
            // Nothing to be done, is enforced through IFileBased.
        }

        public void Close()
        {
            // Nothing to be done, is enforced through IFileBased.
        }

        public void Open(string path)
        {
            // Nothing to be done, is enforced through IFileBased.
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
            // Nothing to delete
        }

        public Type GetEntityType()
        {
            return GetType();
        }

        public object Clone()
        {
            var clonedStore = new ReadOnlyMapHisFileFunctionStore() {Path = Path};

            foreach (IFeatureCoverage existingFeatureCoverage in Functions.OfType<IFeatureCoverage>())
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

        public IMultiDimensionalArray GetVariableValues(IVariable variable, params IVariableFilter[] filters)
        {
            if (!HasValidFile)
            {
                return CreateEmptyArrayForType(variable.ValueType);
            }

            if (variable.IsIndependent)
            {
                // is argument
                return GetArgumentValues(variable, filters);
            }

            IVariable timeVariable = variable.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime));

            // should be time dependent component with single value filters
            if (IsValidVariable(variable, filters, timeVariable))
            {
                throw new NotImplementedException();
            }

            string parameterName = GetParameterName != null ? GetParameterName(variable.Name) : variable.Name;
            if (string.IsNullOrEmpty(parameterName))
            {
                return CreateEmptyArrayForType(variable.ValueType);
            }

            VariableValueFilter<DateTime> timeFilter = filters.Where(f => f.Variable == timeVariable).OfType<VariableValueFilter<DateTime>>().FirstOrDefault();

            IVariable locationIndexVariable = variable.Arguments.FirstOrDefault(a => a.ValueType == typeof(int));
            if (locationIndexVariable != null)
            {
                return CreateMultiDimensionalArrayForUnstructuredGridCoverage(filters, locationIndexVariable, timeFilter, parameterName, variable);
            }

            IVariable featureVariable = variable.Arguments.FirstOrDefault(a => a.ValueType.Implements(typeof(IFeature)));
            if (featureVariable != null)
            {
                return CreateMultiDimensionalArrayForFeatureCoverage(filters, featureVariable, timeFilter, parameterName, variable);
            }

            if (variable.Arguments.Count == 1 && !filters.Any() && MetaData.NumberOfLocations == 1)
            {
                return CreateMultiDimensionalArrayForTimeSeries(variable, parameterName);
            }

            return CreateEmptyMultiDimensionalArray();
        }

        public IMultiDimensionalArray<T> GetVariableValues<T>(IVariable variable, params IVariableFilter[] filters)
        {
            return (IMultiDimensionalArray<T>) GetVariableValues(variable, filters);
        }

        private IMultiDimensionalArray CreateMultiDimensionalArrayForTimeSeries(IVariable variable, string parameterName)
        {
            List<double> data = MapHisFileReader.GetTimeSeriesData(path, MetaData, parameterName, 0);
            if (!data.Any())
            {
                return CreateEmptyMultiDimensionalArray();
            }

            int[] shape =
            {
                MetaData.NumberOfTimeSteps
            };

            UpdateMinMax(data, parameterName, variable);
            return new MultiDimensionalArray<double>(data, shape);
        }

        private IMultiDimensionalArray CreateMultiDimensionalArrayForFeatureCoverage(IVariableFilter[] filters, IVariable featureVariable, VariableValueFilter<DateTime> timeFilter, string parameterName, IVariable variable)
        {
            if (!filters.Any())
            {
                Func<IMultiDimensionalArray<double>> realGetFunction = () =>
                {
                    List<double> values = Enumerable.Range(0, MetaData.Times.Count)
                                                    .SelectMany(i => MapHisFileReader.GetTimeStepData(path, MetaData, i, parameterName))
                                                    .ToList();
                    UpdateMinMax(values, parameterName, variable);
                    return new MultiDimensionalArray<double>(values, new[] { MetaData.NumberOfTimeSteps, MetaData.NumberOfLocations });
                };
                return new LazyMultiDimensionalArray<double>(realGetFunction, () => MetaData.NumberOfTimeSteps * MetaData.NumberOfLocations);
            }

            List<VariableValueFilter<IFeature>> featureVariableFilters = filters.Where(f => f.Variable == featureVariable).OfType<VariableValueFilter<IFeature>>().ToList();
            int locationIndex = featureVariableFilters.Count == 1
                                    ? MetaData.Locations.IndexOf(LocationFromObjectToString(featureVariableFilters[0].Values[0]))
                                    : -1;
            int timeIndex = timeFilter != null ? MetaData.Times.IndexOf(timeFilter.Values[0]) : -1;

            List<double> data = timeIndex != -1
                                    ? MapHisFileReader.GetTimeStepData(path, MetaData, timeIndex, parameterName, locationIndex)
                                    : MapHisFileReader.GetTimeSeriesData(path, MetaData, parameterName, locationIndex);

            if (!data.Any())
            {
                return CreateEmptyMultiDimensionalArray();
            }

            int[] shape = timeIndex != -1
                              ? new[]
                              {
                                  1,
                                  data.Count
                              }
                              : new[]
                              {
                                  MetaData.NumberOfTimeSteps,
                                  1
                              };

            UpdateMinMax(data, parameterName, variable);
            return new MultiDimensionalArray<double>(data, shape);
        }

        private IMultiDimensionalArray CreateMultiDimensionalArrayForUnstructuredGridCoverage(IVariableFilter[] filters, IVariable locationIndexVariable, VariableValueFilter<DateTime> timeFilter, string parameterName, IVariable variable)
        {
            VariableValueFilter<int> locationFilter = filters.Where(f => f.Variable == locationIndexVariable).OfType<VariableValueFilter<int>>().FirstOrDefault();

            if (locationFilter == null && timeFilter == null)
            {
                throw new NotImplementedException();
            }

            int locationIndex = locationFilter != null ? locationFilter.Values[0] : -1;
            int timeIndex = timeFilter != null ? MetaData.Times.IndexOf(timeFilter.Values[0]) : -1;

            List<double> data = timeIndex != -1
                                    ? MapHisFileReader.GetTimeStepData(path, MetaData, timeIndex, parameterName, locationIndex)
                                    : MapHisFileReader.GetTimeSeriesData(path, MetaData, parameterName, locationIndex);

            if (!data.Any())
            {
                return CreateEmptyMultiDimensionalArray();
            }

            int[] shape = timeIndex != -1
                              ? new[]
                              {
                                  1,
                                  data.Count
                              }
                              : new[]
                              {
                                  MetaData.NumberOfTimeSteps,
                                  1
                              };

            UpdateMinMax(data, parameterName, variable);
            return new MultiDimensionalArray<double>(data, shape);
        }

        private static IMultiDimensionalArray CreateEmptyMultiDimensionalArray()
        {
            return new MultiDimensionalArray<double>(new List<double>(), new[]
            {
                0,
                0
            });
        }

        private static bool IsValidVariable(IVariable variable, IVariableFilter[] filters, IVariable timeVariable)
        {
            return variable.ValueType != typeof(double) ||
                   variable.IsIndependent ||
                   timeVariable == null ||
                   filters.OfType<IVariableValueFilter>().Any(f => f.Values.Count > 1);
        }

        public T GetMaxValue<T>(IVariable variable)
        {
            if (typeof(T) == typeof(double) && !variable.IsIndependent)
            {
                string parameterName = GetParameterName != null ? GetParameterName(variable.Name) : variable.Name;
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
                DateTime maxDateTime = MetaData.Times.Count == 0 ? MetaData.Times[0] : MetaData.Times.Last();
                return (T) Convert.ChangeType(maxDateTime, typeof(T));
            }

            throw new NotSupportedException("Map or His file only contains doubles or datetime values");
        }

        public T GetMinValue<T>(IVariable variable)
        {
            if (typeof(T) == typeof(double) && !variable.IsIndependent)
            {
                double minValue;
                string parameterName = GetParameterName != null ? GetParameterName(variable.Name) : variable.Name;
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

        private bool HasValidFile
        {
            get
            {
                return !string.IsNullOrEmpty(path) && File.Exists(path) && MetaData != null;
            }
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

        private IMultiDimensionalArray GetArgumentValues(IVariable function, IVariableFilter[] filters)
        {
            VariableValueFilter<DateTime> argumentTimeFilter = filters.OfType<VariableValueFilter<DateTime>>().FirstOrDefault();
            if (function.ValueType == typeof(DateTime))
            {
                return new MultiDimensionalArray<DateTime>(argumentTimeFilter == null ? MetaData.Times : argumentTimeFilter.Values);
            }

            if (function.ValueType.Implements(typeof(IFeature)))
            {
                List<IFeature> features = MetaData.Locations.Select(GetObjectForLocationName).OfType<IFeature>().ToList();

                if (filters.Length == 1 && argumentTimeFilter != null)
                {
                    return new MultiDimensionalArray<IFeature>(features, new[]
                    {
                        1,
                        features.Count
                    });
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

        private void UpdateMinMaxWithOneTimeStep(IVariable variable)
        {
            if (MetaData.NumberOfTimeSteps == 0)
            {
                return;
            }

            IVariable dateTimeVariable = variable.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime));
            GetVariableValues(variable, new VariableValueFilter<DateTime>(dateTimeVariable, MetaData.Times[0]));
        }

        private static IMultiDimensionalArray CreateEmptyArrayForType(Type type)
        {
            Type listType = typeof(List<>).MakeGenericType(type);
            Type mda = typeof(MultiDimensionalArray<>).MakeGenericType(type);
            return (IMultiDimensionalArray) Activator.CreateInstance(mda, Activator.CreateInstance(listType));
        }

        private void UpdateMinMax(IEnumerable<double> timeStepData, string name, IVariable function)
        {
            double? min = null;
            double? max = null;

            var noDataValue = (double) (function.NoDataValue ?? 0.0);

            foreach (double value in timeStepData)
            {
                if (Equals(value, noDataValue))
                {
                    continue;
                }

                min = GetMin(min, value);
                max = GetMax(max, value);
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

            if (!minMaxChanged)
            {
                return;
            }

            FireFunctionValuesChanged(this, new FunctionValuesChangingEventArgs {Function = function});
        }

        private static double? GetMax(double? newValue, double currentValue)
        {
            if (newValue == null || newValue.Value < currentValue)
            {
                return currentValue;
            }

            return newValue;
        }

        private static double? GetMin(double? newValue, double currentValue)
        {
            if (newValue == null || newValue.Value > currentValue)
            {
                return currentValue;
            }

            return newValue;
        }

        private void FireFunctionValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            if (FunctionValuesChanged == null)
            {
                return;
            }

            FunctionValuesChanged(sender, e);
        }

        #region Unsupported properties

        public bool SkipChildItemEventBubbling { get; set; }

        public bool SupportsPartialRemove
        {
            get
            {
                return false;
            }
        }

        public IList<ITypeConverter> TypeConverters
        {
            get
            {
                return null;
            }
        }

        public bool DisableCaching { get; set; }

        public bool IsMultiValueFilteringSupported
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region Unsupported functions

        public void SetVariableValues<T>(IVariable variable, IEnumerable<T> values, params IVariableFilter[] filters)
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
    }
};