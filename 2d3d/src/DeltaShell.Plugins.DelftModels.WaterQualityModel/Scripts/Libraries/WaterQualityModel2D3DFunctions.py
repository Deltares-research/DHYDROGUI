import os

from Libraries.StandardFunctions import GetItemByName as _GetItemByName

from DeltaShell.Plugins.DelftModels.WaterQualityModel import WaterQualityModel
from DeltaShell.Plugins.DelftModels.WaterQualityModel.IO import HydFileImporter, SubFileImporter
from DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.SubstanceProcessLibrary import SubstanceProcessLibrary
from DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils import DelwaqFileStructureHelper
from DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas import WaterQualityObservationPoint
from DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils import FunctionTypeCreatorFactory as _FunctionTypeCreatorFactory

class SubstanceProcessLibraryFolder:
    Sobek = 1
    
class OutputTimeType:
    MonitoringLocations = 1
    Cells = 2
    Balance = 3
    
class InputType:
    InitialConditions = 1
    ProcessCoefficient = 2
    Dispersion = 3
    Meteo = 4    

class VariableType:
    Constant = 1
    TimeSeries = 2
    Coverage = 3

def CreateWaterQualityModelFromHydFile(hydFilePath):
    """ Create a water quality model based on a hyd file.
        Returns None if the hyd file doesn't exist."""
    if not os.path.exists(hydFilePath):
        return None
    
    hydFileImporter = HydFileImporter()
    return hydFileImporter.ImportItem(hydFilePath)
    
def GetDefaultSubstanceProcessLibraryPath(folder, libraryName):
    """ The folder should be of type SubstanceProcessLibraryFolder, libraryName is the file name to search for in that folder.
        Returns the complete file path to the process library.
        Adds the .sub at the end of the library name"""
    folderPath = None
    if folder == SubstanceProcessLibraryFolder.Sobek:
        folderPath = DelwaqFileStructureHelper.GetDelwaqDataFolderPath() + os.sep + "Sobek"    
    if not folderPath == None:
        return folderPath + os.sep + libraryName + ".sub"

def ImportSubstanceProcessLibrary(wqModel, subFilePath):
    importer = SubFileImporter()
    importer.Import(wqModel.SubstanceProcessLibrary, subFilePath)

def GetWaqCellIndex(waqModel, coordinate, zValue):
    """Get the waq cell index for a certain coordinate and the z value for the depth"""
    return waqModel.PointToGridCellMapper.GetWaqSegmentIndex(coordinate.X, coordinate.Y, zValue)

def SetDefaultValue(wqModel, inputType, variableName, variableType, value):
    functionList = _GetFunctionList(wqModel,inputType)
    function = _GetItemByName(functionList, variableName)
    converter = _GetFunctionConverter(variableType)
    converter.SetDefaultValueForFunction(function, value)

def ChangeInputVariableType(wqModel,inputType, variableName, variableType):
    """ Change a variable into constant/coverage/time series.
        Parameters
        ----------
        inputType: InputType
            The type of the list to alter. Where is the variable present?
        variableName: string
            The variable to convert
        variableType: VariableType. 
            constant/coverage/time series"""
            
    functionList = _GetFunctionList(wqModel,inputType)
    function = _GetItemByName(functionList, variableName)
    converter = _GetFunctionConverter(variableType)
    newFunction = converter.TransformToFunctionType(function)
    functionList.Remove(function)
    functionList.Add(newFunction)

def _GetFunctionConverter(variableType):
    if (variableType == VariableType.Constant):
        return _FunctionTypeCreatorFactory.CreateConstantCreator()
    if (variableType == VariableType.TimeSeries):
        return _FunctionTypeCreatorFactory.CreateTimeseriesCreator()
    if (variableType == VariableType.Coverage):
        return _FunctionTypeCreatorFactory.CreateUnstructuredGridCoverageCreator()

def _GetFunctionList(wqModel,inputType):
    if (inputType == InputType.InitialConditions):
        return wqModel.InitialConditions
    if (inputType == InputType.ProcessCoefficient):
        return wqModel.ProcessCoefficients
    if (inputType == InputType.Dispersion):
        return wqModel.Dispersion
    if (inputType == InputType.Meteo):
        return wqModel.Meteo
