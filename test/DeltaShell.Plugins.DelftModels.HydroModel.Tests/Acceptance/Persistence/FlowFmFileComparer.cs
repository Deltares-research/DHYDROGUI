using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence.CustomComparers;
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
        /// <remarks>
        /// Files are also considered to be equal when the relevant file contents are equivalent (i.o.w. same file contents but in different order).
        /// </remarks>
        public static void Compare(string[] expectedFlowFmFiles, string[] actualFlowFmFiles, string tempDirectory)
        {
            var identical = true;
            var actualFlowFmFileNames = actualFlowFmFiles.Select(Path.GetFileName);
            var expectedFlowFmFileNames = expectedFlowFmFiles.Select(Path.GetFileName);
            var allFileNames = actualFlowFmFileNames.Union(expectedFlowFmFileNames).ToArray();
            var overallErrorMessage = $"{Environment.NewLine}{FileComparerHelper.VerticalLine}";

            foreach (var fileName in allFileNames)
            {
                var linesToIgnore = new string[] { };

                string expectedFlowFmFile = expectedFlowFmFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName));
                string actualFlowFmFile = actualFlowFmFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName));

                if (!FileComparerHelper.FileNameIsEqual(fileName, expectedFlowFmFile, actualFlowFmFile, ref overallErrorMessage))
                {
                    identical = false;
                    continue;
                }

                switch (Path.GetExtension(expectedFlowFmFile))
                {
                    case ".mdu":
                    {
                        linesToIgnore = MduLinesToIgnore;
                        break;
                    }

                    case ".nc":
                    {
                        linesToIgnore = NetCdfLinesToIgnore;

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
                if (Path.GetFileNameWithoutExtension(expectedFlowFmFile).Equals("crsdef", StringComparison.InvariantCultureIgnoreCase))
                {
                    identical = CrossSectionDefinitionFileComparer.CompareFiles(expectedFlowFmFile, actualFlowFmFile, out errorMessage) && identical;
                }
                else
                {
                    identical = FileComparerHelper.CompareFiles(expectedFlowFmFile, actualFlowFmFile, linesToIgnore, out errorMessage) && identical;
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
        
        private static void SortScrambledFiles(string expectedFlowFmFile, string actualFlowFmFile)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(expectedFlowFmFile);

            if (fileNameWithoutExtension.Equals("crsloc", StringComparison.InvariantCultureIgnoreCase))
            {
                SortFmIniFile(expectedFlowFmFile, actualFlowFmFile, CrossSectionRegion.IniHeader, LocationRegion.Id.Key);
            }

            if (fileNameWithoutExtension.Equals("crsdef", StringComparison.InvariantCultureIgnoreCase))
            {
                //linesToIgnore = new[] { "    xCoordinates", "    yCoordinates", "    zCoordinates" };
                SortFmIniFile(expectedFlowFmFile, actualFlowFmFile, DefinitionPropertySettings.Header, DefinitionPropertySettings.Id.Key);
            }

            string fileExtension = Path.GetExtension(expectedFlowFmFile);
            if (fileNameWithoutExtension.EndsWith("_bnd", StringComparison.InvariantCultureIgnoreCase) 
                && fileExtension.Equals(".ext", StringComparison.InvariantCultureIgnoreCase))
            {
                SortBoundaryAndLateralCategories(expectedFlowFmFile);
                SortBoundaryAndLateralCategories(actualFlowFmFile);
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

        private static void SortFmIniFile(string expectedFlowFmFile, string actualFlowFmFile, string iniHeader, string idKey)
        {
            var readCategories = new DelftIniReader().ReadDelftIniFile(expectedFlowFmFile);
            new DelftIniWriter().WriteDelftIniFile(readCategories.Where(c => c.Name.Equals(iniHeader)).OrderBy(c => c.ReadProperty<string>(idKey)), expectedFlowFmFile);

            readCategories = new DelftIniReader().ReadDelftIniFile(actualFlowFmFile);
            new DelftIniWriter().WriteDelftIniFile(readCategories.Where(c => c.Name.Equals(iniHeader)).OrderBy(c => c.ReadProperty<string>(idKey)), actualFlowFmFile);
        }
    }
}