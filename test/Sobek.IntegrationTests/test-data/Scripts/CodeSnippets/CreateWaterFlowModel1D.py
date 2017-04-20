# create simple network containing 3 nodes and 2 branches
network = HydroNetwork(Name = "test network")

node1 = HydroNode(Name = "node1")
node2 = HydroNode(Name = "node2")
node3 = HydroNode(Name = "node3")

channel1 = Channel(Name = "channel 1", Source = node1, Target = node2, Length = 10)
channel2 = Channel(Name = "channel 2", Source = node2, Target = node3, Length = 10)

# add nodes to the network
network.Nodes.Add(node1)
network.Nodes.Add(node2)
network.Nodes.Add(node3)

# add channels to the network
network.Branches.Add(channel1)
network.Branches.Add(channel2)

# add network to the project
CurrentProject.RootFolder.Add(network)

# define geometry (just in case, model can run without it
geometryFactory = GeometryFactory()

node1.Geometry = geometryFactory.CreatePoint(Coordinate(0, 0))
node2.Geometry = geometryFactory.CreatePoint(Coordinate(10, 0))
node3.Geometry = geometryFactory.CreatePoint(Coordinate(20, 0))

channel1.Geometry = geometryFactory.CreateLineString((Coordinate(0, 0), Coordinate(10, 0)))
channel2.Geometry = geometryFactory.CreateLineString((Coordinate(10, 0), Coordinate(20, 0)))

# add 2 cross sections
crossSection1 = CrossSection(Name = "crs1", Offset = 5.0)
# crossSection1.Geometry = geometryFactory.CreatePoint(Coordinate(5, 0))

crossSection1.Definition.DefinitionData[0.0] = (0.0, 1, 0.001, 0.001)
crossSection1.Definition.DefinitionData[100.0] = (0.0, 1, 0.001, 0.001)
crossSection1.Definition.DefinitionData[150.0] = (-10.0, 1, 0.001, 0.001)
crossSection1.Definition.DefinitionData[300.0] = (-10.0, 1, 0.001, 0.001)
crossSection1.Definition.DefinitionData[350.0] = (0.0, 1, 0.001, 0.001)
crossSection1.Definition.DefinitionData[500.0] = (0.0, 1, 0.001, 0.001)

channel1.BranchFeatures.Add(crossSection1)
crossSection1.Branch = channel1 # set it automatically once crossSection is added to the branch!

crossSection2 = CrossSection(Name = "crs1", Offset = 15.0)
# crossSection2.Geometry = geometryFactory.CreatePoint(Coordinate(15, 0))

crossSection2.Definition.DefinitionData[0.0] = (0.0, 1, 0.001, 0.001)
crossSection2.Definition.DefinitionData[100.0] = (0.0, 1, 0.001, 0.001)
crossSection2.Definition.DefinitionData[150.0] = (-10.0, 1, 0.001, 0.001)
crossSection2.Definition.DefinitionData[300.0] = (-10.0, 1, 0.001, 0.001)
crossSection2.Definition.DefinitionData[350.0] = (0.0, 1, 0.001, 0.001)
crossSection2.Definition.DefinitionData[500.0] = (0.0, 1, 0.001, 0.001)

channel2.BranchFeatures.Add(crossSection2)
crossSection2.Branch = channel2

# create computational grid
import NetTopologySuite.Extensions.Coverages.Discretization as Discretization
import GeoAPI.Extensions.Coverages.SegmentGenerationMethod as SegmentGenerationMethod
grid = Discretization(Network = network, SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered)
grid.Name = "test network grid"

import DelftTools.DataObjects.Helpers.HydroNetworkHelper as HydroNetworkHelper
HydroNetworkHelper.GenerateDiscretization(grid, channel1, 0, False, 0.5, False, True, channel1.Length / 10.0);
HydroNetworkHelper.GenerateDiscretization(grid, channel2, 0, False, 0.5, False, True, channel2.Length / 10.0);

CurrentProject.RootFolder.Add(grid)

