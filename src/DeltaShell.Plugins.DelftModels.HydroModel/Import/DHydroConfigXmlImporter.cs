using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Import
{
    public class DHydroConfigXmlImporter: IFileImporter
    {
        private readonly Func<List<IDimrModelFileImporter>> _getDimrModelFileImporters;

        public DHydroConfigXmlImporter(Func<List<IDimrModelFileImporter>> dimrFileImporters)
        {
            _getDimrModelFileImporters = dimrFileImporters;
        }

        public bool CanImportOn(object targetObject)
        {
            return targetObject is Project;
        }

        public object ImportItem(string path, object target = null)
        {
            var dimrModelFileImporters = _getDimrModelFileImporters?.Invoke() ?? new List<IDimrModelFileImporter>();
            return HydroModelReader.Read(path, dimrModelFileImporters );
        }


        public string Name
        {
            get { return "Integrated Model Configuration File Importer"; }
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
    }
}
