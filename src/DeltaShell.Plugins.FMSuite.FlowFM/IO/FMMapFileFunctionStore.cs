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
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.NetCdf;
using Deltares.Infrastructure.Logging;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Grid.DeltaresUGrid;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class FMMapFileFunctionStore : FMNetCdfFileFunctionStore
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FMMapFileFunctionStore));

        private static readonly IList<string> DeprecatedVariables = new[]
        {
            "s0",
            "u0"
        };

        private readonly IList<ITimeSeries> boundaryCellValues = new List<ITimeSeries>();

        private readonly Dictionary<string, UnstructuredGridCoverage> velocityCoverages =
            new Dictionary<string, UnstructuredGridCoverage>();

        /// <summary>
        /// Creates a new instance of <see cref="FMMapFileFunctionStore"/>.
        /// </summary>
        /// <remarks> This class needs a parameterless constructor because of NHibernate functionality. </remarks>
        public FMMapFileFunctionStore()
        {
            DisableCaching = true;
        }
        public UnstructuredGrid Grid { get; private set; }

        public IList<ILink1D2D> Links { get; private set; }

        public IDiscretization Discretization { get; private set; }

        public HydroNetwork Network { get; private set; }

        public IList<ITimeSeries> BoundaryCellValues => boundaryCellValues;

        public IFunction CustomVelocityCoverage
        {
            get
            {
                return Functions.FirstOrDefault(f => f.Name == VelocityCoverageName);
            }
        }

        public void SetCoordinateSystem(ICoordinateSystem coordinateSystem)
        {
            if (Grid != null)
            {
                Grid.CoordinateSystem = coordinateSystem;
            }
            else
            {
                log.Warn(Resources.FMMapFileFunctionStore_CoordinateSystem_Could_not_set_coordinate_system_in_output_map_because_grid_is_not_set);
            }
        }

        public IEnumerable<IGrouping<string, IFunction>> GetFunctionGrouping()
        {
            // Filter out custom velocity coverage
            IEnumerable<IFunction> regularFunctions = Functions.Where(f => f.Name != VelocityCoverageName);
            return regularFunctions.GroupBy(f => f.Components[0].Attributes[NcNameAttribute]);
        }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            if (!ValidateTimes())
            {
                return Array.Empty<IFunction>();
            }
            Grid = new UnstructuredGrid();
            Network = new HydroNetwork();
            Discretization = new Discretization { Network = Network };
            Links = new List<ILink1D2D>();
            IConvertedUgridFileObjects convertedUgridFileObjects = new ConvertedUgridFileObjects
            {
                Discretization = Discretization,
                Grid = Grid,
                HydroNetwork = Network,
                Links1D2D = Links
            };
            boundaryCellValues.Clear();
            using (var ugridFile = new UGridFile(netCdfFile.Path))
            {
                var logHandler = new LogHandler($"Reading fm 2D map file {netCdfFile.Path} (as output) into our model.", log);
                ugridFile.ReadNetFileDataIntoModel(convertedUgridFileObjects, loadFlowLinksAndCells: true, recreateCells: false, forceCustomLengths: true, logHandler: logHandler, reportProgress: null);
                logHandler.LogReport();

                var isUGrid = ugridFile.IsUGridFile();
                
                var functions = GetFunctions(dataVariables, isUGrid);

                if (!isUGrid)
                {
                    LogWarningsForExcludedTimeDependentVariables(dataVariables);
                }

                return functions;
            }
        }

        protected override int GetVariableValuesCount(IVariable function, IVariableFilter[] filters)
        {
            if (!HasValidFile)
            {
                return 0;
            }

            int variableValuesCount = base.GetVariableValuesCount(function, filters);

            if (function.IsIndependent)
            {
                return variableValuesCount;
            }

            IFunction coverage = Functions.FirstOrDefault(f => f.Components.Contains(function));

            if (coverage == null || !coverage.Attributes.ContainsKey(SedIndexAttributeName))
            {
                return variableValuesCount;
            }

            var netcdfVariableDimensionLength = 1;

            using (ReconnectToMapFile())
            {
                NetCdfVariable netcdfVariable =
                    netCdfFile.GetVariableByName(function.Components[0].Attributes[NcNameAttribute]);
                if (netcdfVariable == null)
                {
                    throw new Exception("Missing NetCdf name");
                }

                List<NetCdfDimension> dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();
                List<string> dimensionNames = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();

                int sedSusVarIndex = dimensionNames.IndexOf(NSedSusName);
                int sedTotVarIndex = dimensionNames.IndexOf(NSedTotName);

                if ((sedSusVarIndex != -1 || sedTotVarIndex != -1) && dimensions.Count != 3)
                {
                    throw new Exception("Number of expected dimensions was 3");
                }

                if (sedSusVarIndex >= 0)
                {
                    netcdfVariableDimensionLength = netCdfFile.GetDimensionLength(NSedSusName);
                }
                else if (sedTotVarIndex >= 0)
                {
                    netcdfVariableDimensionLength = netCdfFile.GetDimensionLength(NSedTotName);
                }
                else
                {
                    return variableValuesCount;
                }
            }

            return variableValuesCount / netcdfVariableDimensionLength;
        }

        protected override void GetShapeAndOrigin(IVariable function, IVariableFilter[] filters, out int[] shape, out int[] origin, out int[] stride)
        {
            base.GetShapeAndOrigin(function, filters, out shape, out origin, out stride);

            if (function.IsIndependent)
            {
                return;
            }

            IFunction coverage = Functions.FirstOrDefault(f => f.Components.Contains(function));

            if (coverage == null)
            {
                coverage = Functions.FirstOrDefault(f => f == function);

                if (coverage == null || !coverage.Attributes.ContainsKey(SedIndexAttributeName))
                {
                    return;
                }
            }
            else
            {
                if (!coverage.Attributes.ContainsKey(SedIndexAttributeName))
                {
                    return;
                }
            }

            NetCdfVariable netcdfVariable =
                netCdfFile.GetVariableByName(function.Components[0].Attributes[NcNameAttribute]);
            if (netcdfVariable == null)
            {
                throw new Exception("Missing NetCdf name");
            }

            List<NetCdfDimension> dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();

            List<string> dimensionNames = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();
            int sedSusVarIndex = dimensionNames.IndexOf(NSedSusName);
            int sedTotVarIndex = dimensionNames.IndexOf(NSedTotName);

            if ((sedSusVarIndex != -1 || sedTotVarIndex != -1) && dimensions.Count != 3)
            {
                throw new Exception("Number of dimensions is wrong");
            }

            var sedIndex = 0;
            if (!int.TryParse(coverage.Attributes[SedIndexAttributeName], out sedIndex))
            {
                throw new Exception("Sediment Index is not of integer type");
            }

            int dimensionIndex = Math.Max(sedTotVarIndex, sedSusVarIndex);
            var sedShape = 1;
            int sedOrigin = sedIndex;
            var sedStride = 1;

            if (filters.Length == 0)
            {
                shape[dimensionIndex] = sedShape;
                origin[dimensionIndex] = sedOrigin;
                stride[dimensionIndex] = sedStride;
            }
            else
            {
                shape = InsertItem(shape, dimensionIndex, sedShape);
                origin = InsertItem(origin, dimensionIndex, sedOrigin);
                stride = InsertItem(stride, dimensionIndex, sedStride);
            }
        }

        public override IMultiDimensionalArray<T> GetVariableValues<T>(IVariable variable, params IVariableFilter[] filters)
        {
            if (!HasValidFile)
            {
                return (IMultiDimensionalArray<T>)CreateEmptyArrayForType(variable.ValueType);
            }
            if (variable.IsIndependent && variable.ValueType == typeof(ILink1D2D))
            {
                var featureFilter = filters.FirstOrDefault(f => f.Variable.ValueType == typeof(ILink1D2D));
                if (filters.Length == 0 || featureFilter == null)
                {
                    return new MultiDimensionalArray<T>((IList<T>)Links);
                }

                if (featureFilter is VariableIndexFilter indexFilter)
                {
                    return new MultiDimensionalArray<T>(new List<T>(indexFilter.Indices.Select(i => (T)Links[i])));
                }

                return new MultiDimensionalArray<T>();
            }
            
            return base.GetVariableValues<T>(variable, filters);
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
        
        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(IVariable function, IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] == "false") // has no explicit variable (for example nFlowElem, which is only a dimension)
            {
                int size = GetSize(function);
                return new MultiDimensionalArray<T>(Enumerable.Range(0, size).Cast<T>().ToList(), new[]
                {
                    size
                });
            }

            //if this is a component find the coverage in Functions and apply filter
            if (!function.IsIndependent)
            {
                // is component or coverage
                IFunction coverage = Functions.FirstOrDefault(f => f.Components.Contains(function)); // check if function is component
                if (coverage == null)
                {
                    coverage = Functions.FirstOrDefault(f => f == function); //check if function is coverage

                    if (coverage != null)
                    {
                        // is coverage
                        //check if there are multidimensional sedimentnames
                        var indexOfSedimentToRender = string.Empty;
                        if (coverage.Attributes.TryGetValue(SedIndexAttributeName, out indexOfSedimentToRender) && 
                            int.TryParse(indexOfSedimentToRender, out int _))
                        {
                            var filter = new VariableIndexFilter(function.Components[0], 0);
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                        }
                    }
                }
                else
                {
                    // is component
                    //check if there are multidimensional sedimentnames
                    var indexOfSedimentToRender = string.Empty;
                    if (coverage.Attributes.TryGetValue(SedIndexAttributeName, out indexOfSedimentToRender) && 
                        int.TryParse(indexOfSedimentToRender, out int _))
                    {
                        var filter = new VariableIndexFilter(function, 0);
                        Array.Resize(ref filters, filters.Length + 1);
                        filters[filters.Length - 1] = filter;
                    }
                }
            }

            try
            {
                return base.GetVariableValuesCore<T>(function, filters);
            }
            catch (Exception e) when (e.Message.Contains("NetCDF error code"))
            {
                log.Error(string.Format("While reading variable {0} from the file {1} an error was encountered: {2}", function.Name, System.IO.Path.GetFileName(Path), e.Message));
                int functionSize = GetSize(function);
                return new MultiDimensionalArray<T>(new List<T>(new T[functionSize]), new [] { functionSize });
            }
        }

        private IEnumerable<IFunction> GetFunctions(IEnumerable<NetCdfVariableInfo> dataVariables, bool isUgridConvention)
        {
            var timeDepVariables = dataVariables
                .Where(v =>
                {
                    var isTimeDependent = v.IsTimeDependent && v.NumDimensions > 1;

                    return isUgridConvention
                        ? isTimeDependent
                        : isTimeDependent && v.NumDimensions <= 2;
                })
                .ToList();

            using (ReconnectToMapFile())
            {
                var mesh2DVariables = timeDepVariables
                    .Where(v =>
                    {
                        var attributeValue = netCdfFile.GetAttributeValue(v.NetCdfDataVariable, "mesh");
                        return string.IsNullOrEmpty(attributeValue) || // backward compatibility => no mesh attribute, so assume 2d mesh
                               string.Equals(attributeValue, "Mesh2d", StringComparison.InvariantCultureIgnoreCase);
                    })
                    .ToList();

                var linkVariables = timeDepVariables
                    .Where(v => string.Equals(netCdfFile.GetAttributeValue(v.NetCdfDataVariable, "mesh"), "links", StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                
                return GetUnstructuredGridCoverages(mesh2DVariables)
                    .Concat(Get1D2DLinksCoverages(linkVariables));
            }
        }

        private IEnumerable<ICoverage> Get1D2DLinksCoverages(IEnumerable<NetCdfVariableInfo> timeDepVariables)
        {
            return timeDepVariables.Select(v =>
            {
                var data = GetCoverageCreationData(v);
                if (data == null)
                    return null;

                var coverage = new FeatureCoverage(data.CoverageLongName)
                {
                    IsTimeDependent = true,
                    IsEditable = false,
                    CoordinateSystem = Grid.CoordinateSystem,
                    Features = new EventedList<IFeature>(Links)
                };

                coverage.Arguments.Add(new Variable<ILink1D2D>("Links"));
                coverage.Components.Add(new Variable<double>());

                InitializeCoverage(coverage, data.SecondDimensionName, data.VariableName, data.UnitSymbol, v.ReferenceDate);

                return coverage;
            }).Where(c => c != null);
        }

        private IEnumerable<ICoverage> GetUnstructuredGridCoverages(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            // Construct UnstructuredGridCoverages from file
            var functions = dataVariables.SelectMany(Process2dMeshVariable).Where(c => c != null).ToList();

            // Construct custom Velocity Coverage
            if (velocityCoverages.ContainsKey(EastwardSeaWaterVelocityStandardName) &&
                velocityCoverages.ContainsKey(NorthwardSeaWaterVelocityStandardName))
            {
                functions.Add(AddCustomVelocityCoverage(velocityCoverages[EastwardSeaWaterVelocityStandardName],
                    velocityCoverages[NorthwardSeaWaterVelocityStandardName]));
            }

            // Backwards compatibility...
            if (velocityCoverages.ContainsKey(SeaWaterXVelocityStandardName) &&
                velocityCoverages.ContainsKey(SeaWaterYVelocityStandardName))
            {
                functions.Add(AddCustomVelocityCoverage(velocityCoverages[SeaWaterXVelocityStandardName],
                    velocityCoverages[SeaWaterYVelocityStandardName]));
            }

            return functions;
        }

        private void LogWarningsForExcludedTimeDependentVariables(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            // When the NetCDF file is not UGRID1+, log a warning for the time dependent variables that have been filtered out
            List<NetCdfVariableInfo> filteredTimeDepVariables =
                dataVariables.Where(v => v.IsTimeDependent && v.NumDimensions > 2).ToList();
            IEnumerable<string> timeDepVariablesNames =
                filteredTimeDepVariables.Select(v => netCdfFile.GetVariableName(v.NetCdfDataVariable));
            foreach (string timeDepVarName in timeDepVariablesNames)
            {
                log.WarnFormat(
                    Resources
                        .FMMapFileFunctionStore_ConstructFunctions_Time_dependent_variable___0___has_been_filtered_out,
                    timeDepVarName);
            }
        }

        private UnstructuredGridCoverage AddCustomVelocityCoverage(UnstructuredGridCoverage ucxCoverage,
                                                                   UnstructuredGridCoverage ucyCoverage)
        {
            UnstructuredGridCoverage coverage = CreateUnstructuredGridCoverage(UGridConstants.Naming.FaceLocationAttributeName, VelocityCoverageName);

            coverage.Components.Add(new Variable<double>()); // add 2nd component
            coverage.Components[1].Name = ucyCoverage.Components[0].Name;
            coverage.Components[1].Attributes[NcNameAttribute] = ucyCoverage.Components[0].Name;
            coverage.Components[1].Attributes[NcUseVariableSizeAttribute] = "true";
            coverage.Components[1].IsEditable = false;

            InitializeCoverage(coverage, ucxCoverage.Arguments[1].Name, ucxCoverage.Components[0].Name, "m/s",
                               ucxCoverage.Arguments[0].Attributes[NcRefDateAttribute]);

            return coverage;
        }

        private UnstructuredGridCoverage CreateUnstructuredGridCoverage(string location, string coverageLongName, int number = -1)
        {
            string suffix = number < 0 ? string.Empty : string.Format(" ({0})", number);
            string coverageName = coverageLongName + suffix;
            switch (location)
            {
                // UGrid standard
                case UGridConstants.Naming.FaceLocationAttributeName:
                    return new UnstructuredGridCellCoverage(Grid, true) { Name = coverageName };
                case UGridConstants.Naming.EdgeLocationAttributeName:
                    return new UnstructuredGridEdgeCoverage(Grid, true) { Name = coverageName };
                case UGridConstants.Naming.NodeLocationAttributeName:
                    return new UnstructuredGridVertexCoverage(Grid, true) { Name = coverageName };
                case UGridConstants.Naming.VolumeLocationAttributeName:
                    log.WarnFormat(
                        Resources.FMMapFileFunctionStore_CreateCoverage_CannotCreateSpatialDataOnVolumeLocation,
                        coverageName);
                    return null;

                // backwards compatibility
                case NFlowElemName:
                    return new UnstructuredGridCellCoverage(Grid, true) { Name = coverageName };
                case NFlowLinkName:
                    return new UnstructuredGridFlowLinkCoverage(Grid, true) { Name = coverageName };
                case NNetLinkName:
                case NFlowElemBndName:
                    log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverage_NetlinkDimensionCurrentyNotSupported,
                                   coverageName);
                    return null;
                default:
                    throw new NotImplementedException(
                        string.Format(Resources.FMMapFileFunctionStore_CreateCoverage_UnexpectedLocationDimension,
                                      location));
            }
        }

        private void InitializeCoverage(IFunction coverage, string secondDimensionName, string variableName,
                                        string unitSymbol, string refDate,
                                        IEnumerable<Tuple<string, string>> secondDimensionAdditionalAttributes = null)
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

            // Allowing us to add additional attributes (e.g. sedimentation related)
            if (secondDimensionAdditionalAttributes != null)
            {
                foreach (Tuple<string, string> secondDimensionAdditionalAttribute in secondDimensionAdditionalAttributes)
                {
                    if (string.IsNullOrEmpty(secondDimensionAdditionalAttribute.Item1))
                    {
                        continue;
                    }

                    coverage.Attributes[secondDimensionAdditionalAttribute.Item1] = secondDimensionAdditionalAttribute.Item2;
                }
            }

            secondDimension.IsEditable = false;

            IVariable coverageComponent = coverage.Components[0];
            coverageComponent.Name = variableName;
            coverageComponent.Attributes[NcNameAttribute] = variableName;
            coverageComponent.Attributes[NcUseVariableSizeAttribute] = "true";
            coverageComponent.NoDataValue = MissingValue;
            coverageComponent.IsEditable = false;
            coverageComponent.Unit = new Unit(unitSymbol, unitSymbol);

            coverage.IsEditable = false;
        }

        private int[] InsertItem(int[] original, int index, int value)
        {
            List<int> list = original.ToList();
            if (index < list.Count && index >= 0)
            {
                list.Insert(index, value);
            }
            else
            {
                list.Add(value);
            }

            return list.ToArray();
        }

        private CoverageCreationData GetCoverageCreationData(NetCdfVariableInfo timeDependentVariable)
        {
            NetCdfVariable netcdfVariable = timeDependentVariable.NetCdfDataVariable;

            string netCdfVariableName = netCdfFile.GetVariableName(netcdfVariable);
            if (DeprecatedVariables.Contains(netCdfVariableName))
            {
                return null;
            }

            NetCdfDataType netCdfVariableType = netCdfFile.GetVariableDataType(netcdfVariable);
            if (netCdfVariableType != NetCdfDataType.NcDoublePrecision)
            {
                log.WarnFormat(
                    Resources.FMMapFileFunctionStore_CreateCoverageFromNetCdfVariable_FailedToConstructGridSpatialData,
                    netCdfVariableName, netCdfVariableType);
                return null;
            }

            List<NetCdfDimension> dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();

            string secondDimensionName = netCdfFile.GetDimensionName(dimensions[1]);

            string longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute) ??
                              netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttribute);

            string coverageLongName = longName != null
                ? string.Format("{0} ({1})", longName, netCdfVariableName)
                : netCdfVariableName;
            using (var ugridFile = new UGridFile(netCdfFile.Path))
            {
                string location = ugridFile.IsUGridFile()
                                      ? netCdfFile.GetAttributeValue(netcdfVariable, UGridConstants.Naming.LocationAttributeName)
                                      : secondDimensionName; // backwards compatibility
                
                string unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);

                return new CoverageCreationData
                {
                    VariableName = netCdfVariableName,
                    SecondDimensionName = secondDimensionName,
                    Dimensions = dimensions,
                    VariableInfo = timeDependentVariable,
                    CoverageLongName = coverageLongName,
                    Location = location,
                    UnitSymbol = unitSymbol
                };
            }
        }

        private IEnumerable<ICoverage> Process2dMeshVariable(NetCdfVariableInfo timeDependentVariable)
        {
            var data = GetCoverageCreationData(timeDependentVariable);
            if (data == null) 
                yield break;

            // Depending on the NetCdfVariable, Sediment dimension can be SedSus (suspended) or SedTot (total)
            List<string> dimensionNameList = data.Dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();
            int sedSusVarIndex = dimensionNameList.IndexOf(NSedSusName);
            int sedTotVarIndex = dimensionNameList.IndexOf(NSedTotName);

            if ((sedSusVarIndex != -1 || sedTotVarIndex != -1) && data.Dimensions.Count == 3)
            {
                //Process variable as three dimensional time dependent variable
                int sedimentDimensionIndex = Math.Max(sedTotVarIndex, sedSusVarIndex);
                foreach (UnstructuredGridCoverage unstructuredGridCoverage in ProcessThreeDimensionalTimeDependentVariable(data, sedimentDimensionIndex))
                {
                    yield return unstructuredGridCoverage;
                }

                yield break;
            }

            var coverage = CreateUnstructuredGridCoverage(data.Location, data.CoverageLongName);

            if (coverage != null)
            {
                InitializeCoverage(coverage, data.SecondDimensionName, data.VariableName, data.UnitSymbol, timeDependentVariable.ReferenceDate);
            }

            string standardName = netCdfFile.GetAttributeValue(timeDependentVariable.NetCdfDataVariable, StandardNameAttribute);

            if (standardName == EastwardSeaWaterVelocityStandardName ||
                standardName == NorthwardSeaWaterVelocityStandardName ||
                standardName == SeaWaterXVelocityStandardName ||
                standardName == SeaWaterYVelocityStandardName) // Backwards compatibility *ugh*
            {
                velocityCoverages[standardName] = coverage;
            }

            yield return coverage;
        }

        private IEnumerable<UnstructuredGridCoverage> ProcessThreeDimensionalTimeDependentVariable(CoverageCreationData data, int sedimentDimensionIndex)
        {
            int numberOfSedLayers = netCdfFile.GetDimensionLength(data.Dimensions[sedimentDimensionIndex]);

            for (var index = 0; index < numberOfSedLayers; index++)
            {
                var sedCoverage = CreateUnstructuredGridCoverage(data.Location, data.CoverageLongName, index);
                if (sedCoverage != null)
                {
                    string secondDimensionName = netCdfFile.GetDimensionName(sedimentDimensionIndex != 1 ? data.Dimensions[1] : data.Dimensions[2]);
                    InitializeCoverage(sedCoverage, secondDimensionName,  data.VariableName, data.UnitSymbol, data.VariableInfo.ReferenceDate, 
                        new[]
                                       {
                                           new Tuple<string, string>(SedIndexAttributeName, index.ToString())
                                       });
                }

                yield return sedCoverage;
            }
        }
        
        #region Map file constants

        private const string NSedSusName = "nSedSus";
        private const string NSedTotName = "nSedTot";
        private const string VelocityCoverageName = "velocity (ucx + ucy)";
        private const string NFlowElemName = "nFlowElem";
        private const string NFlowLinkName = "nFlowLink";
        private const string NNetLinkName = "nNetLink";
        private const string NFlowElemBndName = "nFlowElemBnd";

        private const string StandardNameAttribute = "standard_name";
        private const string LongNameAttribute = "long_name";
        private const string UnitAttribute = "units";

        private const string EastwardSeaWaterVelocityStandardName = "eastward_sea_water_velocity";
        private const string NorthwardSeaWaterVelocityStandardName = "northward_sea_water_velocity";

        // For Backwards compatibility: since the fm kernel keeps changing between the two
        private const string SeaWaterXVelocityStandardName = "sea_water_x_velocity";
        private const string SeaWaterYVelocityStandardName = "sea_water_y_velocity";

        private const string SedIndexAttributeName = "SedIndex";

        #endregion

        private class CoverageCreationData
        {
            public string Location { get; set; }

            public string CoverageLongName { get; set; }

            public string VariableName { get; set; }

            public string UnitSymbol { get; set; }

            public string SecondDimensionName { get; set; }

            public NetCdfVariableInfo VariableInfo { get; set; }
            
            public List<NetCdfDimension> Dimensions { get; set; }
        }
    }
}