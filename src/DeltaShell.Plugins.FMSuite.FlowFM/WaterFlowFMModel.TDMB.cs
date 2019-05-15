using System;
using BasicModelInterface;
using DelftTools.Utils.Aop;
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
    }
}