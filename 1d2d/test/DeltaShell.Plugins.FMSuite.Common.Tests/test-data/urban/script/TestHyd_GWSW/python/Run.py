# coding: latin-1
import os, sys
from GWSWreader import GWSWreader
from FMwriter import FMwriter
from Logger import Logger

p = 'D:\source\Branch-1D2D-flooding\\test\\DeltaShell.Plugins.FMSuite.Common.Tests\\test-data\\urban\\script\\TestHyd_GWSW\\'
#p = r'd:\dam_ar\dflowfm_models\urban\FMSuite.Common.Tests_urban_svn\script\TestHyd_GWSW'

inputDir = 'GWSW_DidactischStelsel'
oppervlakOnNode = False
#inputDir = 'GWSW_Waardenburg'
#oppervlakOnNode = True
#inputDir = 'GWSW_Leiden'
#oppervlakOnNode = True
#inputDir = 'GWSW_Doorlaat'
#oppervlakOnNode = True

#inputDir = 'GWSW_gemaal'
#oppervlakOnNode = True

#inputDir = 'W_C4'
#oppervlakOnNode = True

#gridFile = "waardenburg_2dC_net.nc"

gridFile = ""
generate2DGrid = False

outputDir = 'output_FM'
dirPath = os.path.abspath(p)

if __name__ == '__main__':
    sys.stdout = Logger(os.path.join(dirPath, outputDir,'run.log'))
    reader = GWSWreader()
    model = reader.readAll(dirPath, inputDir, gridFile, oppervlakOnNode)
    writer = FMwriter(model)
    gridFileAvailable = gridFile is not None and gridFile != ''
    succeeded = writer.writeAll(dirPath, outputDir, gridFileAvailable, generate2DGrid)
    print('Succeeded = ' + str(succeeded))






