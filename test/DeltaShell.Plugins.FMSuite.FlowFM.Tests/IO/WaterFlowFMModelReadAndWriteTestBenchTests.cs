using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{

    [Category(TestCategory.Slow)]
    [Category(TestCategory.WindowsForms)]
    [TestFixture]
    public class WaterFlowFMModelReadAndWriteTestBenchTests
    {

        #region TestFixture
        private static string TestFixtureDirectory = string.Empty;
        private static string PathTestBench = string.Empty;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            TestFixtureDirectory = FileUtils.CreateTempDirectory();
            PathTestBench = Path.Combine(TestHelper.GetTestDataDirectory(), "DSCTestBenchTests");

            //Ensure we do not accidentally incorporate previous results
            //FileUtils.DeleteIfExists("");
            //Directory.CreateDirectory("");
            //Directory.CreateDirectory("");

        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            FileUtils.DeleteIfExists(TestFixtureDirectory);
        }

        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }
        #endregion

        #region TestBanchCasesTests

        [Test]
        [TestCase(@"f100_1D\c01_straightchannel_weirs\dflowfm\flowfm.mdu", "c01_straightchannel_weirs")]
        [TestCase(@"f100_1D\c02_retention\flowfm.mdu", "c02_retention")]
        [TestCase(@"f100_1D\c03_orifice\flowfm.mdu", "c03_orifice")]
        [TestCase(@"f100_1D\c04_closed_channel_circle\flowfm.mdu", "c04_closed_channel_circle")]
        [TestCase(@"f100_1D\c05_closed_channel_rectangle\flowfm.mdu", "c05_closed_channel_rectangle")]
        [TestCase(@"f100_1D\c06_closed_channel_egg\flowfm.mdu", "c06_closed_channel_egg")]
        //[TestCase(@"f100_1D\c07_pipe_circle_weirs\flowfm.mdu", "c07_pipe_circle_weirs")]
        [TestCase(@"f101_1D-boundaries\c01_steady-state-flow\Boundary.mdu", "c01_steady-state-flow")]
        [TestCase(@"f102_lateral-flows\c01_straightchannel_weirs\dflowfm\flowfm.mdu", "c01_straightchannel_weirs")]
        [TestCase(@"f100_1D\c02_1d-precipitation\model1.mdu", "c02_1d-precipitation")]
        [TestCase(@"f105_cross-sections\c03_zw-closed-egg-profile\flow1d.mdu", "c03_zw-closed-egg-profile")]
        [TestCase(@"f105_cross-sections\c04_rectangular-profile\flow1d.mdu", "c04_rectangular-profile")]
        [TestCase(@"f105_cross-sections\c07_tabulated-profile-zw\flow1d.mdu", "c07_tabulated-profile-zw")]
        [TestCase(@"f105_cross-sections\c08_YZ-profile-storage\YZ_Storage.mdu", "c08_YZ-profile-storage")]
        [TestCase(@"f105_cross-sections\c11_rectangular-profile-storage\rectangular_storage.mdu", "c11_rectangular-profile-storage")]
        [TestCase(@"f105_cross-sections\c14_tabulated-profile-zw-storage\ZW_Storage.mdu", "c14_tabulated-profile-zw-storage")]
        [TestCase(@"f105_cross-sections\c16_YZ-profile_lumped\Lumped_YZ_flow_area.mdu", "c16_YZ-profile_lumped")]
        [Category("Quarantine")]
        public void WaterFlowFMModel_CompareReadWriteTest(string relativeMduFilePath, string testname)
        {
            var mduPathTestBench = Path.Combine(PathTestBench,relativeMduFilePath);
            var mduPathToWrite = relativeMduFilePath + "";
            var model = WaterFlowFMModelReader.Read(mduPathTestBench);
            model.ExportTo(mduPathToWrite);
            CompareFiles(mduPathTestBench, mduPathToWrite, testname);
        }

        #endregion

        #region Helpers

        private void CompareFiles(string mduPathTestBench, string mduPathToWrite, string testname)
        {
            var sourceFiles = Directory.GetFiles(mduPathTestBench);
            var targetFiles = Directory.GetFiles(mduPathToWrite);

            //Assert.AreEqual(sourceFiles.Length, targetFiles.Length, String.Format("{0}: number of source files({1}) and target files({2}) are not the same.",testname , sourceFiles.Length, targetFiles.Length));

        }

        #endregion

    }
}
