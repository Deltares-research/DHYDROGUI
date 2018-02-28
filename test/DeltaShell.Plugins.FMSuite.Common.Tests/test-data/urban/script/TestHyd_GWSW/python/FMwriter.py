# coding: latin-1
import os, shutil
from math import pi
from UgridWriter import UgridWriter


class FMwriter:
    """Writer for FM files"""

    deltaGateHeightTopLevel = 0.10

    def __init__(self, model):
        self.model = model

    def writeAll(self, dirPath, outputDir):  # write all fm files from GWSW model
        self.rewriteGeometryOfManholeCompartments()

        ugridWriter = UgridWriter(self.model)
        ugridWriter.write(dirPath, outputDir)
        self.writeMdFiles(dirPath, outputDir)
        self.writeRetentions(dirPath, outputDir)
        self.writePipes(dirPath, outputDir)
        self.writeProfiles(dirPath, outputDir)
        self.writeStructures(dirPath, outputDir)
        #self.writeBoundaries(dirPath, outputDir)
        #self.writeLaterals(dirPath, outputDir)
        self.writeExternalForcingFiles(dirPath, outputDir)
        self.writeXYZStreetlevel(dirPath, outputDir)

        print("********************************************")
        print("                 THE END                    ")
        print("********************************************")
        return True

    def to2Dec(selfselft, strValue):
        result = str("%.2f" % round(float(strValue),2))
        return result;

    def rewriteGeometryOfManholeCompartments(self): # on the supernode (manhole with compartments) we rewrite the geometry. Each compartment is set one meter to the east. (structure polygon from south to north)
        manholes = {} #key manholeId: array nodeIds

        for keyvalue in self.model.nodes.items():

            value = keyvalue[1]
            id = value[0]
            manholeId = value[2]

            if str(value[14]) == 'UIT': # is a boundary
                continue

            if manholeId in manholes:
                #set coordinate 1 meter to east
                lastId = manholes[manholeId][-1]
                xLastId = self.model.nodes[lastId][3]
                yLastId = self.model.nodes[lastId][4]
                xNext = float(xLastId) + 1.0 # 1 meter to the east
                self.model.nodes[id][3] = self.to2Dec(xNext)
                self.model.nodes[id][4] = self.to2Dec(yLastId) #same y values
                #add to list
                manholes[manholeId].append(id)
            else:
                manholes[manholeId] = [id]
        return True

    def writeMdFiles(self, dirPath, outputDir):
        srcMdu = os.path.join(dirPath, 'python','resources', 'FMmdu.txt')
        targetMdu = os.path.join(dirPath, outputDir, 'sewer_system.mdu')
        srcMd1d = os.path.join(dirPath, 'python','resources', 'FMmd1d.txt')
        targetMd1d = os.path.join(dirPath, outputDir, 'sewer_system.md1d')

        shutil.copy2(srcMdu, targetMdu)
        shutil.copy2(srcMd1d, targetMd1d)

        return True

    def writeBoundaries(self, dirPath, outputDir):  # write boundaries ini and bc

        #UNI_IDE  Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #RST_IDE  Identificatie (naam, nummer, code) van het rioolstelsel
        #PUT_IDE  Identificatie (naam, nummer, code) van de put of het bouwwerk
        #KNP_XCO  X-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #KNP_YCO  Y-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #CMP_IDE  Identificatie (naam, nummer, code) van het compartiment
        #MVD_NIV  Niveau maaiveld t.o.v. NAP
        #MVD_SCH  Type maaiveldschematisering
        #WOS_OPP  Oppervlak water op straat
        #KNP_MAT  Materiaal put
        #KNP_VRM  Vorm put
        #KNP_BOK  Niveau binnenonderkant put t.o.v. NAP
        #KNP_BRE  Breedte/diameter putbodem
        #KNP_LEN  Lengte putbodem
        #KNP_TYP  Type knooppunt
        #INZ_TYP  Type afvalwater dat wordt ingezameld
        #INI_NIV  Initiële waterstand t.o.v. NAP
        #STA_OBJ  Status van het object
        #AAN_MVD  Aanname maaiveldhoogte
        #ITO_IDE  Definitie infiltratiekarakteristieken. Koppeling tussen ItObject.csv en Verbinding.csv of Knooppunt.csv
        #ALG_TOE  Toelichting bij deze regel

        defaultWaterlevelOut = '-100.00' # low value needed
        defaultDischargeIn = '0.01' # m3/s from street

        filePathIni = os.path.join(dirPath, outputDir, 'boundary_locations.ini')
        filePathBc = os.path.join(dirPath, outputDir, 'boundary_conditions.bc')

        fileLocs = open(filePathIni, 'w')
        fileBc = open(filePathBc, 'w')

        #header Ini
        fileLocs.write('[general]\n')
        fileLocs.write('majorVersion = 1\n')
        fileLocs.write('minorVersion = 0\n')
        fileLocs.write('fileType = boundLocs\n')
        fileLocs.write('\n')

        #header Bc
        fileBc.write('[boundary]\n')
        fileBc.write('majorVersion = 1\n')
        fileBc.write('minorVersion = 0\n')
        fileBc.write('fileType = boundConds\n')
        fileBc.write('\n')

        for keyvalue in self.model.nodes.items():

            value = keyvalue[1]

            if str(value[14]) != 'UIT': # is not a boundary
                continue

            #location
            fileLocs.write('[boundary]\n')
            fileLocs.write('nodeId = ' + str(value[0]) + '\n')
            fileLocs.write('type = 1\n')
            fileLocs.write('\n')

            #condition
            fileBc.write('[boundary]\n')
            fileBc.write('name = ' + str(value[0]) + '\n')
            fileBc.write('function = constant\n')
            fileBc.write('time-interpolation = linear-extrapolate\n')
            fileBc.write('quantity = water_level\n')
            fileBc.write('unit = m\n')
            fileBc.write(defaultWaterlevelOut + '\n')
            fileBc.write('\n')

        for keyvalue in self.model.inlets.items():
            value = keyvalue[1]
            #laterals
            fileBc.write('[lateraldischarge]\n')
            fileBc.write('name = lateral' + str(value[0]) + '\n')
            fileBc.write('function = constant\n')
            fileBc.write('time-interpolation = linear-extrapolate\n')
            fileBc.write('quantity = water_discharge\n')
            fileBc.write('unit = m3/s\n')
            fileBc.write(defaultDischargeIn + ' #surface of ' + str(value[1]) + ' m2\n')
            fileBc.write('\n')

        fileLocs.close()
        fileBc.close()

        return True

    def writeExternalForcingFiles(self, dirPath, outputDir):  # write boundaries and laterals as external force files
        #UNI_IDE  Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #RST_IDE  Identificatie (naam, nummer, code) van het rioolstelsel
        #PUT_IDE  Identificatie (naam, nummer, code) van de put of het bouwwerk
        #KNP_XCO  X-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #KNP_YCO  Y-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #CMP_IDE  Identificatie (naam, nummer, code) van het compartiment
        #MVD_NIV  Niveau maaiveld t.o.v. NAP
        #MVD_SCH  Type maaiveldschematisering
        #WOS_OPP  Oppervlak water op straat
        #KNP_MAT  Materiaal put
        #KNP_VRM  Vorm put
        #KNP_BOK  Niveau binnenonderkant put t.o.v. NAP
        #KNP_BRE  Breedte/diameter putbodem
        #KNP_LEN  Lengte putbodem
        #KNP_TYP  Type knooppunt
        #INZ_TYP  Type afvalwater dat wordt ingezameld
        #INI_NIV  Initiële waterstand t.o.v. NAP
        #STA_OBJ  Status van het object
        #AAN_MVD  Aanname maaiveldhoogte
        #ITO_IDE  Definitie infiltratiekarakteristieken. Koppeling tussen ItObject.csv en Verbinding.csv of Knooppunt.csv
        #ALG_TOE  Toelichting bij deze regel

        inflowBC = 'sewer_system_inflow.bc'
        outflowBC = 'sewer_system_outflow.bc'
        filePathInflow = os.path.join(dirPath, outputDir, inflowBC)
        filePathOutflow = os.path.join(dirPath, outputDir, outflowBC)
        filePathEF = os.path.join(dirPath, outputDir, 'ext_force_file.ext')

        fileExternalForce = open(filePathEF, 'w')
        fileBcIn = open(filePathInflow, 'w')
        fileBcOut = open(filePathOutflow, 'w')

        for keyvalue in self.model.nodes.items():

            value = keyvalue[1]

            if str(value[14]) != 'UIT': # is not an out boundary
                continue

            name = str(value[0])
            bndName = 'bnd_' + name

            #relation
            fileExternalForce.write('[boundary]\n')
            fileExternalForce.write('quantity=outflowbnd\n')
            fileExternalForce.write('nodeId = ' + str(value[0]) + '\n')
            fileExternalForce.write('locationfile=' + bndName + '.pli\n')
            fileExternalForce.write('forcingfile=' + outflowBC + '\n')
            fileExternalForce.write('\n')

            #location
            xyz = self.getGeometryBoundary(name,False)
            self.writePliFile(dirPath,outputDir,bndName,xyz)

            #boundary data
            l = self.getBcOutBlock(bndName)
            fileBcOut.write(l)

        for keyvalue in self.model.inlets.items():
            value = keyvalue[1]
            name = str(value[0])
            bndName = 'bnd_' + name

            #relation
            fileExternalForce.write('[lateraldischarge]\n')
            fileExternalForce.write('nodeId = ' + str(value[0]) + '\n')
            fileExternalForce.write('type = lateraldischarge1d\n')
            fileExternalForce.write('locationfile=' + bndName + '.pol\n')
            fileExternalForce.write('forcingfile=' + inflowBC + '\n')
            fileExternalForce.write('\n')

            #location
            xyz = self.getGeometryBoundary(name,True)
            self.writePolFile(dirPath,outputDir,bndName,xyz)

            #boundary data
            l = self.getBcInBlock(bndName)
            fileBcIn.write(l)

        fileExternalForce.close()
        fileBcIn.close()
        fileBcOut.close()

        return True

    def getBcInBlock(self, name):
        result = ''
        result += '[forcing]\n'
        result += 'Name                            = ' + name + '_0001\n'
        result += 'Function                        = timeseries\n'
        result += 'Time-interpolation              = linear\n'
        result += 'Quantity                        = time\n'
        result += 'Unit                            = days since 1900-01-01 00:00:00\n'
        result += 'Quantity                        = dischargebnd\n'
        result += 'Unit                            = m³/s\n'
        result += '0        0.01\n'
        result += '73050    0.01\n\n'
        return result

    def getBcOutBlock(self, name):
        result = ''
        result += '[forcing]\n'
        result += 'Name                            = ' + name + '_0001\n'
        result += 'Function                        = timeseries\n'
        result += 'Time-interpolation              = linear\n'
        result += 'Quantity                        = time\n'
        result += 'Unit                            = days since 1900-01-01 00:00:00\n'
        result += 'Quantity                        = outflowbnd\n'
        result += 'Unit                            = m\n'
        result += '0        -100.0\n'
        result += '73050    -100.0\n\n'
        return result

    def  writeXYZStreetlevel(self, dirPath, outputDir):  # write streetlevel as a xyz file

        #UNI_IDE  Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #RST_IDE  Identificatie (naam, nummer, code) van het rioolstelsel
        #PUT_IDE  Identificatie (naam, nummer, code) van de put of het bouwwerk
        #KNP_XCO  X-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #KNP_YCO  Y-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #CMP_IDE  Identificatie (naam, nummer, code) van het compartiment
        #MVD_NIV  Niveau maaiveld t.o.v. NAP
        #MVD_SCH  Type maaiveldschematisering
        #WOS_OPP  Oppervlak water op straat
        #KNP_MAT  Materiaal put
        #KNP_VRM  Vorm put
        #KNP_BOK  Niveau binnenonderkant put t.o.v. NAP
        #KNP_BRE  Breedte/diameter putbodem
        #KNP_LEN  Lengte putbodem
        #KNP_TYP  Type knooppunt
        #INZ_TYP  Type afvalwater dat wordt ingezameld
        #INI_NIV  Initiële waterstand t.o.v. NAP
        #STA_OBJ  Status van het object
        #AAN_MVD  Aanname maaiveldhoogte
        #ITO_IDE  Definitie infiltratiekarakteristieken. Koppeling tussen ItObject.csv en Verbinding.csv of Knooppunt.csv
        #ALG_TOE  Toelichting bij deze regel

        filePath = os.path.join(dirPath, outputDir, 'street_level.xyz')
        file = open(filePath, 'w')
        tab = '     '

        for keyvalue in self.model.nodes.items():

            value = keyvalue[1]
            x = self.to2Dec(value[3])
            y = self.to2Dec(value[4])
            try:
                z = self.to2Dec(value[6])
                file.write(x + tab + y + tab + z)
                file.write('\n')
            except:
                print("No streetlevel available of node " + str(keyvalue[0]))


        file.close()
        return True

    def writeRetentions(self, dirPath, outputDir):  # write all manholes from GWSW model

        #UNI_IDE  Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #RST_IDE  Identificatie (naam, nummer, code) van het rioolstelsel
        #PUT_IDE  Identificatie (naam, nummer, code) van de put of het bouwwerk
        #KNP_XCO  X-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #KNP_YCO  Y-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #CMP_IDE  Identificatie (naam, nummer, code) van het compartiment
        #MVD_NIV  Niveau maaiveld t.o.v. NAP
        #MVD_SCH  Type maaiveldschematisering
        #WOS_OPP  Oppervlak water op straat
        #KNP_MAT  Materiaal put
        #KNP_VRM  Vorm put
        #KNP_BOK  Niveau binnenonderkant put t.o.v. NAP
        #KNP_BRE  Breedte/diameter putbodem
        #KNP_LEN  Lengte putbodem
        #KNP_TYP  Type knooppunt
        #INZ_TYP  Type afvalwater dat wordt ingezameld
        #INI_NIV  Initiële waterstand t.o.v. NAP
        #STA_OBJ  Status van het object
        #AAN_MVD  Aanname maaiveldhoogte
        #ITO_IDE  Definitie infiltratiekarakteristieken. Koppeling tussen ItObject.csv en Verbinding.csv of Knooppunt.csv
        #ALG_TOE  Toelichting bij deze regel

        defaultStreetArea = 100.0
        #filePath = os.path.join(dirPath, outputDir, 'Retention.ini')
        filePath = os.path.join(dirPath, outputDir, 'node.ini')
        file = open(filePath, 'w')

        #header
        file.write('[general]\n')
        file.write('majorVersion = 1\n')
        file.write('minorVersion = 0\n')
        file.write('\n')

        for keyvalue in self.model.nodes.items():

            value = keyvalue[1]

            if str(value[14]) == 'UIT': # is a boundary
                continue

            id = str(value[0])

            # [Retention]
            # id=''
            # name=''
            # nodeId='' #new is ugrid id
            # manholeId=' #new is manhole parent
            # storageType=Closed
            # useTable=0
            # bedLevel=0.0
            # area=0.0
            # streetLevel=0.0
            # file.write('[retention]\n')
            file.write('[node]\n')
            file.write('id = ' + id + '\n')
            file.write('name = ' + str(value[2]) + '\n')
            file.write('nodeId = ' + id + '\n')
            file.write('manholeId = ' + str(value[2]) + '\n')
            file.write('useTable = 0\n')
            file.write('bedLevel = ' + str(value[11]) + '\n')

            area = 0.0

            try:
                br = (float(value[12]) / 100)
            except ValueError:
                print("Missing data for area " + id)
                br = 0.0

            if str(value[11]) == 'RND':
                area = pi * br**2
            else:
                try:
                    l = (float(value[13]) / 100)
                except ValueError:
                    print("Missing data for area " + id + " (width will be used as length)")
                    l = br

                area = br * l

            areaStr  = self.to2Dec(area)

            try:
                streetLevel = float(value[6])
            except ValueError:
                print("Missing data for street level " + id)
                streetLevel = 0.0

            file.write('area = ' + areaStr + '\n')
            file.write('streetLevel = ' + self.to2Dec(streetLevel) + '\n')
            file.write('storageType = Reservoir\n')
            street_Area = defaultStreetArea
            if str(value[8]) != '' and float(value[8]) > 0.0:
                street_Area = float(value[8])
            file.write('streetStorageArea = ' + self.to2Dec(street_Area) + '\n')
            file.write('\n')
        file.close()
        return True

    def writePipes(self, dirPath, outputDir):  # write pipes described as crossections from GWSW model

        #UNI_IDE  Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #KN1_IDE  Identificatie knooppunt 1. Verwijzing naar UNI_IDE in Knooppunt.csv. Als het type verbinding een overstortdrempel of doorlaat is (Verbinding/VRB_TYP=DRP, DRL) dan moet het type knooppunt een compartiment zijn (Knooppunt/KNP_TYP=CMP).
        #KN2_IDE  Identificatie knooppunt 2. Verwijzing naar UNI_IDE in Knooppunt.csv. Als het type verbinding een overstortdrempel of doorlaat is (Verbinding/VRB_TYP=DRP, DRL) dan moet het type knooppunt een compartiment zijn (Knooppunt/KNP_TYP=CMP).
        #VRB_TYP  Type verbinding
        #LEI_IDE
        #BOB_KN1  Binnenonderkant buis knooppunt 1 t.o.v. NAP
        #BOB_KN2  Binnenonderkant buis knooppunt 2 t.o.v. NAP
        #STR_RCH  Mogelijke stromingsrichting door verbinding
        #VRB_LEN  Lengte van de leiding of de lengte die via het kunstwerk overbrugd wordt (bijvoorbeeld de lengte van de persleiding tussen pomp en lozingspunt)
        #INZ_TYP  Type afvalwater dat wordt ingezameld
        #INV_KN1  Instroomverliescoëfficient knooppunt 1
        #UTV_KN1  Uitstroomverliescoëfficient knooppunt 1
        #INV_KN2  Instroomverliescoëfficient knooppunt 2
        #UTV_KN2  Uitstroomverliescoëfficient knooppunt 2
        #ITO_IDE  Definitie infiltratiekarakteristieken. Koppeling tussen ItObject.csv en Verbinding.csv of Knooppunt.csv
        #PRO_IDE  Profieldefinitie. Koppeling tussen Profiel.csv en Verbinding.csv
        #STA_OBJ  Status van het object
        #AAN_BB1  Aanname waarde BOB_KN1
        #AAN_BB2  Aanname waarde BOB_KN2
        #INI_NIV  Initiële waterstand t.o.v. NAP
        #ALG_TOE  Toelichting bij deze regel

        fileCSLoc = open(os.path.join(dirPath, outputDir, 'cross_section_locations.ini'), 'w')
        fileRough = open(os.path.join(dirPath, outputDir, 'roughness_sewer_system.ini'), 'w')

        #header location
        fileCSLoc.write('[general]\n')
        fileCSLoc.write('majorVersion = 1\n')
        fileCSLoc.write('minorVersion = 0\n')
        fileCSLoc.write('fileType = crossLoc\n')
        fileCSLoc.write('\n')

        #header roughness
        fileRough.write('[general]\n')
        fileRough.write('majorVersion = 1\n')
        fileRough.write('minorVersion = 0\n')
        fileRough.write('fileType = roughness\n')
        fileRough.write('\n')
        fileRough.write('[content]\n')
        fileRough.write('sectionId = sewer_system\n')
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

            #start
            fileCSLoc.write('[crosssection]\n')
            fileCSLoc.write('id = ' + str(value[0]) + '_start\n')
            fileCSLoc.write('branchid = ' + str(value[0]) + '\n')
            fileCSLoc.write('chainage = 0.00\n')
            fileCSLoc.write('shift = ' + self.to2Dec(value[5]) + '\n')
            fileCSLoc.write('definition = ' + str(value[15]) + '\n')
            fileCSLoc.write('\n')

            #end
            fileCSLoc.write('[crosssection]\n')
            fileCSLoc.write('id = ' + str(value[0]) + '_end\n')
            fileCSLoc.write('branchid = ' + str(value[0]) + '\n')
            fileCSLoc.write('chainage = ' + self.to2Dec(value[8]) + '\n')
            fileCSLoc.write('shift = ' + self.to2Dec(value[6]) + '\n')
            fileCSLoc.write('definition = ' + str(value[15]) + '\n')
            fileCSLoc.write('\n')

            #roughness
            fileRough.write('[definition]\n')
            fileRough.write('branchid = ' + str(value[0]) + '\n')
            fileRough.write('chainage = 0.00\n')
            fileRough.write('value = 3.000\n')
            fileRough.write('\n')

        fileCSLoc.close()
        fileRough.close()
        return True

    def writeProfiles(self, dirPath, outputDir):  # write all profiles from GWSW model

        #PRO_IDE  Profieldefinitie. Koppeling tussen Profiel.csv en Verbinding.csv
        #PRO_MAT  Materiaal profiel
        #PRO_VRM  Vorm profiel
        #PRO_BRE  Breedte/diameter profiel
        #PRO_HGT  Hoogte profiel
        #OPL_HL1  Co-tangens helling 1
        #OPL_HL2  Co-tangens helling 2
        #PRO_NIV  Niveau boven b.o.b. Als er meerdere profielwaardes per niveau gelden, dan meerdere Profiel-regels opnemen met gelijke waarde van PRO_IDE. De waarde van PRO_NIV mag daarbij dus niet gelijk zijn
        #PRO_NOP  Nat oppervlak bij niveau
        #PRO_NOM  Natte omtrek bij niveau
        #PRO_BRE  Breedte bij niveau
        #AAN_PBR  Aanname profielbreedte
        #ALG_TOE  Toelichting bij deze regel

        fileCSDef = open(os.path.join(dirPath, outputDir, 'cross_section_definitions.ini'), 'w')

        #header
        fileCSDef.write('[general]\n')
        fileCSDef.write('majorVersion = 1\n')
        fileCSDef.write('minorVersion = 0\n')
        fileCSDef.write('fileType = crossDef\n')
        fileCSDef.write('\n')

        for keyvalue in self.model.profiles.items():

            value = keyvalue[1]

            fileCSDef.write('[definition]\n')
            fileCSDef.write('id = ' + str(value[0]) + '\n')

            w = float(value[3])/100.0 #mm -> m
            t = str(value[2])
            if t == 'RND':
                fileCSDef.write('type = circle\n')
                if str(value[0]) == 'PVC':
                    w -= ((2*w)/34.0)
                fileCSDef.write('diameter = ' + self.to2Dec(w) + '\n')
            elif t == 'EIV':
                fileCSDef.write('type = egg\n')
                fileCSDef.write('width = ' + self.to2Dec(w) + '\n')
                fileCSDef.write('height = ' + self.to2Dec(w * 1.5) + '\n') #redundant ??
            else:
                h = float(value[4])/100.0 #mm -> m
                fileCSDef.write('type = rectangle\n')
                fileCSDef.write('width = ' + self.to2Dec(w) + '\n')
                fileCSDef.write('height = ' + self.to2Dec(h) + '\n')

            fileCSDef.write('closed = 1\n')
            fileCSDef.write('groundlayerUsed = 0\n')
            fileCSDef.write('roughnessNames = sewer_system\n')
            fileCSDef.write('\n')

        fileCSDef.close()
        return True

    def writeStructures(self, dirPath, outputDir):

        #UNI_IDE  Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #KWK_TYP  Type hydraulisch component in het kunstwerk
        #BWS_NIV  Buitenwaterstand t.o.v. NAP
        #PRO_BOK  Niveau binnenonderkant profiel t.o.v. NAP
        #DRL_COE  Contractiecoëfficient doorlaatprofiel
        #DRL_CAP  Maximale capaciteit doorlaat
        #OVS_BRE  Breedte overstortdrempel / crest length
        #OVS_NIV  Niveau overstortdrempel t.o.v. NAP / crest height
        #OVS_COE  Afvoercoëfficient overstortdrempel
        #PMP_CAP  Capaciteit van de individuele pomp
        #PMP_AN1  Aanslagniveau benedenstrooms (zuigzijde) pomp t.o.v. NAP
        #PMP_AF1  Afslagniveau benedenstrooms (zuigzijde) pomp t.o.v. NAP
        #PMP_AN2  Aanslagniveau bovenstrooms (perszijde) pomp t.o.v. NAP
        #PMP_AF2  Afslagniveau benedenstrooms (perszijde) pomp t.o.v. NAP
        #QDH_NIV  Niveauverschil bij debiet-verhangrelatie
        #QDH_DEB  Debietverschil bij debiet-verhangrelatie
        #AAN_OVN  Aanname waarde OVS_NIV
        #AAN_OVB  Aanname waarde OVS_BRE
        #AAN_CAP  Aanname waarde PMP_CAP
        #AAN_ANS  Aanname waarde PMP_ANS
        #AAN_AFS  Aanname waarde PMP_AFS
        #ALG_TOE  Toelichting bij deze regel

        fileStructures = open(os.path.join(dirPath, outputDir, 'structures.ini'), 'w')

        #header
        fileStructures.write('[general]\n')
        fileStructures.write('majorVersion = 1\n')
        fileStructures.write('minorVersion = 0\n')
        fileStructures.write('fileType = structure\n')
        fileStructures.write('\n')

        for keyvalue in self.model.structures.items():

            value = keyvalue[1]
            type = value[1]
            writePliz = False
            name = value[0]
            pliName = 'str_' + name
            branchId = name
            branchOffset = '0.5'

            if type == 'DRL':
                writePliz = True
                fileStructures.write('[structure]\n')
                fileStructures.write('type = orifice\n')
                fileStructures.write('id = ' + value[0] + '\n')
                fileStructures.write('branch_id = ' + branchId + '\n')
                fileStructures.write('chainage = ' + branchOffset + '\n')
                fileStructures.write('polylinefile = ' + pliName + '.pli\n')

                direction = self.getDirectionOfStructure(value[0])
                if direction == '1_2':
                    fileStructures.write('allowed_flow_dir = 2\n')
                elif direction == '2_1':
                    fileStructures.write('allowed_flow_dir = 3\n')
                else:
                    fileStructures.write('allowed_flow_dir = 1\n')

                fileStructures.write('bottom_level = ' + self.to2Dec(value[3]) + '\n')

                area = self.getAreaOfOrifice(value[0])

                fileStructures.write('area = ' + self.to2Dec(area) + '\n')
                fileStructures.write('lat_contr_coeff = ' + self.to2Dec(value[4]) + '\n')
                fileStructures.write('\n')

            elif type == 'OVS':
                writePliz = True
                fileStructures.write('[structure]\n')
                fileStructures.write('type = gate\n')
                fileStructures.write('id = ' + value[0] + '\n')
                fileStructures.write('branchid = ' + branchId + '\n')
                fileStructures.write('chainage = ' + branchOffset + '\n')
                fileStructures.write('polylinefile = ' + pliName + '.pli\n')

                fileStructures.write('sill_level = ' + self.to2Dec(value[7]) + '\n')
                fileStructures.write('door_height = ' + self.to2Dec(self.deltaGateHeightTopLevel) + '\n')
                fileStructures.write('lower_edge_level = ' + self.to2Dec(self.getGateHeight(value[0])) + '\n')
                fileStructures.write('opening_width = ' + self.to2Dec(value[6]) + '\n')
                fileStructures.write('horizontal_opening_direction = symmetric\n')

                fileStructures.write('\n')

            elif type == 'PMP':
                writePliz = True
                fileStructures.write('[structure]\n')
                fileStructures.write('type = pump\n')
                fileStructures.write('id = ' + value[0] + '\n')
                fileStructures.write('branchid = ' + branchId + '\n')
                fileStructures.write('chainage = ' + branchOffset + '\n')
                fileStructures.write('polylinefile = ' + pliName + '.pli\n')
                valuePerSecond = float(value[9])/3600
                fileStructures.write('capacity = ' + str("%.4f" % round(float(valuePerSecond),4)) + '  #m3/s\n')

                direction = self.getDirectionOfStructure(value[0])
                if direction == '1_2':
                    fileStructures.write('direction = 1\n')
                elif direction == '2_1':
                    fileStructures.write('direction = 2\n')
                else:
                    fileStructures.write('direction = 3\n')

                if value[10] != '' and value[11] != '':
                    fileStructures.write('start_level_suction_side = ' + self.to2Dec(value[10]) + '\n')
                    fileStructures.write('stop_level_suction_side = ' + self.to2Dec(value[11]) + '\n')

                if value[12] != '' and value[13] != '':
                    fileStructures.write('start_level_delivery_side = ' + self.to2Dec(value[12]) + '\n')
                    fileStructures.write('stop_level_delivery_side = ' + self.to2Dec(value[13]) + '\n')
                direction = 1
                fileStructures.write('reduction_factor_no_levels = 1.00\n')
                fileStructures.write('\n')

            if writePliz:
                xyz = self.getGeometryOfStructure(value[0])
                name = value[0]
                self.writePliFile(dirPath, outputDir, name,xyz, pliName)

        fileStructures.close()
        return True

    def writeLaterals(self, dirPath, outputDir):  # write all inlets from GWSW model

        fileLateral = open(os.path.join(dirPath, outputDir, 'lateral_locations.ini'), 'w')

        #header
        fileLateral.write('[general]\n')
        fileLateral.write('majorVersion = 1\n')
        fileLateral.write('minorVersion = 0\n')
        fileLateral.write('fileType = latLocs\n')
        fileLateral.write('\n')

        for keyvalue in self.model.inlets.items():
            value = keyvalue[1]
            fileLateral.write('[lateraldischarge]\n')
            fileLateral.write('id = lateral' + value[0] + '\n')
            fileLateral.write('name = nameLateral' + value[0] + '\n')
            fileLateral.write('nodeId = ' + value[0] + '\n')
            fileLateral.write('\n')

        fileLateral.close()
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

    def getGeometryOfStructure(self, id): #0,5 meter from node, 1 meter south -> north
        xyz = []
        connection = self.model.connections[id]
        nodeId = connection[1]
        node = self.model.nodes[nodeId]
        z = connection[5]
        x = float(node[3]) + 0.5
        y0 = float(node[4]) - 0.5
        y1 = y0 + 1.0
        xyz.append([x, y0, z])
        xyz.append([x, y1, z])
        return xyz

    def getDirectionOfStructure(self, id):
        connection = self.model.connections[id]
        result = connection[7]
        return result

    def getAreaOfOrifice(self, id):
        connection = self.model.connections[id]
        profile = self.model.profiles[connection[15]]
        result = pi * ((float(profile[3])/200.0) ** 2)
        return result

    def getGateHeight(self, id):
        connection = self.model.connections[id]
        result = 0.0
        try:
            cmp1Level = float(self.model.nodes[connection[1]][6])
            cmp2Level = float(self.model.nodes[connection[2]][6])
            if cmp1Level < cmp2Level:
                result = cmp1Level - self.deltaGateHeightTopLevel
            else:
                result = cmp2Level - self.deltaGateHeightTopLevel
        except ValueError:
            print("Missing data for gateheight " + id)
        return result

