using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface IPipe : ISewerConnection
    {
        string PipeId { get; set; }

        string CrossSectionDefinitionName { get; set; }

        ICrossSectionDefinition CrossSectionDefinition { get; set; }
        CrossSectionDefinitionStandard Profile { get; }

        SewerProfileMapping.SewerProfileMaterial Material { get; set; }
    }
}