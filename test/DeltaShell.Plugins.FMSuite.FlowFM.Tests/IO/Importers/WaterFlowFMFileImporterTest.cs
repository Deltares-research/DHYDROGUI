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
        public void GivenGeneralStructureMduFile_WhenImportItemFails_ThenExpectedErrorReturned()
        {
            // Given
            const string relativeFilePath = @"c071_generalstructure_door_closing_at_sill\dflowfm\t2.mdu";
            string testFilePath = TestHelper.GetTestFilePath(relativeFilePath);
            Assert.That(File.Exists(testFilePath));
            string base_error = "The method or operation is not implemented.";
            string first_error = $"Failed to convert .ini structure definition 'Maeslantkering' to actual structure: {base_error}";
            string last_error = $"An error occurred while trying to import a Flow Flexible Mesh Model from {testFilePath}. Cause: {first_error}";
            string expectedErrorMessage = $"{last_error}";

            // When
            var importer = new WaterFlowFMFileImporter(() => null);
            TestDelegate testAction = () => importer.ImportItem(testFilePath);

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
            Assert.That(testAction, Throws.Exception.With.Message.EqualTo(expectedErrorMessage));
            Assert.That(renderedMessages.Contains(base_error), Is.False);
            Assert.That(renderedMessages.Contains(first_error), Is.False);
            Assert.That(renderedMessages.Contains(last_error), Is.True);

        }
    }
}