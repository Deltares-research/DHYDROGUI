using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
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

            Assert.That(importer.MasterFileExtension, Is.EqualTo("mdu"));
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
        [Category(TestCategory.Integration)]
        [Category("Quarantine")]
        public void GivenGeneralStructureMduFileWithUnhandledType_WhenImportItem_ThenNotImplementedExceptionThrown()
        {
            // Given
            const string relativeFilePath = @"c071_generalstructure_door_closing_at_sill\dflowfm\t2.mdu";
            string testFilePath = TestHelper.GetTestFilePath(relativeFilePath);
            Assert.That(File.Exists(testFilePath));
            const string propertyName = "Horizontal opening width: REALTIME (generalstructure)";
            var structureFactoryException = $"Trying to generate Time series for 2D Structure: Maeslantkering, property: {propertyName} mapped as GateOpeningWidth, type: External which is not yet supported.";

            // When
            var importer = new WaterFlowFMFileImporter(() => null);
            TestDelegate testAction = () => importer.ImportItem(testFilePath);

            // Then
            string[] renderedMessages = TestHelper.GetAllRenderedMessages(() =>
            {
                try
                {
                    testAction.Invoke();
                }
                catch
                {
                    // ignored
                }
            }).ToArray();
            
            Assert.That(renderedMessages[3].Contains(structureFactoryException));

        }

        [Test]
        [Category("Quarantine")]
        [Category(TestCategory.Integration)]
        public void GivenGeneralStructureMduFileWithUnhandledType_WhenImportItem_ThenLoggedExpectedMessages()
        {
            // Given
            const string relativeFilePath = @"c071_generalstructure_door_closing_at_sill\dflowfm\";
            string iniFilePath = TestHelper.GetTestFilePath(Path.Combine(relativeFilePath, "t2.mdu"));
            string structureFilePath = TestHelper.GetTestFilePath(Path.Combine(relativeFilePath, "tst-1_structures.ini"));
            Assert.That(File.Exists(iniFilePath));
            Assert.That(File.Exists(structureFilePath));
            const string propertyName = "Horizontal opening width: REALTIME (generalstructure)";
            const string structureName = "Maeslantkering";
            const string propertyFileName = "GateOpeningWidth";
            const string expectedMappedTime = "External";
            string structureFactoryException = 
                string.Format("Trying to generate Time series for 2D Structure: {0}, property: {1} mapped as {2}, type: {3} which is not yet supported.",
                                                             structureName, propertyName, propertyFileName, expectedMappedTime) + "\r\n";
            string structuresFileError =
                string.Format("Error while reading and converting 2D Structures from {0}", structureFilePath);
            string convertStructureError = string.Format("Failed to convert .ini structure definition \'{0}\' to actual structure.", structureName);
            string waterFlowFmFileImporterError = string.Format("Error while importing a {0} from {1}: {2}", "Flow Flexible Mesh Model", iniFilePath, string.Empty);

            // When
            var importer = new WaterFlowFMFileImporter(() => null);
            TestDelegate testAction = () => importer.ImportItem(iniFilePath);

            string[] renderedMessages = TestHelper.GetAllRenderedMessages(() =>
            {
                try
                {
                    testAction.Invoke();
                }
                catch
                {
                    // ignored
                }
            }).ToArray();

            // Then
            Assert.That(renderedMessages[1].Contains(convertStructureError), Is.True, $"Not found error message: {convertStructureError}\n Log messages: {renderedMessages[1]}");
            Assert.That(renderedMessages[2].Contains(structuresFileError), Is.True, $"Not found error message: {structuresFileError}\n Log messages: {renderedMessages[2]}");
            Assert.That(renderedMessages[3].Contains(waterFlowFmFileImporterError), Is.True, $"Not found error message: {waterFlowFmFileImporterError}\n Log messages: {renderedMessages[3]}");
            Assert.That(renderedMessages[3].Contains(structureFactoryException), Is.True, $"Not found error message: {structureFactoryException}\n Log messages: {renderedMessages[3]}");
        }


        [Test]
        [Category(TestCategory.Integration)]
        [Category("Quarantine")]
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
        [Category("Quarantine")]
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
