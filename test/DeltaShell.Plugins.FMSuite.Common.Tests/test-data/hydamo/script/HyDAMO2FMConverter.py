# coding: latin-1
import os, sys, math
from gdal import ogr
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
        fm_model.crosssections = self.generate_crossections(hydamo_model.profiles, hydamo_model.network)
        return fm_model


    # generate network and 1d mesh
    def generate_networkdata(self, hydamo_model, one_d_mesh_distance = 40.0):
        networkdata = {}
        networkdata["node_ids"] = []
        networkdata["node_names"] = []
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
        i_branch = 1
        i_edge_point = 1
        for key, value in hydamo_model.network.items():

            branch_id = key
            points = value[0]
            branch_name = value[3]

            #test AaMaas
            #if branch_name != 'AaMaas':
            #    continue

            if points is None or len(points) < 2:
                continue

            length = value[1]
            first_point = points[0]
            first_point_x_y_name =  self.get_point_id(first_point) #for filtering duplicates
            last_point = points[-1]
            last_point_x_y_name =  self.get_point_id(last_point) #for filtering duplicates
            n_points = len(points)

            if first_point_x_y_name not in nodes:
                nodes[first_point_x_y_name] = first_point
            if last_point_x_y_name not in nodes:
                nodes[last_point_x_y_name] = last_point

            #save branches
            networkdata["branch_ids"].append(i_branch)
            networkdata["branch_names"].append(self.str2chars(branch_id,self.idstrlength))
            networkdata["branch_longnames"].append(self.str2chars("long_"+ branch_id,self.longstrlength))
            networkdata["branch_order"].append(-1)
            networkdata["branch_ngeometrypoints"].append(n_points)
            networkdata["branch_length"].append(length)

            #edge_node (start & endnode branch)
            i_from = list(nodes.keys()).index(first_point_x_y_name) + 1
            i_to = list(nodes.keys()).index(last_point_x_y_name) + 1
            networkdata["edge_node"].append([i_from, i_to])

            #calculation points [1d mesh]

            #first point
            offset = 0.0
            mesh_point_name = branch_id + '_'+ str("%.0f" % offset)
            networkdata["point_ids"].append(self.str2chars(mesh_point_name, self.idstrlength))
            networkdata["point_longnames"].append(self.str2chars(mesh_point_name, self.longstrlength))
            networkdata["point_branch_id"].append(i_branch)
            networkdata["point_branch_offset"].append(offset)
            i_edge_point += 1
            offset = one_d_mesh_distance

            while offset < length - (one_d_mesh_distance/10.0):
                mesh_point_name = branch_id + '_'+ str("%.0f" %offset)
                networkdata["point_ids"].append(self.str2chars(mesh_point_name,self.idstrlength))
                networkdata["point_longnames"].append(self.str2chars(mesh_point_name,self.longstrlength))
                networkdata["point_branch_id"].append(i_branch)
                networkdata["point_branch_offset"].append(offset)
                networkdata["edge_point"].append([i_edge_point - 1, i_edge_point])
                i_edge_point += 1
                offset += one_d_mesh_distance

            #last calcpoint
            mesh_point_name = branch_id + '_'+ str("%.0f" % length)
            networkdata["point_ids"].append(self.str2chars(mesh_point_name,self.idstrlength))
            networkdata["point_longnames"].append(self.str2chars(mesh_point_name,self.longstrlength))
            networkdata["point_branch_id"].append(i_branch)
            networkdata["point_branch_offset"].append(length)
            networkdata["edge_point"].append([i_edge_point - 1, i_edge_point])
            i_edge_point += 1

            #save geometry
            for p in points:
                x, y = p
                networkdata["geom_x"].append(x)
                networkdata["geom_y"].append(y)

            i_branch += 1

            #if i_branch > 100: break

        node_i = 1
        for keyvalue in nodes.items():
            id = keyvalue[0]
            tuple = keyvalue[1]
            networkdata["node_ids"].append(node_i)
            networkdata["node_names"].append(self.str2chars("node" + id, self.idstrlength))
            networkdata["node_longnames"].append(self.str2chars("longname" + id,self.longstrlength))
            x, y = tuple
            networkdata["node_x"].append(x)
            networkdata["node_y"].append(y)
            node_i += 1

        return networkdata


    def generate_2dmesh_data(self, geom_x, geom_y):

        #just generate a west and east cell of the area for running the model
        minX = min(geom_x)
        maxX = max(geom_x)
        deltaX = (maxX - minX) * 0.5
        middleX = minX + deltaX
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

        grid["node_x"].extend([minX, minX, middleX, middleX, maxX, maxX])
        grid["node_y"].extend([minY, maxY, minY, minY, minY, maxY])
        grid["edge_node"].extend([[1, 2],[2, 4],[4, 3],[3, 1],[3,5],[5,6],[6,4]])
        grid["edge_x"].extend([minX, minX + (0.5 * deltaX), maxX, minX + (0.5 * deltaX),minX + (1.5 * deltaX),maxX,minX + (1.5 * deltaX)])
        grid["edge_y"].extend([minY + (0.5 * deltaY), maxY, minY + (0.5 * deltaY), minY,minY, minY + (0.5 * deltaY), maxY])
        grid["face_node"].append([3, 5, 6, 7])
        grid["face_x"].append(minX + (0.5 * deltaX))
        grid["face_x"].append(minX + (1.5 * deltaX))
        grid["face_y"].append(minY + (0.5 * deltaY))
        grid["face_y"].append(minY + (0.5 * deltaY))

        return grid

    def generate_crossections(self, profiles, branches):
        crosssections = []
        line = ogr.Geometry(ogr.wkbLineString)
        z_values = []
        name = None
        i_cs = 0

        for keyvalue in profiles.items():
            value = keyvalue[1]
            if name is None:
                name = value[8]

            if name != value[8]:

                #get cs
                if line.GetPointCount() <= 1:
                    print(str(name) + " has not enough points to construct a cross-section")
                else:
                    cs = self.get_yz_cs(name,line,z_values, branches)
                    if cs is not None:
                        crosssections.append(cs)
                        i_cs += 1

                        #if i_cs > 100: return crosssections


                #new cs
                line = ogr.Geometry(ogr.wkbLineString)
                z_values = []
                name = value[8]


            point = value[0][0]
            x, y, z = point
            line.AddPoint(x,y)
            z_values.append(z)

        if name is not None:
            if line.GetPointCount() <= 1:
                print(str(name) + " has not enough points to construct a cross-section")
            else:
                cs = self.get_yz_cs(name, line, z_values, branches)
                if cs is not None:
                    crosssections.append(cs)

        return crosssections

    def get_yz_cs(self, name, cs_line, z_values, branches):

        for keyvalue in branches.items():
            branch_id = keyvalue[0]
            points = keyvalue[1][0]
            branch = ogr.Geometry(ogr.wkbLineString)
            for xy in points:
                branch.AddPoint(xy[0], xy[1])

            if branch.GetPointCount() <= 1:
                continue

            if branch.Intersects(cs_line):
                point = branch.Intersection(cs_line)
                offset = self.get_offset(branch, point)
                yz_values = self.get_yz_values(cs_line, z_values)
                return [name, branch_id, offset, yz_values]

        print('No intersection of crossection = ' + str(name))
        return None

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

    def get_offset(self, line, geom):
        offset = 0.0

        for i in range(1, line.GetPointCount()):
            p_from = ogr.Geometry(ogr.wkbPoint)
            p = line.GetPoint(i-1)
            p_from.AddPoint(p[0],p[1])
            p_to = ogr.Geometry(ogr.wkbPoint)
            p = line.GetPoint(i)
            p_to.AddPoint(p[0],p[1])
            distance = p_from.Distance(p_to)
            segment = ogr.Geometry(ogr.wkbLineString)
            segment.AddPoint(p_from.GetX(),p_from.GetY())
            segment.AddPoint(p_to.GetX(),p_to.GetY())

            if(segment.Distance(geom) < 0.001):
                offset += p_from.Distance(geom)
                return offset

            offset += distance

        return -1.0

    def get_yz_values(self, line,z_values):
        points = []
        y = 0.0
        points.append([y,z_values[0]])
        for i in range(1, line.GetPointCount()):
            p_from = ogr.Geometry(ogr.wkbPoint)
            p = line.GetPoint(i-1)
            p_from.AddPoint(p[0],p[1])
            p_to = ogr.Geometry(ogr.wkbPoint)
            p = line.GetPoint(i)
            p_to.AddPoint(p[0],p[1])
            distance = p_from.Distance(p_to)
            y += distance
            points.append([y, z_values[i]])
        return points
