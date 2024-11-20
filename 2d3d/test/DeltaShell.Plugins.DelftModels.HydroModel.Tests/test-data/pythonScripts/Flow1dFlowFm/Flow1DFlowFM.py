#region Import libs
from datetime import datetime, time
from System import TimeSpan
from Libraries.Conversions import *
from Libraries.StandardFunctions import *
from Libraries.NetworkFunctions import *
from Libraries.SobekWaterFlowFunctions import *
from Libraries.FlowFlexibleMeshFunctions import *
from Libraries.MapFunctions import CreateLineGeometry, CreatePointGeometry, CreateCoordinateSystem
from Libraries.SobekFunctions import *
from DelftTools.Utils.Globalization import RegionalSettingsManager
from DelftTools.Hydro.Structures import Gate
from DelftTools.Hydro import Embankment
from NetTopologySuite.Extensions.Features import FixedWeir
#endregion

#region create a fmModel

fmModel = WaterFlowFMModel()
flowModel = WaterFlowModel1D()
integratedModel = CreateIntegratedModel([flowModel,fmModel], WorkingDir)
AddToProject(integratedModel)
#fmModel.CoordinateSystem = CreateCoordinateSystem(3857) # WGS 84 / Pseudo-Mercator

# Rename Project
Application.Project.Name = "Flow1D+FlowFM"

# set model times
SetModelTimes(integratedModel, datetime(2016,1,1, 0,0,0), datetime(2016,1,1, 1,0,0), time(0,1,0))
SetModelTimes(fmModel, datetime(2016,1,1, 0,0,0), datetime(2016,1,1, 1,0,0), time(0,1,0))
SetModelTimes(flowModel, datetime(2016,1,1, 0,0,0), datetime(2016,1,1, 1,0,0), time(0,1,0))

fmModel.OutputTimeStep = TimeSpan(0,1,0)
flowModel.OutputTimeStep = TimeSpan(0,1,0)
flowModel.OutputSettings.StructureOutputTimeStep = TimeSpan(0,1,0)
#-> grid punten (Map file)
#Hisout -> obs punten (His file)
#endregion

#region create model
# create 2 nodes and a channel
node1 = HydroNode(Name = "node1", Geometry = CreatePointGeometry(0, 2050))
node2 = HydroNode(Name = "node2", Geometry = CreatePointGeometry(2000, 2050))
node3 = HydroNode(Name = "node3", Geometry = CreatePointGeometry(0, -50))
node4 = HydroNode(Name = "node4", Geometry = CreatePointGeometry(2000, -50))

channel1 = Channel(Name = "channel 1", Source = node1, Target = node2, Geometry = CreateLineGeometry([[0, 2050],[2000, 2050]]))
channel2 = Channel(Name = "channel 2", Source = node3, Target = node4, Geometry = CreateLineGeometry([[0, -50],[2000, -50]]))

flowModel.Network.Branches.AddRange([channel1, channel2])
flowModel.Network.Nodes.AddRange([node1, node2, node3, node4])

# create crosssection
crossSection1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.CrossSectionYZ, "crossSection1", channel1, 1000)
crossSectionProfile1 = [[0,0,0], # y, z, storage
                        [20,-6,0],
                        [40,-10,0],
                        [60,-10,0],
                        [80,-6,0],
                        [100,0,0]]
SetCrossSectionProfile(crossSection1, crossSectionProfile1, 50) # thalweg = 50

crossSection2 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.CrossSectionYZ, "crossSection2", channel2, 1000)
SetCrossSectionProfile(crossSection2, crossSectionProfile1, 50) # thalweg = 50

# add observationpoint
observationpoint1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.ObservationPoint, "observationpoint1", channel2, 1950)

# set boundary conditions for node 1 and 2
SetBoundaryCondition(flowModel, "node1", BoundaryConditionType.FlowConstant, 1000)
#SetBoundaryCondition(flowModel, "node2", BoundaryConditionType.WaterLevelConstant, 0.1)

# set initial condition
SetInitialConditionType(flowModel, InitialConditionType.WaterLevel)
flowModel.DefaultInitialWaterLevel = 0
flowModel.InitialConditions.Locations.Clear()

# create computational grid
CreateComputationalGrid(flowModel, gridAtFixedLength = True, fixedLength = 25)

# create embankment
embankment1 = Embankment(Name = "embankment1", Geometry = CreateLineGeometry([[0, 2000],[2000, 2000]]))
embankment1.Geometry.Coordinates[0].Z = 0.0
embankment1.Geometry.Coordinates[1].Z = 0.0
fmModel.Area.Embankments.Add(embankment1)

embankment2 = Embankment(Name = "embankment2", Geometry = CreateLineGeometry([[0, 0],[2000, 0]]))
embankment2.Geometry.Coordinates[0].Z = 0.0
embankment2.Geometry.Coordinates[1].Z = 0.0
fmModel.Area.Embankments.Add(embankment2)

#endregion

#region
# create regular grid
fmModel.Grid.Clear()
GenerateRegularGridForModel(fmModel, 20, 20, 100, 100, 0, 0)

# set bathymetry
list = []
for i in range(len(fmModel.Grid.Vertices)):
    list.append(0.0)
    
fmModel.Bathymetry.SetValues(list)
#endregion

# run fm model
RunModel(integratedModel)

# get waterlevel timeseries for observation point and observation crosssection
obspoint1 = GetBranchObjectByType(flowModel.Network, BranchObjectType.ObservationPoint, "observationpoint1")
timeSeriesPoint1 = GetTimeSeriesFromWaterFlowModel(flowModel, obspoint1, "Water level")