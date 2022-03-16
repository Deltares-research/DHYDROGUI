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
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Function store wrapper for WaterQuality map file. Only supports readonly time-dependent UnstructuredGridCellCoverages
    /// </summary>
    public class LazyMapFileFunctionStore : IFunctionStore
    {
        private readonly IDictionary<string, double> minValues = new Dictionary<string, double>();
        private readonly IDictionary<string, double> maxValues = new Dictionary<string, double>();
        private IEventedList<IFunction> functions = new EventedList<IFunction>();
        private bool isNetCdfFile;
        private MapFileMetaData metaData;
        private string path;

        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanged;

        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanging;

        public event NotifyCollectionChangingEventHandler CollectionChanging;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public string Path
        {
            get => path;
            set
            {
                path = value;
                isNetCdfFile = path?.EndsWith("_map.nc") ?? false;
                metaData = null;
                minValues.Clear();
                maxValues.Clear();
            }
        }

        public long Id { get; set; }

        public IEventedList<IFunction> Functions
        {
            get => functions;
            set => functions = value;
        }

        public bool FireEvents { get; set; }

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

        public Type GetEntityType()
        {
            return GetType();
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Get data from the DelwaqOutputFile by using the function store.
        /// </summary>
        /// <param name="variable"> Determines which parameter is needed from the delwaq output file </param>
        /// <param name="filters"> Determines which data for the parameter is filtered out </param>
        /// <returns>
        /// An IMultiDimensionalArray of double values.
        /// Which is empty in case of:
        /// - File is not valid
        /// - Required parameter is dependent, but location is not an argument
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// In case of:
        /// - Required parameter is called with the wrong/not supported expected type (should be double).
        /// - Required parameter is independent of time.
        /// - A filter contains more than 1 value.
        /// - Required parameter is dependent and there are no filters for time and location given.
        /// - Required parameter is independent and type is not DateTime.
        /// </exception>
        public IMultiDimensionalArray GetVariableValues(IVariable variable, params IVariableFilter[] filters)
        {
            Type type = variable.ValueType;
            if (!HasValidMapFile)
            {
                return CreateEmptyArrayForType(type);
            }

            if (variable.IsIndependent)
            {
                return GetArgumentValues(variable, filters);
            }

            IVariable timeVariable = variable.Arguments.FirstOrDefault(a => a.ValueType == typeof(DateTime));

            if (type != typeof(double) || timeVariable == null ||
                filters.OfType<IVariableValueFilter>().Any(f => f.Values.Count > 1))
            {
                throw new NotImplementedException();
            }

            if (string.IsNullOrEmpty(variable.Name))
            {
                return CreateEmptyArrayForType(type);
            }

            IVariable locationIndexVariable = variable.Arguments.FirstOrDefault(a => a.ValueType == typeof(int));

            List<double> data = null;

            if (locationIndexVariable != null)
            {
                data = GetTimeDataForSpecificLocation(variable, filters);
            }

            if (data == null)
            {
                return new MultiDimentionalArrayAdapter<double>(new List<double>());
            }

            UpdateMinMax(data, variable.Name, (double) (variable.NoDataValue ?? 0.0));
            return new MultiDimentionalArrayAdapter<double>(data);
        }

        public IMultiDimensionalArray<T> GetVariableValues<T>(IVariable variable, params IVariableFilter[] filters)
        {
            return (IMultiDimensionalArray<T>) GetVariableValues(variable, filters);
        }

        public T GetMaxValue<T>(IVariable variable)
        {
            if (typeof(T) == typeof(double))
            {
                double maxValue = maxValues.ContainsKey(variable.Name) ? maxValues[variable.Name] : 0D;
                return (T) Convert.ChangeType(maxValue, typeof(T));
            }

            if (typeof(T) == typeof(DateTime))
            {
                DateTime maxDateTime = MetaData.Times.Count == 0 ? MetaData.Times[0] : MetaData.Times.Last();
                return (T) Convert.ChangeType(maxDateTime, typeof(T));
            }

            throw new NotSupportedException("Map file only contains doubles or datetime values");
        }

        public T GetMinValue<T>(IVariable variable)
        {
            if (typeof(T) == typeof(double))
            {
                double minValue = minValues.ContainsKey(variable.Name) ? minValues[variable.Name] : 0D;
                return (T) Convert.ChangeType(minValue, typeof(T));
            }

            if (typeof(T) == typeof(DateTime))
            {
                return (T) Convert.ChangeType(MetaData.Times[0], typeof(T));
            }

            throw new NotSupportedException("Map file only contains doubles or datetime values");
        }

        private bool HasValidMapFile => !string.IsNullOrEmpty(path) && MetaData != null;

        private MapFileMetaData MetaData => metaData ?? (metaData = isNetCdfFile
                                                                        ? DelwaqNetCdfMapFileReader.ReadMetaData(path)
                                                                        : DelwaqMapFileReader.ReadMetaData(path));

        private MultiDimentionalArrayAdapter<DateTime> GetArgumentValues(IVariable function, IVariableFilter[] filters)
        {
            VariableValueFilter<DateTime> argumentTimeFilter =
                filters.OfType<VariableValueFilter<DateTime>>().FirstOrDefault();
            if (function.ValueType == typeof(DateTime))
            {
                return new MultiDimentionalArrayAdapter<DateTime>(argumentTimeFilter == null
                                                                      ? MetaData.Times
                                                                      : argumentTimeFilter.Values);
            }

            throw new NotSupportedException(
                string.Format(Resources.LazyMapFileFunctionStore_GetArgumentValues_Filters_of_type___0___can_only_filter_on_functions_with_value_type___1___, typeof(VariableValueFilter<DateTime>), typeof(DateTime)));
        }

        private List<double> GetTimeDataForSpecificLocation(IVariable function, IVariableFilter[] filters)
        {
            VariableValueFilter<int> locationFilter = filters.OfType<VariableValueFilter<int>>().FirstOrDefault();
            VariableValueFilter<DateTime> timeFilter = filters.OfType<VariableValueFilter<DateTime>>()
                                                              .FirstOrDefault();

            if (locationFilter == null && timeFilter == null)
            {
                return new List<double>();
            }

            int locationIndex = locationFilter != null ? locationFilter.Values[0] : -1;
            int timeIndex = timeFilter != null ? MetaData.Times.IndexOf(timeFilter.Values[0]) : -1;

            List<double> data = timeIndex != -1
                                    ? GetTimeStepData(function, timeIndex, locationIndex)
                                    : GetTimeSeriesData(function, locationIndex);

            return data;
        }

        private List<double> GetTimeSeriesData(IVariable function, int locationIndex)
        {
            return isNetCdfFile
                       ? DelwaqNetCdfMapFileReader.GetTimeSeriesData(path, MetaData, function.Name, locationIndex)
                       : DelwaqMapFileReader.GetTimeSeriesData(path, MetaData, function.Name, locationIndex);
        }

        private List<double> GetTimeStepData(IVariable function, int timeIndex, int locationIndex)
        {
            return isNetCdfFile
                       ? DelwaqNetCdfMapFileReader.GetTimeStepData(path, MetaData, timeIndex, function.Name, locationIndex)
                       : DelwaqMapFileReader.GetTimeStepData(path, MetaData, timeIndex, function.Name, locationIndex);
        }

        private static IMultiDimensionalArray CreateEmptyArrayForType(Type type)
        {
            Type listType = typeof(List<>).MakeGenericType(type);
            Type mda = typeof(MultiDimentionalArrayAdapter<>).MakeGenericType(type);
            return (IMultiDimensionalArray) Activator.CreateInstance(mda, Activator.CreateInstance(listType));
        }

        private void UpdateMinMax(IEnumerable<double> timeStepData, string name, double noDataValue)
        {
            double? min = null;
            double? max = null;

            foreach (double value in timeStepData)
            {
                if (Equals(value, noDataValue))
                {
                    continue;
                }

                if (min == null || min.Value > value)
                {
                    min = value;
                }

                if (max == null || max.Value < value)
                {
                    max = value;
                }
            }

            if (min != null && (!minValues.ContainsKey(name) || minValues[name] > min.Value))
            {
                minValues[name] = min.Value;
                FireFunctionValuesChanged(null, new FunctionValuesChangingEventArgs());
            }

            if (max != null && (!maxValues.ContainsKey(name) || maxValues[name] < max.Value))
            {
                maxValues[name] = max.Value;
                FireFunctionValuesChanged(null, new FunctionValuesChangingEventArgs());
            }
        }

        private void FireFunctionValuesChanged(object sender, FunctionValuesChangingEventArgs e) => 
            FunctionValuesChanged?.Invoke(sender, e);

        #region Unsupported properties

        public bool SkipChildItemEventBubbling { get; set; }

        public bool SupportsPartialRemove => false;

        public IList<ITypeConverter> TypeConverters => new List<ITypeConverter>();

        public bool DisableCaching { get; set; }

        public bool IsMultiValueFilteringSupported => true;

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
}