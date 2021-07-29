using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
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
        public void TestOutputIsNotEmptyForSimpleModel()
        {
            /*Sobek3-848*/
            var simpleModel = new SimpleModel();
            Assert.IsTrue(simpleModel.OutputIsEmpty, "No models, so no output");

            simpleModel.Initialize();
            simpleModel.Execute();
            simpleModel.Finish();

            Assert.IsFalse(simpleModel.OutputIsEmpty, "1 empty model and 1 model with output, so Simple model has output");
        }

        [Test]
        public void TestOutputIsNotEmptyForHydroModel()
        {
            /*Sobek3-848*/
            var hydroModel = new SimpleModel();
            Assert.IsTrue(hydroModel.OutputIsEmpty, "No models, so no output");

            ActivityRunner.RunActivity(hydroModel);
            Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));
            Assert.IsFalse(hydroModel.OutputIsEmpty, "Integrated model with output");
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

        [Test]
        [Category(TestCategory.WindowsForms), Apartment(ApartmentState.STA)]
        public void CreateNewModelWithRuralAndUrbanNetworkConnectedCheckAfterSaveLoadTheyAreStillConnectedAndYouCanOpenPipeViewInTheGui()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Plugins.Add(new HydroModelGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new ProjectExplorerGuiPlugin());

                gui.Run();
                Action mainWindowShown = delegate
                {
                    const string path = "mdu.dsproj";
                    app.SaveProjectAs(path); // save to initialize file repository..
                    var builder = new HydroModelBuilder();
                    var integratedModel = builder.BuildModel(ModelGroup.Empty);
                    app.Project.RootFolder.Add(integratedModel);
                    using (var model = new WaterFlowFMModel())
                    {
                        model.NetworkDiscretization.Name = "mesh1d";
                        model.MoveModelIntoIntegratedModel(app.Project.RootFolder, integratedModel);
                        HydroNetworkHelper.AddSnakeHydroNetwork(model.Network,
                            new[] {new Point(0, 0), new Point(1, 1), new Point(2, 4)});
                        var targetHydroNode1 = model.Network.Nodes[1];
                        var targetHydroNode2 = model.Network.Nodes[2];

                        var compartment1 = new Compartment("Compartment 1")
                            {SurfaceLevel = 1, BottomLevel = -3, ManholeWidth = 2.5};
                        var compartment2 = new Compartment("Compartment 2")
                            {SurfaceLevel = 1, BottomLevel = -3, ManholeWidth = 2.5};
                        var manHole1 = new Manhole("MyManhole1")
                            {Geometry = new Point(1, 0), Compartments = new EventedList<ICompartment>() {compartment1}};
                        model.Network.Nodes.Add(manHole1);

                        var manHole2 = new Manhole("MyManhole2")
                            {Geometry = new Point(1, 4), Compartments = new EventedList<ICompartment>() {compartment2}};
                        model.Network.Nodes.Add(manHole2);

                        var pipe1 = new Pipe
                        {
                            Name = "MyPipe1",
                            Geometry = new LineString(new[] {new Coordinate(1, 0), new Coordinate(1, 1),}),
                            Source = manHole1,
                            SourceCompartment = compartment1,
                            Target = targetHydroNode1
                        };

                        SewerFactory.AddDefaultPipeToNetwork(pipe1, model.Network);
                        
                        var pipe2 = new Pipe
                        {
                            Name = "MyPipe2",
                            Geometry = new LineString(new[] {new Coordinate(1, 4), new Coordinate(2, 4),}),
                            Source = manHole2,
                            SourceCompartment = compartment2,
                            Target = targetHydroNode2,
                        };
                        SewerFactory.AddDefaultPipeToNetwork(pipe2, model.Network);
                        
                        Assert.That(targetHydroNode1.IncomingBranches, Has.Member(pipe1));
                        Assert.That(targetHydroNode2.IncomingBranches, Has.Member(pipe2));
                        Assert.That(gui.DocumentViews.OfType<SewerConnectionView>().Count(), Is.EqualTo(0));

                        //Opening view does not crash now. (JIRA: FM1D2D-720)
                        Assert.DoesNotThrow(() => gui.DocumentViewsResolver.OpenViewForData(pipe1), "Could not open PipeView for pipe1");
                        Assert.DoesNotThrow(() => gui.DocumentViewsResolver.OpenViewForData(pipe2), "Could not open PipeView for pipe2");
                        Assert.That(gui.DocumentViews.OfType<SewerConnectionView>().Count(), Is.EqualTo(2));
                        app.SaveProject();
                        app.CloseProject();
                        Assert.That(gui.DocumentViews.OfType<SewerConnectionView>().Count(), Is.EqualTo(0));
                    }
                    
                    app.OpenProject(path);

                    var retrievedModel = app.Project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>()
                        .FirstOrDefault();
                    Assert.That(retrievedModel, Is.Not.Null);
                    var retrievedTargetHydroNodeOfPipe1 = retrievedModel.Network.Nodes.ElementAtOrDefault(1);
                    Assert.That(retrievedTargetHydroNodeOfPipe1, Is.Not.Null);
                    var retrievedPipe1 = retrievedModel.Network.Pipes.FirstOrDefault();
                    Assert.That(retrievedPipe1, Is.Not.Null);
                    Assert.That(retrievedTargetHydroNodeOfPipe1.IncomingBranches, Has.Member(retrievedPipe1));
                    Assert.That(gui.DocumentViews.OfType<SewerConnectionView>().Count(), Is.EqualTo(0));

                    //Opening view crashes now. (JIRA: FM1D2D-720)
                    Assert.DoesNotThrow(() => gui.DocumentViewsResolver.OpenViewForData(retrievedPipe1), "Could not open PipeView for pipe1 after save load");
                    Assert.That(gui.DocumentViews.OfType<SewerConnectionView>().Count(), Is.EqualTo(1));
                    var retrievedTargetHydroNodeOfPipe2 = retrievedModel.Network.Nodes.ElementAtOrDefault(2);
                    Assert.That(retrievedTargetHydroNodeOfPipe2, Is.Not.Null);
                    var retrievedPipe2 = retrievedModel.Network.Pipes.LastOrDefault();
                    Assert.That(retrievedPipe2, Is.Not.Null);
                    Assert.That(retrievedTargetHydroNodeOfPipe2.IncomingBranches, Has.Member(retrievedPipe2));
                    Assert.That(gui.DocumentViews.OfType<SewerConnectionView>().Count(), Is.EqualTo(1));
                    Assert.DoesNotThrow(() => gui.DocumentViewsResolver.OpenViewForData(retrievedPipe2), "Could not open PipeView for pipe2 after save load");
                    Assert.That(gui.DocumentViews.OfType<SewerConnectionView>().Count(), Is.EqualTo(2));
                };
                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }
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
