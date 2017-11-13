using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridApiFactory
    {
        public static IGridApi CreateNew(bool runRemote = true)
        {
            if (!Environment.Is64BitOperatingSystem || (!runRemote && !Environment.Is64BitProcess)) return null;
            
            return runRemote
                ? (IGridApi)new RemoteGridApi()
                : new GridApi();
        }
    }
}