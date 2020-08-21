using System;
using System.Collections.Generic;
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

        public IEnumerable<RestartFile> RestartOutput { get; private set; } = Enumerable.Empty<RestartFile>();

        public virtual bool UseRestart => !RestartInput.IsEmpty;

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

        public override bool IsDataItemActive(IDataItem dataItem)
        {
            // TODO D3DFMIQ-2075
            //if (dataItem.Tag == RestartInputStateTag)
            //{
            //    return UseRestart;
            //}

            return base.IsDataItemActive(dataItem);
        }

        private void ReconnectRestartFiles(IEnumerable<string> restartFilePaths)
        {
            RestartOutput = restartFilePaths.Select(p => new RestartFile(p)).ToList();
        }
    }
}