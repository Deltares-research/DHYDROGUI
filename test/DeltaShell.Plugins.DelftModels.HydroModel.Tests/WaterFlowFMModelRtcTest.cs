using System;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class WaterFlowFMModelRtcTest
    {
        [Test]
        [Category(TestCategory.Integration),Category(TestCategory.WorkInProgress)]
        public void RunFlowFMRtc()
        {
            LogHelper.ConfigureLogging(Level.Debug);
            var projectPath = TestHelper.GetTestFilePath(@"RtcFM\RtcFM.dsproj");
            projectPath = TestHelper.CreateLocalCopy(projectPath);
            
            var mduPath = TestHelper.GetTestFilePath(@"RtcFM\FlowFM\FlowFM.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Run();
                app.SaveProjectAs(projectPath); // to initialize file repository

                //setup models
                var flowFmModel = new WaterFlowFMModel(mduPath);
                app.Project.RootFolder.Add(flowFmModel);
                flowFmModel.ReloadGrid();
                flowFmModel.UseRPC = false;

                var rtcModel = new RealTimeControlModel("Real-Time Control");
                Assert.That(rtcModel, Is.Not.Null);
                var cg = new ControlGroup();
                cg.Name = "Control Group 1";
                rtcModel.ControlGroups.Add(cg);

                var hydroModel = new HydroModel();
                app.Project.RootFolder.Add(hydroModel);
                flowFmModel.MoveModelIntoIntegratedModel(app.Project.RootFolder,hydroModel);
                hydroModel.Activities.Add(rtcModel);

                hydroModel.StartTime = new DateTime(2001, 1, 1);
                hydroModel.StopTime = new DateTime(2001, 4, 11);
                hydroModel.TimeStep = new TimeSpan(0, 0, 5, 0);

                //link timerules to weirs
                CreateAndLinkTimeRulesForControlGroup(hydroModel, cg, rtcModel, flowFmModel);
                app.SaveProject();
                
                // run
                ActivityRunner.RunActivity(hydroModel);
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                app.CloseProject();
            }
        }

        private static void CreateAndLinkTimeRulesForControlGroup(IHydroModel hydroModel, IControlGroup cg, IModel rtcModel, IModel flowFmModel)
        {
            var area = hydroModel.Region.SubRegions.OfType<HydroArea>().FirstOrDefault();
            var weir1 = area.Weirs.FirstOrDefault(w => w.Name == "weir01");
            Assert.That(weir1, Is.Not.Null);

            var weir2 = area.Weirs.FirstOrDefault(w => w.Name == "weir02");
            Assert.That(weir2, Is.Not.Null);

            var timeRule1 = new TimeRule();
            timeRule1.TimeSeries[new DateTime(2001, 1, 1)] = 5.0;
            timeRule1.TimeSeries[new DateTime(2001, 4, 11)] = 0.0;
            timeRule1.InterpolationOptionsTime = InterpolationType.Linear;
            timeRule1.Periodicity = ExtrapolationType.Constant;
            timeRule1.Name = "rule01";

            var outputWeir1CrestLevel = new Output();
            cg.Rules.Add(timeRule1);
            cg.Outputs.Add(outputWeir1CrestLevel);
            timeRule1.Outputs.Add(outputWeir1CrestLevel);

            // link output
            var outputDataItem1 = rtcModel.GetDataItemByValue(outputWeir1CrestLevel);
            var weir1DataItem = flowFmModel.GetChildDataItems(weir1).First(di => (di.Role & DataItemRole.Input) > 0);
            weir1DataItem.LinkTo(outputDataItem1);

            var timeRule2 = new TimeRule();
            timeRule2.TimeSeries[new DateTime(2001, 1, 1)] = -20.0;
            timeRule2.TimeSeries[new DateTime(2001, 4, 11)] = 0.0;
            timeRule2.InterpolationOptionsTime = InterpolationType.Linear;
            timeRule2.Periodicity = ExtrapolationType.Constant;
            timeRule2.Name = "rule02";

            var outputWeir2CrestLevel = new Output();
            cg.Rules.Add(timeRule2);
            cg.Outputs.Add(outputWeir2CrestLevel);
            timeRule2.Outputs.Add(outputWeir2CrestLevel);

            // link output
            var outputDataItem2 = rtcModel.GetDataItemByValue(outputWeir2CrestLevel);
            var weir2DataItem = flowFmModel.GetChildDataItems(weir2).First(di => (di.Role & DataItemRole.Input) > 0);
            weir2DataItem.LinkTo(outputDataItem2);
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        public void RunHarlingenRtc()
        {
            var projectPath = TestHelper.GetTestFilePath(@"rtc\rtc.dsproj");
            projectPath = TestHelper.CopyProjectToLocalDirectory(projectPath);


            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Run();

                app.OpenProject(projectPath);
                
                var retrievedModel = (HydroModel)app.Project.RootFolder.Items[0];
                var integratedArea = retrievedModel.Region.SubRegions.OfType<HydroArea>().First();
                var flowfmArea = retrievedModel.Activities.OfType<WaterFlowFMModel>().First().DataItems.First(di => di.ValueType == typeof(HydroArea));
                Assert.AreEqual(integratedArea, flowfmArea.LinkedTo);

                var fmModel = (WaterFlowFMModel) retrievedModel.Models.First(m => m is WaterFlowFMModel);
                var weir02 = fmModel.Area.Weirs.First(w => w.Name == "weir02");
                var obsPoint = fmModel.Area.ObservationPoints.First(w => w.Name == "party");

                bool sal20passed = false, sal5passed = false;
                bool sal5ButTooSmallCrestLevel = false, sal20ButNotCrestMinus3 = false;
                double previousSalinity = double.NaN;
                // check the crest level of the weir and see that it changes over time, because the source&sink blows fresh water into the harbor.
                fmModel.AfterExecute += (o, e) =>
                    {
                        double crestLevel = fmModel.GetValueFromModelApi(weir02, KnownStructureProperties.CrestLevel);

                        if (previousSalinity != double.NaN)
                        {
                            Console.WriteLine("AfterExecute. Salinity t-1: {0}. Crest level: {1}", previousSalinity, crestLevel);

                            if (previousSalinity == 20.0)
                            {
                                sal20passed = true;

                                if (crestLevel != -3.0)
                                {
                                    sal20ButNotCrestMinus3 = true;
                                }
                            }

                            if (previousSalinity < 5.0)
                            {
                                sal5passed = true;

                                // make sure that the crest level is interpolated when the salinity changes in the observation point
                                // use a bool and no assert, because the asserts are caught by the activity runner
                                if (crestLevel <= 6.0)
                                    sal5ButTooSmallCrestLevel = true;
                            }

                            // remember salinity for the next time step
                            previousSalinity = fmModel.GetValueFromModelApi(obsPoint, "salinity");
                        }
                    };

                ActivityRunner.RunActivity(retrievedModel);

                Assert.IsTrue(sal20passed);
                Assert.IsTrue(sal5passed);
                Assert.IsFalse(sal5ButTooSmallCrestLevel);
                Assert.IsFalse(sal20ButNotCrestMinus3, "It appears that there was a time step where the salinity was 20, but the crest level was not -3");
            }
        }

        
    }
}
