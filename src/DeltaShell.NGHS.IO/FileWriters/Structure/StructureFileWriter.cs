using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="StructureFileWriter"/> provides a convenience <see cref="WriteFile"/>
    /// method to generate <see cref="DelftIniCategory"/> and write the files to the
    /// specified target path.
    /// </summary>
    public static class StructureFileWriter
    {
        /// <summary>
        /// Write the set of <see cref="DelftIniCategory"/> generated with
        /// <paramref name="createStructureCategoriesFunction"/> to the specified
        /// <paramref name="targetIniFile"/>.
        /// </summary>
        /// <param name="targetIniFile">
        /// The path to which to write the structures.ini file.
        /// </param>
        /// <param name="regionsWithStructures">
        /// The <see cref="IHydroRegion"/> to obtain the structures from.
        /// </param>
        /// <param name="referenceTime">
        /// The reference time used to write the time series contained in the model.
        /// </param>
        /// <param name="createStructureCategoriesFunction">
        /// The function to generate the categories with.
        /// </param>
        public static void WriteFile(string targetIniFile, 
                                     IEnumerable<IHydroRegion> regionsWithStructures, 
                                     DateTime referenceTime, 
                                     Func<IEnumerable<IHydroRegion>, DateTime, IEnumerable<DelftIniCategory>> createStructureCategoriesFunction)
        {
            var categories = new List<DelftIniCategory>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.StructureDefinitionsMajorVersion, 
                    GeneralRegion.StructureDefinitionsMinorVersion, 
                    GeneralRegion.FileTypeName.StructureDefinition)
            };

            categories.AddRange(createStructureCategoriesFunction(regionsWithStructures, referenceTime));
            
            if (File.Exists(targetIniFile)) File.Delete(targetIniFile);
            new IniFileWriter().WriteIniFile(categories, targetIniFile);
        }
    }
}
