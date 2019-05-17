using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Files
{
    public class LdbFile : FMSuiteFileBase, IFeature2DFileBase<LandBoundary2D>
    {
        public void Write(string polFilePath, IEnumerable<LandBoundary2D> area2DFeatures)
        {
            OpenOutputFile(polFilePath);
            try
            {
                foreach (LandBoundary2D area2DFeature in area2DFeatures)
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

        public IList<LandBoundary2D> Read(string ldbFilePath)
        {
            var features = new EventedList<LandBoundary2D>();

            OpenInputFile(ldbFilePath);
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
                                                          ldbFilePath));
                    }

                    int numPoints = GetInt(lineFields[0], "value for nr of points");
                    int numColumns = GetInt(lineFields[1], "value for nr of columns");

                    if (numColumns < 2)
                    {
                        throw new Exception(string.Format(
                                                "Number of colums must be at least 2. (line: {0}) in file {1}",
                                                LineNumber, ldbFilePath));
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
                                                              ldbFilePath));
                        }

                        double x = GetDouble(lineFields[0]);
                        double y = GetDouble(lineFields[1]);

                        if (Math.Abs(x - 999.999) < 0.00001 && Math.Abs(y - 999.999) < 0.00001
                        ) //coordinate split; new feature
                        {
                            string featureName = baseName != null
                                                     ? baseName + counter++
                                                     : firstName + "_" + counter++;

                            AddNewFeature(featureName, points, features, ldbFilePath);
                            points.Clear();
                        }
                        else
                        {
                            points.Add(new Coordinate(x, y));
                        }
                    }

                    string lastFeatureName = counter == 1 ? firstName :
                                             baseName != null ? baseName + counter : firstName + "_" + counter;
                    AddNewFeature(lastFeatureName, points, features, ldbFilePath);
                    line = GetNextLine();
                }
            }
            finally
            {
                CloseInputFile();
            }

            return features;
        }

        private static void AddNewFeature(string name, List<Coordinate> points, ICollection<LandBoundary2D> features,
                                          string ldbFilePath)
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