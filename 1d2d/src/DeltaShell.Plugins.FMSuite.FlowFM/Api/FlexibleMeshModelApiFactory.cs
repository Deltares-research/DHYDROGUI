using System;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    public static class FlexibleMeshModelApiFactory
    {
        public static IFlexibleMeshModelApi CreateNew(bool runRemote = true)
        {
            if (!Environment.Is64BitOperatingSystem || (!runRemote && !Environment.Is64BitProcess)) return null;

            return runRemote
                ? (IFlexibleMeshModelApi)new RemoteFlexibleMeshModelApi()
                : new FlexibleMeshModelApi();
        }
    }
}