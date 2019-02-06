using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class HydroModelTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void HydroModelValidatesCurrentWorkflow()
        {
            var hydroModel = new HydroModel();
            hydroModel.CurrentWorkflow = null;

            var result = hydroModel.Validate();
            Assert.AreEqual(1, result.ErrorCount);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void HydroModelAddsItsSelfToIHydroModelWorkFlow()
        {
            var mocks = new MockRepository();
            var hydroModelWorkFlow = mocks.Stub<IHydroModelWorkFlow>();

            Expect.Call(hydroModelWorkFlow.Activities).Return(new EventedList<IActivity>()).Repeat.Any();

            mocks.ReplayAll();

            var hydroModel = new HydroModel();
            hydroModel.Workflows.Add(hydroModelWorkFlow);

            Assert.NotNull(hydroModelWorkFlow.HydroModel);
            
            hydroModel.Workflows.Remove(hydroModelWorkFlow);
            Assert.IsNull(hydroModelWorkFlow.HydroModel);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void AddingRegionsCreatesChildDataItems()
        {
            var hydroModel = new HydroModel();

            var subRegion = new HydroRegion();
            hydroModel.Region.SubRegions.Add(subRegion);

            // asserts
            hydroModel.GetDataItemByValue(hydroModel.Region).Children.Count
                .Should().Be.EqualTo(1);

            hydroModel.GetDataItemByValue(hydroModel.Region).Children.First().Value
                .Should().Be.EqualTo(subRegion);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void RemovingModelBreaksLinks()
        {
            var childModel = new SimpleHydroModel();

            var network = new HydroNetwork();
            var region = new HydroRegion { SubRegions = { network } };
            var hydroModel = new HydroModel { Region = region, Activities = { childModel } };

            var target = childModel.GetDataItemByValue(childModel.Region);
            var source = hydroModel.GetDataItemByValue(network);
            target.LinkTo(source);

            hydroModel.Activities.Clear();

            source.LinkedBy.Count.Should().Be.EqualTo(0);
        }

        private class TimeDepModel : TimeDependentModelBase
        {
            protected override void OnInitialize()
            {
            }

            protected override void OnExecute()
            {
                CurrentTime += TimeStep;
                if (CurrentTime >= StopTime)
                {
                    Status = ActivityStatus.Done;
                }
            }
        }

        [Test]
        public void TestOutputIsEmpty()
        {
            var hydroModel = new HydroModel();
            Assert.IsTrue(hydroModel.OutputIsEmpty, "No models, so no output");

            // Add child model without output:
            var simpleModel = new SimpleModel();
            hydroModel.Activities.Add(simpleModel);
            Assert.IsTrue(hydroModel.OutputIsEmpty, "1 empty model, so no output");

            // Run child model:
            simpleModel.Initialize();
            simpleModel.Execute();
            simpleModel.Finish();
            Assert.IsFalse(hydroModel.OutputIsEmpty, "1 model with output");
            //Submodel should also have output.
            Assert.IsFalse(simpleModel.OutputIsEmpty, "1 model with output");
            
            // Add child model without output:
            hydroModel.Activities.Add(new SimpleModel());
            Assert.IsFalse(hydroModel.OutputIsEmpty, "1 empty model and 1 model with output, so Integrated model has output");

            //Clear output from the parent
            hydroModel.ClearOutput();
            Assert.IsTrue(hydroModel.OutputIsEmpty);
            Assert.IsTrue(simpleModel.OutputIsEmpty);
        }

        [Test]
        public void TestOutputIsNotEmptyForCompositeModelAfterRunActivity()
        {
            /*Sobek3-848*/
            var hydroModel = new CompositeModel();
            var simpleModel = new SimpleModel();
            hydroModel.Activities.Add(simpleModel);
            Assert.IsTrue(hydroModel.OutputIsEmpty);

            ActivityRunner.RunActivity(hydroModel);
            Assert.AreEqual(hydroModel.Status, ActivityStatus.Cleaned);
            Assert.IsFalse(hydroModel.OutputIsEmpty);
        }

        [Test]
        public void TestRunUsingSimpleModel2_FailsValidation()
        {
            var sharedNameOfModels = "SimpleModel";

            var m1 = new SimpleModel { Input = 1, Name = sharedNameOfModels };
            var m2 = new SimpleModel { Input = 2, Name = sharedNameOfModels };

            var workflow = new ParallelActivity { Activities = { new ActivityWrapper { Activity = m1 }, new ActivityWrapper { Activity = m2 } } };

            var hydroModel = new HydroModel
            {
                Activities = { m1, m2 },
                Workflows = { workflow },
                CurrentWorkflow = workflow
            };

            // run
            hydroModel.Initialize();
            Assert.AreEqual(ActivityStatus.Failed, hydroModel.Status);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void RunUsingSimpleModel2()
        {
            var m1 = new SimpleModel { Input = 1, Name = "SimpleModel1"};
            var m2 = new SimpleModel { Input = 2, Name = "SimpleModel2"};

            var workflow = new ParallelActivity { Activities = { new ActivityWrapper { Activity = m1 }, new ActivityWrapper { Activity = m2 } } };

            var hydroModel = new HydroModel
            {
                Activities = { m1, m2 },
                Workflows = { workflow },
                CurrentWorkflow = workflow
            };

            // run
            hydroModel.Initialize();
            Assert.AreEqual(ActivityStatus.Initialized, hydroModel.Status);
            hydroModel.Execute();
            hydroModel.Finish();
            hydroModel.Cleanup();

            // asserts
            m1.Output.Should().Be.EqualTo(1);
            m2.Output.Should().Be.EqualTo(2);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GetCompositeWorkFlowDataForHydroModelWorkFlows()
        {
            var mocks = new MockRepository();
            var hydroModelWorkFlow1 = mocks.Stub<IHydroModelWorkFlow>();
            var hydroModelWorkFlow2 = mocks.Stub<IHydroModelWorkFlow>();
            var hydroModelWorkFlow3 = mocks.Stub<IHydroModelWorkFlow>();
            var hydroModelWorkFlow4 = mocks.Stub<IHydroModelWorkFlow>();

            var hydroModelWorkFlowData1 = mocks.Stub<IHydroModelWorkFlowData>();
            var hydroModelWorkFlowData2 = mocks.Stub<IHydroModelWorkFlowData>();
            var hydroModelWorkFlowData3 = mocks.Stub<IHydroModelWorkFlowData>();
            var hydroModelWorkFlowData4 = mocks.Stub<IHydroModelWorkFlowData>();

            hydroModelWorkFlow1.Expect(wf => wf.Activities).Return(new EventedList<IActivity>()).Repeat.Any();
            hydroModelWorkFlow2.Expect(wf => wf.Activities).Return(new EventedList<IActivity>()).Repeat.Any();
            hydroModelWorkFlow3.Expect(wf => wf.Activities).Return(new EventedList<IActivity>()).Repeat.Any();
            hydroModelWorkFlow4.Expect(wf => wf.Activities).Return(new EventedList<IActivity>()).Repeat.Any();

            hydroModelWorkFlow1.Data = hydroModelWorkFlowData1;
            hydroModelWorkFlow2.Data = hydroModelWorkFlowData2;
            hydroModelWorkFlow3.Data = hydroModelWorkFlowData3;
            hydroModelWorkFlow4.Data = hydroModelWorkFlowData4;

            mocks.ReplayAll();

            var hydroModel = new HydroModel();
            var workFlow = new SequentialActivity
                {
                    Activities =
                        {
                            hydroModelWorkFlow1,
                            new ParallelActivity
                                {
                                    Activities =
                                        {
                                            hydroModelWorkFlow2,
                                            hydroModelWorkFlow3,
                                            new SequentialActivity
                                                {
                                                    Activities = {hydroModelWorkFlow4}
                                                }
                                        }
                                }
                        }
                };

            hydroModel.Workflows.Add(workFlow);
            hydroModel.CurrentWorkflow = workFlow;

            var lookUp = hydroModel.CurrentWorkFlowData.HydroModelWorkFlowDataLookUp;

            Assert.AreEqual(4, lookUp.Count);
            Assert.AreEqual(new[] { 0 }, lookUp[hydroModelWorkFlowData1]);
            Assert.AreEqual(new[] { 1, 0 }, lookUp[hydroModelWorkFlowData2]);
            Assert.AreEqual(new[] { 1, 1 }, lookUp[hydroModelWorkFlowData3]);
            Assert.AreEqual(new[] { 1, 2, 0 }, lookUp[hydroModelWorkFlowData4]);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void SetCompositeWorkFlowDataForHydroModelWorkFlows()
        {
            var mocks = new MockRepository();
            var hydroModelWorkFlow1 = mocks.Stub<IHydroModelWorkFlow>();
            var hydroModelWorkFlow2 = mocks.Stub<IHydroModelWorkFlow>();
            var hydroModelWorkFlow3 = mocks.Stub<IHydroModelWorkFlow>();
            var hydroModelWorkFlow4 = mocks.Stub<IHydroModelWorkFlow>();

            var hydroModelWorkFlowData1 = mocks.Stub<IHydroModelWorkFlowData>();
            var hydroModelWorkFlowData2 = mocks.Stub<IHydroModelWorkFlowData>();
            var hydroModelWorkFlowData3 = mocks.Stub<IHydroModelWorkFlowData>();
            var hydroModelWorkFlowData4 = mocks.Stub<IHydroModelWorkFlowData>();

            hydroModelWorkFlow1.Expect(wf => wf.Activities).Return(new EventedList<IActivity>()).Repeat.Any();
            hydroModelWorkFlow2.Expect(wf => wf.Activities).Return(new EventedList<IActivity>()).Repeat.Any();
            hydroModelWorkFlow3.Expect(wf => wf.Activities).Return(new EventedList<IActivity>()).Repeat.Any();
            hydroModelWorkFlow4.Expect(wf => wf.Activities).Return(new EventedList<IActivity>()).Repeat.Any();

            mocks.ReplayAll();

            var hydroModel = new HydroModel();
            var workFlow = new SequentialActivity
            {
                Activities =
                        {
                            hydroModelWorkFlow1,
                            new ParallelActivity
                                {
                                    Activities =
                                        {
                                            hydroModelWorkFlow2,
                                            hydroModelWorkFlow3,
                                            new SequentialActivity
                                                {
                                                    Activities = {hydroModelWorkFlow4}
                                                }
                                        }
                                }
                        }
            };

            hydroModel.Workflows.Add(workFlow);
            hydroModel.CurrentWorkflow = workFlow;

            Assert.IsNull(hydroModelWorkFlow1.Data);
            Assert.IsNull(hydroModelWorkFlow2.Data);
            Assert.IsNull(hydroModelWorkFlow3.Data);
            Assert.IsNull(hydroModelWorkFlow4.Data);

            var compositeHydroModelWorkFlowData = new CompositeHydroModelWorkFlowData
                {
                    HydroModelWorkFlowDataLookUp = new Dictionary<IHydroModelWorkFlowData, IList<int>>
                        {
                            {hydroModelWorkFlowData1, new List<int>(new[] {0})},
                            {hydroModelWorkFlowData2, new List<int>(new[] {1, 0})},
                            {hydroModelWorkFlowData3, new List<int>(new[] {1, 1})},
                            {hydroModelWorkFlowData4, new List<int>(new[] {1, 2, 0})}
                        }
                };

            hydroModel.CurrentWorkFlowData = compositeHydroModelWorkFlowData;

            Assert.AreEqual(hydroModelWorkFlow1.Data, hydroModelWorkFlowData1);
            Assert.AreEqual(hydroModelWorkFlow2.Data, hydroModelWorkFlowData2);
            Assert.AreEqual(hydroModelWorkFlow3.Data, hydroModelWorkFlowData3);
            Assert.AreEqual(hydroModelWorkFlow4.Data, hydroModelWorkFlowData4);
        }

        public class SimpleHydroModel : ModelBase, IHydroModel
        {
            public SimpleHydroModel()
            {
                DataItems.Add(new DataItem(new HydroNetwork(), "network", SupportedRegionType, DataItemRole.Input, "network"));
            }

            protected override void OnInitialize()
            {
            }

            protected override void OnExecute()
            {
                Status = ActivityStatus.Done;
            }

            public IHydroRegion Region { get { return (IHydroRegion) DataItems.First(di => di.ValueType == SupportedRegionType).Value; } }

            public Type SupportedRegionType { get { return typeof (HydroNetwork); } }
        }

        public class SimpleModel : TimeDependentModelBase
        {
            public int Input { get; set; }

            public int Output { get; private set; }

            protected override void OnInitialize()
            {
                Output = 0;
            }

            protected override void OnExecute()
            {
                Output += Input;

                Status = ActivityStatus.Done;
            }
        }
    }
}
