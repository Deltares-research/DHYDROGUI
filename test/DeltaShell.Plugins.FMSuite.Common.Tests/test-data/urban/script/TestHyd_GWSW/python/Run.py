import os
from GWSWreader import GWSWreader
from FMwriter import FMwriter

p = 'D:\\source\\nghs-1d2dflooding\\test\\DeltaShell.Plugins.FMSuite.Common.Tests\\test-data\\urban\\script\\TestHyd_GWSW\\'
#inputDir = 'input_GWSW'
inputDir = 'input_GWSW_Leiden'
outputDir = 'output_FM'
dirPath = os.path.abspath(p)


if __name__ == '__main__':
    reader = GWSWreader()
    print(reader)
    print(reader.readAll)
    model = reader.readAll(dirPath,inputDir)
    writer = FMwriter(model)
    succeeded = writer.writeAll(dirPath,outputDir)
    print('Succeeded = ' + str(succeeded))

