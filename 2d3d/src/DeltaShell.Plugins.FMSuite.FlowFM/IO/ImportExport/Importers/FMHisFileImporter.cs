using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class FMHisFileImporter : IFileImporter
    {
        public string Name => "Flexible Mesh His File";

        public string Category => Resources.FMImporters_Category_D_Flow_FM_2D_3D;

        public string Description => string.Empty;

        public Bitmap Image => Resources.unstrucWater;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IFMHisFileFunctionStore);
            }
        }

        public bool OpenViewAfterImport => false;

        public bool CanImportOnRootLevel => true;

        public string FileFilter => $"FM His File|*{FileConstants.HisFileExtension}";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            return new FMHisFileFunctionStore(path);
        }
    }
}