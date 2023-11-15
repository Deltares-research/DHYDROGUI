using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence.CustomComparers;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence
{
    /// <summary>
    /// Helper class for comparing the contents of two FlowFM file directories.
    /// </summary>
    public static class FlowFmFileComparer
    {
        private static readonly string[] MduLinesToIgnore =
        {
            "# Generated on",                            // Timestamp dependent
            "# Deltares,Delft3D FM 2018 Suite Version",  // Revision dependent
            "*",                                         // Bug (header is added each write action)
            "Version",                                   // Revision dependent
            "GuiVersion",                                // Revision dependent
            "TStart",                                    // Timestamp dependent
            "TStop",                                     // Timestamp dependent
            "RestartDateTime",                           // Timestamp dependent
            "RefDate",                                   // Timestamp dependent
            "FrictFile"                                  // Bug (changed order after read/write action)
        };

        private static readonly string[] NetCdfLinesToIgnore =
        {
            ":history = \"Created on",
            ":source = \"D-Flow Flexible Mesh Plugin"
        };

        /// <summary>
        /// Compares the contents of two FlowFM file directories.
        /// </summary>
        /// <param name="expectedFlowFmFiles">The file paths of the expected FlowFM files.</param>
        /// <param name="actualFlowFmFiles">The file paths of the actual FlowFM files.</param>
        /// <param name="tempDirectory">A temporary working directory to use during the comparison.</param>
        /// <param name="linesToIgnoreLookup">Lookup for which lines to ignore for a specific file. Key: filename, Value: lines to ignore for that file.</param>
        /// <remarks>
        /// Files are also considered to be equal when the relevant file contents are equivalent (i.o.w. same file contents but in different order).
        /// </remarks>
        public static void Compare(string[] expectedFlowFmFiles, string[] actualFlowFmFiles, string tempDirectory, IReadOnlyDictionary<string, IEnumerable<string>> linesToIgnoreLookup)
        {
            var actualFlowFmFileNames = actualFlowFmFiles.Select(Path.GetFileName);
            var expectedFlowFmFileNames = expectedFlowFmFiles.Select(Path.GetFileName);
            var allFileNames = actualFlowFmFileNames.Union(expectedFlowFmFileNames).ToArray();

            foreach (var fileName in allFileNames)
            {
                if (string.Equals(Path.GetExtension(fileName), ".cache", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue; // ignore the cache file
                }
                
                var linesToIgnore = new List<string>();
                
                if (linesToIgnoreLookup.TryGetValue(fileName, out IEnumerable<string> linesInFileToIgnore))
                {
                    linesToIgnore.AddRange(linesInFileToIgnore);
                }

                string expectedFlowFmFile = expectedFlowFmFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
                string actualFlowFmFile = actualFlowFmFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
                
                Assert.IsNotNull(expectedFlowFmFile, $"The expected file collection contains a file with name '{fileName}'; this file is not part of the actual collection of files.{Environment.NewLine}");
                Assert.IsNotNull(actualFlowFmFile, $"The actual file collection contains a file with name '{fileName}'; this file is not part of the expected collection of files.{Environment.NewLine}");

                switch (Path.GetExtension(expectedFlowFmFile))
                {
                    case ".mdu":
                    {
                        linesToIgnore.AddRange(MduLinesToIgnore);
                        break;
                    }

                    case ".nc":
                    {
                        linesToIgnore.AddRange(NetCdfLinesToIgnore);

                        expectedFlowFmFile = Path.Combine(tempDirectory, "ncdump", "expected", fileName);
                        actualFlowFmFile = Path.Combine(tempDirectory, "ncdump", "actual", fileName);

                        FileComparerHelper.DumpNetCdfToTextFile(expectedFlowFmFile, expectedFlowFmFile);
                        FileComparerHelper.DumpNetCdfToTextFile(actualFlowFmFile, actualFlowFmFile);

                        break;
                    }
                }

                SortScrambledFiles(expectedFlowFmFile, actualFlowFmFile);

                // crsdef.ini requires custom comparison because of rounding errors for coordinates when saving and loading
                if (IsCrossSectionDefinitionFile(actualFlowFmFile))
                {
                    CrossSectionDefinitionFileComparer.CompareFiles(expectedFlowFmFile, actualFlowFmFile);
                }
                else
                {
                    FileComparerHelper.CompareFiles(expectedFlowFmFile, actualFlowFmFile, linesToIgnore.ToArray());
                }
            }
        }

        private static bool IsCrossSectionDefinitionFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (!string.Equals(extension, ".ini"))
            {
                return false;
            }
            
            IList<IniSection> iniSections = new IniReader().ReadIniFile(filePath);
            return iniSections.Any(c => c.ValidGeneralRegion(GeneralRegion.CrossSectionDefinitionsMajorVersion,
                                                             GeneralRegion.CrossSectionDefinitionsMinorVersion,
                                                             GeneralRegion.FileTypeName.CrossSectionDefinition));
        }

        private static void SortScrambledFiles(string expectedFlowFmFile, string actualFlowFmFile)
        {
            string fileExtension = Path.GetExtension(expectedFlowFmFile);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(expectedFlowFmFile);

            switch (fileExtension.ToLower())
            {
                case ".ini":
                    SortFmIniFile(expectedFlowFmFile, actualFlowFmFile, "id");
                    break;
                case ".bc":
                    FileComparerHelper.SortBcFilesByKey(expectedFlowFmFile, actualFlowFmFile, "name");
                    break;
                case ".ext":
                    if (fileNameWithoutExtension.EndsWith("_bnd", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SortBoundaryAndLateralSections(expectedFlowFmFile);
                        SortBoundaryAndLateralSections(actualFlowFmFile);
                    }
                    break;
                case ".gui":
                    if (string.Equals(fileNameWithoutExtension, "branches", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SortFmIniFile(expectedFlowFmFile, actualFlowFmFile, "name");
                    }
                    break;
            }
        }

        private static void SortBoundaryAndLateralSections(string fileName)
        {
            var expectedReadSections = new IniReader().ReadIniFile(fileName);

            var expectedBoundarySections = expectedReadSections.Where(c => c.Name.Equals(DeltaShell.NGHS.IO.FileWriters.Boundary.BoundaryRegion.BcBoundaryHeader, StringComparison.InvariantCultureIgnoreCase))
                                                               .OrderBy(c => c.ReadProperty<string>(BoundaryRegion.NodeId.Key));

            var expectedLateralSections = expectedReadSections.Where(c => c.Name.Equals(BoundaryRegion.LateralHeader, StringComparison.InvariantCultureIgnoreCase))
                                                              .OrderBy(c => c.ReadProperty<string>(LocationRegion.Id.Key))
                                                              .ThenBy(c => c.ReadProperty<string>(LocationRegion.Name.Key))
                                                              .ToArray();

            new IniWriter().WriteIniFile(expectedBoundarySections.Concat(expectedLateralSections), fileName);
        }

        private static void SortFmIniFile(string expectedFlowFmFile, string actualFlowFmFile, string idKey)
        {
            var readSections = new IniReader().ReadIniFile(expectedFlowFmFile);
            new IniWriter().WriteIniFile(readSections.OrderBy(c => c.ReadProperty<string>(idKey)), expectedFlowFmFile);

            readSections = new IniReader().ReadIniFile(actualFlowFmFile);
            new IniWriter().WriteIniFile(readSections.OrderBy(c => c.ReadProperty<string>(idKey)), actualFlowFmFile);
        }
    }
}