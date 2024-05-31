using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    /// <summary>
    /// Provides an exporter for a D-FlowFM model file.
    /// </summary>
    public class WaterFlowFMFileExporter : IDimrModelFileExporter
    {
        /// <inheritdoc/>
        public string Name => "Flow Flexible Mesh model";
        
        /// <inheritdoc/>
        public string Description => Name;

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public Bitmap Icon { get; private set; }
        
        /// <inheritdoc/>
        public string FileFilter => "Flexible Mesh Model Definition|*.mdu";
        
        /// <inheritdoc/>
        public string Category => "General";

        /// <inheritdoc/>
        public bool Export(object item, string path)
        {
            // Check if the item is set
            if (item == null)
            {
                throw new Exception("Item not set") ;
            }

            // Check if the item is set
            var waterFlowFMModel = item as WaterFlowFMModel;
            if (waterFlowFMModel == null)
            {
                throw new Exception("Unexpected object type: " + item.GetType());
            }
            var fullPath = path;
            if (Directory.Exists(path))
            {
                fullPath = Path.Combine(path, waterFlowFMModel.Name + ".mdu");
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