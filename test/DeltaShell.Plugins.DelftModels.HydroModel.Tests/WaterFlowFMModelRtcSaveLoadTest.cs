using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests {
    [TestFixture]
    public class WaterFlowFMModelRtcSaveLoadTest
    {
        [Test]
        [Ignore("Jira issue FM1D2D-1333 should resolve this issue")]
        public void SaveLoadFlowFMRtc()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                var projectPath = Path.Combine(tempDir,"RtcFM.dsproj");
                var pluginsToAdd = new List<IPlugin>()
                {
                    new NHibernateDaoApplicationPlugin(),
                    new CommonToolsApplicationPlugin(),
                    new SharpMapGisApplicationPlugin(),
                    new FlowFMApplicationPlugin(),
                    new HydroModelApplicationPlugin(),
                    new NetworkEditorApplicationPlugin(),
                    new RealTimeControlApplicationPlugin(),
                };
                using (var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build())
                {
                    app.Run();
                    app.SaveProjectAs(projectPath); // to initialize file repository

                    //setup models
                    var flowFmModel = new WaterFlowFMModel();
                    HydroNetworkHelper.AddSnakeHydroNetwork(flowFmModel.Network,
                                                            new[]
                                                            {
                                                                new Point(0, 0),
                                                                new Point(1, 1),
                                                                new Point(2, 4)
                                                            });
                    var branch = flowFmModel.Network.Branches.First();
                    var weir = new Weir("weir01")
                    {
                        Geometry = new Point(0.2, 0.2),
                        Chainage = 0.2
                    };
                    branch.BranchFeatures.Add(weir);
                    var weir2 = new Weir("weir02")
                    {
                        Geometry = new Point(0.6, 0.6),
                        Chainage = 0.6
                    };
                    branch.BranchFeatures.Add(weir2);
                    app.Project.RootFolder.Add(flowFmModel);

                    var rtcModel = new RealTimeControlModel("Real-Time Control");
                    Assert.That(rtcModel, Is.Not.Null);
                    var cg = new ControlGroup();
                    cg.Name = "Control Group 1";
                    rtcModel.ControlGroups.Add(cg);

                    var hydroModel = new HydroModel();
                    app.Project.RootFolder.Add(hydroModel);
                    flowFmModel.MoveModelIntoIntegratedModel(app.Project.RootFolder, hydroModel);
                    hydroModel.Activities.Add(rtcModel);

                    hydroModel.StartTime = new DateTime(2001, 1, 1);
                    hydroModel.StopTime = new DateTime(2001, 4, 11);
                    hydroModel.TimeStep = new TimeSpan(0, 0, 5, 0);

                    //link timerules to weirs
                    CreateAndLinkTimeRulesForControlGroup(hydroModel, cg, rtcModel, flowFmModel);
                    app.SaveProject();
                    app.CloseProject();
                    Assert.DoesNotThrow(() => app.OpenProject(projectPath));
                }
            });
        }
        private static void CreateAndLinkTimeRulesForControlGroup(IHydroModel hydroModel, IControlGroup cg, IModel rtcModel, IModel flowFmModel)
        {
            var hydroNetwork = hydroModel.Region.SubRegions.OfType<HydroNetwork>().FirstOrDefault();
            var weir1 = hydroNetwork.Weirs.FirstOrDefault(w => w.Name == "weir01");
            Assert.That(weir1, Is.Not.Null);

            var weir2 = hydroNetwork.Weirs.FirstOrDefault(w => w.Name == "weir02");
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

    }
}