# coding: latin-1
import os, sys
from HyDAMOreader import HyDAMOreader
from FMwriter import FMwriter
from Logger import Logger

p = 'D:\\source\\nghs-1d2dflooding\\test\\DeltaShell.Plugins.FMSuite.Common.Tests\\test-data\\hydamo\\'
inputDir = 'gml'
gridFile = ''
generate2DGrid = False

outputDir = 'output_FM'
dirPath = os.path.abspath(p)

if __name__ == '__main__':
    sys.stdout = Logger(os.path.join(dirPath, outputDir,'run.log'))
    reader = HyDAMOreader()
    model = reader.readAll(dirPath, inputDir, gridFile)
    #writer = FMwriter(model)
    #gridFileAvailable = gridFile is not None and gridFile != ''
    #succeeded = writer.writeAll(dirPath, outputDir, gridFileAvailable, generate2DGrid)
    succeeded = True
    print('Succeeded = ' + str(succeeded))






