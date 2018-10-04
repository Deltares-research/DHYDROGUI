using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Features;

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
