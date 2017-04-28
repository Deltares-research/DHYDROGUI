from datetime import datetime, time

from Libraries.RainfallRunoffModelFunctions import *
from Libraries.StandardFunctions import *
from Libraries.MapFunctions import GetShapeFileFeatures

rrModel = RainfallRunoffModel()

index = 0
for feature in GetShapeFileFeatures("D:\\Gemeenten.shp"):
    # create catchment and WasteWaterTreatmentPlant
    catchment = Catchment(Name = "Catchment" + str(index),CatchmentType = CatchmentType.Paved, Geometry = feature.Geometry)
    treatmentPlant = WasteWaterTreatmentPlant(Name = "WasteWaterTreatmentPlant" + str(index), Geometry = feature.Geometry.Centroid)
    
    # add catchment and WasteWaterTreatmentPlant to basin
    rrModel.Basin.WasteWaterTreatmentPlants.Add(treatmentPlant)
    rrModel.Basin.Catchments.Add(catchment)
    
    # link catchment to WasteWaterTreatmentPlant
    link = AddLink(catchment, treatmentPlant)
    link.Name = "Link" + str(index)
    index += 1

# set Meteorological Data
catchment = rrModel.Basin.Catchments[0]

timeSeries = [[datetime(2014, 1, 1, 15, 0, 0), 11.0],
        [datetime(2014, 1, 1, 16, 0, 0), 10.0],
        [datetime(2014, 1, 1, 17, 0, 0), 8.0],
        [datetime(2014, 1, 1, 18, 0, 0), 9.0],
        [datetime(2014, 1, 1, 19, 0, 0), 10.0]]

ChangeMeteorologicalDataDistibutionType(rrModel, MeteoDataType.Precipitation, MeteoDataDistributionType.PerFeature)

SetMeteorologicalDataTimeSeries(rrModel, MeteoDataType.Precipitation, timeSeries, catchment)
SetMeteorologicalDataTimeSeries(rrModel, MeteoDataType.Evaporation, timeSeries, catchment)
SetMeteorologicalDataTimeSeries(rrModel, MeteoDataType.Temperature, timeSeries) # set global temperature timeseries

# set cachment data for first catchment
pavedDataCatchment1 = rrModel.GetCatchmentModelData(catchment)
pavedDataCatchment1.InitialStreetStorage = 2
pavedDataCatchment1.MaximumStreetStorage = 3
pavedDataCatchment1.MaximumSewerMixedAndOrRainfallStorage = 3
pavedDataCatchment1.InitialSewerMixedAndOrRainfallStorage = 2

# enable unpaved rainfall output (set to current)
EnableOutput(rrModel, ElementSet.UnpavedElmSet, QuantityType.Rainfall, AggregationOptions.Current)

# set model start and stop time to timeseries range
SetModelTimes(rrModel, datetime(2014, 1, 1, 15, 0, 0), datetime(2014, 1, 1, 19, 0, 0), time(1,0,0))

# run the model
RunModel(rrModel)

# get discharge timeseries for catchment1
dischargeTimeSeries = GetTimeSeriesFromRainfallRunoffModel(rrModel, catchment, "Discharge (bnd)")

# export timeseries to csv file
ExportListToCsvFile("D:\\DischargeTimeSeries.csv",dischargeTimeSeries)
