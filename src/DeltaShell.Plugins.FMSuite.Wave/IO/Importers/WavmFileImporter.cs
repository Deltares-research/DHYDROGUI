using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Importers
{
    public class WavmFileImporter : IFileImporter
    {
        public string Name { get { return "Wave Output (WAVM)"; } }
        public string Category { get { return "2D / 3D"; } }
        public Bitmap Image { get { return Properties.Resources.wave; } }

        public IEnumerable<Type> SupportedItemTypes { get { yield return typeof (WavmFileFunctionStore); } }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel { get { return true; } }
        public string FileFilter { get { return "WAVM file|*.nc"; } }

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get { return false; } }

        public object ImportItem(string path, object target = null)
        {
            return new WavmFileFunctionStore(path);
        }
    }
}