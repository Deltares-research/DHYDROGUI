using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers
{
    public class WaterFlowFMFileImporter : ModelFileImporterBase, IDimrModelFileImporter
    {
        private readonly Func<string> storeWorkingDirectoryPathFunc;

        /// <summary>
        /// Constructor needed for connecting the Application.WorkingDirectory to the WaterFlowFMModel Working Directory.
        /// </summary>
        /// <param name="getWorkingDirectoryPathFunc"> </param>
        public WaterFlowFMFileImporter(Func<string> getWorkingDirectoryPathFunc)
        {
            storeWorkingDirectoryPathFunc = getWorkingDirectoryPathFunc;
        }

        public override string Name
        {
            get { return "Flow Flexible Mesh Model"; }
        }

        public override string Description
        {
            get { return "Imports a Flow Flexible Mesh Model using the .mdu file"; }
        }
        
        public override string Category
        {
            get { return ProductCategories.OneDTwoDModelImportCategory; }
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

        public bool CanImportDimrFile(string path) => Path.GetExtension(path).EqualsCaseInsensitive(".mdu");

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

        public override bool CanImportOn(object targetObject)
        {
            return targetObject is ICompositeActivity || targetObject is WaterFlowFMModel;
        }

        protected override object OnImportItem(string path, object target = null)
        {
            var importedFmModel = new WaterFlowFMModel(path, ProgressChanged)
            {
                WorkingDirectoryPathFunc = storeWorkingDirectoryPathFunc,
                ImportProgressChanged = null
            };

            switch (target)
            {
                //replace the FM Model
                case WaterFlowFMModel targetFmModel:
                {
                    var parent = targetFmModel.Owner();
                    switch (parent)
                    {
                        //add / replace the FM Model in the project
                        case Folder folder:
                            folder.Items.Remove(targetFmModel);
                            folder.Items.Add(importedFmModel);
                            break;
                        //add / replace the FM Model in the integrated model
                        case ICompositeActivity compositeActivity:
                            importedFmModel.MoveModelIntoIntegratedModel(null, compositeActivity);
                            break;
                    }

                    ProgressChanged?.Invoke(Resources.WaterFlowFMFileImporter_OnImportItem_Import_finished, WaterFlowFMModel.TOTALSTEPS, WaterFlowFMModel.TOTALSTEPS);
                    return ShouldCancel ? null : importedFmModel;
                }
                
                //add / replace the FM Model in the integrated model
                case ICompositeActivity hydroModel:
                    importedFmModel.MoveModelIntoIntegratedModel(null, hydroModel);
                    ProgressChanged?.Invoke(Resources.WaterFlowFMFileImporter_OnImportItem_Import_finished, WaterFlowFMModel.TOTALSTEPS, WaterFlowFMModel.TOTALSTEPS);
                    return hydroModel;
    
                default:
                    ProgressChanged?.Invoke(Resources.WaterFlowFMFileImporter_OnImportItem_Import_finished, WaterFlowFMModel.TOTALSTEPS, WaterFlowFMModel.TOTALSTEPS);
                    return ShouldCancel ? null : importedFmModel;
            }
        }
    }
}