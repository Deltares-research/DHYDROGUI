using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface IOrifice : IWeir
    {
        double MaxDischarge { get; set; }
    }
}