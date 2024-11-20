using System;

namespace DeltaShell.Dimr
{
    public class DimrApiFactory : IDimrApiFactory
    {
        public IDimrApi CreateNew(bool runRemote = true)
        {
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