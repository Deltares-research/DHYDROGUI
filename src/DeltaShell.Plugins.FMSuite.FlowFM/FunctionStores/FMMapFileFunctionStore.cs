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
                NetCdfVariable netcdfVariable = GetNetcdfVariable(variable);
                List<NetCdfDimension> dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();
                netcdfVariableDimensionLength = GetDimensionScalingFactor(dimensions);
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
            if (dimensions.Count == 4)
            {
                return; // Temporarily do nothing with 4D cases.
            }

            if (HasSedimentDimensions(dimensions) && dimensions.Count != 3)
            {
                throw new Exception("Number of dimensions is wrong");
            }

            var sedIndex = 0;
            IFunction coverage = GetCoverageFunction(function);
            if (!int.TryParse(coverage.Attributes[SedIndexAttributeName], out sedIndex))
            {
                throw new Exception("Sediment Index is not of integer type");
            }

            int dimensionIndex = GetThirdDimensionIndex(dimensions);
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

        /// <summary>
        /// Gets the scaling factor along additional dimensions.
        /// </summary>
        /// <param name="dimensions">The collection of <see cref="NetCdfDimension"/> to determine the scaling factor for.</param>
        /// <returns>An integer representing the scaling factor along the axes of the collection of dimensions.</returns>
        private int GetDimensionScalingFactor(IEnumerable<NetCdfDimension> dimensions)
        {
            var scalingFactor = 1;
            if (dimensions.Count() == 3)
            {
                scalingFactor = GetThreeDimensionalScalingFactor(dimensions);
            }

            return scalingFactor;
        }

        /// <summary>
        /// Gets the scaling factor along a third dimensional axis.
        /// </summary>
        /// <param name="dimensions">The collection of <see cref="NetCdfDimension"/> to determine the scaling factor for.</param>
        /// <returns>An integer representing the scaling factor along the third dimension.</returns>
        /// <exception cref="Exception">Thrown when the scaling factor could not be determined.</exception>
        private int GetThreeDimensionalScalingFactor(IEnumerable<NetCdfDimension> dimensions)
        {
            string[] dimensionNames = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToArray();
            if (HasSedimentDimensions(dimensions))
            {
                return GetSedimentDimensionScalingFactor(dimensionNames);
            }

            if (HasBedLayerDimensions(dimensions))
            {
                return GetBedLayerScalingFactor(dimensionNames);
            }

            throw new Exception($"Scaling factor could not be determined. Supported dimensions: {NSedSusName}, {NSedTotName}, {NBedLayersName}");
        }

        /// <summary>
        /// Gets the dimensional scaling factor for NetCdf variables with sediment layer related dimensions.  
        /// </summary>
        /// <param name="dimensionNames">The names of the dimensions that are present.</param>
        /// <returns>A scaling factor along the sediment related dimension.</returns>
        /// <exception cref="Exception">Thrown when the scaling factor could not be determined.</exception>
        private int GetSedimentDimensionScalingFactor(IEnumerable<string> dimensionNames)
        {
            if (dimensionNames.Contains(NSedSusName))
            {
                return netCdfFile.GetDimensionLength(NSedSusName);
            }

            if (dimensionNames.Contains(NSedTotName))
            {
                return netCdfFile.GetDimensionLength(NSedTotName);
            }

            throw new Exception("Sediment related dimension not present.");
        }

        /// <summary>
        /// Gets the dimensional scaling factor for NetCdf variables with bed layer dimensions.  
        /// </summary>
        /// <param name="dimensionNames">The names of the dimensions that are present.</param>
        /// <returns>A scaling factor along the bed layer related dimension.</returns>
        /// <exception cref="Exception">Thrown when the scaling factor could not be determined.</exception>
        private int GetBedLayerScalingFactor(IEnumerable<string> dimensionNames)
        {
            if (dimensionNames.Contains(NBedLayersName))
            {
                return netCdfFile.GetDimensionLength(NBedLayersName);
            }

            throw new Exception("Bed layer related dimension not present.");
        }

        /// <summary>
        /// Gets the index of the third dimension in a collection of <see cref="NetCdfDimension"/>.
        /// </summary>
        /// <param name="dimensions">The collection of <see cref="NetCdfDimension"/> to retrieve the index of the third dimension from.</param>
        /// <returns>The index of the third dimension.</returns>
        /// <exception cref="Exception">Thrown when the third dimension could not be found.</exception>
        private int GetThirdDimensionIndex(IEnumerable<NetCdfDimension> dimensions)
        {
            if (HasSedimentDimensions(dimensions))
            {
                return GetSedimentDimensionIndex(dimensions);
            }

            if (HasBedLayerDimensions(dimensions))
            {
                return  GetBedLayersDimensionIndex(dimensions);
            }

            throw new Exception("Dimension Index could not be determined.");
        }

        private int GetBedLayersDimensionIndex(IEnumerable<NetCdfDimension> dimensions)
        {
            return GetDimensionIndex(dimensions, NBedLayersName);
        }

        private int GetSedimentDimensionIndex(IEnumerable<NetCdfDimension> dimensions)
        {
            int sedSusVarIndex = GetDimensionIndex(dimensions, NSedSusName);
            int sedTotVarIndex = GetDimensionIndex(dimensions, NSedTotName);

            return Math.Max(sedTotVarIndex, sedSusVarIndex);
        }

        /// <summary>
        /// Gets the index of a specific dimension name.
        /// </summary>
        /// <param name="dimensions">The collection of <see cref="NetCdfDimension"/> dimensions to retrieve the index from.</param>
        /// <param name="dimensionName">The name of the dimension to retrieve the index for.</param>
        /// <returns>The index of the <paramref name="dimensionName"/> in <paramref name="dimensions"/>, -1 if it could not be found.</returns>
        private int GetDimensionIndex(IEnumerable<NetCdfDimension> dimensions, string dimensionName)
        {
            List<string> dimensionNames = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();
            return dimensionNames.IndexOf(dimensionName);
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
                    log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverage_CannotCreateSpatialDataOnVolumeLocation,
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
        /// <param name="additionalDimensionAttributes">The collection of <see cref="Tuple"/> of attributes to add.</param>
        private static void AddAdditionalDimensionAttributes(IFunction function, IEnumerable<Tuple<string, string>> additionalDimensionAttributes)
        {
            // Allowing us to add additional attributes
            if (additionalDimensionAttributes != null)
            {
                foreach (Tuple<string, string> additionalAttribute in additionalDimensionAttributes)
                {
                    if (string.IsNullOrEmpty(additionalAttribute.Item1))
                    {
                        continue;
                    }

                    function.Attributes[additionalAttribute.Item1] =
                        additionalAttribute.Item2;
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

            //Process variable as three dimensional time dependent variable
            if (dimensions.Count == 3)
            {
                IEnumerable<UnstructuredGridCoverage> threeDimensionVariables = ProcessThreeDimensionalTimeDependentVariable(timeDependentVariable,
                                                                                                                             dimensions,
                                                                                                                             location,
                                                                                                                             coverageLongName,
                                                                                                                             netCdfVariableName,
                                                                                                                             unitSymbol);
                foreach (UnstructuredGridCoverage unstructuredGridCoverage in threeDimensionVariables)
                {
                    yield return unstructuredGridCoverage;
                }

                yield break;
            }

            if (dimensions.Count == 4)
            {
                IEnumerable<UnstructuredGridCoverage> fourDimensionVariables = ProcessFourDimensionalTimeDependentVariable(timeDependentVariable,
                                                                                                                           dimensions,
                                                                                                                           location,
                                                                                                                           coverageLongName,
                                                                                                                           netCdfVariableName,
                                                                                                                           unitSymbol).ToArray();
                foreach (UnstructuredGridCoverage unstructuredGridCoverage in fourDimensionVariables)
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

        private IEnumerable<UnstructuredGridCoverage> ProcessFourDimensionalTimeDependentVariable(NetCdfVariableInfo timeDependentVariable,
                                                                                                  List<NetCdfDimension> dimensions,
                                                                                                  string location,
                                                                                                  string coverageLongName,
                                                                                                  string netCdfVariableName,
                                                                                                  string unitSymbol)
        {
            if (HasSedimentDimensions(dimensions) && HasBedLayerDimensions(dimensions))
            {
                // Depending on the NetCdfVariable, Sediment dimension can be SedSus (suspended) or SedTot (total)
                List<string> dimensionNameList = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();
                int sedSusVarIndex = dimensionNameList.IndexOf(NSedSusName);
                int sedTotVarIndex = dimensionNameList.IndexOf(NSedTotName);

                int sedimentDimensionIndex = Math.Max(sedTotVarIndex, sedSusVarIndex);
                string dimensionName = netCdfFile.GetDimensionName(sedimentDimensionIndex != 1 ? dimensions[1] : dimensions[2]);

                int nBedLayersIndex = dimensionNameList.IndexOf(NBedLayersName);
                var primaryAxis = dimensions[nBedLayersIndex];
                var secondAxis = dimensions[sedimentDimensionIndex];

                return ProcessFourDimensionalTimeDependentVariable(timeDependentVariable,
                                                                   primaryAxis,
                                                                   secondAxis,
                                                                   string.Empty,
                                                                   dimensionName,
                                                                   location,
                                                                   coverageLongName,
                                                                   netCdfVariableName,
                                                                   unitSymbol,
                                                                   new[]
                                                                   {
                                                                       "PrimaryAxis"
                                                                   },
                                                                   new[]
                                                                   {
                                                                       "SecondaryAxis"
                                                                   });
            }

            return Enumerable.Empty<UnstructuredGridCoverage>();
        }

        /// <summary>
        /// Gets an indicator whether the collection of <see cref="NetCdfDimension"/> contains a sediment related dimension.
        /// </summary>
        /// <param name="dimensions">The dimensions to determine from.</param>
        /// <returns><c>true</c> if the collection contains a sediment related dimension, <c>false</c> otherwise.</returns>
        private bool HasSedimentDimensions(IEnumerable<NetCdfDimension> dimensions)
        {
            string[] dimensionNames = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToArray();
            return dimensionNames.Contains(NSedSusName) || dimensionNames.Contains(NSedTotName);
        }

        /// <summary>
        /// Gets an indicator whether the collection of <see cref="NetCdfDimension"/> contains a bed layer related dimension.
        /// </summary>
        /// <param name="dimensions">The dimensions to determine from.</param>
        /// <returns><c>true</c> if the collection contains a bed layer related dimension, <c>false</c> otherwise.</returns>
        private bool HasBedLayerDimensions(IEnumerable<NetCdfDimension> dimensions)
        {
            string[] dimensionNames = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToArray();
            return dimensionNames.Contains(NBedLayersName);
        }

        /// <summary>
        /// Process three dimensional time dependent sediment variables by creating additional <see cref="UnstructuredGridCoverage"/>
        /// along the third dimensional axis for each value.
        /// </summary>
        /// <param name="timeDependentVariable">The <see cref="NetCdfVariableInfo"/>  to process.</param>
        /// <param name="dimensions">The dimension to process along to.</param>
        /// <param name="location">The name of the location which is processed.</param>
        /// <param name="coverageLongName">The long name of the <paramref name="timeDependentVariable"/>.</param>
        /// <param name="netCdfVariableName">The name of the variable.</param>
        /// <param name="unitSymbol">The unit symbol of the variable.</param>
        /// <returns>A collection of <see cref="UnstructuredGridCoverage"/> along the third axis.</returns>
        private IEnumerable<UnstructuredGridCoverage> ProcessThreeDimensionalTimeDependentSedimentVariable(NetCdfVariableInfo timeDependentVariable,
                                                                                                           IReadOnlyList<NetCdfDimension> dimensions,
                                                                                                           string location,
                                                                                                           string coverageLongName,
                                                                                                           string netCdfVariableName,
                                                                                                           string unitSymbol)
        {
            // Depending on the NetCdfVariable, Sediment dimension can be SedSus (suspended) or SedTot (total)
            List<string> dimensionNameList = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();
            int sedSusVarIndex = dimensionNameList.IndexOf(NSedSusName);
            int sedTotVarIndex = dimensionNameList.IndexOf(NSedTotName);

            int sedimentDimensionIndex = Math.Max(sedTotVarIndex, sedSusVarIndex);
            string dimensionName = netCdfFile.GetDimensionName(sedimentDimensionIndex != 1 ? dimensions[1] : dimensions[2]);
            foreach (UnstructuredGridCoverage unstructuredGridCoverage in ProcessThreeDimensionalTimeDependentVariable(timeDependentVariable, dimensions[sedimentDimensionIndex],
                                                                                                                       dimensionName, location, coverageLongName,
                                                                                                                       netCdfVariableName, unitSymbol, new[]
                                                                                                                       {
                                                                                                                           SedIndexAttributeName
                                                                                                                       }))
            {
                yield return unstructuredGridCoverage;
            }
        }

        /// <summary>
        /// Process three dimensional time dependent bed layer variables by creating additional <see cref="UnstructuredGridCoverage"/>
        /// along the third dimensional axis for each value.
        /// </summary>
        /// <param name="timeDependentVariable">The <see cref="NetCdfVariableInfo"/>  to process.</param>
        /// <param name="dimensions">The dimension to process along to.</param>
        /// <param name="location">The name of the location which is processed.</param>
        /// <param name="coverageLongName">The long name of the <paramref name="timeDependentVariable"/>.</param>
        /// <param name="netCdfVariableName">The name of the variable.</param>
        /// <param name="unitSymbol">The unit symbol of the variable.</param>
        /// <returns>A collection of <see cref="UnstructuredGridCoverage"/> along the third axis.</returns>
        private IEnumerable<UnstructuredGridCoverage> ProcessThreeDimensionalTimeDependentBedLayersVariable(NetCdfVariableInfo timeDependentVariable,
                                                                                                            IReadOnlyList<NetCdfDimension> dimensions,
                                                                                                            string location,
                                                                                                            string coverageLongName,
                                                                                                            string netCdfVariableName,
                                                                                                            string unitSymbol)
        {
            List<string> dimensionNameList = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();
            int nBedLayersIndex = dimensionNameList.IndexOf(NBedLayersName);

            string dimensionName = netCdfFile.GetDimensionName(nBedLayersIndex != 1 ? dimensions[1] : dimensions[2]);
            foreach (UnstructuredGridCoverage unstructuredGridCoverage in ProcessThreeDimensionalTimeDependentVariable(timeDependentVariable, dimensions[nBedLayersIndex],
                                                                                                                       dimensionName, location, coverageLongName,
                                                                                                                       netCdfVariableName, unitSymbol, new[]
                                                                                                                       {
                                                                                                                           SedIndexAttributeName
                                                                                                                       }))
            {
                yield return unstructuredGridCoverage;
            }
        }

        /// <summary>
        /// Process three dimensional time dependent variables by creating additional <see cref="UnstructuredGridCoverage"/>
        /// along the third dimensional axis for each value.
        /// </summary>
        /// <param name="timeDependentVariable">The <see cref="NetCdfVariableInfo"/>  to process.</param>
        /// <param name="dimensions">The dimension to process along to.</param>
        /// <param name="location">The name of the location which is processed.</param>
        /// <param name="coverageLongName">The long name of the <paramref name="timeDependentVariable"/>.</param>
        /// <param name="netCdfVariableName">The name of the variable.</param>
        /// <param name="unitSymbol">The unit symbol of the variable.</param>
        /// <returns>A collection of <see cref="UnstructuredGridCoverage"/> along the third axis; empty if it could not be processed.</returns>
        private IEnumerable<UnstructuredGridCoverage> ProcessThreeDimensionalTimeDependentVariable(NetCdfVariableInfo timeDependentVariable,
                                                                                                   IReadOnlyList<NetCdfDimension> dimensions,
                                                                                                   string location,
                                                                                                   string coverageLongName,
                                                                                                   string netCdfVariableName,
                                                                                                   string unitSymbol)
        {
            if (HasSedimentDimensions(dimensions))
            {
                return ProcessThreeDimensionalTimeDependentSedimentVariable(timeDependentVariable, dimensions, location, coverageLongName, netCdfVariableName, unitSymbol);
            }

            if (HasBedLayerDimensions(dimensions))
            {
                return ProcessThreeDimensionalTimeDependentBedLayersVariable(timeDependentVariable, dimensions, location, coverageLongName, netCdfVariableName, unitSymbol);
            }

            return Enumerable.Empty<UnstructuredGridCoverage>();
        }

        /// <summary>
        /// Process three dimensional time dependent variables by creating additional <see cref="UnstructuredGridCoverage"/>
        /// along the third dimensional axis for each value.
        /// </summary>
        /// <param name="timeDependentVariable">The <see cref="NetCdfVariableInfo"/>  to process.</param>
        /// <param name="dimension">The dimension to process along to.</param>
        /// <param name="dimensionName">The name of the third dimensional axis to create the collection of
        /// <see cref="UnstructuredGridCoverage"/> for.</param>
        /// <param name="location">The name of the location which is processed.</param>
        /// <param name="coverageLongName">The long name of the <paramref name="timeDependentVariable"/>.</param>
        /// <param name="netCdfVariableName">The name of the variable.</param>
        /// <param name="unitSymbol">The unit symbol of the variable.</param>
        /// <param name="additionalAttributeNames">The collection of attribute names to add for each created separate <see cref="UnstructuredGridCoverage"/>.</param>
        /// <returns>A collection of <see cref="UnstructuredGridCoverage"/> along the third axis.</returns>
        private IEnumerable<UnstructuredGridCoverage> ProcessThreeDimensionalTimeDependentVariable(NetCdfVariableInfo timeDependentVariable,
                                                                                                   NetCdfDimension dimension,
                                                                                                   string dimensionName,
                                                                                                   string location,
                                                                                                   string coverageLongName,
                                                                                                   string netCdfVariableName,
                                                                                                   string unitSymbol,
                                                                                                   IEnumerable<string> additionalAttributeNames)
        {
            // Get the number of entries along a dimension
            int dimensionLength = netCdfFile.GetDimensionLength(dimension);
            for (var index = 0; index < dimensionLength; index++)
            {
                // TODO : Replace index with values (i.e. sediment names) - this is not currently available in the map file
                UnstructuredGridCoverage coverage = CreateCoverage(location, coverageLongName, index);
                if (coverage != null)
                {
                    InitializeTwoDimensionalCoverage(coverage, dimensionName, netCdfVariableName, unitSymbol, timeDependentVariable.ReferenceDate);

                    Tuple<string, string>[] additionalAttributes = additionalAttributeNames.Select(attributeName => new Tuple<string, string>(attributeName, index.ToString()))
                                                                                           .ToArray();
                    AddAdditionalDimensionAttributes(coverage, additionalAttributes);
                }

                yield return coverage;
            }
        }

        private IEnumerable<UnstructuredGridCoverage> ProcessFourDimensionalTimeDependentVariable(NetCdfVariableInfo timeDependentVariable,
                                                                                                  NetCdfDimension firstDimension,
                                                                                                  NetCdfDimension secondDimension,
                                                                                                  string firstDimensionName,
                                                                                                  string secondDimensionName,
                                                                                                  string location,
                                                                                                  string coverageLongName,
                                                                                                  string netCdfVariableName,
                                                                                                  string unitSymbol,
                                                                                                  IEnumerable<string> additionalAttributeNamesFirstDimension,
                                                                                                  IEnumerable<string> additionalAttributeNamesSecondDimension)
        {
            int dimensionLength = netCdfFile.GetDimensionLength(firstDimension);
            for (var index = 0; index < dimensionLength; index++)
            {
                var coverageName = $"{coverageLongName} ({index})";

                IEnumerable<UnstructuredGridCoverage> coverageAlongSideSecondAxis =
                    ProcessThreeDimensionalTimeDependentVariable(timeDependentVariable,
                                                                 secondDimension,
                                                                 secondDimensionName,
                                                                 location,
                                                                 coverageName,
                                                                 netCdfVariableName,
                                                                 unitSymbol,
                                                                 additionalAttributeNamesSecondDimension);
                foreach (var coverage in coverageAlongSideSecondAxis)
                {
                    Tuple<string, string>[] additionalAttributes =
                        additionalAttributeNamesFirstDimension.Select(attributeName => new Tuple<string, string>(attributeName, index.ToString()))
                                                              .ToArray();
                    AddAdditionalDimensionAttributes(coverage, additionalAttributes);

                    yield return coverage;
                }
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
        private const string NBedLayersName = "nBedLayers";
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