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

            var validNameCharacters = hydroObject.Name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray();
            if (!validNameCharacters.Any()) return;

            var pliFileName = $"{new string(validNameCharacters)}.pli";
            IniCategory.AddProperty(StructureRegion.PolylineFile.Key, pliFileName, StructureRegion.PolylineFile.Description);
        }
    }
}