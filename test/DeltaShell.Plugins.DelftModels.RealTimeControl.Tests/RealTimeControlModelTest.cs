using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.TestUtils;
using DelftTools.Units.Generics;
using DelftTools.Utils.IO;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.CommonTools.TextData;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using DeltaShell.Plugins.ProjectExplorer;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;
using SharpTestsEx;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class RealTimeControlModelTest
    {
        # region ControlledTestModel

        [Test]
        public void TestControlledTestModel()
        {
            var controlledModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0)
            };

            controlledModel.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, controlledModel.Status);

            // Run model
            var timeStepsCount = 0;
            while (controlledModel.Status != ActivityStatus.Done)
            {
                Assert.AreEqual(new DateTime(2000, 1, 1, 0 + timeStepsCount, 0, 0), controlledModel.CurrentTime);

                controlledModel.Execute();
                timeStepsCount++;
            }

            Assert.AreEqual(ActivityStatus.Done, controlledModel.Status);
            Assert.AreEqual(6, timeStepsCount);
        }

        #endregion

        [Test]
        public void GivenRealTimeControlModel_ThenCanRunIsFalse()
        {
            Assert.That(new RealTimeControlModel().CanRun, Is.False);
        }

        [Test]
        public void DimrModelRelativeOutputDirectory_ShouldReturnDirectoryNamePlusOutputDirectoryName()
        {
            var model = new RealTimeControlModel();
            Assert.AreEqual(Path.Combine(model.DirectoryName, DirectoryNameConstants.OutputDirectoryName),
                            model.DimrModelRelativeOutputDirectory);
        }

        [Test]
        public void CleanUpModelAfterModelCoupling_ShouldResetInputIfUnlinked()
        {
            // Given
            var input = new Input
            {
                Name = "test",
                Feature = null,
                ParameterName = "CrestLevel",
                UnitName = "[m]"
            };

            var controlGroup = new ControlGroup();
            controlGroup.Inputs.Add(input);

            var rtcModel = new RealTimeControlModel();
            rtcModel.ControlGroups.Add(controlGroup);

            Input retrievedInputFromRtcModel = rtcModel.ControlGroups[0].Inputs[0];

            Assert.IsFalse(retrievedInputFromRtcModel.IsConnected, "Setup of the test is incorrect");

            // When
            rtcModel.CleanUpModelAfterModelCoupling();

            // Then
            CheckResetConnectionPoint(retrievedInputFromRtcModel);
        }

        [Test]
        public void CleanUpModelAfterModelCoupling_ShouldNotResetInputIfLinked()
        {
            // Given
            var input = new Input
            {
                Feature = new Structure(),
                ParameterName = "CrestLevel",
                UnitName = "[m]"
            };

            var controlGroup = new ControlGroup();
            controlGroup.Inputs.Add(input);

            var rtcModel = new RealTimeControlModel();
            rtcModel.ControlGroups.Add(controlGroup);

            Input retrievedInputFromRtcModel = rtcModel.ControlGroups[0].Inputs[0];
            Assert.IsTrue(retrievedInputFromRtcModel.IsConnected, "Setup of the test is incorrect");

            // When
            rtcModel.CleanUpModelAfterModelCoupling();

            // Then
            Assert.AreEqual("Structure_CrestLevel", input.Name,
                            "The clean up should not have changed the name of the output");
            Assert.AreEqual("CrestLevel", input.ParameterName,
                            "The clean up should not have changed the parameter name of the output");
            Assert.AreEqual("[m]", input.UnitName,
                            "The clean up should not have changed the unit name of the output");
        }

        [Test]
        public void CleanUpModelAfterModelCoupling_ShouldResetOutputIfUnlinked()
        {
            // Given
            var output = new Output
            {
                Name = "test",
                Feature = null,
                ParameterName = "CrestLevel",
                UnitName = "[m]"
            };

            var controlGroup = new ControlGroup();
            controlGroup.Outputs.Add(output);
            var rtcModel = new RealTimeControlModel();
            rtcModel.ControlGroups.Add(controlGroup);

            Output retrievedOutputFromRtcModel = rtcModel.ControlGroups[0].Outputs[0];

            Assert.IsFalse(retrievedOutputFromRtcModel.IsConnected, "Setup of the test is incorrect");

            // When
            rtcModel.CleanUpModelAfterModelCoupling();

            // Then
            CheckResetConnectionPoint(retrievedOutputFromRtcModel);
        }

        [Test]
        public void CleanUpModelAfterModelCoupling_ShouldNotResetOutputIfLinked()
        {
            // Given
            var output = new Output
            {
                Feature = new Structure(),
                ParameterName = "CrestLevel",
                UnitName = "[m]"
            };

            var controlGroup = new ControlGroup();
            controlGroup.Outputs.Add(output);
            var rtcModel = new RealTimeControlModel();
            rtcModel.ControlGroups.Add(controlGroup);

            Assert.IsTrue(output.IsConnected, "Setup of the test is incorrect");

            // When
            rtcModel.CleanUpModelAfterModelCoupling();

            // Then
            Assert.AreEqual("Structure_CrestLevel", output.Name,
                            "The clean up should not have changed the name of the output");
            Assert.AreEqual("CrestLevel", output.ParameterName,
                            "The clean up should not have changed the parameter name of the output");
            Assert.AreEqual("[m]", output.UnitName,
                            "The clean up should not have changed the unit name of the output");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ConnectOutput_OutputDirectoryIsNull_ShouldNotThrowExceptions()
        {
            // Arrange
            var model = new RealTimeControlModel();

            // Act
            model.ConnectOutput(null);

            // Assert
            Assert.IsFalse(model.RestartOutput.Any());
            Assert.IsFalse(model.OutputDocuments.Any());
            Assert.IsNull(model.OutputFileFunctionStore);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ConnectOutput_OutputDirectoryIsEmpty_ShouldNotThrowExceptions()
        {
            // Arrange
            var model = new RealTimeControlModel();

            // Act
            model.ConnectOutput("");

            // Assert
            Assert.IsFalse(model.RestartOutput.Any());
            Assert.IsFalse(model.OutputDocuments.Any());
            Assert.IsNull(model.OutputFileFunctionStore);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ConnectOutput_ParentOutputDirectoryIsMissing_ShouldNotThrowExceptions()
        {
            // Arrange
            var model = new RealTimeControlModel();

            // Act
            model.ConnectOutput("C://");

            // Assert
            Assert.IsFalse(model.RestartOutput.Any());
            Assert.IsFalse(model.OutputDocuments.Any());
            Assert.IsNull(model.OutputFileFunctionStore);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ConnectOutput_WhenOutputDirectoryDoesNotExist_ShouldNotThrowExceptions()
        {
            // Arrange
            var model = new RealTimeControlModel();

            // Act
            model.ConnectOutput("C://test");

            // Assert
            Assert.IsFalse(model.RestartOutput.Any());
            Assert.IsFalse(model.OutputDocuments.Any());
            Assert.IsNull(model.OutputFileFunctionStore);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ConnectOutput_ForEmptyOutputDirectory_ShouldNotThrowExceptions()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Arrange
                var model = new RealTimeControlModel();

                string rtcDirectory = Path.Combine(tempDir.Path, "rtc");
                Directory.CreateDirectory(rtcDirectory);

                // Act
                model.ConnectOutput(rtcDirectory);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ConnectOutput_ForXmlAndCsvFiles_ShouldCreateNewOutputXmlOrCsvDocumentsElements()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Arrange
                RealTimeControlModel model = CreateRtcModelAndFiles(tempDir, out string _, out string rtcDirectory, out string[] relevantFiles);

                // Act
                model.ConnectOutput(rtcDirectory);

                // Assert
                ChecksForOutputXmlOrCsvDocuments(model, relevantFiles);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ConnectOutput_ForXmlAndCsvFiles_ShouldUseExistingOutputXmlOrCsvDocumentsElements()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Arrange
                RealTimeControlModel model = CreateRtcModelAndFiles(tempDir, out string _, out string rtcDirectory, out string[] relevantFiles);

                for (var i = 0; i < 5; i++)
                {
                    model.OutputDocuments.Add(new ReadOnlyTextFileData($"test{i}.csv", "", ReadOnlyTextFileDataType.Default));
                    model.OutputDocuments.Add(new ReadOnlyTextFileData($"test{i}.xml", "", ReadOnlyTextFileDataType.Default));
                }

                // Act
                model.ConnectOutput(rtcDirectory);

                // Assert
                ChecksForOutputXmlOrCsvDocuments(model, relevantFiles);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ConnectOutput_ForXmlCsvAndRestartFiles_ShouldIgnoreRestartFilesInOutputXmlOrCsvDocuments()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Arrange
                RealTimeControlModel model = CreateRtcModelAndFiles(tempDir, out string rtcFolderName, out string rtcDirectory, out string[] relevantFiles);

                CreateRestartFiles(tempDir, rtcFolderName);

                // Act
                model.ConnectOutput(rtcDirectory);

                // Assert
                ChecksForOutputXmlOrCsvDocuments(model, relevantFiles);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void ConnectOutput_RestartFiles_ReconnectsTheRestartFiles()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                const string rtcFolderName = "rtc";
                string rtcDirectory = Path.Combine(tempDir.Path, rtcFolderName);
                Directory.CreateDirectory(rtcDirectory);
                string[] restartFiles = CreateRestartFiles(tempDir, rtcFolderName).ToArray();

                var model = new RealTimeControlModel();

                // Call
                model.ConnectOutput(rtcDirectory);

                // Assert
                Assert.That(model.OutputIsEmpty, Is.False);

                var restartOutput = model.RestartOutput.ToArray();
                Assert.That(restartOutput, Has.Length.EqualTo(5));

                for (var i = 0; i < 5; i++)
                {
                    Assert.That(restartOutput[i].Name, Is.EqualTo(Path.GetFileName(restartFiles[i])));
                }
            }
        }

        [Test]
        public void GivenRealTimeControlModel_WhenSetRestartInputToNull_ThrowsArgumentNullException()
        {
            // Given
            var model = new RealTimeControlModel();

            // When
            void Call() => model.RestartInput = null;

            // Then
            Assert.Throws<ArgumentNullException>(Call);
        }
        
        [Test]
        public void SetRestartInputToDuplicateOf_ThrowsOnNullArgument()
        {
            // Setup
            var model = new RealTimeControlModel();

            // Call
            void Call() => model.SetRestartInputToDuplicateOf(null);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        public void SetRestartInputToDuplicateOf_CreatesCopyOfInput()
        {
            // Setup
            var model = new RealTimeControlModel();
            var restartFile = new RealTimeControlRestartFile( "file.ext", @"hello world" );
            Assert.That(model.RestartInput.Content, Is.Not.EqualTo(restartFile.Content));

            // Call
            model.SetRestartInputToDuplicateOf(restartFile);

            // Assert
            Assert.That(model.RestartInput, Is.Not.SameAs(restartFile));
            Assert.That(model.RestartInput.Content, Is.EqualTo(restartFile.Content));
            Assert.That(model.RestartInput.Name, Is.EqualTo(restartFile.Name));
            Assert.That(model.RestartInput.IsEmpty, Is.EqualTo(restartFile.IsEmpty));
        }



        [Test]
        public void ClearOutput_WithRestartOutput_ThenRestartOutputIsEmpty()
        {
            // Setup
            var realTimeControlModel = new RealTimeControlModelForTestingRestartOutput( new List<RealTimeControlRestartFile>(){ new RealTimeControlRestartFile() } );

            // Call
            realTimeControlModel.ClearOutput(true);

            // Assert
            Assert.That(realTimeControlModel.RestartOutput, Is.Empty);
        }

        [Test]
        public void ClearOutput_WithOutputDocuments_ThenOutputDocumentsIsEmpty()
        {
            // Setup
            var realTimeControlModel = new RealTimeControlModel();
            realTimeControlModel.OutputDocuments.Add(new ReadOnlyTextFileData("filename", "content", ReadOnlyTextFileDataType.Default));

            // Call
            realTimeControlModel.ClearOutput(true);

            // Assert
            Assert.That(realTimeControlModel.OutputDocuments, Is.Empty);
        }

        [Test]
        public void ClearOutput_WithOutputFunctions_ThenFunctionStoreIsRemovedFromModel()
        {
            // Setup
            var realTimeControlModel = new RealTimeControlModel();
            var function = Substitute.For<IFunction>();
            realTimeControlModel.OutputFileFunctionStore = new RealTimeControlOutputFileFunctionStore();
            realTimeControlModel.OutputFileFunctionStore.Functions.Add(function);

            // Call
            realTimeControlModel.ClearOutput(true);

            // Assert
            Assert.That(realTimeControlModel.OutputFileFunctionStore, Is.Null);
        }

        [Test]
        public void FileExceptionsCleaningWorkingDirectory_ShouldAlwaysReturnEmptyCollection()
        {
            using (var model = new RealTimeControlModel())
            {
                Assert.That(model.IgnoredFilePathsWhenCleaningWorkingDirectory, Is.Empty);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModel_WhenSetStartTime_ThenOutputMarkedOutOfSync()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                RealTimeControlModel model = CreateRtcModelAndFiles(tempDir, out string rtcFolderName, out string rtcDirectory, out string[] _);
                CreateRestartFiles(tempDir, rtcFolderName);
                model.ConnectOutput(rtcDirectory);

                // Precondition
                Assert.That(model.OutputOutOfSync, Is.False);

                // When
                model.StartTime = DateTime.Now;

                // Then
                Assert.That(model.OutputOutOfSync, Is.True);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModel_WhenSetStopTime_ThenOutputMarkedOutOfSync()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                RealTimeControlModel model = CreateRtcModelAndFiles(tempDir, out string rtcFolderName, out string rtcDirectory, out string[] _);
                CreateRestartFiles(tempDir, rtcFolderName);
                model.ConnectOutput(rtcDirectory);

                // Precondition
                Assert.That(model.OutputOutOfSync, Is.False);

                // When
                model.StopTime = DateTime.Now;

                // Then
                Assert.That(model.OutputOutOfSync, Is.True);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModel_WhenSetTimeStep_ThenOutputMarkedOutOfSync()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                RealTimeControlModel model = CreateRtcModelAndFiles(tempDir, out string rtcFolderName, out string rtcDirectory, out string[] _);
                CreateRestartFiles(tempDir, rtcFolderName);
                model.ConnectOutput(rtcDirectory);

                // Precondition
                Assert.That(model.OutputOutOfSync, Is.False);

                // When
                model.TimeStep = TimeSpan.FromDays(1);

                // Then
                Assert.That(model.OutputOutOfSync, Is.True);
            }
        }

        [Test]
        public void GetUpToDateDataItemName_ReturnsIdentity()
        {
            const string inputValue = "I like big data items and I cannot lie";
            using (var model = new RealTimeControlModel())
            {
                string result = model.GetUpToDateDataItemName(inputValue);

                Assert.That(result, Is.EqualTo(inputValue));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModelWithOutput_WhenAddControlGroup_ThenOutputPresentAndMarkedOutOfSync()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                RealTimeControlModel model = CreateRtcModelAndFiles(tempDir, out string rtcFolderName, out string rtcDirectory, out string[] _);
                CreateRestartFiles(tempDir, rtcFolderName);
                model.ConnectOutput(rtcDirectory);

                // Precondition
                Assert.That(model.OutputOutOfSync, Is.False);
                Assert.That(model.OutputIsEmpty, Is.False);

                // When
                model.ControlGroups.Add(new ControlGroup());

                // Then
                Assert.That(model.OutputOutOfSync, Is.True);
                Assert.That(model.OutputIsEmpty, Is.False);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModelWithOutput_WhenRemoveControlGroup_ThenOutputPresentAndMarkedOutOfSync()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                RealTimeControlModel model = CreateRtcModelAndFiles(tempDir, out string rtcFolderName, out string rtcDirectory, out string[] _);
                model.ControlGroups.Add(new ControlGroup());

                CreateRestartFiles(tempDir, rtcFolderName);
                model.ConnectOutput(rtcDirectory);

                // Precondition
                Assert.That(model.OutputOutOfSync, Is.False);
                Assert.That(model.OutputIsEmpty, Is.False);

                // When
                model.ControlGroups.RemoveAt(0);

                // Then
                Assert.That(model.OutputOutOfSync, Is.True);
                Assert.That(model.OutputIsEmpty, Is.False);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void GivenRealTimeControlModelWithOutput_WhenUpdateControlGroup_ThenOutputPresentAndMarkedOutOfSync()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                RealTimeControlModel model = CreateRtcModelAndFiles(tempDir, out string rtcFolderName, out string rtcDirectory, out string[] _);
                var controlGroup = new ControlGroup();
                model.ControlGroups.Add(controlGroup);

                CreateRestartFiles(tempDir, rtcFolderName);
                model.ConnectOutput(rtcDirectory);

                // Precondition
                Assert.That(model.OutputOutOfSync, Is.False);
                Assert.That(model.OutputIsEmpty, Is.False);

                // When
                controlGroup.Inputs.Add(new Input());
                controlGroup.Inputs.RemoveAt(0);

                // Then
                Assert.That(model.OutputOutOfSync, Is.True);
                Assert.That(model.OutputIsEmpty, Is.False);
            }
        }

        [Test]
        public void GetExchangeIdentifier_ReturnsDataItemName()
        {
            // Setup
            using (var model = new RealTimeControlModel())
            {
                var dataItem = Substitute.For<IDataItem>();
                dataItem.Name.Returns("randomName");
                
                // Call
                string identifier = ((ICoupledModel)model).GetExchangeIdentifier(dataItem);
                
                // Assert
                Assert.That(identifier, Is.EqualTo(dataItem.Name));
            }
        }

        [Test]
        public void GetDataItemsByExchangeIdentifier_ReturnsAllDataItemsWithGivenName()
        {
            // Setup
            using (var model = new RealTimeControlModel())
            {
                // Add some DataItems with names
                const string randomName = "randomName";
                const string anotherRandomName = "anotherRandomName";

                model.ControlGroups.Add(new ControlGroup() { Name = randomName });
                model.ControlGroups.Add(new ControlGroup() { Name = anotherRandomName });
                model.ControlGroups.Add(new ControlGroup() { Name = randomName });

                // Call
                IReadOnlyList<IDataItem> dataItems = model.GetDataItemsByExchangeIdentifier(randomName).ToArray();

                // Assert
                Assert.That(dataItems, Has.Exactly(2).Items);
                Assert.That(dataItems, Has.All.Matches<IDataItem>(dataItem => dataItem.Name == randomName));
            }
        }

        private static RealTimeControlModel CreateRtcModelAndFiles(TemporaryDirectory tempDir, out string rtcFolderName, out string rtcDirectory, out string[] relevantFiles)
        {
            var model = new RealTimeControlModel();

            rtcFolderName = "rtc";
            rtcDirectory = Path.Combine(tempDir.Path, rtcFolderName);
            Directory.CreateDirectory(rtcDirectory);
            string[] xmlFiles = CreateXmlFiles(tempDir, rtcFolderName).ToArray();
            string[] csvFiles = CreateCsvFiles(tempDir, rtcFolderName).ToArray();

            relevantFiles = xmlFiles.Concat(csvFiles).ToArray();
            Array.Sort(relevantFiles);
            return model;
        }

        private static void ChecksForOutputXmlOrCsvDocuments(RealTimeControlModel model, string[] relevantFiles)
        {
            ReadOnlyTextFileData[] textDocuments = model.OutputDocuments.ToArray();
            Assert.That(textDocuments, Has.Length.EqualTo(10));

            for (var i = 0; i < relevantFiles.Length; i++)
            {
                Assert.That(textDocuments[i].DocumentName, Is.EqualTo(Path.GetFileName(relevantFiles[i])));
                Assert.That(textDocuments[i].Content, Is.EqualTo($"file {i}"));
            }
        }

        private static IEnumerable<string> CreateXmlFiles(TemporaryDirectory tempDir, string rtcFolderName)
        {
            for (var i = 0; i < 5; i++)
            {
                yield return tempDir.CreateFile(Path.Combine(rtcFolderName, $"test{i}.xml"), $"file {(i * 2) + 1}");
            }
        }

        private static IEnumerable<string> CreateCsvFiles(TemporaryDirectory tempDir, string rtcFolderName)
        {
            for (var i = 0; i < 5; i++)
            {
                yield return tempDir.CreateFile(Path.Combine(rtcFolderName, $"test{i}.csv"), $"file {i * 2}");
            }
        }

        private static IEnumerable<string> CreateRestartFiles(TemporaryDirectory tempDir, string rtcFolderName)
        {
            for (var i = 0; i < 5; i++)
            {
                yield return tempDir.CreateFile(Path.Combine(rtcFolderName, $"rtc_1234567{i}_123456.xml"), $"file {i}");
            }
        }

        # region Syncing controlled models, control group items, model settings, etc.

        [Test]
        public void TestChangingControlGroupName_IsRevertedAndGivesWarningWhenDuplicateNameExists()
        {
            var rtcModel = new RealTimeControlModel();
            var controlGroup1 = new ControlGroup() {Name = "ControlGroup1"};
            var controlGroup2 = new ControlGroup() {Name = "ControlGroup2"};

            rtcModel.ControlGroups.Add(controlGroup1);
            rtcModel.ControlGroups.Add(controlGroup2);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => { controlGroup2.Name = "ControlGroup1"; },
                                                           string.Format(Resources.RealTimeControlModel_ControlGroupsPropertyChanged_Unable_to_update_ControlGroup_name__all_ControlGroup_names_must_be_unique__0___1___has_been_reverted_back_to___2__,
                                                                         Environment.NewLine, "ControlGroup1", "ControlGroup2"));

            Assert.AreEqual("ControlGroup1", controlGroup1.Name);
            Assert.AreEqual("ControlGroup2", controlGroup2.Name);
        }

        [Test]
        public void TestChangingControlGroupName_SucceedsAndGivesNoWarningWhenNoDuplicateNameExists()
        {
            var rtcModel = new RealTimeControlModel();
            var controlGroup1 = new ControlGroup() {Name = "ControlGroup1"};
            var controlGroup2 = new ControlGroup() {Name = "ControlGroup2"};

            rtcModel.ControlGroups.Add(controlGroup1);
            rtcModel.ControlGroups.Add(controlGroup2);

            Assert.Throws<AssertionException>(() =>
                                                  TestHelper.AssertAtLeastOneLogMessagesContains(() => { controlGroup2.Name = "ControlGroup3"; },
                                                                                                 string.Format(Resources.RealTimeControlModel_ControlGroupsPropertyChanged_Unable_to_update_ControlGroup_name__all_ControlGroup_names_must_be_unique__0___1___has_been_reverted_back_to___2__,
                                                                                                               Environment.NewLine, "ControlGroup1", "ControlGroup2")),
                                              "Warning message was logged where we did not expect it to be");

            Assert.AreEqual("ControlGroup1", controlGroup1.Name);
            Assert.AreEqual("ControlGroup3", controlGroup2.Name);
        }

        [Test]
        public void TestChangingControlGroupName_WhenNoDuplicateNameExists_UpdatesChildDataItemNames()
        {
            var rtcModel = new RealTimeControlModel();
            var controlGroup1 = new ControlGroup() {Name = "ControlGroup1"};
            var controlGroup2 = new ControlGroup() {Name = "ControlGroup2"};

            controlGroup1.Inputs.Add(new Input());
            controlGroup1.Outputs.Add(new Output());

            controlGroup2.Inputs.Add(new Input());
            controlGroup2.Outputs.Add(new Output());

            rtcModel.ControlGroups.Add(controlGroup1);
            rtcModel.ControlGroups.Add(controlGroup2);

            IDataItem controlGroup1DataItem = rtcModel.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, controlGroup1));
            Assert.NotNull(controlGroup1DataItem);
            Assert.AreEqual(2, controlGroup1DataItem.Children.Count);

            IDataItem controlGroup2DataItem = rtcModel.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, controlGroup2));
            Assert.NotNull(controlGroup2DataItem);
            Assert.AreEqual(2, controlGroup2DataItem.Children.Count);

            IDataItem controlGroup1InputDataItem = controlGroup1DataItem.Children.FirstOrDefault(di => di.Role.HasFlag(DataItemRole.Input));
            Assert.NotNull(controlGroup1InputDataItem);

            IDataItem controlGroup1OutputDataItem = controlGroup1DataItem.Children.FirstOrDefault(di => di.Role.HasFlag(DataItemRole.Output));
            Assert.NotNull(controlGroup1OutputDataItem);

            IDataItem controlGroup2InputDataItem = controlGroup2DataItem.Children.FirstOrDefault(di => di.Role.HasFlag(DataItemRole.Input));
            Assert.NotNull(controlGroup2InputDataItem);

            IDataItem controlGroup2OutputDataItem = controlGroup2DataItem.Children.FirstOrDefault(di => di.Role.HasFlag(DataItemRole.Output));
            Assert.NotNull(controlGroup2OutputDataItem);

            Assert.IsTrue(controlGroup1InputDataItem.Name.StartsWith(controlGroup1.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup1OutputDataItem.Name.StartsWith(controlGroup1.Name + RealTimeControlModel.OutputPostFix));
            Assert.IsTrue(controlGroup2InputDataItem.Name.StartsWith(controlGroup2.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup2OutputDataItem.Name.StartsWith(controlGroup2.Name + RealTimeControlModel.OutputPostFix));

            controlGroup1.Name = "ControlGroup3";
            controlGroup2.Name = "ControlGroup4";

            Assert.AreEqual("ControlGroup3", controlGroup1.Name);
            Assert.AreEqual("ControlGroup4", controlGroup2.Name);

            Assert.IsTrue(controlGroup1InputDataItem.Name.StartsWith(controlGroup1.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup1OutputDataItem.Name.StartsWith(controlGroup1.Name + RealTimeControlModel.OutputPostFix));
            Assert.IsTrue(controlGroup2InputDataItem.Name.StartsWith(controlGroup2.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup2OutputDataItem.Name.StartsWith(controlGroup2.Name + RealTimeControlModel.OutputPostFix));

            controlGroup1.Name = "ControlGroup5";

            Assert.AreEqual("ControlGroup5", controlGroup1.Name);
            Assert.AreEqual("ControlGroup4", controlGroup2.Name);

            Assert.IsTrue(controlGroup1InputDataItem.Name.StartsWith(controlGroup1.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup1OutputDataItem.Name.StartsWith(controlGroup1.Name + RealTimeControlModel.OutputPostFix));
            Assert.IsTrue(controlGroup2InputDataItem.Name.StartsWith(controlGroup2.Name + RealTimeControlModel.InputPostFix));
            Assert.IsTrue(controlGroup2OutputDataItem.Name.StartsWith(controlGroup2.Name + RealTimeControlModel.OutputPostFix));
        }

        [Test]
        public void TestControlledModelsAreAddedAutomaticallyAfterOwnerIsSet()
        {
            var compositeActivity = new TestCompositeActivity();
            var realTimeControlModel = new RealTimeControlModel();

            Assert.AreEqual(0, realTimeControlModel.ControlledModels.Count());

            compositeActivity.Activities.Add(realTimeControlModel);

            Assert.AreEqual(0, realTimeControlModel.ControlledModels.Count());

            compositeActivity.Activities.Add(new ControlledTestModel());

            Assert.AreEqual(1, realTimeControlModel.ControlledModels.Count());
        }

        [Test]
        public void TestControlledModelsAreAddedAutomaticallyIfOwnerIsSet()
        {
            var compositeActivity = new TestCompositeActivity();
            var realTimeControlModel = new RealTimeControlModel();

            compositeActivity.Activities.Add(new ControlledTestModel());
            compositeActivity.Activities.Add(realTimeControlModel);

            Assert.AreEqual(1, realTimeControlModel.ControlledModels.Count());
        }

        [Test]
        public void TestControlledModelsAreRemovedAutomaticallyAfterOwnerIsUnset()
        {
            var compositeActivity = new TestCompositeActivity();
            var realTimeControlModel = new RealTimeControlModel();
            var controlledTestModel = new ControlledTestModel();

            compositeActivity.Activities.Add(controlledTestModel);
            compositeActivity.Activities.Add(realTimeControlModel);
            compositeActivity.Activities.Remove(controlledTestModel);

            Assert.AreEqual(0, realTimeControlModel.ControlledModels.Count());
        }

        [Test]
        public void TestControlledModelsAreRemovedAutomaticallyIfOwnerIsUnset()
        {
            var compositeActivity = new TestCompositeActivity();
            var realTimeControlModel = new RealTimeControlModel();

            compositeActivity.Activities.Add(new ControlledTestModel());
            compositeActivity.Activities.Add(realTimeControlModel);
            compositeActivity.Activities.Remove(realTimeControlModel);

            Assert.AreEqual(0, realTimeControlModel.ControlledModels.Count());
        }

        [Test]
        public void RemoveLinkedInputFromControlGroup()
        {
            var input = new Input();
            var controlGroup = new ControlGroup
            {
                Inputs = {input},
                Outputs = {new Output()}
            };
            var realTimeControlModel = new RealTimeControlModel {ControlGroups = {controlGroup}};
            var outputParameter = new Parameter<double>
            {
                Name = "p1",
                Value = 1.0
            };
            var outputDataItem = new DataItem(outputParameter)
            {
                ValueType = typeof(double),
                ValueConverter = new PropertyValueConverter(outputParameter, "Value")
            };

            int dataItemCount = realTimeControlModel.AllDataItems.Count();

            realTimeControlModel.GetDataItemByValue(input).LinkTo(outputDataItem);

            controlGroup.Inputs.Clear();

            Assert.AreEqual(0, controlGroup.Inputs.Count);
            Assert.AreEqual(dataItemCount - 1, realTimeControlModel.AllDataItems.Count());
        }

        [Test]
        public void RemoveControlGroupWithLinkedInputItem()
        {
            var input = new Input();
            var controlGroup = new ControlGroup
            {
                Inputs = {input},
                Outputs = {new Output()}
            };
            var realTimeControlModel = new RealTimeControlModel {ControlGroups = {controlGroup}};
            var outputParameter = new Parameter<double>
            {
                Name = "p1",
                Value = 1.0
            };
            var outputDataItem = new DataItem(outputParameter)
            {
                ValueType = typeof(double),
                ValueConverter = new PropertyValueConverter(outputParameter, "Value")
            };

            realTimeControlModel.GetDataItemByValue(input).LinkTo(outputDataItem);

            int dataItemCount = realTimeControlModel.AllDataItems.Count();

            realTimeControlModel.ControlGroups.Clear();

            Assert.AreEqual(0, realTimeControlModel.ControlGroups.Count);
            Assert.AreEqual(dataItemCount - 3, realTimeControlModel.AllDataItems.Count());
        }

        [Test]
        public void RemoveControlGroupRemovesDataItem()
        {
            var input = new Input();
            var controlGroup = new ControlGroup
            {
                Inputs = {input},
                Outputs = {new Output()}
            };
            var realTimeControlModel = new RealTimeControlModel {ControlGroups = {controlGroup}};
            var outputParameter = new Parameter<double>
            {
                Name = "p1",
                Value = 1.0
            };
            var outputDataItem = new DataItem(outputParameter)
            {
                ValueType = typeof(double),
                ValueConverter = new PropertyValueConverter(outputParameter, "Value")
            };

            realTimeControlModel.GetDataItemByValue(input).LinkTo(outputDataItem);

            int rootDataItemCount = realTimeControlModel.DataItems.Count;

            realTimeControlModel.ControlGroups.Remove(controlGroup);

            Assert.AreEqual(rootDataItemCount - 1, realTimeControlModel.DataItems.Count);
        }

        [Test]
        public void RealTimeModelInheritsTimersFromControlledModel()
        {
            var testCompositeActivity = new TestCompositeActivity();
            var realTimeControlModel = new RealTimeControlModel();
            var controlledTestModel = new ControlledTestModel
            {
                StartTime = new DateTime(2000, 1, 1, 0, 0, 0),
                StopTime = new DateTime(2000, 1, 1, 6, 0, 0),
                TimeStep = new TimeSpan(0, 1, 0, 0)
            };

            testCompositeActivity.Activities.Add(realTimeControlModel);
            testCompositeActivity.Activities.Add(controlledTestModel);

            Assert.AreEqual(controlledTestModel.StartTime, realTimeControlModel.StartTime);
            Assert.AreEqual(controlledTestModel.StopTime, realTimeControlModel.StopTime);
            Assert.AreEqual(controlledTestModel.TimeStep, realTimeControlModel.TimeStep);
        }

        [Test]
        public void FeatureIsPropagatedToInputAfterLinking()
        {
            // Create domain objects
            var weir = new Structure();

            var input = new Input();
            var realTimeControlModel = new RealTimeControlModel {ControlGroups = {new ControlGroup {Inputs = {input}}}};

            // Create/query data items
            var weirCrestLevelDataItem = new DataItem
            {
                ValueType = typeof(double),
                ValueConverter = new PropertyValueConverter(weir, "CrestLevel")
            };
            IDataItem inputRtcDataItem = realTimeControlModel.GetDataItemByValue(input);

            // Link
            inputRtcDataItem.LinkTo(weirCrestLevelDataItem);

            input.Feature.Should("feature from source data item value converter is set in input").Be.EqualTo(weir);
        }

        [Test]
        public void FeatureIsPropagatedToOutputAfterLinking()
        {
            // Create domain objects
            var weir = new Structure();

            var output = new Output();
            var realTimeControlModel = new RealTimeControlModel {ControlGroups = {new ControlGroup {Outputs = {output}}}};

            // Create/query data items
            var weirCrestLevelDataItem = new DataItem
            {
                ValueType = typeof(double),
                ValueConverter = new PropertyValueConverter(weir, "CrestLevel")
            };
            IDataItem outputRtcDataItem = realTimeControlModel.GetDataItemByValue(output);

            // Link
            outputRtcDataItem.LinkTo(weirCrestLevelDataItem);

            output.Feature.Should("feature from source data item value converter is set in output").Be.EqualTo(weir);
        }

        [Test]
        public void ClearFeatureInInputOnUnlink()
        {
            // Create domain objects
            var weir = new Structure();

            var input = new Input();
            var realTimeControlModel = new RealTimeControlModel {ControlGroups = {new ControlGroup {Inputs = {input}}}};

            // Create/query data items
            var weirCrestLevelDataItem = new DataItem
            {
                ValueType = typeof(double),
                ValueConverter = new PropertyValueConverter(weir, "CrestLevel")
            };
            IDataItem inputRtcDataItem = realTimeControlModel.GetDataItemByValue(input);

            // Link
            inputRtcDataItem.LinkTo(weirCrestLevelDataItem);

            inputRtcDataItem.Unlink();

            input.Feature.Should("feature is cleared in rtc Input after unlink").Be.Null();
        }

        [Test]
        public void ClearFeatureInOutputOnUnlink()
        {
            // Create domain objects
            var weir = new Structure();

            var output = new Output();
            var realTimeControlModel = new RealTimeControlModel {ControlGroups = {new ControlGroup {Outputs = {output}}}};

            // Create/query data items
            var weirCrestLevelDataItem = new DataItem
            {
                ValueType = typeof(double),
                ValueConverter = new PropertyValueConverter(weir, "CrestLevel")
            };
            IDataItem outputRtcDataItem = realTimeControlModel.GetDataItemByValue(output);

            // link
            outputRtcDataItem.LinkTo(weirCrestLevelDataItem);

            outputRtcDataItem.Unlink();

            output.Feature.Should("feature is cleared in rtc Output after unlink").Be.Null();
        }

        [Test]
        public void LinkingOutputShouldResultInIsConnected()
        {
            var realTimeControlModel = new RealTimeControlModel();
            var controlGroup = new ControlGroup {Outputs = {new Output()}};

            realTimeControlModel.ControlGroups.Add(controlGroup);

            var intputDataItem = new DataItem
            {
                Value = 12.0,
                ValueConverter = new FeaturePropertyValueConverter(new RtcTestFeature(), "Value")
            };

            intputDataItem.LinkTo(realTimeControlModel.GetDataItemByValue(controlGroup.Outputs[0]));

            Assert.IsTrue(realTimeControlModel.ControlGroups.First().Outputs.First().IsConnected);
        }

        [Test]
        public void LinkingInputShouldResultInIsConnected()
        {
            var realTimeControlModel = new RealTimeControlModel();
            var controlGroup = new ControlGroup {Inputs = {new Input()}};

            realTimeControlModel.ControlGroups.Add(controlGroup);

            var outputDataItem = new DataItem
            {
                Value = 12.0,
                ValueConverter = new FeaturePropertyValueConverter(new RtcTestFeature(), "Value")
            };

            realTimeControlModel.GetDataItemByValue(controlGroup.Inputs[0]).LinkTo(outputDataItem);

            Assert.IsTrue(controlGroup.Inputs.First().IsConnected);
        }

        [Test]
        public void ChildDataItemsAreCreatedForInputsAndOutputs()
        {
            var controlGroup = new ControlGroup
            {
                Outputs = {new Output()},
                Inputs = {new Input()}
            };
            var realTimeControlModel = new RealTimeControlModel();

            realTimeControlModel.ControlGroups.Add(controlGroup);

            IDataItem controlGroupDataItem = realTimeControlModel.GetDataItemByValue(controlGroup);

            Assert.AreEqual(2, controlGroupDataItem.Children.Count);

            IDataItem inputDataItem = controlGroupDataItem.Children[0];
            Assert.AreEqual(controlGroup.Inputs[0], inputDataItem.ValueConverter.OriginalValue);
            Assert.AreEqual(controlGroupDataItem, inputDataItem.Parent);

            IDataItem outputDataItem = controlGroupDataItem.Children[1];
            Assert.AreEqual(controlGroup.Outputs[0], outputDataItem.ValueConverter.OriginalValue);
            Assert.AreEqual(controlGroupDataItem, outputDataItem.Parent);
        }

        [Test]
        public void TestOnly1ChildDataItemIsAddedWhenAddingAConnectionPoint()
        {
            var rtcModel = new RealTimeControlModel();
            var controlGroup = new ControlGroup();
            rtcModel.ControlGroups.Add(controlGroup);

            IDataItem controlGroupDataItem = rtcModel.DataItems.FirstOrDefault(di => ReferenceEquals(di.Value, controlGroup));
            Assert.NotNull(controlGroupDataItem);
            Assert.IsFalse(controlGroupDataItem.Children.Any());

            controlGroup.Inputs.Add(new Input());
            Assert.AreEqual(1, controlGroupDataItem.Children.Count);

            controlGroup.Outputs.Add(new Output());
            Assert.AreEqual(2, controlGroupDataItem.Children.Count);
        }

        # endregion

        # region Events handling (and refreshing)

        [Test]
        public void TestControlGroupPropertyChanged_IsHandledInRTCModel()
        {
            var rtcModel = new RealTimeControlModel();
            var controlGroup = new ControlGroup();
            var counter = 0;

            rtcModel.ControlGroups.Add(controlGroup);
            ((INotifyPropertyChanged) rtcModel.ControlGroups).PropertyChanged += (sender, e) => { counter++; };

            Assert.AreEqual(0, counter);
            controlGroup.Name = "Renamed";
            Assert.AreEqual(1, counter);
        }

        [Test]
        public void TestControlGroupPropertyChanging_IsHandledInRTCModel()
        {
            var rtcModel = new RealTimeControlModel();
            var controlGroup = new ControlGroup();
            var counter = 0;

            rtcModel.ControlGroups.Add(controlGroup);
            ((INotifyPropertyChanging) rtcModel.ControlGroups).PropertyChanging += (sender, e) => { counter++; };

            Assert.AreEqual(0, counter);
            controlGroup.Name = "Renamed";
            Assert.AreEqual(1, counter);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestPropertyChangedBubbling()
        {
            ControlGroup controlGroup = RealTimeControlTestHelper.CreateGroup2Rules();
            var propertyChangedCount = 0;
            var propertyName = "";
            object sender = null;

            ((INotifyPropertyChanged) controlGroup).PropertyChanged += (s, e) =>
            {
                propertyChangedCount++;
                propertyName = e.PropertyName;
                sender = s;
            };

            controlGroup.Rules[0].Name = "Rumpelstiltskin";

            Assert.AreEqual(1, propertyChangedCount);
            Assert.AreEqual("Name", propertyName);
            Assert.AreEqual(controlGroup.Rules[0], sender);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Slow)]
        [NUnit.Framework.Category(TestCategory.Wpf)]
        public void RulePropertyChangedShouldRefreshTreeView()
        {
            using (var gui = CreateGui())
            {
                Action onShown = delegate
                {
                    IProjectService projectService = gui.Application.ProjectService;
                    Project project = projectService.CreateProject();
                    
                    IProjectExplorer projectExplorer = gui.MainWindow.ProjectExplorer;
                    var realTimeControlModel = new RealTimeControlModel("Test RTC Model");
                    ControlGroup controlGroup = RealTimeControlTestHelper.CreateGroup2Rules();

                    realTimeControlModel.ControlGroups.Clear();
                    realTimeControlModel.ControlGroups.Add(controlGroup);

                    project.RootFolder.Add(realTimeControlModel);

                    ITreeView treeView = projectExplorer.TreeView;

                    treeView.Refresh();

                    projectExplorer.TreeView.WaitUntilAllEventsAreProcessed();

                    ITreeNode nodeModel = treeView.GetNodeByTag(realTimeControlModel);

                    nodeModel.Expand(); // model

                    ITreeNode nodeInput = nodeModel.Nodes[0];
                    nodeInput.Expand();

                    ITreeNode nodeControlGroups = nodeInput.Nodes.First(n => n.Text == "Control Groups");
                    nodeControlGroups.Expand();          // controlGroups
                    nodeControlGroups.Nodes[0].Expand(); // controlGroup

                    ITreeNode nodeCondition = nodeControlGroups.Nodes[0].GetNodeByTag(controlGroup.Conditions[0]);

                    Assert.AreNotEqual("condition1", nodeCondition.Text);
                    controlGroup.Conditions[0].Name = "condition1";

                    treeView.Refresh();
                    treeView.WaitUntilAllEventsAreProcessed();

                    Assert.AreEqual("condition1", nodeCondition.Text);
                };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, onShown);
            }
        }

        # endregion

        # region Other

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void TestResetOrphanedControlGroupInputsAndOutputs()
        {
            // SOBEK3-562: Existing projects can have ControlGroups with locations at inputs/outputs but no underlying dataitem links

            ControlledTestModel controlledModel;
            RealTimeControlModel realTimeControlModel;
            RealTimeControlTestHelper.SetupControlledTestModel(out controlledModel, out realTimeControlModel);
            RealTimeControlTestHelper.SetupHydraulicRuleControlGroup(controlledModel, realTimeControlModel, true);

            ControlGroup controlGroup = realTimeControlModel.ControlGroups.FirstOrDefault();
            Assert.NotNull(controlGroup);

            foreach (Input input in controlGroup.Inputs)
            {
                // Manually recreate control group inputs with features but no underlying dataitem links
                IFeature feature = input.Feature;
                string parameter = input.ParameterName;
                string unit = input.UnitName;

                IDataItem inputDataItem = realTimeControlModel.GetDataItemByValue(input);
                inputDataItem.Unlink();

                input.Feature = feature;
                input.ParameterName = parameter;
                input.UnitName = unit;
            }

            foreach (Output output in controlGroup.Outputs)
            {
                // Manually recreate control group outputs with features but no underlying dataitem links
                IFeature feature = output.Feature;
                string parameter = output.ParameterName;
                string unit = output.UnitName;

                IDataItem outputDataItem = realTimeControlModel.GetDataItemByValue(output);
                List<IDataItem> toUnlink = outputDataItem.LinkedBy.ToList();
                foreach (IDataItem dataItem in toUnlink)
                {
                    dataItem.Unlink();
                }

                output.Feature = feature;
                output.ParameterName = parameter;
                output.UnitName = unit;
            }

            // Call Method
            realTimeControlModel.ResetOrphanedControlGroupInputsAndOutputs(controlGroup);

            // Assert that inputs and outputs have been reset
            foreach (Input input in controlGroup.Inputs)
            {
                Assert.AreEqual("[Not Set]", input.Name);
                Assert.IsNull(input.Feature);
            }

            foreach (Output output in controlGroup.Outputs)
            {
                Assert.AreEqual("[Not Set]", output.Name);
                Assert.IsNull(output.Feature);
            }
        }

        [Test]
        public void DimrExportDirectoryPath_ForGet_ShouldReturnNotSupportedException()
        {
            // Arrange
            var model = new RealTimeControlModel();

            // Act
            void Call()
            {
                string _ = model.DimrExportDirectoryPath;
            }

            // Assert
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void DimrExportDirectoryPath_ForSet_ShouldReturnNotImplementedException()
        {
            // Arrange
            var model = new RealTimeControlModel();

            // Act
            void Call()
            {
                model.DimrExportDirectoryPath = "test";
            }

            // Assert
            Assert.Throws<NotSupportedException>(Call);
        }

        [Test]
        public void UseRestart_WhenNoRestartFileHasBeenSetAsInitialCondition_ShouldReturnFalse()
        {
            // Arrange
            var model = new RealTimeControlModel();

            // Act, Assert
            Assert.IsFalse(model.UseRestart);
        }

        [Test]
        public void UseRestart_WhenARestartFileHasBeenSetAsInitialCondition_ShouldReturnTrue()
        {
            // Arrange
            var model = new RealTimeControlModel {RestartInput = new RealTimeControlRestartFile("test", "test")};

            // Act, Assert
            Assert.IsTrue(model.UseRestart);
        }

        [Test]
        public void SetRestartInput_PropertyReferencesAssignedInstance()
        {
            var rtcRestartFile = new RealTimeControlRestartFile();
            var model = new RealTimeControlModel();
            Assert.That( model.RestartInput, Is.Not.SameAs(rtcRestartFile) );
            
            model.RestartInput = rtcRestartFile;
            
            Assert.That( model.RestartInput, Is.SameAs(rtcRestartFile) );
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void SetRestartInputFromFile_CreatesInputRealTimeControlRestartFileWithNameAndContent()
        {
            var model = new RealTimeControlModel();
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string restartFilePath = Path.Combine(tempDirectory.Path, "test_rst.nc");
                const string content = @"foo";
                File.WriteAllText(restartFilePath, content);

                Assert.That(model.RestartInput.IsEmpty, Is.True);
                Assert.That(string.IsNullOrEmpty(model.RestartInput.Name), Is.True);

                // Call
                model.RestartInput = RealTimeControlRestartFile.CreateFromFile(restartFilePath);

                // Assert
                Assert.That(model.RestartInput.IsEmpty, Is.False);
                Assert.That(model.RestartInput.Name, Is.EqualTo(Path.GetFileName(restartFilePath)));
                Assert.That(model.RestartInput.Content, Is.EqualTo(content));
            }
        }

        [Test]
        public void UpgradeOfOldRestartInput_InvokingRestartInput_SetsInputRealTimeControlRestartFile()
        {
            // Setup
            var model = new RealTimeControlModelWithAccessibleRestartInput();
            var restartFile = new RealTimeControlRestartFile("dummy.path", "content");

            // Call
            model.InvokeProtectedRestartInput(restartFile);

            // Assert
            Assert.That(model.RestartInput, Is.SameAs(restartFile));
        }


        [Test]
        public void Constructor_RealTimeControlModelShouldBeInstanceOfICoupledModel()
        {
            // Arrange, Act
            var rtcModel = new RealTimeControlModel();

            // Assert
            Assert.IsInstanceOf<ICoupledModel>(rtcModel);
        }

        [Test]
        [TestCase(DataItemRole.Input)]
        [TestCase(DataItemRole.Output)]
        [TestCase(DataItemRole.None)]
        public void GetDataItemsUsedForCouplingModel_ForDifferentRoles_ShouldReturnCorrespondingDataItems(DataItemRole role)
        {
            // Arrange
            var rtcModel = new RealTimeControlModel();
            var dataItem = Substitute.For<IDataItem>();
            dataItem.Role.Returns(role);
            rtcModel.DataItems.Clear();
            rtcModel.DataItems.Add(dataItem);

            // Act
            IList<IDataItem> couplingDataItems = ((ICoupledModel) rtcModel).GetDataItemsUsedForCouplingModel(role).ToList();

            // Assert
            Assert.AreEqual(1, couplingDataItems.Count);
            Assert.AreSame(dataItem, couplingDataItems.First());
        }

        [Test]
        public void CreateNew_ShouldSetIsOpenAndPathProperties()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act
            const string filePath = "test";
            rtcModel.CreateNew(filePath);

            // Assert
            Assert.AreEqual(filePath, rtcModel.Path);
            Assert.IsTrue(rtcModel.IsOpen);
        }

        [Test]
        public void CreateNew_ForFilePathIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act
            void Call() => rtcModel.CreateNew(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.AreEqual("path", exception.ParamName);
        }

        [Test]
        public void CreateNew_ForRootedFilePath_ShouldThrowArgumentNullException()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act
            string filePath = Path.Combine("c:");
            void Call() => rtcModel.CreateNew(filePath);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.AreEqual("projectDirectory", exception.ParamName);
        }

        [Test]
        public void Close_ShouldSetIsOpenPropertyToFalse()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act
            rtcModel.Close();

            // Assert
            Assert.IsFalse(rtcModel.IsOpen);
        }

        [Test]
        public void Open_ShouldSetIsOpenPropertyToTrue()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act
            const string filePath = "test";
            rtcModel.Open(filePath);

            // Assert
            Assert.IsTrue(rtcModel.IsOpen);
        }

        [Test]
        public void Open_ForFilePathIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act
            void Call() => rtcModel.Open(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.AreEqual("path", exception.ParamName);
        }

        [Test]
        public void CopyTo_ForFilePathIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act
            void Call() => rtcModel.CopyTo(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.AreEqual("destinationPath", exception.ParamName);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void CopyTo_ForCurrentOutputDirectoryIsNull_ShouldDoNothing()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                IFileBased rtcModel = new RealTimeControlModel();

                // Act
                string filePath = Path.Combine(tempDirectory.Path, "RtcModel");
                rtcModel.CopyTo(filePath);

                // Assert
                var dir = new DirectoryInfo(tempDirectory.Path);
                DirectoryInfo[] subDirs = dir.GetDirectories();
                FileInfo[] subFiles = dir.GetFiles();

                CollectionAssert.IsEmpty(subDirs);
                CollectionAssert.IsEmpty(subFiles);
            }
        }

        [Test]
        public void SwitchTo_ForFilePathIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act
            void Call() => rtcModel.SwitchTo(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.AreEqual("newPath", exception.ParamName);
        }

        [Test]
        public void Path_ShouldReturnTheEarlierSetPath()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act
            const string filePath = "test";
            rtcModel.Path = filePath;

            // Assert
            Assert.AreEqual(filePath, rtcModel.Path);
        }

        [Test]
        public void Path_ShouldReturnNullAsDefaultValue()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act, Assert
            Assert.IsNull(rtcModel.Path);
        }

        [Test]
        public void Paths_ShouldReturnThePathProperty()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();
            rtcModel.Path = "test";

            // Act
            IList<string> paths = rtcModel.Paths.ToList();

            // Assert
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("test", paths.First());
        }

        [Test]
        public void IsFileCritical_ShouldReturnTrue()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act, Assert
            Assert.IsTrue(rtcModel.IsFileCritical);
        }

        [Test]
        public void IsOpen_ShouldReturnFalseAsDefaultValue()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act, Assert
            Assert.IsFalse(rtcModel.IsOpen);
        }

        [Test]
        public void CopyFromWorkingDirectory_ShouldReturnFalse()
        {
            // Arrange
            IFileBased rtcModel = new RealTimeControlModel();

            // Act, Assert
            Assert.IsFalse(rtcModel.CopyFromWorkingDirectory);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenNewProjectWithRTC_WhenSavingForFirstTime_NewDirectoryShouldNotExist()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSave = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string projectDirectoryAfterSave = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string filePathBeforeSave = Path.Combine(projectDirectoryBeforeSave, "RealTimeControlModelGUID");
                string filePathAfterSave = Path.Combine(projectDirectoryAfterSave, "RealTimeControlModelGUID");

                // When
                frameworkSimulator.NewProject(filePathBeforeSave);
                frameworkSimulator.FirstSave(filePathAfterSave);

                // Then
                // There aren't output files, so there is nothing to write.
                // Due to that model and project_data folder is also missing.
                DirectoryAssert.DoesNotExist(projectDirectoryAfterSave);

                Assert.IsTrue(((IFileBased) rtcModel).IsOpen);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenNewProjectWithRTC_WhenRunningAndSavingForFirstTime_NewPersistentOutputDirectoryShouldExistAndConnected()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSave = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSave = Path.Combine(projectDirectoryBeforeSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSave = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSave = Path.Combine(projectDirectoryAfterSave, "RealTimeControlModelGUID");

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName,
                                                  out string workingDirectoryOutputFileName, out string workingDirectoryOutputRestartFileName, out string workingDirectoryOutputSubDirectoryName,
                                                  out string workingDirectoryForRunning);

                // When
                frameworkSimulator.NewProject(pathBeforeSave);
                SimulateRun(rtcModel, workingDirectoryForRunning);
                frameworkSimulator.FirstSave(pathAfterSave);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryAfterSave, rtcModel, workingDirectoryOutputFileName, workingDirectoryOutputRestartFileName, workingDirectoryOutputSubDirectoryName);

                Assert.IsTrue(((IFileBased) rtcModel).IsOpen);
                Assert.AreEqual(1, rtcModel.OutputDocuments.Count);
                Assert.AreEqual(1, rtcModel.RestartOutput.Count());
                Assert.AreEqual(workingDirectoryOutputRestartFileName, rtcModel.RestartOutput.First().Name);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenNewProjectWithRTC_WhenSavingForFirstTimeRunningAndSavingAgain_NewPersistentOutputDirectoryShouldExistAndConnected()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSave = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSave = Path.Combine(projectDirectoryBeforeSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSave = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSave = Path.Combine(projectDirectoryAfterSave, "RealTimeControlModelGUID");

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName,
                                                  out string workingDirectoryOutputFileName, out string workingDirectoryOutputRestartFileName, out string workingDirectoryOutputSubDirectoryName,
                                                  out string workingDirectoryForRunning);

                // When
                frameworkSimulator.NewProject(pathBeforeSave);
                frameworkSimulator.FirstSave(pathAfterSave);
                SimulateRun(rtcModel, workingDirectoryForRunning);
                frameworkSimulator.Save(pathAfterSave);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryAfterSave, rtcModel, workingDirectoryOutputFileName, workingDirectoryOutputRestartFileName, workingDirectoryOutputSubDirectoryName);

                Assert.IsTrue(((IFileBased) rtcModel).IsOpen);
                Assert.AreEqual(1, rtcModel.OutputDocuments.Count);
                Assert.AreEqual(1, rtcModel.RestartOutput.Count());
                Assert.AreEqual(workingDirectoryOutputRestartFileName, rtcModel.RestartOutput.First().Name);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenNewProjectWithRTC_WhenSavingForFirstTimeRunningAndSavingAs_NewPersistentOutputDirectoryShouldExistAndConnected()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSave = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSave = Path.Combine(projectDirectoryBeforeSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSave = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSave = Path.Combine(projectDirectoryAfterSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSaveAs = Path.Combine(tempDirectory.Path, "ProjectAfterSaveAs_data");
                string pathAfterSaveAs = Path.Combine(projectDirectoryAfterSaveAs, "RealTimeControlModelGUID");

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName,
                                                  out string workingDirectoryOutputFileName, out string workingDirectoryOutputRestartFileName, out string workingDirectoryOutputSubDirectoryName,
                                                  out string workingDirectoryForRunning);

                // When
                frameworkSimulator.NewProject(pathBeforeSave);
                frameworkSimulator.FirstSave(pathAfterSave);
                SimulateRun(rtcModel, workingDirectoryForRunning);
                frameworkSimulator.SaveAs(pathAfterSaveAs);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryAfterSaveAs, rtcModel, workingDirectoryOutputFileName, workingDirectoryOutputRestartFileName, workingDirectoryOutputSubDirectoryName);
                Assert.IsFalse(Directory.Exists(projectDirectoryAfterSave));

                Assert.IsTrue(((IFileBased) rtcModel).IsOpen);
                Assert.AreEqual(1, rtcModel.OutputDocuments.Count);
                Assert.AreEqual(1, rtcModel.RestartOutput.Count());
                Assert.AreEqual(workingDirectoryOutputRestartFileName, rtcModel.RestartOutput.First().Name);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenAProjectWithRTCAndOutput_WhenOpened_ThenIsOpenShouldBeTruePathShouldBeAbsoluteAndOutputShouldBeConnected()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryPersistentFolder = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathPersistentFolder = Path.Combine(projectDirectoryPersistentFolder, "RealTimeControlModelGUID");

                BuildUpModelOutput(projectDirectoryPersistentFolder, rtcModel.Name, out string _, out string persistentOutputRestartFileName, out string _);

                // When
                frameworkSimulator.OpenProject(pathPersistentFolder);

                // Then
                Assert.IsTrue(((IFileBased) rtcModel).IsOpen);
                Assert.AreEqual(pathPersistentFolder, ((IFileBased) rtcModel).Path);
                Assert.AreEqual(1, rtcModel.OutputDocuments.Count);
                Assert.AreEqual(1, rtcModel.RestartOutput.Count());
                Assert.AreEqual(persistentOutputRestartFileName, rtcModel.RestartOutput.First().Name);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenAnOpenedProjectWithRTCAndOutput_WhenDeletingOutputFilesInFileExplorerAndSaving_ThenPersistentOutputDirectoryShouldNotExistAnymore()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryPersistentFolder = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathPersistentFolder = Path.Combine(projectDirectoryPersistentFolder, "RealTimeControlModelGUID");

                string outputFolderPersistentFolder = Path.Combine(projectDirectoryPersistentFolder, rtcModel.Name, "output");
                Directory.CreateDirectory(outputFolderPersistentFolder);

                // When
                frameworkSimulator.OpenProject(pathPersistentFolder);
                Directory.Delete(outputFolderPersistentFolder);
                frameworkSimulator.Save(pathPersistentFolder);

                // Then
                Assert.IsFalse(Directory.Exists(outputFolderPersistentFolder));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenAnOpenedProjectWithRTCAndOutput_WhenSaving_ThenPersistentOutputDirectoryShouldContainOriginalOutputFiles()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryPersistentFolder = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathPersistentFolder = Path.Combine(projectDirectoryPersistentFolder, "RealTimeControlModelGUID");

                BuildUpModelOutput(projectDirectoryPersistentFolder, rtcModel.Name, out string persistentOutputFileName, out string persistentOutputRestartFileName, out string persistentOutputSubDirectoryName);

                // When
                frameworkSimulator.OpenProject(pathPersistentFolder);
                frameworkSimulator.Save(pathPersistentFolder);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryPersistentFolder, rtcModel, persistentOutputFileName, persistentOutputRestartFileName, persistentOutputSubDirectoryName);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenAnOpenedProjectWithRTCAndOutput_WhenSavingAs_ThenNewPersistentOutputDirectoryShouldContainOriginalOutputFiles()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSaveAs = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSaveAs = Path.Combine(projectDirectoryBeforeSaveAs, "RealTimeControlModelGUID");

                BuildUpModelOutput(projectDirectoryBeforeSaveAs, rtcModel.Name, out string persistentOutputFileName, out string persistentOutputRestartFileName, out string persistentOutputSubDirectoryName);

                string projectDirectoryAfterSaveAs = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSaveAs = Path.Combine(projectDirectoryAfterSaveAs, "RealTimeControlModelGUID");

                // When
                frameworkSimulator.OpenProject(pathBeforeSaveAs);
                frameworkSimulator.SaveAs(pathAfterSaveAs);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryAfterSaveAs, rtcModel, persistentOutputFileName, persistentOutputRestartFileName, persistentOutputSubDirectoryName);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenAnOpenedProjectWithRTCAndOutput_WhenRunningAndSaving_ThenPersistentOutputDirectoryShouldContainFilesFromLastRun()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryPersistentFolder = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathPersistentFolder = Path.Combine(projectDirectoryPersistentFolder, "RealTimeControlModelGUID");

                BuildUpModelOutput(projectDirectoryPersistentFolder, rtcModel.Name, out string _, out string _, out string _);
                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName, out string workingDirectoryOutputFileName, out string workingDirectoryOutputRestartFileName, out string workingDirectoryOutputSubDirectoryName,
                                                  out string workingDirectoryForRunning);

                // When
                frameworkSimulator.OpenProject(pathPersistentFolder);
                SimulateRun(rtcModel, workingDirectoryForRunning);
                frameworkSimulator.Save(pathPersistentFolder);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryPersistentFolder, rtcModel, workingDirectoryOutputFileName, workingDirectoryOutputRestartFileName, workingDirectoryOutputSubDirectoryName);

                Thread.Sleep(3000);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenAnOpenedProjectWithRTCAndOutput_WhenRunningAndSavingAs_ThenNewPersistentOutputDirectoryShouldContainFilesFromLastRun()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSaveAs = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSaveAs = Path.Combine(projectDirectoryBeforeSaveAs, "RealTimeControlModelGUID");

                BuildUpModelOutput(projectDirectoryBeforeSaveAs, rtcModel.Name, out string _, out string _, out string _);

                string projectDirectoryAfterSaveAs = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSaveAs = Path.Combine(projectDirectoryAfterSaveAs, "RealTimeControlModelGUID");

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName, out string workingDirectoryOutputFileName, out string workingDirectoryOutputRestartFileName, out string workingDirectoryOutputSubDirectoryName,
                                                  out string workingDirectoryForRunning);

                // When
                frameworkSimulator.OpenProject(pathBeforeSaveAs);
                SimulateRun(rtcModel, workingDirectoryForRunning);
                frameworkSimulator.SaveAs(pathAfterSaveAs);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryAfterSaveAs, rtcModel, workingDirectoryOutputFileName, workingDirectoryOutputRestartFileName, workingDirectoryOutputSubDirectoryName);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenAnOpenedProjectWithRTCAndOutput_WhenRemovingModelFromProject_ThenPersistentOutputDirectoryShouldNotBeDeleted()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSaveAs = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSaveAs = Path.Combine(projectDirectoryBeforeSaveAs, "RealTimeControlModelGUID");

                BuildUpModelOutput(projectDirectoryBeforeSaveAs, rtcModel.Name, out string persistentOutputFileName, out string persistentOutputRestartFileName, out string persistentOutputSubDirectoryName);

                // When
                frameworkSimulator.OpenProject(pathBeforeSaveAs);
                frameworkSimulator.RemoveModelFromProject();

                // Then
                AssertsPersistentFolderStructure(projectDirectoryBeforeSaveAs, rtcModel, persistentOutputFileName, persistentOutputRestartFileName, persistentOutputSubDirectoryName);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenNewProjectWithRTC_WhenSavingForFirstTimeRenamingRunningAndThenSavingAgain_PersistentOutputDirectoryShouldBeBasedOnNewModelName()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSave = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSave = Path.Combine(projectDirectoryBeforeSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSave = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSave = Path.Combine(projectDirectoryAfterSave, "RealTimeControlModelGUID");

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName,
                                                  out string workingDirectoryOutputFileName, out string workingDirectoryOutputRestartFileName, out string workingDirectoryOutputSubDirectoryName,
                                                  out string workingDirectoryForRunning);

                // When
                frameworkSimulator.NewProject(pathBeforeSave);
                frameworkSimulator.FirstSave(pathAfterSave);
                rtcModel.Name = "rtc2";
                SimulateRun(rtcModel, workingDirectoryForRunning);
                frameworkSimulator.Save(pathAfterSave);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryAfterSave, rtcModel, workingDirectoryOutputFileName, workingDirectoryOutputRestartFileName, workingDirectoryOutputSubDirectoryName);

                Assert.IsTrue(((IFileBased) rtcModel).IsOpen);
                Assert.AreEqual(1, rtcModel.OutputDocuments.Count);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenNewProjectWithRTC_WhenSavingForFirstTimeRunningRenamingAndThenSavingAgain_PersistentOutputDirectoryShouldBeBasedOnNewModelName()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSave = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSave = Path.Combine(projectDirectoryBeforeSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSave = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSave = Path.Combine(projectDirectoryAfterSave, "RealTimeControlModelGUID");

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName,
                                                  out string workingDirectoryOutputFileName, out string workingDirectoryOutputRestartFileName, out string workingDirectoryOutputSubDirectoryName,
                                                  out string workingDirectoryForRunning);

                // When
                frameworkSimulator.NewProject(pathBeforeSave);
                frameworkSimulator.FirstSave(pathAfterSave);
                SimulateRun(rtcModel, workingDirectoryForRunning);
                rtcModel.Name = "rtc2";
                frameworkSimulator.Save(pathAfterSave);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryAfterSave, rtcModel, workingDirectoryOutputFileName, workingDirectoryOutputRestartFileName, workingDirectoryOutputSubDirectoryName);

                Assert.IsTrue(((IFileBased) rtcModel).IsOpen);
                Assert.AreEqual(1, rtcModel.OutputDocuments.Count);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenNewProjectWithRTC_WhenRunningSavingForFirstTimeRenamingAndThenSavingAgain_PersistentOutputDirectoryShouldBeBasedOnNewModelNameAndOldOneShouldBeRemoved()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                rtcModel.Name = "rtc1";
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSave = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSave = Path.Combine(projectDirectoryBeforeSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSave = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSave = Path.Combine(projectDirectoryAfterSave, "RealTimeControlModelGUID");

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName,
                                                  out string workingDirectoryOutputFileName, out string workingDirectoryOutputRestartFileName, out string workingDirectoryOutputSubDirectoryName,
                                                  out string workingDirectoryForRunning);

                // When
                frameworkSimulator.NewProject(pathBeforeSave);
                SimulateRun(rtcModel, workingDirectoryForRunning);
                frameworkSimulator.FirstSave(pathAfterSave);

                // test precondition
                Assert.IsTrue(Directory.Exists(Path.Combine(projectDirectoryAfterSave, "rtc1")));

                rtcModel.Name = "rtc2";
                frameworkSimulator.Save(pathAfterSave);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryAfterSave, rtcModel, workingDirectoryOutputFileName, workingDirectoryOutputRestartFileName, workingDirectoryOutputSubDirectoryName);
                Assert.IsFalse(Directory.Exists(Path.Combine(projectDirectoryAfterSave, "rtc1")));

                Assert.IsTrue(((IFileBased) rtcModel).IsOpen);
                Assert.AreEqual(1, rtcModel.OutputDocuments.Count);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenNewProjectWithRTC_WhenRunningSavingForFirstTimeRenamingRunningAndThenSavingAgain_PersistentOutputDirectoryShouldBeBasedOnNewModelNameAndOldOneShouldBeRemoved()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                rtcModel.Name = "rtc1";
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSave = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSave = Path.Combine(projectDirectoryBeforeSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSave = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSave = Path.Combine(projectDirectoryAfterSave, "RealTimeControlModelGUID");

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName,
                                                  out string workingDirectoryOutputFileName, out string workingDirectoryOutputRestartFileName, out string workingDirectoryOutputSubDirectoryName,
                                                  out string workingDirectoryForRunning);

                // When
                frameworkSimulator.NewProject(pathBeforeSave);
                SimulateRun(rtcModel, workingDirectoryForRunning);
                frameworkSimulator.FirstSave(pathAfterSave);

                // test precondition
                Assert.IsTrue(Directory.Exists(Path.Combine(projectDirectoryAfterSave, "rtc1")));

                rtcModel.Name = "rtc2";

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName,
                                                  out string _, out string _, out string _,
                                                  out string _);

                SimulateRun(rtcModel, workingDirectoryForRunning);
                frameworkSimulator.Save(pathAfterSave);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryAfterSave, rtcModel, workingDirectoryOutputFileName, workingDirectoryOutputRestartFileName, workingDirectoryOutputSubDirectoryName);
                Assert.IsFalse(Directory.Exists(Path.Combine(projectDirectoryAfterSave, "rtc1")));
                Assert.IsTrue(Directory.Exists(Path.Combine(workingDirectoryForRunning, rtcModel.DirectoryName, DirectoryNameConstants.OutputDirectoryName)));

                Assert.IsTrue(((IFileBased) rtcModel).IsOpen);
                Assert.AreEqual(1, rtcModel.OutputDocuments.Count);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenNewProjectWithRTC_WhenRunningSavingForFirstTimeRenamingAndThenSavingAs_PersistentOutputDirectoryShouldBeBasedOnNewModelNameAndOldOneShouldStay()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                rtcModel.Name = "rtc1";
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSave = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSave = Path.Combine(projectDirectoryBeforeSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSave = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSave = Path.Combine(projectDirectoryAfterSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSecondSaveAs = Path.Combine(tempDirectory.Path, "ProjectAfterSecondSave_data");
                string pathAfterSecondSaveAs = Path.Combine(projectDirectoryAfterSecondSaveAs, "RealTimeControlModelGUID");

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName,
                                                  out string workingDirectoryOutputFileName, out string workingDirectoryOutputRestartFileName, out string workingDirectoryOutputSubDirectoryName,
                                                  out string workingDirectoryForRunning);

                // When
                frameworkSimulator.NewProject(pathBeforeSave);
                SimulateRun(rtcModel, workingDirectoryForRunning);
                frameworkSimulator.FirstSave(pathAfterSave);

                // test precondition
                Assert.IsTrue(Directory.Exists(Path.Combine(projectDirectoryAfterSave, "rtc1")));

                rtcModel.Name = "rtc2";
                frameworkSimulator.SaveAs(pathAfterSecondSaveAs);

                // Then
                AssertsPersistentFolderStructure(projectDirectoryAfterSecondSaveAs, rtcModel, workingDirectoryOutputFileName, workingDirectoryOutputRestartFileName, workingDirectoryOutputSubDirectoryName);
                Assert.IsTrue(Directory.Exists(Path.Combine(projectDirectoryAfterSave, "rtc1")));

                Assert.IsTrue(((IFileBased) rtcModel).IsOpen);
                Assert.AreEqual(1, rtcModel.OutputDocuments.Count);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenNewProjectWithRTC_WhenRunningAndClearingAndSavingForFirstTime_ThenNoOutputFolderWrittenToProjectFolder()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSave = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSave = Path.Combine(projectDirectoryBeforeSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSave = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSave = Path.Combine(projectDirectoryAfterSave, "RealTimeControlModelGUID");

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName,
                                                  out string _, out string _, out string _,
                                                  out string workingDirectoryForRunning);

                // When
                frameworkSimulator.NewProject(pathBeforeSave);
                SimulateRun(rtcModel, workingDirectoryForRunning);

                ReadOnlyTextFileData[] outputDocumentsBeforeClear = rtcModel.OutputDocuments.ToArray();
                Assert.That(outputDocumentsBeforeClear, Has.Length.AtLeast(1));

                rtcModel.ClearOutput(true);
                frameworkSimulator.FirstSave(pathAfterSave);

                // Then
                Assert.That(!Directory.Exists(Path.Combine(projectDirectoryAfterSave, rtcModel.Name, DirectoryNameConstants.OutputDirectoryName)));

                Assert.That(((IFileBased) rtcModel).IsOpen);
                Assert.That(rtcModel.OutputDocuments.Count, Is.EqualTo(0));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenSavedProjectWithRTCOutput_WhenClearOutputAndSaveAs_ThenNoOutputFolderWrittenToNewProjectFolder()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Given
                var rtcModel = new RealTimeControlModel();
                var frameworkSimulator = new DeltaShellFrameworkSimulator(rtcModel);

                string projectDirectoryBeforeSave = Path.Combine(tempDirectory.Path, "ProjectBeforeSave_data");
                string pathBeforeSave = Path.Combine(projectDirectoryBeforeSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSave = Path.Combine(tempDirectory.Path, "ProjectAfterSave_data");
                string pathAfterSave = Path.Combine(projectDirectoryAfterSave, "RealTimeControlModelGUID");

                string projectDirectoryAfterSaveAs = Path.Combine(tempDirectory.Path, "ProjectAfterSaveAs_data");
                string pathAfterSaveAs = Path.Combine(projectDirectoryAfterSaveAs, "RealTimeControlModelGUID");

                BuildUpWorkingDirectoryWithOutput(tempDirectory, rtcModel.DirectoryName,
                                                  out string _, out string _, out string _,
                                                  out string workingDirectoryForRunning);

                frameworkSimulator.NewProject(pathBeforeSave);
                SimulateRun(rtcModel, workingDirectoryForRunning);
                frameworkSimulator.FirstSave(pathAfterSave);

                // When
                rtcModel.ClearOutput(true);
                frameworkSimulator.SaveAs(pathAfterSaveAs);

                // Then
                Assert.That(Directory.Exists(Path.Combine(projectDirectoryAfterSave, rtcModel.Name, DirectoryNameConstants.OutputDirectoryName)));
                Assert.That(!Directory.Exists(Path.Combine(projectDirectoryAfterSaveAs, rtcModel.Name, DirectoryNameConstants.OutputDirectoryName)));

                Assert.IsTrue(((IFileBased) rtcModel).IsOpen);
                Assert.AreEqual(0, rtcModel.OutputDocuments.Count);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void OnFinishIntegratedModelRun_ShouldOnlyMoveOutputFilesAndDirectoriesInRtcFolderToSeparateOutputFolder()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                var rtcModel = new RealTimeControlModel();
                string workingDirectoryIntegratedModel = Path.Combine(tempDirectory.Path, "IntegratedModel");
                string runRtcDirectory = Path.Combine(workingDirectoryIntegratedModel, rtcModel.DirectoryName);

                string outputSubDirectoryPath = Path.Combine(runRtcDirectory, "OutputSubFolder");
                string outputFilePath = Path.Combine(runRtcDirectory, "output.txt");
                string outputFileInSubFolderPath = Path.Combine(outputSubDirectoryPath, "output.txt");
                Directory.CreateDirectory(outputSubDirectoryPath);
                File.WriteAllText(outputFilePath, "test");
                File.WriteAllText(outputFileInSubFolderPath, "test");

                string inputSubDirectoryPath = Path.Combine(runRtcDirectory, "InputSubFolder");
                string inputFilePath = Path.Combine(runRtcDirectory, "input.txt");
                string inputFileInSubFolderPath = Path.Combine(inputSubDirectoryPath, "input.txt");
                Directory.CreateDirectory(inputSubDirectoryPath);
                File.WriteAllText(inputFilePath, "test");
                File.WriteAllText(inputFileInSubFolderPath, "test");

                rtcModel.LastExportedPaths = new[]
                {
                    inputFilePath,
                    inputSubDirectoryPath
                };

                // Act
                rtcModel.OnFinishIntegratedModelRun(workingDirectoryIntegratedModel);

                // Assert
                string outputFolderPathAfterOnFinish = Path.Combine(runRtcDirectory, DirectoryNameConstants.OutputDirectoryName);
                string outputFilePathAfterOnFinish = Path.Combine(outputFolderPathAfterOnFinish, "output.txt");

                string outputSubDirectoryPathAfterOnFinish = Path.Combine(outputFolderPathAfterOnFinish, "OutputSubFolder");
                string outputFileInSubFolderPathAfterOnFinish = Path.Combine(outputSubDirectoryPathAfterOnFinish, "output.txt");

                Assert.AreEqual(1, Directory.GetFiles(runRtcDirectory).Length);
                Assert.IsTrue(File.Exists(inputFilePath));
                Assert.AreEqual(2, Directory.GetDirectories(runRtcDirectory).Length);
                Assert.IsTrue(Directory.Exists(inputSubDirectoryPath));
                Assert.IsTrue(Directory.Exists(outputFolderPathAfterOnFinish));

                Assert.AreEqual(1, Directory.GetFiles(outputFolderPathAfterOnFinish).Length);
                Assert.IsTrue(File.Exists(outputFilePathAfterOnFinish));
                Assert.AreEqual(1, Directory.GetDirectories(outputFolderPathAfterOnFinish).Length);
                Assert.IsTrue(Directory.Exists(outputSubDirectoryPathAfterOnFinish));

                Assert.AreEqual(1, Directory.GetFiles(outputSubDirectoryPathAfterOnFinish).Length);
                Assert.IsTrue(File.Exists(outputFileInSubFolderPathAfterOnFinish));
                Assert.AreEqual(0, Directory.GetDirectories(outputSubDirectoryPathAfterOnFinish).Length);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void OnFinishIntegratedModelRun_WhenCommunicationFilesAreExisting_ShouldAlsoMoveTheseFilesToRtcOutputFolder()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                var rtcModel = new RealTimeControlModel();
                string workingDirectoryIntegratedModel = Path.Combine(tempDirectory.Path, "IntegratedModel");
                string runRtcDirectory = Path.Combine(workingDirectoryIntegratedModel, rtcModel.DirectoryName);

                Directory.CreateDirectory(runRtcDirectory);

                File.WriteAllText(Path.Combine(workingDirectoryIntegratedModel, rtcModel.CommunicationRtcToFmFileName), "test");
                File.WriteAllText(Path.Combine(workingDirectoryIntegratedModel, rtcModel.CommunicationFmToRtcFileName), "test");

                // Act
                rtcModel.OnFinishIntegratedModelRun(workingDirectoryIntegratedModel);

                // Assert
                string outputFolderPathAfterOnFinish = Path.Combine(runRtcDirectory, DirectoryNameConstants.OutputDirectoryName);
                string[] filePaths = Directory.GetFiles(outputFolderPathAfterOnFinish);
                Assert.AreEqual(2, filePaths.Length);
                Assert.IsTrue(filePaths.Any(f => f.Contains(rtcModel.CommunicationRtcToFmFileName)));
                Assert.IsTrue(filePaths.Any(f => f.Contains(rtcModel.CommunicationFmToRtcFileName)));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void OnFinishIntegratedModelRun_WhenCommunicationFilesAreNotExisting_ShouldDoNothing()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                var rtcModel = new RealTimeControlModel();
                string workingDirectoryIntegratedModel = Path.Combine(tempDirectory.Path, "IntegratedModel");
                string runRtcDirectory = Path.Combine(workingDirectoryIntegratedModel, rtcModel.DirectoryName);

                Directory.CreateDirectory(runRtcDirectory);

                // Act
                rtcModel.OnFinishIntegratedModelRun(workingDirectoryIntegratedModel);

                // Assert
                string outputFolderPathAfterOnFinish = Path.Combine(runRtcDirectory, DirectoryNameConstants.OutputDirectoryName);
                string[] filePaths = Directory.GetFiles(outputFolderPathAfterOnFinish);
                CollectionAssert.IsEmpty(filePaths);
            }
        }

        [Test]
        public void CommunicationRtcToFmFileName_ShouldReturnCorrectDefaultValue()
        {
            // Arrange
            var rtcModel = new RealTimeControlModel();

            // Act, Assert
            Assert.AreEqual("rtc_to_flow.nc", rtcModel.CommunicationRtcToFmFileName);
        }

        [Test]
        public void CommunicationRtcToFmFileName_WhenSet_ShouldAddNcExtensionToValue()
        {
            // Arrange
            var rtcModel = new RealTimeControlModel();

            // Act
            rtcModel.CommunicationRtcToFmFileName = "test";

            // Assert
            Assert.AreEqual("test.nc", rtcModel.CommunicationRtcToFmFileName);
        }

        [Test]
        public void CommunicationFmToRtcFileName_ShouldReturnCorrectDefaultValue()
        {
            // Arrange
            var rtcModel = new RealTimeControlModel();

            // Act, Assert
            Assert.AreEqual("flow_to_rtc.nc", rtcModel.CommunicationFmToRtcFileName);
        }

        [Test]
        public void CommunicationFmToRtcFileName_WhenSet_ShouldAddNcExtensionToValue()
        {
            // Arrange
            var rtcModel = new RealTimeControlModel();

            // Act
            rtcModel.CommunicationFmToRtcFileName = "test";

            // Assert
            Assert.AreEqual("test.nc", rtcModel.CommunicationFmToRtcFileName);
        }

        #endregion

        #region Helper functions

        private static void BuildUpModelOutput(string projectDirectoryPersistentFolder, string rtcModelName,
                                               out string persistentOutputFileName, out string persistentOutputRestartFileName, 
                                               out string persistentOutputSubDirectoryName)
        {
            persistentOutputFileName = "OriginalOutputFile.xml";
            persistentOutputRestartFileName = "rtc_12345678_123456.xml";
            persistentOutputSubDirectoryName = "OriginalOutputSubDirectory";

            string outputFolderPersistentFolder = Path.Combine(projectDirectoryPersistentFolder, rtcModelName, DirectoryNameConstants.OutputDirectoryName);
            Directory.CreateDirectory(outputFolderPersistentFolder);

            string originalOutputFile = Path.Combine(outputFolderPersistentFolder, persistentOutputFileName);
            File.WriteAllText(originalOutputFile, "Original");

            string originalOutputRestartFile = Path.Combine(outputFolderPersistentFolder, persistentOutputRestartFileName);
            File.WriteAllText(originalOutputRestartFile, "Original");

            string originalOutputSubDirectory = Path.Combine(outputFolderPersistentFolder, persistentOutputSubDirectoryName);
            Directory.CreateDirectory(originalOutputSubDirectory);

            string originalOutputSubDirectoryFile = Path.Combine(originalOutputSubDirectory, persistentOutputFileName);
            File.WriteAllText(originalOutputSubDirectoryFile, "Original");
        }

        private static void BuildUpWorkingDirectoryWithOutput(TemporaryDirectory tempDirectory, string rtcModelDirectoryName,
                                                              out string workingDirectoryOutputFileName, out string workingDirectoryOutputRestartFileName, out string workingDirectoryOutputSubDirectoryName,
                                                              out string workingDirectoryForRunning)
        {
            workingDirectoryOutputFileName = "WorkingDirectoryOutputFile.xml";
            workingDirectoryOutputRestartFileName = "rtc_12345678_123456.xml";
            workingDirectoryOutputSubDirectoryName = "WorkingDirectoryOutputSubDirectory";
            workingDirectoryForRunning = Path.Combine(tempDirectory.Path, "DeltaShell_Working_Directory");
            FileUtils.DeleteIfExists(workingDirectoryForRunning);

            string workingDirectoryOutputFolder = Path.Combine(workingDirectoryForRunning, rtcModelDirectoryName);
            Directory.CreateDirectory(workingDirectoryOutputFolder);

            string workingDirectoryFile = Path.Combine(workingDirectoryOutputFolder, workingDirectoryOutputFileName);
            File.WriteAllText(workingDirectoryFile, "WD");

            string workingDirectoryOutputRestartFile = Path.Combine(workingDirectoryOutputFolder, workingDirectoryOutputRestartFileName);
            File.WriteAllText(workingDirectoryOutputRestartFile, "WD");

            string workingDirectorySubDirectory = Path.Combine(workingDirectoryOutputFolder, workingDirectoryOutputSubDirectoryName);
            Directory.CreateDirectory(workingDirectorySubDirectory);

            string workingDirectorySubDirectoryFile = Path.Combine(workingDirectorySubDirectory, workingDirectoryOutputFileName);
            File.WriteAllText(workingDirectorySubDirectoryFile, "WDSub");
        }

        private static void SimulateRun(RealTimeControlModel rtcModel, string workingDirectoryForRunning)
        {
            rtcModel.OnFinishIntegratedModelRun(workingDirectoryForRunning);
            rtcModel.ConnectOutput(Path.Combine(workingDirectoryForRunning, "rtc", DirectoryNameConstants.OutputDirectoryName));
        }

        private static void AssertsPersistentFolderStructure(string projectDirectoryAfterSave, RealTimeControlModel rtcModel, string outputFileName, string outputRestartFileName, string outputSubDirectoryName)
        {
            string outputFolderAfterSave = Path.Combine(projectDirectoryAfterSave, rtcModel.Name, DirectoryNameConstants.OutputDirectoryName);
            string expectedOutputFileAfterSave = Path.Combine(outputFolderAfterSave, outputFileName);
            string expectedOutputRestartFileAfterSave = Path.Combine(outputFolderAfterSave, outputRestartFileName);
            string expectedOutputSubFolderAfterSave = Path.Combine(outputFolderAfterSave, outputSubDirectoryName);
            string expectedOutputFileInSubFolderAfterSave = Path.Combine(expectedOutputSubFolderAfterSave, outputFileName);

            Assert.IsTrue(Directory.Exists(outputFolderAfterSave));
            Assert.IsTrue(File.Exists(expectedOutputFileAfterSave));
            Assert.IsTrue(File.Exists(expectedOutputRestartFileAfterSave));
            Assert.AreEqual(2, Directory.GetFiles(outputFolderAfterSave).Length);
            Assert.AreEqual(1, Directory.GetDirectories(outputFolderAfterSave).Length);
            Assert.IsTrue(Directory.Exists(expectedOutputSubFolderAfterSave));
            Assert.IsTrue(File.Exists(expectedOutputFileInSubFolderAfterSave));
            Assert.AreEqual(1, Directory.GetFiles(expectedOutputSubFolderAfterSave).Length);
            Assert.AreEqual(0, Directory.GetDirectories(expectedOutputSubFolderAfterSave).Length);
        }

        private static void CheckResetConnectionPoint(ConnectionPoint retrievedConnectionPointFromRtcModel)
        {
            Assert.AreEqual("[Not Set]", retrievedConnectionPointFromRtcModel.Name, "Name of the connection point should have been reset");
            Assert.IsEmpty(retrievedConnectionPointFromRtcModel.ParameterName, "Parameter name of the connection point should have been reset");
            Assert.IsEmpty(retrievedConnectionPointFromRtcModel.UnitName, "Unit name of the connection point should have been reset");
        }
        
        private static IGui CreateGui()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new CommonToolsApplicationPlugin(),
                new RealTimeControlApplicationPlugin(),
                new CommonToolsGuiPlugin(),
                new ProjectExplorerGuiPlugin(),
                new RealTimeControlGuiPlugin(),
            };
            var gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
            
            gui.Run();
            return gui;
        }
        
        /// <summary>
        /// This class allows to invoke the protected setter for RestartInput which is called when upgrading the model to v3.9.0.0
        /// </summary>
        internal class RealTimeControlModelWithAccessibleRestartInput : RealTimeControlModel
        {
            public void InvokeProtectedRestartInput(RealTimeControlRestartFile restartFile)
            {
                RestartInput = restartFile;
            }
        }

        # endregion
    }
}