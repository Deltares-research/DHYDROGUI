using System.IO;
using BasicModelInterface;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        #region Overrides of TimeDependentModelBase

        private double previousProgress = 0;
        private string progressText;

        public override string ProgressText => string.IsNullOrEmpty(progressText) ? base.ProgressText : progressText;
        public override IBasicModelInterface BMIEngine => runner.Api;

        protected override void OnInitialize()
        {
            previousProgress = 0;
            DataItems.RemoveAllWhere(di => di.Tag == DiaFileDataItemTag);

            ReportProgressText("Initializing");

            // Force fm kernel to write output to 'output' Directory
            SetOutputDirAndWaqDirProperty();

            if (Directory.Exists(WorkingOutputDirectoryPath))
            {
                DisconnectOutput();
                FileUtils.DeleteIfExists(WorkingOutputDirectoryPath);
                FileUtils.CreateDirectoryIfNotExists(WorkingOutputDirectoryPath);
            }

            runner.OnInitialize();

            ReportProgressText();
        }

        protected override void OnCleanup()
        {
            snapApiInErrorMode = false;
            base.OnCleanup();
            runner.OnCleanup();

            ReportProgressText();
        }

        protected override void OnExecute()
        {
            runner.OnExecute();
        }

        protected override void OnFinish()
        {
            runner.OnFinish();
            currentOutputDirectoryPath = WorkingOutputDirectoryPath;
        }

        protected override void OnProgressChanged()
        {
            // Only update gui for every 1 percent progress (performance)
            if (ProgressPercentage - previousProgress < 0.01)
            {
                return;
            }

            previousProgress = ProgressPercentage;
            runner.OnProgressChanged();
            base.OnProgressChanged();
        }

        private void ReportProgressText(string text = null)
        {
            progressText = text;
            base.OnProgressChanged();
        }

        #endregion

        #region Implementation of IDimrModel

        public virtual bool CanRunParallel => true;

        public new virtual ActivityStatus Status
        {
            get => base.Status;
            set => base.Status = value;
        }

        [EditAction]
        public virtual bool RunsInIntegratedModel { get; set; }

        #endregion

        #region Output

        private void SetOutputDirAndWaqDirProperty()
        {
            WaterFlowFMProperty outputDirProperty = ModelDefinition.GetModelProperty(KnownProperties.OutputDir);

            string existingOutputDir = outputDirProperty.GetValueAsString();
            if (!existingOutputDir.StartsWith(FileConstants.OutputDirectoryName))
            {
                outputDirProperty.SetValueAsString(FileConstants.OutputDirectoryName);
                Log.InfoFormat("Running this model requires the OutputDirectory to be overwritten to: {0}",
                               FileConstants.OutputDirectoryName);
            }

            if (!SpecifyWaqOutputInterval)
            {
                return;
            }

            string relativeDWaqOutputDirectory = Path.Combine(FileConstants.OutputDirectoryName, DelwaqOutputDirectoryName);
            WaterFlowFMProperty waqOutputDirProperty = ModelDefinition.GetModelProperty(KnownProperties.WaqOutputDir);
            waqOutputDirProperty.SetValueAsString(relativeDWaqOutputDirectory);
        }

        private void ClearOutputDirAndWaqDirProperty()
        {
            ModelDefinition.GetModelProperty(KnownProperties.OutputDir).SetValueAsString(string.Empty);
            ModelDefinition.GetModelProperty(KnownProperties.WaqOutputDir).SetValueAsString(string.Empty);
        }

        #endregion Output
    }
}