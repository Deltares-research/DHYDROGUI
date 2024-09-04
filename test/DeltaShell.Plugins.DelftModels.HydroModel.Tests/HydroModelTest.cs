using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.Extensions;
using DeltaShell.Core.Services;
using DeltaShell.Dimr.DimrXsd;
using DeltaShell.Dimr.Export;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.CommonTools.Functions;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using DeltaShell.Plugins.DelftModels.HydroModel.ModelExchanges;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
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
            using (IGui gui = new DHYDROGuiBuilder().WithFlowFM().WithHydroModel().Build())
            {
                IProjectService projectService = gui.Application.ProjectService;

                gui.Run();

                Project project = projectService.CreateProject();
                
                Action mainWindowShown = delegate
                {
                    const string path = "mdu.dsproj";
                    projectService.SaveProjectAs(path); // save to initialize file repository..
                    var builder = new HydroModelBuilder();
                    var integratedModel = builder.BuildModel(ModelGroup.Empty);
                    project.RootFolder.Add(integratedModel);
                    using (var model = new WaterFlowFMModel())
                    {
                        model.NetworkDiscretization.Name = "mesh1d";
                        model.MoveModelIntoIntegratedModel(project.RootFolder, integratedModel);
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
                        projectService.SaveProject();
                        projectService.CloseProject();
                        Assert.That(gui.DocumentViews.OfType<SewerConnectionView>().Count(), Is.EqualTo(0));
                    }
                    
                    project = projectService.OpenProject(path);

                    var retrievedModel = project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>()
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
            public IHydroCoupling HydroCoupling => null;

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

        [Test]
        public void AddingSecondUnpavedCatchmentToLateralSourceSynchronizesBoundaryConditions()
        {
            // Setup
            HydroModel hydroModel = CreateIntegratedModelWithFmAndRr_WithOneUnpavedCatchmentLinkedToALateralSource();
            RainfallRunoffModel rrModel = hydroModel.GetActivitiesOfType<RainfallRunoffModel>().First();
            WaterFlowFMModel fmModel = hydroModel.GetActivitiesOfType<WaterFlowFMModel>().First();
            ILateralSource lateralSource = fmModel.Network.LateralSources.First();
            var existingUnpavedData = rrModel.ModelData.First() as UnpavedData;
            
            UnpavedData newUnpavedData = AddNewUnlinkedUnpavedCatchmentToRrModel(rrModel);

            // Call
            newUnpavedData.Catchment.LinkTo(lateralSource);

            // Assert
            Assert.That(newUnpavedData.BoundarySettings, Is.SameAs(existingUnpavedData.BoundarySettings));
        }
        
        [Test]
        public void GivenAValidIntegratedModelWithRRAndFlowCoupledWhenActivityRunOfRRModelOnlyThenRRModelInputWaterLevelFeatureIsCleared()
        {
            // Setup
            HydroModel hydroModel = CreateIntegratedModel();
            AddCatchmentsAndLateralToModel(hydroModel);
            RainfallRunoffModel rrModel = hydroModel.GetActivitiesOfType<RainfallRunoffModel>().First();
            
            // Call
            ActivityRunner.RunActivity(rrModel);

            // Assert
            Assert.That(rrModel.InputWaterLevel.Features, Is.Empty);
        }
        
        [Test]

        public void GivenAnIntegratedModelWithRRAndFlowCoupledWhenActivityRunOfAnInvalidRRModelOnlyThenRRModelInputWaterLevelFeatureIsCleared()
        {
            // Setup
            HydroModel hydroModel = CreateIntegratedModelWithFmAndRr_WithOneUnpavedCatchmentLinkedToALateralSource();
            RainfallRunoffModel rrModel = hydroModel.GetActivitiesOfType<RainfallRunoffModel>().First();
            
            // Call
            ActivityRunner.RunActivity(rrModel);

            // Assert
            Assert.That(rrModel.InputWaterLevel.Features, Is.Empty);
        }

        [Test]
        public void UnlinkingSecondUnpavedCatchmentFromLateralSourceRemovesSynchronizationOfBoundaryConditions()
        {
            // Setup
            HydroModel hydroModel = CreateIntegratedModelWithFmAndRr_WithOneUnpavedCatchmentLinkedToALateralSource();
            RainfallRunoffModel rrModel = hydroModel.GetActivitiesOfType<RainfallRunoffModel>().First();
            WaterFlowFMModel fmModel = hydroModel.GetActivitiesOfType<WaterFlowFMModel>().First();
            ILateralSource lateralSource = fmModel.Network.LateralSources.First();
            var existingUnpavedData = rrModel.ModelData.First() as UnpavedData;
            
            UnpavedData newUnpavedData = AddNewUnlinkedUnpavedCatchmentToRrModel(rrModel);
            newUnpavedData.Catchment.LinkTo(lateralSource);
            
            // Precondition
            Assert.That(newUnpavedData.BoundarySettings, Is.SameAs(existingUnpavedData.BoundarySettings));

            // Call
            newUnpavedData.Catchment.UnlinkFrom(lateralSource);
            
            // Assert
            Assert.That(newUnpavedData.BoundarySettings, Is.Not.SameAs(existingUnpavedData.BoundarySettings));
            Assert.That(newUnpavedData.BoundarySettings.BoundaryData.Value, Is.EqualTo(existingUnpavedData.BoundarySettings.BoundaryData.Value));
            Assert.That(newUnpavedData.BoundarySettings.BoundaryData.IsConstant, Is.EqualTo(existingUnpavedData.BoundarySettings.BoundaryData.IsConstant));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCase(CatchmentTypes.Greenhouse)]
        [TestCase(CatchmentTypes.OpenWater)]
        [TestCase(CatchmentTypes.Paved)]
        [TestCase(CatchmentTypes.Unpaved)]
        [TestCase(CatchmentTypes.Sacramento)]
        [TestCase(CatchmentTypes.Hbv)]
        [TestCase(CatchmentTypes.NWRW)]
        public void IntegratedModelWithTwoCatchmentsLinkedToSameLateral_ShouldLoadProjectCorrectly(CatchmentTypes catchmentType)
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            using (IApplication app = GetConfiguredApplication())
            using (HydroModel integratedModel = CreateIntegratedModelWithTwoCatchmentsLinkedToSameLateral(catchmentType))
            {
                IProjectService projectService = app.ProjectService;
                projectService.Project.RootFolder.Add(integratedModel);

                string dsprojPath = Path.Combine(temp.Path, "randomName.dsproj");
                projectService.SaveProjectAs(dsprojPath);
                projectService.CloseProject();

                // Call
                Project project = projectService.OpenProject(dsprojPath);
                HydroModel loadedIntegratedModel = project.GetAllItemsRecursive().OfType<HydroModel>().FirstOrDefault();

                // Assert
                Assert.That(loadedIntegratedModel, Is.Not.Null);
                RainfallRunoffModel rrModel = loadedIntegratedModel.Models.OfType<RainfallRunoffModel>().First();

                IEventedList<Catchment> catchments = rrModel.Basin.Catchments;
                Assert.That(catchments.Count, Is.EqualTo(2));
                Catchment firstCatchment = rrModel.Basin.Catchments.First();
                Catchment secondCatchment = rrModel.Basin.Catchments.Last();

                IHydroObject linkTargetOfFirstCatchment = firstCatchment.Links.First().Target;
                IHydroObject linkTargetOfSecondCatchment = secondCatchment.Links.First().Target;
                Assert.That(linkTargetOfFirstCatchment, Is.TypeOf<LateralSource>());
                Assert.That(linkTargetOfFirstCatchment, Is.SameAs(linkTargetOfSecondCatchment));
            }
        }

        private static HydroModel CreateIntegratedModelWithTwoCatchmentsLinkedToSameLateral(CatchmentTypes catchmentType)
        {
            var integratedModel = new HydroModel();
            var fmModel = new WaterFlowFMModel();
            var rrModel = new RainfallRunoffModel();

            integratedModel.Activities.Add(fmModel);
            integratedModel.Activities.Add(rrModel);

            // Add two nodes, a branch and a lateral source
            IHydroNetwork network = fmModel.Network;
            var node1 = new HydroNode
            {
                Name = "Node1",
                Network = network,
                Geometry = new Point(0.0, 0.0)
            };
            var node2 = new HydroNode
            {
                Name = "Node2",
                Network = network,
                Geometry = new Point(100.0, 0.0)
            };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var channel = new Channel("branch1", node1, node2)
            {
                Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate }),
            };
            network.Branches.Add(channel);

            var lateralSource = LateralSource.CreateDefault(channel);
            lateralSource.Chainage = 10;
            channel.BranchFeatures.Add(lateralSource);

            // Create two catchments and link them to same lateral
            IDrainageBasin basin = rrModel.Basin;

            var catchmentOne = new Catchment() { Name = "Catchment1", CatchmentTypes = catchmentType};
            var catchmentTwo = new Catchment() { Name = "Catchment2", CatchmentTypes = catchmentType};

            CatchmentModelData dataOne;
            CatchmentModelData dataTwo;
            
            switch (catchmentType)
            {
                case CatchmentTypes.Greenhouse:
                    catchmentOne.CatchmentTypes = CatchmentTypes.Greenhouse;
                    catchmentTwo.CatchmentTypes = CatchmentTypes.Greenhouse;
                    dataOne = new GreenhouseData(catchmentOne);
                    dataTwo = new GreenhouseData(catchmentTwo);
                    break;
                case CatchmentTypes.OpenWater:
                    catchmentOne.CatchmentTypes = CatchmentTypes.OpenWater;
                    catchmentTwo.CatchmentTypes = CatchmentTypes.OpenWater;
                    dataOne = new OpenWaterData(catchmentOne);
                    dataTwo = new OpenWaterData(catchmentTwo);
                    break;
                case CatchmentTypes.Paved:
                    catchmentOne.CatchmentTypes = CatchmentTypes.Paved;
                    catchmentTwo.CatchmentTypes = CatchmentTypes.Paved;
                    dataOne = new PavedData(catchmentOne);
                    dataTwo = new PavedData(catchmentTwo);
                    break;
                case CatchmentTypes.Unpaved:
                    catchmentOne.CatchmentTypes = CatchmentTypes.Unpaved;
                    catchmentTwo.CatchmentTypes = CatchmentTypes.Unpaved;
                    dataOne = new UnpavedData(catchmentOne);
                    dataTwo = new UnpavedData(catchmentTwo);
                    break;
                case CatchmentTypes.Sacramento:
                    catchmentOne.CatchmentTypes = CatchmentTypes.Sacramento;
                    catchmentTwo.CatchmentTypes = CatchmentTypes.Sacramento;
                    dataOne = new SacramentoData(catchmentOne);
                    dataTwo = new SacramentoData(catchmentTwo);
                    break;
                case CatchmentTypes.Hbv:
                    catchmentOne.CatchmentTypes = CatchmentTypes.Hbv;
                    catchmentTwo.CatchmentTypes = CatchmentTypes.Hbv;
                    dataOne = new HbvData(catchmentOne);
                    dataTwo = new HbvData(catchmentTwo);
                    break;
                case CatchmentTypes.NWRW:
                    catchmentOne.CatchmentTypes = CatchmentTypes.NWRW;
                    catchmentTwo.CatchmentTypes = CatchmentTypes.NWRW;
                    dataOne = new NwrwData(catchmentOne);
                    dataTwo = new NwrwData(catchmentTwo);
                    break;
                case CatchmentTypes.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(catchmentType), catchmentType, $"Unknown catchment type: {catchmentType}.");
            }

            basin.Catchments.Add(catchmentOne);
            basin.Catchments.Add(catchmentTwo);
            rrModel.ModelData[0] = dataOne;
            rrModel.ModelData[1] = dataTwo;
            
            catchmentOne.LinkTo(lateralSource);
            catchmentTwo.LinkTo(lateralSource);

            return integratedModel;
        }

        private static HydroModel CreateIntegratedModelWithFmAndRr_WithOneUnpavedCatchmentLinkedToALateralSource()
        {
            var hydroModel = new HydroModel();
            var fmModel = new WaterFlowFMModel();
            var rrModel = new RainfallRunoffModel();

            hydroModel.Activities.Add(fmModel);
            hydroModel.Activities.Add(rrModel);

            var node1 = new HydroNode();
            var node2 = new HydroNode();
            var branch = new Channel(node1, node2);

            fmModel.Network.Nodes.Add(node1);
            fmModel.Network.Nodes.Add(node2);
            fmModel.Network.Branches.Add(branch);

            var lateralSource = new LateralSource { Branch = branch };
            branch.BranchFeatures.Add(lateralSource);

            UnpavedData unpavedData = AddNewUnlinkedUnpavedCatchmentToRrModel(rrModel);
            unpavedData.Catchment.LinkTo(lateralSource);

            return hydroModel;
        }
        
        private static UnpavedData AddNewUnlinkedUnpavedCatchmentToRrModel(RainfallRunoffModel rrModel)
        {
            var catchment = new Catchment();
            rrModel.Basin.Catchments.Add(catchment);
            
            var boundaryData = new RainfallRunoffBoundaryData()
            {
                IsConstant = true,
                Value = 123
            };
            var unpavedData = new UnpavedData(catchment) 
            { 
                BoundarySettings = { 
                    BoundaryData = boundaryData,
                    UseLocalBoundaryData = true
                }
            };
            rrModel.ModelData.Add(unpavedData);

            return unpavedData;
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.DataAccess)]
        public void IntegratedModelWithLinksFromCatchmentsToFmLateralsAndRRBoundaries_WhenRRBoundaryAndLateralSourceHaveDifferentNames_ShouldImportLinksCorrectly()
        {
            // Setup
            string originalDimrPath = Path.Combine(TestHelper.GetTestDataDirectory(), @"HydroModel\RRBoundaryAndLateralSourceHaveDifferentNames\RRBoundaryAndLateralSourceHaveDifferentNames.xml");
            SetRequiredSettingsForDimrImport();

            using (var tempDir = new TemporaryDirectory())
            {
                string dimrPath = tempDir.CopyTestDataFileAndDirectoryToTempDirectory(originalDimrPath);

                // Call
                object importedModel = ImportFromDimrXml(dimrPath);

                // Assert
                Assert.That(importedModel, Is.TypeOf<HydroModel>());

                var hydroModel = (HydroModel)importedModel;
                RainfallRunoffModel rrModel = hydroModel.Models.OfType<RainfallRunoffModel>().First();

                Assert.That(hydroModel.Region.Links.Count, Is.EqualTo(2));
                HydroLink hydroLink = hydroModel.Region.Links.First();
                Assert.That(hydroLink.Source, Is.TypeOf<Catchment>());
                Assert.That(hydroLink.Target, Is.TypeOf<LateralSource>());
                
                HydroLink lastHydroLink = hydroModel.Region.Links.Last();
                Assert.That(lastHydroLink.Source, Is.TypeOf<Catchment>());
                Assert.That(lastHydroLink.Target, Is.TypeOf<LateralSource>());
                
                Assert.That(rrModel.Basin.Links.Count, Is.EqualTo(0));
                Assert.That(rrModel.Basin.Boundaries.Count, Is.EqualTo(0));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void IntegratedModelWithLinksFromCatchmentsToFmLateralsAndRRBoundaries_ShouldImportLinksCorrectly()
        {
            // Setup
            SetRequiredSettingsForDimrImport();

            using (var tempDir = new TemporaryDirectory())
            using (HydroModel integratedModel = CreateIntegratedModel())
            {
                AddCatchmentsAndLateralToModel(integratedModel);
                string dimrPath = Path.Combine(tempDir.Path, "dimr.xml");

                // Call
                ExportToDimrXml(integratedModel, dimrPath);
                object importedModel = ImportFromDimrXml(dimrPath);
                
                // Assert
                Assert.That(importedModel, Is.TypeOf<HydroModel>());

                var hydroModel = (HydroModel)importedModel;
                RainfallRunoffModel rrModel = hydroModel.Models.OfType<RainfallRunoffModel>().First();

                Assert.That(hydroModel.Region.Links.Count, Is.EqualTo(1));
                HydroLink hydroLink = hydroModel.Region.Links.First();
                Assert.That(hydroLink.Source, Is.TypeOf<Catchment>());
                Assert.That(hydroLink.Target, Is.TypeOf<LateralSource>());
                
                Assert.That(rrModel.Basin.Links.Count, Is.EqualTo(1));
                HydroLink rrLink = rrModel.Basin.Links.First();
                Assert.That(rrLink.Source, Is.TypeOf<Catchment>());
                Assert.That(rrLink.Target, Is.TypeOf<RunoffBoundary>());
                Assert.That(rrModel.Basin.Boundaries.Count, Is.EqualTo(1));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenIntegratedModelWithLinksFromCatchmentsToFmLateralsAndRRBoundaries_WhenImportOnlyRR_ShouldImportLinksOnlyRRWithRainfallRunOffBoundaryInsteadOfLateral()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (HydroModel integratedModel = CreateIntegratedModel())
            {
                AddCatchmentsAndLateralToModel(integratedModel);
                
                string dimrPath = Path.Combine(tempDir.Path, "dimr.xml");
                
                ExportToDimrXml(integratedModel, dimrPath);
                string rainfallRunoffPath = Path.Combine(tempDir.Path, @"rr\Sobek_3b.fnm");
                
                var rainfallRunoffImporter = new SobekHydroModelImporter(true, false, false);
                
                // Call
                object importedModel = rainfallRunoffImporter.ImportItem(rainfallRunoffPath);
                
                // Assert
                Assert.That(importedModel, Is.TypeOf<HydroModel>());

                var hydroModel = (HydroModel)importedModel;
                RainfallRunoffModel rrModel = hydroModel.Models.OfType<RainfallRunoffModel>().First();

                Assert.That(hydroModel.Region.Links.Count, Is.EqualTo(0));
                
                Assert.That(rrModel.Basin.Links.Count, Is.EqualTo(2));
                Assert.That(rrModel.Basin.Boundaries.Count, Is.EqualTo(2));
                
                HydroLink rrLinkFirst = rrModel.Basin.Links.First();
                Assert.That(rrLinkFirst.Source, Is.TypeOf<Catchment>());
                Assert.That(rrLinkFirst.Target, Is.TypeOf<RunoffBoundary>());
                
                HydroLink rrLinkLast = rrModel.Basin.Links.Last();
                Assert.That(rrLinkLast.Source, Is.TypeOf<Catchment>());
                Assert.That(rrLinkLast.Target, Is.TypeOf<RunoffBoundary>());
            }
        }
        
        [Test]
        [Category(TestCategory.Integration)]
        public void GivenIntegratedModelWithLinksFromCatchmentsToFmLateralsAndRRBoundariesProject_WhenLoadingProject_ShouldImportLinksCorrectly()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            using (IApplication app = GetConfiguredApplication())
            using (HydroModel integratedModel = CreateIntegratedModel())
            {
                AddCatchmentsAndLateralToModel(integratedModel);
                
                IProjectService projectService = app.ProjectService;
                projectService.Project.RootFolder.Add(integratedModel);

                string dsprojPath = Path.Combine(temp.Path, "randomName.dsproj");
                projectService.SaveProjectAs(dsprojPath);
                projectService.CloseProject();

                // Call
                Project project = projectService.OpenProject(dsprojPath);
                HydroModel loadedIntegratedModel = project.GetAllItemsRecursive().OfType<HydroModel>().FirstOrDefault();
                
                // Assert
                Assert.That(loadedIntegratedModel, Is.Not.Null);
                RainfallRunoffModel rrModel = loadedIntegratedModel.Models.OfType<RainfallRunoffModel>().First();

                Assert.That(loadedIntegratedModel.Region.Links.Count, Is.EqualTo(1));
                HydroLink hydroLink = loadedIntegratedModel.Region.Links.First();
                Assert.That(hydroLink.Source, Is.TypeOf<Catchment>());
                Assert.That(hydroLink.Target, Is.TypeOf<LateralSource>());
                
                Assert.That(rrModel.Basin.Links.Count, Is.EqualTo(1));
                HydroLink rrLink = rrModel.Basin.Links.First();
                Assert.That(rrLink.Source, Is.TypeOf<Catchment>());
                Assert.That(rrLink.Target, Is.TypeOf<RunoffBoundary>());
                Assert.That(rrModel.Basin.Boundaries.Count, Is.EqualTo(1));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ExportingDimrModelWithWWTPLinkedToLateralSource_WritesExchangesToDimrFile()
        {
            // Setup
            SetRequiredSettingsForDimrImport();
            
            using (var tempDir = new TemporaryDirectory())
            using (HydroModel integratedModel = CreateIntegratedModel())
            {
                AddWWTPLinkedLateralToModel(integratedModel);
                
                string dimrPath = Path.Combine(tempDir.Path, "dimr.xml");
                
                
                // Call
                ExportToDimrXml(integratedModel, dimrPath);

                // Assert
                var parser = new DelftConfigXmlFileParser(Substitute.For<ILogHandler>());
                var dimrContents = parser.Read<dimrXML>(dimrPath);

                dimrCouplerXML rrToFlowCoupler = dimrContents.coupler.First(c => c.name.EqualsCaseInsensitive("rr_to_flow"));
                Assert.That(rrToFlowCoupler.item.Length, Is.EqualTo(1)); // (boundary of) WWTP --> LateralSource

                dimrCoupledItemXML couplerItem = rrToFlowCoupler.item[0];
                Assert.That(couplerItem.sourceName, Is.EqualTo("catchments/LateralSource_1D_1/water_discharge"));
                Assert.That(couplerItem.targetName, Is.EqualTo("laterals/LateralSource_1D_1/water_discharge"));

                bool hasFlowToRRCoupling = dimrContents.coupler.Any(c => c.name.EqualsCaseInsensitive("flow_to_r"));
                Assert.That(hasFlowToRRCoupling, Is.False);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void SavingDimrModelWithWWTPLinkedToLateralSource_WritesExchangesToRegionExchangesJson()
        {
            // Setup
            SetRequiredSettingsForDimrImport();
            
            using (var tempDir = new TemporaryDirectory())
            using (IApplication app = GetConfiguredApplication())
            using (HydroModel integratedModel = CreateIntegratedModel())
            {
                AddWWTPLinkedLateralToModel(integratedModel);
                
                IProjectService projectService = app.ProjectService;
                projectService.Project.RootFolder.Add(integratedModel);

                const string projectName = "randomName.dsproj";
                string dsprojPath = Path.Combine(tempDir.Path, projectName);
                projectService.SaveProjectAs(dsprojPath);
                projectService.CloseProject();

                // Call
                Project project = projectService.OpenProject(dsprojPath);
                HydroModel loadedIntegratedModel = project.GetAllItemsRecursive().OfType<HydroModel>().FirstOrDefault();
                
                // Assert
                Assert.That(loadedIntegratedModel, Is.Not.Null);

                string modelExchangeJsonFile = Path.Combine(tempDir.Path,
                                                        $"{projectName}_data",
                                                        "Integrated Model",
                                                        $"{integratedModel.Name}RegionExchanges.json");
                FileAssert.Exists(modelExchangeJsonFile);

                AssertThatFileContainsExpectedExchange(modelExchangeJsonFile);
            }
        }

        private static void AssertThatFileContainsExpectedExchange(string modelExchangeJsonFile)
        {
            IList<ModelExchangeInfo> modelExchangeInfos = new List<ModelExchangeInfo>();
            modelExchangeInfos.ReadFromJson(modelExchangeJsonFile);
                
            Assert.That(modelExchangeInfos.Count, Is.EqualTo(1));
            Assert.That(modelExchangeInfos[0].Exchanges.Count, Is.EqualTo(1));

            ModelExchange modelExchange = modelExchangeInfos[0].Exchanges[0];
            Assert.That(modelExchange.SourceName, Is.EqualTo("WWTP"));
            Assert.That(modelExchange.TargetName, Is.EqualTo("LateralSource_1D_1"));
        }

        /// <summary>
        /// Creates a valid integrated model with FM and RR with:
        /// <list type="bullet">
        /// <item><description>1 branch with a cross-section and grid points.</description></item>
        /// <item><description>timeseries for precipitation and evaporation.</description></item>
        /// </list>
        /// </summary>
        /// <returns>Created integrated model.</returns>
        private static HydroModel CreateIntegratedModel()
        {
            // Integrated model with FM + RR
            var integratedModel = new HydroModel();
            var fmModel = new WaterFlowFMModel();
            var rrModel = new RainfallRunoffModel();

            integratedModel.Activities.Add(fmModel);
            integratedModel.Activities.Add(rrModel);

            // Add two nodes, a branch and a lateral source
            IHydroNetwork network = fmModel.Network;
            var node1 = new HydroNode
            {
                Name = "Node1",
                Network = network,
                Geometry = new Point(0.0, 0.0)
            };
            var node2 = new HydroNode
            {
                Name = "Node2",
                Network = network,
                Geometry = new Point(100.0, 0.0)
            };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var channel = new Channel("branch1", node1, node2)
            {
                Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate }),
            };
            network.Branches.Add(channel);

            var crossSection = CrossSection.CreateDefault();
            crossSection.Name = "Marlon";
            channel.BranchFeatures.Add(crossSection);

            // Generate grid points
            var offsets = new double[] { 0, 30, 60, 100 };
            HydroNetworkHelper.GenerateDiscretization(fmModel.NetworkDiscretization, channel, offsets);

            // Set PrecipitationMeteoData
            MeteoData precipitation = rrModel.Precipitation;
            var timeseriesGenerator = new TimeSeriesGenerator();
            var hourTimestep = new TimeSpan(1, 0, 0);
            timeseriesGenerator.GenerateTimeSeries(precipitation.Data, integratedModel.StartTime, integratedModel.StopTime, hourTimestep);

            // Set Evaporation
            MeteoData evap = rrModel.Evaporation;
            var dayTimestep = new TimeSpan(1, 0, 0, 0);
            timeseriesGenerator.GenerateTimeSeries(evap.Data, integratedModel.StartTime, integratedModel.StopTime, dayTimestep);
            return integratedModel;
        }
        
        /// <summary>
        /// Adds the following to the given integrated model:
        /// <list type="bullet">
        /// <item><description>A lateral source.</description></item>
        /// <item><description>A RR boundary.</description></item>
        /// <item><description>A paved catchment linked to the lateral source.</description></item>
        /// <item><description>An unpaved catchment linked to the RR boundary.</description></item>
        /// </list>
        /// </summary>
        /// <param name="integratedModel">The integrated model to edit.</param>
        private void AddCatchmentsAndLateralToModel(HydroModel integratedModel)
        {
            WaterFlowFMModel fmModel = GetFMModel(integratedModel);
            RainfallRunoffModel rrModel = GetRRModel(integratedModel);

            IChannel channel = fmModel.Network.Channels.First();
            
            var lateralSource = LateralSource.CreateDefault(channel);
            lateralSource.Chainage = 10;
            channel.BranchFeatures.Add(lateralSource);
            
            // Create two catchments and a runoff boundary. 
            IDrainageBasin basin = rrModel.Basin;

            var pavedCatchment = new Catchment
            {
                Name = "PavedCatchment",
                CatchmentType = CatchmentType.Paved
            };
            var unpavedCatchment = new Catchment
            {
                Name = "UnpavedCatchment",
                CatchmentType = CatchmentType.Unpaved
            };

            var pavedData = new PavedData(pavedCatchment)
            {
                DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode,
                MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode,
            };
            var unpavedData = new UnpavedData(unpavedCatchment);

            rrModel.Basin.Catchments.Add(pavedCatchment);
            rrModel.Basin.Catchments.Add(unpavedCatchment);

            rrModel.ModelData[0] = pavedData;
            rrModel.ModelData[1] = unpavedData;

            var boundary = new RunoffBoundary()
            {
                Geometry = new Point(1, 2)
            };
            basin.Boundaries.Add(boundary);

            // Link 1 catchment two runoff boundary and link 1 to lateral source
            pavedCatchment.LinkTo(lateralSource);
            unpavedCatchment.LinkTo(boundary);
        }

        /// <summary>
        /// Adds the following to the given integrated model:
        /// <list type="bullet">
        /// <item><description>A lateral source.</description></item>
        /// <item><description>A RR boundary.</description></item>
        /// <item><description>A waste water treatment plant linked to the lateral source.</description></item>
        /// <item><description>A paved catchment linked to the waste water treatment plant and the RR boundary.</description></item>
        /// </list>
        /// </summary>
        /// <note>The additional (paved) catchment is required because the WWTP requires an inbound link.
        /// The (RR) boundary is then required, because the catchment needs a link to a boundary.</note>
        /// <param name="integratedModel">The integrated model to edit.</param>
        private void AddWWTPLinkedLateralToModel(HydroModel integratedModel)
        {
            WaterFlowFMModel fmModel = GetFMModel(integratedModel);
            RainfallRunoffModel rrModel = GetRRModel(integratedModel);

            IChannel channel = fmModel.Network.Channels.First();
            
            var lateralSource = LateralSource.CreateDefault(channel);
            lateralSource.Chainage = 10;
            channel.BranchFeatures.Add(lateralSource);

            var wwtp = new WasteWaterTreatmentPlant { Geometry = new Point(0, 0)};
            rrModel.Basin.WasteWaterTreatmentPlants.Add(wwtp);

            wwtp.LinkTo(lateralSource);
            
            var pavedCatchment = new Catchment
            {
                Name = "PavedCatchment",
                CatchmentType = CatchmentType.Paved
            };
            var pavedData = new PavedData(pavedCatchment)
            {
                DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode,
                MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode,
            };
            
            var boundary = new RunoffBoundary()
            {
                Geometry = new Point(1, 2)
            };
            rrModel.Basin.Boundaries.Add(boundary);

            rrModel.Basin.Catchments.Add(pavedCatchment);
            rrModel.ModelData[0] = pavedData;
            pavedCatchment.LinkTo(wwtp);
            pavedCatchment.LinkTo(boundary);
        }

        private static WaterFlowFMModel GetFMModel(HydroModel integratedModel)
        {
            return integratedModel.Models.OfType<WaterFlowFMModel>().First();
        }

        private RainfallRunoffModel GetRRModel(HydroModel integratedModel)
        {
            return integratedModel.Models.OfType<RainfallRunoffModel>().First();
        }

        private static IApplication GetConfiguredApplication()
        {
            IApplication app = new DHYDROApplicationBuilder().WithFlowFM().WithRainfallRunoff().WithHydroModel().Build();
            app.Run();
            app.ProjectService.CreateProject();
            return app;
        }

        private static void SetRequiredSettingsForDimrImport()
        {
            Sobek2ModelImporters.RegisterSobek2Importer(() => new SobekModelToRainfallRunoffModelImporter());
            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }
        
        private static object ImportFromDimrXml(string dimrPath)
        {
            var hybridProjectRepository = Substitute.For<IHybridProjectRepository>();
            var fileImportService = new FileImportService(hybridProjectRepository);
            var hydroModelReader = new HydroModelReader(fileImportService);

            fileImportService.RegisterFileImporter(new WaterFlowFMFileImporter(null));
            fileImportService.RegisterFileImporter(new RainfallRunoffModelImporter());
            
            var importer = new DHydroConfigXmlImporter(fileImportService, hydroModelReader, () => dimrPath);

            return importer.ImportItem(dimrPath);
        }
        
        private static void ExportToDimrXml(HydroModel integratedModel, string dimrPath)
        {
            var fileExportService = new FileExportService();
            fileExportService.RegisterFileExporter(new FMModelFileExporter());
            fileExportService.RegisterFileExporter(new RainfallRunoffModelExporter());
            
            var exporter = new DHydroConfigXmlExporter(fileExportService);

            DimrConfigModelCouplerFactory.CouplerProviders.Add(new RRDimrConfigModelCouplerProvider());
            exporter.Export(integratedModel, dimrPath);
        }
    }
}
