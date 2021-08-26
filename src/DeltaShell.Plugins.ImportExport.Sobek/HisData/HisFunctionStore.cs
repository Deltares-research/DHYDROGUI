using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.ImportExport.Sobek.HisData
{
    /// <summary>
    /// Function Store based on (Sobek) His files
    /// Readonly store
    /// </summary>
    public class HisFunctionStore : Unique<long>, IFunctionStore, IDisposable, IFileBased
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HisFunctionStore));

        private bool isOpen;
        private string path;
        private HisFileReader hisFileReader;

        private const string SobekNetworkFileName = "NETWORK.TP";
        private const string SobekNetworkBrancheGeometryFileName = "NETWORK.CP";
        private const string SobekNetworkGridFileName = "NETWORK.GR";
        private string pathSobekNetwork;
        private string pathSobekNetworkGeometry;
        private string pathSobekGridPoints;
        private bool IsPossibleToMakeNetworkCoverage;
        private IHydroNetwork hydroNetwork;
        private NetworkLocationTypeConvertor networkLocationTypeConvertor;
        private NetworkLocationTypeConvertorOption networkLocationTypeConvertorOption;

        private string argumentLocationsName = "locations";
        private string argumentTimeStepName = "time";
        private bool disposed;

        private readonly Dictionary<IVariable, DelftTools.Utils.Tuple<object, object>> minMaxValues = new Dictionary<IVariable, DelftTools.Utils.Tuple<object, object>>();

        public HisFunctionStore():this(null)
        {
        }

        public HisFunctionStore(string path, NetworkLocationTypeConvertorOption networkLocationTypeConvertorOption = NetworkLocationTypeConvertorOption.CalculationalPoints)
        {
            this.networkLocationTypeConvertorOption = networkLocationTypeConvertorOption;
            Functions = new EventedList<IFunction>();
            if (path != null)
            {
                Open(path);    
            }
        }

        public override string ToString()
        {
            return "His function store";
        }

        #region private

        private void InitArgumentsAndComponents()
        {
            Functions.Clear();
            if (IsPossibleToMakeNetworkCoverage)
            {
                InitArgumentsAndComponentsForNetworkCoverage();
            }
            else
            {
                InitArgumentsAndComponentsForFunction();
            }
        }

        private void InitArgumentsAndComponentsForFunction()
        {
            networkLocationTypeConvertor = null;

            var hisFileHeader = hisFileReader.GetHisFileHeader;

            var variableTime = new Variable<DateTime>(argumentTimeStepName)
            {
                Store = this
            };
            Functions.Add(variableTime);

            var variableLocations = new Variable<string>(argumentLocationsName)
            {
                Store = this
            };
            Functions.Add(variableLocations);

            var components = new EventedList<IVariable>();

            foreach (var component in hisFileHeader.Components)
            {
                var variable = new Variable<double>(component)
                {
                    Store = this,
                    Arguments = { variableTime, variableLocations }

                };
                components.Add(variable);
                Functions.Add(variable);
            }

            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            Functions.Add(new Function(fileName + " As Function")
            {
                Arguments = { variableTime, variableLocations },
                Components = components,
                IsEditable = false,
                Store = this
            });
  
        }

        private void InitArgumentsAndComponentsForNetworkCoverage()
        {
            var hisFileHeader = hisFileReader.GetHisFileHeader;

            IVariable variableTime = new Variable<DateTime>(argumentTimeStepName)
            {
                Store = this
            };
            Functions.Add(variableTime);

            SetConvertorAndNetwork();

            IVariable variableLocations = new Variable<INetworkLocation>(argumentLocationsName)
            {
                Store = this
            };

            Functions.Add(variableLocations);

            var components = new EventedList<IVariable>();

            foreach (var component in hisFileHeader.Components)
            {
                var variable = new Variable<double>(component)
                {
                    Store = this,
                    Arguments = { variableTime, variableLocations }

                };
                components.Add(variable);
                Functions.Add(variable);
            }

            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            var networkCoverage = new NetworkCoverage
            {
                Name = fileName + " As NetworkCoverage",
                Arguments = new EventedList<IVariable>(new[] { variableTime, variableLocations }),
                Components = components,
                Network = hydroNetwork,
                IsTimeDependent = true,
                IsEditable = false,
                Store = this
            };

            Functions.Add(networkCoverage);

        }

        private void SetConvertorAndNetwork()
        {
            if(string.IsNullOrEmpty(pathSobekNetwork))
            {
                return;
            }

            //import network
            hydroNetwork = new HydroNetwork();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(
                pathSobekNetwork,
                hydroNetwork,
                new IPartialSobekImporter[]
                    {
                        new SobekBranchesImporter(),
                        new SobekStructuresImporter()
                    });
            importer.Import();

            networkLocationTypeConvertor = new NetworkLocationTypeConvertor();

            IList<CalcGrid> calcGrids = new SobekGridPointsReader().Read(pathSobekGridPoints).ToList();

            foreach (CalcGrid grid in calcGrids)
            {
                var branch = hydroNetwork.Channels.First(b => b.Name == grid.BranchID);
                var structureOffsets = branch.Structures.Where(s => s is ICompositeBranchStructure).Select(s => s.Chainage).ToList();
                
                //remove structure gridpoints
                IList<SobekCalcGridPoint> gridPointsNoStructures = new List<SobekCalcGridPoint>();
                foreach (var gridPoint in grid.GridPoints)
                {
                    if(!structureOffsets.Any(so => Math.Abs(so - gridPoint.Offset) < BranchFeature.Epsilon))
                    {
                        gridPointsNoStructures.Add(gridPoint);
                    }
                }

                foreach (var gridPoint in gridPointsNoStructures)
                {
                    var offset = gridPoint.Offset;

                    if (networkLocationTypeConvertorOption == NetworkLocationTypeConvertorOption.Segments)
                    {
                        var index = gridPointsNoStructures.IndexOf(gridPoint);
                        double segmentLength = 0;
                        if (index < gridPointsNoStructures.Count - 1)
                        {
                            segmentLength = ((gridPointsNoStructures[index + 1].Offset - offset) / 2);
                        }
                        else
                        {
                            segmentLength = ((branch.Length - offset) / 2);
                        }
                        offset += segmentLength;
                    }

                    if (networkLocationTypeConvertorOption == NetworkLocationTypeConvertorOption.Segments)
                    {
                        if (gridPoint.SegmentId != "") //last point of branch, SegmentId == ""
                        {
                            networkLocationTypeConvertor.AddItem(gridPoint.SegmentId, new NetworkLocation(branch, offset));
                        }
                    }
                    else
                    {
                        if (gridPoint.Id != "")
                        {
                            networkLocationTypeConvertor.AddItem(gridPoint.Id, new NetworkLocation(branch, offset));
                        }
                    }
                }
            }
        }

        private void TryToFindSobekNetworkAndGridPoints()
        {
            var dir = System.IO.Path.GetDirectoryName(Path);
            var pathNetwork = System.IO.Path.Combine(dir, SobekNetworkFileName);
            var pathNetworkGeometry = System.IO.Path.Combine(dir, SobekNetworkBrancheGeometryFileName);
            var pathGridPoints = System.IO.Path.Combine(dir, SobekNetworkGridFileName);

            pathSobekNetwork = null;
            pathSobekNetworkGeometry = null;
            pathSobekGridPoints = null;
            IsPossibleToMakeNetworkCoverage = false;

            if (File.Exists(pathNetwork) && File.Exists(pathGridPoints) && File.Exists(pathNetworkGeometry))
            {
                pathSobekNetwork = pathNetwork;
                pathSobekNetworkGeometry = pathNetworkGeometry;
                pathSobekGridPoints = pathGridPoints;
                IsPossibleToMakeNetworkCoverage = true;
            }
        }

        #endregion

        #region IFileBased Members

        public string Path
        {
            get { return path; } 
            set
            {
                path = value;
                TryToFindSobekNetworkAndGridPoints();
            }
        }

        public void CreateNew(string path)
        {
            throw new NotSupportedException("Readonly Store");
        }

        public void Close()
        {
            hisFileReader.Close();
            isOpen = false;
        }

        public void Open(string path)
        {
            Path = path;

            if(isOpen)
            {
                Close();
            }
            if(Path != null)
            {
                hisFileReader = new HisFileReader(path);
                if(Functions == null || Functions.Count == 0)
                {
                    InitArgumentsAndComponents();
                }
                isOpen = true;
            }
        }

        public bool IsFileCritical { get { return true; } }

        public bool IsOpen
        {
            get { return isOpen; }
        }

        public bool CopyFromWorkingDirectory { get; } = false;

        public void SwitchTo(string newPath)
        {
            path = newPath;
            Open(path);
        }

        public void CopyTo(string destinationPath)
        {
            File.Copy(path, destinationPath);

            if (IsPossibleToMakeNetworkCoverage)
            {
                var newDir = System.IO.Path.GetDirectoryName(destinationPath);
                File.Copy(pathSobekNetwork, System.IO.Path.Combine(newDir, SobekNetworkFileName));
                File.Copy(pathSobekNetworkGeometry, System.IO.Path.Combine(newDir, SobekNetworkBrancheGeometryFileName));
                File.Copy(pathSobekGridPoints, System.IO.Path.Combine(newDir, SobekNetworkGridFileName));

            }
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Paths
        {
            get { yield break; }
        }

        #endregion

        #region IFunctionStore members
        
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event NotifyCollectionChangingEventHandler CollectionChanging;

        public bool SkipChildItemEventBubbling
        {
            get;
            set;
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        private IEventedList<IFunction> functions;

        public IEventedList<IFunction> Functions
        {
            get { return functions; }
            set { functions = value; }
        }

        public void SetVariableValues<T>(IVariable variable, IEnumerable<T> values, params IVariableFilter[] filters)
        {
            throw new NotSupportedException("Readonly Store");
        }

        public void AddIndependentVariableValues<T>(IVariable variable, IEnumerable<T> values)
        {
            throw new NotSupportedException("Readonly Store");
        }

        public IMultiDimensionalArray GetVariableValues(IVariable variable, params IVariableFilter[] filters)
        {
            if (variable.IsIndependent)
            {
                return GetIndependedVariables(filters, variable);               
            }
            return GetDependedVariables(filters, variable);
        }

        private IMultiDimensionalArray GetIndependedVariables(IVariableFilter[] filters, IVariable variable)
        {
            if(networkLocationTypeConvertor == null)
            {
                SetConvertorAndNetwork();
            }
            var hisFileHeader = hisFileReader.GetHisFileHeader;

            if(variable.ValueType == typeof(DateTime))
            {
                var shape = new[] { 1,hisFileHeader.TimeSteps.Count};
                return new MultiDimensionalArray<DateTime>(hisFileHeader.TimeSteps, shape);
            }

            if (variable.ValueType == typeof(INetworkLocation))
            {
                var shape = new[] { 1, hisFileHeader.Locations.Count };
                var networkLocations = hisFileHeader.Locations.Select(locationName => networkLocationTypeConvertor.ConvertFromStore(locationName));
                return new MultiDimensionalArray<INetworkLocation>(networkLocations.OrderBy(l => l).ToArray(), shape); 
            }

            if (variable.ValueType == typeof(string))
            {
                var shape = new[] { 1, hisFileHeader.Locations.Count };
                return new MultiDimensionalArray<string>(hisFileHeader.Locations, shape);
            }

            throw new ArgumentOutOfRangeException("Variable", @"Variable of type DateTime,string,NetworkLoaction is expected.");
        }

        private IMultiDimensionalArray GetDependedVariables(IVariableFilter[] filters, IVariable variable)
        {
            var hisFileHeader = hisFileReader.GetHisFileHeader;

            var argumentFilters = filters.Where(f => f.Variable.IsIndependent).ToArray();


            if (argumentFilters.Length == 0)
            {
                var rows = hisFileReader.ReadAllData(variable.Name);
                var values = GetValuesOrderByNetworkLocations(rows);
                var shape = new[] {hisFileHeader.TimeSteps.Count, hisFileHeader.Locations.Count};

                return new MultiDimensionalArray<double>(values, shape);
            }

            if (argumentFilters.Length >= 1)
            {

                foreach (var argumentFilter in argumentFilters)
                {
                    switch (argumentFilter)
                    {
                        //timestep
                        case VariableValueFilter<DateTime> valueFilter:
                        {
                            var rows = hisFileReader.ReadTimeStep(valueFilter.Values[0], variable.Name);
                            var values = GetValuesOrderByNetworkLocations(rows);
                            var shape = new[] {1, hisFileHeader.Locations.Count};

                            return new MultiDimensionalArray<double>(values, shape);
                        }
                        //location by name
                        case VariableValueFilter<string> filter:
                        {
                            var rows = hisFileReader.ReadLocation(filter.Values[0], variable.Name);
                            var values = rows.Select(r => r.Value).ToArray();
                            var shape = new[] {1, hisFileHeader.TimeSteps.Count};

                            return new MultiDimensionalArray<double>(values, shape);
                        }
                        //location by NetworkLocation
                        case VariableValueFilter<INetworkLocation> filter:
                        {
                            var locationName = networkLocationTypeConvertor.ConvertToStore(filter.Values[0]).First().ToString();
                            var rows = hisFileReader.ReadLocation(locationName, variable.Name);
                            var values = GetValuesOrderByNetworkLocations(rows);
                            var shape = new[] {1, hisFileHeader.TimeSteps.Count};

                            return new MultiDimensionalArray<double>(values, shape);
                        }
                        default:
                            Log.ErrorFormat("Only one filter of type VariableValueFilter<DateTime/string/NetworkLoaction> is supporterd yet. Filter {0} has been skipped", argumentFilter.GetType().FullName);
                            break;
                    }
                }
            }

            throw new NotSupportedException("Only filters of type VariableValueFilter<DateTime/string/NetworkLoaction> are supporterd yet.");
        }

        private double[] GetValuesOrderByNetworkLocations(List<HisFileReader.HisDataRow> rows)
        {
            if (networkLocationTypeConvertor == null)
            {
                return rows.Select(r => r.Value).ToArray();
            }

            //Add NetworkLocations for ordering
            rows.ForEach(r => r.NetworkLocation = networkLocationTypeConvertor.ConvertFromStore(r.LocationName));
            return rows.OrderBy(r => r.TimeStep).ThenBy(r => r.NetworkLocation).Select(r => r.Value).ToArray();
        }

        public IMultiDimensionalArray<T> GetVariableValues<T>(IVariable variable, params IVariableFilter[] filters)
        {
            return (IMultiDimensionalArray<T>)GetVariableValues(variable, filters);
        }

        public void RemoveFunctionValues(IFunction function, params IVariableValueFilter[] filters)
        {
            throw new NotSupportedException("Readonly Store");
        }

        public bool SupportsPartialRemove { get { return false; } }

        public event EventHandler<FunctionValuesChangingEventArgs> BeforeFunctionValuesChanged;
        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanged;
        public event EventHandler<FunctionValuesChangingEventArgs> FunctionValuesChanging;

        public IList<ITypeConverter> TypeConverters
        {
            get;
            set;
        }

        public bool FireEvents
        {
            get; set;
        }

        public void UpdateVariableSize(IVariable variable)
        {
            throw new NotSupportedException("Resizing of variables is not supported");
        }

        public T GetMaxValue<T>(IVariable variable)
        {
            UpdateMinMaxIfNeeded(variable);
            return (T)Convert.ChangeType(minMaxValues[variable].Second, typeof(T));
        }

        public T GetMinValue<T>(IVariable variable)
        {
            UpdateMinMaxIfNeeded(variable);
            return (T)Convert.ChangeType(minMaxValues[variable].First, typeof(T));
        }

        public void CacheVariable(IVariable variable)
        {
        }

        public virtual bool DisableCaching { get; set; }

        public bool IsMultiValueFilteringSupported { get { return true; } }

        private void UpdateMinMaxIfNeeded(IVariable variable)
        {
            if (minMaxValues.ContainsKey(variable) || 
                variable.ValueType != typeof(double)) return;

            //double is expected -> values for networkcoverage
            var min = double.MaxValue;
            var max = double.MinValue;
            var hisDataRows = hisFileReader.ReadAllData(variable.Name);
            foreach (var hisDataRow in hisDataRows)
            {
                min = Math.Min(min, hisDataRow.Value);
                max = Math.Max(max, hisDataRow.Value);
            }
            minMaxValues[variable] = new DelftTools.Utils.Tuple<object,object>(min,max);
        }

        #endregion

        #region IDispose Members

        /// <summary>
        /// See <see cref="System.IDisposable.Dispose"/> for more information.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Called when the object is being disposed or finalized.
        /// </summary>
        /// <param name="disposing">True when the object is being disposed (and therefore can
        /// access managed members); false when the object is being finalized without first
        /// having been disposed (and therefore can only touch unmanaged members).</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }

            disposed = true;
        }

        #endregion

        public enum NetworkLocationTypeConvertorOption
        {
            CalculationalPoints,
            Segments
        }
    }
}
