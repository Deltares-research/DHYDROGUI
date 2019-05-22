using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters
{
    public class WaterFlowFMFileExporter : IFileExporter
    {
        public string Name => "Flow Flexible Mesh model";

        public string Category => "General";

        public string Description => string.Empty;

        public bool Export(object item, string path)
        {
            // Check if the item is set
            if (item == null)
            {
                throw new Exception("Item not set");
            }

            // Check if the item is set
            var waterFlowFMModel = item as WaterFlowFMModel;
            if (waterFlowFMModel == null)
            {
                throw new Exception("Unexpected object type: " + item.GetType());
            }

            string fullPath = path;
            if (Directory.Exists(path))
            {
                fullPath = Path.Combine(path, waterFlowFMModel.Name + ".mdu");
            }

            return waterFlowFMModel.ExportTo(fullPath, false);
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

        public string FileFilter => "Flexible Mesh Model Definition|*.mdu";
    }
}