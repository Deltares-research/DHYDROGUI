using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class GridApiFactory
    {
        public static IUGridApi CreateNew()
        {
            return new UGridApi();
            return (Environment.Is64BitProcess != Environment.Is64BitOperatingSystem)
                    ? (IUGridApi)new RemoteUGridApi()
                    : new UGridApi();
        }

        public static IUGridApi1D CreateNew1D()
        {
            /*return (Environment.Is64BitProcess != Environment.Is64BitOperatingSystem)
                    ? (IUGridApi1D)new RemoteUGridApi1D()
                    : new UGridApi1D();*/
            return new UGridApi1D();
        }

        public static IUGridApi1DMesh CreateNew1DMesh()
        {
            return new UGridApi1DMesh();
        }
    }
}