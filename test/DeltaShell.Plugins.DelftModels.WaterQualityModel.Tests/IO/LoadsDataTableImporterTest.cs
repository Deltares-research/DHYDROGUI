using System;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects.BoundaryData;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class LoadsDataTableImporterTest
    {
        [Test]
        public void DefaultConstructor_ExpectedValues()
        {
            var importer = new LoadsDataTableImporter();

            Assert.IsInstanceOf<DataTableImporter>(importer);
            Assert.AreEqual("Data table loads importer", importer.Name);
            Assert.AreEqual("WAQ data tables", importer.Category);
            Assert.IsNull(importer.Image);
            var supportedTypes = importer.SupportedItemTypes;
            Assert.AreEqual(1, supportedTypes.Count());
            CollectionAssert.AreEqual(new[]
            {
                typeof(DataTableManager)
            }, importer.SupportedItemTypes);
            Assert.IsFalse(importer.CanImportOnRootLevel);
            Assert.AreEqual("WAQ data table (*.csv)|*.csv", importer.FileFilter);
            Assert.IsNull(importer.TargetDataDirectory);
            Assert.IsFalse(importer.ShouldCancel);
            Assert.IsNull(importer.ProgressChanged);
            Assert.IsFalse(importer.OpenViewAfterImport);
        }

        [Test]
        public void CanImportOn_UnsupportedTargetObject_ReturnsFalse()
        {
            // Setup
            var importer = new LoadsDataTableImporter();

            var targetObject = new object();

            // Call
            bool result = importer.CanImportOn(targetObject);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CanImportOn_SupportedTargetObjectWithUnsupportedName_ReturnsFalse()
        {
            // Setup
            var importer = new LoadsDataTableImporter();

            var targetObject = new DataTableManager { Name = "UnsupportedName" };

            // Call
            bool result = importer.CanImportOn(targetObject);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CanImportOn_SupportedTargetObjectWithSupportedName_ReturnsTrue()
        {
            // Setup
            var importer = new LoadsDataTableImporter();

            var targetObject = new DataTableManager { Name = "Loads Data" };

            // Call
            bool result = importer.CanImportOn(targetObject);

            // Assert
            Assert.That(result, Is.True);
        }
        
        [Test]
        public void FilePathNullWhenInstantiatingDataTableBoundaryImporter()
        {
            // setup
            var importer = new LoadsDataTableImporter();

            // assert
            Assert.IsNull(importer.FilePath);
        }
    }
}