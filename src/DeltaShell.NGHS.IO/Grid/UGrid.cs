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

        public double zCoordinateFillValue
        {
            get
            {
                return GridApi != null ? GridApi.zCoordinateFillValue : double.NaN;
            }
            set
            {
                if (GridApi != null) GridApi.zCoordinateFillValue = value;
            }
        }

        public UGrid(string file, GridApiDataSet.NetcdfOpenMode mode = GridApiDataSet.NetcdfOpenMode.nf90_nowrite)
        {
            GridApi = GridApiFactory.CreateNew();
            Initialize(file, mode);
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

        public override bool IsValid()
        {
            return GridApi != null && (GridApi.GetConvention() == GridApiDataSet.DataSetConventions.IONC_CONV_UGRID &&
                                       GridApi.GetVersion() >= GridApiDataSet.GridConstants.UG_CONV_MIN_VERSION);
        }
        
        public int NumberOfMesh()
        {
            return GridApi != null ? (!IsInitialized() || !IsValid() ? 0 : GridApi.GetMeshCount()) : 0;
        }

        public int NumberOfNodes(int mesh)
        {
            return GridApi != null ? ((!IsInitialized() || !IsValid()) ? 0 : GridApi.GetNumberOfNodes(mesh)) : 0;
        }

        public int NumberOfEdges(int mesh)
        {
            return GridApi != null ? ((!IsInitialized() || !IsValid()) ? 0 : GridApi.GetNumberOfEdges(mesh)) : 0;
        }

        public int NumberOfFaces(int mesh)
        {
            return GridApi != null ? ((!IsInitialized() || !IsValid()) ? 0 : GridApi.GetNumberOfFaces(mesh)) : 0;
        }

        public int NumberOfMaxFaceNodes(int mesh)
        {
            return GridApi != null ? ((!IsInitialized() || !IsValid()) ? 0 : GridApi.GetMaxFaceNodes(mesh)) : 0;
        }
        
        public bool GetAllNodeCoordinates(int mesh)
        {
            if (!IsInitialized() || !IsValid()) return false;
            if (GridApi == null) return false;
            var nNode = NumberOfNodes(mesh);
            if(nNode == 0) return false;
            
            //retrieve x
            var xCoordinates = GridApi.GetNodeXCoordinates(mesh);
            
            //retrieve y
            var yCoordinates = GridApi.GetNodeYCoordinates(mesh);
            
            //retrieve z
            var zCoordinates = GridApi.GetNodeZCoordinates(mesh);
            
            
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
            if (GridApi == null) return;
            EdgeNodes[mesh - 1] = GridApi.GetEdgeNodesForMesh(mesh);
        }

        public void GetFaceNodesForMesh(int mesh)
        {
            if (!IsInitialized() || !IsValid()) return;
            if (GridApi == null) return;
            FaceNodes[mesh - 1] = GridApi.GetFaceNodesForMesh(mesh);
        }
        
        public int NumberOfNamesAtLocation(int mesh, int location)
        {
            return GridApi != null ? GridApi.GetVarCount(mesh, location) : 0;
        }

        public void GetNamesAtLocation(int mesh, int location)
        {
            if (GridApi == null) return;
            var varIds = GridApi.GetVarNames(mesh, location);
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
            if (GridApi == null) return;
            GridApi.WriteXYCoordinateValues(mesh, xValues, yValues);
        }

        public void WriteZValues(int mesh, double[] zValues)
        {
            if (GridApi == null) return;
            GridApi.WriteZCoordinateValues(mesh, zValues);
        }

        public string NameOfMesh(int mesh)
        {
            return GridApi != null ? GridApi.GetMeshName(mesh) : string.Empty;
        }
    }
}