using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO
{
    public class PliFile<T> : FMSuiteFileBase, IFeature2DFileBase<T> where T : IFeature, INameable, new ()
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (PliFile<T>));
        private const int MaximumAmountOfNumericValuesInPliFile = 9;
        private const int AmountOfDimensionalValuesInPliFile = 2;

        public static readonly string[] NumericColumnAttributesKeys =
        {
            "Column3",
            "Column4",
            "Column5",
            "Column6",
            "Column7",
            "Column8",
            "Column9"
        };

        protected readonly string[] StringColumnAttributesKeys =
        {
            "WeirType"
        };

        public const string Extension = "pli";

        // Better exception message...
        public static IGeometry CreatePolyLineGeometry(IList<Coordinate> coordinates, bool checkOpen = false)
        {
            if (coordinates.Count < 2)
            {
                throw new ArgumentException(string.Format("Cannot create polyline for {0} with less than 2 points.", typeof (T).Name));
            }

            if (checkOpen && coordinates[0].Equals2D(coordinates[coordinates.Count -1]))
            {
                throw new ArgumentException(string.Format("Cannot create closed polyline for {0}.", typeof (T).Name));
            }

            return new LineString(coordinates.ToArray());
        }

        public Func<List<Coordinate>, string, T> CreateDelegate { private get; set; }

        /// <summary>
        /// Writes a polyline file for the collection of features <see cref="features"/>.
        /// </summary>
        public virtual void Write(string path, IEnumerable<T> features)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                OpenOutputFile(path);
                try
                {
                    foreach (var feature2D in features)
                    {
                        var numericColumnValues = new List<IList<double>>();
                        var stringColumnValues = new List<IList<string>>();
                        IList<string> locationNames = null;

                        var numColumns = 2; // X, Y
                        if (feature2D.Attributes != null)
                        {
                            BuildColumnValuesFromFeature(feature2D, numericColumnValues, NumericColumnAttributesKeys, ref numColumns);
                            BuildColumnValuesFromFeature(feature2D, stringColumnValues, StringColumnAttributesKeys, ref numColumns);
                            if (feature2D.Attributes.ContainsKey(Feature2D.LocationKey))
                            {
                                // location names, does not count as a data column
                                locationNames = (IList<string>) feature2D.Attributes[Feature2D.LocationKey];
                            }
                        }

                        WriteLine(feature2D.Name);
                        WriteLine(String.Format("    {0}    {1}", feature2D.Geometry.NumPoints, numColumns));
                        for (var i = 0; i < feature2D.Geometry.Coordinates.Length; i++)
                        {
                            var coord = feature2D.Geometry.Coordinates[i];
                            var line = string.Format("{0:E15}  {1:E15}", coord.X, coord.Y);
                            ConstructLineContent(i, numericColumnValues, 0.0, NumericColumnAttributesKeys, ref line, feature2D.Name);
                            ConstructLineContent(i, stringColumnValues, "T", StringColumnAttributesKeys, ref line, feature2D.Name);

                            if (locationNames != null && i < locationNames.Count)
                            {
                                line += string.Format(" {0}", locationNames[i]);
                            }
                            WriteLine(line);
                        }
                    }
                }
                finally
                {
                    CloseOutputFile();
                }
            }
        }


        public virtual IList<T> Read(string path)
        {
            return Read(path, null);
        }

        /// <summary>
        /// Reads a polyline file and cerates a collection of features
        /// </summary>
        public virtual IList<T> Read(string path, Action<string, int,int> progress)
        {
            var features = new EventedList<T>();

            OpenInputFile(path);
            int lineCount = 0;
            int numberOfLines = File.ReadLines(path).Count();
            try
            {
                var line = GetNextLine();
                lineCount++;
                int maxColumns = 0;

                while (line != null)
                {
                    if (line == "[General]") break;
                    if (lineCount % 100 == 0)
                    {
                        progress?.Invoke("Reading line", lineCount, numberOfLines);
                    }

                    var featureName = line;
                    var subFeatureCounter = 0;

                    line = GetNextLine();
                    lineCount++;
                    if (line == null)
                    {
                        throw new FormatException($"Unexpected end of file; Expected number of points and columns on line {LineNumber} in file {path}");
                    }

                    var lineFields = (IList<string>) SplitLine(line).ToList();
                    if (lineFields.Count < 2)
                    {
                        throw new FormatException($"Invalid numpoints/numcolums on line {LineNumber} in file {path}");
                    }

                    var numPoints = GetInt(lineFields[0], "value for nr of points");
                    var numColumns = GetInt(lineFields[1], "value for nr of columns");

                    maxColumns = Math.Max(maxColumns, numColumns);

                    var columnNumericalValuesList = new List<IList<double>>(Enumerable.Range(0, numColumns - AmountOfDimensionalValuesInPliFile).Select(i => new List<double>(numPoints)));
                    var columnStringValuesList = new List<IList<string>>(Enumerable.Range(0, Math.Max(0, numColumns - MaximumAmountOfNumericValuesInPliFile)).Select(i => new List<string>(numPoints)));
                    var locationNames = new Dictionary<int, string>(numPoints);
                    var points = new List<Coordinate>(numPoints);

                    for (var i = 0; i < numPoints; i++)
                    {
                        line = GetNextLine();
                        lineCount++;
                        if (line == null)
                        {
                            throw new FormatException($"Unexpected end of file; Expected more data (attempting to read point {i + 1} out of {numPoints}) on line {LineNumber} in file {path}");
                        }

                        lineFields = SplitLine(line).ToList();

                        if (lineFields.Count < numColumns || lineFields.Count >= MaximumAmountOfNumericValuesInPliFile && lineFields.Count > numColumns + 1)
                        {
                            throw new FormatException($"Invalid point row (expected {numColumns} columns, but was {lineFields.Count}) on line {LineNumber} in file {path}");
                        }

                        var x = GetDouble(lineFields[0]);
                        var y = GetDouble(lineFields[1]);

                        if (x.Equals(-999.0d) || y.Equals(-999.0d))
                        {
                            // separator between sub-features
                            try
                            {
                                var feature = CreateFeature2D(featureName + "-" + ++subFeatureCounter, points,
                                    numColumns, columnNumericalValuesList, columnStringValuesList, locationNames, path);
                                features.Add(feature);
                            }
                            catch (Exception e)
                            {
                                throw new FormatException(
                                    string.Format("Failed feature construction for {0} on line {1} in file {2}: {3}",
                                        featureName, LineNumber,
                                        path, e.Message));
                            }
                            points.Clear();
                            foreach (var columnValues in columnNumericalValuesList)
                            {
                                columnValues.Clear();
                            }
                            locationNames.Clear();
                        }
                        else
                        {
                            points.Add(new Coordinate(x, y));

                            for (var j = 2; j < numColumns; ++j)
                            {
                                try
                                {
                                    if (j != 9) columnNumericalValuesList[j - 2].Add(GetDouble(lineFields[j]));
                                    else columnStringValuesList[0].Add(lineFields[j]);
                                }
                                catch (Exception e)
                                {
                                    throw new FormatException(string.Format("Invalid placement of string value '{0}' on line {1} in file {2}: {3}", lineFields[j], LineNumber, path, e.Message));
                                }
                            }

                            if(lineFields.Count == numColumns + 1)
                            {
                                locationNames.Add(i, lineFields[numColumns]);
                            }
                        }
                    }

                    if (points.Count > 0)
                    {
                        var actualFeatureName = subFeatureCounter > 0
                            ? featureName + "-" + ++subFeatureCounter
                            : featureName;

                        try
                        {
                            var feature = CreateFeature2D(actualFeatureName, points, numColumns, columnNumericalValuesList, columnStringValuesList,
                                locationNames, path);
                            features.Add(feature);
                        }
                        catch (Exception e)
                        {
                            throw new FormatException(
                                string.Format("Failed feature construction for {0} on line {1} in file {2}: {3}",
                                    actualFeatureName, LineNumber,
                                    path, e.Message));
                        }
                        points.Clear();
                        foreach (var columnValues in columnNumericalValuesList)
                        {
                            columnValues.Clear();
                        }
                        locationNames.Clear();
                    }

                    line = GetNextLine();
                    lineCount++;
                }

                if (maxColumns > NumericColumnAttributesKeys.Length + StringColumnAttributesKeys.Length + 2)
                {
                    Log.WarnFormat("In file {0}: columns {1} to {2} will be ignored.", path,
                        NumericColumnAttributesKeys.Length + StringColumnAttributesKeys.Length + 3, maxColumns);
                }
            }
            finally
            {
                CloseInputFile();
            }
            return features;
        }

        private T CreateFeature2D(string name, List<Coordinate> points, int numColumns, IList<IList<double>> columnNumericalValueLists, IList<IList<string>> columnStringValueLists,
            IDictionary<int, string> locationNames, string pliFilePath)
        {
            var feature = CreateDelegate != null
                ? CreateDelegate(points, name)
                : new T
                {
                    Name = name,
                    Geometry = points.Count != 1 ? CreatePolyLineGeometry(points) : new Point(points.FirstOrDefault())
                };

            feature.TrySetGroupName(pliFilePath);

            if (numColumns >= 2)
            {
                if (feature.Attributes == null)
                {
                    feature.Attributes = new DictionaryFeatureAttributeCollection();
                }
                var j = 0;
                foreach (var columnValueList in columnNumericalValueLists)
                {
                    if (j < NumericColumnAttributesKeys.Length)
                    {
                        AssignDoubleValuesToAttribute(columnValueList, feature, NumericColumnAttributesKeys[j++]);
                    }
                }

                j = 0;
                foreach (var stringValueList in columnStringValueLists)
                {
                    if (j < StringColumnAttributesKeys.Length)
                    {
                        AssignStringValuesToAttribute(stringValueList, feature, StringColumnAttributesKeys[j++]);
                    }
                }
            }

            if (locationNames.Any())
            {
                if (feature.Attributes == null)
                {
                    feature.Attributes = new DictionaryFeatureAttributeCollection();
                }
                var syncedList = new GeometryPointsSyncedList<string>
                {
                    CreationMethod = DefaultLocationName,
                    Feature = feature
                };
                foreach (var locationName in locationNames)
                {
                    syncedList[locationName.Key] = locationName.Value;
                }
                feature.Attributes[Feature2D.LocationKey] = syncedList;
            }

            return feature;
        }

        private string DefaultLocationName(IFeature feature, int i)
        {
            var feature2D = feature as Feature2D;
            if (feature2D != null)
            {
                return CreateNameByIndex(feature2D.Name, i);
            }
            return (i + 1).ToString("D4");
        }

        private string CreateNameByIndex(string featureName, int i)
        {
            return featureName + "_" + (i + 1).ToString("D4");
        }

        private void BuildColumnValuesFromFeature<TS>(T feature2D, List<IList<TS>> columnValues, string[] columnAttributeKeys, ref int numColumns)
        {
            foreach (var key in columnAttributeKeys)
            {
                if (!feature2D.Attributes.ContainsKey(key)) continue;
                var valueList = feature2D.Attributes[key] as IList<TS>;
                if (valueList != null)
                {
                    columnValues.Add(valueList);
                    numColumns++; // add value columns
                }
            }
        }

        private void ConstructLineContent<TS>(int lineNumber, List<IList<TS>> columnValues, TS defaultValue, string[] columnAttributeKeys, ref string lineContent, string featureName)
        {
            foreach (var columnValueList in columnValues)
            {
                if (lineNumber < columnValueList.Count)
                {
                    lineContent += string.Format("  {0:E15}", columnValueList[lineNumber]);
                }
                else
                {
                    Log.WarnFormat(
                        "Feature {0} has a smaller number of attribute values than geometry points for {1}; filling up remaining entries with zero",
                        featureName,
                        columnAttributeKeys[columnValues.IndexOf(columnValueList)]);

                    lineContent += string.Format("  {0:E15}", defaultValue);
                }
            }
        }

        protected static void AssignDoubleValuesToAttribute(IList<double> columnValues, IFeature feature, string key)
        {
            GeometryPointsSyncedList<double> syncedList;
            if (feature.Attributes.ContainsKey(key) && feature.Attributes[key] is GeometryPointsSyncedList<double>)
            {
                syncedList = (GeometryPointsSyncedList<double>) feature.Attributes[key];
            }
            else
            {
                syncedList = new GeometryPointsSyncedList<double>
                {
                    CreationMethod = (f, i) => 0.0,
                    Feature = feature
                };
                feature.Attributes[key] = syncedList;
            }

            for (var i = 0; i < columnValues.Count; ++i)
            {
                syncedList[i] = columnValues[i];
            }
        }

        protected static void AssignStringValuesToAttribute(IList<string> columnValues, IFeature feature, string key)
        {
            var syncedList = new GeometryPointsSyncedList<string>
            {
                CreationMethod = (f,i) => string.Empty,
                Feature = feature
            };
            feature.Attributes[key] = syncedList;

            for (var i = 0; i < columnValues.Count; ++i)
            {
                syncedList[i] = columnValues[i];
            }
        }
    }
}
