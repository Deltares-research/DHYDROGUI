from datetime import datetime, time
from System import TimeSpan

from Libraries.StandardFunctions import *
from Libraries.MapFunctions import CreateLineGeometry, CreatePointGeometry
from Libraries.NetworkFunctions import *
from Libraries.SobekWaterFlowFunctions import *
from Libraries.SobekFunctions import *
from Libraries.RainfallRunoffModelFunctions import *
from Libraries.SpatialOperations import *

# create flowmodel
flowModel = WaterFlowModel1D()
rrModel = RainfallRunoffModel()
integratedModel = CreateIntegratedModel([flowModel, rrModel], WorkingDir)
AddToProject(integratedModel)

# set model times
SetModelTimes(integratedModel, datetime(2016,1,1, 0,0,0), datetime(2016,1,1, 1,0,0), time(0,1,0))
SetModelTimes(flowModel, datetime(2016,1,1, 0,0,0), datetime(2016,1,1, 1,0,0), time(0,1,0))
SetModelTimes(rrModel, datetime(2016,1,1, 0,0,0), datetime(2016,1,1, 1,0,0), time(0,1,0))

rrModel.OutputTimeStep = TimeSpan(0,1,0)
flowModel.OutputTimeStep = TimeSpan(0,1,0)
flowModel.OutputSettings.StructureOutputTimeStep = TimeSpan(0,1,0)

# Rename Project
Application.Project.Name = "Flow1D+RR"

#region create simple network containing 2 nodes and 1 branches

node1 = HydroNode(Name = "node1", Geometry = CreatePointGeometry(0, 0))
node2 = HydroNode(Name = "node2", Geometry = CreatePointGeometry(100, 0))

channel1 = Channel(Name = "channel 1", Source = node1, Target = node2, Geometry = CreateLineGeometry([[0, 0],[100, 0]]))

flowModel.Network.Branches.AddRange([channel1])
flowModel.Network.Nodes.AddRange([node1, node2])

# create a cross sections and set their profile
crossSection1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.CrossSectionYZ, "crossSection1", channel1, 50)

crossSectionProfile1 = [[0,0,0], # y, z, storage
                        [2,0,0],
                        [4,-10,0],
                        [6,-10,0],
                        [8,0,0],
                        [10,0,0]]
                        
SetCrossSectionProfile(crossSection1, crossSectionProfile1, 5) # thalweg = 5

# add observationpoint
observationpoint = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.ObservationPoint, "observationpoint", channel1, 95)

# add a lateral
lateral1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.LateralSource, "lateral1", channel1, 25 )

# set initial conditions
SetInitialConditionType(flowModel, InitialConditionType.WaterLevel)
flowModel.InitialConditions.Locations.Clear()
flowModel.DefaultInitialWaterLevel = 0
#endregion

#region create RR model

# create catchment
catchment = Catchment(Name = "Catchment1",CatchmentType = CatchmentType.Unpaved)
catchment.Geometry = CreatePolygon([[0, 50],[0, 150],[100, 150],[100, 50],[0, 50]])
rrModel.Basin.Catchments.Add(catchment)

# create link from catchment to lateral source
link = AddLink(catchment, lateral1)

# set Meteorological Data
timeSeries1 = [[datetime(2016, 1, 1, 0, 0, 0), 10.0],
        [datetime(2016, 1, 1, 1, 0, 0), 10.0]]
        
timeSeries2 = [[datetime(2016, 1, 1, 0, 0, 0), 0.0],
        [datetime(2016, 1, 1, 1, 0, 0), 0.0]]

SetMeteorologicalDataTimeSeries(rrModel, MeteoDataType.Precipitation, timeSeries1)
SetMeteorologicalDataTimeSeries(rrModel, MeteoDataType.Evaporation, timeSeries2)
#endregion

# create computational grid with calculation points at every 5 m
CreateComputationalGrid(flowModel, gridAtFixedLength = True, fixedLength = 5)

# run model
RunModel(integratedModel)

# get timeseries for discharge at observationpoint
obspoint = GetBranchObjectByType(flowModel.Network, BranchObjectType.ObservationPoint, "observationpoint")
timeSeriesPoint = GetTimeSeriesFromWaterFlowModel(flowModel, obspoint, "Water level")