using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Csv.Importer;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using DeltaShell.Plugins.ImportExport.GWSW.ViewModels;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests.IO.Importers
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class GwswFileImporterTest : GwswFileImporterTestHelper
    {
        [Test]
        public void TestImport_UnknownFeature_FromGwsw_WithPreviousMapping_Fails_AndLogMessageIsShown()
        {
            // Arrange
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var gwswImporter = new GwswFileImporter(new DefinitionsProvider(logHandler));
            TypeUtils.SetField(gwswImporter,"logHandler", logHandler);
            gwswImporter.CsvDelimeter = ',';
            string filePath = GetFileAndCreateLocalCopy(@"gwswFiles\UnknownFeature.csv");
            string folderPath = Path.GetDirectoryName(filePath);
            
            gwswImporter.CsvDelimeter = ';';

            // Act
            gwswImporter.LoadFeatureFiles(folderPath);
            var importedList = gwswImporter.ImportGwswElementsFromGwswFiles(filePath).SelectMany(e => e).ToList();
            
            // Asserts
            logHandler.Received().ReportInfoFormat(Resources.GwswFileImporterBase_ImportItem_Occurrences_on_file__0__will_not_be_mapped_to_any_element_,
                                                   filePath);
            Assert.IsFalse(importedList.Any());
        }

        [Test]
        public void ImportCsvDebietFileUsingGwswFileImporterAndHardcodedMapping()
        {
            string filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Debiet.csv");
            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = ';',
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    { new CsvRequiredField("UNI_IDE", typeof(string)), new CsvColumnInfo(0, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("DEB_TYP", typeof(string)), new CsvColumnInfo(1, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("VER_IDE", typeof(string)), new CsvColumnInfo(2, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("AVV_ENH", typeof(string)), new CsvColumnInfo(3, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("AFV_OPP", typeof(string)), new CsvColumnInfo(4, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("ALG_TOE", typeof(string)), new CsvColumnInfo(5, CultureInfo.InvariantCulture) },
                }
            };

            GwswFileImportAsDataTableWorksCorrectly(filePath, mappingData);
        }

        [Test]
        public void ImportGwswDefinitionFileWithHardcodedMapping()
        {
            string filePath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");

            var mappingData = new CsvMappingData
            {
                Settings = new CsvSettings
                {
                    Delimiter = csvCommaDelimeter,
                    FirstRowIsHeader = true,
                    SkipEmptyLines = true
                },
                FieldToColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>
                {
                    { new CsvRequiredField("Bestandsnaam", typeof(string)), new CsvColumnInfo(0, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("ElementName", typeof(string)), new CsvColumnInfo(1, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("Kolomnaam", typeof(string)), new CsvColumnInfo(2, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("Code", typeof(string)), new CsvColumnInfo(3, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("Code_International", typeof(string)), new CsvColumnInfo(4, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("Definitie", typeof(string)), new CsvColumnInfo(5, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("Type", typeof(string)), new CsvColumnInfo(6, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("Eenheid", typeof(string)), new CsvColumnInfo(7, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("Verplicht", typeof(string)), new CsvColumnInfo(8, CultureInfo.InvariantCulture) },
                    { new CsvRequiredField("Opmerking", typeof(string)), new CsvColumnInfo(9, CultureInfo.InvariantCulture) },
                }
            };

            CheckCsvIsImportedCorrectly(filePath, mappingData);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WhenImporting2PipesAnd3ManholesFromGwswFiles_ThenCalculationPointsAreAddedToNetwork()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\2Connection3Manholes");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>()
                {
                    Path.Combine(testDir, "Knooppunt.csv"),
                    Path.Combine(testDir, "Verbinding.csv")
                };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var fmModel = new WaterFlowFMModel();
                gwswImporter.ImportItem(null, fmModel);

                IDiscretization discretization = fmModel.NetworkDiscretization;
                Assert.That((object)discretization.Locations.Values.Count, Is.EqualTo(3));

                Coordinate[] coords = discretization.Geometry.Coordinates;
                Assert.That(coords, Contains.Item(new Coordinate(10, 20, double.NaN)));
                Assert.That(coords, Contains.Item(new Coordinate(30, 40, double.NaN)));
                Assert.That(coords, Contains.Item(new Coordinate(23, 99, double.NaN)));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenFmModel_WhenImportingOutletFromGwsw_ThenBoundaryConditionsAreGeneratedWithTimeSeries()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\SimpleModelWithOutlet");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>()
                {
                    Path.Combine(testDir, "Knooppunt.csv"),
                    Path.Combine(testDir, "Verbinding.csv"),
                    Path.Combine(testDir, "Kunstwerk.csv"),
                    Path.Combine(testDir, "Profiel.csv"),
                };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var fmModel = new WaterFlowFMModel();
                gwswImporter.ImportItem(null, fmModel);

                Assert.That((object)fmModel.BoundaryConditions1D.Count, Is.EqualTo(2));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenNoTarget_WhenImportingUsingGwswFileImporter_ThenIntegratedModellIsReturned()
        {
            string testDir = FileUtils.CreateTempDirectory();
            try
            {
                var filesToImport = new List<string>()
                {
                    Path.Combine(testDir, "Knooppunt.csv"),
                };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                object model = gwswImporter.ImportItem(null, null);
                Assert.IsNotNull(model as HydroModel);
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenRrModelOrFmModel_WhenImportingUsingGwswFileImporter_ThenNullIsReturned()
        {
            var gwswImporter = new GwswFileImporter(new DefinitionsProvider());
            var fmModel = new WaterFlowFMModel();
            object model = gwswImporter.ImportItem(null, fmModel);
            Assert.IsNull(model);

            gwswImporter = new GwswFileImporter(new DefinitionsProvider());
            model = gwswImporter.ImportItem(null, fmModel);
            Assert.IsNull(model);
        }

        [Test]
        public void GivenFmModel_WhenImportingNodesFromGwsw_ThenCorrectNumberOfManholesAreAddedToFmModelNetwork()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>() { Path.Combine(testDir, "Knooppunt.csv") };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var fmModel = new WaterFlowFMModel();
                gwswImporter.ImportItem(null, fmModel);

                Assert.That(fmModel.Network.Nodes.Count, Is.EqualTo(76));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenFmModel_WhenImportingNodesFromGwsw_ThenCorrectNumberOfCompartmentsAreAddedToFmModelNetwork()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>() { Path.Combine(testDir, "Knooppunt.csv") };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var fmModel = new WaterFlowFMModel();
                gwswImporter.ImportItem(null, fmModel);

                Assert.That(fmModel.Network.Manholes.SelectMany(m => m.Compartments).Count(), Is.EqualTo(90));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenFmModel_WhenImportingBranchesFromGwsw_ThenCorrectNumberOfSewerConnectionsAreAddedToFmModelNetwork()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>()
                {
                    Path.Combine(testDir, "Knooppunt.csv"),
                    Path.Combine(testDir, "Verbinding.csv")
                };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var fmModel = new WaterFlowFMModel();
                gwswImporter.ImportItem(null, fmModel);

                Assert.That(fmModel.Network.SewerConnections.Count(), Is.EqualTo(97));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenFmModel_WhenImportingNodesFromGwsw_ThenCompartmentInNetworkHasCorrectValues()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>() { Path.Combine(testDir, "Knooppunt.csv") };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var fmModel = new WaterFlowFMModel();
                gwswImporter.ImportItem(null, fmModel);

                ICompartment cmp76 = fmModel.Network.Manholes.SelectMany(m => m.Compartments)
                                            .FirstOrDefault(c => c.Name.Equals("cmp76", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsNotNull(cmp76);
                Assert.IsNotNull(cmp76.Geometry);
                Assert.That(cmp76.FloodableArea, Is.EqualTo(500));
                Assert.That(cmp76.ManholeLength, Is.EqualTo(2));
                Assert.That(cmp76.ManholeWidth, Is.EqualTo(2));
                Assert.That(cmp76.Name, Is.EqualTo("cmp76"));
                Assert.That(cmp76.ParentManholeName, Is.EqualTo("03001"));
                IManhole parentManhole = fmModel.Network.GetManhole(cmp76);
                Assert.That(cmp76.ParentManhole, Is.EqualTo(parentManhole));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenNoTarget_WhenImportingNwrwDefinitionsFromGwsw_ThenCorrectNumberOfDefinitionsAreAddedToRrModel()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>() { Path.Combine(testDir, "Nwrw.csv") };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var returnedModel = gwswImporter.ImportItem(null, null) as HydroModel;

                RainfallRunoffModel rrModel =
                    returnedModel?.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
                Assert.IsNotNull(rrModel);

                Assert.That(rrModel.NwrwDefinitions.Count(), Is.EqualTo(12));
                CollectionAssert.AllItemsAreUnique(rrModel.NwrwDefinitions);
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenTargetIsNull_WhenImportingCsv_ThenIntegratedModelIsReturned()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>() { Path.Combine(testDir, "Knooppunt.csv") };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                object returnedModel = gwswImporter.ImportItem(null, null);

                Assert.That(returnedModel is HydroModel);
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenTargetIsNull_WhenImportingGwswModel_ThenHisAndMapOutputIntervalAreSetEqualToIntegratedModelTimeStep()
        {
            // Setup
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>() { Path.Combine(testDir, "Knooppunt.csv") };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                // Call
                object returnedModel = gwswImporter.ImportItem(null, null);

                // Assert
                Assert.That(returnedModel is HydroModel);
                var hydroModel = (HydroModel)returnedModel;

                IEnumerable<WaterFlowFMModel> flowFmModels = hydroModel.GetAllActivitiesRecursive<WaterFlowFMModel>();
                Assert.That(flowFmModels.Count(), Is.EqualTo(1));
                WaterFlowFMModel flowFmModel = flowFmModels.Single();

                TimeSpan hydroModelTimeStep = hydroModel.TimeStep;
                var flowFmModelHisOutputTimeStep = (TimeSpan)flowFmModel.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value;
                var flowFmModelMapOutputTimeStep = (TimeSpan)flowFmModel.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value;
                Assert.That(flowFmModelHisOutputTimeStep, Is.EqualTo(hydroModelTimeStep));
                Assert.That(flowFmModelMapOutputTimeStep, Is.EqualTo(hydroModelTimeStep));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenTargetIsNull_WhenImportingNwrw_ThenNwrwDefinitionInRrNetworkHasCorrectValues()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>() { Path.Combine(testDir, "Nwrw.csv") };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var returnedModel = gwswImporter.ImportItem(null, null) as HydroModel;

                RainfallRunoffModel rrModel =
                    returnedModel?.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
                Assert.IsNotNull(rrModel);

                NwrwDefinition nwrwDefinition = rrModel.NwrwDefinitions.FirstOrDefault(nd =>
                                                                                           nd.SurfaceType.Equals(NwrwSurfaceType.OpenPavedFlatStretched));
                Assert.IsNotNull(nwrwDefinition);
                Assert.That(nwrwDefinition.Name, Is.EqualTo("OVH_VLU"));
                Assert.That(nwrwDefinition.SurfaceType, Is.EqualTo(NwrwSurfaceType.OpenPavedFlatStretched));
                Assert.That(nwrwDefinition.SurfaceStorage, Is.EqualTo(1.0));
                Assert.That(nwrwDefinition.InfiltrationCapacityMax, Is.EqualTo(2.0));
                Assert.That(nwrwDefinition.InfiltrationCapacityMin, Is.EqualTo(0.5));
                Assert.That(nwrwDefinition.InfiltrationCapacityReduction, Is.EqualTo(3.0));
                Assert.That(nwrwDefinition.InfiltrationCapacityRecovery, Is.EqualTo(0.1));
                Assert.That(nwrwDefinition.RunoffDelay, Is.EqualTo(0.1));
                Assert.That(nwrwDefinition.RunoffLength, Is.EqualTo(0.0));
                Assert.That(nwrwDefinition.RunoffSlope, Is.EqualTo(0.0));
                Assert.That(nwrwDefinition.TerrainRoughness, Is.EqualTo(0.0));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenTargetIsNull_WhenImportingDryWeatherFlowDefinitions_ThenCorrectNumberOfDefinitionsAreAddedToRrModel()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>() { Path.Combine(testDir, "Verloop.csv") };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var returnedModel = gwswImporter.ImportItem(null, null) as HydroModel;

                RainfallRunoffModel rrModel =
                    returnedModel?.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
                Assert.IsNotNull(rrModel);

                Assert.That(rrModel.NwrwDryWeatherFlowDefinitions.Count(), Is.EqualTo(6));
                CollectionAssert.AllItemsAreUnique(rrModel.NwrwDryWeatherFlowDefinitions);
                Assert.False(rrModel.NwrwDryWeatherFlowDefinitions.Any(dwfd =>
                                                                           dwfd.DistributionType.Equals(DryweatherFlowDistributionType.Variable))); // not supported
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenNoTarget_WhenImportingDryWeatherFlowDefintions_ThenDefinitionHasCorrectValuesInRrModel()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>() { Path.Combine(testDir, "Verloop.csv") };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var returnedModel = gwswImporter.ImportItem(null, null) as HydroModel;

                RainfallRunoffModel rrModel =
                    returnedModel?.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
                Assert.IsNotNull(rrModel);

                NwrwDryWeatherFlowDefinition nwrwDryWeatherFlowDefinition = rrModel.NwrwDryWeatherFlowDefinitions
                                                                                   .FirstOrDefault(dwfd => dwfd.Name.Equals("Inwoner", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsNotNull(nwrwDryWeatherFlowDefinition);

                double[] expectedHourlyPercentages = { 1.5, 1.5, 1.5, 1.5, 1.5, 3.0, 4.0, 5.0, 6.0, 6.5, 7.5, 8.5, 7.5, 6.5, 6.0, 5.0, 5.0, 5.0, 4.0, 3.5, 3.0, 2.5, 2.0, 2.0 };
                Assert.That(nwrwDryWeatherFlowDefinition.Name, Is.EqualTo("Inwoner"));
                Assert.That(nwrwDryWeatherFlowDefinition.DistributionType, Is.EqualTo(DryweatherFlowDistributionType.Daily));
                Assert.That(nwrwDryWeatherFlowDefinition.DayNumber, Is.EqualTo(0.0));
                Assert.That(nwrwDryWeatherFlowDefinition.DailyVolumeVariable, Is.EqualTo(120));
                Assert.That(nwrwDryWeatherFlowDefinition.HourlyPercentageDailyVolume,
                            Is.EqualTo(expectedHourlyPercentages));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenNoTarget_WhenImportingGwswData_ThenCorrectNumberOfNwrwCatchmentsAreAddedToRrModel()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>()
                {
                    Path.Combine(testDir, "Knooppunt.csv"),
                    Path.Combine(testDir, "Verbinding.csv"),
                    Path.Combine(testDir, "Verloop.csv"),
                    Path.Combine(testDir, "Nwrw.csv"),
                    Path.Combine(testDir, "Debiet.csv"),
                    Path.Combine(testDir, "Oppervlak.csv"),
                };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var returnedModel = gwswImporter.ImportItem(null, null) as HydroModel;

                RainfallRunoffModel rrModel =
                    returnedModel?.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
                Assert.IsNotNull(rrModel);

                Assert.That(rrModel.GetAllModelData().OfType<NwrwData>().Count(), Is.EqualTo(74));
                CollectionAssert.AllItemsAreUnique(rrModel.GetAllModelData().OfType<NwrwData>());
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenNoTarget_WhenImportingGwswData_ThenNwrwCatchmentWithSurfaceAndDwfDataHasCorrectValuesInRrModel()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>()
                {
                    Path.Combine(testDir, "Knooppunt.csv"),
                    Path.Combine(testDir, "Verbinding.csv"),
                    Path.Combine(testDir, "Verloop.csv"),
                    Path.Combine(testDir, "Nwrw.csv"),
                    Path.Combine(testDir, "Debiet.csv"),
                    Path.Combine(testDir, "Oppervlak.csv"),
                };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var returnedModel = gwswImporter.ImportItem(null, null) as HydroModel;

                RainfallRunoffModel rrModel =
                    returnedModel?.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
                Assert.IsNotNull(rrModel);

                NwrwData lei17 = rrModel.GetAllModelData().OfType<NwrwData>().FirstOrDefault(md =>
                                                                                                 md.Name.Equals("lei17", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsNotNull(lei17);
                Assert.That(lei17.DryWeatherFlows.Count(), Is.EqualTo(2));
                Assert.IsNotNull(lei17.DryWeatherFlows.Select(dwf =>
                                                                  dwf.DryWeatherFlowId.Equals("Bedrijf", StringComparison.InvariantCultureIgnoreCase)));
                Assert.IsNotNull(lei17.DryWeatherFlows.Select(dwf =>
                                                                  dwf.DryWeatherFlowId.Equals(NwrwDryWeatherFlowDefinition.DefaultDwaId, StringComparison.InvariantCultureIgnoreCase)));
                Assert.That(lei17.LateralSurface, Is.EqualTo(0.0));
                Assert.That(lei17.MeteoStationId, Is.EqualTo(string.Empty));
                Assert.That(lei17.NodeOrBranchId, Is.EqualTo("lei17"));
                Assert.That(lei17.NumberOfSpecialAreas, Is.EqualTo(0));
                Assert.That(lei17.SpecialAreas.Count(), Is.EqualTo(0));
                Assert.That(lei17.SurfaceLevelDict.Count(), Is.EqualTo(4));
                Assert.That(lei17.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedFlat], Is.EqualTo(500));
                Assert.That(lei17.SurfaceLevelDict[NwrwSurfaceType.OpenPavedFlat], Is.EqualTo(250));
                Assert.That(lei17.SurfaceLevelDict[NwrwSurfaceType.RoofWithSlope], Is.EqualTo(50));
                Assert.That(lei17.SurfaceLevelDict[NwrwSurfaceType.RoofFlat], Is.EqualTo(150));
                Assert.That(lei17.Name, Is.EqualTo("lei17"));
                Assert.That(lei17.CalculationArea, Is.EqualTo(950.0).Within(0.00005));
                Assert.IsNotNull(lei17.Catchment);
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenNoTarget_WhenImportingGwswData_ThenNwrwCatchmentWithSurfaceAndDefaultDwfDataHasCorrectValuesInRrModel()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>()
                {
                    Path.Combine(testDir, "Knooppunt.csv"),
                    Path.Combine(testDir, "Verbinding.csv"),
                    Path.Combine(testDir, "Verloop.csv"),
                    Path.Combine(testDir, "Nwrw.csv"),
                    Path.Combine(testDir, "Debiet.csv"),
                    Path.Combine(testDir, "Oppervlak.csv"),
                };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var returnedModel = gwswImporter.ImportItem(null, null) as HydroModel;

                RainfallRunoffModel rrModel =
                    returnedModel?.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
                Assert.IsNotNull(rrModel);

                NwrwData lei43 = rrModel.GetAllModelData().OfType<NwrwData>().FirstOrDefault(md =>
                                                                                                 md.Name.Equals("lei43", StringComparison.InvariantCultureIgnoreCase));
                Assert.IsNotNull(lei43);
                Assert.That(lei43.DryWeatherFlows.Count(), Is.EqualTo(2));
                Assert.That(lei43.DryWeatherFlows.Select(dwf => dwf.DryWeatherFlowId).FirstOrDefault(),
                            Is.EqualTo(NwrwDryWeatherFlowDefinition.DefaultDwaId));
                Assert.That(lei43.LateralSurface, Is.EqualTo(0.0));
                Assert.That(lei43.MeteoStationId, Is.EqualTo(string.Empty));
                Assert.That(lei43.NodeOrBranchId, Is.EqualTo("lei43"));
                Assert.That(lei43.NumberOfSpecialAreas, Is.EqualTo(0));
                Assert.That(lei43.SpecialAreas.Count(), Is.EqualTo(0));
                Assert.That(lei43.SurfaceLevelDict.Count(), Is.EqualTo(3));
                Assert.That(lei43.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedFlat], Is.EqualTo(400));
                Assert.That(lei43.SurfaceLevelDict[NwrwSurfaceType.OpenPavedFlatStretched], Is.EqualTo(800));
                Assert.That(lei43.SurfaceLevelDict[NwrwSurfaceType.UnpavedWithSlope], Is.EqualTo(27));
                Assert.That(lei43.Name, Is.EqualTo("lei43"));
                Assert.That(lei43.CalculationArea, Is.EqualTo(1227.0).Within(0.00005));
                Assert.IsNotNull(lei43.Catchment);

                // special case in Debiet.csv
                NwrwData put10 = rrModel.GetAllModelData().OfType<NwrwData>().FirstOrDefault(md =>
                                                                                                 md.Name.Equals("put10", StringComparison.InvariantCultureIgnoreCase));
                Assert.That(put10.SurfaceLevelDict.Count, Is.EqualTo(1));
                Assert.That(put10.SurfaceLevelDict.ContainsKey(NwrwSurfaceType.ClosedPavedWithSlope), Is.True);
                Assert.That(put10.SurfaceLevelDict[NwrwSurfaceType.ClosedPavedWithSlope], Is.EqualTo(12));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenOldGwswFormat_WhenLoadingFeatureFiles_ThenCorrectDefinitionFileIsLoaded()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();

            try
            {
                FileUtils.CopyDirectory(originalDir, testDir);
                var filesToImport = new List<string>() { Path.Combine(testDir, "Knooppunt.csv") };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);
                Assert.That(gwswImporter.GwswAttributesDefinition.Count(), Is.EqualTo(155));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenNewGwswFormat_WhenLoadingFeatureFiles_ThenCorrectDefinitionFileIsLoaded()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_Juinen_New");
            string testDir = FileUtils.CreateTempDirectory();

            try
            {
                FileUtils.CopyDirectory(originalDir, testDir);

                var filesToImport = new List<string>() { Path.Combine(testDir, "Knooppunt.csv") };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                Assert.That(gwswImporter.GwswAttributesDefinition.Count(), Is.EqualTo(132));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenAGWSWImport_ShouldNotAddLateralDataForExistingLaterals()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();

            var fmModel = new WaterFlowFMModel();
            var node1 = new HydroNode("haha") { Geometry = new Point(0, 0) };
            var node2 = new HydroNode("hihi") { Geometry = new Point(100, 0) };
            var channel = new Channel("hehe", node1, node2);
            var lateralSourceToTest = new LateralSource
            {
                Name = "hoho",
                Branch = channel,
                Chainage = 0.0
            };
            channel.BranchFeatures.Add(lateralSourceToTest);
            fmModel.Network.Nodes.Add(node1);
            fmModel.Network.Nodes.Add(node2);
            fmModel.Network.Branches.Add(channel);

            Assert.AreEqual(1, fmModel.Network.LateralSources.Count());
            Assert.AreEqual(1, fmModel.LateralSourcesData.Count(lsd => lsd.Feature == lateralSourceToTest));

            try
            {
                FileUtils.CopyDirectory(originalDir, testDir);

                var filesToImport = new List<string>()
                {
                    Path.Combine(testDir, "Knooppunt.csv"),
                    Path.Combine(testDir, "Verbinding.csv"),
                    Path.Combine(testDir, "Profiel.csv"),
                    Path.Combine(testDir, "Kunstwerk.csv"),
                };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                gwswImporter.ImportItem(testDir, fmModel);

                Assert.AreEqual(1, fmModel.LateralSourcesData.Count(lsd => lsd.Feature == lateralSourceToTest));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenNoTarget_WhenImportingGwswData_ThenLateralSourcesAreCorrectlyAddedToFmModel()
        {
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            try
            {
                var filesToImport = new List<string>()
                {
                    Path.Combine(testDir, "Knooppunt.csv"),
                    Path.Combine(testDir, "Verbinding.csv"),
                    Path.Combine(testDir, "Verloop.csv"),
                    Path.Combine(testDir, "Debiet.csv"),
                };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                var returnedModel = gwswImporter.ImportItem(null, null) as HydroModel;
                Assert.That(returnedModel, Is.Not.Null);

                RainfallRunoffModel rrModel =
                    returnedModel.GetAllActivitiesRecursive<RainfallRunoffModel>()?.FirstOrDefault();
                Assert.IsNotNull(rrModel);

                WaterFlowFMModel fmModel =
                    returnedModel.GetAllActivitiesRecursive<WaterFlowFMModel>()?.FirstOrDefault();
                Assert.IsNotNull(fmModel);

                var expectedLateralSourcesDataCount = 71;
                IEventedList<Model1DLateralSourceData> lateralSourcesData = fmModel.LateralSourcesData;
                Assert.That(lateralSourcesData, Is.Not.Null);
                Assert.That(lateralSourcesData.Count, Is.EqualTo(expectedLateralSourcesDataCount));

                var expectedConstantTypeCount = 2; // 2x 'LAT' in combination with a dryweather flow id
                IEnumerable<Model1DLateralSourceData> constantLateralSourcesData = lateralSourcesData.Where(lsd => lsd.DataType == Model1DLateralDataType.FlowConstant);
                Assert.That(constantLateralSourcesData.Count, Is.EqualTo(expectedConstantTypeCount));

                int expectedRealTimeTypeCount = 68 + 1; //68x 'VWD' and 1 special case
                IEnumerable<Model1DLateralSourceData> realTimeLateralSourcesData = lateralSourcesData.Where(lsd => lsd.DataType == Model1DLateralDataType.FlowRealTime);
                Assert.That(realTimeLateralSourcesData.Count, Is.EqualTo(expectedRealTimeTypeCount));

                var expectedLateralSourcesCount = 71;
                IEventedList<IBranch> branches = fmModel.Network.Branches;
                IEnumerable<LateralSource> lateralSources = branches.SelectMany(b => b.BranchFeatures).OfType<LateralSource>();
                Assert.That(lateralSources.Count, Is.EqualTo(expectedLateralSourcesCount));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenAnIntegratedModel_WhenImportingGwswData_ThenExpectedModelSettingsAreSet()
        {
            // Given
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);

            HydroModel hydroModel = new HydroModelBuilder().BuildModel(ModelGroup.RHUModels);

            try
            {
                var filesToImport = new List<string>()
                {
                    Path.Combine(testDir, "Knooppunt.csv"),
                    Path.Combine(testDir, "Verbinding.csv"),
                };
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, testDir);

                // When
                gwswImporter.ImportItem(null, hydroModel);

                // Then
                Assert.That(hydroModel.OverrideTimeStep, Is.True);
                Assert.That(hydroModel.OverrideStartTime, Is.True);
                Assert.That(hydroModel.OverrideStopTime, Is.True);

                var expectedTimeStep = new TimeSpan(0, 1, 0);

                TimeSpan actualHydroModelTimeStep = hydroModel.TimeStep;
                Assert.That(actualHydroModelTimeStep, Is.EqualTo(expectedTimeStep));

                WaterFlowFMModel fmModel = hydroModel.GetAllActivitiesRecursive<WaterFlowFMModel>()?.FirstOrDefault();
                Assert.IsNotNull(fmModel);

                var flowFmModelHisOutputTimeStep = (TimeSpan)fmModel.ModelDefinition.GetModelProperty(GuiProperties.HisOutputDeltaT).Value;
                Assert.That(flowFmModelHisOutputTimeStep, Is.EqualTo(expectedTimeStep));

                var flowFmModelMapOutputTimeStep = (TimeSpan)fmModel.ModelDefinition.GetModelProperty(GuiProperties.MapOutputDeltaT).Value;
                Assert.That(flowFmModelMapOutputTimeStep, Is.EqualTo(expectedTimeStep));

                var flowFmModelTimeStep = (TimeSpan)fmModel.ModelDefinition.GetModelProperty(KnownProperties.DtUser).Value;
                Assert.That(flowFmModelTimeStep, Is.EqualTo(expectedTimeStep));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportingGWSWModelWithOrifice_CorrectlySetsLowerEdgeLevelOfOrifice()
        {
            // Setup
            string originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");

            using (var tempDir = new TemporaryDirectory())
            {
                string tempDirPath = tempDir.Path;
                FileUtils.CopyDirectory(originalDir, tempDirPath);

                var filesToImport = new List<string>()
                {
                    Path.Combine(tempDirPath, "Knooppunt.csv"),
                    Path.Combine(tempDirPath, "Verbinding.csv"),
                    Path.Combine(tempDirPath, "Verloop.csv"),
                    Path.Combine(tempDirPath, "Debiet.csv"),
                    Path.Combine(tempDirPath, "Kunstwerk.csv"),
                };

                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, tempDirPath);

                // Call
                object importedModel = gwswImporter.ImportItem(tempDirPath);

                // Assert
                Assert.That(importedModel, Is.Not.Null);
                WaterFlowFMModel fmModel = ((HydroModel)importedModel).Models.OfType<WaterFlowFMModel>().First();

                IEnumerable<IOrifice> orifices = fmModel.Network.Orifices;
                foreach (IOrifice orifice in orifices)
                {
                    var gatedWeirFormula = orifice.WeirFormula as GatedWeirFormula;
                    Assert.That(gatedWeirFormula, Is.Not.Null);

                    double expectedGateLowerEdgeLevel = orifice.CrestLevel + orifice.CrestWidth;
                    Assert.That(gatedWeirFormula.LowerEdgeLevel, Is.EqualTo(expectedGateLowerEdgeLevel));
                }
            }
        }

        public class GwswFileImporterTestShadow
        {
            [Test]
            public void Constructor_DefinitionsProviderNull_ThrowsArgumentNullException()
            {
                // Call
                TestDelegate call = () => new GwswFileImporter(null);

                // Assert
                Assert.That(call, Throws.Exception.TypeOf<ArgumentNullException>()
                                        .With.Property(nameof(ArgumentNullException.ParamName))
                                        .EqualTo("definitionsProvider"));
            }

            [Test]
            public void Constructor_ExpectedValues()
            {
                // Call
                var importer = new GwswFileImporter(new DefinitionsProvider());

                // Assert
                Assert.IsInstanceOf<IFileImporter>(importer);
                Assert.That(importer, Is.InstanceOf<IFileImporter>());
                Assert.That(importer.CsvDelimeter, Is.EqualTo(';'));
                Assert.That(importer.GwswAttributesDefinition, Is.Empty);
                Assert.That(importer.GwswDefaultFeatures, Is.Empty);

                CollectionAssert.AreEquivalent(new[] { typeof(HydroModel), typeof(IWaterFlowFMModel), typeof(RainfallRunoffModel) }, importer.SupportedItemTypes);
            }
        }

        private GwswFileImporter SetupGwswFileImporter(IList<string> filesToImport, string testDir, DefinitionsProvider definitionProvider = null)
        {
            if (definitionProvider == null)
            {
                definitionProvider = new DefinitionsProvider();
            }

            var gwswImporter = new GwswFileImporter(definitionProvider);
            gwswImporter.LoadFeatureFiles(testDir);
            gwswImporter.FilesToImport = filesToImport;

            return gwswImporter;
        }

        #region Gwsw Attribute tests

        [Test]
        public void GetEnumTypeFromGwswAttribute_ReturnsDefaultValueAndLogMessage_IfNotFound()
        {
            var elementName = "test_element";
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var attributeTest = new GwswAttribute
            {
                ValueAsString = elementName,
                GwswAttributeType = new GwswAttributeType(logHandler) { AttributeType = typeof(string) }
            };

            var value = SewerConnectionWaterType.StormWater;
            //Just to make sure the test is setting the default value later on.
            Assert.AreNotEqual((object)default(SewerConnectionWaterType), (object)value);

            value = attributeTest.GetValueFromDescription<SewerConnectionWaterType>(logHandler);
            logHandler.Received().ReportWarningFormat(Resources.SewerFeatureFactory_GetValueFromDescription_Type__0__is_not_recognized__please_check_the_syntax, elementName);

            Assert.IsNotNull(value);
            Assert.AreEqual((object)default(SewerConnectionWaterType), (object)value);
        }

        [Test]
        public void GetEnumTypeFromGwswAttribute_ReturnsCorrectValue_IfFound()
        {
            var elementName = "Dry weather";
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var attributeTest = new GwswAttribute
            {
                ValueAsString = elementName,
                GwswAttributeType = new GwswAttributeType(logHandler) { AttributeType = typeof(string) }
            };
            var value = attributeTest.GetValueFromDescription<SewerConnectionWaterType>(logHandler);
            Assert.IsNotNull(value);
            Assert.AreEqual((object)SewerConnectionWaterType.DryWater, (object)value);
        }

        [Test]
        public void GwswAttributeIsValid_ReturnsFalseWithoutLogMessageIfNoTypeIsPresent()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var invalidAttribute = new GwswAttribute { GwswAttributeType = new GwswAttributeType(logHandler) };
            CheckThatGwswAttributeValidationLogMessageIsReturned(null, 0, null, null, invalidAttribute);
        }

        [TestCase("")]
        [TestCase(null)]
        public void GivenGwswAttributeWithEmptyValueAsString_WhenValidatingAttribute_ThenLogMessageIsReturned(
            string valueAsString)
        {
            const string fileName = "myFile.csv";
            const int lineNumber = 3;
            const string localKey = "XXX_YYY";
            const string key = "MY_KEY";
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            var attributeType = new GwswAttributeType(logHandler)
            {
                FileName = fileName,
                LocalKey = localKey,
                Key = key
            };

            var invalidAttribute = new GwswAttribute
            {
                LineNumber = lineNumber,
                ValueAsString = valueAsString,
                GwswAttributeType = attributeType
            };

            CheckThatGwswAttributeValidationLogMessageIsReturned(fileName, lineNumber, localKey, key, invalidAttribute);
        }

        [Test]
        public void GwswAttributeIsValid_ReturnsTrueIfEverythingIsPresent()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            var emptyAttribute = new GwswAttribute
            {
                ValueAsString = "test",
                GwswAttributeType = new GwswAttributeType(logHandler) { AttributeType = typeof(string) }
            };
            
            Assert.IsTrue(emptyAttribute.IsValidAttribute(logHandler));
        }

        [Test]
        public void GwswAttributeIsValid_ReturnsFalseIfNoTypeIsPresent()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>(); 
            var emptyAttribute = new GwswAttribute { GwswAttributeType = new GwswAttributeType(logHandler) };
            Assert.IsFalse(emptyAttribute.IsValidAttribute(logHandler));
        }

        [TestCase("")]
        [TestCase(null)]
        public void GwswAttributeIsValid_ReturnsFalseIfValueAsStringIsNullOrEmpty(string valueAsString)
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var invalidAttribute = new GwswAttribute
            {
                ValueAsString = valueAsString,
                GwswAttributeType = new GwswAttributeType(logHandler)
            };
            Assert.IsFalse(invalidAttribute.IsValidAttribute(logHandler));
        }

        [Test]
        public void GwswAttribute_IsTypeOfInt_Test()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var attr = new GwswAttribute { GwswAttributeType = new GwswAttributeType(logHandler) { AttributeType = typeof(int) } };
            Assert.IsTrue(attr.IsTypeOf(typeof(int)));
            Assert.IsFalse(attr.IsTypeOf(typeof(double)));
            Assert.IsFalse(attr.IsTypeOf(typeof(string)));
        }

        [Test]
        public void GwswAttribute_IsNumerical_GivenInt_Test()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var attr = new GwswAttribute { GwswAttributeType = new GwswAttributeType(logHandler) { AttributeType = typeof(int) } };
            Assert.IsTrue(attr.IsNumerical());
        }

        [Test]
        public void GwswAttribute_IsNumerical_GivenDouble_Test()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var attr = new GwswAttribute { GwswAttributeType = new GwswAttributeType(logHandler) { AttributeType = typeof(double) } };
            Assert.IsTrue(attr.IsNumerical());
        }

        [Test]
        public void GwswAttribute_IsNumerical_GivenString_Test()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var attr = new GwswAttribute { GwswAttributeType = new GwswAttributeType(logHandler) { AttributeType = typeof(string) } };
            Assert.IsFalse(attr.IsNumerical());
        }

        [Test]
        public void GwswAttribute_IsTypeOfDouble_Test()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var attr = new GwswAttribute { GwswAttributeType = new GwswAttributeType(logHandler) { AttributeType = typeof(double) } };
            Assert.IsFalse(attr.IsTypeOf(typeof(int)));
            Assert.IsTrue(attr.IsTypeOf(typeof(double)));
            Assert.IsFalse(attr.IsTypeOf(typeof(string)));
        }

        [Test]
        public void GwswAttribute_IsTypeOfString_Test()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var attr = new GwswAttribute { GwswAttributeType = new GwswAttributeType(logHandler) { AttributeType = typeof(string) } };
            Assert.IsFalse(attr.IsTypeOf(typeof(int)));
            Assert.IsFalse(attr.IsTypeOf(typeof(double)));
            Assert.IsTrue(attr.IsTypeOf(typeof(string)));
        }

        [Test]
        public void GetElementLine_ReturnsLineIfAvailable()
        {
            var elementName = "DWA";
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            var gwswElement = new GwswElement
            {
                GwswAttributeList = new List<GwswAttribute>
                {
                    new GwswAttribute
                    {
                        LineNumber = 2,
                        ValueAsString = elementName,
                        GwswAttributeType = new GwswAttributeType(logHandler) { AttributeType = typeof(string) }
                    }
                }
            };
            Assert.AreEqual(2, gwswElement.GetElementLine());
        }

        [Test]
        public void GetElementLine_ReturnsZeroIfNotAvailable()
        {
            var elementName = "DWA";
            var gwswElement = new GwswElement
            {
                GwswAttributeList = new List<GwswAttribute>
                {
                    new GwswAttribute
                    {
                        ValueAsString = elementName,
                    }
                }
            };
            Assert.AreEqual(0, gwswElement.GetElementLine());
        }

        [Test]
        public void GwswAttributeReturnsElementNameWithoutExtension()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();

            var elementName = "test_element";
            var attributeTest = new GwswAttributeType(logHandler)
            {
                ElementName = elementName + ".csv",
            };
            Assert.AreEqual(elementName, attributeTest.ElementName);

            /* If the name is originally given without extension, it should remain the same.*/
            attributeTest = new GwswAttributeType(logHandler)
            {
                ElementName = elementName,
            };
            Assert.AreEqual(elementName, attributeTest.ElementName);
        }

        [Test]
        [TestCase("string", typeof(string))]
        [TestCase("double", typeof(double))]
        public void GwswAttibuteAssignesATypeToTheValue(string typeAsString, Type expectedType)
        {
            try
            {
                ILogHandler logHandler = Substitute.For<ILogHandler>();
                GwswAttributeType attributeTest = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile.csv", 0,
                                                                                                     "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks", logHandler);
                Assert.IsNotNull(attributeTest);
                Assert.AreEqual(expectedType, attributeTest.AttributeType);
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format(
                                "The following error was given while trying to create a new Gwsw Attribute {0}", e.Message));
            }
        }

        [Test]
        public void GwswElementExtensionsGetAttributeFromList()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var attributeOne = "attributeOne";
            var attributeTwo = "attributeTwo";
            var valueAsString = "valueAttrOne";
            var gwswElement = new GwswElement
            {
                ElementTypeName = "test",
                GwswAttributeList = new List<GwswAttribute>
                {
                    new GwswAttribute
                    {
                        GwswAttributeType = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile", 5,
                                                                                               "columnName", "string", attributeOne,
                                                                                               "unkownDefinition", "mandatoryMaybe", string.Empty, "noRemarks", logHandler),
                        ValueAsString = valueAsString
                    },
                }
            };
            GwswAttribute retrievedAttr = gwswElement.GetAttributeFromList(attributeOne, logHandler);
            Assert.IsNotNull(retrievedAttr);
            Assert.AreEqual(valueAsString, retrievedAttr.ValueAsString);

            Assert.IsNull(gwswElement.GetAttributeFromList(attributeTwo, logHandler));
        }

        [Test]
        public void GwswElementExtensionsGetValidStringValueSucceeds()
        {
            var expectedValue = "test";
            string valueAsString = expectedValue;
            var typeAsString = "string";
            try
            {
                ILogHandler logHandler = Substitute.For<ILogHandler>(); 
                GwswAttributeType gwswAttributeType = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile.csv", 0,
                                                                                                                                                                 "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks", logHandler);
                Assert.IsNotNull(gwswAttributeType);
                Assert.AreEqual(typeof(string), gwswAttributeType.AttributeType);

                var attribute = new GwswAttribute
                {
                    GwswAttributeType = gwswAttributeType,
                    ValueAsString = valueAsString
                };
                Assert.IsNotNull(attribute);
                
                string testVariable = attribute.GetValidStringValue(logHandler);
                Assert.AreEqual(expectedValue, testVariable);
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format(
                                "The following error was given while trying to create a new Gwsw Attribute {0}", e.Message));
            }
        }

        [Test]
        public void GwswElementExtensionsTryGetValueAsDoubleSucceeds()
        {
            var expectedValue = 100.0;
            var valueAsString = expectedValue.ToString(CultureInfo.InvariantCulture);
            var typeAsString = "double";
            var testVariable = 0.0;
            try
            {
                ILogHandler logHandler = Substitute.For<ILogHandler>();
                GwswAttributeType gwswAttributeType = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile.csv", 0,
                                                                                                         "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks", logHandler);
                Assert.IsNotNull(gwswAttributeType);
                Assert.AreEqual(typeof(double), gwswAttributeType.AttributeType);

                var attribute = new GwswAttribute
                {
                    GwswAttributeType = gwswAttributeType,
                    ValueAsString = valueAsString
                };
                Assert.IsNotNull(attribute);
                Assert.IsTrue(attribute.TryGetValueAsDouble(logHandler, out testVariable));
                Assert.AreEqual(expectedValue, testVariable);
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format(
                                "The following error was given while trying to create a new Gwsw Attribute {0}", e.Message));
            }
        }

        [Test]
        public void GwswElementExtensionsSetValueIfPossibleForDoubleFailsWithStringValueAndLogsMessage()
        {
            var valueAsString = "stringValue";
            var typeAsString = "string";
            try
            {
                ILogHandler logHandler = Substitute.For<ILogHandler>();
                GwswAttributeType gwswAttributeType = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile.csv", 0,
                                                                                                         "attributeName", typeAsString, "testCode", "test definition", "mandatory", string.Empty, "remarks", logHandler);
                Assert.IsNotNull(gwswAttributeType);
                Assert.AreEqual(typeof(string), gwswAttributeType.AttributeType);

                var attribute = new GwswAttribute
                {
                    GwswAttributeType = gwswAttributeType,
                    ValueAsString = valueAsString,
                    LineNumber = 2
                };
                Assert.IsNotNull(attribute);

                attribute.TryGetValueAsDouble(logHandler, out double _);
                logHandler.Received(1).ReportErrorFormat(Resources.GwswElementExtensions_LogErrorParseType_File__0___line__1___element__2___It_was_not_possible_to_parse_attribute__3__from_type__4__to_type__5__,
                                                             gwswAttributeType.FileName, attribute.LineNumber, gwswAttributeType.ElementName,
                                                             gwswAttributeType.Name, attribute.ValueAsString, gwswAttributeType.AttributeType,
                                                             typeof(double));
            }
            catch (Exception e)
            {
                Assert.Fail(string.Format(
                                "The following error was given while trying to create a new Gwsw Attribute {0}", e.Message));
            }
        }

        [Test]
        public void GivenGwswAttributeWithEmptyStringAsValue_WhenTryGetDoubleValue_ThenReturnFalseAndDefaultValue()
        {
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            GwswAttributeType gwswAttributeType = SewerFeatureFactoryTestHelper.GetGwswAttributeType("testFile.csv", 0,
                                                                                                     "attributeName", "double", "testCode", "test definition", "mandatory", string.Empty, "remarks", logHandler);
            var attribute = new GwswAttribute
            {
                GwswAttributeType = gwswAttributeType,
                ValueAsString = string.Empty
            };

            double doubleValue;
            bool gettingValueSucceeded = attribute.TryGetValueAsDouble(logHandler, out doubleValue);
            Assert.IsFalse(gettingValueSucceeded);
            Assert.That(doubleValue, Is.EqualTo(0.0));
        }

        #endregion

        #region Gwsw Import Elements

        [TestCase("")]
        [TestCase(null)]
        public void ImportFile_WithLoadedDefinition_GivingEmptyStringAsPath_LoadsDefinitionFeaturesList(
            string importFilePath)
        {
            string definitionPath = GetFileAndCreateLocalCopy(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            var importer = new GwswFileImporter(new DefinitionsProvider());

            importer.LoadFeatureFiles(Path.GetDirectoryName(definitionPath));
            Assert.IsTrue(importer.GwswAttributesDefinition != null && importer.GwswAttributesDefinition.Any());
        }

        private static IHydroNetwork ImportFromDefinitionFileAndCheckFeatures(string testFilePath)
        {
            var model = new WaterFlowFMModel();
            IHydroNetwork network = model.Network;
            Assert.IsFalse(network.Pipes.Any());
            Assert.IsFalse(network.Manholes.Any());
            Assert.IsFalse(network.SharedCrossSectionDefinitions.Any());
            Assert.IsFalse(network.Pumps.Any());

            var gwswImporter = new GwswFileImporter(new DefinitionsProvider());
            string filePath = GetFileAndCreateLocalCopy(testFilePath);
            gwswImporter.LoadFeatureFiles(Path.GetDirectoryName(filePath));
            try
            {
                //Definition file is 'comma' separated, but the features are 'semicolon', so we need to change the delimeter.
                gwswImporter.CsvDelimeter = ';';
                gwswImporter.ImportItem(null, model);
            }
            catch (Exception e)
            {
                Assert.Fail("While importing an exception was thrown {0}", e.Message);
            }

            Assert.IsTrue(network.Manholes.Any());
            Assert.IsTrue(network.SharedCrossSectionDefinitions.Any());
            Assert.IsTrue(network.Pipes.Any()); //There are some pipes defined within the verbinding.csv
            Assert.IsTrue(network.Pumps.Any()); //There are some pumps defined within the verbinding.csv

            return network;
        }

        [Test]
        public void TestImportFromDefinitionFileCreatesAllSortOfElementsInNetwork()
        {
            ImportFromDefinitionFileAndCheckFeatures(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
        }

        [Test]
        [Category(TestCategory.Performance)]
        [TestCase(@"gwswFiles\GWSW_DidactischStelsel\GWSW.hydx_Definitie_DM.csv", 10000)]
        [TestCase(@"gwswFiles\GWSW_Leiden\GWSW.hydx_Definitie_DM.csv", 210000)]
        public void GivenGwswDatabase_WhenImporting_ShouldBeFasterThan(string testFilePath,
                                                                       float maximumImportingTimeInMs)
        {
            var model = new WaterFlowFMModel();
            IHydroNetwork network = model.Network;
            Assert.IsFalse(network.Pipes.Any());
            Assert.IsFalse(network.Manholes.Any());
            Assert.IsFalse(network.SharedCrossSectionDefinitions.Any());
            Assert.IsFalse(network.Pumps.Any());

            var gwswImporter = new GwswFileImporter(new DefinitionsProvider()) { CsvDelimeter = ';' };
            string filePath = GetFileAndCreateLocalCopy(testFilePath);
            try
            {
                //Definition file is 'comma' separated, but the features are 'semicolon', so we need to change the delimeter.
                TestHelper.AssertIsFasterThan(maximumImportingTimeInMs, () =>
                {
                    gwswImporter.LoadFeatureFiles(Path.GetDirectoryName(filePath));
                    gwswImporter.ImportItem(null, model);
                });
            }
            catch (Exception e)
            {
                Assert.Fail("While importing an exception was thrown {0}", e.Message);
            }

            Assert.IsTrue(network.Manholes.Any());
            Assert.IsTrue(network.SharedCrossSectionDefinitions.Any());
            Assert.IsTrue(network.Pipes.Any()); //There are some pipes defined within the verbinding.csv
            Assert.IsTrue(network.Pumps.Any()); //There are some pumps defined within the verbinding.csv
        }

        [Test]
        public void GivenWswsDatabase_WhenImporting_ThenTheRightAmountOfSewerFeaturesArePresentInTheResultingNetwork()
        {
            var testFilePath = @"gwswFiles\GWSW_DidactischStelsel\GWSW.hydx_Definitie_DM.csv";
            var model = new WaterFlowFMModel();

            var gwswImporter = new GwswFileImporter(new DefinitionsProvider()) { CsvDelimeter = ';' };
            string filePath = GetFileAndCreateLocalCopy(testFilePath);
            try
            {
                gwswImporter.LoadFeatureFiles(Path.GetDirectoryName(filePath));
                gwswImporter.ImportItem(null, model);
            }
            catch (Exception e)
            {
                Assert.Fail("While importing an exception was thrown {0}", e.Message);
            }

            IHydroNetwork network = model.Network;
            Assert.That((object)network.Manholes.Count(), Is.EqualTo(76));
            Assert.That((object)network.OutletCompartments.Count(), Is.EqualTo(4));
            Assert.That((object)network.SewerConnections.Count(), Is.EqualTo(97));
            Assert.That((object)network.SharedCrossSectionDefinitions.Count, Is.EqualTo(42));

            Assert.That((object)network.Pumps.Count(), Is.EqualTo(8));
            Assert.That((object)network.Weirs.Count(), Is.EqualTo(8));
            Assert.That((object)network.Orifices.Count(), Is.EqualTo(2));

            Assert.That(
                (object)network.SewerConnections.Count(sc =>
                                                           sc.BranchFeatures.Count >= 2 && sc.BranchFeatures[1] is IPump), Is.EqualTo(8));
            Assert.That(
                (object)network.SewerConnections.Count(sc =>
                                                           sc.BranchFeatures.Count >= 2 && sc.BranchFeatures[1] is IWeir), Is.EqualTo(8));
            Assert.That(
                (object)network.SewerConnections.Count(sc =>
                                                           sc.BranchFeatures.Count >= 2 && sc.BranchFeatures[1] is IOrifice), Is.EqualTo(2));
        }

        [Test]
        public void TestImportFromDefinitionFileAndCheckGwswUseCaseImportsAllSewerConnectionsCorrectly()
        {
            IHydroNetwork network = ImportFromDefinitionFileAndCheckFeatures(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            Assert.IsNotNull(network);
            Assert.IsNotNull(network.SewerConnections);

            var expectedNumberOfPipes = 81;
            var expectedNumberOfPumps = 8;
            var expectedNumberOfOrifices = 2;
            var expectedNumberOfCrossSections = 42;

            Assert.That(network.Pipes.Count(), Is.EqualTo(expectedNumberOfPipes)
                        , "Not all pipes have been imported correctly.");
            Assert.That(network.Pumps.Count(), Is.EqualTo(expectedNumberOfPumps)
                        , "Not all pumps have been imported correctly.");
            Assert.That(network.Orifices.Count(), Is.EqualTo(expectedNumberOfOrifices)
                        , "Not all orifices have been imported correctly.");
            Assert.That(network.SharedCrossSectionDefinitions.Count, Is.EqualTo(expectedNumberOfCrossSections)
                        , "Not all cross sections have been imported correctly.");
        }

        [Test]
        public void TestImportFromDefinitionFileAndCheckCheckGwswUseCaseImportsAllCompartmentsCorrectly()
        {
            IHydroNetwork network = ImportFromDefinitionFileAndCheckFeatures(@"gwswFiles\GWSW.hydx_Definitie_DM.csv");
            var numberOfManholesInGwsw = 76;
            Assert.IsNotNull(network);

            //CheckManholes
            Assert.IsNotNull(network.Manholes);
            IEnumerable<IManhole> repeatedManholes = network.Manholes.Duplicates();
            Assert.IsEmpty((IEnumerable)repeatedManholes,
                           string.Format("Repeated manhole entries. {0}",
                                         string.Concat((IEnumerable<string>)repeatedManholes.Select(cmp => cmp.Name + " "))));

            List<IManhole> manholesWithoutPlaceholders = network.Manholes.Where(mh => mh.Compartments.Any()).ToList();
            Assert.AreEqual(numberOfManholesInGwsw, (int)manholesWithoutPlaceholders.Count);

            //Check compartments
            List<ICompartment> compartments = manholesWithoutPlaceholders.SelectMany(mh => mh.Compartments).ToList();
            List<ICompartment> repeatedCompartments =
                compartments.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
            Assert.IsEmpty((IEnumerable)repeatedCompartments,
                           string.Format("Repeated compartments entries. {0}",
                                         string.Concat((IEnumerable<string>)repeatedCompartments.Select(cmp => cmp.Name + " "))));

            var numberOfCompartmentsInGwsw = 90;
            Assert.AreEqual(numberOfCompartmentsInGwsw, (int)compartments.Count, "Not all compartments were found.");

            //CheckOutlets
            var numberOfOutlets = 4;
            Assert.AreEqual(numberOfOutlets, (int)compartments.Count(cmp => cmp is OutletCompartment),
                            "Not all outlets were found.");
        }

        [Test]
        public void CreateGwswDataTableFromDefinitionFileThenImportFilesAsDataTables()
        {
            string filePath =
                GetFileAndCreateLocalCopy(
                    @"gwswFiles\GWSW.hydx_Definitie_DM.csv"); //should be the same as the resource file
            string folderPath = Path.GetDirectoryName(filePath);
            // Import Csv Definition.
            var gwswImporter = new GwswFileImporter(new DefinitionsProvider());
            gwswImporter.LoadFeatureFiles(folderPath);
            IEventedList<GwswAttributeType> attributeList = gwswImporter.GwswAttributesDefinition;

            Assert.IsTrue(attributeList.Count > 0, string.Format("Attributes found {0}", attributeList.Count));

            List<string> uniqueFileList = attributeList.GroupBy(i => i.FileName).Select(grp => grp.Key).ToList();
            Assert.AreEqual(uniqueFileList.Count, 12, "Mismatch on found filenames.");

            var csvSettings = new CsvSettings
            {
                Delimiter = csvCommaDelimeter,
                FirstRowIsHeader = true,
                SkipEmptyLines = true
            };

            var importedTables = new List<DataTable>();

            //Read each one of the files.
            foreach (string fileName in uniqueFileList)
            {
                string directoryName = Path.GetDirectoryName(filePath);
                string elementFilePath = Path.Combine(directoryName, fileName);
                Assert.IsTrue(File.Exists(elementFilePath), string.Format("Could not find file {0}", elementFilePath));

                //Import file elements based on their attributes
                List<GwswAttributeType> fileAttributes = attributeList.Where(at => at.FileName.Equals(fileName)).ToList();
                var fileColumnMapping = new Dictionary<CsvRequiredField, CsvColumnInfo>();
                //Create column mapping
                fileAttributes.ForEach(
                    attr =>
                        fileColumnMapping.Add(
                            new CsvRequiredField(attr.Key, attr.AttributeType),
                            new CsvColumnInfo(fileAttributes.IndexOf(attr), CultureInfo.InvariantCulture)));

                var mapping = new CsvMappingData()
                {
                    Settings = csvSettings,
                    FieldToColumnMapping = fileColumnMapping
                };

                DataTable importedElementTable = GwswFileImportAsDataTableWorksCorrectly(elementFilePath, mapping, true);
                importedTables.Add(importedElementTable);
            }

            Assert.AreEqual(uniqueFileList.Count, importedTables.Count,
                            string.Format("Not all files were imported correctly."));
        }

        #endregion

        #region Gwsw Import tests

        [TestCase(@"gwswFiles\BOP.csv")]
        [TestCase(@"gwswFiles\Debiet.csv")]
        [TestCase(@"gwswFiles\GroeneDaken.csv")]
        [TestCase(@"gwswFiles\ItObject.csv")]
        [TestCase(@"gwswFiles\Knooppunt.csv")]
        [TestCase(@"gwswFiles\Kunstwerk.csv")]
        [TestCase(@"gwswFiles\Meta.csv")]
        [TestCase(@"gwswFiles\Nwrw.csv")]
        [TestCase(@"gwswFiles\Oppervlak.csv")]
        [TestCase(@"gwswFiles\Profiel.csv")]
        [TestCase(@"gwswFiles\Verbinding.csv")]
        [TestCase(@"gwswFiles\Verloop.csv")]
        public void ImportGwswCsvFileWithLoadedGwswDefinition(string testCasePath)
        {
            string filePath = GetFileAndCreateLocalCopy(testCasePath);
            string folderPath = Path.GetDirectoryName(filePath);

            var gwswImporter = new GwswFileImporter(new DefinitionsProvider());
            gwswImporter.LoadFeatureFiles(folderPath);

            IList<GwswElement> elementList = GwswFileImportAsGwswElementsWorksCorrectly(gwswImporter, filePath);

            int numberOfLines = File.ReadAllLines(filePath).Length - 1; // we should not include the header
            Assert.AreEqual(numberOfLines, elementList.Count,
                            string.Format("There is a mismatch between expected number of elements and imported."));
            GwswAttributeType elementTypeFound =
                gwswImporter.GwswAttributesDefinition.FirstOrDefault(at =>
                                                                         at.FileName.Equals(Path.GetFileName(testCasePath)));
            if (elementTypeFound == null)
            {
                Assert.Fail("Test failed because no element name was found mapped to this file name.");
            }

            if (numberOfLines != 0)
            {
                int numberOfColumns = File.ReadLines(filePath).First().Split(csvSemiColonDelimeter)
                                          .Where(s => !s.Equals(string.Empty)).ToList().Count;
                foreach (GwswElement element in elementList)
                {
                    Assert.AreEqual(elementTypeFound.ElementName, element.ElementTypeName);
                    Assert.AreEqual(numberOfColumns, element.GwswAttributeList.Count,
                                    string.Format("There is a mismatch between expected and imported attributes for element {0}",
                                                  element.ElementTypeName));
                }
            }
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void TestImportSewerConnectionFromFileAssignsNodesWhenTheyExist()
        {
            string filePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            string folderPath = Path.GetDirectoryName(filePath);
            var model = new WaterFlowFMModel();
            IHydroNetwork network = model.Network;

            #region Create network

            /*We know these two nodes are referred in the test data*/
            var sourceManholeName = "man001";
            var sourceCompartmentName = "put9";
            var sourceManhole = new Manhole(sourceManholeName);
            var sourceCompartment = new Compartment(sourceCompartmentName);
            sourceManhole.Compartments.Add(sourceCompartment);
            network.Nodes.Add(sourceManhole);

            var targetManholeName = "man001";
            var targetCompartmentName = "put8";
            var targetManhole = new Manhole(targetManholeName);
            var targetCompartment = new Compartment(targetCompartmentName);
            targetManhole.Compartments.Add(targetCompartment);
            network.Nodes.Add(targetManhole);

            #endregion

            var gwswImporter = new GwswFileImporter(new DefinitionsProvider()) { CsvDelimeter = ';' };
            gwswImporter.LoadFeatureFiles(folderPath);
            gwswImporter.ImportItem(filePath, model);
            Assert.IsTrue(network.SewerConnections.Any(p =>
                                                           p.Target != null && p.Source != null && p.Source.Name.Equals(sourceManhole.Name, StringComparison.InvariantCultureIgnoreCase) && p.Target.Name.Equals(targetManhole.Name, StringComparison.InvariantCulture)));
        }

        [Test]
        public void TestImportOutletsFromStructuresThenImportNodesAssignsStructuresValues()
        {
            //Create network
            var model = new WaterFlowFMModel();
            IHydroNetwork network = model.Network;

            //Load structures.
            var gwswImporter = new GwswFileImporter(new DefinitionsProvider());
            string structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");
            string folderPath = Path.GetDirectoryName(structuresPath);

            gwswImporter.CsvDelimeter = ';';
            gwswImporter.LoadFeatureFiles(folderPath);
            gwswImporter.ImportItem(structuresPath, model);

            //Check placeholders have been created.
            Assert.IsTrue(network.Manholes.Any());

            List<ICompartment> outletCompartments = network.Manholes
                                                           .SelectMany(mh => mh.Compartments.Where(cmp => cmp is OutletCompartment)).ToList();
            Assert.IsTrue(outletCompartments.Any());

            // Now Load connections.
            string compartmentsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            gwswImporter.ImportItem(compartmentsPath, model);

            foreach (ICompartment compartment in outletCompartments)
            {
                var outlet = compartment as OutletCompartment;
                Assert.IsNotNull(outlet);

                IManhole manhole = network.Manholes.FirstOrDefault(m => m.ContainsCompartmentWithName(outlet.Name));
                Assert.IsNotNull(manhole);
                var extendedOutlet = manhole.GetCompartmentByName(outlet.Name) as OutletCompartment;
                Assert.IsNotNull(extendedOutlet);

                Assert.That(extendedOutlet.SurfaceWaterLevel, Is.EqualTo(outlet.SurfaceWaterLevel),
                            $"the SurfaceWaterLevel of compartment {compartment.Name} was changed after importing the connection.");
            }
        }

        [Test]
        public void WhenImportingSewerProfilesToNetworkAndThenImportingSewerConnectionsToNetwork_ThenSewerConnectionsHaveSewerProfiles()
        {
            //Create network
            var model = new WaterFlowFMModel();
            IHydroNetwork network = model.Network;

            var gwswImporter = new GwswFileImporter(new DefinitionsProvider());
            //Load sewer profiles
            gwswImporter.CsvDelimeter = ';';
            string sewerProfilesPath = GetFileAndCreateLocalCopy(@"gwswFiles\Profiel.csv");
            string folderPath = Path.GetDirectoryName(sewerProfilesPath);
            gwswImporter.LoadFeatureFiles(folderPath);
            gwswImporter.ImportItem(sewerProfilesPath, model);

            //Check that sewer profiles have been put into the network
            int numberOfLinesInFile = File.ReadAllLines(sewerProfilesPath).Length - 1;
            Assert.That((object)network.SharedCrossSectionDefinitions.Count, Is.EqualTo(numberOfLinesInFile));

            // Now Load connections.
            string connectionsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            string nodesPath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            gwswImporter.FilesToImport = new List<string>(new[] { nodesPath, connectionsPath });

            gwswImporter.ImportItem(null, model);

            List<Pipe> pipes = network.Branches.OfType<Pipe>().ToList();
            Assert.IsTrue(pipes.Any());
            pipes.ForEach(p => Assert.NotNull(p.CrossSection?.Definition));

            // Check for each pipe that its CrossSectionDefinition is equal to one of the sewer profiles in
            // the SharedCrossSectionDefinitions of the network
            pipes.ForEach(p =>
            {
                CrossSectionDefinitionStandard pipeCsDefinition = p.Profile;
                ICrossSectionDefinition sharedCsDefinition =
                    network.SharedCrossSectionDefinitions.FirstOrDefault(csd => csd.Name == pipeCsDefinition.Name);
                Assert.NotNull(sharedCsDefinition);
                Assert.That((object)pipeCsDefinition.Width, Is.EqualTo(sharedCsDefinition.Width));

                ICrossSectionStandardShape pipeShape = pipeCsDefinition.Shape;
                ICrossSectionStandardShape sharedCsShape = ((CrossSectionDefinitionStandard)sharedCsDefinition).Shape;
                Assert.That((object)pipeShape.Type, Is.EqualTo(sharedCsShape.Type));

                var pipeWidthHeightShape = pipeShape as CrossSectionStandardShapeWidthHeightBase;
                var sharedWidthHeightShape = sharedCsShape as CrossSectionStandardShapeWidthHeightBase;
                if (pipeWidthHeightShape != null && sharedWidthHeightShape != null)
                {
                    Assert.That((object)pipeWidthHeightShape.Height, Is.EqualTo(sharedWidthHeightShape.Height));
                }
            });
        }

        [Test]
        public void WhenImportingSewerConnectionsToNetworkAndThenImportingSewerProfilesToNetwork_ThenSewerConnectionsHaveTheCorrectSewerProfiles()
        {
            const string csdName = "PRO2";

            var model = new WaterFlowFMModel();
            IHydroNetwork network = model.Network;

            //Load connections and nodes
            string nodesFilePath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            string connectionsFilePath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");

            var gwswImporter = new GwswFileImporter(new DefinitionsProvider()) { CsvDelimeter = ';' };
            gwswImporter.FilesToImport.Add(nodesFilePath);
            gwswImporter.FilesToImport.Add(connectionsFilePath);

            string folderPath = Path.GetDirectoryName(connectionsFilePath);
            gwswImporter.LoadFeatureFiles(folderPath);
            gwswImporter.ImportItem(null, model);

            // Check the sewer profiles in the network
            IPipe sewerProfileShapeBefore = network.Pipes.FirstOrDefault(p => p.CrossSectionDefinitionName == csdName);
            Assert.IsNotNull(sewerProfileShapeBefore);

            // Load sewer profiles
            string sewerProfilesPath = GetFileAndCreateLocalCopy(@"gwswFiles\Profiel.csv");
            gwswImporter.ImportItem(sewerProfilesPath, model);

            //Check that sewer profiles have been put into the network
            int numberOfLinesInFile = File.ReadAllLines(sewerProfilesPath).Length;
            Assert.That((object)network.SharedCrossSectionDefinitions.Count, Is.EqualTo(numberOfLinesInFile));

            // Check the sewer profiles in the network
            var sewerProfileShapeAfter = (CrossSectionStandardShapeWidthHeightBase)network.Pipes.Select(p => p.Profile)
                                                                                          .FirstOrDefault(d => d.Name == csdName)?.Shape;
            Assert.IsNotNull(sewerProfileShapeAfter);
        }

        [Test]
        public void TestImportOrificesFromStructuresThenImportOrificesAssignsStructuresValues()
        {
            //Create network
            var model = new WaterFlowFMModel();
            IHydroNetwork network = model.Network;
            var gwswImporter = new GwswFileImporter(new DefinitionsProvider());

            //Load structures.
            string nodesPath = GetFileAndCreateLocalCopy(@"gwswFiles\Knooppunt.csv");
            string connectionPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            string structuresPath = GetFileAndCreateLocalCopy(@"gwswFiles\Kunstwerk.csv");

            string folderPath = Path.GetDirectoryName(structuresPath);
            gwswImporter.LoadFeatureFiles(folderPath);

            gwswImporter.FilesToImport.Add(nodesPath);
            gwswImporter.FilesToImport.Add(connectionPath);
            gwswImporter.FilesToImport.Add(structuresPath);

            gwswImporter.ImportItem(null, model);

            //Check placeholders have been created.
            Assert.IsTrue(network.Branches.Any());

            List<Orifice> orificeStructures = network.SewerConnections
                                                     .SelectMany(sc => sc.GetStructuresFromBranchFeatures<Orifice>()).ToList();
            Assert.IsTrue(orificeStructures.Any());

            // Now Load connections.
            string compartmentsPath = GetFileAndCreateLocalCopy(@"gwswFiles\Verbinding.csv");
            gwswImporter.ImportItem(compartmentsPath, model);

            foreach (Orifice orifice in orificeStructures)
            {
                Orifice extendedOrifice = network.SewerConnections
                                                 .SelectMany(sc => sc.GetStructuresFromBranchFeatures<Orifice>())
                                                 .FirstOrDefault(o => o.Name.Equals(orifice.Name));
                Assert.IsNotNull(extendedOrifice);

                Assert.AreEqual((object)orifice.CrestLevel, extendedOrifice.CrestLevel,
                                "the attributes from the element do not match");
            }
        }

        [Test]
        public void Given_EmptyFlowFmModel_When_ImportingGwswDirectoryForTheFirstTime_Then_GwswAttributesDefinitionIsFilled()
        {
            string originalDirectoryPath = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_DidactischStelsel");
            string testDirectoryPath = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDirectoryPath, testDirectoryPath);

            //Within the Deltares folder the previous chosen File path is saved, by deleting this folder, the Gwsw import dialog starts without a Gwsw File path.
            string deltaresDirectory =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Deltares");
            FileUtils.DeleteIfExists(deltaresDirectory);

            var gwswFileImporter = new GwswFileImporter(new DefinitionsProvider());
            var viewModel = new GwswImportControlViewModel { Importer = gwswFileImporter };
            Assert.IsNotNull(viewModel);
            Assert.IsFalse(viewModel.GwswFeatureFiles.Any());

            gwswFileImporter.LoadFeatureFiles(null);

            viewModel.SelectedDirectoryPath = testDirectoryPath;
            viewModel.OnDirectorySelected.Execute(null);

            Assert.IsTrue(viewModel.GwswFeatureFiles.Any());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenGwswModelThatEndsWithManholeConnectedViaSewerConnection_WhenImporting_CreatesLateralAndConnectsToCatchment()
        {
            // Setup
            string originalDirectoryPath = TestHelper.GetTestFilePath(@"gwswFiles\ModelEndingWithSewerConnection");

            using (var tempDir = new TemporaryDirectory())
            {
                string tempDirPath = tempDir.Path;
                FileUtils.CopyDirectory(originalDirectoryPath, tempDirPath);

                var filesToImport = new List<string>()
                {
                    Path.Combine(tempDirPath, "Knooppunt.csv"),
                    Path.Combine(tempDirPath, "Kunstwerk.csv"),
                    Path.Combine(tempDirPath, "Nwrw.csv"),
                    Path.Combine(tempDirPath, "Oppervlak.csv"),
                    Path.Combine(tempDirPath, "Profiel.csv"),
                    Path.Combine(tempDirPath, "Verbinding.csv")
                };
                
                GwswFileImporter gwswImporter = SetupGwswFileImporter(filesToImport, tempDirPath);

                // Call
                object importedModel = gwswImporter.ImportItem(null, null);
                
                // Assert
                Assert.That(importedModel, Is.InstanceOf<HydroModel>());
                AssertThatLinkIsCreatedBetweenNwrwCatchmentAndLateral((HydroModel)importedModel);
            }
        }

        private static void AssertThatLinkIsCreatedBetweenNwrwCatchmentAndLateral(HydroModel hydroModel)
        {
            WaterFlowFMModel fmModel = hydroModel.Models.OfType<WaterFlowFMModel>().FirstOrDefault();
            Assert.That(fmModel, Is.Not.Null);

            List<ILateralSource> laterals = fmModel.Network.LateralSources.ToList();
            Assert.That(laterals.Count, Is.EqualTo(1));

            ILateralSource lateral = laterals.First();
            Assert.That(lateral.Links.Count, Is.EqualTo(1));

            HydroLink link = lateral.Links.First();
            Assert.That(link.Target, Is.SameAs(lateral));

            List<Model1DLateralSourceData> lateralDatas = fmModel.LateralSourcesData.ToList();
            Assert.That(lateralDatas.Count, Is.EqualTo(1));
            Model1DLateralSourceData lateralData = lateralDatas.First();
            Assert.That(lateralData.Feature, Is.SameAs(lateral));
            const string expectedCompartmentName = "0WADI1a";
            Assert.That(lateralData.Compartment.Name, Is.EqualTo(expectedCompartmentName));

            RainfallRunoffModel rrModel = hydroModel.Models.OfType<RainfallRunoffModel>().FirstOrDefault();
            Assert.That(rrModel, Is.Not.Null);

            List<NwrwData> nwrwDatas = rrModel.ModelData.OfType<NwrwData>().ToList();
            Assert.That(nwrwDatas.Count, Is.EqualTo(1));

            NwrwData nwrwData = nwrwDatas.First();
            Assert.That(link.Source, Is.SameAs(nwrwData.Catchment));
        }
        #endregion
    }
}