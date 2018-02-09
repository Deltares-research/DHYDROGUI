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
        ncfile.createDimension("nnetworkBranches", len(data["branch_ids"]))
        ncfile.createDimension("nnetworkNodes", len(data["node_ids"]))
        ncfile.createDimension("nnetworkGeometry", len(data["node_ids"]))
        ncfile.createDimension("idstrlength", self.idstrlength)
        ncfile.createDimension("longstrlength", self.longstrlength)
        ncfile.createDimension("n1dmeshEdges", len(data["edge_node"]))
        ncfile.createDimension("n1dmeshNodes", len(data["point_branch_id"]))
        ncfile.createDimension("Two", 2)

    def init_2dmesh(self, ncfile, data_2dmesh):

        # dimensions 2d mesh
        edges_2d = len(data_2dmesh["edge_x"])
        faces_2d = len(data_2dmesh["face_node"])
        nodes_2d = len(data_2dmesh["node_x"])

        ncfile.createDimension("max_nmesh2d_face_nodes", 4)
        ncfile.createDimension("nmesh2d_edge", edges_2d)
        ncfile.createDimension("nmesh2d_face", faces_2d)
        ncfile.createDimension("nmesh2d_node", nodes_2d)

    def set_1dnetwork(self, ncfile, data):

        # geometry
        #ntw = ncfile.createVariable("network1D", "u4", ())
        ntw = ncfile.createVariable("network", "i4", ())
        ntw.cf_role = 'mesh_topology'
        ntw.edge_dimension = 'nnetworkNodes'
        ntw.edge_geometry = 'network_geometry'
        ntw.edge_node_connectivity = 'network_edge_nodes'
        ntw.long_name = "Network topology"
        ntw.node_coordinates = 'network_nodes_x network_nodes_y'
        ntw.node_dimension = 'nnetworkNodes'
        ntw.topology_dimension = 1
        ntw.node_ids = "network_node_ids"
        ntw.node_long_names = "network_nodes_long_names"
        ntw.branch_ids = "network_branch_ids"
        ntw.branch_long_names = "network_branch_long_names"
        ntw.branch_lengths = "network_branch_lengths"

        ntw_nodes_id = ncfile.createVariable("network_node_ids", "c", ("nnetworkNodes", "idstrlength"))
        ntw_nodes_id.standard_name = 'network_node_ids'
        ntw_nodes_id.long_name = "The identification name of the node"
        ntw_nodes_id[:] = data["node_ids"]

        ntw_nodes_longname = ncfile.createVariable("network_node_longnames", "c", ("nnetworkNodes", "longstrlength"))
        ntw_nodes_longname.standard_name = 'network_node_longname'
        ntw_nodes_longname.long_name = "The long name of the node"
        ntw_nodes_longname[:] = data["node_longnames"]

        ntw_nodes_x = ncfile.createVariable("network_nodes_x", "f8", "nnetworkNodes")
        ntw_nodes_x.standard_name = 'projection_x_coordinate'
        ntw_nodes_x.long_name = "x coordinates of the network connection nodes"
        ntw_nodes_x.units = 'm'
        ntw_nodes_x[:] = data["node_x"]

        ntw_nodes_y = ncfile.createVariable("network_nodes_y", "f8", "nnetworkNodes")
        ntw_nodes_y.standard_name = 'projection_y_coordinate'
        ntw_nodes_y.long_name = "y coordinates of the network connection nodes"
        ntw_nodes_y.units = 'm'
        ntw_nodes_y[:] = data["node_y"]

        ntw_edge_node = ncfile.createVariable("network_edge_nodes", "i4", ("n1dmeshEdges", "Two"))
        ntw_edge_node.cf_role = 'edge_node_connectivity'
        ntw_edge_node.long_name = 'start and end nodes of each branch in the network'
        ntw_edge_node.start_index = 1
        ntw_edge_node[:] = data["edge_node"]

        ntw_geom = ncfile.createVariable("network_geometry", "i4", ())
        ntw_geom.geometry_type = 'multiline'
        ntw_geom.long_name = "1D Geometry"
        ntw_geom.node_count = "nnetworkGeometry"
        ntw_geom.part_node_count = 'network_part_node_count'
        ntw_geom.node_coordinates = 'network_geom_x network_geom_y'

        ntw_geom_x = ncfile.createVariable("network_geom_x", "f8", ("nnetworkGeometry"))
        ntw_geom_x.standard_name = 'projection_x_coordinate'
        ntw_geom_x.units = 'm'
        ntw_geom_x.cf_role = "geometry_x_node"
        ntw_geom_x.long_name = 'x coordinates of the branch geometries'

        ntw_geom_y = ncfile.createVariable("network_geom_y", "f8", ("nnetworkGeometry"))
        ntw_geom_y.standard_name = 'projection_y_coordinate'
        ntw_geom_y.units = 'm'
        ntw_geom_y.cf_role = "geometry_y_node"
        ntw_geom_y.long_name = 'y coordinates of the branch geometries'

        ntw_geom_x[:] = data["geom_x"]
        ntw_geom_y[:] = data["geom_y"]

        # mesh1D

        mesh1d = ncfile.createVariable("1dmesh", "i4", ())
        mesh1d.cf_role = 'mesh_topology'
        mesh1d.coordinate_space = 'network'
        mesh1d.edge_dimension = 'n1dmeshEdges'
        mesh1d.edge_node_connectivity = '1dmesh_edge_nodes'
        mesh1d.long_name = "1D Mesh"
        mesh1d.node_coordinates = '1dmesh_nodes_branch_id 1dmesh_nodes_branch_offset'
        mesh1d.node_dimension = 'n1dmeshNodes'
        mesh1d.topology_dimension = 1

        mesh1d_branch_id_name = ncfile.createVariable("network_branch_ids", "c", ("nnetworkBranches", "idstrlength"))
        mesh1d_branch_id_name.standard_name = 'network_branch_id_name'
        mesh1d_branch_id_name.long_name = "The identification name of the branch"
        mesh1d_branch_id_name[:] = data["branch_names"]

        mesh1d_branch_id_longname = ncfile.createVariable("network_branch_longnames", "c", ("nnetworkBranches", "longstrlength"))
        mesh1d_branch_id_longname.standard_name = 'network_branch_longname'
        mesh1d_branch_id_longname.long_name = "The long name of the branch"
        mesh1d_branch_id_longname[:] = data["branch_longnames"]

        mesh1d_branch_length = ncfile.createVariable("network_branch_lengths", "f8", "nnetworkBranches")
        mesh1d_branch_length.standard_name = 'network_branch_length'
        mesh1d_branch_length.long_name = "The calculation length of the branch"
        mesh1d_branch_length[:] = data["branch_length"]

        mesh1d_point_branch_id = ncfile.createVariable("1dmesh_nodes_branch_id", "i4", "n1dmeshNodes")
        mesh1d_point_branch_id.standard_name = 'network calculation point branch id'
        mesh1d_point_branch_id.long_name = "The identification the branch of the calculation point"
        mesh1d_point_branch_id[:] = data["point_branch_id"]

        mesh1d_point_branch_offset = ncfile.createVariable("1dmesh_nodes_branch_offset", "f8", "n1dmeshNodes")
        mesh1d_point_branch_offset.standard_name = 'network calculation point branch offset'
        mesh1d_point_branch_offset.long_name = "The offset of the calculation point on the branch"
        mesh1d_point_branch_offset[:] = data["point_branch_offset"]

        mesh1d_geom_offset = ncfile.createVariable("mesh1D_nodes_branch_offset", "f8", "n1dmeshNodes")
        mesh1d_geom_offset.cf_role = 'coordinate_on_feature'
        mesh1d_geom_offset.long_name = 'offset along the branch at which the node is located'
        mesh1d_geom_offset.units = 'm'
        mesh1d_geom_offset[:] = data["point_branch_offset"]

        # END OF THE NTWORK WRITER
        return True

    # set 2d mesh data to netcdf file
    def set_2dmesh(self, ncfile, data_2dmesh):

        mesh2d = ncfile.createVariable("mesh2d", "i4", ())
        mesh2d.long_name = "Topology data of 2D network"
        mesh2d.topology_dimension = 2
        mesh2d.cf_role = 'mesh_topology'
        mesh2d.node_coordinates = 'mesh2d_node_x mesh2d_node_y'
        mesh2d.node_dimension = 'nmesh2d_node'
        mesh2d.edge_coordinates = 'mesh2d_edge_x mesh2d_edge_y'
        mesh2d.edge_dimension = 'nmesh2D_edge'
        mesh2d.edge_node_connectivity = 'mesh2d_edge_nodes'
        mesh2d.face_node_connectivity = 'mesh2d_face_nodes'
        mesh2d.max_face_nodes_dimension = 'max_nmesh2d_face_nodes'
        mesh2d.face_dimension = "nmesh2d_face"
        mesh2d.edge_face_connectivity = "mesh2d_edge_faces"
        mesh2d.face_coordinates = "mesh2d_face_x mesh2d_face_y"

        mesh2d_x = ncfile.createVariable("mesh2d_node_x", "f8", "nmesh2d_node")
        mesh2d_y = ncfile.createVariable("mesh2d_node_y", "f8", "nmesh2d_node")
        mesh2d_x.standard_name = 'projection_x_coordinate'
        mesh2d_x.units = 'm'
        mesh2d_y.standard_name = 'projection_y_coordinate'
        mesh2d_y.units = 'm'
        mesh2d_x[:] = data_2dmesh["node_x"]
        mesh2d_y[:] = data_2dmesh["node_y"]

        mesh2d_xu = ncfile.createVariable("mesh2d_edge_x", "f8", "nmesh2d_edge")
        mesh2d_yu = ncfile.createVariable("mesh2d_edge_y", "f8", "nmesh2d_edge")
        mesh2d_xu.standard_name = 'projection_x_coordinate'
        mesh2d_xu.units = 'm'
        mesh2d_yu.standard_name = 'projection_y_coordinate'
        mesh2d_yu.units = 'm'
        mesh2d_xu[:] = data_2dmesh["edge_x"]
        mesh2d_yu[:] = data_2dmesh["edge_y"]

        mesh2d_en = ncfile.createVariable("mesh2d_edge_nodes", "i4", ("nmesh2d_edge", "Two"))
        mesh2d_en.cf_role = 'edge_node_connectivity'
        mesh2d_en.long_name = 'maps every edge to the two nodes that it connects'
        mesh2d_en.start_index = 1
        mesh2d_en[:] = data_2dmesh["edge_node"]

        mesh2d_fn = ncfile.createVariable("mesh2d_face_nodes", "i4", ("nmesh2d_face", "max_nmesh2d_face_nodes"), fill_value=0)
        mesh2d_fn.cf_role = 'face_node_connectivity'
        mesh2d_fn.long_name = 'maps every face to the nodes that it defines'
        mesh2d_fn.start_index = 1
        mesh2d_fn[:] = data_2dmesh["face_node"]

        #mesh2d_edge_faces = ncfile.createVariable("mesh2d_edge_faces", "i4", ("nmesh2d_edge", "Two"), fill_value=-1)
		#mesh2d_edge_faces.cf_role = "edge_face_connectivity"
		#mesh2d_edge_faces.long_name = "Mapping from every edge to the two faces that it separates"
		#mesh2d_edge_faces.start_index = 1
        #mesh2d_edge_faces[:] = data_2dmesh["edge_faces"]

        mesh2d_face_x = ncfile.createVariable("mesh2d_face_x", "f8", "nmesh2d_face")
        mesh2d_face_x.units = "m"
        mesh2d_face_x.standard_name = "projection_x_coordinate"
        mesh2d_face_x.long_name = "Characteristic x-coordinate of mesh face"
        mesh2d_face_x.mesh = "mesh2d"
        mesh2d_face_x.location = "face"
        #mesh2d_face_x.bounds = "mesh2d_face_x_bnd"
        mesh2d_face_x[:] = data_2dmesh["face_x"]

        mesh2d_face_y = ncfile.createVariable("mesh2d_face_y", "f8", "nmesh2d_face")
        mesh2d_face_y.units = "m"
        mesh2d_face_y.standard_name = "projection_y_coordinate"
        mesh2d_face_y.long_name = "Characteristic y-coordinate of mesh face"
        mesh2d_face_y.mesh = "mesh2d"
        mesh2d_face_y.location = "face"
        #mesh2d_face_y.bounds = "mesh2d_face_y_bnd"
        mesh2d_face_y[:] = data_2dmesh["face_y"]

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
        networkdata["point_branch_id"] = []
        networkdata["point_branch_offset"] = []
        networkdata["node_longnames"] = []
        networkdata["branch_longnames"] = []
        networkdata["branch_length"] = []

        # Temporary dictionary to store the id number of the nodes and branches
        node_order = OrderedDict()
        con_order = OrderedDict()

        i = 0
        for key, value in self.model.nodes.items():
            networkdata["node_ids"].append(self.str2chars(value[0],self.idstrlength))
            networkdata["node_longnames"].append(self.str2chars(str(value[0]) + " longname",self.longstrlength))
            networkdata["node_x"].append(value[3])
            networkdata["node_y"].append(value[4])
            networkdata["geom_x"].append(value[3])
            networkdata["geom_y"].append(value[4])
            node_order[key] = i + 1
            i += 1

        i = 0
        for key,value in self.model.connections.items():
            con_order[key] = i
            networkdata["branch_ids"].append(i)
            networkdata["branch_names"].append(self.str2chars(key,self.idstrlength))
            networkdata["branch_longnames"].append(self.str2chars(str(key) + " longname",self.longstrlength))

            #2 claculation points - start & end branch
            networkdata["point_branch_id"].extend([i]*2)
            networkdata["point_branch_offset"].append(0.0)
            try:
                length = float(value[8])
                networkdata["point_branch_offset"].append(length)
                networkdata["branch_length"].append(length)
            except:
                # print("Empty or not a number in a cell")
                networkdata["point_branch_offset"].append(1.0)
                networkdata["branch_length"].append(1.0)

            node1 = node_order[value[1]]
            node2 = node_order[value[2]]
            networkdata["edge_node"].append([node1, node2])
            i += 1

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
        grid["face_x"] = []
        grid["face_y"] = []
        grid["edge_faces"] = []

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
                grid["face_x"].append(x0 + (0.5 * rasterSize))
                grid["face_y"].append(y0 + (0.5 * rasterSize))
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

        # fill edge - face

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


