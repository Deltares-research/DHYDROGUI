using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.Common;
using NSubstitute;
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
            var testActivity1 = Substitute.For<IActivity, IHydroModel>();
            testActivity1.Name.Returns("SubModel1");
            var testActivity2 = Substitute.For<IActivity, IHydroModel>();
            testActivity2.Name.Returns("SubModel2");
            var testActivity3 = Substitute.For<IActivity, IHydroModel>();
            testActivity3.Name.Returns("SubModel3");

            var activities = new EventedList<IActivity>
            {
                testActivity1,
                testActivity2,
                testActivity3
            };

            var compositeModel = Substitute.For<ICompositeActivity>();
            compositeModel.Name.Returns("CompositeModel");
            compositeModel.Activities.Returns(activities);

            DoInTemperaryDirectory((string testDir) =>
            {
                // Set-up
                var projectDataDirectoryInfo = new DirectoryInfo(Path.Combine(testDir, "project_data"));
                FileUtils.CreateDirectoryIfNotExists(projectDataDirectoryInfo.FullName);

                string compositeModelDir = Path.Combine(projectDataDirectoryInfo.FullName, compositeModel.Name);
                FileUtils.CreateDirectoryIfNotExists(compositeModelDir);

                CreateStandardDirectoriesForCompositeActivityAndActivities(projectDataDirectoryInfo.FullName, compositeModel);
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
            var standaloneModel = Substitute.For<IHydroModel>();
            standaloneModel.Name.Returns("StandaloneModel");

            DoInTemperaryDirectory((string testDir) =>
            {
                // Set-up
                var projectDataDirectoryInfo = new DirectoryInfo(Path.Combine(testDir, "project_data"));
                FileUtils.CreateDirectoryIfNotExists(projectDataDirectoryInfo.FullName);

                string standaloneModelDir = Path.Combine(projectDataDirectoryInfo.FullName, standaloneModel.Name);
                FileUtils.CreateDirectoryIfNotExists(standaloneModelDir);

                CreateStandardDirectoryForActivity(projectDataDirectoryInfo.FullName, standaloneModel);
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

        private static void CreateStandardDirectoriesForCompositeActivityAndActivities(string rootDir, ICompositeActivity compositeActivity)
        {
            string compositeActivityDirectory = Path.Combine(rootDir, compositeActivity.Name);
            FileUtils.CreateDirectoryIfNotExists(compositeActivityDirectory);

            foreach (IActivity subActivity in compositeActivity.Activities)
            {
                CreateStandardDirectoryForActivity(compositeActivityDirectory, subActivity);
            }
        }
        private static void CreateStandardDirectoryForActivity(string rootDir, IActivity activity)
        {
            string activityDirectory = Path.Combine(rootDir, activity.Name);
            FileUtils.CreateDirectoryIfNotExists(activityDirectory);
            FileUtils.CreateDirectoryIfNotExists(Path.Combine(activityDirectory, "input"));
            FileUtils.CreateDirectoryIfNotExists(Path.Combine(activityDirectory, "output"));
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
    }
}