using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.ModelExchange.Queries;
using DelftTools.TestUtils;
using Deltares.IO.FewsPI;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using log4net.Config;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.Fews.Tests
{
    [TestFixture]
    public class FewsAdapterTest : FewsAdapterTestBase
    {
        private DeltaShellApplication app;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            BasicConfigurator.Configure();
        }

        [SetUp]
        public void SetUp()
        {
            CheckWorkingDirectoryForTestRuns(this);
        }

        [TearDown]
        public void TearDown()
        {
            CheckWorkingDirectoryForTestRuns(this);
        }

        [Test]
        [Ignore]  // all OpenDa, Fews and OpenMI tests are ignored
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void ExportToCsvFileLauwersmeerFileCreated()
        {
            // setup
            const string directoryName = "CsvFileCreated";
            var fileName = CopySourceTestDataIntoTestFolder("LWM", directoryName) + @"\Input\pi-run.xml";

            using (app = GetRunningDSApplication())
            {
                var fewsAdapter = new FewsAdapter(app);
                fewsAdapter.ExportCsvFile(fileName, null);
            }

            //check
            var csvFile = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(fileName)), @"fews_ds_Lauwersmeermodel_model_data_items.csv");
            string content = File.ReadAllText(csvFile);
            Assert.IsTrue(content.Contains("HBoundary"), "The output csv file does not contain expected (snippet) data");
            Assert.IsTrue(content.Contains("QBoundary"), "The output csv file does not contain expected (snippet) data");
            Assert.IsTrue(content.Contains("Output,,"), "The output csv file does not contain output items");
            Assert.IsTrue(content.Contains("grid_point,"), "The output csv file does not contain grid point locations");
            Assert.IsTrue(content.Contains("reach_segment,"), "The output csv file does not contain reach_segment locations");
        }

        [Test]
        [Ignore]  // all OpenDa, Fews and OpenMI tests are ignored
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ExportToShapeFileLauwersmeerFilesCreated()
        {
            LogHelper.ConfigureLogging(Level.Debug);
            // setup
            const string directoryName = "ShapeFilesCreated";
            var fileName = CopySourceTestDataIntoTestFolder("LWM", directoryName) + @"\Input\pi-run.xml";

            using (app = GetRunningDSApplication())
            {
                var fewsAdapter = new FewsAdapter(app);
                fewsAdapter.ExportShapeFile(fileName, null);
            }

            RunInfo runInfo = new RunInfo(fileName);
            string expectedFile1 = Path.Combine(runInfo.WorkingDirectory, "fews_ds_Lauwersmeermodel_node_data_items.shp");
            string expectedFile2 = Path.Combine(runInfo.WorkingDirectory, "fews_ds_Lauwersmeermodel_line_data_items.shp");

            AssertThatFileExists(expectedFile1);
            AssertThatFileExists(expectedFile2);
        }
        
        [Test]
        [Ignore]  // all OpenDa, Fews and OpenMI tests are ignored
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GetAllOuputParametersLauwersmeerModelShouldHaveTimeSeriesDataValues()
        {
            // setup
            using (app = GetRunningDSApplication())
            {
                const string testFolder = "ShouldHaveTimeSeriesDataValues";
                string path = CopySourceTestDataIntoTestFolder("LWM", testFolder) + @"\DSModel\Lauwersmeer.dsproj";
                app.OpenProject(path);

                var model = app.Project.RootFolder.Models.ElementAt(0) as WaterFlowModel1D;
                Assert.IsNotNull(model);

                model.Initialize();
                model.Execute();

                ExtendedQueryContext context = new ExtendedQueryContext(app.Project);

                // call
                try
                {
                    IEnumerable<AggregationResult> queryResults = context.GetAllOuputParameters()
                        .Where(result => result.LocationType != FunctionAttributes.StandardFeatureNames.ReachSegment);

                    foreach (AggregationResult queryResult in queryResults)
                    {
                        foreach (var tuple in queryResult.TimeSeriesIterator())
                        {
                            Console.WriteLine("Time: {0} Value: {1}", tuple.First.ToShortTimeString(), tuple.Second);
                        }
                    }
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }

                app.CloseProject();
            }
        }


        [Test]
        [Ignore]  // all OpenDa, Fews and OpenMI tests are ignored
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void GetAllLauwersmeerModelShouldNotReturnDuplicateItems()
        {
            LogHelper.ConfigureLogging(Level.Debug);
            // setup

            using (app = GetRunningDSApplication())
            {
                WaterFlowModel1D model = null;
                try
                {
                    const string testFolder = "ShouldNotReturnDuplicateItems";
                    string path = CopySourceTestDataIntoTestFolder("LWM", testFolder) + @"\DSModel\Lauwersmeer.dsproj";
                    app.OpenProject(path);

                    model = app.Project.RootFolder.Models.ElementAt(0) as WaterFlowModel1D;
                    Assert.IsNotNull(model);

                    model.UseSalt = true;
                    model.UseSaltInCalculation = true;
                    model.OutputSettings.GetEngineParameter(QuantityType.Salinity, ElementSet.GridpointsOnBranches).
                        AggregationOptions = AggregationOptions.Current;
                    model.OutputSettings.GetEngineParameter(QuantityType.Discharge, ElementSet.Observations).
                        AggregationOptions = AggregationOptions.Current;

                    model.Initialize();
                    // normally (flowModel1D.Status != ActivityStatus.Finished) is checked during execute
                    // we only need one timestep, just to add the observationpoint to a networkcoverage
                    model.Execute();

                    // call
                    ExtendedQueryContext context = new ExtendedQueryContext(app.Project);

                    IEnumerable<AggregationResult> queryResults = context.GetAll();

                    // check
                    Assert.IsNotNull(queryResults);

                    // check
                    var items = new HashSet<string>();
                    int resultCount = 0;
                    foreach (string value in queryResults.Select(queryResult => queryResult.ToString()))
                    {
                        if (items.Contains(value))
                        {
                            Console.WriteLine("Found duplicate: {0}", value);
                        }
                        items.Add(value);
                        resultCount++;
                    }

                    Assert.AreEqual(resultCount, items.Count);
                }
                finally
                {
                    if (model != null)
                    {
                        model.Cleanup();
                    }
                    app.CloseProject();
                }
            }
        }
    }
}