using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class GridApiFactory
    {
        public static IUGridApi CreateNew(bool runRemote = true)
        {
            if (!Environment.Is64BitOperatingSystem || (!runRemote && !Environment.Is64BitProcess)) return null;
            
            return runRemote
                ? (IUGridApi)new RemoteUGridApi()
                : new UGridApi();
        }

        public static IUGridNetworkApi CreateNewNetwork()
        {
            /*return (Environment.Is64BitProcess != Environment.Is64BitOperatingSystem)
                    ? (IUGridNetworkApi)new RemoteUGridNetworkApi()
                    : new UGridNetworkApi();*/
            return new UGridNetworkApi();
        }

        public static IUGridNetworkDiscretisationApi CreateNewNetworkDiscretisation()
        {
            return new UGridNetworkDiscretisationApi();
        }
    }
}