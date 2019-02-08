# coding: latin-1
import os, sys, time
from HyDAMO2FMConverter import HyDAMO2FMConverter
from HyDAMOreader import HyDAMOreader
from FMwriter import FMwriter
from Logger import Logger

p = r'D:\source\Branch-1D2D-flooding\test\DeltaShell.Plugins.FMSuite.Common.Tests\test-data\hydamo'
inputDir = 'gml'
gridFile = ''
generate2DGrid = False
oneDMeshDistance = 40.0
outputDir = 'output_FM'
dirPath = os.path.abspath(p)

if __name__ == '__main__':
    sys.stdout = Logger(os.path.join(dirPath, outputDir,'run.log'))
    reader = HyDAMOreader()
    hydamo_model = reader.readAll(dirPath, inputDir, gridFile)
    converter = HyDAMO2FMConverter()
    fm_model = converter.ConvertToFMmodel(hydamo_model, oneDMeshDistance)
    writer = FMwriter(fm_model)
    succeeded = writer.writeAll(dirPath, outputDir)
    print('*********    ' + time.strftime("%Y-%m-%d %H:%M") + '     *********\n')








