from API import *
from SharpMap.Layers import *
import SharpMap.Data.Providers.ShapeFile as ShapeFile

# open SHP/DBF files
shp = ShapeFile()
shp.Open(r"c:\Users\putten_hs\Desktop\Rivierenland\CF_LateralConstantMaatgevendeAfvoer\CF_LateralConstantMaatgevendeAfvoer.shp")

# find model
model = CurrentProject.RootFolder['water flow 1d (1)']

for feature in shp.Features:
  # query name and discharge from feature attributes (DBF)
  lateralName = feature.Attributes['ID_LATCONS']
  lateralDischarge = float(feature.Attributes['AFV_M3PS'])
  
  # change a corresponding lateral data to constant discharge type
  ChangeLateralSourceType(model, lateralName, 'FlowConstant') 
  
  # change value
  lateralData = GetLateralSource(model, lateralName)
  lateralData.Flow = lateralDischarge