using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
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
        [TestCase(KnownProperties.MapFile__Obsolete, "test")]
        [TestCase(KnownProperties.HisFile__Obsolete, "test")]
        [TestCase(KnownProperties.TStart, "0")]
        [TestCase(KnownProperties.TStop, "0")]
        public void GivenAModelDefinitionPropertyChanged_ForADataAccessInputProperty_ShouldNotMarkOutputOutOfSync(string propertyName, string value)
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel())
            {
                // Arrange
                CreateRestartOutputFile(tempDirectory.Path);
              
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
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetTestStepsForChangingModelDefinitionInputData))]
        public void ChangingRealModelDefinitionInputProperties_ShouldMarkOutputOutOfSync(ITestStepsForModelOutputOutOfSync data)
        {
            MarkingOutputOutOfSyncWhenInputChangedTest(data);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetTestStepsForHydroAreaObjects))]
        public void AddingOrRemovingAHydroAreaObject_ShouldMarkOutputOutOfSync(ITestStepsForModelOutputOutOfSync data)
        {
            MarkingOutputOutOfSyncWhenInputChangedTest(data);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetTestStepsForFMRegionObjects))]
        public void AddingOrRemovingAFMRegionObject_ShouldMarkOutputOutOfSync(ITestStepsForModelOutputOutOfSync data)
        {
            MarkingOutputOutOfSyncWhenInputChangedTest(data);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetTestStepsForHydroAreaPropertyChanges))]
        public void ChangingAStructureProperty_ShouldMarkOutputOutOfSync(ITestStepsForModelOutputOutOfSync data)
        {
            MarkingOutputOutOfSyncWhenInputChangedTest(data);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(GetTestStepsForGridChanges))]
        public void ChangingTheGrid_ShouldMarkOutputOutOfSync(ITestStepsForModelOutputOutOfSync data)
        {
            MarkingOutputOutOfSyncWhenInputChangedTest(data);
        }

        private static void MarkingOutputOutOfSyncWhenInputChangedTest(ITestStepsForModelOutputOutOfSync data)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel())
            {
                CreateRestartOutputFile(tempDir.Path);

                data.Arrange(model);
                model.ConnectOutput(tempDir.Path);

                // Check pre-condition
                Assert.IsFalse(model.OutputOutOfSync);
                Assert.IsFalse(model.OutputIsEmpty);

                // Call
                data.Act(model);

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        private static IEnumerable<TestCaseData> GetTestStepsForChangingModelDefinitionInputData() =>
            GetTestStepsForChangingModelDefinitionBySettingValueAsString()
                .Concat(GetTestStepsForChangingDateTimeObjectsInModelDefinition())
                .Concat(GetTestStepsForChangingTimeSpanObjectsInModelDefinition());

        private static IEnumerable<TestCaseData> GetTestStepsForChangingModelDefinitionBySettingValueAsString()
        {
            TestCaseData GetTestCaseData(string propertyName, string value) =>
                new TestCaseData(new ModelDefinitionSetValueAsStringTestSteps(propertyName, value))
                    .SetName($"Modifying the real model definition input property '{propertyName}' should mark the output out of sync.");

            yield return GetTestCaseData(KnownProperties.FixedWeirScheme, "0");
            yield return GetTestCaseData(KnownProperties.Kmx, "0");
            yield return GetTestCaseData(KnownProperties.DtUser, "1");
            yield return GetTestCaseData(KnownProperties.DtMax, "1");
            yield return GetTestCaseData(KnownProperties.Tunit, "H");
            yield return GetTestCaseData(KnownProperties.ICdtyp, "1");
            yield return GetTestCaseData(KnownProperties.Cdbreakpoints, "1");
            yield return GetTestCaseData(KnownProperties.Windspeedbreakpoints, "1");
            yield return GetTestCaseData(KnownProperties.UseSalinity, "1");
            yield return GetTestCaseData(KnownProperties.Limtypsa, "1");
            yield return GetTestCaseData(KnownProperties.Temperature, "1");
            yield return GetTestCaseData(KnownProperties.FrictionType, "1");
            yield return GetTestCaseData(KnownProperties.WaveModelNr, "1");
            yield return GetTestCaseData(KnownProperties.SecondaryFlow, "1");
            yield return GetTestCaseData(KnownProperties.Irov, "1");
            yield return GetTestCaseData(KnownProperties.ISlope, "1");
            yield return GetTestCaseData(KnownProperties.IHidExp, "1");
            yield return GetTestCaseData(KnownProperties.SedimentModelNumber, "1");
            yield return GetTestCaseData(KnownProperties.BedlevType, "1");
            yield return GetTestCaseData(KnownProperties.MapFormat, "1");
            yield return GetTestCaseData(KnownProperties.Conveyance2d, "1");
            yield return GetTestCaseData(KnownProperties.Wrishp_crs, "1");
            yield return GetTestCaseData(KnownProperties.Wrishp_weir, "1");
            yield return GetTestCaseData(KnownProperties.Wrishp_gate, "1");
            yield return GetTestCaseData(KnownProperties.Wrishp_fxw, "1");
            yield return GetTestCaseData(KnownProperties.Wrishp_thd, "1");
            yield return GetTestCaseData(KnownProperties.Wrishp_obs, "1");
            yield return GetTestCaseData(KnownProperties.Wrishp_emb, "1");
            yield return GetTestCaseData(KnownProperties.Wrishp_dryarea, "1");
            yield return GetTestCaseData(KnownProperties.Wrishp_enc, "1");
            yield return GetTestCaseData(KnownProperties.Wrishp_src, "1");
            yield return GetTestCaseData(KnownProperties.Wrishp_pump, "1");
            yield return GetTestCaseData(KnownProperties.DtTrt, "1");
            yield return GetTestCaseData(KnownProperties.SolverType, "2");
            yield return GetTestCaseData(GuiProperties.WriteHisFile, "1");
            yield return GetTestCaseData(GuiProperties.WriteMapFile, "1");
            yield return GetTestCaseData(GuiProperties.WriteClassMapFile, "1");
            yield return GetTestCaseData(GuiProperties.SpecifyHisStart, "1");
            yield return GetTestCaseData(GuiProperties.SpecifyHisStop, "1");
            yield return GetTestCaseData(GuiProperties.SpecifyMapStart, "1");
            yield return GetTestCaseData(GuiProperties.SpecifyMapStop, "1");
            yield return GetTestCaseData(GuiProperties.SpecifyRstStart, "1");
            yield return GetTestCaseData(GuiProperties.SpecifyRstStop, "1");
            yield return GetTestCaseData(GuiProperties.SpecifyWaqOutputInterval, "1");
            yield return GetTestCaseData(GuiProperties.SpecifyWaqOutputStartTime, "1");
            yield return GetTestCaseData(GuiProperties.SpecifyWaqOutputStopTime, "1");
            yield return GetTestCaseData(GuiProperties.UseMorSed, "1");
            yield return GetTestCaseData(GuiProperties.WriteSnappedFeatures, "1");
        }

        private static IEnumerable<TestCaseData> GetTestStepsForChangingDateTimeObjectsInModelDefinition()
        {
            TestCaseData GetTestCaseData(string propertyName) =>
                new TestCaseData(new ModelDefinitionSetValueDirectlyTestSteps(propertyName, new DateTime()))
                    .SetName($"Modifying the DateTime property '{propertyName}' should mark the output out of sync.");

            yield return GetTestCaseData(GuiProperties.StartTime);
            yield return GetTestCaseData(GuiProperties.StopTime);
            yield return GetTestCaseData(KnownProperties.RefDate);
            yield return GetTestCaseData(GuiProperties.HisOutputStartTime);
            yield return GetTestCaseData(GuiProperties.HisOutputStopTime);
            yield return GetTestCaseData(GuiProperties.MapOutputStartTime);
            yield return GetTestCaseData(GuiProperties.MapOutputStopTime);
            yield return GetTestCaseData(GuiProperties.RstOutputStartTime);
            yield return GetTestCaseData(GuiProperties.RstOutputStopTime);
            yield return GetTestCaseData(GuiProperties.WaqOutputStartTime);
            yield return GetTestCaseData(GuiProperties.WaqOutputStopTime);
        }

        private static IEnumerable<TestCaseData> GetTestStepsForChangingTimeSpanObjectsInModelDefinition()
        {
            TestCaseData GetTestCaseData(string propertyName) =>
                new TestCaseData(new ModelDefinitionSetValueDirectlyTestSteps(propertyName, new TimeSpan()))
                    .SetName($"Modifying the TimeSpan property '{propertyName}' should mark the output out of sync.");

            yield return GetTestCaseData(GuiProperties.HisOutputDeltaT);
            yield return GetTestCaseData(GuiProperties.MapOutputDeltaT);
            yield return GetTestCaseData(GuiProperties.ClassMapOutputDeltaT);
            yield return GetTestCaseData(GuiProperties.RstOutputDeltaT);
            yield return GetTestCaseData(GuiProperties.WaqOutputDeltaT);
        }

        private static IEnumerable<TestCaseData> GetTestStepsForHydroAreaObjects()
        {
            yield return new TestCaseData(new AddingObservationPointTestSteps()).SetName("Adding an observation point should mark the output out of sync.");
            yield return new TestCaseData(new RemovingObservationPointTestSteps()).SetName("Removing an observation point should mark the output out of sync.");
            yield return new TestCaseData(new AddingFixedWeirTestSteps()).SetName("Adding a fixed weir should mark the output out of sync.");
            yield return new TestCaseData(new RemovingFixedWeirTestSteps()).SetName("Removing a fixed weir should mark the output out of sync.");
            yield return new TestCaseData(new AddingStructureTestSteps()).SetName("Adding a structure should mark the output out of sync.");
            yield return new TestCaseData(new RemovingStructureTestSteps()).SetName("Removing a structure should mark the output out of sync.");
            yield return new TestCaseData(new AddingObservationCrossSectionTestSteps()).SetName("Adding an observation cross section should mark the output out of sync.");
            yield return new TestCaseData(new RemovingObservationCrossSectionTestSteps()).SetName("Removing an observation cross section should mark the output out of sync.");
            yield return new TestCaseData(new AddingPumpTestSteps()).SetName("Adding a pump should mark the output out of sync.");
            yield return new TestCaseData(new RemovingPumpTestSteps()).SetName("Removing a pump should mark the output out of sync.");
            yield return new TestCaseData(new AddingBridgePillarTestSteps()).SetName("Adding a bridge pillar should mark the output out of sync.");
            yield return new TestCaseData(new RemovingBridgePillarTestSteps()).SetName("Removing a bridge pillar should mark the output out of sync.");
            yield return new TestCaseData(new AddingDryPointTestSteps()).SetName("Adding a dry point should mark the output out of sync.");
            yield return new TestCaseData(new RemovingDryPointTestSteps()).SetName("Removing a dry point should mark the output out of sync.");
            yield return new TestCaseData(new AddingDryAreaTestSteps()).SetName("Adding a dry area should mark the output out of sync.");
            yield return new TestCaseData(new RemovingDryAreaTestSteps()).SetName("Removing a dry area should mark the output out of sync.");
            yield return new TestCaseData(new AddingThinDamTestSteps()).SetName("Adding a thin dam should mark the output out of sync.");
            yield return new TestCaseData(new RemovingThinDamTestSteps()).SetName("Removing a thin dam should mark the output out of sync.");
        }

        private static IEnumerable<TestCaseData> GetTestStepsForFMRegionObjects()
        {
            yield return new TestCaseData(new AddingBoundaryTestSteps()).SetName("Adding a boundary should mark the output out of sync.");
            yield return new TestCaseData(new RemovingBoundaryTestSteps()).SetName("Removing a boundary should mark the output out of sync.");
            yield return new TestCaseData(new AddingPipeTestSteps()).SetName("Adding a pipe should mark the output out of sync.");
            yield return new TestCaseData(new RemovingPipeTestSteps()).SetName("Removing a pipe should mark the output out of sync.");
            yield return new TestCaseData(new AddingSourceAndSinkTestSteps()).SetName("Adding a source and sink should mark the output out of sync.");
            yield return new TestCaseData(new RemovingSourceAndSinkTestSteps()).SetName("Removing a source and sink should mark the output out of sync.");
        }

        private static IEnumerable<TestCaseData> GetTestStepsForHydroAreaPropertyChanges()
        {
            yield return new TestCaseData(new ChangingTheFormulaOfAStructureTestSteps()).SetName("Changing the formula of a structure should mark the output out of sync.");
            yield return new TestCaseData(new ChangingAPropertyInTheFormulaOfAStructureTestSteps()).SetName("Changing a property in the formula of a structure should mark the output out of sync.");
        }

        private static IEnumerable<TestCaseData> GetTestStepsForGridChanges()
        {
            yield return new TestCaseData(new ChangingTheGridTestSteps()).SetName("Changing the grid of the model should mark the output out of sync.");
        }

        public interface ITestStepsForModelOutputOutOfSync
        {
            void Arrange(WaterFlowFMModel model);
            void Act(WaterFlowFMModel model);
        }

        private class ModelDefinitionSetValueDirectlyTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly string propertyName;
            private readonly object value;

            public ModelDefinitionSetValueDirectlyTestSteps(string propertyName, object value)
            {
                this.propertyName = propertyName;
                this.value = value;
            }

            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.ModelDefinition.GetModelProperty(propertyName).Value = value;
        }

        private class ModelDefinitionSetValueAsStringTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly string propertyName;
            private readonly string value;

            public ModelDefinitionSetValueAsStringTestSteps(string propertyName, string value)
            {
                this.propertyName = propertyName;
                this.value = value;
            }

            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.ModelDefinition.GetModelProperty(propertyName).SetValueAsString(value);
        }

        private class AddingBoundaryTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.Boundaries.Add(new Feature2D());
        }

        private class RemovingBoundaryTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly Feature2D boundary = new Feature2D();

            public void Arrange(WaterFlowFMModel model) => model.Boundaries.Add(boundary);
            public void Act(WaterFlowFMModel model) => model.Boundaries.Remove(boundary);
        }

        private class AddingPipeTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.Pipes.Add(new Feature2D());
        }

        private class RemovingPipeTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly Feature2D pipe = new Feature2D();

            public void Arrange(WaterFlowFMModel model) => model.Pipes.Add(pipe);
            public void Act(WaterFlowFMModel model) => model.Pipes.Remove(pipe);
        }

        private class AddingSourceAndSinkTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.SourcesAndSinks.Add(new SourceAndSink());
        }

        private class RemovingSourceAndSinkTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly SourceAndSink sourceAndSink = new SourceAndSink();

            public void Arrange(WaterFlowFMModel model) => model.SourcesAndSinks.Add(sourceAndSink);
            public void Act(WaterFlowFMModel model) => model.SourcesAndSinks.Remove(sourceAndSink);
        }
        
        private class AddingObservationPointTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.Area.ObservationPoints.Add(new GroupableFeature2DPoint());
        }

        private class RemovingObservationPointTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly GroupableFeature2DPoint observationPoint = new GroupableFeature2DPoint();

            public void Arrange(WaterFlowFMModel model) => model.Area.ObservationPoints.Add(observationPoint);
            public void Act(WaterFlowFMModel model) => model.Area.ObservationPoints.Remove(observationPoint);
        }

        private class AddingFixedWeirTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.Area.FixedWeirs.Add(new FixedWeir());
        }

        private class RemovingFixedWeirTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly FixedWeir fixedWeir = new FixedWeir();

            public void Arrange(WaterFlowFMModel model) => model.Area.FixedWeirs.Add(fixedWeir);
            public void Act(WaterFlowFMModel model) => model.Area.FixedWeirs.Remove(fixedWeir);
        }

        private class AddingStructureTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.Area.Structures.Add(new Structure());
        }
        
        private class RemovingStructureTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly Structure structure = new Structure();

            public void Arrange(WaterFlowFMModel model) => model.Area.Structures.Add(structure);
            public void Act(WaterFlowFMModel model) => model.Area.Structures.Remove(structure);
        }

        private class AddingObservationCrossSectionTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D());
        }

        private class RemovingObservationCrossSectionTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly ObservationCrossSection2D observationCrossSection = new ObservationCrossSection2D();

            public void Arrange(WaterFlowFMModel model) => model.Area.ObservationCrossSections.Add(observationCrossSection);
            public void Act(WaterFlowFMModel model) => model.Area.ObservationCrossSections.Remove(observationCrossSection);
        }

        private class AddingPumpTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D());
        }

        private class RemovingPumpTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly Pump pump = new Pump();

            public void Arrange(WaterFlowFMModel model) => model.Area.Pumps.Add(pump);
            public void Act(WaterFlowFMModel model) => model.Area.Pumps.Remove(pump);
        }

        private class AddingBridgePillarTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.Area.BridgePillars.Add(new BridgePillar());
        }

        private class RemovingBridgePillarTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly BridgePillar bridgePillar = new BridgePillar();

            public void Arrange(WaterFlowFMModel model) => model.Area.BridgePillars.Add(bridgePillar);
            public void Act(WaterFlowFMModel model) => model.Area.BridgePillars.Remove(bridgePillar);
        }

        private class AddingDryPointTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.Area.DryPoints.Add(new GroupablePointFeature());
        }

        private class RemovingDryPointTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly GroupablePointFeature dryPoint = new GroupablePointFeature();

            public void Arrange(WaterFlowFMModel model) => model.Area.DryPoints.Add(dryPoint);
            public void Act(WaterFlowFMModel model) => model.Area.DryPoints.Remove(dryPoint);
        }

        private class AddingDryAreaTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.Area.DryAreas.Add(new GroupableFeature2DPolygon());
        }

        private class RemovingDryAreaTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly GroupableFeature2DPolygon dryArea = new GroupableFeature2DPolygon();

            public void Arrange(WaterFlowFMModel model) => model.Area.DryAreas.Add(dryArea);
            public void Act(WaterFlowFMModel model) => model.Area.DryAreas.Remove(dryArea);
        }

        private class AddingThinDamTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            public void Act(WaterFlowFMModel model) => model.Area.ThinDams.Add(new ThinDam2D());
        }

        private class RemovingThinDamTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly ThinDam2D thinDam = new ThinDam2D();

            public void Arrange(WaterFlowFMModel model) => model.Area.ThinDams.Add(thinDam);
            public void Act(WaterFlowFMModel model) => model.Area.ThinDams.Remove(thinDam);
        }

        private class ChangingTheFormulaOfAStructureTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly Structure structure = new Structure
            {
                Formula = new SimpleWeirFormula()
            };

            public void Arrange(WaterFlowFMModel model) => model.Area.Structures.Add(structure);
            public void Act(WaterFlowFMModel model) => structure.Formula = new GeneralStructureFormula();
        }

        private class ChangingAPropertyInTheFormulaOfAStructureTestSteps : ITestStepsForModelOutputOutOfSync
        {
            private readonly Structure structure = new Structure
            {
                Formula = new GeneralStructureFormula()
            };

            public void Arrange(WaterFlowFMModel model) => model.Area.Structures.Add(structure);
            public void Act(WaterFlowFMModel model) => ((GeneralStructureFormula)structure.Formula).PositiveFreeGateFlow = 3.5;
        }

        private class ChangingTheGridTestSteps : ITestStepsForModelOutputOutOfSync
        {
            public void Arrange(WaterFlowFMModel model) { }
            
            public void Act(WaterFlowFMModel model) => model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
    }

        private static void CreateRestartOutputFile(string tempDirectoryPath)
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
