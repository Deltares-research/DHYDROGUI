using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class LdbFile : NGHSFileBase, IFeature2DFileBase<LandBoundary2D>
    {
        /// <summary>
        /// Writes the features to the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="features">The 2D area features.</param>
        /// <exception cref="Exception">
        /// Thrown when <paramref name="features"/> contains invalid geometry.
        /// </exception>
        public void Write(string filePath, IEnumerable<LandBoundary2D> features)
        {
            OpenOutputFile(filePath);
            try
            {
                foreach (LandBoundary2D area2DFeature in features)
                {
                    var polyLine = area2DFeature.Geometry as ILineString;
                    if (polyLine == null)
                    {
                        throw new Exception("Invalid geometry " + area2DFeature.GetType());
                    }

                    const int numColumns = 2; // X, Y
                    WriteLine(area2DFeature.Name);
                    WriteLine(string.Format("    {0}    {1}", polyLine.NumPoints, numColumns));
                    foreach (Coordinate coord in polyLine.Coordinates)
                    {
                        WriteLine(string.Format("{0, 24}{1, 24}", coord.X, coord.Y).TrimStart());
                    }
                }
            }
            finally
            {
                CloseOutputFile();
            }
        }

        public IList<LandBoundary2D> Read(string filePath)
        {
            var features = new EventedList<LandBoundary2D>();

            OpenInputFile(filePath);
            try
            {
                string line = GetNextLine();
                while (line != null)
                {
                    string baseName = null;
                    if (line.EndsWith("1"))
                    {
                        baseName = line.Substring(0, line.Length - 1);
                    }

                    string firstName = line;

                    line = GetNextLine();
                    IList<string> lineFields = SplitLine(line).ToList();

                    if (lineFields.Count < 2)
                    {
                        throw new Exception(string.Format("Invalid numpoints/numcolums {0} in file {1}", LineNumber,
                                                          filePath));
                    }

                    int numPoints = GetInt(lineFields[0], "value for nr of points");
                    int numColumns = GetInt(lineFields[1], "value for nr of columns");

                    if (numColumns < 2)
                    {
                        throw new Exception(string.Format(
                                                "Number of colums must be at least 2. (line: {0}) in file {1}",
                                                LineNumber, filePath));
                    }

                    var points = new List<Coordinate>();
                    var counter = 1;
                    for (var i = 0; i < numPoints; i++)
                    {
                        line = GetNextLine();
                        lineFields = SplitLine(line).ToList();
                        if (lineFields.Count < numColumns)
                        {
                            throw new Exception(string.Format("Invalid point row on line {0} in file {1}", LineNumber,
                                                              filePath));
                        }

                        double x = GetDouble(lineFields[0]);
                        double y = GetDouble(lineFields[1]);

                        if (Math.Abs(x - 999.999) < 0.00001 && Math.Abs(y - 999.999) < 0.00001
                        ) //coordinate split; new feature
                        {
                            string featureName = baseName != null
                                                     ? baseName + counter++
                                                     : firstName + "_" + counter++;

                            AddNewFeature(featureName, points, features);
                            points.Clear();
                        }
                        else
                        {
                            points.Add(new Coordinate(x, y));
                        }
                    }

                    string lastFeatureName = counter == 1 ? firstName :
                                             baseName != null ? baseName + counter : firstName + "_" + counter;
                    AddNewFeature(lastFeatureName, points, features);
                    line = GetNextLine();
                }
            }
            finally
            {
                CloseInputFile();
            }

            return features;
        }

        private static void AddNewFeature(string name, List<Coordinate> points, ICollection<LandBoundary2D> features)
        {
            if (!points.Any())
            {
                return;
            }

            var feature = new LandBoundary2D()
            {
                Name = name,
                Geometry = points.Count == 1
                               ? new LineString(new[]
                               {
                                   points[0],
                                   points[0]
                               })
                               : new LineString(points.ToArray())
            };
            features.Add(feature);
        }
    }
}