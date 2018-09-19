using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.IO.Importers
{
    [TestFixture]
    class WaveBoundaryFileImporterTest
    {
        private WaveBoundaryFileImporter importer;

        [Test]
        public void NamePropertyTest()
        {
            var expected = "Wave Boundary Conditions (*.bcw)";
            importer = new WaveBoundaryFileImporter();
            Assert.AreEqual(expected, importer.Name);
        }

        [Test]
        public void SupportedItemTypesTypesPropertyTest()
        {
            var expected = new List<Type> {typeof(IList<WaveBoundaryCondition>)};
            importer = new WaveBoundaryFileImporter();
            Assert.AreEqual(expected, importer.SupportedItemTypes);
        }

        [Test]
        public void CanImportOnPropertyTest()
        {
            importer = new WaveBoundaryFileImporter();
            Assert.IsTrue(importer.CanImportOn(new object()));
            Assert.IsFalse(importer.CanImportOnRootLevel);
        }

        [Test]
        public void FileFilterTest()
        {
            var expected = "Wave Boundary Condition Files (*.bcw;*.sp2)|*.bcw;*.sp2";
            importer = new WaveBoundaryFileImporter();
            Assert.AreEqual(expected, importer.FileFilter);
        }

        [Test]
        public void OpenViewAfterImport()
        {
            importer = new WaveBoundaryFileImporter();
            Assert.IsTrue(importer.OpenViewAfterImport);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItemTest_WhenBoundaryConditionModelDoesNotExist_ThenWarningIsLogged()
        {
            importer = new WaveBoundaryFileImporter();
            var saveDirPath = FileUtils.CreateTempDirectory();
            var projectName = "MyProject";
            var savePath = Path.Combine(saveDirPath, projectName + ".dsproj");
            var boundaryName = "boundary";
            var boundary = CreateBoundary(boundaryName);
            var boundaryCondition = CreateBoundaryCondition(boundary);

            try
            {
                using (var app = GetRunningApplication(savePath))
                {
                    using (var model = new WaveModel())
                    {
                        var project = app.Project;
                        project.RootFolder.Add(model);

                        var refTime = model.ModelDefinition.ModelReferenceDateTime;
                        SetData(boundaryCondition, refTime);

                        model.Boundaries.Add(boundary);
                        model.BoundaryConditions.Add(boundaryCondition);

                        app.SaveProjectAs(savePath);

                        var bcwFilePath = Directory.GetFiles(Path.Combine(saveDirPath, projectName + ".dsproj_data"),
                            "*.bcw",
                            SearchOption.AllDirectories).FirstOrDefault();
                        Assert.IsNotNullOrEmpty(bcwFilePath, "There was no .bcw file created.");

                        model.BoundaryConditions.Clear();

                        Assert.AreEqual(model.BoundaryConditions.Count, 0);
                        TestHelper.AssertAtLeastOneLogMessagesContains(
                            () => importer.ImportItem(bcwFilePath, model.BoundaryConditions),
                            $"Could not import boundary condition; no boundary with name {boundaryName} found");
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(saveDirPath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItemTest_WhenBoundaryConditionModelIsNotTimeseries_ThenWarningIsLogged()
        {
            importer = new WaveBoundaryFileImporter();
            var saveDirPath = FileUtils.CreateTempDirectory();
            var projectName = "MyProject";
            var savePath = Path.Combine(saveDirPath, projectName + ".dsproj");
            var boundaryName = "boundary";
            var boundary = CreateBoundary(boundaryName);
            var boundaryCondition = CreateBoundaryCondition(boundary);

            try
            {
                using (var app = GetRunningApplication(savePath))
                {
                    using (var model = new WaveModel())
                    {
                        var project = app.Project;
                        project.RootFolder.Add(model);

                        var refTime = model.ModelDefinition.ModelReferenceDateTime;
                        SetData(boundaryCondition, refTime);

                        model.Boundaries.Add(boundary);
                        model.BoundaryConditions.Add(boundaryCondition);

                        app.SaveProjectAs(savePath);

                        var bcwFilePath = Directory.GetFiles(Path.Combine(saveDirPath, projectName + ".dsproj_data"),
                            "*.bcw", SearchOption.AllDirectories).FirstOrDefault();
                        Assert.IsNotNullOrEmpty(bcwFilePath, "There was no .bcw file created.");

                        model.BoundaryConditions.Clear();
                        Assert.AreEqual(model.BoundaryConditions.Count, 0);

                        var newBoundaryCondition =
                            new WaveBoundaryCondition(BoundaryConditionDataType.ParametrizedSpectrumConstant)
                            {
                                Name = boundaryName
                            };

                        model.BoundaryConditions.Add(newBoundaryCondition);

                        var message =
                            $"Could not import boundary condition; boundary {boundaryName} is not of type {BoundaryConditionDataType.ParametrizedSpectrumTimeseries}";
                        TestHelper.AssertAtLeastOneLogMessagesContains(
                            () => importer.ImportItem(bcwFilePath, model.BoundaryConditions), message);
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(saveDirPath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItemTest_WhenBoundaryConditionModelNumberOfSupportPointsDoNotMatchWithFile_ThenWarningIsLogged()
        {
            importer = new WaveBoundaryFileImporter();
            var saveDirPath = FileUtils.CreateTempDirectory();
            var projectName = "MyProject";
            var savePath = Path.Combine(saveDirPath, projectName + ".dsproj");
            var boundaryName = "boundary";
            var boundary = CreateBoundary(boundaryName);
            var boundaryCondition = CreateBoundaryCondition(boundary);

            try
            {
                using (var app = GetRunningApplication(savePath))
                {
                    using (var model = new WaveModel())
                    {
                        var project = app.Project;
                        project.RootFolder.Add(model);

                        var refTime = model.ModelDefinition.ModelReferenceDateTime;
                        SetData(boundaryCondition, refTime);

                        model.Boundaries.Add(boundary);
                        model.BoundaryConditions.Add(boundaryCondition);

                        app.SaveProjectAs(savePath);

                        var bcwFilePath = Directory.GetFiles(Path.Combine(saveDirPath, projectName + ".dsproj_data"),
                            "*.bcw", SearchOption.AllDirectories).FirstOrDefault();
                        Assert.IsNotNullOrEmpty(bcwFilePath, "There was no .bcw file created.");

                        model.BoundaryConditions.Clear();
                        Assert.AreEqual(model.BoundaryConditions.Count, 0);

                        var newBoundaryCondition =
                            new WaveBoundaryCondition(BoundaryConditionDataType.ParametrizedSpectrumTimeseries)
                            {
                                Name = boundaryName,
                                Feature = boundary
                            };

                        model.BoundaryConditions.Add(newBoundaryCondition);

                        var message =
                            $"Could not import data onto boundary {boundaryName}; number of timeseries in file ({1}) did not match the number of support points ({0})";
                        TestHelper.AssertAtLeastOneLogMessagesContains(
                            () => importer.ImportItem(bcwFilePath, model.BoundaryConditions), message);
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(saveDirPath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportItemTest_CorrectDataIsSetOnImportedBoundaryCondition()
        {
            importer = new WaveBoundaryFileImporter();
            var saveDirPath = FileUtils.CreateTempDirectory();
            var projectName = "MyProject";
            var savePath = Path.Combine(saveDirPath, projectName + ".dsproj");
            var boundaryName = "boundary";
            var boundary = CreateBoundary(boundaryName);
            var boundaryCondition = CreateBoundaryCondition(boundary);

            try
            {
                using (var app = GetRunningApplication(savePath))
                {
                    using (var model = new WaveModel())
                    {
                        var project = app.Project;
                        project.RootFolder.Add(model);

                        var refTime = model.ModelDefinition.ModelReferenceDateTime;
                        SetData(boundaryCondition, refTime);

                        model.Boundaries.Add(boundary);
                        model.BoundaryConditions.Add(boundaryCondition);

                        app.SaveProjectAs(savePath);

                        var bcwFilePath = Directory.GetFiles(Path.Combine(saveDirPath, projectName + ".dsproj_data"),
                            "*.bcw", SearchOption.AllDirectories).FirstOrDefault();
                        Assert.IsNotNullOrEmpty(bcwFilePath, "There was no .bcw file created.");

                        model.BoundaryConditions.Clear();
                        Assert.AreEqual(model.BoundaryConditions.Count, 0);

                        var newBoundaryCondition =
                            new WaveBoundaryCondition(BoundaryConditionDataType.ParametrizedSpectrumTimeseries)
                            {
                                Name = boundaryName,
                                Feature = boundary
                            };
                        newBoundaryCondition.AddPoint(1);

                        model.BoundaryConditions.Add(newBoundaryCondition);

                        importer.ImportItem(bcwFilePath, model.BoundaryConditions);

                        var setBoundaryCondition = model.BoundaryConditions.FirstOrDefault();

                        Assert.IsTrue(model.BoundaryConditions.Count == 1);
                        Assert.IsTrue(setBoundaryCondition != null);
                        Assert.IsTrue(setBoundaryCondition.DataPointIndices.Count == 1);
                        Assert.AreEqual(new[] {refTime, refTime.AddDays(1)},
                            setBoundaryCondition.GetDataAtPoint(0).Arguments[0].Values);
                        Assert.AreEqual(new double[] {1, 2},
                            setBoundaryCondition.GetDataAtPoint(0).Components[0].Values);
                        Assert.AreEqual(new double[] {3, 4},
                            setBoundaryCondition.GetDataAtPoint(0).Components[1].Values);
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
            importer = new WaveBoundaryFileImporter {TargetDataDirectory = targetDataDirectory};
            Assert.AreEqual(targetDataDirectory, importer.TargetDataDirectory);
        }

        [Test]
        public void ShouldCancelTest()
        {
            importer = new WaveBoundaryFileImporter {ShouldCancel = true};
            Assert.AreEqual(true, importer.ShouldCancel);
            importer.ShouldCancel = false;
            Assert.AreEqual(false, importer.ShouldCancel);
        }

        [Test]
        public void ProgressChangedTest()
        {
            importer = new WaveBoundaryFileImporter();
            bool succes = false;
            importer.ProgressChanged = (name, current, total) => { succes = true; };
            importer.ProgressChanged("Importing boundary file...", 1, 2);
            Assert.IsTrue(succes);
        }

        private static DeltaShellApplication GetRunningApplication(string savePath)
        {
            var app = new DeltaShellApplication {IsProjectCreatedInTemporaryDirectory = true};
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Run();
            app.SaveProjectAs(savePath);
            return app;
        }

        private static void SetData(WaveBoundaryCondition boundaryCondition, DateTime refTime)
        {
            boundaryCondition.PointData[0].Arguments[0].SetValues(new[] {refTime, refTime.AddDays(1)});
            boundaryCondition.PointData[0].Components[0].SetValues(new double[] {1, 2});
            boundaryCondition.PointData[0].Components[1].SetValues(new double[] {3, 4});
        }

        private static WaveBoundaryCondition CreateBoundaryCondition(Feature2D boundary)
        {
            var boundaryCondition = (WaveBoundaryCondition) new WaveBoundaryConditionFactory().CreateBoundaryCondition(
                boundary, "",
                BoundaryConditionDataType.ParametrizedSpectrumTimeseries);
            boundaryCondition.AddPoint(1);
            return boundaryCondition;
        }

        private static Feature2D CreateBoundary(string boundaryName)
        {
            var boundary = new Feature2D
            {
                Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(0, 1)}),
                Name = boundaryName
            };
            return boundary;
        }
    }
}