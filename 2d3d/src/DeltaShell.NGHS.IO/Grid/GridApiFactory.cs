using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public static class GridApiFactory
    {
        public static IUGridApi CreateNew(bool runRemote = true)
        {
            if (!Environment.Is64BitOperatingSystem || !runRemote && !Environment.Is64BitProcess)
            {
                return null;
            }

            return runRemote
                       ? (IUGridApi) new RemoteUGridApi()
                       : new UGridApi();
        }
    }
}