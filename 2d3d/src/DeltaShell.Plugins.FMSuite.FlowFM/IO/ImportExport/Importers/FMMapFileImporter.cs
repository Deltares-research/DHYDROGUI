using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class FMMapFileImporter : IFileImporter
    {
        public string Name => Resources.FMMapFileImporter_Name_Flexible_Mesh_Map_File;

        public string Category => Resources.FMImporters_Category_D_Flow_FM_2D_3D;

        public string Description => string.Empty;

        public Bitmap Image => Resources.unstrucWater;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IFMMapFileFunctionStore);
            }
        }

        public bool OpenViewAfterImport => true;

        public bool CanImportOnRootLevel => true;

        public string FileFilter => $"FM Map File|*{FileConstants.MapFileExtension}";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public object ImportItem(string path, object target = null)
        {
            return new DataItem {Value = new FMMapFileFunctionStore {Path = path}};
        }
    }
};