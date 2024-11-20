from datetime import datetime, time
from System import TimeSpan

from Libraries.StandardFunctions import *
from Libraries.MapFunctions import CreateLineGeometry, CreatePointGeometry
from Libraries.NetworkFunctions import *
from Libraries.SobekWaterFlowFunctions import *

# create flowmodel
flowModel = WaterFlowModel1D()
flowModel.TimeStep = TimeSpan(0,30,0)
flowModel.OutputTimeStep = TimeSpan(0,30,0) 

#region create simple network containing 5 nodes and 4 branches

node1 = HydroNode(Name = "node1", Geometry = CreatePointGeometry(100, 100))
node4 = HydroNode(Name = "node4", Geometry = CreatePointGeometry(200, 100))

node2 = HydroNode(Name = "node2", Geometry = CreatePointGeometry(100, 0))
node3 = HydroNode(Name = "node3", Geometry = CreatePointGeometry(200, 0))
channel1 = Channel(Name = "channel 1", Source = node1, Target = node4, Geometry = CreateLineGeometry([[100, 100],[200, 100]]))
channel2 = Channel(Name = "channel 2", Source = node2, Target = node3, Geometry = CreateLineGeometry([[100, 0],[200, 0]]))

flowModel.Network.Branches.AddRange([channel1, channel2])
flowModel.Network.Nodes.AddRange([node1, node2, node3, node4])

# create 3 cross sections and set their profile
crossSection1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.CrossSectionYZ, "crossSection1", channel1, 50)
crossSection2 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.CrossSectionZW, "crossSection2", channel2, 50)

crossSectionProfile1 = [[0,0,0], # y, z, storage
                        [2,0,0],
                        [4,-10,0],
                        [6,-10,0],
                        [8,0,0],
                        [10,0,0]]

crossSectionProfile2 = [[0,10,0], # z, width, storage
                        [-2,6,0],
                        [-3,6,0],
                        [-4, 10,0],
                        [-6, 2,0]]

SetCrossSectionProfile(crossSection1, crossSectionProfile1, 5) # thalweg = 5
SetCrossSectionProfile(crossSection2, crossSectionProfile2, 0) # thalweg = 0


# add a lateral
lateral1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.LateralSource, "lateral1", channel1, 25 )
 
# add pump
pump = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.Pump, "pump", channel2, 10 )
pump.Capacity = 5
pump.StartSuction = 2
pump.StopSuction = -2
pump.StartDelivery = -1
pump.StopDelivery = 5

# add bridge
bridge = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.Bridge, "bridge", channel2, 30 )
bridge.IsRectangle = 1
bridge.BottomLevel = crossSection2.LowestPoint + 2
bridge.BridgeLength = 40
bridge.Width = 4
bridge.Height = 6
bridge.Friction = 45.0

# add weir
weir = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.Weir, "weir", channel2, 50)
weir.CrestLevel = 1
weir.CrestWidth = 10

# add observationpoint
observationpoint1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.ObservationPoint, "observationpoint1", channel2, 95)
observationpoint2 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.ObservationPoint, "observationpoint2", channel1, 95)

# add culvert
culvert = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.Culvert, "culvert", channel2, 70)
culvert.Diameter = 8
culvert.InletLevel = -5
culvert.OutletLevel = -10
culvert.Length = 10
culvert.InletLossCoefficient = 0
culvert.OutletLossCoefficient = 0
culvert.Friction = 45

# add resistance
resistance = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.ExtraResistance, "resistance", channel2, 80)
resistance.FrictionTable.Arguments[0].SetValues([2.0,3.0]) # first column 
resistance.FrictionTable.Components[0].SetValues([0.00005,0.00004]) # second column 
#endregion

#region set boundary conditions

# set boundary condition for node 1 and 4
 
SetBoundaryCondition(flowModel, "node1", BoundaryConditionType.FlowConstant, 1)
SetBoundaryCondition(flowModel, "node4", BoundaryConditionType.WaterLevelConstant, 0)

# set boundary condition for node 2 and 3

SetBoundaryCondition(flowModel, "node2", BoundaryConditionType.FlowConstant, 1)
SetBoundaryCondition(flowModel, "node3", BoundaryConditionType.WaterLevelConstant, 0)

# set lateral data for lateral1 to constant flow
SetLateralData(flowModel, "lateral1", LateralDataType.FlowConstant, 1)
#endregion

#region Set roughness
 
# set general (default) roughness value
SetDefaultRoughness(flowModel, "Main", RoughnessType.Chezy ,45)

AddRoughnessAtLocation(flowModel, "Main", channel1, 25, RoughnessType.Chezy, 42)
AddRoughnessAtLocation(flowModel, "Main", channel1, 50, RoughnessType.Chezy, 40)
AddRoughnessAtLocation(flowModel, "Main", channel1, 75, RoughnessType.Chezy, 45)

AddRoughnessAtLocation(flowModel, "Main", channel2, 25, RoughnessType.Chezy, 45)
AddRoughnessAtLocation(flowModel, "Main", channel2, 50, RoughnessType.Chezy, 45)
AddRoughnessAtLocation(flowModel, "Main", channel2, 75, RoughnessType.Chezy, 45)

# set roughness function for channel 2
locations = [25.0, 50.0, 75.0]
hList = [[1.0, 41.0, 42.0, 43.0], # h, 25, 50, 75
        [2.0, 42.0, 43.0, 45.0]]

SetRoughnessFunctionTypeByChannel(flowModel, "Main", channel2, RoughnessFuntionType.Waterlevel, locations, hList)
 
#endregion

#region set initial values
 
# set initial waterlevel
SetInitialConditionType(flowModel, InitialConditionType.WaterLevel)

flowModel.DefaultInitialWaterLevel = 0
flowModel.InitialConditions.Locations.Clear()
AddInitialValueAtLocation(flowModel, channel1, 25, 0.5)
AddInitialValueAtLocation(flowModel, channel1, 75, -0.5)
AddInitialValueAtLocation(flowModel, channel2, 50, 1.0)
AddInitialValueAtLocation(flowModel, channel2, 75, -0.5)
#endregion 

#region add wind level
 
x = flowModel.Wind
x[flowModel.CurrentTime] = (2,3)
x[flowModel.CurrentTime.AddHours(4)] = (4,4)
x[flowModel.CurrentTime.AddHours(8)] = (6,4)
x[flowModel.CurrentTime.AddHours(12)] = (8,5)
x[flowModel.CurrentTime.AddHours(16)] = (5,5)
x[flowModel.CurrentTime.AddHours(20)] = (4,6) 
#endregion
 
# enable (add) output for (Current)discharge on laterals 
EnableOutput(flowModel, ElementSet.Laterals, QuantityType.Discharge, AggregationOptions.Current )
 
# create computational grid with calculation points at every 5 m
CreateComputationalGrid(flowModel, gridAtFixedLength = True, fixedLength = 5)

# run model
RunModel(flowModel)
 
# get timeseries for discharge at observation points points
obspoint1 = GetBranchObjectByType(flowModel.Network, BranchObjectType.ObservationPoint, "observationpoint1")
timeSeriesPoint1 = GetTimeSeriesFromWaterFlowModel(flowModel, obspoint1, "Discharge")

obspoint2 = GetBranchObjectByType(flowModel.Network, BranchObjectType.ObservationPoint, "observationpoint2")
timeSeriesPoint2 = GetTimeSeriesFromWaterFlowModel(flowModel, obspoint2, "Discharge")