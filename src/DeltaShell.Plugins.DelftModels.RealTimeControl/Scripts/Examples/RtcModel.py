from Libraries.MapFunctions import CreateLineGeometry, CreatePointGeometry
from Libraries.NetworkFunctions import *
from Libraries.SobekFunctions import CreateIntegratedModel
from Libraries.StandardFunctions import AddToProject,GetItemByName
from Libraries.SobekWaterFlowFunctions import CreateComputationalGrid, WaterFlowModel1D,BoundaryConditionType, SetBoundaryCondition
from Libraries.RtcModelFunctions import *

flowModel = WaterFlowModel1D()
rtcModel = RealTimeControlModel()

integratedModel = CreateIntegratedModel([flowModel, rtcModel])

#region create network

node1 = HydroNode(Name = "node1", Geometry = CreatePointGeometry(10,10))
node2 = HydroNode(Name = "node2", Geometry = CreatePointGeometry(20,20))

channel1 = Channel(Name = "channel 1", Source = node1, Target = node2, 
                   Geometry = CreateLineGeometry([[10, 10],[20, 20]]))

flowModel.Network.Nodes.AddRange([node1, node2])
flowModel.Network.Branches.Add(channel1)

weir1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.Weir, "Weir1", channel1, 7)
observationPoint1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.ObservationPoint, "ObservationPoint1", channel1, 3)

crossSection1 = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.CrossSectionYZ, "crossSection1", channel1, 7)
crossSectionProfile1 = [[0,0,0], # y, z, storage
                        [2,0,0],
                        [4,-10,0],
                        [6,-10,0],
                        [8,0,0],
                        [10,0,0]]

SetCrossSectionProfile(crossSection1, crossSectionProfile1, 5) # thalweg = 5
#endregion

SetBoundaryCondition(flowModel, "node1", BoundaryConditionType.FlowConstant, 1)
SetBoundaryCondition(flowModel, "node2", BoundaryConditionType.WaterLevelConstant, 0)

# create computational grid for flowModel with calculation points at every 0.5 m
CreateComputationalGrid(flowModel, gridAtFixedLength = True, fixedLength = 0.5)

controlGroup = ControlGroup(Name = "Control group 1")

rule = PIDRule()

rule.Kp = 1.0
rule.Setting.Min = -9.0
rule.Setting.Max = 1.0
rule.Setting.MaxSpeed = 1.0

input = Input(ParameterName = "Water level (op)", Feature = observationPoint1)
output = Output(ParameterName = "Crest level (s)", Feature = weir1)

# connect to input and output
rule.Inputs.Add(input)
rule.Outputs.Add(output)

controlGroup.Inputs.Add(input)
controlGroup.Outputs.Add(output)

controlGroup.Rules.Add(rule)
rtcModel.ControlGroups.Add(controlGroup)

# Connect the inputs and outputs to the inputs and outputs of the used models
ConnectControlGroup(rtcModel, controlGroup)

AddToProject(integratedModel)

