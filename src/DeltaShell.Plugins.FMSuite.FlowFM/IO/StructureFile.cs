using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class StructureFile
    {
        public static IEnumerable<DelftIniCategory> ExtractFunctionStructuresOfAreaGenerator(HydroArea area)
        {
            foreach (var structure2D in area.AllHydroObjects.Cast<IStructure2D>())
            {
                var definitionGeneratorStructure = DefinitionGeneratorFactory.GetDefinitionGeneratorStructure(structure2D.Structure2DType);
                var structureCategory = definitionGeneratorStructure.CreateStructureRegion(structure2D);
                yield return structureCategory;
            }
        }
    }
}
