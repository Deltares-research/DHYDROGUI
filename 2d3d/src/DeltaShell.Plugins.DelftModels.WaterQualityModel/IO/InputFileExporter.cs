using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Model;
using log4net;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class InputFileExporter : IFileExporter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InputFileExporter));

        public string Name => "Input file exporter";

        public string Category => "Water quality";

        public string Description => string.Empty;

        public string FileFilter => "input file and includes|*.inp";

        public Bitmap Icon => null;

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

            ValidationReport validationReport = new WaterQualityModelValidator().Validate(model);
            if (validationReport.ErrorCount > 0)
            {
                log.Error("Water quality model is not valid. Please check the validation report.");
                return false;
            }

            string directoryName = Path.GetDirectoryName(path);
            if (directoryName == null || !Directory.Exists(directoryName))
            {
                log.ErrorFormat("Could not find directory '{0}'", directoryName);
                return false;
            }

            WaqInitializationSettings waqInitializationSettings =
                WaqInitializationSettingsBuilder.BuildWaqInitializationSettings(model);
            File.WriteAllText(path, waqInitializationSettings.InputFile.Content);

            string includeDirectory = Path.Combine(directoryName, FileConstants.IncludesDirectoryName);
            new WaqFileBasedPreProcessor().WriteIncludeFilesAndBinaryFiles(waqInitializationSettings, includeDirectory);

            return true;
        }

        public IEnumerable<Type> SourceTypes()
        {
            yield return typeof(WaterQualityModel);
        }
    }
}