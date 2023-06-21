using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence.CustomComparers
{
    /// <summary>
    /// Class to compare 3BRUNOFF.TP files.
    /// </summary>
    public static class RunoffTpFileComparer
    {
        /// <summary>
        /// Compares two 3BRUNOFF.TP files.
        /// </summary>
        /// <param name="filePathExpected">File path of the expected 3BRUNOFF.TP file.</param>
        /// <param name="filePathActual">File path of the actual 3BRUNOFF.TP file. </param>
        public static void Compare(string filePathExpected, string filePathActual)
        {
            if (filePathExpected == null)
            {
                throw new ArgumentNullException(nameof(filePathExpected));
            }

            if (filePathActual == null)
            {
                throw new ArgumentNullException(nameof(filePathActual));
            }
            
            var linesToIgnore = new string[0]; 
            FileComparerHelper.ParseFile(filePathExpected, linesToIgnore,out List<Tuple<int, string>> linesInExpectedText, out List<Tuple<int, string>> _);
            FileComparerHelper.ParseFile(filePathActual, linesToIgnore, out List<Tuple<int, string>> linesInActualText, out var _);
            
            FileComparerHelper.GetMismatchingLines(linesInExpectedText,linesInActualText, out List<Tuple<int, string>> mismatchingLinesInExpected, out List<Tuple<int, string>> mismatchingLinesInActual);
            FileComparerHelper.RemoveEquivalentLines(mismatchingLinesInExpected, mismatchingLinesInActual);
            
            for (var i = 0; i < mismatchingLinesInExpected.Count; i++)
            {
                bool linesAreIdentical = CompareMismatchLine(mismatchingLinesInExpected[i], mismatchingLinesInActual[i]);
             
                if (!linesAreIdentical)
                {
                    string message = $"Mismatch for file '{Path.GetFileName(filePathExpected)}':" +
                                     $"{Environment.NewLine}" +
                                     $"{FileComparerHelper.CreateErrorMessage(mismatchingLinesInExpected[i], mismatchingLinesInActual[i])}";
                    
                    Assert.Fail(message);
                }
            }
        }
        
        private static bool CompareMismatchLine(Tuple<int, string> expectedLine, Tuple<int, string> actualLine)
        {
            if (expectedLine.Item1 == -1 || actualLine.Item1 == -1) // missing line in either file
            {
                return false;
            }
            
            string[] expectedContents = expectedLine.Item2.Split(' ');
            string[] actualContents = actualLine.Item2.Split(' ');

            if (expectedContents.Length != actualContents.Length)
            {
                return false;
            }

            for (var i = 0; i < expectedContents.Length; i++)
            {
                string expectedContent = expectedContents[i];
                string actualContent = actualContents[i];
                
                if (string.Equals(expectedContent, "px") || string.Equals(expectedContent, "py"))
                {
                    i++; // skip comparing the coordinates for now
                }
                
                if (Equals(expectedContent, actualContent))
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}