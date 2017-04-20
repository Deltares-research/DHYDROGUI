using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public interface IBranchStructureBuilder
    {
        IEnumerable<BranchStructure> GetBranchStructures(SobekStructureDefinition structure);
    }
}