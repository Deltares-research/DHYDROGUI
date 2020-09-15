using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class WaterFlowFMFileImporterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            var importer = new WaterFlowFMFileImporter(() => null);

            Assert.IsTrue(importer is IDimrModelFileImporter, "The IDimrModelFileImporter interface is not implemented by WaterFlowFMFileImporter");
            Assert.AreEqual("mdu", importer.MasterFileExtension, $"Expected mdu for master file extension, but was {importer.MasterFileExtension}");
            Assert.AreEqual("Flow Flexible Mesh Model", importer.Name, $"Expected Flow Flexible Mesh Model for importer name, but was {importer.Name}");
            Assert.AreEqual("D-Flow FM 2D/3D", importer.Category, $"Expected D-Flow FM 2D/3D for importer category, but was {importer.Category}");
            Assert.AreEqual(string.Empty, importer.Description, $"Expected empty string for importer description, but was {importer.Description}");
            Assert.IsTrue(importer.OpenViewAfterImport, "The view should be opened after import");
            Assert.IsTrue(importer.CanImportOnRootLevel, "The importer should be able to import on Root level");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenGeneralStructureMduFileWithUnhandledType_WhenImportItem_ThenNotImplementedExceptionThrown()
        {
            // Given
            const string relativeFilePath = @"c071_generalstructure_door_closing_at_sill\dflowfm\t2.mdu";
            string testFilePath = TestHelper.GetTestFilePath(relativeFilePath);
            Assert.That(File.Exists(testFilePath));
            const string baseError = "Trying to generate Time series for 2D Structure: Maeslantkering, property: GateOpeningWidth mapped as type External which is not yet supported.";

            // When
            var importer = new WaterFlowFMFileImporter(() => null);
            TestDelegate testAction = () => importer.ImportItem(testFilePath);

            // Then
            Assert.That(testAction, Throws.TypeOf<NotImplementedException>().With.Message.EqualTo(baseError));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenGeneralStructureMduFileWithUnhandledType_WhenImportItem_ThenLoggedExpectedMessages()
        {
            // Given
            const string relativeFilePath = @"c071_generalstructure_door_closing_at_sill\dflowfm\";
            string iniFilePath = TestHelper.GetTestFilePath(Path.Combine(relativeFilePath, "t2.mdu"));
            string structureFilePath = TestHelper.GetTestFilePath(Path.Combine(relativeFilePath, "tst-1_structures.ini"));
            Assert.That(File.Exists(iniFilePath));
            Assert.That(File.Exists(structureFilePath));

            const string structureFactoryException = "Trying to generate Time series for 2D Structure: Maeslantkering, property: GateOpeningWidth mapped as type External which is not yet supported.";
            string structuresFileError = $"Error while reading and converting 2D Structures from {structureFilePath}";
            const string convertStructureError = "Failed to convert .ini structure definition 'Maeslantkering' to actual structure.";
            string waterFlowFmFileImporterError = $"Error while importing a Flow Flexible Mesh Model from {iniFilePath}";

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
            string renderMessagesAsString = string.Join("\n", renderedMessages);
            Assert.That(renderedMessages.Contains(structureFactoryException), Is.False);
            Assert.That(renderedMessages.Contains(convertStructureError), Is.True, $"Not found error message: {convertStructureError}\n Log messages: {renderMessagesAsString}");
            Assert.That(renderedMessages.Contains(structuresFileError), Is.True, $"Not found error message: {structuresFileError}\n Log messages: {renderMessagesAsString}");
            Assert.That(renderedMessages.Contains(waterFlowFmFileImporterError), Is.True, $"Not found error message: {waterFlowFmFileImporterError}\n Log messages: {renderMessagesAsString}");
        }

    }
}