import Libraries.StandardFunctions as _sf
import Libraries.Conversions as _cv
import Libraries.MapFunctions as _mf

from System import Convert as _Convert
from System.Collections.Generic import List as _List

from DeltaShell.Plugins.FMSuite.FlowFM.Model import WaterFlowFMModel
from DeltaShell.Plugins.FMSuite.FlowFM.FeatureData import FlowBoundaryCondition as _FlowBoundaryCondition
from DeltaShell.Plugins.FMSuite.FlowFM.FeatureData import FlowBoundaryQuantityType
from DeltaShell.Plugins.FMSuite.Common.FeatureData import BoundaryConditionDataType
from DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition import KnownProperties

from NetTopologySuite.Extensions.Grids import UnstructuredGridFactory as _UnstructuredGridFactory
from NetTopologySuite.Extensions.Grids import Edge as _Edge
from SharpMap.Extensions.Layers import DelftDashboardTileLayer as _DelftDashboardTileLayer

from NetTopologySuite.Extensions.Features import Feature2D,Feature2DPoint
from DelftTools.Hydro.Area.Objects import ObservationCrossSection2D, LandBoundary2D
from DelftTools.Hydro.Area.Objects import ThinDam2D
from GeoAPI.Geometries import ICoordinate as _ICoordinate
from GeoAPI.Geometries import Coordinate as _Coordinate

def GenerateRegularGridForModel(fmModel, numbOfCellsHorizontal, numbOfCellsVertical, cellWidth, cellHeight, xOffset = 0, yOffset = 0, addToGrid = False):
    """Generates a regular grid (with defined number of horizontaly and verticaly cells) and adds it to the fmModel.
    Use the numbOfCellsHorizontal, numbOfCellsVertical, cellWidth and cellHeight to define the grid dimensions"""
    if (fmModel.NetFilePath == None):
        raise StandardError("Path to net file is not defined.")
    
    if (not addToGrid):
        fmModel.Grid.Clear()

    numbOfPointsHorizontal = numbOfCellsHorizontal + 1
    numbOfPointsVertical = numbOfCellsVertical + 1
    
    # compensate for existing vertices
    edgeVertexOffset = fmModel.Grid.Vertices.Count
    
    vertices = []
    edges = []
    
    for verticalIndex in range(numbOfPointsVertical):
        horizontalOffset = numbOfPointsHorizontal * verticalIndex
        for horizontalIndex in range(numbOfPointsHorizontal):
            x = xOffset + (horizontalIndex * cellWidth)
            y = yOffset + (verticalIndex * cellHeight)
            
            # add vertex
            vertices.append(_Coordinate(x,y))
            
            currentPointIndex = horizontalOffset + horizontalIndex + edgeVertexOffset
            
            # add horizontal edges
            if (horizontalIndex != 0):
                edges.append(_Edge(currentPointIndex -1, currentPointIndex))
            # add vertical edges
            if (verticalIndex != 0 ):
                edges.append(_Edge(currentPointIndex - numbOfPointsHorizontal, currentPointIndex))                

    # add vertices and edges to grid
    fmModel.Grid.Vertices.AddRange(vertices)
    fmModel.Grid.Edges.AddRange(edges)
    
    # Reinitialize model for grid
    fmModel.ReloadGrid(True,True)
    
def GenerateRegularGridForModelUsingExtend(fmModel, totalWidth, totalHeight, numbOfCellsHorizontal, numbOfCellsVertical, xOffset = 0, yOffset = 0, addToGrid = False):
    """Generates a regular grid (with defined number of horizontaly and verticaly cells) and adds it to the fmModel.
    Use the totalWidth and totalHeight to define the grid dimensions"""
    cellWidth = totalWidth/numbOfCellsHorizontal
    cellHeight = totalHeight/numbOfCellsVertical
    
    GenerateRegularGridForModel(fmModel, numbOfCellsHorizontal, numbOfCellsVertical, cellWidth, cellHeight, xOffset, yOffset, addToGrid)

def GetGebcoBathymetryData(xMin, yMin, xMax, yMax, epsg):
    """Gets the bathymetry data from the Gebco dataset for the provided extent.
    The epsg code is needed to translate the extent to the coordinate system of the dataset"""
    # convert to WGS 84 -> gebco_08 uses WGS 84
    bottomLeft = _mf.TransformGeometry(_mf.CreatePointGeometry(xMin, yMin), epsg, 4326)
    topRight = _mf.TransformGeometry(_mf.CreatePointGeometry(xMax, yMax), epsg, 4326)
    
    layer = _DelftDashboardTileLayer("gebco_08")
    bathymetryData = layer.GetCoverageForExtent(bottomLeft.X, bottomLeft.Y, topRight.X, topRight.Y, "5") #zoom level 5 (= Highest)

    return bathymetryData

def GetGebcoBathymetryValueFor(coordinate, epsg, bathymetryData):
    """Gets the bathymetry value at the provided coordinate from the bathymetryData. 
    The epsg code is needed to translate the coordinate to the coordinate system of the dataset"""
    # convert to WGS 84 -> gebco_08 uses WGS 84 (epsg 4326)
    location = _mf.TransformGeometry(_mf.CreatePointGeometry(coordinate.X, coordinate.Y), epsg, 4326).Coordinate
    return _Convert.ToDouble(bathymetryData.Evaluate(location))

def CleanupLandCells(fmModel, bathymetryData, maxValue):
    """Removes all the land cells (cells that have a bathymetry value higher than maxValue)
    from the grid."""
    def _CheckDepth(vertex, index):
        value = GetGebcoBathymetryValueFor(vertex.CoordinateValue, fmModel.Grid.CoordinateSystem.AuthorityCode, bathymetryData)
        return (value < maxValue)
        
    # create vertices and edges
    result = _GetNewGridDefinition(fmModel.Grid.Vertices, fmModel.Grid.Edges ,_CheckDepth)       
    result = _RemoveSingleVertices(result[0], result[1])

    # replace vertices and edges
    fmModel.Grid.Vertices = result[0]# newVertices
    fmModel.Grid.Edges = result[1] # newEdges

    # write to file and update
    fmModel.ReloadGrid(True,True)

def SetModelProperty(fmModel, property, valueAsString):
    """Sets the provided property to the given value."""
    fmModel.ModelDefinition.GetModelProperty(property).SetValueFromString(valueAsString)    

def CreateBoundary(name, startPointX, startPointY, endPointX, endPointY, numberOfSupportPoints = 2):
    spDeltaX = (endPointX - startPointX)/(numberOfSupportPoints -1)
    spDeltaY = (endPointY - startPointY)/(numberOfSupportPoints -1)
    
    points = []
    for i in range(numberOfSupportPoints):
        points.append([startPointX + (i * spDeltaX), startPointY + (i * spDeltaY)])
    
    return Feature2D(Name = name, Geometry = _mf.CreateLineGeometry(points))

def AddFlowBoundaryCondition(fmModel, boundaryName, flowBoundaryQuantityType, boundaryConditionDataType):
    """Create a flow boundary condition for a boundary (boundaryName) using the specified flowBoundaryQuantityType and boundaryConditionDataType"""
    boundary = _sf.GetItemByName(fmModel.Boundaries, boundaryName)
    boundarySet = _GetBoundaryConditionSetForBoundary(fmModel, boundary)
    
    flowBoundaryCondition = _FlowBoundaryCondition(flowBoundaryQuantityType, boundaryConditionDataType)
    flowBoundaryCondition.Feature = boundary
    flowBoundaryCondition.Name = str(flowBoundaryQuantityType) + " " + str(boundaryConditionDataType) + " " + boundary.Name
    
    boundarySet.BoundaryConditions.Add(flowBoundaryCondition)
    
    return flowBoundaryCondition

def AddTimeSeriesToSupportPoint(fmModel, boundaryCondition, pointIndex, timeseries):
    """Add a timeseries to the provided boundaryCondition for a support point (pointIndex)"""
    if (not boundaryCondition.DataPointIndices.Contains(pointIndex)):
        boundaryCondition.DataPointIndices.Add(pointIndex)

    pointData = boundaryCondition.GetDataAtPoint(pointIndex)
    _cv.FillTimeSeries(pointData, timeseries)

def AddQHDataToSupportPoint(fmModel, boundaryCondition, pointIndex, qhTable):
    """Add a Q-H table to the provided boundaryCondition for a support point (pointIndex)"""
    if (not boundaryCondition.DataPointIndices.Contains(pointIndex)):
        boundaryCondition.DataPointIndices.Add(pointIndex)
    
    pointData = boundaryCondition.GetDataAtPoint(pointIndex)
    
    for qhRow in qhTable:
        pointData[qhRow[0]] = qhRow[1]

def GetFlowFlexibleMeshTimeSeries(fmModel, outputName, feature):
    """Create a timeseries for the provided feature (from the output with the name outputName)"""
    coverage = _sf.GetItemByName(fmModel.OutputHisFileStore.Functions, outputName)
    timeSeries = coverage.GetTimeSeries(feature)
    return _cv.CreateDateTimeList(timeSeries)

def _GetBoundaryConditionSetForBoundary(fmModel, boundary):
    for boundaryConditionSet in fmModel.BoundaryConditionSets:
        if(boundaryConditionSet.Feature == boundary):
            return boundaryConditionSet

def _GetNewGridDefinition(vertices, edges, keepVertexFunction):
    indexLookup = {}
    newVertices = []
    for index in range(len(vertices)):
        vertex = vertices[index]
        if (keepVertexFunction(vertex, index)):
            indexLookup[index] = len(newVertices) # make lookup for the changed vertexIndices
            newVertices.append(vertex)
    
    newEdges = []
    for edge in edges :
        if (indexLookup.has_key(edge.VertexFromIndex) and indexLookup.has_key(edge.VertexToIndex)):
            newEdge = _Edge(indexLookup[edge.VertexFromIndex], indexLookup[edge.VertexToIndex])
            newEdges.append(newEdge)
            
    return newVertices, newEdges

def _GetVertexIndicesToEdgeDictionary(vertices, edges):
    vertexToEdgeDictionary = {}
    for vertexIndex in range(len(vertices)):
        vertexToEdgeDictionary[vertexIndex] = []
        
    for edge in edges:
        vertexToEdgeDictionary[edge.VertexFromIndex].append(edge)    
        vertexToEdgeDictionary[edge.VertexToIndex].append(edge)

    return vertexToEdgeDictionary

def _RemoveSingleVertices(vertices, edges):
    continueWhile = True
    count = 0
    while (count < 10 and continueWhile): # limit number of cleanup stages to 10
        edgeLookUp = _GetVertexIndicesToEdgeDictionary(vertices, edges)
        
        verticesToRemove = []
        for index in edgeLookUp.keys():
            if (len(edgeLookUp[index]) < 2):
                verticesToRemove.append(index)

        continueWhile = len(verticesToRemove) != 0
        count += 1
        
        if(continueWhile):
            print "Removing " + str(len(verticesToRemove)) + " vertices"
            
            def _KeepVertex(vertex, index):
                return index not in verticesToRemove
                
            result = _GetNewGridDefinition(vertices, edges, _KeepVertex)
            
            vertices = result[0]
            edges = result[1]

    return vertices, edges