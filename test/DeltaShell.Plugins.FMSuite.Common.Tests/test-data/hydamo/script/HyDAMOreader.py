# coding: latin-1
import os

from gdal import ogr
from collections import OrderedDict
from HyDAMOmodel import HyDAMOmodel
from UgridReader import UgridReader

class HyDAMOreader:
    """Reads all HyDAMO gml files"""

    dirPath = ''
    inputDir = ''
    gridFile = ''

    def readAll(self, dirPath, inputDir, gridFile):   # path directory files
        self.dirPath = dirPath
        self.inputDir = inputDir
        model = HyDAMOmodel()
        model.network = self.readNetwork2Dict()
        #model.profiles = self.readProfiles2Dict()
        if gridFile is not None and gridFile != '':
            reader = UgridReader(model)
            filePath = os.path.join(self.dirPath, self.inputDir, gridFile)
            reader.ReadFile(filePath)
        return model

    def file2Dict(self, filePath):
        dict=OrderedDict()
        ogr.UseExceptions()
        gml = ogr.Open(filePath)
        layer = gml.GetLayer()
        layerDefinition = layer.GetLayerDefn()
        fields = []

        for i in range(layerDefinition.GetFieldCount()):
            fields.append(layerDefinition.GetFieldDefn(i).GetName())

        for feature in layer:
            fData = []
            fId = feature.GetField(fields[0])
            geometry = feature.GetGeometryRef()
            fData.append(geometry.GetPoints())

            try:
                length = geometry.Length()
            except:
                length = 0.0

            fData.append(length)
            for field in fields:
                fData.append(feature.GetField(field))
            dict[fId] = fData

        return dict

    def readNetwork2Dict(self):
        # < gml: featureMember >
        # < nhi: hydroobject >
        # < nhi: code > riv__88 < / nhi: code >
        # < nhi: naam > AaMaas < / nhi: naam >
        # < nhi: statusobjectcode > 3 < / nhi: statusobjectcode >
        # < nhi: objectbegintijd > 19800101000000 < / nhi: objectbegintijd >
        # < nhi: objecteindtijd > 20170727000000 < / nhi: objecteindtijd >
        # < nhi: hyperlink > document
        # x < / nhi: hyperlink >
        # < nhi: administratiefgebied > dommel < / nhi: administratiefgebied >
        # < nhi: ruwheidstypecode > 4 < / nhi: ruwheidstypecode >
        # < nhi: ruwheidswaardelaag > 10 < / nhi: ruwheidswaardelaag >
        # < nhi: ruwheidswaardehoog > 25 < / nhi: ruwheidswaardehoog >
        # < gml: lineStringProperty >
        # < gml: LineStringsrsName = "EPSG:28992" > < gml:coordinates > 148506.391, 410424.770000002 148530.742000002, 410412.923000001 < / gml: coordinates > < / gml: LineString >
        # < / gml: lineStringProperty >
        # < / nhi: hydroobject >
        # < / gml: featureMember >
        filePath = os.path.join(self.dirPath, self.inputDir,'hydroobject.gml')
        return self.file2Dict(filePath)

    def readProfiles2Dict(self):
        # < gml: featureMember >
        # < nhi: dwarsprofiel >
        # < nhi: code > prof_01022017 - DP1_8 < / nhi: code >
        # < nhi: statusobjectcode > 3 < / nhi: statusobjectcode >
        # < nhi: objectbegintijd > 19800101000000 < / nhi: objectbegintijd >
        # < nhi: objecteindtijd > 20170727000000 < / nhi: objecteindtijd >
        # < nhi: administratiefgebied > dommel < / nhi: administratiefgebied >
        # < nhi: codevolgnummer > 8 < / nhi: codevolgnummer >
        # < nhi: profielcode > prof_01022017 - DP1 < / nhi: profielcode >
        # < nhi: typeprofielcode > 4 < / nhi: typeprofielcode >
        # < nhi: ruwheidstypecode > 4 < / nhi: ruwheidstypecode >
        # < nhi: ruwheidswaardelaag > 10 < / nhi: ruwheidswaardelaag >
        # < nhi: ruwheidswaardehoog > 20 < / nhi: ruwheidswaardehoog >
        # < gml: pointProperty >
        # < gml: Point srsName = "EPSG:28992" > < gml:coordinates > 150988.503, 380615.680000001, 23.282 < / gml: coordinates > < / gml: Point >
        # < / gml: pointProperty >
        # < / nhi: dwarsprofiel >
        # < / gml: featureMember >
        filePath = os.path.join(self.dirPath, self.inputDir,'dwarsprofiel.gml')
        return self.file2Dict(filePath)

