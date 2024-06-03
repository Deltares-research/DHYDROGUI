using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Services;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Core.Services;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.DelftModels.HydroModel.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMapTestUtils;
using SharpTestsEx;
using Arg = NSubstitute.Arg;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class HydroModelTest
    {
        [Test]
        public void Constructor_InitializesImportContext()
        {
            using (var model = new HydroModel())
            {
                Assert.That(model.FileContext, Is.Not.Null);
                Assert.That(model.FileContext.IsInitialized, Is.False);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RemoveDimrModelFromHydroModelActivities_ModelIsRemovedFromFileContext()
        {
            using (var hydroModel = new HydroModel())
            {
                IDimrModel dimrModel1 = CreateDimrModel("directoryName_dimrModel1");
                IDimrModel dimrModel2 = CreateDimrModel("directoryName_dimrModel2");

                hydroModel.Activities.Add(dimrModel1);
                hydroModel.Activities.Add(dimrModel2);

                hydroModel.FileContext.DimrFilePath = @"C:\dir\dimr.xml";
                hydroModel.FileContext.AddRelativeModelDirectory(dimrModel1, "storedModelDir_dimrModel1");
                hydroModel.FileContext.AddRelativeModelDirectory(dimrModel2, "storedModelDir_dimrModel2");

                hydroModel.Activities.Remove(dimrModel1);

                string dimrModelDir1 = hydroModel.FileContext.GetRelativeModelDirectory(dimrModel1);
                string dimrModelDir2 = hydroModel.FileContext.GetRelativeModelDirectory(dimrModel2);

                Assert.That(dimrModelDir1, Is.EqualTo("directoryName_dimrModel1"));
                Assert.That(dimrModelDir2, Is.EqualTo("storedModelDir_dimrModel2"));
            }
        }
        
        [Test]
        public void CreateCoupler_WhenSourceIsRtcModel_ShouldCreateNewCouplerAndSetCommunicationRtcToFmFileName()
        {
            // Arrange
            var provider = new RealTimeControlDimrConfigModelCouplerProvider();

            using (var rtcModel = new RealTimeControlModel())
            {
                var fmModel = Substitute.For<IDimrModel>();
                fmModel.ShortName.Returns("fm");
                fmModel.Name.Returns("FlowFM");

                // Act
                IDimrConfigModelCoupler coupler = provider.CreateCoupler(rtcModel, fmModel, null, null);

                //Assert
                Assert.AreEqual(rtcModel.ShortName + "_to_" + fmModel.ShortName + ".nc", rtcModel.CommunicationRtcToFmFileName);
                Assert.AreEqual("rtc_to_fm", coupler.Name);
                Assert.AreEqual("RTC Model", coupler.Source);
                Assert.AreEqual("FlowFM", coupler.Target);
            }
        }

        [Test]
        public void CreateCoupler_WhenTargetIsRtcModel_ShouldCreateNewCouplerAndSetCommunicationFmToRtcFileName()
        {
            // Arrange
            var provider = new RealTimeControlDimrConfigModelCouplerProvider();

            using (var rtcModel = new RealTimeControlModel())
            {
                var fmModel = Substitute.For<IDimrModel>();
                fmModel.ShortName.Returns("fm");
                fmModel.Name.Returns("FlowFM");

                // Act
                IDimrConfigModelCoupler coupler = provider.CreateCoupler(fmModel, rtcModel, null, null);

                //Assert
                Assert.AreEqual(fmModel.ShortName + "_to_" + rtcModel.ShortName + ".nc", rtcModel.CommunicationFmToRtcFileName);
                Assert.AreEqual("fm_to_rtc", coupler.Name);
                Assert.AreEqual("FlowFM", coupler.Source);
                Assert.AreEqual("RTC Model", coupler.Target);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void HydroModelValidatesCurrentWorkflow()
        {
            using (var hydroModel = new HydroModel())
            {
                hydroModel.CurrentWorkflow = null;

                ValidationReport result = hydroModel.Validate();
                Assert.AreEqual(1, result.ErrorCount);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void HydroModelAddsItsSelfToIHydroModelWorkFlow()
        {
            var hydroModelWorkFlow = Substitute.For<IHydroModelWorkFlow>();
            hydroModelWorkFlow.Activities.Returns(new EventedList<IActivity>());

            using (var hydroModel = new HydroModel())
            {
                hydroModel.Workflows.Add(hydroModelWorkFlow);

                Assert.NotNull(hydroModelWorkFlow.HydroModel);

                hydroModel.Workflows.Remove(hydroModelWorkFlow);
                Assert.IsNull(hydroModelWorkFlow.HydroModel);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void AddingRegionsCreatesChildDataItems()
        {
            using (var hydroModel = new HydroModel())
            {

                var subRegion = new HydroRegion();
                hydroModel.Region.SubRegions.Add(subRegion);

                // asserts
                hydroModel.GetDataItemByValue(hydroModel.Region).Children.Count
                          .Should().Be.EqualTo(1);

                hydroModel.GetDataItemByValue(hydroModel.Region).Children.First().Value
                          .Should().Be.EqualTo(subRegion);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void RemovingModelBreaksLinks()
        {
            var childModel = new SimpleHydroModel();

            var subRegion = new HydroArea();
            var region = new HydroRegion { SubRegions = { subRegion } };
            using (var hydroModel = new HydroModel { Region = region, Activities = { childModel } })
            {

                IDataItem target = childModel.GetDataItemByValue(childModel.Region);
                IDataItem source = hydroModel.GetDataItemByValue(subRegion);
                target.LinkTo(source);

                hydroModel.Activities.Clear();

                source.LinkedBy.Count.Should().Be.EqualTo(0);
            }

        }

        [Test]
        public void Output_NewModel_IsEmptyAndNotOutOfSync()
        {
            using (var hydroModel = new HydroModel())
            {
                Assert.That(hydroModel.OutputIsEmpty, Is.True);
                Assert.That(hydroModel.OutputOutOfSync, Is.False);
            }
        }

        [Test]
        public void Output_WithModelOutput_IsNotOutOfSyncAndNotEmpty()
        {
            using (var hydroModel = new HydroModel())
            {
                var simpleModel = new SimpleModel();
                hydroModel.Activities.Add(simpleModel);

                var workflow = new ParallelActivity { Activities = { simpleModel, } };

                hydroModel.Workflows.Add(workflow);
                hydroModel.CurrentWorkflow = workflow;

                ActivityRunner.RunActivity(hydroModel);

                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));
                Assert.That(hydroModel.OutputIsEmpty, Is.False);
                Assert.That(hydroModel.OutputOutOfSync, Is.False);
            }
        }

        [Test]
        public void Output_WithCleanOutput_IsEmptyAndNotOutOfSync()
        {
            using (var hydroModel = new HydroModel())
            {
                var simpleModel = new SimpleModel();
                hydroModel.Activities.Add(simpleModel);

                var workflow = new ParallelActivity { Activities = { simpleModel } };

                hydroModel.Workflows.Add(workflow);
                hydroModel.CurrentWorkflow = workflow;

                ActivityRunner.RunActivity(hydroModel);
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                hydroModel.ClearOutput();

                Assert.That(hydroModel.OutputIsEmpty, Is.True);
                Assert.That(hydroModel.OutputOutOfSync, Is.False);
            }
        }

        [Test]
        public void Output_WithMarkOutputOutOfSync_IsOutOfSyncAndNotEmpty()
        {
            using (var hydroModel = new HydroModel())
            {
                var simpleModel = new SimpleModel();
                hydroModel.Activities.Add(simpleModel);

                var workflow = new ParallelActivity { Activities = { simpleModel } };

                hydroModel.Workflows.Add(workflow);
                hydroModel.CurrentWorkflow = workflow;

                ActivityRunner.RunActivity(hydroModel);
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                hydroModel.MarkOutputOutOfSync();

                Assert.That(hydroModel.OutputIsEmpty, Is.False);
                Assert.That(hydroModel.OutputOutOfSync, Is.True);
            }
        }

        [Test]
        public void ClearOutput_WithTextDocumentDataItem_ThenDataItemIsRemovedFromModel()
        {
            const string textDocumentTag = "TextDocumentTag";

            // Setup
            var simpleModel = new SimpleModel();
            using (var hydroModel = new HydroModel())
            {
                hydroModel.Activities.Add(simpleModel);

                var workflow = new ParallelActivity { Activities = { simpleModel } };

                hydroModel.Workflows.Add(workflow);
                hydroModel.CurrentWorkflow = workflow;

                ActivityRunner.RunActivity(hydroModel);
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                hydroModel.DataItems.Add(new DataItem(new TextDocument(), DataItemRole.Output, textDocumentTag));

                // Pre-condition
                Assert.That(hydroModel.OutputIsEmpty, Is.False);

                // Call
                hydroModel.ClearOutput();

                // Assert
                Assert.That(hydroModel.GetDataItemByTag(textDocumentTag), Is.Null,
                            "Text Document data item should have been removed at model output clearance.");
            }
        }

        [Test]
        public void TestRunUsingSimpleModel2_FailsValidation()
        {
            const string sharedNameOfModels = "SimpleModel";

            var m1 = new SimpleModel
            {
                Input = 1,
                Name = sharedNameOfModels
            };
            var m2 = new SimpleModel
            {
                Input = 2,
                Name = sharedNameOfModels
            };

            using (var m1Activity = new ActivityWrapper(m1))
            using (var m2Activity = new ActivityWrapper(m2))
            {

                var workflow = new ParallelActivity { Activities = { m1Activity, m2Activity } };

                using (var hydroModel = new HydroModel
                {
                    Activities =
                    {
                        m1,
                        m2
                    },
                    Workflows = { workflow },
                    CurrentWorkflow = workflow
                })
                {
                    // run
                    hydroModel.Initialize();
                    Assert.AreEqual(ActivityStatus.Failed, hydroModel.Status);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void RunUsingSimpleModel2()
        {
            var m1 = new SimpleModel
            {
                Input = 1,
                Name = "SimpleModel1"
            };
            var m2 = new SimpleModel
            {
                Input = 2,
                Name = "SimpleModel2"
            };

            using (var m1Activity = new ActivityWrapper(m1))
            using (var m2Activity = new ActivityWrapper(m2))
            {
                var workflow = new ParallelActivity { Activities = { m1Activity, m2Activity } };

                using (var hydroModel = new HydroModel
                {
                    Activities =
                    {
                        m1,
                        m2
                    },
                    Workflows = { workflow },
                    CurrentWorkflow = workflow
                })
                {
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
            }
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

            using (var hydroModel = new HydroModel())
            {
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
                                new SequentialActivity {Activities = {hydroModelWorkFlow4}}
                            }
                        }
                    }
                };

                hydroModel.Workflows.Add(workFlow);
                hydroModel.CurrentWorkflow = workFlow;

                IDictionary<IHydroModelWorkFlowData, IList<int>> lookUp = hydroModel.CurrentWorkFlowData.HydroModelWorkFlowDataLookUp;

                Assert.AreEqual(4, lookUp.Count);
                Assert.AreEqual(new[]
                {
                    0
                }, lookUp[hydroModelWorkFlowData1]);
                Assert.AreEqual(new[]
                {
                    1,
                    0
                }, lookUp[hydroModelWorkFlowData2]);
                Assert.AreEqual(new[]
                {
                    1,
                    1
                }, lookUp[hydroModelWorkFlowData3]);
                Assert.AreEqual(new[]
                {
                    1,
                    2,
                    0
                }, lookUp[hydroModelWorkFlowData4]);
            }
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

            using (var hydroModel = new HydroModel())
            {
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
                                new SequentialActivity {Activities = {hydroModelWorkFlow4}}
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
                        {
                            hydroModelWorkFlowData1, new List<int>(new[]
                            {
                                0
                            })
                        },
                        {
                            hydroModelWorkFlowData2, new List<int>(new[]
                            {
                                1,
                                0
                            })
                        },
                        {
                            hydroModelWorkFlowData3, new List<int>(new[]
                            {
                                1,
                                1
                            })
                        },
                        {
                            hydroModelWorkFlowData4, new List<int>(new[]
                            {
                                1,
                                2,
                                0
                            })
                        }
                    }
                };

                hydroModel.CurrentWorkFlowData = compositeHydroModelWorkFlowData;

                Assert.AreEqual(hydroModelWorkFlow1.Data, hydroModelWorkFlowData1);
                Assert.AreEqual(hydroModelWorkFlow2.Data, hydroModelWorkFlowData2);
                Assert.AreEqual(hydroModelWorkFlow3.Data, hydroModelWorkFlowData3);
                Assert.AreEqual(hydroModelWorkFlow4.Data, hydroModelWorkFlowData4);
            }
        }

        [Test]
        public void GivenAHydroModelWithIDimrModel_WhenFinishIsCalled_ThenAfterSuccessfulIntegratedModelRunActionsShouldBeCalled()
        {
            // Given
            using (var hydroModel = new HydroModel())
            {
                var activity = Substitute.For<IDimrModel>();
                var workflow = new SequentialActivity { Activities = { activity } };
                hydroModel.CurrentWorkflow = workflow;

                // When 
                hydroModel.Finish();

                // Then
                activity.Received(1).OnFinishIntegratedModelRun(hydroModel.WorkingDirectoryPath);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAHydroModelWithFMModelAndCacheFile_WhenInitializeIsCalled_ThenTheCacheFileShouldBeCopiedToWorkingDirectory()
        {
            // Given
            using (var tempDirectory = new TemporaryDirectory())
            using (var hydroModel = new HydroModel())
            {
                string testTempDirectory = tempDirectory.Path;
                string saveFolderPath = Path.Combine(testTempDirectory, "SaveLocation");
                Directory.CreateDirectory(saveFolderPath);

                var fileExportService = new FileExportService();
                fileExportService.RegisterFileExporter(new FMModelFileExporter());
                
                hydroModel.WorkingDirectoryPathFunc = () => testTempDirectory;
                hydroModel.HydroModelExporter.FileExportService = fileExportService;
                
                string cacheFilePath = Path.Combine(saveFolderPath, "test.cache");
                string mduFilePath = Path.Combine(saveFolderPath, "test.mdu");

                using (FileStream fs = File.Create(cacheFilePath))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes("test");
                    fs.Write(info, 0, info.Length);
                }

                var activity = new WaterFlowFMModel { Grid = UnstructuredGridTestHelper.GenerateRegularGrid(20, 20, 20, 20) };
                activity.CacheFile.UpdatePathToMduLocation(mduFilePath);

                var workflow = new SequentialActivity { Activities = { activity } };
                hydroModel.CurrentWorkflow = workflow;

                // When 
                hydroModel.Initialize();

                // Then
                Assert.AreEqual(cacheFilePath, activity.CacheFile.Path);
                Assert.IsTrue(File.Exists(Path.Combine(hydroModel.WorkingDirectoryPath, activity.DirectoryName, activity.Name + ".cache")));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAHydroModelWithFMModelAfterSuccessFulRun_WhenFinishIsCalled_ThenTheCacheFilePathOfTheFMShouldReferToTheOneInWorkingDirectory()
        {
            // Given
            using (var hydroModel = new HydroModel())
            {
                var activity = new WaterFlowFMModel();
                string testTempDirectory = Path.GetTempPath();
                string nonExistingMduFilePath = Path.Combine(testTempDirectory, "SaveLocation", activity.Name + ".mdu");
                activity.CacheFile.UpdatePathToMduLocation(nonExistingMduFilePath);

                hydroModel.WorkingDirectoryPathFunc = () => testTempDirectory;

                var workflow = new SequentialActivity { Activities = { activity } };
                hydroModel.CurrentWorkflow = workflow;

                // When 
                hydroModel.Finish();

                // Then
                string expectedCachePath = Path.Combine(hydroModel.WorkingDirectoryPath, activity.DirectoryName, activity.Name + ".cache");
                Assert.AreEqual(expectedCachePath, activity.CacheFile.Path);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenEmptyWorkflowHydroModel_WhenInitialize_ThenValidationFailsAndDoesNotCrash()
        {
            // Given
            using (var hydroModel = new HydroModel())
            {
                Assert.That(hydroModel.CurrentWorkflow, Is.Null);
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.None));

                // When
                TestDelegate testAction = () => hydroModel.Initialize();

                // Then
                Assert.That(testAction, Throws.Nothing);
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Failed));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAHydroModel_WhenOnInitializeIsCalled_ThenTheExportShouldBeDoneToWorkingDirectoryOfModel()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var hydroModel = new HydroModel())
            {
                // Given
                hydroModel.WorkingDirectoryPathFunc = () => tempDirectory.Path;
                const string hydroModelName = "TestModel";
                hydroModel.Name = hydroModelName;
                SetUpHydroModelWithActivity(hydroModel,
                                            out string modelDirectoryName,
                                            out string modelMduFileName);

                string oldFilePath = Path.Combine(hydroModel.WorkingDirectoryPath, "test.txt");

                FileUtils.CreateDirectoryIfNotExists(hydroModel.WorkingDirectoryPath);

                using (FileStream fs = File.Create(oldFilePath))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes("test");
                    fs.Write(info, 0, info.Length);
                }

                // When
                hydroModel.Initialize();

                // Then
                Assert.IsTrue(File.Exists(Path.Combine(hydroModel.WorkingDirectoryPath, "dimr.xml")));
                Assert.IsTrue(File.Exists(Path.Combine(hydroModel.WorkingDirectoryPath, modelDirectoryName, modelMduFileName)));
                // Check if working directory was cleared before export.
                Assert.IsFalse(File.Exists(Path.Combine(hydroModel.WorkingDirectoryPath, "test.txt")));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAHydroModel_WhenOnCleanupIsCalled_ThenTheOutputShouldBeConnected()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var hydroModel = new HydroModel())
            {
                // Arrange
                const string hydroModelName = "TestModel";
                hydroModel.Name = hydroModelName;
                hydroModel.WorkingDirectoryPathFunc = () => tempDirectory.Path;

                FileUtils.CreateDirectoryIfNotExists(hydroModel.WorkingDirectoryPath);

                string path = Path.Combine(hydroModel.WorkingDirectoryPath, "dimr_redirected.log");
                const string text = "This is some text in the file.";

                using (FileStream fs = File.Create(path))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(text);
                    fs.Write(info, 0, info.Length);
                }

                var activity = Substitute.For<IDimrModel>();
                activity.DimrModelRelativeOutputDirectory.Returns("");

                var workflow = new SequentialActivity { Activities = { activity } };

                hydroModel.Activities.Add(activity);
                hydroModel.CurrentWorkflow = workflow;

                // Act
                hydroModel.Cleanup();

                // Assert
                activity.Received(1).ConnectOutput(hydroModel.WorkingDirectoryPath);
                Assert.AreEqual(text, ((TextDocument)hydroModel.DataItems.First(di => di.Tag == "DimrRunLog").Value).Content);
            }
        }

        [Test]
        public void WorkingDirectoryPath_ShouldReturnCombinationOfInvokedWorkingDirectoryPathFuncAndModelName()
        {
            // Arrange
            using (var hydroModel = new HydroModel
            {
                Name = "Model",
                WorkingDirectoryPathFunc = () => "TestWorkingDirectory"
            })
            {
                // Act, Assert
                Assert.AreEqual(Path.Combine(hydroModel.WorkingDirectoryPathFunc(),
                                             hydroModel.Name), hydroModel.WorkingDirectoryPath);
            }
        }

        [Test]
        public void WorkingDirectoryPathFunc_ShouldReturnDefaultDeltaShellWorkingDirectory()
        {
            // Arrange
            using (var hydroModel = new HydroModel())
            {
                // Act, Assert
                Assert.AreEqual(DefaultModelSettings.DefaultDeltaShellWorkingDirectory,
                                hydroModel.WorkingDirectoryPathFunc());
            }
        }

        [Test]
        public void WorkingDirectoryPathFunc_WhenValueForSetterIsNull_ShouldReturnArgumentNullException()
        {
            // Arrange
            using (var hydroModel = new HydroModel())
            {
                // Act
                void Call()
                {
                    hydroModel.WorkingDirectoryPathFunc = null;
                }

                // Assert
                var exception = Assert.Throws<ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("value"));
            }
        }

        [Test]
        [TestCase(DataItemRole.Input)]
        [TestCase(DataItemRole.Output)]
        [TestCase(DataItemRole.None)]
        public void GetDataItemsUsedForCouplingModel_WhenCalledForModelWhichCannotCouple_ShouldReturnEmptyList(DataItemRole role)
        {
            var model = Substitute.For<IModel>();
            IEnumerable<IDataItem> dataItems = HydroModel.GetDataItemsUsedForCouplingModel(model, role);

            CollectionAssert.IsEmpty(dataItems);
        }

        [Test]
        [TestCase(DataItemRole.Input)]
        [TestCase(DataItemRole.Output)]
        [TestCase(DataItemRole.None)]
        public void GetDataItemsUsedForCouplingModel_WhenCalledForModelWhichCanCouple_ShouldReturnItsDataItemsUsedForCoupling(DataItemRole role)
        {
            IModel model = Substitute.For<IModel, ICoupledModel>();
            var expectedDataItems = new List<IDataItem>();
            ((ICoupledModel)model).GetDataItemsUsedForCouplingModel(role).Returns(expectedDataItems);
            IEnumerable<IDataItem> dataItems = HydroModel.GetDataItemsUsedForCouplingModel(model, role);

            Assert.AreSame(dataItems, expectedDataItems);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Initialise_WhenThereIsAFileExceptionDuringClearingWorkingDirectory_ShouldNotRemoveThisFileException()
        {
            // Arrange
            using (var tempDirectory = new TemporaryDirectory())
            using (var hydroModel = new HydroModel())
            {
                hydroModel.WorkingDirectoryPathFunc = () => tempDirectory.Path;
                const string hydroModelName = "TestModel";
                hydroModel.Name = hydroModelName;

                Directory.CreateDirectory(hydroModel.WorkingDirectoryPath);

                string exceptionFilePath = Path.Combine(hydroModel.WorkingDirectoryPath, "test.txt");
                File.WriteAllText(exceptionFilePath, "test");
                string shouldBeRemovedFilePath = Path.Combine(hydroModel.WorkingDirectoryPath, "test2.txt");
                File.WriteAllText(shouldBeRemovedFilePath, "test");

                SetUpHydroModelWithActivity(hydroModel,
                                            out string _,
                                            out string _,
                                            exceptionFilePath);

                // Act
                hydroModel.Initialize();

                //Assert
                Assert.IsFalse(File.Exists(shouldBeRemovedFilePath));
                Assert.IsTrue(File.Exists(exceptionFilePath));
            }
        }

        [Test]
        public void GivenHydroModelWithoutModels_ThenCanRunIsFalse()
        {
            using (var hydroModel = new HydroModel())
            {
                Assert.That(hydroModel.CanRun, Is.False);
            }
        }

        [Test]
        public void GivenHydroModelWithModelWhereCanRunIsFalse_ThenCanRunIsFalse()
        {
            using (var hydroModel = new HydroModel())
            {

                var model = Substitute.For<IModel>();
                model.CanRun.Returns(false);
                hydroModel.Activities.Add(model);

                Assert.That(hydroModel.CanRun, Is.False);
            }
        }

        [Test]
        public void GivenHydroModelWithModelWhereCanRunIsTrue_ThenCanRunIsTrue()
        {
            using (var hydroModel = new HydroModel())
            {
                var model = Substitute.For<IModel>();
                model.CanRun.Returns(true);
                hydroModel.Activities.Add(model);

                Assert.That(hydroModel.CanRun, Is.True);
            }
        }

        [Test]
        public void GivenHydroModelWithModelWhereCanRunIsFalseAndTrue_ThenCanRunIsTrue()
        {
            using (var hydroModel = new HydroModel())
            {

                var model1 = Substitute.For<IModel>();
                model1.CanRun.Returns(false);

                var model2 = Substitute.For<IModel>();
                model2.CanRun.Returns(true);

                hydroModel.Activities.Add(model1);
                hydroModel.Activities.Add(model2);

                Assert.That(hydroModel.CanRun, Is.True);
            }
        }

        [Test]
        public void ResetActivity_CurrentWorkFlowNotEligibleForDIMRRun_CallsResetActivity()
        {
            // Setup
            var workFlowActivity = Substitute.For<IModel>();
            var workflow = Substitute.For<ICompositeActivity>();
            workflow.Activities.Returns(new EventedList<IActivity>
            {
                workFlowActivity
            });

            using (var model = new HydroModel { CurrentWorkflow = workflow })
            {
                // Call
                model.ResetActivity();

                // Assert
                workflow.Received(1).ResetActivity();
                workFlowActivity.DidNotReceive().ResetActivity();
            }
        }


        [Test]
        public void ResetActivity_CurrentWorkFlowEligibleForDIMRRun_CallsResetActivityOfWorkFlowActivities()
        {
            // Setup
            var workFlowActivityOne = Substitute.For<IDimrModel>();
            var workFlowActivityTwo = Substitute.For<IDimrModel>();
            var workflow = Substitute.For<ICompositeActivity>();
            workflow.Activities.Returns(new EventedList<IActivity>
            {
                workFlowActivityOne,
                workFlowActivityTwo
            });

            using (var model = new HydroModel { CurrentWorkflow = workflow })
            {
                // Call
                model.ResetActivity();

                // Assert
                workflow.DidNotReceive().ResetActivity();
                workFlowActivityOne.Received(1).ResetActivity();
                workFlowActivityTwo.Received(1).ResetActivity();
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void InputPropertyChanged_CallsMarkOutputOutOfSyncOnSelfAndSubModels()
        {
            // Setup
            using (var hydroModel = new HydroModel())
            {
                var simpleModel = Substitute.For<IModel>();
                hydroModel.Activities.Add(simpleModel);

                var workflow = new ParallelActivity { Activities = { simpleModel } };
                simpleModel.Status.Returns(ActivityStatus.Done);

                hydroModel.Workflows.Add(workflow);
                hydroModel.CurrentWorkflow = workflow;

                ActivityRunner.RunActivity(hydroModel);
                Assert.That(hydroModel.Status, Is.EqualTo(ActivityStatus.Cleaned));

                // Call: TimeStep is considered an input property, and thus
                //       will trigger the appropriate behaviour.
                hydroModel.TimeStep = TimeSpan.FromHours(10.0);

                // Assert
                Assert.That(hydroModel.OutputIsEmpty, Is.False);
                Assert.That(hydroModel.OutputOutOfSync, Is.True);
                simpleModel.Received(1).MarkOutputOutOfSync();
            }
        }

        private static void SetUpHydroModelWithActivity(HydroModel hydroModel,
                                                        out string modelDirectoryName,
                                                        out string modelMduFileName,
                                                        string fileException = null)
        {
            var activity = Substitute.For<IDimrModel>();

            modelDirectoryName = "flowfm";
            modelMduFileName = "fm.mdu";

            activity.Validate().Returns(new ValidationReport("", new List<ValidationIssue>()));

            string modelDirectory = Path.Combine(hydroModel.WorkingDirectoryPath, modelDirectoryName);

            activity.GetExporterPath(Arg.Is(modelDirectory))
                    .Returns(Path.Combine(modelDirectory, modelMduFileName));

            activity.DirectoryName.Returns(modelDirectoryName);

            activity.IgnoredFilePathsWhenCleaningWorkingDirectory
                    .Returns(fileException != null
                                 ? new HashSet<string> { fileException }
                                 : new HashSet<string>());

            var workflow = new SequentialActivity { Activities = { activity } };

            var fileExporter = new SimpleFileExporter();
            var fileExportService = Substitute.For<IFileExportService>();
            fileExportService.GetFileExportersFor(activity).Returns(new[] { fileExporter });

            hydroModel.Activities.Add(activity);
            hydroModel.CurrentWorkflow = workflow;
            hydroModel.HydroModelExporter.FileExportService = fileExportService;
        }

        private static IDimrModel CreateDimrModel(string directoryName = null)
        {
            var model = Substitute.For<IDimrModel>();
            model.DirectoryName.Returns(directoryName);

            return model;
        }

        private class SimpleFileExporter : IDimrModelFileExporter
        {
            public string Name { get; }

            public string Category { get; }

            public string Description { get; }

            public string FileFilter { get; }

            public Bitmap Icon { get; }

            public bool Export(object item, string path)
            {
                using (FileStream fs = File.Create(path))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.");
                    fs.Write(info, 0, info.Length);
                }

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
        }

        public class SimpleHydroModel : ModelBase, IHydroModel
        {
            public SimpleHydroModel()
            {
                DataItems.Add(new DataItem(new HydroArea(), "area", SupportedRegionType, DataItemRole.Input, "area"));
            }

            public Type SupportedRegionType => typeof(HydroArea);

            public IHydroRegion Region
            {
                get
                {
                    return (IHydroRegion)DataItems.First(di => di.ValueType == SupportedRegionType).Value;
                }
            }

            protected override void OnInitialize() { }

            protected override void OnExecute()
            {
                Status = ActivityStatus.Done;
            }
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