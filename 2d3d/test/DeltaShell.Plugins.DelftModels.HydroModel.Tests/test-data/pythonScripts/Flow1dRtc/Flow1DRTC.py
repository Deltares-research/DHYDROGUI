from datetime import datetime

from System import TimeSpan, DateTime
from Libraries.StandardFunctions import *
from Libraries.MapFunctions import CreateLineGeometry, CreatePointGeometry
from Libraries.NetworkFunctions import *
from Libraries.SobekWaterFlowFunctions import *
from Libraries.SobekFunctions import *
from Libraries.RtcModelFunctions import *
from DeltaShell.Plugins.DelftModels.RealTimeControl.Domain import StandardCondition, RelativeTimeRule, Operation

# create flow + rtc model
flowModel = WaterFlowModel1D()
flowModel.TimeStep = TimeSpan(0,1,0)
flowModel.OutputTimeStep = TimeSpan(0,1,0) 
flowModel.OutputSettings.StructureOutputTimeStep = TimeSpan(0,1,0)
flowModel.StartTime = DateTime(2016, 1, 1, 0, 0, 0)
flowModel.StopTime = DateTime(2016, 1, 1, 1, 0, 0)

rtcModel = RealTimeControlModel()
integratedModel = CreateIntegratedModel([flowModel, rtcModel], WorkingDir)

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

SetBoundaryCondition(flowModel, "node1", BoundaryConditionType.FlowConstant, 0.05)
SetBoundaryCondition(flowModel, "node2", BoundaryConditionType.WaterLevelConstant, 0)

# add observationpoint
observationpoint1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.ObservationPoint, "observationpoint1", channel1, 25)
observationpoint2 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.ObservationPoint, "observationpoint2", channel1, 75)

# add weir
weir1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.Weir, "weir", channel1, 50)
weir1.CrestLevel = 1
weir1.CrestWidth = 10

# set initial water level
SetInitialConditionType(flowModel, InitialConditionType.WaterLevel)
flowModel.DefaultInitialWaterLevel = 0
flowModel.InitialConditions.Clear()

#endregion

# create computational grid with calculation points at every 5 m
CreateComputationalGrid(flowModel, gridAtFixedLength = True, fixedLength = 5)

#region Create rtc model
controlGroup = ControlGroup(Name = "Control Group 1")

input = Input()
input.ParameterName = "Water level (op)"
input.Feature = observationpoint1

condition = StandardCondition()
condition.Input = input
condition.Operation = Operation.Greater 
condition.Value = 0.1


output = Output(ParameterName = "Crest level (s)", Feature = weir1)

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

# run model
RunModel(integratedModel)

# get timeseries for discharge at observationpoint
obspoint1 = GetBranchObjectByType(flowModel.Network, BranchObjectType.ObservationPoint, "observationpoint1")
obspoint2 = GetBranchObjectByType(flowModel.Network, BranchObjectType.ObservationPoint, "observationpoint2")

timeSeriesPoint1 = GetTimeSeriesFromWaterFlowModel(flowModel, obspoint2, "Discharge")
timeSeriesPoint2 = GetTimeSeriesFromWaterFlowModel(flowModel, obspoint1, "Water level")