using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.NetCdf;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.NGHS.IO.Store1D;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public class FM1DFileFunctionStore : NetCdfFunctionStore1DBase<TimeDependentVariableMetaDataBase>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FM1DFileFunctionStore));
        public static readonly string LocationAttributeName = "Location";

        private readonly object readLock = new object();
        protected NetCdfFile netCdfFile;
        private const string TimeDimensionName = "time";
        protected string dateTimeFormat = "yyyy-MM-dd hh:mm:ss"; // default
        private IHydroNetwork outputNetwork = new HydroNetwork();
        private IHydroNetwork inputNetwork;
        private IDiscretization outputDiscretization = new Discretization();

        private const string StandardNameAttribute = "standard_name";
        private const string LongNameAttribute = "long_name";
        private const string UnitAttribute = "units";

        public FM1DFileFunctionStore(IHydroNetwork network)
        {
            OutputFileReader = new FmMapFile1DOutputFileReader();
            sobekStartIndex = 0;
            inputNetwork = network;
        }
        public override void Delete()
        {
            //lets not delete! I only do this for the stipid prototype.... the _map.nc file is also used by fmfilefunctionstore.... can't delete it!!
        }

        public override object Clone()
        {
            var clonedStore = new FM1DFileFunctionStore((IHydroNetwork)inputNetwork.Clone()) { Path = this.Path, OutputFileReader = new FmMapFile1DOutputFileReader() };

            foreach (var existingNetworkCoverage in Functions.OfType<INetworkCoverage>())
            {
                var newNetworkCoverage = new NetworkCoverage(existingNetworkCoverage.Name, true)
                {
                    Network = existingNetworkCoverage.Network,
                    Store = clonedStore
                };

                clonedStore.Functions.AddRange(newNetworkCoverage.Arguments);
                clonedStore.Functions.AddRange(newNetworkCoverage.Components);
                clonedStore.Functions.Add(newNetworkCoverage);
            }

            foreach (var existingFeatureCoverage in Functions.OfType<IFeatureCoverage>())
            {
                var newFeatureCoverage = new FeatureCoverage(existingFeatureCoverage.Name)
                {
                    Features = existingFeatureCoverage.Features,
                    Store = clonedStore
                };

                clonedStore.Functions.AddRange(newFeatureCoverage.Arguments);
                clonedStore.Functions.AddRange(newFeatureCoverage.Components);
                clonedStore.Functions.Add(newFeatureCoverage);
            }

            foreach (var function in Functions.Where(f => !(f is ICoverage)))
            {
                var matchingFunction = (IVariable)Enumerable.FirstOrDefault<IFunction>(clonedStore.Functions, f => f.Name == function.Name
                                                                                                                   && f.GetType() == function.GetType());

                if (matchingFunction != null) matchingFunction.CopyFrom(function);
            }

            return clonedStore;
        }
        public IHydroNetwork OutputNetwork
        {
            get { return outputNetwork; }
        }

        public IDiscretization OutputDiscretization
        {
            get { return outputDiscretization; }
        }

        public override string Path
        {
            get { return base.Path; }
            set
            {
                var previousPath = base.Path;
                base.Path = value;

                if (previousPath == base.Path) return;

                UpdateNetworkAndDiscretisationAfterPathSet();
                UpdateFunctionsAfterPathSet();
            }
        }

        private void UpdateNetworkAndDiscretisationAfterPathSet()
        {
            var netFilePath = Path;
            if (!File.Exists(netFilePath)) return;
            int numberOfNetworks;
            using (var uGridNetwork = new UGridNetwork(netFilePath))
            {
                numberOfNetworks = uGridNetwork.GetNumberOfNetworks();
            }
            if (numberOfNetworks != 1) return;

            int numberOfNetworkDiscretisations;
            using (var uGridNetworkDiscretisation = new UGridNetworkDiscretisation(netFilePath))
            {
                numberOfNetworkDiscretisations = uGridNetworkDiscretisation.GetNumberOfNetworkDiscretisations();
            }
            if (numberOfNetworkDiscretisations != 1) return;

            using (ReconnectToMapFile())
            {
                if (GetNcFileConvention() != GridApiDataSet.DataSetConventions.CONV_UGRID) return;

                var branchData = UGridToNetworkAdapter.ReadPropertiesPerBranchFromFile(netFilePath);
                outputNetwork.Nodes.Clear();
                outputNetwork.Branches.Clear();
                outputDiscretization.Clear();
                UGridToNetworkAdapter.LoadNetworkAndDiscretisation(netFilePath, outputDiscretization, outputNetwork, null, branchData);

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
        private GridApiDataSet.DataSetConventions GetNcFileConvention()
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
                log.ErrorFormat("Failed to construct grid spatial data : {0}", e.Message);
            }

            return GridApiDataSet.DataSetConventions.CONV_NULL;
        }

        protected virtual void UpdateFunctionsAfterPathSet()
        {
            Functions.Clear();
            if (File.Exists(Path))
            {
                using (ReconnectToMapFile())
                {
                    if (GetNcFileConvention() != GridApiDataSet.DataSetConventions.CONV_UGRID) return;
                    Functions.AddRange(ConstructFunctions(GetVariableInfos()));
                }
            }
        }

        protected IEnumerable<IFunction> ConstructFunctions(IEnumerable<NetCdfVariableInfo> dataVariables)
        {
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

            var location = netCdfFile.GetAttributeValue(netcdfVariable, GridApiDataSet.UGridAttributeConstants.Names.Location);
            var unitSymbol = netCdfFile.GetAttributeValue(netcdfVariable, UnitAttribute);

            coverage = CreateNetworkCoverage(coverageLongName, unitSymbol);
            coverage.Attributes.Add("NetCdfVariableName",netCdfVariableName);
            coverage.Attributes.Add(LocationAttributeName, GetNetworkLocation(location).ToString());
            coverage.Store = this;
            var times = MetaData.Times;
            AddNetworkLocationsToNetworkCoverage(outputDiscretization, times, coverage);
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

        protected override string GetNetCdfVariableName(ICoverage coverage)
        {
            var nwConverage = coverage as NetworkCoverage;
            if (nwConverage == null && !nwConverage.Attributes.ContainsKey("NetCdfVariableName")) return base.GetNetCdfVariableName(coverage);
            return nwConverage.Attributes["NetCdfVariableName"];
        }

        public static void AddNetworkLocationsToNetworkCoverage(IDiscretization discretization, ICollection<DateTime> times, INetworkCoverage networkCoverage)
        {
            if (networkCoverage.Store is FM1DFileFunctionStore) return; // temporary until modelApi is removed

            var networkLocations = discretization.Locations.Values.OrderBy(l => l).ToArray();

            networkCoverage.Clear();

            networkCoverage.Time.FixedSize = times.Count;
            networkCoverage.Locations.FixedSize = networkLocations.Length;

            if (times.Count != 0) networkCoverage.Time.SetValues(times);
            if (networkLocations.Length != 0) networkCoverage.SetLocations(networkLocations);
        }
        private NetworkCoverage CreateNetworkCoverage(string coverageLongName, string unitSymbol, int number = -1)
        {
            var suffix = number < 0 ? string.Empty : string.Format(" ({0})", number);
            var coverageName = coverageLongName + suffix;
            var networkCoverage = new NetworkCoverage(coverageName, true, coverageName, unitSymbol) { Network = inputNetwork };
            networkCoverage.Components[0].NoDataValue = double.NaN;
            
            networkCoverage.Locations.FixedSize = 0;
            networkCoverage.Locations.InterpolationType = InterpolationType.Constant;
            networkCoverage.Locations.ExtrapolationType = ExtrapolationType.Constant;
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
        protected virtual string ReadReferenceDateFromFile(string timeVariableName)
        {
            NetCdfVariable timeVariable = netCdfFile.GetVariableByName(timeVariableName);
            string timeReference = netCdfFile.GetAttributeValue(timeVariable, "units");

            const string secondsSinceStr = "seconds since ";

            var dateTime = new DateTime(1970, 1, 1); // assume epoch otherwise
            if (timeReference.StartsWith(secondsSinceStr))
            {
                string timeStr = timeReference.Substring(secondsSinceStr.Length);

                if (!DateTime.TryParseExact(timeStr, dateTimeFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out dateTime))
                    throw new ArgumentException("Could not parse time reference");
            }
            return dateTime.ToString(DateTimeFormatInfo.InvariantInfo.FullDateTimePattern, CultureInfo.InvariantCulture);
        }
        protected IDisposable ReconnectToMapFile()
        {
            return new NetCdfFileConnection(this);
        }
        private class NetCdfFileConnection : IDisposable
        {
            private readonly bool fileWasAlreadyOpen = true;
            private FM1DFileFunctionStore store;

            public NetCdfFileConnection(FM1DFileFunctionStore store)
            {
                Monitor.Enter(store.readLock);

                this.store = store;

                if (store.netCdfFile != null)
                    return;

                store.netCdfFile = NetCdfFile.OpenExisting(store.Path);
                fileWasAlreadyOpen = false;
            }

            public void Dispose()
            {
                try
                {
                    // there might be nesting:
                    if (fileWasAlreadyOpen)
                        return;

                    store.netCdfFile.Close();
                    store.netCdfFile = null;
                }
                finally
                {
                    Monitor.Exit(store.readLock);
                    store = null;
                }
            }
        }
        protected class NetCdfVariableInfo
        {
            public NetCdfVariableInfo(NetCdfVariable dataVariable, int numDimensions)
            {
                NetCdfDataVariable = dataVariable;
                NumDimensions = numDimensions;
                IsTimeDependent = false;
            }

            public NetCdfVariableInfo(NetCdfVariable dataVariable, int numDimensions, string timeVariableName,
                string referenceDate)
                : this(dataVariable, numDimensions)
            {
                IsTimeDependent = true;
                TimeVariableName = timeVariableName;
                ReferenceDate = referenceDate;
            }

            public NetCdfVariable NetCdfDataVariable { get; private set; }
            public int NumDimensions { get; private set; }

            public bool IsTimeDependent { get; private set; }
            public string TimeVariableName { get; private set; }
            public string ReferenceDate { get; private set; }
        }

        private class VariableSizeInfo
        {
            public object Max;
            public object Min;

            public VariableSizeInfo(object min, object max)
            {
                Min = min;
                Max = max;
            }
        }
        protected IList<string> TimeVariableNames
        {
            get { return new[] { GetTimeVariableName(TimeDimensionName) }; }
        }
        protected IList<string> TimeDimensionNames
        {
            get { return new[] { TimeDimensionName }; }
        }
        protected string GetTimeVariableName(string dimName)
        {
            return "time";
        }

    }
}