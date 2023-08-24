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
            boundaryCellValues.Clear();

            var fileOperations = new UnstructuredGridFileOperations(netCdfFile.Path);
            Grid = fileOperations.GetGrid(true);
            bool isUgridConvention = fileOperations.DataSetConventions == GridApiDataSet.DataSetConventions.CONV_UGRID;

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

            if (coverage == null)
            {
                return variableValuesCount;
            }

            var netcdfVariableDimensionLength = 1;

            using (ReconnectToMapFile())
            {
                NetCdfVariable netcdfVariable = GetNetcdfVariable(variable);
                NetCdfDimension[] dimensions = netCdfFile.GetDimensions(netcdfVariable).ToArray();
                netcdfVariableDimensionLength = GetDimensionScalingFactor(dimensions);
            }

            return variableValuesCount / netcdfVariableDimensionLength;
        }

        protected override void GetShapeAndOrigin(IVariable function, IVariableFilter[] filters, out int[] shape,
                                                  out int[] origin, out int[] stride)
        {
            base.GetShapeAndOrigin(function, filters, out shape, out origin, out stride);

            IFunction coverageFunction = GetCoverageFunction(function);
            if (function.IsIndependent || coverageFunction == null)
            {
                return;
            }

            NetCdfVariable netcdfVariable = GetNetcdfVariable(function);
            NetCdfDimension[] dimensions = netCdfFile.GetDimensions(netcdfVariable).ToArray();
            if (dimensions.Length == 3)
            {
                SetThreeDimensionalProperties(function, dimensions, filters, ref shape, ref origin, ref stride);
            }
            else if (dimensions.Length == 4)
            {
                SetFourDimensionalProperties(function, dimensions, filters, ref shape, ref origin, ref stride);
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
            NetCdfVariable netcdfVariable = GetNetcdfVariable(function);
            IFunction coverage = GetCoverageFunction(function);
            int nrOfDimensions = netCdfFile.GetDimensions(netcdfVariable).Count();
            if (coverage != null && !function.IsIndependent && nrOfDimensions >= 3)
            {
                // Create filter in case it is a multidimensionsal variable.
                filters = CreateFilters(function, filters, 0);
            }

            try
            {
                return base.GetVariableValuesCore<T>(function, filters);
            }
            catch (Exception e) when (e.Message.Contains("NetCDF error code"))
            {
                log.Error(string.Format(Resources.FMMapFileFunctionStore_GetVariableValuesCore_While_reading_variable__0__from_the_file__1__an_error_was_encountered___2_, function.Name, System.IO.Path.GetFileName(Path), e.Message));
                int functionSize = GetSize(function);
                return new MultiDimensionalArray<T>(new List<T>(new T[functionSize]), new[] { functionSize });
            }
        }

        /// <summary>
        /// Sets the shape, origin and stride properties based on the input arguments for a three dimensional variable.
        /// </summary>
        /// <param name="function">The <see cref="IVariable{T}"/> to base the properties for.</param>
        /// <param name="dimensions">The dimensions to base the properties on.</param>
        /// <param name="filters">The filters to apply on the <paramref name="function"/>.</param>
        /// <param name="shape">The array to set the shape values for.</param>
        /// <param name="origin">The array to set the origin values for.</param>
        /// <param name="stride">The array to set the shape values for.</param>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///         The shape describes how the data is structured in each dimension. (Generally the number of elements
        ///         across every dimension)
        ///         </item>
        ///         <item>
        ///         The origin describes where to start looking for the the data in each dimension. (Generally value 1 across the
        ///         dimensions,
        ///         and value n being the corresponding slice along the third dimensional axis)
        ///         </item>
        ///         <item>
        ///         The stride describes the offset from each looked up value in each dimension (generally value 1 across all
        ///         dimensions)
        ///         </item>
        ///     </list>
        /// </remarks>
        private void SetThreeDimensionalProperties(IVariable function,
                                                   IEnumerable<NetCdfDimension> dimensions,
                                                   IReadOnlyCollection<IVariableFilter> filters,
                                                   ref int[] shape,
                                                   ref int[] origin,
                                                   ref int[] stride)
        {
            int dimensionIndex = GetThirdDimensionIndex(dimensions);
            int originValue = GetOrigin(function, PrimaryAxisAttributeName);
            SetShapeAndOriginProperties(filters, dimensionIndex, originValue, ref shape, ref origin, ref stride);
        }

        /// <summary>
        /// Sets the shape, origin and stride properties based on the input arguments for a four dimensional variable.
        /// </summary>
        /// <param name="function">The <see cref="IVariable{T}"/> to base the properties for.</param>
        /// <param name="dimensions">The dimensions to base the properties on.</param>
        /// <param name="filters">The filters to apply on the <paramref name="function"/>.</param>
        /// <param name="shape">The array to set the shape values for.</param>
        /// <param name="origin">The array to set the origin values for.</param>
        /// <param name="stride">The array to set the shape values for.</param>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///         The shape describes how the data is structured in each dimension. (Generally the number of elements
        ///         across every dimension)
        ///         </item>
        ///         <item>
        ///         The origin describes where to start looking for the the data in each dimension. (Generally value 1 across the
        ///         dimensions,
        ///         and value n being the corresponding slice along the third dimensional axis and m being the corresponding slice
        ///         along the fourth dimensional axis)
        ///         </item>
        ///         <item>
        ///         The stride describes the offset from each looked up value in each dimension (generally value 1 across all
        ///         dimensions)
        ///         </item>
        ///     </list>
        /// </remarks>
        private void SetFourDimensionalProperties(IVariable function,
                                                  IEnumerable<NetCdfDimension> dimensions,
                                                  IReadOnlyCollection<IVariableFilter> filters,
                                                  ref int[] shape,
                                                  ref int[] origin,
                                                  ref int[] stride)
        {
            int secondaryDimensionIndex = GetSedimentDimensionIndex(dimensions);
            int originValueAlongSediments = GetOrigin(function, SecondaryAxisAttributeName);
            SetShapeAndOriginProperties(filters, secondaryDimensionIndex, originValueAlongSediments, ref shape, ref origin, ref stride);

            int primaryDimensionIndex = GetBedLayersDimensionIndex(dimensions);
            int originValueAlongBedLayer = GetOrigin(function, PrimaryAxisAttributeName);
            SetShapeAndOriginProperties(filters, primaryDimensionIndex, originValueAlongBedLayer, ref shape, ref origin, ref stride);
        }

        /// <summary>
        /// Sets the shape, origin and stride properties based on the input arguments.
        /// </summary>
        /// <param name="filters">The filters to apply.</param>
        /// <param name="dimensionIndex">The index of the dimension to set the shape, origin and the stride value for.</param>
        /// <param name="originValue">The value to set for the origin at the <paramref name="dimensionIndex"/>.</param>
        /// <param name="shape">The array to set the shape values for.</param>
        /// <param name="origin">The array to set the origin values for.</param>
        /// <param name="stride">The array to set the shape values for.</param>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///         The shape describes how the data is structured in each dimension. (Generally the number of elements
        ///         across every dimension)
        ///         </item>
        ///         <item>
        ///         The origin describes where to start looking for the the data in each dimension. (Generally value 1 across the
        ///         dimensions,
        ///         and value n being the corresponding slice along the third dimensional axis)
        ///         </item>
        ///         <item>
        ///         The stride describes the offset from each looked up value in each dimension (generally value 1 across all
        ///         dimensions)
        ///         </item>
        ///     </list>
        /// </remarks>
        private void SetShapeAndOriginProperties(IReadOnlyCollection<IVariableFilter> filters, int dimensionIndex, int originValue,
                                                 ref int[] shape, ref int[] origin, ref int[] stride)
        {
            const int strideValue = 1;
            const int shapeValue = 1;
            if (filters.Count == 0)
            {
                shape[dimensionIndex] = shapeValue;
                origin[dimensionIndex] = originValue;
                stride[dimensionIndex] = strideValue;
            }
            else
            {
                shape = InsertItem(shape, dimensionIndex, shapeValue);
                origin = InsertItem(origin, dimensionIndex, originValue);
                stride = InsertItem(stride, dimensionIndex, strideValue);
            }
        }

        /// <summary>
        /// Gets the origin based on an <see cref="IVariable"/> and the attribute name.
        /// </summary>
        /// <param name="function">The function to retrieve the origin for.</param>
        /// <param name="attributeName">The attribute name to retrieve the origin from.</param>
        /// <returns>An integer representing the origin.</returns>
        /// <exception cref="NetCdfFileParsingException">Thrown when the origin could not be determined.</exception>
        private int GetOrigin(IVariable function, string attributeName)
        {
            var origin = 0;
            IFunction coverage = GetCoverageFunction(function);
            if (!int.TryParse(coverage.Attributes[attributeName], out origin))
            {
                throw new NetCdfFileParsingException("Index is not of integer type");
            }

            return origin;
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

            if (dimensions.Count() == 4)
            {
                scalingFactor = GetFourDimensionalScalingFactor(dimensions);
            }

            return scalingFactor;
        }

        /// <summary>
        /// Gets the scaling factor along a third dimensional axis.
        /// </summary>
        /// <param name="dimensions">The collection of <see cref="NetCdfDimension"/> to determine the scaling factor for.</param>
        /// <returns>An integer representing the scaling factor along the third dimension.</returns>
        /// <exception cref="NetCdfFileParsingException">Thrown when the scaling factor could not be determined.</exception>
        private int GetThreeDimensionalScalingFactor(IEnumerable<NetCdfDimension> dimensions)
        {
            IEnumerable<string> dimensionNames = GetDimensionNames(dimensions);
            if (HasSedimentDimensions(dimensions))
            {
                return GetSedimentDimensionScalingFactor(dimensionNames);
            }

            if (HasBedLayerDimensions(dimensions))
            {
                return GetBedLayerScalingFactor(dimensionNames);
            }

            throw new NetCdfFileParsingException($"Scaling factor could not be determined. Supported dimensions: {NSedSusName}, {NSedTotName}, {NBedLayersName}");
        }

        /// <summary>
        /// Gets the scaling factor along a third and fourth dimensional axis.
        /// </summary>
        /// <param name="dimensions">The collection of <see cref="NetCdfDimension"/> to determine the scaling factor for.</param>
        /// <returns>An integer representing the scaling factor along the third and fourth dimension.</returns>
        /// <exception cref="NetCdfFileParsingException">Thrown when the scaling factor could not be determined.</exception>
        private int GetFourDimensionalScalingFactor(IEnumerable<NetCdfDimension> dimensions)
        {
            IEnumerable<string> dimensionNames = GetDimensionNames(dimensions);
            if (HasSedimentDimensions(dimensions) && HasBedLayerDimensions(dimensions))
            {
                return GetSedimentDimensionScalingFactor(dimensionNames) * GetBedLayerScalingFactor(dimensionNames);
            }

            throw new NetCdfFileParsingException($"Scaling factor could not be determined. Supported dimensions: {NSedSusName}, {NSedTotName}, {NBedLayersName}");
        }

        /// <summary>
        /// Retrieve the names of the dimensions based on a collection of <see cref="NetCdfDimension"/>.
        /// </summary>
        /// <param name="dimensions">The collection of <see cref="NetCdfDimension"/> to retrieve the names from.</param>
        /// <returns>A collection of the names of the dimensions.</returns>
        private IEnumerable<string> GetDimensionNames(IEnumerable<NetCdfDimension> dimensions)
        {
            return dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToArray();
        }

        /// <summary>
        /// Gets the dimensional scaling factor for NetCdf variables with sediment layer related dimensions.
        /// </summary>
        /// <param name="dimensionNames">The names of the dimensions that are present.</param>
        /// <returns>A scaling factor along the sediment related dimension.</returns>
        /// <exception cref="NetCdfFileParsingException">Thrown when the scaling factor could not be determined.</exception>
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

            throw new NetCdfFileParsingException("Sediment related dimension not present.");
        }

        /// <summary>
        /// Gets the dimensional scaling factor for NetCdf variables with bed layer dimensions.
        /// </summary>
        /// <param name="dimensionNames">The names of the dimensions that are present.</param>
        /// <returns>A scaling factor along the bed layer related dimension.</returns>
        /// <exception cref="NetCdfFileParsingException">Thrown when the scaling factor could not be determined.</exception>
        private int GetBedLayerScalingFactor(IEnumerable<string> dimensionNames)
        {
            if (dimensionNames.Contains(NBedLayersName))
            {
                return netCdfFile.GetDimensionLength(NBedLayersName);
            }

            throw new NetCdfFileParsingException("Bed layer related dimension not present.");
        }

        /// <summary>
        /// Gets the index of the primary axis dimension in three dimensional case based on a collection of
        /// <see cref="NetCdfDimension"/>.
        /// </summary>
        /// <param name="dimensions">
        /// The collection of <see cref="NetCdfDimension"/> to retrieve the index of the third dimension
        /// from.
        /// </param>
        /// <returns>The index of the third dimension.</returns>
        /// <exception cref="NetCdfFileParsingException">Thrown when the third dimension could not be found.</exception>
        private int GetThirdDimensionIndex(IEnumerable<NetCdfDimension> dimensions)
        {
            const string exceptionMessage = "Dimension Index could not be determined.";
            if (dimensions.Count() != 3)
            {
                throw new NetCdfFileParsingException(exceptionMessage);
            }

            if (HasSedimentDimensions(dimensions))
            {
                return GetSedimentDimensionIndex(dimensions);
            }

            if (HasBedLayerDimensions(dimensions))
            {
                return GetBedLayersDimensionIndex(dimensions);
            }

            throw new NetCdfFileParsingException(exceptionMessage);
        }

        /// <summary>
        /// Gets the index of the bed layers dimension based on a collection of <see cref="NetCdfDimension"/>.
        /// </summary>
        /// <param name="dimensions">The collection to determine the index from.</param>
        /// <returns>
        /// The index of the bed layers dimension in the <paramref name="dimensions"/>, -1 if the dimension could
        /// not be found.
        /// </returns>
        private int GetBedLayersDimensionIndex(IEnumerable<NetCdfDimension> dimensions)
        {
            return GetDimensionIndex(dimensions, NBedLayersName);
        }

        /// <summary>
        /// Gets the index of the sediment dimension based on a collection of <see cref="NetCdfDimension"/>.
        /// </summary>
        /// <param name="dimensions">The collection to determine the index from.</param>
        /// <returns>
        /// The index of the sediment dimension in the <paramref name="dimensions"/>, -1 if the dimension could
        /// not be found.
        /// </returns>
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
        /// <returns>
        /// The index of the <paramref name="dimensionName"/> in <paramref name="dimensions"/>, -1 if it could not be
        /// found.
        /// </returns>
        private int GetDimensionIndex(IEnumerable<NetCdfDimension> dimensions, string dimensionName)
        {
            List<string> dimensionNames = GetDimensionNames(dimensions).ToList();
            return dimensionNames.IndexOf(dimensionName);
        }

        private IVariableFilter[] CreateFilters(IVariable function, IVariableFilter[] filters, int index)
        {
            IVariable variableToFilter = IsCoverageFunctionComponent(function)
                                             ? function
                                             : function.Components[0];

            var filter = new VariableIndexFilter(variableToFilter, index);
            Array.Resize(ref filters, filters.Length + 1);
            filters[filters.Length - 1] = filter;

            return filters;
        }

        /// <summary>
        /// Gets the NetCdf variable based on the function.
        /// </summary>
        /// <param name="function">The <see cref="IVariable"/> to get the NetCdf variable for.</param>
        /// <returns>The <see cref="NetCdfVariable"/> corresponding with the <paramref name="function"/>.</returns>
        /// <exception cref="NetCdfFileParsingException">Thrown when the <paramref name="function"/> does not have a name.</exception>
        private NetCdfVariable GetNetcdfVariable(IVariable function)
        {
            NetCdfVariable netcdfVariable = netCdfFile.GetVariableByName(function.Components[0].Attributes[NcNameAttribute]);
            if (netcdfVariable == null)
            {
                throw new NetCdfFileParsingException("Missing NetCdf name");
            }

            return netcdfVariable;
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
            Func<NetCdfVariableInfo, bool> timeDepVarSelectionCriteria = 
                isUgridConvention 
                    ? (Func<NetCdfVariableInfo, bool>)(v => v.IsTimeDependent && v.NumDimensions > 1) 
                    : (Func<NetCdfVariableInfo, bool>)(v => v.IsTimeDependent && v.NumDimensions > 1 && v.NumDimensions <= 2);
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

        /// <summary>
        /// Initializes a coverage with a two dimensions.
        /// </summary>
        /// <param name="function">The function to initialize the coverage for.</param>
        /// <param name="surfaceName">The name of the surface that is created for the coverage.</param>
        /// <param name="variableName">The name of the variable to initialize the coverage for.</param>
        /// <param name="unitSymbol">The symbol of the variable.</param>
        /// <param name="refDate">The reference date.</param>
        private void InitializeTwoDimensionalCoverage(IFunction function, string surfaceName, string variableName,
                                                      string unitSymbol, string refDate)
        {
            function.Store = this;

            IVariable timeDimension = function.Arguments[0];
            timeDimension.Name = "Time";
            timeDimension.Attributes[NcNameAttribute] = TimeVariableNames[0];
            timeDimension.Attributes[NcUseVariableSizeAttribute] = "true";
            timeDimension.Attributes[NcRefDateAttribute] = refDate;
            timeDimension.IsEditable = false;

            InitializeSurfaceProperties(function.Arguments[1], surfaceName);

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
        /// <param name="surfaceName">The name of the additional dimension.</param>
        private static void InitializeSurfaceProperties(IVariable variable, string surfaceName)
        {
            variable.Name = surfaceName;
            variable.Attributes[NcNameAttribute] = surfaceName;
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
            string surfaceName = netCdfFile.GetDimensionName(dimensions[1]);
            string longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute) ??
                              netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttribute);
            string coverageLongName = longName != null
                                          ? string.Format("{0} ({1})", longName, netCdfVariableName)
                                          : netCdfVariableName;

            string location = isUgridConvention
                                  ? netCdfFile.GetAttributeValue(netcdfVariable,
                                                                 GridApiDataSet.UGridAttributeConstants.Names.Location)
                                  : surfaceName; // backwards compatibility

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
                                                                                                                           unitSymbol);
                foreach (UnstructuredGridCoverage unstructuredGridCoverage in fourDimensionVariables)
                {
                    yield return unstructuredGridCoverage;
                }

                yield break;
            }

            coverage = CreateCoverage(location, coverageLongName);
            if (coverage != null)
            {
                InitializeTwoDimensionalCoverage(coverage, surfaceName, netCdfVariableName, unitSymbol,
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

        /// <summary>
        /// Gets an indicator whether the collection of <see cref="NetCdfDimension"/> contains a sediment related dimension.
        /// </summary>
        /// <param name="dimensions">The dimensions to determine from.</param>
        /// <returns><c>true</c> if the collection contains a sediment related dimension, <c>false</c> otherwise.</returns>
        private bool HasSedimentDimensions(IEnumerable<NetCdfDimension> dimensions)
        {
            IEnumerable<string> dimensionNames = GetDimensionNames(dimensions);
            return dimensionNames.Contains(NSedSusName) || dimensionNames.Contains(NSedTotName);
        }

        /// <summary>
        /// Gets an indicator whether the collection of <see cref="NetCdfDimension"/> contains a bed layer related dimension.
        /// </summary>
        /// <param name="dimensions">The dimensions to determine from.</param>
        /// <returns><c>true</c> if the collection contains a bed layer related dimension, <c>false</c> otherwise.</returns>
        private bool HasBedLayerDimensions(IEnumerable<NetCdfDimension> dimensions)
        {
            IEnumerable<string> dimensionNames = GetDimensionNames(dimensions);
            return dimensionNames.Contains(NBedLayersName);
        }

        /// <summary>
        /// Process three dimensional time dependent sediment variables by creating additional
        /// <see cref="UnstructuredGridCoverage"/>
        /// along the third dimensional axis for each value.
        /// </summary>
        /// <param name="timeDependentVariable">The <see cref="ReadOnlyNetCdfFunctionStoreBase.NetCdfVariableInfo"/>  to process.</param>
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
            List<string> dimensionNameList = GetDimensionNames(dimensions).ToList();
            int sedSusVarIndex = dimensionNameList.IndexOf(NSedSusName);
            int sedTotVarIndex = dimensionNameList.IndexOf(NSedTotName);

            int sedimentDimensionIndex = Math.Max(sedTotVarIndex, sedSusVarIndex);
            string dimensionName = netCdfFile.GetDimensionName(sedimentDimensionIndex != 1 ? dimensions[1] : dimensions[2]);
            foreach (UnstructuredGridCoverage unstructuredGridCoverage in ProcessThreeDimensionalTimeDependentVariable(timeDependentVariable, dimensions[sedimentDimensionIndex],
                                                                                                                       dimensionName, location, coverageLongName,
                                                                                                                       netCdfVariableName, unitSymbol, new[]
                                                                                                                       {
                                                                                                                           PrimaryAxisAttributeName
                                                                                                                       }))
            {
                yield return unstructuredGridCoverage;
            }
        }

        /// <summary>
        /// Process three dimensional time dependent bed layer variables by creating additional
        /// <see cref="UnstructuredGridCoverage"/>
        /// along the third dimensional axis for each value.
        /// </summary>
        /// <param name="timeDependentVariable">The <see cref="ReadOnlyNetCdfFunctionStoreBase.NetCdfVariableInfo"/>  to process.</param>
        /// <param name="dimensions">The collection of <see cref="NetCdfDimension"/> to process with.</param>
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
            List<string> dimensionNameList = GetDimensionNames(dimensions).ToList();
            int nBedLayersIndex = dimensionNameList.IndexOf(NBedLayersName);

            string surfaceName = netCdfFile.GetDimensionName(nBedLayersIndex != 1 ? dimensions[1] : dimensions[2]);
            foreach (UnstructuredGridCoverage unstructuredGridCoverage in ProcessThreeDimensionalTimeDependentVariable(timeDependentVariable, dimensions[nBedLayersIndex],
                                                                                                                       surfaceName, location, coverageLongName,
                                                                                                                       netCdfVariableName, unitSymbol, new[]
                                                                                                                       {
                                                                                                                           PrimaryAxisAttributeName
                                                                                                                       }))
            {
                yield return unstructuredGridCoverage;
            }
        }

        /// <summary>
        /// Process three dimensional time dependent variables by creating additional <see cref="UnstructuredGridCoverage"/>
        /// along the third dimensional axis for each value.
        /// </summary>
        /// <param name="timeDependentVariable">The <see cref="ReadOnlyNetCdfFunctionStoreBase.NetCdfVariableInfo"/>  to process.</param>
        /// <param name="dimensions">The collection of <see cref="NetCdfDimension"/> to process with.</param>
        /// <param name="location">The name of the location which is processed.</param>
        /// <param name="coverageLongName">The long name of the <paramref name="timeDependentVariable"/>.</param>
        /// <param name="netCdfVariableName">The name of the variable.</param>
        /// <param name="unitSymbol">The unit symbol of the variable.</param>
        /// <returns>
        /// A collection of <see cref="UnstructuredGridCoverage"/> along the third axis; empty if it could not be
        /// processed.
        /// </returns>
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
        /// <param name="timeDependentVariable">The <see cref="ReadOnlyNetCdfFunctionStoreBase.NetCdfVariableInfo"/>  to process.</param>
        /// <param name="dimension">The dimension to process along to.</param>
        /// <param name="surfaceName">The name of the surfaces for each entry along the <paramref name="dimension"/>.</param>
        /// <param name="location">The name of the location which is processed.</param>
        /// <param name="coverageLongName">The long name of the <paramref name="timeDependentVariable"/>.</param>
        /// <param name="netCdfVariableName">The name of the variable.</param>
        /// <param name="unitSymbol">The unit symbol of the variable.</param>
        /// <param name="additionalAttributeNames">
        /// The collection of attribute names to add for each created separate
        /// <see cref="UnstructuredGridCoverage"/>.
        /// </param>
        /// <returns>A collection of <see cref="UnstructuredGridCoverage"/> along the third axis.</returns>
        private IEnumerable<UnstructuredGridCoverage> ProcessThreeDimensionalTimeDependentVariable(NetCdfVariableInfo timeDependentVariable,
                                                                                                   NetCdfDimension dimension,
                                                                                                   string surfaceName,
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
                var coverageName = $"{coverageLongName} ({index})";
                UnstructuredGridCoverage coverage = CreateCoverage(location, coverageName);
                if (coverage != null)
                {
                    InitializeTwoDimensionalCoverage(coverage, surfaceName, netCdfVariableName, unitSymbol, timeDependentVariable.ReferenceDate);
                    Tuple<string, string>[] additionalAttributes = additionalAttributeNames.Select(attributeName => new Tuple<string, string>(attributeName, index.ToString()))
                                                                                           .ToArray();
                    AddAdditionalDimensionAttributes(coverage, additionalAttributes);
                }

                yield return coverage;
            }
        }

        /// <summary>
        /// Process three dimensional time dependent variables by creating additional <see cref="UnstructuredGridCoverage"/>
        /// along the third dimensional axis for each value.
        /// </summary>
        /// <param name="timeDependentVariable">The <see cref="ReadOnlyNetCdfFunctionStoreBase.NetCdfVariableInfo"/>  to process.</param>
        /// <param name="dimensions">The dimensions to process along with.</param>
        /// <param name="location">The name of the location which is processed.</param>
        /// <param name="coverageLongName">The long name of the <paramref name="timeDependentVariable"/>.</param>
        /// <param name="netCdfVariableName">The name of the variable.</param>
        /// <param name="unitSymbol">The unit symbol of the variable.</param>
        /// <returns>
        /// A collection of <see cref="UnstructuredGridCoverage"/> along the third and fourth axis; empty if it could not be
        /// processed.
        /// </returns>
        private IEnumerable<UnstructuredGridCoverage> ProcessFourDimensionalTimeDependentVariable(NetCdfVariableInfo timeDependentVariable,
                                                                                                  IReadOnlyList<NetCdfDimension> dimensions,
                                                                                                  string location,
                                                                                                  string coverageLongName,
                                                                                                  string netCdfVariableName,
                                                                                                  string unitSymbol)
        {
            if (HasSedimentDimensions(dimensions) && HasBedLayerDimensions(dimensions))
            {
                // Depending on the NetCdfVariable, Sediment dimension can be SedSus (suspended) or SedTot (total)
                List<string> dimensionNameList = GetDimensionNames(dimensions).ToList();
                int sedSusVarIndex = dimensionNameList.IndexOf(NSedSusName);
                int sedTotVarIndex = dimensionNameList.IndexOf(NSedTotName);

                int sedimentDimensionIndex = Math.Max(sedTotVarIndex, sedSusVarIndex);
                string dimensionName = netCdfFile.GetDimensionName(sedimentDimensionIndex != 1 ? dimensions[1] : dimensions[2]);

                int nBedLayersIndex = dimensionNameList.IndexOf(NBedLayersName);
                NetCdfDimension primaryAxis = dimensions[nBedLayersIndex];
                NetCdfDimension secondAxis = dimensions[sedimentDimensionIndex];

                return ProcessFourDimensionalTimeDependentVariable(timeDependentVariable,
                                                                   primaryAxis,
                                                                   secondAxis,
                                                                   dimensionName,
                                                                   location,
                                                                   coverageLongName,
                                                                   netCdfVariableName,
                                                                   unitSymbol,
                                                                   new[]
                                                                   {
                                                                       PrimaryAxisAttributeName
                                                                   },
                                                                   new[]
                                                                   {
                                                                       SecondaryAxisAttributeName
                                                                   });
            }

            return Enumerable.Empty<UnstructuredGridCoverage>();
        }

        /// <summary>
        /// Process three dimensional time dependent variables by creating additional <see cref="UnstructuredGridCoverage"/>
        /// along the third dimensional axis for each value.
        /// </summary>
        /// <param name="timeDependentVariable">The <see cref="ReadOnlyNetCdfFunctionStoreBase.NetCdfVariableInfo"/>  to process.</param>
        /// <param name="firstDimension">The first dimension to process along with.</param>
        /// <param name="secondDimension">The second dimension to process along with.</param>
        /// <param name="surfaceName">
        /// The name of the surfaces for each entry along the <paramref name="firstDimension"/> and
        /// <paramref name="secondDimension"/>.
        /// </param>
        /// <param name="location">The name of the location which is processed.</param>
        /// <param name="coverageLongName">The long name of the <paramref name="timeDependentVariable"/>.</param>
        /// <param name="netCdfVariableName">The name of the variable.</param>
        /// <param name="unitSymbol">The unit symbol of the variable.</param>
        /// <param name="additionalAttributeNamesFirstDimension">
        /// The collection of attribute names to add for each created separate  <see cref="UnstructuredGridCoverage"/> along the
        /// first dimension.
        /// </param>
        /// <param name="additionalAttributeNamesSecondDimension">
        /// The collection of attribute names to add for each created separate  <see cref="UnstructuredGridCoverage"/> along the
        /// second dimension.
        /// </param>
        /// <returns>A collection of <see cref="UnstructuredGridCoverage"/> along the third axis.</returns>
        private IEnumerable<UnstructuredGridCoverage> ProcessFourDimensionalTimeDependentVariable(NetCdfVariableInfo timeDependentVariable,
                                                                                                  NetCdfDimension firstDimension,
                                                                                                  NetCdfDimension secondDimension,
                                                                                                  string surfaceName,
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
                var coverageName = $"{coverageLongName} ({index}),";

                IEnumerable<UnstructuredGridCoverage> coverageAlongSideSecondAxis =
                    ProcessThreeDimensionalTimeDependentVariable(timeDependentVariable,
                                                                 secondDimension,
                                                                 surfaceName,
                                                                 location,
                                                                 coverageName,
                                                                 netCdfVariableName,
                                                                 unitSymbol,
                                                                 additionalAttributeNamesSecondDimension);
                foreach (UnstructuredGridCoverage coverage in coverageAlongSideSecondAxis)
                {
                    Tuple<string, string>[] additionalAttributes =
                        additionalAttributeNamesFirstDimension.Select(attributeName => new Tuple<string, string>(attributeName, index.ToString()))
                                                              .ToArray();
                    AddAdditionalDimensionAttributes(coverage, additionalAttributes);

                    yield return coverage;
                }
            }
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

        // Variables used to retrieve multidimensional properties
        private const string PrimaryAxisAttributeName = "PrimaryAxis";
        private const string SecondaryAxisAttributeName = "SecondaryAxis";

        #endregion
    }
}
