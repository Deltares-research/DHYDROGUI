using System.Linq;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using log4net;
using log4net.Core;
using NUnit.Framework;
using SharpTestsEx;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class HydroModelBuilderIntegrationTest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroModelBuilderIntegrationTest));

        [Test]
        public void TestValueConvertersAreSetUpCorrectly()
        {
            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.All);
            hydroModel.CurrentWorkflow = hydroModel.Workflows[3]; // (rr + flow + rtc)

            var flowModel = hydroModel.Models.OfType<WaterFlowModel1D>().First();
            var rrModel = hydroModel.Models.OfType<RainfallRunoffModel>().First();

            var inflowsDataItem = flowModel.GetDataItemByValue(flowModel.Inflows);
            var inputWaterLevelDataItem = rrModel.GetDataItemByValue(rrModel.InputWaterLevel);

            inflowsDataItem.Children.Count.Should("flow has child data item").Be.EqualTo(1);
            inputWaterLevelDataItem.Children.Count.Should("rr has child data item").Be.EqualTo(1);
        }

        [Test]
        public void TestRRIsNotLinkedToFlowIfSetUpSequentially()
        {
            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.All);

            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

            hydroModel.CurrentWorkflow = new SequentialActivity {Activities = {rr, flow}};

            Assert.IsFalse(rr.InputWaterLevelIsLinked, "not linked for sequential runs");
            Assert.IsTrue(flow.GetDataItemByValue(flow.Inflows).Children.Any(), "rr -> flow");

            hydroModel.CurrentWorkflow = new ParallelActivity {Activities = {rr, flow}};

            Assert.IsTrue(rr.InputWaterLevelIsLinked, "linked for simultaneous runs");
            Assert.IsTrue(flow.GetDataItemByValue(flow.Inflows).Children.Any(), "rr -> flow");

            hydroModel.CurrentWorkflow = new SequentialActivity { Activities = { rr, flow } };

            Assert.IsFalse(rr.InputWaterLevelIsLinked, "not linked for sequential runs");
            Assert.IsTrue(flow.GetDataItemByValue(flow.Inflows).Children.Any(), "rr -> flow");
        }
        
        [Test]
        [Category(TestCategory.Slow)]
        public void ActivitiesRunSimultaneous()
        {
            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.SobekModels);

            hydroModel.CurrentWorkflow = hydroModel.Workflows[3]; // (rr + flow + rtc)
            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

            Assert.IsTrue(hydroModel.GetActivitiesRunningSimultaneous(flow).Contains(rr));

            hydroModel.CurrentWorkflow = new SequentialActivity { Activities = { rr, flow } };

            Assert.IsFalse(hydroModel.GetActivitiesRunningSimultaneous(flow).Contains(rr));
        }

        [Test]
        public void RemovingAndAddingModelUpdatesLinks()
        {
            var builder = new HydroModelBuilder();
            var hydroModel = builder.BuildModel(ModelGroup.All);

            hydroModel.CurrentWorkflow = hydroModel.Workflows[3]; // (rr + flow + rtc)
            var rr = hydroModel.Activities.OfType<RainfallRunoffModel>().First();
            var flow = hydroModel.Activities.OfType<WaterFlowModel1D>().First();
            
            Assert.IsTrue(rr.InputWaterLevelIsLinked, "linked");

            hydroModel.Activities.Remove(flow);

            Assert.IsFalse(rr.InputWaterLevelIsLinked, "no longer linked");

            hydroModel.Activities.Add(flow);
            hydroModel.CurrentWorkflow = hydroModel.Workflows[3]; // (rr + flow + rtc)

            Assert.IsTrue(rr.InputWaterLevelIsLinked, "linked");
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void AutoAddSubRegion()
        {
            var hydroModel = new HydroModel();
            
            var flowModel = new WaterFlowModel1D();
            hydroModel.Activities.Add(flowModel);
            hydroModel.AutoAddRequiredLinks(flowModel);

            hydroModel.Region.SubRegions.Count.Should("network is added under hydro model").Be.EqualTo(1);
            flowModel.GetDataItemByValue(flowModel.Network).LinkedTo.Should("flow model network is linked").Not.Be.Null();
        }


        [Test]
        [Category(TestCategory.Slow)]
        public void LinkHydroArea()
        {
            var hydroModel = new HydroModel();
            var fmModel = new WaterFlowFMModel();
            hydroModel.Activities.Add(fmModel);
            hydroModel.AutoAddRequiredLinks(fmModel);

            var dataItem = fmModel.GetDataItemByValue(fmModel.Area);
            dataItem.LinkedTo.Should("FM area is linked").Not.Be.Null();
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void BuildEmptyModelContainingAllSupportedModels()
        {
            var builder = new HydroModelBuilder();
            var model = builder.BuildModel(ModelGroup.All);

            LogHelper.ConfigureLogging(Level.Debug);
            LogModel(model);
            LogHelper.ResetLogging();

            model.Name.Should().Be.EqualTo("Integrated Model");
            model.Workflows.Count.Should().Be.GreaterThan(9);
        }

        /// <summary>
        /// TODO: move it into HydroModel.ToString()?
        /// </summary>
        /// <param name="model"></param>
        private static void LogModel(HydroModel model)
        {
            log.DebugFormat("HydroModel:" + model);

            log.DebugFormat("");
            log.DebugFormat("Activities:");
            foreach (var activity in model.Activities)
            {
                log.DebugFormat("  " + activity);
            }

            log.DebugFormat("");
            log.DebugFormat("Workflows:");
            foreach (var workflow in model.Workflows)
            {
                log.DebugFormat(" " + workflow + ":");

                foreach (var activity in workflow.Activities)
                {
                    log.DebugFormat("    " + activity);

                    if (activity is CompositeActivity) // show 1 level deep only
                    {
                        foreach (var activity2 in ((CompositeActivity)activity).Activities)
                        {
                            log.DebugFormat("      " + activity2);
                        }
                    }
                }
            }
        }
    }
}