from DeltaShell.Plugins.DelftModels.RealTimeControl import RealTimeControlModel
from DeltaShell.Plugins.DelftModels.RealTimeControl.Domain import ControlGroup, Input, Output, PIDRule
from DelftTools.Shell.Core.Workflow.DataItems import DataItemRole

def _GetDataItemForInput(input, dataItems, dataItemRole):
    for dataItem in dataItems:
        if (dataItem.ValueConverter == None or (not dataItem.Role.HasFlag(dataItemRole))):
            continue
        if (input.Feature == dataItem.ValueConverter.Location and 
            input.ParameterName == dataItem.ValueConverter.ParameterName):
                return dataItem
    print "No dataItem found for " + str(input) + "!"
    
def ConnectControlGroup(rtcModel, controlGroup):
    """Connects the inputs and outputs of the controlgroup to the inputs and outputs of the used models"""
    for input in controlGroup.Inputs:        
        dataitems = rtcModel.GetChildDataItemsFromControlledModelsForLocation(input.Feature)
        diSource = _GetDataItemForInput(input, dataitems, DataItemRole.Output)
        diTarget = rtcModel.GetDataItemByValue(input)
        diTarget.LinkTo(diSource)
    for output in controlGroup.Outputs:
        dataitems = rtcModel.GetChildDataItemsFromControlledModelsForLocation(output.Feature)
        diTarget = _GetDataItemForInput(output, dataitems, DataItemRole.Input)
        diSource = rtcModel.GetDataItemByValue(output)        
        diTarget.LinkTo(diSource)
