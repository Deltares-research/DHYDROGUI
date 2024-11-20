from datetime import datetime

from Libraries.StandardFunctions import *
from Libraries.MapFunctions import CreateLineGeometry, CreatePointGeometry
from Libraries.NetworkFunctions import *
from Libraries.SobekWaterFlowFunctions import *
from Libraries.SobekFunctions import *

# create flowmodel
flowModel = WaterFlowModel1D()

#region create simple network containing 2 nodes and 1 branches

node1 = HydroNode(Name = "node1", Geometry = CreatePointGeometry(0, 0))
node2 = HydroNode(Name = "node2", Geometry = CreatePointGeometry(100, 0))

channel1 = Channel(Name = "channel 1", Source = node1, Target = node2, Geometry = CreateLineGeometry([[0, 0],[100, 0]]))

flowModel.Network.Branches.AddRange([channel1])
flowModel.Network.Nodes.AddRange([node1, node2])

# create 2 cross sections and set their profile
crossSection1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.CrossSectionYZ, "crossSection1", channel1, 50)

crossSectionProfile1 = [[0,0,0], # y, z, storage
                        [2,0,0],
                        [4,-10,0],
                        [6,-10,0],
                        [8,0,0],
                        [10,0,0]]
                        
SetCrossSectionProfile(crossSection1, crossSectionProfile1, 5) # thalweg = 5

SetBoundaryCondition(flowModel, "node1", BoundaryConditionType.FlowConstant, 1)
SetBoundaryCondition(flowModel, "node2", BoundaryConditionType.WaterLevelConstant, 0)

# add observationpoint
observationpoint = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.ObservationPoint, "observationpoint", channel1, 95)
#endregion

# create computational grid with calculation points at every 5 m
CreateComputationalGrid(flowModel, gridAtFixedLength = True, fixedLength = 5)

# run model
RunModel(flowModel)

# get timeseries for discharge at observationpoint
obspoint = GetBranchObjectByType(flowModel.Network, BranchObjectType.ObservationPoint, "observationpoint")
timeSeriesPoint = GetTimeSeriesFromWaterFlowModel(flowModel, obspoint, "Discharge")