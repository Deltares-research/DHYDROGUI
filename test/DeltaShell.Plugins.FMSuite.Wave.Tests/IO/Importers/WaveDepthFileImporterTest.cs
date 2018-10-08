using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using DeltaShell.Plugins.SharpMapGis;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Importers
{
    [TestFixture]
    public class WaveDepthFileImporterTest
    {
        private WaveDepthFileImporter importer;

        [Test]
        public void NamePropertyTest()
        {
            var expected = "Delft3D Depth File";
            importer = new WaveDepthFileImporter("Waves Model", null);
            Assert.AreEqual(expected, importer.Name);
        }

        [Test]
        public void CategoryPropertyTest()
        {
            var expected = "Waves Model";
            importer = new WaveDepthFileImporter("Waves Model", null);
            Assert.AreEqual(expected, importer.Category);
        }

        [Test]
        public void SupportedItemTypesTypesPropertyTest()
        {
            var expected = new List<Type> { typeof(CurvilinearCoverage) };
            importer = new WaveDepthFileImporter("Waves Model", null);
            Assert.AreEqual(expected, importer.SupportedItemTypes);
        }

        [Test]
        public void CanImportOnPropertyTest()
        {
            importer = new WaveDepthFileImporter("Waves Model", null);
            Assert.IsTrue(importer.CanImportOn(new object()));
            Assert.IsFalse(importer.CanImportOnRootLevel);
        }

        [Test]
        public void FileFilterTest()
        {
            var expected = "Delft3D Depth File (*.dep)|*.dep|All Files (*.*)|*.*";
            importer = new WaveDepthFileImporter("Waves Model", null);
            Assert.AreEqual(expected, importer.FileFilter);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItemTest_WhenTargetIsNotCurvilinearCoverage_ThenExceptionIsThrown()
        {
            importer = new WaveDepthFileImporter("Waves Model", null);
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
        public void ImportItemTest_BathymetryIsCorrectlyImported()
        {
            var saveDirPath = FileUtils.CreateTempDirectory();
            var projectName = "MyProject";
            var savePath = Path.Combine(saveDirPath, projectName + ".dsproj");
            var size = 3;
            var oldBathymetry = CreateCurvilinearCoverageWithValues(size, size);

            try
            {
                using (var app = GetRunningApplication(savePath))
                {
                    importer = new WaveDepthFileImporter("Waves Model", () => app.Project.RootFolder.GetAllItemsRecursive().OfType<WaveModel>());

                    using (var model = new WaveModel())
                    {
                        var project = app.Project;
                        project.RootFolder.Add(model);

                        model.OuterDomain.Grid = CreateCurvilinearGrid(size, size);
                        model.OuterDomain.Bathymetry = oldBathymetry;

                        Assert.AreEqual(size, model.OuterDomain.Bathymetry.Size1);

                        app.SaveProjectAs(savePath);

                        var depFilePath = Directory.GetFiles(Path.Combine(saveDirPath, projectName + ".dsproj_data"),
                            "*.dep",
                            SearchOption.AllDirectories).FirstOrDefault();
                        Assert.IsNotNullOrEmpty(depFilePath, "There was no .dep file created.");

                        model.OuterDomain.Bathymetry = new CurvilinearCoverage();
                        Assert.AreEqual(0, model.OuterDomain.Bathymetry.Size1);

                        importer.ImportItem(depFilePath, model.OuterDomain.Bathymetry);

                        var importedBathymetry = model.OuterDomain.Bathymetry;

                        Assert.AreEqual(oldBathymetry.Size1, importedBathymetry.Size1);
                        Assert.AreEqual(oldBathymetry.GetValues().Count, importedBathymetry.GetValues().Count);
                        Assert.AreEqual(oldBathymetry.GetValues(), importedBathymetry.GetValues());
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
            string targetDataDirectory = "dir";
            importer = new WaveDepthFileImporter("Waves Model", null) { TargetDataDirectory = targetDataDirectory };
            Assert.AreEqual(targetDataDirectory, importer.TargetDataDirectory);
        }

        [Test]
        public void ShouldCancelTest()
        {
            importer = new WaveDepthFileImporter("Waves Model", null) { ShouldCancel = true };
            Assert.AreEqual(true, importer.ShouldCancel);
            importer.ShouldCancel = false;
            Assert.AreEqual(false, importer.ShouldCancel);
        }

        [Test]
        public void ProgressChangedTest()
        {
            importer = new WaveDepthFileImporter("Waves Model", null);
            bool succes = false;
            importer.ProgressChanged = (name, current, total) => { succes = true; };
            importer.ProgressChanged("Importing depth file...", 1, 2);
            Assert.IsTrue(succes);
        }

        private static DeltaShellApplication GetRunningApplication(string savePath)
        {
            var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true };
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Run();
            app.SaveProjectAs(savePath);
            return app;
        }

        private static CurvilinearCoverage CreateCurvilinearCoverageWithValues(int length, int width)
        {
            int size = length * width;
            var bathymetry = new CurvilinearCoverage();
            var x = new double[size];
            var y = new double[size];
            var values = new double[size];
            for (int i = 0; i < size; i++)
            {
                x[i] = i;
                y[i] = i;
                values[i] = i;
            }
            bathymetry.Resize(length, width, x, y);
            bathymetry.SetValues(values);
            return bathymetry;
        }

        private static CurvilinearGrid CreateCurvilinearGrid(int length, int width)
        {
            int size = length * width;

            var x = new double[size];
            var y = new double[size];
            for (int i = 0; i < size; i++)
            {
                x[i] = i;
                y[i] = i;
            }
            var grid = new CurvilinearGrid(length, width, x, y, WaveModel.CoordinateSystemType.Spherical);
            return grid;
        }
    }
}
