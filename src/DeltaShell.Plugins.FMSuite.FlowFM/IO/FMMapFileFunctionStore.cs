using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Link1d2d;
using DelftTools.Units;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.NetCdf;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
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

        private const string EastwardSeaWaterVelocityStandardName = "eastward_sea_water_velocity";
        private const string NorthwardSeaWaterVelocityStandardName = "northward_sea_water_velocity";
        
        // For Backwards compatibility: since the fm kernel keeps changing between the two
        private const string SeaWaterXVelocityStandardName = "sea_water_x_velocity";
        private const string SeaWaterYVelocityStandardName = "sea_water_y_velocity";
        
        private const string SedIndexAttributeName = "SedIndex";
        #endregion

        private static readonly ILog log = LogManager.GetLogger(typeof(FMMapFileFunctionStore));
        private static readonly IList<string> DeprecatedVariables = new[] { "s0", "u0" };

        private DateTime[] ncDatetimes;
        private IList<FlowLink> flowLinks1D2D;
        private IEventedList<ILink1D2D> links1D2D;
        private UnstructuredGrid grid;
        private readonly IList<ITimeSeries> boundaryCellValues = new List<ITimeSeries>();
        private Dictionary<string, UnstructuredGridCoverage> velocityCoverages = new Dictionary<string, UnstructuredGridCoverage>();
        private IHydroNetwork network;
        private IDiscretization discretisation;
        private readonly Dictionary<IVariable, IMultiDimensionalArray> argumentVariableCache = new Dictionary<IVariable, IMultiDimensionalArray>();
        private NetworkLocationTypeConverter networkLocationTypeConverter = new NetworkLocationTypeConverter();
        private List<FeatureCoverage> linkCoverages;

        // nhib
        protected FMMapFileFunctionStore()
        {
        }

        public FMMapFileFunctionStore(WaterFlowFMModel waterFlowFmModel)
        {
            this.waterFlowFmModel = waterFlowFmModel;
            linkCoverages = new List<FeatureCoverage>();
            DisableCaching = true;
        }

        /*public IHydroNetwork Network
        {
            get { return network; }
        }

        public IDiscretization Discretisation
        {
            get { return discretisation; }
        }*/


        public IEventedList<ILink1D2D> Links1D2D
        {
            get { return links1D2D; }
        }
        public UnstructuredGrid Grid
        {
            get { return grid; }
        }
        public List<FeatureCoverage> LinkCoverages
        {
            get
            {
                /*if (linkCoverages == null)
                {
                    Generate1D2DLinkCoverages();
                }*/
                return linkCoverages;
            }
        }

        private void Generate1D2DLinkCoverages()
        {
            /*var flow2dDimrModel = Flow2DModel as IDimrModel;
            if (flow2dDimrModel == null) return;

            var timeSeriesList = flow2dDimrModel.GetVar(CellsToFeaturesName) as ITimeSeries[];
            if (timeSeriesList == null) return;
            
            if (!Links1D2D.Any()) return;
            
            linkCoverages = new List<FeatureCoverage>();

            var edgeToFeature = Features.ToDictionary(f => f.LinkEdge);

            foreach (var timeSeries in timeSeriesList)
            {
                var unit = timeSeries.Components[0].Unit;
                var linkFeatureCoverage = CreateLinkFeatureCoverage(timeSeries.Name, Links1D2D, (unit != null ? (IUnit)unit.Clone() : null));

                // set times
                linkFeatureCoverage.Time.SkipUniqueValuesCheck = true;
                linkFeatureCoverage.Time.SetValues(timeSeries.Time.Values);
                linkFeatureCoverage.Time.SkipUniqueValuesCheck = false;

                foreach (FlowLink flowLink in timeSeries.Arguments[1].Values)
                {
                    if (!edgeToFeature.ContainsKey(flowLink.Edge)) continue;

                    var argumentFeature = edgeToFeature[flowLink.Edge];
                    var valuesToSet = timeSeries.GetValues<double>(new VariableValueFilter<FlowLink>(timeSeries.Arguments[1], flowLink));
                    linkFeatureCoverage.SetValues(valuesToSet, new VariableValueFilter<IFeature>(linkFeatureCoverage.FeatureVariable, argumentFeature));
                }

                linkCoverages.Add(linkFeatureCoverage);
            }*/
        }

        private FeatureCoverage CreateLinkFeatureCoverage(string name, IEventedList<ILink1D2D> features, IUnit unit)
        {
            var featureCoverage = new FeatureCoverage(name)
            {
                IsEditable = false,
                IsTimeDependent = true,
                Features = new EventedList<IFeature>(features),
                CoordinateSystem = CoordinateSystem,
            };

            var featureVariable = new Variable<IFeature>("FlowLink") { IsAutoSorted = false };
            featureCoverage.Arguments.Add(featureVariable);
            featureCoverage.Time.InterpolationType = InterpolationType.Linear;
            featureCoverage.Components.Add(new Variable<double> { Name = name, InterpolationType = InterpolationType.Linear, Unit = unit });
            featureVariable.SetValues(features);

            return featureCoverage;
        }

        public ICoordinateSystem CoordinateSystem
        {
            get
            {
                return grid != null
                    ? grid.CoordinateSystem
                    : (network != null ? network.CoordinateSystem : discretisation?.CoordinateSystem);
            }
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
            var netCdfVariables = netCdfFile.GetVariables().ToList();
            var mesh2DNameNetCdfVariableInfo = netCdfVariables.FirstOrDefault(dv =>
            {
                var attributes = netCdfFile.GetAttributes(dv);
                object dimension;
                if (attributes.TryGetValue("topology_dimension", out dimension))
                {
                    if (int.Parse(dimension.ToString()) == 2)
                    {
                        return true;
                    }
                }
                return false;
            });
            var mesh2DName = mesh2DNameNetCdfVariableInfo == null ? string.Empty : netCdfFile.GetVariableName(mesh2DNameNetCdfVariableInfo);

            var isUgridConvention = GetNcFileConvention() == GridApiDataSet.DataSetConventions.CONV_UGRID;
            
            var functions2D = Get2DFunctions(dataVariables, isUgridConvention, mesh2DName);
            var links1d2dNameNetCdfVariableInfo = netCdfVariables.FirstOrDefault(dv =>
            {
                var attributes = netCdfFile.GetAttributes(dv);
                object topologyrole;
                if (attributes.TryGetValue("cf_role", out topologyrole))
                {
                    if (topologyrole.ToString() == "mesh_topology_contact")
                    {
                        return true;
                    }
                }
                return false;
            });

            var links1D2DName = links1d2dNameNetCdfVariableInfo == null ? string.Empty : netCdfFile.GetVariableName(links1d2dNameNetCdfVariableInfo);
            var functions1D2DLinks = Get1D2DLinkFunctions(dataVariables, isUgridConvention, links1D2DName);
            if (!isUgridConvention)
            {
                LogWarningsForExcludedTimeDependentVariables(dataVariables);
            }

            return functions2D.Cast<IFunction>().Concat(functions1D2DLinks.Cast<IFunction>());
        }
        
        private List<UnstructuredGridCoverage> Get2DFunctions(IEnumerable<NetCdfVariableInfo> dataVariables, bool isUgridConvention, string mesh2DName)
        {
            // Construct UnstructuredGridCoverages from file
            var timeDepVarSelectionCriteria = isUgridConvention
                ? (Func<NetCdfVariableInfo, bool>)(v =>
                {
                    var b = v.IsTimeDependent && v.NumDimensions > 1;
                    var attributes = netCdfFile.GetAttributes(v.NetCdfDataVariable);
                    object meshName;
                    if (attributes.TryGetValue("mesh", out meshName))
                    {
                        return b && (meshName.ToString() == mesh2DName);
                    }
                    return false;
                }) : (v => v.IsTimeDependent && v.NumDimensions > 1 && v.NumDimensions <= 2);
            var timeDepVariables = dataVariables.Where(timeDepVarSelectionCriteria).ToList();
            var functions = timeDepVariables.SelectMany(ProcessTimeDependent2DVariable).Where(c => c != null).ToList();

            // Construct custom Velocity Coverage
            if (velocityCoverages.ContainsKey(EastwardSeaWaterVelocityStandardName) &&
                velocityCoverages.ContainsKey(NorthwardSeaWaterVelocityStandardName))
            {
                functions.Add(AddCustomVelocityCoverage(velocityCoverages[EastwardSeaWaterVelocityStandardName], velocityCoverages[NorthwardSeaWaterVelocityStandardName]));
            }

            // Backwards compatibility...
            if (velocityCoverages.ContainsKey(SeaWaterXVelocityStandardName) &&
                velocityCoverages.ContainsKey(SeaWaterYVelocityStandardName))
            {
                functions.Add(AddCustomVelocityCoverage(velocityCoverages[SeaWaterXVelocityStandardName], velocityCoverages[SeaWaterYVelocityStandardName]));
            }

            return functions;
        }

        private List<IFunction> Get1D2DLinkFunctions(IEnumerable<NetCdfVariableInfo> dataVariables, bool isUgridConvention, string links1d2dName)
        {
            // Construct UnstructuredGridCoverages from file
            var timeDepVarSelectionCriteria = isUgridConvention
                ? (Func<NetCdfVariableInfo, bool>)(v =>
                {
                    var b = v.IsTimeDependent && v.NumDimensions > 1;
                    var attributes = netCdfFile.GetAttributes(v.NetCdfDataVariable);
                    object meshName;
                    if (attributes.TryGetValue("mesh", out meshName))
                    {
                        return b && (meshName.ToString() == links1d2dName);
                    }
                    return false;
                }) : (v => v.IsTimeDependent && v.NumDimensions > 1 && v.NumDimensions <= 2);
            var timeDepVariables = dataVariables.Where(timeDepVarSelectionCriteria).ToList();
            boundaryCellValues.Clear();
            var functions = timeDepVariables.SelectMany(ProcessTimeDependent1D2DLinkVariable).Where(c => c != null).ToList();
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

        protected override int GetVariableValuesCount(IVariable function, IVariableFilter[] filters)
        {
            var variableValuesCount = base.GetVariableValuesCount(function, filters);
            
            if (function.IsIndependent)
            {
                return variableValuesCount;
            }

            var coverage = Functions.FirstOrDefault(f => f.Components.Contains(function));

            if (coverage == null || !coverage.Attributes.ContainsKey(SedIndexAttributeName))
            {
                return variableValuesCount;
            }

            var netcdfVariableDimensionLength = 1;

            using (ReconnectToMapFile())
            {
                var netcdfVariable = netCdfFile.GetVariableByName(function.Components[0].Attributes[NcNameAttribute]);
                if (netcdfVariable == null) throw new Exception("Missing NetCdf name");

                var dimensions = netCdfFile.GetDimensions(netcdfVariable).ToList();
                var dimensionNames = dimensions.Select(d => netCdfFile.GetDimensionName(d)).ToList();

                var sedSusVarIndex = dimensionNames.IndexOf(NSedSusName);
                var sedTotVarIndex = dimensionNames.IndexOf(NSedTotName);

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
                else return variableValuesCount;
            }

            return variableValuesCount / netcdfVariableDimensionLength;
        }



        protected override void GetShapeAndOrigin(IVariable function, IVariableFilter[] filters, out int[] shape,
            out int[] origin, out int[] stride)
        {
            base.GetShapeAndOrigin(function, filters, out shape, out origin, out stride);
            
            if (function.IsIndependent)
                return;
            
            var coverage = Functions.FirstOrDefault(f => f.Components.Contains(function));

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
            if(!Int32.TryParse(coverage.Attributes[SedIndexAttributeName], out sedIndex))
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
                if (typeof(T) == typeof(INetworkLocation))
                {

                    //var location = discretisation.Locations.AllValues.IndexOf(function);
                    var convertedList = (List<INetworkLocation>)TypeUtils.CreateGeneric(typeof(List<>), networkLocationTypeConverter.ConvertedType);
                    int[] shape = Enumerable.Range(0, discretisation.Locations.AllValues.Count).ToArray();
                    return (IMultiDimensionalArray<T>)new MultiDimensionalArray<INetworkLocation>(convertedList, shape);
                    var genericType = typeof(MultiDimensionalArray<>).MakeGenericType(function.ValueType);
                    return (IMultiDimensionalArray<T>)Activator.CreateInstance(genericType);
                }
                else
                {
                    int size = GetSize(function);
                    return new MultiDimensionalArray<T>(Enumerable.Range(0, size).Cast<T>().ToList(), new[] {size});
                }
            }
            
            //if this is a component find the coverage in Functions and apply filter
            if (!function.IsIndependent)
            {
                // is component or coverage
                var coverage =
                    Functions.FirstOrDefault(f => f.Components.Contains(function)); // check if function is component
                if (coverage == null)
                {
                    coverage = Functions.FirstOrDefault(f => f == function); //check if function is coverage

                    if (coverage != null)
                    {
                        // is coverage
                        //check if there are multidimensional sedimentnames
                        var indexOfSedimentToRender = string.Empty;
                        if (coverage.Attributes.TryGetValue(SedIndexAttributeName, out indexOfSedimentToRender))
                        {
                            var nIndex = -1;
                            if (int.TryParse(indexOfSedimentToRender, out nIndex))
                            {
                                var filter = new VariableIndexFilter(function.Components[0], 0);
                                Array.Resize(ref filters, filters.Length + 1);
                                filters[filters.Length - 1] = filter;
                            }
                        }
                    }
                }
                else
                {
                    // is component
                    //check if there are multidimensional sedimentnames
                    var indexOfSedimentToRender = string.Empty;
                    if (coverage.Attributes.TryGetValue(SedIndexAttributeName, out indexOfSedimentToRender))
                    {
                        var nIndex = -1;
                        if (int.TryParse(indexOfSedimentToRender, out nIndex))
                        {
                            var filter = new VariableIndexFilter(function, 0);
                            Array.Resize(ref filters, filters.Length + 1);
                            filters[filters.Length - 1] = filter;
                        }
                    }
                }
                /*
                if (function.ValueType == typeof(double))
                {
                    var coverage1d = GetCoverage(function);
                    var ncVariableName = GetNetCdfVariableName(coverage1d);
                    if (ncVariableName == null)
                    {
                        return (MultiDimensionalArray<T>) new MultiDimensionalArray<double>(new List<double>(), new[] { 0, 0 }).Cast<T>();
                    }

                    if (filters.Length == 0)
                    {
                        return (MultiDimensionalArray<T>) GetValuesForTimeSeriesAtAllLocations(ncVariableName);
                    }

                    var dateTimeFilter = filters.OfType<VariableValueFilter<DateTime>>().FirstOrDefault(f => f.Variable == coverage1d.Time);

                    var featureVariable = coverage1d.Arguments.FirstOrDefault(a => a != coverage1d.Time && a.ValueType.Implements(typeof(IBranchFeature)));
                    var branchFeatureFilter = filters.OfType<IVariableValueFilter>().FirstOrDefault(f => f.Variable == featureVariable);
                    var branchRangeFilter = filters.OfType<VariableIndexRangesFilter>().FirstOrDefault(f => f.Variable == featureVariable);

                    var hasBranchRangeFilter = branchRangeFilter != null && branchRangeFilter.IndexRanges.Count == 1;
                    var hasBranchFilter = branchFeatureFilter != null && branchFeatureFilter.Values.Count == 1;
                    var hasTimeFilter = dateTimeFilter != null && dateTimeFilter.Values.Count == 1;

                    int[] shape = null;
                    IList<double> timeSeriesData = null;
                    try
                    {
                        if (hasTimeFilter)
                        {
                            var timeStepIndex = MetaData.Times.IndexOf(dateTimeFilter.Values[0]);
                            if (hasBranchFilter)
                            {
                                timeSeriesData = GetValueForTimeStepAtSingleLocation(ncVariableName, branchFeatureFilter, timeStepIndex, out shape);
                            }
                            else if (hasBranchRangeFilter)
                            {
                                timeSeriesData = GetValuesForTimeStepAtRangeOfLocations(ncVariableName, branchRangeFilter, timeStepIndex, out shape);
                            }
                            else
                            {
                                timeSeriesData = GetValuesForTimeStepAtAllLocations(ncVariableName, timeStepIndex, out shape);
                            }
                        }
                        else
                        {
                            if (hasBranchFilter)
                            {
                                timeSeriesData = GetValuesForTimeSeriesAtSingleLocation(ncVariableName, branchFeatureFilter, out shape);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
                        return new MultiDimensionalArray<double>();
                    }


                    if (shape == null || timeSeriesData == null)
                    {
                        throw new NotImplementedException();
                    }

                    UpdateMinMax(timeSeriesData, variable);

                    return new MultiDimensionalArray<double>(timeSeriesData, shape);
                }
            }
            if (function.ValueType == typeof(INetworkLocation))
            {
                return (MultiDimensionalArray <T>)GetResultsFromCache(function, () => GetNetworkLocationsForLocations(function, Enumerable.Range(0, discretisation.Locations.AllValues.Count).ToList()));
            }*/

            }
            return base.GetVariableValuesCore<T>(function, filters);
        }


        /*

        public override T GetMinValue<T>(IVariable variable)
        {
            if (typeof(T) == typeof(INetworkLocation))
            {
                var minValue = GetNetworkLocationsForLocations(variable, new List<int> { 0 }).First();
                return (T)minValue;
            }

            return base.GetMinValue<T>(variable);
        }

        public override T GetMaxValue<T>(IVariable variable)
        {
            if (typeof(T) == typeof(INetworkLocation))
            {
                var maxValue = GetNetworkLocationsForLocations(variable, new List<int> {discretisation.Locations.AllValues.Count - 1}).Last();
                return (T) maxValue;
            }

            return base.GetMaxValue<T>(variable);
        }
        */
        private IMultiDimensionalArray GetResultsFromCache(IVariable variable, Func<IMultiDimensionalArray> getResult)
        {
            if (DisableCaching)
            {
                return getResult();
            }

            if (!argumentVariableCache.ContainsKey(variable))
            {
                argumentVariableCache[variable] = getResult();
            }

            return argumentVariableCache[variable];
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

        private NetworkCoverage CreateNetworkCoverage(string location, string coverageLongName, string unitSymbol, int number = -1)
        {
            // TODO : Suffix should not be the sediment index but the actual name of the sediment - this is not currently available in the map file
            var suffix = number < 0 ? string.Empty : string.Format(" ({0})", number);
            var coverageName = coverageLongName + suffix;
            return new NetworkCoverage(coverageName, true,coverageName, unitSymbol) {Network = network };
            //switch (location)
            //{
            //    // UGrid standard
            //    case GridApiDataSet.UGridAttributeConstants.LocationValues.Edge:
            //        return new UnstructuredGridEdgeCoverage(grid, true) { Name = coverageName };
            //    case GridApiDataSet.UGridAttributeConstants.LocationValues.Node:
            //        return new UnstructuredGridVertexCoverage(grid, true) { Name = coverageName };
            //    case GridApiDataSet.UGridAttributeConstants.LocationValues.Volume:
            //        log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverage_CannotCreateSpatialDataOnVolumeLocation, coverageName);
            //        return null;
            //    default:
            //        /*throw new NotImplementedException(
            //            string.Format(Resources.FMMapFileFunctionStore_CreateCoverage_UnexpectedLocationDimension, location));*/
            //        log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverage_UnexpectedLocationDimension, location);
            //        return null;
            //}
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
                //case "contact":
                    return new UnstructuredGridFlowLinkCoverage(grid, true) { Name = coverageName };
                case NNetLinkName:
                case NFlowElemBndName:
                    log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverage_NetlinkDimensionCurrentyNotSupported, coverageName);
                    return null;
                default:
                    /*throw new NotImplementedException(
                        string.Format(Resources.FMMapFileFunctionStore_CreateCoverage_UnexpectedLocationDimension, location));*/
                    log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverage_UnexpectedLocationDimension, location);
                    return null;
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
        private IFunction GetNewBoundaryLinkValues(string variableName, string coverageLongName)
        {
            // some variables need to be read only once
            if (flowLinks1D2D == null)
            {
                var link1D2Ds = UGrid1D2DLinksAdapter.Load1D2DLinks(Path).ToList();
                var discretisation = UGridToNetworkAdapter.LoadNetworkAndDiscretisation(Path);
                Links1D2DHelper.SetGeometry1D2DLinks(link1D2Ds, discretisation.Locations, grid.Cells);
                links1D2D = new EventedList<ILink1D2D>(link1D2Ds);
                //Links1D2DHelper.SetIndexes1D2DLinks(links1D2D, discretisation, grid);
                flowLinks1D2D = link1D2Ds.ConvertMultiThreaded(l1d2d =>
                {
                    var startCoordinate = l1d2d.Geometry.Coordinates.First();
                    var cellFromIndex = Links1D2DHelper.FindCellIndex(startCoordinate, grid);
                    var endCoordinate = l1d2d.Geometry.Coordinates.Last();
                    var cellToIndex = Links1D2DHelper.FindCellIndex(endCoordinate, grid);
                    var cellEdges = grid.GetCellEdgeIndices(grid.Cells[cellToIndex])
                        .Concat(grid.GetCellEdgeIndices(grid.Cells[cellToIndex]))
                        .Distinct()
                        .Select(edgeIndex => grid.Edges[edgeIndex])
                        .ToList();
                    var centroidCoordinate = l1d2d.Geometry.Centroid.Coordinate;
                    var indexOfNearestEdgeOfCentroidCoordinateOf1D2DGeometryLineString = grid.IndexOfNearestEdge(centroidCoordinate, cellEdges);
                    var gridEdge = grid.Edges[indexOfNearestEdgeOfCentroidCoordinateOf1D2DGeometryLineString];
                    return new FlowLink(cellFromIndex, cellToIndex, gridEdge);
                });
                grid.FlowLinks = flowLinks1D2D;
                linkCoverages = new List<FeatureCoverage>();
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
            var flowLinkVariable = new Variable<ILink1D2D>()
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

            //function.Arguments[1].SetValues(flowLinks1D2D);
            function.Arguments[1].SetValues(Links1D2D);
            function.SetValues(totalValuesArray);

            boundaryCellValues.Add(function);
            function.Components[0].Attributes[NcNameAttribute] = "link1d2d";
            var unit1 = function.Components[0].Unit;
            var linkFeatureCoverage = CreateLinkFeatureCoverage(coverageLongName, Links1D2D, (unit1 != null ? (IUnit) unit1.Clone() : null));
            // set times
            linkFeatureCoverage.Time.SkipUniqueValuesCheck = true;
            linkFeatureCoverage.Time.SetValues(function.Time.Values);
            linkFeatureCoverage.Time.SkipUniqueValuesCheck = false;

            foreach (ILink1D2D link in function.Arguments[1].Values)
            {
                var valuesToSet = function.GetValues<double>(new VariableValueFilter<ILink1D2D>(function.Arguments[1], link));
                linkFeatureCoverage.SetValues(valuesToSet, new VariableValueFilter<IFeature>(linkFeatureCoverage.FeatureVariable, link));
            }
            linkFeatureCoverage.Components[0].Attributes[NcNameAttribute] = "link1d2d";
            linkCoverages.Add(linkFeatureCoverage);
            return linkFeatureCoverage;
        }

        public GridApiDataSet.DataSetConventions GetNcFileConvention()
        {
            try
            {
                var api = GridApiFactory.CreateNew();
                if (api != null)
                {
                    using (api)
                    {
                        GridApiDataSet.DataSetConventions convention;
                        var ierr = api.GetConvention(netCdfFile.Path, out convention);
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
                log.ErrorFormat(Resources.FMMapFileFunctionStore_CreateCoverageFromNetCdfVariable_FailedToConstructGridSpatialData, e.Message);
            }

            return GridApiDataSet.DataSetConventions.CONV_NULL;
        }

        private void InitializeCoverage(IFunction coverage, string secondDimensionName, string variableName, string unitSymbol, string refDate, IEnumerable<Tuple<string, string>> secondDimensionAdditionalAttributes = null, bool isNetworkCoverage = false)
        {
            coverage.Store = this;

            var timeDimension = coverage.Arguments[0];
            timeDimension.Name = "Time";
            timeDimension.Attributes[NcNameAttribute] = TimeVariableNames[0];
            timeDimension.Attributes[NcUseVariableSizeAttribute] = "true";
            timeDimension.Attributes[NcRefDateAttribute] = refDate;
            timeDimension.IsEditable = false;

            if (!isNetworkCoverage)
            {
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
                        coverage.Attributes[secondDimensionAdditionalAttribute.Item1] = secondDimensionAdditionalAttribute.Item2;
                    }
                }

                secondDimension.IsEditable = false;
            }
            else
            {
                coverage.Arguments[1].Attributes[NcUseVariableSizeAttribute] = "true";
                coverage.Arguments[1].Attributes[NcNameAttribute] = variableName;
            }

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

        private IEnumerable<UnstructuredGridCoverage> ProcessTimeDependent2DVariable(NetCdfVariableInfo timeDependentVariable)
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
            if (secondDimensionName.Equals("nlinks1d2d_connections")) // UGrid 1d2d links
            {
                //GetNewBoundaryLinkValues(netCdfVariableName);
                yield break;
            }

            var longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute) ??
                           netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttribute);

            var coverageLongName = (longName != null)
                ? string.Format("{0} ({1})", longName, netCdfVariableName)
                : netCdfVariableName;

            var convention = GetNcFileConvention();

            var location = convention == GridApiDataSet.DataSetConventions.CONV_UGRID
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

            if (standardName == EastwardSeaWaterVelocityStandardName || standardName == NorthwardSeaWaterVelocityStandardName ||
                standardName == SeaWaterXVelocityStandardName || standardName == SeaWaterYVelocityStandardName) // Backwards compatibility *ugh*
            {
                velocityCoverages[standardName] = coverage;
            }

            yield return coverage;
        }

        private IEnumerable<IFunction> ProcessTimeDependent1D2DLinkVariable(NetCdfVariableInfo timeDependentVariable)
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

            if (secondDimensionName.Equals("nlinks1d2d_connections")) // UGrid 1d2d links
            {
                yield return GetNewBoundaryLinkValues(netCdfVariableName, coverageLongName);
                yield break;
            }

            
            var convention = GetNcFileConvention();

            var location = convention == GridApiDataSet.DataSetConventions.CONV_UGRID
                ? netCdfFile.GetAttributeValue(netcdfVariable, GridApiDataSet.UGridAttributeConstants.Names.Location)
                : secondDimensionName; // backwards compatibility

            var unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);
            if (location != "contact") log.WarnFormat(Resources.FMMapFileFunctionStore_CreateCoverage_UnexpectedLocationDimension, location);
            //coverage = new Links1D2DCoverage(links1D2D, grid, discretisation, true) { Name = coverageLongName, CoordinateSystem = CoordinateSystem};
            coverage = new UnstructuredGridFlowLinkCoverage(grid, true) { Name = coverageLongName };

            if (coverage != null)
            {
                InitializeCoverage(coverage, secondDimensionName, netCdfVariableName, unitSymbol, timeDependentVariable.ReferenceDate);
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
                            new Tuple<string, string>(SedIndexAttributeName, index.ToString()),
                        });
                }
                yield return sedCoverage;
            }
        }

        private void UpdateGrid()
        {
            // import the grid from the map file if there is no model grid available
            grid = UnstructuredGridFileHelper.LoadFromFile(netCdfFile.Path, true);
            //network = waterFlowFmModel.Network;
            //discretisation = waterFlowFmModel.NetworkDiscretization;
        }
        /*
        #region private GetValue helper methods

        private IMultiDimensionalArray GetValuesForTimeSeriesAtAllLocations(string ncVariableName)
        {
            var variableData = WaterFlowModel1DOutputFileReader.GetAllVariableData(path, ncVariableName, MetaData);
            var variableDataShape = variableData.GetShape();
            return new MultiDimensionalArray<double>(variableData, variableDataShape);
        }

        private IList<double> GetValuesForTimeSeriesAtSingleLocation(string ncVariableName, IVariableValueFilter branchFeatureFilter, out int[] shape)
        {
            var locationIndex = GetLocationIndex((IBranchFeature)branchFeatureFilter.Values[0]);

            var origin = new[] { 0, locationIndex };
            shape = new[] { MetaData.NumTimes, 1 };

            return GetSelectionOfVariableData(ncVariableName, origin, ref shape);
        }

        private IList<double> GetValuesForTimeStepAtAllLocations(string ncVariableName, int timeStepIndex, out int[] shape)
        {
            var origin = new[] { timeStepIndex, 0 };
            shape = new[] { 1, MetaData.NumLocations };

            return GetSelectionOfVariableData(ncVariableName, origin, ref shape);
        }

        private IList<double> GetValuesForTimeStepAtRangeOfLocations(string ncVariableName, VariableIndexRangesFilter branchRangeFilter, int timeStepIndex, out int[] shape)
        {
            var endIndex = branchRangeFilter.IndexRanges[0].Second;
            var beginIndex = branchRangeFilter.IndexRanges[0].First;

            var origin = new[] { timeStepIndex, beginIndex };
            shape = new[] { 1, endIndex - beginIndex + 1 };

            return GetSelectionOfVariableData(ncVariableName, origin, ref shape);
        }

        private IList<double> GetValueForTimeStepAtSingleLocation(string ncVariableName, IVariableValueFilter branchFeatureFilter, int timeStepIndex, out int[] shape)
        {
            var locationIndex = GetLocationIndex((IBranchFeature)branchFeatureFilter.Values[0]);

            var origin = new[] { timeStepIndex, locationIndex };
            shape = new[] { 1, 1 };

            return GetSelectionOfVariableData(ncVariableName, origin, ref shape);
        }

        private IList<double> GetSelectionOfVariableData(string ncVariableName, int[] origin, ref int[] shape)
        {
            try
            {
                return WaterFlowModel1DOutputFileReader.GetSelectionOfVariableData(path, ncVariableName, origin, shape);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error retrieving data for variable {0}: {1}", ncVariableName, ex.Message);
                shape = new[] { 0, 0 };
                return new List<double>();
            }
        }

        #endregion

        #region private other helper methods

        private IMultiDimensionalArray<INetworkLocation> GetNetworkLocationsForLocations(IVariable function, ICollection<int> locations)
        {
            UpdateTypeConverters(function);
            var convertedList = (List<INetworkLocation>)TypeUtils.CreateGeneric(typeof(List<>), networkLocationTypeConverter.ConvertedType);

            foreach (var location in locations)
            {
                var branchId = MetaData.Locations[location].BranchId - 1; // minus one because fortran is 1 based...
                var chainage = MetaData.Locations[location].Chainage;
                var networkLocation = networkLocationTypeConverter.ConvertFromStore(new object[] { branchId, chainage });
                convertedList.Add(networkLocation);
            }

            var shape = new[] { locations.Count };
            return new MultiDimensionalArray<INetworkLocation>(convertedList, shape);
        }

        private IMultiDimensionalArray<IBranchFeature> GetBranchFeaturesForLocations(IVariable function, ICollection<int> locations)
        {
            UpdateTypeConverters(function);
            var convertedList = (List<IBranchFeature>)TypeUtils.CreateGeneric(typeof(List<>), featureTypeConverter.ConvertedType);

            convertedList.AddRange(locations
                .Select(location => featureTypeConverter.ConvertFromStore(new object[] { location }))
                .OfType<IBranchFeature>());

            var shape = new[] { locations.Count };
            return new MultiDimensionalArray<IBranchFeature>(convertedList, shape);
        }

        private int GetLocationIndex(IBranchFeature branchFeature)
        {
            LocationMetaData location;
            if (branchFeature is INetworkLocation)
            {
                var branchIndex = branchFeature.Network.Branches.IndexOf(branchFeature.Branch);
                location = MetaData.Locations.FirstOrDefault(l => l.BranchId - 1 == branchIndex && Math.Abs(l.Chainage - branchFeature.Chainage) < double.Epsilon);
            }
            else if (branchFeature is IStructure1D)
            {
                var structure = (IStructure1D)branchFeature;

                var compositePrefix = structure.ParentStructure?.Structures.Count > 1
                    ? structure.ParentStructure.Name + "_"
                    : string.Empty;

                var structureName = compositePrefix + branchFeature.Name;

                location = MetaData.Locations.FirstOrDefault(l => l.Id == structureName);
            }
            else
            {
                location = MetaData.Locations.FirstOrDefault(l => l.Id == branchFeature.Name);
            }

            if (location == null)
            {
                throw new ArgumentException(string.Format(Resources.WaterFlowModel1DNetCdfFunctionStore_GetLocationIndex_Values_for__0__feature_type__1__could_not_be_found_, branchFeature.Name, branchFeature.GetType().Name));
            }

            return MetaData.Locations.IndexOf(location);
        }

        private string GetNetCdfVariableName(ICoverage coverage)
        {
            return WaterFlowModel1DOutputCoverageMappings.GetMappingForCoverage(fileName, coverage.Name);
        }

        private ICoverage GetCoverage(IVariable variable)
        {
            return functions.OfType<ICoverage>().FirstOrDefault(f => f.Arguments.Concat(f.Components).Contains(variable));
        }

        private void UpdateTypeConverters(IVariable function)
        {
            if (functions.Any(f => f is INetworkCoverage))
            {
                var networkCoverage = functions.OfType<INetworkCoverage>().First(f => f.Arguments.Contains(function));
                networkLocationTypeConverter.Network = networkCoverage.Network;
                networkLocationTypeConverter.Coverage = networkCoverage;
            }

            if (functions.Any(f => f is IFeatureCoverage))
            {
                var featureCoverage = functions.OfType<IFeatureCoverage>().First(f => f.Arguments.Contains(function));
                featureTypeConverter.FeatureCoverage = featureCoverage;
            }
        }

        private void UpdateMinMax(IEnumerable<double> timeStepData, IVariable function)
        {
            double? min = null;
            double? max = null;

            foreach (var value in timeStepData)
            {
                if (Equals(value, function.NoDataValue)) continue;

                if (min == null || min.Value > value)
                {
                    min = value;
                }

                if (max == null || max.Value < value)
                {
                    max = value;
                }
            }

            var name = function.Name;
            var minMaxChanged = false;
            if (min != null && (!minValues.ContainsKey(name) || minValues[name] > min.Value))
            {
                minValues[name] = min.Value;
                minMaxChanged = true;
            }

            if (max != null && (!maxValues.ContainsKey(name) || maxValues[name] < max.Value))
            {
                maxValues[name] = max.Value;
                minMaxChanged = true;
            }

            if (!minMaxChanged) return;

            FireFunctionValuesChanged(this, new FunctionValuesChangingEventArgs { Function = function });
        }

        private void FireFunctionValuesChanged(object sender, FunctionValuesChangingEventArgs e)
        {
            if (FunctionValuesChanged == null) return;
            FunctionValuesChanged(sender, e);
        }

        #endregion
        */
    }
}