using DelftTools.Utils.Remoting;
using Deltares.UGrid.Api;

namespace DeltaShell.NGHS.IO.Grid.DeltaresUGrid
{
    public sealed class RemoteUGridApi : Deltares.UGrid.Api.IUGridApi
    {
        private Deltares.UGrid.Api.IUGridApi api;

        public RemoteUGridApi()
        {
            api = RemoteInstanceContainer.CreateInstance<Deltares.UGrid.Api.IUGridApi, Deltares.UGrid.Api.UGridApi>(true);
        }
        
        public void Dispose()
        {
            api?.Dispose();
            RemoteInstanceContainer.RemoveInstance(api);
            api = null;
        }

        public bool IsUGridFile()
        {
            return api.IsUGridFile();
        }

        public bool CreateFile(string filePath, FileMetaData fileMetaData)
        {
            return api.CreateFile(filePath, fileMetaData);
        }

        public bool Open(string filePath)
        {
            return api.Open(filePath);
        }

        public bool Close()
        {
            return api.Close();
        }

        public double GetVersion()
        {
            return api.GetVersion();
        }

        public int GetMeshCount()
        {
            return api.GetMeshCount();
        }

        public int GetNumberOfMeshByType(Deltares.UGrid.Api.UGridMeshType meshType)
        {
            return api.GetNumberOfMeshByType(meshType);
        }

        public int[] GetMeshIdsByMeshType(Deltares.UGrid.Api.UGridMeshType meshType)
        {
            return api.GetMeshIdsByMeshType(meshType);
        }

        public int GetVarCount(int meshId, GridLocationType locationType)
        {
            return api.GetVarCount(meshId, locationType);
        }

        public int[] GetVarIds(int meshId, GridLocationType locationType)
        {
            return api.GetVarIds(meshId, locationType);
        }

        public int GetCoordinateSystemCode()
        {
            return api.GetCoordinateSystemCode();
        }

        public int[] GetNetworkIds()
        {
            return api.GetNetworkIds();
        }

        public int GetNumberOfNetworks()
        {
            return api.GetNumberOfNetworks();
        }

        public DisposableNetworkGeometry GetNetworkGeometry(int networkId)
        {
            return api.GetNetworkGeometry(networkId);
        }

        public int WriteNetworkGeometry(DisposableNetworkGeometry geometry)
        {
            return api.WriteNetworkGeometry(geometry);
        }

        public int GetNetworkIdFromMeshId(int meshId)
        {
            return api.GetNetworkIdFromMeshId(meshId);
        }

        public Disposable1DMeshGeometry GetMesh1D(int meshId)
        {
            return api.GetMesh1D(meshId);
        }

        public int WriteMesh1D(Disposable1DMeshGeometry mesh, int networkId)
        {
            return api.WriteMesh1D(mesh,networkId);
        }

        public Disposable2DMeshGeometry GetMesh2D(int meshId)
        {
            return api.GetMesh2D(meshId);
        }

        public int WriteMesh2D(Disposable2DMeshGeometry mesh)
        {
            return api.WriteMesh2D(mesh);
        }

        public int GetLinksId()
        {
            return api.GetLinksId();
        }

        public DisposableLinksGeometry GetLinks(int linksId)
        {
            return api.GetLinks(linksId);
        }

        public int WriteLinks(DisposableLinksGeometry links)
        {
            return api.WriteLinks(links);
        }
    }
}