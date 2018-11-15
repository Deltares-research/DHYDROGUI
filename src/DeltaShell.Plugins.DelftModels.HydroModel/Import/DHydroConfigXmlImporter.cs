using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.IO.FileReaders.ConfigXml;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    public class DHydroConfigXmlImporter: IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DHydroConfigXmlImporter));

        public bool CanImportOn(object targetObject)
        {
            throw new NotImplementedException();
        }

        public object ImportItem(string path, object target = null)
        {
            return DelftConfigXmlFileReader.Read(path);
        }

        public string Name { get { return "DIMR Configuration File Importer (dimr.xml)"; }}
        public string Category { get { return "Dimr Configuration File"; } }
        public Bitmap Image { get; }
        public IEnumerable<Type> SupportedItemTypes { get; }
        public bool CanImportOnRootLevel { get; }
        public string FileFilter { get { return "xml|*.xml"; } }
        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        public bool OpenViewAfterImport { get {return true;} }
    }
}
