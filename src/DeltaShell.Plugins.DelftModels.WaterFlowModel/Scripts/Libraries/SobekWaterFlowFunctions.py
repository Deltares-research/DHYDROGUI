from Libraries.Conversions import CreateDateTimeList as _CreateDateTimeList
from Libraries.Conversions import ConvertToDotNetDateTime as _ConvertToDotNetDateTime
from Libraries.StandardFunctions import GetItemByName as _GetItemByName

from DelftTools.Hydro import RoughnessType
from DelftTools.Hydro.Helpers import HydroNetworkHelper as _HydroNetworkHelper
from DelftTools.Hydro.Structures import BridgeFrictionType
from DelftTools.Hydro.CrossSections import CrossSectionSectionType as _CrossSectionSectionType
from DelftTools.Hydro.CrossSections import CrossSectionSection as _CrossSectionSection
from NetTopologySuite.Extensions.Coverages import NetworkLocation as _NetworkLocation

from DeltaShell.Plugins.DelftModels.WaterFlowModel import WaterFlowModel1D
from DeltaShell.Plugins.DelftModels.WaterFlowModel import InitialConditionsType as _InitialConditionsType
from DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects import WaterFlowModel1DBoundaryNodeDataType as _WaterFlowModel1DBoundaryNodeDataType 
from DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects import WaterFlowModel1DLateralDataType as _WaterFlowModel1DLateralDataType
from DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi import QuantityType, ElementSet, AggregationOptions
#region Enumerations

class InitialConditionType:
    """Type of initial condition"""
    Depth = 0
    WaterLevel = 1

class RoughnessFuntionType:
    Constant = 0
    Discharge = 1
    Waterlevel = 2

class LateralDataType:
        FlowTimeSeries = 0 # Q(t)
        FlowWaterLevelTable = 1 # Q(h)
        FlowConstant = 2 # Q

class BoundaryConditionType:
        NoBoundary = 0
        WaterLevelTimeSeries  = 1 # H(t)
        FlowTimeSeries  = 2 # Q(t)         
        FlowWaterLevelTable  = 3 # Q(h)
        FlowConstant  = 4 # Q
        WaterLevelConstant  = 5 # H

#endregion

def EnableOutput(flowModel, elementSet, quantityType, aggregationOption ):
    """Enables an output variable of the waterflow model"""
    parameter = flowModel.OutputSettings.GetEngineParameter(quantityType, elementSet)
    parameter.AggregationOptions = aggregationOption

def GetBranchByName(flowModel, branchName):
    """Gives the first branch object with the provided branch name"""
    return _GetItemByName(flowModel.Network.Branches, branchName)

def GetComputationGridLocationByName(flowModel, locationName):
    """Gives the first location with the provided location name"""
    return _GetItemByName(flowModel.NetworkDiscretization.Locations.Values, locationName)

def GetBranchIndex(flowModel, branch):
    """Gives branch index as sent to the computational core"""
    branchindex = flowModel.Network.Branches.IndexOf(branch) + 1
    return branchindex

def GetGridPointIndex(flowModel, gridpoint):
    """Gives gridpoint index as sent to the computational core"""
    gridpointindex = flowModel.NetworkDiscretization.Locations.Values.IndexOf(gridpoint) + 1
    return gridpointindex

def GetBoundaryDataByName(flowModel, boundaryName):
    """Gives the numeric value assigned to a Boundary Condition with the provided name"""
    for condition in flowModel.BoundaryConditions:
        if (condition.Feature.Name == boundaryName):
            boundaryData = condition
            return boundaryData

def GetLateralDataByName(flowModel, lateralName):
    """Gives the numeric value assigned to a Boundary Condition with the provided name"""
    for latData in flowModel.LateralSourceData:
        if (latData.Feature.Name == lateralName):
            lateralData = latData
            return lateralData

def GetTimeSeriesFromWaterFlowModel(flowModel, location, variableName):
    """Creates a list of [datetime, float] for the provided location and variable"""
    coverage = _GetItemByName(flowModel.OutputFunctions,variableName)
    ts = coverage.GetTimeSeries(location)
    return _CreateDateTimeList(ts)

def CreateComputationalGrid(flowModel, gridAtCrossSection = False, gridAtLaterals = False, gridAtStructure=True, structureDistance = 10, minimumCellLength = 0.5, gridAtFixedLength = False, fixedLength = 100):
    """Generate a computational grid for the provided flow model"""
    _HydroNetworkHelper.GenerateDiscretization(flowModel.NetworkDiscretization, True, False, minimumCellLength, gridAtStructure, 
                                              structureDistance, gridAtCrossSection, gridAtLaterals,gridAtFixedLength, fixedLength)

def SetBoundaryCondition(flowModel, boundaryName, boundaryConditionType, value):
    """Sets the boundary condition with the 'boundaryName' to the type 'boundaryConditionType'
    and adds the data in value.
    timeseries -> value = [[datetime, float], [datetime, float]]
    FlowWaterLevelTable -> value = [[float,float],[float,float]]"""
    boundaryData = GetBoundaryDataByName(flowModel, boundaryName)
    
    if (boundaryConditionType == BoundaryConditionType.NoBoundary):
        boundaryData.DataType = _WaterFlowModel1DBoundaryNodeDataType.None
    elif (boundaryConditionType == BoundaryConditionType.WaterLevelConstant):
        boundaryData.DataType = _WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant
        boundaryData.WaterLevel = value
    elif (boundaryConditionType == BoundaryConditionType.FlowConstant):
        boundaryData.DataType = _WaterFlowModel1DBoundaryNodeDataType.FlowConstant
        boundaryData.Flow = value
    elif (boundaryConditionType == BoundaryConditionType.WaterLevelTimeSeries):
        boundaryData.DataType = _WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries
        for item in value:
            boundaryData.Data[_ConvertToDotNetDateTime(item[0])] = item[1]
    elif (boundaryConditionType == BoundaryConditionType.FlowTimeSeries):
        boundaryData.DataType = _WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries
        for item in value:
            boundaryData.Data[_ConvertToDotNetDateTime(item[0])] = item[1]
    elif (boundaryConditionType == BoundaryConditionType.FlowWaterLevelTable):
        boundaryData.DataType = _WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable
        for item in value:
            boundaryData.Data[item[0]] = item[1] 

def SetLateralData(flowModel, lateralName, lateralDataType, value):
    """Sets the lateral data with the 'lateralName' to the type 'LateralDataType'
    and adds the data in value.
    timeseries -> value = [[datetime, float], [datetime, float]]
    FlowWaterLevelTable -> value = [[float,float],[float,float]]"""
    
    lateralData = GetLateralDataByName(flowModel, lateralName)
    
    if (lateralDataType == LateralDataType.FlowConstant):
        lateralData.DataType = _WaterFlowModel1DLateralDataType.FlowConstant
        lateralData.Flow = value
    elif (lateralDataType == LateralDataType.FlowTimeSeries):
        lateralData.DataType = _WaterFlowModel1DLateralDataType.FlowTimeSeries
        for item in value:
            lateralData.Data[_ConvertToDotNetDateTime(item[0])] = item[1]            
    elif (lateralDataType == LateralDataType.FlowWaterLevelTable):
        lateralData.DataType = _WaterFlowModel1DLateralDataType.FlowWaterLevelTable
        for item in value:
            lateralData.Data[item[0]] = item[1]

def AddNewRoughnessSection(flowModel, roughnessTypeName):
    newRoughnessSectionType = _CrossSectionSectionType(Name = roughnessTypeName)
    flowModel.Network.CrossSectionSectionTypes.Add(newRoughnessSectionType)
    return newRoughnessSectionType

def SetDefaultRoughness(flowModel, sectionName, roughnessType, value):
    """Sets the default type and value of the roughness section"""
    roughness = _GetItemByName(flowModel.RoughnessSections, sectionName)
    roughness.SetDefaults(roughnessType, value)

def AddCrossSectionRoughness(flowModel, crossSection, minY, maxY, roughnessType):
    section = _CrossSectionSection()
    section.MinY = minY
    section.MaxY = maxY
    section.SectionType = roughnessType
    crossSection.Definition.Sections.Add(section)

def AddRoughnessAtLocation(flowModel, sectionName, channel, chainage, RoughnessType, constantValue):
    section = _GetItemByName(flowModel.RoughnessSections, sectionName)
    functionType = section.GetRoughnessFunctionType(channel)
    section.RoughnessNetworkCoverage[_NetworkLocation(channel, chainage)] = [constantValue, int(RoughnessType)]

def SetRoughnessFunctionTypeByChannel(flowModel, sectionName, channel, roughnessFuntionType, chainages, values):
    section = _GetItemByName(flowModel.RoughnessSections, sectionName)
    section.RemoveRoughnessFunctionsForBranch(channel)
    if (roughnessFuntionType == RoughnessFuntionType.Waterlevel):
        function = section.AddHRoughnessFunctionToBranch(channel)
        for value in values:
            index = 0
            for chainage in chainages:
                function[chainage, value[0]] = value[1 + index]
                index += 1
    elif(roughnessFuntionType == RoughnessFuntionType.Discharge):
        function = section.AddQRoughnessFunctionToBranch(channel)
        for value in values:
            index = 0
            for chainage in chainages:
                function[chainage, value[0]] = value[1 + index]
                index += 1

def SetInitialConditionType(flowModel, initialConditionType):
    if (initialConditionType == InitialConditionType.Depth):
        flowModel.InitialConditionsType = _InitialConditionsType.Depth
    elif(initialConditionType == InitialConditionType.WaterLevel):
        flowModel.InitialConditionsType = _InitialConditionsType.WaterLevel

def AddInitialValueAtLocation(flowModel, channel, chainage, value):
    flowModel.InitialConditions[_NetworkLocation(channel, chainage)] = value
