using System.Collections.Generic;
using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.Structures
{
    public interface ICompositeNetworkPointFeature
    {
        IEnumerable<IFeature> GetPointFeatures();

        NetworkFeatureType NetworkFeatureType { get; }
    }
}