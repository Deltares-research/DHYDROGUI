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
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using log4net;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport
{
    public class WaterFlowModel1DFileImporter : IDimrModelFileImporter
    {
        private readonly ILog log = LogManager.GetLogger(typeof(WaterFlowModel1DFileImporter));

        /// <summary>
        /// Initialize a new instance of the <see cref="WaterFlowModel1DFileImporter"/> class
        /// with the specified model read function.
        /// </summary>
        /// <param name="modelReaderFunc">The model reader function.</param>
        public WaterFlowModel1DFileImporter(Func<string, Action<string, int, int>, WaterFlowModel1D> modelReaderFunc)
        {
            this.modelReaderFunc = modelReaderFunc;
        }

        /// <inheritdoc />
        /// <summary>Initializes a new instance of the <see cref="T:DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.WaterFlowModel1DFileImporter" /> class using
        /// WaterFlowModel1DFileReader to read specified items.
        /// </summary>
        public WaterFlowModel1DFileImporter() : this(WaterFlowModel1DFileReader.Read)
        {

        }

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
        
        [ExcludeFromCodeCoverage]
        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public bool OpenViewAfterImport => true;

        public object ImportItem(string path, object target = null)
        {
            try
            {
                var imported1DModel = ReadModel(path);

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
                                        e is InvalidOperationException ||
                                        e is PropertyNotFoundInFileException)
            {
                log.Error(string.Format(Resources.WaterFlowModel1DFileImporter_ImportItem_An_error_occurred_while_trying_to_import_a__0___, Name), e);
                return null;
            }
        }

        /// <summary>
        /// Read the model with the specified model read function..
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns> The model read from the specified path.</returns>
        private WaterFlowModel1D ReadModel(string path)
        {
            void ReportProgress(string currentStepName, int currentStep, int totalSteps) => ProgressChanged(currentStepName, currentStep, totalSteps);
            return modelReaderFunc.Invoke(path, ReportProgress);
        }

        /// <summary> Function responsible for reading the model. </summary>
        private readonly Func<string, Action<string, int, int>, WaterFlowModel1D> modelReaderFunc;

        public string MasterFileExtension => "md1d";

        public IEnumerable<string> SubFolders
        {
            get { yield return "dflow1d"; }
        }
    }
}