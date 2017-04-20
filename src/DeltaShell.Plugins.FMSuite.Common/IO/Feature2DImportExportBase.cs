using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using SharpMap.CoordinateSystems.Transformations;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public enum Feature2DImportExportMode
    {
        Import,
        Export
    }

    public interface IFeature2DImporterExporter
    {
        ICoordinateTransformation CoordinateTransformation { set; }

        string[] Files { set; }

        string FileFilter { get; }

        Feature2DImportExportMode Mode { get; }

        Func<object, bool> ShouldReplaceFeature { set; }
    }


    public abstract class Feature2DImportExportBase<TFeat> : MapFeaturesImporterBase, IFileExporter, IFeature2DImporterExporter where TFeat : IFeature, INameable
    {    
        protected abstract string ExporterName { get; }

        protected abstract string ImporterName { get; }

        public string[] Files { get; set; }
        
        public Feature2DImportExportMode Mode { get; set; }

        public Func<object,bool> ShouldReplaceFeature { private get; set; }

        public ICoordinateTransformation CoordinateTransformation { private get; set; }

        protected abstract IEnumerable<TFeat> Import(string path);

        protected abstract void Export(IEnumerable<TFeat> features, string path);

        #region IFileImporter

        public override string Name { get { return ImporterName; } }

        public override IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof (IList<TFeat>); }
        }

        public override bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public override abstract string FileFilter { get; }

        public Bitmap Icon { get; private set; }
        public bool CanExportFor(object item)
        {
            return true;
        }

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        public override bool OpenViewAfterImport
        {
            get { return true; }
        }

        protected override object OnImportItem(string path, object target = null)
        {
            var files = path == null ? Files : new[] {path};

            if (target != null && !(target is IList<TFeat>)) return target;

            var featureList = target != null ? (IList<TFeat>) target : new List<TFeat>();

            if (files == null) return featureList;

            foreach (var file in files)
            {
                var featuresToImport = Import(file).ToList();
                if (CoordinateTransformation != null)
                {
                    foreach (var feature2D in featuresToImport)
                    {
                        feature2D.Geometry = GeometryTransform.TransformGeometry(feature2D.Geometry,
                                                                                 CoordinateTransformation.MathTransform);
                    }
                }
                AddOrReplace(featureList, featuresToImport);
            }

            return featureList;
        }

        #endregion

        #region IFileExporter

        string IFileExporter.Name { get { return ExporterName; } }

        public virtual bool Export(object item, string path)
        {
            var file = path;

            if (file == null && Files != null && Files.Any())
            {
                file = Files[0];
            }

            if (file == null) return false;

            IList<TFeat> featureList = null;
            if (item is TFeat)
            {
                featureList = new[] {FeatureToExport((TFeat) item)};
            }
            if (item is IList<TFeat>)
            {
                featureList = ((IList<TFeat>) item).Select(FeatureToExport).ToList();
            }
            if (featureList != null)
            {
                Export(featureList, file);
                return true;
            }
            return false;
        }

        private TFeat FeatureToExport(TFeat feature)
        {
            if (CoordinateTransformation == null) return feature;
            var clonedFeature = (TFeat) feature.Clone();
            clonedFeature.Geometry = GeometryTransform.TransformGeometry(feature.Geometry,
                CoordinateTransformation.MathTransform);
            return clonedFeature;
        }

        public virtual IEnumerable<Type> SourceTypes()
        {
            yield return typeof (TFeat);
            yield return typeof (IList<TFeat>);
        }

        #endregion

        [InvokeRequired]
        protected void AddOrReplace<T>(IList<T> featureList, IEnumerable<T> featuresToAdd) where T : INameable
        {
            var count = featureList.Count;
            foreach (var feature in featuresToAdd)
            {
                var replaced = false;
                for (var i = 0; i < count; ++i)
                {
                    if (featureList[i].Name != feature.Name) continue;

                    if (ShouldReplaceFeature == null || ShouldReplaceFeature(featureList[i]))
                    {
                        featureList[i] = feature;
                    }
                    replaced = true;
                    break;
                }
                if (!replaced)
                {
                    featureList.Add(feature);
                }
            }
        }
    }
}
