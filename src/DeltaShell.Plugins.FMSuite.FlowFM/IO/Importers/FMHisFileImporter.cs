using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class FMHisFileImporter : IFileImporter
    {
        public string Name
        {
            get { return "Flexible Mesh His File"; }
        }

        public string Category
        {
            get { return "2D / 3D"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.unstrucWater; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(FMHisFileFunctionStore); }
        }

        public bool OpenViewAfterImport { get { return false; } }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public string FileFilter
        {
            get { return "FM His File|*_his.nc"; }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target = null)
        {
            return new FMHisFileFunctionStore(path);
        }
    }
}