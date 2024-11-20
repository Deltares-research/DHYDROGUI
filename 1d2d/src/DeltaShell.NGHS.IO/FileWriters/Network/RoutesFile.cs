using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    /// <summary>
    /// Class responsible for reading and writing routes files.
    /// </summary>
    public static class RoutesFile
    {
        /// <summary>
        /// The routes filename.
        /// </summary>
        public const string RoutesFileName = "routes.gui";

        private const string RouteIniHeader = "Route";
        private static readonly ILog log = LogManager.GetLogger(typeof(RoutesFile));

        private static readonly ConfigurationSetting Name = new ConfigurationSetting("name");
        private static readonly ConfigurationSetting Branches = new ConfigurationSetting("branch");
        private static readonly ConfigurationSetting Chainages = new ConfigurationSetting("chainage", format: "F6");

        /// <summary>
        /// Writes the given routes to the specified filepath.
        /// </summary>
        /// <param name="filePath">The filepath to write the routes to.</param>
        /// <param name="routes">The routes to write.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is <c>null</c> or white space.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="routes"/> is <c>null</c>.</exception>
        public static void Write(string filePath, IEnumerable<Route> routes)
        {
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNull(routes, nameof(routes));

            IEnumerable<IniSection> iniSections = CreateIniSections(routes);
            WriteIniFile(filePath, iniSections);
        }

        private static IEnumerable<IniSection> CreateIniSections(IEnumerable<Route> routes)
        {
            var iniSections = new List<IniSection>();

            foreach (Route route in routes)
            {
                IniSection iniSection = CreateIniSectionForRoute(route);
                iniSections.Add(iniSection);
            }

            return iniSections;
        }

        private static IniSection CreateIniSectionForRoute(Route route)
        {
            IMultiDimensionalArray<INetworkLocation> locations = route.Locations.AllValues;

            var iniSection = new IniSection(RouteIniHeader);

            iniSection.AddPropertyFromConfiguration(Name, route.Name);
            iniSection.AddPropertyFromConfiguration(Branches, string.Join(";", locations.Select(nl => nl.Branch.Name)));
            iniSection.AddPropertyFromConfigurationWithMultipleValues(Chainages, locations.Select(nl => nl.Chainage));

            return iniSection;
        }

        private static void WriteIniFile(string filePath, IEnumerable<IniSection> iniSections)
        {
            var iniFormatter = new IniFormatter() { Configuration = { PropertyIndentationLevel = 4 } };

            var iniData = new IniData();
            iniData.AddMultipleSections(iniSections);

            log.InfoFormat(Resources.RoutesFile_WriteIniFile_Writing_routes_to__0__, filePath);
            using (Stream iniStream = File.Open(filePath, FileMode.Create))
            {
                iniFormatter.Format(iniData, iniStream);
            }
        }

        /// <summary>
        /// Reads the routes from the given filepath and adds them to the network.
        /// </summary>
        /// <param name="filePath">The filepath to read the routes from.</param>
        /// <param name="network">The network to add the routes to.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is <c>null</c> or white space.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="network"/> is <c>null</c>.</exception>
        public static void Read(string filePath, IHydroNetwork network)
        {
            Ensure.NotNullOrWhiteSpace(filePath, nameof(filePath));
            Ensure.NotNull(network, nameof(network));

            IniData iniData = ReadIniFile(filePath);

            if (!iniData.Sections.Any())
            {
                return;
            }

            SetRoutesOnNetwork(network, iniData.Sections);
        }

        private static void SetRoutesOnNetwork(IHydroNetwork network, IEnumerable<IniSection> iniSections)
        {
            Dictionary<string, IBranch> branchesByName = GetBranchesByName(network.Branches);
            
            foreach (IniSection iniSection in iniSections)
            {
                string name = iniSection.GetPropertyValue(Name.Key);
                IList<string> branchNames = iniSection.ReadPropertiesToListOfType(Branches.Key, true, ';', default(IList<string>), false);
                IList<double> chainages = iniSection.ReadPropertiesToListOfType<double>(Chainages.Key, true);

                var route = new Route { Name = name };

                if (branchNames != null)
                {
                    AddLocationsToRoute(branchNames, chainages, branchesByName, route);
                }

                network.Routes.Add(route);
            }
        }

        private static IniData ReadIniFile(string filePath)
        {
            var iniParser = new IniParser();

            log.InfoFormat(Resources.RoutesFile_ReadIniFile_Reading_routes_from__0__, filePath);

            using (FileStream iniStream = File.OpenRead(filePath))
            {
                return iniParser.Parse(iniStream);
            }
        }

        private static Dictionary<string, IBranch> GetBranchesByName(IEnumerable<IBranch> networkBranches)
        {
            return networkBranches.ToDictionary(b => b.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        private static void AddLocationsToRoute(IEnumerable<string> branchNames,
                                                IEnumerable<double> chainages,
                                                IReadOnlyDictionary<string, IBranch> branchesByName,
                                                Route route)
        {
            IBranch[] branches = branchNames.Select(id => branchesByName[id]).ToArray();
            List<NetworkLocation> locations = branches.Select((branch, i) => new NetworkLocation(branch, chainages.ElementAt(i))).ToList();

            route.Locations.AddValues(locations);
        }
    }
}