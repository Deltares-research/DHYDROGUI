using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Location on a branch were extra water is entering or leaving the network
    /// </summary>
    public interface ILateralSource : IHydroNetworkFeature, IBranchFeature
    {
        /// <summary>
        /// Defines if lateral source is diffuse.
        /// </summary>
        bool IsDiffuse { get; }
    }
}