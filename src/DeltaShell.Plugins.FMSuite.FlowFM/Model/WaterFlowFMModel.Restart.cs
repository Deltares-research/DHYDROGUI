using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    // The Restart related code for WaterFlowFM..
    public partial class WaterFlowFMModel
    {
        private RestartFile restartFile = new RestartFile();

        /// <summary>
        /// Gets or sets the input restart file.
        /// </summary>
        public RestartFile RestartInput
        {
            get => restartFile;
            set
            {
                Ensure.NotNull(value, nameof(value));

                restartFile = value;
            }
        }

        // TODO D3DFMIQ-2165
        public IEnumerable<RestartFile> RestartOutput { get; } = Enumerable.Empty<RestartFile>();

        public virtual bool UseRestart => !RestartInput.IsEmpty;

        public virtual bool UseSaveStateTimeRange
        {
            get => WriteRestart;
// always when writing restart (interval is always choosable)
            set {}
        }

        public virtual DateTime SaveStateStartTime
        {
            get
            {
                if (UserSpecifiedRestartStartTime)
                {
                    return (DateTime) ModelDefinition.GetModelProperty(GuiProperties.RstOutputStartTime).Value;
                }

                return StartTime;
            }
            set
            {
                if (value != StartTime)
                {
                    UserSpecifiedRestartStartTime = true;
                }

                if (UserSpecifiedRestartStartTime)
                {
                    ModelDefinition.GetModelProperty(GuiProperties.RstOutputStartTime).Value = value;
                }
            }
        }

        public virtual DateTime SaveStateStopTime
        {
            get
            {
                if (UserSpecifiedRestartStopTime)
                {
                    return (DateTime) ModelDefinition.GetModelProperty(GuiProperties.RstOutputStopTime).Value;
                }

                return StopTime;
            }
            set
            {
                if (value != StopTime)
                {
                    UserSpecifiedRestartStopTime = true;
                }

                if (UserSpecifiedRestartStopTime)
                {
                    ModelDefinition.GetModelProperty(GuiProperties.RstOutputStopTime).Value = value;
                }
            }
        }

        public virtual TimeSpan SaveStateTimeStep
        {
            get => (TimeSpan) ModelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value;
            set => ModelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value = value;
        }

        // TODO D3DFMIQ-2075
        public virtual bool WriteRestart
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value;
            set => ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value = value;
        }

        public virtual void ImportRestartFile(string restartFilePath)
        {
            // TODO D3DFMIQ-2075
        }

        public override bool IsDataItemActive(IDataItem dataItem)
        {
            // TODO D3DFMIQ-2075
            //if (dataItem.Tag == RestartInputStateTag)
            //{
            //    return UseRestart;
            //}

            return base.IsDataItemActive(dataItem);
        }

        private bool UserSpecifiedRestartStartTime
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStart).Value;
            set => ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStart).Value = value;
        }

        private bool UserSpecifiedRestartStopTime
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStop).Value;
            set => ModelDefinition.GetModelProperty(GuiProperties.SpecifyRstStop).Value = value;
        }

    }
}