using System.Reflection;
using DelftTools.Utils.Remoting;
using DeltaShell.Dimr;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGridApi : RemoteGridApi, IUGridApi
    {
        public RemoteUGridApi()
        {
            // We need to pass the Dimr Assembly here, in order to get the SharedDllPath
            Assembly dimrDllAssembly = typeof(DimrRunner).Assembly;

            api = RemoteInstanceContainer.CreateInstance<IUGridApi, UGridApi>(null, false, dimrDllAssembly);
        }

        public double ZCoordinateFillValue
        {
            get
            {
                var ugridApi = api as IUGridApi;
                return ugridApi?.ZCoordinateFillValue ?? double.NaN;
            }
            set
            {
                var ugridApi = api as IUGridApi;
                if (ugridApi != null)
                {
                    ugridApi.ZCoordinateFillValue = value;
                }
            }
        }

        public int GetNumberOfNodes(int meshId, out int numberOfNodes)
        {
            numberOfNodes = -1;
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetNumberOfNodes(meshId, out numberOfNodes)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetNumberOfEdges(int meshId, out int numberOfEdges)
        {
            numberOfEdges = -1;
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetNumberOfEdges(meshId, out numberOfEdges)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetNumberOfFaces(int meshId, out int numberOfFaces)
        {
            numberOfFaces = -1;
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetNumberOfFaces(meshId, out numberOfFaces)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetMaxFaceNodes(int meshId, out int maxFaceNodes)
        {
            maxFaceNodes = -1;
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetMaxFaceNodes(meshId, out maxFaceNodes)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetNodeXCoordinates(int meshId, out double[] xCoordinates)
        {
            xCoordinates = new double[0];
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetNodeXCoordinates(meshId, out xCoordinates)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetNodeYCoordinates(int meshId, out double[] yCoordinates)
        {
            yCoordinates = new double[0];
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetNodeYCoordinates(meshId, out yCoordinates)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetNodeZCoordinates(int meshId, out double[] zCoordinates)
        {
            zCoordinates = new double[0];
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetNodeZCoordinates(meshId, out zCoordinates)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetEdgeNodesForMesh(int meshId, out int[,] edgeNodes)
        {
            edgeNodes = new int[0, 0];
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetEdgeNodesForMesh(meshId, out edgeNodes)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetFaceNodesForMesh(int meshId, out int[,] faceNodes)
        {
            faceNodes = new int[0, 0];
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetFaceNodesForMesh(meshId, out faceNodes)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetVarCount(int meshId, GridApiDataSet.LocationType locationType, out int nCount)
        {
            nCount = -1;
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetVarCount(meshId, locationType, out nCount)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetVarNames(int meshId, GridApiDataSet.LocationType locationType, out int[] varIds)
        {
            varIds = new int[0];
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetVarNames(meshId, locationType, out varIds)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int WriteXYCoordinateValues(int meshId, double[] xValues, double[] yValues)
        {
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.WriteXYCoordinateValues(meshId, xValues, yValues)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int WriteZCoordinateValues(int meshId, GridApiDataSet.LocationType locationType, string varName, string longName, double[] zValues)
        {
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.WriteZCoordinateValues(meshId, locationType, varName, longName, zValues)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int ReadZCoordinateValues(int meshId, GridApiDataSet.LocationType locationType, string varName, out double[] zValues)
        {
            zValues = new double[0];

            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.ReadZCoordinateValues(meshId, locationType, varName, out zValues)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int GetMeshName(int meshId, out string meshName)
        {
            meshName = string.Empty;
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.GetMeshName(meshId, out meshName)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int write_geom_ugrid(string filename)
        {
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.write_geom_ugrid(filename)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        public int write_map_ugrid(string filename)
        {
            bool isValid;
            IUGridApi uGridApi = GetValidUGridApi(out isValid);
            return isValid
                       ? uGridApi.write_map_ugrid(filename)
                       : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }

        private IUGridApi GetValidUGridApi(out bool isValid)
        {
            var uGridApi = api as IUGridApi;
            isValid = uGridApi != null;
            return uGridApi;
        }
    }
}