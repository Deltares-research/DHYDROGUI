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
lat2 = AddPointLateralSourceOnBranch("lat2", channel2, 1500, 0)

# Set Boundary Conditions
ChangeBoundaryConditionType(flow, "node1", "FlowConstant")
EditConstantFlowBoundaryCondition(flow, "node1", 0.01)
ChangeBoundaryConditionType(flow, "node3", "WaterLevelConstant")
EditConstantWaterLevelBoundaryCondition(flow, "node3", 0)

# Create timeseries for lateral sources
values = [0,0.01,0,0]
dates = [System.DateTime(2012,12,01,0,0,0),System.DateTime(2012,12,03,0,0,0),System.DateTime(2012,12,01,3,0,0),System.DateTime(2012,12,07,0,0,0)]

# Add time series to lat2
lat2Condition = GetLateralSource(flow, lat2.Name)
for j in range(len(values)):
        lat2Condition.Data[dates[j]]= float(values[j])
print lat2Condition        
        
# Save the project under a new name for safety reasons
Application.SaveProjectAs("E:\\testing\\scripts\\out\\DemoIMtemp.dsproj")