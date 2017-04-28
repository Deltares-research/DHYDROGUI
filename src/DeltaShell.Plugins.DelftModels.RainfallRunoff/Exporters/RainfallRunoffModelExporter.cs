using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    public class RainfallRunoffModelExporter : IFileExporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RainfallRunoffModelExporter));
        
        public string Name
        {
            get { return "Rainfall Runoff Model exporter"; }
        }

        public string Category
        {
            get { return ""; }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(RainfallRunoffModel);
        }

        public string FileFilter
        {
            get { return "RR file folder name export|*."; }
        }

        public Bitmap Icon { get; private set; }
        public bool CanExportFor(object item)
        {
            return true;
        }

        public bool Export(object item, string path)
        {
            var model = item as RainfallRunoffModel;
            if (model == null) return false;
            var bcWriter = new RainfallRunoffBoundaryDataFileWriter();
            bcWriter.WriteFile(Path.Combine(Path.GetFullPath(path), "BoundaryConditions.bc"), model);
            model.ModelController.GetWorkingDirectoryDelegate = () => Path.GetFullPath(path); 
            return model.ModelController.WriteFiles();
        }
    }
}