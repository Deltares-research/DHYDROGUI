using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using Deltares.Infrastructure.Extensions;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers
{
    public class WaterFlowFMFileImporter : ModelFileImporterBase, IDimrModelFileImporter
    {
        private readonly ILog log = LogManager.GetLogger(typeof(WaterFlowFMFileImporter));

        private readonly Func<string> storeWorkingDirectoryPathFunc;

        /// <summary>
        /// Constructor needed for connecting the Application.WorkingDirectory to the WaterFlowFMModel Working Directory.
        /// </summary>
        /// <param name="getWorkingDirectoryPathFunc"> </param>
        public WaterFlowFMFileImporter(Func<string> getWorkingDirectoryPathFunc)
        {
            storeWorkingDirectoryPathFunc = getWorkingDirectoryPathFunc;
        }

        public override string Name => "Flow Flexible Mesh Model";

        public override string Category => Resources.FMImporters_Category_D_Flow_FM_2D_3D;

        public override string Description => string.Empty;

        public override Bitmap Image => Resources.unstrucModel;

        public override IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(IHydroModel);
            }
        }

        public override bool OpenViewAfterImport => true;

        public bool CanImportDimrFile(string path) => Path.GetExtension(path).EqualsCaseInsensitive(".mdu");

        public override bool CanImportOnRootLevel => true;

        public override string FileFilter => $"Flexible Mesh Model Definition|*{FileConstants.MduFileExtension}";

        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        public override bool CanImportOn(object targetObject)
        {
            return targetObject is ICompositeActivity || targetObject is WaterFlowFMModel;
        }

        protected override object OnImportItem(string path, object target = null)
        {
            try
            {
                var importedFmModel = new WaterFlowFMModel
                {
                    WorkingDirectoryPathFunc = storeWorkingDirectoryPathFunc
                };

                importedFmModel.ImportFromMdu(path, true, ProgressChanged);

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
                log.ErrorFormat(Resources.WaterFlowFMFileImporter_ImportItem_Error_while_importing_a__0__from__1__2__, Name, path, e);
                return null;
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