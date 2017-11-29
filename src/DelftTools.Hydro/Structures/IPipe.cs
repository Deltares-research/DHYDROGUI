using DelftTools.Hydro.CrossSections;

namespace DelftTools.Hydro.Structures
{
    public interface IPipe : ISewerConnection
    {
        string PipeId { get; set; }
        CrossSection SewerProfile { get; set; }
    }
}