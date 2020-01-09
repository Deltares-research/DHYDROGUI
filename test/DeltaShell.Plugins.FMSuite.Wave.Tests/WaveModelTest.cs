using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Wave.IO.Importers;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.Properties;
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
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadTestModelFromFile()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw");
            Assert.IsTrue(File.Exists(mdwPath));

            var waveModel = new WaveModel(mdwPath);

            var grid = waveModel.OuterDomain.Grid;
            Assert.IsTrue(grid != null);
            Assert.AreEqual(121, grid.Arguments[0].Values.Count); // N or y-direction
            Assert.AreEqual(236, grid.Arguments[1].Values.Count); // M or x-direction
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTestModelWithFlowCouplingFromFile()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"flow_coupled/wave.mdw");
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
            
            var localMdwPath = WaveTestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"flow_coupled/wave.mdw"));
            var waveModel = new WaveModel(localMdwPath)
            {
                CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(wgs84CS)
            };
            
            var src = new OgrCoordinateSystemFactory().CreateFromEPSG(wgs84CS);
            var target = new OgrCoordinateSystemFactory().CreateFromEPSG(rd);
            var transformation = new OgrCoordinateSystemFactory().CreateTransformation(src, target);

            var grid = waveModel.OuterDomain.Grid;
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

            var localMdwPath =
                WaveTestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw"));
            var waveModel = new WaveModel(localMdwPath)
            {
                CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(nad27_utm16N)
            };

            var src = new OgrCoordinateSystemFactory().CreateFromEPSG(nad27_utm16N);
            var target = new OgrCoordinateSystemFactory().CreateFromEPSG(pseudo_webm);
            var fromUTM16ToWebMercator = new OgrCoordinateSystemFactory().CreateTransformation(src, target);
            var fromWebMercatorToUTM16 = new OgrCoordinateSystemFactory().CreateTransformation(target, src);

            var coordinates =
                WaveModelCoordinateConversion.GetAllModelFeatures(waveModel)
                    .SelectMany(f => f.Geometry.Coordinates)
                    .ToList();

            waveModel.TransformCoordinates(fromUTM16ToWebMercator);
            waveModel.TransformCoordinates(fromWebMercatorToUTM16);

            var coordinatesAfter =
                WaveModelCoordinateConversion.GetAllModelFeatures(waveModel)
                    .SelectMany(f => f.Geometry.Coordinates)
                    .ToList();

            Assert.AreEqual(coordinatesAfter.Count, coordinates.Count);
            for (int i = 0; i < coordinates.Count; ++i)
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
            var expectedMssg = Resources
                .WaveModel_WaveSetup_With_WaveSetup_set_to_True_parallel_runs_will_fail__normal_runs_with_lakes_will_produce_unreliable_values_;
            TestHelper.AssertAtLeastOneLogMessagesContains( () => waveModel.ModelDefinition.WaveSetup = true, expectedMssg);
            Assert.IsTrue(waveModel.ModelDefinition.WaveSetup);
        }

        [Test]
        public void GivenAWaveModel_WhenSettingBedfrictionToCollins_ThenTheBedfrictionCoefficientShouldAlsoBeChanged()
        {
            var waveModel = new WaveModel();
            
            var prop = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory,
                KnownWaveProperties.BedFriction);
            var prop2 = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory,
                KnownWaveProperties.BedFrictionCoef);

            Assert.AreEqual("0.038", prop2.GetValueAsString());
            prop.SetValueAsString("collins");
            Assert.AreEqual("0.015", prop2.GetValueAsString());
        }

        [Test]
        public void GivenAWaveModel_WhenSettingBedfrictionToMadsenetal_ThenTheBedfrictionCoefficientShouldAlsoBeChanged()
        {
            var waveModel = new WaveModel();

            var prop = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory,
                KnownWaveProperties.BedFriction);
            var prop2 = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory,
                KnownWaveProperties.BedFrictionCoef);

            Assert.AreEqual("0.038", prop2.GetValueAsString());
            prop.SetValueAsString("madsen et al.");
            Assert.AreEqual("0.05", prop2.GetValueAsString());
        }

        [Test]
        public void GivenAWaveModel_WhenSettingSimModeToNonStationary_ThenTheMaxIterationPropertyShouldAlsoBeChanged()
        {
            var waveModel = new WaveModel();

            var prop = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.GeneralCategory,
                KnownWaveProperties.SimulationMode);
            var prop2 = waveModel.ModelDefinition.GetModelProperty(KnownWaveCategories.NumericsCategory,
                KnownWaveProperties.MaxIter);

            Assert.AreEqual("50", prop2.GetValueAsString());
            prop.SetValueAsString("non-stationary");
            Assert.AreEqual("15", prop2.GetValueAsString());
        }

        [Test]
        public void
            GivenWaveModelWithOuterDomainWithCoordinateSystemSetWhenAddingInnerDomainThenInnerDomainShouldGetSameCoordinateSystem()
        {
            var waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_001.grd");
            var waveModel = new WaveModel();
            var tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[] { waveModel });
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
            var waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_001.grd");
            var waveModel = new WaveModel();
            var tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[] { waveModel });
                waveModel.OuterDomain.Grid.Attributes[CurvilinearGrid.CoordinateSystemKey] = "Test";
                waveGridFileImporter.ImportItem(waveGridFileFilePath, waveModel.OuterDomain.Grid);
                Assert.That(waveModel.OuterDomain.Grid.CoordinateSystem, Is.Not.Null);
                Assert.That(waveModel.CoordinateSystem, Is.Not.Null);
                var exterior = new WaveDomainData("exterior");
                var oldOuterDomain = waveModel.OuterDomain;
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
            var waveGridFileFilePath = TestHelper.GetTestFilePath(@"importers\Grid_001.grd");
            var waveModel = new WaveModel();
            var tempWorkingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                waveModel.MdwFile.MdwFilePath = tempWorkingDirectory;
                var waveGridFileImporter = new WaveGridFileImporter("my test", () => new[] {waveModel});
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
            var waveFilePath = TestHelper.GetTestFilePath(@"mdw_coordinates\spherical.mdw");
            var localFilePath = TestHelper.CreateLocalCopy(waveFilePath);
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
            var waveFilePath = TestHelper.GetTestFilePath(@"mdw_coordinates\cartesian.mdw");
            var localFilePath = TestHelper.CreateLocalCopy(waveFilePath);
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
        public void TestUpdatingGridFileIsSyncedWithDataItemTag()
        {
            const string originalName = "Outer";
            const string newName = "Blarg";

            var waveModel = new WaveModel
            {
                OuterDomain = new WaveDomainData(originalName)
            };

            var dataItem = waveModel.GetDataItemByTag(WaveModel.WavmStoreDataItemTag + originalName) as DataItem;
            Assert.NotNull(dataItem);

            // Simulate setting a new Grid Filepath
            waveModel.OuterDomain.GridFileName = newName;

            Assert.AreEqual(WaveModel.WavmStoreDataItemTag + newName, dataItem.Tag);
        }

        [Test]
        public void ClearOutput_WithSwanRunLogDataItem_ThenSwanRunLogContentIsEmpty()
        {
            // Setup
            var waveModel = new WaveModel();
            var swanTextDocument = (TextDocument) waveModel.GetDataItemByTag(WaveModel.SwanLogDataItemTag).Value;
            swanTextDocument.Content = new Random().Next(100).ToString();

            // Private field outputIsEmpty is set to false after a successful model run. This field should be false when clearing model output.
            // As we do not focus on model run, we use reflection to set this field and omit the model run.
            TypeUtils.SetField(waveModel, "outputIsEmpty", false);
            
            // Call
            waveModel.ClearOutput();

            // Assert
            Assert.That(swanTextDocument.Content, Is.Empty, "Swan run log should be empty after clearing model output.");
        }

        [Test]
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
            // Arrange
            var waveModel = new WaveModel();
            string outputDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "output_wavm", "Output1Domain");
            
            // Act
            waveModel.ConnectOutput(outputDirectory);

            // Assert
            Assert.IsFalse(waveModel.OutputIsEmpty);
            Assert.AreEqual(Path.Combine(outputDirectory, "wavm-Waves.nc"), waveModel.WavmFunctionStores.First().Path);
           
            IDataItem swanLogDataItem = waveModel.AllDataItems.Single(di => di.Tag == "SwanLogDataItemTag");
            Assert.AreEqual(File.ReadAllText(Path.Combine(outputDirectory, "swn-diag.Waves")),((TextDocument) swanLogDataItem.Value).Content);
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConnectOutput_WhenModelHasMultipleDomains_ShouldConnectWavmFileAndReadSwanDiagFile()
        {
            // Arrange
            var waveModel = new WaveModel();
            waveModel.AddSubDomain(waveModel.OuterDomain, new WaveDomainData("Inner"));
           
            string outputDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "output_wavm", "Output2Domains");

            // Act
            waveModel.ConnectOutput(outputDirectory);

            // Assert
            Assert.IsFalse(waveModel.OutputIsEmpty);
            Assert.AreEqual(2, waveModel.WavmFunctionStores.Count());
            Assert.AreEqual(Path.Combine(outputDirectory, "wavm-Waves-Outer.nc"), waveModel.WavmFunctionStores.First().Path);
            Assert.AreEqual(Path.Combine(outputDirectory, "wavm-Waves-Inner.nc"), waveModel.WavmFunctionStores.Last().Path);

            IDataItem swanLogDataItem = waveModel.AllDataItems.Single(di => di.Tag == "SwanLogDataItemTag");
            Assert.AreEqual(File.ReadAllText(Path.Combine(outputDirectory, "swn-diag.Waves")), ((TextDocument)swanLogDataItem.Value).Content);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConnectOutput_WhenSwanFileMissing_ShouldGiveLogWarningToUser()
        {
            // Arrange
            var waveModel = new WaveModel {Name = "wave"};
            string outputDirectory = Path.Combine(TestHelper.GetTestDataDirectory(), "output_wavm");

            // Act
            waveModel.ConnectOutput(outputDirectory);

            // Assert
            string expectedMssg = $"Error reading log file: {Path.Combine(outputDirectory, "swn-diag.wave")}";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => waveModel.ConnectOutput(outputDirectory), expectedMssg);
        }
    }
}