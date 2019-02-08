# coding: latin-1
import os, shutil
from FMmodel import FMmodel
from UgridWriter import UgridWriter


class FMwriter:
    """Writer for FM files"""

    def __init__(self, model = FMmodel()):
        self.model = model

    def writeAll(self, dirPath, outputDir):  # write all fm files from HyDAMO
        ugridWriter = UgridWriter()
        ugridWriter.write(dirPath, outputDir, self.model.networkdata, self.model.griddata)
        self.writeMdFiles(dirPath, outputDir)
        self.writeCrossSectionDefinitions(dirPath, outputDir)
        self.writeCrossSections(dirPath, outputDir)

        print("********************************************")
        print("                 THE END                    ")
        print("********************************************")
        return True

    def to2Dec(selfselft, strValue):
        result = str("%.2f" % round(float(strValue),2))
        return result;

    def writeMdFiles(self, dirPath, outputDir):
        srcMdu = os.path.join(dirPath, 'script','resources', 'FMmdu.txt')
        targetMdu = os.path.join(dirPath, outputDir, 'hydamo.mdu')

        shutil.copy2(srcMdu, targetMdu)

        return True

    def writeCrossSections(self, dirPath, outputDir):  # write cross-sections

        fileCSLoc = open(os.path.join(dirPath, outputDir, 'hydamo_cross_section_locations.ini'), 'w')
        fileRough = open(os.path.join(dirPath, outputDir, 'hydamo_roughness.ini'), 'w')

        # header location
        fileCSLoc.write('[general]\n')
        fileCSLoc.write('majorVersion = 1\n')
        fileCSLoc.write('minorVersion = 0\n')
        fileCSLoc.write('fileType = crossLoc\n')
        fileCSLoc.write('\n')

        # header roughness
        fileRough.write('[general]\n')
        fileRough.write('majorVersion = 1\n')
        fileRough.write('minorVersion = 0\n')
        fileRough.write('fileType = roughness\n')
        fileRough.write('\n')
        fileRough.write('[content]\n')

        fileRough.write('sectionId = HyDAMO\n')
        fileRough.write('flowDirection = False\n')
        fileRough.write('interpolate = 1\n')
        fileRough.write('globalType = 1\n')
        fileRough.write('globalValue = 45.0\n')
        fileRough.write('\n')

        for cs in self.model.crosssections:

            id = str(cs[0])
            branch_id = str(cs[1])
            offset = str(cs[2])

            fileCSLoc.write('[crosssection]\n')
            fileCSLoc.write('id = ' + id + '\n')
            fileCSLoc.write('branchid = ' + branch_id + '\n')
            fileCSLoc.write('chainage = ' + offset + '\n')
            fileCSLoc.write('shift = 0.0\n')
            fileCSLoc.write('definition = def_' + id + '\n')
            fileCSLoc.write('\n')

            # roughness per cross-section
            fileRough.write('[definition]\n')
            fileRough.write('branchid = ' + branch_id + '\n')
            fileRough.write('chainage = ' + offset + '\n')
            fileRough.write('value = 45.0\n')
            fileRough.write('\n')

        fileCSLoc.close()
        fileRough.close()
        return True

    def writeCrossSectionDefinitions(self, dirPath, outputDir):

        fileCSDef = open(os.path.join(dirPath, outputDir, 'hydamo_cross_section_definitions.ini'), 'w')

        # header
        fileCSDef.write('[general]\n')
        fileCSDef.write('majorVersion = 1\n')
        fileCSDef.write('minorVersion = 0\n')
        fileCSDef.write('fileType = crossDef\n')
        fileCSDef.write('\n')

        # [Definition]
        # type = yz (String)
        # yzCount  Int
        # yValues Double[]
        # zValues Double[]
        # sectionCount Int
        # roughnessNames String[]
        # roughnessPositions Double[]

        for cs in self.model.crosssections:

            id = str(cs[0])
            yValues = []
            zValues = []
            for point in cs[3]:
                yValues.append(point[0])
                zValues.append(point[1])

            fileCSDef.write('[definition]\n')
            fileCSDef.write('id = def_' + id + '\n')
            fileCSDef.write('type = yz\n')
            fileCSDef.write('yzCount = ' + str(len(yValues)) + '\n')
            fileCSDef.write('yValues = ' + " ".join(str(y) for y in yValues) + '\n')
            fileCSDef.write('zValues = ' + " ".join(str(z) for z in zValues) + '\n')
            fileCSDef.write('sectionCount = 1\n')
            fileCSDef.write('roughnessNames = HyDAMO\n')
            fileCSDef.write('roughnessPositions = 0.0 ' + str(yValues[-1]) + '\n')
            fileCSDef.write('\n')

        fileCSDef.close()
        return True