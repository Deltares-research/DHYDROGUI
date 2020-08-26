using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    // The Restart related code for WaterFlowFM..
    public partial class WaterFlowFMModel
    {
        private RestartFile restartInput = new RestartFile();

        /// <summary>
        /// Gets or sets the input restart file.
        /// </summary>
        public RestartFile RestartInput
        {
            get => restartInput;
            set
            {
                Ensure.NotNull(value, nameof(value));

                restartInput = value;
            }
        }

        public IEnumerable<RestartFile> RestartOutput { get; private set; } = Enumerable.Empty<RestartFile>();

        public virtual bool UseRestart => !RestartInput.IsEmpty;

        /// <summary>
        /// Gets or sets the restart time step.
        /// </summary>
        public virtual TimeSpan RestartTimeStep
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