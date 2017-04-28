from System import *
from DelftTools.Hydro import *
from GisSharpBlog.NetTopologySuite.Geometries import *

# create simple network containing 3 nodes and 2 branches
network = HydroNetwork(Name = "test network")

node1 = HydroNode(Name = "node1", Geometry = Point(10, 10))
node2 = HydroNode(Name = "node2", Geometry = Point(20, 20))
node3 = HydroNode(Name = "node3", Geometry = Point(30, 25))

channel1 = Channel(Name = "channel 1", Source = node1, Target = node2, Geometry = LineString((Coordinate(10, 10), Coordinate(20, 20))))
channel2 = Channel(Name = "channel 2", Source = node2, Target = node3, Geometry = LineString((Coordinate(20, 20), Coordinate(30, 25))))

# add nodes to the network
network.Nodes.Add(node1)
network.Nodes.Add(node2)
network.Nodes.Add(node3)

# add channels to the network
network.Branches.Add(channel1)
network.Branches.Add(channel2)

# add network to the project
RootFolder.Add(network)

Gui.CommandHandler.OpenView(network)
