using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.Dimr;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class WaterFlowModel1DFileImporter : IDimrModelFileImporter
    {
        public string Name { get { return "WaterFlowModel1D Importer (md1d)"; } }
      
        public string Category { get { return "1D Standalone Models"; } }
        
        public Bitmap Image { get; private set; }
        
        public IEnumerable<Type> SupportedItemTypes { get { yield return typeof(WaterFlowModel1D); } }

        public bool CanImportOn(object targetObject)
        {
            return false;
        }

        public bool CanImportOnRootLevel { get {return true;} }

        public string FileFilter { get { return "md1d|*.md1d"; } }
        
        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }
        
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get { return true; } }

        public object ImportItem(string path, object target = null)
        {
            return WaterFlowModel1DFileReader.Read(path, (currentStepName, currentStep, totalSteps) => ProgressChanged(currentStepName, currentStep, totalSteps));
        }

        public string LibraryName
        {
            get { return "cf_dll"; }
        }
    }
}