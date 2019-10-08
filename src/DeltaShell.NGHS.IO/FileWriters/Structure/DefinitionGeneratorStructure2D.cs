using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public abstract class DefinitionGeneratorStructure2D : DefinitionGeneratorStructure
    {

        protected override void AddCommonRegionElements(IHydroObject hydroObject, string definitionType)
        {
            AddIdPropertyToIniCategory(hydroObject);
            AddDefinitionTypePropertyToIniCategory(definitionType);

            if (hydroObject.Geometry.Coordinates.Length >= 2)
            {
                IniCategory.AddProperty(StructureRegion.NumberOfCoordinates.Key, hydroObject.Geometry.Coordinates.Length, StructureRegion.NumberOfCoordinates.Description);
                IniCategory.AddProperty(StructureRegion.XCoordinates.Key, hydroObject.Geometry.Coordinates.Select(c => c.X), StructureRegion.XCoordinates.Description,"F4");
                IniCategory.AddProperty(StructureRegion.YCoordinates.Key, hydroObject.Geometry.Coordinates.Select(c => c.Y), StructureRegion.YCoordinates.Description,"F4");
            }
        }
    }
}