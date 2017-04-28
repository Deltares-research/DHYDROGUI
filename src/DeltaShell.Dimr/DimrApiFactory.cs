using System;

namespace DeltaShell.Dimr
{
    public class DimrApiFactory
    {
        public static IDimrApi CreateNew(bool useMessagesBuffering = false, bool runRemote = true)
        {
            if(useMessagesBuffering)
                return new DimrExe(useMessagesBuffering);
            return runRemote ? (IDimrApi)new RemoteDimrApi(Environment.Is64BitProcess) : new DimrApi(false);
            //return new RemoteDimrApi(Environment.Is64BitProcess);
            //return new DimrApi(false);
        }
    }
}