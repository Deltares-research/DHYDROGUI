using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
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
            var categories = new List<DelftIniCategory>();

            foreach (var route in routes)
            {
                var locations = route.Locations.AllValues;

                var iniCategory = new DelftIniCategory(RouteIniHeader);

                iniCategory.AddProperty(Name, route.Name);
                iniCategory.AddProperty(Branches, string.Join(";", locations.Select(nl => nl.Branch.Name)));
                iniCategory.AddProperty(Chainages, locations.Select(nl => nl.Chainage));

                categories.Add(iniCategory);
            }

            new DelftIniWriter().WriteDelftIniFile(categories, filePath);
        }

        public static void Read(string filePath, IHydroNetwork network)
        {
            var categories = new DelftIniReader().ReadDelftIniFile(filePath).ToList();

            foreach (var category in categories)
            {
                var name = category.ReadProperty<string>(Name.Key);
                var branchNames = category.ReadPropertiesToListOfType(Branches.Key, true, ';', default(IList<string>), false);
                var chainages = category.ReadPropertiesToListOfType<double>(Chainages.Key, true);

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