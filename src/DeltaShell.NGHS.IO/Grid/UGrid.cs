using System.Collections.Generic;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGrid : AGrid<IUGridApi>
    {
        public int nNodes;

        public int[][,] FaceNodesByMeshId { get; protected set; }
        public int[][,] EdgeNodesByMeshId { get; protected set; }
        public Dictionary<int, Coordinate[]> NodeCoordinatesByMeshId { get; protected set; }
        public Dictionary<int, Dictionary<GridApiDataSet.LocationType, int[]>> VarNameIdsByLocationTypeByMeshId;

        public UGrid(string file, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : base(file, mode)
        {
           GridApi = GridApiFactory.CreateNew();
        }

        public UGrid(string file, UGridGlobalMetaData globalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : base(file, globalMetaData, mode)
        {
            GridApi = GridApiFactory.CreateNew();
        }

        public double ZCoordinateFillValue
        {
            get { return GetFromValidGridApi(uGridApi => uGridApi.zCoordinateFillValue, double.NaN, Resources.UGrid_ZCoordinateFillValue_Couldn_t_get_the_z_coordinate); }
            set { DoWithValidGridApi(uGridApi => uGridApi.zCoordinateFillValue = value, Resources.UGrid_ZCoordinateFillValue_Couldn_t_set_the_z_coordinate); }
        }

        public int GetNumberOf2DMeshes()
        {
            int numberOfMeshes;
            var uGridApi = GetValidGridApi(Resources.UGrid_GetNumberOf2DMeshes_Couldn_t_get_the_number_of_2D_meshes);
            var ierr = uGridApi.GetNumberOfMeshByType(UGridMeshType.Mesh2D, out numberOfMeshes);
            ThrowIfError(ierr, Resources.UGrid_GetNumberOf2DMeshes_Couldn_t_get_the_number_of_2D_meshes);
            return numberOfMeshes;
        }

        public int GetNumberOfNodesForMeshId(int meshId)
        {
            int numberOfNodes;
            var uGridApi = GetValidGridApi(Resources.UGrid_GetNumberOfNodesForMeshId_Couldn_t_get_the_number_of_nodes);
            var ierr = uGridApi.GetNumberOfNodes(meshId, out numberOfNodes);
            ThrowIfError(ierr, Resources.UGrid_GetNumberOfNodesForMeshId_Couldn_t_get_the_number_of_nodes);
            return numberOfNodes;
        }

        public int GetNumberOfEdgesForMeshId(int meshId)
        {
            int numberOfEdges;
            var uGridApi = GetValidGridApi(Resources.UGrid_GetNumberOfEdgesForMeshId_Couldn_t_get_number_of_edges);
            var ierr = uGridApi.GetNumberOfEdges(meshId, out numberOfEdges);
            ThrowIfError(ierr, Resources.UGrid_GetNumberOfEdgesForMeshId_Couldn_t_get_number_of_edges);
            
            return numberOfEdges;
        }

        public int GetNumberOfFacesForMeshId(int meshId)
        {
            int numberOfFaces;
            var uGridApi = GetValidGridApi(Resources.UGrid_GetNumberOfFacesForMeshId_Couldn_t_get_number_of_faces);
            var ierr = uGridApi.GetNumberOfFaces(meshId, out numberOfFaces);
            ThrowIfError(ierr, Resources.UGrid_GetNumberOfFacesForMeshId_Couldn_t_get_number_of_faces);
            return numberOfFaces;
        }

        public int GetNumberOfMaxFaceNodesForMeshId(int meshId)
        {
            int maxFaceNodes;
            var uGridApi = GetValidGridApi(Resources.UGrid_GetNumberOfMaxFaceNodesForMeshId_Couldn_t_get_max_face_nodes);
            var ierr = uGridApi.GetMaxFaceNodes(meshId, out maxFaceNodes);
            ThrowIfError(ierr, Resources.UGrid_GetNumberOfMaxFaceNodesForMeshId_Couldn_t_get_max_face_nodes);
            return maxFaceNodes;
        }

        public Coordinate[] GetAllNodeCoordinatesForMeshId(int meshId)
        {
            return GetFromValidGridApi(uGridApi =>
            {
                var numberOfNodes = GetNumberOfNodesForMeshId(meshId);
                if (numberOfNodes == 0) return new Coordinate[0];
                
                //retrieve x
                double[] xCoordinates;
                var ierr = uGridApi.GetNodeXCoordinates(meshId, out xCoordinates);
                ThrowIfError(ierr, Resources.UGrid_GetAllNodeCoordinatesForMeshId_Couldn_t_get_x_node_coordinates);

                //retrieve y
                double[] yCoordinates;
                ierr = uGridApi.GetNodeYCoordinates(meshId, out yCoordinates);
                ThrowIfError(ierr, Resources.UGrid_GetAllNodeCoordinatesForMeshId_Couldn_t_get_y_node_coordinates);
                
                //retrieve z
                double[] zCoordinates;
                ierr = uGridApi.GetNodeZCoordinates(meshId, out zCoordinates);
                ThrowIfError(ierr, Resources.UGrid_GetAllNodeCoordinatesForMeshId_Couldn_t_get_z_node_coordinates);
                
                var coordinates = new Coordinate[numberOfNodes];
                for (int i = 0; i < numberOfNodes; i++)
                {
                    coordinates[i] = new Coordinate(xCoordinates[i], yCoordinates[i], zCoordinates[i]);
                }
                if(NodeCoordinatesByMeshId == null) NodeCoordinatesByMeshId = new Dictionary<int, Coordinate[]>();
                NodeCoordinatesByMeshId[meshId - 1] = coordinates;

                return coordinates;
            }, new Coordinate[0], Resources.UGrid_GetAllNodeCoordinatesForMeshId_Couldn_t_get_the_node_coordinates);
        }

        public int[,] GetEdgeNodesForMeshId(int meshId)
        {
            int[,] edgeNodes;

            if (EdgeNodesByMeshId == null) EdgeNodesByMeshId = new int[GetNumberOf2DMeshes()][,];
            var uGridApi = GetValidGridApi(Resources.UGrid_GetEdgeNodesForMeshId_Couldn_t_get_edge_nodes_of_the_mesh);
            var ierr = uGridApi.GetEdgeNodesForMesh(meshId, out edgeNodes);
            ThrowIfError(ierr, Resources.UGrid_GetEdgeNodesForMeshId_Couldn_t_get_edge_nodes_of_the_mesh);

            EdgeNodesByMeshId[meshId - 1] = edgeNodes;
            return edgeNodes;
        }


        public int[,] GetFaceNodesForMeshId(int meshId)
        {
            int[,] faceNodes;
            var uGridApi = GetValidGridApi(Resources.UGrid_GetFaceNodesForMeshId_Couldn_t_get_face_nodes_of_the_mesh);
            var ierr = uGridApi.GetFaceNodesForMesh(meshId, out faceNodes);
            ThrowIfError(ierr, Resources.UGrid_GetFaceNodesForMeshId_Couldn_t_get_face_nodes_of_the_mesh);

            if(FaceNodesByMeshId == null) FaceNodesByMeshId = new int[GetNumberOf2DMeshes()][,];
            FaceNodesByMeshId[meshId - 1] = faceNodes;
            return faceNodes;
        }

        public int NumberOfNamesForLocationType(int meshId, GridApiDataSet.LocationType locationType)
        {
            int nCount;
            var uGridApi = GetValidGridApi(Resources.UGrid_NumberOfNamesAtLocation_Couldn_t_get_the_number_of_names_for_location_type);
            var ierr = uGridApi.GetVarCount(meshId, locationType, out nCount);
            ThrowIfError(ierr, Resources.UGrid_NumberOfNamesAtLocation_Couldn_t_get_the_number_of_names_for_location_type);

            return nCount;
        }

        public Dictionary<GridApiDataSet.LocationType, int[]> GetNamesAtLocation(int meshId, GridApiDataSet.LocationType locationType)
        {
            DoWithValidGridApi(uGridApi =>
            {
                int[] varIds;
                var ierr = uGridApi.GetVarNames(meshId, locationType, out varIds);
                ThrowIfError(ierr, Resources.UGrid_GetNamesAtLocation_Couldn_t_get_the_names_at_location);

                var varNameIdsAtLocation = new Dictionary<GridApiDataSet.LocationType, int[]>();
                varNameIdsAtLocation[locationType] = varIds;
                if (VarNameIdsByLocationTypeByMeshId == null) VarNameIdsByLocationTypeByMeshId = new Dictionary<int, Dictionary<GridApiDataSet.LocationType, int[]>>();
                VarNameIdsByLocationTypeByMeshId[meshId - 1] = varNameIdsAtLocation;
            }, Resources.UGrid_GetNamesAtLocation_Couldn_t_get_the_names_at_location);
            return VarNameIdsByLocationTypeByMeshId[meshId - 1];
        }

        /// <summary>
        /// Overwrites the existing x and y coordinates of the vertices with new values. Note: the number of supplied values must equal
        /// the number of existing values. Useful for coordinate transformation etc.
        /// </summary>
        public void RewriteGridCoordinatesForMeshId(int meshId, double[] xValues, double[] yValues)
        {
            DoWithValidGridApi(
                uGridApi => uGridApi.WriteXYCoordinateValues(meshId, xValues, yValues)
                , Resources.UGrid_RewriteGridCoordinates_Couldn_t_rewrite_grid_coordinates);
        }
        
        public void WriteZValuesAtFacesForMeshId(int meshId, double[] zValues)
        {
            DoWithValidGridApi(
                uGridApi => uGridApi.WriteZCoordinateValues(meshId, GridApiDataSet.LocationType.UG_LOC_FACE, GridApiDataSet.UGridApiConstants.FaceZ, Resources.UGrid_WriteZValuesAtFacesForMeshId_z_coordinate_of_mesh_faces, zValues)
                , Resources.UGrid_WriteZValuesAtFacesForMeshId_Couldn_t_get_z_values_at_mesh_faces);
        }

        public void WriteZValuesAtNodesForMeshId(int meshId, double[] zValues)
        {
            DoWithValidGridApi(
                uGridApi => uGridApi.WriteZCoordinateValues(meshId, GridApiDataSet.LocationType.UG_LOC_NODE, GridApiDataSet.UGridApiConstants.NodeZ, Resources.UGrid_WriteZValuesAtNodesForMeshId_z_coordinate_of_mesh_nodes, zValues)
                , Resources.UGrid_WriteZValuesAtNodesForMeshId_Couldn_t_write_z_values_at_mesh_nodes);
        }

        public string GetMeshName(int meshId)
        {
            string meshName;
            var uGridApi = GetValidGridApi(Resources.UGrid_GetMeshName_Couldn_t_get_meshname);
            var ierr = uGridApi.GetMeshName(meshId, out meshName);
            ThrowIfError(ierr, Resources.UGrid_GetMeshName_Couldn_t_get_meshname);

            return meshName;
        }
    }
}