using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils;
using DeltaShell.NGHS.IO;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class PolFile<T> : FMSuiteFileBase, IFeature2DFileBase<T> where T : Feature2DPolygon, new()
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(T));

        public const string Extension = "pol";

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
                    foreach (var feature in features)
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
                            WriteLine("poly_" + (i++));
                        }

                        var coordinates = lineString.Coordinates.Take(lineString.Coordinates.Length -
                                                                        (IncludeClosingCoordinate ? 0 : 1)).ToList();

                        WriteLine(String.Format("    {0}    {1}", coordinates.Count, numColumns));

                        foreach (var coordinate in coordinates)
                        {
                            WriteLine(String.Format("{0, 24}{1, 24}", coordinate.X, coordinate.Y));
                        }
                    }
                }
                finally
                {
                    CloseOutputFile();
                }
            }
        }

        public void Write(string path, IEnumerable<T> features)
        {
            Write(path, features.Cast<IFeature>());
        }

        public IList<T> Read(string path)
        {
            var features = new List<T>();

            OpenInputFile(path);
            try
            {
                var line = GetNextLine();
                while (line != null)
                {
                    //  start of polyLine
                    var feature = new T {Name = line};
                    feature.TrySetGroupName(path);
                    
                    line = GetNextLine();
                    if (line == null)
                    {
                        throw new FormatException(
                            String.Format(
                                "Unexpected end of file; Expected line stating number of points and columns on line {0} of file {1}",
                                LineNumber, path));
                    }

                    var lineFields = (IList<string>) SplitLine(line).Take(2).ToList();
                    if (lineFields.Count != 2)
                    {
                        throw new FormatException(String.Format("Invalid numpoints/numcolums {0} in file {1}",
                                                                lineFields, path));
                    }

                    var numPoints = GetInt(lineFields[0], "value for nr of points");
                    var numColumns = GetInt(lineFields[1], "value for nr of columns");
                    if (numColumns < 2)
                    {
                        throw new FormatException(String.Format(
                            "Invalid number of colums ({0}, must be 2) in file {1}", numColumns, path));
                    }

                    var points = new List<Coordinate>();
                    for (int i = 0; i < numPoints; i++)
                    {
                        line = GetNextLine();
                        if (line == null)
                        {
                            throw new FormatException(
                                String.Format(
                                    "Unexpected end of file; Expected point row ({0} out of {1}) on line {2} in file {3}",
                                    i + 1, numPoints, LineNumber, path));
                        }

                        lineFields = SplitLine(line).Take(2).ToList();
                        if (lineFields.Count != 2)
                        {
                            throw new FormatException(String.Format("Invalid point row on line {0} in file {1}",
                                                                    LineNumber, path));
                        }

                        var x = GetDouble(lineFields[0]);
                        var y = GetDouble(lineFields[1]);
                        points.Add(new Coordinate(x, y));
                    }
                    if ((points.First().X != points.Last().X) || (points.First().Y != points.Last().Y))
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
                        Log.ErrorFormat("Invalid geometry in pol-file '{0}' with id '{1}', skipping", path,
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