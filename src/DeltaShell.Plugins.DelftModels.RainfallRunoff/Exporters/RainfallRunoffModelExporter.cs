using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters
{
    public class RainfallRunoffModelExporter : IFileExporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RainfallRunoffModelExporter));
        
        [ExcludeFromCodeCoverage]
        public string Name
        {
            get { return "Rainfall Runoff Model exporter"; }
        }

        [ExcludeFromCodeCoverage]
        public string Category
        {
            get { return ""; }
        }

        public string Description
        {
            get { return Name; }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(RainfallRunoffModel);
        }
        
        public string FileFilter
        {
            get { return "RR file folder name export|*."; }
        }

        [ExcludeFromCodeCoverage]
        public Bitmap Icon { get; private set; }

        [ExcludeFromCodeCoverage]
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
            var nwrwWriter = new NwrwModelFileWriter(new NwrwComponentFileWriterBase[]
            {
                new Nwrw3BComponentFileWriter(model),
                new NwrwAlgComponentFileWriter(model),
                new NwrwDwfComponentFileWriter(model), 
                new NwrwTpComponentFileWriter(model), 
            });
            nwrwWriter.WriteNwrwFiles(path);
            model.ModelController.GetWorkingDirectoryDelegate = () => Path.GetFullPath(path); 
            return model.ModelController.WriteFiles();
        }
    }
}