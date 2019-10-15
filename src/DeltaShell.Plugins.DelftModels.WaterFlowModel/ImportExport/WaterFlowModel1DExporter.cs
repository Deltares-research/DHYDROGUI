using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.Validation;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class WaterFlowModel1DExporter : IFileExporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModel1DExporter));

        public string Name { get { return "WaterFlowModel1D Exporter"; } }
        public string Description { get { return Name; } }
        public bool Export(object item, string filepath)
        {
            var flow1DModel = item as WaterFlowModel1D;
            if (flow1DModel == null) return false;
            var modelName = Path.GetFileNameWithoutExtension(filepath);
            
            var validationReport = flow1DModel.Validate();

            if (validationReport.Severity() == ValidationSeverity.Error)
            {
                var validationErrorMessages = validationReport.GetAllIssuesRecursive()
                    .Where(i => i.Severity == ValidationSeverity.Error)
                    .Select(i => string.Format("\t{0}: {1}", i.Subject, i.Message))
                    .ToArray();

                var errorMessage = string.Format("Validation errors: {0}", string.Join("\n", validationErrorMessages));
                Log.Error("Model validation failed. Please review the validation report.\n\r" + errorMessage);
                return false;
            }

            try
            {
                WaterFlowModel1DFileWriter.Write(filepath, flow1DModel);
                if(flow1DModel.Name != modelName)
                    flow1DModel.Name = modelName;
            }
            catch (Exception exception)
            {
                Log.Error("Model writing failed." + Environment.NewLine + exception.Message);
                return false;
            }
            return true;
        }

        public string Category
        {
            get { return "1D Standalone Models"; }
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(WaterFlowModel1D);
        }

        public string FileFilter { get { return "md1d|*.md1d"; } }
        
        public Bitmap Icon { get; private set; }
        public bool CanExportFor(object item)
        {
            return true;
        }
    }
}
