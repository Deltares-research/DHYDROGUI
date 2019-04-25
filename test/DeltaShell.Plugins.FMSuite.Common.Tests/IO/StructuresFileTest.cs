using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using StructureType = DeltaShell.Plugins.FMSuite.Common.IO.StructureType;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructuresFileTest
    {
        private const string ExpectedCrestLevelValue = "    CrestLevel            = 10                  # Weir crest height (in [m])";
        private const string ExpectedCrestLevelTimeSeries = "    CrestLevel            = TestStructure_CrestLevel.tim# Weir crest height (in [m])";
        private const string ExpectedSillLevelValue = "    CrestLevel            = 10                  # Gate sill level (in [m])";
        private const string ExpectedSillLevelTimeSeries = "    CrestLevel            = TestStructure_CrestLevel.tim# Gate sill level (in [m])";
        private const string ExpectedLowerEdgeLevelValueGatedFormula = "    GateLowerEdgeLevel    = 40                  # Gate lower edge level (in [m])";
        private const string ExpectedLowerEdgeLevelTimeSeriesGatedFormula = "    GateLowerEdgeLevel    = TestStructure_GateLowerEdgeLevel.tim# Gate lower edge level (in [m])";
        private const string ExpectedDoorOpeningValueGatedFormula = "    GateOpeningWidth      = 30                  # Gate opening width (in [m])";
        private const string ExpectedDoorOpeningTimeSeriesGatedFormula = "    GateOpeningWidth      = TestStructure_GateOpeningWidth.tim# Gate opening width (in [m])";
        private const string ExpectedGSCrestLevelValue = "    CrestLevel            = 10                  # Bed level at centre of structure (m AD)";
        private const string ExpectedGSCrestLevelTimeSeries = "    CrestLevel            = TestStructure_CrestLevel.tim# Bed level at centre of structure (m AD)";
        private const string ExpectedLowerEdgeLevelValueGeneralStructureFormula = "    GateLowerEdgeLevel    = 40                  # Gate lower edge level (m AD)";
        private const string ExpectedLowerEdgeLevelTimeSeriesGeneralStructureFormula = "    GateLowerEdgeLevel    = TestStructure_GateLowerEdgeLevel.tim# Gate lower edge level (m AD)";
        private const string ExpectedDoorOpeningValueGeneralStructureFormula = "    GateOpeningWidth      = 30                  # Horizontal opening width between the doors (m)";
        private const string ExpectedDoorOpeningTimeSeriesGeneralStructureFormula = "    GateOpeningWidth      = TestStructure_GateOpeningWidth.tim# Horizontal opening width between the doors (m)";
        private StructureSchema<ModelPropertyDefinition> schema;

        [SetUp]
        public void Setup()
        {
            schema = new StructureSchemaCsvFile().ReadStructureSchema(StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresUsingExampleFile()
        {
            var path = TestHelper.GetTestFilePath(@"structures\example-structures.imp");

            var structureFile = new StructuresFile
            {
                StructureSchema = schema
            };
            var structures = structureFile.ReadStructures2D(path).ToList();

            Assert.AreEqual(7, structures.Count);
            Assert.AreEqual(2, structures.Count(s => s.StructureType == StructureType.Weir));
            Assert.AreEqual(3, structures.Count(s => s.StructureType == StructureType.Pump));
            Assert.AreEqual(1, structures.Count(s => s.StructureType == StructureType.Gate));
            Assert.AreEqual(1, structures.Count(s => s.StructureType == StructureType.GeneralStructure));

            var weirDown = structures.First(s => s.Name == "Weir_down");
            Assert.AreEqual(6, weirDown.Properties.Count);
            Assert.AreEqual("680", weirDown.GetProperty(KnownStructureProperties.X).GetValueAsString());
            Assert.AreEqual("360", weirDown.GetProperty(KnownStructureProperties.Y).GetValueAsString());
            Assert.AreEqual("2", weirDown.GetProperty(KnownStructureProperties.CrestLevel).GetValueAsString());
            Assert.AreEqual("1", weirDown.GetProperty(KnownStructureProperties.LateralContractionCoefficient).GetValueAsString());

            var generalStructure = structures.First(s => s.Name == "gs_01");
            Assert.That(generalStructure.Properties.Count, Is.EqualTo(4));
            Assert.That(generalStructure.GetProperty(KnownStructureProperties.PolylineFile).GetValueAsString(), Is.EqualTo("gs_01.pli"));
            Assert.That(generalStructure.GetProperty(KnownGeneralStructureProperties.CrestWidth).GetValueAsString(), Is.EqualTo("2.3"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresInvalidFileFormat()
        {
            var path = TestHelper.GetTestFilePath(@"structures\invalidFormat.imp");
            var structuresFile = new StructuresFile
            {
                StructureSchema = schema
            };
            var structures = structuresFile.ReadStructures2D(path);
            Assert.AreEqual(0, structures.Count(), "Nothing should have been read.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresWithUnsupportedCategories()
        {
            // [test]
            // dummy			= value		#I'm just a dumy value
            // 
            // [structure]
            // type             = weir
            // id               = w
            // x                = 1
            // y                = 2
            // crest_level      = 3
            // crest_width      = 4
            // discharge_coeff  = 1
            // lat_dis_coeff    = 1
            // allowed_flow_dir = 0
            var path = TestHelper.GetTestFilePath(@"structures\mixedFile.imp");
            var schema =
                new StructureSchemaCsvFile().ReadStructureSchema(
                    StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);

            IList<Structure2D> structures = null;
            var structuresFile = new StructuresFile
            {
                StructureSchema = schema
            };
            TestHelper.AssertLogMessageIsGenerated(
                () => structures = structuresFile.ReadStructures2D(path).ToList(),
                "Category [test] not supported for structures and is skipped.");
            Assert.AreEqual(1, structures.Count, "Only one structure category in file.");

            var w = structures[0];
            Assert.AreEqual("w", w.GetProperty(KnownStructureProperties.Name).GetValueAsString());
            Assert.IsNull(w.GetProperty("dummy"), "Should not accidentally take key from [test] category.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresWithKeysNotInSchema()
        {
            // [structure]
            // type             = weir
            // id               = w
            // x                = 1
            // y                = 2
            // crest_level      = 3
            // crest_width      = 4
            // Im_a_nonexistent_property = hax      # This property is not in the schema!
            // discharge_coeff  = 1
            // lat_dis_coeff    = 1
            // allowed_flow_dir = 0
            var path = TestHelper.GetTestFilePath(@"structures\keyNotInSchema.imp");
            var schema =
                new StructureSchemaCsvFile().ReadStructureSchema(
                    StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);

            IList<Structure2D> structures = null;
            var structuresFile = new StructuresFile
            {
                StructureSchema = schema
            };
            TestHelper.AssertLogMessageIsGenerated(
                () => structures = structuresFile.ReadStructures2D(path).ToList(),
                String.Format(
                    "Property 'Im_a_nonexistent_property' not supported for structures of type 'weir' and is skipped. (Line 8 of file {0})",
                    path));
            Assert.AreEqual(1, structures.Count, "Only one structure category in file.");

            var w = structures[0];
            Assert.AreEqual("w", w.GetProperty(KnownStructureProperties.Name).GetValueAsString());
            Assert.IsNull(w.GetProperty("Im_a_nonexistent_property"), "Should not add keys outside of schema.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresWithMissingTypeProperty()
        {
            // [structure]
            // id               = w
            // x                = 1
            // y                = 2
            // crest_level      = 3
            // crest_width      = 4
            // discharge_coeff  = 1
            // lat_dis_coeff    = 1
            // allowed_flow_dir = 0
            var path = TestHelper.GetTestFilePath(@"structures\missingTypeProperty.imp");

            IList<Structure2D> structures = null;
            var structuresFile = new StructuresFile
            {
                StructureSchema = schema
            };
            TestHelper.AssertLogMessageIsGenerated(() => structures = structuresFile.ReadStructures2D(path).ToList(),
                                                   "Obligated property 'type' expected but is missing; Structure is skipped.");
            Assert.AreEqual(0, structures.Count, "No valid structures in file.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteStructures()
        {
            var exportFilePath = TestHelper.GetCurrentMethodName() + ".imp";
            if (File.Exists(exportFilePath)) File.Delete(exportFilePath);

            var weir = new Structure2D("weir");
            weir.AddProperty(KnownStructureProperties.Type, typeof(string), "weir");
            weir.AddProperty(KnownStructureProperties.Name, typeof(string), "Weir_down");
            weir.AddProperty(KnownStructureProperties.X, typeof(double), "680");
            weir.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            weir.AddProperty(KnownStructureProperties.CrestLevel, typeof(double), "2");
            weir.AddProperty(KnownStructureProperties.LateralContractionCoefficient, typeof(double), "1");

            var structures = new[]
            {
                weir
            };

            StructuresFile.WriteStructures2D(exportFilePath, structures);

            var fileContents = File.ReadAllText(exportFilePath);
            Assert.AreEqual(
                "[structure]" + Environment.NewLine +
                "    type                  = weir                " + Environment.NewLine +
                "    id                    = Weir_down           " + Environment.NewLine +
                "    x                     = 680                 " + Environment.NewLine +
                "    y                     = 360                 " + Environment.NewLine +
                "    CrestLevel            = 2                   " + Environment.NewLine +
                "    lat_contr_coeff       = 1                   " + Environment.NewLine, fileContents);

            File.Delete(exportFilePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CanRepeatedlyReadAndWrite()
        {
            var path = TestHelper.GetTestFilePath(@"structures\example-structures.imp");

            var structureFile = new StructuresFile
            {
                StructureSchema = schema
            };
            var structures = structureFile.ReadStructures2D(path).ToList();

            var exportFilePath = TestHelper.GetCurrentMethodName() + ".imp";
            var newStructuresFile = new StructuresFile
            {
                StructureSchema = schema
            };
            StructuresFile.WriteStructures2D(exportFilePath, structures);

            CompareStructureIniFiles(path, exportFilePath); // Note: Comments in user file can differ from schema!

            var newStructures = newStructuresFile.ReadStructures2D(exportFilePath).ToList();

            CompareStructures(structures, newStructures);

            File.Delete(exportFilePath);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenGeneralStructureWhenWritingToFileAndReadingFromThatFileThenResultingStructuresAreTheSame()
        {
            var iniFilePath = TestHelper.GetTestFilePath(@"structures\temp_file.ini");
            var pliFilePath = TestHelper.GetTestFilePath(@"structures\gs01.pli");
            if (File.Exists(iniFilePath)) File.Delete(iniFilePath);
            if (File.Exists(pliFilePath)) File.Delete(pliFilePath);

            var initialFormula = new GeneralStructureWeirFormula
            {
                PositiveFreeGateFlow = 1,
                PositiveDrownedGateFlow = 2,
                PositiveFreeWeirFlow = 3,
                PositiveDrownedWeirFlow = 4,
                PositiveContractionCoefficient = 5,
                NegativeFreeGateFlow = 6,
                NegativeDrownedGateFlow = 7,
                NegativeFreeWeirFlow = 8,
                NegativeDrownedWeirFlow = 9,
                NegativeContractionCoefficient = 10,
                WidthLeftSideOfStructure = 11,
                WidthStructureLeftSide = 12,
                WidthStructureCentre = 13,
                WidthStructureRightSide = 14,
                WidthRightSideOfStructure = 15,
                BedLevelLeftSideOfStructure = 16,
                BedLevelLeftSideStructure = 17,
                BedLevelStructureCentre = 18,
                BedLevelRightSideStructure = 19,
                BedLevelRightSideOfStructure = 20,
                UseExtraResistance = true,
                ExtraResistance = 40,
                DoorHeight = 50,
                UseHorizontalDoorOpeningWidthTimeSeries = false,
                HorizontalDoorOpeningWidth = 60,
                UseLowerEdgeLevelTimeSeries = false,
                LowerEdgeLevel = 70
            };
            var initialGeneralStructure = new Weir("gs01", true)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(4, 5),
                    new Coordinate(6, 7)
                }),
                WeirFormula = initialFormula
            };
            var structures = new IStructure1D[]
            {
                initialGeneralStructure
            };
            var structuresFile = new StructuresFile
            {
                StructureSchema = schema, ReferenceDate = new DateTime(2013, 1, 1, 0, 0, 0)
            };

            structuresFile.Write(iniFilePath, structures);
            var readResult = structuresFile.Read(iniFilePath);
            Assert.That(readResult.Count, Is.EqualTo(1));

            var resultingGeneralStructure = readResult.FirstOrDefault() as Weir;
            Assert.IsNotNull(resultingGeneralStructure);
            var resultingFormula = resultingGeneralStructure.WeirFormula as GeneralStructureWeirFormula;
            Assert.IsNotNull(resultingFormula);

            var weirPropertiesAreEqual = initialGeneralStructure.CanBeTimedependent == resultingGeneralStructure.CanBeTimedependent &&
                                         initialGeneralStructure.IsGated == resultingGeneralStructure.IsGated &&
                                         initialGeneralStructure.AllowNegativeFlow == resultingGeneralStructure.AllowNegativeFlow &&
                                         initialGeneralStructure.AllowPositiveFlow == resultingGeneralStructure.AllowPositiveFlow &&
                                         initialGeneralStructure.IsRectangle == resultingGeneralStructure.IsRectangle &&
                                         initialGeneralStructure.SpecifyCrestLevelAndWidthOnWeir == resultingGeneralStructure.SpecifyCrestLevelAndWidthOnWeir &&
                                         initialGeneralStructure.FormulaName == resultingGeneralStructure.FormulaName;

            var formulasAreEqual = initialFormula.Name == resultingFormula.Name &&
                                   initialFormula.PositiveContractionCoefficient == resultingFormula.PositiveContractionCoefficient &&
                                   initialFormula.PositiveContractionCoefficient == resultingFormula.PositiveContractionCoefficient &&
                                   initialFormula.PositiveDrownedGateFlow == resultingFormula.PositiveDrownedGateFlow &&
                                   initialFormula.PositiveDrownedWeirFlow == resultingFormula.PositiveDrownedWeirFlow &&
                                   initialFormula.PositiveFreeWeirFlow == resultingFormula.PositiveFreeWeirFlow &&
                                   initialFormula.NegativeContractionCoefficient == resultingFormula.NegativeContractionCoefficient &&
                                   initialFormula.NegativeDrownedGateFlow == resultingFormula.NegativeDrownedGateFlow &&
                                   initialFormula.NegativeDrownedWeirFlow == resultingFormula.NegativeDrownedWeirFlow &&
                                   initialFormula.NegativeFreeGateFlow == resultingFormula.NegativeFreeGateFlow &&
                                   initialFormula.NegativeFreeWeirFlow == resultingFormula.NegativeFreeWeirFlow &&
                                   initialFormula.BedLevelLeftSideOfStructure == resultingFormula.BedLevelLeftSideOfStructure &&
                                   initialFormula.BedLevelLeftSideStructure == resultingFormula.BedLevelLeftSideStructure &&
                                   initialFormula.BedLevelStructureCentre == resultingFormula.BedLevelStructureCentre &&
                                   initialFormula.BedLevelRightSideStructure == resultingFormula.BedLevelRightSideStructure &&
                                   initialFormula.BedLevelRightSideOfStructure == resultingFormula.BedLevelRightSideOfStructure &&
                                   initialFormula.WidthLeftSideOfStructure == resultingFormula.WidthLeftSideOfStructure &&
                                   initialFormula.WidthStructureLeftSide == resultingFormula.WidthStructureLeftSide &&
                                   initialFormula.WidthStructureCentre == resultingFormula.WidthStructureCentre &&
                                   initialFormula.WidthStructureRightSide == resultingFormula.WidthStructureRightSide &&
                                   initialFormula.WidthRightSideOfStructure == resultingFormula.WidthRightSideOfStructure &&
                                   initialFormula.UseExtraResistance == resultingFormula.UseExtraResistance &&
                                   initialFormula.ExtraResistance == resultingFormula.ExtraResistance &&
                                   initialFormula.DoorHeight == resultingFormula.DoorHeight &&
                                   initialFormula.UseHorizontalDoorOpeningWidthTimeSeries == resultingFormula.UseHorizontalDoorOpeningWidthTimeSeries &&
                                   initialFormula.HorizontalDoorOpeningWidth == resultingFormula.HorizontalDoorOpeningWidth &&
                                   initialFormula.UseLowerEdgeLevelTimeSeries == resultingFormula.UseLowerEdgeLevelTimeSeries &&
                                   initialFormula.LowerEdgeLevel == resultingFormula.LowerEdgeLevel &&
                                   initialFormula.GateOpening == resultingFormula.GateOpening;

            Assert.IsTrue(weirPropertiesAreEqual && formulasAreEqual);

            //cleanup
            if (File.Exists(iniFilePath)) File.Delete(iniFilePath);
            if (File.Exists(pliFilePath)) File.Delete(pliFilePath);
        }

        [Test]
        public void ReadThrowsForInvalidFilePath()
        {
            var structureFile = new StructuresFile
            {
                StructureSchema = new StructureSchema<ModelPropertyDefinition>()
            };
            Assert.Throws<FileNotFoundException>(() => structureFile.ReadStructures2D("I do not exist").ToList());
        }

        [Test]
        public void GivenAStructuresFileAndAValidFilePathAndASetOfStructuresContainingOnlyAPump2DWhenWriteIsCalledOnStructuresFileWithTheFilePathAndStructuresThenTheCorrectFileIsWritten()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Given
                var structuresFile = new StructuresFile
                {
                    StructureSchema = schema
                };
                var writePath = Path.Combine(tempDir, "structures.ini");

                var pump = new Pump2D("TestStructure", true)
                {
                    Capacity = 20.0
                };

                var structures = new[]
                {
                    pump
                };

                // When
                structuresFile.Write(writePath, structures);

                // Then
                var expectedFileContents =
                    "[structure]" + Environment.NewLine +
                    "    type                  = pump                # Type of structure" + Environment.NewLine +
                    "    id                    = TestStructure       # Name of the structure" + Environment.NewLine +
                    "    capacity              = 20                  # Pump capacity (in [m3/s])" + Environment.NewLine;
                var fileContents = File.ReadAllText(writePath);

                Assert.That(fileContents, Is.EqualTo(expectedFileContents));
            });
        }

        [Test]
        public void GivenAStructuresFileWithOldVariableNames_WhenReadingAndWriting_ThenPropertiesAreRenamed()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDirectory =>
            {
                // Given
                var structuresFilePath = CreateStructuresFileWithPliFiles(
                    tempDirectory,
                    CreateWeirDelftIniCategory(),
                    CreateGateDelftIniCategory(),
                    CreateGeneralStructureDelftIniCategory());

                var structuresFile = new StructuresFile
                {
                    StructureSchema = schema
                };

                // When
                var structures = structuresFile.Read(structuresFilePath);
                structuresFile.Write(structuresFilePath, structures);

                // Then
                ValidateWrittenStructuresFile(structuresFilePath);
            });
        }

        [Test]
        public void GivenAStructuresFileWithASimpleWeir_WhenReading_CorrectStructureIsCreated()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDirectory =>
            {
                // Given
                var weirCategory = CreateWeirDelftIniCategory();

                var structuresFilePath = CreateStructuresFileWithPliFiles(
                    tempDirectory,
                    weirCategory);

                var structuresFile = new StructuresFile
                {
                    StructureSchema = schema
                };

                // When
                var structures = structuresFile.Read(structuresFilePath);

                // Then
                Weir2D weir = ValidatedWeir(structures);
                ValidateCommonWeirProperties(weir, "simple_weir");
            });
        }

        [Test]
        public void GivenAStructuresFileWithAGatedWeir_WhenReading_CorrectStructureIsCreated()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDirectory =>
            {
                // Given
                var gateCategory = CreateGateDelftIniCategory();
                var structuresFilePath = CreateStructuresFileWithPliFiles(
                    tempDirectory,
                    gateCategory);

                var structuresFile = new StructuresFile
                {
                    StructureSchema = new WaterFlowFMModelDefinition().StructureSchema
                };

                // When
                var structures = structuresFile.Read(structuresFilePath);

                // Then
                Weir2D weir = ValidatedWeir(structures);
                ValidateGatedWeirFormulaProperties(weir, "gated_weir");
            });
        }

        [Test]
        public void GivenAStructuresFileWithAGeneralStructure_WhenReading_CorrectStructureIsCreated()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDirectory =>
            {
                // Given
                var generalStructureCategory = CreateGeneralStructureDelftIniCategory();
                var structuresFilePath = CreateStructuresFileWithPliFiles(
                    tempDirectory,
                    generalStructureCategory);

                var structuresFile = new StructuresFile
                {
                    StructureSchema = new WaterFlowFMModelDefinition().StructureSchema
                };

                // When
                var structures = structuresFile.Read(structuresFilePath);

                // Then
                Weir2D weir = ValidatedWeir(structures);
                ValidateGeneralWeirFormulaProperties(weir, "general_structure");
            });
        }

        // Tests added in relation to DELFT3DFM
        [TestCase(false, ExpectedCrestLevelValue)]
        [TestCase(true, ExpectedCrestLevelTimeSeries)]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileAndAValidFilePathAndASetOfStructuresContainingOnlyASimpleWeirWhenWriteIsCalledOnStructuresFileWithTheFilePathAndStructuresThenTheCorrectFileIsWritten(bool useCrestLevelTimeSeries, string expectedCrestLevelVal)
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Given
                var structuresFile = new StructuresFile
                {
                    StructureSchema = schema
                };
                var writePath = Path.Combine(tempDir, "structures.ini");

                var weir = new Weir2D("TestStructure", true)
                {
                    WeirFormula = new SimpleWeirFormula(), CrestWidth = 20.0, CrestLevel = 10.0, UseCrestLevelTimeSeries = useCrestLevelTimeSeries
                };

                var structures = new[]
                {
                    weir
                };

                // When
                structuresFile.Write(writePath, structures);

                // Then
                var expectedFileContents =
                    "[structure]" + Environment.NewLine +
                    "    type                  = weir                # Type of structure" + Environment.NewLine +
                    "    id                    = TestStructure       # Name of the structure" + Environment.NewLine +
                    expectedCrestLevelVal + Environment.NewLine +
                    "    CrestWidth            = 20                  # Weir crest width (in [m])" + Environment.NewLine +
                    "    lat_contr_coeff       = 1                   # Lateral contraction coefficient" + Environment.NewLine;

                var fileContents = File.ReadAllText(writePath);

                Assert.That(fileContents, Is.EqualTo(expectedFileContents));
            });
        }

        [TestCase(false, ExpectedSillLevelValue,
            false, ExpectedLowerEdgeLevelValueGatedFormula,
            false, ExpectedDoorOpeningValueGatedFormula)]
        [TestCase(false, ExpectedSillLevelValue,
            false, ExpectedLowerEdgeLevelValueGatedFormula,
            true, ExpectedDoorOpeningTimeSeriesGatedFormula)]
        [TestCase(false, ExpectedSillLevelValue,
            true, ExpectedLowerEdgeLevelTimeSeriesGatedFormula,
            false, ExpectedDoorOpeningValueGatedFormula)]
        [TestCase(false, ExpectedSillLevelValue,
            true, ExpectedLowerEdgeLevelTimeSeriesGatedFormula,
            true, ExpectedDoorOpeningTimeSeriesGatedFormula)]
        [TestCase(true, ExpectedSillLevelTimeSeries,
            false, ExpectedLowerEdgeLevelValueGatedFormula,
            false, ExpectedDoorOpeningValueGatedFormula)]
        [TestCase(true, ExpectedSillLevelTimeSeries,
            false, ExpectedLowerEdgeLevelValueGatedFormula,
            true, ExpectedDoorOpeningTimeSeriesGatedFormula)]
        [TestCase(true, ExpectedSillLevelTimeSeries,
            true, ExpectedLowerEdgeLevelTimeSeriesGatedFormula,
            false, ExpectedDoorOpeningValueGatedFormula)]
        [TestCase(true, ExpectedSillLevelTimeSeries,
            true, ExpectedLowerEdgeLevelTimeSeriesGatedFormula,
            true, ExpectedDoorOpeningTimeSeriesGatedFormula)]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileAndAValidFilePathAndASetOfStructuresContainingOnlyAGatedWeirWhenWriteIsCalledOnStructuresFileWithTheFilePathAndStructuresThenTheCorrectFileIsWritten(
            bool useSillLevelTimeSeries, string expectedSillLevelVal,
            bool useLowerEdgeLevelTimeSeries, string expectedLowerEdgeLevelVal,
            bool useHorizontalDoorOpeningWidthTimeSeries, string expectedHorizontalDoorOpeningWidthVal)
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Given
                var structuresFile = new StructuresFile
                {
                    StructureSchema = schema
                };
                var writePath = Path.Combine(tempDir, "structures.ini");

                var weir = new Weir2D("TestStructure", true)
                {
                    WeirFormula = new GatedWeirFormula(true)
                    {
                        DoorHeight = 50.0,
                        HorizontalDoorOpeningWidth = 30.0,
                        HorizontalDoorOpeningWidthTimeSeries = new TimeSeries(),
                        UseHorizontalDoorOpeningWidthTimeSeries = useHorizontalDoorOpeningWidthTimeSeries,
                        LowerEdgeLevel = 40.0,
                        LowerEdgeLevelTimeSeries = new TimeSeries(),
                        UseLowerEdgeLevelTimeSeries = useLowerEdgeLevelTimeSeries
                    },
                    CrestWidth = 20.0,
                    CrestLevel = 10.0,
                    UseCrestLevelTimeSeries = useSillLevelTimeSeries
                };

                var structures = new[]
                {
                    weir
                };

                // When
                structuresFile.Write(writePath, structures);

                // Then
                var expectedFileContents =
                    "[structure]" + Environment.NewLine +
                    "    type                  = gate                # Type of structure" + Environment.NewLine +
                    "    id                    = TestStructure       # Name of the structure" + Environment.NewLine +
                    expectedSillLevelVal + Environment.NewLine +
                    expectedLowerEdgeLevelVal + Environment.NewLine +
                    expectedHorizontalDoorOpeningWidthVal + Environment.NewLine +
                    "    GateHeight            = 50                  # Gate door height (in [m])" + Environment.NewLine +
                    "    GateOpeningHorizontalDirection= symmetric           # Horizontal direction of the opening doors" + Environment.NewLine +
                    "    CrestWidth            = 20                  # Gate sill width (in [m])" + Environment.NewLine;

                var fileContents = File.ReadAllText(writePath);

                Assert.That(fileContents, Is.EqualTo(expectedFileContents));
            });
        }

        [TestCase(false, ExpectedGSCrestLevelValue,
            false, ExpectedLowerEdgeLevelValueGeneralStructureFormula,
            false, ExpectedDoorOpeningValueGeneralStructureFormula)]
        [TestCase(false, ExpectedGSCrestLevelValue,
            false, ExpectedLowerEdgeLevelValueGeneralStructureFormula,
            true, ExpectedDoorOpeningTimeSeriesGeneralStructureFormula)]
        [TestCase(false, ExpectedGSCrestLevelValue,
            true, ExpectedLowerEdgeLevelTimeSeriesGeneralStructureFormula,
            false, ExpectedDoorOpeningValueGeneralStructureFormula)]
        [TestCase(false, ExpectedGSCrestLevelValue,
            true, ExpectedLowerEdgeLevelTimeSeriesGeneralStructureFormula,
            true, ExpectedDoorOpeningTimeSeriesGeneralStructureFormula)]
        [TestCase(true, ExpectedGSCrestLevelTimeSeries,
            false, ExpectedLowerEdgeLevelValueGeneralStructureFormula,
            false, ExpectedDoorOpeningValueGeneralStructureFormula)]
        [TestCase(true, ExpectedGSCrestLevelTimeSeries,
            false, ExpectedLowerEdgeLevelValueGeneralStructureFormula,
            true, ExpectedDoorOpeningTimeSeriesGeneralStructureFormula)]
        [TestCase(true, ExpectedGSCrestLevelTimeSeries,
            true, ExpectedLowerEdgeLevelTimeSeriesGeneralStructureFormula,
            false, ExpectedDoorOpeningValueGeneralStructureFormula)]
        [TestCase(true, ExpectedGSCrestLevelTimeSeries,
            true, ExpectedLowerEdgeLevelTimeSeriesGeneralStructureFormula,
            true, ExpectedDoorOpeningTimeSeriesGeneralStructureFormula)]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileAndAValidFilePathAndASetOfStructuresContainingOnlyAGeneralStructureWhenWriteIsCalledOnStructuresFileWithTheFilePathAndStructuresThenTheCorrectFileIsWritten(
            bool useCrestLevelTimeSeries, string expectedCrestLevelVal,
            bool useLowerEdgeLevelTimeSeries, string expectedLowerEdgeLevelVal,
            bool useHorizontalDoorOpeningWidthTimeSeries, string expectedHorizontalDoorOpeningWidthVal)
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Given
                var structuresFile = new StructuresFile
                {
                    StructureSchema = schema
                };
                var writePath = Path.Combine(tempDir, "structures.ini");

                var weir = new Weir2D("TestStructure", true)
                {
                    WeirFormula = new GeneralStructureWeirFormula
                    {
                        DoorHeight = 50.0,
                        HorizontalDoorOpeningWidth = 30.0,
                        HorizontalDoorOpeningWidthTimeSeries = new TimeSeries(),
                        UseHorizontalDoorOpeningWidthTimeSeries = useHorizontalDoorOpeningWidthTimeSeries,
                        LowerEdgeLevel = 40.0,
                        LowerEdgeLevelTimeSeries = new TimeSeries(),
                        UseLowerEdgeLevelTimeSeries = useLowerEdgeLevelTimeSeries,
                        WidthLeftSideOfStructure = 1.0,
                        WidthStructureLeftSide = 2.0,
                        WidthStructureRightSide = 3.0,
                        WidthRightSideOfStructure = 4.0,
                        BedLevelLeftSideOfStructure = 5.0,
                        BedLevelLeftSideStructure = 6.0,
                        BedLevelRightSideStructure = 7.0,
                        BedLevelRightSideOfStructure = 8.0,
                        PositiveFreeGateFlow = 9.0,
                        PositiveDrownedGateFlow = 11.0,
                        PositiveFreeWeirFlow = 12.0,
                        PositiveDrownedWeirFlow = 13.0,
                        PositiveContractionCoefficient = 14.0,
                        NegativeFreeGateFlow = 15.0,
                        NegativeDrownedGateFlow = 16.0,
                        NegativeFreeWeirFlow = 17.0,
                        NegativeDrownedWeirFlow = 18.0,
                        NegativeContractionCoefficient = 19.0,
                        UseExtraResistance = true,
                        ExtraResistance = 21.0
                    },
                    CrestWidth = 20.0,
                    CrestLevel = 10.0,
                    UseCrestLevelTimeSeries = useCrestLevelTimeSeries
                };

                var structures = new[]
                {
                    weir
                };

                // When
                structuresFile.Write(writePath, structures);

                // Then
                var expectedFileContents =
                    "[structure]" + Environment.NewLine +
                    "    type                  = generalstructure    # Type of structure" + Environment.NewLine +
                    "    id                    = TestStructure       # Name of the structure" + Environment.NewLine +
                    "    Upstream2Width        = 1                   # Width left side of structure (m)" + Environment.NewLine +
                    "    Upstream1Width        = 2                   # Width structure left side (m)" + Environment.NewLine +
                    "    CrestWidth            = 20                  # Width structure centre (m)" + Environment.NewLine +
                    "    Downstream1Width      = 3                   # Width structure right side (m)" + Environment.NewLine +
                    "    Downstream2Width      = 4                   # Width right side of structure (m)" + Environment.NewLine +
                    "    Upstream2Level        = 5                   # Bed level left side of structure (m AD)" + Environment.NewLine +
                    "    Upstream1Level        = 6                   # Bed level left side structure (m AD)" + Environment.NewLine +
                    expectedCrestLevelVal + Environment.NewLine +
                    "    Downstream1Level      = 7                   # Bed level right side structure (m AD)" + Environment.NewLine +
                    "    Downstream2Level      = 8                   # Bed level right side of structure (m AD)" + Environment.NewLine +
                    expectedLowerEdgeLevelVal + Environment.NewLine +
                    "    pos_freegateflowcoeff = 9                   # Positive free gate flow (-)" + Environment.NewLine +
                    "    pos_drowngateflowcoeff= 11                  # Positive drowned gate flow (-)" + Environment.NewLine +
                    "    pos_freeweirflowcoeff = 12                  # Positive free weir flow (-)" + Environment.NewLine +
                    "    pos_drownweirflowcoeff= 13                  # Positive drowned weir flow (-)" + Environment.NewLine +
                    "    pos_contrcoeffreegate = 14                  # Positive flow contraction coefficient (-)" + Environment.NewLine +
                    "    neg_freegateflowcoeff = 15                  # Negative free gate flow (-)" + Environment.NewLine +
                    "    neg_drowngateflowcoeff= 16                  # Negative drowned gate flow (-)" + Environment.NewLine +
                    "    neg_freeweirflowcoeff = 17                  # Negative free weir flow (-)" + Environment.NewLine +
                    "    neg_drownweirflowcoeff= 18                  # Negative drowned weir flow (-)" + Environment.NewLine +
                    "    neg_contrcoeffreegate = 19                  # Negative flow contraction coefficient (-)" + Environment.NewLine +
                    "    extraresistance       = 21                  # Extra resistance (-)" + Environment.NewLine +
                    "    GateHeight            = 50                  # Vertical gate door height (m)" + Environment.NewLine +
                    expectedHorizontalDoorOpeningWidthVal + Environment.NewLine +
                    "    GateOpeningHorizontalDirection= symmetric           # Horizontal direction of the opening doors" + Environment.NewLine;
                var fileContents = File.ReadAllText(writePath);

                Assert.That(fileContents, Is.EqualTo(expectedFileContents));
            });
        }

        private static Weir2D ValidatedWeir(IList<IStructure> structures)
        {
            Assert.AreEqual(1, structures.Count, "One structure was expected to be created.");
            var weir = structures.FirstOrDefault() as Weir2D;
            Assert.NotNull(weir, "The structure was expected to be a weir.");
            return weir;
        }

        private static void ValidateCommonWeirProperties(Weir2D weir, string expectedWeirName)
        {
            Assert.AreEqual(expectedWeirName, weir.Name,
                            "Name of weir was different than expected.");
            Assert.AreEqual(1, weir.CrestLevel,
                            "Crest level of weir was different than expected.");
            Assert.AreEqual(2, weir.CrestWidth,
                            "Crest width of weir was different than expected.");
        }

        private static void ValidateGatedWeirFormulaProperties(Weir2D weir, string expectedWeirName)
        {
            ValidateCommonWeirProperties(weir, expectedWeirName);

            var gatedWeirFormula = weir.WeirFormula as IGatedWeirFormula;
            Assert.NotNull(gatedWeirFormula,
                           "Expected a gated weir formula for weir.");
            Assert.AreEqual(3, gatedWeirFormula.LowerEdgeLevel,
                            "Lower edge level of weir was different than expected.");
            Assert.AreEqual(4, gatedWeirFormula.HorizontalDoorOpeningWidth,
                            "Horizontal gate opening width of weir was different than expected.");
            Assert.AreEqual(5, gatedWeirFormula.DoorHeight,
                            "Gate height of weir was different than expected.");
            Assert.AreEqual(GateOpeningDirection.Symmetric, gatedWeirFormula.HorizontalDoorOpeningDirection,
                            "Gate opening direction of weir was different than expected.");
        }

        private void ValidateGeneralWeirFormulaProperties(Weir2D weir, string expectedWeirName)
        {
            ValidateGatedWeirFormulaProperties(weir, expectedWeirName);

            var generalStructureFormula = weir.WeirFormula as GeneralStructureWeirFormula;
            Assert.NotNull(generalStructureFormula,
                           "Expected a general structure weir formula for weir.");
            Assert.AreEqual(6, generalStructureFormula.WidthLeftSideOfStructure,
                            "Upstream 2 Width of weir was different than expected.");
            Assert.AreEqual(7, generalStructureFormula.WidthStructureLeftSide,
                            "Upstream 1 Width of weir was different than expected.");
            Assert.AreEqual(8, generalStructureFormula.WidthStructureRightSide,
                            "Downstream 1 Width of weir was different than expected.");
            Assert.AreEqual(9, generalStructureFormula.WidthRightSideOfStructure,
                            "Downstream 2 Width of weir was different than expected.");
            Assert.AreEqual(10, generalStructureFormula.BedLevelLeftSideOfStructure,
                            "Upstream 2 Level of weir was different than expected.");
            Assert.AreEqual(11, generalStructureFormula.BedLevelLeftSideStructure,
                            "Upstream 1 Level of weir was different than expected.");
            Assert.AreEqual(12, generalStructureFormula.BedLevelRightSideStructure,
                            "Downstream 1 Level of weir was different than expected.");
            Assert.AreEqual(13, generalStructureFormula.BedLevelRightSideOfStructure,
                            "Downstream 2 Level of weir was different than expected.");
        }

        private void ValidateWrittenStructuresFile(string filePath)
        {
            var iniReader = new DelftIniReader();
            var categories = iniReader.ReadDelftIniFile(filePath);

            Assert.AreEqual(3, categories.Count,
                            "3 categories were expected to be read from the structures file.");
            var weirCategory = GetCategoryForStructureType(categories, StructureRegion.StructureTypeName.Weir);
            Assert.IsNotNull(weirCategory,
                             $"There was no delft ini catageory with structure type {StructureRegion.StructureTypeName.Weir}");
            ValidateCommonWeirIniProperties(weirCategory);

            var gateCategory = GetCategoryForStructureType(categories, StructureRegion.StructureTypeName.Gate);
            Assert.IsNotNull(weirCategory,
                             $"There was no delft ini catageory with structure type {StructureRegion.StructureTypeName.Gate}");
            ValidateCommonWeirIniProperties(gateCategory);
            ValidateGateIniProperties(gateCategory);

            var generalStructureCategory = GetCategoryForStructureType(categories, StructureRegion.StructureTypeName.GeneralStructure);
            Assert.IsNotNull(weirCategory,
                             $"There was no delft ini catageory with structure type {StructureRegion.StructureTypeName.GeneralStructure}");
            ValidateCommonWeirIniProperties(generalStructureCategory);
            ValidateGateIniProperties(gateCategory);
            ValidateGeneralStructureIniProperties(generalStructureCategory);
        }

        private void ValidateGeneralStructureIniProperties(DelftIniCategory category)
        {
            ValidateProperty(category, KnownGeneralStructureProperties.Upstream2Width.GetDescription(), "6");
            ValidateProperty(category, KnownGeneralStructureProperties.Upstream1Width.GetDescription(), "7");
            ValidateProperty(category, KnownGeneralStructureProperties.Downstream1Width.GetDescription(), "8");
            ValidateProperty(category, KnownGeneralStructureProperties.Downstream2Width.GetDescription(), "9");
            ValidateProperty(category, KnownGeneralStructureProperties.Upstream2Level.GetDescription(), "10");
            ValidateProperty(category, KnownGeneralStructureProperties.Upstream1Level.GetDescription(), "11");
            ValidateProperty(category, KnownGeneralStructureProperties.Downstream1Level.GetDescription(), "12");
            ValidateProperty(category, KnownGeneralStructureProperties.Downstream2Level.GetDescription(), "13");
        }

        private void ValidateGateIniProperties(DelftIniCategory category)
        {
            ValidateProperty(category, KnownStructureProperties.GateLowerEdgeLevel, "3");
            ValidateProperty(category, KnownStructureProperties.GateOpeningWidth, "4");
            ValidateProperty(category, KnownStructureProperties.GateHeight, "5");
            ValidateProperty(category, KnownStructureProperties.GateOpeningHorizontalDirection, "symmetric");
        }

        private void ValidateCommonWeirIniProperties(DelftIniCategory category)
        {
            ValidateProperty(category, KnownStructureProperties.CrestLevel, "1");
            ValidateProperty(category, KnownStructureProperties.CrestWidth, "2");
        }

        private static DelftIniCategory GetCategoryForStructureType(IList<DelftIniCategory> categories, string type)
        {
            return categories.FirstOrDefault(c => c.Properties.FirstOrDefault(p => p.Name.Equals(KnownStructureProperties.Type)).Value == type);
        }

        private void ValidateProperty(IDelftIniCategory category, string propertyName, string expectedValue)
        {
            var property = category.Properties.FirstOrDefault(p => p.Name.Equals(propertyName));
            Assert.NotNull(property,
                           $"There was no property '{propertyName}' in category '{category.Name}' in the structures file.");
            Assert.AreEqual(expectedValue, property.Value,
                            $"Value of property '{propertyName}' was not as expected.");
        }

        private string CreateStructuresFileWithPliFiles(string tempDirectoryPath, params DelftIniCategory[] delftIniCategories)
        {
            var filePath = Path.Combine(tempDirectoryPath, "structures.ini");

            foreach (var category in delftIniCategories)
            {
                var structureName = category.Properties.FirstOrDefault(p => p.Name.Equals("id")).Value;
                WritePliFile(tempDirectoryPath, structureName);
            }

            new DelftIniWriter().WriteDelftIniFile(delftIniCategories, filePath);

            return filePath;
        }

        private void WritePliFile(string tempDirectoryPath, string structureName)
        {
            File.WriteAllLines(Path.Combine(tempDirectoryPath, $"{structureName}.pli"), new[]
            {
                structureName,
                "2 2",
                "-1 1",
                "-2 2"
            });
        }

        private static DelftIniCategory CreateGeneralStructureDelftIniCategory()
        {
            const string structureName = "general_structure";
            var generalStructureCategory = new DelftIniCategory("structure")
            {
                Properties =
                {
                    new DelftIniProperty("type", "generalstructure", ""),
                    new DelftIniProperty("id", structureName, ""),
                    new DelftIniProperty("polylinefile", $"{structureName}.pli", ""),
                    new DelftIniProperty("pos_freegateflowcoeff", "0", ""),
                    new DelftIniProperty("pos_drowngateflowcoeff", "0", ""),
                    new DelftIniProperty("pos_freeweirflowcoeff", "0", ""),
                    new DelftIniProperty("pos_drownweirflowcoeff", "0", ""),
                    new DelftIniProperty("pos_contrcoeffreegate", "0", ""),
                    new DelftIniProperty("neg_freegateflowcoeff", "0", ""),
                    new DelftIniProperty("neg_drowngateflowcoeff", "0", ""),
                    new DelftIniProperty("neg_freeweirflowcoeff", "0", ""),
                    new DelftIniProperty("neg_drownweirflowcoeff", "0", ""),
                    new DelftIniProperty("neg_contrcoeffreegate", "0", ""),
                    new DelftIniProperty("extraresistance", "0", ""),
                    new DelftIniProperty("levelcenter", "1", ""),
                    new DelftIniProperty("widthcenter", "2", ""),
                    new DelftIniProperty("gateheight", "3", ""),
                    new DelftIniProperty("door_opening_width", "4", ""),
                    new DelftIniProperty("gatedoorheight", "5", ""),
                    new DelftIniProperty("horizontal_opening_direction", "symmetric", ""),
                    new DelftIniProperty("widthleftW1", "6", ""),
                    new DelftIniProperty("widthleftWsdl", "7", ""),
                    new DelftIniProperty("widthrightWsdr", "8", ""),
                    new DelftIniProperty("widthrightW2", "9", ""),
                    new DelftIniProperty("levelleftZb1", "10", ""),
                    new DelftIniProperty("levelleftZbsl", "11", ""),
                    new DelftIniProperty("levelrightZbsr", "12", ""),
                    new DelftIniProperty("levelrightZb2", "13", "")
                }
            };
            return generalStructureCategory;
        }

        private static DelftIniCategory CreateGateDelftIniCategory()
        {
            const string structureName = "gated_weir";
            var gateCategory = new DelftIniCategory("structure")
            {
                Properties =
                {
                    new DelftIniProperty("type", "gate", ""),
                    new DelftIniProperty("id", structureName, ""),
                    new DelftIniProperty("polylinefile", $"{structureName}.pli", ""),
                    new DelftIniProperty("sill_level", "1", ""),
                    new DelftIniProperty("sill_width", "2", ""),
                    new DelftIniProperty("lower_edge_level", "3", ""),
                    new DelftIniProperty("opening_width", "4", ""),
                    new DelftIniProperty("door_height", "5", ""),
                    new DelftIniProperty("horizontal_opening_direction", "symmetric", "")
                }
            };
            return gateCategory;
        }

        private static DelftIniCategory CreateWeirDelftIniCategory()
        {
            const string structureName = "simple_weir";
            var weirCategory = new DelftIniCategory("structure")
            {
                Properties =
                {
                    new DelftIniProperty("type", "weir", ""),
                    new DelftIniProperty("id", structureName, ""),
                    new DelftIniProperty("polylinefile", $"{structureName}.pli", ""),
                    new DelftIniProperty("lat_contr_coeff", "0", ""),
                    new DelftIniProperty("crest_level", "1", ""),
                    new DelftIniProperty("crest_width", "2", "")
                }
            };
            return weirCategory;
        }

        #region Sobek Structures

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAsSobekStructuresTest()
        {
            var path = TestHelper.GetTestFilePath(@"structures\example-structures-sobek.imp");

            var structureFile = new StructuresFile
            {
                StructureSchema = schema, ReferenceDate = new DateTime()
            };

            var structures = structureFile.Read(path).ToList();

            Assert.AreEqual(3, structures.Count); // There are 4 pumps in the file
            Assert.AreEqual(0, structures.OfType<IWeir>().Count());
            Assert.AreEqual(3, structures.OfType<IPump>().Count());

            var pump = structures.OfType<IPump>().First();
            Assert.AreEqual("pump01", pump.Name);
            Assert.IsNull(pump.LongName);
            Assert.IsNull(pump.Branch);
            Assert.IsNaN(pump.Chainage);
            Assert.AreEqual(new Point(500, 360), pump.Geometry);
            Assert.AreEqual(3.0, pump.Capacity);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTimeDependentSobekStructuresTest()
        {
            var path = TestHelper.GetTestFilePath(@"structures\time_dependent_structures.ini");

            var structureFile = new StructuresFile
            {
                StructureSchema = schema, ReferenceDate = new DateTime(2013, 1, 1)
            };

            var structures = structureFile.Read(path).ToList();

            Assert.AreEqual(3, structures.Count);
            Assert.AreEqual(2, structures.OfType<IWeir>().Count());
            Assert.AreEqual(1, structures.OfType<IPump>().Count());

            var pump = structures.OfType<IPump>().First();
            Assert.AreEqual(2, pump.CapacityTimeSeries.Time.Values.Count);
            Assert.AreEqual(5.6, pump.CapacityTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(11.12, pump.CapacityTimeSeries[new DateTime(2013, 1, 1, 1, 2, 0)]);

            var weir = structures.OfType<IWeir>().First(w => w.WeirFormula is SimpleWeirFormula);
            Assert.AreEqual(2, weir.CrestLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, weir.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, weir.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);

            var gate = structures.OfType<IWeir>().First(w => w.WeirFormula is GatedWeirFormula);
            var gateFormula = gate.WeirFormula as GatedWeirFormula;
            Assert.NotNull(gateFormula);

            Assert.AreEqual(2, gateFormula.LowerEdgeLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, gateFormula.LowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, gateFormula.LowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);
            Assert.AreEqual(2, gateFormula.HorizontalDoorOpeningWidthTimeSeries.Time.Values.Count);
            Assert.AreEqual(5.6, gateFormula.HorizontalDoorOpeningWidthTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(7.8, gateFormula.HorizontalDoorOpeningWidthTimeSeries[new DateTime(2013, 1, 1, 2, 3, 0)]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteSobekStructuresTest()
        {
            var exportFilePath = TestHelper.GetCurrentMethodName() + ".imp";

            #region Clean up left overs:

            if (File.Exists(exportFilePath))
            {
                File.Delete(exportFilePath);
            }

            var expectedFileNames = new[]
            {
                "pump1.pli",
                "weir1.pli",
                "gate1.pli"
            };
            foreach (var expectedFileName in expectedFileNames)
            {
                var pliFile = NGHSFileBase.GetOtherFilePathInSameDirectory(exportFilePath, expectedFileName);
                if (File.Exists(pliFile))
                {
                    File.Delete(pliFile);
                }
            }

            #endregion

            var pump = new Pump("pump1", true)
            {
                Branch = null,
                Chainage = double.NaN,
                UseCapacityTimeSeries = false,
                Capacity = 3.0,
                ControlDirection = PumpControlDirection.DeliverySideControl,
                StartDelivery = 3.4,
                StopDelivery = 2.3,
                Geometry = new LineString(new[]
                {
                    new Coordinate(1, 2),
                    new Coordinate(2, 3)
                })
            };
            var simpleWeir = new Weir("weir1", true)
            {
                Branch = null,
                Chainage = double.NaN,
                UseCrestLevelTimeSeries = false,
                CrestLevel = 2.0,
                CrestWidth = 25,
                WeirFormula = new SimpleWeirFormula
                {
                    LateralContraction = 0.7
                },
                Geometry = new LineString(new[]
                {
                    new Coordinate(4, 5),
                    new Coordinate(6, 7)
                })
            };
            var simpleGate = new Gate("gate1")
            {
                Branch = null,
                Chainage = double.NaN,
                UseLowerEdgeLevelTimeSeries = false,
                LowerEdgeLevel = 4.0,
                UseOpeningWidthTimeSeries = false,
                OpeningWidth = 12.0,
                DoorHeight = 3.0,
                HorizontalOpeningDirection = GateOpeningDirection.FromLeft,
                UseSillLevelTimeSeries = false,
                SillLevel = 1.0,
                Geometry = new LineString(new[]
                {
                    new Coordinate(8, 9),
                    new Coordinate(10, 11)
                })
            };

            var structures = new IStructure1D[]
            {
                pump,
                simpleWeir,
                simpleGate
            };
            var structuresFile = new StructuresFile
            {
                StructureSchema = schema, ReferenceDate = new DateTime(2013, 1, 1, 0, 0, 0)
            };
            structuresFile.Write(exportFilePath, structures);

            var fileContents = File.ReadAllText(exportFilePath);
            Assert.AreEqual(
                "[structure]" + Environment.NewLine +
                "    type                  = pump                # Type of structure" + Environment.NewLine +
                "    id                    = pump1               # Name of the structure" + Environment.NewLine +
                "    polylinefile          = pump1.pli           # *.pli; Polyline geometry definition for 2D structure" + Environment.NewLine +
                "    capacity              = 3                   # Pump capacity (in [m3/s])" + Environment.NewLine +
                "[structure]" + Environment.NewLine +
                "    type                  = weir                # Type of structure" + Environment.NewLine +
                "    id                    = weir1               # Name of the structure" + Environment.NewLine +
                "    polylinefile          = weir1.pli           # *.pli; Polyline geometry definition for 2D structure" + Environment.NewLine +
                "    CrestLevel            = 2                   # Weir crest height (in [m])" + Environment.NewLine +
                "    CrestWidth            = 25                  # Weir crest width (in [m])" + Environment.NewLine +
                "    lat_contr_coeff       = 0.7                 # Lateral contraction coefficient" + Environment.NewLine +
                "[structure]" + Environment.NewLine +
                "    type                  = gate                # Type of structure" + Environment.NewLine +
                "    id                    = gate1               # Name of the structure" + Environment.NewLine +
                "    polylinefile          = gate1.pli           # *.pli; Polyline geometry definition for 2D structure" + Environment.NewLine +
                "    CrestLevel            = 1                   # Gate sill level (in [m])" + Environment.NewLine +
                "    GateLowerEdgeLevel    = 4                   # Gate lower edge level (in [m])" + Environment.NewLine +
                "    GateOpeningWidth      = 12                  # Gate opening width (in [m])" + Environment.NewLine +
                "    GateHeight            = 3                   # Gate door height (in [m])" + Environment.NewLine +
                "    GateOpeningHorizontalDirection= from_left           # Horizontal direction of the opening doors" + Environment.NewLine, fileContents);

            foreach (var expectedFileName in expectedFileNames)
            {
                var filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(exportFilePath, expectedFileName);
                Assert.IsTrue(File.Exists(filePath), String.Format("File '{0}' expected to exist.", filePath));
                File.Delete(filePath);
            }

            File.Delete(exportFilePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteTimeDependentSobekStructuresTest()
        {
            var exportFilePath = TestHelper.GetCurrentMethodName() + ".imp";

            #region Clean up left overs:

            if (File.Exists(exportFilePath))
            {
                File.Delete(exportFilePath);
            }

            var expectedFileNames = new[]
            {
                "pump1.pli",
                "weir1.pli",
                "gate1.pli",
                "pump1_capacity.tim",
                "weir1_CrestLevel.tim",
                "gate1_GateLowerEdgeLevel.tim",
                "gate1_GateOpeningWidth.tim"
            };
            foreach (var expectedFileName in expectedFileNames)
            {
                var pliFile = NGHSFileBase.GetOtherFilePathInSameDirectory(exportFilePath, expectedFileName);
                if (File.Exists(pliFile))
                {
                    File.Delete(pliFile);
                }
            }

            #endregion

            var pump = new Pump("pump1", true)
            {
                Branch = null,
                Chainage = double.NaN,
                UseCapacityTimeSeries = true,
                Capacity = 3.0,
                ControlDirection = PumpControlDirection.DeliverySideControl,
                StartDelivery = 3.4,
                StopDelivery = 2.3,
                Geometry = new LineString(new[]
                {
                    new Coordinate(1, 2),
                    new Coordinate(2, 3)
                })
            };
            pump.CapacityTimeSeries[new DateTime(2013, 1, 2, 3, 4, 0)] = 5.6;
            pump.CapacityTimeSeries[new DateTime(2013, 7, 8, 9, 10, 0)] = 11.12;

            var simpleWeir = new Weir("weir1", true)
            {
                Branch = null,
                Chainage = double.NaN,
                UseCrestLevelTimeSeries = true,
                CrestLevel = 2.0,
                CrestWidth = 0,
                WeirFormula = new SimpleWeirFormula
                {
                    LateralContraction = 0.7
                },
                Geometry = new LineString(new[]
                {
                    new Coordinate(4, 5),
                    new Coordinate(6, 7)
                })
            };
            simpleWeir.CrestLevelTimeSeries[new DateTime(2013, 1, 2, 3, 4, 0)] = 6.5;
            simpleWeir.CrestLevelTimeSeries[new DateTime(2013, 7, 8, 9, 10, 0)] = 12.11;

            var simpleGate = new Gate("gate1")
            {
                Branch = null,
                Chainage = double.NaN,
                UseLowerEdgeLevelTimeSeries = true,
                LowerEdgeLevel = 4.0,
                UseOpeningWidthTimeSeries = true,
                OpeningWidth = 12.0,
                DoorHeight = 3.0,
                HorizontalOpeningDirection = GateOpeningDirection.Symmetric,
                UseSillLevelTimeSeries = true,
                SillLevel = 1.0,
                SillWidth = 15.5,
                Geometry = new LineString(new[]
                {
                    new Coordinate(8, 9),
                    new Coordinate(10, 11)
                })
            };

            simpleGate.SillLevelTimeSeries[new DateTime(2013, 6, 5, 4, 3, 2)] = 1.0;
            simpleGate.SillLevelTimeSeries[new DateTime(2013, 1, 2, 3, 4, 5)] = 6.7;

            simpleGate.LowerEdgeLevelTimeSeries[new DateTime(2013, 4, 3, 2, 1, 0)] = 4.3;
            simpleGate.LowerEdgeLevelTimeSeries[new DateTime(2013, 8, 7, 6, 5, 0)] = 2.1;

            simpleGate.OpeningWidthTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)] = 8.7;
            simpleGate.OpeningWidthTimeSeries[new DateTime(2013, 2, 3, 4, 5, 0)] = 6.5;

            var structures = new IStructure1D[]
            {
                pump,
                simpleWeir,
                simpleGate
            };
            var structuresFile = new StructuresFile
            {
                StructureSchema = schema, ReferenceDate = new DateTime(2013, 1, 1, 0, 0, 0)
            };
            structuresFile.Write(exportFilePath, structures);

            var fileContents = File.ReadAllText(exportFilePath);
            Assert.AreEqual(
                "[structure]" + Environment.NewLine +
                "    type                  = pump                # Type of structure" + Environment.NewLine +
                "    id                    = pump1               # Name of the structure" + Environment.NewLine +
                "    polylinefile          = pump1.pli           # *.pli; Polyline geometry definition for 2D structure" +
                Environment.NewLine +
                "    capacity              = pump1_capacity.tim  # Pump capacity (in [m3/s])" + Environment.NewLine +
                "[structure]" + Environment.NewLine +
                "    type                  = weir                # Type of structure" + Environment.NewLine +
                "    id                    = weir1               # Name of the structure" + Environment.NewLine +
                "    polylinefile          = weir1.pli           # *.pli; Polyline geometry definition for 2D structure" +
                Environment.NewLine +
                "    CrestLevel            = weir1_CrestLevel.tim# Weir crest height (in [m])" + Environment.NewLine +
                "    lat_contr_coeff       = 0.7                 # Lateral contraction coefficient" +
                Environment.NewLine +
                "[structure]" + Environment.NewLine +
                "    type                  = gate                # Type of structure" + Environment.NewLine +
                "    id                    = gate1               # Name of the structure" + Environment.NewLine +
                "    polylinefile          = gate1.pli           # *.pli; Polyline geometry definition for 2D structure" +
                Environment.NewLine +
                "    CrestLevel            = gate1_CrestLevel.tim# Gate sill level (in [m])" +
                Environment.NewLine +
                "    GateLowerEdgeLevel    = gate1_GateLowerEdgeLevel.tim# Gate lower edge level (in [m])" +
                Environment.NewLine +
                "    GateOpeningWidth      = gate1_GateOpeningWidth.tim# Gate opening width (in [m])" + Environment.NewLine +
                "    GateHeight            = 3                   # Gate door height (in [m])" + Environment.NewLine +
                "    GateOpeningHorizontalDirection= symmetric           # Horizontal direction of the opening doors" +
                Environment.NewLine +
                "    CrestWidth            = 15.5                # Gate sill width (in [m])" + Environment.NewLine, fileContents);

            foreach (var expectedFileName in expectedFileNames)
            {
                var filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(exportFilePath, expectedFileName);
                Assert.IsTrue(File.Exists(filePath), String.Format("File '{0}' expected to exist.", filePath));
                File.Delete(filePath);
            }

            File.Delete(exportFilePath);
        }

        #endregion

        #region Comparison helper methods for .ini files:

        private static void CompareStructureIniFiles(string iniFilePathA, string iniFilePathB)
        {
            var iniCategoriesA = new DelftIniReader().ReadDelftIniFile(iniFilePathA);
            var iniCategoriesB = new DelftIniReader().ReadDelftIniFile(iniFilePathB);

            CompareCategories(iniCategoriesA, iniCategoriesB);
        }

        private static void CompareCategories(IList<DelftIniCategory> iniCategoriesA, IList<DelftIniCategory> iniCategoriesB)
        {
            Assert.AreEqual(iniCategoriesA.Count, iniCategoriesB.Count, "Expected the same number of categories.");
            for (var i = 0; i < iniCategoriesA.Count; i++)
            {
                Assert.AreEqual(iniCategoriesA[i].Name, iniCategoriesB[i].Name, String.Format("Names are not the same at index = {0}.", i));
                CompareProperties(iniCategoriesA[i].Properties, iniCategoriesB[i].Properties);
            }
        }

        private static void CompareProperties(IList<DelftIniProperty> propertiesA, IList<DelftIniProperty> propertiesB)
        {
            Assert.AreEqual(propertiesA.Count, propertiesB.Count, "Expected the same number of properties.");
            for (var i = 0; i < propertiesA.Count; i++)
            {
                Assert.AreEqual(propertiesA[i].Name, propertiesB[i].Name, String.Format("Names are not the same at index = {0}.", i));
                Assert.AreEqual(propertiesA[i].Value, propertiesB[i].Value, String.Format("Values are not the same at index = {0}.", i));
                // Don't care about comments
            }
        }

        #endregion

        #region Comparison helper methods for structure collections:

        private static void CompareStructures(IList<Structure2D> structures, IList<Structure2D> newStructures)
        {
            Assert.AreEqual(structures.Count, newStructures.Count, "Expected the same number of structures.");
            for (int i = 0; i < structures.Count; i++)
            {
                Assert.AreEqual(structures[i].StructureType, newStructures[i].StructureType,
                                String.Format("Expected same types at index {0}", i));
                CompareStructureProperties(structures[i].Properties, newStructures[i].Properties);
            }
        }

        private static void CompareStructureProperties(IList<ModelProperty> propertiesA, IList<ModelProperty> propertiesB)
        {
            Assert.AreEqual(propertiesA.Count, propertiesB.Count);
            for (int i = 0; i < propertiesA.Count; i++)
            {
                Assert.AreEqual(propertiesA[i].GetValueAsString(), propertiesB[i].GetValueAsString());

                Assert.AreEqual(propertiesA[i].PropertyDefinition.Caption, propertiesB[i].PropertyDefinition.Caption);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.Category, propertiesB[i].PropertyDefinition.Category);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.DataType, propertiesB[i].PropertyDefinition.DataType);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.DefaultValueAsString, propertiesB[i].PropertyDefinition.DefaultValueAsString);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.Description, propertiesB[i].PropertyDefinition.Description);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.FilePropertyName, propertiesB[i].PropertyDefinition.FilePropertyName);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.IsDefinedInSchema, propertiesB[i].PropertyDefinition.IsDefinedInSchema);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.ModelFileOnly, propertiesB[i].PropertyDefinition.ModelFileOnly);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.MaxValueAsString, propertiesB[i].PropertyDefinition.MaxValueAsString);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.MinValueAsString, propertiesB[i].PropertyDefinition.MinValueAsString);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.IsFile, propertiesB[i].PropertyDefinition.IsFile);
            }
        }

        #endregion
    }
}