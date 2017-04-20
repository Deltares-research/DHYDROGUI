using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Plugins.DelftModels.HydroModel;
using log4net.Config;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    public class FewsAdapterHKTGTest : FewsAdapterTestBase
    {
        [SetUp]
        public void SetUpFixture()
        {
            XmlConfigurator.Configure();
        }

        /// <summary>
        /// Extract FEWS info for Heel Klein Test Geval
        /// </summary>
        [Test]
        [Ignore]  // all OpenDa, Fews and OpenMI tests are ignored
        [Category(TestCategory.Integration)]
        [Category(TestCategory.BackwardCompatibility)]
        public void GenerateFewsInfoForHKTG()
        {
            // setup                           
            const string directoryName = "InfoForHKTG";
            string testRunDir = CopySourceTestDataIntoTestFolder("HKTG", directoryName);
            string dsProjFilePath = Path.Combine(testRunDir, "DSModel/HeelKleinTestGeval_TC.dsproj");
            string csvFilePath = Path.Combine(testRunDir, "test.csv");
            string nodeShapeFilePath = Path.Combine(testRunDir, "test_node_data_items.shp");
            string lineShapeFilePath = Path.Combine(testRunDir, "test_line_data_items.shp");
            string profilesOnNodesFilePath = Path.Combine(testRunDir, "profile_definition_route_1_grid_points.xml");
            string profilesOnLinesFilePath = Path.Combine(testRunDir, "profile_definition_route_1_reach_segments.xml");

            using (DeltaShellApplication app = GetRunningDSApplication())
            {
                app.OpenProject(dsProjFilePath);
                var fewsAdapter = new FewsAdapter(app);
                LogHelper.SetLoggingLevel(Level.Debug);
                var model = app.Project.RootFolder.Models.OfType<HydroModel>().FirstOrDefault();
                fewsAdapter.ExportAll(csvFilePath, model);
                LogHelper.SetLoggingLevel(Level.Info);
            }

            AssertThatFileExists(csvFilePath);
            AssertThatFileExists(nodeShapeFilePath);
            AssertThatFileExists(lineShapeFilePath);
            AssertThatFileExists(profilesOnNodesFilePath);
            AssertThatFileExists(profilesOnLinesFilePath);

            // Check CSV
            string content = File.ReadAllText(csvFilePath);
            Assert.IsTrue(content.Contains("Input,,wind,wind_velocity,global,wind,NaN,NaN"), "Check content line a");
            Assert.IsTrue(content.Contains("Output,,Water level,water_level,R_3_15,grid_point,85499.7,452000"), "Check content line b");
            Assert.IsTrue(content.Contains("Output,,Water depth,water_depth,R_3_48,grid_point,88900.02,452000"), "Check content line c");
            Assert.IsTrue(content.Contains("Output,,Discharge,water_discharge,R_3-1649.67,reach_segment,85649.67,452000"), "Check content line d");
            Assert.IsTrue(content.Contains("Output,,Velocity,water_velocity,R_3-4950.01,reach_segment,88950.01,452000"), "Check content line e");
            Assert.IsTrue(content.Contains("Output,,Flow area,water_flow_area,R_3-2650.47,reach_segment,86650.47,452000"), "Check content line f");
            Assert.IsTrue(content.Contains("Input,,N_0 - Salinity concentration time series,water_salinity,N_0,HydroNode,84000,452000"), "Check content line g");
            Assert.IsTrue(content.Contains("Input,,N_1 - Salinity concentration time series,water_salinity,N_1,HydroNode,89000,452000"), "Check content line h");
            Assert.IsTrue(content.Contains("Output,,Discharge,water_discharge,R_3-4650.07,reach_segment,88650.07,452000"), "Check content line i");
            Assert.IsTrue(content.Contains("Input,,N_0 - Salinity concentration time series,water_salinity,N_0,HydroNode,84000,452000"), "Check content line j");
            Assert.IsTrue(content.Contains("Input,,N_1 - Salinity concentration time series,water_salinity,N_1,HydroNode,89000,452000"), "Check content line k");
            Assert.IsTrue(content.Contains("Input,,3 - Salt discharge concentration time series,water_salinity,3,LateralSource,85935.3972872368,452000"), "Check content line l");
            Assert.IsTrue(content.Contains("Input,,3 - Salt discharge load time series,salt_discharge,3,LateralSource,85935.3972872368,452000"), "Check content line m");
            Assert.IsTrue(content.Contains("Input,,Control group of S_10_##1 - Time series,rtc_timerule,Control group of S_10_##1,FakeRtcTimeSeriesFeature,NaN,NaN"), "Check content line n");
            Assert.IsTrue(content.Contains("Input,,N_0 - H(t),water_level,N_0,HBoundary,84000,452000"), "Check content line o");
            Assert.IsTrue(content.Contains("Input,,3 - Q(t),water_discharge,3,LateralSource,85935.3972872368,452000"), "Check content line p");

            ShapeFileReader nodeShapeFileReader = new ShapeFileReader(nodeShapeFilePath);
            var nodeFeatureCollection = nodeShapeFileReader.Read();
            Assert.IsNotNull(nodeFeatureCollection, "Check if node shape file could can be read");
            Assert.IsTrue(nodeFeatureCollection.Count() == 56, "Expected #items in node shape file");

            var lineShapeFileReader = new ShapeFileReader(lineShapeFilePath);
            var lineFeatureCollection = lineShapeFileReader.Read();
            Assert.IsNotNull(lineFeatureCollection, "Check if line shape file could can be read");
            Assert.IsTrue(lineFeatureCollection.Count() == 51, "Expected #items in line shape file");
        }
    }
}
