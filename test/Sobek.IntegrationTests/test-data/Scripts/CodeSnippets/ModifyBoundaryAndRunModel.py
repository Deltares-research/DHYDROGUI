# Run this script by typing:
# Application.ScriptRunner.RunScript(IO.File.ReadAllText("script.py"))
# or Application.ScriptRunner.RunScript((open('script.py', 'r').read()))
import time
import System

###################################################################################################
# This is the function for editing a constant boundary 
def EditConstantBoundary(flow, boundaryname, boundaryvalue):
   # Find and edit the desired Boundary condition
   bcArray = flow.BoundaryConditions
   for i in range(bcArray.Count):
       if bcArray[i].Node.Name == boundaryname:
          bc = bcArray[i]
          bc.WaterLevel = boundaryvalue
   print bc.Name
   
   # Or use (only when you are certain that it is a unique boundary condition):
   # bc = [x for x in flow.BoundaryConditions if x.Node.Name == "benedenrand_ZwarteWater"][0]
   # bc.WaterLevel = constant
   # print bc.Name
#
####################################################################################################

###################################################################################################
# This is the function for editing a boundary with time series 
def EditConstantBoundary(flow, boundaryname, boundarytimeseries):
   # Find and edit the desired Boundary condition
   bcArray = flow.BoundaryConditions
   for i in range(bcArray.Count):
       if bcArray[i].Node.Name == boundaryname:
          bc = bcArray[i]
          bc.WaterLevel = boundaryvalue
   print bc.Name
   
   # Or use (only when you are certain that it is a unique boundary condition):
   # bc = [x for x in flow.BoundaryConditions if x.Node.Name == "benedenrand_ZwarteWater"][0]
   # bc.WaterLevel = constant
   # print bc.Name
#
####################################################################################################   

###################################################################################################
# This is the function for changing the boundary type
def ChangeBoundaryType(flow, boundaryname, boundarytype):
   from DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects import WaterFlowModel1DBoundaryNodeDataType
   # Find and edit the desired Boundary condition
   bcArray = flow.BoundaryConditions
   boundarytypeEnum = System.Enum.Parse(WaterFlowModel1DBoundaryNodeDataType,boundarytype)
   for i in range(bcArray.Count):
       if bcArray[i].Node.Name == boundaryname:
          bc = bcArray[i]
          print bc.DataType
          bc.DataType = boundarytypeEnum
   print bc.DataType
#
####################################################################################################   

###################################################################################################
# This is the function running the model 
def InitializeAndRunModel(flow):
   # Initialize and run model
   flow.Initialize()
   TotalTimesteps = int((flow.StopTime-flow.StartTime).TotalSeconds/flow.TimeStep.TotalSeconds)
   for i in range(1, (TotalTimesteps + 1)):
       flow.Execute()
       print "Timestep:", i
   # or use:
   # Application.RunActivity(flow)
   print flow.Status
#
####################################################################################################
  
###################################################################################################
# This is the function for getting results at desired location
def GetWaterLevelResults(flow, location):
   
   # Get results at desired location
   lateralSources = list(flow.Network.LateralSources)
   lateralSource = [x for x in lateralSources if x.Name == location][0]
   ts = flow.OutputWaterLevel.GetTimeSeries(lateralSource)
   times = ts.Arguments[0].Values
   values = ts.GetValues()
   for i in range(0, times.Count):
       print times[i], ' ', values[i]
   # When running script within DeltaShell GUI run command below to get plot of timeseries
   Gui.CommandHandler.OpenView(ts)
#
####################################################################################################

###################################################################################################
# This is the function for getting results at desired location
def ComposeProjectName(mainProjectName, parameter):
   # Compose the projectName for saving
   string = mainProjectName
   string += "_" 
   string += str(parameter)
   string += ".dsproj"
   projectName = string
   return projectName
#
####################################################################################################

###################################################################################################
# This is the function for cloning a flow model
def CloneFlowModel(flowmodel, name_of_clone):
    flow_clone = flow.Clone()
    flow_clone.Name = name_of_clone
    CurrentProject.RootFolder.Add(flow_clone)
    return flow_clone
#
####################################################################################################

# Case 1: Batch run
Application.OpenProject("SW_max.dsproj")
flow = CurrentProject.RootFolder["flow model 1d"]

#constantBoundaries = [1.5, 1.75, 2.0]
#for x in constantBoundaries:
#   EditConstantBoundary(flow, "benedenrand_ZwarteWater", x)
#   InitializeAndRunModel(flow)
#   GetWaterLevelResults(flow, "Zwolle")
#   projectName = ComposeProjectName("SW_max", x)
#   Application.SaveProjectAs(projectName)


ChangeBoundaryType(flow, "Boxbergen", "FlowTimeSeries")
# Set the names for the three cases
cases = ["average case", "high case", "low case"]
# Set the time series for the three cases
time_series = [[0,2,5,10,5,2,0],[0,4,10,20,10,4,0],[0,1,2.5,5,2.5,1,0]]
for i in range(len(cases)): 
    case = cases[i]
    ts = time_series[i]
    print case
    print ts
    #flow_clone = CloneFlowModel(flow, case)