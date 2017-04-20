using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NtsShapeFileWriter = NetTopologySuite.IO.ShapefileDataWriter;

namespace DeltaShell.Plugins.Fews
{
    public class ShapeFileWriter
    {
        private readonly string fileName;

        public ShapeFileWriter(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || fileName.Trim() == "") 
                throw new ArgumentNullException("fileName");

            this.fileName = fileName;
        }

        ///<summary>
        /// Creates the three (mandatory) files (.dbf, .shx, .shp) belonging to shape files 
        /// and writes the feature data contained in the feature collection
        /// </summary>
        /// <param name="path">The output folder</param>
        /// <param name="name">The name of the file/layer (without extension)</param>
        /// <param name="featureCollection">The collection containing all data to be written to shapefiles</param>
        /// <remarks>
        /// If the files already exist they will be overriden
        /// </remarks>
        /// <exception cref="DirectoryNotFoundException" />
        /// <exception cref="ArgumentNullException">
        /// When the name is empty or null. A valid name is required.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// When the feature collection does not contain features.
        /// </exception>
        public static void Create(string path, string name, IEnumerable<DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>> featureCollection)
        {
            if (featureCollection == null) 
                throw new ArgumentNullException("featureCollection");

            if (string.IsNullOrEmpty(name) || name.Trim() == string.Empty)
                throw new ArgumentNullException("name");

            path = string.IsNullOrEmpty(path) || path.Trim() == "" || path.Trim() == "." ?
                Directory.GetCurrentDirectory() : path;

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(path);

            // Write the file
            var fileName = Path.Combine(path, name);
            var writer = new ShapeFileWriter(fileName) {FeatureData = featureCollection};

            writer.Write();
        }
        /// <summary>
        /// Gets or sets the feature data to write to the shape file.
        /// </summary>
        /// <remarks>
        /// This list structure represents a feature collection. One DelftTools.Utils.Tuple represents one feature.
        /// The dictionary represents the feature attributes which are written to the database (dbf) file
        /// The geometry (first argument type of the DelftTools.Utils.Tuple) is the geometry that is written to the binary shape (shp) file
        /// </remarks>
        public IEnumerable<DelftTools.Utils.Tuple<IGeometry, IDictionary<string, object>>> FeatureData { get; set; }

        /// <summary>
        /// Writes the data
        /// </summary>
        public void Write()
        {
            if (FeatureData == null)
                throw new InvalidOperationException("The feature data collection is null");

            if (!FeatureData.Any())
                throw new InvalidOperationException("The feature data collection doesn't contain data. It should contain at least one feature data DelftTools.Utils.Tuple");

            //var geomType = "";
            var featureCollection = FeatureData.Select(f =>
                                                {

/*
                                                    if (!string.IsNullOrEmpty(geomType) && f.First.GeometryType != geomType)
                                                    {
                                                        if (geomType.EndsWith("Point"))
                                                            throw new InvalidOperationException("Can't write different geometries to one shape file");

                                                        if (geomType.ToLower().EndsWith("string") && f.First.GeometryType.ToLower().Con)
                                                            throw new InvalidOperationException("Can't write different geometries to one shape file");

                                                    }
                                                                                                            
                                                    geomType = f.First.GeometryType;
*/

                                                    var attributes = new AttributesTable();
                                                    foreach (var attrKeyVal in f.Second)
                                                    {
                                                        if (string.IsNullOrEmpty(attrKeyVal.Key) || attrKeyVal.Key == string.Empty)
                                                            throw new InvalidOperationException("Found an invalid attribute name. The name was empty or null");

                                                        if (attrKeyVal.Key.Length > 11)
                                                            throw new InvalidOperationException("The name of the field was to long. Max 11 characters allowed");

                                                        attributes.AddAttribute(attrKeyVal.Key, attrKeyVal.Value);                                                        
                                                    }

                                                    return new Feature(f.First, attributes);
                                                }).OfType<IFeature>().ToList();

            var writer = new NtsShapeFileWriter(fileName)
                         {
                             Header = NtsShapeFileWriter.GetHeader(featureCollection[0], featureCollection.Count)
                         };

            try
            {
                writer.Write(featureCollection);
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException("Different geometry types in feature collection not allowed. Only one geometry (base) type is allowed.", e);
            }
            finally
            {
                // no need to close writer??
            }
            
        }
    }
}