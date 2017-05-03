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

        public UGrid(string file, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite)
        {
            GridApi = GridApiFactory.CreateNew();
            
            Initialize(file, mode);
        }

        public double zCoordinateFillValue
        {
            get
            {
                var uGridApi = GridApi as IUGridApi;
                return uGridApi != null ? uGridApi.zCoordinateFillValue : double.NaN;
            }
            set
            {
                var uGridApi = GridApi as IUGridApi;
                if (uGridApi != null) uGridApi.zCoordinateFillValue = value;
            }
        }

        public override void Initialize(string filename, GridApiDataSet.NetcdfOpenMode mode)
        {
            base.Initialize(filename, mode);
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
            
        }
        
        public int NumberOfMesh()
        {
            var uGridApi = GridApi as IUGridApi;
            return uGridApi != null ? (!IsInitialized() || !IsValid() ? 0 : uGridApi.GetMeshCount()) : 0;
        }

        public int NumberOfNodes(int mesh)
        {
            var uGridApi = GridApi as IUGridApi;
            return uGridApi != null ? ((!IsInitialized() || !IsValid()) ? 0 : uGridApi.GetNumberOfNodes(mesh)) : 0;
        }

        public int NumberOfEdges(int mesh)
        {
            var uGridApi = GridApi as IUGridApi;
            return uGridApi != null ? ((!IsInitialized() || !IsValid()) ? 0 : uGridApi.GetNumberOfEdges(mesh)) : 0;
        }

        public int NumberOfFaces(int mesh)
        {
            var uGridApi = GridApi as IUGridApi;
            return uGridApi != null ? ((!IsInitialized() || !IsValid()) ? 0 : uGridApi.GetNumberOfFaces(mesh)) : 0;
        }

        public int NumberOfMaxFaceNodes(int mesh)
        {
            var uGridApi = GridApi as IUGridApi;
            return uGridApi != null ? ((!IsInitialized() || !IsValid()) ? 0 : uGridApi.GetMaxFaceNodes(mesh)) : 0;
        }
        
        public bool GetAllNodeCoordinates(int mesh)
        {
            if (!IsInitialized() || !IsValid()) return false;
            var uGridApi = GridApi as IUGridApi;
            if (uGridApi == null) return false;
            var nNode = NumberOfNodes(mesh);
            if(nNode == 0) return false;
            
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
            NodeCoordinates[mesh-1] = coordinates;
        
            return true;
        }
        
        public void GetEdgeNodesForMesh(int mesh)
        {
            if (!IsInitialized() || !IsValid()) return;
            var uGridApi = GridApi as IUGridApi;
            if (uGridApi == null) return;
            EdgeNodes[mesh - 1] = uGridApi.GetEdgeNodesForMesh(mesh);
        }

        public void GetFaceNodesForMesh(int mesh)
        {
            if (!IsInitialized() || !IsValid()) return;
            var uGridApi = GridApi as IUGridApi;
            if (uGridApi == null) return;
            FaceNodes[mesh - 1] = uGridApi.GetFaceNodesForMesh(mesh);
        }
        
        public int NumberOfNamesAtLocation(int mesh, int location)
        {
            var uGridApi = GridApi as IUGridApi;
            return uGridApi != null ? uGridApi.GetVarCount(mesh, location) : 0;
        }

        public void GetNamesAtLocation(int mesh, int location)
        {
            var uGridApi = GridApi as IUGridApi;
            if (uGridApi == null) return;
            var varIds = uGridApi.GetVarNames(mesh, location);
            var VarNameIdsAtLocation =  new Dictionary<int, int[]>();
            VarNameIdsAtLocation[location] = varIds;
            VarNameIdsAtLocationInMesh[mesh - 1] = VarNameIdsAtLocation;
        }

        /// <summary>
        /// Overwrites the existing x and y coordinates of the vertices with new values. Note: the number of supplied values must equal
        /// the number of existing values. Useful for coordinate transformation etc.
        /// </summary>
        public void RewriteGridCoordinates(int mesh, double[] xValues, double[] yValues)
        {
            var uGridApi = GridApi as IUGridApi;
            if (uGridApi == null) return;
            uGridApi.WriteXYCoordinateValues(mesh, xValues, yValues);
        }

        public void WriteZValues(int mesh, double[] zValues)
        {
            var uGridApi = GridApi as IUGridApi;
            if (uGridApi == null) return;
            uGridApi.WriteZCoordinateValues(mesh, zValues);
        }

        public string NameOfMesh(int mesh)
        {
            var uGridApi = GridApi as IUGridApi;
            return uGridApi != null ? uGridApi.GetMeshName(mesh) : string.Empty;
        }
    }
}