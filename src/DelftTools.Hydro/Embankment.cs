using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    public class Embankment : Feature2D, IHydroObject
    {
        public virtual IHydroRegion Region { get; set; }
    }
}