using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class WaterFlowFMFileImporterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var importer = new WaterFlowFMFileImporter(null);

            // Assert
            Assert.That(importer, Is.InstanceOf<ModelFileImporterBase>());
            Assert.That(importer, Is.InstanceOf<IDimrModelFileImporter>());
            Assert.That(importer.Name, Is.EqualTo("Flow Flexible Mesh Model"));
            Assert.That(importer.Category, Is.EqualTo("D-Flow FM 2D/3D"));
            Assert.That(importer.Description, Is.Empty);
            Assert.That(importer.Image, Is.Not.Null);

            CollectionAssert.AreEqual(new[] {typeof(IHydroModel)}, importer.SupportedItemTypes);
            Assert.That(importer.CanImportOnRootLevel, Is.True);
            Assert.That(importer.FileFilter, Is.EqualTo("Flexible Mesh Model Definition|*.mdu"));
            Assert.That(importer.TargetDataDirectory, Is.Null);
            Assert.That(importer.ShouldCancel, Is.False);
            Assert.That(importer.ProgressChanged, Is.Null);
            Assert.That(importer.OpenViewAfterImport, Is.True);

            Assert.That(importer.MasterFileExtension, Is.EqualTo("mdu"));
        }

        [Test]
        public void CanImportOn_TargetObjectWaterFlowFMModel_ReturnsTrue()
        {
            // Setup
            var importer = new WaterFlowFMFileImporter(null);
            using (var target = new WaterFlowFMModel())
            {
                // Call
                bool result = importer.CanImportOn(target);

                // Assert
                Assert.That(result, Is.True);
            }
        }

        [Test]
        public void CanImportOn_TargetObjectICompositeActivity_ReturnsTrue()
        {
            // Setup
            var target = Substitute.For<ICompositeActivity>();
            var importer = new WaterFlowFMFileImporter(null);

            // Call
            bool result = importer.CanImportOn(target);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CanImportOn_TargetObjectNull_ReturnsFalse()
        {
            // Setup
            var importer = new WaterFlowFMFileImporter(null);

            // Call
            bool result = importer.CanImportOn(null);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CanImportOn_TargetObjectUnsupportedType_ReturnsFalse()
        {
            // Setup
            var target = new object();
            var importer = new WaterFlowFMFileImporter(null);

            // Call
            bool result = importer.CanImportOn(target);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}