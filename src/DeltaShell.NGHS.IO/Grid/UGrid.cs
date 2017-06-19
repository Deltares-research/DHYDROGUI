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

        private bool initializing = false;
        private bool initialized = false;

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
            get { return GetFromValidUGridApi(uGridApi => uGridApi.zCoordinateFillValue, double.NaN); }
            set { DoWithValidUGridApi(uGridApi => uGridApi.zCoordinateFillValue = value); }
        }

        public override void Initialize()
        {
            initializing = true;
            base.Initialize();
            if(!IsValid()) return;
            var nMesh = NumberOfMesh();
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

            initializing = false;
            initialized = true;
        }
        
        public int NumberOfMesh()
        {
            return GetFromValidUGridApi(uGridApi => uGridApi.GetMeshCount(), 0);
        }

        public int NumberOfNodes(int mesh)
        {
            int numberOfNodes;
            var uGridApi = GetValidUGridApi();
            var ierr = uGridApi.GetNumberOfNodes(mesh, out numberOfNodes);
            ThrowIfError(ierr, "Couldn't get number of nodes");
            return numberOfNodes;
            
        }

        public int NumberOfEdges(int mesh)
        {
            int numberOfEdges;
            var uGridApi = GetValidUGridApi();
            var ierr = uGridApi.GetNumberOfEdges(mesh, out numberOfEdges);
            ThrowIfError(ierr, "Couldn't get number of edges");
            
            return numberOfEdges;
        }

        public int NumberOfFaces(int mesh)
        {
            int numberOfFaces;
            var uGridApi = GetValidUGridApi();
            var ierr = uGridApi.GetNumberOfFaces(mesh, out numberOfFaces);
            ThrowIfError(ierr, "Couldn't get number of faces");
            return numberOfFaces;
        }

        public int NumberOfMaxFaceNodes(int mesh)
        {
            int maxFaceNodes;
            var uGridApi = GetValidUGridApi();
            var ierr = uGridApi.GetMaxFaceNodes(mesh, out maxFaceNodes);
            ThrowIfError(ierr, "Couldn't get max face nodes");
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
            }, false);
        }

        public void GetEdgeNodesForMesh(int mesh)
        {
            var uGridApi = GetValidUGridApi();
            int[,] edgeNodes;
            var ierr = uGridApi.GetEdgeNodesForMesh(mesh, out edgeNodes);
            ThrowIfError(ierr, "Couldn't get edge nodes list");
            
            EdgeNodes[mesh - 1] = edgeNodes;
        }

        public void GetFaceNodesForMesh(int mesh)
        {
            var uGridApi = GetValidUGridApi();
            int[,] faceNodes;
            var ierr = uGridApi.GetFaceNodesForMesh(mesh, out faceNodes);
            ThrowIfError(ierr, "Couldn't get face nodes list");

            FaceNodes[mesh - 1] = faceNodes;
        }

        public int NumberOfNamesAtLocation(int mesh, int location)
        {
            var uGridApi = GetValidUGridApi();
            int nCount;
            var ierr = uGridApi.GetVarCount(mesh, location, out nCount);
            ThrowIfError(ierr, "Couldn't get the nr of number of names at location");

            return nCount;
        }

        public void GetNamesAtLocation(int mesh, int location)
        {
            DoWithValidUGridApi(uGridApi =>
            {
                int[] varIds;
                var ierr = uGridApi.GetVarNames(mesh, location, out varIds);
                ThrowIfError(ierr, "Couldn't get the names at location");

                var varNameIdsAtLocation = new Dictionary<int, int[]>();
                varNameIdsAtLocation[location] = varIds;
                VarNameIdsAtLocationInMesh[mesh - 1] = varNameIdsAtLocation;
            });
        }

        /// <summary>
        /// Overwrites the existing x and y coordinates of the vertices with new values. Note: the number of supplied values must equal
        /// the number of existing values. Useful for coordinate transformation etc.
        /// </summary>
        public void RewriteGridCoordinates(int mesh, double[] xValues, double[] yValues)
        {
            var uGridApi = GetValidUGridApi();
            var ierr = uGridApi.WriteXYCoordinateValues(mesh, xValues, yValues);
            ThrowIfError(ierr, "Couldn't save x and y coordinates");
        }
        
        public void WriteZValuesAtFaces(int meshId, double[] zValues)
        {
            const string faceBedLevelVariableName = "face_z";
            const string faceBedLevelVariableLongName = "z-coordinate of mesh faces";

            var uGridApi = GetValidUGridApi();
            var ierr = uGridApi.WriteZCoordinateValues(
                meshId,
                (int)GridApiDataSet.Locations.UG_LOC_FACE,
                faceBedLevelVariableName,
                faceBedLevelVariableLongName,
                zValues);
            ThrowIfError(ierr, "Couldn't save x and y coordinates");
        }

        public void WriteZValuesAtNodes(int meshId, double[] zValues)
        {
            const string nodeBedLevelVariableName = "node_z";
            const string nodeBedLevelVariableLongName = "z-coordinate of mesh nodes";

            var uGridApi = GetValidUGridApi();
            var ierr = uGridApi.WriteZCoordinateValues(
                meshId,
                (int)GridApiDataSet.Locations.UG_LOC_NODE,
                nodeBedLevelVariableName,
                nodeBedLevelVariableLongName,
                zValues);
            ThrowIfError(ierr, "Couldn't save x and y coordinates");
        }

        public string NameOfMesh(int mesh)
        {
            var uGridApi = GetValidUGridApi();
            string meshName;
            var ierr = uGridApi.GetMeshName(mesh, out meshName);
            ThrowIfError(ierr, "Couldn't get meshname");

            return meshName;
        }

        private void ThrowIfError(int ierr, string exceptionText)
        {
            if (ierr != GridApiDataSet.GridConstants.IONC_NOERR)
            {
                throw new Exception(string.Format(exceptionText + " because of error number: {0}", ierr));
            }
        }

        private IUGridApi GetValidUGridApi()
        {
            var uGridApi = GridApi as IUGridApi;
            if (!initialized && !initializing)
            {
                Initialize();
            }

            bool isValid = uGridApi != null && IsInitialized() && IsValid();
            if (!isValid)
            {
                throw new InvalidOperationException("Communication with netCDF file was unsuccessful, API is not set");
            }
            return uGridApi;
        }

        private T GetFromValidUGridApi<T>(Func<IUGridApi, T> function, T defaultValue)
        {
            bool isValid;
            var uGridApi = IsValidUGridApi(out isValid);
            return isValid ? function(uGridApi) : defaultValue;
        }

        private void DoWithValidUGridApi(Action<IUGridApi> action)
        {
            bool isValid;
            var uGridApi = IsValidUGridApi(out isValid);

            if (!isValid) return;

            action(uGridApi);
        }

        private IUGridApi IsValidUGridApi(out bool isValid)
        {
            var uGridApi = GridApi as IUGridApi;
            if (!initialized && !initializing)
            {
                Initialize();
            }

            isValid = uGridApi != null && IsInitialized() && IsValid();
            return uGridApi;
        }
    }
}