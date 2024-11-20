using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Acceptance.Persistence.CustomComparers
{
    /// <summary>
    /// Class that compares two cross-section definition files.
    /// </summary>
    public static class CrossSectionDefinitionFileComparer
    {
        private static double tolerance = 1e-4;
        
        /// <summary>
        /// Compares two cross-section definition files.
        /// </summary>
        /// <param name="filePathExpected">File path of the expected crsdef.ini file.</param>
        /// <param name="filePathActual">File path of the actual crsdef.ini file. </param>
        public static void CompareFiles(string filePathExpected, string filePathActual)
        {
            if (filePathExpected == null)
            {
                throw new ArgumentNullException(nameof(filePathExpected));
            }

            if (filePathActual == null)
            {
                throw new ArgumentNullException(nameof(filePathExpected));
            }

            string[] coordinatesLineIdentifiers = new[]
            {
                DefinitionPropertySettings.XCoors.Key, 
                DefinitionPropertySettings.YCoors.Key,
                DefinitionPropertySettings.ZCoors.Key
            };

            CompareNonCoordinateLines(filePathExpected, filePathActual, coordinatesLineIdentifiers, out var coordinatesLinesInExpectedText, out var coordinatesLinesInActualText);
            CompareCoordinateLines(coordinatesLinesInExpectedText, coordinatesLinesInActualText, filePathExpected);

        }

        private static void CompareNonCoordinateLines(string filePathExpected, 
                                                      string filePathActual, 
                                                      string[] coordinatesLineIdentifiers, 
                                                      out List<Tuple<int, string>> coordinatesLinesInExpectedText, 
                                                      out List<Tuple<int, string>> coordinatesLinesInActualText)
        {
            FileComparerHelper.ParseFile(filePathExpected, coordinatesLineIdentifiers, out var linesWithoutCoordinatesInExpectedText, out coordinatesLinesInExpectedText);
            FileComparerHelper.ParseFile(filePathActual, coordinatesLineIdentifiers, out var linesWithoutCoordinatesInActualText, out coordinatesLinesInActualText);

            FileComparerHelper.GetMismatchingLines(linesWithoutCoordinatesInExpectedText, linesWithoutCoordinatesInActualText, out var mismatchingLinesInExpected, out var mismatchingLinesInActual);
            FileComparerHelper.RemoveEquivalentLines(mismatchingLinesInExpected, mismatchingLinesInActual);

            if (mismatchingLinesInExpected.Any())
            {
                string message = $"Mismatch for file '{Path.GetFileName(filePathExpected)}':" +
                                 $"{Environment.NewLine}" +
                                 $"{FileComparerHelper.CreateErrorMessage(mismatchingLinesInExpected.First(), mismatchingLinesInActual.First())}";
                
                Assert.Fail(message);
            }
        }

        private static void CompareCoordinateLines(IReadOnlyCollection<Tuple<int, string>> coordinatesLinesInExpectedText, 
                                                   IReadOnlyCollection<Tuple<int, string>> coordinatesLinesInActualText,
                                                   string filePathExpected)
        {
            FileComparerHelper.GetMismatchingLines(coordinatesLinesInExpectedText,
                                                   coordinatesLinesInActualText,
                                                   out var mismatchingCoordinateLinesInExpected,
                                                   out var mismatchingCoordinateLinesInActual);
            FileComparerHelper.RemoveEquivalentLines(mismatchingCoordinateLinesInExpected, mismatchingCoordinateLinesInActual);

            for (var i = 0; i < mismatchingCoordinateLinesInExpected.Count; i++)
            {
                bool equivalentLine = CompareMismatchLine(mismatchingCoordinateLinesInExpected[i], mismatchingCoordinateLinesInActual[i]);
                
                if (!equivalentLine)
                {
                    string message = $"Mismatch for file '{Path.GetFileName(filePathExpected)}':" +
                                     $"{Environment.NewLine}" +
                                     $"{FileComparerHelper.CreateErrorMessage(mismatchingCoordinateLinesInExpected[i], mismatchingCoordinateLinesInActual[i])}";

                    Assert.Fail(message);
                }
            }
        }

        private static bool CompareMismatchLine(Tuple<int, string> mismatchingCoordinateLineInExpected, 
                                            Tuple<int, string> mismatchingCoordinateLineInActual)
        {
            if (mismatchingCoordinateLineInExpected.Item1 == -1 || mismatchingCoordinateLineInActual.Item1 == -1) // missing line in either file
            {
                return false;
            }

            string[] expectedCoordinates = mismatchingCoordinateLineInExpected.Item2.Split('=')[1].Split(' ');
            string[] actualCoordinates = mismatchingCoordinateLineInActual.Item2.Split('=')[1].Split(' ');

            if (expectedCoordinates.Length != actualCoordinates.Length)
            {
                return false;
            }

            for (var i = 0; i < expectedCoordinates.Length; i++)
            {
                string expectedCoordinate = expectedCoordinates[i].Trim();
                string actualCoordinate = actualCoordinates[i].Trim();

                if (string.IsNullOrWhiteSpace(expectedCoordinate) || string.IsNullOrWhiteSpace(actualCoordinate))
                {
                    continue;
                }

                bool equivalentCoordinates = CompareCoordinateStringsAsDoubles(expectedCoordinate, actualCoordinate);
                if (!equivalentCoordinates)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CompareCoordinateStringsAsDoubles(string expectedCoordinateString, string actualCoordinateString)
        {
            bool expectedCoordinateParsed = double.TryParse(expectedCoordinateString, NumberStyles.Any, CultureInfo.InvariantCulture, out double expectedCoordinate);
            bool actualCoordinateParsed = double.TryParse(actualCoordinateString, NumberStyles.Any, CultureInfo.InvariantCulture, out double actualCoordinate);

            if (!expectedCoordinateParsed || !actualCoordinateParsed)
            {
                return false;
            }

            if (Math.Abs(expectedCoordinate - actualCoordinate) > tolerance)
            {
                return false;
            }

            return true;
        }
    }
}