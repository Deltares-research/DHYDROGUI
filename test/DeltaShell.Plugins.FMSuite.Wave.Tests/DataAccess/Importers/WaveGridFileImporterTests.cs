using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.FMSuite.Common.IO.Writers;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Importers
{
    [TestFixture]
    public class WaveGridFileImporterTests
    {
        private WaveGridFileImporter importer;

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAGridFileWithASphericalCoordinateSystemWhenImportingThenCoordinateSystemOnTheModelIsSpherical()
        {
            string waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_001.grd");
            var waveModel = new WaveModel();
            string tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[]
                {
                    waveModel
                });
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
        public void GivenAGridFileWithACartesianCoordinateSystemWhenImportingThenCoordinateSystemOnTheModelIsCartesian()
        {
            string waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_002.grd");
            var waveModel = new WaveModel();
            string tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[]
                {
                    waveModel
                });
                waveModel.OuterDomain.Grid.Attributes[CurvilinearGrid.CoordinateSystemKey] = "Test";
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
            string waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_002.grd");
            var waveModel = new WaveModel();
            string tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[]
                {
                    waveModel
                });
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
            string waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_001.grd");
            var waveModel = new WaveModel();
            string tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[]
                {
                    waveModel
                });
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
            string waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_001.grd");
            var waveModel = new WaveModel();
            string tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[]
                {
                    waveModel
                });
                
                waveModel.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326);
                
                string message = string.Format(Resources.WaveModel_OnOuterDomainPropertyChanged_Grid_is_set_in_project_but_doesn_t_contain_a_coordinate_system__The_model_has_co_ordinate_system__0___setting_grid_to_this_co_oordinate_system_type_,
                                              waveModel.CoordinateSystem);
                
                IEnumerable<string> logMessages = TestHelper.GetAllRenderedMessages(() => waveGridFileImporter.ImportItem(waveGridFileFilePath, waveModel.OuterDomain.Grid));

                Assert.That(logMessages, Does.Not.Contain(message).IgnoreCase);
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
        public void NamePropertyTest()
        {
            var expected = "Delft3D Grid";
            importer = new WaveGridFileImporter("Waves Model", null);
            Assert.AreEqual(expected, importer.Name);
        }

        [Test]
        public void CategoryPropertyTest()
        {
            var expected = "Waves Model";
            importer = new WaveGridFileImporter("Waves Model", null);
            Assert.AreEqual(expected, importer.Category);
        }

        [Test]
        public void SupportedItemTypesTypesPropertyTest()
        {
            var expected = new List<Type> {typeof(CurvilinearGrid)};
            importer = new WaveGridFileImporter("Waves Model", null);
            Assert.AreEqual(expected, importer.SupportedItemTypes);
        }

        [Test]
        public void CanImportOnPropertyTest()
        {
            importer = new WaveGridFileImporter("Waves Model", null);
            Assert.IsTrue(importer.CanImportOn(new object()));
            Assert.IsFalse(importer.CanImportOnRootLevel);
        }

        [Test]
        public void FileFilterTest()
        {
            var expected = "Delft3D Grid (*.grd)|*.grd|All Files (*.*)|*.*";
            importer = new WaveGridFileImporter("Waves Model", null);
            Assert.AreEqual(expected, importer.FileFilter);
        }

        [Test]
        public void ImportItemTest_WhenTargetIsNotCurvilinearGrid_ThenExceptionIsThrown()
        {
            importer = new WaveGridFileImporter("Waves Model", null);
            var target = new List<string>();
            try
            {
                importer.ImportItem(string.Empty, target);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(NotSupportedException));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItemTest_GridIsCorrectlyImported()
        {
            string saveDirPath = FileUtils.CreateTempDirectory();
            var projectName = "MyProject";
            string savePath = Path.Combine(saveDirPath, projectName + ".dsproj");
            var size = 3;
            CurvilinearGrid oldGrid = CreateCurvilinearGrid(size, size);
            string grdFilePath = Path.Combine(saveDirPath, projectName + ".dsproj_data", "Outer.grd");

            try
            {
                using (IApplication app = CreateRunningApplication())
                {
                    IProjectService projectService = app.ProjectService;
                    importer = new WaveGridFileImporter("Waves Model", () => projectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaveModel>());

                    using (var model = new WaveModel())
                    {
                        Project project = projectService.CreateProject();
                        projectService.SaveProjectAs(savePath);
                        project.RootFolder.Add(model);

                        model.OuterDomain.Grid = oldGrid;
                        Assert.AreEqual(size, model.OuterDomain.Grid.Size1);

                        Delft3DGridFileWriter.Write(model.OuterDomain.Grid, grdFilePath);
                        Assert.That(grdFilePath, Is.Not.Null.And.Not.Empty, "There was no .grd file created.");

                        model.OuterDomain.Grid = new CurvilinearGrid(0, 0, null, null, string.Empty);
                        Assert.AreEqual(0, model.OuterDomain.Grid.Size1);

                        importer.ImportItem(grdFilePath, model.OuterDomain.Grid);

                        CurvilinearGrid importedGrid = model.OuterDomain.Grid;

                        Assert.AreEqual(oldGrid.Size1, importedGrid.Size1);
                        Assert.AreEqual(oldGrid.GetValues().Count, importedGrid.GetValues().Count);
                        Assert.AreEqual(oldGrid.GetValues(), importedGrid.GetValues());
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(saveDirPath);
            }
        }

        [Test]
        public void TargetDataDirectory()
        {
            var targetDataDirectory = "dir";
            importer = new WaveGridFileImporter("Waves Model", null) {TargetDataDirectory = targetDataDirectory};
            Assert.AreEqual(targetDataDirectory, importer.TargetDataDirectory);
        }

        [Test]
        public void ShouldCancelTest()
        {
            importer = new WaveGridFileImporter("Waves Model", null) {ShouldCancel = true};
            Assert.AreEqual(true, importer.ShouldCancel);
            importer.ShouldCancel = false;
            Assert.AreEqual(false, importer.ShouldCancel);
        }

        [Test]
        public void ProgressChangedTest()
        {
            importer = new WaveGridFileImporter("Waves Model", null);
            var succes = false;
            importer.ProgressChanged = (name, current, total) => { succes = true; };
            importer.ProgressChanged("Importing depth file...", 1, 2);
            Assert.IsTrue(succes);
        }

        private static IApplication CreateRunningApplication()
        {
            var app = CreateApplication();
            app.Run();
            return app;
        }

        private static IApplication CreateApplication()
        {
            return new DHYDROApplicationBuilder().Build();
        }

        private static CurvilinearGrid CreateCurvilinearGrid(int length, int width)
        {
            int size = length * width;

            var x = new double[size];
            var y = new double[size];
            var values = new double[size];
            for (var i = 0; i < size; i++)
            {
                x[i] = i;
                y[i] = i;
                values[i] = i;
            }

            var grid = new CurvilinearGrid(length, width, x, y, WaveModel.CoordinateSystemType.Cartesian);
            grid.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992);
            grid.IsTimeDependent = false;
            grid.Name = "Grid (Outer)";
            return grid;
        }
    }
}