# create simple network containing 3 nodes and 2 branches
# network = HydroNetwork(Name = "test network")

network = Gui.DocumentViews[1].Data

network.Nodes.Clear()
network.Branches.Clear()

node1 = HydroNode(Name = "node1")
node2 = HydroNode(Name = "node2")
node3 = HydroNode(Name = "node3")

channel1 = Channel(Name = "channel 1", Source = node1, Target = node2)
channel2 = Channel(Name = "channel 2", Source = node2, Target = node3)

crs1 = CrossSection(Name = "crs1", Offset = 5)

# from GisSharpBlog.NetTopologySuite.IO import WKTReader
# crs1.Geometry = WKTReader().Read("LINESTRING(10 14 1,11 13 0,15 12 1)")

crs1.Geometry = LineString((Coordinate(10, 14, 2), 
							Coordinate(11, 13, 1), 
							Coordinate(15, 12, 2)))

channel1.BranchFeatures.Add(crs1) 

# define geometry
node1.Geometry = Point(10, 10)
node2.Geometry = Point(20, 20)
node3.Geometry = Point(25, 22)
channel1.Geometry = LineString((Coordinate(10, 10), Coordinate(20, 20)))
channel2.Geometry = LineString((Coordinate(20, 20), Coordinate(25, 22)))

# add nodes to the network
network.Nodes.Add(node1)
network.Nodes.Add(node2)
network.Nodes.Add(node3)

# add channels to the network
network.Branches.Add(channel1)
network.Branches.Add(channel2)

# add network to the project
RootFolder.Add(network)

Gui.DocumentViews[1].ViewContext.Map.ZoomToExtents()
