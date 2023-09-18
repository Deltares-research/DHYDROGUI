using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.NGHS.IO.FileWriters.Network
{
    public static class RoutesFile
    {
        public const string RoutesFileName = "routes.gui";

        private const string RouteIniHeader = "Route";

        private static readonly ConfigurationSetting Name = new ConfigurationSetting("name");
        private static readonly ConfigurationSetting Branches = new ConfigurationSetting("branch");
        private static readonly ConfigurationSetting Chainages = new ConfigurationSetting("chainage", format: "F6");

        public static void Write(string filePath, IEnumerable<Route> routes)
        {
            var iniSections = new List<IniSection>();

            foreach (var route in routes)
            {
                var locations = route.Locations.AllValues;

                var iniSection = new IniSection(RouteIniHeader);

                iniSection.AddPropertyFromConfiguration(Name, route.Name);
                iniSection.AddPropertyFromConfiguration(Branches, string.Join(";", locations.Select(nl => nl.Branch.Name)));
                iniSection.AddPropertyFromConfigurationWithMultipleValues(Chainages, locations.Select(nl => nl.Chainage));

                iniSections.Add(iniSection);
            }

            new DelftIniWriter().WriteDelftIniFile(iniSections, filePath);
        }

        public static void Read(string filePath, IHydroNetwork network)
        {
            var iniSections = new DelftIniReader().ReadDelftIniFile(filePath).ToList();

            foreach (var iniSection in iniSections)
            {
                var name = iniSection.ReadProperty<string>(Name.Key);
                var branchNames = iniSection.ReadPropertiesToListOfType(Branches.Key, true, ';', default(IList<string>), false);
                var chainages = iniSection.ReadPropertiesToListOfType<double>(Chainages.Key, true);

                var route = new Route
                {
                    Name = name
                };

                if (branchNames != null)
                {
                    var branchLookup = network.Branches.ToDictionary(b => b.Name);
                    var branches = branchNames.Select(id => branchLookup[id]).ToArray();
                    var locations = branches.Select((branch, i) => new NetworkLocation(branch, chainages.ElementAt(i))).ToList();

                    route.Locations.AddValues(locations);
                }

                network.Routes.Add(route);
            }
        }
    }
}