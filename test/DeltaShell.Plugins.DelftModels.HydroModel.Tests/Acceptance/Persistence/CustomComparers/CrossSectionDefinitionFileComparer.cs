using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

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
        /// <param name="errorMessage">Reference to an error message.</param>
        /// <returns>Returns <c>true</c> if the provided files are equal.</returns>
        public static bool CompareFiles(string filePathExpected,
                                        string filePathActual,
                                        out string errorMessage)
        {
            if (filePathExpected == null)
            {
                throw new ArgumentNullException(nameof(filePathExpected));
            }

            if (filePathActual == null)
            {
                throw new ArgumentNullException(nameof(filePathExpected));
            }

            errorMessage = string.Empty;

            string[] coordinatesLineIdentifiers = new[] { "    xCoordinates", "    yCoordinates", "    zCoordinates" };

            bool identical = CompareNonCoordinateLines(filePathExpected, 
                                                  filePathActual, 
                                                  coordinatesLineIdentifiers, 
                                                  ref errorMessage, 
                                                  out var coordinatesLinesInExpectedText, 
                                                  out var coordinatesLinesInActualText);

            if (!identical)
            {
                return false;
            }

            identical = CompareCoordinateLines(coordinatesLinesInExpectedText, 
                                               coordinatesLinesInActualText,
                                               filePathExpected,
                                               ref errorMessage);

            return identical;

        }

        private static bool CompareNonCoordinateLines(string filePathExpected, 
                                                      string filePathActual, 
                                                      string[] coordinatesLineIdentifiers, 
                                                      ref string errorMessage, 
                                                      out List<Tuple<int, string>> coordinatesLinesInExpectedText, 
                                                      out List<Tuple<int, string>> coordinatesLinesInActualText)
        {
            FileComparerHelper.ParseFile(filePathExpected, coordinatesLineIdentifiers, out var linesWithoutCoordinatesInExpectedText, out coordinatesLinesInExpectedText);
            FileComparerHelper.ParseFile(filePathActual, coordinatesLineIdentifiers, out var linesWithoutCoordinatesInActualText, out coordinatesLinesInActualText);

            FileComparerHelper.GetMismatchingLines(linesWithoutCoordinatesInExpectedText, linesWithoutCoordinatesInActualText, out var mismatchingLinesInExpected, out var mismatchingLinesInActual);
            FileComparerHelper.RemoveEquivalentLines(mismatchingLinesInExpected, mismatchingLinesInActual);

            if (mismatchingLinesInExpected.Any())
            {
                errorMessage = $"Mismatch for file '{Path.GetFileName(filePathExpected)}':" +
                               $"{Environment.NewLine}" +
                               $"{FileComparerHelper.CreateErrorMessage(mismatchingLinesInExpected.First(), mismatchingLinesInActual.First())}";

                return false;
            }

            return true;
        }

        private static bool CompareCoordinateLines(IReadOnlyCollection<Tuple<int, string>> coordinatesLinesInExpectedText, 
                                                   IReadOnlyCollection<Tuple<int, string>> coordinatesLinesInActualText,
                                                   string filePathExpected,
                                                   ref string errorMessage)
        {
            FileComparerHelper.GetMismatchingLines(coordinatesLinesInExpectedText,
                                                   coordinatesLinesInActualText,
                                                   out var mismatchingCoordinateLinesInExpected,
                                                   out var mismatchingCoordinateLinesInActual);
            FileComparerHelper.RemoveEquivalentLines(mismatchingCoordinateLinesInExpected, mismatchingCoordinateLinesInActual);

            if (!mismatchingCoordinateLinesInExpected.Any())
            {
                return true;
            }

            for (var i = 0; i < mismatchingCoordinateLinesInExpected.Count; i++)
            {
                bool equivalentLine = CompareMismatchLine(mismatchingCoordinateLinesInExpected[i], mismatchingCoordinateLinesInActual[i]);
                if (!equivalentLine)
                {
                    errorMessage = $"Mismatch for file '{Path.GetFileName(filePathExpected)}':" +
                                   $"{Environment.NewLine}" +
                                   $"{FileComparerHelper.CreateErrorMessage(mismatchingCoordinateLinesInExpected[i], mismatchingCoordinateLinesInActual[i])}";

                    return false;
                }
            }
            
            return true;
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