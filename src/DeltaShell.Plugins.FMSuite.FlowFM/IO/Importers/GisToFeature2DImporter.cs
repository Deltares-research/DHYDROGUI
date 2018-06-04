using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class GisToFeature2DImporter<TGeometry, TFeature2D> : MapFeaturesImporterBase  where TGeometry : IGeometry where TFeature2D: Feature2D
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GisToFeature2DImporter<TGeometry, TFeature2D>));

        public Func<IEnumerable, WaterFlowFMModel> GetModelForList { get; set; }

        public override bool OpenViewAfterImport { get { return false; } }


        #region IFileImporter

        public override string Name
        {
            get
            {
                return "GIS to 2D feature importer";
            }
        }

        public override string Category { get { return "2D / 3D"; } }

        public override Bitmap Image
        {
            get
            {
                if (typeof (TGeometry) == typeof (IPoint))
                {
                    return Properties.Resources.points;
                }
                else if (typeof (TGeometry) == typeof (ILineString))
                {
                    return Properties.Resources.lines;
                }
                else
                {
                    return Properties.Resources.polygon;
                }

            }
        }

        public override IEnumerable<Type> SupportedItemTypes
        {
            get 
            {
              yield return typeof(IList<TFeature2D>);
            }
        }

        public override bool CanImportOnRootLevel { get { return false; } }

        public override string FileFilter { get { return "Shape file (*.shp)|*.shp|GML file (*.gml)|*.gml|GeoJSON (*.geojson)|*.geojson"; } }

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        protected override object OnImportItem(string path, object target = null)
        {
            if (String.IsNullOrEmpty(path))
            {
                Log.ErrorFormat("No file was presented to import from.");
                return null;
            }
            if (target == null)
            {
                Log.ErrorFormat("No target was presented to import to.");
                return null;
            }

            var list = (IList)target;
            var fileExtention = Path.GetExtension(path).ToLower();

            switch (fileExtention)
            {
                case ".shp":
                    return ImportShapeFile(path, list);
                case ".gml":
                    return ImportByOgrFeatureProvider(path, list);
                case ".geojson":
                    return ImportByOgrFeatureProvider(path, list);
                default:
                    return null;
            }
        }

        #endregion IFileImporter


        private static IList ImportShapeFile(string path, IList list)
        {
            var importer = new ShapeFile(path);

            if (IsShapeTypeValid(importer))
            {
                InsertFeatures(importer.Features.OfType<IFeature>(), list);
            }
            else
            {
                Log.ErrorFormat("Shape type {0} is not matching the expected type {1}", importer.ShapeType, typeof(TGeometry));
            }

            importer.Close();
            importer.Dispose();
            return list;
        }


        private static IList ImportByOgrFeatureProvider(string path, IList list)
        {
            var provider = new OgrFeatureProvider(path);

            var firstFeature = provider.Features.OfType<IFeature>().FirstOrDefault();
            if (firstFeature != null)
            {
                if (IsGeometryTypeValid(firstFeature.Geometry))
                {
                    InsertFeatures(provider.Features.OfType<IFeature>(), list);
                }
                else
                {
                    Log.ErrorFormat("Import type {0} is not matching the expected type {1}", firstFeature.Geometry.GeometryType, typeof(TGeometry));
                }
            }

            provider.Close();
            provider.Dispose();

            return list;
        }

        [InvokeRequired]
        private static void InsertFeatures(IEnumerable<IFeature> features, IList list)// where TFeat : INameable
        {
            foreach (var feature in features)
            {
                var instance = (TFeature2D)Activator.CreateInstance(typeof(TFeature2D));
                instance.Name = NamingHelper.GetUniqueName<TFeature2D>("imported_feature_{0}",list as IList<TFeature2D>);
                instance.Geometry = ConvertGeometry(feature);
                list.Add(instance);
            }
        }

        private static bool IsShapeTypeValid(ShapeFile importer)
        {
            switch (importer.ShapeType)
            {
                case ShapeType.Point:
                    return typeof(TGeometry) == typeof(IPoint);
                case ShapeType.PolyLine:
                    return typeof(TGeometry) == typeof(ILineString);
                case ShapeType.Polygon:
                    return typeof(TGeometry) == typeof(IPolygon);
                default:
                    return false;
            }
        }

        private static bool IsGeometryTypeValid(IGeometry geometry)
        {
            switch (geometry.GeometryType)
            {
                case "Points":
                case "Point":
                    return typeof(TGeometry) == typeof(IPoint);
                case "LineString":
                    return typeof(TGeometry) == typeof(ILineString);
                case "Polygon":
                    return typeof(TGeometry) == typeof(IPolygon);
                default:
                    return false;
            }
        }

        private static IGeometry ConvertGeometry(IFeature shapeFeature)
        {
            var coordinates = shapeFeature.Geometry.Coordinates;
            var factory = new GeometryFactory();
            IGeometry result = null;
            if (typeof (TGeometry) == typeof (IPoint))
            {
                var coordinate = coordinates.FirstOrDefault();
                if (coordinate != null)
                {
                    result = factory.CreatePoint(coordinate);
                }
            }
            else if (typeof(TGeometry) == typeof(ILineString))
            {
                result = factory.CreateLineString(coordinates);
            }
            else if (typeof(TGeometry) == typeof(IPolygon))
            {
                result = factory.CreatePolygon(coordinates);
            }
            return result;
        }
    }
}