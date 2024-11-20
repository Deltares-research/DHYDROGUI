using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using SharpMap.CoordinateSystems.Transformations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    /// <summary>
    /// ShapeFile importer is responsible for importing ESRI .shp files as
    /// Feature2Ds into FlowFM. It provides a very basic implementation, which
    /// ensures the Geometry to be set correctly. Extra data can be loaded
    /// through the afterCreateAction.
    /// </summary>
    /// <typeparam name="TGeometry"> The type of the geometry. </typeparam>
    /// <typeparam name="TFeature2D">The type of the Feature2D. </typeparam>
    /// <remarks>
    /// This class should be constructed with <see cref="ShapeFileImporterFactory"/>.
    /// </remarks>
    /// <seealso cref="MapFeaturesImporterBase"/>
    /// <seealso cref="IFeature2DImporterExporter"/>
    public class ShapeFileImporter<TGeometry, TFeature2D> : MapFeaturesImporterBase,
                                                            IFeature2DImporterExporter where TGeometry : IGeometry
                                                                                       where TFeature2D : IFeature
    {
        /// <summary> Logger of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>. </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof(ShapeFileImporter<TGeometry, TFeature2D>));

        /// <summary>
        /// Construct a new <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>
        /// with the given <paramref name="readFunc"/>.
        /// </summary>
        /// <param name="readFunc"> The read function. </param>
        /// <param name="afterCreateAction">
        /// An optional <see cref="Action"/> describing the read IFeature, the
        /// TFeature2D that is created upon import, and the set of target
        /// items. This function is executed after the creation of each element
        /// during the <see cref="OnImportItem"/>. It can be used to add
        /// additional data, or make modifications to the constructed feature.
        /// <see cref="ShapeFileImporterFactory.AfterFeatureCreateActions"/>
        /// describes a set of predefined AfterFeatureCreateActions
        /// </param>
        /// <remarks>
        /// This function should not be called directly. Use
        /// <see cref="ShapeFileImporterFactory.Construct{TGeometry,TFeature2D}"/>
        /// to construct new instances of this class.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="readFunc"/> == null.
        /// </exception>
        internal ShapeFileImporter(Func<string, ILog, IEnumerable<IFeature>> readFunc,
                                   Action<IFeature, TFeature2D, IEnumerable<TFeature2D>> afterCreateAction = null)
        {
            this.readFunc = readFunc ?? throw new ArgumentNullException(nameof(readFunc));
            this.afterCreateAction = afterCreateAction;

            Mode = Feature2DImportExportMode.Import;
        }

        #region IFileImporter

        /// <summary>
        /// Get the name of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>.
        /// </summary>
        /// <value> The name of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>. </value>
        public override string Name => "ESRI Shapefile importer";

        /// <summary>
        /// Get the category of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>.
        /// </summary>
        /// <value> The category of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>. </value>
        public override string Category => "2D / 3D";

        /// <summary>
        /// Get the description of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>.
        /// </summary>
        /// <value> The category of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>. </value>
        public override string Description => "ESRI Shapefile importer";

        /// <summary>
        /// Get the icon of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>.
        /// </summary>
        /// <value> The icon of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>. </value>
        [ExcludeFromCodeCoverage]
        public override Bitmap Image => null;

        /// <summary>
        /// Get the set of types this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/> can import.
        /// </summary>
        /// <value>The set of types this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/> can import.</value>
        public override IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IList<TFeature2D>);
            }
        }

        /// <summary>
        /// Get whether this instance can import on root level.
        /// </summary>
        /// <value><c>true</c> if this instance can import on root level; otherwise, <c>false</c>.</value>
        public override bool CanImportOnRootLevel => false;

        /// <summary>
        /// Get or set the coordinate transformation.
        /// </summary>
        /// <value>The coordinate transformation.</value>
        [ExcludeFromCodeCoverage]
        public ICoordinateTransformation CoordinateTransformation { get; set; }

        /// <summary>
        /// Get the file filter of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>.
        /// </summary>
        /// <value>The file filter of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>. </value>
        public override string FileFilter => "Shape file (*.shp)|*.shp";

        /// <summary>
        /// Get the mode of this <see cref="ShapeFileImporter{TGeometry, TFeature2D}"/>.
        /// </summary>
        /// <value> <c></c>Feature2DImportExportMode.Import<c></c> </value>
        public Feature2DImportExportMode Mode { get; }

        [ExcludeFromCodeCoverage]
        public string[] Files { get; set; }

        [ExcludeFromCodeCoverage]
        public IEqualityComparer EqualityComparer { get; set; }

        [ExcludeFromCodeCoverage]
        public Func<object, object, bool> ShouldReplace { get; set; }

        [ExcludeFromCodeCoverage]
        public override string TargetDataDirectory { get; set; }

        [ExcludeFromCodeCoverage]
        public override bool ShouldCancel { get; set; }

        [ExcludeFromCodeCoverage]
        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        [ExcludeFromCodeCoverage]
        public override bool OpenViewAfterImport { get; }

        /// <summary>
        /// Import the features described in the .shp file at
        /// <paramref name="path"/> and at these to <paramref name="target"/>.
        /// </summary>
        /// <param name="path"> The path. </param>
        /// <param name="target">The target. </param>
        /// <returns> IF success THEN target ELSE null </returns>
        protected override object OnImportItem(string path, object target = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.ErrorFormat("No file was presented to import from.");
                return null;
            }

            if (target == null)
            {
                Log.ErrorFormat("No target was presented to import to.");
                return null;
            }

            var featureItems = (IList<TFeature2D>) target;

            ProgressChanged?.Invoke($"Reading {path}...", 0, 3);
            IEnumerable<IFeature> rawFeatures = ReadFeatures(path);

            ProgressChanged?.Invoke($"Constructing features...", 1, 3);
            foreach (IFeature feature in rawFeatures)
            {
                var instance = (TFeature2D) Activator.CreateInstance(typeof(TFeature2D));
                instance.Geometry = ShapeFileImporterHelper.ConvertGeometry<TGeometry>(feature);
                OnAfterCreate(feature, instance, featureItems);

                featureItems.Add(instance);
            }

            ProgressChanged?.Invoke($"Performing coordinate transformation...", 2, 3);
            if (CoordinateTransformation != null)
            {
                TransformGeometry(featureItems);
            }

            return featureItems;
        }

        /// <summary>
        /// Reads the feature stored in .shp file at <paramref name="path"/>
        /// </summary>
        /// <param name="path"> The path to the .shp file. </param>
        /// <returns>
        /// A list of <see cref="IFeature"/> describing the shapes in <paramref name="path"/>
        /// </returns>
        private IEnumerable<IFeature> ReadFeatures(string path)
        {
            return readFunc.Invoke(path, Log);
        }

        /// <summary> The read function. </summary>
        private readonly Func<string, ILog, IEnumerable<IFeature>> readFunc;

        /// <summary>
        /// Function to modify a <paramref name="targetObject"/> created with <paramref name="srcObj"/>
        /// after its creation.
        /// </summary>
        /// <param name="srcObj"> The source object. </param>
        /// <param name="targetObject"> The target object. </param>
        /// <param name="targets"> The targets to which the newly created object will be added. </param>
        private void OnAfterCreate(IFeature srcObj, TFeature2D targetObject, IEnumerable<TFeature2D> targets)
        {
            afterCreateAction?.Invoke(srcObj, targetObject, targets);
        }

        /// <summary> The after create action. </summary>
        private readonly Action<IFeature, TFeature2D, IEnumerable<TFeature2D>> afterCreateAction;

        /// <summary>
        /// Transform the geometry with the <see cref="CoordinateTransformation"/>
        /// of this <see cref="ShapeFileImporter{TGeometry,TFeature2D}"/>.
        /// </summary>
        /// <param name="features">The features to be transformed.</param>
        /// <remarks> features != null && CoordinateTransform != null. </remarks>
        private void TransformGeometry(IEnumerable<TFeature2D> features)
        {
            foreach (TFeature2D feature in features)
            {
                feature.Geometry = GeometryTransform.TransformGeometry(feature.Geometry,
                                                                       CoordinateTransformation.MathTransform);
            }
        }

        #endregion
    }
}