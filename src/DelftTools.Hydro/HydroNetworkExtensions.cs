using System;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public static class HydroNetworkExtensions
    {
        public static void UpdateGeodeticDistancesOfChannels(this IHydroNetwork network)
        {
            throw new NotImplementedException();
        }

        /// Ensure that all
        /// <see cref="ICompositeBranchStructure"/>
        /// have a unique name
        /// </summary>
        /// <param name="network"> Network to check </param>
        /// <param name="enableLogging"> Add log message for changed <see cref="ICompositeBranchStructure"/> names </param>
        public static void MakeNamesUnique<T>(this IHydroNetwork network, bool enableLogging = true)
            where T : class, IBranchFeature
        {
            throw new NotImplementedException();
        }
    }
}