using System;
using DelftTools.Utils.Remoting;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGridApi : RemoteGridApi, IUGridApi
    {
        public RemoteUGridApi()
        {
            // DeltaShell is 32bit, however we still want to take advantage of the 64bit dflowfm.dll if the system can use it, 
            // so we need to start the 64bit worker. This works as long as the data send over the IFlexibleMeshModelApi border 
            // is not bit dependent, eg IntPtr and the like.
            api = RemoteInstanceContainer.CreateInstance<IUGridApi, UGridApi>(Environment.Is64BitOperatingSystem);
        }
       
        public int GetNumberOfNodes(int meshid, out int numberOfNodes)
        {
            numberOfNodes = -1;
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.GetNumberOfNodes(meshid, out numberOfNodes)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetNumberOfEdges(int meshid, out int numberOfEdges)
        {
            numberOfEdges = -1;
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.GetNumberOfEdges(meshid, out numberOfEdges)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetNumberOfFaces(int meshid, out int numberOfFaces)
        {
            numberOfFaces = -1;
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.GetNumberOfFaces(meshid, out numberOfFaces)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetMaxFaceNodes(int meshid, out int maxFaceNodes)
        {
            maxFaceNodes = -1;
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.GetMaxFaceNodes(meshid, out maxFaceNodes)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetNodeXCoordinates(int meshId, out double[] xCoordinates)
        {
            xCoordinates = new double[0];
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.GetNodeXCoordinates(meshId, out xCoordinates)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetNodeYCoordinates(int meshId, out double[] yCoordinates)
        {
            yCoordinates = new double[0];
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.GetNodeYCoordinates(meshId, out yCoordinates)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetNodeZCoordinates(int meshId, out double[] zCoordinates)
        {
            zCoordinates = new double[0];
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.GetNodeZCoordinates(meshId, out zCoordinates)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public double zCoordinateFillValue
        {
            get
            {
                var ugridApi = api as IUGridApi;
                return ugridApi != null ? ugridApi.zCoordinateFillValue : double.NaN;
            }
            set
            {
                var ugridApi = api as IUGridApi;
                if (ugridApi != null)
                    ugridApi.zCoordinateFillValue = value;
            }
        }

        public int GetEdgeNodesForMesh(int meshId, out int[,] edgeNodes)
        {
            edgeNodes = new int[0,0];
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.GetEdgeNodesForMesh(meshId, out edgeNodes)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetFaceNodesForMesh(int meshId, out int[,] faceNodes)
        {
            faceNodes = new int[0,0];
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.GetFaceNodesForMesh(meshId, out faceNodes)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetVarCount(int meshId, int locationId, out int nCount)
        {
            nCount = -1;
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.GetVarCount(meshId, locationId, out nCount)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetVarNames(int meshId, int locationId, out int[] varIds)
        {
            varIds = new int[0];
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.GetVarNames(meshId, locationId, out varIds)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int WriteXYCoordinateValues(int meshId, double[] xValues, double[] yValues)
        {
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.WriteXYCoordinateValues(meshId, xValues, yValues)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int WriteZCoordinateValues(int meshId, double[] zValues)
        {
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.WriteZCoordinateValues(meshId, zValues)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int GetMeshName(int meshId, out string meshName)
        {
            meshName = string.Empty;
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                    ? uGridApi.GetMeshName(meshId, out meshName)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int ionc_write_geom_ugrid(string filename)
        {
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.ionc_write_geom_ugrid(filename)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        public int ionc_write_map_ugrid(string filename)
        {
            bool isValid;
            var uGridApi = GetValidUGridApi(out isValid);
            return isValid
                ? uGridApi.ionc_write_map_ugrid(filename)
                : GridApiDataSet.GridConstants.IONC_GENERAL_FATAL_ERR;
        }

        private IUGridApi GetValidUGridApi(out bool isValid)
        {
            var uGridApi = api as IUGridApi;
            isValid = uGridApi != null;
           return uGridApi;
        }
    }
}