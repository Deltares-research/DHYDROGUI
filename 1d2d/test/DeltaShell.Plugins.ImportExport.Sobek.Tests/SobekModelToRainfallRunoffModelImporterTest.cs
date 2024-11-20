using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekModelToRainfallRunoffModelImporterTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            var importer = new SobekModelToRainfallRunoffModelImporter();

            Assert.That(importer.Name, Is.EqualTo("Sobek 2 RR Model (into RR model)"));
            Assert.That(importer.DisplayName, Is.EqualTo("Sobek 2 RR importer for RR"));
            Assert.That(importer.Category, Is.EqualTo(ProductCategories.OneDTwoDModelImportCategory));
            Assert.That(((IPartialSobekImporter) importer).Category, Is.EqualTo(SobekImporterCategories.RainfallRunoff));
            Assert.That(importer.Description, Is.EqualTo("Sobek 2 RR importer for RR"));
            Assert.That(importer.SupportedItemTypes, Is.EqualTo(new[] { typeof(RainfallRunoffModel) }));
            Assert.That(importer.FileFilter, Is.EqualTo("RR Sobek_3b.fnm file model import|Sobek_3b.fnm"));
            Assert.That(importer.OpenViewAfterImport, Is.True);
            Assert.That(importer.CanImportOnRootLevel, Is.True);
            Assert.That(importer.CanImportOn(null), Is.True);
        }

        [Test]
        public void TargetItem_NotSet_ReturnsNewRainfallRunoffModel()
        {
            var importer = new SobekModelToRainfallRunoffModelImporter();
            object result = importer.TargetObject;

            Assert.That(result, Is.InstanceOf<RainfallRunoffModel>());
        }

        [Test]
        public void TargetItem_ResetToNull_RefreshesRainfallRunoffModel()
        {
            var importer = new SobekModelToRainfallRunoffModelImporter();
            object firstModel = importer.TargetObject;
            importer.TargetObject = null;
            object secondModel = importer.TargetObject;

            Assert.That(firstModel, Is.Not.SameAs(secondModel));
        }

        [Test]
        public void TargetItem_ReturnsSameRainfallRunoffModel()
        {
            var importer = new SobekModelToRainfallRunoffModelImporter();
            object firstModel = importer.TargetObject;
            object secondModel = importer.TargetObject;

            Assert.That(firstModel, Is.SameAs(secondModel));
        }
    }
}