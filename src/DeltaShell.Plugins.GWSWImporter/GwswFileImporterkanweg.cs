using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    /// <summary>
    /// Importer for GWSW files
    /// </summary>
    /// <seealso cref="DelftTools.Shell.Core.IFileImporter" />
    public class GwswFileImporterkanweg : IFileImporter
    {
        public string Name { get; }
        public string Category { get; }
        public string Description { get; }
        public bool CanImportOn(object targetObject)
        {
            throw new NotImplementedException();
        }

        public object ImportItem(string path, object target = null)
        {
            throw new NotImplementedException();
        }

        public Bitmap Image { get; }
        public IEnumerable<Type> SupportedItemTypes { get; }
        public bool CanImportOnRootLevel { get; }
        public string FileFilter { get; }
        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get; }
    }
}