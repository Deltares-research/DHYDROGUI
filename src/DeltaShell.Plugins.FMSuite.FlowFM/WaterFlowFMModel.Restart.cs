using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.Restart;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    // The Restart related code for WaterFlowFM..
    public partial class WaterFlowFMModel
    {
        private const string ModelTypeId = "FMModel";

        private ModelFileBasedStateHandler modelStateHandler;

        private readonly IList<DelftTools.Utils.Tuple<string, string>> outAndInFileNames =
            new List<DelftTools.Utils.Tuple<string, string>>();

        private static readonly int[] SupportedMetaDataVersions = new[]
        {
            1
        };

        private const string RestartInfoPath = "restart.meta";

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
            return Path.Combine(Path.GetDirectoryName(mduPath),
                                Path.GetFileName(filePath));
        }

        private void InitializeRestart(string targetDir)
        {
            ModelStateHandler.ModelWorkingDirectory = Path.GetFullPath(targetDir);

            if (!UseRestart)
            {
                ModelDefinition.GetModelProperty(KnownProperties.RestartFile).SetValueAsString("");
                return;
            }

            // copies file to correct directory and set RestartFile property in mdu
            IModelState unpackedState = ModelStateHandler.CreateStateFromFile(Name, RestartInput.Path);
            string restartFileName = Path.GetFileName(((ModelStateFilesImpl) unpackedState)
                                                      .GetFilesInModelState()
                                                      .FirstOrDefault(f => f.EndsWith("_rst.nc")));
            if (ModelStateHandler.FeedStateToModel(unpackedState))
            {
                ModelDefinition.GetModelProperty(KnownProperties.RestartFile).SetValueAsString(restartFileName);
            }
            else
            {
                throw new InvalidOperationException("Something went wrong with restart preparations");
            }
        }

        public void ImportRestartFile(string restartFilePath)
        {
            string fileName = Path.GetFileName(restartFilePath);
            string[] splitFileName = fileName.Split(new[]
            {
                '_',
                '.'
            }, StringSplitOptions.RemoveEmptyEntries);
            int length = splitFileName.Length;
            if (length < 5 || splitFileName[length - 2] != "rst")
            {
                throw new ArgumentException(
                    string.Format(
                        "Invalid restart file name {0}: your file should be formatted as <name>_yyyyMMdd_HHmmss_rst.nc",
                        fileName));
            }

            if (splitFileName.Last() != "nc")
            {
                throw new ArgumentException(string.Format("Invalid restart file {0}: not a NetCDF file.", fileName));
            }

            string dateTimeString = string.Concat(splitFileName[length - 4], splitFileName[length - 3]);
            DateTime dateTime;
            if (!DateTime.TryParseExact(dateTimeString, "yyyyMMddhhmmss", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out dateTime))
            {
                throw new ArgumentException(
                    string.Format(
                        "Invalid restart file name {0}: your file should be formatted as <name>_yyyyMMdd_HHmmss_rst.nc",
                        fileName));
            }

            ModelStateHandler.ModelWorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(MduFilePath));
            string destFileName = Path.Combine(ModelStateHandler.ModelWorkingDirectory, fileName);
            if (Path.GetFullPath(restartFilePath) != Path.GetFullPath(destFileName))
            {
                File.Copy(restartFilePath, destFileName, true);
            }

            outAndInFileNames[0].First = Path.GetFullPath(destFileName);
            outAndInFileNames[0].Second = fileName;
            RestartInput = CreateRestartState(this, ModelStateHandler.GetState(), dateTime);
            UseRestart = true;
        }

        IModelState IStateAwareModelEngine.GetCopyOfCurrentState()
        {
            ModelFileBasedStateHandler modelFileBasedStateHandler = ModelStateHandler;

            // modify Out filename list to account for CurrentTime (instance is same as used inside ModelStateHandler)
            string restartFileName = string.Format("{0}_{1}_rst.nc", Name,
                                                   CurrentTime.ToString("yyyyMMdd_HHmmss"));
            outAndInFileNames[0].First = Path.Combine(ModelDefinition.OutputDirectoryName, restartFileName);
            outAndInFileNames[0].Second = restartFileName; //out and in is the same

            return modelFileBasedStateHandler.GetState();
        }

        void IStateAwareModelEngine.SetState(IModelState modelState)
        {
            ModelStateHandler.FeedStateToModel(modelState);
        }

        void IStateAwareModelEngine.ReleaseState(IModelState modelState)
        {
            ModelStateHandler.ReleaseState(modelState);
        }

        IModelState IStateAwareModelEngine.CreateStateFromFile(string persistentStateFilePath)
        {
            return ModelStateHandler.CreateStateFromFile(Name, persistentStateFilePath);
        }

        public override bool WriteRestart
        {
            get => (bool) ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value;
            set => ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value = value;
        }

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

        public virtual IEnumerable<DateTime> GetRestartWriteTimes()
        {
            if (UseSaveStateTimeRange)
            {
                if (SaveStateTimeStep.Ticks == 0L)
                {
                    yield break; //interval 0 would cause infinite loop; break
                }

                DateTime time = SaveStateStartTime;
                while (time <= SaveStateStopTime)
                {
                    yield return time;

                    time += SaveStateTimeStep;
                }
            }
        }

        public override bool IsDataItemActive(IDataItem dataItem)
        {
            if (dataItem.Tag == RestartInputStateTag)
            {
                return UseRestart;
            }

            return base.IsDataItemActive(dataItem);
        }

        void IStateAwareModelEngine.SaveStateToFile(IModelState modelState, string persistentStateFilePath)
        {
            modelState.MetaData = new ModelStateMetaData
            {
                ModelTypeId = ModelTypeId,
                Version = SupportedMetaDataVersions.Last(),
                Attributes = GetMetaDataRequirements(SupportedMetaDataVersions.Last())
            };
            ModelStateHandler.SaveStateToFile(modelState, persistentStateFilePath);
        }

        public virtual void ValidateInputState(out IEnumerable<string> errors, out IEnumerable<string> warnings)
        {
            try
            {
                var modelState =
                    (ModelStateFilesImpl) ModelStateHandler.CreateStateFromFile("validate", RestartInput.Path);
                errors = ModelStateValidator.ValidateInputState(modelState, SupportedMetaDataVersions,
                                                                GetMetaDataRequirements, ModelTypeId);
                warnings = Enumerable.Empty<string>();
            }
            catch (ArgumentException e)
            {
                errors = new[]
                {
                    e.Message
                };
                warnings = Enumerable.Empty<string>();
            }
        }

        private Dictionary<string, string> GetMetaDataRequirements(int version)
        {
            if (version == 1)
            {
                return new Dictionary<string, string>
                {
                    {"NrOfVertices", Grid.Vertices.Count.ToString(CultureInfo.InvariantCulture)},
                    {"NrOfEdges", Grid.Edges.Count.ToString(CultureInfo.InvariantCulture)},
                    // todo
                };
            }

            throw new NotImplementedException(string.Format("Meta data version {0} for model type {1} is not supported",
                                                            version, ModelTypeId));
        }

        private ModelFileBasedStateHandler ModelStateHandler
        {
            get
            {
                if (modelStateHandler == null)
                {
                    outAndInFileNames.Add(new DelftTools.Utils.Tuple<string, string>(
                                              "<filled in GetCopyOfCurrentState>",
                                              "<filled in GetCopyOfCurrentState>"));
                    modelStateHandler = new ModelFileBasedStateHandler(Name, outAndInFileNames);
                }

                return modelStateHandler;
            }
        }

        #region Implementation of IDimrStateAwareModel

        public virtual void PrepareRestart()
        {
            string workDirectory = ExplicitWorkingDirectory ?? FileUtils.CreateTempDirectory();
            InitializeRestart(workDirectory);
            ClearStatesIfRequired();
        }

        public virtual void WriteRestartFiles()
        {
            WriteRestartIfRequired(false);
        }

        public virtual void FinalizeRestart()
        {
            WriteRestartIfRequired(true);
        }

        #endregion
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
        public string Name;
        public string ZipPath;

        // for deserializer
        protected StateInfo() {}

        public StateInfo(string name, string zipPath)
        {
            Name = name;
            ZipPath = zipPath;
        }
    }
}