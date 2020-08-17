using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class FMRstFileImporter : IFileImporter
    {
        // TODO D3DFMIQ-2075
        //public Func<FileBasedRestartState, WaterFlowFMModel> GetFMModelForRestartState { get; set; }

        public string Name => "Restart File";

        public string Category => "NetCdf";

        public string Description => string.Empty;

        public Bitmap Image => Resources.unstrucModel;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield break;

                // TODO D3DFMIQ-2075
                //yield return typeof(FileBasedRestartState);
            }
        }

        public bool CanImportOnRootLevel => false;

        public string FileFilter => $"FM restart files|*{FileConstants.RestartFileExtension}";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport { get; private set; }

        public bool CanImportOn(object targetObject)
        {
            return false; 

            // TODO D3DFMIQ-2075
            //return GetFMModelForRestartState != null &&
            //       GetFMModelForRestartState(targetObject as FileBasedRestartState) != null;
        }

        public object ImportItem(string path, object target = null)
        {
            // TODO D3DFMIQ-2075
            //WaterFlowFMModel model = GetFMModelForRestartState?.Invoke(target as FileBasedRestartState);

            //if (model != null)
            //{
            //    model.ImportRestartFile(path);
            //    return model.RestartInput;
            //}

            return null;
        }
    }
}