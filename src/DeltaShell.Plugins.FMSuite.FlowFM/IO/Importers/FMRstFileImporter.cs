using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class FMRstFileImporter: IFileImporter
    {
        public Func<FileBasedRestartState, WaterFlowFMModel> GetFMModelForRestartState { get; set; }

        public string Name
        {
            get { return "Restart File"; }
        }

        public string Category
        {
            get { return "NetCdf"; }
        }

        public Bitmap Image
        {
            get { return Properties.Resources.unstrucModel; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof (FileBasedRestartState); }
        }

        public bool CanImportOn(object targetObject)
        {
            return GetFMModelForRestartState != null &&
                   GetFMModelForRestartState(targetObject as FileBasedRestartState) != null;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public string FileFilter
        {
            get { return "FM restart files|*_rst.nc"; }
        }

        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }
        
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        
        public bool OpenViewAfterImport { get; private set; }
        
        public object ImportItem(string path, object target = null)
        {
            var model = GetFMModelForRestartState == null
                ? null
                : GetFMModelForRestartState(target as FileBasedRestartState);

            if (model != null)
            {
                model.ImportRestartFile(path);
                return model.RestartInput;
            }

            return null;
        }
    }
}
