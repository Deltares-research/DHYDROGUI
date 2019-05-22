using System;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using NUnit.Framework;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class WaterFlowModel1DSalinityIniWriterTest
    {
        [Test]
        public void WhenWritingSalinityFileWithConstantDispersionType_ThenExceptionIsThrown()
        {
            var tempDirectory = FileUtils.CreateTempDirectory();
            var targetFilePath = Path.Combine(tempDirectory, "Salinity,ini");
            try
            {
                Assert.Throws<InvalidOperationException>(() => WaterFlowModel1DSalinityIniWriter.WriteFile(targetFilePath, DispersionFormulationType.Constant, ""));
            }
            finally
            {
                FileUtils.DeleteIfExists(tempDirectory);
            }
        }


        [Test, Category(TestCategory.DataAccess)]
        public void WhenWritingSalinityFile_ThenFileContentIsAsExpected()
        {
            var targetFilePath = Path.Combine(".", TestHelper.GetCurrentMethodName(),"salinity.ini");
            const string expectedText = "[NumericalOptions]\r\n" +
                                        "    teta                  = 1.0000000e+000      \r\n" +
                                        "    tidalPeriod           = 1.2417000e+001      \r\n" +
                                        "    advectionScheme       = vanLeer-2           \r\n" +
                                        "    c3                    = 1.0                 \r\n" +
                                        "    c4                    = 1.0                 \r\n" +
                                        "    c5                    = 0.5                 \r\n" +
                                        "    c6                    = 1.0                 \r\n" +
                                        "    c7                    = 0.0                 \r\n" +
                                        "    c8                    = 1.0                 \r\n" +
                                        "    c9                    = 0.0                 \r\n" +
                                        "    c10                   = 0.5                 \r\n" +
                                        "    c11                   = 0.0                 \r\n" +
                                        "[Mouth]\r\n" +
                                        "    nodeId                = Test                # Estuary mouth node id\r\n";

            try
            {
                WaterFlowModel1DSalinityIniWriter.WriteFile(targetFilePath, DispersionFormulationType.KuijperVanRijnPrismatic, "Test");
                Assert.AreEqual(expectedText, File.ReadAllText(targetFilePath));
            }
            finally
            {
                FileUtils.DeleteIfExists(targetFilePath);
            }
        }
    }
}
