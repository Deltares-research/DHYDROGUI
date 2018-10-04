using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    public static class StructureFileWriter
    {
        public static void WriteFile(string targetIniFile, IModel modelWithStructures, Func<IModel, IEnumerable<DelftIniCategory>> createStructureCategoriesFunction)
        {
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(GeneralRegion.StructureDefinitionsMajorVersion, 
                                                             GeneralRegion.StructureDefinitionsMinorVersion, 
                                                             GeneralRegion.FileTypeName.StructureDefinition)
            };

            categories.AddRange(createStructureCategoriesFunction(modelWithStructures));
            
            if (File.Exists(targetIniFile)) File.Delete(targetIniFile);
            new IniFileWriter().WriteIniFile(categories, targetIniFile);
        }
    }
}
