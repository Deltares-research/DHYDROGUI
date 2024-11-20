using System;
using DelftTools.Utils.Remoting;
using DeltaShell.Dimr;
using ProtoBufRemote;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Api
{
    /// <summary>
    /// <see cref="FlexibleMeshModelApi"/> provides the method to construct <see cref="IFlexibleMeshModelApi"/> objects.
    /// </summary>
    public static class FlexibleMeshModelApiFactory
    {
        /// <summary>
        /// Creates a new <see cref="IFlexibleMeshModelApi"/> object.
        /// </summary>
        /// <param name="runRemote"> If set to <c>true</c> run the <see cref="IFlexibleMeshModelApi"/> as a remote process. </param>
        /// <returns> A new instance of  <see cref="IFlexibleMeshModelApi"/></returns>
        public static IFlexibleMeshModelApi CreateNew(bool runRemote = true)
        {
            if (!Environment.Is64BitOperatingSystem || !runRemote && !Environment.Is64BitProcess)
            {
                return null;
            }

            if (runRemote)
            {
                // DeltaShell is 32bit, however we still want to take advantage of the 64bit dflowfm.dll if the system can use it, 
                // so we need to start the 64bit worker. This works as long as the data send over the IFlexibleMeshModelApi border 
                // is not bit dependent, eg IntPtr and the like.
                RemotingTypeConverters.RegisterTypeConverter(new LoggerToProtoConverter());

                return new RemoteFlexibleMeshModelApi(
                    RemoteInstanceContainer.CreateInstance<IFlexibleMeshModelApi, FlexibleMeshModelApi>());
            }

            return new FlexibleMeshModelApi();
        }
    }
}