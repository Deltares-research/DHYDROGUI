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
        #self.writeMdFiles(dirPath, outputDir)
        #self.writeProfiles(dirPath, outputDir)

        print("********************************************")
        print("                 THE END                    ")
        print("********************************************")
        return True

    def to2Dec(selfselft, strValue):
        result = str("%.2f" % round(float(strValue),2))
        return result;

    def writeMdFiles(self, dirPath, outputDir):
        srcMdu = os.path.join(dirPath, 'python','resources', 'FMmdu.txt')
        targetMdu = os.path.join(dirPath, outputDir, 'hydamo.mdu')
        srcMd1d = os.path.join(dirPath, 'python','resources', 'FMmd1d.txt')
        targetMd1d = os.path.join(dirPath, outputDir, 'hydamo.md1d')

        shutil.copy2(srcMdu, targetMdu)
        shutil.copy2(srcMd1d, targetMd1d)

        return True

    def writeCrossSections(self, dirPath, outputDir):  # write cross-sections

        fileCSLoc = open(os.path.join(dirPath, outputDir, 'cross_section_locations.ini'), 'w')
        fileRough = open(os.path.join(dirPath, outputDir, 'roughness_hydamo.ini'), 'w')

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

        fileRough.write('sectionId = Main\n')
        fileRough.write('flowDirection = False\n')
        fileRough.write('interpolate = 1\n')
        fileRough.write('globalType = 7\n')
        fileRough.write('globalValue = 3.000\n')
        fileRough.write('\n')

        for keyvalue in self.model.connections.items():

            value = keyvalue[1]

            # Gesloten leiding   GSL
            if str(value[3]) != 'GSL':
                continue

            # start
            fileCSLoc.write('[crosssection]\n')
            fileCSLoc.write('id = ' + str(value[0]) + '_start\n')
            fileCSLoc.write('branchid = ' + str(value[0]) + '\n')
            fileCSLoc.write('chainage = 0.00\n')
            fileCSLoc.write('shift = ' + self.to2Dec(value[5]) + '\n')
            fileCSLoc.write('definition = ' + str(value[15]) + '\n')
            fileCSLoc.write('\n')

            # end
            fileCSLoc.write('[crosssection]\n')
            fileCSLoc.write('id = ' + str(value[0]) + '_end\n')
            fileCSLoc.write('branchid = ' + str(value[0]) + '\n')
            fileCSLoc.write('chainage = ' + self.to2Dec(value[8]) + '\n')
            fileCSLoc.write('shift = ' + self.to2Dec(value[6]) + '\n')
            fileCSLoc.write('definition = ' + str(value[15]) + '\n')
            fileCSLoc.write('\n')

            # roughness
            fileRough.write('[definition]\n')
            fileRough.write('branchid = ' + str(value[0]) + '\n')
            fileRough.write('chainage = 0.00\n')
            fileRough.write('value = 0.003\n')
            fileRough.write('\n')

        fileCSLoc.close()
        fileRough.close()
        return True

    def writeCrossSectionDefinitions(self, dirPath, outputDir):

        fileCSDef = open(os.path.join(dirPath, outputDir, 'cross_section_definitions.ini'), 'w')

        # header
        fileCSDef.write('[general]\n')
        fileCSDef.write('majorVersion = 1\n')
        fileCSDef.write('minorVersion = 0\n')
        fileCSDef.write('fileType = crossDef\n')
        fileCSDef.write('\n')

        for keyvalue in self.model.profiles.items():

            value = keyvalue[1]

            fileCSDef.write('[definition]\n')
            fileCSDef.write('id = ' + str(value[0]) + '\n')

            w = float(value[3]) / 1000.0  # m -> mm
            t = str(value[2])
            if t == 'RND':
                fileCSDef.write('type = circle\n')
                if str(value[0]) == 'PVC':
                    w -= ((2 * w) / 34.0)
                fileCSDef.write('diameter = ' + self.to2Dec(w) + '\n')
            elif t == 'EIV':
                fileCSDef.write('type = egg\n')
                fileCSDef.write('diameter = ' + self.to2Dec(w) + '\n')
                # fileCSDef.write('width = ' + self.to2Dec(w) + '\n') not supported by kernel
                # fileCSDef.write('height = ' + self.to2Dec(w * 1.5) + '\n') #redundant ??
            else:
                h = float(value[4]) / 1000.0  # m -> mm
                fileCSDef.write('type = rectangle\n')
                fileCSDef.write('width = ' + self.to2Dec(w) + '\n')
                fileCSDef.write('height = ' + self.to2Dec(h) + '\n')

            fileCSDef.write('closed = 1\n')
            fileCSDef.write('groundlayerUsed = 0\n')
            fileCSDef.write('roughnessNames = Main\n')
            # fileCSDef.write('roughnessNames = sewer_system\n') not supported yet by kernel
            fileCSDef.write('\n')

        fileCSDef.close()
        return True

    def writePliFile(self, dirPath, outputDir,  name, xyz, fileName = None):
        if fileName is None:
            fileName = name
        plizFile = open(os.path.join(dirPath, outputDir, fileName + '.pli'), 'w')
        plizFile.write(name + '\n')
        plizFile.write(str(len(xyz)) + '    2\n')
        for p in xyz:
            x = p[0]
            y = p[1]
            plizFile.write(self.to2Dec(x) + '    ' + self.to2Dec(y) + '\n')
        plizFile.close()
        return True

    def writePolFile(self, dirPath, outputDir,  name, xyz, fileName = None):
        if fileName is None:
            fileName = name
        plizFile = open(os.path.join(dirPath, outputDir, fileName + '.pol'), 'w')
        plizFile.write(name + '\n')
        plizFile.write(str(len(xyz)) + '    2\n')
        for p in xyz:
            x = p[0]
            y = p[1]
            plizFile.write(self.to2Dec(x) + '    ' + self.to2Dec(y) + '\n')
        plizFile.close()
        return True

    def getGeometryBoundary(self, nodeId, isInlet = True):
        xyz = []
        node = self.model.nodes[nodeId]
        z = 0.0;
        x = float(node[3])
        y = float(node[4])
        if isInlet:
            xyz.append([x - 0.5, y, z])
            xyz.append([x, y - 0.5, z])
            xyz.append([x + 0.5, y, z])
            xyz.append([x, y + 0.5, z])
            xyz.append([x - 0.5, y, z])
        else:

            for keyvalue in self.model.connections.items():
                value = keyvalue[1]
                if(value[1] == nodeId):
                    fromNodeId = value[2]
                    break
                if(value[2] == nodeId):
                    fromNodeId = value[1]
                    break

            fromNode = self.model.nodes[fromNodeId]
            xFrom = float(fromNode[3])
            yFrom = float(fromNode[4])
            if(x - xFrom > 0.0):
                if(y - yFrom > 0.0):
                    xyz.append([x , y + 1.0, z])
                    xyz.append([x + 1.0, y + 1.0, z])
                    xyz.append([x + 1.0, y , z])
                else:
                    xyz.append([x , y - 1.0, z])
                    xyz.append([x + 1.0, y - 1.0, z])
                    xyz.append([x + 1.0, y , z])
            else:
                if(y - yFrom > 0.0):
                    xyz.append([x , y + 1.0, z])
                    xyz.append([x - 1.0, y + 1.0, z])
                    xyz.append([x - 1.0, y , z])
                else:
                    xyz.append([x , y - 1.0, z])
                    xyz.append([x - 1.0, y - 1.0, z])
                    xyz.append([x - 1.0, y , z])

        return xyz


