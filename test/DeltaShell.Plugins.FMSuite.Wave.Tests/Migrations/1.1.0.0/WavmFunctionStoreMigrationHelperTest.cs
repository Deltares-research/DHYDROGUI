using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.IO;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._1._0._0
{
    [TestFixture]
    public class WavmFunctionStoreMigrationHelperTest
    {
        private static IEnumerable<TestCaseData> UpdateWavmFileFunctionStorePaths_ParameterNull_Data()
        {
            yield return new TestCaseData(null, new WaveModel(), "modelPath");
            yield return new TestCaseData("some/path", null, "model");
        }

        [Test]
        [TestCaseSource(nameof(UpdateWavmFileFunctionStorePaths_ParameterNull_Data))]
        public void UpdateWavmFileFunctionStorePath_ParameterNull_ThrowsArgumentNullException(string modelPath, WaveModel model, string expectedParameterName)
        {
            void Call() => WavmFunctionStoreMigrationHelper.UpdateWavmFileFunctionStorePaths(modelPath, model);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));

            model?.Dispose();
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateWavmFileFunctionStorePaths_ExpectedResults()
        {
            // Setup
            string inputDataPath = TestHelper.GetTestFilePath(Path.Combine("Migrations", "1.1.0.0", nameof(WaveDirectoryStructureMigrationHelperTest), "wavm-wad.nc"));

            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                string oldModelPath = tempDir.CreateDirectory("oldModel");
                string newModelPath = tempDir.CreateDirectory("newModel");
                string expectedOutputPath = tempDir.CreateDirectory(
                    Path.Combine("newModel", "output"));

                string ncOldPath = Path.Combine(oldModelPath, Path.GetFileName(inputDataPath));
                string ncNewPath = Path.Combine(expectedOutputPath, Path.GetFileName(inputDataPath));

                File.Copy(inputDataPath, ncOldPath);
                File.Copy(inputDataPath, ncNewPath);

                // Associate wavm file function store with the outer domain
                var functionStore = new WavmFileFunctionStore(ncOldPath);
                var dataItem = new DataItem(functionStore, DataItemRole.Output, WaveModel.WavmStoreDataItemTag + model.OuterDomain.Name);
                model.DataItems.Add(dataItem);

                // Call
                WavmFunctionStoreMigrationHelper.UpdateWavmFileFunctionStorePaths(newModelPath, model);

                // Assert
                Assert.That(functionStore.Path, Is.EqualTo(ncNewPath));
                functionStore.Close();
            }
        }

        [Test]
        public void RemoveInvalidWavmFunctionStores_ModelNull_ThrowsArgumentNullException()
        {
            void Call() => WavmFunctionStoreMigrationHelper.RemoveInvalidWavmFunctionStores(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveModel"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void RemoveInvalidWavmFunctionStores_FileNotExists_FunctionStoreRemoved()
        {
            // Setup
            string inputDataPath = TestHelper.GetTestFilePath(Path.Combine("Migrations", "1.1.0.0", nameof(WaveDirectoryStructureMigrationHelperTest), "wavm-wad.nc"));

            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                string ncPath = Path.Combine(tempDir.Path, Path.GetFileName(inputDataPath));
                File.Copy(inputDataPath, ncPath);

                // Associate wavm file function store with the outer domain
                var functionStore = new WavmFileFunctionStore(ncPath);
                var dataItem = new DataItem(functionStore, DataItemRole.Output, WaveModel.WavmStoreDataItemTag + model.OuterDomain.Name);
                model.DataItems.Add(dataItem);

                File.Delete(ncPath);
                Assert.That(model.WavmFunctionStores.Count(), Is.EqualTo(1), "Precondition: model has one WavmFileFunctionStore.");

                // Call
                WavmFunctionStoreMigrationHelper.RemoveInvalidWavmFunctionStores(model);

                // Assert
                Assert.That(model.WavmFunctionStores, Has.No.Member(functionStore));
                functionStore.Close();
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void RemoveInvalidWavmFunctionStores_FileExists_FunctionStoreRetained()
        {
            // Setup
            string inputDataPath = TestHelper.GetTestFilePath(Path.Combine("Migrations", "1.1.0.0", nameof(WaveDirectoryStructureMigrationHelperTest), "wavm-wad.nc"));

            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                string ncPath = Path.Combine(tempDir.Path, Path.GetFileName(inputDataPath));
                File.Copy(inputDataPath, ncPath);

                // Associate wavm file function store with the outer domain
                var functionStore = new WavmFileFunctionStore(ncPath);
                var dataItem = new DataItem(functionStore, DataItemRole.Output, WaveModel.WavmStoreDataItemTag + model.OuterDomain.Name);
                model.DataItems.Add(dataItem);

                Assert.That(model.WavmFunctionStores.Count(), Is.EqualTo(1), "Precondition: model has one WavmFileFunctionStore.");

                // Call
                WavmFunctionStoreMigrationHelper.RemoveInvalidWavmFunctionStores(model);

                // Assert
                Assert.That(model.WavmFunctionStores, Has.Member(functionStore));
                functionStore.Close();
            }
        }
    }
}