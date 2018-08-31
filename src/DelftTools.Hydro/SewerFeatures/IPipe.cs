using DelftTools.Hydro.CrossSections;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface IPipe : ISewerConnection
    {
        string PipeId { get; set; }

        string CrossSectionDefinitionId { get; set; }

        CrossSectionDefinitionStandard CrossSectionDefinition { get; set; }
    }
}