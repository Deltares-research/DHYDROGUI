using DelftTools.Hydro.CrossSections;
using GeoAPI.Extensions.Networks;

namespace DelftTools.Hydro
{
    public interface IPipe : ISewerConnection, IBranch
    {
        string PipeId { get; set; }
        CrossSection CrossSectionShape { get; set; }
    }
}