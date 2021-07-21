using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class WaterFlowFMIntoWaterFlowFMFileImporter : ModelFileImporterBase, IDimrModelFileImporter
    {
        public override string Name => "FlowFM (into FlowFM)";
        
        public override string Category => ProductCategories.OneDTwoDModelImportCategory;
        
        public override string Description => "Import a FlowFM model into an existing FlowFM model.";
        
        public override Bitmap Image => Resources.unstrucModel;

        public override bool CanImportOnRootLevel => false;
        
        public override string FileFilter => "Flexible Mesh Model Definition|*.mdu";
        
        public override string TargetDataDirectory { get; set; }
        
        public override bool ShouldCancel { get; set; }
        
        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        public override bool OpenViewAfterImport => true;
        
        public string MasterFileExtension => "mdu";

        public override IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IWaterFlowFMModel);
            }
        }
        
        public override bool CanImportOn(object targetObject)
        {
            return targetObject is WaterFlowFMModel;
        }

        protected override object OnImportItem(string path, object target = null)
        {
            return new object();
        }
        
        
    }
}