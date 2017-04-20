using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.CompareSobek.Tests;
using NUnit.Framework;

namespace SobekCompare.Tests
{
    [TestFixture]
    [Category(TestCategorySobekValidation.WaterFlow1D)]
    public class CompareStuwJaapResultTest
    {
        private string baseDir;

        [SetUp]
        public void SetUp()
        {
            LogHelper.ConfigureLogging();
            baseDir = Path.Combine(TestHelper.GetDataDir(), "StuwJaap.lit");
        }
        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestFreeFlowWeir()
        {
            var pathDirSobek = Path.Combine(baseDir, "2");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestDrownedFlowWeir()
        {
            var pathDirSobek = Path.Combine(baseDir, "18");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestPump()
        {
            var pathDirSobek = Path.Combine(baseDir, "5");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestBridge()
        {
            var pathDirSobek = Path.Combine(baseDir, "6");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestBridgeFree()
        {
            var pathDirSobek = Path.Combine(baseDir, "20");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestBridgeConstantQ()
        {
            var pathDirSobek = Path.Combine(baseDir, "9");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestPillarBridgeConstantQ()
        {
            var pathDirSobek = Path.Combine(baseDir, "11");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestPillarBridgeConstantH()
        {
            var pathDirSobek = Path.Combine(baseDir, "22");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestCulvert()
        {
            var pathDirSobek = Path.Combine(baseDir, "7");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestCulvertTooLow()
        {
            var pathDirSobek = Path.Combine(baseDir, "31");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestCulvertDrowned()
        {
            var pathDirSobek = Path.Combine(baseDir, "12");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }
        [Test]
        public void TestCulvertConstantQ()
        {
            var pathDirSobek = Path.Combine(baseDir, "10");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestCulvertConstantQFree()
        {
            var pathDirSobek = Path.Combine(baseDir, "13");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestCulvertCunette()
        {
            var pathDirSobek = Path.Combine(baseDir, "14");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestCulvertCunetteFree()
        {
            var pathDirSobek = Path.Combine(baseDir, "15");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        [Category(TestCategory.VerySlow)]
        public void TestPidController()
        {
            var pathDirSobek = Path.Combine(baseDir, "24");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestSiphon()
        {
            var pathDirSobek = Path.Combine(baseDir, "1");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestInvertedSiphon()
        {
            var pathDirSobek = Path.Combine(baseDir, "3");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestFreeUniversalWeir()
        {
            var pathDirSobek = Path.Combine(baseDir, "4");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestDrownedUniversalWeir()
        {
            var pathDirSobek = Path.Combine(baseDir, "8");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestFreeAdvancedWeir()
        {
            var pathDirSobek = Path.Combine(baseDir, "16");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestDrownedAdvancedWeir()
        {
            var pathDirSobek = Path.Combine(baseDir, "17");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestFreeGeneralStructure()
        {
            var pathDirSobek = Path.Combine(baseDir, "21");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestDrownedGeneralStructure()
        {
            var pathDirSobek = Path.Combine(baseDir, "23");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestFreeOrifice()
        {
            var pathDirSobek = Path.Combine(baseDir, "26");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestDrownedOrifice()
        {
            var pathDirSobek = Path.Combine(baseDir, "25");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestFreeRiverWeir()
        {
            var pathDirSobek = Path.Combine(baseDir, "27");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestDrownedRiverWeir()
        {
            var pathDirSobek = Path.Combine(baseDir, "28");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestCulvertValve()
        {
            var pathDirSobek = Path.Combine(baseDir, "30");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestCompoundWithControl()
        {
            var pathDirSobek = Path.Combine(baseDir, "19");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestCompoundNoControl()
        {
            var pathDirSobek = Path.Combine(baseDir, "29");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);

        }
    }
}
