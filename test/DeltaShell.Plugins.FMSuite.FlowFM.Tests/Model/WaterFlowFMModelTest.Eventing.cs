using System;
using System.IO;
using System.Text;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        [Test]
        [TestCase(KnownProperties.RestartFile, "test")]
        [TestCase(KnownProperties.ThinDamFile, "test")]
        [TestCase(KnownProperties.LandBoundaryFile, "test")]
        [TestCase(KnownProperties.FixedWeirFile, "test")]
        [TestCase(KnownProperties.BridgePillarFile, "test")]
        [TestCase(KnownProperties.ObsFile, "test")]
        [TestCase(KnownProperties.ObsCrsFile, "test")]
        [TestCase(KnownProperties.StructuresFile, "test")]
        [TestCase(KnownProperties.EnclosureFile, "test")]
        [TestCase(KnownProperties.DryPointsFile, "test")]
        [TestCase(KnownProperties.PathsRelativeToParent, "test")]
        [TestCase(KnownProperties.OutputDir, "test")]
        [TestCase(KnownProperties.WaqOutputDir, "test")]
        [TestCase(KnownProperties.ExtForceFile, "test")]
        [TestCase(KnownProperties.BndExtForceFile, "test")]
        [TestCase(KnownProperties.MorFile, "test")]
        [TestCase(KnownProperties.SedFile, "test")]
        [TestCase(KnownProperties.HisInterval, "test")]
        [TestCase(KnownProperties.MapInterval, "test")]
        [TestCase(KnownProperties.RstInterval, "test")]
        [TestCase(KnownProperties.WaqInterval, "test")]
        [TestCase(KnownProperties.ClassMapInterval, "test")]
        [TestCase(KnownProperties.Version, "test")]
        [TestCase(KnownProperties.GuiVersion, "test")]
        [TestCase(KnownProperties.NetFile, "test")]
        [TestCase(KnownProperties.StructuresFile, "test")]
        [TestCase(KnownProperties.PartitionFile, "test")]
        [TestCase(KnownProperties.ManholeFile, "test")]
        [TestCase(KnownProperties.ProfdefFile, "test")]
        [TestCase(KnownProperties.ProflocFile, "test")]
        [TestCase(KnownProperties.WaterLevIniFile, "test")]
        [TestCase(KnownProperties.TrtRou, "Y")]
        [TestCase(KnownProperties.TrtDef, "test")]
        [TestCase(KnownProperties.TrtL, "test")]
        [TestCase(KnownProperties.SolverType, "2")]
        [TestCase(KnownProperties.MapFile__Obsolete, "test")]
        [TestCase(KnownProperties.HisFile__Obsolete, "test")]
        [TestCase(KnownProperties.TStart, "0")]
        [TestCase(KnownProperties.TStop, "0")]
        public void GivenAModelDefinitionPropertyChanged_ForADataAccessInputProperty_ShouldNotMarkOutputOutOfSync(string propertyName, string value)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                var model = new WaterFlowFMModel();
                model.ConnectOutput(tempDirectory.Path);
                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.ModelDefinition.GetModelProperty(propertyName).SetValueAsString(value);

                // Assert
                Assert.IsFalse(model.OutputOutOfSync);
            }
        }

        [Test]
        [TestCase(KnownProperties.FixedWeirScheme, "0")]
        [TestCase(KnownProperties.Kmx, "0")]
        [TestCase(KnownProperties.DtUser, "1")]
        [TestCase(KnownProperties.DtMax, "1")]
        [TestCase(KnownProperties.Tunit, "H")]
        [TestCase(KnownProperties.ICdtyp, "1")]
        [TestCase(KnownProperties.Cdbreakpoints, "1")]
        [TestCase(KnownProperties.Windspeedbreakpoints, "1")]
        [TestCase(KnownProperties.UseSalinity, "1")]
        [TestCase(KnownProperties.Limtypsa, "1")]
        [TestCase(KnownProperties.Temperature, "1")]
        [TestCase(KnownProperties.FrictionType, "1")]
        [TestCase(KnownProperties.WaveModelNr, "1")]
        [TestCase(KnownProperties.SecondaryFlow, "1")]
        [TestCase(KnownProperties.Irov, "1")]
        [TestCase(KnownProperties.ISlope, "1")]
        [TestCase(KnownProperties.IHidExp, "1")]
        [TestCase(KnownProperties.SedimentModelNumber, "1")]
        [TestCase(KnownProperties.BedlevType, "1")]
        [TestCase(KnownProperties.MapFormat, "1")]
        [TestCase(KnownProperties.Conveyance2d, "1")]
        [TestCase(KnownProperties.Wrishp_crs, "1")]
        [TestCase(KnownProperties.Wrishp_weir, "1")]
        [TestCase(KnownProperties.Wrishp_gate, "1")]
        [TestCase(KnownProperties.Wrishp_fxw, "1")]
        [TestCase(KnownProperties.Wrishp_thd, "1")]
        [TestCase(KnownProperties.Wrishp_obs, "1")]
        [TestCase(KnownProperties.Wrishp_emb, "1")]
        [TestCase(KnownProperties.Wrishp_dryarea, "1")]
        [TestCase(KnownProperties.Wrishp_enc, "1")]
        [TestCase(KnownProperties.Wrishp_src, "1")]
        [TestCase(KnownProperties.Wrishp_pump, "1")]
        [TestCase(KnownProperties.DtTrt, "1")]
        [TestCase(GuiProperties.WriteHisFile, "1")]
        [TestCase(GuiProperties.WriteMapFile, "1")]
        [TestCase(GuiProperties.WriteClassMapFile, "1")]
        [TestCase(GuiProperties.SpecifyHisStart, "1")]
        [TestCase(GuiProperties.SpecifyHisStop, "1")]
        [TestCase(GuiProperties.SpecifyMapStart, "1")]
        [TestCase(GuiProperties.SpecifyMapStop, "1")]
        [TestCase(GuiProperties.SpecifyRstStart, "1")]
        [TestCase(GuiProperties.SpecifyRstStop, "1")]
        [TestCase(GuiProperties.SpecifyWaqOutputInterval, "1")]
        [TestCase(GuiProperties.SpecifyWaqOutputStartTime, "1")]
        [TestCase(GuiProperties.SpecifyWaqOutputStopTime, "1")]
        [TestCase(GuiProperties.UseMorSed, "1")]
        [TestCase(GuiProperties.WriteSnappedFeatures, "1")]
        public void GivenAModelDefinitionPropertyChanged_ForARealInputProperty_ShouldMarkOutputOutOfSync(string propertyName, string value)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                WaterFlowFMModel model = new WaterFlowFMModel();
                model.ConnectOutput(tempDirectory.Path);
                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.ModelDefinition.GetModelProperty(propertyName).SetValueAsString(value);

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        [TestCase(GuiProperties.StartTime)]
        [TestCase(GuiProperties.StopTime)]
        [TestCase(KnownProperties.RefDate)]
        [TestCase(GuiProperties.HisOutputStartTime)]
        [TestCase(GuiProperties.HisOutputStopTime)]
        [TestCase(GuiProperties.MapOutputStartTime)]
        [TestCase(GuiProperties.MapOutputStopTime)]
        [TestCase(GuiProperties.RstOutputStartTime)]
        [TestCase(GuiProperties.RstOutputStopTime)]
        [TestCase(GuiProperties.WaqOutputStartTime)]
        [TestCase(GuiProperties.WaqOutputStopTime)]
        public void GivenAModelDefinitionPropertyChanged_ForARealInputDateTimeProperty_ShouldMarkOutputOutOfSync(string propertyName)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                WaterFlowFMModel model = new WaterFlowFMModel();
                model.ConnectOutput(tempDirectory.Path);
                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.ModelDefinition.GetModelProperty(propertyName).Value = new DateTime();

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        [TestCase(GuiProperties.HisOutputDeltaT)]
        [TestCase(GuiProperties.MapOutputDeltaT)]
        [TestCase(GuiProperties.ClassMapOutputDeltaT)]
        [TestCase(GuiProperties.RstOutputDeltaT)]
        [TestCase(GuiProperties.WaqOutputDeltaT)]
        public void GivenAModelDefinitionPropertyChanged_ForARealInputTimeSpanProperty_ShouldMarkOutputOutOfSync(string propertyName)
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                WaterFlowFMModel model = new WaterFlowFMModel();
                model.ConnectOutput(tempDirectory.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.ModelDefinition.GetModelProperty(propertyName).Value = new TimeSpan();

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        public void AddingAnObservationPoint_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                var model = new WaterFlowFMModel();
                model.ConnectOutput(tempDirectory.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.Area.ObservationPoints.Add(new GroupableFeature2DPoint());

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        public void RemovingAnObservationPoint_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                var model = new WaterFlowFMModel();
                var observationPoint = new GroupableFeature2DPoint();
                model.Area.ObservationPoints.Add(observationPoint);
                model.ConnectOutput(tempDirectory.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.Area.ObservationPoints.Remove(observationPoint);

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        public void AddingAFixedWeir_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                var model = new WaterFlowFMModel();
                model.ConnectOutput(tempDirectory.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.Area.FixedWeirs.Add(new FixedWeir());

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        public void RemovingAFixedWeir_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                var model = new WaterFlowFMModel();
                var fixedWeir = new FixedWeir();
                model.Area.FixedWeirs.Add(fixedWeir);
                model.ConnectOutput(tempDirectory.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.Area.FixedWeirs.Remove(fixedWeir);

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        public void AddingAStructure_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                var model = new WaterFlowFMModel();
                model.ConnectOutput(tempDirectory.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.Area.Weirs.Add(new Weir2D());

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        public void RemovingAStructure_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                var model = new WaterFlowFMModel();
                var structure = new Weir2D();
                model.Area.Weirs.Add(structure);
                model.ConnectOutput(tempDirectory.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.Area.Weirs.Remove(structure);

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        public void AddingAnObservationCrossSection_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                var model = new WaterFlowFMModel();
                model.ConnectOutput(tempDirectory.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D());

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        public void RemovingAnObservationCrossSection_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                var model = new WaterFlowFMModel();
                var observationCrossSection = new ObservationCrossSection2D();
                model.Area.ObservationCrossSections.Add(observationCrossSection);
                model.ConnectOutput(tempDirectory.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.Area.ObservationCrossSections.Remove(observationCrossSection);

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        public void AddingAPump_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                var model = new WaterFlowFMModel();
                model.ConnectOutput(tempDirectory.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.Area.Pumps.Add(new Pump2D());

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        [Test]
        public void RemovingAPump_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
                var model = new WaterFlowFMModel();
                var pump = new Pump2D();
                model.Area.Pumps.Add(pump);
                model.ConnectOutput(tempDirectory.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Act
                model.Area.Pumps.Remove(pump);

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }
        
        private void CreateRestartOutputFile(string tempDirectoryPath)
        {
            string restartFilePath = Path.Combine(tempDirectoryPath, "test_rst.nc");
            const string text = "This is some text in the file.";

            using (FileStream fs = File.Create(restartFilePath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(text);
                fs.Write(info, 0, info.Length);
            }
        }
    }
}
