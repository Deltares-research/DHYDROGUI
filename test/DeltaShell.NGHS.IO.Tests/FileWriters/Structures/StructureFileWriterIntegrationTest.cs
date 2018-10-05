using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class StructureFileWriterIntegrationTest
    {
        [Test]
        public void TestStructureFileWriterGivesExpectedResults_CompoundStructureIDs()
        {
            var flow1Dmodel = new WaterFlowModel1D { Network = HydroNetworkHelper.GetSnakeHydroNetwork(1) };
            var branch = flow1Dmodel.Network.Channels.First();

            var pump = (IPump)new Pump(false);
            branch.BranchFeatures.Add(pump);

            var weir = new Weir();
            branch.BranchFeatures.Add(weir);

            var culvert = Culvert.CreateDefault();
            branch.BranchFeatures.Add(culvert);

            var composite1 = new CompositeBranchStructure("composite1", 0.5);
            branch.BranchFeatures.Add(composite1);
            composite1.Structures.Add(pump);
            pump.ParentStructure = composite1;
            composite1.Structures.Add(culvert);
            culvert.ParentStructure = composite1;

            var composite2 = new CompositeBranchStructure("composite2", 0.5);
            branch.BranchFeatures.Add(composite2);
            composite2.Structures.Add(weir);
            weir.ParentStructure = composite2;

            StructureFileWriter.WriteFile(FileWriterTestHelper.ModelFileNames.Structures, flow1Dmodel, WaterFlowModel1DFileWriter.GenerateFlow1DStructureCategoriesFrom1DModel);

            var categories = new DelftIniReader().ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.Structures).ToList();
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(3, categories.Count(op => op.Name == StructureRegion.Header));

            var pumpProperties = categories.First(c => c.Properties.Any(p => p.Name == StructureRegion.Id.Key && p.Value == pump.Name)).Properties;
            var weirProperties = categories.First(c => c.Properties.Any(p => p.Name == StructureRegion.Id.Key && p.Value == weir.Name)).Properties;
            var culvertProperties = categories.First(c => c.Properties.Any(p => p.Name == StructureRegion.Id.Key && p.Value == culvert.Name)).Properties;

            var pumpCompoundProperty = pumpProperties.First(p => p.Name == StructureRegion.Compound.Key);
            var weirCompoundProperty = weirProperties.First(p => p.Name == StructureRegion.Compound.Key);
            var culvertCompoundProperty = culvertProperties.First(p => p.Name == StructureRegion.Compound.Key);

            int pumpCompoundStructureId;
            Assert.IsTrue(int.TryParse(pumpCompoundProperty.Value, out pumpCompoundStructureId));
            Assert.IsTrue(pumpCompoundStructureId > 0, "A valid CompoundStructureId must be greater than zero");

            int weirCompoundStructureId;
            Assert.IsTrue(int.TryParse(weirCompoundProperty.Value, out weirCompoundStructureId));
            Assert.IsTrue(weirCompoundStructureId <= 0, "An individual structure should have a CompoundStructureId less than or equal to zero");

            Assert.AreEqual(pumpCompoundProperty.Value, culvertCompoundProperty.Value, "CompoundStructureIds should match for structure with the same parent CompoundStructure");

            var pumpCompoundNameProperty = pumpProperties.FirstOrDefault(p => p.Name == StructureRegion.CompoundName.Key);
            var weirCompoundNameProperty = weirProperties.FirstOrDefault(p => p.Name == StructureRegion.CompoundName.Key);
            var culvertCompoundNameProperty = culvertProperties.FirstOrDefault(p => p.Name == StructureRegion.CompoundName.Key);

            Assert.NotNull(pumpCompoundNameProperty);
            Assert.AreEqual(composite1.Name, pumpCompoundNameProperty.Value);

            //Names for compounds with only 1 sub - structure should not be written to file!
            Assert.Null(weirCompoundNameProperty);

            Assert.NotNull(culvertCompoundNameProperty);
            Assert.AreEqual(composite1.Name, culvertCompoundNameProperty.Value);
        }

        #region Write Pump

        [Test]
        public void GivenFmModelWithPump_WhenWritingStructures_ThenPumpIsBeingWrittenToIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var pumpName = "myPump";
            var expectedType = "pump";
            var expectedPliFileName = pumpName + ".pli";
            var expectedCapacity = 25.08;

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var pump2D = new Pump2D(pumpName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                Capacity = expectedCapacity,
            };
            fmModel.Area.Pumps.Add(pump2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, WaterFlowFMModelWriter.GenerateFlow2DStructureCategoriesFromFMModel);

                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(4));

                CheckCommon2DDelftIniProperties(structureCategory, pumpName, expectedType, expectedPliFileName);
                CheckKeyValuePair(structureCategory, StructureRegion.Capacity.Key, expectedCapacity);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        [Test]
        public void GivenFmModelWithPumpThatHasATimeSeriesForCapacity_WhenWritingStructures_ThenPumpIsBeingWrittenToFileWithTimeSeriesFileNameInIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var pumpName = "myPump";
            var expectedType = "pump";
            var expectedPliFileName = pumpName + ".pli";
            var expectedCapacityString = $"{pumpName}_{StructureRegion.Capacity.Key}.tim";

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var pump2D = new Pump2D(pumpName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                CanBeTimedependent = true,
                UseCapacityTimeSeries = true
            };
            fmModel.Area.Pumps.Add(pump2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, WaterFlowFMModelWriter.GenerateFlow2DStructureCategoriesFromFMModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(4));

                CheckCommon2DDelftIniProperties(structureCategory, pumpName, expectedType, expectedPliFileName);
                CheckKeyValuePair(structureCategory, StructureRegion.Capacity.Key, expectedCapacityString);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        #endregion

        #region Write Weir

        [Test]
        public void GivenFmModelWithWeirThatHasATimeSeriesForCrestLevel_WhenWritingStructures_ThenWeirIsBeingWrittenToFileWithTimeSeriesFileNameInIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var weirName = "myWeir";
            var expectedType = "weir";
            var expectedPliFileName = weirName + ".pli";
            var expectedCrestLevelString = $"{weirName}_crest_level.tim";
            var expectedCrestWidth = 2.58;
            var expectedLateralContraction = 0.34;

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var weir2D = new Weir2D(weirName, true)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                CrestWidth = expectedCrestWidth,
                WeirFormula = new SimpleWeirFormula { LateralContraction = expectedLateralContraction},
                UseCrestLevelTimeSeries = true
            };
            fmModel.Area.Weirs.Add(weir2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, WaterFlowFMModelWriter.GenerateFlow2DStructureCategoriesFromFMModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(6));

                CheckCommon2DDelftIniProperties(structureCategory, weirName, expectedType, expectedPliFileName);
                CheckKeyValuePair(structureCategory, StructureRegion.CrestLevel.Key, expectedCrestLevelString);
                CheckKeyValuePair(structureCategory, StructureRegion.CrestWidth.Key, expectedCrestWidth);
                CheckKeyValuePair(structureCategory, StructureRegion.LatContrCoeff.Key, expectedLateralContraction);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        [Test]
        public void GivenFmModelWithWeir_WhenWritingStructures_ThenWeirIsBeingCorrectlyWrittenToIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var weirName = "myWeir";
            var expectedType = "weir";
            var expectedPliFileName = weirName + ".pli";
            var expectedCrestLevel = 1.12;
            var expectedCrestWidth = 2.58;
            var expectedLateralContraction = 0.34;

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var weir2D = new Weir2D(weirName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                CrestLevel = expectedCrestLevel,
                CrestWidth = expectedCrestWidth,
                WeirFormula = new SimpleWeirFormula { LateralContraction = expectedLateralContraction }
            };
            fmModel.Area.Weirs.Add(weir2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, WaterFlowFMModelWriter.GenerateFlow2DStructureCategoriesFromFMModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(6));

                CheckCommon2DDelftIniProperties(structureCategory, weirName, expectedType, expectedPliFileName);
                CheckKeyValuePair(structureCategory, StructureRegion.CrestLevel.Key, expectedCrestLevel);
                CheckKeyValuePair(structureCategory, StructureRegion.CrestWidth.Key, expectedCrestWidth);
                CheckKeyValuePair(structureCategory, StructureRegion.LatContrCoeff.Key, expectedLateralContraction);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        #endregion

        #region Write General Structure

        [Test]
        public void GivenFmModelWithGeneralStructure_WhenWritingStructures_ThenGeneralStructureIsBeingCorrectlyWrittenToIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var generalStructureName = "myGeneralStructure";
            var expectedType = "generalstructure";
            var expectedPliFileName = generalStructureName + ".pli";
            var expectedCrestLevel = 1.12;
            var expectedCrestWidth = 2.58;

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };

            var weir2D = new Weir2D(generalStructureName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                CrestLevel = expectedCrestLevel,
                CrestWidth = expectedCrestWidth,
                WeirFormula = new GeneralStructureWeirFormula()
            };
            fmModel.Area.Weirs.Add(weir2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, WaterFlowFMModelWriter.GenerateFlow2DStructureCategoriesFromFMModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(26));

                CheckCommon2DDelftIniProperties(structureCategory, generalStructureName, expectedType, expectedPliFileName);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        #endregion

        #region Write Gate

        [Test]
        public void GivenFmModelWithGate_WhenWritingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var gateName = "myGate";
            var expectedType = "gate";
            var expectedPliFileName = gateName + ".pli";
            var expectedSillLevel = 1.12;
            var expectedSillWidth = 1.23;
            var expectedLowerEdgeLevel = 0.01;
            var expectedOpeningWidth = 5.11;
            var expectedHorizontalOpeningDirection = "from_right";

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var gate2D = new Gate2D(gateName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                SillLevel = expectedSillLevel,
                SillWidth = expectedSillWidth,
                LowerEdgeLevel = expectedLowerEdgeLevel,
                OpeningWidth = expectedOpeningWidth,
                HorizontalOpeningDirection = GateOpeningDirection.FromRight
            };
            fmModel.Area.Gates.Add(gate2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, WaterFlowFMModelWriter.GenerateFlow2DStructureCategoriesFromFMModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(8));

                CheckCommon2DDelftIniProperties(structureCategory, gateName, expectedType, expectedPliFileName);
                CheckKeyValuePair(structureCategory, StructureRegion.GateSillLevel.Key, expectedSillLevel);
                CheckKeyValuePair(structureCategory, StructureRegion.GateSillWidth.Key, expectedSillWidth);
                CheckKeyValuePair(structureCategory, StructureRegion.GateLowerEdgeLevel.Key, expectedLowerEdgeLevel);
                CheckKeyValuePair(structureCategory, StructureRegion.GateOpeningWidth.Key, expectedOpeningWidth);
                CheckKeyValuePair(structureCategory, StructureRegion.GateHorizontalOpeningDirection.Key, expectedHorizontalOpeningDirection);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        [Test]
        public void GivenFmModelWithGateThatHasSillLevelTimeSeries_WhenWritingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var gateName = "myGate";
            var timFileName = $"{gateName}_sill_level.tim";

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var gate2D = new Gate2D(gateName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                UseSillLevelTimeSeries = true
            };
            fmModel.Area.Gates.Add(gate2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, WaterFlowFMModelWriter.GenerateFlow2DStructureCategoriesFromFMModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                CheckKeyValuePair(structureCategory, StructureRegion.GateSillLevel.Key, timFileName);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        [Test]
        public void GivenFmModelWithGateThatHasLowerEdgeLevelTimeSeries_WhenWritingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var gateName = "myGate";
            var timFileName = $"{gateName}_{KnownStructureProperties.GateLowerEdgeLevel}.tim";

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var gate2D = new Gate2D(gateName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                UseLowerEdgeLevelTimeSeries = true
            };
            fmModel.Area.Gates.Add(gate2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, WaterFlowFMModelWriter.GenerateFlow2DStructureCategoriesFromFMModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                CheckKeyValuePair(structureCategory, StructureRegion.GateLowerEdgeLevel.Key, timFileName);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        [Test]
        public void GivenFmModelWithGateThatHasOpeningWidthTimeSeries_WhenWritingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var gateName = "myGate";
            var timFileName = $"{gateName}_{KnownStructureProperties.GateOpeningWidth}.tim";

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var gate2D = new Gate2D(gateName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                UseOpeningWidthTimeSeries = true
            };
            fmModel.Area.Gates.Add(gate2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, fmModel, WaterFlowFMModelWriter.GenerateFlow2DStructureCategoriesFromFMModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                CheckKeyValuePair(structureCategory, StructureRegion.GateOpeningWidth.Key, timFileName);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        #endregion

        private static void CheckCommon2DDelftIniProperties(DelftIniCategory structureCategory, string structureName, string expectedType, string expectedPliFileName)
        {
            CheckKeyValuePair(structureCategory, StructureRegion.Id.Key, structureName);
            CheckKeyValuePair(structureCategory, StructureRegion.DefinitionType.Key, expectedType);
            CheckKeyValuePair(structureCategory, StructureRegion.PolylineFile.Key, expectedPliFileName);
        }

        private static void CheckKeyValuePair(IDelftIniCategory structureCategory, string key, string expectedValue)
        {
            var property = structureCategory.Properties.FirstOrDefault(p => p.Name == key);
            Assert.That(property?.Value, Is.EqualTo(expectedValue));
        }

        private static void CheckKeyValuePair(IDelftIniCategory structureCategory, string key, double expectedValue)
        {
            var property = structureCategory.Properties.FirstOrDefault(p => p.Name == key);
            if (property != null)
            {
                var valueAsDouble = double.Parse(property.Value, CultureInfo.InvariantCulture);
                Assert.That(valueAsDouble, Is.EqualTo(expectedValue));
            }
            else
            {
                throw new AssertionException("The requested property was not present in the DelftIniCategory.");
            }
        }
    }
}