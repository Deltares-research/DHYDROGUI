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
                               "    teta                  = 1.0000000e+000      " + Environment.NewLine +
                               "    tidalPeriod           = 1.2417000e+001      " + Environment.NewLine +
                               "    advectionScheme       = vanLeer-2           " + Environment.NewLine +
                               "    c3                    = 1.0                 " + Environment.NewLine +
                               "    c4                    = 1.0                 " + Environment.NewLine +
                               "    c5                    = 0.5                 " + Environment.NewLine +
                               "    c6                    = 1.0                 " + Environment.NewLine +
                               "    c7                    = 0.0                 " + Environment.NewLine +
                               "    c8                    = 1.0                 " + Environment.NewLine +
                               "    c9                    = 0.0                 " + Environment.NewLine +
                               "    c10                   = 0.5                 " + Environment.NewLine +
                               "    c11                   = 0.0                 " + Environment.NewLine +
                               Environment.NewLine + 
                               "[Mouth]" + Environment.NewLine +
                               "    nodeId                = Test                # Estuary mouth node id" + Environment.NewLine;

            Assert.AreEqual(expectedText, File.ReadAllText(targetPath));
        }
    }
}
