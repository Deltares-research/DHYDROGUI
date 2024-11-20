using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO
{
    public class HydFileImporter : ModelFileImporterBase
    {
        private readonly Func<string> storeWorkingDirectoryPathFunc;

        /// <summary>
        /// Constructor needed for connecting the Application.WorkingDirectory to the
        /// WaterQualityModelSettings Working Directory when a new Water Quality model
        /// needs to be created.
        /// </summary>
        /// <param name="getWorkingDirectoryPathFunc"> </param>
        public HydFileImporter(Func<string> getWorkingDirectoryPathFunc)
        {
            storeWorkingDirectoryPathFunc = getWorkingDirectoryPathFunc;
        }

        /// <summary>
        /// Constructor for when the Water Quality model already exists, but empty hyd file.
        /// </summary>
        public HydFileImporter() {}

        public Action<WaterQualityModel> ExpandModelNode { get; set; }

        public bool MarkModelOutputOutOfSync { get; set; }

        public bool SkipImportTimers { get; set; }

        public override string Name => "Hydrodynamics (*.hyd)";

        public override string Category => "Water Quality";

        public override string Description => string.Empty;

        public override Bitmap Image => Resources.hydFile;

        public override IEnumerable<Type> SupportedItemTypes
        {
            get
            {
                yield return typeof(WaterQualityModel);
            }
        }

        public override bool CanImportOnRootLevel => true;

        public override string FileFilter => "Hydrodynamics File (*.hyd)|*.hyd";

        // automatic setters used by the application plugin
        public override string TargetDataDirectory { get; set; }

        public override bool ShouldCancel { get; set; }

        public override ImportProgressChangedDelegate ProgressChanged { get; set; }

        public override bool OpenViewAfterImport => true;

        public override bool CanImportOn(object targetObject)
        {
            return true;
        }

        /// <summary>
        /// Import data on a water quality model,
        /// or create a new one if the target doesn't exist.
        /// </summary>
        protected override object OnImportItem(string path, object target = null)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Couldn't find file: " + path);
            }

            SetProgress("Reading hydrodynamics file", 0, 0);
            HydFileData data = HydFileReader.ReadAll(new FileInfo(path));

            var model = target as WaterQualityModel;

            if (model == null)
            {
                model = new WaterQualityModel();
                if (storeWorkingDirectoryPathFunc != null)
                {
                    model.SetWorkingDirectoryInModelSettings(storeWorkingDirectoryPathFunc);
                }
            }

            EventHandler progressChangedHandler = (s, e) => SetProgress(model.ProgressText, 0, 0);

            model.ProgressChanged += progressChangedHandler;

            try
            {
                model.ImportHydroData(data, skipImportTimers: SkipImportTimers,
                                      markOutputOutOfSync: MarkModelOutputOutOfSync);

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
            if (ProgressChanged == null)
            {
                return;
            }

            ProgressChanged(currentStepName, currentStep, totalSteps);
        }
    }
}