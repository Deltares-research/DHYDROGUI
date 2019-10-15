using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class WaterFlowFMFileImporter : IFileImporter
    {
        private readonly ILog log = LogManager.GetLogger(typeof (WaterFlowFMFileImporter));

        public string Name
        {
            get { return "Flow Flexible Mesh Model"; }
        }
        public string Description { get { return Name; } }
        public string Category
        {
            get { return "2D / 3D"; }
        }
        
        public Bitmap Image
        {
            get { return Resources.unstrucModel; }
        }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(IHydroModel); }
        }

        public bool OpenViewAfterImport
        {
            get { return true; }
        }

        public bool CanImportOn(object targetObject)
        {
           return targetObject is ICompositeActivity || targetObject is WaterFlowFMModel;
        }

        public bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public string FileFilter
        {
            get { return "Flexible Mesh Model Definition|*.mdu"; }
        }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target = null)
        {
            try
            {
                var importedFmModel = new WaterFlowFMModel(path, ProgressChanged)
                {
                    ImportProgressChanged = null
                };

                //replace the FM Model
                var targetFmModel = target as WaterFlowFMModel;
                if (targetFmModel != null)
                {
                    var parent = targetFmModel.Owner();
                    
                    //add / replace the FM Model in the project
                    var folder = parent as Folder;
                    if (folder != null)
                    {
                        folder.Items.Remove(targetFmModel);
                        folder.Items.Add(importedFmModel);
                    }

                    //add / replace the FM Model in the integrated model
                    var compositeActivity = parent as ICompositeActivity;
                    if (compositeActivity != null)
                    {
                        importedFmModel.MoveModelIntoIntegratedModel(null, compositeActivity);
                    }
                    FireProgressChanged("Import finished", 10, 10);
                    return ShouldCancel ? null : importedFmModel;
                }
                
                //add / replace the FM Model in the integrated model
                var hydroModel = target as ICompositeActivity;
                if (hydroModel != null)
                {
                    importedFmModel.MoveModelIntoIntegratedModel(null, hydroModel);
                    FireProgressChanged("Import finished", 10, 10);
                    return hydroModel;
                }
                FireProgressChanged("Import finished", 10, 10);
                return ShouldCancel ? null : importedFmModel;
            }
            catch (Exception e)
            {
                if (e is ArgumentException || e is PathTooLongException || e is FormatException ||
                    e is OutOfMemoryException || e is IOException || e is InvalidOperationException)
                {
                    log.Error(String.Format("An error occurred while trying to import a {0}; Cause: ",
                        Name), e);
                    return null;
                }

                // !!Unexpected type of exception (like NotSupportedException or NotImplementedException), so fail fast!!
                throw;
            }
        }

        private void FireProgressChanged(string currentStepName, int currentStep, int totalSteps)
        {
            if (ProgressChanged == null) return;
            
            ProgressChanged(currentStepName, currentStep, totalSteps);
        }
    }
}