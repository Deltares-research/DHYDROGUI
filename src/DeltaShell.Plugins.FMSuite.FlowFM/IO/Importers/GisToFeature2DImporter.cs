using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api;
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
                return Properties.Resources.PumpSmall;
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

        public override string FileFilter { get { return "Shape file|*.shp"; } }

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

            var importer = new ShapeFile(path);

            if (!ValidateShapeType(importer))
            {
                Log.ErrorFormat("Shape type {0} is not matching the expected type {1}", importer.ShapeType, typeof(TGeometry));
                return null;
            }

            var list = (IList)target;



            InsertFeatures(importer.Features.OfType<Feature>(), list);
            return list;
        }

        private static bool ValidateShapeType(ShapeFile importer)
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

        [InvokeRequired]
        private static void InsertFeatures(IEnumerable<Feature> features, IList list)// where TFeat : INameable
        {
            foreach (var feature in features)
            {
                    list.Add(new Feature2D() {Geometry = feature.Geometry});
            }
        }

        #endregion
    }
}