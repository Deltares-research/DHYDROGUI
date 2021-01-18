using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.CoordinateSystems;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores
{
    public class FMMapFileFunctionStore : FMNetCdfFileFunctionStore, IFMMapFileFunctionStore
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
                log.Warn(Resources
                             .FMMapFileFunctionStore_CoordinateSystem_Could_not_set_coordinate_system_in_output_map_because_grid_is_not_set);
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
            boundaryCellValues.Clear();
            UpdateGrid();
            bool isUgridConvention = GetNcFileConvention() == GridApiDataSet.DataSetConventions.CONV_UGRID;

            List<UnstructuredGridCoverage> functions = GetFunctions(dataVariables, isUgridConvention);
            if (!isUgridConvention)
            {
                LogWarningsForExcludedTimeDependentVariables(dataVariables);
            }

            return functions;
        }

        protected override int GetVariableValuesCount(IVariable variable, IVariableFilter[] filters)
        {
            int variableValuesCount = base.GetVariableValuesCount(variable, filters);

            if (variable.IsIndependent)
            {
                return variableValuesCount;
            }

            IFunction coverage = Functions.FirstOrDefault(f => f.Components.Contains(variable));

            if (coverage == null || !coverage.Attributes.ContainsKey(SedIndexAttributeName))
            {
                return variableValuesCount;
            }

            var netcdfVariableDimensionLength = 1;

            using (ReconnectToMapFile())
            {
                NetCdfVariable netcdfVariable =
                    netCdfFile.GetVariableByName(variable.Components[0].Attributes[NcNameAttribute]);
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

        protected override void GetShapeAndOrigin(IVariable function, IVariableFilter[] filters, out int[] shape,
                                                  out int[] origin, out int[] stride)
        {
            base.GetShapeAndOrigin(function, filters, out shape, out origin, out stride);

            if (function.IsIndependent || !IsCoverageFunction(function))
            {
                return;
            }

            NetCdfVariable netcdfVariable = GetNetcdfVariable(function);

            List<NetCdfDimension> dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();

            List<string> dimensionNames = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();
            int sedSusVarIndex = dimensionNames.IndexOf(NSedSusName);
            int sedTotVarIndex = dimensionNames.IndexOf(NSedTotName);

            if ((sedSusVarIndex != -1 || sedTotVarIndex != -1) && dimensions.Count != 3)
            {
                throw new Exception("Number of dimensions is wrong");
            }

            var sedIndex = 0;
            IFunction coverage = GetCoverageFunction(function);
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

        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(
            IVariable function, IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] == "false") // has no explicit variable (for example nFlowElem, which is only a dimension)
            {
                int size = GetSize(function);
                return new MultiDimensionalArray<T>(Enumerable.Range(0, size).Cast<T>().ToList(), new[]
                {
                    size
                });
            }

            // Find the coverage and apply filter
            if (!function.IsIndependent && IsCoverageFunction(function))
            {
                IFunction coverage = GetCoverageFunction(function);
                if (coverage != null)
                {
                    // is coverage
                    //check if there are multidimensional sedimentnames
                    var indexOfSedimentToRender = string.Empty;
                    if (coverage.Attributes.TryGetValue(SedIndexAttributeName, out indexOfSedimentToRender))
                    {
                        int nIndex = -1;
                        if (int.TryParse(indexOfSedimentToRender, out nIndex))
                        {
                            IVariable variableToFilter = IsCoverageFunctionComponent(function)
                                                             ? function
                                                             : function.Components[0];

                            var filter = new VariableIndexFilter(variableToFilter, 0);
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                        }
                    }
                }
            }

            try
            {
                return base.GetVariableValuesCore<T>(function, filters);
            }
            catch (Exception e) when (e.Message.Contains("NetCDF error code"))
            {
                log.Error(string.Format(Resources.FMMapFileFunctionStore_GetVariableValuesCore_While_reading_variable__0__from_the_file__1__an_error_was_encountered___2_, function.Name, System.IO.Path.GetFileName(Path), e.Message));
                int functionSize = GetSize(function);
                return new MultiDimensionalArray<T>(new List<T>(new T[functionSize]), functionSize);
            }
        }

        /// <summary>
        /// Gets the NetCdf variable based on the function.
        /// </summary>
        /// <param name="function">The <see cref="IVariable"/> to get the NetCdf variable for.</param>
        /// <returns>The <see cref="NetCdfVariable"/> corresponding with the <paramref name="function"/>.</returns>
        /// <exception cref="Exception">Thrown when the <paramref name="function"/> does not have a name.</exception>
        private NetCdfVariable GetNetcdfVariable(IVariable function)
        {
            NetCdfVariable netcdfVariable = netCdfFile.GetVariableByName(function.Components[0].Attributes[NcNameAttribute]);
            if (netcdfVariable == null)
            {
                throw new Exception("Missing NetCdf name");
            }

            return netcdfVariable;
        }

        /// <summary>
        /// Gets an indicator whether the <see cref="IVariable"/> is a coverage function.
        /// </summary>
        /// <param name="function">The <see cref="IVariable"/> to determine whether it is a coverage function.</param>
        /// <returns><c>true</c> if it is a coverage function, <c>false</c> otherwise.</returns>
        private bool IsCoverageFunction(IVariable function)
        {
            IFunction coverage = GetCoverageFunction(function);
            return coverage != null && coverage.Attributes.ContainsKey(SedIndexAttributeName);
        }

        /// <summary>
        /// Function to determine whether the coverage function is a component.
        /// </summary>
        /// <param name="coverageFunction">The function to determine for.</param>
        /// <returns><c>true</c> if the function is a component, <c>false</c> otherwise.</returns>
        private bool IsCoverageFunctionComponent(IVariable coverageFunction)
        {
            return GetCoverageFunction(coverageFunction) != null
                   && Functions.Any(f => f.Components.Contains(coverageFunction));
        }

        /// <summary>
        /// Gets the coverage function from an <see cref="IVariable"/>.
        /// </summary>
        /// <param name="function">The <see cref="IVariable"/> to get the coverage function for.</param>
        /// <returns>An <see cref="IFunction"/> if the variable is a coverage function, <c>null</c> otherwise.</returns>
        private IFunction GetCoverageFunction(IVariable function)
        {
            // Check if the function is a  component to determine whether it is a coverage function
            IFunction coverage = Functions.FirstOrDefault(f => f.Components.Contains(function));

            // If the coverage is null, check if the function itself is a coverage function
            if (coverage == null)
            {
                coverage = Functions.FirstOrDefault(f => f == function);
            }

            return coverage;
        }

        private List<UnstructuredGridCoverage> GetFunctions(IEnumerable<NetCdfVariableInfo> dataVariables,
                                                            bool isUgridConvention)
        {
            // Construct UnstructuredGridCoverages from file
            Func<NetCdfVariableInfo, bool> timeDepVarSelectionCriteria = isUgridConvention
                                                                             ? (Func<NetCdfVariableInfo, bool>)
                                                                             (v => v.IsTimeDependent &&
                                                                                   v.NumDimensions > 1)
                                                                             : v => v.IsTimeDependent &&
                                                                                    v.NumDimensions > 1 &&
                                                                                    v.NumDimensions <= 2;
            List<NetCdfVariableInfo> timeDepVariables = dataVariables.Where(timeDepVarSelectionCriteria).ToList();
            List<UnstructuredGridCoverage> functions =
                timeDepVariables.SelectMany(v => ProcessTimeDependentVariable(v, isUgridConvention))
                                .Where(c => c != null).ToList();

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
            UnstructuredGridCoverage coverage =
                CreateCoverage(GridApiDataSet.UGridAttributeConstants.LocationValues.Face, VelocityCoverageName);

            coverage.Components.Add(new Variable<double>()); // add 2nd component
            coverage.Components[1].Name = ucyCoverage.Components[0].Name;
            coverage.Components[1].Attributes[NcNameAttribute] = ucyCoverage.Components[0].Name;
            coverage.Components[1].Attributes[NcUseVariableSizeAttribute] = "true";
            coverage.Components[1].IsEditable = false;

            InitializeTwoDimensionalCoverage(coverage, ucxCoverage.Arguments[1].Name, ucxCoverage.Components[0].Name, "m/s",
                                             ucxCoverage.Arguments[0].Attributes[NcRefDateAttribute]);

            return coverage;
        }

        private UnstructuredGridCoverage CreateCoverage(string location, string coverageLongName, int number = -1)
        {
            // TODO : Suffix should not be the sediment index but the actual name of the sediment - this is not currently available in the map file
            string suffix = number < 0 ? string.Empty : string.Format(" ({0})", number);
            string coverageName = coverageLongName + suffix;
            switch (location)
            {
                // UGrid standard
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Face:
                    return new UnstructuredGridCellCoverage(Grid, true)
                    {
                        Name = coverageName
                    };
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Edge:
                    return new UnstructuredGridEdgeCoverage(Grid, true)
                    {
                        Name = coverageName
                    };
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Node:
                    return new UnstructuredGridVertexCoverage(Grid, true)
                    {
                        Name = coverageName
                    };
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Volume:
                    log.WarnFormat(
                        Resources.FMMapFileFunctionStore_CreateCoverage_CannotCreateSpatialDataOnVolumeLocation,
                        coverageName);
                    return null;

                // backwards compatibility
                case NFlowElemName:
                    return new UnstructuredGridCellCoverage(Grid, true)
                    {
                        Name = coverageName
                    };
                case NFlowLinkName:
                    return new UnstructuredGridFlowLinkCoverage(Grid, true)
                    {
                        Name = coverageName
                    };
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

        private GridApiDataSet.DataSetConventions GetNcFileConvention()
        {
            try
            {
                IUGridApi api = GridApiFactory.CreateNew();
                if (api != null)
                {
                    using (api)
                    {
                        GridApiDataSet.DataSetConventions convention;
                        int ierr = api.GetConvention(netCdfFile.Path, out convention);
                        if (ierr != GridApiDataSet.GridConstants.NOERR)
                        {
                            throw new Exception("Couldn't get the nc file convention because of error number: " + ierr);
                        }

                        return convention;
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat(
                    Resources.FMMapFileFunctionStore_CreateCoverageFromNetCdfVariable_FailedToConstructGridSpatialData,
                    e.Message);
            }

            return GridApiDataSet.DataSetConventions.CONV_NULL;
        }

        /// <summary>
        /// Initializes a coverage with a two dimensions.
        /// </summary>
        /// <param name="function">The function to initialize the coverage for.</param>
        /// <param name="secondDimensionName">The name of the second dimension.</param>
        /// <param name="variableName">The name of the variable to initialize the coverage for.</param>
        /// <param name="unitSymbol">The symbol of the variable.</param>
        /// <param name="refDate">The reference date.</param>
        private void InitializeTwoDimensionalCoverage(IFunction function, string secondDimensionName, string variableName,
                                                      string unitSymbol, string refDate)
        {
            function.Store = this;

            IVariable timeDimension = function.Arguments[0];
            timeDimension.Name = "Time";
            timeDimension.Attributes[NcNameAttribute] = TimeVariableNames[0];
            timeDimension.Attributes[NcUseVariableSizeAttribute] = "true";
            timeDimension.Attributes[NcRefDateAttribute] = refDate;
            timeDimension.IsEditable = false;

            InitializeAdditionalDimension(function.Arguments[1], secondDimensionName);

            IVariable coverageComponent = function.Components[0];
            coverageComponent.Name = variableName;
            coverageComponent.Attributes[NcNameAttribute] = variableName;
            coverageComponent.Attributes[NcUseVariableSizeAttribute] = "true";
            coverageComponent.NoDataValue = MissingValue;
            coverageComponent.IsEditable = false;
            coverageComponent.Unit = new Unit(unitSymbol, unitSymbol);

            function.IsEditable = false;
        }

        /// <summary>
        /// Initializes an additional dimension for a <see cref="IVariable"/>.
        /// </summary>
        /// <param name="variable">The variable to initialize an additional dimension for.</param>
        /// <param name="additionalDimensionName">The name of the additional dimension.</param>
        private static void InitializeAdditionalDimension(IVariable variable, string additionalDimensionName)
        {
            variable.Name = additionalDimensionName;
            variable.Attributes[NcNameAttribute] = additionalDimensionName;
            variable.Attributes[NcUseVariableSizeAttribute] = "false";

            variable.IsEditable = false;
        }

        /// <summary>
        /// Adds attributes to an additional dimension for a <see cref="IFunction"/>.
        /// </summary>
        /// <param name="function">The function to add the additional dimension for.</param>
        /// <param name="secondDimensionAdditionalAttributes">The collection of <see cref="Tuple"/> of attributes to add.</param>
        private static void AddAdditionalDimensionAttributes(IFunction function, IEnumerable<Tuple<string, string>> secondDimensionAdditionalAttributes)
        {
            // Allowing us to add additional attributes
            if (secondDimensionAdditionalAttributes != null)
            {
                foreach (Tuple<string, string> secondDimensionAdditionalAttribute in secondDimensionAdditionalAttributes)
                {
                    if (string.IsNullOrEmpty(secondDimensionAdditionalAttribute.Item1))
                    {
                        continue;
                    }

                    function.Attributes[secondDimensionAdditionalAttribute.Item1] =
                        secondDimensionAdditionalAttribute.Item2;
                }
            }
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

        private IEnumerable<UnstructuredGridCoverage> ProcessTimeDependentVariable(
            NetCdfVariableInfo timeDependentVariable, bool isUgridConvention)
        {
            UnstructuredGridCoverage coverage = null;
            NetCdfVariable netcdfVariable = timeDependentVariable.NetCdfDataVariable;

            string netCdfVariableName = netCdfFile.GetVariableName(netcdfVariable);
            if (DeprecatedVariables.Contains(netCdfVariableName))
            {
                yield break;
            }

            NetCdfDataType netCdfVariableType = netCdfFile.GetVariableDataType(netcdfVariable);
            if (netCdfVariableType != NetCdfDataType.NcDoublePrecision)
            {
                log.WarnFormat(
                    Resources.FMMapFileFunctionStore_CreateCoverageFromNetCdfVariable_FailedToConstructGridSpatialData,
                    netCdfVariableName, netCdfVariableType);
                yield break;
            }

            List<NetCdfDimension> dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();

            string secondDimensionName = netCdfFile.GetDimensionName(dimensions[1]);

            string longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute) ??
                              netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttribute);

            string coverageLongName = longName != null
                                          ? string.Format("{0} ({1})", longName, netCdfVariableName)
                                          : netCdfVariableName;

            string location = isUgridConvention
                                  ? netCdfFile.GetAttributeValue(netcdfVariable,
                                                                 GridApiDataSet.UGridAttributeConstants.Names.Location)
                                  : secondDimensionName; // backwards compatibility

            string unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);

            // Depending on the NetCdfVariable, Sediment dimension can be SedSus (suspended) or SedTot (total)
            List<string> dimensionNameList = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();
            int sedSusVarIndex = dimensionNameList.IndexOf(NSedSusName);
            int sedTotVarIndex = dimensionNameList.IndexOf(NSedTotName);

            if ((sedSusVarIndex != -1 || sedTotVarIndex != -1) && dimensions.Count == 3)
            {
                //Process variable as three dimensional time dependent variable
                int sedimentDimensionIndex = Math.Max(sedTotVarIndex, sedSusVarIndex);
                foreach (UnstructuredGridCoverage unstructuredGridCoverage in
                    ProcessThreeDimensionalTimeDependentVariable(timeDependentVariable, dimensions,
                                                                 sedimentDimensionIndex, location, coverageLongName,
                                                                 netCdfVariableName, unitSymbol))
                {
                    yield return unstructuredGridCoverage;
                }

                yield break;
            }

            coverage = CreateCoverage(location, coverageLongName);

            if (coverage != null)
            {
                InitializeTwoDimensionalCoverage(coverage, secondDimensionName, netCdfVariableName, unitSymbol,
                                                 timeDependentVariable.ReferenceDate);
            }

            string standardName =
                netCdfFile.GetAttributeValue(timeDependentVariable.NetCdfDataVariable, StandardNameAttribute);

            if (standardName == EastwardSeaWaterVelocityStandardName ||
                standardName == NorthwardSeaWaterVelocityStandardName ||
                standardName == SeaWaterXVelocityStandardName ||
                standardName == SeaWaterYVelocityStandardName) // Backwards compatibility *ugh*
            {
                velocityCoverages[standardName] = coverage;
            }

            yield return coverage;
        }

        private IEnumerable<UnstructuredGridCoverage> ProcessThreeDimensionalTimeDependentVariable(
            NetCdfVariableInfo timeDependentVariable, IList<NetCdfDimension> dimensions, int sedimentDimensionIndex,
            string location, string coverageLongName, string netCdfVariableName, string unitSymbol)
        {
            int numberOfSedLayers = netCdfFile.GetDimensionLength(dimensions[sedimentDimensionIndex]);

            for (var index = 0; index < numberOfSedLayers; index++)
            {
                // TODO : Replace index with values (i.e. sediment names) - this is not currently available in the map file
                UnstructuredGridCoverage sedCoverage = CreateCoverage(location, coverageLongName, index);
                if (sedCoverage != null)
                {
                    string secondDimensionName = netCdfFile.GetDimensionName(sedimentDimensionIndex != 1 ? dimensions[1] : dimensions[2]);
                    InitializeTwoDimensionalCoverage(sedCoverage, secondDimensionName, netCdfVariableName, unitSymbol, timeDependentVariable.ReferenceDate);
                    AddAdditionalDimensionAttributes(sedCoverage, new[]
                    {
                        new Tuple<string, string>(SedIndexAttributeName, index.ToString())
                    });
                }

                yield return sedCoverage;
            }
        }

        private void UpdateGrid()
        {
            // import the grid from the map file if there is no model grid available
            Grid = UnstructuredGridFileHelper.LoadFromFile(netCdfFile.Path, true);
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
    }
}