using System;
using DelftTools.Utils.Guards;
using GeoAPI.Extensions.Coverages;

namespace DeltaShell.NGHS.Utils
{
    public static class NetworkLocationExtensions
    {
        public static bool IsOnEndOfBranch(this INetworkLocation networkLocation)
        {
            Ensure.NotNull(networkLocation, nameof(networkLocation));
            Ensure.NotNull(networkLocation.Branch, nameof(networkLocation.Branch));

            return networkLocation.Branch != null 
                   && Math.Abs(networkLocation.Chainage - networkLocation.Branch.Length) < 1e-10;
        }
    }
}