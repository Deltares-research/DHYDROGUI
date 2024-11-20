import os
from GWSWreader import GWSWreader
from FMwriter import FMwriter

p = 'D:\\source\\delta-shell\\Products\\NGHS\\test\\DeltaShell.Plugins.FMSuite.Common.Tests\\test-data\\urban\\script\\TestHyd_GWSW\\'
dirPath = os.path.abspath(p)


if __name__ == '__main__':
    reader = GWSWreader()
    print(reader)
    print(reader.readAll)
    model = reader.readAll(dirPath)
    writer = FMwriter(model)
    succeeded = writer.writeAll(dirPath)
    print('Succeeded = ' + str(succeeded))

