using DelftTools.Hydro;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public interface IDefinitionGeneratorStructure
    {
        DelftIniCategory CreateStructureRegion(IHydroObject hydroObject);
    }
}