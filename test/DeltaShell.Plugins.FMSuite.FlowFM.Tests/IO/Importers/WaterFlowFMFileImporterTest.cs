using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class WaterFlowFMFileImporterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var importer = new WaterFlowFMFileImporter(null);

            // Assert
            Assert.That(importer, Is.InstanceOf<ModelFileImporterBase>());
            Assert.That(importer, Is.InstanceOf<IDimrModelFileImporter>());
            Assert.That(importer.Name, Is.EqualTo("Flow Flexible Mesh Model"));
            Assert.That(importer.Category, Is.EqualTo("1D / 2D Model"));
            Assert.That(importer.Description, Is.EqualTo("Imports a Flow Flexible Mesh Model using the .mdu file"));
            Assert.That(importer.Image, Is.Not.Null);

            CollectionAssert.AreEqual(new[] {typeof(IHydroModel)}, importer.SupportedItemTypes);
            Assert.That(importer.CanImportOnRootLevel, Is.True);
            Assert.That(importer.FileFilter, Is.EqualTo("Flexible Mesh Model Definition|*.mdu"));
            Assert.That(importer.TargetDataDirectory, Is.Null);
            Assert.That(importer.ShouldCancel, Is.False);
            Assert.That(importer.ProgressChanged, Is.Null);
            Assert.That(importer.OpenViewAfterImport, Is.True);
        }

        [Test]
        [TestCaseSource(nameof(CanImportOnCases))]
        public void CanImportOn_VariousTargetObjects_ReturnsExpectedValue(object targetObject,
                                                                          bool expectedResult)
        {
            // Setup
            var importer = new WaterFlowFMFileImporter(null);

            // Call
            bool result = importer.CanImportOn(targetObject);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        private static IEnumerable<TestCaseData> CanImportOnCases()
        {
            yield return new TestCaseData(Substitute.For<ICompositeActivity>(), true);
            yield return new TestCaseData(new WaterFlowFMModel(), true);
            yield return new TestCaseData(new object(), false);
            yield return new TestCaseData(null, false);
        }
        
        [Test]
        [TestCase(null, ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(".", ExpectedResult = false)]
        [TestCase("settings.json", ExpectedResult = false)]
        [TestCase("settings.xml", ExpectedResult = false)]
        [TestCase("flowfm.mdu", ExpectedResult = true)]
        [TestCase("FLOWFM.MDU", ExpectedResult = true)]
        [TestCase("Sobek_3b.fnm", ExpectedResult = false)]
        public bool GivenARealTimeControlModelImporter_WithInputFile_ThenExpectedIsReturned(string path)
        {
            // Setup
            var importer = new WaterFlowFMFileImporter(null);

            // Call
            return importer.CanImportDimrFile(path);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenFmModelWithUnsupportedBoundaries_WhenImportItem_ThenDoesNotThrow()
        {
            const string relativeFilePath = @"c003_westerschelde_2d_dynamo\westerscheldt01.mdu";
            string testFilePath = TestHelper.GetTestFilePath(relativeFilePath);
            Assert.That(File.Exists(testFilePath));
            WaterFlowFMModel importedModel = null;

            // When
            var importer = new WaterFlowFMFileImporter(() => null);
            TestDelegate testAction = () => importedModel = (WaterFlowFMModel) importer.ImportItem(testFilePath);

            // Then
            Assert.That(testAction, Throws.Nothing);
            Assert.That(importedModel, Is.Not.Null);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenFmModelWithUnsupportedBoundaries_WhenImportItem_ThenLogsExpectedMessage()
        {
            const string relativeFilePath = @"c003_westerschelde_2d_dynamo\westerscheldt01.mdu";
            string testFilePath = TestHelper.GetTestFilePath(relativeFilePath);
            Assert.That(File.Exists(testFilePath));
            const int errorDataPoint = 0;
            const string errorComponent = "Tracer";
            const string errorBoundaryCondition = "RiverBoundary01-Green";
            var exceptionMessage = $"Specified argument was out of the range of valid values.\r\nParameter name: {errorComponent}";
            string expectedLogMssg = string.Format("Skipped DataPoint {0} for Boundary Condition {1} could not be added as the following exception was risen during import: {2}",
                                                   errorDataPoint, errorBoundaryCondition, exceptionMessage);
            WaterFlowFMModel importedModel = null;

            // When
            var importer = new WaterFlowFMFileImporter(() => null);
            TestHelper.AssertAtLeastOneLogMessagesContains( () => importedModel = (WaterFlowFMModel)importer.ImportItem(testFilePath), expectedLogMssg);

            // Then
            Assert.That(importedModel, Is.Not.Null);
        }
    }
}
