using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace DeltaShell.NGHS.IO.Grid
{
    public class UGrid : AGrid, IUGrid
    {
        public int[][,] FaceNodes { get; protected set; }
        public int[][,] EdgeNodes { get; protected set; }
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
            get { return GetFromValidUGridApi<IUGridApi, double>(uGridApi => uGridApi.zCoordinateFillValue, double.NaN, "Couldn't get the z-coordinate"); }
            set { DoWithValidUGridApi(uGridApi => uGridApi.zCoordinateFillValue = value, "Couldn't set the z-coordinate"); }
        }


        public void SetupForLoading()
        {
            var nMesh = NumberOf2DMeshes();
            NodeCoordinates = new Dictionary<int, Coordinate[]>();
            EdgeNodes = new int[nMesh][,];
            FaceNodes = new int[nMesh][,];
            VarNameIdsAtLocationInMesh = new Dictionary<int, Dictionary<int, int[]>>();

            for (var mesh = 1; mesh <= nMesh; mesh++)
            {
                GetAllNodeCoordinates(mesh);
                GetEdgeNodesForMesh(mesh);
                GetFaceNodesForMesh(mesh);
            }
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

        public int NumberOfNodes(int mesh)
        {
            int numberOfNodes;
            const string errorMessage = "Couldn't get number of nodes";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetNumberOfNodes(mesh, out numberOfNodes);
            ThrowIfError(ierr, errorMessage);
            return numberOfNodes;
            
        }

        public int NumberOfEdges(int mesh)
        {
            int numberOfEdges;
            const string errorMessage = "Couldn't get number of edges";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetNumberOfEdges(mesh, out numberOfEdges);
            ThrowIfError(ierr, errorMessage);
            
            return numberOfEdges;
        }

        public int NumberOfFaces(int mesh)
        {
            int numberOfFaces;
            const string errorMessage = "Couldn't get number of faces";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetNumberOfFaces(mesh, out numberOfFaces);
            ThrowIfError(ierr, "Couldn't get number of faces");
            return numberOfFaces;
        }

        public int NumberOfMaxFaceNodes(int mesh)
        {
            int maxFaceNodes;
            const string errorMessage = "Couldn't get max face nodes";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetMaxFaceNodes(mesh, out maxFaceNodes);
            ThrowIfError(ierr, errorMessage);
            return maxFaceNodes;
        }

        public bool GetAllNodeCoordinates(int mesh)
        {
            return GetFromValidUGridApi(uGridApi =>
            {
                var nNode = NumberOfNodes(mesh);
                if (nNode == 0) return false;
                
                //retrieve x
                double[] xCoordinates;
                var ierr = uGridApi.GetNodeXCoordinates(mesh, out xCoordinates);
                ThrowIfError(ierr, "Couldn't get x node coordinates");

                //retrieve y
                double[] yCoordinates;
                ierr = uGridApi.GetNodeYCoordinates(mesh, out yCoordinates);
                ThrowIfError(ierr, "Couldn't get y node coordinates");
                
                //retrieve z
                double[] zCoordinates;
                ierr = uGridApi.GetNodeZCoordinates(mesh, out zCoordinates);
                ThrowIfError(ierr, "Couldn't get z node coordinates");
                
                var coordinates = new Coordinate[nNode];
                for (int i = 0; i < nNode; i++)
                {
                    coordinates[i] = new Coordinate(xCoordinates[i], yCoordinates[i], zCoordinates[i]);
                }
                NodeCoordinates[mesh - 1] = coordinates;

                return true;
            }, false, "Couldn't get the node coordinates");
        }

        public void GetEdgeNodesForMesh(int mesh)
        {
            int[,] edgeNodes;
            const string errorMessage = "Couldn't get face nodes of the mesh";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetEdgeNodesForMesh(mesh, out edgeNodes);
            ThrowIfError(ierr, errorMessage);
            
            EdgeNodes[mesh - 1] = edgeNodes;
        }

        public void GetFaceNodesForMesh(int mesh)
        {
            int[,] faceNodes;
            const string errorMessage = "Couldn't get face nodes of the mesh";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.GetFaceNodesForMesh(mesh, out faceNodes);
            ThrowIfError(ierr, errorMessage);

            FaceNodes[mesh - 1] = faceNodes;
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

        public void GetNamesAtLocation(int mesh, int location)
        {
            const string errorMessage = "Couldn't get the names at location";
            DoWithValidUGridApi(uGridApi =>
            {
                int[] varIds;
                var ierr = uGridApi.GetVarNames(mesh, location, out varIds);
                ThrowIfError(ierr, errorMessage);

                var varNameIdsAtLocation = new Dictionary<int, int[]>();
                varNameIdsAtLocation[location] = varIds;
                VarNameIdsAtLocationInMesh[mesh - 1] = varNameIdsAtLocation;
            }, errorMessage);
        }

        /// <summary>
        /// Overwrites the existing x and y coordinates of the vertices with new values. Note: the number of supplied values must equal
        /// the number of existing values. Useful for coordinate transformation etc.
        /// </summary>
        public void RewriteGridCoordinates(int mesh, double[] xValues, double[] yValues)
        {
            const string errorMessage = "Couldn't rewrite grid coordinates";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.WriteXYCoordinateValues(mesh, xValues, yValues);
            ThrowIfError(ierr, errorMessage);
        }
        
        public void WriteZValuesAtFaces(int meshId, double[] zValues)
        {
            const string faceBedLevelVariableName = "face_z";
            const string faceBedLevelVariableLongName = "z-coordinate of mesh faces";

            const string errorMessage = "Couldn't save x and y coordinates at mesh faces";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.WriteZCoordinateValues(
                meshId,
                (int)GridApiDataSet.Locations.UG_LOC_FACE,
                faceBedLevelVariableName,
                faceBedLevelVariableLongName,
                zValues);
            ThrowIfError(ierr, errorMessage);
        }

        public void WriteZValuesAtNodes(int meshId, double[] zValues)
        {
            const string nodeBedLevelVariableName = "node_z";
            const string nodeBedLevelVariableLongName = "z-coordinate of mesh nodes";

            const string errorMessage = "Couldn't save x and y coordinates at mesh nodes";
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            var ierr = uGridApi.WriteZCoordinateValues(
                meshId,
                (int)GridApiDataSet.Locations.UG_LOC_NODE,
                nodeBedLevelVariableName,
                nodeBedLevelVariableLongName,
                zValues);
            ThrowIfError(ierr, errorMessage);
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

        //private IUGridApi GetValidUGridApi()
        //{
        //    var uGridApi = GridApi as IUGridApi;
        //    if (!IsInitialized() && !initializing)
        //    {
        //        Initialize();
        //    }

        //    bool isValid = uGridApi != null && IsInitialized() && IsValid();
        //    if (!isValid)
        //    {
        //        throw new InvalidOperationException("Communication with netCDF file was unsuccessful, API is not set");
        //    }
        //    return uGridApi;
        //}

        private T GetFromValidUGridApi<T>(Func<IUGridApi, T> function, T defaultValue, string errorMessage)
        {
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            return uGridApi != null ? function(uGridApi) : defaultValue;
        }

        private void DoWithValidUGridApi(Action<IUGridApi> action, string errorMessage)
        {
            var uGridApi = GetValidGridApi<IUGridApi>(errorMessage);
            action(uGridApi);
        }
    }
}