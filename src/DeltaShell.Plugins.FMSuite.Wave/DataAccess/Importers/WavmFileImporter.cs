using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers
{
    public class WavmFileImporter : IFileImporter
    {
        public string Name => "Wave Output (WAVM)";
        public string Category => "D-Flow FM 2D/3D";

        public string Description => string.Empty;
        public Bitmap Image => Resources.wave;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(WavmFileFunctionStore);
            }
        }

        public bool CanImportOnRootLevel => true;
        public string FileFilter => "WAVM file|*.nc";

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport => false;

        public bool CanImportOn(object targetObject)
        {
            return false;
        }

        public object ImportItem(string path, object target = null)
        {
            return new WavmFileFunctionStore(path);
        }
    }
}