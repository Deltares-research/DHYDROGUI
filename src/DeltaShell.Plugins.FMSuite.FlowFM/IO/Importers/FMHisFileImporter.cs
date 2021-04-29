using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.Common;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class FMHisFileImporter : IFileImporter
    {
        public string Name
        {
            get { return "Flexible Mesh His File"; }
        }
        public string Description { get { return Name; } }

        public string Category
        {
            get { return ProductCategories.OneDTwoDDataImportCategory; }
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
            var currentHisFileFunctionStore = target as FMHisFileFunctionStore;
            if (currentHisFileFunctionStore == null)
                return null;
            return new FMHisFileFunctionStore(currentHisFileFunctionStore.Network, currentHisFileFunctionStore.Area){Path = path, CoordinateSystem = currentHisFileFunctionStore.CoordinateSystem};
        }
    }
}