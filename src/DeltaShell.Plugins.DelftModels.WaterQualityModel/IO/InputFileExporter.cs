using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class InputFileExporter : IFileExporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InputFileExporter));

        public string Name { get { return "Input file exporter"; } }

        public string Category { get { return "Water quality"; } }
        public string Description
        {
            get { return string.Empty; }
        }

        public string FileFilter { get { return "input file and includes|*.inp"; } }

        public Bitmap Icon { get{return null;} }
        public bool CanExportFor(object item)
        {
            return true;
        }

        public bool Export(object item, string path)
        {
            var model = item as WaterQualityModel;
            if (model == null)
            {
                log.ErrorFormat("Can't export model '{0}'. It is not a valid water quality model.", item);
                return false;
            }

            var validationReport = new WaterQualityModelValidator().Validate(model);
            if (validationReport.ErrorCount > 0)
            {
                log.Error("Water quality model is not valid. Please check the validation report.");
                return false;
            }

            var directoryName = Path.GetDirectoryName(path);
            if (directoryName == null || !Directory.Exists(directoryName))
            {
                log.ErrorFormat("Could not find directory '{0}'", directoryName);
                return false;
            }
            
            var waqInitializationSettings = WaqInitializationSettingsBuilder.BuildWaqInitializationSettings(model);
            File.WriteAllText(path, waqInitializationSettings.InputFile.Content);

            
            var includeDirectory = Path.Combine(directoryName, "includes_deltashell");
            new WaqFileBasedPreProcessor().WriteIncludeFilesAndBinaryFiles(waqInitializationSettings, includeDirectory);

            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof (WaterQualityModel);
        }
    }
}