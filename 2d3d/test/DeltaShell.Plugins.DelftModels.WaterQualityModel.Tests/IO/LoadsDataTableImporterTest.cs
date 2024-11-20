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
            Assert.IsNull(importer.FilePath);
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

            var targetObject = new DataTableManager {Name = "UnsupportedName"};

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

            var targetObject = new DataTableManager {Name = "Loads Data"};

            // Call
            bool result = importer.CanImportOn(targetObject);

            // Assert
            Assert.That(result, Is.True);
        }
    }
}