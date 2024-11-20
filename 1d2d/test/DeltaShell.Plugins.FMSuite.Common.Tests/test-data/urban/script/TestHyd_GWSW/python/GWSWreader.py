# coding: latin-1
import os
import csv
from collections import OrderedDict
from GWSWmodel import GWSWmodel
from UgridReader import UgridReader

class GWSWreader:
    """Reads all GWSW files"""

    csvDelimeter = ';'
    dirPath = ''
    inputDir = ''
    gridFile = ''

    def readAll(self, dirPath, inputDir, gridFile, oppervlakOnNode = False):   # path directory files
        self.dirPath = dirPath
        self.inputDir = inputDir
        self.oppervlakOnNode = oppervlakOnNode
        model = GWSWmodel()
        model.nodes = self.readNodes2Dict()
        model.connections = self.readConnections2Dict()
        model.profiles = self.readProfiles2Dict()
        model.structures = self.readStructures2Dict()
        model.inlets = self.readInlet2Dict(model)
        if gridFile is not None and gridFile != '':
            reader = UgridReader(model)
            filePath = os.path.join(self.dirPath, self.inputDir, gridFile)
            reader.ReadFile(filePath)

        return model

    def file2Dict(self, filePath):
        dict=OrderedDict()
        with open(filePath) as csvfile:
            file = csv.reader(csvfile,delimiter = self.csvDelimeter)
            firstLine = True
            for line in file:
                if not firstLine:
                    dict[line[0]] = line[0:]
                firstLine = False
        return dict

    def readNodes2Dict(self):
        filePath = os.path.join(self.dirPath, self.inputDir,'Knooppunt.csv')
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
        return self.file2Dict(filePath)


    def readConnections2Dict(self):
        filePath = os.path.join(self.dirPath, self.inputDir,'Verbinding.csv')
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
        return self.file2Dict(filePath)

    def readProfiles2Dict(self):
        filePath = os.path.join(self.dirPath, self.inputDir,'Profiel.csv')
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
        return self.file2Dict(filePath)

    def readStructures2Dict(self):
        filePath = os.path.join(self.dirPath, self.inputDir,'Kunstwerk.csv')
        #UNI_IDE  Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #KWK_TYP  Type hydraulisch component in het kunstwerk
        #BWS_NIV  Buitenwaterstand t.o.v. NAP
        #PRO_BOK  Niveau binnenonderkant profiel t.o.v. NAP
        #DRL_COE  Contractiecoëfficient doorlaatprofiel
        #DRL_CAP  Maximale capaciteit doorlaat
        #OVS_BRE  Breedte overstortdrempel
        #OVS_NIV  Niveau overstortdrempel t.o.v. NAP
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
        return self.file2Dict(filePath)


    def readBoundary2Dict(self):
        filePath = os.path.join(self.dirPath, self.inputDir,'Debiet.csv')
        #UNI_IDE  Unieke identificatie van het knooppunt of de verbinding, een verwijzing naar de bestandsregel-identificatie. De waarde van deze kolom mag slechts één keer voorkomen in zowel Knooppunt.csv als Verbinding.csv. Koppeling tussen Knooppunt.csv of Verbinding.csv met Kunstwerk.csv, BOP.csv, Oppervlak.csv, Debiet.csv.
        #DEB_TYP  Debiet vanuit DWA, RWA en Lozing
        #VER_IDE  Verloop van debiet. Koppeling tussen Debiet.csv en Verloop.csv.
        #AVV_ENH  Aantal vervuilingseenheden behorend bij de verloop-definitie
        #AFV_OPP  Omvang afvoerend oppervlak
        #ALG_TOE  Toelichting bij deze regel
        return self.file2Dict(filePath)

    def readTimeSeries2Dict(self):
        filePath = os.path.join(self.dirPath, self.inputDir,'Verloop.csv')
        #VER_IDE  Verloop van debiet. Koppeling tussen Debiet.csv en Verloop.csv.
        #VER_TYP  Constant of variabel verloop
        #VER_DAG  Dagnummer (1 t/m 7) Maandag is dag 1, dinsdag is dag 2 etc.
        #VER_VOL  Dagvolume
        #U00_DAG  Percentage van het dagvolume op 00 uur
        #U01_DAG  Percentage van het dagvolume op 01 uur
        #U02_DAG  Percentage van het dagvolume op 02 uur
        #U03_DAG  Percentage van het dagvolume op 03 uur
        #U04_DAG  Percentage van het dagvolume op 04 uur
        #U05_DAG  Percentage van het dagvolume op 05 uur
        #U06_DAG  Percentage van het dagvolume op 06 uur
        #U07_DAG  Percentage van het dagvolume op 07 uur
        #U08_DAG  Percentage van het dagvolume op 08 uur
        #U09_DAG  Percentage van het dagvolume op 09 uur
        #U10_DAG  Percentage van het dagvolume op 10 uur
        #U11_DAG  Percentage van het dagvolume op 11 uur
        #U12_DAG  Percentage van het dagvolume op 12 uur
        #U13_DAG  Percentage van het dagvolume op 13 uur
        #U14_DAG  Percentage van het dagvolume op 14 uur
        #U15_DAG  Percentage van het dagvolume op 15 uur
        #U16_DAG  Percentage van het dagvolume op 16 uur
        #U17_DAG  Percentage van het dagvolume op 17 uur
        #U18_DAG  Percentage van het dagvolume op 18 uur
        #U19_DAG  Percentage van het dagvolume op 19 uur
        #U20_DAG  Percentage van het dagvolume op 20 uur
        #U21_DAG  Percentage van het dagvolume op 21 uur
        #U22_DAG  Percentage van het dagvolume op 22 uur
        #U23_DAG  Percentage van het dagvolume op 23 uur
        #ALG_TOE  Toelichting bij deze regel
        return self.file2Dict(filePath)

    def readInlet2Dict(self, model):
        filePath = os.path.join(self.dirPath, self.inputDir,'Oppervlak.csv')

        #UNI_IDE
        #NSL_STA
        #AFV_DEF
        #AFV_IDE
        #AFV_OPP
        dict=OrderedDict()
        with open(filePath) as csvfile:
            file = csv.reader(csvfile,delimiter = self.csvDelimeter)
            firstLine = True
            for line in file:
                if not firstLine:
                    if self.oppervlakOnNode:
                        nodeId = line[0]
                        if nodeId in model.nodes:
                            node =  model.nodes[nodeId]
                            if nodeId in dict:
                                dict[nodeId][1] += float(line[4])
                            else:
                                dict[nodeId] = [nodeId,float(line[4])]
                        else:
                             print("Node " + nodeId + " not found from Oppervlak.csv")
                    else:
                        connectionId = line[0]
                        if connectionId in model.connections:
                            connection = model.connections[connectionId]
                            if float(connection[6]) > float(connection[5]):
                                nodeId = connection[2]
                            else:
                                nodeId = connection[1]

                            if nodeId in dict:
                                dict[nodeId][1] += float(line[4])
                            else:
                                dict[nodeId] = [nodeId,float(line[4])]
                        else:
                            print("Connection " + connectionId + " not found from Oppervlak.csv")
                firstLine = False
        return dict
