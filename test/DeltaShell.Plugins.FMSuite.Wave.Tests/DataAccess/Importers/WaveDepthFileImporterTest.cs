using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using DeltaShell.Plugins.SharpMapGis;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Importers
{
    [TestFixture]
    public class WaveDepthFileImporterTest
    {
        private WaveDepthFileImporter importer;

        [Test]
        public void NamePropertyTest()
        {
            const string expected = "Delft3D Depth File";
            importer = new WaveDepthFileImporter("Waves Model", null);
            Assert.AreEqual(expected, importer.Name);
        }

        [Test]
        public void CategoryPropertyTest()
        {
            const string expected = "Waves Model";
            importer = new WaveDepthFileImporter("Waves Model", null);
            Assert.AreEqual(expected, importer.Category);
        }

        [Test]
        public void SupportedItemTypesTypesPropertyTest()
        {
            var expected = new List<Type>
            {
                typeof(CurvilinearCoverage)
            };
            importer = new WaveDepthFileImporter("Waves Model", null);
            Assert.AreEqual(expected, importer.SupportedItemTypes);
        }

        [Test]
        public void CanImportOnRootLevel_Always_ReturnsFalse()
        {
            // Setup
            importer = new WaveDepthFileImporter("Waves Model", null);

            // Call 
            bool canImportOnRootLevel = importer.CanImportOnRootLevel;

            // Assert
            Assert.That(canImportOnRootLevel, Is.False);
        }

        [Test]
        [TestCaseSource(nameof(GetCurvilinearCoverageConfigurations))]
        public void CanImportOn_WithVariousCurvilinearCoverageConfigurations_ReturnsExpectedValue(
            object argument, bool expectedValue)
        {
            // Setup
            importer = new WaveDepthFileImporter("Waves Model", null);

            // Call
            bool canImport = importer.CanImportOn(argument);

            // Assert
            Assert.That(canImport, Is.EqualTo(expectedValue));
        }

        [Test]
        public void FileFilterTest()
        {
            const string expected = "Delft3D Depth File (*.dep)|*.dep|All Files (*.*)|*.*";
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
            string saveDirPath = FileUtils.CreateTempDirectory();
            const string projectName = "MyProject";
            string savePath = Path.Combine(saveDirPath, projectName + ".dsproj");
            const int size = 3;
            CurvilinearCoverage oldBathymetry = CreateCurvilinearCoverageWithValues(size, size);

            try
            {
                using (IApplication app = GetRunningApplication())
                {
                    IProjectService projectService = app.ProjectService;
                    importer = new WaveDepthFileImporter("Waves Model", () => projectService.Project.RootFolder.GetAllModelsRecursive().OfType<WaveModel>());

                    using (var model = new WaveModel())
                    {
                        Project project = projectService.CreateProject();
                        project.RootFolder.Add(model);

                        model.OuterDomain.Grid = CreateCurvilinearGrid(size, size);
                        model.OuterDomain.Bathymetry = oldBathymetry;

                        Assert.AreEqual(size, model.OuterDomain.Bathymetry.Size1);

                        projectService.SaveProjectAs(savePath);

                        string depFilePath = Directory.GetFiles(Path.Combine(saveDirPath, projectName + ".dsproj_data"),
                                                                "*.dep",
                                                                SearchOption.AllDirectories).FirstOrDefault();
                        Assert.That(depFilePath, Is.Not.Null.And.Not.Empty, "There was no .dep file created.");

                        model.OuterDomain.Bathymetry = new CurvilinearCoverage();
                        Assert.AreEqual(0, model.OuterDomain.Bathymetry.Size1);

                        importer.ImportItem(depFilePath, model.OuterDomain.Bathymetry);

                        CurvilinearCoverage importedBathymetry = model.OuterDomain.Bathymetry;

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
            const string targetDataDirectory = "dir";
            importer = new WaveDepthFileImporter("Waves Model", null)
            {
                TargetDataDirectory = targetDataDirectory
            };
            Assert.AreEqual(targetDataDirectory, importer.TargetDataDirectory);
        }

        [Test]
        public void ShouldCancelTest()
        {
            importer = new WaveDepthFileImporter("Waves Model", null)
            {
                ShouldCancel = true
            };
            Assert.AreEqual(true, importer.ShouldCancel);
            importer.ShouldCancel = false;
            Assert.AreEqual(false, importer.ShouldCancel);
        }

        [Test]
        public void ProgressChangedTest()
        {
            importer = new WaveDepthFileImporter("Waves Model", null);
            var success = false;
            importer.ProgressChanged = (name, current, total) =>
            {
                success = true;
            };
            importer.ProgressChanged("Importing depth file...", 1, 2);
            Assert.IsTrue(success);
        }

        /// <summary>
        /// GIVEN a bathymetry file
        /// AND a wave model with this bathymetry file loaded
        /// WHEN this bathymetry file is loaded again
        /// THEN the values of the bathymetry should be unchanged.
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void
            GivenABathymetryFileAndAWaveModelWithThisBathymetryFileLoaded_WhenThisBathymetryFileIsLoadedAgain_ThenTheValuesOfTheBathymetryShouldBeUnchanged()
        {
            // Given
            var model = new WaveModel();
            Func<IEnumerable<WaveModel>> getModelsFunc = () => new List<WaveModel>()
            {
                model
            };
            var importerWithFunc = new WaveDepthFileImporter("Waves Model", getModelsFunc);
            string fileDataPath = Path.Combine(TestHelper.GetTestDataDirectory(), "SimpleBathemetry");

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Save created model
                string modelDirPath = Path.Combine(tempDir, "waves-model");
                FileUtils.CreateDirectoryIfNotExists(modelDirPath);

                const string mdwFileName = "muffins.mdw";
                string mdwFilePath = Path.Combine(modelDirPath, mdwFileName);
                model.ModelSaveTo(mdwFilePath, true);

                // Copy relevant data
                FileUtils.CopyDirectory(fileDataPath, modelDirPath);
                FileUtils.CopyDirectory(fileDataPath, tempDir);

                // Load relevant data onto the model
                const string grdFileName = "Outer.grd";
                model.OuterDomain.BedLevelGridFileName = grdFileName;
                WaveModel.LoadGrid(modelDirPath, model.OuterDomain);

                const string depFileName = "Outer.dep";
                model.OuterDomain.BedLevelFileName = depFileName;
                WaveModel.LoadBathymetry(model, modelDirPath, model.OuterDomain);

                model.ModelSaveTo(mdwFilePath, true);

                // Make a copy of the relevant data to compare later
                var bathymetryDataBefore =
                    (IMultiDimensionalArray<double>)
                    model.OuterDomain.Bathymetry.Components[0].Values.Clone();

                // When
                string depImportPath = Path.Combine(tempDir, depFileName);
                importerWithFunc.ImportItem(depImportPath,
                                            model.OuterDomain.Bathymetry);

                // Then
                IMultiDimensionalArray bathymetryDataAfter =
                    model.OuterDomain.Bathymetry.Components[0].Values;

                Assert.That(bathymetryDataAfter.Count,
                            Is.EqualTo(bathymetryDataBefore.Count));

                for (var i = 0; i < bathymetryDataAfter.Count; i++)
                {
                    Assert.That(bathymetryDataAfter[i],
                                Is.EqualTo(bathymetryDataBefore[i]));
                }
            });
        }

        private static IEnumerable<TestCaseData> GetCurvilinearCoverageConfigurations()
        {
            yield return new TestCaseData(null, true)
                .SetName("Null value");
            yield return new TestCaseData(new object(), true)
                .SetName("Not CurvilinearCoverage");

            yield return new TestCaseData(new CurvilinearCoverage
                {
                    Store = new WavmFileFunctionStore(string.Empty)
                }, false)
                .SetName("CurvilinearCoverage with WavmFileFunctionStore");

            yield return new TestCaseData(new CurvilinearCoverage
                {
                    Store = Substitute.For<IFunctionStore>()
                }, true)
                .SetName("CurvilinearCoverage with generic FunctionStore");
        }

        private static IApplication GetRunningApplication()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
            };
            var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
            
            app.Run();
            return app;
        }

        private static CurvilinearCoverage CreateCurvilinearCoverageWithValues(int length, int width)
        {
            int size = length * width;
            var bathymetry = new CurvilinearCoverage();
            var x = new double[size];
            var y = new double[size];
            var values = new double[size];
            for (var i = 0; i < size; i++)
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
            for (var i = 0; i < size; i++)
            {
                x[i] = i;
                y[i] = i;
            }

            var grid = new CurvilinearGrid(length, width, x, y, WaveModel.CoordinateSystemType.Spherical);
            return grid;
        }
    }
}