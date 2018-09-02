using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface IPipe : ISewerConnection
    {
        string PipeId { get; set; }

        string CrossSectionDefinitionId { get; set; }

        CrossSectionDefinitionStandard CrossSectionDefinition { get; set; }

        SewerProfileMapping.SewerProfileMaterial Material { get; set; }
    }
}