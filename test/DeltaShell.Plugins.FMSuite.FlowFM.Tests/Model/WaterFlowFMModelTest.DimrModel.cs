using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Core.Workflow.DataItems.ValueConverters;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net.Core;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        [Test]
        public void GetDataItemsByItemString_ReturnsExpectedDataItem()
        {
            // Given
            var fmModel = new WaterFlowFMModel();
            const string gateName = "structure01";
            var gate = new Structure
            {
                Name = gateName,
                Formula = new SimpleGateFormula()
            };
            fmModel.Area.Structures.Add(gate);

            // When
            var itemStringComponents = new List<string>(new[]
            {
                KnownFeatureCategories.Gates,
                gateName,
                KnownStructureProperties.CrestLevel
            });
            string itemString = string.Join("/", itemStringComponents);

            IEnumerable<IDataItem> dataItems = fmModel.GetDataItemsByItemString(itemString).ToArray();

            // Then
            AssertDataItemIsGate(dataItems.Single(), gate);
        }

        [Test]
        public void GetDataItemByItemString_ForItemStringContainingOnly2Elements_ThrowArgumentException()
        {
            // Given
            var fmModel = new WaterFlowFMModel();

            // When
            var itemStringComponents = new List<string>(new[]
            {
                KnownFeatureCategories.Gates,
                KnownStructureProperties.CrestLevel
            });
            string itemString = string.Join("/", itemStringComponents);

            void Call() => fmModel.GetDataItemsByItemString(itemString);

            // Then
            var ex = Assert.Throws<ArgumentException>(Call);
            Assert.AreEqual($"{itemString} should contain a category, feature name and a parameter name.", ex.Message,
                            "The exception message is different than expected");
        }

        [Test]
        public void GetDataItemByItemString_ForItemStringContainingUnknownFeatureName_ThrowArgumentException()
        {
            // Given
            var fmModel = new WaterFlowFMModel();

            // When
            const string featureName = "NotExisting";

            var itemStringComponents = new List<string>(new[]
            {
                KnownFeatureCategories.Gates,
                featureName,
                KnownStructureProperties.CrestLevel
            });
            string itemString = string.Join("/", itemStringComponents);

            void Call() => fmModel.GetDataItemsByItemString(itemString);

            // Then
            var ex = Assert.Throws<ArgumentException>(Call);
            Assert.AreEqual($"feature {featureName} in {itemString} cannot be found in the FM model.", ex.Message,
                            "The exception message is different than expected");
        }

        [Test]
        public void GetDataItemByItemString_ForItemStringContainingUnknownParameterName_ThrowArgumentException()
        {
            // Given
            var fmModel = new WaterFlowFMModel();
            var gate = new Structure
            {
                Name = "structure01",
                Formula = new SimpleGateFormula()
            };
            fmModel.Area.Structures.Add(gate);

            // When
            const string parameterName = "NotExisting";

            var itemStringComponents = new List<string>(new[]
            {
                KnownFeatureCategories.Gates,
                gate.Name,
                parameterName
            });
            string itemString = string.Join("/", itemStringComponents);

            void Call() => fmModel.GetDataItemsByItemString(itemString);

            // Then
            var ex = Assert.Throws<ArgumentException>(Call);
            Assert.AreEqual($"parameter name {parameterName} in {KnownFeatureCategories.Gates}/{gate.Name}/{parameterName} cannot be found in the FM model.",
                            ex.Message, "The exception message is different than expected");
        }

        [Test]
        public void OnFinishIntegratedModelRun_WhenUseCachingIsTrue_SetsCacheFileToTheCorrectWorkingDirectory()
        {
            string workingDirectoryIntegratedModel = Path.Combine(Path.GetTempPath(), "IntegratedModel");

            // Setup
            using (var model = new WaterFlowFMModel())
            {
                // Call
                model.OnFinishIntegratedModelRun(workingDirectoryIntegratedModel);

                string expectedPath = Path.Combine(workingDirectoryIntegratedModel, model.DirectoryName,
                                                   Path.ChangeExtension(model.Name,
                                                                        FileConstants.CachingFileExtension));

                // Assert
                Assert.That(model.CacheFile.Path, Is.EqualTo(expectedPath), "Expected a different path:");
            }
        }

        [Test]
        public void OnFinishIntegratedModelRun_WhenUseCachingIsFalse_SetsCacheFileToTheCorrectWorkingDirectory()
        {
            string workingDirectoryIntegratedModel = Path.Combine(Path.GetTempPath(), "IntegratedModel");

            // Setup
            using (var model = new WaterFlowFMModel())
            {
                model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).SetValueFromString("false");
                // Call
                model.OnFinishIntegratedModelRun(workingDirectoryIntegratedModel);

                // Assert
                Assert.IsNull(model.CacheFile.Path);
            }
        }

        [Test]
        public void OnFinishIntegratedModelRun_WriteRestartOn_LogsCorrectWarning()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                model.ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value = true;

                // Precondition
                Assert.That(model.WriteRestart, Is.True);

                // Call
                void Call() => model.OnFinishIntegratedModelRun("");

                // Assert
                IEnumerable<string> warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn);
                Assert.That(warnings, Contains.Item("Please save the project after a model run with 'write restart' on."));
            }
        }

        [Test]
        public void OnFinishIntegratedModelRun_WriteRestartOff_DoesNotLogWarning()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                model.ModelDefinition.GetModelProperty(GuiProperties.WriteRstFile).Value = false;

                // Precondition
                Assert.That(model.WriteRestart, Is.False);

                // Call
                void Call() => model.OnFinishIntegratedModelRun("");

                // Assert
                IEnumerable<string> warnings = TestHelper.GetAllRenderedMessages(Call, Level.Warn);
                Assert.That(warnings, Is.Empty);
            }
        }

        [Test]
        [TestCase(@"dflowfm")]
        [TestCase(@"dflowfm\computations\test\JAMM\D2776")]
        public void FileExceptionsCleaningWorkingDirectory_WhenUseCachingIsTrueAndCacheFilePathSetToWorkingDirectory_ShouldReturnCacheFilePath(string mduDirectory)
        {
            using (var model = new WaterFlowFMModel())
            {
                string runMduPath = Path.Combine(model.WorkingDirectoryPath, mduDirectory, $"{model.Name}{FileConstants.MduFileExtension}");
                model.CacheFile.UpdatePathToMduLocation(runMduPath);
                model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).SetValueFromString("true");

                ISet<string> ignoredFilePaths = model.IgnoredFilePathsWhenCleaningWorkingDirectory;

                string[] expectedFileExceptions = { Path.ChangeExtension(runMduPath, FileConstants.CachingFileExtension) };
                Assert.That(ignoredFilePaths, Is.EquivalentTo(expectedFileExceptions));
            }
        }

        [Test]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, true)]
        public void FileExceptionsCleaningWorkingDirectory_WhenUseCachingIsFalseAndOrCacheFilePathNotSetToWorkingDirectory_ShouldReturnNoFiles(bool cacheFileInWorkingDirectory, bool useCaching)
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                string workingDir = cacheFileInWorkingDirectory ? model.WorkingDirectoryPath : "NotWorkingDirectory";
                string runMduPath = Path.Combine(workingDir, "dflowfm", $"{model.Name}{FileConstants.MduFileExtension}");
                model.CacheFile.UpdatePathToMduLocation(runMduPath);
                
                model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).SetValueFromString(useCaching.ToString());

                // Call | Assert
                Assert.That(model.IgnoredFilePathsWhenCleaningWorkingDirectory, Is.Empty);
            }
        }

        [Test]
        [TestCase(@"dflowfm")]
        [TestCase(@"dflowfm\computations\test\JAMM\D2776")]
        [Category(TestCategory.Integration)]
        public void OnInitialize_WhenCacheFilePathIsInWorkingDirectoryAndUseCachingTrue_ShouldNotRemoveThisCacheFileBeforeANewRunStarts(string mduDirectory)
        {
            // Arrange
            using (var tempDirectory = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel())
            {
                string tempPath = tempDirectory.Path;
                model.WorkingDirectoryPathFunc = () => tempPath;
                
                string modelExportDirectoryPath = Path.Combine(model.WorkingDirectoryPath, mduDirectory);
                Directory.CreateDirectory(modelExportDirectoryPath);
                string runMduPath = Path.Combine(modelExportDirectoryPath, $"{model.Name}{FileConstants.MduFileExtension}");
                
                string cacheFilePath = Path.ChangeExtension(runMduPath, FileConstants.CachingFileExtension);
                File.WriteAllText(cacheFilePath, "test");
                string shouldBeClearedFilePath = Path.Combine(modelExportDirectoryPath, "test.txt");
                File.WriteAllText(shouldBeClearedFilePath, "test");
                
                model.CacheFile.UpdatePathToMduLocation(runMduPath);
                model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).SetValueFromString("true");

                model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(5, 5, 20, 20);

                // Act
                model.Initialize();

                // Assert
                Assert.IsFalse(File.Exists(shouldBeClearedFilePath));
                Assert.IsTrue(File.Exists(cacheFilePath));
            }
        }

        private static void AssertDataItemIsGate(IDataItem dataItem, Structure gate)
        {
            const string messageDifferentFeatureInDataItem = "The retrieved dataItem is not correct, since the features are not the same";
            const string messageDifferentParameterInDataItem = "The retrieved dataItem is not correct, since the parameters are not the same";

            var dataItemParameterConverter = (ParameterValueConverter) dataItem.ValueConverter;

            Assert.That(dataItemParameterConverter.Location, Is.EqualTo(gate), messageDifferentFeatureInDataItem);
            Assert.That(dataItem.Name, Is.EqualTo(gate.Name), messageDifferentFeatureInDataItem);
            Assert.That(dataItemParameterConverter.ParameterName, Is.EqualTo(KnownStructureProperties.CrestLevel), messageDifferentParameterInDataItem);
            Assert.That(dataItem.Tag, Is.EqualTo(KnownStructureProperties.CrestLevel), messageDifferentParameterInDataItem);
        }
    }
}