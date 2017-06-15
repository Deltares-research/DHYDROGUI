using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Helpers;
using DelftTools.Units;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class FMMapFileFunctionStore : FMNetCdfFileFunctionStore
    {
        private readonly WaterFlowFMModel waterFlowFmModel;

        #region Map file constants
        private const string FlowLinkNrsName = "1d2d_flowlinknrs";
        private const string FlowLinkName = "FlowLink";
        private const string TimeVariableName = "time";

        private const string NSedSusName = "nSedSus";
        private const string NSedTotName = "nSedTot";

        private const string NBnd1D2DName = "nBnd1d2d";
        private const string UnitsName = "units";
        private const string VelocityCoverageName = "velocity (ucx + ucy)";
        private const string FlowlinkXu = "FlowLink_xu";
        private const string FlowlinkYu = "FlowLink_yu";

        private const string NFlowElemName = "nFlowElem";
        private const string NFlowLinkName = "nFlowLink";
        private const string NNetLinkName = "nNetLink";
        private const string NFlowElemBndName = "nFlowElemBnd";

        private const string StandardNameAttribute = "standard_name";
        private const string LongNameAttribute = "long_name";
        private const string UnitAttribute = "units";

        private const string EastwardSeaWaterVelocityStandardName = "sea_water_x_velocity";
        private const string NorthwardSeaWaterVelocityStandardName = "sea_water_y_velocity";
        private const string SedindexAttributeName = "SedIndex";
        #endregion

        private static readonly ILog log = LogManager.GetLogger(typeof(FMMapFileFunctionStore));
        private static readonly IList<string> DeprecatedVariables = new[] { "s0", "u0" };

        private DateTime[] ncDatetimes;
        private IList<FlowLink> flowLinks1D2D;
        private UnstructuredGrid grid;
        private readonly IList<ITimeSeries> boundaryCellValues = new List<ITimeSeries>();
        private Dictionary<string, UnstructuredGridCoverage> velocityCoverages = new Dictionary<string, UnstructuredGridCoverage>();

        // nhib
        protected FMMapFileFunctionStore()
        {
        }

        public FMMapFileFunctionStore(WaterFlowFMModel waterFlowFmModel)
        {
            this.waterFlowFmModel = waterFlowFmModel;
            DisableCaching = true;
        }

        public UnstructuredGrid Grid
        {
            get { return grid; }
        }

        public ICoordinateSystem CoordinateSystem
        {
            set
            {
                if (grid != null)
                {
                    grid.CoordinateSystem = value;
                }
                else
                {
                    log.Warn(Resources.FMMapFileFunctionStore_CoordinateSystem_Could_not_set_coordinate_system_in_output_map_because_grid_is_not_set);
                }
            }
        }

        public IList<ITimeSeries> BoundaryCellValues
        {
            get { return boundaryCellValues; }
        }

        public IFunction CustomVelocityCoverage
        {
            get { return Functions.FirstOrDefault(f => f.Name == VelocityCoverageName); }
        }

        public IEnumerable<IGrouping<string, IFunction>> GetFunctionGrouping()
        {
            // Filter out custom velocity coverage
            var regularFunctions = Functions.Where(f => f.Name != VelocityCoverageName);
            return regularFunctions.GroupBy(f => f.Components[0].Attributes[NcNameAttribute]);
        }

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            boundaryCellValues.Clear();
            UpdateGrid();
            var isNotUgridConvention = GetNcFileConvention() != GridApiDataSet.DataSetConventions.IONC_CONV_UGRID;

            var functions = GetFunctions(dataVariables, isNotUgridConvention);
            if (isNotUgridConvention)
            {
                LogWarningsForExcludedTimeDependentVariables(dataVariables);
            }

            return functions;
        }

        private List<UnstructuredGridCoverage> GetFunctions(IEnumerable<NetCdfVariableInfo> dataVariables, bool isNotUgridConvention)
        {
            // Construct UnstructuredGridCoverages from file
            var timeDepVarSelectionCriteria = isNotUgridConvention
                ? (Func<NetCdfVariableInfo, bool>)(v => v.IsTimeDependent && v.NumDimensions > 1 && v.NumDimensions <= 2) : (v => v.IsTimeDependent && v.NumDimensions > 1);
            var timeDepVariables = dataVariables.Where(timeDepVarSelectionCriteria).ToList();
            var functions = timeDepVariables.SelectMany(ProcessTimeDependentVariable).Where(c => c != null).ToList();

            // Construct custom Velocity Coverage
            if (velocityCoverages.ContainsKey(EastwardSeaWaterVelocityStandardName) &&
                velocityCoverages.ContainsKey(NorthwardSeaWaterVelocityStandardName))
            {
                functions.Add(AddCustomVelocityCoverage(velocityCoverages[EastwardSeaWaterVelocityStandardName], velocityCoverages[NorthwardSeaWaterVelocityStandardName]));
            }
            return functions;
        }
        
        private void LogWarningsForExcludedTimeDependentVariables(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            // When the NetCDF file is not UGRID1+, log a warning for the time dependent variables that have been filtered out
            var filteredTimeDepVariables = dataVariables.Where(v => v.IsTimeDependent && v.NumDimensions > 2).ToList();
            var timeDepVariablesNames = 
                filteredTimeDepVariables.Select(v => netCdfFile.GetVariableName(v.NetCdfDataVariable));
            foreach (var timeDepVarName in timeDepVariablesNames)
            {
                log.WarnFormat(
                    Resources.FMMapFileFunctionStore_ConstructFunctions_Time_dependent_variable___0___has_been_filtered_out,
                    timeDepVarName);
            }
            
        }

        protected override void GetShapeAndOrigin(IVariable function, IVariableFilter[] filters, out int[] shape,
            out int[] origin, out int[] stride)
        {
            base.GetShapeAndOrigin(function, filters, out shape, out origin, out stride);

            if (function.Arguments.Count <= 1 || !function.Arguments[1].Attributes.ContainsKey(SedindexAttributeName))
                return;

            var netcdfVariable = netCdfFile.GetVariableByName(function.Components[0].Attributes[NcNameAttribute]);
            if (netcdfVariable == null) throw new Exception("Missing NetCdf name");

            var dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();
            
            var dimensionNames = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();
            var sedSusVarIndex = dimensionNames.IndexOf(NSedSusName);
            var sedTotVarIndex = dimensionNames.IndexOf(NSedTotName);

            if ((sedSusVarIndex != -1 || sedTotVarIndex != -1) && dimensions.Count != 3)
            {
                throw new Exception("Number of dimensions is wrong");
            }

            var sedIndex = 0;
            if(!Int32.TryParse(function.Arguments[1].Attributes[SedindexAttributeName], out sedIndex))
            {
                throw new Exception("Sediment Index is not of integer type");
            }

            var dimensionIndex = Math.Max(sedTotVarIndex, sedSusVarIndex);
            var sedShape = 1;
            var sedOrigin = sedIndex;
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

        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(IVariable function, IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] == "false") // has no explicit variable (for example nFlowElem, which is only a dimension)
            {
                int size = GetSize(function);
                return new MultiDimensionalArray<T>(Enumerable.Range(0, size).Cast<T>().ToList(), new[] { size });
            }

            return base.GetVariableValuesCore<T>(function, filters);
        }
        
        private UnstructuredGridCoverage AddCustomVelocityCoverage(UnstructuredGridCoverage ucxCoverage, UnstructuredGridCoverage ucyCoverage)
        {
            var coverage = CreateCoverage(GridApiDataSet.UGridAttributeConstants.LocationValues.Face, VelocityCoverageName);

            coverage.Components.Add(new Variable<double>()); // add 2nd component
            coverage.Components[1].Name = ucyCoverage.Components[0].Name;
            coverage.Components[1].Attributes[NcNameAttribute] = ucyCoverage.Components[0].Name;
            coverage.Components[1].Attributes[NcUseVariableSizeAttribute] = "true";
            coverage.Components[1].IsEditable = false;

            InitializeCoverage(coverage, ucxCoverage.Arguments[1].Name, ucxCoverage.Components[0].Name, "m/s",
                               ucxCoverage.Arguments[0].Attributes[NcRefDateAttribute]);

            return coverage;
        }

        private UnstructuredGridCoverage CreateCoverage(string location, string coverageLongName, int number = -1)
        {
            // TODO : Suffix should not be the sediment index but the actual name of the sediment - this is not currently available in the map file
            var suffix = number < 0 ? string.Empty : string.Format(" ({0})", number);
            var coverageName = coverageLongName + suffix;
            switch (location)
            {
                // UGrid standard
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Face:
                    return new UnstructuredGridCellCoverage(grid, true) { Name = coverageName };
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Edge:
                    return new UnstructuredGridEdgeCoverage(grid, true) { Name = coverageName };
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Node:
                    return new UnstructuredGridVertexCoverage(grid, true) { Name = coverageName };
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Volume:
                    log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverage_CannotCreateSpatialDataOnVolumeLocation, coverageName);
                    return null;

                // backwards compatibility
                case NFlowElemName:
                    return new UnstructuredGridCellCoverage(grid, true) { Name = coverageName };
                case NFlowLinkName:
                    return new UnstructuredGridFlowLinkCoverage(grid, true) { Name = coverageName };
                case NNetLinkName:
                case NFlowElemBndName:
                    log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverage_NetlinkDimensionCurrentyNotSupported, coverageName);
                    return null;
                default:
                    throw new NotImplementedException(
                        string.Format(Resources.FMMapFileFunctionStore_CreateCoverage_UnexpectedLocationDimension, location));
            }
        }

        private void GetBoundaryLinkValues(string variableName)
        {
            // some variables need to be read only once
            if (flowLinks1D2D == null)
            {
                var ncFlowlinks = (int[,])netCdfFile.Read(netCdfFile.GetVariableByName(FlowLinkName));
                var ncFlowlink1D2Dlinknrs = (int[])netCdfFile.Read(netCdfFile.GetVariableByName(FlowLinkNrsName));
                var ncFlowlinkXu = (double[])netCdfFile.Read(netCdfFile.GetVariableByName(FlowlinkXu));
                var ncFlowlinkYu = (double[])netCdfFile.Read(netCdfFile.GetVariableByName(FlowlinkYu));

                var coordinates = ncFlowlinkXu.Zip(ncFlowlinkYu, (x, y) => new Coordinate(x, y)).ToArray();

                flowLinks1D2D = ncFlowlink1D2Dlinknrs.ConvertMultiThreaded(nr =>
                {
                    var cellFromIndex = ncFlowlinks[nr - 1, 0] - 1;
                    var cellToIndex = ncFlowlinks[nr - 1, 1] - 1;

                    var cellEdges = grid.GetCellEdgeIndices(grid.Cells[cellToIndex])
                        .Concat(grid.GetCellEdgeIndices(grid.Cells[cellToIndex]))
                        .Distinct()
                        .Select(edgeIndex => grid.Edges[edgeIndex])
                        .ToList();

                    var nearestEdge = grid.Edges[grid.IndexOfNearestEdge(coordinates[nr - 1], cellEdges)];
                    return new FlowLink(cellFromIndex, cellToIndex, nearestEdge);
                });
            }

            if (ncDatetimes == null)
            {
                var time = netCdfFile.GetVariableByName(TimeVariableName);
                var times = (double[])netCdfFile.Read(time);

                var rds = ReadReferenceDateFromFile(TimeVariableName);

                ncDatetimes = times.Select(d => DateTime.Parse(rds).AddSeconds(d)).ToArray();
            }

            var variable = netCdfFile.GetVariableByName(variableName);

            var totalValuesArray = (double[,])netCdfFile.Read(variable);
            var unit = netCdfFile.GetAttributeValue(variable, UnitsName);

            // create timeseries with a component for every flowlink (cell index)
            var function = new TimeSeries() { Name = variableName };
            var flowLinkVariable = new Variable<FlowLink>()
            {
                Name = "FlowLink",
            };
            function.Arguments.Add(flowLinkVariable);
            function.Components.Add(new Variable<double>()
            {
                Name = "value",
                Unit = new Unit(unit, unit),
            });

            // set time values and interpolation times
            function.Time.SkipUniqueValuesCheck = true;
            function.Time.SetValues(ncDatetimes);
            function.Time.SkipUniqueValuesCheck = false;

            function.Arguments[1].SetValues(flowLinks1D2D);

            function.SetValues(totalValuesArray);

            boundaryCellValues.Add(function);
        }

        private GridApiDataSet.DataSetConventions GetNcFileConvention()
        {
            try
            {
                using (var api = GridApiFactory.CreateNew())
                {
                    return api.GetConvention(netCdfFile.Path);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat(Resources.FMMapFileFunctionStore_CreateCoverageFromNetCdfVariable_FailedToConstructGridSpatialData, e.Message);
            }

            return GridApiDataSet.DataSetConventions.IONC_CONV_NULL;
        }

        private void InitializeCoverage(IFunction coverage, string secondDimensionName, string variableName, string unitSymbol, string refDate, IEnumerable<Tuple<string, string>> secondDimensionAdditionalAttributes = null)
        {
            coverage.Store = this;

            var timeDimension = coverage.Arguments[0];
            timeDimension.Name = "Time";
            timeDimension.Attributes[NcNameAttribute] = TimeVariableNames[0];
            timeDimension.Attributes[NcUseVariableSizeAttribute] = "true";
            timeDimension.Attributes[NcRefDateAttribute] = refDate;
            timeDimension.IsEditable = false;

            var secondDimension = coverage.Arguments[1];
            secondDimension.Name = secondDimensionName;
            secondDimension.Attributes[NcNameAttribute] = secondDimensionName;
            secondDimension.Attributes[NcUseVariableSizeAttribute] = "false";

            // Allowing us to add additional attributes (e.g. sedimentation related)
            if (secondDimensionAdditionalAttributes != null)
            {
                foreach (var secondDimensionAdditionalAttribute in secondDimensionAdditionalAttributes)
                {
                    if (string.IsNullOrEmpty(secondDimensionAdditionalAttribute.Item1)) continue;
                    secondDimension.Attributes[secondDimensionAdditionalAttribute.Item1] = secondDimensionAdditionalAttribute.Item2;
                }
            }

            secondDimension.IsEditable = false;

            var coverageComponent = coverage.Components[0];
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
            var list = original.ToList();
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

        private IEnumerable<UnstructuredGridCoverage> ProcessTimeDependentVariable(NetCdfVariableInfo timeDependentVariable)
        {
            UnstructuredGridCoverage coverage = null;
            var netcdfVariable = timeDependentVariable.NetCdfDataVariable;

            var netCdfVariableName = netCdfFile.GetVariableName(netcdfVariable);
            if (DeprecatedVariables.Contains(netCdfVariableName)) yield break;

            var netCdfVariableType = netCdfFile.GetVariableDataType(netcdfVariable);
            if (netCdfVariableType != NetCdfDataType.NcDoublePrecision)
            {
                log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverageFromNetCdfVariable_FailedToConstructGridSpatialData,
                    netCdfVariableName, netCdfVariableType);
                yield break;
            }

            var dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();

            var secondDimensionName = netCdfFile.GetDimensionName(dimensions[1]);
            if (secondDimensionName.Equals(NBnd1D2DName)) // Not supported by UGrid yet
            {
                GetBoundaryLinkValues(netCdfVariableName);
                yield break;
            }

            var longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute) ??
                           netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttribute);

            var coverageLongName = (longName != null)
                ? string.Format("{0} ({1})", longName, netCdfVariableName)
                : netCdfVariableName;

            var convention = GetNcFileConvention();

            var location = convention == GridApiDataSet.DataSetConventions.IONC_CONV_UGRID
                ? netCdfFile.GetAttributeValue(netcdfVariable, GridApiDataSet.UGridAttributeConstants.Names.Location)
                : secondDimensionName; // backwards compatibility

            var unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);

            // Depending on the NetCdfVariable, Sediment dimension can be SedSus (suspended) or SedTot (total)
            var dimensionNameList = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();
            var sedSusVarIndex = dimensionNameList.IndexOf(NSedSusName);
            var sedTotVarIndex = dimensionNameList.IndexOf(NSedTotName);

            if ((sedSusVarIndex != -1 || sedTotVarIndex != -1) && dimensions.Count == 3)
            {
                //Process variable as three dimensional time dependent variable
                var sedimentDimensionIndex = Math.Max(sedTotVarIndex, sedSusVarIndex);
                foreach (var unstructuredGridCoverage in ProcessThreeDimensionalTimeDependentVariable(timeDependentVariable, dimensions, sedimentDimensionIndex, location, coverageLongName, netCdfVariableName, unitSymbol))
                    yield return unstructuredGridCoverage;
                yield break;
            }
            
            coverage = CreateCoverage(location, coverageLongName);
                
            if (coverage != null)
            {
                InitializeCoverage(coverage, secondDimensionName, netCdfVariableName, unitSymbol, timeDependentVariable.ReferenceDate);
            }

            var standardName = netCdfFile.GetAttributeValue(timeDependentVariable.NetCdfDataVariable, StandardNameAttribute);

            if (standardName == EastwardSeaWaterVelocityStandardName ||
                standardName == NorthwardSeaWaterVelocityStandardName)
            {
                velocityCoverages[standardName] = coverage;
            }

            yield return coverage;
        }

        private IEnumerable<UnstructuredGridCoverage> ProcessThreeDimensionalTimeDependentVariable(NetCdfVariableInfo timeDependentVariable, IList<NetCdfDimension> dimensions, int sedimentDimensionIndex, string location, string coverageLongName, string netCdfVariableName, string unitSymbol)
        {
            var numberOfSedLayers = netCdfFile.GetDimensionLength(dimensions[sedimentDimensionIndex]);

            for (int index = 0; index < numberOfSedLayers; index++)
            {
                // TODO : Replace index with values (i.e. sediment names) - this is not currently available in the map file
                var sedCoverage = CreateCoverage(location, coverageLongName, index);
                if (sedCoverage != null)
                {
                    var secondDimensionName = netCdfFile.GetDimensionName(sedimentDimensionIndex != 1 ? dimensions[1] : dimensions[2]);
                    InitializeCoverage(sedCoverage, secondDimensionName, netCdfVariableName, unitSymbol,
                        timeDependentVariable.ReferenceDate, new[]
                        {
                            new Tuple<string, string>(SedindexAttributeName, index.ToString()),
                        });
                }
                yield return sedCoverage;
            }
        }

        private void UpdateGrid()
        {
            // import the grid from the map file if there is no model grid available
            grid = UnstructuredGridFileHelper.LoadFromFile(netCdfFile.Path, true);
        }

    }
}