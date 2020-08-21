using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    // The Restart related code for WaterFlowFM..
    public partial class WaterFlowFMModel
    {
        /// <summary>
        /// Gets or sets the input restart file.
        /// </summary>
        public RestartFile RestartInput { get; } = new RestartFile();

        public IEnumerable<RestartFile> RestartOutput { get; private set; } = Enumerable.Empty<RestartFile>();

        public virtual bool UseRestart => !RestartInput.IsEmpty;

        public virtual TimeSpan SaveStateTimeStep
        {
            get => (TimeSpan) ModelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value;
            set => ModelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value = value;
        }

        public virtual bool WriteRestart
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value;
            set => ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value = value;
        }

        private void ReconnectRestartFiles(IEnumerable<string> restartFilePaths)
        {
            RestartOutput = restartFilePaths.Select(p => new RestartFile(p)).ToList();
        }
    }
}