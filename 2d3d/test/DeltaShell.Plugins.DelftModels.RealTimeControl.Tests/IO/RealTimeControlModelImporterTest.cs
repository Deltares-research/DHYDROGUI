using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlModelImporterTest
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void ImportItem_PathIsNullOrEmpty_ThrowsArgumentException(string path)
        {
            // Given
            RealTimeControlModelImporter importer = CreateImporter();

            // When
            // Then
            Assert.That(() => importer.ImportItem(path), Throws.ArgumentException);
        }

        [Test]
        public void ImportItem_DirectoryDoesNotExist_ThrowsArgumentException()
        {
            // Given
            RealTimeControlModelImporter importer = CreateImporter();

            // When
            // Then
            Assert.That(() => importer.ImportItem("dir"), Throws.ArgumentException);
        }

        [Test]
        public void ImportItem_XmlReadersIsEmpty_ThrowsInvalidOperationException()
        {
            // Given
            string directoryPath = GetTestDataDirectory();

            RealTimeControlModelImporter importer = CreateImporter();

            // When
            importer.XmlReaders.Clear();

            // Then
            Assert.That(() => importer.ImportItem(directoryPath), Throws.InvalidOperationException);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_SettingsJsonFileMissing_LogsWarningMessage()
        {
            // Given
            string directoryPath = GetTestDataDirectory();

            RealTimeControlModelImporter importer = CreateImporter();

            // When
            // Then
            string expectedMessage = string.Format(Resources.RealTimeControlModelImporter_GetXmlDirectory_Could_not_find_settings_json__importing_from_RTC_model_from__0__, directoryPath);
            TestHelper.AssertLogMessageIsGenerated(() => importer.ImportItem(directoryPath), expectedMessage, Level.Warn);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_SettingsJsonFileMissing_ReturnsRtcModel()
        {
            // Given
            string directoryPath = GetTestDataDirectory();

            RealTimeControlModelImporter importer = CreateImporter();

            // When
            object modelObject = importer.ImportItem(directoryPath);

            // Then
            Assert.That(modelObject, Is.Not.Null);
            Assert.That(modelObject, Is.InstanceOf<RealTimeControlModel>());
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_InvalidSettingsJsonFile_LogsErrorMessage()
        {
            // Given
            string directoryPath = GetTestDataDirectory();
            string invalidSettingsJsonFile = Path.Combine(directoryPath, "invalidJson");

            RealTimeControlModelImporter importer = CreateImporter();

            // When
            // Then
            string expectedMessage = string.Format(Resources.RealTimeControlModelImporter_GetXmlDirectory_Could_not_import_RTC_model_the_settings_json_file_should_contain_an_xml_directory, directoryPath);
            TestHelper.AssertLogMessageIsGenerated(() => importer.ImportItem(invalidSettingsJsonFile), expectedMessage, Level.Error);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_InvalidSettingsJsonFile_ReturnsNull()
        {
            // Given
            string directoryPath = GetTestDataDirectory();
            string invalidSettingsJsonFile = Path.Combine(directoryPath, "invalidJson");

            RealTimeControlModelImporter importer = CreateImporter();

            object modelObject = null;

            // When
            Assert.DoesNotThrow(() => modelObject = importer.ImportItem(invalidSettingsJsonFile));

            // Then
            Assert.Null(modelObject, "After importing from invalid settings json directory, rtcModel object was expected to be NULL.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_ValidSettingsJsonFile_ReturnsRtcModel()
        {
            // Given
            string directoryPath = GetTestDataDirectory();
            string validSettingsJsonFile = Path.Combine(directoryPath, "validJson");

            RealTimeControlModelImporter importer = CreateImporter();

            // when
            object modelObject = importer.ImportItem(validSettingsJsonFile);

            // Then
            Assert.That(modelObject, Is.Not.Null);
            Assert.That(modelObject, Is.InstanceOf<RealTimeControlModel>());
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenCanImportOnRootLevelIsCalled_ThenExpectedIsReturned()
        {
            RealTimeControlModelImporter importer = CreateImporter();

            Assert.AreEqual(false, importer.CanImportOnRootLevel);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenCategoryIsCalled_ThenExpectedIsReturned()
        {
            RealTimeControlModelImporter importer = CreateImporter();

            Assert.AreEqual("Xml files", importer.Category);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenFileFilterIsCalled_ThenExpectedIsReturned()
        {
            RealTimeControlModelImporter importer = CreateImporter();

            Assert.AreEqual("xml files|*.xml", importer.FileFilter);
        }
        
        [Test]
        [TestCase(null, ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(".", ExpectedResult = true)]
        [TestCase("settings.json", ExpectedResult = true)]
        [TestCase("SETTINGS.JSON", ExpectedResult = true)]
        [TestCase("settings.xml", ExpectedResult = false)]
        [TestCase("flowfm.mdu", ExpectedResult = false)]
        [TestCase("waves.mdw", ExpectedResult = false)]
        public bool GivenARealTimeControlModelImporter_WithInputFile_ThenExpectedIsReturned(string path)
        {
            RealTimeControlModelImporter importer = CreateImporter();

            return importer.CanImportDimrFile(path);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenNameIsCalled_ThenExpectedIsReturned()
        {
            RealTimeControlModelImporter importer = CreateImporter();

            Assert.AreEqual("RTC-Tools xml files", importer.Name);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenOpenViewAfterImportIsCalled_ThenExpectedIsReturned()
        {
            RealTimeControlModelImporter importer = CreateImporter();

            Assert.AreEqual(false, importer.OpenViewAfterImport);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenSupportedItemTypesIsCalled_ThenExpectedIsReturned()
        {
            RealTimeControlModelImporter importer = CreateImporter();

            Assert.AreEqual(new List<Type>(), importer.SupportedItemTypes);
        }

        private static RealTimeControlModelImporter CreateImporter()
        {
            return new RealTimeControlModelImporter { XmlReaders = { new RealTimeControlModelXmlReader() } };
        }

        private static string GetTestDataDirectory()
        {
            return TestHelper.GetTestFilePath(Path.Combine("ImportExport", "SimpleModel"));
        }
    }
}