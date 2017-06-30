using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGrid : AGrid, IUGrid
    {
        public int[][,] FaceNodesPerMesh { get; protected set; }
        public int[][,] EdgeNodesPerMesh { get; protected set; }
        public Dictionary<int, Coordinate[]> NodeCoordinates { get; protected set; }
        public Dictionary<int, Dictionary<int, int[]>> VarNameIdsAtLocationInMesh;

        public UGrid(string file, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : base(file, mode)
        {
           GridApi = GridApiFactory.CreateNew();
        }

        public UGrid(string file, UGridGlobalMetaData globalMetaData, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite) : base(file, globalMetaData, mode)
        {
            GridApi = GridApiFactory.CreateNew();
        }

        public double zCoordinateFillValue
        {
            get { return GetFromValidGridApi<IUGridApi, double>(uGridApi => uGridApi.zCoordinateFillValue, double.NaN, "Couldn't get the z-coordinate"); }
            set { DoWithValidGridApi<IUGridApi>(uGridApi => uGridApi.zCoordinateFillValue = value, "Couldn't set the z-coordinate"); }
        }

        public int NumberOf2DMeshes()
        {
            int numberOfMeshes;
            const string errorMessage = "Couldn't get the number of 2D meshes";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetNumberOfMeshByType(UGridMeshType.Mesh2D, out numberOfMeshes);
            ThrowIfError(ierr, errorMessage);
            return numberOfMeshes;
        }

        public int NumberOfNodes(int meshId)
        {
            int numberOfNodes;
            const string errorMessage = "Couldn't get number of nodes";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetNumberOfNodes(meshId, out numberOfNodes);
            ThrowIfError(ierr, errorMessage);
            return numberOfNodes;
            
        }

        public int NumberOfEdges(int meshId)
        {
            int numberOfEdges;
            const string errorMessage = "Couldn't get number of edges";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetNumberOfEdges(meshId, out numberOfEdges);
            ThrowIfError(ierr, errorMessage);
            
            return numberOfEdges;
        }

        public int NumberOfFaces(int meshId)
        {
            int numberOfFaces;
            const string errorMessage = "Couldn't get number of faces";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetNumberOfFaces(meshId, out numberOfFaces);
            ThrowIfError(ierr, "Couldn't get number of faces");
            return numberOfFaces;
        }

        public int NumberOfMaxFaceNodes(int meshId)
        {
            int maxFaceNodes;
            const string errorMessage = "Couldn't get max face nodes";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetMaxFaceNodes(meshId, out maxFaceNodes);
            ThrowIfError(ierr, errorMessage);
            return maxFaceNodes;
        }

        public Coordinate[] GetAllNodeCoordinatesForMesh(int meshId)
        {
            return GetFromValidGridApi<IUGridApi, Coordinate[]>(uGridApi =>
            {
                var nNode = NumberOfNodes(meshId);
                if (nNode == 0) return new Coordinate[0];
                
                //retrieve x
                double[] xCoordinates;
                var ierr = uGridApi.GetNodeXCoordinates(meshId, out xCoordinates);
                ThrowIfError(ierr, "Couldn't get x node coordinates");

                //retrieve y
                double[] yCoordinates;
                ierr = uGridApi.GetNodeYCoordinates(meshId, out yCoordinates);
                ThrowIfError(ierr, "Couldn't get y node coordinates");
                
                //retrieve z
                double[] zCoordinates;
                ierr = uGridApi.GetNodeZCoordinates(meshId, out zCoordinates);
                ThrowIfError(ierr, "Couldn't get z node coordinates");
                
                var coordinates = new Coordinate[nNode];
                for (int i = 0; i < nNode; i++)
                {
                    coordinates[i] = new Coordinate(xCoordinates[i], yCoordinates[i], zCoordinates[i]);
                }
                if(NodeCoordinates == null) NodeCoordinates = new Dictionary<int, Coordinate[]>();
                NodeCoordinates[meshId - 1] = coordinates;

                return coordinates;
            }, new Coordinate[0], "Couldn't get the node coordinates");
        }

        public int[,] GetEdgeNodesForMesh(int meshId)
        {
            int[,] edgeNodes;
            const string errorMessage = "Couldn't get edge nodes of the mesh";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetEdgeNodesForMesh(meshId, out edgeNodes);
            ThrowIfError(ierr, errorMessage);

            if(EdgeNodesPerMesh == null) EdgeNodesPerMesh = new int[NumberOf2DMeshes()][,];
            EdgeNodesPerMesh[meshId - 1] = edgeNodes;
            return edgeNodes;
        }

        public int[,] GetFaceNodesForMesh(int meshId)
        {
            int[,] faceNodes;
            const string errorMessage = "Couldn't get face nodes of the mesh";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetFaceNodesForMesh(meshId, out faceNodes);
            ThrowIfError(ierr, errorMessage);

            if(FaceNodesPerMesh == null) FaceNodesPerMesh = new int[NumberOf2DMeshes()][,];
            FaceNodesPerMesh[meshId - 1] = faceNodes;
            return faceNodes;
        }

        public int NumberOfNamesAtLocation(int mesh, int location)
        {
            int nCount;
            const string errorMessage = "Couldn't get the number of names at location";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetVarCount(mesh, location, out nCount);
            ThrowIfError(ierr, errorMessage);

            return nCount;
        }

        public Dictionary<int, int[]> GetNamesAtLocation(int mesh, int location)
        {
            const string errorMessage = "Couldn't get the names at location";
            DoWithValidGridApi<IUGridApi>(uGridApi =>
            {
                int[] varIds;
                var ierr = uGridApi.GetVarNames(mesh, location, out varIds);
                ThrowIfError(ierr, errorMessage);

                var varNameIdsAtLocation = new Dictionary<int, int[]>();
                varNameIdsAtLocation[location] = varIds;
                if (VarNameIdsAtLocationInMesh == null) VarNameIdsAtLocationInMesh = new Dictionary<int, Dictionary<int, int[]>>();
                VarNameIdsAtLocationInMesh[mesh - 1] = varNameIdsAtLocation;
            }, errorMessage);
            return VarNameIdsAtLocationInMesh[mesh - 1];
        }

        /// <summary>
        /// Overwrites the existing x and y coordinates of the vertices with new values. Note: the number of supplied values must equal
        /// the number of existing values. Useful for coordinate transformation etc.
        /// </summary>
        public void RewriteGridCoordinates(int mesh, double[] xValues, double[] yValues)
        {
            DoWithValidGridApi<IUGridApi>(
                uGridApi => uGridApi.WriteXYCoordinateValues(mesh, xValues, yValues)
                , "Couldn't rewrite grid coordinates");
        }
        
        public void WriteZValuesAtFaces(int meshId, double[] zValues)
        {
            const string faceBedLevelVariableName = "face_z";
            const string faceBedLevelVariableLongName = "z-coordinate of mesh faces";
            DoWithValidGridApi<IUGridApi>(
                uGridApi => uGridApi.WriteZCoordinateValues(meshId, (int)GridApiDataSet.Locations.UG_LOC_FACE, faceBedLevelVariableName, faceBedLevelVariableLongName, zValues)
                , "Couldn't save x and y coordinates at mesh faces");
        }

        public void WriteZValuesAtNodes(int meshId, double[] zValues)
        {
            const string nodeBedLevelVariableName = "node_z";
            const string nodeBedLevelVariableLongName = "z-coordinate of mesh nodes";
            DoWithValidGridApi<IUGridApi>(
                uGridApi => uGridApi.WriteZCoordinateValues(meshId, (int)GridApiDataSet.Locations.UG_LOC_NODE, nodeBedLevelVariableName, nodeBedLevelVariableLongName, zValues)
                , "Couldn't save x and y coordinates at mesh nodes");
        }

        public string GetMeshName(int mesh)
        {
            string meshName;
            const string errorMessage = "Couldn't get meshname";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetMeshName(mesh, out meshName);
            ThrowIfError(ierr, errorMessage);

            return meshName;
        }
    }
}