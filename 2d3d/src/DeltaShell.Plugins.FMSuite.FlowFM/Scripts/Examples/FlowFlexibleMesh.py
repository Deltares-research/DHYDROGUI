from datetime import datetime, time

from Libraries.MapFunctions import CreateCoordinateSystem, CreateLineGeometry, CreatePointGeometry
from Libraries.StandardFunctions import *
from Libraries.FlowFlexibleMeshFunctions import *

# create a fmModel
fmModel = WaterFlowFMModel()
fmModel.CoordinateSystem = CreateCoordinateSystem(3857) # WGS 84 / Pseudo-Mercator

AddToProject(fmModel)

# create regular grid
fmModel.Grid.Clear()
GenerateRegularGridForModel(fmModel, 50, 60, 4000, 3000, 230000, 6800000)

# add observation point
observationPoint = Feature2DPoint(Name = "Observation point 1", Geometry = CreatePointGeometry(230000 + 100000, 6800000 + 100000))
fmModel.Area.ObservationPoints.Add(observationPoint)

# set bathymetry
list = []
for i in range(len(fmModel.Grid.Vertices)):
    list.append((i * 0.01))

fmModel.Bathymetry.SetValues(list)

# add 4 boundaries
lb = [230000, 6800000]
rb = [230000 + (50 * 4000), 6800000]
lt = [230000, 6800000 + (60 * 3000)]
rt = [230000 + (50 * 4000), 6800000 + (60 * 3000)]

boundary1 = Feature2D(Name = "boundary 1", Geometry = CreateLineGeometry([lb, rb]))
boundary2 = Feature2D(Name = "boundary 2", Geometry = CreateLineGeometry([lt, rt]))
boundary3 = Feature2D(Name = "boundary 3", Geometry = CreateLineGeometry([lb, lt]))
boundary4 = Feature2D(Name = "boundary 4", Geometry = CreateLineGeometry([rb, rt]))

fmModel.Boundaries.AddRange([boundary1, boundary2, boundary3, boundary4])

# add timeseries to boundary1 - support point 0
flowBoundaryCondition = AddFlowBoundaryCondition(fmModel, boundary1.Name, FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)

flowBoundaryCondition.Factor = 2.0
flowBoundaryCondition.Offset = 1.0

boundaryTimeSeries = [[datetime(2014,1,1, 12,0,0), 10.0],
              [datetime(2014,1,2, 12,0,0), 11.0],
              [datetime(2014,1,3, 12,0,0), 9.0],
              [datetime(2014,1,4, 12,0,0), 10.0]]

AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition, 0, boundaryTimeSeries)

# set model times
SetModelTimes(fmModel, datetime(2014,1,1, 12,0,0), datetime(2014,1,4, 12,0,0), time(12,0,0,))

# run fm model
RunModel(fmModel)

# get waterlevel timeseries for observation point
waterlevelSeries = GetFlowFlexibleMeshTimeSeries(fmModel, "Water level (waterlevel)", observationPoint)

# export timeseries to csv file
ExportListToCsvFile("D:\\test.csv", waterlevelSeries)
