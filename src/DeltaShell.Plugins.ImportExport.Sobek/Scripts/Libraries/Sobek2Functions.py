from DelftTools.Functions import TimeSeries as _TimeSeries
from DelftTools.Functions.Generic import Variable as _Variable

from DeltaShell.Sobek.Readers.Readers import HisFileReader as _HisFileReader
from DeltaShell.Plugins.ImportExport.Sobek import SobekHydroModelImporter as _SobekHydroModelImporter

def ImportSobek2Model(networkPath, useRR=True, useFLOW=True, useRTC=True, enableWaqOutput=True):
    """Imports a Sobek 2.x model from the networkPath (Network.tp).
    Use the options useRR, useFLOW and useRTC to select the models
    that need to be imported (all models will be imported as default).    
    enableWaqOutput enables generation of hyd-file for the water-quality model"""
    
    importer = _SobekHydroModelImporter()
    importer.PathSobek = networkPath

    # override standard settings for importing models (import all)
    importer.useRR = useRR
    importer.useFlow = useFLOW
    importer.useRTC = useRTC
    importer.enableWaqOutput = enableWaqOutput
    
    importer.Import()

    # return the integrated model
    return importer.TargetObject

#region His file functions
def GetVariableNamesForHisFile(hisFile):
    """Gives the names of the variables in the his file"""
    importer = _HisFileReader(hisFile)
    importer.Close()
    return importer.GetHisFileHeader.Components

def GetLocationNamesForHisFile(hisFile):
    """Gives the names of the locations in the his file"""
    importer = _HisFileReader(hisFile)
    importer.Close()
    return importer.GetHisFileHeader.Locations

def GetTimeStepsForHisFile(hisFile):
    """Gives the time steps in the his file as string"""
    importer = _HisFileReader(hisFile)
    importer.Close()
    return importer.GetHisFileHeader.TimeSteps

def GetDeltaShellTimeSeriesFromHisFile(hisFile, locName, varName):
    """Creates a timeseries for the his file data (for the provided location and variable)"""
    importer = _HisFileReader(hisFile)

    # read the data from the his file
    hisRowList = importer.ReadLocation(locName, varName)

    importer.Close()
    
    variable = _Variable[float]()
    variable.Name = str(varName) + "-HIS"
    
    series = _TimeSeries()
    series.Name = str(locName) + " " + str(varName)
    series.Components.Add(variable)

    # add all data to the series
    for row in hisRowList :
        series[row.TimeStep] = row.Value

    return series
    
def GetTimeSeriesFromHisFile(hisFile, locName, varName):
    """Creates a timeseries for the his file data (for the provided location and variable)"""
    importer = _HisFileReader(hisFile)

    # read the data from the his file
    hisRowList = importer.ReadLocation(locName, varName)

    importer.Close()

    list = []
    # add all data to the series
    for row in hisRowList :
        list.append([row.TimeStep, row.Value]) 

    return list

def GetTimeSeriesFromCSVFile(fileName, skipHeader = True, delimiterChar = ",", dateTimeFormat = "%Y/%m/%d %H:%M:%S" ):
    """Creates a time series from a csv file.
    Keyword arguments:
    fileName -- name of the csv file to be imported
    skipHeader -- indicates whether a header row should be skipped in the csv file ( default = True )
    delimiterChar -- character used to delimit different values within a row ( default = "," )
    dateTimeFormat -- string defining format of date and time ( default = "%Y/%m/%d %H:%M:%S" )
    """
    import csv
    import datetime
    list = []
    with open(fileName) as csvfile:
        lines = csv.reader(csvfile,delimiter = delimiterChar)
        for line in lines:
            if skipHeader:
                skipHeader = False
            else:
                dateLine = datetime.datetime.strptime(line[0],dateTimeFormat)
                valueLine = float(line[1])
                list.append([dateLine, valueLine])
    
    
    return list


#endregion
