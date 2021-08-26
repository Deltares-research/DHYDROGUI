using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    /// <summary>
    /// Allows to do an import / export of a PLIZ file, which extends the PliImporterExporter.
    /// </summary>
    /// <typeparam name="TParent">The type of the parent.</typeparam>
    /// <typeparam name="TFeat">The type of the feat.</typeparam>
    /// <seealso cref="DeltaShell.Plugins.FMSuite.Common.IO.Feature2DImportExportBase{TFeat}" />
    public class PlizFileImporterExporter<TParent, TFeat> : Feature2DImportExportBase<TFeat>
        where TFeat : class, IFeature, INameable, new() where TParent : INameable
    {
        /// <summary>
        /// Gets the name of the importer.
        /// </summary>
        /// <value>
        /// The name of the importer.
        /// </value>
        protected override string ImporterName
        {
            get { return "Features from .pliz file"; }
        }

        /// <summary>
        /// Gets the name of the exporter.
        /// </summary>
        /// <value>
        /// The name of the exporter.
        /// </value>
        protected override string ExporterName
        {
            get { return "Features to .pliz file"; }
        }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public override string Category
        {
            get { return "Feature geometries"; }
        }

        /// <summary>
        /// Gets the file filter.
        /// </summary>
        /// <value>
        /// The file filter.
        /// </value>
        public override string FileFilter
        {
            get { return "Feature polyline-z files (*.pliz)|*.pliz"; }
        }

        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <value>
        /// The image.
        /// </value>
        public override Bitmap Image
        {
            get { return Properties.Resources.TextDocument; }
        }

        /// <summary>
        /// Sources the types.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Type> SourceTypes()
        {
            yield return typeof(TParent);
            yield return typeof(IList<TParent>);
        }

        /// <summary>
        /// Gets the supported item types.
        /// </summary>
        /// <value>
        /// The supported item types.
        /// </value>
        public override IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(IList<TParent>); }
        }

        protected override IEnumerable<TFeat> Import(string path)
        {
            if (Path.GetExtension(path) == ".pliz")
            {
                var reader = new PlizFile<TFeat>
                {
                    CreateDelegate = CreateDelegate,
                };
                return reader.Read(path, (s, c, t) => ProgressChanged?.Invoke(s, c, t));
            }
            return Enumerable.Empty<TFeat>();
        }

        protected override void Export(IEnumerable<TFeat> features, string path)
        {
            BeforeExportActionDelegate?.Invoke(features);

            if (Path.GetExtension(path) == ".pliz")
            {
                var writer = new PlizFile<TFeat>
                {
                    CreateDelegate = CreateDelegate,

                };
                writer.Write(path, features);
            }

            AfterExportActionDelegate?.Invoke(features);
        }

        /// <summary>
        /// Gets or sets the create from feature.
        /// </summary>
        /// <value>
        /// The create from feature.
        /// </value>
        public Func<TFeat, TParent> CreateFromFeature { get; set; }

        /// <summary>
        /// Gets or sets the get feature.
        /// </summary>
        /// <value>
        /// The get feature.
        /// </value>
        public Func<TParent, TFeat> GetFeature { get; set; }
        
        /// <summary>
        /// Gets or sets the create delegate.
        /// </summary>
        /// <value>
        /// The create delegate.
        /// </value>
        public Func<List<Coordinate>, string, TFeat> CreateDelegate { private get; set; }

        private TParent CreateParentFromFeature(TFeat feature)
        {
            if (CreateFromFeature != null)
            {
                return CreateFromFeature(feature);
            }
            if (feature is TParent)
            {
                //prevent compiler from whining
                object o = feature;
                return (TParent) o;
            }
            throw new InvalidCastException(
                string.Format("Cannot create object of type {0} from feature of type {1}",
                    typeof(TParent), typeof(TFeat)));
        }

        private TFeat GetFeatureFromParent(TParent parent)
        {
            if (GetFeature != null)
            {
                return GetFeature(parent);
            }
            if (typeof(TFeat).IsAssignableFrom(typeof(TParent)))
            {
                return parent as TFeat;
            }
            throw new InvalidCastException(string.Format("Cannot get feature of type {0} from object of type {1}",
                typeof(TFeat), typeof(TParent)));
        }

        protected override object OnImportItem(string path, object target = null)
        {
            if (typeof(TParent).IsAssignableFrom(typeof(TFeat)))
            {
                return base.OnImportItem(path, target);
            }
            if (target is IList<TParent> list)
            {
                var featureList = new List<TFeat>();
                base.OnImportItem(path, featureList);
                AddOrReplace(list, featureList.Select(CreateParentFromFeature),
                    EqualityComparer);
            }
            return target;
        }

        /// <summary>
        /// Exports the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public override bool Export(object item, string path)
        {
            var file = path;

            if (file == null && Files != null && Files.Any())
            {
                file = Files[0];
            }

            if (file == null) return false;

            var itemsToExport = Enumerable.Empty<TFeat>();

            if (typeof(TFeat).IsAssignableFrom(typeof(TParent)))
            {
                if (item is IList<TParent> list)
                {
                    itemsToExport = list.Cast<TFeat>();
                }
                else if (item is TParent)
                {
                    itemsToExport = new List<TFeat>(new[] {(TFeat) item});
                }
            }
            else
            {
                if (item is IList<TParent> list)
                {
                    itemsToExport = list.Select(GetFeatureFromParent);
                }
                else if (item is TParent parent)
                {
                    itemsToExport = new List<TFeat>(new[] {GetFeatureFromParent(parent)});
                }
            }
            Export(itemsToExport, file);
            return true;
        }
    }
}