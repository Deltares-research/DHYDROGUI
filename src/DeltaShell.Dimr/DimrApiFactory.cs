using System;

namespace DeltaShell.Dimr
{
    public static class DimrApiFactory
    {
        public static IDimrApi CreateNew(bool runRemote = true)
        {
            /*if(useMessagesBuffering)
                return new DimrExe(useMessagesBuffering);*/

            if (!Environment.Is64BitOperatingSystem || !runRemote && !Environment.Is64BitProcess)
            {
                return null;
            }

            return runRemote
                       ? (IDimrApi) new RemoteDimrApi()
                       : new DimrApi(false);
        }
    }
}