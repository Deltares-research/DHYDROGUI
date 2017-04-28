using System;
using System.IO;
using System.Linq;

using DelftTools.Shell.Core.Dao;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;

using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate;

using NUnit.Framework;

using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.NHibernate
{
    [TestFixture]
    public class WaterQualityModelDataAccessListenerTest
    {
        private enum PrePersistMethod { OnPreUpdate, OnPreInsert, OnPreDelete }

        private enum PostPersistMethod{ OnPostLoad, OnPostUpdate, OnPostInsert }

        [Test]
        public void TestClone()
        {
            // setup
            var listener = new WaterQualityModelDataAccessListener();

            // call
            var clone = listener.Clone();

            // assert
            Assert.IsInstanceOf<WaterQualityModelDataAccessListener>(clone);
        }

        [Test]
        public void OnPostLoadTestNonWaqModelShouldDoNothing()
        {
            // setup
            var mocks = new MockRepository();
            var objectMock = mocks.StrictMock<object>();
            // No calls should be made on mock object => Nothing should be done with mocked object.
            mocks.ReplayAll();

            var listener = new WaterQualityModelDataAccessListener();

            // call
            listener.OnPostLoad(objectMock, new object[0], new string[0]);

            // assert
            mocks.VerifyAll();
        }

        [Test]
        public void OnPreUpdate_SubstituteOutProjectDataDirectoryInPaths()
        {
            DoPrePersistCallAndAssertProjectDataDirSubstitution(PrePersistMethod.OnPreUpdate);
        }

        [Test]
        public void OnPreInsert_SubstituteOutProjectDataDirectoryInPaths()
        {
            DoPrePersistCallAndAssertProjectDataDirSubstitution(PrePersistMethod.OnPreInsert);
        }

        [Test]
        public void OnPreDelete_SubstituteOutProjectDataDirectoryInPaths()
        {
            DoPrePersistCallAndAssertProjectDataDirSubstitution(PrePersistMethod.OnPreDelete);
        }

        private void DoPrePersistCallAndAssertProjectDataDirSubstitution(PrePersistMethod methodToCall)
        {
            // setup
            var repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            var listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var waqModel = new WaterQualityModel { ModelDataDirectory = Path.Combine(repoPath+"_data", "test", "foo", "bar") };

            var stateArray = new object[]
            {
                "haha",
                waqModel.ModelDataDirectory,
                "hihi",
                double.NaN
            };
            var propertyNamesArray = new[]
            {
                "hihi",
                TypeUtils.GetMemberName<WaterQualityModel>(m => m.ModelDataDirectory),
                "haha",
                "number"
            };

            // call
            var returnResult = CallPrePersistMethod(methodToCall, listener, waqModel, stateArray, propertyNamesArray);

            // assert
            Assert.IsFalse(returnResult, "Should not veto.");
            Assert.AreEqual(@"$data$.\test\foo\bar", waqModel.ModelDataDirectory);
            Assert.AreEqual(@"$data$.\test\foo\bar", stateArray[1]);
        }

        [Test]
        public void OnPreUpdate_DataTableManager_SubstituteOutProjectDataDirectoryInPaths()
        {
            DoPrePersistCallForDataTableManagerAndAssertProjectDataDirSubstitution(PrePersistMethod.OnPreUpdate);
        }

        [Test]
        public void OnPreInsert_DataTableManager_SubstituteOutProjectDataDirectoryInPaths()
        {
            DoPrePersistCallForDataTableManagerAndAssertProjectDataDirSubstitution(PrePersistMethod.OnPreInsert);
        }

        [Test]
        public void OnPreDelete_DataTableManager_SubstituteOutProjectDataDirectoryInPaths()
        {
            DoPrePersistCallForDataTableManagerAndAssertProjectDataDirSubstitution(PrePersistMethod.OnPreDelete);
        }

        private void DoPrePersistCallForDataTableManagerAndAssertProjectDataDirSubstitution(PrePersistMethod methodToCall)
        {
            // setup
            var repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            var listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var manager = new DataTableManager { FolderPath = Path.Combine(repoPath + "_data", "foo", "bar") };

            var stateArray = new object[]
            {
                "haha",
                manager.FolderPath,
                "hihi"
            };
            var propertyNamesArray = new[]
            {
                "hihi",
                TypeUtils.GetMemberName<DataTableManager>(m => m.FolderPath),
                "haha"
            };

            // call
            var returnResult = CallPrePersistMethod(methodToCall, listener, manager, stateArray, propertyNamesArray);

            // assert
            Assert.IsFalse(returnResult, "Should not veto.");
            Assert.AreEqual(@"$data$.\foo\bar", manager.FolderPath);
            Assert.AreEqual(@"$data$.\foo\bar", stateArray[1]);
        }

        private static bool CallPrePersistMethod(PrePersistMethod methodToCall, WaterQualityModelDataAccessListener listener, 
            object entity, object[] stateArray, string[] propertyNamesArray)
        {
            switch (methodToCall)
            {
                case PrePersistMethod.OnPreUpdate:
                    return listener.OnPreUpdate(entity, stateArray, propertyNamesArray);
                case PrePersistMethod.OnPreInsert:
                    return listener.OnPreInsert(entity, stateArray, propertyNamesArray);
                case PrePersistMethod.OnPreDelete:
                    return listener.OnPreDelete(entity, stateArray, propertyNamesArray);
                default:
                    throw new NotImplementedException();
            }
        }

        [Test]
        public void OnPreUpdate_WaterQualityModel1DSettings_SubstituteOutProjectDataDirectoryInPaths()
        {
            DoPrePersistCallForWaterQualityModel1DSettingsAndAssertProjectDataDirSubstitution(PrePersistMethod.OnPreUpdate);
        }

        [Test]
        public void OnPreInsert_WaterQualityModel1DSettings_SubstituteOutProjectDataDirectoryInPaths()
        {
            DoPrePersistCallForWaterQualityModel1DSettingsAndAssertProjectDataDirSubstitution(PrePersistMethod.OnPreInsert);
        }

        [Test]
        public void OnPreDelete_WaterQualityModel1DSettings_SubstituteOutProjectDataDirectoryInPaths()
        {
            DoPrePersistCallForWaterQualityModel1DSettingsAndAssertProjectDataDirSubstitution(PrePersistMethod.OnPreDelete);
        }

        private void DoPrePersistCallForWaterQualityModel1DSettingsAndAssertProjectDataDirSubstitution(PrePersistMethod methodToCall)
        {
            // setup
            var repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            var listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var settings = new WaterQualityModelSettings { OutputDirectory = Path.Combine(repoPath + "_data", "foo", "bar") };

            var stateArray = new object[]
            {
                settings.OutputDirectory,
                "haha",
                "hihi"
            };
            var propertyNamesArray = new[]
            {
                TypeUtils.GetMemberName<WaterQualityModelSettings>(m => m.OutputDirectory),
                "hihi",
                "haha"
            };

            // call
            var returnResult = CallPrePersistMethod(methodToCall, listener, settings, stateArray, propertyNamesArray);

            // assert
            Assert.IsFalse(returnResult, "Should not veto.");
            Assert.AreEqual(@"$data$.\foo\bar", settings.OutputDirectory);
            Assert.AreEqual(@"$data$.\foo\bar", stateArray[0]);
        }

        private static WaterQualityModelDataAccessListener CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(
            string repoPath)
        {
            var mocks = new MockRepository();
            var projectRepoMock = mocks.Stub<IProjectRepository>();
            projectRepoMock.Stub(pr => pr.Path).Return(repoPath);
            mocks.ReplayAll();
            var listener = new WaterQualityModelDataAccessListener { ProjectRepository = projectRepoMock };
            return listener;
        }

        [Test]
        public void OnPostLoadTestWaqModel_RestoreModelDataDirectoryToRootedPath()
        {
            DoPostPersistCallOnModelAndRestoreProjectDataDirectoryPaths(PostPersistMethod.OnPostLoad);
        }

        [Test]
        public void OnPostUpdateTestWaqModel_RestoreModelDataDirectoryToRootedPath()
        {
            DoPostPersistCallOnModelAndRestoreProjectDataDirectoryPaths(PostPersistMethod.OnPostUpdate);
        }

        [Test]
        public void OnPostInsertTestWaqModel_RestoreModelDataDirectoryToRootedPath()
        {
            DoPostPersistCallOnModelAndRestoreProjectDataDirectoryPaths(PostPersistMethod.OnPostInsert);
        }

        private static void DoPostPersistCallOnModelAndRestoreProjectDataDirectoryPaths(PostPersistMethod methodToCall)
        {
            // setup
            var repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            var listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var waqModel = new WaterQualityModel { ModelDataDirectory = @"$data$.\test\foo\bar" };

            var stateArray = new object[]
            {
                "haha",
                0.2,
                waqModel.ModelDataDirectory,
                double.NaN
            };
            var propertyNames = new[]
            {
                "hihi",
                "number",
                TypeUtils.GetMemberName<WaterQualityModel>(m => m.ModelDataDirectory),
                "not a number"
            };

            // call
            CallPostPersistMethod(methodToCall, listener, waqModel, stateArray, propertyNames);

            // assert
            Assert.AreEqual(Path.Combine(repoPath + "_data", "test", "foo", "bar"), waqModel.ModelDataDirectory);
            Assert.AreEqual(Path.Combine(repoPath + "_data", "test", "foo", "bar"), stateArray[2]);
        }

        [Test]
        public void OnPostLoadTestDataTableManager_RestoreModelDataDirectoryToRootedPath()
        {
            DoPostPersistCallOnDataTableManagerAndRestoreProjectDataDirectoryPaths(PostPersistMethod.OnPostLoad);
        }

        [Test]
        public void OnPostUpdateTestDataTableManager_RestoreModelDataDirectoryToRootedPath()
        {
            DoPostPersistCallOnDataTableManagerAndRestoreProjectDataDirectoryPaths(PostPersistMethod.OnPostUpdate);
        }

        [Test]
        public void OnPostInsertTestDataTableManager_RestoreModelDataDirectoryToRootedPath()
        {
            DoPostPersistCallOnDataTableManagerAndRestoreProjectDataDirectoryPaths(PostPersistMethod.OnPostInsert);
        }

        private static void DoPostPersistCallOnDataTableManagerAndRestoreProjectDataDirectoryPaths(PostPersistMethod methodToCall)
        {
            // setup
            var repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            var listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var manager = new DataTableManager { FolderPath = @"$data$.\foo\bar" };

            var stateArray = new object[]
            {
                "haha",
                manager.FolderPath,
                double.NaN
            };
            var propertyNames = new[]
            {
                "hihi",
                TypeUtils.GetMemberName<DataTableManager>(m => m.FolderPath),
                "not a number"
            };

            // call
            CallPostPersistMethod(methodToCall, listener, manager, stateArray, propertyNames);

            // assert
            Assert.AreEqual(Path.Combine(repoPath + "_data", "foo", "bar"), manager.FolderPath);
            Assert.AreEqual(Path.Combine(repoPath + "_data", "foo", "bar"), stateArray[1]);
        }

        [Test]
        public void OnPostLoadTest_WaterQualityModel1DSettings_RestoreModelDataDirectoryToRootedPath()
        {
            DoPostPersistCallOnWaterQualityModel1DSettingsAndRestoreProjectDataDirectoryPaths(PostPersistMethod.OnPostLoad);
        }

        [Test]
        public void OnPostUpdateTest_WaterQualityModel1DSettings_RestoreModelDataDirectoryToRootedPath()
        {
            DoPostPersistCallOnWaterQualityModel1DSettingsAndRestoreProjectDataDirectoryPaths(PostPersistMethod.OnPostUpdate);
        }

        [Test]
        public void OnPostInsertTest_WaterQualityModel1DSettings_RestoreModelDataDirectoryToRootedPath()
        {
            DoPostPersistCallOnWaterQualityModel1DSettingsAndRestoreProjectDataDirectoryPaths(PostPersistMethod.OnPostInsert);
        }

        private static void DoPostPersistCallOnWaterQualityModel1DSettingsAndRestoreProjectDataDirectoryPaths(PostPersistMethod methodToCall)
        {
            // setup
            var repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            var listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var settings = new WaterQualityModelSettings { OutputDirectory = @"$data$.\foo\bar" };

            var stateArray = new object[]
            {
                "haha",
                double.NaN,
                settings.OutputDirectory
            };
            var propertyNames = new[]
            {
                "hihi",
                "not a number",
                TypeUtils.GetMemberName<WaterQualityModelSettings>(m => m.OutputDirectory),
            };

            // call
            CallPostPersistMethod(methodToCall, listener, settings, stateArray, propertyNames);

            // assert
            Assert.AreEqual(Path.Combine(repoPath + "_data", "foo", "bar"), settings.OutputDirectory);
            Assert.AreEqual(Path.Combine(repoPath + "_data", "foo", "bar"), stateArray[2]);
        }

        private static void CallPostPersistMethod(PostPersistMethod methodToCall, WaterQualityModelDataAccessListener listener,
            object entity, object[] stateArray, string[] propertyNames)
        {
            switch (methodToCall)
            {
                case PostPersistMethod.OnPostLoad:
                    listener.OnPostLoad(entity, stateArray, propertyNames);
                    break;
                case PostPersistMethod.OnPostUpdate:
                    listener.OnPostUpdate(entity, stateArray, propertyNames);
                    break;

                case PostPersistMethod.OnPostInsert:
                    listener.OnPostInsert(entity, stateArray, propertyNames);
                    break;
            }
        }
    }
}