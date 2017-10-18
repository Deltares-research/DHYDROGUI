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
        private readonly string destinationFile = TestHelper.GetTestFilePath(@"FileWriters/salinity/salinity.ini");

        [SetUp]
        public void Setup()
        {
            FileUtils.DeleteIfExists(destinationFile);
        }

        [Test]
        public void TestSalinityIniWriterThrowsExceptionWithConstantDispersionType()
        {
            var sourceFile = TestHelper.GetTestFilePath(@"FileWriters/salinity/ThatcherHarleman/salinity.ini");
            Assert.Throws<InvalidOperationException>(()=> WaterFlowModel1DSalinityIniWriter.WriteFile(sourceFile, DispersionFormulationType.Constant, ""));
        }


        [Test, Category(TestCategory.DataAccess)]
        public void SalinitiyFileForKuijperVanRijnPrismaticShouldBeCorrect()
        {
            var targetPath = Path.Combine(".", TestHelper.GetCurrentMethodName(),"salinity.ini");
            WaterFlowModel1DSalinityIniWriter.WriteFile(targetPath, DispersionFormulationType.KuijperVanRijnPrismatic, "Test");

            var expectedText = "[NumericalOptions]\r\n" + 
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

            Assert.AreEqual(expectedText, File.ReadAllText(targetPath));
        }
    }
}
