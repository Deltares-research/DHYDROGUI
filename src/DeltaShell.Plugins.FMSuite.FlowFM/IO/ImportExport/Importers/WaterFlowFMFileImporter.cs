using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class WaterFlowFMFileImporter : IDimrModelFileImporter
    {
        private readonly ILog log = LogManager.GetLogger(typeof(WaterFlowFMFileImporter));

        private Func<string> StoreWorkingDirectoryPathFunc;

        /// <summary>
        /// Constructor needed for connecting the Application.WorkingDirectory to the WaterFlowFMModel Working Directory.
        /// </summary>
        /// <param name="getWorkingDirectoryPathFunc"> </param>
        public WaterFlowFMFileImporter(Func<string> getWorkingDirectoryPathFunc)
        {
            StoreWorkingDirectoryPathFunc = getWorkingDirectoryPathFunc;
        }

        public string Name => "Flow Flexible Mesh Model";

        public string Category => Resources.FMImporters_Category_D_Flow_FM_2D_3D;

        public string Description => string.Empty;

        public Bitmap Image => Resources.unstrucModel;

        public IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IHydroModel);
            }
        }

        public bool OpenViewAfterImport => true;

        public bool CanImportOnRootLevel => true;

        public string FileFilter => $"Flexible Mesh Model Definition|*{FileConstants.MduFileExtension}";

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public string MasterFileExtension => "mdu";

        public bool CanImportOn(object targetObject)
        {
            return targetObject is ICompositeActivity || targetObject is WaterFlowFMModel;
        }

        public object ImportItem(string path, object target = null)
        {
            try
            {
                WaterFlowFMModel importedFmModel = WaterFlowFMModel.Import(path, ProgressChanged);
                importedFmModel.WorkingDirectoryPathFunc = StoreWorkingDirectoryPathFunc;

                //replace the FM Model
                var targetFmModel = target as WaterFlowFMModel;
                if (targetFmModel != null)
                {
                    IProjectItem parent = targetFmModel.Owner();

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
                    log.Error(string.Format("An error occurred while trying to import a {0}; Cause: ",
                                            Name), e);
                    return null;
                }

                // !!Unexpected type of exception (like NotSupportedException or NotImplementedException), so fail fast!!
                throw;
            }
        }

        private void FireProgressChanged(string currentStepName, int currentStep, int totalSteps)
        {
            if (ProgressChanged == null)
            {
                return;
            }

            ProgressChanged(currentStepName, currentStep, totalSteps);
        }
    }
}