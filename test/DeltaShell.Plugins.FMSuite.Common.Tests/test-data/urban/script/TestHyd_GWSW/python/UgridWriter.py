import os
from netCDF4 import Dataset
from collections import OrderedDict
from datetime import *


class UgridWriter:
    """Writer for FM files"""

    def __init__(self, model):
        self.model = model

    def write(self, dirPath, outputDir):  # write ugrid file from GWSW model
        self.writeFMnetwork(dirPath, outputDir, "sewer_system")
        return True

    ## writeFMnetwork documentation
    # This fuction is going to prepare 1D Ugrid files
    # Following code is just a DRAFT AND NEEDS SIGNIFICANT IMPROVEMENTS
    ## writeFMnetwork documentation
    # This fuction is going to prepare 1D Ugrid files
    # Following code is not a piece of art so please do improve it

    def writeFMnetwork(self, dirPath, outputDir, name):
        ### NETCDF approach
        output_file = os.path.join(dirPath, outputDir, name + "_net.nc")

        # File format:
        outformat = "NETCDF4" #"NETCDF3_CLASSIC"
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

        # dimensions of the street grid
        min_x = -10000000 #sys.float_info.max
        max_x = 10000000 #sys.float_info.min
        min_y = -10000000 #sys.float_info.max
        max_y = 10000000 #sys.float_info.min
        for keyvalue in self.model.nodes.items():
            node = keyvalue[1]
            x = float(node[3])
            y = float(node[4])
            if x < min_x :  min_x = x
            if x > max_x :  max_x = x
            if y < min_y :  min_y = y
            if y > max_y :  max_y = y

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