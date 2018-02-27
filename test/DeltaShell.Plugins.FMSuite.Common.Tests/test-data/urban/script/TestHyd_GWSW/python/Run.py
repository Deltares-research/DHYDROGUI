import os, sys
from GWSWreader import GWSWreader
from FMwriter import FMwriter
from Logger import Logger

p = 'D:\\source\\nghs-1d2dflooding\\test\\DeltaShell.Plugins.FMSuite.Common.Tests\\test-data\\urban\\script\\TestHyd_GWSW\\'
#inputDir = 'GWSW_DidactischStelsel'
#oppervlakOnNode = False
inputDir = 'GWSW_Waardenburg'
oppervlakOnNode = True
outputDir = 'output_FM'
dirPath = os.path.abspath(p)


if __name__ == '__main__':
    sys.stdout = Logger(os.path.join(dirPath, outputDir,'run.log'))
    reader = GWSWreader()
    model = reader.readAll(dirPath, inputDir, oppervlakOnNode)
    writer = FMwriter(model)
    succeeded = writer.writeAll(dirPath, outputDir)
    print('Succeeded = ' + str(succeeded))






