using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;
using log4net;

namespace DeltaShell.NGHS.IO.FileWriters.Structure
{
    /// <summary>
    /// <see cref="StructureFileWriter"/> provides a convenience <see cref="WriteFile"/>
    /// method to generate <see cref="IniSection"/> and write the files to the
    /// specified target path.
    /// </summary>
    public class StructureFileWriter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StructureFileWriter));

        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureFileWriter"/> class.
        /// </summary>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="fileSystem"/> is <c>null</c>.</exception>
        public StructureFileWriter(IFileSystem fileSystem)
        {
            Ensure.NotNull(fileSystem, nameof(fileSystem));

            this.fileSystem = fileSystem;
        }

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
        public void WriteFile(string targetIniFile, 
                              IEnumerable<IHydroRegion> regionsWithStructures, 
                              DateTime referenceTime, 
                              Func<IEnumerable<IHydroRegion>, DateTime, IEnumerable<IniSection>> createStructureIniSectionsFunction)
        {
            var iniData = new IniData();
            
            iniData.AddSection(GeneralRegionGenerator.GenerateGeneralRegion(
                                   GeneralRegion.StructureDefinitionsMajorVersion,
                                   GeneralRegion.StructureDefinitionsMinorVersion,
                                   GeneralRegion.FileTypeName.StructureDefinition));

            iniData.AddMultipleSections(createStructureIniSectionsFunction(regionsWithStructures, referenceTime));

            log.InfoFormat(Resources.StructureFileWriter_WriteFile_Writing_structure_definitions_to__0__, targetIniFile);
            
            using (FileSystemStream stream = fileSystem.File.Open(targetIniFile, FileMode.Create))
            {
                GetIniFormatter().Format(iniData, stream);
            }
        }

        private static IniFormatter GetIniFormatter()
        {
            return new IniFormatter
            {
                Configuration =
                {
                    WriteComments = false,
                    PropertyIndentationLevel = 4,
                }
            };
        }
    }
}
