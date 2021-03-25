using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public interface IPipe : ISewerConnection
    {
        string PipeId { get; set; }
        ICrossSectionDefinition CrossSectionDefinition { get; }
        ICrossSection CrossSection { get; set; }
        CrossSectionDefinitionStandard Profile { get; }
        Action<object, EventArgs> EditSharedDefinitionClicked { get; set; }
        SewerProfileMapping.SewerProfileMaterial Material { get; set; }
    }
}