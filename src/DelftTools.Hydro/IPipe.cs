using DelftTools.Hydro.CrossSections;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface IPipe : IBranch
    {
        string PipeId { get; set; }
        CrossSection CrossSectionShape { get; set; }
        PipeType PipeType { get; set; }
        double Length { get; set; }
        double LevelSource { get; set; }
        double LevelTarget { get; set; }
    }

    public enum PipeType
    {
        Orifice,
        ClosedConnection,
        InfiltrationPipe,
        Open,
        Crest,
        Pump
    }
}