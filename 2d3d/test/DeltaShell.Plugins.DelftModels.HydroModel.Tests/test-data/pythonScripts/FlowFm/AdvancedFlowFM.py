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
from Libraries.SpatialOperations import *
from DelftTools.Utils.Globalization import RegionalSettingsManager
from DelftTools.Hydro.Structures import Gate
from NetTopologySuite.Extensions.Features import FixedWeir, PointFeature, Feature2DPolygon

#endregion

#region create a fmModel

fmModel = WaterFlowFMModel()
integratedModel = CreateIntegratedModel([fmModel], WorkingDir)
AddToProject(integratedModel)
#fmModel.CoordinateSystem = CreateCoordinateSystem(3857) # WGS 84 / Pseudo-Mercator

# Rename Project
Application.Project.Name = "Advanced_FlowFM"

# set model times
SetModelTimes(fmModel, datetime(2016,1,1, 0,0,0), datetime(2016,1,1, 1,0,0), time(0,1,0))

fmModel.OutputTimeStep = TimeSpan(0,1,0)
#-> grid punten (Map file)
#Hisout -> obs punten (His file)
#endregion

#region create model
# create regular grid
fmModel.Grid.Clear()
GenerateRegularGridForModel(fmModel, 5, 11, 100, 100, 0, 0)
GenerateRegularGridForModel(fmModel, 5, 1, 100, 100, 1600, 800, True)
GenerateRegularGridForModel(fmModel, 5, 1, 100, 100, 1600, 600, True)
GenerateRegularGridForModel(fmModel, 5, 1, 100, 100, 1600, 400, True)

# add observation point
observationPoint1 = Feature2DPoint(Name = "ObservationPoint1", Geometry = CreatePointGeometry(250, 50))
fmModel.Area.ObservationPoints.Add(observationPoint1)
observationPoint2 = Feature2DPoint(Name = "ObservationPoint2", Geometry = CreatePointGeometry(2050, 850))
fmModel.Area.ObservationPoints.Add(observationPoint2)
observationPoint3 = Feature2DPoint(Name = "ObservationPoint3", Geometry = CreatePointGeometry(2050, 650))
fmModel.Area.ObservationPoints.Add(observationPoint3)
observationPoint4 = Feature2DPoint(Name = "ObservationPoint4", Geometry = CreatePointGeometry(2050, 450))
fmModel.Area.ObservationPoints.Add(observationPoint4)

# add observation crosssection
obscross1 = ObservationCrossSection2D(Name = "ObservationCrossSection1", Geometry = CreateLineGeometry([[0, 100],[500, 100]]))
fmModel.Area.ObservationCrossSections.Add(obscross1)

# add Pump
pump = Pump(Name = "Pump 1", Geometry = CreateLineGeometry([[0, 900],[500, 900]]))
pump.Capacity = 40
fmModel.Area.Pumps.Add(pump)

# add weir
weir = Weir(Name = "Weir 1", Geometry = CreateLineGeometry([[0, 700],[500, 700]]))
weir.CrestLevel = 0.5
fmModel.Area.Weirs.Add(weir)

# add gate
gate = Gate(Name = "Gate 1", Geometry = CreateLineGeometry([[0, 500],[500, 500]]))
gate.SillLevel = 0
gate.OpeningWidth = 200
fmModel.Area.Gates.Add(gate)

# add Fixed Weir
fw = FixedWeir(Name = "Fixed Weir 1", Geometry = CreateLineGeometry([[0, 300],[500, 300]]))
fmModel.Area.FixedWeirs.Add(fw)

# add dry point
drypoint = PointFeature()
drypoint.Geometry = CreatePointGeometry(1850, 850)
fmModel.Area.DryPoints.Add(drypoint)

# add thin dam
thindam = ThinDam2D(Name = "Thin Dam 1", Geometry = CreateLineGeometry([[1800, 600],[1800, 700]]))
fmModel.Area.ThinDams.Add(thindam)

# add dry area
dryarea = Feature2DPolygon()
dryarea.Geometry = CreatePolygon([[1800, 400],[1900, 400],[1900, 500],[1800, 500],[1800, 400]])
fmModel.Area.DryAreas.Add(dryarea)

# set bathymetry
list = []
for i in range(len(fmModel.Grid.Vertices)):
    list.append((i * 0.01))
    
fmModel.Bathymetry.SetValues(list)

# add 2 boundaries
lb = [0, 0]
rb = [500, 0]
lu = [0, 1100]
ru = [500, 1100]

boundary1 = Feature2D(Name = "boundary 1", Geometry = CreateLineGeometry([lb, rb]))
boundary2 = Feature2D(Name = "boundary 2", Geometry = CreateLineGeometry([lu, ru]))

fmModel.Boundaries.AddRange([boundary1, boundary2])

# add timeseries to boundary1 - support point 0 main area
flowBoundaryCondition1 = AddFlowBoundaryCondition(fmModel, boundary1.Name, FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)

flowBoundaryCondition1.Factor = 2.0
flowBoundaryCondition1.Offset = 1.0

boundaryTimeSeries1 = [[datetime(2016,1,1, 0,0,0), -1.0],
              [datetime(2016,1,1, 1,0,0), -1.0]]

AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition1, 0, boundaryTimeSeries1)
AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition1, 1, boundaryTimeSeries1)

flowBoundaryCondition2 = AddFlowBoundaryCondition(fmModel, boundary2.Name, FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.TimeSeries)

boundaryTimeSeries2 = [[datetime(2016,1,1, 0,0,0), 50.0],
              [datetime(2016,1,1, 1,0,0), 50.0]]


AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition2, 0, boundaryTimeSeries2)
AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition2, 1, boundaryTimeSeries2)

# add boundary for drypoint, dry area and thindam

boundary3 = Feature2D(Name = "boundary drypoint discharge", Geometry = CreateLineGeometry([[1600,800], [1600,900]]))
boundary4 = Feature2D(Name = "boundary thinpoint discharge", Geometry = CreateLineGeometry([[1600,600], [1600,700]]))
boundary5 = Feature2D(Name = "boundary dry area discharge", Geometry = CreateLineGeometry([[1600,400], [1600,500]]))
fmModel.Boundaries.AddRange([boundary3, boundary4, boundary5])

boundaryTimeSeries2 = [[datetime(2016,1,1, 0,0,0), 2.0],
              [datetime(2016,1,1, 1,0,0), 2.0]]
              
flowBoundaryCondition3 = AddFlowBoundaryCondition(fmModel, boundary3.Name, FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.TimeSeries)
AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition3, 0, boundaryTimeSeries2)
AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition3, 1, boundaryTimeSeries2)

flowBoundaryCondition4 = AddFlowBoundaryCondition(fmModel, boundary4.Name, FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.TimeSeries)
AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition4, 0, boundaryTimeSeries2)

flowBoundaryCondition5 = AddFlowBoundaryCondition(fmModel, boundary5.Name, FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.TimeSeries)
AddTimeSeriesToSupportPoint(fmModel, flowBoundaryCondition5, 0, boundaryTimeSeries2)
#endregion

# run fm model
RunModel(integratedModel)

# get waterlevel timeseries for observation point and observation crosssection
waterlevelSeries = GetFlowFlexibleMeshTimeSeries(fmModel, "water level (waterlevel)", observationPoint1)
dischargeSeries = GetFlowFlexibleMeshTimeSeries(fmModel, "cross_section_discharge", obscross1)