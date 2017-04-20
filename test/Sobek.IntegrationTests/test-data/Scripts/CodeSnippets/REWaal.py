import API
from System.Collections.Generic import List
from DelftTools.Functions import *

# Import specific API functions
GetRoughnessSection = API.GetRoughnessSection
ChangeRoughnessValuesForSection = API.ChangeRoughnessValuesForSection
InitializeAndRunModel = API.InitializeAndRunModel
CloneFlowModel = API.CloneFlowModel
AddObservationPointOnBranch = API.AddObservationPointOnBranch
 
# create time series for plotting
timeseries = List[IFunction]()
flow = CurrentProject.RootFolder["Base case"]

# Add observation point on first branch
branch = flow.Network.Branches[0]
obs = AddObservationPointOnBranch(branch, 30000)

cases = ["mainChezy45", "mainChezy100", "FP1Chezy45", "FP1Chezy100"]
roughnessValuePerCase = [45, 100]

# Edit roughness for Main and run two cases 
for i in range(len(roughnessValuePerCase)):
   case = cases[i]
   flow_clone = CloneFlowModel(flow, case)
   obs_clone = list(flow_clone.Network.ObservationPoints)[0]
   CurrentProject.RootFolder.Add(flow_clone)
   rsMain = GetRoughnessSection(flow_clone,"Main")
   ChangeRoughnessValuesForSection(rsMain, roughnessValuePerCase[i], RoughnessType.Chezy)
   # Run the model
   Application.RunActivity(flow_clone)
   timeseries.Add(flow_clone.OutputFlow.GetTimeSeries(obs_clone))

# Edit roughness for FP1 and run two cases 
for i in range(len(roughnessValuePerCase)):
   case = cases[i+2]
   flow_clone = CloneFlowModel(flow, case)
   obs_clone = list(flow_clone.Network.ObservationPoints)[0]
   CurrentProject.RootFolder.Add(flow_clone)
   rsFP1 = GetRoughnessSection(flow_clone,"FloodPlain1")
   ChangeRoughnessValuesForSection(rsFP1, roughnessValuePerCase[i], RoughnessType.Chezy)
   # Run the model
   Application.RunActivity(flow_clone)
   timeseries.Add(flow_clone.OutputFlow.GetTimeSeries(obs_clone))

# Visualize results
Gui.CommandHandler.OpenView(timeseries)
Application.SaveProjectAs("c:\Users\putten_hs\Desktop\REWaal_test.dsproj")