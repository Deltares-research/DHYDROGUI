using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class ModelStateTest
    {
        private string statesTestFolderName;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            string testFolderName = TestHelper.GetDataDir();
            string testFolderOriginal = Path.Combine(testFolderName, "fbModelState");
            statesTestFolderName = Path.Combine(testFolderName, "fbModelState.test");
            FileUtils.DeleteIfExists(statesTestFolderName);
            FileUtils.CopyDirectory(testFolderOriginal, statesTestFolderName, ".svn");
            Assert.IsTrue(Directory.Exists(statesTestFolderName));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestSaveAndRestoreFileBasedModelState()
        {
            // prepare directories
            string dirForFirstModelRun = Path.Combine(statesTestFolderName, "toBeSaved");
            string dirForSecondModelRun = Path.Combine(statesTestFolderName, "restored");
            Directory.CreateDirectory(dirForSecondModelRun);
            foreach (string directory in Directory.GetDirectories(  dirForFirstModelRun))
            {
                string restoreSubDir = Path.Combine(dirForSecondModelRun, Path.GetFileName(directory));
                Directory.CreateDirectory(restoreSubDir);
            }

            // composite modelRun that produces the state
            DummyModelOrComposedModel firstComposedModel = new DummyModelOrComposedModel(dirForFirstModelRun);
            IModelState savedState = firstComposedModel.GetCopyOfCurrentState();
            string savedModelRunStateFile = Path.Combine(statesTestFolderName, "modelRunModelState.zip");
            firstComposedModel.SaveStateToFile(savedState, savedModelRunStateFile);

            string dirWithSavedStates = Path.Combine(dirForFirstModelRun, DummyModelOrComposedModel.ZippedModelStatesDirName);
            Assert.IsTrue(File.Exists(Path.Combine(dirWithSavedStates, "model1_state.zip")), "zip1 exists");
            Assert.IsTrue(File.Exists(Path.Combine(dirWithSavedStates, "model2_state.zip")), "zip2 exists");
            Assert.IsTrue(File.Exists(Path.Combine(dirWithSavedStates, "model3_state.zip")), "zip3 exists");

            firstComposedModel.ReleaseState(savedState);

            Assert.IsFalse(File.Exists(Path.Combine(dirWithSavedStates, "model1_state.zip")), "zip1 removed");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithSavedStates, "model2_state.zip")), "zip2 removed");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithSavedStates, "model3_state.zip")), "zip3 removed");

            // composite model run that uses the state for restart
            IStateAwareModelEngine secondComposedModel = new DummyModelOrComposedModel(dirForSecondModelRun);
            IModelState secondState = secondComposedModel.CreateStateFromFile(savedModelRunStateFile);

            string dirWithRestoredStates = Path.Combine(dirForSecondModelRun, DummyModelOrComposedModel.ZippedModelStatesDirName);
            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model1_state.zip")), "restored zip1 not yet there");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model2_state.zip")), "restored zip2 not yet there");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model3_state.zip")), "restored zip3 not yet there");

            secondComposedModel.SetState(secondState);

            Assert.IsTrue(File.Exists(Path.Combine(dirWithRestoredStates, "model1_state.zip")), "zip1  restored");
            Assert.IsTrue(File.Exists(Path.Combine(dirWithRestoredStates, "model2_state.zip")), "zip2  restored");
            Assert.IsTrue(File.Exists(Path.Combine(dirWithRestoredStates, "model3_state.zip")), "zip3  restored");

            secondComposedModel.ReleaseState(secondState);

            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model1_state.zip")), "zip1 removed");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model2_state.zip")), "zip2 removed");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model3_state.zip")), "zip3 removed");

            String[] modelStateFiles = {
			                             	"model1/modelStatePartA.txt",
			                             	"model1/modelStatePartB.txt",
			                             	"model2/modelState.txt",
			                             	"model3/modelStatePart-I.txt",
			                             	"model3/modelStatePart-II.txt",
			                             	"model3/modelStatePart-III.txt"
			                             };
            foreach (String modelStateFile in modelStateFiles)
            {
                string orgFile = Path.Combine(dirForFirstModelRun, modelStateFile);
                string restoredFile = Path.Combine(dirForSecondModelRun, modelStateFile);
                Assert.IsTrue(File.Exists(restoredFile), String.Format("File {0} does not exist", restoredFile));
                Assert.IsTrue(FileUtils.FilesAreEqual(orgFile,restoredFile));
            }
        }

        #region Nested type: DummyModel

        private class DummyModelOrComposedModel : Activity, IStateAwareModelEngine
        {
            public const string ZippedModelStatesDirName = "zippedModelStates";
            private const string ZippedModelStateFileExtension = "_state.zip";
            private readonly string dirForPersistentSubModelStates;
            private readonly string workingDir;
            private readonly DummyModelOrComposedModel[] subModels;

            public DummyModelOrComposedModel(string workingDir)
            {
                this.workingDir = workingDir;
                dirForPersistentSubModelStates = Path.GetFullPath(Path.Combine(workingDir, ZippedModelStatesDirName));

                List<string> subModelDirs = ListSubModelDirs(workingDir);
                subModels = new DummyModelOrComposedModel[subModelDirs.Count];
                for (int i = 0; i < subModelDirs.Count; i++)
                {
                    subModels[i] = new DummyModelOrComposedModel(subModelDirs[i]);
                }
                if (subModelDirs.Count > 0)
                {
                    // 'composite model', create dir for zip files with states of sub models
                    Directory.CreateDirectory(dirForPersistentSubModelStates);
                }
            }

            public IModelState GetCopyOfCurrentState()
			{
				// Create model instance state files, and add them state files to model state
				ModelStateFilesImpl modelState = new ModelStateFilesImpl("test-state");

                List<string> subModelDirs = ListSubModelDirs(workingDir);
                if (subModelDirs.Count == 0)
                {
                    // sub model, get sub-model's state file(s)
                    modelState.DirContainingModelStateFiles = workingDir;
                    foreach (string file in Directory.GetFiles(workingDir))
                    {
                        modelState.AddFile(file);
                    }
                }
                else
                {
                    // composed main model, get sub-model's states
                    modelState.DirContainingModelStateFiles = dirForPersistentSubModelStates;
                    for (int i = 0; i < subModelDirs.Count; i++)
                    {
                        IModelState subModelState = subModels[i].GetCopyOfCurrentState();
                        string subModelStateFile = Path.Combine(dirForPersistentSubModelStates,
                                                               Path.GetFileName(subModelDirs[i]) + ZippedModelStateFileExtension);
                        subModels[i].SaveStateToFile(subModelState, subModelStateFile);
                        string fileToBeAddedToState;
                        if (i == 1)
                        {
                            // test absolute path
                            fileToBeAddedToState = Path.GetFullPath(subModelStateFile);
                        }
                        else
                        {
                            // test relative path
                            fileToBeAddedToState = subModelStateFile;
                        }
                        modelState.AddFile(fileToBeAddedToState);
                    }
                }
                return modelState;
            }

            public void SetState(IModelState savedInternalState)
            {
                // first unzip the composed model's state file
                if (!(savedInternalState is ModelStateFilesImpl))
                {
                    throw new ArgumentException("Unknown state type (" + savedInternalState.GetType() +
                                                       " for " + GetType() + ".releaseInternalState");
                }
                ModelStateFilesImpl modelState = (ModelStateFilesImpl)savedInternalState;

                List<string> subModelDirs = ListSubModelDirs(workingDir);
                if (subModelDirs.Count == 0)
                {
                    // indivual model, extract the state file(s)
                    modelState.DirContainingModelStateFiles = workingDir;
                    modelState.RestoreState();
                }
                else
                {
                    if (!Directory.Exists(dirForPersistentSubModelStates))
                    {
                        Directory.CreateDirectory(dirForPersistentSubModelStates);
                    }
                    modelState.DirContainingModelStateFiles = dirForPersistentSubModelStates;
                    modelState.RestoreState();

                    // for each model instance, set its state
                    for (int index = 0; index < subModelDirs.Count; index++)
                    {
                        string subModelDir = subModelDirs[index];
                        string modelStateFile = Path.Combine(dirForPersistentSubModelStates,
                                                             Path.GetFileName(subModelDir) + "_state.zip");
                        if (File.Exists(modelStateFile))
                        {
                            IModelState stateFromFile = subModels[index].CreateStateFromFile(modelStateFile);
                            subModels[index].SetState(stateFromFile);
                        }
                        else
                        {
                            throw new Exception("State File {0} does noet exist");
                        }
                    }
                }
            }

            public void ReleaseState(IModelState savedInternalState)
            {
                ModelStateFilesImpl modelStateFilesImpl = savedInternalState as ModelStateFilesImpl;
                if (modelStateFilesImpl == null)
                {
                    throw new ArgumentException("Unknown state type (" + savedInternalState.GetType() +
                                                       " for " + GetType() + ".releaseInternalState");
                }

                // now for each model instance, unzip its state
                List<string> subModelDirs = ListSubModelDirs(workingDir);
                if (subModelDirs.Count == 0)
                {
                    // sub model, no action
                }
                else
                {
                    // sub model, get sub-model's state file(s)
                    for (int i = 0; i < modelStateFilesImpl.GetFilesInModelState().Count; i++)
                    {
                        string fileInModelState = modelStateFilesImpl.GetFilesInModelState()[i];
                        IModelState stateFromFile = subModels[i].CreateStateFromFile(fileInModelState);
                        subModels[i].ReleaseState(stateFromFile);
                        File.Delete(fileInModelState);
                    }
                }
            }

            public IModelState CreateStateFromFile(string modelStateFile)
            {
                ModelStateFilesImpl modelState = new ModelStateFilesImpl("test-state");
                modelState.CreateStateFromFile(modelStateFile, workingDir);
                return modelState;
            }

            public IEnumerable<DateTime> GetRestartWriteTimes()
            {
                yield break;
            }

            public void SaveStateToFile(IModelState modelState, string persistentStateFilePath)
            {
                if (!(modelState is ModelStateFilesImpl))
                {
                    throw new ArgumentException("Unknown state type (" + modelState.GetType() +
                                                       " for " + GetType() + ".releaseInternalState");
                }
                ((ModelStateFilesImpl)modelState).SaveStateToFile(persistentStateFilePath);
            }

            public virtual void ValidateInputState(out IEnumerable<string> errors, out IEnumerable<string> warnings)
            {
                throw new NotImplementedException();
            }

            public string WorkDirectory { get; private set; }

            private static List<string> ListSubModelDirs(string workingDir)
            {
                var files = new List<string>();
                foreach (string file in Directory.GetDirectories(workingDir))
                {
                    if (Path.GetFileName(file).StartsWith("model"))
                    {
                        files.Add(file);
                    }
                }
                return files;
            }

            public IEventedList<IDataItem> DataItems
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IEnumerable<IDataItem> AllDataItems
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool IsDataItemActive(IDataItem dataItem)
            {
                throw new NotImplementedException();
            }

            public bool IsDataItemValid(IDataItem dataItem)
            {
                throw new NotImplementedException();
            }

            public bool IsLinkAllowed(IDataItem source, IDataItem target)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IDataItem> GetChildDataItems(IFeature location)
            {
                throw new NotImplementedException();
            }

            protected override void OnFinish()
            {
                throw new NotImplementedException();
            }

            public IEnumerable<object> GetDirectChildren()
            {
                throw new NotImplementedException();
            }

            public object DeepClone()
            {
                throw new NotImplementedException();
            }

            public bool ReadOnly
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public bool CanRename(IDataItem item)
            {
                throw new NotImplementedException();
            }

            public bool CanRemove(IDataItem item)
            {
                throw new NotImplementedException();
            }

            public bool CanCopy(IDataItem item)
            {
                throw new NotImplementedException();
            }

            public object Owner
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public bool IsFileBased
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsCopyable
            {
                get { throw new NotImplementedException(); }
            }

            public bool OutputOutOfSync
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public string ExplicitWorkingDirectory
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public void LoadDataFromDirectory(string path)
            {
                throw new NotImplementedException();
            }

            public void SaveDataToDirectory(string path)
            {
                throw new NotImplementedException();
            }

            public void UpdateLink(object data)
            {
                throw new NotImplementedException();
            }

            public IDataItem GetDataItemByValue(object value)
            {
                throw new NotImplementedException();
            }

            public bool SuspendClearOutputOnInputChange { get; set; }
            
            public void ClearOutput()
            {
                throw new NotImplementedException();
            }
            
            public string GetProgressText()
            {
                throw new NotImplementedException();
            }

            public event EventHandler ProgressChanged;

            public DateTime StartTime
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public DateTime StopTime
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public TimeSpan TimeStep
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public DateTime CurrentTime
            {
                get { throw new NotImplementedException(); }
            }

            public bool UseRestart { get; set; }
            public bool WriteRestart { get; set; }

            protected override void OnInitialize()
            {
                throw new NotImplementedException();
            }

            protected override void OnExecute()
            {
                throw new NotImplementedException();
            }

            protected override void OnCancel()
            {
                throw new NotImplementedException();
            }

            protected override void OnCleanUp()
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
