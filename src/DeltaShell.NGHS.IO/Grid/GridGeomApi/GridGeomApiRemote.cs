using System;
using DelftTools.Hydro;
using DelftTools.Utils.Remoting;

namespace DeltaShell.NGHS.IO.Grid.GridGeomApi
{
    public sealed class GridGeomApiRemote : IGridGeomApi
    {
        private IGridGeomApi gridGeomApi;

        public GridGeomApiRemote()
        {
            gridGeomApi = RemoteInstanceContainer.CreateInstance<IGridGeomApi, GridGeomApi>();
        }
        
        /// <inheritdoc/>
        public LinkInformation GetLinkInformation(DisposableMeshGeometryGridGeom mesh2D, Mesh1DGeometry mesh1D,
            GeometriesData selectedArea, bool[] filter1DMesh, LinkGeneratingType linkType,
            GeometriesData geometryGullies = null)
        {
            return gridGeomApi.GetLinkInformation(mesh2D, mesh1D, selectedArea, filter1DMesh, linkType, geometryGullies);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            try
            {
                if (RemoteInstanceContainer.IsProcessAlive(gridGeomApi))
                {
                    gridGeomApi?.Dispose();
                }
            }
            catch (InvalidOperationException)
            {
                // remote connection lost, so all data of process has cleaned-up
            }
            finally
            {
                if (RemoteInstanceContainer.NumInstances != 0)
                {
                    // remove instance from RemoteInstanceContainer
                    RemoteInstanceContainer.RemoveInstance(gridGeomApi);
                }
            }
        }

    }
}