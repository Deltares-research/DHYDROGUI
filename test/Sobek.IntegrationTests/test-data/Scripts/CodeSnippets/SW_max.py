# Run this script by typing:
# Application.ScriptRunner.RunScript(IO.File.ReadAllText("script.py"))
# or Application.ScriptRunner.RunScript((open('script.py', 'r').read()))
import time
import System
from System.Collections.Generic import List
from DelftTools.Functions import *
from DelftTools.Functions.TimeSeriesFactory import CreateFlowTimeSeries
from DelftTools.Controls.Swf.Charting import PointerStyles
from DelftTools.Utils.Drawing import ColorHelper

###################################################################################################
# This is the function for getting the desired boundary
def GetBoundaryCondition(flow, boundaryname):
   # Find and edit the desired Boundary condition
   bcArray = flow.BoundaryConditions
   for i in range(bcArray.Count):
       if bcArray[i].Node.Name == boundaryname:
          bc = bcArray[i]
   print "Boundary is: ",bc.Name
   return bc
#
####################################################################################################

###################################################################################################
# This is the function for editing a constant boundary 
def EditConstantBoundaryCondition(flow, boundaryname, boundaryvalue):
   # Find and edit the desired Boundary condition
   bc = GetBoundaryCondition(flow, boundaryname)
   bc.WaterLevel = boundaryvalue
   
   # Or use (only when you are certain that it is a unique boundary condition):
   # bc = [x for x in flow.BoundaryConditions if x.Node.Name == "benedenrand_ZwarteWater"][0]
   # bc.WaterLevel = constant
   # print bc.Name
#
####################################################################################################   

###################################################################################################
# This is the function for changing the boundary type
def ChangeBoundaryConditionType(flow, boundaryname, boundarytype):
   from DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects import WaterFlowModel1DBoundaryNodeDataType
   boundarytypeEnum = System.Enum.Parse(WaterFlowModel1DBoundaryNodeDataType,boundarytype)
   # Find and edit the desired Boundary condition
   bc = GetBoundaryCondition(flow, boundaryname)
   print "Current boundary type is: ", bc.DataType
   bc.DataType = boundarytypeEnum
   print "Boundary type changed to: ",bc.DataType

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
# This is the function for getting WaterLevel results at desired Lateral source location
def GetWaterLevelResultsAtLateralSource(flow, location):
   
   # Get results at desired location
   lateralSources = list(flow.Network.LateralSources)
   lateralSource = [x for x in lateralSources if x.Name == location][0]
   ts = flow.OutputWaterLevel.GetTimeSeries(lateralSource)
   times = ts.Arguments[0].Values
   values = ts.GetValues()
   for i in range(0, times.Count):
       print times[i], ' ', values[i]
   Gui.CommandHandler.OpenView(ts)
#
####################################################################################################

###################################################################################################
# This is the function for getting WaterLevel results at desired Calcpoint location
def GetWaterLevelResultsAtCalcPoint(flow, location):
   
   # Get results at desired location
   calcPoints = list(flow.NetworkDiscretization.Locations.Values)
   calcPoint = [x for x in calcPoints if x.Name == location][0]
   ts = flow.OutputWaterLevel.GetTimeSeries(calcPoint)
   times = ts.Arguments[0].Values
   values = ts.GetValues()
   for i in range(0, times.Count):
       print times[i], ' ', values[i]
   Gui.CommandHandler.OpenView(ts)
#
####################################################################################################

###################################################################################################
# This is the function for getting Discharge results at desired Calcpoint location
def GetDischargeResultsAtCalcPoint(flow, location):
   
   # Get results at desired location
   calcPoints = list(flow.NetworkDiscretization.Locations.Values)
   calcPoint = [x for x in calcPoints if x.Name == location][0]
   ts = flow.OutputFlow.GetTimeSeries(calcPoint)
   times = ts.Arguments[0].Values
   values = ts.GetValues()
   for i in range(0, times.Count):
       print times[i], ' ', values[i]
   return ts
#
####################################################################################################

###################################################################################################
# This is the function for setting a new project name 
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

###################################################################################################
# This is the function for adding a discharge timeseries
def AddDischargeTimeSeries(dates, values):
    ts = CreateFlowTimeSeries()
    for i in range(len(values)):
       ts[dates[i]] = values[i]
    return ts
#
####################################################################################################

# Case 1: Batch run
flow = CurrentProject.RootFolder["Base case"]

# Changing constant boundaries and save them to completely new projects
#constantBoundaries = [1.5, 1.75, 2.0]
#for x in constantBoundaries:
#   EditConstantBoundaryCondition(flow, "benedenrand_ZwarteWater", x)
#   InitializeAndRunModel(flow)
#   GetWaterLevelResultsAtLateralSource(flow, "Zwolle")
#   projectName = ComposeProjectName("SW_max", x)
#   Application.SaveProjectAs(projectName)

# Changing boundary type to flow time series, run with three different time series 
# and save them to three new cases within the same project
ChangeBoundaryConditionType(flow, "Boxbergen", "FlowTimeSeries")
# Set the names for the three cases
cases = ["average case", "high case", "low case"]
timeseries = List[IFunction]()
# Set the time series for the three cases
values = [[0,2,5,10,5,2,0],[0,4,10,20,10,4,0],[0,1,2.5,5,2.5,1,0]]
dates = [System.DateTime(2007,1,15,1,0,0),System.DateTime(2007,1,15,9,0,0),System.DateTime(2007,1,15,17,0,0),System.DateTime(2007,1,16,1,0,0),System.DateTime(2007,1,16,9,0,0),System.DateTime(2007,1,16,17,0,0),System.DateTime(2007,1,17,1,0,0)]
for i in range(len(cases)):
    case = cases[i]
    v = values[i]
    flow_clone = CloneFlowModel(flow, case)
    bc = GetBoundaryCondition(flow_clone, "Boxbergen")
    bc.Data.Clear()
    for j in range(len(v)):
        bc.Data[dates[j]]= float(v[j])
    # Run the model and visualize results
    InitializeAndRunModel(flow_clone)
    timeseries.Add(GetDischargeResultsAtCalcPoint(flow_clone, "Bo_Soestwetering_2"))

# Add an artificial measurement time series
measurements = [0.4, 3.3, 7.1, 15.8, 7.9, 3.2, 0.2]
ts_measurements = AddDischargeTimeSeries(dates, measurements)
#Add measurements to timeseries
timeseries.Add(ts_measurements)

# Plot results
Gui.CommandHandler.OpenView(timeseries)
activeView = Gui.DocumentViews.ActiveView
activeView.ChartView.Chart.Series[3].Color = ColorHelper.Transparent
activeView.ChartView.Chart.Series[3].PointerStyle = PointerStyles.Triangle
activeView.ChartView.Chart.Series[3].PointerSize = 10
#Save the project
Application.SaveProjectAs("SW_max_Boxbergen_varying.dsproj")