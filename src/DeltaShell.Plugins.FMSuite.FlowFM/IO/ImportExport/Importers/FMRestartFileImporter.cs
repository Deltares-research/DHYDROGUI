using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class FMRestartFileImporter : IFileImporter
    {
        private readonly Func<IEnumerable<WaterFlowFMModel>> getModels;

        public FMRestartFileImporter(Func<IEnumerable<WaterFlowFMModel>> getModels)
        {
            Ensure.NotNull(getModels, nameof(getModels));

            this.getModels = getModels;
        }

        public string Name => "Restart File";

        public string Category => "NetCdf";

        public string Description => string.Empty;

        public Bitmap Image => Resources.unstrucModel;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(RestartFile);
            }
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => $"FM restart files|*{FileConstants.RestartFileExtension}";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get; private set; }

        public bool CanImportOn(object targetObject) => targetObject is RestartFile;

        public object ImportItem(string path, object target)
        {
            Ensure.NotNull(target, nameof(target));

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty.");
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Restart file does not exist: {path}");
            }

            WaterFlowFMModel model = getModels().First(m => m.RestartInput == target);

            model.UseRestart = true;

            return model.RestartInput = new RestartFile(path);
        }
    }
}