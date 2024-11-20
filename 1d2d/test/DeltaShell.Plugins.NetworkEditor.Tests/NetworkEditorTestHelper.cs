using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    public static class NetworkEditorTestHelper
    {
        public static IHydroNetwork CreateDemoNetwork(CrossSectionSectionType crossSectionSectionType)
        {
            // create simplest network
            var network = new HydroNetwork();

            // add nodes and branches
            INode node1 = new HydroNode { Name = "node1", Network = network };
            INode node2 = new HydroNode { Name = "node2", Network = network };
            INode node3 = new HydroNode { Name = "node3", Network = network };

            // create simplest network
            node1.Geometry = new Point(0.0, 0.0);
            node2.Geometry = new Point(100.0, 0.0);
            node3.Geometry = new Point(100.0, 150.0);

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Channel("branch1", node1, node2);
            var branch2 = new Channel("branch2", node2, node3);

            branch1.Geometry = new LineString(new[]
                                                  {
                                                      new Coordinate(0, 0),
                                                      new Coordinate(50, 0),
                                                      new Coordinate(100, 0),
                                                      new Coordinate(100, 0)
                                                  });

            branch2.Geometry = new LineString(new[]
                                                  {
                                                      new Coordinate(100, 0),
                                                      new Coordinate(100, 50),
                                                      new Coordinate(100, 100),
                                                      new Coordinate(100, 150)
                                                  });

            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            IList<Coordinate> yzCoordinates = new List<Coordinate>
                                                   {
                                                       new Coordinate(0.0, 3),
                                                       new Coordinate(10.0, 2),
                                                       new Coordinate(20.0, 1),
                                                       new Coordinate(30.0, 0),
                                                       new Coordinate(40.0, 0),
                                                       new Coordinate(50.0, 1),
                                                       new Coordinate(60.0, 2),
                                                       new Coordinate(70.0, 3)
                                                   };
            var cs1 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch1, 50.0, yzCoordinates, "crs1");
            var crossSection1Def = cs1.Definition;

            crossSection1Def.Sections.Add(new CrossSectionSection
                                              {
                                                  MinY = crossSection1Def.GetProfile().Select(yz => yz.X).Min(),
                                                  MaxY = crossSection1Def.GetProfile().Select(yz => yz.X).Max(),
                                                  SectionType = crossSectionSectionType
                                              });

            IList<Coordinate> yzCoordinates2 = new List<Coordinate>
                                                    {
                                                        new Coordinate(0.0, 3),
                                                        new Coordinate(10.0, 2),
                                                        new Coordinate(20.0, 1),
                                                        new Coordinate(30.0, 0),
                                                        new Coordinate(40.0, 0),
                                                        new Coordinate(50.0, 1),
                                                        new Coordinate(60.0, 2),
                                                        new Coordinate(70.0, 3)
                                                    };

            var cs2 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(branch2, 75.0, yzCoordinates2, "crs2");
            var crossSection2Def = cs2.Definition;

            crossSection2Def.Sections.Add(new CrossSectionSection
                                              {
                                                  MinY = crossSection2Def.GetProfile().Select(yz => yz.X).Min(),
                                                  MaxY = crossSection2Def.GetProfile().Select(yz => yz.X).Max(),
                                                  SectionType = crossSectionSectionType
                                              });

            return network;
        }
    }
}