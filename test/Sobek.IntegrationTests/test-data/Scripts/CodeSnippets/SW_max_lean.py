from API import *

# Case 1: Batch run
Application.OpenProject("BernhardSW_max.dsproj")
flow = CurrentProject.RootFolder["Base case"]

# Changing constant boundaries and save them to completely new projects
#constantBoundaries = [1.5, 1.75, 2.0]
#for x in constantBoundaries:
#   EditConstantBoundaryCondition(flow, "benedenrand_ZwarteWater", x)
#   Application.RunActivity(flow)
#   timeseries = GetWaterLevelResultsAtLateralSource(flow, "Zwolle")
#   projectName = ComposeProjectName("SW_max", x)
#   Application.SaveProjectAs(projectName)

# Changing boundary type to flow time series, run with three different time series 
# and save them to three new cases within the same project
ChangeBoundaryConditionType(flow, "Boxbergen", "FlowTimeSeries")
# Set the names for the three cases
cases = ["average case", "high case", "low case"]
# Set the time series for the three cases
values = [[0,2,5,10,5,2,0],[0,4,10,20,10,4,0],[0,1,2.5,5,2.5,1,0]]
dates = [System.DateTime(2007,1,15,1,0,0),System.DateTime(2007,1,15,9,0,0),System.DateTime(2007,1,15,17,0,0),System.DateTime(2007,1,16,1,0,0),System.DateTime(2007,1,16,9,0,0),System.DateTime(2007,1,16,17,0,0),System.DateTime(2007,1,17,1,0,0)]
for i in range(len(cases)):
    case = cases[i]
    v = values[i]
    flow_clone = CloneFlowModel(flow, case)
    CurrentProject.RootFolder.Add(flow_clone)
    bc = GetBoundaryCondition(flow_clone, "Boxbergen")
    bc.Data.Clear()
    for j in range(len(v)):
        bc.Data[dates[j]]= float(v[j])
    # Run the model
    Application.RunActivity(flow_clone)

#Save the project
Application.SaveProjectAs("SW_max_Boxbergen_varying.dsproj")