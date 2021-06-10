using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using log4net;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using SharpMap.CoordinateSystems.Transformations;

namespace DeltaShell.Plugins.FMSuite.Common.IO.ImportExport
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

        IEqualityComparer EqualityComparer { get; set; }

        Func<object, object, bool> ShouldReplace { get; set; }
    }

    public abstract class Feature2DImportExportBase<TFeature> : MapFeaturesImporterBase, IFileExporter,
                                                                IFeature2DImporterExporter
        where TFeature : IFeature, INameable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Feature2DImportExportBase<TFeature>));

        /// <summary>
        /// Gets the editable object associated with the import action
        /// </summary>
        public Func<object, IEditableObject> GetEditableObject { get; set; }

        /// <summary>
        /// Action to perform after creating the <see cref="TFeature"/> feature
        /// (before adding features to the target)
        /// </summary>
        public Action<object, TFeature> AfterCreateAction { get; set; }

        /// <summary>
        /// Gets or sets the before export action delegate.
        /// </summary>
        /// <value>
        /// The before export action delegate.
        /// </value>
        public Action<object> BeforeExportActionDelegate { get; set; }

        /// <summary>
        /// Gets or sets the after export action delegate.
        /// </summary>
        /// <value>
        /// The after export action delegate.
        /// </value>
        public Action<object> AfterExportActionDelegate { get; set; }

        public string[] Files { get; set; }

        public Feature2DImportExportMode Mode { get; set; }

        /// <summary>
        /// Comparer used to determine if items are equal (most generate valid HashCodes)
        /// </summary>
        public IEqualityComparer EqualityComparer { get; set; }

        public ICoordinateTransformation CoordinateTransformation { private get; set; }

        /// <summary>
        /// Optional check for replacing duplicate features (return false to cancel)
        /// (object1 = current feature, object 2 = new feature to replace with)
        /// </summary>
        public Func<object, object, bool> ShouldReplace { get; set; }

        public override string Name => Mode == Feature2DImportExportMode.Export ? ExporterName : ImporterName;

        /// <summary>
        /// Gets the name of the exporter.
        /// </summary>
        /// <value>
        /// The name of the exporter.
        /// </value>
        protected abstract string ExporterName { get; }

        /// <summary>
        /// Gets the name of the importer.
        /// </summary>
        /// <value>
        /// The name of the importer.
        /// </value>
        protected abstract string ImporterName { get; }

        /// <summary>
        /// Imports the file at the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A collection of <see cref="TFeature"/> objects.</returns>
        protected abstract IEnumerable<TFeature> Import(string path);

        /// <summary>
        /// Exports the specified features.
        /// </summary>
        /// <param name="features">The features.</param>
        /// <param name="path">The path.</param>
        protected abstract void Export(IEnumerable<TFeature> features, string path);

        [InvokeRequired]
        protected void AddOrReplace<T>(IList<T> featureList, IEnumerable<T> featuresToAdd, IEqualityComparer comparer)
            where T : INameable
        {
            List<T> featuresToAddList = featuresToAdd.ToList();
            featuresToAddList.OfType<TFeature>().ForEach(f => AfterCreateAction?.Invoke(featureList, f));

            GetEditableObject?.Invoke(featureList)
                             .BeginEdit(new DefaultEditAction($"Importing features of type {typeof(T).Name}"));

            IEqualityComparer<T> equalityComparer =
                comparer != null ? (IEqualityComparer<T>)comparer
                                 : new NameableFeatureComparer<T>();

            Dictionary<int, int> hashListIndexLookup;
            try
            {
                hashListIndexLookup = featureList
                                      .Select((f, i) => new System.Tuple<int, int>(
                                                  equalityComparer.GetHashCode(f), i))
                                      .ToDictionary(t => t.Item1, t => t.Item2);
            }

            catch (ArgumentException)
            {
                log.ErrorFormat(Resources.Feature2DImportExportBase_AddOrReplace_Import_failed_Current_project_does_not_contain_unique_names_for_features);
                return;
            }

            var hashSet = new HashSet<T>(featureList, equalityComparer);
            var newItems = new List<T>();

            for (var i = 0; i < featuresToAddList.Count; i++)
            {
                ProgressChanged?.Invoke("Adding features", i, featuresToAddList.Count);

                T item = featuresToAddList[i];
                if (hashSet.Contains(item))
                {
                    int hash = equalityComparer.GetHashCode(item);
                    int index = hashListIndexLookup[hash];

                    if (ShouldReplace != null && !ShouldReplace(featureList[index], item))
                    {
                        continue;
                    }

                    featureList[index] = item;
                }
                else
                {
                    newItems.Add(item);
                }
            }

            featureList.AddRange(newItems);
            if (featureList.Any() && featureList.Select(i => i.Name).Distinct().Count() != featureList.Count)
            {
                NamingHelper.MakeNamesUnique((IEnumerable<INameable>)featureList);
                log.WarnFormat(Resources.Feature2DImportExportBase_AddOrReplace_The_list_of_imported_features_did_not_contain_unique_names_Names_were_made_unique_during_import);
            }

            GetEditableObject?.Invoke(featureList).EndEdit();
        }

        #region IFileImporter

        public override IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IList<TFeature>);
            }
        }

        public override bool CanImportOnRootLevel => false;

        public abstract override string FileFilter { get; }

        public Bitmap Icon { get; private set; }

        public bool CanExportFor(object item)
        {
            return true;
        }

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        public override bool OpenViewAfterImport => true;

        protected override object OnImportItem(string path, object target = null)
        {
            string[] files = path == null
                                 ? Files
                                 : new[]
                                 {
                                     path
                                 };

            if (target != null && !(target is IList<TFeature>))
            {
                return target;
            }

            IList<TFeature> featureList = target != null ? (IList<TFeature>)target : new List<TFeature>();

            if (files == null)
            {
                return featureList;
            }

            for (var index = 0; index < files.Length; index++)
            {
                string file = files[index];

                ProgressChanged?.Invoke($"Reading file \"{Path.GetFileName(file)}\"", index, files.Length);

                List<TFeature> featuresToImport = Import(file).ToList();

                ProgressChanged?.Invoke("Transforming coordinates", 0, 0);

                if (CoordinateTransformation != null)
                {
                    foreach (TFeature feature2D in featuresToImport)
                    {
                        feature2D.Geometry = GeometryTransform.TransformGeometry(feature2D.Geometry,
                                                                                 CoordinateTransformation
                                                                                     .MathTransform);
                    }
                }

                AddOrReplace(featureList, featuresToImport, EqualityComparer);
            }

            return featureList;
        }

        #endregion

        #region IFileExporter

        public virtual bool Export(object item, string path)
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

            IList<TFeature> featureList = null;
            if (item is TFeature feature)
            {
                featureList = new[]
                {
                    FeatureToExport(feature)
                };
            }

            if (item is IList<TFeature> features)
            {
                featureList = features.Select(FeatureToExport).ToList();
            }

            if (featureList != null)
            {
                BeforeExportActionDelegate?.Invoke(featureList);
                Export(featureList, file);
                AfterExportActionDelegate?.Invoke(featureList);
                return true;
            }

            return false;
        }

        private TFeature FeatureToExport(TFeature feature)
        {
            if (CoordinateTransformation == null)
            {
                return feature;
            }

            var clonedFeature = (TFeature)feature.Clone();
            clonedFeature.Geometry = GeometryTransform.TransformGeometry(feature.Geometry,
                                                                         CoordinateTransformation.MathTransform);
            return clonedFeature;
        }

        public virtual IEnumerable<Type> SourceTypes()
        {
            yield return typeof(TFeature);
            yield return typeof(IList<TFeature>);
        }

        #endregion
    }
}