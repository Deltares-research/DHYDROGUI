using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        public void GivenARealTimeControlModelImporter_WhenCanImportOnIsCalled_ThenExpectedIsReturned()
        {
            var canImportOn = importer.CanImportOn(new Object());
            Assert.AreEqual(false, canImportOn);
        }

        [Test]
        public void GivenAnInvalidRtcDirectoryPath_WhenReading_ThenNoExceptionIsThrownAndObjectIsReturned()
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "Invalid"));
            Assert.That(!Directory.Exists(directoryPath));

            Assert.DoesNotThrow(() =>
            {
                // When
                var modelObject = importer.ImportItem(directoryPath);
                Assert.Null(modelObject);
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidRtcDirectoryPath_WhenReading_ThenNoExceptionIsThrownAndObjectIsReturned()
        {
            // Given
            var directoryPath = TestHelper.GetTestFilePath(Path.Combine("ImportExport", "SimpleModel"));
            Assert.That(Directory.Exists(directoryPath));

            Assert.DoesNotThrow(() =>
            {
                // When
                var modelObject = importer.ImportItem(directoryPath);
                Assert.NotNull(modelObject);

                var rtcModel = modelObject as RealTimeControlModel;
                Assert.NotNull(rtcModel);
            });
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenNameIsCalled_ThenExpectedIsReturned()
        {
            var name = importer.Name;
            Assert.AreEqual("RTC-Tools xml files", name);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenCategoryIsCalled_ThenExpectedIsReturned()
        {
            var category = importer.Category;
            Assert.AreEqual("Xml files", category);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenImageIsCalled_ThenABitmapImageShouldBeReturned()
        {
            var image = importer.Image;
            Assert.AreEqual(typeof(Bitmap), image.GetType());
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenSupportedItemTypesIsCalled_ThenExpectedIsReturned()
        {
            var supportedItemTypes = importer.SupportedItemTypes;
            var expectedSupportedItemTypes = new List<Type> { typeof(HydroModel.HydroModel) };
            Assert.AreEqual(expectedSupportedItemTypes, supportedItemTypes);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenCanImportOnRootLevelIsCalled_ThenExpectedIsReturned()
        {
            var canImportOnRootLevel = importer.CanImportOnRootLevel;
            Assert.AreEqual(false, canImportOnRootLevel);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenFileFilterIsCalled_ThenExpectedIsReturned()
        {
            var fileFilter = importer.FileFilter;
            Assert.AreEqual("xml files|*.xml", fileFilter);

        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenTargetDataDirectoryIsSetAndCalled_ThenExpectedIsReturned()
        {
            var expectedTargetDataDirectory = "some_directory";
            importer.TargetDataDirectory = expectedTargetDataDirectory;
            Assert.AreEqual(expectedTargetDataDirectory, importer.TargetDataDirectory);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenARealTimeControlModelImporter_WhenShouldCancelIsSetAndCalled_ThenExpectedIsReturned(bool expectedShouldCancel)
        {
            importer.ShouldCancel = expectedShouldCancel;
            Assert.AreEqual(expectedShouldCancel, importer.ShouldCancel);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenProgressChangedIsSetAndCalled_ThenExpectedIsReturned()
        {
            ImportProgressChangedDelegate expectedImportProgressChangedDelegate = (cst, cs, ts) => { };
            importer.ProgressChanged = expectedImportProgressChangedDelegate;
            Assert.AreEqual(expectedImportProgressChangedDelegate, importer.ProgressChanged);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenOpenViewAfterImportIsCalled_ThenExpectedIsReturned()
        {
            var openViewAfterImport = importer.OpenViewAfterImport;
            Assert.AreEqual(false, openViewAfterImport);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenMasterFileExtensionIsCalled_ThenExpectedIsReturned()
        {
            var masterFileExtension = importer.MasterFileExtension;
            Assert.AreEqual("json", masterFileExtension);
        }

        [Test]
        public void GivenARealTimeControlModelImporter_WhenSubFoldersIsCalled_ThenExpectedIsReturned()
        {
            var subFolders = importer.SubFolders;
            var expectedSubFolders = new List<string> { "rtc" };
            Assert.AreEqual(expectedSubFolders, subFolders);
        }

        [TearDown]
        public void TearDown()
        {
            importer = null;
        }
    }
}
