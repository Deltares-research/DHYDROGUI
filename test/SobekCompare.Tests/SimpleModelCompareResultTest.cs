using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.CompareSobek.Tests;
using NUnit.Framework;

namespace SobekCompare.Tests
{
    [TestFixture]
    [Category(TestCategorySobekValidation.WaterFlow1D)]
    public class SimpleModelCompareResultTest
    {
        private string baseDir;

        [SetUp]
        public void SetUp()
        {
            LogHelper.ConfigureLogging();
            baseDir = Path.Combine(TestHelper.GetDataDir(), "BasicTst.lit");
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void CSBasic()
        {
            var pathDirSobek = Path.Combine(baseDir,"1");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void CSCase2()
        {
            var pathDirSobek = Path.Combine(baseDir, "2");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void FrictionManning()
        {
            var pathDirSobek = Path.Combine(baseDir, "3");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void FrictionStrickler()
        {
            var pathDirSobek = Path.Combine(baseDir, "4");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void Weir()
        {
            var pathDirSobek = Path.Combine(baseDir, "5");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void Weir_CA()
        {
            var pathDirSobek = Path.Combine(baseDir, "23");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void Culvert()
        {
            var pathDirSobek = Path.Combine(baseDir, "6");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void Orifice()
        {
            var pathDirSobek = Path.Combine(baseDir, "7");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void RiverWeir()
        {
            var pathDirSobek = Path.Combine(baseDir, "8");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void UniWeir()
        {
            var pathDirSobek = Path.Combine(baseDir, "9");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void UniWeirConstantQ()
        {
            var pathDirSobek = Path.Combine(baseDir, "27");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void AdvancedWeir()
        {
            var pathDirSobek = Path.Combine(baseDir, "10");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void InitialFlow()
        {
            var pathDirSobek = Path.Combine(baseDir, "11");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void InitialLevel()
        {
            var pathDirSobek = Path.Combine(baseDir, "12");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void InitialDepth()
        {
            var pathDirSobek = Path.Combine(baseDir, "13");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void FlowBoundary()
        {
            var pathDirSobek = Path.Combine(baseDir, "14");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void LevelBoundary()
        {
            var pathDirSobek = Path.Combine(baseDir, "15");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void LateralConstant()
        {
            var pathDirSobek = Path.Combine(baseDir, "16");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void LateralTimeSeries()
        {
            var pathDirSobek = Path.Combine(baseDir, "17");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void Pump()
        {
            var pathDirSobek = Path.Combine(baseDir, "18");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void PumpSwitching()
        {
            var pathDirSobek = Path.Combine(baseDir, "26");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void RiverPump()
        {
            var pathDirSobek = Path.Combine(baseDir, "19");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void Bridge()
        {
            var pathDirSobek = Path.Combine(baseDir, "20");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TabulatedCrossSectionWithStorage()
        {
            var pathDirSobek = Path.Combine(baseDir, "21");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void RiverProfileMainOnly()
        {
            var pathDirSobek = Path.Combine(baseDir, "22");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void RiverProfileMainFP1()
        {
            var pathDirSobek = Path.Combine(baseDir, "24");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

       [Test]
       public void RiverProfileMainFP1FP2()
       {
          var pathDirSobek = Path.Combine(baseDir, "25");
          CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
       }
       [Test]
       public void RiverProfileSummerDike()
       {
          var pathDirSobek = Path.Combine(baseDir, "28");
          CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
       }
    }
}
