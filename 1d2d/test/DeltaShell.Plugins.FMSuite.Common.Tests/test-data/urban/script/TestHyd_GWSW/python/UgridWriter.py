# coding: latin-1
import os, sys, math
from netCDF4 import Dataset
from collections import OrderedDict
from datetime import *


class UgridWriter:
    """Writer for FM files"""
    idstrlength = 40
    longstrlength = 80

    def __init__(self, model):
        self.model = model

    def write(self, dirPath, outputDir, gridFileAvailable = False, generate2DGrid = True):  # write ugrid file from GWSW model
        ncfile = self.create_netcdf(dirPath, outputDir, "sewer_system")

        print("start generating 1d data")
        networkdata = self.generate_networkdata()

        if gridFileAvailable:
            print("using import 2d data")
            data_2dmesh = self.model.grid
        elif generate2DGrid:
            print("start generating 2d data")
            data_2dmesh = self.generate_2dmesh_data()
        else:
            print("start generating 2d DUMMY data")
            data_2dmesh = self.generate_dummy2dmesh_2columnsrightside()


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
        ncfile.Conventions = "CF-1.8 UGRID-1.0"
        ncfile.history = "Created on {} D-Flow 1D, D-Flow FM".format(datetime.now())
        ncfile.institution = "Deltares"
        ncfile.reference = "http://www.deltares.nl"
        ncfile.source = "Python script to prepare D-Flow FM 1D network"

        return ncfile

    def init_1dnetwork(self, ncfile, data):

        # dimensions of the network

        ncfile.createDimension("time", None)
        ncfile.createDimension("nnetwork_branches", len(data["branch_ids"]))
        ncfile.createDimension("nnetwork_nodes", len(data["node_ids"]))
        ncfile.createDimension("nnetwork_geometry", len(data["geom_x"]))
        ncfile.createDimension("idstrlength", self.idstrlength)
        ncfile.createDimension("longstrlength", self.longstrlength)
        ncfile.createDimension("nmesh1d_edges", len(data["edge_node"]))
        ncfile.createDimension("nmesh1d_nodes", len(data["point_branch_id"]))
        ncfile.createDimension("Two", 2)

    def init_2dmesh(self, ncfile, data_2dmesh):

        # dimensions 2d mesh
        edges_2d = len(data_2dmesh["edge_x"])
        faces_2d = len(data_2dmesh["face_node"])
        nodes_2d = len(data_2dmesh["node_x"])

        ncfile.createDimension("max_nmesh2d_face_nodes", 4)
        ncfile.createDimension("nmesh2d_edges", edges_2d)
        ncfile.createDimension("nmesh2d_faces", faces_2d)
        ncfile.createDimension("nmesh2d_nodes", nodes_2d)

    def set_1dnetwork(self, ncfile, data):

        # geometry
        #ntw = ncfile.createVariable("network1D", "u4", ())
        ntw = ncfile.createVariable("network", "i4", ())
        ntw.cf_role = 'mesh_topology'
        ntw.edge_dimension = 'nnetwork_branches'
        ntw.edge_geometry = 'network_geometry'
        ntw.edge_node_connectivity = 'network_edge_nodes'
        ntw.long_name = "Network topology"
        ntw.node_coordinates = 'network_node_x network_node_y'
        ntw.node_dimension = 'nnetwork_nodes'
        ntw.topology_dimension = 1
        ntw.node_ids = "network_node_ids"
        ntw.node_long_names = "network_node_long_names"
        ntw.branch_ids = "network_branch_ids"
        ntw.branch_long_names = "network_branch_long_names"
        ntw.branch_lengths = "network_branch_lengths"
        ntw.branch_order = "network_branch_order"

        ntw_node_id = ncfile.createVariable("network_node_ids", "c", ("nnetwork_nodes", "idstrlength"))
        ntw_node_id.standard_name = 'network_node_ids'
        ntw_node_id.long_name = "The identification name of the node"
        ntw_node_id.mesh = 'network1D'
        ntw_node_id[:] = data["node_ids"]

        ntw_node_longname = ncfile.createVariable("network_node_long_names", "c", ("nnetwork_nodes", "longstrlength"))
        ntw_node_longname.standard_name = 'network_node_longname'
        ntw_node_longname.long_name = "The long name of the node"
        ntw_node_longname.mesh = 'network1D'
        ntw_node_longname[:] = data["node_longnames"]

        ntw_node_x = ncfile.createVariable("network_node_x", "f8", "nnetwork_nodes")
        ntw_node_x.standard_name = 'projection_x_coordinate'
        ntw_node_x.long_name = "x coordinates of the network connection nodes"
        ntw_node_x.units = 'm'
        ntw_node_x[:] = data["node_x"]

        ntw_node_y = ncfile.createVariable("network_node_y", "f8", "nnetwork_nodes")
        ntw_node_y.standard_name = 'projection_y_coordinate'
        ntw_node_y.long_name = "y coordinates of the network connection nodes"
        ntw_node_y.units = 'm'
        ntw_node_y[:] = data["node_y"]

        ntw_branch_id_name = ncfile.createVariable("network_branch_ids", "c", ("nnetwork_branches", "idstrlength"))
        ntw_branch_id_name.standard_name = 'network_branch_id_name'
        ntw_branch_id_name.long_name = "The identification name of the branch"
        ntw_branch_id_name[:] = data["branch_names"]

        ntw_branch_id_longname = ncfile.createVariable("network_branch_long_names", "c", ("nnetwork_branches", "longstrlength"))
        ntw_branch_id_longname.standard_name = 'network_branch_longname'
        ntw_branch_id_longname.long_name = "The long name of the branch"
        ntw_branch_id_longname[:] = data["branch_longnames"]

        ntw_branch_length = ncfile.createVariable("network_branch_lengths", "f8", "nnetwork_branches")
        ntw_branch_length.standard_name = 'network_branch_length'
        ntw_branch_length.long_name = "The calculation length of the branch"
        ntw_branch_length[:] = data["branch_length"]

        ntw_branch_order = ncfile.createVariable("network_branch_order", "i4", "nnetwork_branches")
        ntw_branch_order.standard_name = 'network branch order'
        ntw_branch_order.long_name = "The order of the branches for interpolation"
        ntw_branch_order[:] = data["branch_order"]

        ntw_edge_node = ncfile.createVariable("network_edge_nodes", "i4", ("nmesh1d_edges", "Two"))
        ntw_edge_node.cf_role = 'edge_node_connectivity'
        ntw_edge_node.long_name = 'start and end nodes of each branch in the network'
        ntw_edge_node.start_index = 1
        ntw_edge_node[:] = data["edge_node"]

        ntw_geom = ncfile.createVariable("network_geometry", "i4", ())
        ntw_geom.geometry_type = 'multiline'
        ntw_geom.long_name = "1D Geometry"
        ntw_geom.node_count = "nnetwork_geometry"
        ntw_geom.part_node_count = 'network_part_node_count'
        ntw_geom.node_coordinates = 'network_geom_x network_geom_y'

        ntw_geom_x = ncfile.createVariable("network_geom_x", "f8", ("nnetwork_geometry"))
        ntw_geom_x.standard_name = 'projection_x_coordinate'
        ntw_geom_x.units = 'm'
        ntw_geom_x.cf_role = "geometry_x_node"
        ntw_geom_x.long_name = 'x coordinates of the branch geometries'

        ntw_geom_y = ncfile.createVariable("network_geom_y", "f8", ("nnetwork_geometry"))
        ntw_geom_y.standard_name = 'projection_y_coordinate'
        ntw_geom_y.units = 'm'
        ntw_geom_y.cf_role = "geometry_y_node"
        ntw_geom_y.long_name = 'y coordinates of the branch geometries'

        ntw_geom_x[:] = data["geom_x"]
        ntw_geom_y[:] = data["geom_y"]

        # mesh1D

        mesh1d = ncfile.createVariable("mesh1d", "i4", ())
        mesh1d.cf_role = 'mesh_topology'
        mesh1d.coordinate_space = 'network'
        mesh1d.edge_dimension = 'nmesh1d_edges'
        mesh1d.edge_node_connectivity = 'mesh1d_edge_nodes'
        mesh1d.long_name = "1D Mesh"
        mesh1d.node_coordinates = 'mesh1d_nodes_branch_id mesh1d_nodes_branch_offset'
        mesh1d.node_dimension = 'nmesh1d_nodes'
        mesh1d.node_ids = "mesh1d_node_ids"
        mesh1d.node_long_names = "mesh1d_node_long_names"
        mesh1d.topology_dimension = 1

        mesh1d_node_count = ncfile.createVariable("network_part_node_count", "i4", "nnetwork_branches")
        mesh1d_node_count.standard_name = 'network part node count'
        mesh1d_node_count.long_name = "The number of nodes in per branch"
        mesh1d_node_count[:] = data["branch_ngeometrypoints"]

        mesh1d_node_id = ncfile.createVariable("mesh1d_node_ids", "c", ("nmesh1d_nodes", "idstrlength"))
        mesh1d_node_id.standard_name = 'mesh1d_node_ids'
        mesh1d_node_id.long_name = "The name of the calculation points"
        mesh1d_node_id.mesh = 'mesh1d'
        mesh1d_node_id[:] = data["point_ids"]

        mesh1d_node_longname = ncfile.createVariable("mesh1d_node_long_names", "c", ("nmesh1d_nodes", "longstrlength"))
        mesh1d_node_longname.standard_name = 'mesh1d_node_longname'
        mesh1d_node_longname.long_name = "The long name of calculation points"
        mesh1d_node_longname.mesh = 'mesh1d'
        mesh1d_node_longname[:] = data["point_longnames"]

        mesh1d_edge_node = ncfile.createVariable("mesh1d_edge_nodes", "i4", ("nmesh1d_edges", "Two"))
        mesh1d_edge_node.cf_role = 'edge_node_connectivity'
        mesh1d_edge_node.long_name = 'start and end nodes of each branch in the 1d mesh'
        mesh1d_edge_node.start_index = 1
        mesh1d_edge_node[:] = data["edge_point"]

        mesh1d_point_branch_id = ncfile.createVariable("mesh1d_nodes_branch_id", "i4", "nmesh1d_nodes")
        mesh1d_point_branch_id.standard_name = 'network calculation point branch id'
        mesh1d_point_branch_id.long_name = "The identification the branch of the calculation point"
        mesh1d_point_branch_id.start_index = 1
        mesh1d_point_branch_id[:] = data["point_branch_id"]

        mesh1d_point_branch_offset = ncfile.createVariable("mesh1d_nodes_branch_offset", "f8", "nmesh1d_nodes")
        mesh1d_point_branch_offset.standard_name = 'network calculation point branch offset'
        mesh1d_point_branch_offset.long_name = "The offset of the calculation point on the branch"
        mesh1d_point_branch_offset.start_index = 1
        mesh1d_point_branch_offset[:] = data["point_branch_offset"]

        # END OF THE NTWORK WRITER
        return True

    # set 2d mesh data to netcdf file
    def set_2dmesh(self, ncfile, data_2dmesh):

        mesh2d = ncfile.createVariable("mesh2d", "i4", ())
        mesh2d.long_name = "Topology data of 2D network"
        mesh2d.topology_dimension = 2
        mesh2d.cf_role = 'mesh_topology'
        mesh2d.node_coordinates = 'mesh2d_node_x mesh2d_node_y'
        mesh2d.node_dimension = 'nmesh2d_nodes'
        mesh2d.edge_coordinates = 'mesh2d_edge_x mesh2d_edge_y'
        mesh2d.edge_dimension = 'nmesh2d_edges'
        mesh2d.edge_node_connectivity = 'mesh2d_edge_nodes'
        mesh2d.face_node_connectivity = 'mesh2d_face_nodes'
        mesh2d.max_face_nodes_dimension = 'max_nmesh2d_face_nodes'
        mesh2d.face_dimension = "nmesh2d_faces"
        #mesh2d.edge_face_connectivity = "mesh2d_edge_faces"
        mesh2d.face_coordinates = "mesh2d_face_x mesh2d_face_y"

        mesh2d_x = ncfile.createVariable("mesh2d_node_x", "f8", "nmesh2d_nodes")
        mesh2d_y = ncfile.createVariable("mesh2d_node_y", "f8", "nmesh2d_nodes")
        mesh2d_x.standard_name = 'projection_x_coordinate'
        mesh2d_x.units = 'm'
        mesh2d_y.standard_name = 'projection_y_coordinate'
        mesh2d_y.units = 'm'
        mesh2d_x[:] = data_2dmesh["node_x"]
        mesh2d_y[:] = data_2dmesh["node_y"]

        mesh2d_xu = ncfile.createVariable("mesh2d_edge_x", "f8", "nmesh2d_edges")
        mesh2d_yu = ncfile.createVariable("mesh2d_edge_y", "f8", "nmesh2d_edges")
        mesh2d_xu.standard_name = 'projection_x_coordinate'
        mesh2d_xu.units = 'm'
        mesh2d_yu.standard_name = 'projection_y_coordinate'
        mesh2d_yu.units = 'm'
        mesh2d_xu[:] = data_2dmesh["edge_x"]
        mesh2d_yu[:] = data_2dmesh["edge_y"]

        mesh2d_en = ncfile.createVariable("mesh2d_edge_nodes", "i4", ("nmesh2d_edges", "Two"))
        mesh2d_en.cf_role = 'edge_node_connectivity'
        mesh2d_en.long_name = 'maps every edge to the two nodes that it connects'
        mesh2d_en.start_index = 1
        mesh2d_en[:] = data_2dmesh["edge_node"]

        mesh2d_fn = ncfile.createVariable("mesh2d_face_nodes", "i4", ("nmesh2d_faces", "max_nmesh2d_face_nodes"), fill_value=0)
        mesh2d_fn.cf_role = 'face_node_connectivity'
        mesh2d_fn.long_name = 'maps every face to the nodes that it defines'
        mesh2d_fn.start_index = 1
        mesh2d_fn[:] = data_2dmesh["face_node"]

        #mesh2d_edge_faces = ncfile.createVariable("mesh2d_edge_faces", "i4", ("nmesh2d_edge", "Two"), fill_value=-1)
      #mesh2d_edge_faces.cf_role = "edge_face_connectivity"
      #mesh2d_edge_faces.long_name = "Mapping from every edge to the two faces that it separates"
      #mesh2d_edge_faces.start_index = 1
        #mesh2d_edge_faces[:] = data_2dmesh["edge_faces"]

        mesh2d_face_x = ncfile.createVariable("mesh2d_face_x", "f8", "nmesh2d_faces")
        mesh2d_face_x.units = "m"
        mesh2d_face_x.standard_name = "projection_x_coordinate"
        mesh2d_face_x.long_name = "Characteristic x-coordinate of mesh face"
        mesh2d_face_x.mesh = "mesh2d"
        mesh2d_face_x.location = "face"
        #mesh2d_face_x.bounds = "mesh2d_face_x_bnd"
        mesh2d_face_x[:] = data_2dmesh["face_x"]

        mesh2d_face_y = ncfile.createVariable("mesh2d_face_y", "f8", "nmesh2d_faces")
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
        networkdata["point_ids"] = []
        networkdata["point_longnames"] = []
        networkdata["point_branch_id"] = []
        networkdata["point_branch_offset"] = []
        networkdata["edge_point"] = []
        networkdata["node_longnames"] = []
        networkdata["branch_longnames"] = []
        networkdata["branch_length"] = []
        networkdata["branch_order"] = []
        networkdata["branch_ngeometrypoints"] = []

        # Temporary dictionary to store the id number of the nodes and branches
        node_order = OrderedDict()
        con_order = OrderedDict()

        i = 0
        for key, value in self.model.nodes.items():
            networkdata["node_ids"].append(self.str2chars(value[0],self.idstrlength))
            networkdata["node_longnames"].append(self.str2chars(str(value[0]) + " longname",self.longstrlength))
            networkdata["node_x"].append(value[3])
            networkdata["node_y"].append(value[4])
            node_order[key] = i + 1
            i += 1

        i = 1
        for key,value in self.model.connections.items():
            con_order[key] = i
            networkdata["branch_ids"].append(i)
            networkdata["branch_names"].append(self.str2chars(key,self.idstrlength))
            networkdata["branch_longnames"].append(self.str2chars(str(key) + " longname",self.longstrlength))
            networkdata["branch_order"].append(-1)
            networkdata["branch_ngeometrypoints"].append(2)

            #2 claculation points - start & end branch
            networkdata["point_ids"].extend([self.str2chars('calc ' + key + ' begin',self.idstrlength),self.str2chars('calc ' + key + ' end',self.idstrlength)])
            networkdata["point_longnames"].extend([self.str2chars('calc ' + key + ' begin longname',self.longstrlength),self.str2chars('calc ' + key + ' end longname',self.longstrlength)])
            networkdata["point_branch_id"].extend([i]*2)
            networkdata["point_branch_offset"].append(0.0)
            i_edge = 2 * i
            networkdata["edge_point"].append([i_edge-1,i_edge])

            node1Id = value[1]
            node1 = self.model.nodes[node1Id]
            node1Index = node_order[node1Id]
            node2Id = value[2]
            node2 = self.model.nodes[node2Id]
            node2Index = node_order[node2Id]

            try:
                length = float(value[8])
                networkdata["point_branch_offset"].append(length)
                networkdata["branch_length"].append(length)
            except:
                # print("Empty or not a number in a cell")
                l = 1.0
                width = float(node2[3])-float(node1[3])
                height = float(node2[4])-float(node1[4])
                if width > 0.0 or height > 0.0:
                    l = round(math.sqrt(math.pow(width,2) + math.pow(height,2)))
                networkdata["point_branch_offset"].append(l)
                networkdata["branch_length"].append(l)

            #edge-nodes (
            networkdata["edge_node"].append([node1Index, node2Index])
            #node1
            networkdata["geom_x"].append(node1[3])
            networkdata["geom_y"].append(node1[4])
            #node2
            networkdata["geom_x"].append(node2[3])
            networkdata["geom_y"].append(node2[4])
            i += 1

        return networkdata

    # generate a street grid based on the manholes
    def generate_2dmesh_data(self):
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

        minX = int(round(minX - margin))
        minY = int(round(minY - margin))
        maxX = int(round(maxX + rasterSize + margin))
        maxY = int(round(maxY + rasterSize + margin))
        xElements = range(minX, maxX, int(rasterSize))
        yElements = range(minY, maxY, int(rasterSize))
        n_xElements = len(xElements)
        n_yElements = len(yElements)

        return self.get_2dmesh_grid(xElements, yElements, n_xElements, n_yElements, rasterSize)

    # generate dummy 2d data just right from the 1d area
    def generate_dummy2dmesh_2columnsrightside(self):
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

        minX = int(round(maxX + rasterSize + margin))
        minY = int(round(minY - margin))
        maxX = int(round(maxX + 3* rasterSize + margin))
        maxY = int(round(maxY + rasterSize + margin))
        xElements = range(minX, maxX, int(rasterSize))
        yElements = range(minY, maxY, int(rasterSize))
        n_xElements = len(xElements)
        n_yElements = len(yElements)

        return self.get_2dmesh_grid(xElements, yElements, n_xElements, n_yElements, rasterSize)

    # fills the 2d grid object
    def get_2dmesh_grid(self, xElements, yElements, n_xElements, n_yElements, rasterSize):
        grid = {}

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
                inode2 = inode1 + n_xElements
                inode3 = inode0 + n_xElements

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
            inode0 = ((iy-1)* n_xElements) + ix
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


