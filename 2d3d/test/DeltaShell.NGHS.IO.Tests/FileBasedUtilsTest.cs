using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests
{
    [Category(TestCategory.DataAccess)]
    [TestFixture]
    public class FileBasedUtilsTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void CollectNonRecursivePaths_ShouldReturnNonRecursivePathsInsideADirectory()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string filePath = Path.Combine(tempDirectory.Path, "file.txt");
                string subFolderPath = Path.Combine(tempDirectory.Path, "subfolder");
                string subFolderFilePath = Path.Combine(subFolderPath, "subfolderfile.txt");

                File.WriteAllText(filePath, "file");
                Directory.CreateDirectory(subFolderPath);
                File.WriteAllText(subFolderFilePath, "subfolderfile");

                // Act
                string[] nonRecursivePaths = FileBasedUtils.CollectNonRecursivePaths(tempDirectory.Path);

                // Assert
                Assert.AreEqual(2, nonRecursivePaths.Length);
                Assert.IsTrue(nonRecursivePaths.Contains(filePath));
                Assert.IsTrue(nonRecursivePaths.Contains(subFolderPath));
            }
        }

        [Test]
        public void TestCleanPersistentDirectories_CompositeModel()
        {
            var compositeModel = new TestCompositeActivity {Name = "CompositeModel"};
            compositeModel.Activities.AddRange(new List<IActivity>
            {
                new TestActivity {Name = "SubModel1"},
                new TestActivity {Name = "SubModel2"},
                new TestActivity {Name = "SubModel3"}
            });

            DoInTemperaryDirectory((string testDir) =>
            {
                // Set-up
                var projectDataDirectoryInfo = new DirectoryInfo(Path.Combine(testDir, "project_data"));
                FileUtils.CreateDirectoryIfNotExists(projectDataDirectoryInfo.FullName);

                string compositeModelDir = Path.Combine(projectDataDirectoryInfo.FullName, compositeModel.Name);
                FileUtils.CreateDirectoryIfNotExists(compositeModelDir);

                CreateStandardDirectoriesForActivity(projectDataDirectoryInfo.FullName, compositeModel);
                RecursivelyAddNoiseToDirectories(projectDataDirectoryInfo);

                // Call
                FileBasedUtils.CleanPersistentDirectories(projectDataDirectoryInfo, compositeModel);

                // Assert
                Assert.True(VerifyStandardDirectoriesForActivity(projectDataDirectoryInfo.FullName, compositeModel));
            });
        }

        [Test]
        public void TestTestCleanPersistentDirectories_StandAloneModel()
        {
            var standaloneModel = new TestActivity {Name = "StandaloneModel"};

            DoInTemperaryDirectory((string testDir) =>
            {
                // Set-up
                var projectDataDirectoryInfo = new DirectoryInfo(Path.Combine(testDir, "project_data"));
                FileUtils.CreateDirectoryIfNotExists(projectDataDirectoryInfo.FullName);

                string standaloneModelDir = Path.Combine(projectDataDirectoryInfo.FullName, standaloneModel.Name);
                FileUtils.CreateDirectoryIfNotExists(standaloneModelDir);

                CreateStandardDirectoriesForActivity(projectDataDirectoryInfo.FullName, standaloneModel);
                RecursivelyAddNoiseToDirectories(projectDataDirectoryInfo);

                // Call
                FileBasedUtils.CleanPersistentDirectories(projectDataDirectoryInfo, standaloneModel);

                // Assert
                Assert.True(VerifyStandardDirectoriesForActivity(projectDataDirectoryInfo.FullName, standaloneModel));
            });
        }

        private static void DoInTemperaryDirectory(Action<string> action)
        {
            string testDir = FileUtils.CreateTempDirectory();
            try
            {
                action.Invoke(testDir);
            }
            finally
            {
                // Clean-up
                FileUtils.DeleteIfExists(testDir);
            }
        }

        private static void CreateStandardDirectoriesForActivity(string rootDir, IActivity activity)
        {
            var compositeActivity = activity as ICompositeActivity;
            if (compositeActivity != null)
            {
                string compositeActivityDirectory = Path.Combine(rootDir, compositeActivity.Name);
                FileUtils.CreateDirectoryIfNotExists(compositeActivityDirectory);

                foreach (IActivity subActivity in compositeActivity.Activities)
                {
                    CreateStandardDirectoriesForActivity(compositeActivityDirectory, subActivity);
                }
            }
            else
            {
                string activityDirectory = Path.Combine(rootDir, activity.Name);
                FileUtils.CreateDirectoryIfNotExists(activityDirectory);
                FileUtils.CreateDirectoryIfNotExists(Path.Combine(activityDirectory, "input"));
                FileUtils.CreateDirectoryIfNotExists(Path.Combine(activityDirectory, "output"));
            }
        }

        private static void RecursivelyAddNoiseToDirectories(DirectoryInfo rootDirectoryInfo)
        {
            FileUtils.CreateDirectoryIfNotExists(rootDirectoryInfo.FullName);
            DirectoryInfo[] subDirectories = rootDirectoryInfo.GetDirectories("*", SearchOption.AllDirectories);

            foreach (DirectoryInfo directory in subDirectories)
            {
                if (directory.Name == "input" || directory.Name == "output")
                {
                    continue;
                }

                Directory.CreateDirectory(Path.Combine(directory.FullName, "blarg"));
                using (File.Create(Path.Combine(directory.FullName, "blarg.txt"))) {} // dispose of FileStream
            }
        }

        private static bool VerifyStandardDirectoriesForActivity(string rootDir, IActivity activity)
        {
            var compositeActivity = activity as ICompositeActivity;
            if (compositeActivity != null)
            {
                var compositeActivityDirectoryInfo = new DirectoryInfo(Path.Combine(rootDir, compositeActivity.Name));
                if (!compositeActivityDirectoryInfo.Exists)
                {
                    return false;
                }

                List<string> subDirectoryNames = compositeActivityDirectoryInfo.GetDirectories().Select(di => di.Name).ToList();
                if (subDirectoryNames.Count != compositeActivity.Activities.Count)
                {
                    return false;
                }

                foreach (IActivity subActivity in compositeActivity.Activities)
                {
                    if (!subDirectoryNames.Contains(subActivity.Name))
                    {
                        return false;
                    }

                    if (!VerifyStandardDirectoriesForActivity(compositeActivityDirectoryInfo.FullName, subActivity))
                    {
                        return false;
                    }
                }
            }
            else
            {
                var modelDirectoryInfo = new DirectoryInfo(Path.Combine(rootDir, activity.Name));
                if (!modelDirectoryInfo.Exists)
                {
                    return false;
                }

                List<string> subDirectoryNames = modelDirectoryInfo.GetDirectories().Select(di => di.Name).ToList();
                if (subDirectoryNames.Count != 2)
                {
                    return false;
                }

                if (!(subDirectoryNames.Contains("input") && subDirectoryNames.Contains("output")))
                {
                    return false;
                }

                FileInfo[] filesInDirectory = modelDirectoryInfo.GetFiles();
                if (filesInDirectory.Length > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private class TestCompositeActivity : CompositeActivity, IHydroModel
        {
            #region CompositeActivity implementation

            protected override void OnInitialize() {}

            protected override void OnExecute() {}

            #endregion

            #region IHydroModel implementation

            public bool CanRename(IDataItem item)
            {
                return false;
            }

            public bool CanRemove(IDataItem item)
            {
                return false;
            }

            public bool CanCopy(IDataItem item)
            {
                return false;
            }

            public bool IsDataItemActive(IDataItem dataItem)
            {
                return false;
            }

            public bool IsDataItemValid(IDataItem dataItem)
            {
                return false;
            }

            public bool IsLinkAllowed(IDataItem source, IDataItem target)
            {
                return false;
            }

            public IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
            {
                return Enumerable.Empty<IFeature>();
            }

            public IEnumerable<IDataItem> GetChildDataItems(IFeature location)
            {
                return Enumerable.Empty<IDataItem>();
            }

            public void UpdateLink(object data) {}

            public IDataItem GetDataItemByValue(object value)
            {
                return null;
            }

            public void ClearOutput(bool forceClean = false) {}
            
            public void MarkOutputOutOfSync() {}

            public IEventedList<IDataItem> DataItems { get; set; }
            public IEnumerable<IDataItem> AllDataItems { get; }
            public string KernelVersions { get; }
            public object Owner { get; set; }
            public bool IsCopyable { get; }
            public bool OutputOutOfSync { get; set; }
            public bool SuspendClearOutputOnInputChange { get; set; }
            public bool SuspendMarkOutputOutOfSyncOnInputChange { get; set; }
            public bool CanRun { get; }
            public IHydroRegion Region { get; }

            #endregion
        }

        private class TestActivity : Activity, IHydroModel
        {
            #region Activity implementation

            protected override void OnInitialize() {}

            protected override void OnExecute() {}

            protected override void OnCancel() {}

            protected override void OnCleanUp() {}

            protected override void OnFinish() {}

            #endregion

            #region IHydroModel implementation

            public bool CanRename(IDataItem item)
            {
                return false;
            }

            public bool CanRemove(IDataItem item)
            {
                return false;
            }

            public bool CanCopy(IDataItem item)
            {
                return false;
            }

            public bool ReadOnly { get; set; }

            public bool IsDataItemActive(IDataItem dataItem)
            {
                return false;
            }

            public bool IsDataItemValid(IDataItem dataItem)
            {
                return false;
            }

            public bool IsLinkAllowed(IDataItem source, IDataItem target)
            {
                return false;
            }

            public IEnumerable<IFeature> GetChildDataItemLocations(DataItemRole role)
            {
                return Enumerable.Empty<IFeature>();
            }

            public IEnumerable<IDataItem> GetChildDataItems(IFeature location)
            {
                return Enumerable.Empty<IDataItem>();
            }

            public void UpdateLink(object data) {}

            public IDataItem GetDataItemByValue(object value)
            {
                return null;
            }

            public void ClearOutput(bool forceClean = false) {}
            
            public void MarkOutputOutOfSync() {}

            public IEventedList<IDataItem> DataItems { get; set; }
            public IEnumerable<IDataItem> AllDataItems { get; }
            public string KernelVersions { get; }
            public object Owner { get; set; }
            public bool IsCopyable { get; }
            public bool OutputOutOfSync { get; set; }
            public bool SuspendClearOutputOnInputChange { get; set; }
            public bool SuspendMarkOutputOutOfSyncOnInputChange { get; set; }
            public bool CanRun { get; }
            public IHydroRegion Region { get; }

            #endregion
        }
    }
}