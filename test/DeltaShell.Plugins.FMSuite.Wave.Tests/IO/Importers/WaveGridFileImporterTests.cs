using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Importers
{
    [TestFixture]
    public class WaveGridFileImporterTests
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Spherical()
        {
            var waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_001.grd");
            var waveModel = new WaveModel();
            var tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[] { waveModel });
                waveModel.OuterDomain.Grid.Attributes[CurvilinearGrid.CoordinateSystemKey] = "Test";
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Null);
                waveGridFileImporter.ImportItem(waveGridFileFilePath, waveModel.OuterDomain.Grid);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem.IsGeographic, Is.True);
                Assert.That(waveModel.OuterDomain.Grid.Attributes[CurvilinearGrid.CoordinateSystemKey], Is.EqualTo("Spherical"));
                Assert.That(waveModel.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.CoordinateSystem.IsGeographic, Is.True);
            }
            finally
            {
                FileUtils.DeleteIfExists(tempWorkingDirectory);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Cartesian()
        {
            var waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\smallbend.grd");
            var waveModel = new WaveModel();
            var tempWorkingDirectory = FileUtils.CreateTempDirectory();
            try
            {
                waveModel.MdwFile.MdwFilePath = Path.Combine(tempWorkingDirectory, Path.GetRandomFileName());
                waveModel.OuterDomain.Grid.Attributes[CurvilinearGrid.CoordinateSystemKey] = "Test";
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[] { waveModel });
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Null);
                waveGridFileImporter.ImportItem(waveGridFileFilePath, waveModel.OuterDomain.Grid);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Null);
                Assert.That(waveModel.OuterDomain.Grid.Attributes[CurvilinearGrid.CoordinateSystemKey], Is.EqualTo("Cartesian"));
                Assert.That(waveModel.CoordinateSystem, Is.Null);
            }
            finally
            {
                FileUtils.DeleteIfExists(tempWorkingDirectory);
            }
        }
    }
}
