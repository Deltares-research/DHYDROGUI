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
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
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

                ((INotifyCollectionChanged) model).CollectionChanged += delegate { counter = 1; };

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
        public void TestUpdatingGridFileIsSyncedWithDataItemTag()
        {
            //const string originalName = "Outer";
            //const string newName = "Blarg";

            //var waveModel = new WaveModel {OuterDomain = new WaveDomainData(originalName)};

            //var dataItem = waveModel.GetDataItemByTag(WaveModel.WavmStoreDataItemTag + originalName) as DataItem;
            //Assert.NotNull(dataItem);

            //// Simulate setting a new Grid Filepath
            //waveModel.OuterDomain.GridFileName = newName;

            //Assert.AreEqual(WaveModel.WavmStoreDataItemTag + newName, dataItem.Tag);
            Assert.Fail("Needs to be reimplemented as part of the D-Waves output epic D3DFMIQ-2272");
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
        [Category(TestCategory.Jira)] // D3DFMIQ-2272
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
                Assert.IsFalse(waveModel.OutputIsEmpty);
                Assert.AreEqual(Path.Combine(outputDirectoryInTemp, "wavm-Waves.nc"),
                                waveModel.WavmFunctionStores.First().Path);

                IDataItem swanLogDataItem = waveModel.AllDataItems.Single(di => di.Tag == "SwanLogDataItemTag");
                Assert.AreEqual(File.ReadAllText(Path.Combine(outputDirectoryInTemp, "swn-diag.Waves")),
                                ((TextDocument) swanLogDataItem.Value).Content);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Jira)] // D3DFMIQ-2272
        public void ConnectOutput_WhenOutputWAVMFileIsMissing_ShouldGiveLogWarningToUser()
        {
            // Arrange
            using (var waveModel = new WaveModel())
            {
                string outputDirectory =
                    Path.Combine(TestHelper.GetTestDataDirectory(), "output_wavm", "NotExisting");

                // Act and Assert
                var expectedMssg =
                    $"Could not find output (WAVM) file: {Path.Combine(outputDirectory, "wavm-Waves.nc")}";
                TestHelper.AssertAtLeastOneLogMessagesContains(() => waveModel.ConnectOutput(outputDirectory),
                                                               expectedMssg);
                Assert.IsTrue(waveModel.OutputIsEmpty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Jira)] // D3DFMIQ-2272
        public void ConnectOutput_WhenModelHasMultipleDomains_ShouldConnectWavmFileAndReadSwanDiagFile()
        {
            //using (var tempDirectory = new TemporaryDirectory())
            //using (var waveModel = new WaveModel())
            //{
            //    // Arrange
            //    waveModel.AddSubDomain(waveModel.OuterDomain, new WaveDomainData("Inner"));

            //    string outputDirectory =
            //        Path.Combine(TestHelper.GetTestDataDirectory(), "output_wavm", "Output2Domains");
            //    string outputDirectoryInTemp = tempDirectory.CopyDirectoryToTempDirectory(outputDirectory);

            //    // Act
            //    waveModel.ConnectOutput(outputDirectoryInTemp);

            //    // Assert
            //    Assert.IsFalse(waveModel.OutputIsEmpty);
            //    IEnumerable<WavmFileFunctionStore> functionStores = waveModel.WavmFunctionStores.ToList();
            //    Assert.AreEqual(2, functionStores.Count());
            //    Assert.AreEqual(Path.Combine(outputDirectoryInTemp, "wavm-Waves-Outer.nc"),
            //                    functionStores.First().Path);
            //    Assert.AreEqual(Path.Combine(outputDirectoryInTemp, "wavm-Waves-Inner.nc"),
            //                    functionStores.Last().Path);

            //    IDataItem swanLogDataItem = waveModel.AllDataItems.Single(di => di.Tag == WaveModel.SwanLogDataItemTag);
            //    Assert.AreEqual(File.ReadAllText(Path.Combine(outputDirectoryInTemp, "swn-diag.Waves")),
            //                    ((TextDocument) swanLogDataItem.Value).Content);
            //}
            Assert.Fail("Should be improved as part of D3DFMIQ-2286");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Jira)] // D3DFMIQ-2272
        public void ConnectOutput_WhenModelHasMultipleDomainsAndOutputWAVMFilesAreMissing_ShouldGiveLogWarningToUser()
        {
            // Arrange
            using (var waveModel = new WaveModel())
            {
                waveModel.AddSubDomain(waveModel.OuterDomain, new WaveDomainData("Inner"));

                string outputDirectory =
                    Path.Combine(TestHelper.GetTestDataDirectory(), "output_wavm", "NotExisting");

                // Act
                IEnumerable<string> messages = TestHelper
                                               .GetAllRenderedMessages(() => waveModel.ConnectOutput(outputDirectory))
                                               .ToList();

                // Assert
                var expectedMssg =
                    $"Could not find output (WAVM) file: {Path.Combine(outputDirectory, "wavm-Waves-Outer.nc")}";
                var expectedMssg2 =
                    $"Could not find output (WAVM) file: {Path.Combine(outputDirectory, "wavm-Waves-Inner.nc")}";

                Assert.IsTrue(messages.Contains(expectedMssg));
                Assert.IsTrue(messages.Contains(expectedMssg2));
                Assert.IsTrue(waveModel.OutputIsEmpty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Jira)] // D3DFMIQ-2272
        public void ConnectOutput_WhenModelHasMultipleDomainsAndOneOfTheOutputWAVMFilesIsMissing_ShouldGiveLogWarningToUserAndConnectTheExisting()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var waveModel = new WaveModel())
            {
                // Arrange
                waveModel.AddSubDomain(waveModel.OuterDomain, new WaveDomainData("Inner"));

                string wavmOuterFilePath =
                    Path.Combine(TestHelper.GetTestDataDirectory(), "output_wavm", "Output2Domains", "wavm-Waves-Outer.nc");
                string swanDiagFilePath =
                    Path.Combine(TestHelper.GetTestDataDirectory(), "output_wavm", "Output2Domains", "swn-diag.Waves");

                // Don't copy inner output file
                string wavmOuterFilePathInTemp = tempDirectory.CopyTestDataFileToTempDirectory(wavmOuterFilePath);
                tempDirectory.CopyTestDataFileToTempDirectory(swanDiagFilePath);
                string outputDirectoryInTemp = Path.GetDirectoryName(wavmOuterFilePathInTemp);

                // Act
                List<string> messages = TestHelper
                                        .GetAllRenderedMessages(
                                            () => waveModel.ConnectOutput(outputDirectoryInTemp))
                                        .ToList();

                // Assert
                var expectedMssg =
                    $"Could not find output (WAVM) file: {Path.Combine(outputDirectoryInTemp, "wavm-Waves-Inner.nc")}";

                Assert.AreEqual(1, messages.Count());
                Assert.IsTrue(messages.Contains(expectedMssg));
                Assert.IsFalse(waveModel.OutputIsEmpty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Jira)] // D3DFMIQ-2272
        public void ConnectOutput_WhenSwanFileMissing_ShouldGiveLogWarningToUser()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var waveModel = new WaveModel {Name = "wave"})
            {
                // Arrange
                string outputDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "output_wavm");
                string outputDirectoryInTemp = tempDirectory.CopyDirectoryToTempDirectory(outputDirectory);

                // Act and Assert
                var expectedMssg =
                    $"Could not find log file: {Path.Combine(outputDirectoryInTemp, "swn-diag.wave")}";
                TestHelper.AssertLogMessageIsGenerated(() => waveModel.ConnectOutput(outputDirectoryInTemp), expectedMssg);
                Assert.IsFalse(waveModel.OutputIsEmpty);
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
                model.WaveOutputData.ConnectTo(initialPath);

                // Call
                model.DisconnectOutput();

                // Assert
                Assert.That(model.WaveOutputData.IsConnected, Is.False);
                Assert.That(model.WaveOutputData.DataSourcePath, Is.Null);
            }
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void ConnectOutput_UpdatesWaveOutputDataCorrectly(bool withInitialPath)
        {
            using (var model = new WaveModel())
            {
                // Setup
                if (withInitialPath)
                {
                    const string initialPath = "some/toad/to/a/directory";
                    model.WaveOutputData.ConnectTo(initialPath);
                }

                const string newPath = "a/different/output/path";

                // Call
                model.ConnectOutput(newPath);

                // Assert
                Assert.That(model.WaveOutputData.IsConnected, Is.True);
                Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(newPath));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ModelSaveTo_SwitchToTrue_ConnectsWaveOutputData()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                tempDir.CreateDirectory("project_data");
                tempDir.CreateDirectory($"project_data\\{model.Name}");
                string inputFolder = tempDir.CreateDirectory($"project_data\\{model.Name}\\input");
                string outputFolder = tempDir.CreateDirectory($"project_data\\{model.Name}\\output");

                string mdwPath = Path.Combine(inputFolder, $"{model.Name}.mdw");

                // Call
                model.ModelSaveTo(mdwPath, true);

                // Assert
                Assert.That(model.WaveOutputData.IsConnected, Is.True);
                Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(outputFolder));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ModelSaveTo_SwitchToFalse_NotConnected_DoesNotChangeTheWaveOutputData()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                string mdwPath = Path.Combine(tempDir.Path, $"{model.Name}.mdw");

                // Call
                model.ModelSaveTo(mdwPath, false);

                // Assert
                Assert.That(model.WaveOutputData.IsConnected, Is.False);
                Assert.That(model.WaveOutputData.DataSourcePath, Is.Null);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ModelSaveTo_SwitchToFalse_Connected_DoesNotChangeTheWaveOutputData()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaveModel())
            {
                string outputFolder = tempDir.CreateDirectory("someUnrelatedFolder");
                model.WaveOutputData.ConnectTo(outputFolder);

                string mdwPath = Path.Combine(tempDir.Path, $"{model.Name}.mdw");

                // Call
                model.ModelSaveTo(mdwPath, false);

                // Assert
                Assert.That(model.WaveOutputData.IsConnected, Is.True);
                Assert.That(model.WaveOutputData.DataSourcePath, Is.EqualTo(outputFolder));
            }
        }
    }
}