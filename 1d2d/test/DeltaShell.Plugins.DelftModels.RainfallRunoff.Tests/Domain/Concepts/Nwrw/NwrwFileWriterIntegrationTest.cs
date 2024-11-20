using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Domain.Concepts.Nwrw
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class NwrwFileWriterIntegrationTest
    {
        [Test]
        public void DryWeatherFlowDefinitionsAreCorrectlyWrittenToFile()
        {
            // Setup
            var rrModel = new RainfallRunoffModel();
            rrModel.NwrwDryWeatherFlowDefinitions = NwrwFileWriterIntegrationTestHelper.GenerateNwrwDryWeatherFlowDefinitions();

            var nwrwWriter = new NwrwModelFileWriter(new NwrwComponentFileWriterBase[]
            {
                new NwrwDwfComponentFileWriter(rrModel)
            });

            var expectedFile = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath("nwrw/pluvius.dwa"));
            var testFolder = FileUtils.CreateTempDirectory();
            var pluviusDwaFilePath = Path.Combine(testFolder, "pluvius.dwa");
            try
            {
                // Call
                nwrwWriter.WriteNwrwFiles(testFolder);

                // Assert
                Assert.IsTrue(File.Exists(pluviusDwaFilePath));
                FileAssert.AreEqual(expectedFile, pluviusDwaFilePath);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(pluviusDwaFilePath));
                FileUtils.DeleteIfExists(Path.GetDirectoryName(expectedFile));
            }
        }

        [Test]
        public void GeneralNwrwDataIsCorrectlyWrittenToFile()
        {
            // Setup
            var rrModel = new RainfallRunoffModel();
            rrModel.NwrwDefinitions = NwrwFileWriterIntegrationTestHelper.GenerateNwrwDefinitions();

            var nwrwWriter = new NwrwModelFileWriter(new NwrwComponentFileWriterBase[]
            {
                new NwrwAlgComponentFileWriter(rrModel),
            });

            var expectedFile = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath("nwrw/pluvius.alg"));
            var testFolder = FileUtils.CreateTempDirectory();
            var pluviusAlgFilePath = Path.Combine(testFolder, "pluvius.alg");
            try
            {
                // Call
                nwrwWriter.WriteNwrwFiles(testFolder);

                // Assert
                Assert.IsTrue(File.Exists(pluviusAlgFilePath));
                FileAssert.AreEqual(expectedFile, pluviusAlgFilePath);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(pluviusAlgFilePath));
                FileUtils.DeleteIfExists(Path.GetDirectoryName(expectedFile));
            }
        }

        [Test]
        public void Nwrw3bDataIsCorrectlyWrittenToFile()
        {
            // Setup
            var rrModel = new RainfallRunoffModel();
            NwrwFileWriterIntegrationTestHelper.GenerateNwrwModelData(rrModel);
            IEnumerable<NwrwData> nwrwData = rrModel.GetAllModelData().OfType<NwrwData>();

            var nwrwWriter = new NwrwModelFileWriter(new NwrwComponentFileWriterBase[]
            {
                new Nwrw3BComponentFileWriter(rrModel),
            });

            var expectedFile = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath("nwrw/pluvius.3b"));
            var testFolder = FileUtils.CreateTempDirectory();
            var pluviusAlgFilePath = Path.Combine(testFolder, "pluvius.3b");
            try
            {
                // Call
                nwrwWriter.WriteNwrwFiles(testFolder);

                // Assert
                Assert.IsTrue(File.Exists(pluviusAlgFilePath));
                FileAssert.AreEqual(expectedFile, pluviusAlgFilePath);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(pluviusAlgFilePath));
                FileUtils.DeleteIfExists(Path.GetDirectoryName(expectedFile));
            }
        }

        [Test]
        public void NwrwTpDataIsCorrectlyWrittenToFile()
        {
            // Setup
            var rrModel = new RainfallRunoffModel();
            NwrwFileWriterIntegrationTestHelper.GenerateNwrwModelData(rrModel);

            var nwrwWriter = new NwrwModelFileWriter(new NwrwComponentFileWriterBase[]
            {
                new NwrwTpComponentFileWriter(rrModel),
            });

            var expectedFile = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath("nwrw/3BRUNOFF.TP"));
            var testFolder = FileUtils.CreateTempDirectory();
            var pluviusAlgFilePath = Path.Combine(testFolder, "3BRUNOFF.TP");
            try
            {
                // Call
                nwrwWriter.WriteNwrwFiles(testFolder);

                // Assert
                Assert.IsTrue(File.Exists(pluviusAlgFilePath));
                FileAssert.AreEqual(expectedFile, pluviusAlgFilePath);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(pluviusAlgFilePath));
                FileUtils.DeleteIfExists(Path.GetDirectoryName(expectedFile));
            }
        }

    }
}