# coding: latin-1
import os, sys, math
from collections import OrderedDict
from HyDAMOmodel import HyDAMOmodel
from FMmodel import FMmodel

class HyDAMO2FMConverter:
    idstrlength = 40
    longstrlength = 80

    def __init__(self):
        return

    def ConvertToFMmodel(self, hydamo_model = HyDAMOmodel(), one_d_mesh_distance = 40.0):
        fm_model = FMmodel()
        fm_model.networkdata = self.generate_networkdata(hydamo_model, one_d_mesh_distance)
        fm_model.griddata = self.generate_2dmesh_data(fm_model.networkdata["geom_x"], fm_model.networkdata["geom_y"])
        return fm_model


    # generate network and 1d mesh
    def generate_networkdata(self, hydamo_model, one_d_mesh_distance = 40.0):
        networkdata = {}
        networkdata["node_ids"] = []
        networkdata["node_longnames"] = []
        networkdata["node_x"] = []
        networkdata["node_y"] = []
        networkdata["geom_x"] = []
        networkdata["geom_y"] = []
        networkdata["edge_node"] = []
        networkdata["point_ids"] = []
        networkdata["point_longnames"] = []
        networkdata["point_branch_id"] = []
        networkdata["point_branch_offset"] = []
        networkdata["edge_point"] = []
        networkdata["branch_ids"] = []
        networkdata["branch_names"] = []
        networkdata["branch_longnames"] = []
        networkdata["branch_length"] = []
        networkdata["branch_order"] = []
        networkdata["branch_ngeometrypoints"] = []

        # Temporary dictionary to store the id number of the nodes and branches
        nodes = OrderedDict()

        #parse branches
        i_branch = 0
        i_edge_point = 1
        i_edge_node = 1
        for key, value in hydamo_model.network.items():
            branch_id = key
            points = value[0]
            branch_name = value[2]
            if points is None or len(points) < 2:
                continue

            length = value[1]
            first_point = points[0]
            first_point_id = self.get_point_id(first_point)
            last_point = points[-1]
            last_point_id = self.get_point_id(last_point)
            n_points = len(points)


            if first_point_id not in nodes:
                nodes[first_point_id] = first_point
            if last_point_id not in nodes:
                nodes[last_point_id] = last_point

            #save branches
            networkdata["branch_ids"].append(i_branch)
            networkdata["branch_names"].append(self.str2chars(branch_id,self.idstrlength))
            networkdata["branch_longnames"].append(self.str2chars("longname " + branch_name,self.longstrlength))
            networkdata["branch_order"].append(-1)
            networkdata["branch_ngeometrypoints"].append(n_points)
            networkdata["branch_length"].append(length)

            #calculation points [1d mesh]
            offset = 0.0
            i_mesh_point = 1
            while offset < length:
                mesh_point_name = branch_id + str("%.0f" %offset)
                networkdata["point_ids"].append(self.str2chars(mesh_point_name,self.idstrlength))
                networkdata["point_longnames"].append(self.str2chars(mesh_point_name,self.longstrlength))
                networkdata["point_branch_id"].append(i_branch)
                networkdata["point_branch_offset"].append(offset)
                if i_mesh_point > 1:
                    networkdata["edge_point"].append([i_edge_point - 1, i_edge_point])
                i_mesh_point += 1
                i_edge_point += 1
                offset += one_d_mesh_distance

            #last calcpoint
            if i_mesh_point > 1:
                mesh_point_name = branch_id + str("%.0f" % length)
                networkdata["point_ids"].append(self.str2chars(mesh_point_name,self.idstrlength))
                networkdata["point_longnames"].append(self.str2chars(mesh_point_name,self.longstrlength))
                networkdata["point_branch_id"].append(i_branch)
                networkdata["point_branch_offset"].append(length)
                networkdata["edge_point"].append([i_edge_point - 1, i_edge_point])
                i_edge_point += 1

            #save geometry
            first_point = True
            for p in points:
                x, y = p
                networkdata["geom_x"].append(x)
                networkdata["geom_y"].append(y)
                if not first_point:
                    networkdata["edge_node"].append([i_edge_node-1, i_edge_node])
                first_point = False
                i_edge_node += 1

        i = 0
        for keyvalue in nodes.items():
            id = keyvalue[0]
            tuple = keyvalue[1]
            networkdata["node_ids"].append(self.str2chars("node" + id,self.idstrlength))
            networkdata["node_longnames"].append(self.str2chars("longname" + id,self.longstrlength))
            x, y = tuple
            networkdata["node_x"].append(x)
            networkdata["node_y"].append(y)

        return networkdata


    def generate_2dmesh_data(self, geom_x, geom_y):

        minX = min(geom_x)
        maxX = max(geom_x)
        deltaX = maxX - minX
        minY = min(geom_y)
        maxY = max(geom_y)
        deltaY = maxY - minY

        # generate extend as one cell
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

        grid["node_x"].extend([minX, minX, maxX, maxX])
        grid["node_y"].extend([minY, maxY, maxY, minY])
        grid["edge_node"].extend([[1, 2],[2, 3],[3, 4],[4, 1]])
        grid["edge_x"].extend([minX, minX + (0.5 * deltaX), maxX, minX + (0.5 * deltaX)])
        grid["edge_y"].extend([minY + (0.5 * deltaY), maxY, minY + (0.5 * deltaY), minY])
        grid["face_node"].append([1, 2, 3, 4])
        grid["face_x"].append(minX + (0.5 * deltaX))
        grid["face_y"].append(minY + (0.5 * deltaY))

        return grid

    def get_point_id(self, point):
        x,y = point
        return str("%.0f" % float(x)) + "_" + str("%.0f" % float(y))

    def str2chars(self,str,size):
        chars = list(str)
        if len(chars) > size:
            chars = chars[:size]
        elif len(chars) < size:
            chars.extend(list(' '* (size - len(chars))))
        return chars
