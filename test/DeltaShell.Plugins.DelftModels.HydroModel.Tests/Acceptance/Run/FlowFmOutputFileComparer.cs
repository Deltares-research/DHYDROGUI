using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Run
{
    public static class FlowFmOutputFileComparer
    {
        /// <summary>
        /// Compares the contents of two output file directories.
        /// </summary>
        /// <param name="expectedOutputFiles">The file paths of the expected output files.</param>
        /// <param name="actualOutputFiles">The file paths of the actual output files.</param>
        /// <remarks>
        /// Files are also considered to be equal when the relevant file contents are equivalent (i.o.w. same file contents but in different order).
        /// </remarks>
        public static void Compare(string[] expectedOutputFiles, string[] actualOutputFiles)
        {
            bool identical = true;

            IEnumerable<string> expectedOutputFileNames = expectedOutputFiles.Select(Path.GetFileName);
            IEnumerable<string> actualOutputFileNames = actualOutputFiles.Select(Path.GetFileName);
            string[] allFileNames = actualOutputFileNames.Union(expectedOutputFileNames).ToArray();

            string overallErrorMessage = $"{Environment.NewLine}{FileComparerHelper.VerticalLine}";

            foreach (string fileName in allFileNames)
            {
                if (!AcceptanceModelTestHelper.IsNetcdfFile(fileName))
                {
                    continue;
                }

                string actualOutputFileName = actualOutputFileNames.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName));
                string expectedOutputFileName = expectedOutputFileNames.FirstOrDefault(f => Path.GetFileName(f).Equals(fileName));

                if (!FileComparerHelper.FileNameIsEqual(fileName, expectedOutputFileName, actualOutputFileName, ref overallErrorMessage))
                {
                    identical = false;
                    continue;
                }

                string actualOutputFile = actualOutputFiles.Single(f => Path.GetFileName(f).Equals(fileName));
                string expectedOutputFile = expectedOutputFiles.Single(f => Path.GetFileName(f).Equals(fileName));
                
                ICollection<string> validationErrors = NetcdfFileValidator.Validate(actualOutputFile, expectedOutputFile);
                
                if (validationErrors.Any())
                {
                    identical = false;
                    overallErrorMessage += string.Join(Environment.NewLine, validationErrors);
                }
            }

            if (!identical)
            {
                Assert.Fail(overallErrorMessage);
            }
        }
    }
}