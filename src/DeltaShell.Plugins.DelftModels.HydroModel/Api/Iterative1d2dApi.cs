using DelftTools.Utils.Interop;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Api
{
    public class Iterative1d2dApi
    {
        public const string ITERATIVE1D2D_DLL_NAME = "flow1d2d.dll";
        
        static Iterative1d2dApi()
        {
            DimrApiDataSet.SetSharedPath();
            NativeLibrary.LoadNativeDll(ITERATIVE1D2D_DLL_NAME, DimrApiDataSet.Iterative1D2DDllPath);
        }
    }
}
