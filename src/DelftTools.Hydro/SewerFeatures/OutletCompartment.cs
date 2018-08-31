using GeoAPI.Extensions.Feature;

namespace DelftTools.Hydro.SewerFeatures
{
    public class OutletCompartment : Compartment
    {
        public OutletCompartment() : this("outletCompartment")
        {
        }

        public OutletCompartment(string uniqueId) : base(uniqueId)
        {
        }

        [FeatureAttribute]
        public double SurfaceWaterLevel { get; set; }
    }
}