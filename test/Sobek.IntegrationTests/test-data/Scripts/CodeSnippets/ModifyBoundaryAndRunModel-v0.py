# Run this script by typing:
# Application.ScriptRunner.RunScript(IO.File.ReadAllText("script.py"))
# or Application.ScriptRunner.RunScript((open('script.py', 'r').read()))
import time
import System

flow = CurrentProject.RootFolder["flow model 1d"]

# while loop is currently not working, when this is fixed use:
# while (flow.Status != "Finished" and flow.Status != "Failed"):

# Find the desired Boundary condition
bcArray = flow.BoundaryConditions
for i in range(bcArray.Count):
    if bcArray[i].Node.Name == "benedenrand_ZwarteWater":
       bc = bcArray[i]
       bc.WaterLevel = 2.00
print bc.Name
# Or use (only when you are certain that it is a unique boundary condition):
# bc = [x for x in flow.BoundaryConditions if x.Node.Name == "benedenrand_ZwarteWater"][0]
# bc.WaterLevel = 1.5
# print bc.Name

# Initialize and run model
flow.Initialize()
TotalTimesteps = int((flow.StopTime-flow.StartTime).TotalSeconds/flow.TimeStep.TotalSeconds)
for i in range(1, (TotalTimesteps + 1)):
    flow.Execute()
    print "Timestep:", i
# or use:
# Application.RunActivity(flow)
	
print flow.Status

# Get results at desired location
lateralSources = list(flow.Network.LateralSources)
lateralSource = [x for x in lateralSources if x.Name == "Zwolle"][0]
ts = flow.OutputWaterLevel.GetTimeSeries(lateralSource)
times = ts.Arguments[0].Values
values = ts.GetValues()
for i in range(0, times.Count):
	print times[i], ' ', values[i]
# When running script within DeltaShell GUI run command below to get plot of timeseries
Gui.CommandHandler.OpenView(ts)
# When running outside DeltaShell GUI go to working directory and inspect *.his files
# print flow.WorkingDirectory

# Or: get lateral source data item from model
# lateralSourceData = [x for x in flow.LateralSourceData if x.Feature.Name == "Zwolle"][0]
# ts = flow.OutputWaterLevel.GetTimeSeries(lateralSourceData.Feature)
# times = ts.Arguments[0].Values
# values = ts.GetValues()
#for i in range(0, times.Count):
#	print times[i], ' ', values[i]
