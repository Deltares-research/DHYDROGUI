using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using log4net;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class WaterFlowModel1DFileImporter : IDimrModelFileImporter
    {
        private readonly ILog log = LogManager.GetLogger(typeof(WaterFlowModel1DFileImporter));

        [ExcludeFromCodeCoverage]
        public string Name => "Water Flow Model 1D (*.md1d)";

        [ExcludeFromCodeCoverage]
        public string Category => "Water Flow Model 1D";

        [ExcludeFromCodeCoverage]
        public Bitmap Image { get; private set; }
        
        public IEnumerable<Type> SupportedItemTypes { get { yield return typeof(IHydroModel); } }

        public bool CanImportOn(object targetObject)
        {
            return targetObject is ICompositeActivity || targetObject is WaterFlowModel1D;
        }

        public bool CanImportOnRootLevel => true;

        public string FileFilter => "md1d|*.md1d";

        [ExcludeFromCodeCoverage]
        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }
        
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport => true;

        public object ImportItem(string path, object target = null)
        {
            try
            {
                void ReportProgress(string currentStepName, int currentStep, int totalSteps) => ProgressChanged(currentStepName, currentStep, totalSteps);
                var imported1DModel = WaterFlowModel1DFileReader.Read(path, ReportProgress);

                var target1DModel = target as WaterFlowModel1D;
                if (target1DModel != null)
                {
                    target = target1DModel.Owner();
                }

                object result = imported1DModel;
                
                if (target is Folder folder)
                {
                    // add / replace the WaterFlowModel1D in the project
                    folder.Items.Remove(target1DModel);
                    folder.Items.Add(imported1DModel);
                }
                else if (target is ICompositeActivity compositeActivity)
                {
                    // add / replace the WaterFlowModel1D in the integrated model
                    imported1DModel.MoveModelIntoIntegratedModel(null, compositeActivity);
                    result = compositeActivity;
                }

                return ShouldCancel ? null : result;

            } catch (Exception e) when (e is ArgumentException    ||
                                        e is PathTooLongException || 
                                        e is FormatException      ||
                                        e is OutOfMemoryException || 
                                        e is IOException          || 
                                        e is InvalidOperationException)
            {
                log.Error($"An error occurred while trying to import a {Name}; Cause: ", e);
                return null;
            }
        }

        public string MasterFileExtension => "md1d";

        public IEnumerable<string> SubFolders
        {
            get { yield return "dflow1d"; }
        }
    }
}