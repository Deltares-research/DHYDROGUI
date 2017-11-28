import os
from math import pi
from netCDF4 import Dataset
from datetime import *


class FMwriter:
    """Writer for FM files"""

    def __init__(self, model):
        self.model = model

    def writeAll(self, dirPath):  # write all fm files from GWSW model
        self.writeBoundaries(dirPath)
        self.writeRetentions(dirPath)
        self.writePipes(dirPath)
        self.writeProfiles(dirPath)
        self.writeFMnetwork(dirPath, "test")
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

        filePathIni = os.path.join(dirPath, 'output_inputFM', 'BoundaryLocations.ini')
        filePathBc = os.path.join(dirPath, 'output_inputFM', 'BoundaryConditions.bc')

        fileLocs = open(filePathIni, 'w')
        fileBc = open(filePathBc, 'w')

        #header Ini
        fileLocs.write('[General]\n')
        fileLocs.write('majorVersion = 1\n')
        fileLocs.write('minorVersion = 0\n')
        fileLocs.write('fileType = boundLocs\n')
        fileLocs.write('\n')

        #header Bc
        fileBc.write('[Boundary]\n')
        fileBc.write('majorVersion = 1\n')
        fileBc.write('minorVersion = 0\n')
        fileBc.write('fileType = boundConds\n')
        fileBc.write('\n')

        for keyvalue in self.model.nodes.items():

            value = keyvalue[1]

            if str(value[14]) != 'UIT': # is not a boundary
                continue

            #location
            fileLocs.write('[Boundary]\n')
            fileLocs.write('nodeId = ' + str(value[0]) + '\n')
            fileLocs.write('type = 1\n')
            fileLocs.write('\n')

            #condition
            fileBc.write('[Boundary]\n')
            fileBc.write('name = ' + str(value[0]) + '\n')
            fileBc.write('function = constant\n')
            fileBc.write('time-interpolation = linear-extrapolate\n')
            fileBc.write('quantity = water_level\n')
            fileBc.write('unit = m\n')
            fileBc.write(defaultWaterlevelOut + '\n')
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
        file.write('[General]\n')
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
            file.write('[Retention]\n')
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
        fileCSLoc.write('[General]\n')
        fileCSLoc.write('majorVersion = 1\n')
        fileCSLoc.write('minorVersion = 0\n')
        fileCSLoc.write('fileType = crossLoc\n')
        fileCSLoc.write('\n')

        #header roughness
        fileRough.write('[General]\n')
        fileRough.write('majorVersion = 1\n')
        fileRough.write('minorVersion = 0\n')
        fileRough.write('fileType = roughness\n')
        fileRough.write('\n')
        fileRough.write('[Content]\n')
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
            fileCSLoc.write('[CrossSection]\n')
            fileCSLoc.write('id = ' + str(value[0]) + '_start\n')
            fileCSLoc.write('branchid = ' + str(value[0]) + '\n')
            fileCSLoc.write('chainage = 0.00\n')
            fileCSLoc.write('shift = ' + self.to2Dec(value[5]) + '\n')
            fileCSLoc.write('definition = ' + str(value[15]) + '\n')
            fileCSLoc.write('\n')

            #end
            fileCSLoc.write('[CrossSection]\n')
            fileCSLoc.write('id = ' + str(value[0]) + '_end\n')
            fileCSLoc.write('branchid = ' + str(value[0]) + '\n')
            length = str("%.2f" % round(float(value[8]),2))
            fileCSLoc.write('chainage = ' + length + '\n')
            fileCSLoc.write('shift = ' + self.to2Dec(value[6]) + '\n')
            fileCSLoc.write('definition = ' + str(value[15]) + '\n')
            fileCSLoc.write('\n')

            #roughness
            fileRough.write('[Definition]\n')
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
        fileCSDef.write('[General]\n')
        fileCSDef.write('majorVersion = 1\n')
        fileCSDef.write('minorVersion = 0\n')
        fileCSDef.write('fileType = crossDef\n')
        fileCSDef.write('\n')

        for keyvalue in self.model.profiles.items():

            value = keyvalue[1]

            fileCSDef.write('[Definition]\n')
            fileCSDef.write('id = ' + str(value[0]) + '\n')

            w = float(value[3])
            wString = str("%.2f" % round(w,2))
            t = str(value[2])
            if t == 'RND':
                fileCSDef.write('type = circle\n')
                fileCSDef.write('diameter = ' + wString + '\n')
            elif t == 'EIV':
                fileCSDef.write('type = egg\n')
                fileCSDef.write('width = ' + wString + '\n')
                fileCSDef.write('height = ' + self.to2Dec(w * 1.5) + '\n') #redundant ??
            else:
                fileCSDef.write('type = rectangle\n')
                fileCSDef.write('width = ' + wString + '\n')
                fileCSDef.write('height = ' + self.to2Dec(value[4]) + '\n')

            fileCSDef.write('closed = 1\n')
            fileCSDef.write('groundlayerUsed = 0\n')
            fileCSDef.write('roughnessNames = SewerSystem\n')
            fileCSDef.write('\n')

        fileCSDef.close()
        return True

    ## writeFMnetwork documentation
    # This fuction is going to prepare 1D Ugrid files
    # Following code is just a DRAFT AND NEEDS SIGNIFICANT IMPROVEMENTS
    def writeFMnetwork(self, dirPath, name):
        ### NETCDF approach
        output_file = os.path.join(dirPath, 'output_inputFM', name + "_net.nc")
        print("OWN NETCDF")
        outformat = "NETCDF3_CLASSIC"

        ncfile = Dataset(output_file,'w',format=outformat)

        nodes_nr = len(self.model.nodes)
        edges_nr = len(self.model.connections)

        print("Amount of nodes:",nodes_nr )
        print("Amount of connections:", edges_nr)

        ncfile.createDimension("nNetworkNodes", nodes_nr)

        ncfile.createDimension("nMeshEdges",edges_nr)
        ncfile.createDimension("nMeshNodes", nodes_nr)
        ncfile.createDimension("Two", 2)
        ncfile.createDimension("instance", 1)

        # global attributes
        ncfile.Conventions = "CF-1.8 UGRID-1.0/Deltares-0.91"
        ncfile.history = "Created on %s D-Flow 1D" % datetime.now()
        ncfile.institution = "Deltares"
        ncfile.reference = "http://www.deltares.nl"
        ncfile.source = "Python script to prepare D-Flow FM 1D network"

        branch_x = ncfile.createVariable("network1D_geom_x", "f8", ("nNetworkNodes"))
        branch_x.standard_name = 'projection_x_coordinate'
        branch_x.units = 'm'
        branch_x.cf_role = "geometry_x_node"

        branch_y = ncfile.createVariable("network1D_geom_y", "f8", ("nNetworkNodes"))
        branch_y.standard_name = 'projection_y_coordinate'
        branch_y.units = 'm'
        branch_y.cf_role = "geometry_y_node"

        i=0
        for key in self.model.nodes.keys():
            print(key)
            print(self.model.nodes[key][2],self.model.nodes[key][3])
            branch_x[i]= self.model.nodes[key][2]
            branch_y[i] = self.model.nodes[key][3]
            i+=1
        print(i)
        print(branch_x)
        print(branch_y)

        return True
