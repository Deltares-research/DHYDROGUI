using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.Properties;
using GeoAPI.Extensions.Coverages;
using SharpMap;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    /// <summary>
    /// Importer for the point values of a <see cref="Samples"/> object.
    /// </summary>
    public sealed class SamplesImporter : IFileImporter
    {
        /// <inheritdoc/>
        public string Name => Resources.PointCloudImporter_Name_Points_from_XYZ_file;

        /// <inheritdoc/>
        public string Category => Resources.GdalFileImporter_Category_Spatial_Data;

        /// <inheritdoc/>
        public string Description => Resources.PointCloudImporter_Description_Imports_XYZ_file_as_a_point_cloud_;

        /// <inheritdoc/>
        public Bitmap Image => Resources.points;

        /// <summary>
        /// The data types supported by the importer: <see cref="Samples"/>.
        /// </summary>
        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(Samples);
            }
        }

        /// <inheritdoc/>
        public bool CanImportOnRootLevel => false;

        /// <inheritdoc/>
        public string FileFilter => $"{Resources.PointCloudImporter_FileFilter_XYZ_file} (*.xyz)|*.xyz";

        /// <inheritdoc/>
        public string TargetDataDirectory { get; set; }

        /// <inheritdoc/>
        public bool ShouldCancel { get; set; }

        /// <inheritdoc/>
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        /// <inheritdoc/>
        public bool OpenViewAfterImport => false;

        /// <summary>
        /// Indicates if this importer can import on the <paramref name="targetObject"/>
        /// </summary>
        /// <param name="targetObject">Target object to check.</param>
        /// <returns>
        /// <c>true</c> when <paramref name="targetObject"/> is a <see cref="Samples"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool CanImportOn(object targetObject)
        {
            return targetObject is Samples;
        }

        /// <summary>
        /// Imports the point value data from the file at the specified path and sets it on the <see cref="Samples"/> object.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="path"/> is <c>null</c> or white space.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="target"/> is not a <see cref="Samples"/> object.
        /// </exception>
        /// <remarks>
        /// If the file at the specified <paramref name="path"/> does not exist, an error message is logged and this method
        /// returns.
        /// </remarks>
        public object ImportItem(string path, object target = null)
        {
            Ensure.NotNullOrWhiteSpace(path, nameof(path));

            if (!(target is Samples samples))
            {
                throw new ArgumentException($"{nameof(target)} is not a {typeof(IPointCloud)}.");
            }

            IList<IPointValue> pointValues = XyzFile.Read(path);
            samples.SetPointValues(pointValues);
            samples.SourceFileName = Path.GetFileName(path);

            return samples;
        }
    }
}