using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence.CustomComparers;
using DeltaShell.Plugins.FMSuite.Common.IO;
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
            var identical = true;
            var actualFlowFmFileNames = actualFlowFmFiles.Select(Path.GetFileName);
            var expectedFlowFmFileNames = expectedFlowFmFiles.Select(Path.GetFileName);
            var allFileNames = actualFlowFmFileNames.Union(expectedFlowFmFileNames).ToArray();
            var overallErrorMessage = $"{Environment.NewLine}{FileComparerHelper.VerticalLine}";

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

                if (!FileComparerHelper.FileNameIsEqual(fileName, expectedFlowFmFile, actualFlowFmFile, ref overallErrorMessage))
                {
                    identical = false;
                    continue;
                }

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

                string errorMessage = string.Empty;

                // crsdef.ini requires custom comparison because of rounding errors for coordinates when saving and loading
                if (IsCrossSectionDefinitionFile(actualFlowFmFile))
                {
                    identical = CrossSectionDefinitionFileComparer.CompareFiles(expectedFlowFmFile, actualFlowFmFile, out errorMessage) && identical;
                }
                else
                {
                    identical = FileComparerHelper.CompareFiles(expectedFlowFmFile, actualFlowFmFile, linesToIgnore.ToArray(), out errorMessage) && identical;
                }
                
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    overallErrorMessage += $"{errorMessage}{FileComparerHelper.VerticalLine}";
                }
            }

            if (!identical)
            {
                Assert.Fail(overallErrorMessage);
            }
        }

        private static bool IsCrossSectionDefinitionFile(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (!string.Equals(extension, ".ini"))
            {
                return false;
            }
            
            IList<DelftIniCategory> categories = new DelftIniReader().ReadDelftIniFile(filePath);
            return categories.Any(c => c.ValidGeneralRegion(GeneralRegion.CrossSectionDefinitionsMajorVersion,
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
                    SortFmBcFile(expectedFlowFmFile, actualFlowFmFile, "name");
                    break;
                case ".ext":
                    if (fileNameWithoutExtension.EndsWith("_bnd", StringComparison.InvariantCultureIgnoreCase))
                    {
                        SortBoundaryAndLateralCategories(expectedFlowFmFile);
                        SortBoundaryAndLateralCategories(actualFlowFmFile);
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

        private static void SortBoundaryAndLateralCategories(string fileName)
        {
            var expectedReadCategories = new DelftIniReader().ReadDelftIniFile(fileName);

            var expectedBoundaryCategories = expectedReadCategories.Where(c => c.Name.Equals(DeltaShell.NGHS.IO.FileWriters.Boundary.BoundaryRegion.BcBoundaryHeader, StringComparison.InvariantCultureIgnoreCase))
                                                                   .OrderBy(c => c.ReadProperty<string>(BoundaryRegion.NodeId.Key));

            var expectedLateralCategories = expectedReadCategories.Where(c => c.Name.Equals(BoundaryRegion.LateralHeader, StringComparison.InvariantCultureIgnoreCase))
                                                                  .OrderBy(c => c.ReadProperty<string>(LocationRegion.Id.Key))
                                                                  .ThenBy(c => c.ReadProperty<string>(LocationRegion.Name.Key))
                                                                  .ToArray();

            new DelftIniWriter().WriteDelftIniFile(expectedBoundaryCategories.Concat(expectedLateralCategories), fileName);
        }

        private static void SortFmIniFile(string expectedFlowFmFile, string actualFlowFmFile, string idKey)
        {
            var readCategories = new DelftIniReader().ReadDelftIniFile(expectedFlowFmFile);
            new DelftIniWriter().WriteDelftIniFile(readCategories.OrderBy(c => c.ReadProperty<string>(idKey)), expectedFlowFmFile);

            readCategories = new DelftIniReader().ReadDelftIniFile(actualFlowFmFile);
            new DelftIniWriter().WriteDelftIniFile(readCategories.OrderBy(c => c.ReadProperty<string>(idKey)), actualFlowFmFile);
        }
        
        private static void SortFmBcFile(string expectedFlowFmFile, string actualFlowFmFile, string idKey)
        {
            var readCategories = new DelftBcReader().ReadDelftBcFile(expectedFlowFmFile);
            new DelftBcWriter().WriteDelftIniFile(readCategories.OrderBy(c => c.ReadProperty<string>(idKey)), expectedFlowFmFile);

            readCategories = new DelftBcReader().ReadDelftBcFile(actualFlowFmFile);
            new DelftBcWriter().WriteDelftIniFile(readCategories.OrderBy(c => c.ReadProperty<string>(idKey)), actualFlowFmFile);
        }
    }
}