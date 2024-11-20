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
#endregion

#region create a fmModel

fmModel = WaterFlowFMModel()
integratedModel = CreateIntegratedModel([fmModel], WorkingDir)
AddToProject(integratedModel)
#fmModel.CoordinateSystem = CreateCoordinateSystem(3857) # WGS 84 / Pseudo-Mercator

# Rename Project
Application.Project.Name = "Basic_FlowFM"

# set model times
SetModelTimes(fmModel, datetime(2016,1,1, 0,0,0), datetime(2016,1,1, 1,0,0), time(0,1,0))

fmModel.OutputTimeStep = TimeSpan(0,1,0)
#-> grid punten (Map file)
#Hisout -> obs punten (His file)
#endregion

#region create model
# create regular grid
fmModel.Grid.Clear()
GenerateRegularGridForModel(fmModel, 5, 5, 100, 100, 0, 0)

# add observation point
observationPoint = Feature2DPoint(Name = "ObservationPoint1", Geometry = CreatePointGeometry(250, 250))
fmModel.Area.ObservationPoints.Add(observationPoint)

# add observation crosssection
obscross1 = ObservationCrossSection2D(Name = "ObservationCrossSection1", Geometry = CreateLineGeometry([[0, 100],[500, 100]]))
fmModel.Area.ObservationCrossSections.Add(obscross1)

# set bathymetry
list = []
for i in range(len(fmModel.Grid.Vertices)):
    list.append(0.0)
    
fmModel.Bathymetry.SetValues(list)

# add 4 boundaries
lb = [0, 0]
rb = [500, 0]
lu = [0, 500]
ru = [500, 500]

boundary1 = Feature2D(Name = "boundary 1", Geometry = CreateLineGeometry([lb, rb]))
boundary2 = Feature2D(Name = "boundary 2", Geometry = CreateLineGeometry([lu, ru]))

fmModel.Boundaries.AddRange([boundary1, boundary2])

# add timeseries to boundary1 - support point 0
flowBoundaryCondition1 = AddFlowBoundaryCondition(fmModel, boundary1.Name, FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)

boundaryTimeSeries = [[datetime(2016,1,1, 0,0,0), 0.0],
              [datetime(2016,1,1, 1,0,0), 0.0]]

AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition1, 0, boundaryTimeSeries)
AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition1, 1, boundaryTimeSeries)

flowBoundaryCondition2 = AddFlowBoundaryCondition(fmModel, boundary2.Name, FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.TimeSeries)

boundaryTimeSeries = [[datetime(2016,1,1, 0,0,0), 10.0],
              [datetime(2016,1,1, 1,0,0), 10.0]]

AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition2, 0, boundaryTimeSeries)
#endregion

# run fm model
RunModel(integratedModel)

# get waterlevel timeseries for observation point and observation crosssection
waterlevelSeries = GetFlowFlexibleMeshTimeSeries(fmModel, "water level (waterlevel)", observationPoint)
dischargeSeries = GetFlowFlexibleMeshTimeSeries(fmModel, "cross_section_discharge", obscross1)