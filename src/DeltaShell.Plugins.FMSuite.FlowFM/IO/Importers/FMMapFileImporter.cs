using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class FMMapFileImporter : IFileImporter
    {
        public string Name => "Flexible Mesh Map File";

        public string Category => "D-Flow FM 2D/3D";

        public string Description => string.Empty;

        public Bitmap Image => Resources.unstrucWater;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(FMMapFileFunctionStore);
            }
        }

        public bool OpenViewAfterImport => true;

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel => true;

        public string FileFilter => "FM Map File|*_map.nc";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target = null)
        {
            return new DataItem()
            {
                Value = new FMMapFileFunctionStore(null)
                {
                    Path = path,
                }
            };
        }
    }
};