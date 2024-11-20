using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using SharpMap.Data.Providers;
using SharpMap.Extensions.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class GisToFeature2DImporter<TGeometry, TFeature2D> : MapFeaturesImporterBase, IFeature2DImporterExporter where TGeometry : IGeometry where TFeature2D: Feature2D
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GisToFeature2DImporter<TGeometry, TFeature2D>));

        public GisToFeature2DImporter()
        {
            Mode = Feature2DImportExportMode.Import; 
        }

        public Func<IEnumerable, WaterFlowFMModel> GetModelForList { get; set; }

        #region IFileImporter

        public override string Name
        {
            get
            {
                return "GIS to 2D feature importer";
            }
        }
        public override string Description { get { return Name; } }
        public override string Category { get { return "2D / 3D"; } }

        public override Bitmap Image
        {
            get
            {
                if (typeof (TGeometry) == typeof (IPoint))
                {
                    return Properties.Resources.points;
                }
                if (typeof (TGeometry) == typeof (ILineString))
                {
                    return Properties.Resources.lines;
                }

                return Properties.Resources.polygon;
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

        public ICoordinateTransformation CoordinateTransformation { get; set; }

        public string[] Files { get; set; }
        public override string FileFilter { get { return "Shape file (*.shp)|*.shp|GML file (*.gml)|*.gml|GeoJSON (*.geojson)|*.geojson"; } }
        public Feature2DImportExportMode Mode { get; }
        public IEqualityComparer EqualityComparer { get; set; }
        public Func<object, object, bool> ShouldReplace { get; set; }

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }
        public override bool OpenViewAfterImport { get; }

        protected override object OnImportItem(string path, object target = null)
        {
            var filePath = path;

            if (String.IsNullOrEmpty(path) && Files != null && Files.Any())
            {
                filePath = Files.First();
            }

            if (String.IsNullOrEmpty(filePath))
            {
                Log.ErrorFormat("No file was presented to import from.");
                return null;
            }
            if (target == null)
            {
                Log.ErrorFormat("No target was presented to import to.");
                return null;
            }

            var list = (IList<TFeature2D>)target;

            var fileExtention = Path.GetExtension(filePath).ToLower();

            switch (fileExtention)
            {
                case ".shp":
                    return ImportShapeFile(filePath, list);
                case ".gml":
                    return ImportByOgrFeatureProvider(filePath, list);
                case ".geojson":
                    return ImportByOgrFeatureProvider(filePath, list);
                default:
                    return null;
            }
        }

        #endregion IFileImporter


        private IList<TFeature2D> ImportShapeFile(string path, IList<TFeature2D> list)
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

        private IList<TFeature2D> ImportByOgrFeatureProvider(string path, IList<TFeature2D> list)
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
        private void InsertFeatures(IEnumerable<IFeature> features, IList<TFeature2D> list)// where TFeat : INameable
        {
            foreach (var feature in features)
            {
                var instance = CreateInstanceOfFeature2D != null 
                    ? CreateInstanceOfFeature2D() 
                    : (TFeature2D)Activator.CreateInstance(typeof(TFeature2D));

                instance.Name = $"Imported {instance.GetType().Name}";
                instance.Geometry = ConvertGeometry(feature);
                list.Add(instance);
            }

            NamingHelper.MakeNamesUnique(list);
        }

        public Func<TFeature2D> CreateInstanceOfFeature2D { get; set; }
        

        private static bool IsShapeTypeValid(ShapeFile importer)
        {
            switch (importer.ShapeType)
            {
                case ShapeType.Point:
                    return typeof(TGeometry) == typeof(IPoint);
                case ShapeType.PolyLine:
                case ShapeType.PolyLineZ:
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