using DelftTools.Functions;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Implements a retention area
    /// </summary>
    public interface IRetention : IHydroNetworkFeature, IBranchFeature
    {
        RetentionType Type { get; set; }
        double StorageArea { get; set; }
        double BedLevel { get; set; }

        /// <summary>
        /// Storage bed definition
        /// </summary>
        IFunction Data { get; set; }

        double StreetLevel { get; set; }
        double StreetStorageArea { get; set; }
        /// <summary>
        /// Use storage as function of level
        /// </summary>
        bool UseTable { get; set; }
    }
}