using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Geometries;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    /// <summary>
    /// ShapeFileImporterHelper exposes the Read and supporting functions.
    /// These are used by the <see cref="ShapeFileImporter{TGeometry,TFeature2D}"/>.
    /// </summary>
    public static class ShapeFileImporterHelper
    {
        /// <summary>
        /// Read the specified .shp file at <paramref name="path"/>.
        /// </summary>
        /// <typeparam name="T"> The Type of <see cref="IGeometry"/> to which to convert to. </typeparam>
        /// <typeparam name="TGeometry"> </typeparam>
        /// <param name="path"> The path. </param>
        /// <param name="Log"> The log. </param>
        /// <returns>
        /// IF   (IsShapeTypeValid) A set of <see cref="IFeature"/> described in the file at <paramref name="path"/>.
        /// ELSE An empty set.
        /// </returns>
        /// <remarks>
        /// path != null && Path.Exists(path)
        /// </remarks>
        public static IEnumerable<IFeature> Read<TGeometry>(string path, ILog Log) where TGeometry : IGeometry
        {
            var features = new List<IFeature>();

            using (var shapeFileReader = new ShapeFile(path))
            {
                if (IsShapeTypeValid<TGeometry>(shapeFileReader))
                {
                    // We make an explicit clone of the Features. When getting the geometry or attributes of any of
                    // these features, the original file will be reopened, causing it to lock. If we make a clone now,
                    // this behaviour will not happen and the features can be used without relying on the file system.
                    features = (from IFeature feature in shapeFileReader.Features select (IFeature) feature.Clone()).ToList();
                }
                else
                {
                    Log?.Error(string.Format(Resources.ShapeFileImporterHelper_Read_Shape_type__0__is_not_matching_expected_type__1__, shapeFileReader.ShapeType, typeof(TGeometry)));
                }

                shapeFileReader.Close();
            }

            return features;
        }

        /// <summary>
        /// Determines whether the [ShapeType] is valid for [the specified reader].
        /// </summary>
        /// <typeparam name="T"> The Type of <see cref="IGeometry"/> to which to convert to. </typeparam>
        /// <param name="reader">The reader.</param>
        /// <returns>
        /// <c>true</c> if [is shape type valid] [the specified reader]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsShapeTypeValid<T>(ShapeFile reader) where T : IGeometry
        {
            switch (reader.ShapeType)
            {
                case ShapeType.Point:
                case ShapeType.PointZ:
                    return typeof(T) == typeof(IPoint);
                case ShapeType.PolyLine:
                case ShapeType.PolyLineZ:
                    return typeof(T) == typeof(ILineString);
                case ShapeType.Polygon:
                case ShapeType.PolygonZ:
                    return typeof(T) == typeof(IPolygon);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Converts the geometry from the <paramref name="shapeFeature"/> to an <see cref="IGeometry"/>.
        /// </summary>
        /// <typeparam name="T"> The Type of <see cref="IGeometry"/> to which to convert to. </typeparam>
        /// <param name="shapeFeature"> The shape feature containing the geometry. </param>
        /// <param name="factory"> The factory used to construct the geometry. </param>
        /// <returns>
        /// An <see cref="IGeometry"/> describing the geometry of <paramref name="shapeFeature"/>.
        /// </returns>
        /// <remarks>
        /// <paramref name="shapeFeature"/> != null.
        /// </remarks>
        /// <remarks>
        /// This function currently supports IPoint, ILineString, and IPolygon, other geometries will
        /// return null.
        /// </remarks>
        public static IGeometry ConvertGeometry<T>(IFeature shapeFeature,
                                                   IGeometryFactory factory = null) where T : IGeometry
        {
            Coordinate[] coordinates = shapeFeature.Geometry.Coordinates;

            if (factory == null)
            {
                factory = new GeometryFactory();
            }

            IGeometry result = null;

            Type t = typeof(T);
            if (t == typeof(IPoint))
            {
                Coordinate coordinate = coordinates.FirstOrDefault();
                if (coordinate != null)
                {
                    result = factory.CreatePoint(coordinate);
                }
            }
            else if (t == typeof(ILineString))
            {
                result = factory.CreateLineString(coordinates);
            }
            else if (t == typeof(IPolygon))
            {
                result = factory.CreatePolygon(coordinates);
            }

            return result;
        }
    }
}