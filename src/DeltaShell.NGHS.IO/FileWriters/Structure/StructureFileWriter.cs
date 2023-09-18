using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileWriters.General;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="StructureFileWriter"/> provides a convenience <see cref="WriteFile"/>
    /// method to generate <see cref="IniSection"/> and write the files to the
    /// specified target path.
    /// </summary>
    public static class StructureFileWriter
    {
        /// <summary>
        /// Write the set of <see cref="IniSection"/> generated with
        /// <paramref name="createStructureIniSectionsFunction"/> to the specified
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
        /// <param name="createStructureIniSectionsFunction">
        /// The function to generate the INI sections with.
        /// </param>
        public static void WriteFile(string targetIniFile, 
                                     IEnumerable<IHydroRegion> regionsWithStructures, 
                                     DateTime referenceTime, 
                                     Func<IEnumerable<IHydroRegion>, DateTime, IEnumerable<IniSection>> createStructureIniSectionsFunction)
        {
            var iniSections = new List<IniSection>
            {
                GeneralRegionGenerator.GenerateGeneralRegion(
                    GeneralRegion.StructureDefinitionsMajorVersion, 
                    GeneralRegion.StructureDefinitionsMinorVersion, 
                    GeneralRegion.FileTypeName.StructureDefinition)
            };

            iniSections.AddRange(createStructureIniSectionsFunction(regionsWithStructures, referenceTime));
            
            if (File.Exists(targetIniFile)) File.Delete(targetIniFile);
            new IniFileWriter().WriteIniFile(iniSections, targetIniFile);
        }
    }
}
