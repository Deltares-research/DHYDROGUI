using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class WaterFlowFmAndF1DFileExporter : IFileExporter
    {
        public string Name { get { return "Flow Flexible Mesh model with 1D Network"; } }
        public string Description { get { return Name; } }
        public string Category { get { return "General"; } }

        public bool Export(object item, string path)
        {
            // Check if the item is set
            if (item == null)
            {
                throw new ArgumentException("Item to export Flow Flexible Mesh model with 1D Network is not set");
            }

            // Check if the export path is set
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Cannot export to unknown location, path is null or empty");
            }

            // Check if the item is set
            var waterFlowFMModel = item as WaterFlowFMModel;
            if (waterFlowFMModel == null)
            {
                throw new ArgumentException("Unexpected object type: " + item.GetType());
            }
            var fullPath = path;
            if (Directory.Exists(path))
            {
                fullPath = Path.Combine(path, waterFlowFMModel.Name + ".mdu");
            }
           return WaterFlowFMModelWriter.Write(fullPath, waterFlowFMModel);
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(WaterFlowFMModel);
        }

        [ExcludeFromCodeCoverage]
        public Bitmap Icon { get; private set; }

        public bool CanExportFor(object item)
        {
            return true;
        }
        
        public string FileFilter { get { return "Flexible Mesh Model Definition|*.mdu"; } }
    }
}