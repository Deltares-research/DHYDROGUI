using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    public static class PartialSobekImporterTestHelper
    {
        public static IHydroNetwork GetTestNetwork()
        {
            var hydroNetwork = new HydroNetwork();

            var node1 = new HydroNode { Name = "node1", Geometry = new Point(0, 0) };
            var node2 = new HydroNode { Name = "node2", Geometry = new Point(0, 100) };

            var channel = new Channel("TestChannel", node1, node2) { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(0, 100) }) };

            hydroNetwork.Branches.Add(channel);
            hydroNetwork.Nodes.AddRange(new[] { node1, node2 });

            var crossSectionDefinition = new CrossSectionDefinitionYZ();
            SetDefaultYZTableAndUpdateThalWeg(crossSectionDefinition);

            var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(channel, crossSectionDefinition, 0);
            cs.Name = "TestCS";
            return hydroNetwork;
        }

        private static void SetDefaultYZTableAndUpdateThalWeg(CrossSectionDefinitionYZ crossSectionDefinition)
        {
            const double width = 18.0;
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow(0, 0.0);
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow((4 * width / 18), 0.0);
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow((6 * width / 18), -10.0);
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow((12 * width / 18), -10.0);
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow((14 * width / 18), 0.0);
            crossSectionDefinition.YZDataTable.AddCrossSectionYZRow(width, 0.0);

            crossSectionDefinition.Thalweg = crossSectionDefinition.Width / 2;
        }
    }
}
