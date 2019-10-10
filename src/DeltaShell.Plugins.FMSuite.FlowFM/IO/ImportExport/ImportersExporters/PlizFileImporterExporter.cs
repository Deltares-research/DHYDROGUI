using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters
{
    /// <summary>
    /// Imports and exports features of type <see cref="TFeat"/> from and to a .pliz file.
    /// </summary>
    /// <typeparam name="TParent"> The type of the parent. </typeparam>
    /// <typeparam name="TFeat"> The type of the feature. </typeparam>
    public class PlizFileImporterExporter<TParent, TFeat> : Feature2DImportExportBase<TFeat>
        where TFeat : class, IFeature, INameable, new() where TParent : INameable
    {
        /// <summary>
        /// Gets the name of the importer.
        /// </summary>
        /// <value>
        /// The name of the importer.
        /// </value>
        protected override string ImporterName => "Features from .pliz file";

        /// <summary>
        /// Gets the name of the exporter.
        /// </summary>
        /// <value>
        /// The name of the exporter.
        /// </value>
        protected override string ExporterName => "Features to .pliz file";

        /// <summary>
        /// Gets the file filter.
        /// </summary>
        /// <value>
        /// The file filter.
        /// </value>
        public override string FileFilter => "Feature polyline-z files (*.pliz)|*.pliz";

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public override string Category => "Feature geometries";

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public override string Description => string.Empty;

        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <value>
        /// The image.
        /// </value>
        public override Bitmap Image => Resources.TextDocument;

        /// <summary>
        /// Get the source types
        /// </summary>
        /// <returns>The source types</returns>
        public override IEnumerable<Type> SourceTypes()
        {
            yield return typeof(TParent);
            yield return typeof(IList<TParent>);
        }

        /// <summary>
        /// Gets the types of supported items.
        /// </summary>
        /// <value>
        /// The types of supported items.
        /// </value>
        public override IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IList<TParent>);
            }
        }

        /// <summary>
        /// Gets or sets the function to create from feature.
        /// </summary>
        /// <value>
        /// Function to create from feature.
        /// </value>
        public Func<TFeat, TParent> CreateFromFeature { get; set; }

        /// <summary>
        /// Gets or sets the function to get the feature.
        /// </summary>
        /// <value>
        /// The function to get the feature.
        /// </value>
        public Func<TParent, TFeat> GetFeature { get; set; }

        /// <summary>
        /// Gets or sets the create delegate.
        /// </summary>
        /// <value>
        /// The create delegate to create the feature.
        /// </value>
        public Func<List<Coordinate>, string, TFeat> CreateDelegate { protected get; set; }

        /// <summary>
        /// Gets or sets the after import action.
        /// </summary>
        /// <value>
        /// The action to perform after an import.
        /// </value>
        public Action<IList<TFeat>> AfterImportAction { get; set; }

        /// <summary>
        /// Imports the file at the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Called when item is importer
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="target">The target to import on.</param>
        /// <returns></returns>
        protected override object OnImportItem(string path, object target = null)
        {
            if (typeof(TParent).IsAssignableFrom(typeof(TFeat)))
            {
                object importedItem = base.OnImportItem(path, target);
                AfterImportAction?.Invoke(target as IList<TFeat>);
                return importedItem;
            }

            if (target is IList<TParent> list)
            {
                var featureList = new List<TFeat>();
                base.OnImportItem(path, featureList);
                AddOrReplace(list, featureList.Select(CreateParentFromFeature), EqualityComparer);
            }

            return target;
        }

        /// <summary>
        /// Creates the parent from feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">
        /// Thrown when object of type <see cref="TParent"/> cannot be created from type of <see cref="TFeat"/>
        /// </exception>
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
                return (TParent)o;
            }

            throw new InvalidCastException(string.Format("Cannot create object of type {0} from feature of type {1}",
                                                         typeof(TParent), typeof(TFeat)));
        }


        /// <summary>
        /// Exports the specified features.
        /// </summary>
        /// <param name="features">The features.</param>
        /// <param name="path">The path.</param>
        protected override void Export(IEnumerable<TFeat> features, string path)
        {
            if (Path.GetExtension(path) == ".pliz")
            {
                var writer = new PlizFile<TFeat>
                {
                    CreateDelegate = CreateDelegate,
                };
                writer.Write(path, features);
            }
        }

        /// <summary>
        /// Exports the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="path">The file path.</param>
        /// <returns></returns>
        public override bool Export(object item, string path)
        {
            string file = path;

            if (file == null && Files != null && Files.Any())
            {
                file = Files[0];
            }

            if (file == null)
            {
                return false;
            }

            IEnumerable<TFeat> itemsToExport = Enumerable.Empty<TFeat>();

            if (typeof(TFeat).IsAssignableFrom(typeof(TParent)))
            {
                if (item is IList<TParent> list)
                {
                    itemsToExport = list.Cast<TFeat>();
                }
                else if (item is TParent)
                {
                    itemsToExport = new List<TFeat>(new[]
                    {
                        (TFeat) item
                    });
                }
            }
            else
            {
                if (item is IList<TParent> list)
                {
                    itemsToExport = list.Select(GetFeatureFromParent);
                }
                else if (item is TParent)
                {
                    itemsToExport = new List<TFeat>(new[]
                    {
                        GetFeatureFromParent((TParent) item)
                    });
                }
            }

            BeforeExportActionDelegate?.Invoke(itemsToExport);

            Export(itemsToExport, file);

            AfterExportActionDelegate?.Invoke(itemsToExport);
            return true;
        }

        /// <summary>
        /// Gets the feature from parent.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException">
        /// Thrown when feature of type <see cref="TFeat"/> cannot be created from type of <see cref="TParent"/>
        /// </exception>
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
    }
}