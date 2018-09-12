using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface IOrifice : IWeir
    {
        double BottomLevel { get; set; }
        double MaxDischarge { get; set; }
    }
}