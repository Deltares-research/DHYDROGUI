import os
from GWSWreader import GWSWreader
from FMwriter import FMwriter


dirPath = os.path.abspath('D:\\Documents\\D-Hydro\\Urban\\TestHyd_GWSW') + "\\"


if __name__ == '__main__':
    reader = GWSWreader()
    print(reader)
    print(reader.readAll)
    model = reader.readAll(dirPath)
    writer = FMwriter(model)
    succeeded = writer.writeAll(dirPath)
    print('Succeeded = ' + str(succeeded))

