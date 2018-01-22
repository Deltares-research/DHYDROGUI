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
                : new UGridApi();        }

        public static IUGridNetworkApi CreateNewNetwork()
        {
            // TODO: consider allowing remote running explicitly (like the 'CreateNew' function above)
            return (Environment.Is64BitProcess != Environment.Is64BitOperatingSystem)
                    ? (IUGridNetworkApi)new RemoteUGridNetworkApi()
                    : new UGridNetworkApi();
        }

        public static IUGridNetworkDiscretisationApi CreateNewNetworkDiscretisation()
        {
            // TODO: consider allowing remote running explicitly (like the 'CreateNew' function above)
            return (Environment.Is64BitProcess != Environment.Is64BitOperatingSystem)
                   ? (IUGridNetworkDiscretisationApi)new RemoteUGridNetworkDiscretisationApi()
                   : new UGridNetworkDiscretisationApi();
        }
    }
}