using System;
using DelftTools.Utils.Remoting;
using DeltaShell.Dimr;

namespace DeltaShell.NGHS.IO.Grid
{
    public class RemoteUGrid2DMeshApi : RemoteGridApi, IUGridMesh2DApi
    {

        public RemoteUGrid2DMeshApi()
        {
            // We need to pass the Dimr Assembly here, in order to get the SharedDllPath
            var dimrDllAssembly = typeof(DimrRunner).Assembly;

            api = RemoteInstanceContainer.CreateInstance<IUGridMesh2DApi, UGridMesh2DApi>(Environment.Is64BitOperatingSystem, null, false, dimrDllAssembly);
        }

        public virtual int CreateMesh2D(GridWrapper.meshgeomdim dimensions, GridWrapper.meshgeom data)
        {
            var uGrid2DMeshApi = api as IUGridMesh2DApi;
            return uGrid2DMeshApi != null
                ? uGrid2DMeshApi.CreateMesh2D(dimensions, data)
                : GridApiDataSet.GridConstants.GENERAL_FATAL_ERR;
        }
        

        private T GetFromValidUGrid2DMeshApi<T>(Func<IUGridMesh2DApi, T> function, T defaultValue)
        {
            var ugrid2DMeshApi = api as IUGridMesh2DApi;
            return ugrid2DMeshApi != null ? function(ugrid2DMeshApi) : defaultValue;
        }
    }
}