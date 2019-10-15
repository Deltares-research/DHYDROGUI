using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters
{
    public class WaterFlowFMFileExporter : IFileExporter
    {
        public string Name { get { return "Flow Flexible Mesh model"; } }
        public string Description { get { return Name; } }

        public string Category { get { return "General"; } }

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