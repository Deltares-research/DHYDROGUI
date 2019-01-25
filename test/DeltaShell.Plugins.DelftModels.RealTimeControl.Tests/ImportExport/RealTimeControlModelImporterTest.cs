using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.ImportExport
{
    [TestFixture]
    public class RealTimeControlModelImporterTest
    {
        private RealTimeControlModelImporter importer;

        [SetUp]
        public void SetUp()
        {
            importer = new RealTimeControlModelImporter();
        }

        [Test]
        public void GivenAnInvalidRtcDirectoryPath_WhenReading_ThenNoExceptionIsThrownAndNullIsReturned()
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
            Assert.That(!Directory.Exists(directoryPath));

            Assert.DoesNotThrow(() =>
            {
                // When
                var modelObject = importer.ImportItem(directoryPath);

                // Then
                Assert.Null(modelObject);
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidRtcDirectoryPath_WhenReading_ThenNoExceptionIsThrownAndRtcModelIsReturned()
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "SimpleModel"));
            Assert.That(Directory.Exists(directoryPath));

            Assert.DoesNotThrow(() =>
            {
                // When
                var rtcModel = importer.ImportItem(directoryPath) as RealTimeControlModel;

                // Then
                Assert.NotNull(rtcModel);
            });
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenNameIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual("RTC-Tools xml files", importer.Name);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenCategoryIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual("Xml files", importer.Category);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenSupportedItemTypesIsCalled_ThenExpectedIsReturned()
        {
            var expectedSupportedItemTypes = new List<Type> { typeof(HydroModel.HydroModel) };
            Assert.AreEqual(expectedSupportedItemTypes, importer.SupportedItemTypes);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenCanImportOnRootLevelIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual(false, importer.CanImportOnRootLevel);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenFileFilterIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual("xml files|*.xml", importer.FileFilter);

        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenOpenViewAfterImportIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual(false, importer.OpenViewAfterImport);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenMasterFileExtensionIsCalled_ThenExpectedIsReturned()
        {
            Assert.AreEqual("json", importer.MasterFileExtension);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenSubFoldersIsCalled_ThenExpectedIsReturned()
        {
            var expectedSubFolders = new List<string> { "rtc" };
            Assert.AreEqual(expectedSubFolders, importer.SubFolders);
        }

        [TearDown]
        public void TearDown()
        {
            importer = null;
        }
    }
}
