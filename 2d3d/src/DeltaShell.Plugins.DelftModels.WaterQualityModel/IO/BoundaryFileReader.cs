using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using DelftTools.Utils.RegularExpressions;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    /// <summary>
    /// Reads a boundary data file (.bdn) referred to by a hydrodynamics file (.hyd).
    /// </summary>
    public static class BoundaryFileReader
    {
        /// <summary>
        /// Reads all boundary data.
        /// </summary>
        /// <returns> All boundaries with the boundary node ID's associated with each. </returns>
        /// <exception cref="System.InvalidOperationException"> When the boundaries file cannot be found. </exception>
        /// <exception cref="FormatException"> When the file is in invalid format. </exception>
        public static IDictionary<WaterQualityBoundary, int[]> ReadAll(FileInfo boundariesFile)
        {
            var readNumberOfBoundaries = 0;
            if (!boundariesFile.Exists)
            {
                throw new InvalidOperationException("Cannot find boundaries file (" + boundariesFile.FullName + ").");
            }

            IDictionary<WaterQualityBoundary, int[]> boundaries;
            using (StreamReader streamReader = boundariesFile.OpenText())
            {
                int expectedNumberOfBoundaries = GetNumberOfBoundaries(streamReader);

                boundaries = new Dictionary<WaterQualityBoundary, int[]>(expectedNumberOfBoundaries);
                for (var i = 0; i < expectedNumberOfBoundaries; i++)
                {
                    KeyValuePair<WaterQualityBoundary, int[]> kvp =
                        GetNextBoundary(streamReader, expectedNumberOfBoundaries, readNumberOfBoundaries);
                    boundaries[kvp.Key] = kvp.Value;
                    readNumberOfBoundaries++;
                }
            }

            return boundaries;
        }

        private static int GetNumberOfBoundaries(TextReader streamReader)
        {
            string line = streamReader.ReadLine();
            if (line == null)
            {
                throw new FormatException("Error reading file: Missing statement of the number of boundaries.");
            }

            try
            {
                return int.Parse(line.Trim());
            }
            catch (FormatException formatException)
            {
                throw new FormatException(
                    "Error reading file: Statement of the number of boundaries is not an integer.",
                    formatException);
            }
        }

        private static KeyValuePair<WaterQualityBoundary, int[]> GetNextBoundary(
            TextReader streamReader, int expectedNumberOfBoundaries, int readNumberOfBoundaries)
        {
            string currectReadingBoundary =
                ReadBoundaryName(streamReader, expectedNumberOfBoundaries, readNumberOfBoundaries);
            int boundaryNodeCount = ReadBoundaryNodeCount(streamReader, currectReadingBoundary);

            int[] boundaryNodeIds;
            IGeometry geometry =
                ReadBoundaryNodeData(streamReader, boundaryNodeCount, out boundaryNodeIds, currectReadingBoundary);

            var boundary = new WaterQualityBoundary()
            {
                Name = currectReadingBoundary,
                Geometry = geometry
            };
            return new KeyValuePair<WaterQualityBoundary, int[]>(boundary, boundaryNodeIds);
        }

        private static string ReadBoundaryName(TextReader streamReader, int expectedNumberOfBoundaries,
                                               int readNumberOfBoundaries)
        {
            string line = streamReader.ReadLine();
            if (line == null)
            {
                throw new FormatException(string.Format(
                                              "Error reading file: Expected number of boundaries: {0}; But read: {1}.",
                                              expectedNumberOfBoundaries, readNumberOfBoundaries));
            }

            return line.Trim();
        }

        private static int ReadBoundaryNodeCount(TextReader streamReader, string currectReadingBoundary)
        {
            string line = streamReader.ReadLine();
            if (line == null)
            {
                throw new FormatException(string.Format(
                                              "Error reading file: Missing statement of number of boundary node ID's for boundary '{0}'.",
                                              currectReadingBoundary));
            }

            try
            {
                return int.Parse(line.Trim());
            }
            catch (FormatException formatException)
            {
                throw new FormatException(
                    string.Format(
                        "Error reading file: Statement of number of boundary node ID's for boundary '{0}' is not an integer.",
                        currectReadingBoundary), formatException);
            }
        }

        private static IGeometry ReadBoundaryNodeData(TextReader streamReader, int boundaryNodeCount,
                                                      out int[] boundaryNodeIds, string currectReadingBoundary)
        {
            var lineStrings = new ILineString[boundaryNodeCount];
            boundaryNodeIds = new int[boundaryNodeCount];
            for (var i = 0; i < boundaryNodeCount; i++)
            {
                Match match = GetBoundaryNodeLine(streamReader, currectReadingBoundary);
                boundaryNodeIds[i] = int.Parse(match.Groups[GroupBoundaryNodeId].Value);
                var point1 = new Coordinate(ParseDoubleFromGroup(match, GroupX1), ParseDoubleFromGroup(match, GroupY1));
                var point2 = new Coordinate(ParseDoubleFromGroup(match, GroupX2), ParseDoubleFromGroup(match, GroupY2));
                lineStrings[i] = new LineString(new Coordinate[]
                {
                    point1,
                    point2
                });
            }

            return new MultiLineString(lineStrings);
        }

        /// <summary>
        ///     <para>
        ///     Performs a regex match to the next line that should match the pattern:
        ///     <c> -integer double double double double </c>. The following capture groups are
        ///     available:
        ///     <list type="bullet">
        ///         <item> boundaryNodeID </item>
        ///         <item> x1 </item>
        ///         <item> y1 </item>
        ///         <item> x2 </item>
        ///         <item> y2 </item>
        ///     </list>
        ///     </para>
        ///     <para> The boundary node ID is required data for the delwaq input file. </para>
        ///     <para> x1 and y1, and x2 with y2 respectively, form a coordinate on that line. </para>
        /// </summary>
        private static Match GetBoundaryNodeLine(TextReader streamReader, string currectReadingBoundary)
        {
            string line = streamReader.ReadLine();
            if (line == null)
            {
                throw new FormatException(string.Format(
                                              "Error reading file: Unexpected end of file while reading boundary '{0}'.",
                                              currectReadingBoundary));
            }

            line = line.Trim();
            Match match = Regex.Match(line, GetRegexPattern(), RegexOptions.Compiled);
            if (!match.Success)
            {
                throw new FormatException(
                    string.Format(
                        "Error reading file: Boundary node data line of boundary '{0}' is not in valid format. (Expected '-<integer> <double> <double> <double> <double>', but was '{1}')",
                        currectReadingBoundary, line));
            }

            return match;
        }

        private static string GetRegexPattern()
        {
            return string.Format(@"-(?<{0}>\d+)\s+" +
                                 @"(?<{1}>{5})\s+" +
                                 @"(?<{2}>{5})\s+" +
                                 @"(?<{3}>{5})\s+" +
                                 "(?<{4}>{5})",
                                 GroupBoundaryNodeId,
                                 GroupX1, GroupY1,
                                 GroupX2, GroupY2,
                                 RegularExpression.Scientific);
        }

        private static double ParseDoubleFromGroup(Match match, string groupname)
        {
            return double.Parse(match.Groups[groupname].Value, CultureInfo.InvariantCulture);
        }

        #region Regex capture group name constants

        private const string GroupBoundaryNodeId = "boundaryNodeID";
        private const string GroupX1 = "x1";
        private const string GroupY1 = "y1";
        private const string GroupX2 = "x2";
        private const string GroupY2 = "y2";

        #endregion
    }
}