using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.IO.FileReaders.ConfigXml;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    public class DHydroConfigXmlImporter: IFileImporter
    {
        public DHydroConfigXmlImporter()
        {

        }
        public bool CanImportOn(object targetObject)
        {
            throw new NotImplementedException();
        }

        public object ImportItem(string path, object target = null)
        {
            return DelftConfigXmlFileReader.Read(path);
        }

        public string Name { get; }
        public string Category { get; }
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
