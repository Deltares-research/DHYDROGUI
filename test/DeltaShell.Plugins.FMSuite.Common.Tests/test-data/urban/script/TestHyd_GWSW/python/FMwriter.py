import os
import netCDF4

class FMwriter:
    """Writer for FM files"""

    def __init__(self, model):
        self.model = model

    def writeAll(self, dirPath):  # write all fm files from GWSW model
        self.writeNodes(dirPath)
        self.writeConnections(dirPath)
        return True

    def writeNodes(self, dirPath):  # write all fm files from GWSW model
        filePath = os.path.join(dirPath, 'output_inputFM\\node.ini')
        # UNIQUE_ID	UNI_IDE
        # MANHOLE_ID	IDE_KNP
        # network_node_id
        # MANHOLE_INDICATOR	KNP_KNM
        # COMPARTMENT_NUMBER	IDE_COM
        # SURFACE_LEVEL	MVD_NIV
        # FLOODABLE_AREA	WOS_ OPP
        # NODE_SHAPE	KNP_VRM
        # BOTTOM_LEVEL	KNP_BOK
        # NODE_WIDTH	KNP_BRE
        # NODE_LENGTH	KNP_LEN

        file  = open(filePath, 'w')
        for key, value in sorted(self.model.nodes.items()):
            file.write('[node]\n')
            file.write('id='+str(key)+'\n')
            file.write('network_node_id='+str(value[1])+'\n')
            file.write('manhole_id='+str(value[1])+'\n')
            file.write('manhole_indicator='+str(value[4])+'\n')
            file.write('compartment_number=\n')
            file.write('surface_level='+str(value[5])+'\n')
            file.write('floodable_area='+str(value[7])+'\n')
            file.write('node_shape='+str(value[9])+'\n')
            file.write('bottom_level='+str(value[10])+'\n')
            file.write('node_width='+str(value[11])+'\n')
            file.write('node_length='+str(value[12])+'\n')
            file.write('\n')
        file.close()
        return True

    def writeConnections(self, dirPath):  # write all fm files from GWSW model
        filePath = os.path.join(dirPath, 'output_inputFM\\pipes.ini')
        # UNIQUE_ID	UNI_IDE
        # NODE_ID_START	UNI_ID1
        # NODE_ID_END	UNI_ID2
        # network_branch_id
        # PIPE_TYPE	LEI_TYP
        # LEVEL_START	BOB_IDE1
        # LEVEL_END	BOB_IDE2
        # LENGTH	VRB_LEN
        # CROSS_SECTION_DEF	PRO_DEF
        file  = open(filePath, 'w')
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
                file.write('id='+str(key)+'\n')
                file.write('node_id_start='+str(value[0])+'\n')
                file.write('node_id_end='+str(value[1])+'\n')
                file.write('network_branch_id='+str(value[3])+'\n')
                #file.write('pipe_type'+str(value[1])+'\n')
                file.write('level_start='+str(value[4])+'\n')
                file.write('level_end'+str(value[5])+'\n')
                #file.write('length'+str(value[7])+'\n')
                #file.write('cross_section_def'+str(value[14])+'\n')