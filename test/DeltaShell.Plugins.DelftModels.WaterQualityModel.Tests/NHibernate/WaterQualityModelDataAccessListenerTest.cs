using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.Model;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.NHibernate;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.NHibernate
{
    [TestFixture]
    public class WaterQualityModelDataAccessListenerTest
    {
        [Test]
        public void TestClone()
        {
            // setup
            var listener = new WaterQualityModelDataAccessListener(null);

            // call
            object clone = listener.Clone();

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

            var listener = new WaterQualityModelDataAccessListener(null);

            // call
            listener.OnPostLoad(objectMock, new object[0], new string[0]);

            // assert
            mocks.VerifyAll();
        }

        [TestCaseSource(nameof(PrePersistMethods))]
        public void DoPrePersistCallAndAssertProjectDataDirSubstitution(Func<WaterQualityModelDataAccessListener, object, object[], string[], bool> prePersistMethod)
        {
            // setup
            string repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            WaterQualityModelDataAccessListener listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var waqModel = new WaterQualityModel {ModelDataDirectory = Path.Combine(repoPath + "_data", "test", "foo", "bar")};

            var stateArray = new object[]
            {
                "haha",
                waqModel.ModelDataDirectory,
                "hihi",
                double.NaN
            };
            string[] propertyNamesArray = new[]
            {
                "hihi",
                nameof(WaterQualityModel.ModelDataDirectory),
                "haha",
                "number"
            };

            // call
            bool returnResult = prePersistMethod.Invoke(listener, waqModel, stateArray, propertyNamesArray);

            // assert
            Assert.IsFalse(returnResult, "Should not veto.");
            Assert.AreEqual(@"$data$test\foo\bar", waqModel.ModelDataDirectory);
            Assert.AreEqual(@"$data$test\foo\bar", stateArray[1]);
        }

        [TestCaseSource(nameof(PrePersistMethods))]
        public void DoPrePersistCallForDataTableManagerAndAssertProjectDataDirSubstitution(Func<WaterQualityModelDataAccessListener, object, object[], string[], bool> prePersistMethod)
        {
            // setup
            string repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            WaterQualityModelDataAccessListener listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var manager = new DataTableManager {FolderPath = Path.Combine(repoPath + "_data", "foo", "bar")};

            var stateArray = new object[]
            {
                "haha",
                manager.FolderPath,
                "hihi"
            };
            string[] propertyNamesArray = new[]
            {
                "hihi",
                nameof(DataTableManager.FolderPath),
                "haha"
            };

            // call
            bool returnResult = prePersistMethod.Invoke(listener, manager, stateArray, propertyNamesArray);

            // assert
            Assert.IsFalse(returnResult, "Should not veto.");
            Assert.AreEqual(@"$data$foo\bar", manager.FolderPath);
            Assert.AreEqual(@"$data$foo\bar", stateArray[1]);
        }

        [TestCaseSource(nameof(PrePersistMethods))]
        public void DoPrePersistCallForWaterQualityModel1DSettingsAndAssertProjectDataDirSubstitution(Func<WaterQualityModelDataAccessListener, object, object[], string[], bool> prePersistMethod)
        {
            // setup
            string repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            WaterQualityModelDataAccessListener listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var settings = new WaterQualityModelSettings {OutputDirectory = Path.Combine(repoPath + "_data", "foo", "bar")};

            var stateArray = new object[]
            {
                settings.OutputDirectory,
                "haha",
                "hihi"
            };
            string[] propertyNamesArray = new[]
            {
                nameof(WaterQualityModelSettings.OutputDirectory),
                "hihi",
                "haha"
            };

            // call
            bool returnResult = prePersistMethod.Invoke(listener, settings, stateArray, propertyNamesArray);

            // assert
            Assert.IsFalse(returnResult, "Should not veto.");
            Assert.AreEqual(@"$data$foo\bar", settings.OutputDirectory);
            Assert.AreEqual(@"$data$foo\bar", stateArray[0]);
        }

        [TestCaseSource(nameof(PostPersistMethods))]
        public static void DoPostPersistCallOnModelAndRestoreProjectDataDirectoryPaths(Action<WaterQualityModelDataAccessListener, object, object[], string[]> postPersistMethod)
        {
            // setup
            string repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            WaterQualityModelDataAccessListener listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var waqModel = new WaterQualityModel {ModelDataDirectory = @"$data$.\test\foo\bar"};

            var stateArray = new object[]
            {
                "haha",
                0.2,
                waqModel.ModelDataDirectory,
                double.NaN
            };
            string[] propertyNames = new[]
            {
                "hihi",
                "number",
                nameof(WaterQualityModel.ModelDataDirectory),
                "not a number"
            };

            // call
            postPersistMethod.Invoke(listener, waqModel, stateArray, propertyNames);

            // assert
            Assert.AreEqual(Path.Combine(repoPath + "_data", "test", "foo", "bar"), waqModel.ModelDataDirectory);
            Assert.AreEqual(Path.Combine(repoPath + "_data", "test", "foo", "bar"), stateArray[2]);
        }

        [TestCaseSource(nameof(PostPersistMethods))]
        public static void DoPostPersistCallOnDataTableManagerAndRestoreProjectDataDirectoryPaths(Action<WaterQualityModelDataAccessListener, object, object[], string[]> postPersistMethod)
        {
            // setup
            string repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            WaterQualityModelDataAccessListener listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var manager = new DataTableManager {FolderPath = @"$data$.\foo\bar"};

            var stateArray = new object[]
            {
                "haha",
                manager.FolderPath,
                double.NaN
            };
            string[] propertyNames = new[]
            {
                "hihi",
                nameof(DataTableManager.FolderPath),
                "not a number"
            };

            // call
            postPersistMethod.Invoke(listener, manager, stateArray, propertyNames);

            // assert
            Assert.AreEqual(Path.Combine(repoPath + "_data", "foo", "bar"), manager.FolderPath);
            Assert.AreEqual(Path.Combine(repoPath + "_data", "foo", "bar"), stateArray[1]);
        }

        [TestCaseSource(nameof(PostPersistMethods))]
        public static void DoPostPersistCallOnWaterQualityModel1DSettingsAndRestoreProjectDataDirectoryPaths(Action<WaterQualityModelDataAccessListener, object, object[], string[]> postPersistMethod)
        {
            // setup
            string repoPath = Path.Combine(Directory.GetCurrentDirectory(), "A");
            WaterQualityModelDataAccessListener listener = CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(repoPath);

            var settings = new WaterQualityModelSettings {OutputDirectory = @"$data$.\foo\bar"};

            var stateArray = new object[]
            {
                "haha",
                double.NaN,
                settings.OutputDirectory
            };
            string[] propertyNames = new[]
            {
                "hihi",
                "not a number",
                nameof(WaterQualityModelSettings.OutputDirectory)
            };

            // call
            postPersistMethod.Invoke(listener, settings, stateArray, propertyNames);

            // assert
            Assert.AreEqual(Path.Combine(repoPath + "_data", "foo", "bar"), settings.OutputDirectory);
            Assert.AreEqual(Path.Combine(repoPath + "_data", "foo", "bar"), stateArray[2]);
        }

        private static IEnumerable<Action<WaterQualityModelDataAccessListener, object, object[], string[]>> PostPersistMethods()
        {
            yield return (listener, entity, state, propertyNames) => listener.OnPostLoad(entity, state, propertyNames);
            yield return (listener, entity, state, propertyNames) => listener.OnPostInsert(entity, state, propertyNames);
            yield return (listener, entity, state, propertyNames) => listener.OnPostUpdate(entity, state, propertyNames);
        }

        private static IEnumerable<Func<WaterQualityModelDataAccessListener, object, object[], string[], bool>> PrePersistMethods()
        {
            yield return (listener, entity, state, propertyNames) => listener.OnPreInsert(entity, state, propertyNames);
            yield return (listener, entity, state, propertyNames) => listener.OnPreDelete(entity, state, propertyNames);
            yield return (listener, entity, state, propertyNames) => listener.OnPreUpdate(entity, state, propertyNames);
        }

        private static WaterQualityModelDataAccessListener CreateWaterQualityModelDataAcessListenerWithMockedProjectRepository(
            string repoPath)
        {
            var mocks = new MockRepository();
            var projectRepoMock = mocks.Stub<IProjectRepository>();
            projectRepoMock.Stub(pr => pr.Path).Return(repoPath);
            mocks.ReplayAll();
            return new WaterQualityModelDataAccessListener(projectRepoMock);
        }
    }
}