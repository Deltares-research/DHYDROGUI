using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers
{
    public class WaveBoundaryFileImporter : IFileImporter
    {
        public string Name => "Wave Boundary Conditions (*.bcw)";

        public string Category { get; private set; }
        public string Description => string.Empty;
        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield break;
            }
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => "Wave Boundary Condition Files (*.bcw;*.sp2)|*.bcw;*.sp2";

        public bool OpenViewAfterImport => true;

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            throw new NotSupportedException("Importing time series on boundaries is not supported. Implement when needed.");
        }
    }
}