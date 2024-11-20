using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._1._0._0
{
    [TestFixture]
    public class WavmFunctionStoreMigrationHelperTest
    {
        [Test]
        public void DisconnectWavmFunctionStores_ModelNull_ThrowsArgumentNullException()
        {
            void Call() => WavmFunctionStoreMigrationHelper.DisconnectWavmFunctionStores(null, Substitute.For<ILogHandler>());

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveModel"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void DisconnectWavmFunctionStores_FileNotExists_FunctionStorePathIsSetToEmpty()
        {
            // Setup
            string inputDataPath = TestHelper.GetTestFilePath(Path.Combine("Migrations", "1.1.0.0", nameof(WaveDirectoryStructureMigrationHelperTest), "wavm-wad.nc"));

            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                string ncPath = Path.Combine(tempDir.Path, Path.GetFileName(inputDataPath));
                File.Copy(inputDataPath, ncPath);

                // Associate wavm file function store with the outer domain
                const string functionStoreName = "someName";
                var functionStore = new WavmFileFunctionStore(ncPath) {Name = functionStoreName};
                var dataItem = new DataItem(functionStore, DataItemRole.Output, 
                                            WavmFunctionStoreMigrationHelper.WavmStoreDataItemTag + model.OuterDomain.Name);
                model.DataItems.Add(dataItem);

                File.Delete(ncPath);
                Assert.That(GetWavmFunctionStoresFromModel(model).Count(), Is.EqualTo(1), "Precondition: model has one WavmFileFunctionStore.");

                var logHandler = Substitute.For<ILogHandler>();

                // Call
                WavmFunctionStoreMigrationHelper.DisconnectWavmFunctionStores(model, logHandler);

                // Assert
                logHandler.Received(1).ReportWarningFormat("The link with {0} has been broken.", functionStoreName);
                Assert.That(GetWavmFunctionStoresFromModel(model), Has.Member(functionStore));
                Assert.That(functionStore.Path, Is.EqualTo(string.Empty));
                functionStore.Close();
            }
        }

        private static IEnumerable<WavmFileFunctionStore> GetWavmFunctionStoresFromModel(WaveModel model)
        {
                return
                    WaveDomainHelper.GetAllDomains(model.OuterDomain).Select(
                                        domain => model.GetDataItemByTag(WavmFunctionStoreMigrationHelper.WavmStoreDataItemTag + domain.Name))
                                    .Where(di => di != null)
                                    .Select(di => di.Value as WavmFileFunctionStore);
        }
    }
}