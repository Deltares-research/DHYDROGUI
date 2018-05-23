using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DelftTools.Utils.Validation;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class HydroModelTest
    {
        [Test]
        public void HydroModelValidatesCurrentWorkflow()
        {
            var hydroModel = new HydroModel();
            hydroModel.CurrentWorkflow = null;

            var result = hydroModel.Validate();
            Assert.AreEqual(1, result.ErrorCount);
        }

        [Test]
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
        [NUnit.Framework.Category(TestCategory.Slow)]
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
        [NUnit.Framework.Category(TestCategory.Slow)]
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
        public void CheckIfSubModelStatusIsSetToNoneWhenRunningOnInitializeTest()
        {
            var mocks = new MockRepository();
            
            var dimrModel = mocks.DynamicMultiMock<IDimrModel>(typeof(IActivity));
            dimrModel.Expect(m => m.Status).PropertyBehavior();
            dimrModel.Expect(m => m.ExplicitWorkingDirectory).PropertyBehavior();
            dimrModel.Expect(m => m.RunsInIntegratedModel).PropertyBehavior();
            dimrModel.Expect(m => m.KernelDirectoryLocation).Return(".").Repeat.Any();
            dimrModel.Expect(m => m.DirectoryName).Return(".").Repeat.Any();
            dimrModel.Expect(m => m.Name).Return("dimrModel").Repeat.Any();
            dimrModel.Expect(m => m.ExporterType).Return(typeof(SimpleExporter)).Repeat.Any();
            Expect.Call(dimrModel.Validate()).Return(new ValidationReport("dimrmodel",new List<ValidationIssue>())).Repeat.Any();
            Expect.Call(dimrModel.GetExporterPath(Arg<string>.Is.Anything)).IgnoreArguments().Return(".").Repeat.Any();
            
            var activities = new EventedList<IActivity>() {dimrModel};
            
            mocks.ReplayAll();
            dimrModel.Status = ActivityStatus.Failed;
            dimrModel.RunsInIntegratedModel = true;
            dimrModel.ExplicitWorkingDirectory = ".";

            var hydroModel = new HydroModel();
            var workFlow = new SequentialActivity
            {
                Activities = activities
                        
            };

            hydroModel.Workflows.Add(workFlow);
            hydroModel.CurrentWorkflow = workFlow;
            Assert.That(dimrModel.Status, Is.EqualTo(ActivityStatus.Failed));
            TypeUtils.CallPrivateMethod<HydroModel>(hydroModel, "OnInitialize");
            Assert.That(dimrModel.Status, Is.EqualTo(ActivityStatus.None));
            mocks.VerifyAll();

            
        }

        //[Test]
        //public void CheckIfSubSubModelStatusIsSetToNoneWhenRunningOnInitializeTest()
        //{
        //    var mocks = new MockRepository();
        //    /*
        //    var dimrModel1 = mocks.DynamicMultiMock<IDimrModel>(typeof(ITimeDependentModel));
        //    //((ITimeDependentModel)dimrModel1).Expect(m => m.AllDataItems).Return(new List<IDataItem>() { new }).Repeat.Any();
        //    dimrModel1.Expect(m => m.Status).PropertyBehavior();
        //    dimrModel1.Expect(m => m.ExplicitWorkingDirectory).PropertyBehavior();
        //    dimrModel1.Expect(m => m.RunsInIntegratedModel).PropertyBehavior();
        //    dimrModel1.Expect(m => m.KernelDirectoryLocation).Return(".").Repeat.Any();
        //    dimrModel1.Expect(m => m.DirectoryName).Return(".").Repeat.Any();
        //    dimrModel1.Expect(m => m.Name).Return("dimrModel1").Repeat.Any();
        //    dimrModel1.Expect(m => m.ExporterType).Return(typeof(SimpleExporter)).Repeat.Any();
        //    Expect.Call(dimrModel1.Validate()).Return(new ValidationReport("dimrmodel", new List<ValidationIssue>())).Repeat.Any();
        //    Expect.Call(dimrModel1.GetExporterPath(Arg<string>.Is.Anything)).IgnoreArguments().Return(".").Repeat.Any();

        //    var dimrModel2 = mocks.DynamicMultiMock<IDimrModel>(typeof(ITimeDependentModel));
        //  //  ((ITimeDependentModel)dimrModel2).Expect(m => m.AllDataItems).Return(new List<IDataItem>()).Repeat.Any();
        //    dimrModel2.Expect(m => m.Status).PropertyBehavior();
        //    dimrModel2.Expect(m => m.ExplicitWorkingDirectory).PropertyBehavior();
        //    dimrModel2.Expect(m => m.RunsInIntegratedModel).PropertyBehavior();
        //    dimrModel2.Expect(m => m.KernelDirectoryLocation).Return(".").Repeat.Any();
        //    dimrModel2.Expect(m => m.DirectoryName).Return(".").Repeat.Any();
        //    dimrModel2.Expect(m => m.Name).Return("dimrModel2").Repeat.Any();
        //    dimrModel2.Expect(m => m.ExporterType).Return(typeof(SimpleExporter)).Repeat.Any();
        //    Expect.Call(dimrModel2.Validate()).Return(new ValidationReport("dimrmodel", new List<ValidationIssue>())).Repeat.Any();
        //    Expect.Call(dimrModel2.GetExporterPath(Arg<string>.Is.Anything)).IgnoreArguments().Return(".").Repeat.Any();

        //    //var iterativeModel  = new Iterative1D2DCoupler() {Flow1DModel = (ITimeDependentModel)dimrModel1, Flow2DModel = (ITimeDependentModel)dimrModel2};
        //    var iterativeModel  = new Iterative1D2DCoupler();
        //    TypeUtils.SetField<Iterative1D2DCoupler>(iterativeModel, "flow1DModel", (ITimeDependentModel)dimrModel1);
        //    TypeUtils.SetField<Iterative1D2DCoupler>(iterativeModel, "flow2DModel", (ITimeDependentModel)dimrModel2);
        //    var subActivities = new EventedList<IActivity> {new ActivityWrapper(dimrModel1), new ActivityWrapper(dimrModel2)};
        //    TypeUtils.SetPropertyValue(iterativeModel, "Activities", subActivities);

        //    //mocked1d2dCoupler.Expect(wf => wf.Activities).Return().Repeat.Any();
        //    //var iterativeModel = mocks.Stub<Iterative1D2DCoupler>();
        //    //iterativeModel.Expect(i => i.f).SetPropertyWithArgument().Return(dimrModel1 as ITimeDependentModel).Repeat.Any();
        //    //iterativeModel.Expect(i => i.Flow2DModel).Return(dimrModel2 as ITimeDependentModel).Repeat.Any();
        //    var activities = new EventedList<IActivity>() { iterativeModel };

        //    mocks.ReplayAll();
        //    dimrModel1.Status = ActivityStatus.Failed;
        //    dimrModel1.RunsInIntegratedModel = true;
        //    dimrModel1.ExplicitWorkingDirectory = ".";

        //    dimrModel2.Status = ActivityStatus.Failed;
        //    dimrModel2.RunsInIntegratedModel = true;
        //    dimrModel2.ExplicitWorkingDirectory = ".";
        //    */
        //    var mockedF1dModel = mocks.DynamicMultiMock<ITimeDependentModel>(typeof(IModel), typeof(IDimrModel));
        //    mockedF1dModel.Expect(m => m.AllDataItems).Return(new List<IDataItem>()).Repeat.Any();
        //    mockedF1dModel.Expect(m => m.Name).Return("dimrModel1").Repeat.Any();
        //    ((IDimrModel)mockedF1dModel).Expect(m => m.ShortName).Return("mockedF1dModel").Repeat.Any();
        //    ((IDimrModel)mockedF1dModel).Expect(m => m.Status).PropertyBehavior();
        //    ((IDimrModel)mockedF1dModel).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();
        //    ((IDimrModel)mockedF1dModel).Expect(m => m.ExplicitWorkingDirectory).PropertyBehavior();
        //    ((IDimrModel)mockedF1dModel).Expect(m => m.RunsInIntegratedModel).PropertyBehavior();
        //    ((IDimrModel)mockedF1dModel).Expect(m => m.KernelDirectoryLocation).Return(".").Repeat.Any();
        //    ((IDimrModel)mockedF1dModel).Expect(m => m.DirectoryName).Return("dm1").Repeat.Any();
        //    ((IDimrModel)mockedF1dModel).Expect(m => m.ExporterType).Return(typeof(WaterFlowModel1DExporter)).Repeat.Any();
        //    Expect.Call(((IDimrModel)mockedF1dModel).Validate()).Return(new ValidationReport("dimrmodel1", new List<ValidationIssue>())).Repeat.Any();
        //    Expect.Call(((IDimrModel)mockedF1dModel).GetExporterPath(Arg<string>.Is.Anything)).IgnoreArguments().Return("dm1\\").Repeat.Any();

        //    var now = new DateTime(1981, 7, 12);
        //    ((ITimeDependentModel)mockedF1dModel).Expect(m => m.StartTime).Return(now - TimeSpan.FromDays(2)).Repeat.Any();
        //    ((ITimeDependentModel)mockedF1dModel).Expect(m => m.StopTime).Return(now).Repeat.Any();
        //    ((ITimeDependentModel)mockedF1dModel).Expect(m => m.TimeStep).Return(TimeSpan.FromHours(1)).Repeat.Any();

        //    var mockedFMModel = mocks.DynamicMultiMock<ITimeDependentModel>(typeof(IModel), typeof(IDimrModel));
        //    mockedFMModel.Expect(m => m.AllDataItems).Return(new List<IDataItem>()).Repeat.Any();

        //    mockedFMModel.Expect(m => m.Name).Return("dimrModel2").Repeat.Any();
        //    ((IDimrModel)mockedFMModel).Expect(dm => dm.ShortName).Return("source").Repeat.Any();
        //    ((IDimrModel)mockedFMModel).Expect(dm => dm.Status).PropertyBehavior();
        //    ((IDimrModel)mockedFMModel).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();
        //    ((IDimrModel)mockedFMModel).Expect(m => m.ExplicitWorkingDirectory).PropertyBehavior();
        //    ((IDimrModel)mockedFMModel).Expect(m => m.RunsInIntegratedModel).PropertyBehavior();
        //    ((IDimrModel)mockedFMModel).Expect(m => m.KernelDirectoryLocation).Return(".").Repeat.Any();
        //    ((IDimrModel)mockedFMModel).Expect(m => m.DirectoryName).Return("dm2").Repeat.Any();
        //    ((IDimrModel)mockedFMModel).Expect(m => m.ExporterType).Return(typeof(WaterFlowFMFileExporter)).Repeat.Any();
        //    Expect.Call(((IDimrModel)mockedFMModel).Validate()).Return(new ValidationReport("dimrmodel2", new List<ValidationIssue>())).Repeat.Any();
        //    Expect.Call(((IDimrModel)mockedFMModel).GetExporterPath(Arg<string>.Is.Anything)).IgnoreArguments().Return("dm2\\").Repeat.Any();
        //    ((ITimeDependentModel)mockedFMModel).Expect(m => m.StartTime).Return(now - TimeSpan.FromDays(2)).Repeat.Any();
        //    ((ITimeDependentModel)mockedFMModel).Expect(m => m.StopTime).Return(now).Repeat.Any();
        //    ((ITimeDependentModel)mockedFMModel).Expect(m => m.TimeStep).Return(TimeSpan.FromHours(1)).Repeat.Any();


        //    var mocked1d2dCoupler = mocks.DynamicMultiMock<Iterative1D2DCoupler>(typeof(IModel), typeof(ICompositeActivity), typeof(IDimrModel));
        //    mocked1d2dCoupler.Expect(wf => wf.Activities).Return(new EventedList<IActivity> { new ActivityWrapper(mockedF1dModel), new ActivityWrapper(mockedFMModel) }).Repeat.Any();
        //    mocked1d2dCoupler.Expect(wf => wf.Flow1DModel).Return(mockedF1dModel as ITimeDependentModel).Repeat.Any();
        //    mocked1d2dCoupler.Expect(wf => wf.Flow2DModel).Return(mockedFMModel as ITimeDependentModel).Repeat.Any();
        //    mocked1d2dCoupler.Expect(wf => wf.DeepClone()).Return(mocked1d2dCoupler).Repeat.Any();
        //    mocked1d2dCoupler.Expect(wf => wf.Name).Return("Coupler").Repeat.Any();
        //    mocked1d2dCoupler.Expect(wf => wf.AllDataItems).Return(Enumerable.Empty<IDataItem>()).Repeat.Any();
        //    mocked1d2dCoupler.Expect(wf => wf.GetHashCode()).Return(1).Repeat.Any();
        //    mocked1d2dCoupler.Expect(wf => wf.Equals(Arg<object>.Is.Anything)).IgnoreArguments().Return(false).Repeat.Any();
        //    ((IDimrModel)mocked1d2dCoupler).Expect(dm => dm.IsMasterTimeStep).Return(true).Repeat.Any();
        //    ((IDimrModel)mocked1d2dCoupler).Expect(dm => dm.ShortName).Return("Coupler").Repeat.Any();
        //    ((IDimrModel)mocked1d2dCoupler).Expect(dm => dm.LibraryName).Return(string.Empty).Repeat.Any();
        //    ((IDimrModel)mocked1d2dCoupler).Expect(dm => dm.DirectoryName).Return("iterative1d2d").Repeat.Any();
        //    ((IDimrModel)mocked1d2dCoupler).Expect(dm => dm.InputFile).Return(string.Empty).Repeat.Any();
        //    ((IDimrModel)mocked1d2dCoupler).Expect(m => m.Status).PropertyBehavior();
        //    ((IDimrModel)mocked1d2dCoupler).Expect(m => m.ExplicitWorkingDirectory).PropertyBehavior();
        //    ((IDimrModel)mocked1d2dCoupler).Expect(m => m.RunsInIntegratedModel).PropertyBehavior();
        //    ((IDimrModel)mocked1d2dCoupler).Expect(m => m.KernelDirectoryLocation).Return(".").Repeat.Any();
        //    ((IDimrModel)mocked1d2dCoupler).Expect(m => m.ExporterType).Return(typeof(Iterative1D2DCouplerExporter)).Repeat.Any();
        //    Expect.Call(((IDimrModel)mocked1d2dCoupler).Validate()).Return(new ValidationReport("dimrmodel3", new List<ValidationIssue>())).Repeat.Any();
        //    Expect.Call(((IDimrModel)mocked1d2dCoupler).GetExporterPath(Arg<string>.Is.Anything)).IgnoreArguments().Return("iterative1d2d\\").Repeat.Any();

        //    mocks.ReplayAll();
        //    var activities = new EventedList<IActivity>() {mocked1d2dCoupler};
        //    var hydroModel = new HydroModel();
        //    var workFlow = new SequentialActivity
        //    {
        //        Activities = activities

        //    };
        //    ((IDimrModel)mockedF1dModel).Status = ActivityStatus.Failed;
        //    ((IDimrModel)mockedF1dModel).ExplicitWorkingDirectory = string.Empty;
        //    ((IDimrModel)mockedF1dModel).RunsInIntegratedModel= default(bool);

        //    ((IDimrModel)mockedFMModel).Status = ActivityStatus.Failed;
        //    ((IDimrModel)mockedFMModel).ExplicitWorkingDirectory = string.Empty;
        //    ((IDimrModel)mockedFMModel).RunsInIntegratedModel= default(bool);

        //    ((IDimrModel)mocked1d2dCoupler).Status = ActivityStatus.Failed;
        //    ((IDimrModel)mocked1d2dCoupler).ExplicitWorkingDirectory = string.Empty;
        //    ((IDimrModel)mocked1d2dCoupler).RunsInIntegratedModel = default(bool);

        //    hydroModel.Workflows.Add(workFlow);
        //    hydroModel.CurrentWorkflow = workFlow;
        //    Assert.That(((IDimrModel)mockedF1dModel).Status, Is.EqualTo(ActivityStatus.Failed));
        //    Assert.That(((IDimrModel)mockedFMModel).Status, Is.EqualTo(ActivityStatus.Failed));
        //    TypeUtils.CallPrivateMethod<HydroModel>(hydroModel, "OnInitialize");
        //    Assert.That(((IDimrModel)mockedF1dModel).Status, Is.EqualTo(ActivityStatus.None));
        //    Assert.That(((IDimrModel)mockedFMModel).Status, Is.EqualTo(ActivityStatus.None));
        //    mocks.VerifyAll();


        //}

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

        public class SimpleExporter : IFileExporter
        {
            public bool Export(object item, string path)
            {
                return true;
            }

            public IEnumerable<Type> SourceTypes()
            {
                throw new NotImplementedException();
            }

            public bool CanExportFor(object item)
            {
                throw new NotImplementedException();
            }

            public string Name { get; }
            public string Category { get; }
            public string FileFilter { get; }
            public Bitmap Icon { get; }
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
