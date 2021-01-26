using DelftTools.Utils.Remoting;
using Deltares.UGrid.Api;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    /// <summary>
    /// Remote variant of <see cref="UGridApi"/>.
    /// </summary>
    public sealed class RemoteUGridApi : IUGridApi
    {
        private IUGridApi api;

        public RemoteUGridApi()
        {
            api = RemoteInstanceContainer.CreateInstance<IUGridApi, UGridApi>(true);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (RemoteInstanceContainer.IsProcessAlive(api))
            {
                try
                {
                    api?.Dispose();
                }
                catch
                {
                    // ignored because process will be killed (thus all resources will be released)
                }
            }
            
            RemoteInstanceContainer.RemoveInstance(api);
            api = null;
        }

        /// <inheritdoc/>
        public bool IsUGridFile()
        {
            return api.IsUGridFile();
        }

        /// <inheritdoc/>
        public void CreateFile(string filePath, FileMetaData fileMetaData)
        {
            api.CreateFile(filePath, fileMetaData);
        }

        /// <inheritdoc/>
        public void Open(string filePath, OpenMode mode = OpenMode.Reading)
        {
            api.Open(filePath, mode);
        }

        /// <inheritdoc/>
        public void Close()
        {
            api.Close();
        }

        /// <inheritdoc/>
        public double GetVersion()
        {
            return api.GetVersion();
        }

        /// <inheritdoc/>
        public int GetMeshCount()
        {
            return api.GetMeshCount();
        }

        /// <inheritdoc/>
        public int GetNumberOfMeshByType(UGridMeshType meshType)
        {
            return api.GetNumberOfMeshByType(meshType);
        }

        /// <inheritdoc/>
        public int[] GetMeshIdsByMeshType(UGridMeshType meshType)
        {
            return api.GetMeshIdsByMeshType(meshType);
        }

        /// <inheritdoc/>
        public int GetVarCount(int meshId, GridLocationType locationType)
        {
            return api.GetVarCount(meshId, locationType);
        }

        /// <inheritdoc/>
        public int[] GetVarIds(int meshId, GridLocationType locationType)
        {
            return api.GetVarIds(meshId, locationType);
        }

        /// <inheritdoc/>
        public double GetVariableNoDataValue(string variableName, int meshId, GridLocationType location)
        {
            return api.GetVariableNoDataValue(variableName, meshId, location);
        }

        /// <inheritdoc/>
        public double[] GetVariableValues(string variableName, int meshId, GridLocationType location)
        {
            return api.GetVariableValues(variableName, meshId, location);
        }

        /// <inheritdoc/>
        public void SetVariableValues(string variableName, string standardName, string longName, string unit, int meshId,
            GridLocationType location, double[] values, double noDataValue = -999)
        {
            api.SetVariableValues(variableName, standardName, longName, unit, meshId, location, values, noDataValue);
        }

        /// <inheritdoc/>
        public void ResetMeshVerticesCoordinates(int meshId, double[] xValues, double[] yValues)
        {
            api.ResetMeshVerticesCoordinates(meshId, xValues, yValues);
        }

        /// <inheritdoc/>
        public int GetCoordinateSystemCode()
        {
            return api.GetCoordinateSystemCode();
        }

        /// <inheritdoc/>
        public void SetCoordinateSystemCode(int epsgCode)
        {
            api.SetCoordinateSystemCode(epsgCode);
        }

        /// <inheritdoc/>
        public int[] GetNetworkIds()
        {
            return api.GetNetworkIds();
        }

        /// <inheritdoc/>
        public int GetNumberOfNetworks()
        {
            return api.GetNumberOfNetworks();
        }

        /// <inheritdoc/>
        public DisposableNetworkGeometry GetNetworkGeometry(int networkId)
        {
            return api.GetNetworkGeometry(networkId);
        }

        /// <inheritdoc/>
        public int WriteNetworkGeometry(DisposableNetworkGeometry geometry)
        {
            return api.WriteNetworkGeometry(geometry);
        }

        /// <inheritdoc/>
        public int GetNetworkIdFromMeshId(int meshId)
        {
            return api.GetNetworkIdFromMeshId(meshId);
        }

        /// <inheritdoc/>
        public Disposable1DMeshGeometry GetMesh1D(int meshId)
        {
            return api.GetMesh1D(meshId);
        }

        /// <inheritdoc/>
        public int WriteMesh1D(Disposable1DMeshGeometry mesh, int networkId)
        {
            return api.WriteMesh1D(mesh,networkId);
        }

        /// <inheritdoc/>
        public Disposable2DMeshGeometry GetMesh2D(int meshId)
        {
            return api.GetMesh2D(meshId);
        }

        /// <inheritdoc/>
        public int WriteMesh2D(Disposable2DMeshGeometry mesh)
        {
            return api.WriteMesh2D(mesh);
        }

        /// <inheritdoc/>
        public int GetLinksId()
        {
            return api.GetLinksId();
        }

        /// <inheritdoc/>
        public DisposableLinksGeometry GetLinks(int linksId)
        {
            return api.GetLinks(linksId);
        }

        /// <inheritdoc/>
        public int WriteLinks(DisposableLinksGeometry links)
        {
            return api.WriteLinks(links);
        }
    }
}