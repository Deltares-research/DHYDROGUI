using System;
using System.IO;
using BasicModelInterface;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public partial class WaterFlowFMModel
    {
        public override string ProgressText => string.IsNullOrEmpty(progressText) ? base.ProgressText : progressText;

        public override IBasicModelInterface BMIEngine => runner.Api;

        [NoNotifyPropertyChange]
        public override DateTime StartTime
        {
            get => (DateTime) ModelDefinition.GetModelProperty(GuiProperties.StartTime).Value;
            set
            {
                ModelDefinition.GetModelProperty(GuiProperties.StartTime).Value = value;
                // This base model setting is made to make the base logic right
                base.StartTime = value;
            }
        }

        [NoNotifyPropertyChange]
        public override DateTime StopTime
        {
            get => (DateTime) ModelDefinition.GetModelProperty(GuiProperties.StopTime).Value;
            set
            {
                ModelDefinition.GetModelProperty(GuiProperties.StopTime).Value = value;
                // This base model setting is made to make the base logic right
                base.StopTime = value;
            }
        }

        public override TimeSpan TimeStep
        {
            get => (TimeSpan) ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value;
            set
            {
                ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value = value;
                // This base model setting is made to make the base logic right
                base.TimeStep = value;
            }
        }

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
    }
}