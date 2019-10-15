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

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Function store wrapper for waterquality map file. Only supports readonly timedependend UnstructuredGridCellCoverages
    /// </summary>
    public class LazyMapFileFunctionStore : IFunctionStore, IFileBased
    {
        private IEventedList<IFunction> functions = new EventedList<IFunction>();
        private MapFileMetaData metaData;
        private readonly IDictionary<string, double> minValues = new Dictionary<string, double>();
        private readonly IDictionary<string, double> maxValues = new Dictionary<string, double>();
        private string path;

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

                minValues.Clear();
                maxValues.Clear();
            }
        }

        public IEnumerable<string> Paths { get { return new[] {Path}; } }

        public bool IsFileCritical { get { return false; } }

        public bool IsOpen { get { return false; }}

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

        public bool IsMultiValueFilteringSupported { get { return true; } }

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
            return MemberwiseClone();
        }

        public IMultiDimensionalArray GetVariableValues(IVariable function, params IVariableFilter[] filters)
        {
            var type = function.ValueType;
            if (!HasValidMapFile || !IsUnstructuredGridCellCoverageComponent(function))
                return CreateEmptyArrayForType(type);

            var dateTimeFilter = filters.OfType<VariableValueFilter<DateTime>>().FirstOrDefault();
            var cellFilter = filters.OfType<VariableValueFilter<int>>().FirstOrDefault();

            if (dateTimeFilter == null || dateTimeFilter.Values.Count != 1)
                return null;

            var timeIndex = MetaData.Times.IndexOf(dateTimeFilter.Values[0]);
            if (timeIndex == -1) return null;

            var segmentIndex = cellFilter != null ? cellFilter.Values[0] : -1;
            var timeStepData = DelwaqMapFileReader.GetTimeStepData(path, MetaData, timeIndex, function.Name, segmentIndex);

            UpdateMinMax(timeStepData, function.Name, (double) (function.NoDataValue ?? 0.0));

            return new MultiDimentionalArrayAdapter<double>(timeStepData);
        }

        public IMultiDimensionalArray<T> GetVariableValues<T>(IVariable function, params IVariableFilter[] filters)
        {
            if (!HasValidMapFile)
            {
                return new MultiDimentionalArrayAdapter<T>(new T[]{});
            }

            if (typeof(T) == typeof(DateTime) && function.IsIndependent && !filters.Any())
            {
                return (IMultiDimensionalArray<T>) new MultiDimentionalArrayAdapter<DateTime>(MetaData.Times);
            }

            var variableValueFilters = filters.OfType<VariableValueFilter<int>>().ToList();
            if (typeof(T) == typeof(double) && !function.IsIndependent && variableValueFilters.Count == 1)
            {
                var segmentIndex = variableValueFilters[0].Values[0];
                var values = DelwaqMapFileReader.GetTimeSeriesData(path, MetaData, function.Name, segmentIndex);

                return (IMultiDimensionalArray<T>)new MultiDimentionalArrayAdapter<double>(values);
            }

            throw new NotImplementedException();
        }

        public T GetMaxValue<T>(IVariable variable)
        {
            if (typeof (T) == typeof (double))
            {
                var maxValue = maxValues.ContainsKey(variable.Name) ? maxValues[variable.Name] :double.MaxValue;
                return (T)Convert.ChangeType(maxValue, typeof(T));
            }

            if (typeof(T) == typeof(DateTime))
            {
                var maxDateTime = MetaData.Times.Count == 0 ? MetaData.Times[0] : MetaData.Times.Last();
                return (T)Convert.ChangeType(maxDateTime, typeof(T));
            }

            throw new NotSupportedException("Map file only contains doubles or datetime values");
        }

        public T GetMinValue<T>(IVariable variable)
        {
            if (typeof(T) == typeof(double))
            {
                var minValue = minValues.ContainsKey(variable.Name) ? minValues[variable.Name] : double.MinValue;
                return (T)Convert.ChangeType(minValue, typeof(T));
            }

            if (typeof(T) == typeof(DateTime))
            {
                return (T)Convert.ChangeType(MetaData.Times[0], typeof(T));
            }

            throw new NotSupportedException("Map file only contains doubles or datetime values");
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
            FileUtils.DeleteIfExists(path);
        }

        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanged;

        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanging;

        public event NotifyCollectionChangingEventHandler CollectionChanging;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private bool HasValidMapFile
        {
            get { return !string.IsNullOrEmpty(path) && MetaData != null; }
        }

        private MapFileMetaData MetaData
        {
            get { return metaData ?? DelwaqMapFileReader.ReadMetaData(path); }
        }

        private static IMultiDimensionalArray CreateEmptyArrayForType(Type type)
        {
            var listType = typeof(List<>).MakeGenericType(type);
            var mda = typeof (MultiDimentionalArrayAdapter<>).MakeGenericType(type);
            return (IMultiDimensionalArray) Activator.CreateInstance(mda, Activator.CreateInstance(listType));
        }

        private void UpdateMinMax(IEnumerable<double> timeStepData, string name, double noDataValue)
        {
            double? min = null;
            double? max = null;

            foreach (var value in timeStepData)
            {
                if (Equals(value, noDataValue)) continue;

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
                FireFunctionValuesChanged(null,new FunctionValuesChangingEventArgs());
            }

            if (max != null && (!maxValues.ContainsKey(name) || maxValues[name] < max.Value))
            {
                maxValues[name] = max.Value;
                FireFunctionValuesChanged(null, new FunctionValuesChangingEventArgs());
            }
        }

        private bool IsUnstructuredGridCellCoverageComponent(IVariable function)
        {
            return MetaData.Substances.Contains(function.Name) &&
                   function.ValueType == typeof (double) &&
                   function.Arguments.Count == 2 &&
                   function.Arguments[0].Name == "datetime" &&
                   function.Arguments[0].ValueType == typeof (DateTime) &&
                   function.Arguments[1].Name == "cell_index" &&
                   function.Arguments[1].ValueType == typeof (int);
        }

        private void FireCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (CollectionChanging == null) return;
            CollectionChanging(sender, e);
        }

        private void FireCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged == null) return;
            CollectionChanged(sender, e);
        }

        private void FireFunctionValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            if (FunctionValuesChanged == null) return;
            FunctionValuesChanged(sender, e);
        }

        private void FireFunctionValuesChanging(object sender, FunctionValuesChangingEventArgs e)
        {
            if (FunctionValuesChanging == null) return;
            FunctionValuesChanging(sender, e);
        }
    }
}