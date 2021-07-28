using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    public class SobekRRKasInitImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Import_SetsCorrectKasInitDataOnModel()
        {
            using (var temp = new TemporaryDirectory())
            using (var model = new RainfallRunoffModel())
            {
                // Setup
                var importer = new SobekRRKasInitImporter
                {
                    TargetObject = model,
                    PathSobek = Path.Combine(temp.Path, "Sobek_3b.fnm")
                };

                const string fileContent = "some_custom_content";
                temp.CreateFile("KASINIT", fileContent);

                // Call
                importer.Import();

                // Assert
                var document = (TextDocument) model.GetDataItemByTag("GreenhouseStorageFile").Value;
                Assert.That(document.Content, Is.EqualTo(fileContent));
            }
        }
    }
}