using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    public class RealTimeControlFlowRestartTests
    {
        /// <summary>
        /// Run RTC/Flow model that has not run yet, save the model, then run again.
        /// Compare results.
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.BackwardCompatibility)]
        [Category(TestCategory.WorkInProgress)] //backward compatibility for in between version (not 3.0)...remake the test? we're never going to support this
        public void RunModelTwice()
        {
            // setup runs
            string sourcePath = Path.Combine(TestHelper.GetTestDataDirectory(), "RtcFlow1DRestart");
            const string testRunDir = "StartModelTwice";
            FileUtils.CopyDirectory(sourcePath, testRunDir, ".svn");

            string dsProjPath = Path.Combine(testRunDir, "IntervalControllerStartRestart.dsproj");

            using (DeltaShellApplication app = GetRunningDSApplication())
            {
                app.OpenProject(dsProjPath);
                var hydroModel = app.Project.RootFolder.Models.OfType<HydroModel>().First();
                var rtcModel = hydroModel.Activities.OfType<RealTimeControlModel>().First();
                var flowModel = hydroModel.Activities.OfType<WaterFlowModel1D>().First();
                Assert.AreEqual(0, flowModel.OutputFlow.Time.Values.Count, "no initial results");

                app.RunActivity(hydroModel);
                Assert.AreEqual(25, flowModel.OutputFlow.Time.Values.Count, "initial result and 24 computed results");
                IFeatureCoverage crestlevelCoverageFirstRun = rtcModel.OutputFeatureCoverages.First(fc => fc.Name.Contains("Crest level (s)"));
                double[] crestLevelsFirstRun = crestlevelCoverageFirstRun.
                    Components[0].Values.OfType<double>().ToArray();
                INetworkLocation networkLocation = flowModel.NetworkDiscretization.Locations.Values.First(l => l.Name.Equals("1_75"));
                double[] waterLevelValuesFirstRun = flowModel.OutputWaterLevel.GetTimeSeries(networkLocation).GetValues().OfType<double>().ToArray();
                app.SaveProject();
                app.CloseProject();

                app.OpenProject(dsProjPath);
                hydroModel = app.Project.RootFolder.Models.OfType<HydroModel>().First();
                rtcModel = hydroModel.Activities.OfType<RealTimeControlModel>().First();
                flowModel = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

                app.RunActivity(hydroModel);
                IFeatureCoverage crestlevelCoverageSecondRun = rtcModel.OutputFeatureCoverages.First(fc => fc.Name.Contains("Crest level (s)"));
                double[] crestLevelsSecondRun = crestlevelCoverageSecondRun.
                    Components[0].Values.OfType<double>().ToArray();
                networkLocation = flowModel.NetworkDiscretization.Locations.Values.First(l => l.Name.Equals("1_75"));
                double[] waterLevelsSecondRun = flowModel.OutputWaterLevel.GetTimeSeries(networkLocation).GetValues().OfType<double>().ToArray();
                app.CloseProject();

                string wrongValues = "";
                for (int index = 0; index < crestLevelsFirstRun.Length; index++)
                {
                    double first = crestLevelsFirstRun[index];
                    double second = crestLevelsSecondRun[index];
                    if (Math.Abs(first - second) > 1e-6)
                    {
                        wrongValues += "CL(" + index + ") " + first + " != " + second + "\n";
                    }
                }
                for (int index = 0; index < waterLevelValuesFirstRun.Count(); index++)
                {
                    double first = waterLevelValuesFirstRun[index];
                    double second = waterLevelsSecondRun[index];
                    if (Math.Abs(first - second) > 1e-6)
                    {
                        wrongValues += "WL(" + index + ") " + first + " != " + second + "\n";
                    }
                }
                Assert.IsEmpty(wrongValues, "no differences expected between first and second run");
            }
        }

        private static DeltaShellApplication GetRunningDSApplication()
        {
            // make sure log4net is initialized
            var app = new DeltaShellApplication();
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new SobekImportApplicationPlugin());

            //app.Plugins.Add(new ScriptingPlugin());
            app.Run();
            return app;
        }
    }
}