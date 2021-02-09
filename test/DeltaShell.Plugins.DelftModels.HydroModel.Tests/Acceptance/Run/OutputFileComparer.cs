using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    public class OutputFileComparer
    {
        private static readonly string[] NetCdfLinesToIgnore =
        {
            ":history = \"Created on",
            ":source = \"D-Flow Flexible Mesh Plugin"
        };
        
        /// <summary>
        /// Compares the contents of two output file directories.
        /// </summary>
        /// <param name="expectedOutputFiles">The file paths of the expected output files.</param>
        /// <param name="actualOutputFiles">The file paths of the actual output files.</param>
        /// <param name="tempDirectory">A temporary working directory to use during the comparison.</param>
        /// <remarks>
        /// Files are also considered to be equal when the relevant file contents are equivalent (i.o.w. same file contents but in different order).
        /// </remarks>
        public static void Compare(string[] expectedOutputFiles, string[] actualOutputFiles, string tempDirectory)
        {
            bool identical = true;
            IEnumerable<string> expectedOutputFileNames = expectedOutputFiles.Select(Path.GetFileName);
            IEnumerable<string> actualOutputFileNames = actualOutputFiles.Select(Path.GetFileName);
            string[] allFileNames = actualOutputFileNames.Union(expectedOutputFileNames).ToArray();
            string overallErrorMessage = $"{Environment.NewLine}{FileComparerHelper.VerticalLine}";

            foreach (string fileName in allFileNames)
            {
                string[] linesToIgnore = NetCdfLinesToIgnore;
                
                string expectedOutputFile = expectedOutputFileNames.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName));
                string actualOutputFile = actualOutputFileNames.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName));
                
                if (!FileComparerHelper.FileNameIsEqual(fileName, expectedOutputFile, actualOutputFile, ref overallErrorMessage))
                {
                    identical = false;
                    continue;
                }
                
                expectedOutputFile = Path.Combine(tempDirectory, "ncdump", "expected", fileName);
                actualOutputFile = Path.Combine(tempDirectory, "ncdump", "actual", fileName);

                FileComparerHelper.DumpNetCdfToTextFile(expectedOutputFile, expectedOutputFile);
                FileComparerHelper.DumpNetCdfToTextFile(actualOutputFile, actualOutputFile);
                
                identical = FileComparerHelper.CompareFiles(expectedOutputFile, actualOutputFile, linesToIgnore, out var errorMessage) && identical;
            }
        }
    }
}