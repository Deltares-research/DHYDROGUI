from SharpMap.Layers import *
import SharpMap.Map as Map
import SharpMap.Data.Providers.ShapeFile as ShapeFile

map1 = Map(Name="map1")

CurrentProject.RootFolder.Add(map1)
Gui.CommandHandler.OpenView(map1)

mapView = Gui.DocumentViews.ActiveView

shapeFile = ShapeFile("D:\\gis\\Europe_Lakes.shp")
vectorLayer = VectorLayer(Name = "lakes", DataSource = shapeFile)

map1.Layers.Add(vectorLayer)

mapView.Map.ZoomToExtents

indexOfNameAttribute = 2

print "Names:"
for feature in shapeFile.Features:
    print feature.Attributes.Values[indexOfNameAttribute]
 

