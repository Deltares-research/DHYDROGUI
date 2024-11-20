using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.NGHS.IO;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO.Files
{
    public class PolFile<T> : NGHSFileBase, IFeature2DFileBase<T> where T : Feature2DPolygon, new()
    {
        public const string Extension = "pol";
        private static readonly ILog Log = LogManager.GetLogger(typeof(T));

        public PolFile()
        {
            IncludeClosingCoordinate = false;
        }

        public bool IncludeClosingCoordinate { private get; set; }

        public void Write(string polFilePath, IEnumerable<IFeature> features)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                OpenOutputFile(polFilePath);
                try
                {
                    var i = 1;
                    foreach (IFeature feature in features)
                    {
                        var lineString = new LineString(feature.Geometry.Coordinates);
                        const int numColumns = 2; // X, Y

                        if (!lineString.IsClosed)
                        {
                            throw new Exception("Invalid geometry " + feature.GetType());
                        }

                        var nameable = feature as INameable;
                        if (nameable != null)
                        {
                            WriteLine(nameable.Name);
                        }
                        else
                        {
                            WriteLine("poly_" + i++);
                        }

                        List<Coordinate> coordinates = lineString.Coordinates.Take(lineString.Coordinates.Length -
                                                                                   (IncludeClosingCoordinate ? 0 : 1))
                                                                 .ToList();

                        WriteLine(string.Format("    {0}    {1}", coordinates.Count(), numColumns));

                        foreach (Coordinate coordinate in coordinates)
                        {
                            WriteLine(string.Format("{0, 24}{1, 24}", coordinate.X, coordinate.Y));
                        }
                    }
                }
                finally
                {
                    CloseOutputFile();
                }
            }
        }

        public void Write(string filePath, IEnumerable<T> features)
        {
            Write(filePath, features.Cast<IFeature>());
        }

        public IList<T> Read(string filePath)
        {
            var features = new List<T>();

            OpenInputFile(filePath);
            try
            {
                string line = GetNextLine();
                while (line != null)
                {
                    //  start of polyLine
                    var feature = new T {Name = line};
                    feature.TrySetGroupName(filePath);

                    line = GetNextLine();
                    if (line == null)
                    {
                        throw new FormatException(
                            string.Format(
                                "Unexpected end of file; Expected line stating number of points and columns on line {0} of file {1}",
                                LineNumber, filePath));
                    }

                    var lineFields = (IList<string>) SplitLine(line).Take(2).ToList();
                    if (lineFields.Count != 2)
                    {
                        throw new FormatException(string.Format("Invalid numpoints/numcolums {0} in file {1}",
                                                                lineFields, filePath));
                    }

                    int numPoints = GetInt(lineFields[0], "value for nr of points");
                    int numColumns = GetInt(lineFields[1], "value for nr of columns");
                    if (numColumns < 2)
                    {
                        throw new FormatException(string.Format(
                                                      "Invalid number of colums ({0}, must be 2) in file {1}",
                                                      numColumns, filePath));
                    }

                    var points = new List<Coordinate>();
                    for (var i = 0; i < numPoints; i++)
                    {
                        line = GetNextLine();
                        if (line == null)
                        {
                            throw new FormatException(
                                string.Format(
                                    "Unexpected end of file; Expected point row ({0} out of {1}) on line {2} in file {3}",
                                    i + 1, numPoints, LineNumber, filePath));
                        }

                        lineFields = SplitLine(line).Take(2).ToList();
                        if (lineFields.Count != 2)
                        {
                            throw new FormatException(string.Format("Invalid point row on line {0} in file {1}",
                                                                    LineNumber, filePath));
                        }

                        double x = GetDouble(lineFields[0]);
                        double y = GetDouble(lineFields[1]);
                        points.Add(new Coordinate(x, y));
                    }

                    if (points.First().X != points.Last().X || points.First().Y != points.Last().Y)
                    {
                        points.Add(points[0]); // close polygon
                    }

                    ILinearRing linearRing;
                    try
                    {
                        linearRing = new LinearRing(points.ToArray());
                    }
                    catch (Exception)
                    {
                        Log.ErrorFormat("Invalid geometry in pol-file '{0}' with id '{1}', skipping", filePath,
                                        feature.Name);
                        line = GetNextLine();
                        continue;
                    }

                    feature.Geometry = new Polygon(linearRing);
                    features.Add(feature);
                    line = GetNextLine();
                }
            }
            finally
            {
                CloseInputFile();
            }

            return features;
        }
    }
}