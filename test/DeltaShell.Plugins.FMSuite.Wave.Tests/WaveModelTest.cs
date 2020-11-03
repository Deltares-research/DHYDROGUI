using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveModelTest
    {
        [Test]
        public void Constructor_SetsCorrectBoundaryContainer()
        {
            // Call
            using (var model = new WaveModel())
            {
                // Assert
                Assert.That(model.BoundaryContainer, Is.Not.Null);
            }
        }

        [Test]
        public void Constructor_SetsCorrectWaveOutputData()
        {
            // Call
            using (var model = new WaveModel())
            {
                // Assert
                Assert.That(model.WaveOutputData, Is.Not.Null);
            }
        }

        [Test]
        public void Constructor_AddingABoundaryToTheBoundaryContainerShouldFireCollectionChangedEvent()
        {
            // Call
            using (var model = new WaveModel())
            {
                var waveBoundary = Substitute.For<IWaveBoundary>();

                var counter = 0;

                ((INotifyCollectionChanged) model).CollectionChanged += delegate { counter += 1; };

                model.BoundaryContainer.Boundaries.Add(waveBoundary);

                Assert.AreEqual(1, counter);
            }
        }

        [Test]
        public void DefaultConstructor_SetsCorrectTimeProperties()
        {
            // Setup
            using (var model = new WaveModel())
            {
                // Assert
                WaveModelDefinition modelDefinition = model.ModelDefinition;
                DateTime modelReferenceDateTime = modelDefinition.ModelReferenceDateTime;

                DateTime currentTime = DateTime.Now;
                var expectedDateTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day);
                Assert.That(modelReferenceDateTime, Is.EqualTo(expectedDateTime));
                Assert.That(model.StartTime, Is.EqualTo(modelReferenceDateTime));
                Assert.That(model.StopTime, Is.EqualTo(modelReferenceDateTime.AddDays(1)));
            }
        }

        [Test]
        public void ConstructorWithParameter_SetsCorrectTimeProperties()
        {
            // Setup
            string waveFilePath = TestHelper.GetTestFilePath(@"mdw_coordinates\cartesian.mdw");
            string localFilePath = TestHelper.CreateLocalCopy(waveFilePath);
            using (var model = new WaveModel(localFilePath))
            {
                // Assert
                WaveModelDefinition modelDefinition = model.ModelDefinition;
                DateTime modelReferenceDateTime = modelDefinition.ModelReferenceDateTime;

                Assert.That(modelReferenceDateTime, Is.EqualTo(new DateTime(2000, 07, 14)));
                Assert.That(model.StartTime, Is.EqualTo(modelReferenceDateTime));
                Assert.That(model.StopTime, Is.EqualTo(modelReferenceDateTime.AddDays(1)));
            }
        }

        [Test]
        public void Constructor_SetsDefaultWorkingDirectoryPathFunc()
        {
            // Call
            using (var model = new WaveModel())
            {
                // Assert
                Assert.That(model.WorkingDirectoryPathFunc, Is.Not.Null);
                Assert.That(model.WorkingDirectoryPathFunc(), Is.EqualTo($"{Path.GetTempPath()}DeltaShell_Working_Directory"));
            }
        }

        [Test]
        public void GetWorkingDirectoryPathFunc_ReturnsCorrectFunc()
        {
            // Setup
            Func<string> func = () => "working_dir";
            using (var model = new WaveModel {WorkingDirectoryPathFunc = func})
            {
                // Call
                Func<string> result = model.WorkingDirectoryPathFunc;

                // Assert
                Assert.That(result, Is.SameAs(func));
            }
        }

        [Test]
        public void GetDimrExportDirectoryPath_ReturnsCorrectPath()
        {
            // Setup
            using (var model = new WaveModel
            {
                WorkingDirectoryPathFunc = () => "working_dir",
                Name = "model_name"
            })
            {
                // Call
                string result = model.DimrExportDirectoryPath;

                // Assert
                Assert.That(result, Is.EqualTo("working_dir\\model_name"));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void DimrExportDirectoryPath_ShouldAlwaysBeUpToDate()
        {
            // Setup
            var provider = new PathProvider {Path = "orig_working_dir"};

            using (var model = new WaveModel
            {
                Name = "model_name",
                WorkingDirectoryPathFunc = () => provider.Path
            })
            {
                // Precondition
                Assert.That(model.DimrExportDirectoryPath, Is.EqualTo("orig_working_dir\\model_name"));

                // Call
                provider.Path = "new_working_dir";

                // Assert
                Assert.That(model.DimrExportDirectoryPath, Is.EqualTo("new_working_dir\\model_name"));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadTestModelFromFile()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw");
            Assert.IsTrue(File.Exists(mdwPath));

            var waveModel = new WaveModel(mdwPath);

            CurvilinearGrid grid = waveModel.OuterDomain.Grid;
            Assert.IsTrue(grid != null);
            Assert.AreEqual(121, grid.Arguments[0].Values.Count); // N or y-direction
            Assert.AreEqual(236, grid.Arguments[1].Values.Count); // M or x-direction
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTestModelWithFlowCouplingFromFile()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"flow_coupled/wave.mdw");
            Assert.IsTrue(File.Exists(mdwPath));

            var waveModel = new WaveModel(mdwPath);

            Assert.AreEqual(false, waveModel.OuterDomain.HydroFromFlowData.UseDefaultHydroFromFlowSettings);
            Assert.AreEqual(UsageFromFlowType.UseAndExtend, waveModel.OuterDomain.HydroFromFlowData.WaterLevelUsage);
            Assert.AreEqual(UsageFromFlowType.UseAndExtend, waveModel.OuterDomain.HydroFromFlowData.BedLevelUsage);
            Assert.AreEqual(UsageFromFlowType.UseAndExtend, waveModel.OuterDomain.HydroFromFlowData.WindUsage);
            Assert.AreEqual(UsageFromFlowType.UseAndExtend, waveModel.OuterDomain.HydroFromFlowData.VelocityUsage);
            Assert.AreEqual(VelocityComputationType.DepthAveraged,
                            waveModel.OuterDomain.HydroFromFlowData.VelocityUsageType); // not specified, hence default
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TransformModelCoordinateSystemUpdatesGridCoordinateSystem()
        {
            const int wgs84CS = 4326;
            const int rd = 28992;

            string localMdwPath = WaveTestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"flow_coupled/wave.mdw"));
            var waveModel = new WaveModel(localMdwPath) {CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(wgs84CS)};

            ICoordinateSystem src = new OgrCoordinateSystemFactory().CreateFromEPSG(wgs84CS);
            ICoordinateSystem target = new OgrCoordinateSystemFactory().CreateFromEPSG(rd);
            ICoordinateTransformation transformation = new OgrCoordinateSystemFactory().CreateTransformation(src, target);

            CurvilinearGrid grid = waveModel.OuterDomain.Grid;
            Assert.IsTrue(grid.CoordinateSystem.IsGeographic);

            string coordinateSystemType;
            Assert.IsTrue(grid.Attributes.TryGetValue(CurvilinearGrid.CoordinateSystemKey, out coordinateSystemType));
            Assert.AreEqual("Spherical", coordinateSystemType);

            waveModel.TransformCoordinates(transformation);

            Assert.IsFalse(grid.CoordinateSystem.IsGeographic);
            Assert.IsTrue(grid.Attributes.TryGetValue(CurvilinearGrid.CoordinateSystemKey, out coordinateSystemType));
            Assert.AreEqual("Cartesian", coordinateSystemType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void TransformModelCoordinateSystem()
        {
            const int nad27_utm16N = 26916;
            const int pseudo_webm = 3857;

            string localMdwPath =
                WaveTestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw"));
            var waveModel = new WaveModel(localMdwPath) {CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(nad27_utm16N)};

            ICoordinateSystem src = new OgrCoordinateSystemFactory().CreateFromEPSG(nad27_utm16N);
            ICoordinateSystem target = new OgrCoordinateSystemFactory().CreateFromEPSG(pseudo_webm);
            ICoordinateTransformation fromUTM16ToWebMercator = new OgrCoordinateSystemFactory().CreateTransformation(src, target);
            ICoordinateTransformation fromWebMercatorToUTM16 = new OgrCoordinateSystemFactory().CreateTransformation(target, src);

            List<Coordinate> coordinates =
                WaveModelCoordinateConversion.GetAllModelFeatures(waveModel)
                                             .SelectMany(f => f.Geometry.Coordinates)
                                             .ToList();

            waveModel.TransformCoordinates(fromUTM16ToWebMercator);
            waveModel.TransformCoordinates(fromWebMercatorToUTM16);

            List<Coordinate> coordinatesAfter =
                WaveModelCoordinateConversion.GetAllModelFeatures(waveModel)
                                             .SelectMany(f => f.Geometry.Coordinates)
                                             .ToList();

            Assert.AreEqual(coordinatesAfter.Count, coordinates.Count);
            for (var i = 0; i < coordinates.Count; ++i)
            {
                Assert.AreEqual(coordinatesAfter[i].X, coordinates[i].X, 1e-05);
                Assert.AreEqual(coordinatesAfter[i].Y, coordinates[i].Y, 1e-05);
            }
        }

        [Test]
        public void WaveModel_WaveSetup_DefaultValue_IsFalse()
        {
            var waveModel = new WaveModel();
            Assert.IsFalse(waveModel.ModelDefinition.WaveSetup);
        }

        [Test]
        public void WaveModel_LogMessage_IsShown_When_WaveSetup_SetsValue_True()
        {
            var waveModel = new WaveModel();
            string expectedMssg = Resources
                .WaveModel_WaveSetup_With_WaveSetup_set_to_True_parallel_runs_will_fail__normal_runs_with_lakes_will_produce_unreliable_values_;
            TestHelper.AssertAtLeastOneLogMessagesContains(() => waveModel.ModelDefinition.WaveSetup = true, expectedMssg);
            Assert.IsTrue(waveModel.ModelDefinition.WaveSetup);
        }

        [Test]
        public void GivenAWaveModel_WhenSettingBedfrictionToCollins_ThenTheBedfrictionCoefficientShouldAlsoBeChanged()
        {
            var waveModel = new WaveModel();

            WaveModelProperty prop = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory,
                                                                                KnownWaveProperties.BedFriction);
            WaveModelProperty prop2 = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory,
                                                                                 KnownWaveProperties.BedFrictionCoef);

            Assert.AreEqual("0.038", prop2.GetValueAsString());
            prop.SetValueAsString("collins");
            Assert.AreEqual("0.015", prop2.GetValueAsString());
        }

        [Test]
        public void GivenAWaveModel_WhenSettingBedfrictionToMadsenetal_ThenTheBedfrictionCoefficientShouldAlsoBeChanged()
        {
            var waveModel = new WaveModel();

            WaveModelProperty prop = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory,
                                                                                KnownWaveProperties.BedFriction);
            WaveModelProperty prop2 = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory,
                                                                                 KnownWaveProperties.BedFrictionCoef);

            Assert.AreEqual("0.038", prop2.GetValueAsString());
            prop.SetValueAsString("madsen et al.");
            Assert.AreEqual("0.05", prop2.GetValueAsString());
        }

        [Test]
        public void GivenAWaveModel_WhenSettingSimModeToNonStationary_ThenTheMaxIterationPropertyShouldAlsoBeChanged()
        {
            var waveModel = new WaveModel();

            WaveModelProperty prop = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory,
                                                                                KnownWaveProperties.SimulationMode);
            WaveModelProperty prop2 = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.NumericsCategory,
                                                                                 KnownWaveProperties.MaxIter);

            Assert.AreEqual("50", prop2.GetValueAsString());
            prop.SetValueAsString("non-stationary");
            Assert.AreEqual("15", prop2.GetValueAsString());
        }

        [Test]
        public void
            GivenWaveModelWithOuterDomainWithCoordinateSystemSetWhenAddingInnerDomainThenInnerDomainShouldGetSameCoordinateSystem()
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
                waveGridFileImporter.ImportItem(waveGridFileFilePath, waveModel.OuterDomain.Grid);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.CoordinateSystem, Is.Not.Null);
                waveModel.AddSubDomain(waveModel.OuterDomain, new WaveDomainData("inner"));
                Assert.That(waveModel.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.OuterDomain.SubDomains[0].Grid.CoordinateSystem, Is.Not.Null);
            }
            finally
            {
                FileUtils.DeleteIfExists(tempWorkingDirectory);
            }
        }

        [Test]
        public void
            GivenWaveModelWithOuterDomainWithCoordinateSystemSetWhenSettingExteriorDomainThenExteriorDomainShouldGetSameCoordinateSystem()
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
                waveGridFileImporter.ImportItem(waveGridFileFilePath, waveModel.OuterDomain.Grid);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.CoordinateSystem, Is.Not.Null);
                var exterior = new WaveDomainData("exterior");
                IWaveDomainData oldOuterDomain = waveModel.OuterDomain;
                waveModel.OuterDomain = exterior;
                waveModel.AddSubDomain(exterior, oldOuterDomain);
                Assert.That(waveModel.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.OuterDomain.SubDomains[0].Grid.CoordinateSystem, Is.Not.Null);
            }
            finally
            {
                FileUtils.DeleteIfExists(tempWorkingDirectory);
            }
        }

        [Test]
        public void
            GivenWaveModelWithOuterDomainWithCoordinateSystemSetWhenAddingInnerDomainTwiceThenInnerDomainsShouldGetSameCoordinateSystem()
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
                waveGridFileImporter.ImportItem(waveGridFileFilePath, waveModel.OuterDomain.Grid);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.CoordinateSystem, Is.Not.Null);
                var waveDomainData = new WaveDomainData("inner");
                waveModel.AddSubDomain(waveModel.OuterDomain, waveDomainData);
                waveModel.AddSubDomain(waveDomainData, new WaveDomainData("innerior"));
                Assert.That(waveModel.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.OuterDomain.SubDomains[0].Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.OuterDomain.SubDomains[0].SubDomains[0].Grid.CoordinateSystem, Is.Not.Null);
            }
            finally
            {
                FileUtils.DeleteIfExists(tempWorkingDirectory);
            }
        }

        [Test]
        public void GivenWaveModelWithSphericalCoordinates_WhenSettingCoordinateSystem_ThenOuterDomainGridHasTheSameCoordinateSystem()
        {
            string waveFilePath = TestHelper.GetTestFilePath(@"mdw_coordinates\spherical.mdw");
            string localFilePath = TestHelper.CreateLocalCopy(waveFilePath);
            try
            {
                var waveModel = new WaveModel(localFilePath);
                Assert.That(waveModel.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.CoordinateSystem.EqualsTo(new OgrCoordinateSystemFactory().CreateFromEPSG(4326)), Is.True);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem.EqualsTo(new OgrCoordinateSystemFactory().CreateFromEPSG(4326)), Is.True);
            }
            finally
            {
                FileUtils.DeleteIfExists(localFilePath);
            }
        }

        [Test]
        public void GivenWaveModelWithCartesianCoordinates_WhenSettingCoordinateSystem_ThenOuterDomainGridHasTheSameCoordinateSystem()
        {
            string waveFilePath = TestHelper.GetTestFilePath(@"mdw_coordinates\cartesian.mdw");
            string localFilePath = TestHelper.CreateLocalCopy(waveFilePath);
            try
            {
                var waveModel = new WaveModel(localFilePath);
                Assert.That(waveModel.CoordinateSystem, Is.Null);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Null);
                waveModel.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(3857);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem.EqualsTo(new OgrCoordinateSystemFactory().CreateFromEPSG(3857)), Is.True);
            }
            finally
            {
                FileUtils.DeleteIfExists(localFilePath);
            }
        }

        [Test]
        [Category(TestCategory.Jira)] // D3DFMIQ-2272
        public void ClearOutput_WithSwanRunLogDataItem_ThenSwanRunLogContentIsEmpty()
        {
            //// Setup
            //var waveModel = new WaveModel();
            //var swanTextDocument = (TextDocument) waveModel.GetDataItemByTag(WaveModel.SwanLogDataItemTag).Value;
            //swanTextDocument.Content = new Random().Next(100).ToString();

            //// Private field outputIsEmpty is set to false after a successful model run. This field should be false when clearing model output.
            //// As we do not focus on model run, we use reflection to set this field and omit the model run.
            //TypeUtils.SetField(waveModel, "outputIsEmpty", false);

            //// Call
            //waveModel.ClearOutput();

            //// Assert
            //Assert.That(swanTextDocument.Content, Is.Empty, "Swan run log should be empty after clearing model output.");
            Assert.Fail("Should be improved as part of D3DFMIQ-2286");
        }

        [Test]
        [Category(TestCategory.Jira)] // D3DFMIQ-2272
        public void ClearOutput_WithOutputFunctions_ThenFunctionsAreRemovedFromModel()
        {
            // Setup
            var waveModel = new WaveModel();
            var function = Substitute.For<IFunction>();
            waveModel.WavmFunctionStores.Single().Functions.Add(function);

            // Private field outputIsEmpty is set to false after a successful model run. This field should be false when clearing model output.
            // As we do not focus on model run, we use reflection to set this field and omit the model run.
            TypeUtils.SetField(waveModel, "outputIsEmpty", false);

            // Call
            waveModel.ClearOutput();

            // Assert
            Assert.That(waveModel.WavmFunctionStores.Single().Functions, Is.Empty, "All output functions should be removed at clearing model output.");
        }

        [Test]
        public void CheckDefaultTimeDateWhenCreatingWaveTimePointData()
        {
            var waveInputFieldData = new WaveInputFieldData();

            // get the today datetime

            DateTime datetime = DateTime.Today;

            // Assert

            Assert.AreEqual(datetime, waveInputFieldData.InputFields.Arguments[0].DefaultValue);
        }

        [Test]
        public void IsCoupledToFlow_ShouldAlwaysBeFalseForAStandAloneModel()
        {
            var waveModel = new WaveModel();
            Assert.IsFalse(waveModel.IsCoupledToFlow);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConnectOutput_ShouldConnectWavmFileAndReadSwanDiagFile()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var waveModel = new WaveModel())
            {
                // Arrange
                string outputDirectory =
                    Path.Combine(TestHelper.GetTestDataDirectory(), "output_wavm", "Output1Domain");
                string outputDirectoryInTemp = tempDirectory.CopyDirectoryToTempDirectory(outputDirectory);

                // Act
                waveModel.ConnectOutput(outputDirectoryInTemp);

                // Assert
                Assert.AreEqual(Path.Combine(outputDirectoryInTemp, "wavm-Waves.nc"),
                                waveModel.WaveOutputData.WavmFileFunctionStores.First().Path);

                ReadOnlyTextFileData swnDiagData = waveModel.WaveOutputData.DiagnosticFiles.First(x => x.DocumentName == "swn-diag.Waves");
                Assert.AreEqual(File.ReadAllText(Path.Combine(outputDirectoryInTemp, "swn-diag.Waves")), swnDiagData.Content);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConnectOutput_WhenModelHasMultipleDomains_ShouldConnectWavmFileAndReadSwanDiagFile()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var waveModel = new WaveModel())
            {
                // Arrange
                waveModel.AddSubDomain(waveModel.OuterDomain, new WaveDomainData("Inner"));

                string outputDirectory =
                    Path.Combine(TestHelper.GetTestDataDirectory(), "output_wavm", "Output2Domains");
                string outputDirectoryInTemp = tempDirectory.CopyDirectoryToTempDirectory(outputDirectory);

                // Act
                waveModel.ConnectOutput(outputDirectoryInTemp);

                // Assert
                IEnumerable<WavmFileFunctionStore> functionStores = waveModel.WaveOutputData.WavmFileFunctionStores.ToList();
                Assert.AreEqual(2, functionStores.Count());

                string[] expectedPaths =
                {
                    Path.Combine(outputDirectoryInTemp, "wavm-Waves-Outer.nc"),
                    Path.Combine(outputDirectoryInTemp, "wavm-Waves-Inner.nc"),
                };

                string[] functionStorePaths = functionStores.Select(x => x.Path).ToArray();
                Assert.That(functionStorePaths, Is.EquivalentTo(expectedPaths));

                ReadOnlyTextFileData swnDiagData = waveModel.WaveOutputData.DiagnosticFiles.First(x => x.DocumentName == "swn-diag.Waves");
                Assert.AreEqual(File.ReadAllText(Path.Combine(outputDirectoryInTemp, "swn-diag.Waves")), swnDiagData.Content);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void IsMasterTimeStep_ShouldReturnInverseOfIsCoupledToFlow(bool isCoupledToFlow)
        {
            using (var waveModel = new WaveModel())
            {
                waveModel.IsCoupledToFlow = isCoupledToFlow;
                Assert.AreEqual(!isCoupledToFlow, waveModel.IsMasterTimeStep);
            }
        }

        [Test]
        public void GetDirectChildren_ContainsBoundaries()
        {
            // Setup
            var model = new WaveModel();

            IWaveBoundary[] boundaries = Enumerable.Range(0, 10).Select(_ => Substitute.For<IWaveBoundary>()).ToArray();
            model.BoundaryContainer.Boundaries.AddRange(boundaries);

            // Call
            IEnumerable<object> result = model.GetDirectChildren();

            // Assert
            foreach (IWaveBoundary waveBoundary in boundaries)
            {
                Assert.That(result, Has.Member(waveBoundary));
            }
        }

        private sealed class PathProvider
        {
            public string Path { get; set; }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ExportModelInputTo_ExportsToPath(bool switchTo)
        {
            using (var temp = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                const string currentPath = "current.mdw";
                model.MdwFile.MdwFilePath = currentPath;

                string exportPath = Path.Combine(temp.Path, "model.mdw");

                // Call
                model.ExportModelInputTo(exportPath, switchTo);

                // Assert
                Assert.That(exportPath, Does.Exist);
                Assert.That(model.MdwFilePath, Is.EqualTo(switchTo ? exportPath : currentPath));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ExportModelInputTo_PathNull_ThrowsArgumentException(bool switchTo)
        {
            using (var model = new WaveModel())
            {
                // Call
                void Call() => model.ExportModelInputTo(null, switchTo);

                // Assert
                var e = Assert.Throws<ArgumentException>(Call);
                Assert.That(e.ParamName, Is.EqualTo("mdwFilePath"));
            }
        }

        [Test]
        public void DisconnectOutput_UpdatesWaveOutputDataCorrectly()
        {
            using (var model = new WaveModel())
            {
                // Setup
                const string initialPath = "some/toad/to/a/directory";
                model.WaveOutputData.ConnectTo(initialPath, true);

                // Call
                model.DisconnectOutput();

                // Assert
                Assert.That(model.WaveOutputData.IsConnected, Is.False);
                Assert.That(model.WaveOutputData.DataSourcePath, Is.Null);
                Assert.That(model.WaveOutputData.IsStoredInWorkingDirectory, Is.False);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void ConnectOutput_UpdatesWaveOutputDataCorrectly(bool withInitialPath, 
                                                                 bool inWorkingDirectory)
        {
            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                string newPath = tempDir.CreateDirectory("newPath");

                // Setup
                if (withInitialPath)
                {
                    string initialPath = tempDir.CreateDirectory("initialPath");
                    model.WaveOutputData.ConnectTo(initialPath, true);
                }

                model.WorkingDirectoryPathFunc = () => inWorkingDirectory ? newPath : @"D:\nope\"; 

                // Call
                model.ConnectOutput(newPath);

                // Assert
                Assert.That(model.WaveOutputData.IsConnected, Is.True);
                Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(newPath));
                Assert.That(model.WaveOutputData.IsStoredInWorkingDirectory, Is.EqualTo(inWorkingDirectory));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Constructor_FromMdwPath_ConnectsToOutputDir()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                string testDataPath = TestHelper.GetTestFilePath(@"WaveModelTest\Waves\");
                string modelPath = tempDir.CopyDirectoryToTempDirectory(testDataPath);

                string mdwPath = Path.Combine(modelPath, "input", "Waves.mdw");
                string outputDir = Path.Combine(modelPath, "output");

                using (var model = new WaveModel(mdwPath))
                {
                    Assert.That(model.WaveOutputData.IsConnected, Is.True);
                    Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(outputDir));
                }
            }
        }

        [Test]
        public void Constructor_Empty_DoesNotConnectToOutputDir()
        {
            using (var model = new WaveModel())
            {
                Assert.That(model.WaveOutputData.IsConnected, Is.False);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAValidModelWithNoDataAvailable_WhenTheModelIsSavedAtTheSameLocation_ThenTheOutputFolderIsCleared()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                string testDataPath = TestHelper.GetTestFilePath(@"WaveModelTest\Waves");
                string modelPath = tempDir.CopyDirectoryToTempDirectory(testDataPath);

                string mdwPath = Path.Combine(modelPath, "input", "Waves.mdw");
                string outputDir = Path.Combine(modelPath, "output");

                using (var model = new WaveModel(mdwPath))
                {
                    // Mimic DeltaShell behaviour to first switch to the model before saving.
                    // This is necessary because the save to logic relies on having the correct
                    // previous save directory set, which is set as part of Switch to. 
                    // This is far from ideal, however changing this would require significant 
                    // changes to DeltaShell's save logic.
                    ((IFileBased) model).SwitchTo(modelPath);

                    // Disconnect data
                    model.WaveOutputData.Disconnect();

                    // When 
                    model.ModelSaveTo(mdwPath, true);

                    // Then
                    var outputDirInfo = new DirectoryInfo(outputDir);
                    Assert.That(outputDirInfo.EnumerateFiles(), Is.Empty);
                    Assert.That(outputDirInfo.EnumerateDirectories(), Is.Empty);

                    Assert.That(model.WaveOutputData.IsConnected, Is.False);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAValidModelWithNoDataAvailable_WhenTheModelIsSavedAtADifferentLocation_ThenTheOutputFolderIsEmpty()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                const string mdwFileName = "Waves.mdw";
                string testDataPath = TestHelper.GetTestFilePath(@"WaveModelTest\Waves");

                string modelPath = tempDir.CopyDirectoryToTempDirectory(testDataPath);
                string mdwPath = Path.Combine(modelPath, "input", mdwFileName);

                tempDir.CreateDirectory("AnotherWaves");
                string goalInputDir = tempDir.CreateDirectory("AnotherWaves\\input");
                string goalOutputDir = tempDir.CreateDirectory("AnotherWaves\\output");

                string goalMdwPath = Path.Combine(goalInputDir, mdwFileName);

                using (var model = new WaveModel(mdwPath))
                {
                    // Mimic DeltaShell behaviour to first switch to the model before saving.
                    // This is necessary because the save to logic relies on having the correct
                    // previous save directory set, which is set as part of Switch to. 
                    // This is far from ideal, however changing this would require significant 
                    // changes to DeltaShell's save logic.
                    ((IFileBased) model).SwitchTo(modelPath);

                    // Disconnect data
                    model.WaveOutputData.Disconnect();

                    // When 
                    model.ModelSaveTo(goalMdwPath, true);

                    // Then
                    var outputDirInfo = new DirectoryInfo(goalOutputDir);
                    Assert.That(outputDirInfo.EnumerateFiles(), Is.Empty);
                    Assert.That(outputDirInfo.EnumerateDirectories(), Is.Empty);

                    Assert.That(model.WaveOutputData.IsConnected, Is.False);
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAValidModelWithDataAvailableFromTheWorkingDirectory_WhenTheModelIsSavedAtTheSameLocation_ThenTheOutputFolderContainsTheDataFromTheWorkingLocation()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                const string mdwFileName = "Waves.mdw";
                string modelSourcePath = TestHelper.GetTestFilePath(@"WaveModelTest\Waves");

                string modelPath = tempDir.CopyDirectoryToTempDirectory(modelSourcePath);
                string mdwPath = Path.Combine(modelPath, "input", mdwFileName);
                string outputDir = Path.Combine(modelPath, "output");

                string alternativeOutputSourcePath = TestHelper.GetTestFilePath(@"WaveModelTest\alternative_output");
                string alternativeOutputPath = tempDir.CopyDirectoryToTempDirectory(alternativeOutputSourcePath);

                string outputReferencePath = TestHelper.GetTestFilePath(@"WaveModelTest\alternative_output_reference");
                FileCompareInfo[] origFileDataReference = 
                    CollectFileInformation(new DirectoryInfo(outputReferencePath)).ToArray();

                using (var model = new WaveModel(mdwPath))
                {
                    // Mimic DeltaShell behaviour to first switch to the model before saving.
                    // This is necessary because the save to logic relies on having the correct
                    // previous save directory set, which is set as part of Switch to. 
                    // This is far from ideal, however changing this would require significant 
                    // changes to DeltaShell's save logic.
                    ((IFileBased) model).SwitchTo(modelPath);

                    // Connect to different data source
                    model.WaveOutputData.ConnectTo(alternativeOutputPath, true);

                    // When 
                    model.ModelSaveTo(mdwPath, true);

                    // Then
                    var outputDirInfo = new DirectoryInfo(outputDir);
                    FileCompareInfo[] saveFileData = CollectFileInformation(outputDirInfo).ToArray();

                    AssertContainsSameFiles(origFileDataReference, saveFileData);

                    Assert.That(outputDirInfo.EnumerateDirectories(), Is.Empty);

                    Assert.That(model.WaveOutputData.IsConnected, Is.True);
                    Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(outputDirInfo.FullName));
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAValidModelWithDataAvailableInTheWorkingDirectory_WhenTheModelIsSavedAtADifferentLocation_ThenTheOutputFolderContainsTheDataFromTheWorkingLocation()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                const string mdwFileName = "Waves.mdw";
                string testDataPath = TestHelper.GetTestFilePath(@"WaveModelTest\Waves");

                string modelPath = tempDir.CopyDirectoryToTempDirectory(testDataPath);
                string mdwPath = Path.Combine(modelPath, "input", mdwFileName);

                tempDir.CreateDirectory("AnotherWaves");
                string goalInputDir = tempDir.CreateDirectory("AnotherWaves\\input");
                string goalOutputDir = tempDir.CreateDirectory("AnotherWaves\\output");

                string goalMdwPath = Path.Combine(goalInputDir, mdwFileName);

                string alternativeOutputSourcePath = TestHelper.GetTestFilePath(@"WaveModelTest\alternative_output");
                string alternativeOutputPath = tempDir.CopyDirectoryToTempDirectory(alternativeOutputSourcePath);

                string alternativeOutputReferencePath = TestHelper.GetTestFilePath(@"WaveModelTest\alternative_output_reference");
                FileCompareInfo[] origFileDataReference = 
                    CollectFileInformation(new DirectoryInfo(alternativeOutputReferencePath)).ToArray();

                using (var model = new WaveModel(mdwPath))
                {
                    // Mimic DeltaShell behaviour to first switch to the model before saving.
                    // This is necessary because the save to logic relies on having the correct
                    // previous save directory set, which is set as part of Switch to. 
                    // This is far from ideal, however changing this would require significant 
                    // changes to DeltaShell's save logic.
                    ((IFileBased) model).SwitchTo(modelPath);

                    // Connect to different data source
                    model.WaveOutputData.ConnectTo(alternativeOutputPath, true);

                    // When 
                    model.ModelSaveTo(goalMdwPath, true);

                    // Then
                    var outputDirInfo = new DirectoryInfo(goalOutputDir);
                    FileCompareInfo[] saveFileData = CollectFileInformation(outputDirInfo).ToArray();

                    AssertContainsSameFiles(origFileDataReference, saveFileData);

                    Assert.That(outputDirInfo.EnumerateDirectories(), Is.Empty);

                    Assert.That(model.WaveOutputData.IsConnected, Is.True);
                    Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(outputDirInfo.FullName));
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAValidModelWithDataAvailableInTheModelOutputPath_WhenTheModelIsSavedAtInADifferentLocation_ThenTheOutputFolderContainsTheSameData()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                const string mdwFileName = "Waves.mdw";
                string modelSourcePath = TestHelper.GetTestFilePath(@"WaveModelTest\Waves");

                string modelPath = tempDir.CopyDirectoryToTempDirectory(modelSourcePath);
                string mdwPath = Path.Combine(modelPath, "input", mdwFileName);
                string outputDir = Path.Combine(modelPath, "output");
                var outputDirInfo = new DirectoryInfo(outputDir);

                FileCompareInfo[] origFileData = CollectFileInformation(outputDirInfo).ToArray();

                tempDir.CreateDirectory("AnotherWaves");
                string goalInputDir = tempDir.CreateDirectory("AnotherWaves\\input");
                string goalOutputDir = tempDir.CreateDirectory("AnotherWaves\\output");

                string goalMdwPath = Path.Combine(goalInputDir, mdwFileName);

                using (var model = new WaveModel(mdwPath))
                {
                    // Mimic DeltaShell behaviour to first switch to the model before saving.
                    // This is necessary because the save to logic relies on having the correct
                    // previous save directory set, which is set as part of Switch to. 
                    // This is far from ideal, however changing this would require significant 
                    // changes to DeltaShell's save logic.
                    ((IFileBased) model).SwitchTo(modelPath);

                    // When 
                    model.ModelSaveTo(goalMdwPath, true);

                    // Then
                    var goalOutputDirInfo = new DirectoryInfo(goalOutputDir);
                    FileCompareInfo[] saveFileData = CollectFileInformation(goalOutputDirInfo).ToArray();

                    AssertContainsSameFiles(origFileData, saveFileData);
                    Assert.That(outputDirInfo.EnumerateDirectories(), Is.Empty);

                    Assert.That(model.WaveOutputData.IsConnected, Is.True);
                    Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(goalOutputDirInfo.FullName));
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAValidModelWithDataAvailableInTheModelOutputPath_WhenTheModelIsSavedAtTheSameLocation_ThenTheOutputFolderContainsTheSameData()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                const string mdwFileName = "Waves.mdw";
                string modelSourcePath = TestHelper.GetTestFilePath(@"WaveModelTest\Waves");

                string modelPath = tempDir.CopyDirectoryToTempDirectory(modelSourcePath);
                string mdwPath = Path.Combine(modelPath, "input", mdwFileName);
                string outputDir = Path.Combine(modelPath, "output");
                var outputDirInfo = new DirectoryInfo(outputDir);

                FileCompareInfo[] origFileData = CollectFileInformation(outputDirInfo).ToArray();

                using (var model = new WaveModel(mdwPath))
                {
                    // Mimic DeltaShell behaviour to first switch to the model before saving.
                    // This is necessary because the save to logic relies on having the correct
                    // previous save directory set, which is set as part of Switch to. 
                    // This is far from ideal, however changing this would require significant 
                    // changes to DeltaShell's save logic.
                    ((IFileBased) model).SwitchTo(modelPath);

                    string originalDataSourcePath = model.WaveOutputData.DataSourcePath;

                    // When 
                    model.ModelSaveTo(mdwPath, true);

                    // Then
                    FileCompareInfo[] saveFileData = CollectFileInformation(outputDirInfo).ToArray();

                    AssertContainsSameFiles(origFileData, saveFileData);
                    Assert.That(outputDirInfo.EnumerateDirectories(), Is.Empty);

                    Assert.That(model.WaveOutputData.IsConnected, Is.True);
                    Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(originalDataSourcePath));
                }
            }
        }

        [Test]
        public void GivenAValidModelWithOutputDataConnectedToTheWorkingDirectoryButFilesRemoved_WhenTheModelIsSavedToTheSameLocation_ThenNothingIsCopied()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                const string mdwFileName = "Waves.mdw";
                string modelSourcePath = TestHelper.GetTestFilePath(@"WaveModelTest\Waves");

                string modelPath = tempDir.CopyDirectoryToTempDirectory(modelSourcePath);
                string mdwPath = Path.Combine(modelPath, "input", mdwFileName);
                string outputDir = Path.Combine(modelPath, "output");

                string alternativeOutputSourcePath = TestHelper.GetTestFilePath(@"WaveModelTest\alternative_output");
                string alternativeOutputPath = tempDir.CopyDirectoryToTempDirectory(alternativeOutputSourcePath);

                using (var model = new WaveModel(mdwPath))
                {
                    // Mimic DeltaShell behaviour to first switch to the model before saving.
                    // This is necessary because the save to logic relies on having the correct
                    // previous save directory set, which is set as part of Switch to. 
                    // This is far from ideal, however changing this would require significant 
                    // changes to DeltaShell's save logic.
                    ((IFileBased) model).SwitchTo(modelPath);

                    // Connect to different data source
                    model.WaveOutputData.ConnectTo(alternativeOutputPath, true);
                    // Remove the files on disk
                    FileUtils.DeleteIfExists(alternativeOutputPath);

                    // When 
                    model.ModelSaveTo(mdwPath, true);

                    // Then
                    var outputDirInfo = new DirectoryInfo(outputDir);
                    Assert.That(outputDirInfo.EnumerateFiles(), Is.Empty);
                    Assert.That(outputDirInfo.EnumerateDirectories(), Is.Empty);

                    Assert.That(model.WaveOutputData.IsConnected, Is.True);
                    Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(outputDirInfo.FullName));
                }
            }
        }

        [Test]
        public void GivenAValidModelWithOutputDataConnectedToTheWorkingDirectoryButFilesRemoved_WhenTheModelIsSavedToADifferentLocation_ThenNothingIsCopied()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                const string mdwFileName = "Waves.mdw";
                string testDataPath = TestHelper.GetTestFilePath(@"WaveModelTest\Waves");

                string modelPath = tempDir.CopyDirectoryToTempDirectory(testDataPath);
                string mdwPath = Path.Combine(modelPath, "input", mdwFileName);

                tempDir.CreateDirectory("AnotherWaves");
                string goalInputDir = tempDir.CreateDirectory("AnotherWaves\\input");
                string goalOutputDir = tempDir.CreateDirectory("AnotherWaves\\output");

                string goalMdwPath = Path.Combine(goalInputDir, mdwFileName);

                string alternativeOutputSourcePath = TestHelper.GetTestFilePath(@"WaveModelTest\alternative_output");
                string alternativeOutputPath = tempDir.CopyDirectoryToTempDirectory(alternativeOutputSourcePath);

                using (var model = new WaveModel(mdwPath))
                {
                    // Mimic DeltaShell behaviour to first switch to the model before saving.
                    // This is necessary because the save to logic relies on having the correct
                    // previous save directory set, which is set as part of Switch to. 
                    // This is far from ideal, however changing this would require significant 
                    // changes to DeltaShell's save logic.
                    ((IFileBased) model).SwitchTo(modelPath);

                    // Connect to different data source
                    model.WaveOutputData.ConnectTo(alternativeOutputPath, true);

                    // Remove the files on disk
                    FileUtils.DeleteIfExists(alternativeOutputPath);

                    // When 
                    model.ModelSaveTo(goalMdwPath, true);

                    // Then
                    var outputDirInfo = new DirectoryInfo(goalOutputDir);
                    Assert.That(outputDirInfo.EnumerateFiles(), Is.Empty);
                    Assert.That(outputDirInfo.EnumerateDirectories(), Is.Empty);

                    Assert.That(model.WaveOutputData.IsConnected, Is.True);
                    Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(outputDirInfo.FullName));
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAValidModelWithDataAvailableInTheModelOutputPathButTheFolderIsRemoved_WhenTheModelIsSavedAtInADifferentLocation_ThenTheOutputFolderContainsTheSameData()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                const string mdwFileName = "Waves.mdw";
                string modelSourcePath = TestHelper.GetTestFilePath(@"WaveModelTest\Waves");

                string modelPath = tempDir.CopyDirectoryToTempDirectory(modelSourcePath);
                string mdwPath = Path.Combine(modelPath, "input", mdwFileName);
                string outputDir = Path.Combine(modelPath, "output");

                tempDir.CreateDirectory("AnotherWaves");
                string goalInputDir = tempDir.CreateDirectory("AnotherWaves\\input");
                string goalOutputDir = tempDir.CreateDirectory("AnotherWaves\\output");

                string goalMdwPath = Path.Combine(goalInputDir, mdwFileName);

                using (var model = new WaveModel(mdwPath))
                {
                    // Mimic DeltaShell behaviour to first switch to the model before saving.
                    // This is necessary because the save to logic relies on having the correct
                    // previous save directory set, which is set as part of Switch to. 
                    // This is far from ideal, however changing this would require significant 
                    // changes to DeltaShell's save logic.
                    ((IFileBased) model).SwitchTo(modelPath);
                    // Remove the files on disk
                    FileUtils.DeleteIfExists(outputDir);

                    // When 
                    model.ModelSaveTo(goalMdwPath, true);

                    // Then
                    var goalOutputDirInfo = new DirectoryInfo(goalOutputDir);
                    Assert.That(goalOutputDirInfo.EnumerateFiles(), Is.Empty);
                    Assert.That(goalOutputDirInfo.EnumerateDirectories(), Is.Empty);

                    Assert.That(model.WaveOutputData.IsConnected, Is.True);
                    Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(goalOutputDirInfo.FullName));
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAValidModelWithDataAvailableInTheModelOutputPathButTheFolderIsRemoved_WhenTheModelIsSavedAtTheSameLocation_ThenTheOutputFolderContainsTheSameData()
        {
            // Given
            using (var tempDir = new TemporaryDirectory())
            {
                const string mdwFileName = "Waves.mdw";
                string modelSourcePath = TestHelper.GetTestFilePath(@"WaveModelTest\Waves");

                string modelPath = tempDir.CopyDirectoryToTempDirectory(modelSourcePath);
                string mdwPath = Path.Combine(modelPath, "input", mdwFileName);
                string outputDir = Path.Combine(modelPath, "output");
                var outputDirInfo = new DirectoryInfo(outputDir);

                using (var model = new WaveModel(mdwPath))
                {
                    // Mimic DeltaShell behaviour to first switch to the model before saving.
                    // This is necessary because the save to logic relies on having the correct
                    // previous save directory set, which is set as part of Switch to. 
                    // This is far from ideal, however changing this would require significant 
                    // changes to DeltaShell's save logic.
                    ((IFileBased) model).SwitchTo(modelPath);
                    // Remove the files on disk
                    FileUtils.DeleteIfExists(outputDir);

                    string originalDataSourcePath = model.WaveOutputData.DataSourcePath;

                    // When 
                    model.ModelSaveTo(mdwPath, true);

                    // Then
                    Assert.That(outputDirInfo.EnumerateFiles(), Is.Empty);
                    Assert.That(outputDirInfo.EnumerateDirectories(), Is.Empty);

                    Assert.That(model.WaveOutputData.IsConnected, Is.True);
                    Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(originalDataSourcePath));
                }
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WaveOutputData_EventsAreProperlyPropagated()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                var observer = new NotifyPropertyChangedTestObserver();
                ((INotifyPropertyChange) model).PropertyChanged += observer.OnPropertyChanged;

                // Call
                model.WaveOutputData.ConnectTo(tempDir.Path, true);

                // Assert
                Assert.That(observer.NCalls, Is.EqualTo(5));
                Assert.That(observer.Senders, Has.All.EqualTo(model.WaveOutputData));
            }
        }


        private static void AssertContainsSameFiles(IReadOnlyList<FileCompareInfo> originalFileData,
                                                    IReadOnlyList<FileCompareInfo> savedFileData)
        {
            Assert.That(savedFileData.Count, Is.EqualTo(originalFileData.Count));

            for (var i = 0; i < savedFileData.Count; i++)
            {
                Assert.That(savedFileData[i].Name, Is.EqualTo(originalFileData[i].Name));
                Assert.That(savedFileData[i].Hash, Is.EqualTo(originalFileData[i].Hash));
            }
        }

        private class FileCompareInfo
        {
            public FileCompareInfo(string name, string hash)
            {
                Name = name;
                Hash = hash;
            }

            public string Name { get; }
            public string Hash { get; }
        }

        private static IEnumerable<FileCompareInfo> CollectFileInformation(DirectoryInfo directoryInfo) =>
            directoryInfo.EnumerateFiles().Select(fi => new FileCompareInfo(fi.Name, FileUtils.GetChecksum(fi.FullName)));
    }
}