using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlModelImporterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var importer = new RealTimeControlModelImporter();

            // Assert
            Assert.That(importer, Is.InstanceOf<ModelFileImporterBase>());
            Assert.That(importer, Is.InstanceOf<IDimrModelFileImporter>());
            Assert.That(importer.Name, Is.EqualTo("RTC-Tools xml files"));
            Assert.That(importer.Category, Is.EqualTo("Xml files"));
            Assert.That(importer.Description, Is.Empty);
            Assert.That(importer.Image, Is.Not.Null);

            Assert.That(importer.SupportedItemTypes, Is.Empty);
            Assert.That(importer.CanImportOnRootLevel, Is.False);
            Assert.That(importer.FileFilter, Is.EqualTo("xml files|*.xml"));
            Assert.That(importer.TargetDataDirectory, Is.Null);
            Assert.That(importer.ShouldCancel, Is.False);
            Assert.That(importer.ProgressChanged, Is.Null);
            Assert.That(importer.OpenViewAfterImport, Is.False);

            Assert.That(importer.MasterFileExtension, Is.EqualTo("json"));
        }

        [Test]
        public void CanImportOn_Always_ReturnsFalse()
        {
            // Setup
            var importer = new RealTimeControlModelImporter();

            // Call
            bool canImportOnResult = importer.CanImportOn(null);

            // Assert
            Assert.That(canImportOnResult, Is.False);
        }

        [Test]
        public void ImportItem_WithTargetObject_ThrowsArgumentException()
        {
            // Setup
            var importer = new RealTimeControlModelImporter();

            // Call
            TestDelegate call = () => importer.ImportItem("path", new object());

            // Assert
            Assert.That(call, Throws.ArgumentException
                                    .With.Message.EqualTo("Null is expected, because target argument is unused."));
        }

        [Test]
        public void GivenAnInvalidRtcDirectoryPath_WhenReading_ThenNoExceptionIsThrownAndNullIsReturned()
        {
            // Given
            string directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
            Assert.That(!Directory.Exists(directoryPath),
                        $"Directory path '{directoryPath}' was expected to exist.");

            object modelObject = null;

            var importer = new RealTimeControlModelImporter();

            // When
            TestDelegate call = () => modelObject = importer.ImportItem(directoryPath);

            // Then
            Assert.That(call, Throws.Nothing);
            Assert.Null(modelObject, "After importing from invalid directory, rtcModel object was expected to be NULL.");
        }
    }
}