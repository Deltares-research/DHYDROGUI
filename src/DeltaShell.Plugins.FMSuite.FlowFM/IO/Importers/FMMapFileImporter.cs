using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.Common;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class FMMapFileImporter : IFileImporter
    {
        public string Name
        {
            get { return "Flexible Mesh Map File"; }
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
            get { yield return typeof(FMMapFileFunctionStore); }
        }
        
        public bool OpenViewAfterImport { get { return true; } }

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
            get { return "FM Map File|*_map.nc"; }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }
        
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target = null)
        {
            return new DataItem()
            {
                Value = new FMMapFileFunctionStore
                {
                    Path = path,
                }
            };
        }
    }
};