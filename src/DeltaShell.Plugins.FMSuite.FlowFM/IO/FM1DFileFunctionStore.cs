using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Units;
using DelftTools.Utils.NetCdf;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class FM1DFileFunctionStore : FMNetCdfFileFunctionStore
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FM1DFileFunctionStore));
        public static readonly string LocationAttributeName = "Location";

        private const string TimeDimensionName = "time";
        private IHydroNetwork outputNetwork = new HydroNetwork();
        private IHydroNetwork inputNetwork;
        private IDiscretization outputDiscretization = new Discretization();
        public IDictionary<string, IList<INetworkLocation>> LocationsByNetworkDataType { get; set; }

        private readonly IDictionary<string, double> minValues = new Dictionary<string, double>();
        private readonly IDictionary<string, double> maxValues = new Dictionary<string, double>();
        private readonly Dictionary<IVariable, IMultiDimensionalArray> argumentVariableCache = new Dictionary<IVariable, IMultiDimensionalArray>();
        
        private FeatureTypeConverter featureTypeConverter = new FeatureTypeConverter();
        private NetworkLocationTypeConverter networkLocationTypeConverter = new NetworkLocationTypeConverter();
        private OutputFile1DMetaData metaData;
        private int sobekStartIndex = 1;// minus one because fortran is 1 based...

        private const string StandardNameAttribute = "standard_name";
        private const string LongNameAttribute = "long_name";
        private const string UnitAttribute = "units";
        private const string FillValueAttribute = "_FillValue";

        public FM1DFileFunctionStore(IHydroNetwork network)
        {
            OutputFileReader = new FmMapFile1DOutputFileReader();
            sobekStartIndex = 0;
            inputNetwork = network;
            DisableCaching = true;
        }
        public IHydroNetwork OutputNetwork
        {
            get { return outputNetwork; }
        }

        public IDiscretization OutputDiscretization
        {
            get { return outputDiscretization; }
        }
        
        private void UpdateNetworkAndDiscretisationAfterPathSet()
        {
            var netFilePath = Path;
            if (!File.Exists(netFilePath)) return;

            int numberOfNetworks = UGridFileHelper.GetNumberOfNetworks(netFilePath);
            if (numberOfNetworks != 1) return;

            int numberOfNetworkDiscretisations = UGridFileHelper.GetNumberOfNetworkDiscretizations(netFilePath);
            if (numberOfNetworkDiscretisations != 1) return;

            using (ReconnectToMapFile())
            {
                if (!UGridFileHelper.IsUGridFile(netCdfFile.Path)) return;

                var branchData = NetworkPropertiesHelper.ReadPropertiesPerBranchFromFile(netFilePath);
                var compartmentData = NetworkPropertiesHelper.ReadPropertiesPerNodeFromFile(netFilePath);
                outputNetwork.Nodes.Clear();
                outputNetwork.Branches.Clear();
                outputDiscretization.Clear();

                UGridFileHelper.ReadNetworkAndDiscretisation(netFilePath, outputDiscretization, outputNetwork, compartmentData, branchData);

                foreach (var hydroObject in outputNetwork.AllHydroObjects)
                {
                    hydroObject.Name = hydroObject.Name + "_output";
                }
                foreach (var outputNetworkLocation in outputDiscretization.Locations.AllValues)
                {
                    outputNetworkLocation.Name = outputNetworkLocation.Name + "_output";
                }

            }
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

        protected override int GetVariableValuesCount(IVariable variable, IVariableFilter[] filters)
        {
            if (!HasValidFile)
            {
                return 0;
            }
            return base.GetVariableValuesCount(variable, filters);
        }

        public override IMultiDimensionalArray<T> GetVariableValues<T>(IVariable function, params IVariableFilter[] filters)
        {
            if (!HasValidFile)
            {
                return (IMultiDimensionalArray<T>)CreateEmptyArrayForType(function.ValueType);
            }
            if (function.IsIndependent && typeof(T) == typeof(INetworkLocation))
            {
                var featureFilter = filters.FirstOrDefault(f => f.Variable.ValueType == typeof(INetworkLocation));
                if (function.Attributes != null && function.Attributes.ContainsKey(NcNameAttribute))
                {
                    var location = function.Attributes[NcNameAttribute];
                    var networkLocations = LocationsByNetworkDataType[location];
                    if (filters.Length == 0 || featureFilter == null)
                    {

                        return new MultiDimensionalArray<T>((IList<T>)networkLocations);
                    }

                    if (featureFilter is VariableIndexFilter indexFilter)
                    {
                        return new MultiDimensionalArray<T>(
                            new List<T>(indexFilter.Indices.Select(i => (T)networkLocations[i])));
                    }
                }
                return new MultiDimensionalArray<T>();
            }
            return base.GetVariableValues<T>(function, filters);
        }
        protected override void UpdateFunctionsAfterPathSet()
        {
            UpdateNetworkAndDiscretisationAfterPathSet();
            if (CoordinateSystem == null) CoordinateSystem = UGridFileHelper.ReadCoordinateSystem(Path);
            Functions.Clear();
            if (File.Exists(Path))
            {
                using (ReconnectToMapFile())
                {
                    if (! UGridFileHelper.IsUGridFile(netCdfFile.Path)) return;
                    Functions.AddRange(ConstructFunctions(GetVariableInfos()));
                }
            }
        }


        protected override IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
            LocationsByNetworkDataType = new Dictionary<string, IList<INetworkLocation>>();
            var netCdfVariables = netCdfFile.GetVariables().ToList();
            var mesh1DNameNetCdfVariable = netCdfVariables.FirstOrDefault(dv =>
            {
                var attributes = netCdfFile.GetAttributes(dv);
                object dimension;
                if (attributes.TryGetValue("topology_dimension", out dimension) && attributes.ContainsKey("coordinate_space"))
                {
                    if (int.Parse(dimension.ToString()) == 1)
                    {
                        return true;
                    }
                }
                return false;
            });
            var mesh1DName = mesh1DNameNetCdfVariable == null ? string.Empty : netCdfFile.GetVariableName(mesh1DNameNetCdfVariable);
            var isUgridConvention = true;

            return Get1DFunctions(dataVariables, isUgridConvention, mesh1DName);
        }
        private IEnumerable<INetworkCoverage> Get1DFunctions(IEnumerable<NetCdfVariableInfo> dataVariables, bool isUgridConvention, string mesh1DName)
        {
            var timeDepVarSelectionCriteria = isUgridConvention
                ? (Func<NetCdfVariableInfo, bool>) (v =>
                {
                    var attributes = netCdfFile.GetAttributes(v.NetCdfDataVariable);
                    if (!attributes.TryGetValue("mesh", out var meshName) ||
                        !attributes.TryGetValue("location", out var locationName))
                    {
                        return false;
                    }

                    var location = GetNetworkLocation(locationName.ToString());

                    return v.IsTimeDependent && 
                           v.NumDimensions > 1 && 
                           meshName.ToString() == mesh1DName &&
                           (location == NetworkDataLocation.Edge ||
                            location == NetworkDataLocation.Node );
                })
                : v => v.IsTimeDependent && v.NumDimensions > 1 && v.NumDimensions <= 2;

            var timeDepVariables = dataVariables.Where(timeDepVarSelectionCriteria).ToList();
            var functions = timeDepVariables.SelectMany(ProcessTimeDependent1DVariable).Where(c => c != null).ToList();

            return functions;
        }

        private IEnumerable<NetworkCoverage> ProcessTimeDependent1DVariable(NetCdfVariableInfo timeDependentVariable)
        {
            NetworkCoverage coverage = null;
            var netcdfVariable = timeDependentVariable.NetCdfDataVariable;

            var netCdfVariableName = netCdfFile.GetVariableName(netcdfVariable);
            
            var netCdfVariableType = netCdfFile.GetVariableDataType(netcdfVariable);
            if (netCdfVariableType != NetCdfDataType.NcDoublePrecision)
            {
                yield break;
            }

            var longName = netCdfFile.GetAttributeValue(netcdfVariable, LongNameAttribute) ??
                           netCdfFile.GetAttributeValue(netcdfVariable, StandardNameAttribute);

            var coverageLongName = (longName != null)
                ? string.Format("{0} ({1})", longName, netCdfVariableName)
                : netCdfVariableName;

            var location = netCdfFile.GetDimensionName(netCdfFile.GetDimensions(netcdfVariable).ToArray()[1]);//netCdfFile.GetAttributeValue(netcdfVariable, UGridConstants.Naming.LocationAttributeName);
            var timeDependentVariableMetaDataBaseKeyForThisLocation = MetaData.Locations.Keys.FirstOrDefault(tdv => tdv.Name.Equals(netCdfVariableName) );
            if (!LocationsByNetworkDataType.ContainsKey(location) &&
                timeDependentVariableMetaDataBaseKeyForThisLocation != null)
            {
                LocationsByNetworkDataType[location] = MetaData
                    .Locations[timeDependentVariableMetaDataBaseKeyForThisLocation]
                    .Select(l => new NetworkLocation(inputNetwork.Branches[l.BranchId - sobekStartIndex], l.Chainage)
                        {Geometry = new Point(l.XCoordinate, l.YCoordinate), Name = l.Id, Attributes = new DictionaryFeatureAttributeCollection(){{LocationAttributeName, location}}}).Cast<INetworkLocation>().ToList();
            }
            var unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);
            var noDataValue = double.Parse(netCdfFile.GetAttributeValue(netcdfVariable, FillValueAttribute));

            coverage = CreateNetworkCoverage(coverageLongName, unitSymbol, netCdfVariableName, location, timeDependentVariable.ReferenceDate, noDataValue:noDataValue);

            yield return coverage;
        }

        private static NetworkDataLocation GetNetworkLocation(string locationName)
        {
            switch (locationName)
            {
                case "edge":
                    return NetworkDataLocation.Edge;
                case "node":
                    return NetworkDataLocation.Node;
                default:
                    return NetworkDataLocation.UnKnown;
            }
        }
        
        public ICoordinateSystem CoordinateSystem { get; set; }
        
        private NetworkCoverage CreateNetworkCoverage(string coverageLongName, string unitSymbol,
            string netCdfVariableName, string location, string refDate, int number = -1, double noDataValue = -999.0d)
        {
            var suffix = number < 0 ? string.Empty : string.Format(" ({0})", number);
            var coverageName = coverageLongName + suffix;
            var networkCoverage = new NetworkCoverage(coverageName, true, coverageName, unitSymbol) { Network = inputNetwork, CoordinateSystem = CoordinateSystem};
            networkCoverage.Store = this;

            networkCoverage.Components[0].NoDataValue = noDataValue;
            
            networkCoverage.Locations.FixedSize = 0;
            networkCoverage.Locations.InterpolationType = InterpolationType.Constant;
            networkCoverage.Locations.ExtrapolationType = ExtrapolationType.Constant;
            
            networkCoverage.IsEditable = false;

            var timeDimension = networkCoverage.Arguments[0];
            timeDimension.Name = "Time";
            timeDimension.Attributes[NcNameAttribute] = TimeVariableNames[0];
            timeDimension.Attributes[NcUseVariableSizeAttribute] = "true";
            timeDimension.Attributes[NcRefDateAttribute] = refDate;
            timeDimension.IsEditable = false;

            networkCoverage.Arguments[1].Attributes[NcUseVariableSizeAttribute] = "false";
            networkCoverage.Arguments[1].Attributes[NcNameAttribute] = location;
            
            var coverageComponent = networkCoverage.Components[0];
            coverageComponent.Name = netCdfVariableName;
            coverageComponent.Attributes[NcNameAttribute] = netCdfVariableName;
            coverageComponent.Attributes[NcUseVariableSizeAttribute] = "true";
            
            coverageComponent.NoDataValue = MissingValue;
            coverageComponent.IsEditable = false;
            coverageComponent.Unit = new Unit(unitSymbol, unitSymbol);

            networkCoverage.Attributes.Add("NetCdfVariableName", netCdfVariableName);
            networkCoverage.Attributes.Add(LocationAttributeName, GetNetworkLocation(location).ToString());

            networkCoverage.IsEditable = false;

            return networkCoverage;
        }

        private IEnumerable<NetCdfVariableInfo> GetVariableInfos()
        {
            using (ReconnectToMapFile())
            {
                foreach (NetCdfVariable variable in netCdfFile.GetVariables())
                {
                    List<NetCdfDimension> dimensions = netCdfFile.GetDimensions(variable).ToList();
                    string firstDimensionName = dimensions.Select(d => netCdfFile.GetDimensionName(d)).FirstOrDefault();

                    if (dimensions.Count == 0)
                        continue;

                    if (TimeVariableNames.Contains(netCdfFile.GetVariableName(variable)))
                        continue;

                    // maybe add some relationship checks here: now we're also returning some arguments & components seperately.
                    if (firstDimensionName != null && TimeDimensionNames.Contains(firstDimensionName))
                    {
                        // time dependent variable
                        string timeVariableName = GetTimeVariableName(firstDimensionName);
                        string refDate = ReadReferenceDateFromFile(timeVariableName);
                        yield return new NetCdfVariableInfo(variable, dimensions.Count, timeVariableName, refDate);
                    }
                    else
                    {
                        // non time dependent variable
                        yield return new NetCdfVariableInfo(variable, dimensions.Count);
                    }
                }
            }
        }
        private OutputFile1DMetaData MetaData
        {
            get
            {
                if (metaData != null) return metaData;

                if(!File.Exists(Path)) return new OutputFile1DMetaData();

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
        private FmMapFile1DOutputFileReader OutputFileReader { get; set; }

        protected override IMultiDimensionalArray<T> GetVariableValuesCore<T>(IVariable function,
            IVariableFilter[] filters)
        {
            if (function.Attributes[NcUseVariableSizeAttribute] == "false") // has no explicit variable (for example nFlowElem, which is only a dimension)
            {
                int size = GetSize(function);
                return new MultiDimensionalArray<T>(Enumerable.Range(0, size).Cast<T>().ToList(), new[]
                {
                    size
                });
            }
            try
            {
                return base.GetVariableValuesCore<T>(function, filters);
            }
            catch (Exception e) when (e.Message.Contains("NetCDF error code"))
            {
                log.Error(string.Format("While reading variable {0} from the file {1} an error was encountered: {2}", function.Name, System.IO.Path.GetFileName(Path), e.Message));
                int functionSize = GetSize(function);
                return new MultiDimensionalArray<T>(new List<T>(new T[functionSize]), functionSize);
            }
        }
    }
}