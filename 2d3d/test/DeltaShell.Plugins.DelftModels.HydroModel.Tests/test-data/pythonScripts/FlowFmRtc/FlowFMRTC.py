#region Import libs
from datetime import datetime, time
from System import TimeSpan
from Libraries.Conversions import *
from Libraries.StandardFunctions import *
from Libraries.NetworkFunctions import *
from Libraries.FlowFlexibleMeshFunctions import *
from Libraries.MapFunctions import CreateLineGeometry, CreatePointGeometry, CreateCoordinateSystem
from Libraries.SobekFunctions import *
from DelftTools.Utils.Globalization import RegionalSettingsManager
from Libraries.RtcModelFunctions import *
from DeltaShell.Plugins.DelftModels.RealTimeControl.Domain import StandardCondition, RelativeTimeRule, Operation
#endregion

#region create a flowModel

flowModel = WaterFlowFMModel()
rtcModel = RealTimeControlModel()
integratedModel = CreateIntegratedModel([flowModel, rtcModel], WorkingDir)
AddToProject(integratedModel)
#flowModel.CoordinateSystem = CreateCoordinateSystem(3857) # WGS 84 / Pseudo-Mercator

# Rename Project
Application.Project.Name = "FlowFM_RTC"

# set model times
SetModelTimes(flowModel, datetime(2016,1,1, 0,0,0), datetime(2016,1,1, 1,0,0), time(0,1,0))

flowModel.OutputTimeStep = TimeSpan(0,1,0)
#-> grid punten (Map file)
#Hisout -> obs punten (His file)
#endregion

#region create model
# create regular grid
flowModel.Grid.Clear()
GenerateRegularGridForModel(flowModel, 5, 5, 100, 100, 0, 0)

# add observation point
observationPoint1 = Feature2DPoint(Name = "ObservationPoint1", Geometry = CreatePointGeometry(250, 50))
flowModel.Area.ObservationPoints.Add(observationPoint1)
observationPoint2 = Feature2DPoint(Name = "ObservationPoint2", Geometry = CreatePointGeometry(250, 350))
flowModel.Area.ObservationPoints.Add(observationPoint2)

# add observation crosssection
obscross1 = ObservationCrossSection2D(Name = "ObservationCrossSection1", Geometry = CreateLineGeometry([[0, 100],[500, 100]]))
flowModel.Area.ObservationCrossSections.Add(obscross1)


# add weir
weir = Weir(Name = "Weir 1", Geometry = CreateLineGeometry([[0, 200],[500, 200]]))
weir.CrestLevel = 0.5
flowModel.Area.Weirs.Add(weir)


# set bathymetry
list = []
for i in range(len(flowModel.Grid.Vertices)):
    list.append(0.0)
    
flowModel.Bathymetry.SetValues(list)

# add 2 boundaries
lb = [0, 0]
rb = [500, 0]
lu = [0, 500]
ru = [500, 500]

boundary1 = Feature2D(Name = "boundary 1", Geometry = CreateLineGeometry([lb, rb]))
boundary2 = Feature2D(Name = "boundary 2", Geometry = CreateLineGeometry([lu, ru]))

flowModel.Boundaries.AddRange([boundary1, boundary2])

# add timeseries to boundary1 - support point 0 main area
flowBoundaryCondition1 = AddFlowBoundaryCondition(flowModel, boundary1.Name, FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.TimeSeries)

flowBoundaryCondition1.Factor = 2.0
flowBoundaryCondition1.Offset = 1.0

boundaryTimeSeries1 = [[datetime(2016,1,1, 0,0,0), -1.0],
              [datetime(2016,1,1, 1,0,0), -1.0]]

AddTimeSeriesToSupportPoint(flowModel, flowBoundaryCondition1, 0, boundaryTimeSeries1)
AddTimeSeriesToSupportPoint(flowModel, flowBoundaryCondition1, 1, boundaryTimeSeries1)

flowBoundaryCondition2 = AddFlowBoundaryCondition(flowModel, boundary2.Name, FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.TimeSeries)

boundaryTimeSeries2 = [[datetime(2016,1,1, 0,0,0), 10.0],
              [datetime(2016,1,1, 1,0,0), 10.0]]


AddTimeSeriesToSupportPoint(flowModel, flowBoundaryCondition2, 0, boundaryTimeSeries2)
AddTimeSeriesToSupportPoint(flowModel, flowBoundaryCondition2, 1, boundaryTimeSeries2)

#endregion

#region Create rtc model
controlGroup = ControlGroup(Name = "Control Group 1")

input = Input()
input.ParameterName = "water_level"
input.Feature = observationPoint2

condition = StandardCondition()
condition.Input = input
condition.Operation = Operation.Greater 
condition.Value = 0.1


output = Output(ParameterName = "crest_level", Feature = weir)

rule = RelativeTimeRule("RelativeTimeRule", False)
rule.Function[0.0] = -10.0;
rule.Function[100.0] = -10.0;
rule.Outputs.Add(output)

condition.TrueOutputs.Add(rule)

controlGroup.Inputs.Add(input)
controlGroup.Outputs.Add(output)
controlGroup.Conditions.Add(condition)
controlGroup.Rules.Add(rule)

rtcModel.ControlGroups.Add(controlGroup)

# Connect the inputs and outputs to the inputs and outputs of the used models
ConnectControlGroup(rtcModel, controlGroup)

#endregion

# run fm model
RunModel(integratedModel)

# get waterlevel timeseries for observation point and observation crosssection
waterlevelSeries = GetFlowFlexibleMeshTimeSeries(flowModel, "water level (waterlevel)", observationPoint2)
dischargeSeries = GetFlowFlexibleMeshTimeSeries(flowModel, "cross_section_discharge", obscross1)