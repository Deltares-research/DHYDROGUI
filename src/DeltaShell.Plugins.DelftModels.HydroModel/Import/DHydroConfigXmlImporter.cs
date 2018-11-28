using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    public class DHydroConfigXmlImporter: IFileImporter
    {
        private IEnumerable<IFileImporter> fileImporters;

        private Func<List<IDimrModelFileImporter>> GetDimrModelFileImporters;

        public DHydroConfigXmlImporter(Func<List<IDimrModelFileImporter>> dimrFileImporters)
        {
            GetDimrModelFileImporters = dimrFileImporters;
        }

        public bool CanImportOn(object targetObject)
        {
            return targetObject is Project;
        }

        public object ImportItem(string path, object target = null)
        {
            return HydroModelReader.Read(path, GetDimrModelFileImporters);
        }


        public string Name
        {
            get { return "DIMR Configuration File Importer (dimr.xml)"; }
        }

        public string Category
        {
            get { return "Dimr Configuration File"; }
        }

        public Bitmap Image { get; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield break; }
        }

        public bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public string FileFilter
        {
            get { return "xml|*.xml"; }
        }

        public string TargetDataDirectory { get; set; }
        public bool ShouldCancel { get; set; }
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport
        {
            get { return true; }
        }

        public IEnumerable<IFileImporter> FileImporters
        {
            get { return FileImporters; }
        }

    }
}
