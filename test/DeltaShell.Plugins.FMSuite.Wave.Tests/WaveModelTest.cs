using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.CommonTools.TextData;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
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
        public void Constructor_SetsCorrectTimeFrameData()
        {
            // Call
            using (var model = new WaveModel())
            {
                // Assert
                Assert.That(model.TimeFrameData, Is.Not.Null);
            }
        }

        [Test]
        public void Constructor_SetsCorrectFeatureContainer()
        {
            // Call
            using (var model = new WaveModel())
            {
                // Assert
                Assert.That(model.FeatureContainer, Is.Not.Null);
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

                ((INotifyCollectionChanged)model).CollectionChanged += delegate { counter += 1; };

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
            using (var model = new WaveModel { WorkingDirectoryPathFunc = func })
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
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void DimrExportDirectoryPath_ShouldAlwaysBeUpToDate()
        {
            // Setup
            var provider = new PathProvider { Path = "orig_working_dir" };

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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TransformModelCoordinateSystemUpdatesGridCoordinateSystem()
        {
            const int wgs84CS = 4326;
            const int rd = 28992;

            string localMdwPath = WaveTestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"flow_coupled/wave.mdw"));
            var waveModel = new WaveModel(localMdwPath) { CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(wgs84CS) };

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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TransformModelCoordinateSystem()
        {
            const int nad27_utm16N = 26916;
            const int pseudo_webm = 3857;

            string localMdwPath =
                WaveTestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw"));
            var waveModel = new WaveModel(localMdwPath) { CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(nad27_utm16N) };

            ICoordinateSystem src = new OgrCoordinateSystemFactory().CreateFromEPSG(nad27_utm16N);
            ICoordinateSystem target = new OgrCoordinateSystemFactory().CreateFromEPSG(pseudo_webm);
            ICoordinateTransformation fromUTM16ToWebMercator = new OgrCoordinateSystemFactory().CreateTransformation(src, target);
            ICoordinateTransformation fromWebMercatorToUTM16 = new OgrCoordinateSystemFactory().CreateTransformation(target, src);

            List<Coordinate> coordinates = waveModel.FeatureContainer.GetAllFeatures()
                                                    .SelectMany(f => f.Geometry.Coordinates)
                                                    .ToList();

            waveModel.TransformCoordinates(fromUTM16ToWebMercator);
            waveModel.TransformCoordinates(fromWebMercatorToUTM16);

            List<Coordinate> coordinatesAfter =
                waveModel.FeatureContainer.GetAllFeatures()
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
        public void IsCoupledToFlow_ShouldAlwaysBeFalseForAStandAloneModel()
        {
            var waveModel = new WaveModel();
            Assert.IsFalse(waveModel.IsCoupledToFlow);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
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
                IEnumerable<IWavmFileFunctionStore> functionStores = waveModel.WaveOutputData.WavmFileFunctionStores.ToList();
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

        [Test]
        public void GetDirectChildren_ContainsDiagnosticFiles()
        {
            // Setup
            var diagFile1 = new ReadOnlyTextFileData("", "", ReadOnlyTextFileDataType.Default);
            var diagFile2 = new ReadOnlyTextFileData("", "", ReadOnlyTextFileDataType.Default);
            using (var model = new WaveModel())
            {

                model.WaveOutputData.DiagnosticFiles.Add(diagFile1);
                model.WaveOutputData.DiagnosticFiles.Add(diagFile2);

                // Call
                IEnumerable<object> result = model.GetDirectChildren()
                                                  .ToList();

                Assert.That(result, Has.Member(diagFile1));
                Assert.That(result, Has.Member(diagFile2));
            }
        }

        [Test]
        public void GetDirectChildren_ContainsSpectraFiles()
        {
            // Setup
            var spectraFile1 = new ReadOnlyTextFileData("", "", ReadOnlyTextFileDataType.Default);
            var spectraFile2 = new ReadOnlyTextFileData("", "", ReadOnlyTextFileDataType.Default);
            using (var model = new WaveModel())
            {

                model.WaveOutputData.SpectraFiles.Add(spectraFile1);
                model.WaveOutputData.SpectraFiles.Add(spectraFile2);

                // Call
                IEnumerable<object> result = model.GetDirectChildren()
                                                  .ToList();

                Assert.That(result, Has.Member(spectraFile1));
                Assert.That(result, Has.Member(spectraFile2));
            }
        }
        
        [Test]
        public void GetDirectChildren_ContainsSwanFiles()
        {
            // Setup
            var swanFile1 = new ReadOnlyTextFileData("", "", ReadOnlyTextFileDataType.Default);
            var swanFile2 = new ReadOnlyTextFileData("", "", ReadOnlyTextFileDataType.Default);
            using (var model = new WaveModel())
            {

                model.WaveOutputData.SwanFiles.Add(swanFile1);
                model.WaveOutputData.SwanFiles.Add(swanFile2);

                // Call
                IEnumerable<object> result = model.GetDirectChildren()
                                                  .ToList();

                Assert.That(result, Has.Member(swanFile1));
                Assert.That(result, Has.Member(swanFile2));
            }
        }

        [Test]
        public void GetDirectChildren_ContainsWavmFileFunctionStores()
        {
            // Setup
            var functionStore1 = Substitute.For<IWavmFileFunctionStore>();
            var functionStore2 = Substitute.For<IWavmFileFunctionStore>();
            using (var model = new WaveModel())
            {

                model.WaveOutputData.WavmFileFunctionStores.Add(functionStore1);
                model.WaveOutputData.WavmFileFunctionStores.Add(functionStore2);

                // Call
                IEnumerable<object> result = model.GetDirectChildren()
                                                  .ToList();

                Assert.That(result, Has.Member(functionStore1));
                Assert.That(result, Has.Member(functionStore2));
            }
        }

        [Test]
        public void GetDirectChildren_ContainsWavhFileFunctionStores()
        {
            // Setup
            using (var model = new WaveModel())
            {
                var functionStore1 = Substitute.For<IWavhFileFunctionStore>();
                functionStore1.Functions = new EventedList<IFunction>(new[]
                {
                    Substitute.For<IFunction>()
                });
                var functionStore2 = Substitute.For<IWavhFileFunctionStore>();
                functionStore2.Functions = new EventedList<IFunction>(new[]
                {
                    Substitute.For<IFunction>(),
                    Substitute.For<IFunction>()
                });
                model.WaveOutputData.WavhFileFunctionStores.Add(functionStore1);
                model.WaveOutputData.WavhFileFunctionStores.Add(functionStore2);

                List<IFunction> functions = model.WaveOutputData.WavhFileFunctionStores.SelectMany(s => s.Functions).ToList();

                // Precondition
                Assert.That(functions, Has.Count.EqualTo(3));

                // Call
                IEnumerable<object> result = model.GetDirectChildren()
                                                  .ToList();

                Assert.That(result, Has.Member(functionStore1));
                Assert.That(result, Has.Member(functionStore2));

                foreach (IFunction function in functions)
                {
                    Assert.That(result, Has.Member(function));
                }
            }
        }

        [Test]
        public void GetDirectChildren_ContainsTimeFrameData()
        {
            // Setup 
            using (var model = new WaveModel())
            {
                // Call
                object[] result = model.GetDirectChildren().ToArray();

                // Assert
                Assert.That(result, Has.Member(model.TimeFrameData));
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
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
        [NUnit.Framework.Category(TestCategory.Integration)]
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
                    ((IFileBased)model).SwitchTo(modelPath);

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
        [NUnit.Framework.Category(TestCategory.Integration)]
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
                    ((IFileBased)model).SwitchTo(modelPath);

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
        [NUnit.Framework.Category(TestCategory.Integration)]
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
                    ((IFileBased)model).SwitchTo(modelPath);

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
        [NUnit.Framework.Category(TestCategory.Integration)]
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
                    ((IFileBased)model).SwitchTo(modelPath);

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
        [NUnit.Framework.Category(TestCategory.Integration)]
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
                    ((IFileBased)model).SwitchTo(modelPath);

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
        [NUnit.Framework.Category(TestCategory.Integration)]
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
                    ((IFileBased)model).SwitchTo(modelPath);

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
                    ((IFileBased)model).SwitchTo(modelPath);

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
                    ((IFileBased)model).SwitchTo(modelPath);

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
        [NUnit.Framework.Category(TestCategory.Integration)]
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
                    ((IFileBased)model).SwitchTo(modelPath);
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
        [NUnit.Framework.Category(TestCategory.Integration)]
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
                    ((IFileBased)model).SwitchTo(modelPath);
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void WaveOutputData_EventsAreProperlyPropagated()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                string modelOutputPath = TestHelper.GetTestFilePath(@"WaveModelTest\alternative_output");
                string connectDirectoryPath = tempDir.CopyDirectoryToTempDirectory(modelOutputPath);

                var observer = new EventTestObserver<PropertyChangedEventArgs>();
                ((INotifyPropertyChange)model).PropertyChanged += observer.OnEventFired;

                // Call
                model.WaveOutputData.ConnectTo(connectDirectoryPath, true);

                // Assert
                Assert.That(observer.NCalls, Is.EqualTo(4));
                Assert.That(observer.Senders, Has.All.EqualTo(model.WaveOutputData));
            }
        }

        public static IEnumerable<TestCaseData> GetTimeFramePropertyChangedData()
        {
            object Identity(ITimeFrameData data) => data;
            void UpdateHydrodynamicsInputDataType(ITimeFrameData data) => data.HydrodynamicsInputDataType = HydrodynamicsInputDataType.TimeVarying;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateHydrodynamicsInputDataType,
                                          (Func<ITimeFrameData, object>)Identity,
                                          nameof(ITimeFrameData.HydrodynamicsInputDataType));
            void UpdateWindInputDataType(ITimeFrameData data) => data.WindInputDataType = WindInputDataType.TimeVarying;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateWindInputDataType,
                                          (Func<ITimeFrameData, object>)Identity,
                                          nameof(ITimeFrameData.WindInputDataType));

            object Hydrodynamics(ITimeFrameData data) => data.HydrodynamicsConstantData;
            void UpdateWaterLevel(ITimeFrameData data) => data.HydrodynamicsConstantData.WaterLevel = 10.0;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateWaterLevel,
                                          (Func<ITimeFrameData, object>)Hydrodynamics,
                                          nameof(HydrodynamicsConstantData.WaterLevel));
            void UpdateVelocityX(ITimeFrameData data) => data.HydrodynamicsConstantData.VelocityX = 10.0;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateVelocityX,
                                          (Func<ITimeFrameData, object>)Hydrodynamics,
                                          nameof(HydrodynamicsConstantData.VelocityX));
            void UpdateVelocityY(ITimeFrameData data) => data.HydrodynamicsConstantData.VelocityY = 10.0;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateVelocityY,
                                          (Func<ITimeFrameData, object>)Hydrodynamics,
                                          nameof(HydrodynamicsConstantData.VelocityY));

            object Wind(ITimeFrameData data) => data.WindConstantData;
            void UpdateSpeed(ITimeFrameData data) => data.WindConstantData.Speed = 10.0;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateSpeed,
                                          (Func<ITimeFrameData, object>)Wind,
                                          nameof(WindConstantData.Speed));
            void UpdateDirection(ITimeFrameData data) => data.WindConstantData.Direction = 10.0;
            yield return new TestCaseData((Action<ITimeFrameData>)UpdateDirection,
                                          (Func<ITimeFrameData, object>)Wind,
                                          nameof(WindConstantData.Direction));
        }

        [Test]
        [TestCaseSource(nameof(GetTimeFramePropertyChangedData))]
        public void TimeFrameData_EventsAreProperlyPropagated(Action<ITimeFrameData> updateProperty,
                                                              Func<ITimeFrameData, object> getSender,
                                                              string expectedPropertyName)
        {
            using (var model = new WaveModel())
            {
                var observer = new EventTestObserver<PropertyChangedEventArgs>();

                ((INotifyPropertyChanged)model).PropertyChanged += observer.OnEventFired;

                // Call
                updateProperty(model.TimeFrameData);

                // Assert
                Assert.That(observer.NCalls, Is.EqualTo(1));
                Assert.That(observer.Senders.First(), Is.SameAs(getSender(model.TimeFrameData)));

                PropertyChangedEventArgs args = observer.EventArgses.First();
                Assert.That(args.PropertyName, Is.EqualTo(expectedPropertyName));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenAValidModelWithDataAvailableInTheWorkingDirectoryWithADifferentMdwName_WhenTheModelIsSaved_ThenTheOutputFolderContainsTheDataFromTheWorkingLocation()
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

                // Rename the output name to something other than "Waves.mdw"
                File.Move(Path.Combine(alternativeOutputPath, "Waves.mdw"), Path.Combine(alternativeOutputPath, "5a.mdw"));

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
                    ((IFileBased)model).SwitchTo(modelPath);

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
        [TestCase("Waves", "Waves.mdw")]
        [TestCase("Potato", "Potato.mdw")]
        [TestCase("", "Waves.mdw")]
        [TestCase(null, "Waves.mdw")]

        public void InputFile_MatchesModelName(string modelName, string expectedInputFile)
        {
            // Setup
            using (var model = new WaveModel())
            {
                model.Name = modelName;

                // Call
                string result = model.InputFile;

                // Assert
                Assert.That(result, Is.EqualTo(expectedInputFile));
            }
        }

        [Test]
        [TestCase("Waves", "path/to/workDir", "Waves.mdw")]
        [TestCase("Potato", "path/to/workDir", "Potato.mdw")]
        [TestCase("", "path/to/workDir", "Waves.mdw")]
        [TestCase(null, "path/to/workDir", "Waves.mdw")]
        public void GetExporterPath_ReturnsExpectedPath(string modelName,
                                                        string dirPath,
                                                        string expectedInputFile)
        {
            // Setup
            using (var model = new WaveModel())
            {
                model.Name = modelName;

                // Call
                string result = model.GetExporterPath(dirPath);

                // Assert
                string expectedExporterPath = Path.Combine(dirPath, expectedInputFile);
                Assert.That(result, Is.EqualTo(expectedExporterPath));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenASavedModel_WhenTheModelIsRenamedAndSaved_ThenTheModelDirectoryIsUpdatedCorrectly()
        {
            // Setup
            const string persistPath = "$data$WaveModel-47e9a5e9-fe4c-4528-966e-6a7b8fd97082";
            const string relativePath = "WaveModelTest\\Waves";
            string testPath = TestHelper.GetTestFilePath(relativePath);

            using (var tempDir = new TemporaryDirectory())
            {
                string modelDirectory = tempDir.CopyDirectoryToTempDirectory(testPath);
                string mdwPath = Path.Combine(modelDirectory, "input", "Waves.mdw");
                DirectoryInfo initialInputDirectoryInfo =
                    new DirectoryInfo(mdwPath).Parent;
                var initialOutputDirectoryInfo =
                    new DirectoryInfo(Path.Combine(initialInputDirectoryInfo.Parent.FullName, "output"));

                IReadOnlyList<FileCompareInfo> referenceFiles =
                    CollectFileInformation(initialInputDirectoryInfo)
                        .Where(x => !x.Name.EndsWith(".mdw"))
                        .Concat(CollectFileInformation(initialOutputDirectoryInfo))
                        .ToList();
                const string newName = "NotWaves";

                using (var model = new WaveModel(mdwPath))
                {
                    ((IFileBased)model).Open(mdwPath);

                    model.Name = newName;

                    // Call
                    // Trigger save: This mimicks the behaviour of a BeforePersist call, which passes a
                    // string containing a hash to the path.
                    ((IFileBased)model).Path = persistPath;

                    // Assert
                    var actualInputDirectoryInfo =
                        new DirectoryInfo(Path.Combine(tempDir.Path, newName, "input"));
                    var actualOutputDirectoryInfo =
                        new DirectoryInfo(Path.Combine(tempDir.Path, newName, "output"));

                    IReadOnlyList<FileCompareInfo> actualFiles =
                        CollectFileInformation(actualInputDirectoryInfo)
                            .Where(x => !x.Name.EndsWith(".mdw"))
                            .Concat(CollectFileInformation(actualOutputDirectoryInfo))
                            .ToList();

                    AssertContainsSameFiles(referenceFiles, actualFiles);
                    Assert.That(File.Exists(Path.Combine(actualInputDirectoryInfo.FullName, (newName + ".mdw"))),
                                Is.True);
                }
            }
        }

        [Test]
        public void FileExceptionsCleaningWorkingDirectory_ShouldAlwaysReturnEmptyCollection()
        {
            using (var model = new WaveModel())
            {
                Assert.That(model.IgnoredFilePathsWhenCleaningWorkingDirectory, Is.Empty);
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