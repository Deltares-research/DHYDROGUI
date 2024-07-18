# import required libraries
import os
from Libraries.StandardFunctions import *
from Libraries.MapFunctions import *
from Libraries.Conversions import ConvertToDotNetTimeSpan
from Libraries.WaterQualityModel2D3DFunctions import *
from Libraries.SpatialOperations import *
from datetime import time
import os.path
from DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model import *

""" moving observation point example
    this example assumes that there is a waq model (SF) present with an observation point (MOVING_Boat)
    the example shows how you can transform a shape file into a function by changing coordinates into waq cell indices."""

__hyd_file_path = "P:\\dflowfmgui\\WAQ\\Examples hyd-file and doc\\FM_SFBay_Small\\uni3d.hyd"
__boat_shape_file_path = "D:\\waq_moving_boat\\movement.shp"
__boat_shape_file_exists = os.path.isfile(__boat_shape_file_path)

# create a model from a hyd file
waqModel = CreateWaterQualityModelFromHydFile(__hyd_file_path)
waqModel.Name = "SF"
Application.ProjectService.Project.RootFolder.Add(waqModel)

# set model time steps for this example
waqModel.TimeStep = ConvertToDotNetTimeSpan(time(0,10))
waqModel.ModelSettings.HisTimeStep = ConvertToDotNetTimeSpan(time(0,10))

# import a process library
defaultSubstanceLibrary = GetDefaultSubstanceProcessLibraryPath(SubstanceProcessLibraryFolder.Sobek, "01_Oxygen_simple")
ImportSubstanceProcessLibrary(waqModel, defaultSubstanceLibrary)

# create observation point named MOVING_Boat, (X, Y and Z coordinates are irrelevant, since they will be superseded by the script)
boat = WaterQualityObservationPoint(Name="MOVING_Boat", X = 550126.089353351, Y = 4218996.101622113, Z = 0)
waqModel.ObservationPoints.Add(boat)


# open the map
view = OpenView(waqModel)
view.MapView.Map.CoordinateSystem = CreateCoordinateSystem(3857) # EPSG code => WGS 84 / Pseudo-Mercator

# add a background layer to the map
backgroundLayer = CreateSatelliteImageLayer()
view.MapView.Map.Layers.Add(backgroundLayer)

if __boat_shape_file_exists:
    # create a layer for the shape file and add it to the map as well
    # import a shape file with the trajectory of the boat
    # the shape file contains a polyline in the San Fransisco bay
    coordinateSystem = GetShapeFileCoordinateSystem(__boat_shape_file_path)
    features = GetShapeFileFeatures(__boat_shape_file_path)
    lineLayer = CreateLayerForFeatures("movement", features, coordinateSystem)
    lineLayer.Style.Line.Color = Color.Yellow
    view.MapView.Map.Layers.Add(lineLayer)
    view.MapView.Map.BringToFront(lineLayer)

# Set up initial conditions for CBOD5 and OXY
# 1. change initial conditions type of CBOD5 to a coverage
ChangeInputVariableType(waqModel, InputType.InitialConditions, "CBOD5", VariableType.Coverage)
# 2. set values by a polygon mask on the CBOD5 coverage
icCBOD5 = GetItemByName(waqModel.InitialConditions, "CBOD5")
polygon = CreatePolygon([[550603.989195383, 4209354.37603252], [546658.305621886, 4200714.68958711], [549583.553788444, 4194183.9029827],
           [552985.005144906, 4194456.01909121], [549311.437679927, 4201871.1830483], [553937.411524716, 4208401.96965271]])
AddSetValueOperation(waqModel, icCBOD5, polygon, 10, "Overwrite")
# 3. set the oxygen concentration globally to 5.5
SetDefaultValue(waqModel, InputType.InitialConditions, "OXY", VariableType.Constant,  5.5)

# save the project so there is an explicit working directory
Application.SaveProjectAs("D:\\waq_moving_boat\\project\\theproject.dsproj")

# make a list of time steps
timesteps = []
# make a list of cell indices that correspond to the shape file
cellIndices = []

if __boat_shape_file_exists:
    # transform the geometry to work with
    movementLine = features[0].Geometry # get that one polyline in the feature
    transformedLine = TransformGeometryByCoordinateSystems(movementLine, coordinateSystem, waqModel.CoordinateSystem)

    for coordinate in transformedLine.Coordinates:
        cellIndices.append(GetWaqCellIndex(waqModel, coordinate, boat.Z))

    refTime = waqModel.StartTime
    # add a timestep for each coordinate through time
    for i in range(0, transformedLine.Coordinates.Count):
        timesteps.append(refTime)
        refTime = refTime.Add(waqModel.TimeStep)
    
# create a file and write the delwaq time steps
dir = waqModel.ExplicitWorkingDirectory + "\\scripting\\"
if not os.path.exists(dir):
    os.makedirs(dir)

filePath = dir + "B7_moving_monitor.inc"

with open(filePath, "w") as file:
    file.write("FUNCTIONS ")
    file.write(boat.Name)
    file.write("\n")
    file.write("BLOCK DATA")
    file.write("\n")
    
    # write the time steps and the cell indices
    for i, val in enumerate(timesteps):
        file.write(val.ToString("yyyy'/'MM'/'dd-HH:mm:ss"))
        file.write(" ")
        file.write(str(cellIndices[i]))
        file.write("\n")

""" Result: a file in your project with the following information:
    FUNCTIONS MOVING_Boat
    BLOCK DATA
    1999/12/01-00:00:00 6547
    1999/12/01-00:00:10 98798
    ...
    ...                      """

# Add an extra line to the input file: INCLUDE 'scripting/B7_moving_monitor.inc'
inputFile = waqModel.InputFile.Content
endBlock7 = "#7"
inpLineMovingObservationPoint = "\r\nINCLUDE 'scripting\B7_moving_monitor.inc'  ; SCRIPTED: TRACK MOVING OBSERVATION POINT\r\n"
waqModel.InputFile.Content = inputFile.replace(endBlock7, inpLineMovingObservationPoint + "\r\n" + endBlock7)

# Change model settings to generate output at observation points
waqModel.ModelSettings.MonitoringOutputLevel = MonitoringOutputLevel.Points

# Increase RcBOD, decay rate BOD (first pool) at 20 oC
SetDefaultValue(waqModel, InputType.ProcessCoefficient, "RcBOD", VariableType.Constant,  5.0)

if __boat_shape_file_exists:
	# now run the model!
	RunModel(waqModel, True)

# Show results in moving observation point
movingObsPointOutput = GetItemByName(waqModel.ObservationVariableOutputs, "MOVING_Boat")
outputCBOD5 = GetItemByName(movingObsPointOutput.TimeSeriesList, "CBOD5")
outputOXY = GetItemByName(movingObsPointOutput.TimeSeriesList, "OXY")
chartView = OpenView(outputCBOD5)
chartView.Text = "Moving obs. point"
chartView.Data = outputOXY
chartView.ChartSeries[0].PointerVisible = True
chartView.ChartSeries[1].PointerVisible = True