using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class HydFileImporter : IFileImporter
    {
        public string Name { get{ return "Hydrodynamics (*.hyd)";} }

        public string Category { get { return "Water Quality"; } }
        public string Description
        {
            get { return string.Empty; }
        }

        public Bitmap Image { get { return Resources.hydFile; } }
        
        public IEnumerable<Type> SupportedItemTypes { get { yield return typeof (WaterQualityModel); } }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel { get { return true; } }
        
        public string FileFilter { get { return "Hydrodynamics File (*.hyd)|*.hyd"; } }

        // automatic setters used by the application plugin
        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }
        
        public ImportProgressChangedDelegate ProgressChanged { get; set; }
        
        public bool OpenViewAfterImport { get { return true; } }
        
        public Action<WaterQualityModel> ExpandModelNode { get; set; }

        public bool MarkModelOutputOutOfSync { get; set; }

        public bool SkipImportTimers { get; set; }

        /// <summary>
        /// Import data on a water quality model,
        /// or create a new one if the target doesn't exist.
        /// </summary>
        public object ImportItem(string path, object target = null)
        {
            if(!File.Exists(path))
                throw new FileNotFoundException("Couldn't find file: " + path);

            SetProgress("Reading hydrodynamics file", 0, 0);
            var data = HydFileReader.ReadAll(new FileInfo(path));

            var model = target as WaterQualityModel ?? new WaterQualityModel();
            
            EventHandler progressChangedHandler = (s, e) => SetProgress(model.ProgressText, 0, 0);

            model.ProgressChanged += progressChangedHandler;

            try
            {
                model.ImportHydroData(data, skipImportTimers: SkipImportTimers, markOutputOutOfSync: MarkModelOutputOutOfSync);

                if (ExpandModelNode != null)
                {
                    ExpandModelNode(model);
                }
            }
            finally
            {
                model.ProgressChanged -= progressChangedHandler;
            }

            return model;
        }

        private void SetProgress(string currentStepName, int currentStep, int totalSteps)
        {
            if (ProgressChanged == null) return;
            ProgressChanged(currentStepName, currentStep, totalSteps);
        }
    }
}
