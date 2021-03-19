using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class WaterFlowFMFileImporter : ModelFileImporterBase, IDimrModelFileImporter
    {
        private readonly ILog log = LogManager.GetLogger(typeof (WaterFlowFMFileImporter));
        private readonly Func<string> StoreWorkingDirectoryPathFunc;

        /// <summary>
        /// Constructor needed for connecting the Application.WorkingDirectory to the WaterFlowFMModel Working Directory.
        /// </summary>
        /// <param name="getWorkingDirectoryPathFunc"> </param>
        public WaterFlowFMFileImporter(Func<string> getWorkingDirectoryPathFunc)
        {
            StoreWorkingDirectoryPathFunc = getWorkingDirectoryPathFunc;
        }

        public override string Name
        {
            get { return "Flow Flexible Mesh Model"; }
        }
        public override string Description { get { return Name; } }
        public override string Category
        {
            get { return "1D / 2D"; }
        }
        
        public override Bitmap Image
        {
            get { return Resources.unstrucModel; }
        }

        public override IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(IHydroModel); }
        }

        public override bool OpenViewAfterImport
        {
            get { return true; }
        }

        public override bool CanImportOn(object targetObject)
        {
           return targetObject is ICompositeActivity || targetObject is WaterFlowFMModel;
        }

        public override bool CanImportOnRootLevel
        {
            get { return true; }
        }

        public override string FileFilter
        {
            get { return "Flexible Mesh Model Definition|*.mdu"; }
        }

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        protected override object OnImportItem(string path, object target = null)
        {
            try
            {
                var importedFmModel = new WaterFlowFMModel(path, ProgressChanged)
                {
                    WorkingDirectoryPathFunc = StoreWorkingDirectoryPathFunc,
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

        public string MasterFileExtension => "mdu";
    }
}