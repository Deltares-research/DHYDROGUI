using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using log4net.Core;
using NetTopologySuite.Extensions.Grids;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess
{
    [TestFixture]
    public class MdwFileTest
    {
        private readonly string testDataPath = Path.Combine(TestHelper.GetTestDataDirectory(), nameof(MdwFileTest));

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAndWriteMdwFile()
        {
            using (var temp = new TemporaryDirectory())
            {
                string mdwPath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary\obw.mdw");
                string mdwTargetPath = Path.Combine(temp.Path, "obw_compare.mdw");

                var mdwFile = new MdwFile();
                MdwFileDTO dto = mdwFile.Load(mdwPath);

                mdwFile.SaveTo(mdwTargetPath, dto, true);

                var target = new MdwFile();
                MdwFileDTO modelDTOOut = target.Load(mdwTargetPath);

                foreach (WaveModelPropertyDefinition propDef in dto.WaveModelDefinition.ModelSchema.PropertyDefinitions.Values)
                {
                    object valueBefore = dto.WaveModelDefinition.GetModelProperty(propDef.FileCategoryName, propDef.FilePropertyName).Value;
                    object valueAfter = modelDTOOut.WaveModelDefinition.GetModelProperty(propDef.FileCategoryName, propDef.FilePropertyName).Value;
                    Assert.AreEqual(valueBefore, valueAfter);
                }
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_GridsAreImportedOnDomains()
        {
            // Setup
            string mdwPath = TestHelper.GetTestFilePath(@"coordinateBasedBoundary\obw.mdw");

            // Call
            MdwFileDTO dto = new MdwFile().Load(mdwPath);

            // Assert
            Assert.That(dto.WaveModelDefinition.OuterDomain.Grid.IsEmpty, Is.False);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadObstacles()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_spacevarbnd\tst.mdw");
            var mdwFile = new MdwFile();
            MdwFileDTO dto = mdwFile.Load(mdwPath);
            WaveModelDefinition modelDef = dto.WaveModelDefinition;

            WaveObstacle obs1 = modelDef.FeatureContainer.Obstacles[0];
            WaveObstacle obs2 = modelDef.FeatureContainer.Obstacles[1];

            Assert.AreEqual("Obstacle 1", obs1.Name);
            Assert.AreEqual(ObstacleType.Dam, obs1.Type);
            Assert.AreEqual(0, obs1.Height, 1e-05);
            Assert.AreEqual(2.5999, obs1.Alpha, 1e-03);
            Assert.AreEqual(0.15, obs1.Beta, 1e-05);
            Assert.AreEqual(0, obs1.TransmissionCoefficient, 1e-05);
            Assert.AreEqual(ReflectionType.No, obs1.ReflectionType);

            Assert.AreEqual("Obstacle 2", obs2.Name);
            Assert.AreEqual(ObstacleType.Sheet, obs2.Type);
            Assert.AreEqual(0, obs2.Height, 1e-05);
            Assert.AreEqual(0, obs2.Alpha, 1e-05);
            Assert.AreEqual(0, obs2.Beta, 1e-05);
            Assert.AreEqual(0.5, obs2.TransmissionCoefficient, 1e-05);
            Assert.AreEqual(ReflectionType.No, obs2.ReflectionType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadObstaclesInvalidFile()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_spacevarbnd\tstInvalidObtFile.mdw");
            var mdwFile = new MdwFile();

            const string warningMessage = "Parsing error in file 'tstInvalid.obt'. Can't convert 'ThisInvalidString' to a double. The property 'Beta' has been given the default value '0'.";

            MdwFileDTO dto = null;
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Warn);
            TestHelper.AssertLogMessageIsGenerated(
                () => dto = mdwFile.Load(mdwPath), warningMessage);
            LogHelper.SetLoggingLevel(Level.Error);
            LogHelper.ResetLogging();

            WaveModelDefinition modelDef = dto.WaveModelDefinition;
            WaveObstacle obs1 = modelDef.FeatureContainer.Obstacles[0];
            WaveObstacle obs2 = modelDef.FeatureContainer.Obstacles[1];

            Assert.AreEqual("Obstacle 1", obs1.Name);
            Assert.AreEqual(ObstacleType.Dam, obs1.Type);
            Assert.AreEqual(0, obs1.Height, 1e-05);
            Assert.AreEqual(2.5999, obs1.Alpha, 1e-03);
            Assert.AreEqual(0, obs1.Beta, 1e-05); // The Beta value in the file is "ThisInvalidString" -> reading this should return the default value (0)
            Assert.AreEqual(0, obs1.TransmissionCoefficient, 1e-05);
            Assert.AreEqual(ReflectionType.No, obs1.ReflectionType);

            Assert.AreEqual("Obstacle 2", obs2.Name);
            Assert.AreEqual(ObstacleType.Sheet, obs2.Type);
            Assert.AreEqual(0, obs2.Height, 1e-05);
            Assert.AreEqual(0, obs2.Alpha, 1e-05);
            Assert.AreEqual(0, obs2.Beta, 1e-05);
            Assert.AreEqual(0.5, obs2.TransmissionCoefficient, 1e-05);
            Assert.AreEqual(ReflectionType.No, obs2.ReflectionType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTimePoints()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            var mdwFile = new MdwFile();
            MdwFileDTO dto = mdwFile.Load(mdwPath);
            IFunction function = dto.TimeFrameData.TimeVaryingData;

            Assert.AreEqual(3, function.Arguments[0].Values.Count);
            Assert.AreEqual(new DateTime(2006, 1, 5), function.Arguments[0].Values[0]);
            Assert.AreEqual(0.0, function.Components[0].Values[0]);
            Assert.AreEqual(0.0, function.Components[1].Values[0]);
            Assert.AreEqual(new DateTime(2006, 1, 5).AddMinutes(60.0), function.Arguments[0].Values[1]);
            Assert.AreEqual(new DateTime(2006, 1, 5).AddMinutes(120.0), function.Arguments[0].Values[2]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveLoadSpectralSpacePerDomain()
        {
            using (var temp = new TemporaryDirectory())
            {
                string mdwPath = TestHelper.GetTestFilePath(@"domainWithSpectralData\te0.mdw");
                var mdwFile = new MdwFile();

                MdwFileDTO dto = mdwFile.Load(mdwPath);
                WaveModelDefinition modelDef = dto.WaveModelDefinition;

                string targetPath = Path.Combine(temp.Path, "output.mdw");
                mdwFile.SaveTo(targetPath, dto, true);

                MdwFileDTO savedDto = mdwFile.Load(targetPath);
                WaveModelDefinition savedModelDef = savedDto.WaveModelDefinition;

                Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.NFreq, savedModelDef.OuterDomain.SpectralDomainData.NFreq);
                Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.FreqMax, savedModelDef.OuterDomain.SpectralDomainData.FreqMax, 1e-07);
                Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.FreqMin, savedModelDef.OuterDomain.SpectralDomainData.FreqMin, 1e-07);
                Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.NDir, savedModelDef.OuterDomain.SpectralDomainData.NDir);
                Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.StartDir, savedModelDef.OuterDomain.SpectralDomainData.StartDir, 1e-07);
                Assert.AreEqual(modelDef.OuterDomain.SpectralDomainData.EndDir, savedModelDef.OuterDomain.SpectralDomainData.EndDir, 1e-07);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_ForUniformConstantBoundary_MdwFileShouldContainBoundaryCategoryWithoutBcwFile()
        {
            // Arrange
            WaveModelDefinition modelDefinition = CreateWaveModelDefinition();
            var timeFrameData = new TimeFrameData();

            UniformDataComponent<ConstantParameters<PowerDefinedSpreading>> uniformComponent = CreateUniformConstantDataComponent();
            IWaveBoundary boundary = BuildWaveBoundary(uniformComponent);

            modelDefinition.BoundaryContainer.Boundaries.Add(boundary);

            using (var tempDirectory = new TemporaryDirectory())
            {
                string targetPath = Path.Combine(tempDirectory.Path, "output.mdw");

                // Act
                new MdwFile().SaveTo(targetPath, new MdwFileDTO(modelDefinition, timeFrameData), true);

                // Assert
                Assert.IsTrue(File.Exists(targetPath));
                Assert.IsFalse(File.Exists(Path.Combine(tempDirectory.Path, "output.bcw")));

                string[] lines = File.ReadAllLines(targetPath);

                Assert.IsTrue(lines.Any(line => line.Contains(KnownWaveCategories.BoundaryCategory)));
                Assert.IsFalse(lines.Any(line => line.Contains(KnownWaveProperties.TimeSeriesFile)));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_ForUniformTimeDependentBoundary_MdwFileShouldContainBoundaryCategoryWithBcwFile()
        {
            // Arrange
            WaveModelDefinition modelDefinition = CreateWaveModelDefinition();
            var timeFrameData = new TimeFrameData();

            UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>> spatiallyVaryingComponent = CreateUniformTimeDependentDataComponent();
            IWaveBoundary boundary = BuildWaveBoundary(spatiallyVaryingComponent);

            modelDefinition.BoundaryContainer.Boundaries.Add(boundary);

            using (var tempDirectory = new TemporaryDirectory())
            {
                string targetPath = Path.Combine(tempDirectory.Path, "output.mdw");

                // Act
                new MdwFile().SaveTo(targetPath, new MdwFileDTO(modelDefinition, timeFrameData), false);

                // Assert
                Assert.IsTrue(File.Exists(targetPath));
                Assert.IsTrue(File.Exists(Path.Combine(tempDirectory.Path, "output.bcw")));

                string[] lines = File.ReadAllLines(targetPath);

                Assert.IsTrue(lines.Any(line => line.Contains(KnownWaveCategories.BoundaryCategory)));

                string timeSeriesFileNameLine = lines.Single(line => line.Contains(KnownWaveProperties.TimeSeriesFile));
                string timeSeriesFileNameMentioned = timeSeriesFileNameLine.Split('=').Last().Trim();
                Assert.AreEqual("output.bcw", timeSeriesFileNameMentioned);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_ForSpatiallyVaryingFileBasedConstantBoundary_WithEmptyFilePaths_MdwFileShouldContainCorrectBoundaryCategory()
        {
            // Setup
            WaveModelDefinition modelDefinition = CreateWaveModelDefinition();
            var timeFrameData = new TimeFrameData();

            using (var tempDirectory = new TemporaryDirectory())
            {
                var dataComponent = new SpatiallyVaryingDataComponent<FileBasedParameters>();
                IWaveBoundary boundary = BuildWaveBoundary(dataComponent);
                IEventedList<SupportPoint> supportPoints = boundary.GeometricDefinition.SupportPoints;
                dataComponent.AddParameters(supportPoints[0], new FileBasedParameters(string.Empty));
                dataComponent.AddParameters(supportPoints[1], new FileBasedParameters(string.Empty));

                modelDefinition.BoundaryContainer.Boundaries.Add(boundary);
                string saveFilePath = Path.Combine(tempDirectory.Path, "output.mdw");

                // Call
                new MdwFile().SaveTo(saveFilePath, new MdwFileDTO(modelDefinition, timeFrameData), true);

                // Assert
                Assert.That(saveFilePath, Does.Exist);

                string[] lines = GetBoundaryLines(saveFilePath);
                Assert.That(lines, Has.Length.EqualTo(11));
                AssertPropertyLine(lines[0], KnownWaveProperties.Name, "boundary_name");
                AssertPropertyLine(lines[1], KnownWaveProperties.Definition, "xy-coordinates");
                AssertPropertyLine(lines[2], KnownWaveProperties.StartCoordinateX, "0.0000000");
                AssertPropertyLine(lines[3], KnownWaveProperties.EndCoordinateX, "9.0000000");
                AssertPropertyLine(lines[4], KnownWaveProperties.StartCoordinateY, "0.0000000");
                AssertPropertyLine(lines[5], KnownWaveProperties.EndCoordinateY, "9.0000000");
                AssertPropertyLine(lines[6], KnownWaveProperties.SpectrumSpec, "from file");
                AssertPropertyLine(lines[7], KnownWaveProperties.CondSpecAtDist, "0.0000000");
                AssertPropertyLine(lines[8], KnownWaveProperties.Spectrum, string.Empty);
                AssertPropertyLine(lines[9], KnownWaveProperties.CondSpecAtDist, "10.0000000");
                AssertPropertyLine(lines[10], KnownWaveProperties.Spectrum, string.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAMdwFileWithObstacleFile_WhenImportedAndObstaclesRemoved_ThenObstacleFileShouldBeRemovedFromTheModeldefinitionProperties()
        {
            using (var temp = new TemporaryDirectory())
            {
                var mdwFile = new MdwFile();
                string importedMdwFilePath = TestHelper.GetTestFilePath(@"wad\wad.mdw");

                MdwFileDTO dto = mdwFile.Load(importedMdwFilePath);
                WaveModelDefinition modelDef = dto.WaveModelDefinition;

                string targetPath = Path.Combine(temp.Path, "output.mdw");

                Assert.AreEqual("wad.obt",
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.ObstacleFile)
                                        .GetValueAsString());

                modelDef.FeatureContainer.Obstacles.Clear();

                // Model definition properties updated during save
                mdwFile.SaveTo(targetPath, dto, true);

                Assert.AreEqual(string.Empty,
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.ObstacleFile)
                                        .GetValueAsString());

                // Verify what was really written in the file
                MdwFileDTO dto2 = mdwFile.Load(targetPath);
                WaveModelDefinition modelDef2 = dto2.WaveModelDefinition;

                Assert.AreEqual(string.Empty,
                                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.ObstacleFile)
                                         .GetValueAsString());
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAMdwFileWithConstantWind_WhenImportedAndChangedToTimeSeries_ThenZerosShouldBeWrittenForWindSpeedAndDirectionInTheModelDefinitionProperties()
        {
            using (var temp = new TemporaryDirectory())
            {
                var mdwFile = new MdwFile();
                string importedMdwFilePath = TestHelper.GetTestFilePath(@"wad\wad.mdw");

                MdwFileDTO dto = mdwFile.Load(importedMdwFilePath);
                WaveModelDefinition modelDef = dto.WaveModelDefinition;

                string targetPath = Path.Combine(temp.Path, "output.mdw");

                Assert.AreEqual("10",
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindSpeed)
                                        .GetValueAsString());

                Assert.AreEqual("315",
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindDirection)
                                        .GetValueAsString());

                var timeFrameData = dto.TimeFrameData;
                timeFrameData.WindInputDataType = WindInputDataType.TimeVarying;

                // Model definition properties updated during save
                mdwFile.SaveTo(targetPath, dto, true);

                Assert.AreEqual("0",
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindSpeed)
                                        .GetValueAsString());

                Assert.AreEqual("0",
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindDirection)
                                        .GetValueAsString());

                // Verify what was really written in the file
                MdwFileDTO dto2 = mdwFile.Load(targetPath);
                WaveModelDefinition modelDef2 = dto2.WaveModelDefinition;

                Assert.AreEqual("0",
                                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindSpeed)
                                         .GetValueAsString());

                Assert.AreEqual("0",
                                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WindDirection)
                                         .GetValueAsString());
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAWaveModelWithConstantHydronamics_WhenChangedToTimeSeries_ThenZerosShouldBeWrittenForConstantWaterLevelVelocityXAndVelocityYInTheModelDefinitionProperties()
        {
            using (var temp = new TemporaryDirectory())
            {
                var mdwFile = new MdwFile();
                var modelDef = new WaveModelDefinition { OuterDomain = new WaveDomainData("Outer") };
                string targetPath = Path.Combine(temp.Path, "output.mdw");

                var timeFrameData = new TimeFrameData();

                timeFrameData.HydrodynamicsInputDataType = HydrodynamicsInputDataType.Constant;
                timeFrameData.HydrodynamicsConstantData.WaterLevel = 6;
                timeFrameData.HydrodynamicsConstantData.VelocityX = 6;
                timeFrameData.HydrodynamicsConstantData.VelocityY = 6;

                // update model definition properties
                mdwFile.SaveTo(targetPath, new MdwFileDTO(modelDef, timeFrameData), true);

                Assert.AreEqual("6",
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterLevel)
                                        .GetValueAsString());

                Assert.AreEqual("6",
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                                        .GetValueAsString());

                Assert.AreEqual("6",
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                                        .GetValueAsString());

                timeFrameData.HydrodynamicsInputDataType = HydrodynamicsInputDataType.TimeVarying;

                mdwFile.SaveTo(targetPath, new MdwFileDTO(modelDef, timeFrameData), true);

                Assert.AreEqual("0",
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterLevel)
                                        .GetValueAsString());

                Assert.AreEqual("0",
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                                        .GetValueAsString());

                Assert.AreEqual("0",
                                modelDef.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                                        .GetValueAsString());

                //Verify what was really written in the file
                MdwFileDTO resultDto = mdwFile.Load(targetPath);
                WaveModelDefinition modelDef2 = resultDto.WaveModelDefinition;

                Assert.AreEqual("0",
                                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterLevel)
                                         .GetValueAsString());

                Assert.AreEqual("0",
                                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                                         .GetValueAsString());

                Assert.AreEqual("0",
                                modelDef2.GetModelProperty(KnownWaveCategories.GeneralCategory, KnownWaveProperties.WaterVelocityX)
                                         .GetValueAsString());
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAWaveModelWithObservationPoints_WhenImportedAndObservationsPointsRemoved_ThenLocationFileWithObservationPointsShouldBeRemovedFromTheModelDefinitionProperties()
        {
            using (var temp = new TemporaryDirectory())
            {
                var mdwFile = new MdwFile();
                string importedMdwFilePath = TestHelper.GetTestFilePath(@"wad\wad.mdw");

                MdwFileDTO dto = mdwFile.Load(importedMdwFilePath);
                WaveModelDefinition modelDef = dto.WaveModelDefinition;

                string targetPath = Path.Combine(temp.Path, "output.mdw");

                Assert.AreEqual("wad.loc",
                                modelDef.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.LocationFile)
                                        .GetValueAsString());

                modelDef.FeatureContainer.ObservationPoints.Clear();

                // Model definition properties updated during save
                mdwFile.SaveTo(targetPath, dto, true);

                Assert.AreEqual(string.Empty,
                                modelDef.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.LocationFile)
                                        .GetValueAsString());

                // Verify what was really written in the file
                MdwFileDTO dto2 = mdwFile.Load(targetPath);
                WaveModelDefinition modelDef2 = dto2.WaveModelDefinition;

                Assert.AreEqual(string.Empty,
                                modelDef2.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.LocationFile)
                                         .GetValueAsString());
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAMdwFileWithMissingBedFrictionCoefAndMaxIter_WhenImportingThisModel_ThenTheCorrectDefaultValuesShouldBeSetBasedOnBedFrictionAndSimMode()
        {
            var mdwFile = new MdwFile();
            string importedMdwFilePath = TestHelper.GetTestFilePath(@"ModelWithMissingMultipleDefaultValues\Waves.mdw");

            void Action()
            {
                MdwFileDTO dto = mdwFile.Load(importedMdwFilePath);
                WaveModelDefinition modelDefinition = dto.WaveModelDefinition;

                WaveModelProperty propertyBedFrictionCoef = modelDefinition.GetModelProperty(KnownWaveCategories.ProcessesCategory, KnownWaveProperties.BedFrictionCoef);
                Assert.IsNotNull(propertyBedFrictionCoef);
                Assert.AreEqual("0.05", propertyBedFrictionCoef.GetValueAsString());

                WaveModelProperty propertyMaxIter = modelDefinition.GetModelProperty(KnownWaveCategories.NumericsCategory, KnownWaveProperties.MaxIter);
                Assert.IsNotNull(propertyMaxIter);
                Assert.AreEqual("15", propertyMaxIter.GetValueAsString());
            }

            string logMessage = TestHelper.GetAllRenderedMessages(Action).Single();

            Assert.That(logMessage, new ContainsConstraint("- In the MDW file the property BedFricCoef is missing. Based on property BedFriction the default value is set"));
            Assert.That(logMessage, new ContainsConstraint("- In the MDW file the property MaxIter is missing. Based on property SimMode the default value is set"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_WithCommunicationFilePathWithBackSlashFileSeparators_ThenFilePathIsExportedWithForwardSlashFileSeparators()
        {
            // Arrange
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                const string comFilePath = @"myDir1\myDir2\myComFile_com.nc";
                var modelDefinition = new WaveModelDefinition { CommunicationsFilePath = comFilePath };
                var timeFrameData = new TimeFrameData();

                // Act
                var mdwFile = new MdwFile();
                string mdwFilePath = Path.Combine(temporaryDirectory.Path, "myModel.mdw");
                mdwFile.SaveTo(mdwFilePath, new MdwFileDTO(modelDefinition, timeFrameData), false);

                // Assert
                IEnumerable<string> mdwFileLines = File.ReadLines(mdwFilePath);
                string comFileLine = mdwFileLines.Single(line => line.Trim().StartsWith(KnownWaveProperties.COMFile));

                string exportedComFilePath = comFileLine.Split('=')[1].Trim();
                Assert.That(exportedComFilePath, Is.EqualTo(comFilePath.Replace('\\', '/')));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Load_LegacyPropertiesAreReplaced()
        {
            // Setup
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string legacyFile = temporaryDirectory.CopyTestDataFileToTempDirectory("MdwFile\\TScaleLegacy.mdw");

                var mdwFile = new MdwFile();

                // Call
                MdwFileDTO resultDto = null;
                void Call() => resultDto = mdwFile.Load(legacyFile);

                List<string> logMessages =
                    TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToList();

                // Assert
                WaveModelDefinition result = resultDto.WaveModelDefinition;
                WaveModelProperty timeFrameProperty =
                    result.Properties.FirstOrDefault(x => x.PropertyDefinition.FilePropertyName == "TimeInterval");

                Assert.That(timeFrameProperty, Is.Not.Null, "Expected the TimeFrame property to be found.");
                Assert.That(timeFrameProperty.PropertyDefinition.Category, Is.EqualTo("General"));

                var value = (double)timeFrameProperty.Value;
                Assert.That(value, Is.EqualTo(255.0));

                Assert.That(result.Properties.Any(x => x.PropertyDefinition.FilePropertyName == "TScale"),
                            Is.False,
                            "Expected no property with the file name TScale");

                string expectedMsg = string.Format(
                    Resources.DelftIniBackwardsCompatibilityHelper_GetUpdatedName_Backwards_Compatibility____0___has_been_updated_to___1__,
                    "TScale", "TimeInterval");
                Assert.That(logMessages.Any(x => x.Contains(expectedMsg)), Is.True, "Expected a warning messages logged.");
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_ConstantUniformBoundary_LoadsBoundaryCorrectly()
        {
            // Setup
            string mdwPath = TestHelper.GetTestFilePath(@"read_wave_boundaries\constant-uniform.mdw");

            // Call
            MdwFileDTO dto = new MdwFile().Load(mdwPath);
            WaveModelDefinition modelDefinition = dto.WaveModelDefinition;

            // Assert
            IWaveBoundary boundary = modelDefinition.BoundaryContainer.Boundaries.Single();
            Assert.That(boundary.Name, Is.EqualTo("boundary_name"));
            IWaveBoundaryGeometricDefinition geometricDefinition = boundary.GeometricDefinition;
            Assert.That(geometricDefinition, Is.Not.Null);
            IWaveBoundaryConditionDefinition conditionDefinition = boundary.ConditionDefinition;
            Assert.That(conditionDefinition, Is.Not.Null);

            AssertCorrectGeometricDefinition(geometricDefinition);

            Assert.That(geometricDefinition.SupportPoints, Has.Count.EqualTo(2));
            AssertCorrectSupportPoint(geometricDefinition, 0);
            AssertCorrectSupportPoint(geometricDefinition, 100);

            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(BoundaryConditionPeriodType.Peak));
            var shape = conditionDefinition.Shape as GaussShape;
            Assert.That(shape, Is.Not.Null);
            Assert.That(shape.GaussianSpread, Is.EqualTo(5));

            var dataComponent = conditionDefinition.DataComponent as UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>;
            Assert.That(dataComponent, Is.Not.Null);

            AssertCorrectConstantParameters(dataComponent.Data, 1, 2, 3, 4);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_ConstantSpatiallyVaryingBoundary_LoadsBoundaryCorrectly()
        {
            // Setup
            string mdwPath = TestHelper.GetTestFilePath(@"read_wave_boundaries\constant-spatially_varying.mdw");

            // Call
            MdwFileDTO dto = new MdwFile().Load(mdwPath);
            WaveModelDefinition modelDefinition = dto.WaveModelDefinition;

            // Assert
            IWaveBoundary boundary = modelDefinition.BoundaryContainer.Boundaries.Single();
            Assert.That(boundary.Name, Is.EqualTo("boundary_name"));
            IWaveBoundaryGeometricDefinition geometricDefinition = boundary.GeometricDefinition;
            Assert.That(geometricDefinition, Is.Not.Null);
            IWaveBoundaryConditionDefinition conditionDefinition = boundary.ConditionDefinition;
            Assert.That(conditionDefinition, Is.Not.Null);

            AssertCorrectGeometricDefinition(geometricDefinition);

            Assert.That(geometricDefinition.SupportPoints, Has.Count.EqualTo(3));
            SupportPoint supportPoint1 = AssertCorrectSupportPoint(geometricDefinition, 0);
            SupportPoint supportPoint2 = AssertCorrectSupportPoint(geometricDefinition, 50);
            SupportPoint supportPoint3 = AssertCorrectSupportPoint(geometricDefinition, 100);

            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(BoundaryConditionPeriodType.Mean));

            var shape = conditionDefinition.Shape as JonswapShape;
            Assert.That(shape, Is.Not.Null);
            Assert.That(shape.PeakEnhancementFactor, Is.EqualTo(13));

            var dataComponent = conditionDefinition.DataComponent as
                                    SpatiallyVaryingDataComponent<ConstantParameters<PowerDefinedSpreading>>;
            Assert.That(dataComponent, Is.Not.Null);

            IReadOnlyDictionary<SupportPoint, ConstantParameters<PowerDefinedSpreading>> data = dataComponent.Data;
            Assert.That(data, Has.Count.EqualTo(3));

            AssertCorrectConstantParameters(data[supportPoint1], 1, 2, 3, 4);
            AssertCorrectConstantParameters(data[supportPoint2], 5, 6, 7, 8);
            AssertCorrectConstantParameters(data[supportPoint3], 9, 10, 11, 12);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_TimeDependentUniformBoundary_LoadsBoundaryCorrectly()
        {
            // Setup
            string mdwPath = TestHelper.GetTestFilePath(@"read_wave_boundaries\time_dependent-uniform.mdw");

            // Call
            MdwFileDTO dto = new MdwFile().Load(mdwPath);
            WaveModelDefinition modelDefinition = dto.WaveModelDefinition;

            // Assert
            IWaveBoundary boundary = modelDefinition.BoundaryContainer.Boundaries.Single();
            Assert.That(boundary.Name, Is.EqualTo("boundary_name"));
            IWaveBoundaryGeometricDefinition geometricDefinition = boundary.GeometricDefinition;
            Assert.That(geometricDefinition, Is.Not.Null);
            IWaveBoundaryConditionDefinition conditionDefinition = boundary.ConditionDefinition;
            Assert.That(conditionDefinition, Is.Not.Null);

            AssertCorrectGeometricDefinition(geometricDefinition);

            Assert.That(geometricDefinition.SupportPoints, Has.Count.EqualTo(2));
            AssertCorrectSupportPoint(geometricDefinition, 0);
            AssertCorrectSupportPoint(geometricDefinition, 100);

            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(BoundaryConditionPeriodType.Peak));

            var shape = conditionDefinition.Shape as PiersonMoskowitzShape;
            Assert.That(shape, Is.Not.Null);

            var dataComponent = conditionDefinition.DataComponent as UniformDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>;
            Assert.That(dataComponent, Is.Not.Null);

            IWaveEnergyFunction<DegreesDefinedSpreading> waveEnergyFunction = dataComponent.Data.WaveEnergyFunction;

            AssertCorrectWaveEnergyFunction(waveEnergyFunction, 0, new DateTime(2020, 4, 1), 1, 3, 5, 7);
            AssertCorrectWaveEnergyFunction(waveEnergyFunction, 1, new DateTime(2020, 4, 2), 2, 4, 6, 8);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_TimeDependentSpatiallyVaryingBoundary_LoadsBoundaryCorrectly()
        {
            // Setup
            string mdwPath = TestHelper.GetTestFilePath(@"read_wave_boundaries\time_dependent-spatially_varying.mdw");

            // Call
            MdwFileDTO dto = new MdwFile().Load(mdwPath);
            WaveModelDefinition modelDefinition = dto.WaveModelDefinition;

            // Assert
            IWaveBoundary boundary = modelDefinition.BoundaryContainer.Boundaries.Single();
            Assert.That(boundary.Name, Is.EqualTo("boundary_name"));
            IWaveBoundaryGeometricDefinition geometricDefinition = boundary.GeometricDefinition;
            Assert.That(geometricDefinition, Is.Not.Null);
            IWaveBoundaryConditionDefinition conditionDefinition = boundary.ConditionDefinition;
            Assert.That(conditionDefinition, Is.Not.Null);

            AssertCorrectGeometricDefinition(geometricDefinition);

            Assert.That(geometricDefinition.SupportPoints, Has.Count.EqualTo(3));
            SupportPoint supportPoint1 = AssertCorrectSupportPoint(geometricDefinition, 0);
            SupportPoint supportPoint2 = AssertCorrectSupportPoint(geometricDefinition, 50);
            SupportPoint supportPoint3 = AssertCorrectSupportPoint(geometricDefinition, 100);

            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(BoundaryConditionPeriodType.Mean));

            var shape = conditionDefinition.Shape as GaussShape;
            Assert.That(shape, Is.Not.Null);
            Assert.That(shape.GaussianSpread, Is.EqualTo(25));

            var dataComponent = conditionDefinition.DataComponent as
                                    SpatiallyVaryingDataComponent<TimeDependentParameters<DegreesDefinedSpreading>>;
            Assert.That(dataComponent, Is.Not.Null);

            IReadOnlyDictionary<SupportPoint, TimeDependentParameters<DegreesDefinedSpreading>> data = dataComponent.Data;
            Assert.That(dataComponent.Data, Has.Count.EqualTo(3));

            var startDate = new DateTime(2020, 4, 1);
            var endDate = new DateTime(2020, 4, 2);

            AssertCorrectWaveEnergyFunction(data[supportPoint1].WaveEnergyFunction, 0,
                                            startDate, 1, 3, 5, 7);
            AssertCorrectWaveEnergyFunction(data[supportPoint1].WaveEnergyFunction, 1,
                                            endDate, 2, 4, 6, 8);
            AssertCorrectWaveEnergyFunction(data[supportPoint2].WaveEnergyFunction, 0,
                                            startDate, 9, 11, 13, 15);
            AssertCorrectWaveEnergyFunction(data[supportPoint2].WaveEnergyFunction, 1,
                                            endDate, 10, 12, 14, 16);
            AssertCorrectWaveEnergyFunction(data[supportPoint3].WaveEnergyFunction, 0,
                                            startDate, 17, 19, 21, 23);
            AssertCorrectWaveEnergyFunction(data[supportPoint3].WaveEnergyFunction, 1,
                                            endDate, 18, 20, 22, 24);
        }

        [Test]
        [Category(NghsTestCategory.PerformanceDotTrace)]
        public void Load_TimeDependentSpatiallyVaryingBoundary_ShouldBeWithinExecutionTime()
        {
            string mdwPath = TestHelper.GetTestFilePath(@"read_wave_boundaries\time_dependent-spatially_varying.mdw");
            TimerMethod_LoadTimeDependentSpatiallyVaryingBoundary(mdwPath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_FileBasedUniformBoundary_LoadsBoundaryCorrectly()
        {
            // Setup
            string mdwPath = TestHelper.GetTestFilePath(@"read_wave_boundaries\file_based-uniform.mdw");

            // Call
            MdwFileDTO dto = new MdwFile().Load(mdwPath);
            WaveModelDefinition modelDefinition = dto.WaveModelDefinition;

            // Assert
            IWaveBoundary boundary = modelDefinition.BoundaryContainer.Boundaries.Single();
            Assert.That(boundary.Name, Is.EqualTo("boundary_name"));
            IWaveBoundaryGeometricDefinition geometricDefinition = boundary.GeometricDefinition;
            Assert.That(geometricDefinition, Is.Not.Null);
            IWaveBoundaryConditionDefinition conditionDefinition = boundary.ConditionDefinition;
            Assert.That(conditionDefinition, Is.Not.Null);

            AssertCorrectGeometricDefinition(geometricDefinition);

            Assert.That(geometricDefinition.SupportPoints, Has.Count.EqualTo(2));
            AssertCorrectSupportPoint(geometricDefinition, 0);
            AssertCorrectSupportPoint(geometricDefinition, 100);

            var dataComponent = conditionDefinition.DataComponent as UniformDataComponent<FileBasedParameters>;
            Assert.That(dataComponent, Is.Not.Null);

            Assert.That(dataComponent.Data.FilePath, Is.EqualTo(Path.Combine(Path.GetDirectoryName(mdwPath), "SpectrumFile.sp1")));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_FileBasedSpatiallyVaryingBoundary_LoadsBoundaryCorrectly()
        {
            // Setup
            string mdwPath = TestHelper.GetTestFilePath(@"read_wave_boundaries\file_based-spatially_varying.mdw");

            // Call
            MdwFileDTO dto = new MdwFile().Load(mdwPath);
            WaveModelDefinition modelDefinition = dto.WaveModelDefinition;

            // Assert
            IWaveBoundary boundary = modelDefinition.BoundaryContainer.Boundaries.Single();
            Assert.That(boundary.Name, Is.EqualTo("boundary_name"));
            IWaveBoundaryGeometricDefinition geometricDefinition = boundary.GeometricDefinition;
            Assert.That(geometricDefinition, Is.Not.Null);
            IWaveBoundaryConditionDefinition conditionDefinition = boundary.ConditionDefinition;
            Assert.That(conditionDefinition, Is.Not.Null);

            AssertCorrectGeometricDefinition(geometricDefinition);

            Assert.That(geometricDefinition.SupportPoints, Has.Count.EqualTo(3));
            SupportPoint supportPoint1 = AssertCorrectSupportPoint(geometricDefinition, 0);
            SupportPoint supportPoint2 = AssertCorrectSupportPoint(geometricDefinition, 50);
            SupportPoint supportPoint3 = AssertCorrectSupportPoint(geometricDefinition, 100);

            Assert.That(conditionDefinition.PeriodType, Is.EqualTo(BoundaryConditionPeriodType.Mean));

            var dataComponent = conditionDefinition.DataComponent as SpatiallyVaryingDataComponent<FileBasedParameters>;
            Assert.That(dataComponent, Is.Not.Null);

            IReadOnlyDictionary<SupportPoint, FileBasedParameters> data = dataComponent.Data;
            Assert.That(data, Has.Count.EqualTo(3));

            string pathRoot = Path.GetDirectoryName(mdwPath);
            Assert.That(data[supportPoint1].FilePath, Is.EqualTo(Path.Combine(pathRoot, "SpectrumFile1.sp1")));
            Assert.That(data[supportPoint2].FilePath, Is.EqualTo(Path.Combine(pathRoot, "SpectrumFile2.sp1")));
            Assert.That(data[supportPoint3].FilePath, Is.EqualTo(Path.Combine(pathRoot, "SpectrumFile3.sp1")));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_OverallFileBasedBoundary_LoadsBoundaryCorrectly()
        {
            // Setup
            string mdwPath = TestHelper.GetTestFilePath(@"read_wave_boundaries\overall-file_based.mdw");

            // Call
            MdwFileDTO dto = new MdwFile().Load(mdwPath);
            WaveModelDefinition modelDefinition = dto.WaveModelDefinition;

            // Assert
            IBoundaryContainer boundaryContainer = modelDefinition.BoundaryContainer;
            Assert.That(boundaryContainer.Boundaries, Is.Empty);
            Assert.That(boundaryContainer.DefinitionPerFileUsed, Is.True);
            Assert.That(boundaryContainer.FilePathForBoundariesPerFile, Is.EqualTo(Path.Combine(Path.GetDirectoryName(mdwPath), "OverallSpectrumFile.sp2")));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_WhenOuterGridIsNotImported_ThenWarningMessageIsLogged()
        {
            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string filePath = CreateMinimalMdwFile(tempDirectory.Path);

                // Call
                void Call() => new MdwFile().Load(filePath);

                // Assert
                List<string> warningMessages = TestHelper.GetAllRenderedMessages(Call, Level.Warn).ToList();
                Assert.That(warningMessages, Has.Count.EqualTo(1));
                Assert.That(warningMessages[0], Is.EqualTo("Boundaries cannot be imported, because there is no grid detected."));
            }
        }

        [Test]
        [TestCase(WaveDirectionalSpaceType.Sector)]
        [TestCase(WaveDirectionalSpaceType.Circle)]
        [Category(TestCategory.DataAccess)]
        public void Load_ThenCorrectSpectralDomainDataIsSet(WaveDirectionalSpaceType directionalSpaceType)
        {
            var random = new Random();
            var expectedDomainData = new SpectralDomainData
            {
                DirectionalSpaceType = directionalSpaceType,
                NDir = random.Next(),
                StartDir = GetRandomRoundedValue(random),
                EndDir = GetRandomRoundedValue(random),
                NFreq = random.Next(),
                FreqMin = GetRandomRoundedValue(random),
                FreqMax = GetRandomRoundedValue(random)
            };

            // Setup
            using (var tempDirectory = new TemporaryDirectory())
            {
                string filePath = CreateMdwFileWithSpectralDomainData(tempDirectory.Path, expectedDomainData);

                // Call
                MdwFileDTO dto = new MdwFile().Load(filePath);
                WaveModelDefinition modelDefinition = dto.WaveModelDefinition;

                // Assert
                SpectralDomainData spectralData = modelDefinition.OuterDomain.SpectralDomainData;
                Assert.That(spectralData.DirectionalSpaceType, Is.EqualTo(directionalSpaceType), "Directional space type");
                Assert.That(spectralData.NDir, Is.EqualTo(expectedDomainData.NDir), "NDir");
                Assert.That(spectralData.StartDir, Is.EqualTo(expectedDomainData.StartDir).Within(1e-5), "StartDir");
                Assert.That(spectralData.EndDir, Is.EqualTo(expectedDomainData.EndDir).Within(1e-5), "EndDir");
                Assert.That(spectralData.FreqMin, Is.EqualTo(expectedDomainData.FreqMin).Within(1e-5), "FreqMin");
                Assert.That(spectralData.FreqMax, Is.EqualTo(expectedDomainData.FreqMax).Within(1e-5), "FreqMax");
                Assert.That(spectralData.NFreq, Is.EqualTo(expectedDomainData.NFreq), "NFreq");
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_WithXYComponentMeteoFiles_LoadsFilesCorrectly()
        {
            // Setup
            string mdwPath = Path.Combine(testDataPath, "Wind.mdw");

            // Call
            MdwFileDTO dto = new MdwFile().Load(mdwPath);
            ITimeFrameData timeFrameData = dto.TimeFrameData;

            // Assert
            Assert.That(timeFrameData.WindFileData.XComponentFilePath, Is.EqualTo("xwind.wnd"));
            Assert.That(timeFrameData.WindFileData.YComponentFilePath, Is.EqualTo("ywind.wnd"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_WithInvalidXYComponentMeteoFiles_LogsErrorMessages()
        {
            // Setup
            string mdwPath = Path.Combine(testDataPath, "InvalidWind.mdw");

            // Call
            void Call() => new MdwFile().Load(mdwPath);

            // Assert
            string logMessage = TestHelper.GetAllRenderedMessages(Call).ElementAt(1);
            Assert.That(logMessage, Is.EqualTo("During loading the D-Waves model the following errors were reported:" + Environment.NewLine +
                                               "- Could not find meteo file for 'x_wind'" + Environment.NewLine +
                                               "- Could not find meteo file for 'y_wind'"));
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Load_WithSwanFileSettings_LoadsSwanFileSettingsCorrectly()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Setup
                string testFilePath = Path.Combine(testDataPath, "Swan.mdw");
                string mdwPath = tempDirectory.CopyTestDataFileAndDirectoryToTempDirectory(testFilePath);
                
                // Call
                MdwFileDTO dto = new MdwFile().Load(mdwPath);
                WaveModelDefinition modelDefinition = dto.WaveModelDefinition;

                WaveModelProperty waveModelProperty = modelDefinition.GetModelProperty(KnownWaveCategories.OutputCategory, KnownWaveProperties.KeepINPUT);

                // Assert
                Assert.IsNotNull(waveModelProperty);
                Assert.IsTrue((bool)waveModelProperty.Value);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_ForUniformFileBasedConstantBoundary_MdwFileShouldContainCorrectBoundaryCategoryAndFileIsCopied(bool switchTo)
        {
            // Setup
            WaveModelDefinition modelDefinition = CreateWaveModelDefinition();

            using (var tempDirectory = new TemporaryDirectory())
            {
                string sourceDir = tempDirectory.CreateDirectory("source");
                string targetDir = tempDirectory.CreateDirectory("target");

                const string fileName = "file.txt";
                string sourceFilePath = Path.Combine(sourceDir, fileName);
                File.WriteAllText(sourceFilePath, sourceFilePath);

                var parameters = new FileBasedParameters(sourceFilePath);
                var dataComponent = new UniformDataComponent<FileBasedParameters>(parameters);
                IWaveBoundary boundary = BuildWaveBoundary(dataComponent);
                modelDefinition.BoundaryContainer.Boundaries.Add(boundary);

                string saveFilePath = Path.Combine(targetDir, "output.mdw");

                var timeFrameData = new TimeFrameData();

                // Call
                new MdwFile().SaveTo(saveFilePath, new MdwFileDTO(modelDefinition, timeFrameData), switchTo);

                // Assert
                Assert.That(saveFilePath, Does.Exist);
                Assert.That(sourceFilePath, Does.Exist);
                string targetFilePath = Path.Combine(targetDir, fileName);
                Assert.That(targetFilePath, Does.Exist);

                string[] lines = GetBoundaryLines(saveFilePath);
                Assert.That(lines, Has.Length.EqualTo(8));
                AssertPropertyLine(lines[0], KnownWaveProperties.Name, "boundary_name");
                AssertPropertyLine(lines[1], KnownWaveProperties.Definition, "xy-coordinates");
                AssertPropertyLine(lines[2], KnownWaveProperties.StartCoordinateX, "0.0000000");
                AssertPropertyLine(lines[3], KnownWaveProperties.EndCoordinateX, "9.0000000");
                AssertPropertyLine(lines[4], KnownWaveProperties.StartCoordinateY, "0.0000000");
                AssertPropertyLine(lines[5], KnownWaveProperties.EndCoordinateY, "9.0000000");
                AssertPropertyLine(lines[6], KnownWaveProperties.SpectrumSpec, "from file");
                AssertPropertyLine(lines[7], KnownWaveProperties.Spectrum, fileName);

                Assert.That(parameters.FilePath, Is.EqualTo(switchTo ? targetFilePath : sourceFilePath));
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_ForUniformFileBasedConstantBoundary_WithEmptyFilePath_MdwFileShouldContainCorrectBoundaryCategory(bool switchTo)
        {
            // Setup
            WaveModelDefinition modelDefinition = CreateWaveModelDefinition();

            using (var tempDirectory = new TemporaryDirectory())
            {
                var parameters = new FileBasedParameters(string.Empty);
                var dataComponent = new UniformDataComponent<FileBasedParameters>(parameters);
                IWaveBoundary boundary = BuildWaveBoundary(dataComponent);
                modelDefinition.BoundaryContainer.Boundaries.Add(boundary);

                string saveFilePath = Path.Combine(Path.Combine(tempDirectory.Path), "output.mdw");
                var timeFrameData = new TimeFrameData();

                // Call
                new MdwFile().SaveTo(saveFilePath, new MdwFileDTO(modelDefinition, timeFrameData), switchTo);

                // Assert
                Assert.That(saveFilePath, Does.Exist);

                string[] lines = GetBoundaryLines(saveFilePath);
                Assert.That(lines, Has.Length.EqualTo(8));
                AssertPropertyLine(lines[0], KnownWaveProperties.Name, "boundary_name");
                AssertPropertyLine(lines[1], KnownWaveProperties.Definition, "xy-coordinates");
                AssertPropertyLine(lines[2], KnownWaveProperties.StartCoordinateX, "0.0000000");
                AssertPropertyLine(lines[3], KnownWaveProperties.EndCoordinateX, "9.0000000");
                AssertPropertyLine(lines[4], KnownWaveProperties.StartCoordinateY, "0.0000000");
                AssertPropertyLine(lines[5], KnownWaveProperties.EndCoordinateY, "9.0000000");
                AssertPropertyLine(lines[6], KnownWaveProperties.SpectrumSpec, "from file");
                AssertPropertyLine(lines[7], KnownWaveProperties.Spectrum, string.Empty);

                Assert.That(parameters.FilePath, Is.Empty);
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_ForSpatiallyVaryingFileBasedConstantBoundary_MdwFileShouldContainCorrectBoundaryCategoryAndFilesAreCopied(bool switchTo)
        {
            // Setup
            WaveModelDefinition modelDefinition = CreateWaveModelDefinition();

            using (var tempDirectory = new TemporaryDirectory())
            {
                string sourceDir = tempDirectory.CreateDirectory("source");
                string targetDir = tempDirectory.CreateDirectory("target");

                const string fileName1 = "file_1.txt";
                string sourceFilePath1 = Path.Combine(sourceDir, fileName1);
                File.WriteAllText(sourceFilePath1, sourceFilePath1);

                const string fileName2 = "file_2.txt";
                string sourceFilePath2 = Path.Combine(sourceDir, fileName2);
                File.WriteAllText(sourceFilePath2, sourceFilePath2);

                var dataComponent = new SpatiallyVaryingDataComponent<FileBasedParameters>();
                IWaveBoundary boundary = BuildWaveBoundary(dataComponent);
                IEventedList<SupportPoint> supportPoints = boundary.GeometricDefinition.SupportPoints;
                var parameters1 = new FileBasedParameters(sourceFilePath1);
                dataComponent.AddParameters(supportPoints[0], parameters1);
                var parameters2 = new FileBasedParameters(sourceFilePath2);
                dataComponent.AddParameters(supportPoints[1], parameters2);

                modelDefinition.BoundaryContainer.Boundaries.Add(boundary);
                string saveFilePath = Path.Combine(targetDir, "output.mdw");

                var timeFrameData = new TimeFrameData();

                // Call
                new MdwFile().SaveTo(saveFilePath, new MdwFileDTO(modelDefinition, timeFrameData), switchTo);

                // Assert
                Assert.That(saveFilePath, Does.Exist);
                Assert.That(sourceFilePath1, Does.Exist);
                Assert.That(sourceFilePath2, Does.Exist);
                string targetFilePath1 = Path.Combine(targetDir, fileName1);
                Assert.That(targetFilePath1, Does.Exist);
                string targetFilePath2 = Path.Combine(targetDir, fileName2);
                Assert.That(targetFilePath2, Does.Exist);

                string[] lines = GetBoundaryLines(saveFilePath);
                Assert.That(lines, Has.Length.EqualTo(11));
                AssertPropertyLine(lines[0], KnownWaveProperties.Name, "boundary_name");
                AssertPropertyLine(lines[1], KnownWaveProperties.Definition, "xy-coordinates");
                AssertPropertyLine(lines[2], KnownWaveProperties.StartCoordinateX, "0.0000000");
                AssertPropertyLine(lines[3], KnownWaveProperties.EndCoordinateX, "9.0000000");
                AssertPropertyLine(lines[4], KnownWaveProperties.StartCoordinateY, "0.0000000");
                AssertPropertyLine(lines[5], KnownWaveProperties.EndCoordinateY, "9.0000000");
                AssertPropertyLine(lines[6], KnownWaveProperties.SpectrumSpec, "from file");
                AssertPropertyLine(lines[7], KnownWaveProperties.CondSpecAtDist, "0.0000000");
                AssertPropertyLine(lines[8], KnownWaveProperties.Spectrum, fileName1);
                AssertPropertyLine(lines[9], KnownWaveProperties.CondSpecAtDist, "10.0000000");
                AssertPropertyLine(lines[10], KnownWaveProperties.Spectrum, fileName2);

                Assert.That(parameters1.FilePath, Is.EqualTo(switchTo ? targetFilePath1 : sourceFilePath1));
                Assert.That(parameters2.FilePath, Is.EqualTo(switchTo ? targetFilePath2 : sourceFilePath2));
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_ForFromSpectrumFileDefinedBoundaries_MdwFileShouldContainCorrectBoundaryCategory(bool switchTo)
        {
            // Setup
            WaveModelDefinition modelDefinition = CreateWaveModelDefinition();
            IBoundaryContainer boundaryContainer = modelDefinition.BoundaryContainer;
            boundaryContainer.DefinitionPerFileUsed = true;

            using (var tempDirectory = new TemporaryDirectory())
            {
                string sourceDir = tempDirectory.CreateDirectory("source");
                string targetDir = tempDirectory.CreateDirectory("target");

                const string fileName = "file.txt";
                string sourceFilePath = Path.Combine(sourceDir, fileName);
                File.WriteAllText(sourceFilePath, sourceFilePath);

                boundaryContainer.FilePathForBoundariesPerFile = sourceFilePath;
                string saveFilePath = Path.Combine(targetDir, "output.mdw");

                var timeFrameData = new TimeFrameData();

                // Call
                new MdwFile().SaveTo(saveFilePath, new MdwFileDTO(modelDefinition, timeFrameData), switchTo);

                // Assert
                Assert.That(saveFilePath, Does.Exist);
                Assert.That(sourceFilePath, Does.Exist);
                string targetFilePath = Path.Combine(targetDir, fileName);
                Assert.That(targetFilePath, Does.Exist);

                string[] lines = GetBoundaryLines(saveFilePath);
                Assert.That(lines, Has.Length.EqualTo(2));
                AssertPropertyLine(lines[0], KnownWaveProperties.Definition, "fromsp2file");
                AssertPropertyLine(lines[1], KnownWaveProperties.OverallSpecFile, fileName);

                Assert.That(boundaryContainer.FilePathForBoundariesPerFile,
                            Is.EqualTo(switchTo ? targetFilePath : sourceFilePath));
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_ForFromSpectrumFileDefinedBoundaries_WithEmptyFilePath_MdwFileShouldContainCorrectBoundaryCategory(bool switchTo)
        {
            // Setup
            WaveModelDefinition modelDefinition = CreateWaveModelDefinition();
            IBoundaryContainer boundaryContainer = modelDefinition.BoundaryContainer;
            boundaryContainer.DefinitionPerFileUsed = true;
            var timeFrameData = new TimeFrameData();

            using (var tempDirectory = new TemporaryDirectory())
            {
                boundaryContainer.FilePathForBoundariesPerFile = string.Empty;
                string saveFilePath = Path.Combine(tempDirectory.Path, "output.mdw");

                // Call
                new MdwFile().SaveTo(saveFilePath, new MdwFileDTO(modelDefinition, timeFrameData), switchTo);

                // Assert
                Assert.That(saveFilePath, Does.Exist);

                string[] lines = GetBoundaryLines(saveFilePath);
                Assert.That(lines, Has.Length.EqualTo(2));
                AssertPropertyLine(lines[0], KnownWaveProperties.Definition, "fromsp2file");
                AssertPropertyLine(lines[1], KnownWaveProperties.OverallSpecFile, string.Empty);

                Assert.That(boundaryContainer.FilePathForBoundariesPerFile, Is.EqualTo(string.Empty));
            }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_WithWindXYComponentMeteoFilesAndOldMeteoFilePropertyInModel_MdwFileShouldContainCorrectMeteoFiles(bool switchTo)
        {
            // Setup
            WaveModelDefinition modelDefinition = CreateWaveModelDefinition();

            using (var tempDirectory = new TemporaryDirectory())
            {
                modelDefinition.Properties.First(p => p.PropertyDefinition.FilePropertyName == KnownWaveProperties.MeteoFile).Value = "wind.wnd";

                var timeFrameData = new TimeFrameData();
                timeFrameData.WindInputDataType = WindInputDataType.FileBased;
                timeFrameData.WindFileData.FileType = WindDefinitionType.WindXWindY;
                timeFrameData.WindFileData.XComponentFilePath = Path.Combine(testDataPath, "xwind.wnd");
                timeFrameData.WindFileData.YComponentFilePath = Path.Combine(testDataPath, "ywind.wnd");

                string saveFilePath = Path.Combine(tempDirectory.Path, "output.mdw");

                // Call
                new MdwFile { MdwFilePath = saveFilePath }.SaveTo(saveFilePath, new MdwFileDTO(modelDefinition, timeFrameData), switchTo);

                // Assert
                Assert.That(saveFilePath, Does.Exist);

                string[] meteoFileLines = File.ReadAllLines(saveFilePath).Where(l => l.Contains(KnownWaveProperties.MeteoFile)).ToArray();
                Assert.That(meteoFileLines.Length, Is.EqualTo(2));
                AssertPropertyLine(meteoFileLines[0], KnownWaveProperties.MeteoFile, "xwind.wnd");
                AssertPropertyLine(meteoFileLines[1], KnownWaveProperties.MeteoFile, "ywind.wnd");
            }
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        [Category(TestCategory.DataAccess)]
        public void SaveTo_WithSwanFileSettings_MdwFileShouldContainSwanFileSettings(bool switchTo, bool keepInput)
        {
            // Setup
            WaveModelDefinition modelDefinition = CreateWaveModelDefinition();
            modelDefinition.Properties.First(p => p.PropertyDefinition.FilePropertyName == KnownWaveProperties.KeepINPUT).Value = keepInput;

            var timeFrameData = new TimeFrameData();

            using (var tempDirectory = new TemporaryDirectory())
            {
                string saveFilePath = Path.Combine(tempDirectory.Path, "output.mdw");

                // Call
                new MdwFile { MdwFilePath = saveFilePath }.SaveTo(saveFilePath, new MdwFileDTO(modelDefinition, timeFrameData), switchTo);

                // Assert
                Assert.That(saveFilePath, Does.Exist);

                string[] meteoFileLines = File.ReadAllLines(saveFilePath).Where(l => l.Contains(KnownWaveProperties.KeepINPUT)).ToArray();
                Assert.That(meteoFileLines.Length, Is.EqualTo(1));
                AssertPropertyLine(meteoFileLines[0], KnownWaveProperties.KeepINPUT, keepInput.ToString().ToLower());
            }
        }

        public static IEnumerable<TestCaseData> GetLoadMeteoTestData()
        {

            var spwOnly = new WaveMeteoData
            {
                FileType = WindDefinitionType.SpiderWebGrid,
                SpiderWebFilePath = "empty.spw",
            };

            yield return new TestCaseData("spw_only", spwOnly);

            var wndOnly = new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXY,
                XYVectorFilePath = "empty.wnd",
            };

            yield return new TestCaseData("wnd_only", wndOnly);

            var wndSpw = new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXY,
                XYVectorFilePath = "empty.wnd",
                HasSpiderWeb = true,
                SpiderWebFilePath = "empty.spw",
            };

            yield return new TestCaseData("wnd_spw", wndSpw);

            var xyOnly = new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXWindY,
                XComponentFilePath = "xwind.wnd",
                YComponentFilePath = "ywind.wnd",
            };

            yield return new TestCaseData("xy_only", xyOnly);

            var xySpw = new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXWindY,
                XComponentFilePath = "xwind.wnd",
                YComponentFilePath = "ywind.wnd",
                HasSpiderWeb = true,
                SpiderWebFilePath = "empty.spw",
            };

            yield return new TestCaseData("xy_spw", xySpw);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetLoadMeteoTestData))]
        public void Load_MdwFileWithMeteoData_ReturnsCorrectMeteoData(string subDirectory,
                                                                      WaveMeteoData expectedFileData)
        {
            // Setup
            string meteoDataBasePath = TestHelper.GetTestFilePath("MdwFile/MeteoFiles");
            string dataPath = Path.Combine(meteoDataBasePath, subDirectory);

            using (var tempDir = new TemporaryDirectory())
            {
                string localPath = tempDir.CopyDirectoryToTempDirectory(dataPath);
                string mdwPath = Path.Combine(localPath, "waves.mdw");

                var mdwFile = new MdwFile();

                // Call
                MdwFileDTO result = mdwFile.Load(mdwPath);

                // Assert
                WaveMeteoData fileData = result.TimeFrameData.WindFileData;

                Assert.That(fileData.FileType, Is.EqualTo(expectedFileData.FileType));
                Assert.That(fileData.XYVectorFilePath, Is.EqualTo(expectedFileData.XYVectorFilePath));
                Assert.That(fileData.XComponentFilePath, Is.EqualTo(expectedFileData.XComponentFilePath));
                Assert.That(fileData.YComponentFilePath, Is.EqualTo(expectedFileData.YComponentFilePath));
                Assert.That(fileData.HasSpiderWeb, Is.EqualTo(expectedFileData.HasSpiderWeb));
                Assert.That(fileData.SpiderWebFilePath, Is.EqualTo(expectedFileData.SpiderWebFilePath));
            }
        }

        /// <summary>
        /// Method to test by dot Trace. Should be public for setting thresholds.
        /// </summary>
        /// <param name="mdwPath"> The Mdw file path. </param>
        public static void TimerMethod_LoadTimeDependentSpatiallyVaryingBoundary(string mdwPath)
        {
            new MdwFile().Load(mdwPath);
        }

        private static string CreateMdwFileWithSpectralDomainData(string tempDirPath, SpectralDomainData domainData)
        {
            string filePath = Path.Combine(tempDirPath, "file.mdw");
            string directionalSpaceTypeValue = domainData.DirectionalSpaceType == WaveDirectionalSpaceType.Circle
                                                   ? "circle"
                                                   : "sector";

            string[] content =
            {
                "[Domain]",
                $"DirSpace  = {directionalSpaceTypeValue}",
                $"NDir      = {domainData.NDir}",
                $"StartDir  = {ToDoubleString(domainData.StartDir)}",
                $"EndDir    = {ToDoubleString(domainData.EndDir)}",
                $"FreqMin   = {ToDoubleString(domainData.FreqMin)}",
                $"FreqMax   = {ToDoubleString(domainData.FreqMax)}",
                $"NFreq     = {domainData.NFreq}",
                "[Output]",
                "[General]"
            };

            File.WriteAllLines(filePath, content);

            return filePath;
        }

        private static string ToDoubleString(double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static double GetRandomRoundedValue(Random random)
        {
            const int factor = 10000000;
            return Math.Floor(random.NextDouble() * factor) / factor;
        }

        private static void AssertPropertyLine(string line, string propertyName, string value)
        {
            string[] pair = line.Split('=');

            Assert.That(pair[0].Trim(), Is.EqualTo(propertyName));
            Assert.That(pair[1].Trim(), Is.EqualTo(value));
        }

        private static string[] GetBoundaryLines(string filePath)
        {
            return File.ReadAllLines(filePath)
                       .SkipWhile(l => !l.Contains($"[{KnownWaveCategories.BoundaryCategory}]"))
                       .Skip(1)
                       .TakeWhile(l => !l.Contains("[") && !string.IsNullOrWhiteSpace(l))
                       .ToArray();
        }

        private static IWaveBoundary BuildWaveBoundary(ISpatiallyDefinedDataComponent dataComponent)
        {
            IWaveBoundaryGeometricDefinition geometricDefinition = CreateGeometricDefinition();
            var conditionDefinition = new WaveBoundaryConditionDefinition(new PiersonMoskowitzShape(),
                                                                          BoundaryConditionPeriodType.Peak,
                                                                          dataComponent);

            var boundary = Substitute.For<IWaveBoundary>();
            boundary.Name = "boundary_name";
            boundary.GeometricDefinition.Returns(geometricDefinition);
            boundary.ConditionDefinition.Returns(conditionDefinition);
            return boundary;
        }

        private static WaveModelDefinition CreateWaveModelDefinition()
        {
            var modelDefinition = new WaveModelDefinition();
            CurvilinearGrid grid = CreateCurvilinearGrid(10, 10);
            modelDefinition.OuterDomain = new WaveDomainData("Outer") { Grid = grid };
            modelDefinition.BoundaryContainer.UpdateGridBoundary(new GridBoundary(grid));
            return modelDefinition;
        }

        private static CurvilinearGrid CreateCurvilinearGrid(int length, int width)
        {
            int size = length * width;

            var x = new double[size];
            var y = new double[size];
            for (var i = 0; i < size; i++)
            {
                x[i] = i;
                y[i] = i;
            }

            var grid = new CurvilinearGrid(length, width, x, y, WaveModel.CoordinateSystemType.Cartesian);
            return grid;
        }

        private static UniformDataComponent<ConstantParameters<PowerDefinedSpreading>> CreateUniformConstantDataComponent()
        {
            return new UniformDataComponent<ConstantParameters<PowerDefinedSpreading>>(
                new ConstantParameters<PowerDefinedSpreading>(1, 2, 3, new PowerDefinedSpreading { SpreadingPower = 10 }));
        }

        private static UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>> CreateUniformTimeDependentDataComponent()
        {
            return new UniformDataComponent<TimeDependentParameters<PowerDefinedSpreading>>(
                new TimeDependentParameters<PowerDefinedSpreading>(
                    Substitute.For<IWaveEnergyFunction<PowerDefinedSpreading>>()));
        }

        private static IWaveBoundaryGeometricDefinition CreateGeometricDefinition()
        {
            var geometricDefinition = Substitute.For<IWaveBoundaryGeometricDefinition>();

            geometricDefinition.EndingIndex.Returns(10);
            geometricDefinition.StartingIndex.Returns(0);
            geometricDefinition.Length.Returns(10);
            geometricDefinition.GridSide.Returns(GridSide.East);
            geometricDefinition.SupportPoints.Returns(new EventedList<SupportPoint>
            {
                new SupportPoint(0, geometricDefinition),
                new SupportPoint(10, geometricDefinition)
            });

            return geometricDefinition;
        }

        private static void AssertCorrectConstantParameters(ConstantParameters<PowerDefinedSpreading> supportPointData,
                                                            double height, double period, double direction, double spreading)
        {
            Assert.That(supportPointData.Height, Is.EqualTo(height));
            Assert.That(supportPointData.Period, Is.EqualTo(period));
            Assert.That(supportPointData.Direction, Is.EqualTo(direction));
            Assert.That(supportPointData.Spreading.SpreadingPower, Is.EqualTo(spreading));
        }

        private static void AssertCorrectWaveEnergyFunction(IWaveEnergyFunction<DegreesDefinedSpreading> waveEnergyFunction, int i, DateTime date,
                                                            double height, double period, double direction, double spreading)
        {
            Assert.That(waveEnergyFunction.TimeArgument.Values[i], Is.EqualTo(date));
            Assert.That(waveEnergyFunction.HeightComponent.Values[i], Is.EqualTo(height));
            Assert.That(waveEnergyFunction.PeriodComponent.Values[i], Is.EqualTo(period));
            Assert.That(waveEnergyFunction.DirectionComponent.Values[i], Is.EqualTo(direction));
            Assert.That(waveEnergyFunction.SpreadingComponent.Values[i], Is.EqualTo(spreading));
        }

        private static void AssertCorrectGeometricDefinition(IWaveBoundaryGeometricDefinition geometricDefinition)
        {
            Assert.That(geometricDefinition.StartingIndex, Is.EqualTo(0));
            Assert.That(geometricDefinition.EndingIndex, Is.EqualTo(10));
            Assert.That(geometricDefinition.Length, Is.EqualTo(100));
            Assert.That(geometricDefinition.GridSide, Is.EqualTo(GridSide.South));
        }

        private static SupportPoint AssertCorrectSupportPoint(IWaveBoundaryGeometricDefinition geometricDefinition, double distance)
        {
            SupportPoint supportPoint = geometricDefinition.SupportPoints
                                                           .FirstOrDefault(s => s.Distance.Equals(distance));

            Assert.That(supportPoint, Is.Not.Null);
            Assert.That(supportPoint.GeometricDefinition, Is.SameAs(geometricDefinition));

            return supportPoint;
        }

        private static string CreateMinimalMdwFile(string tempDirPath)
        {
            string filePath = Path.Combine(tempDirPath, "file.mdw");

            string[] content =
            {
                "[Domain]",
                "[Output]",
                "[General]"
            };

            File.WriteAllLines(filePath, content);

            return filePath;
        }
    }
}