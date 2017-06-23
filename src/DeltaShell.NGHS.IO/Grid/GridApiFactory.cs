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

        public static IUGridApiNetwork CreateNewNetwork()
        {
            /*return (Environment.Is64BitProcess != Environment.Is64BitOperatingSystem)
                    ? (IUGridApiNetwork)new RemoteUGridApi1D()
                    : new UGridApiNetwork();*/
            return new UGridApiNetwork();
        }

        public static IUGridApiNetworkDiscretisation CreateNewNetworkDiscretisation()
        {
            return new UGridApiNetworkDiscretisation();
        }
    }
}