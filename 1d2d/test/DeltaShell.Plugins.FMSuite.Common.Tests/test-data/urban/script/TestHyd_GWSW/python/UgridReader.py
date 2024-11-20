# coding: latin-1
from netCDF4 import Dataset

class UgridReader:

    def __init__(self, model):
        self.model = model

    def ReadFile(self, gridFile):
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

        nc = Dataset(gridFile, mode='r')

        for var in nc.variables:
            if var.lower() == 'mesh2d':
                mesh2d = nc.variables[var]
                break

        if mesh2d is None:
            print('Variable Mesh2D has not been found in ' + gridFile)
            return

        for ncattr in mesh2d.ncattrs():
                print('\t\t%s:' % ncattr,\
                      repr(mesh2d.getncattr(ncattr)))

        # node_coordinates: 'mesh2d_node_x mesh2d_node_y'
        node_coordinates = mesh2d.getncattr('node_coordinates')
        x_y = node_coordinates.split(' ')
        grid["node_x"] = nc.variables[x_y[0]][:]
        grid["node_y"] = nc.variables[x_y[1]][:]

        # edge_node_connectivity: 'mesh2d_edge_nodes'  start_index=1
        edge_node_connectivity = mesh2d.getncattr('edge_node_connectivity')
        grid["edge_node"] = nc.variables[edge_node_connectivity][:]

        # edge_coordinates: 'mesh2d_edge_x mesh2d_edge_y'
        edge_coordinates = mesh2d.getncattr('edge_coordinates')
        x_y = edge_coordinates.split(' ')
        grid["edge_x"] = nc.variables[x_y[0]][:]
        grid["edge_y"] = nc.variables[x_y[1]][:]

        # face_node_connectivity: 'mesh2d_face_nodes' start_index=1
        face_node_connectivity = mesh2d.getncattr('face_node_connectivity')
        grid["face_node"] = nc.variables[face_node_connectivity][:]

        # face_coordinates: 'mesh2d_face_x mesh2d_face_y'
        face_coordinates = mesh2d.getncattr('face_coordinates')
        x_y = face_coordinates.split(' ')
        grid["face_x"] = nc.variables[x_y[0]][:]
        grid["face_y"] = nc.variables[x_y[1]][:]

        nc.close()

        self.model.grid = grid

    def print_ncattr(self,nc, key):
        try:
            print("\t\ttype:", repr(nc.variables[key].dtype))
            for ncattr in nc.variables[key].ncattrs():
                print('\t\t%s:' % ncattr,\
                      repr(nc.variables[key].getncattr(ncattr)))
        except KeyError:
            print("\t\tWARNING: %s does not contain variable attributes" % key)

