using System.Linq;
using DeltaShell.Dimr;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class WaterFlowFMFileImporterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            var importer = new WaterFlowFMFileImporter(() => null);

            Assert.IsTrue(importer is IDimrModelFileImporter, "The IDimrModelFileImporter interface is not implemented by WaterFlowFMFileImporter");
            Assert.AreEqual("mdu", importer.MasterFileExtension, $"Expected mdu for master file extension, but was {importer.MasterFileExtension}");
            Assert.AreEqual("Flow Flexible Mesh Model", importer.Name, $"Expected Flow Flexible Mesh Model for importer name, but was {importer.Name}");
            Assert.AreEqual("D-Flow FM 2D/3D", importer.Category, $"Expected D-Flow FM 2D/3D for importer category, but was {importer.Category}");
            Assert.AreEqual(string.Empty, importer.Description, $"Expected empty string for importer description, but was {importer.Description}");
            Assert.IsTrue(importer.OpenViewAfterImport, "The view should be opened after import");
            Assert.IsTrue(importer.CanImportOnRootLevel, "The importer should be able to import on Root level");
        }
    }
}