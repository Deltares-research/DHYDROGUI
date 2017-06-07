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
            return GetFromValidUGridApi(uGridApi => uGridApi.GetNumberOfNodes(mesh), 0);
        }

        public int NumberOfEdges(int mesh)
        {
            return GetFromValidUGridApi(uGridApi => uGridApi.GetNumberOfEdges(mesh), 0);
        }

        public int NumberOfFaces(int mesh)
        {
            return GetFromValidUGridApi(uGridApi => uGridApi.GetNumberOfFaces(mesh), 0);
        }

        public int NumberOfMaxFaceNodes(int mesh)
        {
            return GetFromValidUGridApi(uGridApi => uGridApi.GetMaxFaceNodes(mesh),0);
        }

        public bool GetAllNodeCoordinates(int mesh)
        {
            return GetFromValidUGridApi(uGridApi =>
            {

                var nNode = NumberOfNodes(mesh);
                if (nNode == 0) return false;

                //retrieve x
                var xCoordinates = uGridApi.GetNodeXCoordinates(mesh);

                //retrieve y
                var yCoordinates = uGridApi.GetNodeYCoordinates(mesh);

                //retrieve z
                var zCoordinates = uGridApi.GetNodeZCoordinates(mesh);
                
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
            DoWithValidUGridApi(uGridApi => EdgeNodes[mesh - 1] = uGridApi.GetEdgeNodesForMesh(mesh));
        }

        public void GetFaceNodesForMesh(int mesh)
        {
            DoWithValidUGridApi(uGridApi => FaceNodes[mesh - 1] = uGridApi.GetFaceNodesForMesh(mesh));
        }
        
        public int NumberOfNamesAtLocation(int mesh, int location)
        {
            return GetFromValidUGridApi(uGridApi => uGridApi.GetVarCount(mesh, location), 0);
        }

        public void GetNamesAtLocation(int mesh, int location)
        {
            DoWithValidUGridApi(uGridApi =>
            {
                var varIds = uGridApi.GetVarNames(mesh, location);
                var VarNameIdsAtLocation = new Dictionary<int, int[]>();
                VarNameIdsAtLocation[location] = varIds;
                VarNameIdsAtLocationInMesh[mesh - 1] = VarNameIdsAtLocation;
            });
        }

        /// <summary>
        /// Overwrites the existing x and y coordinates of the vertices with new values. Note: the number of supplied values must equal
        /// the number of existing values. Useful for coordinate transformation etc.
        /// </summary>
        public void RewriteGridCoordinates(int mesh, double[] xValues, double[] yValues)
        {
            DoWithValidUGridApi(uGridApi => uGridApi.WriteXYCoordinateValues(mesh, xValues, yValues));
        }

        public void WriteZValues(int mesh, double[] zValues)
        {
            DoWithValidUGridApi(uGridApi => uGridApi.WriteZCoordinateValues(mesh, zValues));
        }

        public string NameOfMesh(int mesh)
        {
            return GetFromValidUGridApi(uGridApi => uGridApi.GetMeshName(mesh), string.Empty);
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