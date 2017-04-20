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
        private const string FlowLinkNrsName = "1d2d_flowlinknrs";
        private const string FlowLinkName = "FlowLink";
        private const string TimeVariableName = "time";
        private const string NBnd1D2DName = "nBnd1d2d";
        private const string UnitsName = "units";
        private const string VelocityCoverageName = "velocity (ucx + ucy)";
        private const string FlowlinkXu = "FlowLink_xu";
        private const string FlowlinkYu = "FlowLink_yu";

        private const string NFlowElemName = "nFlowElem";
        private const string NFlowLinkName = "nFlowLink";
        private const string NNetLinkName = "nNetLink";

        protected const string StandardNameAttribute = "standard_name";
        protected const string LongNameAttribute = "long_name";
        protected const string UnitAttribute = "units";

        private const string EastwardSeaWaterVelocityStandardName = "sea_water_x_velocity";
        private const string NorthwardSeaWaterVelocityStandardName = "sea_water_y_velocity";

        private static readonly ILog log = LogManager.GetLogger(typeof(FMMapFileFunctionStore));
        private static readonly IList<string> DeprecatedVariables = new[] { "s0", "u0" };

        private DateTime[] ncDatetimes;
        private IList<FlowLink> flowLinks1D2D;
        private UnstructuredGrid grid;
        private readonly IList<ITimeSeries> boundaryCellValues = new List<ITimeSeries>();

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

        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            boundaryCellValues.Clear();
            UpdateGrid();
            var timeDepVariables = dataVariables.Where(v => v.IsTimeDependent && v.NumDimensions == 2);
            var velocityCoverages = new Dictionary<string, UnstructuredGridCoverage>();
            
            foreach (var function in ReadCoveragesFromFile(timeDepVariables, velocityCoverages)) yield return function;

            if (velocityCoverages.ContainsKey(EastwardSeaWaterVelocityStandardName) &&
                velocityCoverages.ContainsKey(NorthwardSeaWaterVelocityStandardName))
            {
                yield return AddCustomVelocityCoverage(velocityCoverages[EastwardSeaWaterVelocityStandardName], velocityCoverages[NorthwardSeaWaterVelocityStandardName]);
            }
        }

        private IEnumerable<IFunction> ReadCoveragesFromFile(IEnumerable<NetCdfVariableInfo> timeDepVariables, IDictionary<string, UnstructuredGridCoverage> velocityCoverages)
        {
            foreach (var timeDependentVariable in timeDepVariables)
            {
                var coverage = ProcessTimeDependantVariable(timeDependentVariable);
                if (coverage == null) continue;
                var standardName = netCdfFile.GetAttributeValue(timeDependentVariable.NetCdfDataVariable, StandardNameAttribute);

                if (standardName == EastwardSeaWaterVelocityStandardName ||
                    standardName == NorthwardSeaWaterVelocityStandardName)
                {
                    velocityCoverages[standardName] = coverage;
                }
                yield return coverage;
            }
        }

        private void UpdateGrid()
        {
            // import the grid from the map file if there is no model grid available
            grid = UnstructuredGridFileHelper.LoadFromFile(netCdfFile.Path, true);
        }

        private UnstructuredGridCoverage ProcessTimeDependantVariable(NetCdfVariableInfo timeDependentVariable)
        {
            UnstructuredGridCoverage coverage = null;
            var netcdfVariable = timeDependentVariable.NetCdfDataVariable;
            
            try
            {  
                var variableName = netCdfFile.GetVariableName(netcdfVariable);
                if (DeprecatedVariables.Contains(variableName)) return null;

                var variableType = netCdfFile.GetVariableDataType(netcdfVariable);
                if (variableType != NetCdfDataType.NcDoublePrecision)
                {
                    log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverageFromNetCdfVariable_FailedToConstructGridSpatialData,
                        variableName, variableType);
                    return null;
                }

                var dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();
                var secondDimensionName = netCdfFile.GetDimensionName(dimensions[1]);

                if (secondDimensionName.Equals(NBnd1D2DName)) // Not supported by UGrid yet
                {
                    GetBoundaryLinkValues(variableName);
                    return null;
                }

                var longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute) ?? 
                    netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttribute);

                var coverageLongName = (longName != null) 
                    ? string.Format("{0} ({1})", longName, variableName) 
                    : variableName;

                GridApiDataSet.DataSetConventions convention;
                using (var api = GridApiFactory.CreateNew())
                {
                    convention = api.GetConvention(netCdfFile.Path);
                }

                if (convention == GridApiDataSet.DataSetConventions.IONC_CONV_UGRID)
                {
                    var location = netCdfFile.GetAttributeValue(netcdfVariable, GridApiDataSet.UGridAttributeConstants.Names.Location);
                    coverage = CreateCoverage(location, coverageLongName);
                }
                else // backwards compatibility
                {
                    coverage = CreateCoverage(secondDimensionName, coverageLongName);
                }

                if (coverage != null)
                {
                    var unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);
                    InitializeCoverage(coverage, secondDimensionName, variableName, unitSymbol, timeDependentVariable.ReferenceDate);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat(Resources.FMMapFileFunctionStore_CreateCoverageFromNetCdfVariable_FailedToConstructGridSpatialData,
                    netCdfFile.GetVariableName(netcdfVariable), e.Message);
            }
            
            return coverage;
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

        private void InitializeCoverage(IFunction coverage, string secondDimensionName, string variableName,
            string unitSymbol, string refDate)
        {
            coverage.Store = this;
            var timeVariableName = TimeVariableNames[0];
            coverage.Arguments[0].Name = "Time";
            coverage.Arguments[0].Attributes[NcNameAttribute] = timeVariableName;
            coverage.Arguments[0].Attributes[NcUseVariableSizeAttribute] = "true";
            coverage.Arguments[0].Attributes[NcRefDateAttribute] = refDate;
            coverage.Arguments[0].IsEditable = false;

            coverage.Arguments[1].Name = secondDimensionName;
            coverage.Arguments[1].Attributes[NcNameAttribute] = secondDimensionName;
            coverage.Arguments[1].Attributes[NcUseVariableSizeAttribute] = "false";
            coverage.Arguments[1].IsEditable = false;

            coverage.Components[0].Name = variableName;
            coverage.Components[0].Attributes[NcNameAttribute] = variableName;
            coverage.Components[0].Attributes[NcUseVariableSizeAttribute] = "true";
            coverage.Components[0].NoDataValue = MissingValue;
            coverage.Components[0].IsEditable = false;

            coverage.Components[0].Unit = new Unit(unitSymbol, unitSymbol);
            coverage.IsEditable = false;
        }

        private void GetBoundaryLinkValues(string variableName)
        {
            // some variables need to be read only once
            if (flowLinks1D2D == null)
            {
                var ncFlowlinks = (int[,]) netCdfFile.Read(netCdfFile.GetVariableByName(FlowLinkName));
                var ncFlowlink1D2Dlinknrs = (int[]) netCdfFile.Read(netCdfFile.GetVariableByName(FlowLinkNrsName));
                var ncFlowlinkXu = (double[]) netCdfFile.Read(netCdfFile.GetVariableByName(FlowlinkXu));
                var ncFlowlinkYu = (double[]) netCdfFile.Read(netCdfFile.GetVariableByName(FlowlinkYu));

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
                var times = (double[]) netCdfFile.Read(time);
                
                var rds = ReadReferenceDateFromFile(TimeVariableName);

                ncDatetimes = times.Select(d => DateTime.Parse(rds).AddSeconds(d)).ToArray();
            }

            var variable = netCdfFile.GetVariableByName(variableName);
            
            var totalValuesArray = (double [,]) netCdfFile.Read(variable);
            var unit = netCdfFile.GetAttributeValue(variable, UnitsName);

            // create timeseries with a component for every flowlink (cell index)
            var function = new TimeSeries(){Name = variableName};
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
        
        private UnstructuredGridCoverage CreateCoverage(string location, string coverageLongName)
        {
            switch (location)
            {
                // UGrid standard
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Face:
                    return new UnstructuredGridCellCoverage(grid, true) { Name = coverageLongName };
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Edge:
                    return new UnstructuredGridEdgeCoverage(grid, true) { Name = coverageLongName };
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Node:
                    return new UnstructuredGridVertexCoverage(grid, true) { Name = coverageLongName };
                case GridApiDataSet.UGridAttributeConstants.LocationValues.Volume:
                    log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverage_CannotCreateSpatialDataOnVolumeLocation, coverageLongName);
                    return null;

                // backwards compatibility
                case NFlowElemName:
                    return new UnstructuredGridCellCoverage(grid, true) { Name = coverageLongName };
                case NFlowLinkName:
                    return new UnstructuredGridFlowLinkCoverage(grid, true) { Name = coverageLongName };
                case NNetLinkName:
                    log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverage_NetlinkDimensionCurrentyNotSupported, coverageLongName);
                    return null;
                default:
                    throw new NotImplementedException(
                        string.Format(Resources.FMMapFileFunctionStore_CreateCoverage_UnexpectedLocationDimension, location));
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
    }
}