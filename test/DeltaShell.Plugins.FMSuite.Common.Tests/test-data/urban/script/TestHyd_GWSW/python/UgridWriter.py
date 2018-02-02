import os, sys
from netCDF4 import Dataset
from collections import OrderedDict
from datetime import *


class UgridWriter:
    """Writer for FM files"""
    idstrlength = 40
    longstrlength = 80

    def __init__(self, model):
        self.model = model

    def write(self, dirPath, outputDir):  # write ugrid file from GWSW model
        ncfile = self.create_netcdf(dirPath, outputDir, "sewer_system")

        print("start generating 1d data")
        networkdata = self.generate_networkdata()

        print("start generating 2d data")
        data_2dmesh = self.generate_2dmesh_data()

        print("init ugrid 1d")
        self.init_1dnetwork(ncfile,networkdata)

        print("init ugrid 2d")
        self.init_2dmesh(ncfile,data_2dmesh)

        print("set ugrid 1d data")
        self.set_1dnetwork(ncfile,networkdata)

        print("set ugrid 2d data")
        self.set_2dmesh(ncfile,data_2dmesh)

        print("finished ugrid section")

        return True

    def create_netcdf(self,dirPath, outputDir, name):

        output_file = os.path.join(dirPath, outputDir, name + "_net.nc")
        # File format:
        outformat = "NETCDF3_CLASSIC" #"NETCDF4"
        # File where we going to write
        ncfile = Dataset(output_file, 'w', format=outformat)

        # global attributes
        ncfile.Conventions = "CF-1.8 UGRID-1.0/Deltares-0.91"
        ncfile.history = "Created on {} D-Flow 1D, D-Flow FM".format(datetime.now())
        ncfile.institution = "Deltares"
        ncfile.reference = "http://www.deltares.nl"
        ncfile.source = "Python script to prepare D-Flow FM 1D network"

        return ncfile

    def init_1dnetwork(self, ncfile, data):

        # dimensions of the network

        ncfile.createDimension("time", None)
        ncfile.createDimension("nNetworkBranches", len(data["branch_ids"]))
        ncfile.createDimension("nNetworkNodes", len(data["node_ids"]))
        ncfile.createDimension("nGeometryNodes", len(data["node_ids"]))
        ncfile.createDimension("idstrlength", self.idstrlength)
        ncfile.createDimension("longstrlength", self.longstrlength)
        ncfile.createDimension("nMesh1DEdges", len(data["edge_node"]))
        ncfile.createDimension("nMesh1DNodes", len(data["node_ids"]))
        ncfile.createDimension("Two", 2)

    def init_2dmesh(self, ncfile, data_2dmesh):

        # dimensions 2d mesh
        edges_2d = len(data_2dmesh["edge_x"])
        faces_2d = len(data_2dmesh["face_node"])
        nodes_2d = len(data_2dmesh["node_x"])

        ncfile.createDimension("max_nMesh2D_face_nodes", 4)
        ncfile.createDimension("nMesh2D_edge", edges_2d)
        ncfile.createDimension("nMesh2D_face", faces_2d)
        ncfile.createDimension("nMesh2D_node", nodes_2d)

    def set_1dnetwork(self, ncfile, data):

         # geometry
        #ntw = ncfile.createVariable("network1D", "u4", ())
        ntw = ncfile.createVariable("network1D", "i4", ())
        ntw.cf_role = 'mesh_topology'
        ntw.edge_dimension = 'nNetworkBranches'
        ntw.edge_geometry = 'network1D_geometry'
        ntw.edge_node_connectivity = 'network1D_edge_nodes'
        ntw.long_name = "Network topology"
        ntw.node_coordinates = 'network1D_nodes_x network1D_nodes_y'
        ntw.node_dimension = 'nNetworkNodes'
        ntw.topology_dimension = 1
        ntw.node_ids = "network_node_ids"
        ntw.node_long_names = "network_nodes_long_names"
        ntw.branch_ids = "network_branch_ids"
        ntw.branch_long_names = "network_branch_long_names"
        ntw.branch_lengths = "network_branch_lengths"

        ntw_nodes_id = ncfile.createVariable("network1D_node_id", "c", ("nNetworkNodes", "idstrlength"))
        ntw_nodes_id.standard_name = 'network1D_node_id_name'
        ntw_nodes_id.long_name = "The identification name of the node"
        ntw_nodes_id[:] = data["node_ids"]

        ntw_nodes_x = ncfile.createVariable("network1D_nodes_x", "f8", "nNetworkNodes")
        ntw_nodes_x.standard_name = 'projection_x_coordinate'
        ntw_nodes_x.long_name = "x coordinates of the network connection nodes"
        ntw_nodes_x.units = 'm'
        ntw_nodes_x[:] = data["node_x"]

        ntw_nodes_y = ncfile.createVariable("network1D_nodes_y", "f8", "nNetworkNodes")
        ntw_nodes_y.standard_name = 'projection_y_coordinate'
        ntw_nodes_y.long_name = "y coordinates of the network connection nodes"
        ntw_nodes_y.units = 'm'
        ntw_nodes_y[:] = data["node_y"]

        ntw_edge_node = ncfile.createVariable("network1D_edge_nodes", "i4", ("nMesh1DEdges", "Two"))
        ntw_edge_node.cf_role = 'edge_node_connectivity'
        ntw_edge_node.long_name = 'start and end nodes of each branch in the network'
        ntw_edge_node.start_index = 1
        ntw_edge_node[:] = data["edge_node"]

        ntw_geom = ncfile.createVariable("network1D_geometry", "i4", ())
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

        ntw_geom_x[:] = data["geom_x"]
        ntw_geom_y[:] = data["geom_y"]

        # mesh1D

        mesh1d = ncfile.createVariable("mesh1D", "i4", ())
        mesh1d.cf_role = 'mesh_topology'
        mesh1d.coordinate_space = 'network1D'
        mesh1d.edge_dimension = 'nmesh1DEdges'
        mesh1d.edge_node_connectivity = 'mesh1D_edge_nodes'
        mesh1d.long_name = "Mesh 1D"
        mesh1d.node_coordinates = 'mesh1D_nodes_branch_id mesh1D_nodes_branch_offset'
        mesh1d.node_dimension = 'nmesh1DNodes'
        mesh1d.topology_dimension = 1

        mesh1d_branch_id_name = ncfile.createVariable("network1D_branch_id", "c", ("nNetworkBranches", "idstrlength"))
        mesh1d_branch_id_name.standard_name = 'network1D_branch_id_name'
        mesh1d_branch_id_name.long_name = "The identification name of the branch"
        mesh1d_branch_id_name[:] = data["branch_names"]

        mesh1d_branch_id = ncfile.createVariable("mesh1D_nodes_branch_id", "i4", "nNetworkBranches")
        mesh1d_branch_id.cf_role = 'feature_index'
        mesh1d_branch_id.long_name = 'number of branch on which node is located'
        mesh1d_branch_id[:] = data["branch_ids"]

        mesh1d_point_branch_id = ncfile.createVariable("network1D_branch_id", "i4", ("nNetworkBranches", "idstrlength"))
        mesh1d_point_branch_id.standard_name = 'network1D_branch_id_name'
        mesh1d_point_branch_id.long_name = "The identification name of the branch"
        mesh1d_point_branch_id[:] = data["point_branch_id"]

        mesh1d_point_branch_offset = ncfile.createVariable("1dmesh_nodes_branch_offset", "c", ("nNetworkBranches", "idstrlength"))
        mesh1d_point_branch_offset.standard_name = 'network1D_branch_id_name'
        mesh1d_point_branch_offset.long_name = "The identification name of the branch"
        mesh1d_point_branch_offset[:] = data["point_branch_offset"]

        mesh1d_geom_offset = ncfile.createVariable("mesh1D_nodes_branch_offset", "f8", "nMesh1DNodes")
        mesh1d_geom_offset.cf_role = 'coordinate_on_feature'
        mesh1d_geom_offset.long_name = 'offset along the branch at which the node is located'
        mesh1d_geom_offset.units = 'm'
        mesh1d_geom_offset[0] = 0.
        i = 0
        for key in self.model.connections.keys():
            try:
                mesh1d_geom_offset[i] = self.model.connections[key][8]
            except:
                # print("Empty or not a number in a cell")
                mesh1d_geom_offset[i] = 1.
            i += 1

        # END OF THE NTWORK WRITER
        return True

    # set 2d mesh data to netcdf file
    def set_2dmesh(self, ncfile, data_2dmesh):

        mesh2d = ncfile.createVariable("Mesh2D", "i4", ())
        mesh2d.cf_role = 'mesh_topology'
        mesh2d.edge_coordinates = 'Mesh2D_edge_x Mesh2D_edge_y'
        mesh2d.edge_dimension = 'nMesh2D_edge'
        mesh2d.edge_face_connectivity = 'Mesh2D_edge_faces'
        mesh2d.edge_node_connectivity = 'Mesh2D_edge_nodes'
        #mesh2d.face_coordinates = 'Mesh2D_face_x Mesh2D_face_y'
        #mesh2d.face_dimension = 'nMesh2D_face'
        #mesh2d.face_edge_connectivity = 'Mesh2D_face_edges'
        #mesh2d.face_face_connectivity = 'Mesh2D_face_face'
        mesh2d.face_node_connectivity = 'Mesh2D_face_nodes'
        mesh2d.long_name = "Mesh 2D"
        mesh2d.max_face_nodes_dimension = 'max_nMeshFaceNodes'
        mesh2d.node_coordinates = 'Mesh2D_node_x Mesh2D_node_y'
        mesh2d.node_dimension = 'nMesh2D_node'
        mesh2d.topology_dimension = 2

        mesh2d_x = ncfile.createVariable("Mesh2D_node_x", "f8", ("nMesh2D_node"))
        mesh2d_y = ncfile.createVariable("Mesh2D_node_y", "f8", ("nMesh2D_node"))
        mesh2d_x.standard_name = 'projection_x_coordinate'
        mesh2d_x.units = 'm'
        mesh2d_y.standard_name = 'projection_y_coordinate'
        mesh2d_y.units = 'm'
        mesh2d_x[:] = data_2dmesh["node_x"]
        mesh2d_y[:] = data_2dmesh["node_y"]

        mesh2d_xu = ncfile.createVariable("Mesh2D_edge_x", "f8", ("nMesh2D_edge"))
        mesh2d_yu = ncfile.createVariable("Mesh2D_edge_y", "f8", ("nMesh2D_edge"))
        mesh2d_xu.standard_name = 'projection_x_coordinate'
        mesh2d_xu.units = 'm'
        mesh2d_yu.standard_name = 'projection_y_coordinate'
        mesh2d_yu.units = 'm'
        mesh2d_xu[:] = data_2dmesh["edge_x"]
        mesh2d_yu[:] = data_2dmesh["edge_y"]

        mesh2d_en = ncfile.createVariable("Mesh2D_edge_nodes", "i4", ("nMesh2D_edge", "Two"))
        mesh2d_en.cf_role = 'edge_node_connectivity'
        mesh2d_en.long_name = 'maps every edge to the two nodes that it connects'
        mesh2d_en.start_index = 1
        mesh2d_en[:] = data_2dmesh["edge_node"]

        mesh2d_fn = ncfile.createVariable("Mesh2D_face_nodes", "i4", ("nMesh2D_face", "max_nMesh2D_face_nodes"), fill_value=0)
        mesh2d_fn.cf_role = 'face_node_connectivity'
        mesh2d_fn.long_name = 'maps every face to the nodes that it defines'
        mesh2d_fn.start_index = 1
        mesh2d_fn[:] = data_2dmesh["face_node"]

        #cm = ncfile.createVariable("composite_mesh", "u4", ())
        #cm.cf_role = 'mesh_topology_parent'
        #cm.meshes= 'mesh1D mesh2D'
        #cm.mesh_contact = 'link1d2d'

        #link1d2d = ncfile.createVariable("link1d2d", "u4", ("nlinks_1d2d", "Two"))
        #link1d2d.cf_role = 'mesh_topology_contact'
        #link1d2d.contact= 'mesh1D:node mesh2D:face'
        #link1d2d.start_index = 1
        #link1d2d[:,:] = None

    # generate sewer system
    def generate_networkdata(self):
        networkdata = {}
        networkdata["node_ids"] = []
        networkdata["node_x"] = []
        networkdata["node_y"] = []
        networkdata["geom_x"] = []
        networkdata["geom_y"] = []
        networkdata["branch_ids"] = []
        networkdata["branch_names"] = []
        networkdata["edge_node"] = []

        # Temporary dictionary to store the id number of the nodes and branches
        node_order = OrderedDict()
        con_order = OrderedDict()

        i = 0
        for key in self.model.nodes.keys():
            networkdata["node_ids"].append(self.str2chars(self.model.nodes[key][0],self.idstrlength))
            networkdata["node_x"].append(self.model.nodes[key][3])
            networkdata["node_y"].append(self.model.nodes[key][4])
            networkdata["geom_x"].append(self.model.nodes[key][3])
            networkdata["geom_y"].append(self.model.nodes[key][4])
            node_order[key] = i + 1
            i += 1

        i = 0
        for key in self.model.connections.keys():
            con_order[key] = i
            networkdata["branch_ids"].append(con_order[self.model.connections[key][0]])
            networkdata["branch_names"].append(self.str2chars(self.model.connections[key][0],self.idstrlength))
            i += 1

        for key in self.model.connections.keys():
            node1 = node_order[self.model.connections[key][1]]
            node2 = node_order[self.model.connections[key][2]]
            networkdata["edge_node"].append([node1, node2])

        return networkdata

    # generate a street grid based on the manholes
    def generate_2dmesh_data(self):
        grid = {}
        rasterSize = 10.0
        margin = 5.0
        minX = sys.float_info.max
        minY = sys.float_info.max
        maxX = sys.float_info.min
        maxY = sys.float_info.min

        for keyvalue in self.model.nodes.items():
            value = keyvalue[1]
            x = float(value[3])
            y = float(value[4])
            if x < minX: minX = x
            if y < minY: minY = y
            if x > maxX: maxX = x
            if y > maxY: maxY = y

        minX = int(round(minX -margin))
        minY = int(round(minY -margin))
        maxX = int(round(maxX -margin))
        maxY = int(round(maxY -margin))
        xElements = range(minX, maxX, int(rasterSize))
        yElements = range(minY, maxY, int(rasterSize))
        n_xElements = len(xElements)
        n_yElements = len(yElements)
        n_nodes = n_xElements * n_yElements

        grid["node_x"] = []
        grid["node_y"] = []
        grid["edge_node"] = []
        grid["edge_x"] = []
        grid["edge_y"] = []
        grid["face_node"] = []
        grid["face_edge"] = []

        for y in yElements:
            grid["node_x"].extend(xElements)
            grid["node_y"].extend([y]*n_xElements)

        iy = 1
        while iy < n_yElements:
            y0 = yElements[iy-1]
            y1 = yElements[iy]
            ix = 1
            while ix < n_xElements:
                x0 = xElements[ix-1]
                x1 = xElements[ix]
                inode0 = (((iy-1) * n_xElements) + ix)
                inode1 = inode0 + 1
                inode2 = inode1 + n_yElements
                inode3 = inode0 + n_yElements

                if ix == 1:
                    grid["edge_node"].extend([inode0,inode3])
                    grid["edge_x"].append(x0)
                    grid["edge_y"].append(y0 + (0.5 * rasterSize))

                grid["edge_node"].extend([inode0,inode1,inode1, inode2])
                grid["edge_x"].extend([x0 + (0.5 * rasterSize),x1])
                grid["edge_y"].extend([y0, y0 + (0.5 * rasterSize)])
                grid["face_node"].append([inode0, inode1, inode2, inode3])
                ix += 1
            iy += 1

        # finish edges on top of the raster
        ix = 1
        while ix < n_xElements:
            x0 = xElements[ix-1]
            x1 = xElements[ix]
            inode0 = (iy* n_xElements) + ix
            inode1 = inode0 + 1
            grid["edge_node"].extend([inode0, inode1])
            grid["edge_x"].append(x0 + (0.5 * rasterSize))
            grid["edge_y"].append(y1)
            ix += 1

        return grid

    # generate a street grid based on the manholes and street grid
    def generate_1d2dlinks(self):
        return True

    def str2chars(self,str,size):
        chars = list(str)
        if len(chars) > size:
            chars = chars[:size]
        elif len(chars) < size:
            chars.extend(list(' '* (size - len(chars))))
        return chars


