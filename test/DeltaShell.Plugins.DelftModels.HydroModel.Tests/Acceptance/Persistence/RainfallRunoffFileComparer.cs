using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence.CustomComparers;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence
{
    /// <summary>
    /// Helper class for comparing the contents of two Rainfall Runoff file directories.
    /// </summary>
    public static class RainfallRunoffFileComparer
    {
        /// <summary>
        /// Compares the contents of two Rainfall Runoff file collections.
        /// </summary>
        /// <param name="expectedRainfallRunoffFiles">The file paths of the expected Rainfall Runoff files.</param>
        /// <param name="actualRainfallRunoffFiles">The file paths of the actual Rainfall Runoff files.</param>
        /// <param name="linesToIgnoreLookup">Lookup for which lines to ignore for a specific file. Key: filename, Value: lines to ignore for that file.</param>
        public static void Compare(string[] expectedRainfallRunoffFiles, 
                                   string[] actualRainfallRunoffFiles, 
                                   IReadOnlyDictionary<string, IEnumerable<string>> linesToIgnoreLookup)
        {
            IEnumerable<string> actualRainfallRunoffFileNames = actualRainfallRunoffFiles.Select(Path.GetFileName);
            IEnumerable<string> expectedFlowFmFileNames = expectedRainfallRunoffFiles.Select(Path.GetFileName);
            string[] allFileNames = actualRainfallRunoffFileNames.Union(expectedFlowFmFileNames).ToArray();
            
            foreach (string fileName in allFileNames)
            {
                var linesToIgnore = new string[] {};
                
                if (linesToIgnoreLookup.TryGetValue(fileName, out IEnumerable<string> linesInFileToIgnore))
                {
                    linesToIgnore = linesInFileToIgnore.ToArray();
                }

                string expectedRainfallRunoffFile = expectedRainfallRunoffFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
                string actualRainfallRunoffFile = actualRainfallRunoffFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName, StringComparison.InvariantCultureIgnoreCase));
                
                Assert.IsNotNull(expectedRainfallRunoffFile, $"The expected file collection contains a file with name '{fileName}'; this file is not part of the actual collection of files.{Environment.NewLine}");
                Assert.IsNotNull(actualRainfallRunoffFile, $"The actual file collection contains a file with name '{fileName}'; this file is not part of the expected collection of files.{Environment.NewLine}");

                if (string.Equals(fileName, "3brunoff.tp", StringComparison.InvariantCultureIgnoreCase) || 
                    string.Equals(fileName, "3b_nod.tp", StringComparison.InvariantCultureIgnoreCase))
                {
                    RunoffTpFileComparer.Compare(expectedRainfallRunoffFile, actualRainfallRunoffFile);
                }
                else
                {
                    FileComparerHelper.CompareFiles(expectedRainfallRunoffFile, actualRainfallRunoffFile, linesToIgnore);                    
                }
            }
        }
    }
}