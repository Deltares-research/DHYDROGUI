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
        public static void Compare(string[] expectedRainfallRunoffFiles, string[] actualRainfallRunoffFiles)
        {
            var identical = true;
            var overallErrorMessage = $"{Environment.NewLine}{FileComparerHelper.VerticalLine}";
            
            IEnumerable<string> actualRainfallRunoffFileNames = actualRainfallRunoffFiles.Select(Path.GetFileName);
            IEnumerable<string> expectedFlowFmFileNames = expectedRainfallRunoffFiles.Select(Path.GetFileName);
            string[] allFileNames = actualRainfallRunoffFileNames.Union(expectedFlowFmFileNames).ToArray();
            
            foreach (string fileName in allFileNames)
            {
                var linesToIgnore = new string[]
                    {};

                string expectedRainfallRunoffFile = expectedRainfallRunoffFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName));
                string actualRainfallRunoffFile = actualRainfallRunoffFiles.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName));
                
                if (!FileComparerHelper.FileNameIsEqual(fileName, expectedRainfallRunoffFile, actualRainfallRunoffFile, ref overallErrorMessage))
                {
                    identical = false;
                    continue;
                }
                
                string errorMessage = string.Empty;
                
                if (string.Equals(fileName, "3brunoff.tp", StringComparison.InvariantCultureIgnoreCase))
                {
                    identical = RunoffTpFileComparer.Compare(expectedRainfallRunoffFile, actualRainfallRunoffFile, out errorMessage) && identical;
                }
                else
                {
                    identical = FileComparerHelper.CompareFiles(expectedRainfallRunoffFile, actualRainfallRunoffFile, linesToIgnore, out errorMessage) && identical;                    
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
    }
}