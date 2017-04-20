using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using log4net;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public class PliFile<T> : FMSuiteFileBase, IFeature2DFileBase<T> where T : IFeature, INameable, new ()
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (PliFile<T>));

        protected readonly string[] numericColumnAttributes =
        {
            "Column3",
            "Column4",
            "Column5",
            "Column6",
            "Column7",
            "Column8"
        };

        public const string Extension = "pli";

        // Better exception message...
        public static IGeometry CreatePolyLine(IList<Coordinate> coordinates, bool checkOpen = false)
        {
            if (coordinates.Count() < 2)
            {
                throw new ArgumentException(string.Format("Cannot create polyline for {0} with less than 2 points.",
                    typeof (T).Name));
            }
            if (checkOpen && coordinates.First().Equals2D(coordinates.Last()))
            {
                throw new ArgumentException(string.Format("Cannot create closed polyline for {0}.", typeof (T).Name));
            }
            return new LineString(coordinates.ToArray());
        }

        public Func<List<Coordinate>, string, T> CreateDelegate { private get; set; }

        /// <summary>
        /// Writes a polyline file for the collection of features <see cref="features"/>.
        /// </summary>
        public virtual void Write(string pliFilePath, IEnumerable<T> features)
        {
            using (CultureUtils.SwitchToInvariantCulture())
            {
                OpenOutputFile(pliFilePath);
                try
                {
                    foreach (var feature2D in features)
                    {
                        var columnValues = new List<IList<double>>();
                        IList<string> locationNames = null;

                        var numColumns = 2; // X, Y
                        if (feature2D.Attributes != null)
                        {
                            foreach (var columnAttribute in numericColumnAttributes)
                            {
                                if (feature2D.Attributes.ContainsKey(columnAttribute))
                                {
                                    var valueList = feature2D.Attributes[columnAttribute] as IList<double>;
                                    if (valueList != null)
                                    {
                                        columnValues.Add(valueList);
                                        numColumns++; // add value columns (e.g.thin dam height) 
                                    }
                                }
                            }
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
                            var line = String.Format("{0:E15}  {1:E15}", coord.X, coord.Y);
                            foreach (var columnValueList in columnValues)
                            {
                                if (i < columnValueList.Count)
                                {
                                    line += string.Format("  {0:E15}", columnValueList[i]);
                                }
                                else
                                {
                                    Log.WarnFormat(
                                        "Feature {0} has smaller number of attribute values than geometry points for {1}; filling up remaining entries with zero",
                                        feature2D.Name,
                                        numericColumnAttributes[columnValues.IndexOf(columnValueList)]);

                                    line += string.Format("  {0:E15}", (double) 0);
                                }
                            }
                            if (locationNames != null)
                            {
                                if (i < locationNames.Count)
                                {
                                    line += string.Format(" {0}", locationNames[i]);
                                }
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

        /// <summary>
        /// Reads a polyline file and cerates a collection of features
        /// </summary>
        public virtual IList<T> Read(string pliFilePath)
        {
            var features = new EventedList<T>();

            OpenInputFile(pliFilePath);

            try
            {
                var line = GetNextLine();
                int maxColumns = 0;

                while (line != null)
                {
                    var featureName = line;
                    var subFeatureCounter = 0;

                    line = GetNextLine();
                    if (line == null)
                    {
                        throw new FormatException(
                            string.Format(
                                "Unexpected end of file; Expected number of points and columns on line {0} in file {1}",
                                LineNumber, pliFilePath));
                    }

                    var lineFields = (IList<string>) SplitLine(line).ToList();
                    if (lineFields.Count < 2)
                    {
                        throw new FormatException(string.Format("Invalid numpoints/numcolums on line {0} in file {1}",
                            LineNumber, pliFilePath));
                    }

                    var numPoints = GetInt(lineFields[0], "value for nr of points");
                    var numColumns = GetInt(lineFields[1], "value for nr of columns");

                    maxColumns = Math.Max(maxColumns, numColumns);

                    var columnValuesList =
                        new List<IList<double>>(
                            Enumerable.Range(0, numColumns - 2).Select(i => new List<double>(numPoints)));
                    var locationNames = new Dictionary<int, string>(numPoints);
                    var points = new List<Coordinate>(numPoints);

                    for (var i = 0; i < numPoints; i++)
                    {
                        line = GetNextLine();
                        if (line == null)
                        {
                            throw new FormatException(
                                string.Format(
                                    "Unexpected end of file; Expected more data (attempting to read point {0} out of {1}) on line {2} in file {3}",
                                    i + 1, numPoints, LineNumber, pliFilePath));
                        }

                        lineFields = SplitLine(line).ToList();

                        if (lineFields.Count < numColumns)
                        {
                            throw new FormatException(
                                string.Format(
                                    "Invalid point row (expected {0} columns, but was {1}) on line {2} in file {3}",
                                    numColumns, lineFields.Count, LineNumber, pliFilePath));
                        }

                        var x = GetDouble(lineFields[0]);
                        var y = GetDouble(lineFields[1]);

                        if (x.Equals(-999.0d) || y.Equals(-999.0d))
                        {
                            // separator between sub-features
                            try
                            {
                                var feature = CreateFeature2D(featureName + "-" + ++subFeatureCounter, points,
                                    numColumns, columnValuesList, locationNames);
                                features.Add(feature);
                            }
                            catch (Exception e)
                            {
                                throw new FormatException(
                                    string.Format("Failed feature construction for {0} on line {1} in file {2}: {3}",
                                        featureName, LineNumber,
                                        pliFilePath, e.Message));
                            }
                            points.Clear();
                            foreach (var columnValues in columnValuesList)
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
                                columnValuesList[j - 2].Add(GetDouble(lineFields[j]));
                            }

                            if (lineFields.Count > numColumns)
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
                            var feature = CreateFeature2D(actualFeatureName, points, numColumns, columnValuesList,
                                locationNames);
                            features.Add(feature);
                        }
                        catch (Exception e)
                        {
                            throw new FormatException(
                                string.Format("Failed feature construction for {0} on line {1} in file {2}: {3}",
                                    actualFeatureName, LineNumber,
                                    pliFilePath, e.Message));
                        }
                        points.Clear();
                        foreach (var columnValues in columnValuesList)
                        {
                            columnValues.Clear();
                        }
                        locationNames.Clear();
                    }

                    line = GetNextLine();
                }

                if (maxColumns > numericColumnAttributes.Count() + 2)
                {
                    Log.WarnFormat("In file {0}: columns {1} to {2} will be ignored.", pliFilePath,
                        numericColumnAttributes.Count() + 3, maxColumns);
                }
            }
            finally
            {
                CloseInputFile();
            }
            return features;
        }

        private T CreateFeature2D(string name, List<Coordinate> points,
            int numColumns, IEnumerable<IList<double>> columnValueLists,
            IDictionary<int, string> locationNames)
        {
            var feature = CreateDelegate != null
                ? CreateDelegate(points, name)
                : new T {Name = name, Geometry = CreatePolyLine(points)};

            if (numColumns > 2)
            {
                if (feature.Attributes == null)
                {
                    feature.Attributes = new DictionaryFeatureAttributeCollection();
                }
                var cols = columnValueLists.ToList();
                var j = 0;
                foreach (var columnValueList in cols)
                {
                    if (j < numericColumnAttributes.Count())
                    {
                        AssignValuesToAttribute(columnValueList, feature, numericColumnAttributes[j++]);
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
                    CreationMethod = BoundaryConditionSet.DefaultLocationName,
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

        private static void AssignValuesToAttribute(IList<double> columnValues, IFeature feature, string key)
        {
            GeometryPointsSyncedList<double> syncedList;
            if (feature.Attributes.ContainsKey(key) && (feature.Attributes[key] is GeometryPointsSyncedList<double>))
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
    }
}
