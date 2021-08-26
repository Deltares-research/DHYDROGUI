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
using DelftTools.Utils.NetCdf;
using GeoAPI.Extensions.Coverages;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    public class FouFileFunctionStore : IFunctionStore, IFileBased
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FouFileFunctionStore));

        private string path;
        private IEventedList<IFunction> functions;
        private FouFileMetaData metaData;

        private readonly IDictionary<string, double> minValues = new Dictionary<string, double>();
        private readonly IDictionary<string, double> maxValues = new Dictionary<string, double>();

        public long Id { get; set; }

        public IEventedList<IFunction> Functions
        {
            get { return functions ?? (functions = new EventedList<IFunction>(CreateFunctions())); }
            set { functions = value; }
        }

        public bool FireEvents { get; set; }

        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                CleanCaches();
            }
        }

        public IEnumerable<string> Paths
        {
            get { return new[] { Path }; }
        }

        public bool IsFileCritical
        {
            get { return false; }
        }

        public bool IsOpen
        {
            get { return false; }
        }

        public bool CopyFromWorkingDirectory { get; } = true;

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
        
        private bool HasValidFile
        {
            get { return !string.IsNullOrEmpty(path) && File.Exists(path) && MetaData != null; }
        }

        private FouFileMetaData MetaData
        {
            get
            {
                if (metaData != null)
                {
                    return metaData;
                }

                try
                {
                    metaData = FouFileReader.ReadMetaData(path);
                }
                catch (Exception e)
                {
                    metaData = null;
                    log.Error($"Could not load {path} because : {e.Message}");
                }
                return metaData;
            }
        }
        
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

        public Type GetEntityType()
        {;
            return GetType();
        }

        public object Clone()
        {
            return new FouFileFunctionStore { Path = Path };
        }
        
        public IMultiDimensionalArray GetVariableValues(IVariable variable, params IVariableFilter[] filters)
        {
            if (!HasValidFile)
            {
                return CreateEmptyArrayForType(variable.ValueType);
            }

            if (variable.IsIndependent && !filters.Any())
            {
                // network locations
                if (variable.ValueType == typeof(INetworkLocation))
                {
                    return new MultiDimensionalArray<INetworkLocation>(MetaData.Mesh1dLocations, MetaData.Mesh1dLocations.Count);
                }

                // cell indices 
                if (variable.ValueType == typeof(int))
                {
                    return new MultiDimensionalArray<int>(Enumerable.Range(0, MetaData.Grid.Cells.Count).ToArray(), MetaData.Grid.Cells.Count);
                }

                return CreateEmptyArrayForType(variable.ValueType);
            }

            // check for unsupported filters
            if (filters.Length >= 2 && filters.OfType<IVariableValueFilter>().Count() != filters.Length)
            {
                throw new NotSupportedException("Invalid set of filters. Either to many filters or not supported filters");
            }

            if (!variable.Attributes.TryGetValue(FouFileReader.ncVariableName, out string variableName))
            {
                return CreateEmptyArrayForType(variable.ValueType);
            }

            var variableValueFilter = filters.OfType<IVariableValueFilter>().FirstOrDefault();
            var indices = variableValueFilter != null
                              ? GetIndicesForFilter(variableValueFilter)
                              : Array.Empty<int>();

            return FouFileReader.DoWithNetCdfFile(MetaData.Path, file =>
            {
                var ncVariable = file.GetVariableByName(variableName);

                var data = GetMultiDimensionalArray<double>(file, ncVariable, indices);
                UpdateMinMax(data, variableName, variable);
                return data;
            });

        }

        public IMultiDimensionalArray<T> GetVariableValues<T>(IVariable variable, params IVariableFilter[] filters)
        {
            return (IMultiDimensionalArray<T>)GetVariableValues(variable, filters);
        }

        public T GetMaxValue<T>(IVariable variable)
        {
            if (!variable.Attributes.TryGetValue(FouFileReader.ncVariableName, out string variableName) || variable.IsIndependent)
            {
                throw new NotSupportedException("Fou file only contains doubles values");
            }

            if (maxValues.TryGetValue(variableName, out double maxValue))
            {
                return (T)Convert.ChangeType(maxValue, typeof(T));
            }

            return default(T);
        }

        public T GetMinValue<T>(IVariable variable)
        {
            if (!variable.Attributes.TryGetValue(FouFileReader.ncVariableName, out string variableName) || variable.IsIndependent)
            {
                throw new NotSupportedException("Fou file only contains doubles values");
            }

            if (minValues.TryGetValue(variableName, out double minValue))
            {
                return (T)Convert.ChangeType(minValue, typeof(T));
            }

            return default(T);
        }

        public void CreateNew(string path)
        {
        }

        public void Close()
        {
            Path = null;
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
            Path = newPath;
        }

        public void Delete()
        {
            // read-only store, editing not supported
        }

        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanged;

        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanging;

        public event NotifyCollectionChangingEventHandler CollectionChanging;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private int[] GetIndicesForFilter(IVariableValueFilter filter)
        {
            var filterVariable = filter.Variable;
            if (!filterVariable.IsIndependent)
            {
                throw new NotSupportedException("Filter variable should be independent");
            }

            if (filterVariable.ValueType == typeof(INetworkLocation))
            {
                var locations = filter.Values.OfType<INetworkLocation>().ToArray();
                return locations.Select(l => MetaData.IndexByLocation[l]).ToArray();
            }

            if (filterVariable.ValueType == typeof(int))
            {
                return filter.Values.OfType<int>().ToArray();
            }

            throw new NotSupportedException("The requested filter is not supported, can not retrieve fou file results");
        }

        private static IMultiDimensionalArray<T> GetMultiDimensionalArray<T>(NetCdfFile file, NetCdfVariable ncVariable, int[] indices)
        {
            var values = new List<T>();
            if (indices.Length == 1)
            {
                values = file.Read(ncVariable, new []{ indices[0]}, new[] { 1 }).OfType<T>().ToList();
            }
            else
            {
                var array = file.Read(ncVariable);

                values = (indices.Any()
                              ? indices.Select(i => array.GetValue(i)).OfType<T>()
                              : array.OfType<T>()).ToList();
            }

            return new MultiDimensionalArray<T>(true, values, values.Count);
        }

        private IEnumerable<IFunction> CreateFunctions()
        {
            if (!HasValidFile) 
                return Enumerable.Empty<IFunction>();

            var allCoverages = FouFileReader.Create1dMeshCoverages(MetaData).OfType<IFunction>()
                                            .Concat(FouFileReader.Create2dMeshCoverages(MetaData))
                                            .ToArray();

            allCoverages.ForEach(c => c.Store = this);

            return allCoverages;
        }

        private static IMultiDimensionalArray CreateEmptyArrayForType(Type type)
        {
            var listType = typeof(List<>).MakeGenericType(type);
            var mda = typeof(MultiDimensionalArray<>).MakeGenericType(type);
            return (IMultiDimensionalArray)Activator.CreateInstance(mda, Activator.CreateInstance(listType));
        }

        private void UpdateMinMax(IEnumerable<double> data, string name, IVariable function)
        {
            // always use cached version (clear cache to reset)
            if (minValues.ContainsKey(name) && maxValues.ContainsKey(name))
            {
                return;
            }

            double? min = null;
            double? max = null;

            double noDataValue = (double)(function.NoDataValue ?? 0.0);

            foreach (var value in data)
            {
                if (Equals(value, noDataValue))
                    continue;

                if (!min.HasValue || min.Value > value)
                {
                    min = value;
                }

                if (!max.HasValue || max.Value < value)
                {
                    max = value;
                }
            }

            if (min.HasValue)
            {
                minValues[name] = min.Value;
            }

            if (max.HasValue)
            {
                maxValues[name] = max.Value;
            }

            FireFunctionValuesChanged(this, new FunctionValuesChangingEventArgs { Function = function });
        }

        private void FireFunctionValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            FunctionValuesChanged?.Invoke(sender, e);
        }

        private void CleanCaches()
        {
            metaData = null;
            functions?.ForEach(f => f.Store = null);
            functions = null;
            minValues.Clear();
            maxValues.Clear();
        }
    }
}