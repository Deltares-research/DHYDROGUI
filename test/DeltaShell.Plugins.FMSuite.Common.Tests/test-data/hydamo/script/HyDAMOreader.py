# coding: latin-1
import os
import ogr as osgeo
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
        
        return dict

    def readNetwork2Dict(self):
        filePath = os.path.join(self.dirPath, self.inputDir,'hydroobject.gml')
        return self.file2Dict(filePath)

    def readProfiles2Dict(self):
        filePath = os.path.join(self.dirPath, self.inputDir,'dwarsprofiel.csv')
        return self.file2Dict(filePath)

