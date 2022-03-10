using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class StructureFileWriterIntegrationTest
    {
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
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>(){ fmModel.Network, fmModel.Area},fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);

                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(6));

                CheckCommon2DDelftIniProperties(structureCategory, pumpName, expectedType);
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
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>(){fmModel.Network, fmModel.Area},fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(6));

                CheckCommon2DDelftIniProperties(structureCategory, pumpName, expectedType);
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
            var expectedCrestLevelString = $"{weirName}_crest_level.tim";
            var expectedCrestWidth = 2.58;
            var expectedCorrectionCoeff = 0.34;

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var weir2D = new Weir2D(weirName, true)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                CrestWidth = expectedCrestWidth,
                WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = expectedCorrectionCoeff},
                UseCrestLevelTimeSeries = true
            };
            fmModel.Area.Weirs.Add(weir2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>(){fmModel.Network, fmModel.Area}, fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(9));

                CheckCommon2DDelftIniProperties(structureCategory, weirName, expectedType);
                CheckKeyValuePair(structureCategory, StructureRegion.CrestLevel.Key, expectedCrestLevelString);
                CheckKeyValuePair(structureCategory, StructureRegion.CrestWidth.Key, expectedCrestWidth);
                CheckKeyValuePair(structureCategory, StructureRegion.CorrectionCoeff.Key, expectedCorrectionCoeff);
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
            var expectedCrestLevel = 1.12;
            var expectedCrestWidth = 2.58;
            var expectedCorrectionCoef = 0.34;

            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath
            };
            var weir2D = new Weir2D(weirName)
            {
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                CrestLevel = expectedCrestLevel,
                CrestWidth = expectedCrestWidth,
                WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = expectedCorrectionCoef }
            };
            fmModel.Area.Weirs.Add(weir2D);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>(){fmModel.Network, fmModel.Area}, fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(9));

                CheckCommon2DDelftIniProperties(structureCategory, weirName, expectedType);
                CheckKeyValuePair(structureCategory, StructureRegion.CrestLevel.Key, expectedCrestLevel);
                CheckKeyValuePair(structureCategory, StructureRegion.CrestWidth.Key, expectedCrestWidth);
                CheckKeyValuePair(structureCategory, StructureRegion.CorrectionCoeff.Key, expectedCorrectionCoef);
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
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>() {fmModel.Network, fmModel.Area}, fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(32));

                CheckCommon2DDelftIniProperties(structureCategory, generalStructureName, expectedType);
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
            var expectedSillLevel = 1.12;
            var expectedSillWidth = 1.23;
            var expectedLowerEdgeLevel = 0.01;
            var expectedOpeningWidth = 5.11;
            var expectedHorizontalOpeningDirection = "fromRight";

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
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>(){fmModel.Network, fmModel.Area}, fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(11));

                CheckCommon2DDelftIniProperties(structureCategory, gateName, expectedType);
                CheckKeyValuePair(structureCategory, StructureRegion.GateCrestLevel.Key, expectedSillLevel);
                CheckKeyValuePair(structureCategory, StructureRegion.GateCrestWidth.Key, expectedSillWidth);
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
            var timFileName = $"{gateName}_crestLevel.tim";

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
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>() { fmModel.Network, fmModel.Area }, fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                CheckKeyValuePair(structureCategory, StructureRegion.GateCrestLevel.Key, timFileName);
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
            var timFileName = $"{gateName}_{StructureRegion.GateLowerEdgeLevel.Key}.tim";

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
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>() { fmModel.Network, fmModel.Area }, fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);
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
            var timFileName = $"{gateName}_{StructureRegion.GateOpeningWidth.Key}.tim";

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
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>() { fmModel.Network, fmModel.Area }, fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);
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

        #region Write Levee Breach

        [Test]
        public void GivenFmModelWithLeveeBreach_WhenWritingStructures_ThenLeveeBreachIsBeingCorrectlyWrittenToIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var leveeBreachName = "myBreach";
            var expectedBreachLocationX = 1.1;
            var expectedBreachLocationY = 1.1;
            var expectedStartTimeBreachGrowth = 7200;
            var expectedBreachGrowthActivated = "0";

            var referenceTime = new DateTime(2018, 8, 25);
            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                ReferenceTime = referenceTime
            };
            var leveeBreach = new LeveeBreach
            {
                Name = leveeBreachName,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                BreachLocationX = expectedBreachLocationX,
                BreachLocationY = expectedBreachLocationY
            };
            leveeBreach.SetBaseLeveeBreachSettings(referenceTime.AddHours(2.0), false);
            fmModel.Area.LeveeBreaches.Add(leveeBreach);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>() { fmModel.Network, fmModel.Area }, fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(9));

                CheckKeyValuePair(structureCategory, StructureRegion.BreachLocationX.Key, expectedBreachLocationX);
                CheckKeyValuePair(structureCategory, StructureRegion.BreachLocationY.Key, expectedBreachLocationY);
                CheckKeyValuePair(structureCategory, StructureRegion.StartTimeBreachGrowth.Key, expectedStartTimeBreachGrowth);
                CheckKeyValuePair(structureCategory, StructureRegion.BreachGrowthActivated.Key, expectedBreachGrowthActivated);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        [Test]
        public void GivenFmModelWithLeveeBreachThatHasVerheijAsGrowthFormula_WhenWritingStructures_ThenLeveeBreachIsBeingCorrectlyWrittenToIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var leveeBreachName = "myBreach";
            var expectedBreachLocationX = 1.1;
            var expectedBreachLocationY = 1.1;
            var expectedStartTimeBreachGrowth = 7200;
            var expectedBreachGrowthActivated = "1";
            var expectedAlgorithmValue = (int) LeveeBreachGrowthFormula.VerheijvdKnaap2002;
            var expectedSettingsValue = 1.09;
            var expectedTimeToReachMinimumCrestLevel = 3600;

            var referenceTime = new DateTime(2018, 8, 25);
            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                ReferenceTime = referenceTime
            };
            var leveeBreach = new LeveeBreach
            {
                Name = leveeBreachName,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                BreachLocationX = expectedBreachLocationX,
                BreachLocationY = expectedBreachLocationY,
                LeveeBreachFormula = LeveeBreachGrowthFormula.VerheijvdKnaap2002
            };
            leveeBreach.SetBaseLeveeBreachSettings(referenceTime.AddHours(2.0), true);

            var leveeBreachSettings = leveeBreach.GetActiveLeveeBreachSettings() as VerheijVdKnaap2002BreachSettings;
            Assert.IsNotNull(leveeBreachSettings);
            leveeBreachSettings.InitialCrestLevel = expectedSettingsValue;
            leveeBreachSettings.MinimumCrestLevel = expectedSettingsValue;
            leveeBreachSettings.InitialBreachWidth = expectedSettingsValue;
            leveeBreachSettings.PeriodToReachZmin = new TimeSpan(0, 1, 0, 0);
            leveeBreachSettings.Factor1Alfa = expectedSettingsValue;
            leveeBreachSettings.Factor2Beta = expectedSettingsValue;
            leveeBreachSettings.CriticalFlowVelocity = expectedSettingsValue;

            fmModel.Area.LeveeBreaches.Add(leveeBreach);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>(){fmModel.Network, fmModel.Area}, fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(17));

                CheckKeyValuePair(structureCategory, StructureRegion.BreachLocationX.Key, expectedBreachLocationX);
                CheckKeyValuePair(structureCategory, StructureRegion.BreachLocationY.Key, expectedBreachLocationY);
                CheckKeyValuePair(structureCategory, StructureRegion.StartTimeBreachGrowth.Key, expectedStartTimeBreachGrowth);
                CheckKeyValuePair(structureCategory, StructureRegion.BreachGrowthActivated.Key, expectedBreachGrowthActivated);

                CheckKeyValuePair(structureCategory, StructureRegion.Algorithm.Key, expectedAlgorithmValue);
                CheckKeyValuePair(structureCategory, StructureRegion.InitialCrestLevel.Key, expectedSettingsValue);
                CheckKeyValuePair(structureCategory, StructureRegion.MinimumCrestLevel.Key, expectedSettingsValue);
                CheckKeyValuePair(structureCategory, StructureRegion.InitalBreachWidth.Key, expectedSettingsValue);
                CheckKeyValuePair(structureCategory, StructureRegion.TimeToReachMinimumCrestLevel.Key, expectedTimeToReachMinimumCrestLevel);
                CheckKeyValuePair(structureCategory, StructureRegion.Factor1.Key, expectedSettingsValue);
                CheckKeyValuePair(structureCategory, StructureRegion.Factor2.Key, expectedSettingsValue);
                CheckKeyValuePair(structureCategory, StructureRegion.CriticalFlowVelocity.Key, expectedSettingsValue);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        [Test]
        public void GivenFmModelWithLeveeBreachThatHasUserDefinedFormula_WhenWritingStructures_ThenLeveeBreachIsBeingCorrectlyWrittenToIniFile()
        {
            var testFolder = FileUtils.CreateTempDirectory();
            var structuresFilePath = Path.Combine(testFolder, "structures.ini");
            var mduFilePath = Path.Combine(testFolder, "FlowFM.mdu");

            var expectedCategoryName = "Structure";
            var expectedAlgorithmValue = (int) LeveeBreachGrowthFormula.UserDefinedBreach;
            var leveeBreachName = "myBreach";
            var timeSeriesFileName = $"{leveeBreachName}.tim";

            var referenceTime = new DateTime(2018, 8, 25);
            var fmModel = new WaterFlowFMModel
            {
                MduFilePath = mduFilePath,
                ReferenceTime = referenceTime
            };
            var leveeBreach = new LeveeBreach
            {
                Name = leveeBreachName,
                Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(2, 2) }),
                LeveeBreachFormula = LeveeBreachGrowthFormula.UserDefinedBreach
            };
            leveeBreach.SetBaseLeveeBreachSettings(referenceTime.AddHours(2.0), true);
            fmModel.Area.LeveeBreaches.Add(leveeBreach);

            try
            {
                StructureFileWriter.WriteFile(structuresFilePath, new List<IHydroRegion>() { fmModel.Network, fmModel.Area }, fmModel.ReferenceTime, string.IsNullOrEmpty(fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath)?.GetValueAsString()) ? fmModel.MduFilePath : fmModel.ModelDefinition.GetModelProperty(GuiProperties.TargetMduPath).GetValueAsString(), StructureFile.Generate2DStructureCategoriesFromFmModel);
                var categories = new DelftIniReader().ReadDelftIniFile(structuresFilePath);
                Assert.That(categories.Count, Is.EqualTo(2));

                var structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
                Assert.IsNotNull(structureCategory);
                Assert.That(structureCategory.Properties.Count, Is.EqualTo(11));

                CheckKeyValuePair(structureCategory, StructureRegion.Algorithm.Key, expectedAlgorithmValue);
                CheckKeyValuePair(structureCategory, StructureRegion.TimeFileName.Key, timeSeriesFileName);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(structuresFilePath));
            }
        }

        #endregion

        private static void CheckCommon2DDelftIniProperties(DelftIniCategory structureCategory, string structureName, string expectedType)
        {
            CheckKeyValuePair(structureCategory, StructureRegion.Id.Key, structureName);
            CheckKeyValuePair(structureCategory, StructureRegion.DefinitionType.Key, expectedType);
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
                throw new AssertionException($"The requested property with name \"{key}\" was not present in the DelftIniCategory.");
            }
        }
    }
}