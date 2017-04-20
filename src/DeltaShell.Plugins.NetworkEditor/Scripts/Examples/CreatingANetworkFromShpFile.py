from Libraries.MapFunctions import CreateLineGeometry, CreatePointGeometry
from Libraries.MapFunctions import GetShapeFileFeatures, GetShapeFileCoordinateSystem
from Libraries.StandardFunctions import *
from Libraries.NetworkFunctions import *

branchShapeFile = "D:\\rivers.shp"

network = HydroNetwork(Name = "test network")

number = 1
# read shape file features and use them to create branches in the network
for feature in GetShapeFileFeatures(branchShapeFile):
    # create start and end node
    firstCoordinate = feature.Geometry.Coordinates[0]
    lastCoordinate = feature.Geometry.Coordinates[feature.Geometry.Coordinates.Length -1]
    startNode = HydroNode(Name= "StartNode" + str(number) ,Geometry = CreatePointGeometry(firstCoordinate.X, firstCoordinate.Y))
    endNode = HydroNode(Name= "EndNode" + str(number), Geometry = CreatePointGeometry(lastCoordinate.X, lastCoordinate.Y))
    
    # create channel
    channel = Channel(startNode, endNode, Name = "Channel " + str(number))
    channel.Geometry = feature.Geometry
    
    # add nodes and branch to network
    network.Nodes.AddRange([startNode, endNode])
    network.Branches.Add(channel)
    number += 1

network.CoordinateSystem = GetShapeFileCoordinateSystem(branchShapeFile)

# remove all duplicate nodes (from branches where the nodes overlap)
MergeNodesWithSameGeometry(network)

# add a weir (named "Weir1") to the second branch at a chainage of 0.2 
weir = CreateBranchObjectOnBranchUsingChainage(BranchObjectType.Weir, "Weir1", network.Branches[1], 0.2)

# find a weir by the name "Weir1"
weir = GetBranchObjectByType(network, BranchObjectType.Weir, "Weir1")

AddToProject(network)
