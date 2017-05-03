using System;

namespace DeltaShell.Dimr
{
    public static class DimrApiFactory
    {
        public static IDimrApi CreateNew(bool useMessagesBuffering = false, bool runRemote = true)
        {
            if(useMessagesBuffering)
                return new DimrExe(useMessagesBuffering);

            return runRemote 
                ? (IDimrApi)new RemoteDimrApi(Environment.Is64BitProcess) 
                : new DimrApi(false);
        }
    }
}