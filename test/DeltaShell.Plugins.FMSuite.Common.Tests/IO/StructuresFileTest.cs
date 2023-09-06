using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DHYDRO.Common.Logging;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructuresFileTest
    {
        private const string ExpectedCrestLevelValue = "    CrestLevel            = 10                  \t# Weir crest height (in [m])";
        private const string ExpectedCrestLevelTimeSeries = "    CrestLevel            = TestStructure_CrestLevel.tim\t# Weir crest height (in [m])";
        private const string ExpectedGateCrestLevelValue = "    CrestLevel            = 10                  \t# Gate crest level (in [m])";
        private const string ExpectedGateCrestLevelTimeSeries = "    CrestLevel            = TestStructure_CrestLevel.tim\t# Gate crest level (in [m])";
        private const string ExpectedGateLowerEdgeLevelValueGatedFormula = "    GateLowerEdgeLevel    = 40                  \t# Gate lower edge level (in [m])";
        private const string ExpectedGateLowerEdgeLevelTimeSeriesGatedFormula = "    GateLowerEdgeLevel    = TestStructure_GateLowerEdgeLevel.tim\t# Gate lower edge level (in [m])";
        private const string ExpectedGateOpeningValueGatedFormula = "    GateOpeningWidth      = 30                  \t# Gate opening width (in [m])";
        private const string ExpectedGateOpeningTimeSeriesGatedFormula = "    GateOpeningWidth      = TestStructure_GateOpeningWidth.tim\t# Gate opening width (in [m])";
        private const string ExpectedGSCrestLevelValue = "    CrestLevel            = 10                  \t# Crest level (m AD)";
        private const string ExpectedGSCrestLevelTimeSeries = "    CrestLevel            = TestStructure_CrestLevel.tim\t# Crest level (m AD)";
        private const string ExpectedGateLowerEdgeLevelValueGeneralStructureFormula = "    GateLowerEdgeLevel    = 40                  \t# Gate lower edge level (m AD)";
        private const string ExpectedGateLowerEdgeLevelTimeSeriesGeneralStructureFormula = "    GateLowerEdgeLevel    = TestStructure_GateLowerEdgeLevel.tim\t# Gate lower edge level (m AD)";
        private const string ExpectedGateOpeningValueGeneralStructureFormula = "    GateOpeningWidth      = 30                  \t# Horizontal opening width between the gates (m)";
        private const string ExpectedGateOpeningTimeSeriesGeneralStructureFormula = "    GateOpeningWidth      = TestStructure_GateOpeningWidth.tim\t# Horizontal opening width between the gates (m)";
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
            string path = TestHelper.GetTestFilePath(@"structures\example-structures.imp");

            var structureFile = new StructuresFile {StructureSchema = schema};
            List<StructureDAO> structureDataAccessObjects = structureFile.ReadStructuresFromFile(path).ToList();

            Assert.AreEqual(7, structureDataAccessObjects.Count);
            Assert.AreEqual(2, structureDataAccessObjects.Count(s => s.StructureType == StructureType.Weir));
            Assert.AreEqual(3, structureDataAccessObjects.Count(s => s.StructureType == StructureType.Pump));
            Assert.AreEqual(1, structureDataAccessObjects.Count(s => s.StructureType == StructureType.Gate));
            Assert.AreEqual(1, structureDataAccessObjects.Count(s => s.StructureType == StructureType.GeneralStructure));

            StructureDAO weirDown = structureDataAccessObjects.First(s => s.Name == "Weir_down");
            Assert.AreEqual(6, weirDown.Properties.Count);
            Assert.AreEqual("680", weirDown.GetProperty(KnownStructureProperties.X).GetValueAsString());
            Assert.AreEqual("360", weirDown.GetProperty(KnownStructureProperties.Y).GetValueAsString());
            Assert.AreEqual("2", weirDown.GetProperty(KnownStructureProperties.CrestLevel).GetValueAsString());
            Assert.AreEqual("1", weirDown.GetProperty(KnownStructureProperties.LateralContractionCoefficient).GetValueAsString());

            StructureDAO generalStructure = structureDataAccessObjects.First(s => s.Name == "gs_01");
            Assert.That(generalStructure.Properties.Count, Is.EqualTo(4));
            Assert.That(generalStructure.GetProperty(KnownStructureProperties.PolylineFile).GetValueAsString(), Is.EqualTo("gs_01.pli"));
            Assert.That(generalStructure.GetProperty(KnownGeneralStructureProperties.CrestWidth).GetValueAsString(), Is.EqualTo("2.3"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresInvalidFileFormat()
        {
            string path = TestHelper.GetTestFilePath(@"structures\invalidFormat.imp");
            var structuresFile = new StructuresFile {StructureSchema = schema};
            IEnumerable<StructureDAO> structures = structuresFile.ReadStructuresFromFile(path);
            Assert.AreEqual(0, structures.Count(), "Nothing should have been read.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileWithAStructureUsingAMissingPliFileValue_WhenReading_ThenAWarningShouldBeGivenAndTheStructureShouldNotBeCreated()
        {
            // Given
            using (var temp = new TemporaryDirectory())
            {
                List<string> copiesInTempFilePaths =
                    temp.CopyAllTestDataToTempDirectory(@"structures\FlowFMPliFileValueMissing_structures.ini");

                string path = copiesInTempFilePaths[0];

                var structuresFile = new StructuresFile {StructureSchema = schema};

                var logHandlerMock = MockRepository.GenerateStrictMock<ILogHandler>();
                logHandlerMock.Expect(l => l.ReportErrorFormat(
                                          Arg<string>.Matches(fp => fp.Equals(Resources.StructureFile_Failed_to_convert_ini_structure_definition_to_actual_structure_Line__0____1__)),
                                          Arg<object[]>.Matches(fs => fs.Length == 2 &&
                                                                      fs[0].Equals(1) &&
                                                                      fs[1].Equals("Structure 'structure02' does not have a filename specified for property 'polylinefile'."))))
                              .Repeat.Once();

                // When
                logHandlerMock.Replay();

                IList<StructureDAO> structureDataAccessObjects = structuresFile.ReadStructuresFromFile(path, logHandlerMock).ToList();

                // Then
                Assert.AreEqual(0, structureDataAccessObjects.Count, "A valid structure has been read from the file, while an invalid structure was written in the file");
                logHandlerMock.VerifyAllExpectations();
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileWithAnUnsupportedHeaderForAStructure_WhenReading_ThenAWarningShouldBeGivenAndTheStructureShouldNotBeCreated()
        {
            // Given
            using (var temp = new TemporaryDirectory())
            {
                List<string> copiesInTempFilePaths =
                    temp.CopyAllTestDataToTempDirectory(@"structures\FlowFMUnsupportedCategory_structures.ini",
                                                        @"structures\structure01.pli");

                string path = copiesInTempFilePaths[0];

                var structuresFile = new StructuresFile {StructureSchema = schema};

                var logHandlerMock = MockRepository.GenerateStrictMock<ILogHandler>();
                logHandlerMock.Expect(l => l.ReportWarningFormat(
                                          Arg<string>.Matches(fp => fp.Equals(Resources.StructureFile_Section__0__not_supported_for_structures_and_is_skipped_Line__1__)),
                                          Arg<object[]>.Matches(fs => fs.Length == 2 &&
                                                                      fs[0].Equals("test") &&
                                                                      fs[1].Equals(1))))
                              .Repeat.Once();

                // When
                logHandlerMock.Replay();

                IList<StructureDAO> structureDataAccessObjects = structuresFile.ReadStructuresFromFile(path, logHandlerMock).ToList();

                // Then
                Assert.AreEqual(0, structureDataAccessObjects.Count, "A valid structure has been read from the file, while an invalid structure was written in the file");
                logHandlerMock.VerifyAllExpectations();
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileWithAStructureWithUnknownKeys_WhenReading_ThenAWarningShouldBeGivenAndTheStructureShouldBeCreated()
        {
            // Given
            using (var temp = new TemporaryDirectory())
            {
                List<string> copiesInTempFilePaths = temp.CopyAllTestDataToTempDirectory(@"structures\FlowFM2KeysNotInScheme_structures.ini",
                                                                                         @"structures\structure01.pli");

                string path = copiesInTempFilePaths[0];

                var structuresFile = new StructuresFile {StructureSchema = schema};

                var logHandlerMock = MockRepository.GenerateStrictMock<ILogHandler>();
                logHandlerMock.Expect(l => l.ReportWarningFormat(
                                          Arg<string>.Matches(fp => fp.Equals(Resources.StructureFile_Property__0__not_supported_for_structures_of_type__1__and_is_skipped_Line__2__)),
                                          Arg<object[]>.Matches(fs => fs.Length == 3 &&
                                                                      fs[0].Equals("weir1") &&
                                                                      fs[1].Equals("weir") &&
                                                                      fs[2].Equals(8))))
                              .Repeat.Once();
                logHandlerMock.Expect(l => l.ReportWarningFormat(
                                          Arg<string>.Matches(fp => fp.Equals(Resources.StructureFile_Property__0__not_supported_for_structures_of_type__1__and_is_skipped_Line__2__)),
                                          Arg<object[]>.Matches(fs => fs.Length == 3 &&
                                                                      fs[0].Equals("weir2") &&
                                                                      fs[1].Equals("weir") &&
                                                                      fs[2].Equals(9))))
                              .Repeat.Once();

                // When
                logHandlerMock.Replay();

                IList<StructureDAO> structureDataAccessObjects = structuresFile.ReadStructuresFromFile(path, logHandlerMock).ToList();

                // Then
                Assert.AreEqual(1, structureDataAccessObjects.Count, "One structure should have been created");
                logHandlerMock.VerifyAllExpectations();
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileWithAStructureWithMissingType_WhenReading_ThenAWarningShouldBeGivenAndTheStructureShouldNotBeCreated()
        {
            // Given
            using (var temp = new TemporaryDirectory())
            {
                List<string> copiesInTempFilePaths = temp.CopyAllTestDataToTempDirectory(@"structures\FlowFMMissingTypeProperty_structures.ini",
                                                                                         @"structures\structure01.pli");

                string path = copiesInTempFilePaths[0];

                var structuresFile = new StructuresFile {StructureSchema = schema};

                var logHandlerMock = MockRepository.GenerateStrictMock<ILogHandler>();
                logHandlerMock.Expect(l => l.ReportWarningFormat(
                                          Arg<string>.Matches(fp => fp.Equals(Resources.StructureFile_Obligated_property__0__expected_but_is_missing_Structure_is_skipped_Line__1__)),
                                          Arg<object[]>.Matches(fs => fs.Length == 2 &&
                                                                      fs[0].Equals("type") &&
                                                                      fs[1].Equals(1))))
                              .Repeat.Once();

                // When
                logHandlerMock.Replay();

                IList<StructureDAO> structureDataAccessObjects = structuresFile.ReadStructuresFromFile(path, logHandlerMock).ToList();

                // Then
                Assert.AreEqual(0, structureDataAccessObjects.Count, "A valid structure has been read from the file, while an invalid structure was written in the file");
                logHandlerMock.VerifyAllExpectations();
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteStructures()
        {
            string exportFilePath = TestHelper.GetCurrentMethodName() + ".imp";
            if (File.Exists(exportFilePath))
            {
                File.Delete(exportFilePath);
            }

            var weir = new StructureDAO("weir");
            weir.AddProperty(KnownStructureProperties.Type, typeof(string), "weir");
            weir.AddProperty(KnownStructureProperties.Name, typeof(string), "Weir_down");
            weir.AddProperty(KnownStructureProperties.X, typeof(double), "680");
            weir.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            weir.AddProperty(KnownStructureProperties.CrestLevel, typeof(double), "2");
            weir.AddProperty(KnownStructureProperties.LateralContractionCoefficient, typeof(double), "1");

            StructureDAO[] structures = new[]
            {
                weir
            };

            StructuresFile.WriteStructuresDataAccessObjects(exportFilePath, structures);

            string fileContents = File.ReadAllText(exportFilePath);
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
            string path = TestHelper.GetTestFilePath(@"structures\example-structures.imp");

            var structureFile = new StructuresFile {StructureSchema = schema};
            List<StructureDAO> structures = structureFile.ReadStructuresFromFile(path).ToList();

            string exportFilePath = TestHelper.GetCurrentMethodName() + ".imp";
            var newStructuresFile = new StructuresFile {StructureSchema = schema};
            StructuresFile.WriteStructuresDataAccessObjects(exportFilePath, structures);

            CompareStructureIniFiles(path, exportFilePath); // Note: Comments in user file can differ from schema!

            List<StructureDAO> newStructuresDataAccessObjects = newStructuresFile.ReadStructuresFromFile(exportFilePath).ToList();

            CompareStructures(structures, newStructuresDataAccessObjects);

            File.Delete(exportFilePath);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenGeneralStructureWhenWritingToFileAndReadingFromThatFileThenResultingStructuresAreTheSame()
        {
            string iniFilePath = TestHelper.GetTestFilePath(@"structures\temp_file.ini");
            string pliFilePath = TestHelper.GetTestFilePath(@"structures\gs01.pli");
            if (File.Exists(iniFilePath))
            {
                File.Delete(iniFilePath);
            }

            if (File.Exists(pliFilePath))
            {
                File.Delete(pliFilePath);
            }

            var initialFormula = new GeneralStructureFormula
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
                Upstream1Width = 11,
                Upstream2Width = 12,
                CrestWidth = 13,
                Downstream1Width = 14,
                Downstream2Width = 15,
                Upstream1Level = 16,
                Upstream2Level = 17,
                CrestLevel = 18,
                Downstream1Level = 19,
                Downstream2Level = 20,
                UseExtraResistance = true,
                ExtraResistance = 40,
                GateHeight = 50,
                UseHorizontalGateOpeningWidthTimeSeries = false,
                HorizontalGateOpeningWidth = 60,
                UseGateLowerEdgeLevelTimeSeries = false,
                GateLowerEdgeLevel = 70
            };
            var initialGeneralStructure = new Structure()
            {
                Name = "gs01",
                Geometry = new LineString(new[]
                {
                    new Coordinate(4, 5),
                    new Coordinate(6, 7)
                }),
                Formula = initialFormula
            };
            var structures = new IStructureObject[]
            {
                initialGeneralStructure
            };
            var structuresFile = new StructuresFile
            {
                StructureSchema = schema,
                ReferenceDate = new DateTime(2013, 1, 1, 0, 0, 0)
            };

            structuresFile.Write(iniFilePath, structures);
            IList<IStructureObject> readResult = structuresFile.Read(iniFilePath);
            Assert.That(readResult.Count, Is.EqualTo(1));

            var resultingGeneralStructure = readResult.FirstOrDefault() as IStructure;
            Assert.IsNotNull(resultingGeneralStructure);
            var resultingFormula = resultingGeneralStructure.Formula as GeneralStructureFormula;
            Assert.IsNotNull(resultingFormula);

            bool weirPropertiesAreEqual = initialGeneralStructure.FormulaName == resultingGeneralStructure.Formula.Name;

            bool formulasAreEqual = initialFormula.Name == resultingFormula.Name &&
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
                                    initialFormula.Upstream1Level == resultingFormula.Upstream1Level &&
                                    initialFormula.Upstream2Level == resultingFormula.Upstream2Level &&
                                    initialFormula.CrestLevel == resultingFormula.CrestLevel &&
                                    initialFormula.Downstream1Level == resultingFormula.Downstream1Level &&
                                    initialFormula.Downstream2Level == resultingFormula.Downstream2Level &&
                                    initialFormula.Upstream1Width == resultingFormula.Upstream1Width &&
                                    initialFormula.Upstream2Width == resultingFormula.Upstream2Width &&
                                    initialFormula.CrestWidth == resultingFormula.CrestWidth &&
                                    initialFormula.Downstream1Width == resultingFormula.Downstream1Width &&
                                    initialFormula.Downstream2Width == resultingFormula.Downstream2Width &&
                                    initialFormula.UseExtraResistance == resultingFormula.UseExtraResistance &&
                                    initialFormula.ExtraResistance == resultingFormula.ExtraResistance &&
                                    initialFormula.GateHeight == resultingFormula.GateHeight &&
                                    initialFormula.UseHorizontalGateOpeningWidthTimeSeries == resultingFormula.UseHorizontalGateOpeningWidthTimeSeries &&
                                    initialFormula.HorizontalGateOpeningWidth == resultingFormula.HorizontalGateOpeningWidth &&
                                    initialFormula.UseGateLowerEdgeLevelTimeSeries == resultingFormula.UseGateLowerEdgeLevelTimeSeries &&
                                    initialFormula.GateLowerEdgeLevel == resultingFormula.GateLowerEdgeLevel &&
                                    initialFormula.GateOpening == resultingFormula.GateOpening;

            Assert.IsTrue(weirPropertiesAreEqual && formulasAreEqual);

            //cleanup
            if (File.Exists(iniFilePath))
            {
                File.Delete(iniFilePath);
            }

            if (File.Exists(pliFilePath))
            {
                File.Delete(pliFilePath);
            }
        }

        [Test]
        public void ReadThrowsForInvalidFilePath()
        {
            var structureFile = new StructuresFile {StructureSchema = new StructureSchema<ModelPropertyDefinition>()};
            Assert.Throws<FileNotFoundException>(() => structureFile.ReadStructuresFromFile("I do not exist").ToList());
        }

        [Test]
        public void GivenAStructuresFileAndAValidFilePathAndASetOfStructuresContainingOnlyAPump2DWhenWriteIsCalledOnStructuresFileWithTheFilePathAndStructuresThenTheCorrectFileIsWritten()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Given
                var structuresFile = new StructuresFile {StructureSchema = schema};
                string writePath = Path.Combine(tempDir, "structures.ini");

                var pump = new Pump
                {
                    Name = "TestStructure",
                    Capacity = 20.0
                };

                IPump[] structures = { pump };

                // When
                structuresFile.Write(writePath, structures);

                // Then
                string expectedFileContents =
                    "[structure]" + Environment.NewLine +
                    "    type                  = pump                \t# Type of structure" + Environment.NewLine +
                    "    id                    = TestStructure       \t# Name of the structure" + Environment.NewLine +
                    "    capacity              = 20                  \t# Pump capacity (in [m3/s])" + Environment.NewLine;
                string fileContents = File.ReadAllText(writePath);

                Assert.That(fileContents, Is.EqualTo(expectedFileContents));
            });
        }

        [Test]
        public void GivenAStructuresFileWithOldVariableNames_WhenReadingAndWriting_ThenPropertiesAreRenamed()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDirectory =>
            {
                // Given
                string structuresFilePath = CreateStructuresFileWithPliFiles(
                    tempDirectory,
                    CreateOldWeirSection(),
                    CreateOldGateSection(),
                    CreateOldGeneralStructureSection());

                var structuresFile = new StructuresFile {StructureSchema = schema};

                // When
                IList<IStructureObject> structures = structuresFile.Read(structuresFilePath);
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
                string structuresFilePath = CreateStructuresFileWithPliFiles(
                    tempDirectory,
                    CreateOldWeirSection());

                var structuresFile = new StructuresFile {StructureSchema = schema};

                // When
                IList<IStructureObject> structures = structuresFile.Read(structuresFilePath);

                // Then
                IStructure weir = ValidatedWeir(structures);
                ValidateCommonWeirProperties(weir, "simple_weir");
            });
        }

        [Test]
        public void GivenAStructuresFileWithAGatedWeir_WhenReading_CorrectStructureIsCreated()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDirectory =>
            {
                // Given
                string structuresFilePath = CreateStructuresFileWithPliFiles(
                    tempDirectory,
                    CreateOldGateSection());

                var structuresFile = new StructuresFile {StructureSchema = new WaterFlowFMModelDefinition().StructureSchema};

                // When
                IList<IStructureObject> structures = structuresFile.Read(structuresFilePath);

                // Then
                IStructure weir = ValidatedWeir(structures);
                ValidateGatedWeirFormulaProperties(weir, "gated_weir");
            });
        }

        [Test]
        public void GivenAStructuresFileWithAGeneralStructure_WhenReading_CorrectStructureIsCreated()
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDirectory =>
            {
                // Given
                string structuresFilePath = CreateStructuresFileWithPliFiles(
                    tempDirectory,
                    CreateOldGeneralStructureSection());

                var structuresFile = new StructuresFile {StructureSchema = new WaterFlowFMModelDefinition().StructureSchema};

                // When
                IList<IStructureObject> structures = structuresFile.Read(structuresFilePath);

                // Then
                IStructure weir = ValidatedWeir(structures);
                ValidateGeneralWeirFormulaProperties(weir, "general_structure");
            });
        }

        [Test]
        public void GivenAStructureFileWithAGeneralStructure_WhenReading_ThenAllSubfilesOfTheGeneralStructureShouldBeRead()
        {
            // Given
            using (var temp = new TemporaryDirectory())
            {
                var filesList = new[]
                {
                    @"structures\generalstructure\testmodel_structures.ini",
                    @"structures\generalstructure\structure01.pli",
                    @"structures\generalstructure\structure01_CrestLevel.tim",
                    @"structures\generalstructure\structure01_GateLowerEdgeLevel.tim"
                };

                List<string> copiesInTempFilePaths =
                    temp.CopyAllTestDataToTempDirectory(filesList);

                var structureFile = new StructuresFile {StructureSchema = new WaterFlowFMModelDefinition().StructureSchema};

                string copyOfIniInTempFilePath = copiesInTempFilePaths[0];

                // When
                IList<IStructureObject> structures = 
                    structureFile.ReadStructuresFileRelativeToReferenceFile(copyOfIniInTempFilePath,
                                                                            copyOfIniInTempFilePath);

                // Then
                Assert.AreEqual(1, structures.Count, "The ini file for the structures is not correctly read");

                var generalStructure = structures[0] as IStructure;
                Assert.IsNotNull(generalStructure, "The ini file for the structures is not correctly read");

                var weirFormula = generalStructure.Formula as GeneralStructureFormula;
                Assert.IsNotNull(weirFormula, "The ini file for the structures is not correctly read");

                Assert.IsTrue(generalStructure.UseCrestLevelTimeSeries, "The tim file for the crest level is not correctly read");
                Assert.AreEqual(new[]
                                {
                                    5,
                                    6
                                }, generalStructure.CrestLevelTimeSeries.Components[0].Values,
                                "The tim file for the crest level is not correctly read");

                Assert.IsTrue(weirFormula.UseGateLowerEdgeLevelTimeSeries, "The tim file for the lower edge level is not correctly read");
                Assert.AreEqual(new[]
                                {
                                    3,
                                    4
                                }, weirFormula.GateLowerEdgeLevelTimeSeries.Components[0].Values,
                                "The tim file for the lower edge level is not correctly read");

                Assert.AreEqual(2, generalStructure.Geometry.Coordinates.Length, "The pli file for the geometry is not correctly read");
            }
        }

        [Test]
        public void GivenAStructureFileWithTwoKeysNotInScheme_WhenReading_ThenOneGroupWarningShouldBeGiven()
        {
            // Given
            using (var temp = new TemporaryDirectory())
            {
                List<string> copiesInTempFilePaths =
                    temp.CopyAllTestDataToTempDirectory(@"structures\FlowFM2KeysNotInScheme_structures.ini",
                                                        @"structures\structure01.pli");

                var structureFile = new StructuresFile {StructureSchema = schema};

                string copyOfIniInTempFilePath = copiesInTempFilePaths[0];

                // When
                IList<IStructureObject> structures = null;

                IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                    () => structures = structureFile.ReadStructuresFileRelativeToReferenceFile(copyOfIniInTempFilePath,
                                                                                               copyOfIniInTempFilePath));
                // Then
                Assert.AreEqual(1, structures.Count, "The ini file for the structures is not correctly read");
                CheckMessages(messages, copyOfIniInTempFilePath);
            }
        }

        [Test]
        public void GivenAStructureFileWithTwoKeysNotInScheme_WhenImportingTheseArea2DFeatures_ThenOneGroupWarningShouldBeGiven()
        {
            // Given
            using (var temp = new TemporaryDirectory())
            {
                List<string> copiesInTempFilePaths =
                    temp.CopyAllTestDataToTempDirectory(@"structures\FlowFM2KeysNotInScheme_structures.ini",
                                                        @"structures\structure01.pli");

                var structureFile = new StructuresFile {StructureSchema = schema};

                string copyOfIniInTempFilePath = copiesInTempFilePaths[0];

                // When
                IList<IStructureObject> structures = null;

                IEnumerable<string> messages = TestHelper.GetAllRenderedMessages(
                    () => structures = structureFile.Read(copyOfIniInTempFilePath));
                // Then
                Assert.AreEqual(1, structures.Count, "The ini file for the structures is not correctly read");
                CheckMessages(messages, copyOfIniInTempFilePath);
            }
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
                var structuresFile = new StructuresFile {StructureSchema = schema};
                string writePath = Path.Combine(tempDir, "structures.ini");

                var weir = new Structure()
                {
                    Name = "TestStructure",
                    Formula = new SimpleWeirFormula(),
                    CrestWidth = 20.0,
                    CrestLevel = 10.0,
                    UseCrestLevelTimeSeries = useCrestLevelTimeSeries
                };

                IStructure[] structures = {weir};

                // When
                structuresFile.Write(writePath, structures);

                // Then
                string expectedFileContents =
                    "[structure]" + Environment.NewLine +
                    "    type                  = weir                \t# Type of structure" + Environment.NewLine +
                    "    id                    = TestStructure       \t# Name of the structure" + Environment.NewLine +
                    expectedCrestLevelVal + Environment.NewLine +
                    "    CrestWidth            = 20                  \t# Weir crest width (in [m])" + Environment.NewLine +
                    "    lat_contr_coeff       = 1                   \t# Lateral contraction coefficient" + Environment.NewLine;

                string fileContents = File.ReadAllText(writePath);

                Assert.That(fileContents, Is.EqualTo(expectedFileContents));
            });
        }

        [TestCase(false, ExpectedGateCrestLevelValue,
                  false, ExpectedGateLowerEdgeLevelValueGatedFormula,
                  false, ExpectedGateOpeningValueGatedFormula)]
        [TestCase(false, ExpectedGateCrestLevelValue,
                  false, ExpectedGateLowerEdgeLevelValueGatedFormula,
                  true, ExpectedGateOpeningTimeSeriesGatedFormula)]
        [TestCase(false, ExpectedGateCrestLevelValue,
                  true, ExpectedGateLowerEdgeLevelTimeSeriesGatedFormula,
                  false, ExpectedGateOpeningValueGatedFormula)]
        [TestCase(false, ExpectedGateCrestLevelValue,
                  true, ExpectedGateLowerEdgeLevelTimeSeriesGatedFormula,
                  true, ExpectedGateOpeningTimeSeriesGatedFormula)]
        [TestCase(true, ExpectedGateCrestLevelTimeSeries,
                  false, ExpectedGateLowerEdgeLevelValueGatedFormula,
                  false, ExpectedGateOpeningValueGatedFormula)]
        [TestCase(true, ExpectedGateCrestLevelTimeSeries,
                  false, ExpectedGateLowerEdgeLevelValueGatedFormula,
                  true, ExpectedGateOpeningTimeSeriesGatedFormula)]
        [TestCase(true, ExpectedGateCrestLevelTimeSeries,
                  true, ExpectedGateLowerEdgeLevelTimeSeriesGatedFormula,
                  false, ExpectedGateOpeningValueGatedFormula)]
        [TestCase(true, ExpectedGateCrestLevelTimeSeries,
                  true, ExpectedGateLowerEdgeLevelTimeSeriesGatedFormula,
                  true, ExpectedGateOpeningTimeSeriesGatedFormula)]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileAndAValidFilePathAndASetOfStructuresContainingOnlyAGatedWeirWhenWriteIsCalledOnStructuresFileWithTheFilePathAndStructuresThenTheCorrectFileIsWritten(
            bool useCrestLevelTimeSeries, string expectedCrestLevelVal,
            bool useGateLowerEdgeLevelTimeSeries, string expectedGateLowerEdgeLevelVal,
            bool useHorizontalGateOpeningWidthTimeSeries, string expectedHorizontalGateOpeningWidthVal)
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Given
                var structuresFile = new StructuresFile {StructureSchema = schema};
                string writePath = Path.Combine(tempDir, "structures.ini");

                var weir = new Structure()
                {
                    Name = "TestStructure",
                    Formula = new SimpleGateFormula(true)
                    {
                        GateHeight = 50.0,
                        HorizontalGateOpeningWidth = 30.0,
                        HorizontalGateOpeningWidthTimeSeries = new TimeSeries(),
                        UseHorizontalGateOpeningWidthTimeSeries = useHorizontalGateOpeningWidthTimeSeries,
                        GateLowerEdgeLevel = 40.0,
                        GateLowerEdgeLevelTimeSeries = new TimeSeries(),
                        UseGateLowerEdgeLevelTimeSeries = useGateLowerEdgeLevelTimeSeries
                    },
                    CrestWidth = 20.0,
                    CrestLevel = 10.0,
                    UseCrestLevelTimeSeries = useCrestLevelTimeSeries
                };

                IStructure[] structures = {weir};

                // When
                structuresFile.Write(writePath, structures);

                // Then
                string expectedFileContents =
                    "[structure]" + Environment.NewLine +
                    "    type                  = gate                \t# Type of structure" + Environment.NewLine +
                    "    id                    = TestStructure       \t# Name of the structure" + Environment.NewLine +
                    expectedCrestLevelVal + Environment.NewLine +
                    expectedGateLowerEdgeLevelVal + Environment.NewLine +
                    expectedHorizontalGateOpeningWidthVal + Environment.NewLine +
                    "    GateHeight            = 50                  \t# Gate door height (in [m])" + Environment.NewLine +
                    "    GateOpeningHorizontalDirection= symmetric           \t# Horizontal direction of the opening gates" + Environment.NewLine +
                    "    CrestWidth            = 20                  \t# Gate crest width (in [m])" + Environment.NewLine;

                string fileContents = File.ReadAllText(writePath);

                Assert.That(fileContents, Is.EqualTo(expectedFileContents));
            });
        }

        [TestCase(false, ExpectedGSCrestLevelValue,
                  false, ExpectedGateLowerEdgeLevelValueGeneralStructureFormula,
                  false, ExpectedGateOpeningValueGeneralStructureFormula)]
        [TestCase(false, ExpectedGSCrestLevelValue,
                  false, ExpectedGateLowerEdgeLevelValueGeneralStructureFormula,
                  true, ExpectedGateOpeningTimeSeriesGeneralStructureFormula)]
        [TestCase(false, ExpectedGSCrestLevelValue,
                  true, ExpectedGateLowerEdgeLevelTimeSeriesGeneralStructureFormula,
                  false, ExpectedGateOpeningValueGeneralStructureFormula)]
        [TestCase(false, ExpectedGSCrestLevelValue,
                  true, ExpectedGateLowerEdgeLevelTimeSeriesGeneralStructureFormula,
                  true, ExpectedGateOpeningTimeSeriesGeneralStructureFormula)]
        [TestCase(true, ExpectedGSCrestLevelTimeSeries,
                  false, ExpectedGateLowerEdgeLevelValueGeneralStructureFormula,
                  false, ExpectedGateOpeningValueGeneralStructureFormula)]
        [TestCase(true, ExpectedGSCrestLevelTimeSeries,
                  false, ExpectedGateLowerEdgeLevelValueGeneralStructureFormula,
                  true, ExpectedGateOpeningTimeSeriesGeneralStructureFormula)]
        [TestCase(true, ExpectedGSCrestLevelTimeSeries,
                  true, ExpectedGateLowerEdgeLevelTimeSeriesGeneralStructureFormula,
                  false, ExpectedGateOpeningValueGeneralStructureFormula)]
        [TestCase(true, ExpectedGSCrestLevelTimeSeries,
                  true, ExpectedGateLowerEdgeLevelTimeSeriesGeneralStructureFormula,
                  true, ExpectedGateOpeningTimeSeriesGeneralStructureFormula)]
        [Category(TestCategory.DataAccess)]
        public void GivenAStructuresFileAndAValidFilePathAndASetOfStructuresContainingOnlyAGeneralStructureWhenWriteIsCalledOnStructuresFileWithTheFilePathAndStructuresThenTheCorrectFileIsWritten(
            bool useCrestLevelTimeSeries, string expectedCrestLevelVal,
            bool useGateLowerEdgeLevelTimeSeries, string expectedGateLowerEdgeLevelVal,
            bool useHorizontalGateOpeningWidthTimeSeries, string expectedHorizontalGateOpeningWidthVal)
        {
            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Given
                var structuresFile = new StructuresFile {StructureSchema = schema};
                string writePath = Path.Combine(tempDir, "structures.ini");

                var weir = new Structure()
                {
                    Name = "TestStructure",
                    Formula = new GeneralStructureFormula
                    {
                        GateHeight = 50.0,
                        HorizontalGateOpeningWidth = 30.0,
                        HorizontalGateOpeningWidthTimeSeries = new TimeSeries(),
                        UseHorizontalGateOpeningWidthTimeSeries = useHorizontalGateOpeningWidthTimeSeries,
                        GateLowerEdgeLevel = 40.0,
                        GateLowerEdgeLevelTimeSeries = new TimeSeries(),
                        UseGateLowerEdgeLevelTimeSeries = useGateLowerEdgeLevelTimeSeries,
                        Upstream1Width = 1.0,
                        Upstream2Width = 2.0,
                        Downstream1Width = 3.0,
                        Downstream2Width = 4.0,
                        Upstream1Level = 5.0,
                        Upstream2Level = 6.0,
                        Downstream1Level = 7.0,
                        Downstream2Level = 8.0,
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

                IStructure[] structures = {weir};

                // When
                structuresFile.Write(writePath, structures);

                // Then
                string expectedFileContents =
                    "[structure]" + Environment.NewLine +
                    "    type                  = generalstructure    \t# Type of structure" + Environment.NewLine +
                    "    id                    = TestStructure       \t# Name of the structure" + Environment.NewLine +
                    "    Upstream1Width        = 1                   \t# Upstream width 1 (m)" + Environment.NewLine +
                    "    Upstream2Width        = 2                   \t# Upstream width 2 (m)" + Environment.NewLine +
                    "    CrestWidth            = 20                  \t# Crest width (m)" + Environment.NewLine +
                    "    Downstream1Width      = 3                   \t# Downstream width 1 (m)" + Environment.NewLine +
                    "    Downstream2Width      = 4                   \t# Downstream width 2 (m)" + Environment.NewLine +
                    "    Upstream1Level        = 5                   \t# Upstream level 1 (m AD)" + Environment.NewLine +
                    "    Upstream2Level        = 6                   \t# Upstream level 2 (m AD)" + Environment.NewLine +
                    expectedCrestLevelVal + Environment.NewLine +
                    "    Downstream1Level      = 7                   \t# Downstream level 1 (m AD)" + Environment.NewLine +
                    "    Downstream2Level      = 8                   \t# Downstream level 2 (m AD)" + Environment.NewLine +
                    expectedGateLowerEdgeLevelVal + Environment.NewLine +
                    "    pos_freegateflowcoeff = 9                   \t# Positive free gate flow (-)" + Environment.NewLine +
                    "    pos_drowngateflowcoeff= 11                  \t# Positive drowned gate flow (-)" + Environment.NewLine +
                    "    pos_freeweirflowcoeff = 12                  \t# Positive free weir flow (-)" + Environment.NewLine +
                    "    pos_drownweirflowcoeff= 13                  \t# Positive drowned weir flow (-)" + Environment.NewLine +
                    "    pos_contrcoeffreegate = 14                  \t# Positive flow contraction coefficient (-)" + Environment.NewLine +
                    "    neg_freegateflowcoeff = 15                  \t# Negative free gate flow (-)" + Environment.NewLine +
                    "    neg_drowngateflowcoeff= 16                  \t# Negative drowned gate flow (-)" + Environment.NewLine +
                    "    neg_freeweirflowcoeff = 17                  \t# Negative free weir flow (-)" + Environment.NewLine +
                    "    neg_drownweirflowcoeff= 18                  \t# Negative drowned weir flow (-)" + Environment.NewLine +
                    "    neg_contrcoeffreegate = 19                  \t# Negative flow contraction coefficient (-)" + Environment.NewLine +
                    "    extraresistance       = 21                  \t# Extra resistance (-)" + Environment.NewLine +
                    "    GateHeight            = 50                  \t# Vertical gate door height (m)" + Environment.NewLine +
                    expectedHorizontalGateOpeningWidthVal + Environment.NewLine +
                    "    GateOpeningHorizontalDirection= symmetric           \t# Horizontal direction of the opening gates" + Environment.NewLine;
                string fileContents = File.ReadAllText(writePath);

                Assert.That(fileContents, Is.EqualTo(expectedFileContents));
            });
        }

        private static void CheckMessages(IEnumerable<string> messages, string copyOfIniInTempFilePath)
        {
            Assert.That(messages, Has.Count.EqualTo(1), "Expected a single grouped warning message:");

            string msg = messages.First();

            var expectedMsgHeader =
                $"During reading the structures file ({copyOfIniInTempFilePath}), the following warnings were reported";
            Assert.That(msg, Does.StartWith(expectedMsgHeader), "Expected the header of the message to be different:");

            List<string> subMsgs = msg.Split(new[]
            {
                "\n- "
            }, StringSplitOptions.None).ToList();
            subMsgs.RemoveAt(0);                              // Remove header msg.
            subMsgs = subMsgs.Select(s => s.Trim()).ToList(); // Remove excessive white characters.

            Assert.That(subMsgs, Has.Count.EqualTo(2), "Expected 2 sub messages within the warning message.");
            Assert.That(subMsgs[0], Is.EqualTo(string.Format(
                                                   Resources.StructureFile_Property__0__not_supported_for_structures_of_type__1__and_is_skipped_Line__2__,
                                                   "weir1", "weir", 8)),
                        "Expected a different string as first sub message.");
            Assert.That(subMsgs[1], Is.EqualTo(string.Format(
                                                   Resources.StructureFile_Property__0__not_supported_for_structures_of_type__1__and_is_skipped_Line__2__,
                                                   "weir2", "weir", 9)),
                        "Expected a different string as second sub message.");
        }

        private static IStructure ValidatedWeir(IList<IStructureObject> structures)
        {
            Assert.AreEqual(1, structures.Count, "One structure was expected to be created.");
            var weir = structures.FirstOrDefault() as IStructure;
            Assert.NotNull(weir, "The structure was expected to be a weir.");
            return weir;
        }

        private static void ValidateCommonWeirProperties(IStructure weir, string expectedWeirName)
        {
            Assert.AreEqual(expectedWeirName, weir.Name,
                            "Name of weir was different than expected.");
            Assert.AreEqual(1, weir.CrestLevel,
                            "Crest level of weir was different than expected.");
            Assert.AreEqual(2, weir.CrestWidth,
                            "Crest width of weir was different than expected.");
        }

        private static void ValidateGatedWeirFormulaProperties(IStructure weir, string expectedWeirName)
        {
            ValidateCommonWeirProperties(weir, expectedWeirName);

            var gatedWeirFormula = weir.Formula as IGatedStructureFormula;
            Assert.NotNull(gatedWeirFormula,
                           "Expected a gated weir formula for weir.");
            Assert.AreEqual(3, gatedWeirFormula.GateLowerEdgeLevel,
                            "Lower edge level of weir was different than expected.");
            Assert.AreEqual(4, gatedWeirFormula.HorizontalGateOpeningWidth,
                            "Horizontal gate opening width of weir was different than expected.");
            Assert.AreEqual(5, gatedWeirFormula.GateHeight,
                            "Gate height of weir was different than expected.");
            Assert.AreEqual(GateOpeningDirection.Symmetric, gatedWeirFormula.GateOpeningHorizontalDirection,
                            "Gate opening direction of weir was different than expected.");
        }

        private void ValidateGeneralWeirFormulaProperties(IStructure weir, string expectedWeirName)
        {
            ValidateGatedWeirFormulaProperties(weir, expectedWeirName);

            var generalStructureFormula = weir.Formula as GeneralStructureFormula;
            Assert.NotNull(generalStructureFormula,
                           "Expected a general structure weir formula for weir.");
            Assert.AreEqual(6, generalStructureFormula.Upstream1Width,
                            "Upstream 1 Width of weir was different than expected.");
            Assert.AreEqual(7, generalStructureFormula.Upstream2Width,
                            "Upstream 2 Width of weir was different than expected.");
            Assert.AreEqual(8, generalStructureFormula.Downstream1Width,
                            "Downstream 1 Width of weir was different than expected.");
            Assert.AreEqual(9, generalStructureFormula.Downstream2Width,
                            "Downstream 2 Width of weir was different than expected.");
            Assert.AreEqual(10, generalStructureFormula.Upstream1Level,
                            "Upstream 1 Level of weir was different than expected.");
            Assert.AreEqual(11, generalStructureFormula.Upstream2Level,
                            "Upstream 2 Level of weir was different than expected.");
            Assert.AreEqual(12, generalStructureFormula.Downstream1Level,
                            "Downstream 1 Level of weir was different than expected.");
            Assert.AreEqual(13, generalStructureFormula.Downstream2Level,
                            "Downstream 2 Level of weir was different than expected.");
        }

        private void ValidateWrittenStructuresFile(string filePath)
        {
            IniData iniData;
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                iniData = new IniReader().ReadIniFile(fileStream, filePath);
            }

            Assert.AreEqual(3, iniData.SectionCount,
                            "3 sections were expected to be read from the structures file.");
            IniSection weirSection = GetSectionForStructureType(iniData, StructureRegion.StructureTypeName.Weir);
            Assert.IsNotNull(weirSection,
                             $"There was no INI section with structure type {StructureRegion.StructureTypeName.Weir}");
            ValidateCommonWeirIniProperties(weirSection);

            IniSection gateSection = GetSectionForStructureType(iniData, StructureRegion.StructureTypeName.Gate);
            Assert.IsNotNull(weirSection,
                             $"There was no INI section with structure type {StructureRegion.StructureTypeName.Gate}");
            ValidateCommonWeirIniProperties(gateSection);
            ValidateGateIniProperties(gateSection);

            IniSection generalStructureSection = GetSectionForStructureType(iniData, StructureRegion.StructureTypeName.GeneralStructure);
            Assert.IsNotNull(weirSection,
                             $"There was no INI section with structure type {StructureRegion.StructureTypeName.GeneralStructure}");
            ValidateCommonWeirIniProperties(generalStructureSection);
            ValidateGateIniProperties(gateSection);
            ValidateGeneralStructureIniProperties(generalStructureSection);
        }

        private void ValidateGeneralStructureIniProperties(IniSection section)
        {
            ValidateProperty(section, KnownGeneralStructureProperties.Upstream1Width.GetDescription(), "6");
            ValidateProperty(section, KnownGeneralStructureProperties.Upstream2Width.GetDescription(), "7");
            ValidateProperty(section, KnownGeneralStructureProperties.Downstream1Width.GetDescription(), "8");
            ValidateProperty(section, KnownGeneralStructureProperties.Downstream2Width.GetDescription(), "9");
            ValidateProperty(section, KnownGeneralStructureProperties.Upstream1Level.GetDescription(), "10");
            ValidateProperty(section, KnownGeneralStructureProperties.Upstream2Level.GetDescription(), "11");
            ValidateProperty(section, KnownGeneralStructureProperties.Downstream1Level.GetDescription(), "12");
            ValidateProperty(section, KnownGeneralStructureProperties.Downstream2Level.GetDescription(), "13");
        }

        private void ValidateGateIniProperties(IniSection section)
        {
            ValidateProperty(section, KnownStructureProperties.GateLowerEdgeLevel, "3");
            ValidateProperty(section, KnownStructureProperties.GateOpeningWidth, "4");
            ValidateProperty(section, KnownStructureProperties.GateHeight, "5");
            ValidateProperty(section, KnownStructureProperties.GateOpeningHorizontalDirection, "symmetric");
        }

        private void ValidateCommonWeirIniProperties(IniSection section)
        {
            ValidateProperty(section, KnownStructureProperties.CrestLevel, "1");
            ValidateProperty(section, KnownStructureProperties.CrestWidth, "2");
        }

        private static IniSection GetSectionForStructureType(IniData iniData, string type)
        {
            return iniData.Sections.FirstOrDefault(c => c.Properties.FirstOrDefault(p => p.IsKeyEqualTo(KnownStructureProperties.Type))?.Value == type);
        }

        private void ValidateProperty(IniSection section, string propertyKey, string expectedValue)
        {
            IniProperty property = section.Properties.FirstOrDefault(p => p.IsKeyEqualTo(propertyKey));
            Assert.NotNull(property,
                           $"There was no property '{propertyKey}' in section '{section.Name}' in the structures file.");
            Assert.AreEqual(expectedValue, property.Value,
                            $"Value of property '{propertyKey}' was not as expected.");
        }

        private string CreateStructuresFileWithPliFiles(string tempDirectoryPath, params IniSection[] sections)
        {
            string filePath = Path.Combine(tempDirectoryPath, "structures.ini");

            foreach (IniSection section in sections)
            {
                string structureName = section.Properties.FirstOrDefault(p => p.IsKeyEqualTo("id"))?.Value;
                WritePliFile(tempDirectoryPath, structureName);
            }

            var iniData = new IniData();
            iniData.AddMultipleSections(sections);
            
            new IniWriter().WriteIniFile(iniData, filePath);

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

        private static IniSection CreateOldGeneralStructureSection()
        {
            const string structureName = "general_structure";
            var properties = new List<IniProperty>
            {
                new IniProperty("type", "generalstructure"),
                new IniProperty("id", structureName),
                new IniProperty("polylinefile", $"{structureName}.pli"),
                new IniProperty("pos_freegateflowcoeff", "0"),
                new IniProperty("pos_drowngateflowcoeff", "0"),
                new IniProperty("pos_freeweirflowcoeff", "0"),
                new IniProperty("pos_drownweirflowcoeff", "0"),
                new IniProperty("pos_contrcoeffreegate", "0"),
                new IniProperty("neg_freegateflowcoeff", "0"),
                new IniProperty("neg_drowngateflowcoeff", "0"),
                new IniProperty("neg_freeweirflowcoeff", "0"),
                new IniProperty("neg_drownweirflowcoeff", "0"),
                new IniProperty("neg_contrcoeffreegate", "0"),
                new IniProperty("extraresistance", "0"),
                new IniProperty("levelcenter", "1"),
                new IniProperty("widthcenter", "2"),
                new IniProperty("gateheight", "3"),
                new IniProperty("door_opening_width", "4"),
                new IniProperty("gatedoorheight", "5"),
                new IniProperty("horizontal_opening_direction", "symmetric"),
                new IniProperty("widthleftW1", "6"),
                new IniProperty("widthleftWsdl", "7"),
                new IniProperty("widthrightWsdr", "8"),
                new IniProperty("widthrightW2", "9"),
                new IniProperty("levelleftZb1", "10"),
                new IniProperty("levelleftZbsl", "11"),
                new IniProperty("levelrightZbsr", "12"),
                new IniProperty("levelrightZb2", "13")
            };

            var generalStructureSection = new IniSection("structure");
            generalStructureSection.AddMultipleProperties(properties);

            return generalStructureSection;
        }

        private static IniSection CreateOldGateSection()
        {
            const string structureName = "gated_weir";

            var properties = new List<IniProperty>
            {
                new IniProperty("type", "gate"),
                new IniProperty("id", structureName),
                new IniProperty("polylinefile", $"{structureName}.pli"),
                new IniProperty("sill_level", "1"),
                new IniProperty("sill_width", "2"),
                new IniProperty("lower_edge_level", "3"),
                new IniProperty("opening_width", "4"),
                new IniProperty("door_height", "5"),
                new IniProperty("horizontal_opening_direction", "symmetric")
            };
            var section = new IniSection("structure");
            section.AddMultipleProperties(properties);

            return section;
        }

        private static IniSection CreateOldWeirSection()
        {
            const string structureName = "simple_weir";
            var properties = new List<IniProperty>
            {
                new IniProperty("type", "weir"),
                new IniProperty("id", structureName),
                new IniProperty("polylinefile", $"{structureName}.pli"),
                new IniProperty("lat_contr_coeff", "0"),
                new IniProperty("crest_level", "1"),
                new IniProperty("crest_width", "2")
            };
            var section = new IniSection("structure");
            section.AddMultipleProperties(properties);

            return section;
        }

        #region Sobek Structures

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAsSobekStructuresTest()
        {
            string path = TestHelper.GetTestFilePath(@"structures\example-structures-sobek.imp");

            var structureFile = new StructuresFile
            {
                StructureSchema = schema,
                ReferenceDate = new DateTime()
            };

            List<IStructureObject> structures = structureFile.Read(path).ToList();

            Assert.AreEqual(3, structures.Count); // There are 4 pumps in the file
            Assert.AreEqual(0, structures.OfType<IStructure>().Count());
            Assert.AreEqual(3, structures.OfType<IPump>().Count());

            IPump pump = structures.OfType<IPump>().First();
            Assert.AreEqual("pump01", pump.Name);
            Assert.AreEqual(new Point(500, 360), pump.Geometry);
            Assert.AreEqual(3.0, pump.Capacity);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTimeDependentSobekStructuresTest()
        {
            string path = TestHelper.GetTestFilePath(@"structures\time_dependent_structures.ini");

            var structureFile = new StructuresFile
            {
                StructureSchema = schema,
                ReferenceDate = new DateTime(2013, 1, 1)
            };

            List<IStructureObject> structures = structureFile.Read(path).ToList();

            Assert.AreEqual(3, structures.Count);
            Assert.AreEqual(2, structures.OfType<IStructure>().Count());
            Assert.AreEqual(1, structures.OfType<IPump>().Count());

            IPump pump = structures.OfType<IPump>().First();
            Assert.AreEqual(2, pump.CapacityTimeSeries.Time.Values.Count);
            Assert.AreEqual(5.6, pump.CapacityTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(11.12, pump.CapacityTimeSeries[new DateTime(2013, 1, 1, 1, 2, 0)]);

            IStructure weir = structures.OfType<IStructure>().First(w => w.Formula is SimpleWeirFormula);
            Assert.AreEqual(2, weir.CrestLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, weir.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, weir.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);

            IStructure gate = structures.OfType<IStructure>().First(w => w.Formula is SimpleGateFormula);
            var gateFormula = gate.Formula as SimpleGateFormula;
            Assert.NotNull(gateFormula);

            Assert.AreEqual(2, gateFormula.GateLowerEdgeLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, gateFormula.GateLowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, gateFormula.GateLowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);
            Assert.AreEqual(2, gateFormula.HorizontalGateOpeningWidthTimeSeries.Time.Values.Count);
            Assert.AreEqual(5.6, gateFormula.HorizontalGateOpeningWidthTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(7.8, gateFormula.HorizontalGateOpeningWidthTimeSeries[new DateTime(2013, 1, 1, 2, 3, 0)]);
        }

        #endregion

        #region Comparison helper methods for .ini files:

        private static void CompareStructureIniFiles(string iniFilePathA, string iniFilePathB)
        {
            using (var fileStreamA = new FileStream(iniFilePathA, FileMode.Open, FileAccess.Read))
            using (var fileStreamB = new FileStream(iniFilePathB, FileMode.Open, FileAccess.Read))
            {
                IniData iniDataA = new IniReader().ReadIniFile(fileStreamA, iniFilePathA);
                IniData iniDataB = new IniReader().ReadIniFile(fileStreamB, iniFilePathB);
                CompareSections(iniDataA.Sections.ToList(), iniDataB.Sections.ToList());
            }
        }

        private static void CompareSections(IList<IniSection> iniSectionsA, IList<IniSection> iniSectionsB)
        {
            Assert.AreEqual(iniSectionsA.Count, iniSectionsB.Count, "Expected the same number of categories.");
            for (var i = 0; i < iniSectionsA.Count; i++)
            {
                Assert.AreEqual(iniSectionsA[i].Name, iniSectionsB[i].Name, string.Format("Names are not the same at index = {0}.", i));
                CompareProperties(iniSectionsA[i].Properties.ToList(), iniSectionsB[i].Properties.ToList());
            }
        }

        private static void CompareProperties(IList<IniProperty> propertiesA, IList<IniProperty> propertiesB)
        {
            Assert.AreEqual(propertiesA.Count, propertiesB.Count, "Expected the same number of properties.");
            for (var i = 0; i < propertiesA.Count; i++)
            {
                Assert.AreEqual(propertiesA[i].Key, propertiesB[i].Key, string.Format("Names are not the same at index = {0}.", i));
                Assert.AreEqual(propertiesA[i].Value, propertiesB[i].Value, string.Format("Values are not the same at index = {0}.", i));
                // Don't care about comments
            }
        }

        #endregion

        #region Comparison helper methods for structure collections:

        private static void CompareStructures(IList<StructureDAO> structures, IList<StructureDAO> newStructures)
        {
            Assert.AreEqual(structures.Count, newStructures.Count, "Expected the same number of structures.");
            for (var i = 0; i < structures.Count; i++)
            {
                Assert.AreEqual(structures[i].StructureType, newStructures[i].StructureType, $"Expected same types at index {i}");
                CompareStructureProperties(structures[i].Properties, newStructures[i].Properties);
            }
        }

        private static void CompareStructureProperties(IList<ModelProperty> propertiesA, IList<ModelProperty> propertiesB)
        {
            Assert.AreEqual(propertiesA.Count, propertiesB.Count);
            for (var i = 0; i < propertiesA.Count; i++)
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