using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Observation points are points in a hydro network that can provide data either from a calculation or 
    /// an external datasource.
    /// THey are currently used in WaterFlowModel1D to provide data to RTC 
    /// </summary>
    public interface IObservationPoint : IHydroNetworkFeature, IBranchFeature
    {
        
    }
}