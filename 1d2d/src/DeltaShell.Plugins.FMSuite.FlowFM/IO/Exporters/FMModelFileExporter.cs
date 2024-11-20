using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    /// <summary>
    /// Provides an exporter for a D-FlowFM model file.
    /// </summary>
    public class FMModelFileExporter : IDimrModelFileExporter
    {
        /// <inheritdoc/>
        public string Name => "Flow Flexible Mesh model";

        /// <inheritdoc/>
        public string Category => "General";

        /// <inheritdoc/>
        public string Description => Name;

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public Bitmap Icon { get; private set; }

        /// <inheritdoc/>
        public string FileFilter => "Flexible Mesh Model Definition|*.mdu";

        /// <summary>
        /// Gets or sets the directory to export the D-FLowFM model file to.
        /// </summary>
        public string ExportDirectory { get; set; }

        /// <inheritdoc/>
        public bool Export(object item, string path)
        {
            Ensure.NotNull(item, nameof(item));

            // Check if the item is set
            var waterFlowFMModel = item as WaterFlowFMModel;
            if (waterFlowFMModel == null)
            {
                throw new ArgumentException("Unexpected object type: " + item.GetType());
            }

            // Check if either a path or a directory is specified
            if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(ExportDirectory))
            {
                throw new ArgumentException("No export path or directory specified.");
            }

            string fullPath = ExportDirectory ?? path;
            if (Directory.Exists(fullPath))
            {
                fullPath = waterFlowFMModel.GetMduExportPath(fullPath);
            }

            return waterFlowFMModel.ExportTo(fullPath, false);
        }

        /// <inheritdoc/>
        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(WaterFlowFMModel);
        }

        /// <inheritdoc/>
        public bool CanExportFor(object item)
        {
            return true;
        }
    }
}