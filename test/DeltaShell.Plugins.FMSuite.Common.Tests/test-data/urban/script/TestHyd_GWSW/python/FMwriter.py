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
        self.writeConnections(dirPath)
        self.writeFMnetwork(dirPath, "test")
        return True

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

            item = keyvalue[1]

            if str(item[14]) != 'UIT': # is not a boundary
                continue

            #location
            fileLocs.write('[Boundary]\n')
            fileLocs.write('nodeId = ' + str(item[0]) + '\n')
            fileLocs.write('type = 1\n')
            fileLocs.write('\n')

            #condition
            fileBc.write('[Boundary]\n')
            fileBc.write('name = ' + str(item[0]) + '\n')
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
        filePath = os.path.join(dirPath, 'output_inputFM', 'Retention.ini')
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

        file = open(filePath, 'w')

        #header
        file.write('[General]\n')
        file.write('majorVersion = 1\n')
        file.write('minorVersion = 0\n')
        file.write('\n')

        for keyvalue in self.model.nodes.items():

            item = keyvalue[1]

            if str(item[14]) == 'UIT': # is a boundary
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
            file.write('id = ' + str(item[0]) + '\n')
            file.write('name = ' + str(item[2]) + '\n')
            file.write('nodeId = ' + str(item[0]) + '\n')
            file.write('manholeId = ' + str(item[2]) + '\n')
            file.write('storageType = Closed\n')
            file.write('useTable = 0\n')
            file.write('bedLevel = ' + str(item[11]) + '\n')

            area = 0.0
            br = (float(item[12]) / 100)
            l = (float(item[13]) / 100)

            if str(item[11]) == 'RND':
                area = pi * br**2
            else:
                area = br * l

            areaStr  = str("%.2f" % round(area,2))

            file.write('area = ' + areaStr + '\n')
            file.write('streetLevel = ' + str(item[6]) + '\n')
            file.write('\n')
        file.close()
        return True

    def writeConnections(self, dirPath):  # write all fm files from GWSW model
        filePath = os.path.join(dirPath, 'output_inputFM', 'pipes.ini')
        # UNIQUE_ID	UNI_IDE
        # NODE_ID_START	UNI_ID1
        # NODE_ID_END	UNI_ID2
        # network_branch_id
        # PIPE_TYPE	LEI_TYP
        # LEVEL_START	BOB_IDE1
        # LEVEL_END	BOB_IDE2
        # LENGTH	VRB_LEN
        # CROSS_SECTION_DEF	PRO_DEF
        file = open(filePath, 'w')
        for key, value in sorted(self.model.connections.items()):
            # type
            # Doorlaat	DRL
            # Gesloten leiding	GSL
            # Infiltratieriool	ITR
            # Open leiding	OPL
            # Overstortdrempel	OVS
            # Pomp	PMP
            if str(value[2]) == 'GSL':
                file.write('[pipe]\n')
                file.write('id=' + str(key) + '\n')
                file.write('node_id_start=' + str(value[0]) + '\n')
                file.write('node_id_end=' + str(value[1]) + '\n')
                file.write('network_branch_id=' + str(value[3]) + '\n')
                # file.write('pipe_type'+str(value[1])+'\n')
                file.write('level_start=' + str(value[4]) + '\n')
                file.write('level_end=' + str(value[5]) + '\n')
                # file.write('length'+str(value[7])+'\n')
                # file.write('cross_section_def'+str(value[14])+'\n')
                file.write('\n')
        file.close()
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
