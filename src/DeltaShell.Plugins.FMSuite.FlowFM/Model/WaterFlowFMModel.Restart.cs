using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    // The Restart related code for WaterFlowFM..
    public partial class WaterFlowFMModel
    {
        private const string RestartInfoPath = "restart.meta";

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

        public override bool WriteRestart
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
            if (dataItem.Tag == RestartInputStateTag)
            {
                return UseRestart;
            }

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

        private void SaveRestartInfo(string mduPath)
        {
            var restartInfo = new SerializableStatesInfo
            {
                InState = CreateStateInfo(RestartInput),
                OutStates = GetRestartOutputStates().Where(r => !r.IsEmpty).Select(CreateStateInfo).ToArray()
            };

            restartInfo.Save(GetFilePathFromMduPath(mduPath, RestartInfoPath));
        }

        private StateInfo CreateStateInfo(FileBasedRestartState fileBasedRestartState)
        {
            if (fileBasedRestartState.IsEmpty)
            {
                return null;
            }

            return new StateInfo(fileBasedRestartState.Name, fileBasedRestartState.Path);
        }

        private void LoadRestartInfo(string mduPath)
        {
            string infoPath = GetFilePathFromMduPath(mduPath, RestartInfoPath);

            if (File.Exists(infoPath))
            {
                SerializableStatesInfo restartInfo = SerializableStatesInfo.Load(infoPath);

                FileBasedRestartState state = GetFileBasedStateFromStateInfo(restartInfo.InState);
                if (state != null)
                {
                    RestartInput = state;
                }

                // remove any existing ones first:
                IDataItem[] toRemove = dataItems.Where(di => di.Tag == RestartOutputStateTag).ToArray();
                toRemove.ForEach(di => dataItems.Remove(di));

                if (restartInfo.OutStates != null)
                {
                    foreach (StateInfo outStatePath in restartInfo.OutStates)
                    {
                        FileBasedRestartState outState = GetFileBasedStateFromStateInfo(outStatePath);
                        if (outState != null)
                        {
                            AddRestartOutputDataItem(outState);
                        }
                    }
                }
            }
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

        private static FileBasedRestartState GetFileBasedStateFromStateInfo(StateInfo stateInfo)
        {
            if (stateInfo == null)
            {
                return null;
            }

            string zipFilePath = stateInfo.ZipPath;
            if (!string.IsNullOrEmpty(zipFilePath) && File.Exists(zipFilePath))
            {
                var fileBasedState = new FileBasedRestartState(stateInfo.Name, zipFilePath);
                ((IFileBased) fileBasedState).Path = zipFilePath;
                return fileBasedState;
            }

            return null;
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

    // serializable
    public class SerializableStatesInfo
    {
        public StateInfo InState { get; set; }
        public StateInfo[] OutStates { get; set; }

        public void Save(string file)
        {
            using (var writer = new StreamWriter(file))
            {
                var serializer = new XmlSerializer(GetType());
                serializer.Serialize(writer, this);
            }
        }

        public static SerializableStatesInfo Load(string file)
        {
            try
            {
                using (var reader = new StreamReader(file))
                {
                    var serializer = new XmlSerializer(typeof(SerializableStatesInfo));
                    return (SerializableStatesInfo) serializer.Deserialize(reader);
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
                return new SerializableStatesInfo();
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
                return new SerializableStatesInfo();
            }
        }
    }

    // serializable
    public class StateInfo
    {
        public string Name { get; set; }

        public string ZipPath { get; set; }

        public StateInfo(string name, string zipPath)
        {
            Name = name;
            ZipPath = zipPath;
        }

        // for deserializer
        protected StateInfo() {}
    }
}