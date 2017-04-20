using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace DeltaShell.Plugins.Fews
{
    /// <summary>
    /// Wrapper class for reading shape files
    /// </summary>
    public class ShapeFileReader
    {
        private readonly string fileName;

        public ShapeFileReader(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.Trim() == "")
                throw new ArgumentNullException("fileName");

            this.fileName = fileName;
        }

        /// <summary>
        /// Reads all data from the shape file
        /// </summary>
        /// <remarks>
        /// This list structure represents a feature collection. One DelftTools.Utils.Tuple represents one feature.
        /// The dictionary represents the feature attributes which are read from the database (dbf) file
        /// The geometry (first argument type of the DelftTools.Utils.Tuple) is the geometry that is read from the binary shape (shp) file
        /// </remarks>
        /// <returns>Returns a list of DelftTools.Utils.Tuples representing a feature with geometry and attributes</returns>
        public IEnumerable<DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>> Read()
        {
            using (var reader = new ShapefileDataReader(fileName, GeometryFactory.Default))
            {
                while (reader.Read())
                {
                    IDictionary<string,object> attributes = new Dictionary<string, object>();
                    if (!IgnoreAttributeData)
                    {
                        for (int i = 0; i < reader.DbaseHeader.NumFields; i++)
                        {
                            string colName = reader.DbaseHeader.Fields[i].Name;
                            object rowValue = reader.GetValue(i);
                            if (!attributes.ContainsKey(colName))
                                attributes.Add(colName, rowValue);
                        }
                    }
                    yield return new DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>(reader.Geometry, attributes);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value indicating that when false only geometries are read
        /// </summary>
        protected bool IgnoreAttributeData { get; set; }
    }
}