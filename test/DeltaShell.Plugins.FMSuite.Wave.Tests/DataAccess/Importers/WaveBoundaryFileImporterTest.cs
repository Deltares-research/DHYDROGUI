using System;
using System.Collections.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Importers
{
    [TestFixture]
    public class WaveBoundaryFileImporterTest
    {
        private WaveBoundaryFileImporter importer;

        [Test]
        public void NamePropertyTest()
        {
            var expected = "Wave Boundary Conditions (*.bcw)";
            importer = new WaveBoundaryFileImporter();
            Assert.AreEqual(expected, importer.Name);
        }

        [Test]
        public void SupportedItemTypes_ReturnsEmptyCollection()
        {
            // Setup
            importer = new WaveBoundaryFileImporter();

            // Call | Assert
            Assert.That(importer.SupportedItemTypes, Is.Empty);
        }

        [Test]
        public void CanImportOnPropertyTest()
        {
            importer = new WaveBoundaryFileImporter();
            Assert.IsTrue(importer.CanImportOn(new object()));
            Assert.IsFalse(importer.CanImportOnRootLevel);
        }

        [Test]
        public void FileFilterTest()
        {
            var expected = "Wave Boundary Condition Files (*.bcw;*.sp2)|*.bcw;*.sp2";
            importer = new WaveBoundaryFileImporter();
            Assert.AreEqual(expected, importer.FileFilter);
        }

        [Test]
        public void OpenViewAfterImport()
        {
            importer = new WaveBoundaryFileImporter();
            Assert.IsTrue(importer.OpenViewAfterImport);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItem_ThrowsNotSupportedException()
        {
            // Setup
            importer = new WaveBoundaryFileImporter();

            // Call
            void Call() => importer.ImportItem("path", new List<object>());

            // Assert
            Assert.That(Call, Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void TargetDataDirectory()
        {
            var targetDataDirectory = "dir";
            importer = new WaveBoundaryFileImporter {TargetDataDirectory = targetDataDirectory};
            Assert.AreEqual(targetDataDirectory, importer.TargetDataDirectory);
        }

        [Test]
        public void ShouldCancelTest()
        {
            importer = new WaveBoundaryFileImporter {ShouldCancel = true};
            Assert.AreEqual(true, importer.ShouldCancel);
            importer.ShouldCancel = false;
            Assert.AreEqual(false, importer.ShouldCancel);
        }

        [Test]
        public void ProgressChangedTest()
        {
            importer = new WaveBoundaryFileImporter();
            var succes = false;
            importer.ProgressChanged = (name, current, total) => { succes = true; };
            importer.ProgressChanged("Importing boundary file...", 1, 2);
            Assert.IsTrue(succes);
        }
    }
}