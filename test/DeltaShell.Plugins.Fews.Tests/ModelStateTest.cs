using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class ModelStateTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void TestSaveAndRestoreFileBasedModelState()
        {
            var testFolderName = TestHelper.GetTestDataDirectory();
            var testFolderOriginal = Path.Combine(testFolderName, "fbModelState");
            var statesTestFolderName = Path.Combine(testFolderName, "fbModelState.test");
            FileUtils.DeleteIfExists(statesTestFolderName);
            FileUtils.CopyDirectory(testFolderOriginal, statesTestFolderName, ".svn");
            Assert.IsTrue(Directory.Exists(statesTestFolderName));

            // prepare directories
            var dirForFirstModelRun = Path.Combine(statesTestFolderName, "toBeSaved");
            var dirForSecondModelRun = Path.Combine(statesTestFolderName, "restored");
            Directory.CreateDirectory(dirForSecondModelRun);
            foreach (var directory in Directory.GetDirectories(dirForFirstModelRun))
            {
                var restoreSubDir = Path.Combine(dirForSecondModelRun, Path.GetFileName(directory));
                Directory.CreateDirectory(restoreSubDir);
            }

            // composite modelRun that produces the state
            var firstComposedModel = new DummyModelOrComposedModel(dirForFirstModelRun);
            var savedState = firstComposedModel.GetCopyOfCurrentState();
            var savedModelRunStateFile = Path.Combine(statesTestFolderName, "modelRunModelState.zip");
            firstComposedModel.SaveStateToFile(savedState, savedModelRunStateFile);

            var dirWithSavedStates =
                Path.Combine(dirForFirstModelRun, DummyModelOrComposedModel.ZippedModelStatesDirName);
            Assert.IsTrue(File.Exists(Path.Combine(dirWithSavedStates, "model1_state.zip")), "zip1 exists");
            Assert.IsTrue(File.Exists(Path.Combine(dirWithSavedStates, "model2_state.zip")), "zip2 exists");
            Assert.IsTrue(File.Exists(Path.Combine(dirWithSavedStates, "model3_state.zip")), "zip3 exists");

            firstComposedModel.ReleaseState(savedState);

            Assert.IsFalse(File.Exists(Path.Combine(dirWithSavedStates, "model1_state.zip")), "zip1 removed");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithSavedStates, "model2_state.zip")), "zip2 removed");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithSavedStates, "model3_state.zip")), "zip3 removed");

            // composite model run that uses the state for restart
            IStateAwareModelEngine secondComposedModel = new DummyModelOrComposedModel(dirForSecondModelRun);
            var secondState = secondComposedModel.CreateStateFromFile(savedModelRunStateFile);

            var dirWithRestoredStates =
                Path.Combine(dirForSecondModelRun, DummyModelOrComposedModel.ZippedModelStatesDirName);
            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model1_state.zip")),
                "restored zip1 not yet there");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model2_state.zip")),
                "restored zip2 not yet there");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model3_state.zip")),
                "restored zip3 not yet there");

            secondComposedModel.SetState(secondState);

            Assert.IsTrue(File.Exists(Path.Combine(dirWithRestoredStates, "model1_state.zip")), "zip1  restored");
            Assert.IsTrue(File.Exists(Path.Combine(dirWithRestoredStates, "model2_state.zip")), "zip2  restored");
            Assert.IsTrue(File.Exists(Path.Combine(dirWithRestoredStates, "model3_state.zip")), "zip3  restored");

            secondComposedModel.ReleaseState(secondState);

            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model1_state.zip")), "zip1 removed");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model2_state.zip")), "zip2 removed");
            Assert.IsFalse(File.Exists(Path.Combine(dirWithRestoredStates, "model3_state.zip")), "zip3 removed");

            string[] modelStateFiles =
            {
                "model1/modelStatePartA.txt",
                "model1/modelStatePartB.txt",
                "model2/modelState.txt",
                "model3/modelStatePart-I.txt",
                "model3/modelStatePart-II.txt",
                "model3/modelStatePart-III.txt"
            };
            foreach (var modelStateFile in modelStateFiles)
            {
                var orgFile = Path.Combine(dirForFirstModelRun, modelStateFile);
                var restoredFile = Path.Combine(dirForSecondModelRun, modelStateFile);
                Assert.IsTrue(File.Exists(restoredFile), $"File {restoredFile} does not exist");
                Assert.IsTrue(FileUtils.FilesAreEqual(orgFile, restoredFile));
            }
        }

        private class DummyModelOrComposedModel : Activity, IStateAwareModelEngine
        {
            public const string ZippedModelStatesDirName = "zippedModelStates";
            private const string ZippedModelStateFileExtension = "_state.zip";
            private readonly string dirForPersistentSubModelStates;
            private readonly DummyModelOrComposedModel[] subModels;
            private readonly string workingDir;

            public DummyModelOrComposedModel(string workingDir)
            {
                this.workingDir = workingDir;
                dirForPersistentSubModelStates = Path.GetFullPath(Path.Combine(workingDir, ZippedModelStatesDirName));

                var subModelDirs = ListSubModelDirs(workingDir);
                subModels = new DummyModelOrComposedModel[subModelDirs.Count];
                for (var i = 0; i < subModelDirs.Count; i++)
                    subModels[i] = new DummyModelOrComposedModel(subModelDirs[i]);
                if (subModelDirs.Count > 0) Directory.CreateDirectory(dirForPersistentSubModelStates);
            }

            public IModelState GetCopyOfCurrentState()
            {
                // Create model instance state files, and add them state files to model state
                var modelState = new ModelStateFilesImpl("test-state");

                var subModelDirs = ListSubModelDirs(workingDir);
                if (subModelDirs.Count == 0)
                {
                    // sub model, get sub-model's state file(s)
                    modelState.DirContainingModelStateFiles = workingDir;
                    foreach (var file in Directory.GetFiles(workingDir)) modelState.AddFile(file);
                }
                else
                {
                    // composed main model, get sub-model's states
                    modelState.DirContainingModelStateFiles = dirForPersistentSubModelStates;
                    for (var i = 0; i < subModelDirs.Count; i++)
                    {
                        var subModelState = subModels[i].GetCopyOfCurrentState();
                        var subModelStateFile = Path.Combine(dirForPersistentSubModelStates,
                            Path.GetFileName(subModelDirs[i]) + ZippedModelStateFileExtension);
                        subModels[i].SaveStateToFile(subModelState, subModelStateFile);
                        var fileToBeAddedToState = i == 1
                            ? Path.GetFullPath(subModelStateFile)
                            : subModelStateFile;
                        modelState.AddFile(fileToBeAddedToState);
                    }
                }

                return modelState;
            }

            public void SetState(IModelState savedInternalState)
            {
                // first unzip the composed model's state file
                if (!(savedInternalState is ModelStateFilesImpl))
                    throw new ArgumentException("Unknown state type (" + savedInternalState.GetType() +
                                                " for " + GetType() + ".releaseInternalState");
                var modelState = (ModelStateFilesImpl)savedInternalState;

                var subModelDirs = ListSubModelDirs(workingDir);
                if (subModelDirs.Count == 0)
                {
                    // individual model, extract the state file(s)
                    modelState.DirContainingModelStateFiles = workingDir;
                    modelState.RestoreState();
                }
                else
                {
                    if (!Directory.Exists(dirForPersistentSubModelStates))
                        Directory.CreateDirectory(dirForPersistentSubModelStates);
                    modelState.DirContainingModelStateFiles = dirForPersistentSubModelStates;
                    modelState.RestoreState();

                    // for each model instance, set its state
                    for (var index = 0; index < subModelDirs.Count; index++)
                    {
                        var subModelDir = subModelDirs[index];
                        var modelStateFile = Path.Combine(dirForPersistentSubModelStates,
                            Path.GetFileName(subModelDir) + "_state.zip");
                        if (File.Exists(modelStateFile))
                        {
                            var stateFromFile = subModels[index].CreateStateFromFile(modelStateFile);
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
                var modelStateFilesImpl = savedInternalState as ModelStateFilesImpl;
                if (modelStateFilesImpl == null)
                    throw new ArgumentException("Unknown state type (" + savedInternalState.GetType() +
                                                " for " + GetType() + ".releaseInternalState");

                // now for each model instance, unzip its state
                var subModelDirs = ListSubModelDirs(workingDir);
                if (subModelDirs.Count == 0)
                {
                    // sub model, no action
                }
                else
                {
                    // sub model, get sub-model's state file(s)
                    for (var i = 0; i < modelStateFilesImpl.GetFilesInModelState().Count; i++)
                    {
                        var fileInModelState = modelStateFilesImpl.GetFilesInModelState()[i];
                        var stateFromFile = subModels[i].CreateStateFromFile(fileInModelState);
                        subModels[i].ReleaseState(stateFromFile);
                        File.Delete(fileInModelState);
                    }
                }
            }

            public IModelState CreateStateFromFile(string modelStateFile)
            {
                var modelState = new ModelStateFilesImpl("test-state");
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
                    throw new ArgumentException("Unknown state type (" + modelState.GetType() +
                                                " for " + GetType() + ".releaseInternalState");
                ((ModelStateFilesImpl)modelState).SaveStateToFile(persistentStateFilePath);
            }

            private static List<string> ListSubModelDirs(string workingDir)
            {
                var files = new List<string>();
                foreach (var file in Directory.GetDirectories(workingDir))
                    if (Path.GetFileName(file).StartsWith("model"))
                        files.Add(file);
                return files;
            }

            protected override void OnFinish()
            {
                throw new NotImplementedException();
            }

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
    }
}