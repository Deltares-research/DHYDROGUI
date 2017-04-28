import clr as _clr
_clr.AddReference("RainfallRunoffModelEngine")

from Libraries.MapFunctions import CreateLineGeometry as _CreateLineGeometry
from Libraries.Conversions import ConvertToDotNetDateTime as _ConvertToDotNetDateTime
from Libraries.Conversions import CreateDateTimeList as _CreateDateTimeList
from Libraries.StandardFunctions import GetItemByName as _GetItemByName

from System import Array as _Array

from DelftTools.Hydro import Catchment, CatchmentType,WasteWaterTreatmentPlant
from DeltaShell.Plugins.DelftModels.RainfallRunoff import RainfallRunoffModel, AggregationOptions
from DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter import QuantityType, ElementSet
from DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo import MeteoDataDistributionType

class MeteoDataType:
    """Type of meteo data"""
    Precipitation = 0
    Evaporation = 1
    Temperature = 2
    
def AddLink(catchment, hydroObject):
    """Adds a link between a catchment and a hydro object"""
    region = catchment.Region
    link = region.AddNewLink(catchment, hydroObject)
    catchmentCoordinate = catchment.InteriorPoint.Coordinate
    hydroObjectCoordinate = hydroObject.Geometry.Coordinate
    link.Geometry = _CreateLineGeometry([[catchmentCoordinate.X, catchmentCoordinate.Y], [hydroObjectCoordinate.X, hydroObjectCoordinate.Y]])
    return link
    
def ChangeMeteorologicalDataDistibutionType(rrModel, meteoDataType, meteoDataDistributionType):
    meteoData = _GetMeteorologicalData(rrModel, meteoDataType)
    meteoData.DataDistributionType = meteoDataDistributionType

def SetMeteorologicalDataTimeSeries(rrModel, meteoDataType, timeSeries, catchment = None):
    meteoData = _GetMeteorologicalData(rrModel, meteoDataType)
    for timestep in timeSeries:
        if (catchment == None):
            meteoData.Data[_ConvertToDotNetDateTime(timestep[0])] = timestep[1]
        else :
            meteoData.Data[_ConvertToDotNetDateTime(timestep[0]), catchment] = timestep[1]

def _GetMeteorologicalData(rrModel, meteoDataType):
    if (meteoDataType == MeteoDataType.Precipitation):
        return rrModel.Precipitation
    elif(meteoDataType == MeteoDataType.Evaporation):
        return rrModel.Evaporation
    elif(meteoDataType == MeteoDataType.Temperature):
        return rrModel.Temperature
        
def GetTimeSeriesFromRainfallRunoffModel(rrModel, catchment, variableName):
    """Creates a list of [datetime, float] for the provided catchment and variable"""
    coverage = _GetItemByName(rrModel.OutputCoverages,variableName)
    ts = coverage.GetTimeSeries(catchment)
    return _CreateDateTimeList(ts)

def EnableOutput(rrModel, elementSet, quantityType, aggregationOption):
    parameter = rrModel.OutputSettings.GetEngineParameter(quantityType, elementSet)
    parameter.AggregationOptions = aggregationOption
