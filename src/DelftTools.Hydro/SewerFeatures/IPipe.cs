using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface IPipe : ISewerConnection
    {
        string PipeId { get; set; }

        string CrossSectionDefinitionName { get; set; }

        ICrossSection CrossSection { get; set; }
        ICrossSectionDefinition CrossSectionDefinition { get; }
        CrossSectionDefinitionStandard Profile { get; }
        Action<object, EventArgs> EditSharedDefinitionClicked { get; set; }
        SewerProfileMapping.SewerProfileMaterial Material { get; set; }
    }
}