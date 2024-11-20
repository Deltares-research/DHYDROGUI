import os
from math import pi
from netCDF4 import Dataset
from collections import OrderedDict
from datetime import *


class FMwriter:
    """Writer for FM files"""

    deltaGateHeightTopLevel = 0.3

    def __init__(self, model):
        self.model = model

    def writeAll(self, dirPath):  # write all fm files from GWSW model
        self.writeBoundaries(dirPath)
        self.writeRetentions(dirPath)
        self.writePipes(dirPath)
        self.writeProfiles(dirPath)
        self.writeStructures(dirPath)
        self.writeLaterals(dirPath)
        self.writeFMnetwork(dirPath, "ugrid")
        return True

    def to2Dec(selfselft, strValue):
        result = str("%.2f" % round(float(strValue),2))
        return result;

    def writeBoundaries(self, dirPath):  # write boundaries ini and bc

        #UNI_IDE	Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #RST_IDE	Identificatie (naam, nummer, code) van het rioolstelsel
        #PUT_IDE	Identificatie (naam, nummer, code) van de put of het bouwwerk
        #KNP_XCO	X-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #KNP_YCO	Y-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #CMP_IDE	Identificatie (naam, nummer, code) van het compartiment
        #MVD_NIV	Niveau maaiveld t.o.v. NAP
        #MVD_SCH	Type maaiveldschematisering
        #WOS_OPP	Oppervlak water op straat
        #KNP_MAT	Materiaal put
        #KNP_VRM	Vorm put
        #KNP_BOK	Niveau binnenonderkant put t.o.v. NAP
        #KNP_BRE	Breedte/diameter putbodem
        #KNP_LEN	Lengte putbodem
        #KNP_TYP	Type knooppunt
        #INZ_TYP	Type afvalwater dat wordt ingezameld
        #INI_NIV	Initiële waterstand t.o.v. NAP
        #STA_OBJ	Status van het object
        #AAN_MVD	Aanname maaiveldhoogte
        #ITO_IDE	Definitie infiltratiekarakteristieken. Koppeling tussen ItObject.csv en Verbinding.csv of Knooppunt.csv
        #ALG_TOE	Toelichting bij deze regel

        defaultWaterlevelOut = '-100.00' # low value needed
        defaultDischargeIn = '0.01' # m3/s from street

        filePathIni = os.path.join(dirPath, 'output_inputFM', 'BoundaryLocations.ini')
        filePathBc = os.path.join(dirPath, 'output_inputFM', 'BoundaryConditions.bc')

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

        for keyvalue in self.model.nodes.items():
            value = keyvalue[1]
            #laterals
            fileBc.write('[lateraldischarge]\n')
            fileBc.write('name = lateral' + str(value[0]) + '\n')
            fileBc.write('function = constant\n')
            fileBc.write('time-interpolation = linear-extrapolate\n')
            fileBc.write('quantity = water_discharge\n')
            fileBc.write('unit = m3/s\n')
            fileBc.write(defaultDischargeIn + ' #surface of ' + str(value[1]) + '  m2\n')
            fileBc.write('\n')

        fileLocs.close()
        fileBc.close()

        return True


    def writeRetentions(self, dirPath):  # write all manholes from GWSW model

        #UNI_IDE	Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #RST_IDE	Identificatie (naam, nummer, code) van het rioolstelsel
        #PUT_IDE	Identificatie (naam, nummer, code) van de put of het bouwwerk
        #KNP_XCO	X-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #KNP_YCO	Y-coördinaat knooppunt. Conform coördinatenstelsel EPSG:7415 (x/y conform EPSG:28992 (=RD), z conform EPSG:5709 (=NAP).
        #CMP_IDE	Identificatie (naam, nummer, code) van het compartiment
        #MVD_NIV	Niveau maaiveld t.o.v. NAP
        #MVD_SCH	Type maaiveldschematisering
        #WOS_OPP	Oppervlak water op straat
        #KNP_MAT	Materiaal put
        #KNP_VRM	Vorm put
        #KNP_BOK	Niveau binnenonderkant put t.o.v. NAP
        #KNP_BRE	Breedte/diameter putbodem
        #KNP_LEN	Lengte putbodem
        #KNP_TYP	Type knooppunt
        #INZ_TYP	Type afvalwater dat wordt ingezameld
        #INI_NIV	Initiële waterstand t.o.v. NAP
        #STA_OBJ	Status van het object
        #AAN_MVD	Aanname maaiveldhoogte
        #ITO_IDE	Definitie infiltratiekarakteristieken. Koppeling tussen ItObject.csv en Verbinding.csv of Knooppunt.csv
        #ALG_TOE	Toelichting bij deze regel

        filePath = os.path.join(dirPath, 'output_inputFM', 'Retention.ini')
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
            file.write('[retention]\n')
            file.write('id = ' + str(value[0]) + '\n')
            file.write('name = ' + str(value[2]) + '\n')
            file.write('nodeId = ' + str(value[0]) + '\n')
            file.write('manholeId = ' + str(value[2]) + '\n')
            file.write('storageType = Closed\n')
            file.write('useTable = 0\n')
            file.write('bedLevel = ' + str(value[11]) + '\n')

            area = 0.0
            br = (float(value[12]) / 100)
            l = (float(value[13]) / 100)

            if str(value[11]) == 'RND':
                area = pi * br**2
            else:
                area = br * l

            areaStr  = self.to2Dec(area)

            file.write('area = ' + areaStr + '\n')
            file.write('streetLevel = ' + str(value[6]) + '\n')
            file.write('\n')
        file.close()
        return True

    def writePipes(self, dirPath):  # write pipes described as crossections from GWSW model

        #UNI_IDE	Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #KN1_IDE	Identificatie knooppunt 1. Verwijzing naar UNI_IDE in Knooppunt.csv. Als het type verbinding een overstortdrempel of doorlaat is (Verbinding/VRB_TYP=DRP, DRL) dan moet het type knooppunt een compartiment zijn (Knooppunt/KNP_TYP=CMP).
        #KN2_IDE	Identificatie knooppunt 2. Verwijzing naar UNI_IDE in Knooppunt.csv. Als het type verbinding een overstortdrempel of doorlaat is (Verbinding/VRB_TYP=DRP, DRL) dan moet het type knooppunt een compartiment zijn (Knooppunt/KNP_TYP=CMP).
        #VRB_TYP	Type verbinding
        #LEI_IDE
        #BOB_KN1	Binnenonderkant buis knooppunt 1 t.o.v. NAP
        #BOB_KN2	Binnenonderkant buis knooppunt 2 t.o.v. NAP
        #STR_RCH	Mogelijke stromingsrichting door verbinding
        #VRB_LEN	Lengte van de leiding of de lengte die via het kunstwerk overbrugd wordt (bijvoorbeeld de lengte van de persleiding tussen pomp en lozingspunt)
        #INZ_TYP	Type afvalwater dat wordt ingezameld
        #INV_KN1	Instroomverliescoëfficient knooppunt 1
        #UTV_KN1	Uitstroomverliescoëfficient knooppunt 1
        #INV_KN2	Instroomverliescoëfficient knooppunt 2
        #UTV_KN2	Uitstroomverliescoëfficient knooppunt 2
        #ITO_IDE	Definitie infiltratiekarakteristieken. Koppeling tussen ItObject.csv en Verbinding.csv of Knooppunt.csv
        #PRO_IDE	Profieldefinitie. Koppeling tussen Profiel.csv en Verbinding.csv
        #STA_OBJ	Status van het object
        #AAN_BB1	Aanname waarde BOB_KN1
        #AAN_BB2	Aanname waarde BOB_KN2
        #INI_NIV	Initiële waterstand t.o.v. NAP
        #ALG_TOE	Toelichting bij deze regel

        fileCSLoc = open(os.path.join(dirPath, 'output_inputFM', 'CrossSectionLocations.ini'), 'w')
        fileRough = open(os.path.join(dirPath, 'output_inputFM', 'Roughness_SewerSystem.ini'), 'w')

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
        fileRough.write('sectionId = SewerSystem\n')
        fileRough.write('flowDirection = False\n')
        fileRough.write('interpolate = 1\n')
        fileRough.write('globalType = 7\n')
        fileRough.write('globalValue = 3.000\n')
        fileRough.write('\n')

        for keyvalue in self.model.connections.items():

            value = keyvalue[1]

            # Gesloten leiding	GSL
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
            fileRough.write('branchId = ' + str(value[0]) + '\n')
            fileRough.write('chainage = 0.00\n')
            fileRough.write('value = 3.000\n')
            fileRough.write('\n')

        fileCSLoc.close()
        fileRough.close()
        return True

    def writeProfiles(self, dirPath):  # write all profiles from GWSW model

        #PRO_IDE	Profieldefinitie. Koppeling tussen Profiel.csv en Verbinding.csv
        #PRO_MAT	Materiaal profiel
        #PRO_VRM	Vorm profiel
        #PRO_BRE	Breedte/diameter profiel
        #PRO_HGT	Hoogte profiel
        #OPL_HL1	Co-tangens helling 1
        #OPL_HL2	Co-tangens helling 2
        #PRO_NIV	Niveau boven b.o.b. Als er meerdere profielwaardes per niveau gelden, dan meerdere Profiel-regels opnemen met gelijke waarde van PRO_IDE. De waarde van PRO_NIV mag daarbij dus niet gelijk zijn
        #PRO_NOP	Nat oppervlak bij niveau
        #PRO_NOM	Natte omtrek bij niveau
        #PRO_BRE	Breedte bij niveau
        #AAN_PBR	Aanname profielbreedte
        #ALG_TOE	Toelichting bij deze regel

        fileCSDef = open(os.path.join(dirPath, 'output_inputFM', 'CrossSectionsDefinitions.ini'), 'w')

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
            fileCSDef.write('roughnessNames = SewerSystem\n')
            fileCSDef.write('\n')

        fileCSDef.close()
        return True

    def writeStructures(self, dirPath):

        #UNI_IDE	Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #KWK_TYP	Type hydraulisch component in het kunstwerk
        #BWS_NIV	Buitenwaterstand t.o.v. NAP
        #PRO_BOK	Niveau binnenonderkant profiel t.o.v. NAP
        #DRL_COE	Contractiecoëfficient doorlaatprofiel
        #DRL_CAP	Maximale capaciteit doorlaat
        #OVS_BRE	Breedte overstortdrempel / crest length
        #OVS_NIV	Niveau overstortdrempel t.o.v. NAP / crest height
        #OVS_COE	Afvoercoëfficient overstortdrempel
        #PMP_CAP	Capaciteit van de individuele pomp
        #PMP_AN1	Aanslagniveau benedenstrooms (zuigzijde) pomp t.o.v. NAP
        #PMP_AF1	Afslagniveau benedenstrooms (zuigzijde) pomp t.o.v. NAP
        #PMP_AN2	Aanslagniveau bovenstrooms (perszijde) pomp t.o.v. NAP
        #PMP_AF2	Afslagniveau benedenstrooms (perszijde) pomp t.o.v. NAP
        #QDH_NIV	Niveauverschil bij debiet-verhangrelatie
        #QDH_DEB	Debietverschil bij debiet-verhangrelatie
        #AAN_OVN	Aanname waarde OVS_NIV
        #AAN_OVB	Aanname waarde OVS_BRE
        #AAN_CAP	Aanname waarde PMP_CAP
        #AAN_ANS	Aanname waarde PMP_ANS
        #AAN_AFS	Aanname waarde PMP_AFS
        #ALG_TOE	Toelichting bij deze regel

        fileStructures = open(os.path.join(dirPath, 'output_inputFM', 'Structures.ini'), 'w')

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

            if type == 'DRL':
                writePliz = True
                fileStructures.write('[structure]\n')
                fileStructures.write('type = orifice\n')
                fileStructures.write('id = ' + value[0] + '\n')
                fileStructures.write('polylinefile = ' + value[0] + '.pli\n')

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
                fileStructures.write('polylinefile = ' + value[0] + '.pli\n')

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
                fileStructures.write('polylinefile = ' + value[0] + '.pli\n')
                valuePerSecond = float(value[9])/3600
                fileStructures.write('capacity = ' + str("%.2f" % round(float(valuePerSecond),6)) + '  #m3/s\n')

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
                xyz = self.getXYZofStructure(value[0])
                self.writePliFile(dirPath,value[0],xyz)

        fileStructures.close()
        return True

    def writeLaterals(self, dirPath):  # write all inlets from GWSW model

        #_IDE	Profieldefinitie. Koppeling tussen Profiel.csv en Verbinding.csv
        #_Opp

        fileLateral = open(os.path.join(dirPath, 'output_inputFM', 'LateralLocations.ini'), 'w')

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

    def writePliFile(self, dirPath, name, xyz):
        plizFile = open(os.path.join(dirPath, 'output_inputFM', name + '.pli'), 'w')
        plizFile.write(name + '\n')
        plizFile.write('1    2\n')
        plizFile.write(xyz[0] + '    ' + xyz[1] + '\n')
        plizFile.close()
        return True

    def getXYZofStructure(self, id):
        xyz = [0.0,0.0,0.0]
        connection = self.model.connections[id]
        nodeId = connection[1]
        node = self.model.nodes[nodeId]
        xyz[0] = node[3]
        xyz[1] = node[4]
        xyz[2] = connection[5]
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
        cmp1Level = float(self.model.nodes[connection[1]][6])
        cmp2Level = float(self.model.nodes[connection[2]][6])
        if cmp1Level < cmp2Level:
            result = cmp1Level - self.deltaGateHeightTopLevel
        else:
            result = cmp2Level - self.deltaGateHeightTopLevel
        return result;

    ## writeFMnetwork documentation
    # This fuction is going to prepare 1D Ugrid files
    # Following code is just a DRAFT AND NEEDS SIGNIFICANT IMPROVEMENTS
        ## writeFMnetwork documentation
        # This fuction is going to prepare 1D Ugrid files
        # Following code is not a piece of art so please do improve it

    def writeFMnetwork(self, dirPath, name):
        ### NETCDF approach
        output_file = os.path.join(dirPath, 'output_inputFM', name + "_net.nc")

        # File format:
        outformat = "NETCDF4"  #"NETCDF3_CLASSIC"
        # File where we going to write
        ncfile = Dataset(output_file, 'w', format=outformat)

        # dimensions of the network
        nodes_nr = len(self.model.nodes)
        edges_nr = len(self.model.connections)

        # Temporary dictionary to store the id number of the nodes and branches
        node_order = OrderedDict()
        con_order = OrderedDict()

        # Definition of the network dimensions
        ncfile.createDimension("nNetworkBranches", edges_nr)
        ncfile.createDimension("nNetworkNodes", nodes_nr)
        ncfile.createDimension("nGeometryNodes", nodes_nr)
        ncfile.createDimension("nMesh1DEdges", edges_nr)
        ncfile.createDimension("nMesh1DNodes", edges_nr + 1)
        ncfile.createDimension("Two", 2)

        # global attributes
        ncfile.Conventions = "CF-1.8 UGRID-1.0/Deltares-0.91"
        ncfile.history = "Created on {} D-Flow 1D, D-Flow FM".format(datetime.now())
        ncfile.institution = "Deltares"
        ncfile.reference = "http://www.deltares.nl"
        ncfile.source = "Python script to prepare D-Flow FM 1D network"

        # geometry
        ntw = ncfile.createVariable("network1D", "u4", ())
        ntw.cf_role = 'mesh_topology'
        ntw.edge_dimension = 'nNetworkBranches'
        ntw.edge_geometry = 'network1D_geometry'
        ntw.edge_node_connectivity = 'network1D_edge_nodes'
        ntw.long_name = "Network topology"
        ntw.node_coordinates = 'network1D_nodes_x network1D_nodes_y'
        ntw.node_dimension = 'nNetworkNodes'
        ntw.topology_dimension = 1

        ntw_nodes_id = ncfile.createVariable("network1D_node_id", "str", "nNetworkNodes")
        ntw_nodes_id.standard_name = 'network1D_node_id_name'
        ntw_nodes_id.long_name = "The identification name of the node"

        ntw_nodes_x = ncfile.createVariable("network1D_nodes_x", "f8", "nNetworkNodes")
        ntw_nodes_x.standard_name = 'projection_x_coordinate'
        ntw_nodes_x.long_name = "x coordinates of the network connection nodes"
        ntw_nodes_x.units = 'm'

        ntw_nodes_y = ncfile.createVariable("network1D_nodes_y", "f8", "nNetworkNodes")
        ntw_nodes_y.standard_name = 'projection_y_coordinate'
        ntw_nodes_y.long_name = "y coordinates of the network connection nodes"
        ntw_nodes_y.units = 'm'

        i = 0
        for key in self.model.nodes.keys():
            ntw_nodes_id[i] = self.model.nodes[key][0]
            ntw_nodes_x[i] = self.model.nodes[key][3]
            ntw_nodes_y[i] = self.model.nodes[key][4]
            node_order[key] = i + 1
            i += 1

        ntw_geom = ncfile.createVariable("network1D_geometry", "u4", ())
        ntw_geom.geometry_type = 'multiline'
        ntw_geom.long_name = "1D Geometry"
        ntw_geom.node_count = "nGeometryNodes"
        ntw_geom.part_node_count = 'network1D_part_node_count'
        ntw_geom.node_coordinates = 'network1D_geom_x network1D_geom_y'

        ntw_geom_x = ncfile.createVariable("network1D_geom_x", "f8", ("nGeometryNodes"))
        ntw_geom_x.standard_name = 'projection_x_coordinate'
        ntw_geom_x.units = 'm'
        ntw_geom_x.cf_role = "geometry_x_node"
        ntw_geom_x.long_name = 'x coordinates of the branch geometries'

        ntw_geom_y = ncfile.createVariable("network1D_geom_y", "f8", ("nGeometryNodes"))
        ntw_geom_y.standard_name = 'projection_y_coordinate'
        ntw_geom_y.units = 'm'
        ntw_geom_y.cf_role = "geometry_y_node"
        ntw_geom_y.long_name = 'y coordinates of the branch geometries'

        # Note we could use the code that is above
        # In this case geometry and network are the same
        i = 0
        for key in self.model.nodes.keys():
            ntw_geom_x[i] = self.model.nodes[key][3]
            ntw_geom_y[i] = self.model.nodes[key][4]
            i += 1

        # mesh1D

        mesh1d = ncfile.createVariable("mesh1D", "u4", ())
        mesh1d.cf_role = 'mesh_topology'
        mesh1d.coordinate_space = 'network1D'
        mesh1d.edge_dimension = 'nmesh1DEdges'
        mesh1d.edge_node_connectivity = 'mesh1D_edge_nodes'
        mesh1d.long_name = "Mesh 1D"
        mesh1d.node_coordinates = 'mesh1D_nodes_branch_id mesh1D_nodes_branch_offset'
        mesh1d.node_dimension = 'nmesh1DNodes'
        mesh1d.topology_dimension = 1

        mesh1d_branch_id_name = ncfile.createVariable("mesh1D_branch_id", "str", "nMesh1DNodes")
        mesh1d_branch_id_name.cf_role = 'feature_name'
        mesh1d_branch_id_name.long_name = 'name of branch on which node is located'

        mesh1d_branch_id = ncfile.createVariable("mesh1D_nodes_branch_id", "u4", "nMesh1DNodes")
        mesh1d_branch_id.cf_role = 'feature_index'
        mesh1d_branch_id.long_name = 'number of branch on which node is located'
        i = 0
        for key in self.model.connections.keys():
            con_order[key] = i
            if i == 0:
                mesh1d_branch_id[0] = con_order[self.model.connections[key][0]]
                mesh1d_branch_id[1] = con_order[self.model.connections[key][0]]
                mesh1d_branch_id_name[0] = self.model.connections[key][0]
                mesh1d_branch_id_name[1] = self.model.connections[key][0]
                i = 1
            else:
                mesh1d_branch_id[i] = con_order[self.model.connections[key][0]]
                mesh1d_branch_id_name[i] = self.model.connections[key][0]
            i += 1

        #######-------------------------------------
        # This is a bit out of place due to the con_order which is filled above
        ntw_edge_node = ncfile.createVariable("network1D_edge_nodes", "u4", ("nNetworkBranches", "Two"))
        ntw_edge_node.cf_role = 'edge_node_connectivity'
        ntw_edge_node.long_name = 'start and end nodes of each branch in the network'
        ntw_edge_node.start_index = 1
        i = 0
        for key in self.model.connections.keys():
            ntw_edge_node[i, :] = [node_order[self.model.connections[key][1]],
                                   node_order[self.model.connections[key][2]]]
            i += 1
        #######-------------------------------------

        mesh1d_geom_offset = ncfile.createVariable("mesh1D_nodes_branch_offset", "f8", "nMesh1DNodes")
        mesh1d_geom_offset.cf_role = 'coordinate_on_feature'
        mesh1d_geom_offset.long_name = 'offset along the branch at which the node is located'
        mesh1d_geom_offset.units = 'm'
        mesh1d_geom_offset[0] = 0.
        i = 1
        for key in self.model.connections.keys():
            try:
                mesh1d_geom_offset[i] = self.model.connections[key][8]
            except:
                # print("Empty or not a number in a cell")
                mesh1d_geom_offset[i] = 1.
            i += 1

        # END OF THE NTWORK WRITER
        return True
