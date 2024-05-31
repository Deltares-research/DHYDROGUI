using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO
{
    [TestFixture]
    public class RealTimeControlModelImporterTest
    {
        private RealTimeControlModelImporter importer;

        [Test]
        public void GivenAnInvalidRtcDirectoryPath_WhenReading_ThenNoExceptionIsThrownAndNullIsReturned()
        {
            // Given
            string directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
            Assert.That(!Directory.Exists(directoryPath),
                        $"Directory path '{directoryPath}' was expected to exist.");

            object modelObject = null;

            // When
            Assert.DoesNotThrow(
                () => modelObject = importer.ImportItem(directoryPath));

            // Then
            Assert.Null(modelObject, "After importing from invalid directory, rtcModel object was expected to be NULL.");
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenCanImportOnRootLevelIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual(false, importer.CanImportOnRootLevel);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenCategoryIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual("Xml files", importer.Category);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenFileFilterIsCalled_ThenExpectedIsReturned()
        {
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
            return importer.CanImportDimrFile(path);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenNameIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual("RTC-Tools xml files", importer.Name);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenOpenViewAfterImportIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual(false, importer.OpenViewAfterImport);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenSupportedItemTypesIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual(new List<Type>(), importer.SupportedItemTypes);
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            importer = new RealTimeControlModelImporter();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            importer = null;
        }
    }
}