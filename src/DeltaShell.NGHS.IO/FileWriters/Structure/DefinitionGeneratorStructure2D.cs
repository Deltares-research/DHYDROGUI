using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public abstract class DefinitionGeneratorStructure2D : DefinitionGeneratorStructure
    {

        protected override void AddCommonRegionElements(IHydroObject hydroObject, string definitionType)
        {
            AddIdPropertyToIniSection(hydroObject);
            AddDefinitionTypePropertyToIniSection(definitionType);

            if (hydroObject.Geometry.Coordinates.Length >= 2)
            {
                IniSection.AddPropertyWithOptionalComment(StructureRegion.NumberOfCoordinates.Key, hydroObject.Geometry.Coordinates.Length, StructureRegion.NumberOfCoordinates.Description);
                IniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(StructureRegion.XCoordinates.Key, hydroObject.Geometry.Coordinates.Select(c => c.X), StructureRegion.XCoordinates.Description,"F4");
                IniSection.AddPropertyWithMultipleValuesWithOptionalCommentAndFormat(StructureRegion.YCoordinates.Key, hydroObject.Geometry.Coordinates.Select(c => c.Y), StructureRegion.YCoordinates.Description,"F4");
            }
        }
    }
}