using System;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class FMMapFileImporterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Arrange
            var importer = new FMMapFileImporter();

            // Assert
            Assert.That(importer.Name, Is.EqualTo("Flexible Mesh Map File"));
            Assert.That(importer.Category, Is.EqualTo("D-Flow FM 2D/3D"));
            Assert.That(importer.Description, Is.EqualTo(string.Empty));
            Assert.IsNotNull(importer.Image);
            Assert.That(importer.SupportedItemTypes.Single(), Is.EqualTo(typeof(IFMMapFileFunctionStore)));
            Assert.That(importer.OpenViewAfterImport, Is.True);
            Assert.That(importer.CanImportOn(Arg<object>.Is.Anything), Is.True);
            Assert.That(importer.CanImportOnRootLevel, Is.True);
            Assert.That(importer.FileFilter, Is.EqualTo($"FM Map File|*_map.nc"));
        }

        [Test]
        public void ImportItem_WithPath_ReturnsFMMapFileFunctionsStoreWrappedInDataItem()
        {
            // Arrange
            var importer = new FMMapFileImporter();
            var filePath = Guid.NewGuid().ToString();

            // Act
            var returnedDataItem = importer.ImportItem(filePath) as DataItem;

            // Assert
            Assert.IsNotNull(returnedDataItem);

            var wrappedFunctionStore = returnedDataItem.Value as FMMapFileFunctionStore;
            Assert.IsNotNull(wrappedFunctionStore);
            Assert.That(wrappedFunctionStore.Path, Is.EqualTo(filePath));
        }
    }
}