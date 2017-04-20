using System;

namespace DeltaShell.NGHS.IO.Grid
{
    public class GridApiFactory
    {
        public static IGridApi CreateNew()
        {
            return (Environment.Is64BitProcess != Environment.Is64BitOperatingSystem)
                    ? (IGridApi)new RemoteGridApi()
                    : new GridApi();
        }
    }
}