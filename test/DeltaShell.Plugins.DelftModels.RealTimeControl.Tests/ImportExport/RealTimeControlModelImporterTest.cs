using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlModelImporterTest
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            importer = new RealTimeControlModelImporter();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            importer = null;
        }

        private RealTimeControlModelImporter importer;

        [Test]
        public void GivenAnInvalidRtcDirectoryPath_WhenReading_ThenNoExceptionIsThrownAndNullIsReturned()
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
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
        public void GivenARealTimeControlModelImporter_WhenMasterFileExtensionIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual("json", importer.MasterFileExtension);
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
    }
}