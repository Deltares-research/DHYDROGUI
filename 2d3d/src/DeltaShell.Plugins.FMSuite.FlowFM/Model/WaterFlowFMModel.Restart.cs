using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Restart;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Restart;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    // The Restart related code for WaterFlowFM..
    public partial class WaterFlowFMModel
    {
        private WaterFlowFMRestartFile restartInput = new WaterFlowFMRestartFile();
        private readonly List<WaterFlowFMRestartFile> listOfOutputRestartFiles = new List<WaterFlowFMRestartFile>();

        /// <summary>
        /// Gets or sets the restart time step.
        /// </summary>
        public TimeSpan RestartTimeStep
        {
            get => (TimeSpan) ModelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value;
            set => ModelDefinition.GetModelProperty(GuiProperties.RstOutputDeltaT).Value = value;
        }

        /// <summary>
        /// Gets the restart start time.
        /// </summary>
        public DateTime RestartStartTime => (DateTime) ModelDefinition.GetModelProperty(GuiProperties.RstOutputStartTime).Value;

        /// <summary>
        /// Gets the restart stop time.
        /// </summary>
        public DateTime RestartStopTime => (DateTime) ModelDefinition.GetModelProperty(GuiProperties.RstOutputStopTime).Value;

        #region IRestartModel
        /// <inheritdoc cref="IRestartModel{TRestartFile}.RestartInput"/>
        public WaterFlowFMRestartFile RestartInput
        {
            get => restartInput;
            set
            {
                Ensure.NotNull(value, nameof(value));
                restartInput = value;
            }
        }

        /// <inheritdoc cref="IRestartModel{TRestartFile}.RestartOutput"/>
        public IEnumerable<WaterFlowFMRestartFile> RestartOutput => listOfOutputRestartFiles;
        
        /// <inheritdoc cref="IRestartModel{TRestartFile}.UseRestart"/>
        public bool UseRestart => !RestartInput.IsEmpty;

        /// <inheritdoc cref="IRestartModel{TRestartFile}.WriteRestart"/>
        public virtual bool WriteRestart
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value;
            set => ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value = value;
        }

        /// <inheritdoc cref="IRestartModel{TRestartFile}.SetRestartInputToDuplicateOf"/>
        public virtual void SetRestartInputToDuplicateOf(WaterFlowFMRestartFile source)
        {
            Ensure.NotNull(source,nameof(source));
            RestartInput = new WaterFlowFMRestartFile(source);
        }

        private void ReconnectRestartFiles(IEnumerable<string> restartFilePaths)
        {
            listOfOutputRestartFiles.Clear();
            listOfOutputRestartFiles.AddRange( restartFilePaths.Select(p => new WaterFlowFMRestartFile(p)) );
        }
        #endregion
    }
}