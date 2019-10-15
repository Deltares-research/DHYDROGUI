using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.CompareSobek.Tests;
using NUnit.Framework;

namespace SobekCompare.Tests
{
    [TestFixture]
    [Category(TestCategorySobekValidation.WaterFlow1D)]
    public class CompareFlowJaapResultTest
    {
        private string baseDir;

        [SetUp]
        public void SetUp()
        {
            LogHelper.ConfigureLogging();
            baseDir = Path.Combine(TestHelper.GetTestDataDirectory(), "FlowJaap.lit");
        }
        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestWLBasic()
        {
            var pathDirSobek = Path.Combine(baseDir, "4");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestRectangularPressurized()
        {
            var pathDirSobek = Path.Combine(baseDir, "8");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestRectangularFreeSurface()
        {
            var pathDirSobek = Path.Combine(baseDir, "5");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }
        [Test]
        public void TestWLTrapeziumProfile()
        {
            var pathDirSobek = Path.Combine(baseDir, "2");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        [Ignore("Circle does not work yet")]
        public void TestWLCircleProfile()
        {
            var pathDirSobek = Path.Combine(baseDir, "7");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        [Ignore("Egg does not work yet")]
        public void TestWLEggProfile()
        {
            var pathDirSobek = Path.Combine(baseDir, "6");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestLateral()
        {
            var pathDirSobek = Path.Combine(baseDir, "3");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestLateral2()
        {
            var pathDirSobek = Path.Combine(baseDir, "21");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestLateralDiffusePos()
        {
            var pathDirSobek = Path.Combine(baseDir, "1");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestLateralDiffuseNeg()
        {
            var pathDirSobek = Path.Combine(baseDir, "23");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestQhBoundary()
        {
            var pathDirSobek = Path.Combine(baseDir, "16");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestExtraResistanceKsi()
        {
            var pathDirSobek = Path.Combine(baseDir, "17");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestTabulatedExtraResistanceKsi()
        {
            var pathDirSobek = Path.Combine(baseDir, "19");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestLateralMiddle()
        {
           var pathDirSobek = Path.Combine(baseDir, "25");
           CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestLateralDiffuseFlat()
        {
           var pathDirSobek = Path.Combine(baseDir, "27");
           CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestLateralDiffuseSlope()
        {
           var pathDirSobek = Path.Combine(baseDir, "26");
           CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestLateralNearest()
        {
            var pathDirSobek = Path.Combine(baseDir, "28");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestLateralLowest()
        {
            var pathDirSobek = Path.Combine(baseDir, "29");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestDiffuseForkFlat()
        {
            var pathDirSobek = Path.Combine(baseDir, "30");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

        [Test]
        public void TestLateral2TimeSeries()
        {
            var pathDirSobek = Path.Combine(baseDir, "31");
            CompareJaapHelper.RunAndCompareSobekAndWaterFlow1D(pathDirSobek);
        }

    }
}
