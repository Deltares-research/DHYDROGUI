import System
from API import *

# Implement script below

#Application.OpenProject("Exercise2.dsproj")
flow = CurrentProject.RootFolder["Exercise2"]
Application.RunActivity(flow)
results = GetDischargeResultsAtCalcPoint(flow, "channel 1_100.000")
Gui.CommandHandler.OpenView(results)

values = [4,3,2,1,2,3,4]
dates = [System.DateTime(2012,11,29,0,0,0),System.DateTime(2012,11,29,4,0,0),System.DateTime(2012,11,29,8,0,0),System.DateTime(2012,11,29,12,0,0),System.DateTime(2012,11,29,16,0,0),System.DateTime(2012,11,29,20,0,0),System.DateTime(2012,11,30,0,0,0)]
ChangeBoundaryConditionType(flow, "QBound", "FlowTimeSeries")
bc = GetBoundaryCondition(flow, "QBound")
bc.Data.Clear()
for j in range(len(values)):
   bc.Data[dates[j]]= float(values[j])
Application.RunActivity(flow)
results = GetDischargeResultsAtCalcPoint(flow, "channel 1_100.000")
Gui.CommandHandler.OpenView(results)

Application.SaveProjectAs("Exercise2Test.dsproj")