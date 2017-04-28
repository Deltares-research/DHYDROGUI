from System import *
from DelftTools.Hydro import *
from GisSharpBlog.NetTopologySuite.Geometries import *
from DelftTools.Utils.Editing import *

# get existing network from the current project
network = CurrentProject.RootFolder["test network"].Value

# create a new node and channel features
node4 = HydroNode(Name = "node4")
node4.Geometry = Point(Coordinate(10, 20))

channel3 = Channel(Name = "channel 3", Source = network.Nodes[0], Target = node4)
channel3.Geometry = LineString((Coordinate(10, 10), Coordinate(10, 20)))

# add new node and channel to the network
network.Nodes.Add(node4)
network.Branches.Add(channel3)

