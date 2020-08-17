using System;
using System.IO;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    // The Restart related code for WaterFlowFM..
    public partial class WaterFlowFMModel
    {
        // TODO D3DFMIQ-2075
        public bool UseRestart { get; set; }

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
        public bool WriteRestart
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

        private void LoadRestartFile(string mduPath)
        {
            if (mduPath == null)
            {
                return;
            }

            string restartFile = ModelDefinition.GetModelProperty(KnownProperties.RestartFile).GetValueAsString();
            string restartPath = GetFilePathFromMduPath(mduPath, restartFile);
            if (File.Exists(restartPath))
            {
                ImportRestartFile(restartPath);
            }
        }

        private static string GetFilePathFromMduPath(string mduPath, string filePath)
        {
            string directoryName = Path.GetDirectoryName(mduPath);
            string normalizedFilePath = filePath.Replace('/', '\\');
            string combinationPath = Path.Combine(directoryName,
                                                  normalizedFilePath);
            return combinationPath;
        }
    }
}