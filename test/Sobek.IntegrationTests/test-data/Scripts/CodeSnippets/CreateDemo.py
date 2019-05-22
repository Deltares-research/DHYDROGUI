# Import necessary helper functions from either Delta Shell or Python libraries
from DeltaShell.Plugins.DelftModels.HydroModel import HydroModelBuilder
from System.Collections.Generic import List
from DelftTools.Hydro import *
from GisSharpBlog.NetTopologySuite.Geometries import *
from API import *
from NetTopologySuite.Extensions.Coverages import NetworkLocation

# Start building the Integrated Model
builder = HydroModelBuilder()
model =  builder.BuildEmptyModel(builder.SupportedChildModelNames)
CurrentProject.RootFolder.Add(model)
flow = model.Activities[1]

# Build the desired water flow 1d network
network = flow.Network
# Nodes & channels
node1 = HydroNode(Name = "node1")
node1.Geometry = Point(Coordinate(0, 0))
network.Nodes.Add(node1)
node2 = HydroNode(Name = "node2")
node2.Geometry = Point(Coordinate(1000, 0))
network.Nodes.Add(node2)
channel1 = Channel(Name = "channel 1", Source = node1, Target = node2)
channel1.Geometry = LineString((Coordinate(0, 0), Coordinate(1000, 0)))
network.Branches.Add(channel1)
node3 = HydroNode(Name = "node3")
node3.Geometry = Point(Coordinate(2000, 0))
network.Nodes.Add(node3)
channel2 = Channel(Name = "channel 2", Source = node2, Target = node3)
channel2.Geometry = LineString((Coordinate(1000, 0), Coordinate(2000, 0)))
network.Branches.Add(channel2)
# Observation points
AddObservationPointOnBranch("obs1", channel1, 400, 0)
AddObservationPointOnBranch("obs2", channel1, 600, 0)
AddObservationPointOnBranch("obs3", channel2, 1400, 0)
AddObservationPointOnBranch("obs4", channel2, 1600, 0)
# Lateral sources
AddPointLateralSourceOnBranch("lat1", channel1, 500, 0)
AddPointLateralSourceOnBranch("lat2", channel2, 1500, 0)

# Save the project under a new name for safety reasons
Application.SaveProjectAs("c:\Users\putten_hs\Desktop\DemoIMtemp.dsproj")