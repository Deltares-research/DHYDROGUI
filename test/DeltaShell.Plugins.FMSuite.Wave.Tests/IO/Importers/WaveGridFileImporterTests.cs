using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Extensions.CoordinateSystems;

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
        public void SphericalToCartesian()
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
        [Test]
        public void
            GivenWaveModelWithOuterDomainWithDifferentCartesianCoordinateSystemSetWhenImportingCartesianShouldImportedGridGetSameCoordinateSystemAsModelCoordinateSystem()
        {
            var waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_002.grd");
            var waveModel = new WaveModel();
            var tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[] { waveModel });
                waveModel.OuterDomain.Grid.Attributes[CurvilinearGrid.CoordinateSystemKey] = "Test";
                waveModel.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3857);
                TestHelper.AssertLogMessageIsGenerated(
                    () => waveGridFileImporter.ImportItem(waveGridFileFilePath, waveModel.OuterDomain.Grid),
                    string.Format(Resources.WaveModel_OnOuterDomainPropertyChanged_Grid_is_set_in_project_but_doesn_t_contain_a_coordinate_system__The_model_has_co_ordinate_system__0___setting_grid_to_this_co_oordinate_system_type_,
                        waveModel.CoordinateSystem));
                Assert.That(waveModel.CoordinateSystem, Is.Null);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Null);
            }
            finally
            {
                FileUtils.DeleteIfExists(tempWorkingDirectory);
            }
        }
        [Test]
        public void
            GivenWaveModelWithOuterDomainWithDifferentCoordinateSystemSetWhenAddingSphericalDomainShouldModelShouldTransFormAndGetSameCoordinateSystem()
        {
            var waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_001.grd");
            var waveModel = new WaveModel();
            var tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[] { waveModel });
                waveModel.OuterDomain.Grid.Attributes[CurvilinearGrid.CoordinateSystemKey] = "Test";
                waveModel.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3857);
                waveGridFileImporter.ImportItem(waveGridFileFilePath, waveModel.OuterDomain.Grid);
                Assert.That(waveModel.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.CoordinateSystem.AuthorityCode, Is.EqualTo(4326));
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem.AuthorityCode, Is.EqualTo(4326));
            }
            finally
            {
                FileUtils.DeleteIfExists(tempWorkingDirectory);
            }
        }
       
        [Test]
        public void
            GivenWaveModelWithOuterDomainWithSameCoordinateSystemSetWhenAddingSphericalDomainShouldModelShouldNotChangeCoordinateSystem()
        {
            var waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_001.grd");
            var waveModel = new WaveModel();
            var tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[] { waveModel });
                waveModel.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326);
                TestHelper.AssertLogMessageIsNotGenerated(
                    () => waveGridFileImporter.ImportItem(waveGridFileFilePath, waveModel.OuterDomain.Grid),
                    string.Format(Resources.WaveModel_OnOuterDomainPropertyChanged_Grid_is_set_in_project_but_doesn_t_contain_a_coordinate_system__The_model_has_co_ordinate_system__0___setting_grid_to_this_co_oordinate_system_type_,
                        waveModel.CoordinateSystem));
                Assert.That(waveModel.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.CoordinateSystem.AuthorityCode, Is.EqualTo(4326));
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem.AuthorityCode, Is.EqualTo(4326));
            }
            finally
            {
                FileUtils.DeleteIfExists(tempWorkingDirectory);
            }
        }
    }
}
