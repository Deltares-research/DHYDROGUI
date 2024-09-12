using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Units;
using DelftTools.Utils.NetCdf;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.Logging;
using DeltaShell.NGHS.IO.FileWriters.Network;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    /// <summary>
    /// Function store for Class Map Files.
    /// </summary>
    /// <seealso cref="FMClassMapFileFunctionStore"/>
    public class FMClassMapFileFunctionStore : FMNetCdfFileFunctionStore
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FMClassMapFileFunctionStore));
        
        private OutputFile1DMetaData metaData;
        private int sobekStartIndex = 0;//1; euuh is dit aangepast? // minus one because fortran is 1 based...


        private const string LocationAttributeName = "location";
        private const string StandardNameAttributeName = "standard_name";
        private const string LongNameAttributeName = "long_name";
        private const string UnitsAttributeName = "units";
        private const string FillValueAttribute = "_FillValue";

        private static readonly ILog Log = LogManager.GetLogger(typeof(FMClassMapFileFunctionStore));

        private readonly IDictionary<string, IList<INetworkLocation>> locationsByNetworkDataType = new Dictionary<string, IList<INetworkLocation>>();
        private readonly Dictionary<IVariable, IMultiDimensionalArray> networkLocationsForThisFunctionCache = new Dictionary<IVariable, IMultiDimensionalArray>();
        private readonly Dictionary<IVariable, IMultiDimensionalArray> argumentVariableCache = new Dictionary<IVariable, IMultiDimensionalArray>();
        private readonly Dictionary<IVariable, IMultiDimensionalArray> fullVariableCache = new Dictionary<IVariable, IMultiDimensionalArray>();

        private UnstructuredGrid grid;
        private IHydroNetwork network = new HydroNetwork();
        private IDiscretization discretization = new Discretization();
        private IList<ILink1D2D> links;

        /// <summary>
        /// Initializes a new instance of the <see cref="FMClassMapFileFunctionStore"/> class.
        /// </summary>
        /// <param name="classMapFilePath"> The class map file path. </param>
        public FMClassMapFileFunctionStore(string classMapFilePath) : base(classMapFilePath) { }

        private FmMapFile1DOutputFileReader OutputFileReader { get; } = new FmMapFile1DOutputFileReader();

        /// <summary>
        /// Gets the grid.
        /// </summary>
        /// <value>
        /// The output grid.
        /// </value>
        public UnstructuredGrid Grid => grid;

        /// <summary>
        /// Gets the discretization used by the kernel
        /// </summary>
        /// <value>
        /// The output discretization.
        /// </value>
        public IDiscretization Discretization => discretization;

        /// <summary>
        /// Gets the network geometry used by the kernel
        /// </summary>
        /// <value>
        /// The output network.
        /// </value>
        public IHydroNetwork Network => network;

        /// <summary>
        /// Gets the 1d2d links used by the kernel
        /// </summary>
        /// <value>
        /// The output 1d2d links.
        /// </value>
        public IList<ILink1D2D> Links => links;

        /// <summary>
        /// Constructs the functions for netCdf variables that are time dependent.
        /// </summary>
        /// <param name="dataVariables"> The data variables. </param>
        /// <returns> </returns>
        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            if (!ValidateTimes())
            {
                return Array.Empty<IFunction>();
            }
            grid = new UnstructuredGrid();
            network = new HydroNetwork();
            discretization = new Discretization { Network = Network };
            links = new List<ILink1D2D>();

            discretization.Locations.IsAutoSorted = false;
            IEnumerable<CompartmentProperties> compartmentProperties = NetworkPropertiesHelper.ReadPropertiesPerNodeFromFile(netCdfFile.Path);
            IEnumerable<BranchProperties> branchProperties = NetworkPropertiesHelper.ReadPropertiesPerBranchFromFile(netCdfFile.Path);
            IConvertedUgridFileObjects convertedUgridFileObjects = new ConvertedUgridFileObjects
            {
                Discretization = discretization,
                Grid = grid,
                HydroNetwork = network,
                Links1D2D = links,
                CompartmentProperties = compartmentProperties,
                BranchProperties = branchProperties
            };

            var logHandler = new LogHandler(string.Format(Resources.ConstructFunctions_Reading_file_output_into_our_FM_model, "fm class map", netCdfFile.Path), Log);
            using (var ugridFile = new UGridFile(netCdfFile.Path))
            {
                ugridFile.ReadNetFileDataIntoModel(convertedUgridFileObjects, loadFlowLinksAndCells: true, recreateCells: false, forceCustomLengths: true, logHandler: logHandler, reportProgress: null);
            }
            logHandler.LogReport();

            foreach (var hydroObject in network.AllHydroObjects)
            {
                hydroObject.Name += "_class_output";
            }

            foreach (var outputNetworkLocation in discretization.Locations.AllValues)
            {
                outputNetworkLocation.Name += "_class_output";
            }

            IEnumerable<NetCdfVariableInfo> timeDepVariables = dataVariables.Where(v => v.IsTimeDependent && v.NumDimensions > 1);
            IEnumerable<ICoverage> functions = timeDepVariables.Select(CreateCoverageForTimeDependentVariable).Where(c => c != null);
            return functions;
        }

        /// <summary>
        /// Gets the variable values.
        /// </summary>
        /// <typeparam name="T"> </typeparam>
        /// <param name="function"> The function. </param>
        /// <param name="filters"> The variable filters. </param>
        /// <returns> </returns>
        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(IVariable function, IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] != "false")
            {
                return base.GetVariableValuesCore<T>(function, filters);
            }

            if (!function.IsIndependent || typeof(T) != typeof(INetworkLocation))
            {
                int size = GetSize(function);
                return new MultiDimensionalArray<T>(Enumerable.Range(0, size).Cast<T>().ToList(), new [] { size });
            }

            var featureFilter = filters.FirstOrDefault(f => f.Variable.ValueType == typeof(INetworkLocation));
            if (function.Attributes == null || !function.Attributes.ContainsKey(NcNameAttribute))
            {
                return base.GetVariableValuesCore<T>(function, filters);
            }

            var location = function.Attributes[NcNameAttribute];
            var networkLocations = locationsByNetworkDataType[location];
            if (filters.Length == 0 || featureFilter == null)
            {
                if (!networkLocationsForThisFunctionCache.TryGetValue(function, out var networkLocationsForThisFunction))
                {
                    networkLocationsForThisFunction = new MultiDimensionalArray<T>((IList<T>)networkLocations);
                    networkLocationsForThisFunctionCache[function] = networkLocationsForThisFunction;
                }
                return (MultiDimensionalArray<T>)networkLocationsForThisFunction;
            }

            if (featureFilter is VariableIndexFilter indexFilter)
            {
                if (!networkLocationsForThisFunctionCache.TryGetValue(function, out var networkLocationsForThisFunction))
                {
                    networkLocationsForThisFunction = new MultiDimensionalArray<T>((IList<T>)networkLocations);
                    networkLocationsForThisFunctionCache[function] = networkLocationsForThisFunction;
                }
                // ik weet niet helemaal zeker of dit nou moet... maar hier zit volgens mij de conversie naar de output
                var indexesOfLocationsInOutput = indexFilter.Indices.Select(i => locationsByNetworkDataType[location].IndexOf(networkLocations[i])).ToArray();
                return new MultiDimensionalArray<T>((IList<T>)((MultiDimensionalArray<T>)networkLocationsForThisFunction).Select(1, indexesOfLocationsInOutput), new[] { MetaData?.Times.Count ?? 1, indexesOfLocationsInOutput.Length });
            }

            return new MultiDimensionalArray<T>();
        }

        private OutputFile1DMetaData MetaData
        {
            get
            {
                if (metaData != null) return metaData;

                if (!File.Exists(Path)) return new OutputFile1DMetaData();

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

        private ICoverage CreateCoverageForTimeDependentVariable(NetCdfVariableInfo timeDependentVariable)
        {
            NetCdfVariable netCdfVariable = timeDependentVariable.NetCdfDataVariable;
            string netCdfVariableName = netCdfFile.GetVariableName(netCdfVariable);
            NetCdfDataType netCdfVariableType = netCdfFile.GetVariableDataType(netCdfVariable);

            if (netCdfVariableType != NetCdfDataType.NcByte)
            {
                Log.Warn(
                    $"Time dependent functions in the class map file are expected to be of type Byte. Please check the value type for variable '{netCdfVariableName}'.");
                return null;
            }

            NetCdfDimension secondDimension = netCdfFile.GetDimensions(netCdfVariable).ElementAt(1);
            string secondDimensionName = netCdfFile.GetDimensionName(secondDimension);
            string location = netCdfFile.GetAttributeValue(netCdfVariable, LocationAttributeName);
            string longName = netCdfFile.GetAttributeValue(netCdfVariable, LongNameAttributeName) ??
                              netCdfFile.GetAttributeValue(netCdfVariable, StandardNameAttributeName);
            string unit = netCdfFile.GetAttributeValue(netCdfVariable, UnitsAttributeName);
            string coverageLongName = longName != null ? $"{longName} ({netCdfVariableName})" : netCdfVariableName;

            if (!double.TryParse(netCdfFile.GetAttributeValue(netCdfVariable, FillValueAttribute), out double noDataValue))
                noDataValue = MissingValue;

            Type dotNetType = NetCdfConstants.GetClrDataType(netCdfVariableType);
            if (string.Equals(location, UGridConstants.Naming.FaceLocationAttributeName))
            {
                UnstructuredGridCoverage coverage = CreateUnstructuredGridCoverage(location, coverageLongName, dotNetType);
                if (coverage != null)
                {
                    InitializeCoverage(coverage, secondDimensionName, netCdfVariableName, unit, timeDependentVariable.ReferenceDate, noDataValue);
                }
                return coverage;
            }

            var dimensionNameLocation = netCdfFile.GetDimensionName(netCdfFile.GetDimensions(netCdfVariable).ToArray()[1]);
            var timeDependentVariableMetaDataBaseKeyForThisLocation = MetaData.Locations.Keys.FirstOrDefault(tdv => tdv.Name.Equals(netCdfVariableName));
            
            if (timeDependentVariableMetaDataBaseKeyForThisLocation != null &&
                !locationsByNetworkDataType.ContainsKey(dimensionNameLocation))
            {
                locationsByNetworkDataType[dimensionNameLocation] = MetaData
                                                       .Locations[timeDependentVariableMetaDataBaseKeyForThisLocation]
                                                       .Select(l => new NetworkLocation(network.Branches[l.BranchId - sobekStartIndex], l.Chainage)
                                                                   { Geometry = new Point(l.XCoordinate, l.YCoordinate), Name = l.Id, Attributes = new DictionaryFeatureAttributeCollection() { { LocationAttributeName, dimensionNameLocation } } }).Cast<INetworkLocation>().ToList();
            }

            return CreateNetworkCoverage(coverageLongName, unit, netCdfVariableName, dimensionNameLocation, timeDependentVariable.ReferenceDate, noDataValue: noDataValue);
        }

        public override IMultiDimensionalArray<T> GetVariableValues<T>(IVariable variable, params IVariableFilter[] filters)
        {
            if (variable.ValueType != typeof(double) || variable.IsIndependent)
            {
                return base.GetVariableValues<T>(variable, filters);
            }

            var coverage = GetCoverage(variable);
            if (coverage == null)
                return new MultiDimensionalArray<T>(new List<T>(), new[] { 0, 0 });

            var coverageComponent = coverage.Components[0];
            var ncVariableName = coverageComponent.Attributes.ContainsKey(NcNameAttribute)
                                     ? coverageComponent.Attributes[NcNameAttribute]
                                     : null;

            if (ncVariableName == null)
            {
                return new MultiDimensionalArray<T>(new List<T>(), new[] { 0, 0 });
            }

            if (filters == null || filters.Length == 0)
            {
                return GetValuesForTimeSeriesAtAllLocations<T>(variable, ncVariableName);
            }

            if (variable.Attributes["OriginalType"] == "Byte")
            {
                // create 'fake' objects which we 
                IVariable clone = (IVariable) variable.Clone(false, false, false);
                clone.Components.RemoveAt(0);
                clone.Components.Add((IVariable) TypeUtils.CreateGeneric(typeof(Variable<>), typeof(byte)));
                clone.Components[0].Name = "value";

                (IList<byte> timeSeriesData, int[] shape) = GetValuesUsingFilters<byte>(filters, coverage, clone, ncVariableName);
                    
                IEnumerable<double> returnValueInDoubles = timeSeriesData.Select(Convert.ToDouble).ToList();
                var returnValue = new MultiDimensionalArray<T>(returnValueInDoubles.Cast<T>().ToList(), shape);
                UpdateMinMaxCache(returnValueInDoubles, variable);
                return returnValue;
            }
            else
            {
                (IList<T> timeSeriesData, int[] shape) = GetValuesUsingFilters<T>(filters, coverage, variable, ncVariableName);
                UpdateMinMaxCache(timeSeriesData.Cast<double>(), variable);
                return new MultiDimensionalArray<T>(timeSeriesData, shape);
            }
        }

        private (IList<T> timeSeriesData, int[] shape) GetValuesUsingFilters<T>(IVariableFilter[] filters, ICoverage coverage, IVariable variable, string ncVariableName)
        {
            IList<T> timeSeriesData = null;

            var dateTimeFilter = filters.OfType<VariableValueFilter<DateTime>>().FirstOrDefault(f => f.Variable == coverage.Time);

            var featureVariable = coverage.Arguments.FirstOrDefault(a => a != coverage.Time && a.ValueType.Implements(typeof(IBranchFeature)));
            var branchFeatureFilter = filters.OfType<IVariableValueFilter>().FirstOrDefault(f => f.Variable == featureVariable);
            var branchRangeFilter = filters.OfType<VariableIndexRangesFilter>().FirstOrDefault(f => f.Variable == featureVariable);

            var hasBranchRangeFilter = branchRangeFilter != null && branchRangeFilter.IndexRanges.Count == 1;
            var hasBranchFilter = branchFeatureFilter != null && branchFeatureFilter.Values.Count == 1;
            var hasTimeFilter = dateTimeFilter != null && dateTimeFilter.Values.Count == 1;

            int[] shape = null;

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
            catch
            {
                //gulp return empty
                timeSeriesData = new List<T>();
                shape = new[]{0,0};
                return (timeSeriesData, shape);
            }

            if (shape == null || timeSeriesData == null)
            {
                throw new NotImplementedException();
            }

            return (timeSeriesData, shape);
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
                if (fullVariableCache.TryGetValue(function, out var seriesAtAllLocations))
                {
                    return (IMultiDimensionalArray<T>)seriesAtAllLocations;
                }

                IMultiDimensionalArray<T> multiDimensionalArrayOfAllVariableDataTyped;
                if (function.Attributes["OriginalType"] == "Byte")
                {
                    var multiDimensionalArrayOfAllVariableByte = GetAllVariableData<byte>(ncVariableName).ToMultiDimensionalArray();
                    multiDimensionalArrayOfAllVariableDataTyped = new MultiDimensionalArray<T>(multiDimensionalArrayOfAllVariableByte.Select(b => (T) Convert.ChangeType(b, typeof(T))).ToList(), multiDimensionalArrayOfAllVariableByte.Shape);
                }
                else
                {
                    multiDimensionalArrayOfAllVariableDataTyped = GetAllVariableData<T>(ncVariableName).ToMultiDimensionalArray();
                }

                UpdateMinMaxCache(multiDimensionalArrayOfAllVariableDataTyped.Cast<double>(), function);
                fullVariableCache[function] = multiDimensionalArrayOfAllVariableDataTyped;
                return multiDimensionalArrayOfAllVariableDataTyped;
                
            };
            timeSeriesAtAllLocations = new LazyMultiDimensionalArray<T>(realGetFunction, () => (MetaData?.Times.Count ?? 1) * (MetaData?.NumLocationsForFunctionId(ncVariableName) ?? 1));
            argumentVariableCache[function] = timeSeriesAtAllLocations;
            return (IMultiDimensionalArray<T>)timeSeriesAtAllLocations;
        }

        private T[,] GetAllVariableData<T>(string variableName)
        {
            using (ReconnectToMapFile())
            {
                var variable = netCdfFile.GetVariableByName(variableName);
                if (variable == null) return new T[0, 0];
                return (T[,])netCdfFile.Read(variable);
            }
        }

        private IList<T> GetValuesForTimeSeriesAtSingleLocation<T>(IVariable function, string ncVariableName, IVariableValueFilter branchFeatureFilter, out int[] shape)
        {
            var locationIndex = GetLocationIndex((IBranchFeature)branchFeatureFilter.Values[0], ncVariableName);

            var origin = new[] { 0, locationIndex };
            shape = new[] { MetaData.Times.Count, 1 };
            if (argumentVariableCache.TryGetValue(function, out var timeSeriesAtAllLocations))
            {
                return ((IMultiDimensionalArray<T>)timeSeriesAtAllLocations).Select(1, new[] { locationIndex });
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
                return ((IMultiDimensionalArray<T>)timeSeriesAtAllLocations).Select(0, new[] { timeStepIndex });

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
                return ((IMultiDimensionalArray<T>)timeSeriesAtAllLocations).Select(new[] { timeStepIndex, beginIndex }, new[] { timeStepIndex, endIndex });

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
                return ((IMultiDimensionalArray<T>)timeSeriesAtAllLocations).Select(new[] { timeStepIndex, locationIndex }, new[] { timeStepIndex, locationIndex });

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
                    return (IList<T>)locationData.OfType<object>().Select(o => (T)Convert.ChangeType(o,typeof(T))).ToList();
                }
            }
            catch (Exception)
            {
                shape = new[] { 0, 0 };
                return new List<T>();
            }
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
                           ?? MetaData.Locations[timeDependentVariableMetaDataBaseKeyForThisLocation]
                               .FirstOrDefault(l => l.Id.Equals(branchFeature.Name, StringComparison.InvariantCultureIgnoreCase))
                           ?? MetaData.Locations[timeDependentVariableMetaDataBaseKeyForThisLocation]
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

        #endregion

        private ICoverage GetCoverage(IVariable variable)
        {
            return Functions.OfType<ICoverage>().FirstOrDefault(f => f.Arguments.Concat(f.Components).Contains(variable));
        }
        public ICoordinateSystem CoordinateSystem { get; set; }
        private NetworkCoverage CreateNetworkCoverage(string coverageLongName, string unitSymbol,
                                                      string netCdfVariableName, string location, string refDate, int number = -1, double noDataValue = -999.0d)
        {
            var suffix = number < 0 ? string.Empty : $" ({number})";
            var coverageName = coverageLongName + suffix;

            var networkCoverage = new NetworkCoverage(coverageName, true, coverageName, unitSymbol)
            {
                Network = network, 
                CoordinateSystem = CoordinateSystem
            };

            InitializeCoverage(networkCoverage, location, netCdfVariableName, unitSymbol, refDate, noDataValue);

            networkCoverage.Locations.FixedSize = 0;
            networkCoverage.Locations.InterpolationType = InterpolationType.Constant;
            networkCoverage.Locations.ExtrapolationType = ExtrapolationType.Constant;

            var coverageComponent = networkCoverage.Components[0];
            coverageComponent.Attributes["OriginalType"] = "Byte";

            if (coverageComponent.ValueType == typeof(double))
            {
                coverageComponent.NoDataValue = noDataValue;
            }
            
            networkCoverage.Attributes.Add("NetCdfVariableName", netCdfVariableName);
            networkCoverage.Attributes.Add(LocationAttributeName, GetNetworkLocation(location).ToString());

            return networkCoverage;
        }

        private static NetworkDataLocation GetNetworkLocation(string locationName)
        {
            switch (locationName)
            {
                case var _ when string.Equals(locationName,UGridConstants.Naming.EdgeLocationAttributeName, StringComparison.InvariantCultureIgnoreCase):
                    return NetworkDataLocation.Edge;
                case var _ when string.Equals(locationName, UGridConstants.Naming.NodeLocationAttributeName, StringComparison.InvariantCultureIgnoreCase):
                    return NetworkDataLocation.Node;
                case var _ when string.Equals(locationName, UGridConstants.Naming.FaceLocationAttributeName, StringComparison.InvariantCultureIgnoreCase):
                    return NetworkDataLocation.Face;
                default:
                    return NetworkDataLocation.UnKnown;
            }
        }
        private UnstructuredGridCoverage CreateUnstructuredGridCoverage(string location, string coverageLongName, Type outputType)
        {
            if (location != UGridConstants.Naming.FaceLocationAttributeName)
            {
                Log.WarnFormat(
                    $"Cannot create coverage: can only create coverages for cell faces. See '{coverageLongName}' in the class map file: {Path}.");
                return null;
            }

            var coverage = new UnstructuredGridCellCoverage(grid, true) {Name = coverageLongName};

            coverage.Components.RemoveAt(0);
            coverage.Components.Add((IVariable) TypeUtils.CreateGeneric(typeof(Variable<>), outputType));
            coverage.Components[0].Name = "value";

            return coverage;
        }

        private void InitializeCoverage(IFunction coverage, string secondDimensionName, string variableName,
                                        string unitSymbol, string refDate, double noDataValue)
        {
            coverage.Store = this;

            IVariable timeDimension = coverage.Arguments[0];
            timeDimension.Name = "Time";
            timeDimension.Attributes[NcNameAttribute] = TimeVariableNames[0];
            timeDimension.Attributes[NcUseVariableSizeAttribute] = "true";
            timeDimension.Attributes[NcRefDateAttribute] = refDate;
            timeDimension.IsEditable = false;

            IVariable secondDimension = coverage.Arguments[1];
            secondDimension.Name = secondDimensionName;
            secondDimension.Attributes[NcNameAttribute] = secondDimensionName;
            secondDimension.Attributes[NcUseVariableSizeAttribute] = "false";
            secondDimension.IsEditable = false;

            IVariable coverageComponent = coverage.Components[0];
            coverageComponent.Name = variableName;
            coverageComponent.Attributes[NcNameAttribute] = variableName;
            coverageComponent.Attributes[NcUseVariableSizeAttribute] = "true";

            if (coverageComponent.ValueType == typeof(double))
            {
                coverageComponent.NoDataValue = noDataValue;
            }

            coverageComponent.IsEditable = false;
            coverageComponent.Unit = new Unit(unitSymbol, unitSymbol);
            coverage.IsEditable = false;
        }

        protected override void UpdateFunctionsAfterPathSet()
        {
            ClearCaches();
            base.UpdateFunctionsAfterPathSet();
        }

        private void ClearCaches()
        {
            locationsByNetworkDataType.Clear();
            networkLocationsForThisFunctionCache.Clear();
            argumentVariableCache.Clear();
            fullVariableCache.Clear();
            metaData = null;
        }
    }

}