using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DHYDRO.Common.IO.Ini;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class StructureFileTest
    {
        private const string expectedIniSectionName = "Structure";
        private static readonly DateTime referenceTime = new DateTime(2018, 8, 25);

        [SetUp]
        public void SetUp()
        {
            Area = new HydroArea();
            Network = Substitute.For<IHydroNetwork>();
        }

        private HydroArea Area { get; set; }
        private IHydroNetwork Network { get; set; }

        private IEnumerable<IHydroRegion> Regions => new IHydroRegion[] { Network, Area };

        [Test]
        public void GivenAnAreaWithPump2D_WhenGeneratingStructures_ThenPumpIsBeingWrittenToIniFile()
        {
            // Setup
            const string expectedType = "pump";
            const double expectedCapacity = 25.08;

            Pump2D pump2D = StructureFileTestHelper.CreateDefaultPump2D();
            pump2D.Capacity = expectedCapacity;

            Area.Pumps.Add(pump2D);

            // Action
            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();

            // Assert
            Assert.That(iniSections.Count, Is.EqualTo(1));

            IniSection structureIniSection = iniSections.First();
            Assert.That(structureIniSection, Is.Not.Null);
            Assert.That(structureIniSection.Properties.Count, Is.EqualTo(6));

            CheckCommon2DIniProperties(structureIniSection, pump2D.Name, expectedType);
            CheckKeyValuePair(structureIniSection, StructureRegion.Capacity.Key, expectedCapacity);
        }

        [Test]
        public void GivenAnHydroAreaWithPump2DThatHasATimeSeriesForCapacity_WhenGeneratingStructures_ThenThePumpIniSectionHasTheCorrectCapacityValue()
        {
            Pump2D pump2D = StructureFileTestHelper.CreateDefaultPump2D();
            pump2D.UseCapacityTimeSeries = true;
            
            string expected2DStructureFileName = $"{pump2D.Name}_{StructureRegion.Capacity.Key}.tim";

            const string expectedType = "pump";

            Area.Pumps.Add(pump2D);

            // Action
            IEnumerable<IniSection> iniSections =
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(iniSections.Count, Is.EqualTo(1));

            IniSection structureIniSection = iniSections.First();
            Assert.IsNotNull(structureIniSection);
            Assert.That(structureIniSection.Properties.Count, Is.EqualTo(6));

            CheckCommon2DIniProperties(structureIniSection, pump2D.Name, expectedType);
            CheckKeyValuePair(structureIniSection, StructureRegion.Capacity.Key, expected2DStructureFileName);
        }

        [Test]
        public void GivenAnHydroAreaWithWeirThatHasATimeSeriesForCrestLevel_WhenGeneratingStructures_ThenWeirIsBeingWrittenToFileWithTimeSeriesFileNameInIniFile()
        {
            const string expectedType = "weir";
            const double expectedCrestWidth = 2.58;
            const double expectedCorrectionCoeff = 0.34;

            Weir2D weir2D = StructureFileTestHelper.CreateDefaultWeir2D();
            weir2D.CrestWidth = expectedCrestWidth;
            weir2D.WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = expectedCorrectionCoeff };
            weir2D.UseCrestLevelTimeSeries = true;
            Area.Weirs.Add(weir2D);
            
            var expected2DStructureFileName = $"{weir2D.Name}_crest_level.tim";

            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(iniSections.Count, Is.EqualTo(1));

            IniSection structureIniSection = iniSections.First();
            Assert.IsNotNull(structureIniSection);
            Assert.That(structureIniSection.Properties.Count, Is.EqualTo(9));

            CheckCommon2DIniProperties(structureIniSection, weir2D.Name, expectedType);
            CheckKeyValuePair(structureIniSection, StructureRegion.CrestLevel.Key, expected2DStructureFileName);
            CheckKeyValuePair(structureIniSection, StructureRegion.CrestWidth.Key, expectedCrestWidth);
            CheckKeyValuePair(structureIniSection, StructureRegion.CorrectionCoeff.Key, expectedCorrectionCoeff);
        }

        [Test]
        public void GivenAnHydroAreaWithWeir_WhenGeneratingStructures_ThenWeirIsBeingCorrectlyWrittenToIniFile()
        {
            const string expectedType = "weir";
            const double expectedCrestLevel = 1.12;
            const double expectedCrestWidth = 2.58;
            const double expectedCorrectionCoef = 0.34;

            Weir2D weir2D = StructureFileTestHelper.CreateDefaultWeir2D();
            weir2D.CrestLevel = expectedCrestLevel;
            weir2D.CrestWidth = expectedCrestWidth;
            weir2D.WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = expectedCorrectionCoef };
            Area.Weirs.Add(weir2D);

            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(iniSections.Count, Is.EqualTo(1));

            IniSection structureIniSection = iniSections.First();
            Assert.IsNotNull(structureIniSection);
            Assert.That(structureIniSection.Properties.Count, Is.EqualTo(9));

            CheckCommon2DIniProperties(structureIniSection, weir2D.Name, expectedType);
            CheckKeyValuePair(structureIniSection, StructureRegion.CrestLevel.Key, expectedCrestLevel);
            CheckKeyValuePair(structureIniSection, StructureRegion.CrestWidth.Key, expectedCrestWidth);
            CheckKeyValuePair(structureIniSection, StructureRegion.CorrectionCoeff.Key, expectedCorrectionCoef);
        }

        [Test]
        public void GivenAnHydroAreaWithGeneralStructure_WhenGeneratingStructures_ThenGeneralStructureIsBeingCorrectlyWrittenToIniFile()
        {
            const string expectedType = "generalstructure";
            const double expectedCrestLevel = 1.12;
            const double expectedCrestWidth = 2.58;

            Weir2D weir2D = StructureFileTestHelper.CreateDefaultWeir2D();
            weir2D.CrestLevel = expectedCrestLevel;
            weir2D.CrestWidth = expectedCrestWidth;
            weir2D.WeirFormula = new GeneralStructureWeirFormula();
            
            Area.Weirs.Add(weir2D);

            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(iniSections.Count, Is.EqualTo(1));

            IniSection structureIniSection = iniSections.First();
            Assert.IsNotNull(structureIniSection);
            Assert.That(structureIniSection.Properties.Count, Is.EqualTo(32));

            CheckCommon2DIniProperties(structureIniSection, weir2D.Name, expectedType);
        }

        [Test]
        public void GivenAnHydroAreaWithGate_WhenGeneratingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            const string expectedType = "gate";
            const double expectedSillLevel = 1.12;
            const double expectedSillWidth = 1.23;
            const double expectedLowerEdgeLevel = 0.01;
            const double expectedOpeningWidth = 5.11;
            const string expectedHorizontalOpeningDirection = "fromRight";

            Gate2D gate2D = StructureFileTestHelper.CreateDefaultGate2D();
            gate2D.SillLevel = expectedSillLevel;
            gate2D.SillWidth = expectedSillWidth;
            gate2D.LowerEdgeLevel = expectedLowerEdgeLevel;
            gate2D.OpeningWidth = expectedOpeningWidth;
            gate2D.HorizontalOpeningDirection = GateOpeningDirection.FromRight;
            
            Area.Gates.Add(gate2D);

            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();

            Assert.That(iniSections.Count, Is.EqualTo(1));
            IniSection structureIniSection = iniSections.First();
            Assert.IsNotNull(structureIniSection);
            Assert.That(structureIniSection.Properties.Count, Is.EqualTo(11));

            CheckCommon2DIniProperties(structureIniSection, gate2D.Name, expectedType);
            CheckKeyValuePair(structureIniSection, StructureRegion.GateCrestLevel.Key, expectedSillLevel);
            CheckKeyValuePair(structureIniSection, StructureRegion.GateCrestWidth.Key, expectedSillWidth);
            CheckKeyValuePair(structureIniSection, StructureRegion.GateLowerEdgeLevel.Key, expectedLowerEdgeLevel);
            CheckKeyValuePair(structureIniSection, StructureRegion.GateOpeningWidth.Key, expectedOpeningWidth);
            CheckKeyValuePair(structureIniSection, StructureRegion.GateHorizontalOpeningDirection.Key, expectedHorizontalOpeningDirection);
        }

        [Test]
        public void GivenAnHydroAreaWithGateThatHasSillLevelTimeSeries_WhenGeneratingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            Gate2D gate2D = StructureFileTestHelper.CreateDefaultGate2D();
            gate2D.UseSillLevelTimeSeries = true;
            
            var expected2DStructureFileName = $"{gate2D.Name}_crestLevel.tim";

            Area.Gates.Add(gate2D);

            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();

            IniSection structureIniSection = iniSections.FirstOrDefault(c => c.Name == expectedIniSectionName);
            CheckKeyValuePair(structureIniSection, StructureRegion.GateCrestLevel.Key, expected2DStructureFileName);
        }

        [Test]
        public void GivenAnHydroAreaWithGateThatHasLowerEdgeLevelTimeSeries_WhenGeneratingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            Gate2D gate2D = StructureFileTestHelper.CreateDefaultGate2D();
            gate2D.UseLowerEdgeLevelTimeSeries = true;
            
            var expected2DStructureFileName = $"{gate2D.Name}_{StructureRegion.GateLowerEdgeLevel.Key}.tim";

            Area.Gates.Add(gate2D);

            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();

            IniSection structureIniSection = iniSections.FirstOrDefault(c => c.Name == expectedIniSectionName);
            CheckKeyValuePair(structureIniSection, StructureRegion.GateLowerEdgeLevel.Key, expected2DStructureFileName);
        }

        [Test]
        public void GivenAnHydroAreaWithGateThatHasOpeningWidthTimeSeries_WhenGeneratingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            const string gateName = nameof(Gate2D);
            var expected2DStructureFileName = $"{gateName}_{StructureRegion.GateOpeningWidth.Key}.tim";

            Gate2D gate2D = StructureFileTestHelper.CreateDefaultGate2D();
            gate2D.UseOpeningWidthTimeSeries = true;
            Area.Gates.Add(gate2D);

            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();

            IniSection structureIniSection = iniSections.FirstOrDefault(c => c.Name == expectedIniSectionName);
            CheckKeyValuePair(structureIniSection, StructureRegion.GateOpeningWidth.Key, expected2DStructureFileName);
        }

        [Test]
        public void GivenAnHydroAreaWithLeveeBreach_WhenGeneratingStructures_ThenLeveeBreachIsBeingCorrectlyWrittenToIniFile()
        {
            const double expectedBreachLocationX = 1.1;
            const double expectedBreachLocationY = 1.1;
            const int expectedStartTimeBreachGrowth = 7200;
            const string expectedBreachGrowthActivated = "0";

            LeveeBreach leveeBreach = StructureFileTestHelper.CreateDefaultLeveeBreach();
            leveeBreach.BreachLocationX = expectedBreachLocationX;
            leveeBreach.BreachLocationY = expectedBreachLocationY;
            leveeBreach.SetBaseLeveeBreachSettings(referenceTime.AddHours(2.0), false);
            
            Area.LeveeBreaches.Add(leveeBreach);

            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();

            Assert.That(iniSections.Count, Is.EqualTo(1));

            IniSection structureIniSection = iniSections.First();
            Assert.IsNotNull(structureIniSection);
            Assert.That(structureIniSection.Properties.Count, Is.EqualTo(9));

            CheckKeyValuePair(structureIniSection, StructureRegion.BreachLocationX.Key, expectedBreachLocationX);
            CheckKeyValuePair(structureIniSection, StructureRegion.BreachLocationY.Key, expectedBreachLocationY);
            CheckKeyValuePair(structureIniSection, StructureRegion.StartTimeBreachGrowth.Key, expectedStartTimeBreachGrowth);
            CheckKeyValuePair(structureIniSection, StructureRegion.BreachGrowthActivated.Key, expectedBreachGrowthActivated);
        }

        [Test]
        public void GivenAnHydroAreaWithLeveeBreachThatHasVerheijAsGrowthFormula_WhenGeneratingStructures_ThenLeveeBreachIsBeingCorrectlyWrittenToIniFile()
        {
            const double expectedBreachLocationX = 1.1;
            const double expectedBreachLocationY = 1.1;
            const int expectedStartTimeBreachGrowth = 7200;
            const string expectedBreachGrowthActivated = "1";
            const int expectedAlgorithmValue = (int)LeveeBreachGrowthFormula.VerheijvdKnaap2002;
            const double expectedSettingsValue = 1.09;
            const int expectedTimeToReachMinimumCrestLevel = 3600;

            LeveeBreach leveeBreach = StructureFileTestHelper.CreateDefaultLeveeBreach();
            leveeBreach.BreachLocationX = expectedBreachLocationX;
            leveeBreach.BreachLocationY = expectedBreachLocationY;
            leveeBreach.LeveeBreachFormula = LeveeBreachGrowthFormula.VerheijvdKnaap2002;
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

            Area.LeveeBreaches.Add(leveeBreach);

            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(iniSections.Count, Is.EqualTo(1));

            IniSection structureIniSection = iniSections.First();
            Assert.IsNotNull(structureIniSection);
            Assert.That(structureIniSection.Properties.Count, Is.EqualTo(17));

            CheckKeyValuePair(structureIniSection, StructureRegion.BreachLocationX.Key, expectedBreachLocationX);
            CheckKeyValuePair(structureIniSection, StructureRegion.BreachLocationY.Key, expectedBreachLocationY);
            CheckKeyValuePair(structureIniSection, StructureRegion.StartTimeBreachGrowth.Key, expectedStartTimeBreachGrowth);
            CheckKeyValuePair(structureIniSection, StructureRegion.BreachGrowthActivated.Key, expectedBreachGrowthActivated);

            CheckKeyValuePair(structureIniSection, StructureRegion.Algorithm.Key, expectedAlgorithmValue);
            CheckKeyValuePair(structureIniSection, StructureRegion.InitialCrestLevel.Key, expectedSettingsValue);
            CheckKeyValuePair(structureIniSection, StructureRegion.MinimumCrestLevel.Key, expectedSettingsValue);
            CheckKeyValuePair(structureIniSection, StructureRegion.InitalBreachWidth.Key, expectedSettingsValue);
            CheckKeyValuePair(structureIniSection, StructureRegion.TimeToReachMinimumCrestLevel.Key, expectedTimeToReachMinimumCrestLevel);
            CheckKeyValuePair(structureIniSection, StructureRegion.Factor1.Key, expectedSettingsValue);
            CheckKeyValuePair(structureIniSection, StructureRegion.Factor2.Key, expectedSettingsValue);
            CheckKeyValuePair(structureIniSection, StructureRegion.CriticalFlowVelocity.Key, expectedSettingsValue);
            
        }

        [Test]
        public void GivenAnHydroAreaWithLeveeBreachThatHasUserDefinedFormula_WhenGeneratingStructures_ThenLeveeBreachIsBeingCorrectlyWrittenToIniFile()
        {
            const int expectedAlgorithmValue = (int)LeveeBreachGrowthFormula.UserDefinedBreach;

            LeveeBreach leveeBreach = StructureFileTestHelper.CreateDefaultLeveeBreach();
            leveeBreach.LeveeBreachFormula = LeveeBreachGrowthFormula.UserDefinedBreach;
            leveeBreach.SetBaseLeveeBreachSettings(referenceTime.AddHours(2.0), true);
            
            string expected2DStructureFileName = $"{leveeBreach.Name}.tim";

            Area.LeveeBreaches.Add(leveeBreach);

            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(iniSections.Count, Is.EqualTo(1));

            IniSection structureIniSection = iniSections.First();
            Assert.IsNotNull(structureIniSection);
            Assert.That(structureIniSection.Properties.Count, Is.EqualTo(11));

            CheckKeyValuePair(structureIniSection, StructureRegion.Algorithm.Key, expectedAlgorithmValue);
            CheckKeyValuePair(structureIniSection, StructureRegion.TimeFileName.Key, expected2DStructureFileName);
        }

        private static void AssertCorrectProperty(IniSection iniSection, string key, string value)
        {
            IniProperty property = iniSection.Properties.FirstOrDefault(p => p.Key == key);
            Assert.That(property, Is.Not.Null, $"{key} should not be null.");
            Assert.That(property.Value, Is.EqualTo(value), $"{key} should be {value}");
        }

        private static IEnumerable<TestCaseData> NetworkGenerateStructureData()
        {
            TestCaseData GenerateTestCaseData(IStructure1D structure,
                                              Action<IniSection> assertAction)
            {
                var network = Substitute.For<IHydroNetwork>();
                network.Structures.Returns(_ => new[] { structure });
                return new TestCaseData(network, assertAction).SetName(structure.Name);
            }

            var weir = new Weir(nameof(Weir), true);
            void AssertCorrectWeir(IniSection result)
            {
                Assert.That(result.Properties.Count, Is.EqualTo(5));
                AssertCorrectProperty(result, StructureRegion.AllowedFlowDir.Key, "both");
                AssertCorrectProperty(result, StructureRegion.CrestLevel.Key, "1.000");
                AssertCorrectProperty(result, StructureRegion.CrestWidth.Key, "5.000");
                AssertCorrectProperty(result, StructureRegion.CorrectionCoeff.Key, "1.000");
                AssertCorrectProperty(result, StructureRegion.UseVelocityHeight.Key, "true");
            }

            yield return GenerateTestCaseData(weir, AssertCorrectWeir);

            void AssertCorrectBridge(IniSection result)
            {
                Assert.That(result.Properties.Count, Is.EqualTo(8));
                AssertCorrectProperty(result, StructureRegion.AllowedFlowDir.Key, "both");
                AssertCorrectProperty(result, StructureRegion.Shift.Key, "0.000");
                AssertCorrectProperty(result, StructureRegion.CsDefId.Key, "Bridge");
                AssertCorrectProperty(result, StructureRegion.Length.Key, "20.000");
                AssertCorrectProperty(result, StructureRegion.InletLossCoeff.Key, "0.000");
                AssertCorrectProperty(result, StructureRegion.OutletLossCoeff.Key, "0.000");
                AssertCorrectProperty(result, StructureRegion.FrictionType.Key, "0");
                AssertCorrectProperty(result, StructureRegion.Friction.Key, "0.00000");
            }
            var bridge = new Bridge(nameof(Bridge));
            yield return GenerateTestCaseData(bridge, AssertCorrectBridge);

            void AssertCorrectCulvert(IniSection result)
            {
                Assert.That(result.Properties.Count, Is.EqualTo(11));
                AssertCorrectProperty(result, StructureRegion.AllowedFlowDir.Key, "both");
                AssertCorrectProperty(result, StructureRegion.LeftLevel.Key, "0.000");
                AssertCorrectProperty(result, StructureRegion.RightLevel.Key, "0.000");
                AssertCorrectProperty(result, StructureRegion.CsDefId.Key, "Culvert");
                AssertCorrectProperty(result, StructureRegion.Length.Key, "0.000");
                AssertCorrectProperty(result, StructureRegion.InletLossCoeff.Key, "0.000");
                AssertCorrectProperty(result, StructureRegion.OutletLossCoeff.Key, "0.000");
                AssertCorrectProperty(result, StructureRegion.ValveOnOff.Key, "0");
                AssertCorrectProperty(result, StructureRegion.IniValveOpen.Key, "1.000");
                AssertCorrectProperty(result, StructureRegion.BedFrictionType.Key, "0");
                AssertCorrectProperty(result, StructureRegion.BedFriction.Key, "0.00000");

            }
            var culvert = new Culvert(nameof(Culvert));
            yield return GenerateTestCaseData(culvert, AssertCorrectCulvert);

            void AssertCorrectOrifice(IniSection result)
            {
                Assert.That(result.Properties.Count, Is.EqualTo(6));
                AssertCorrectProperty(result, StructureRegion.AllowedFlowDir.Key, "both");
                AssertCorrectProperty(result, StructureRegion.CrestLevel.Key, "1.000");
                AssertCorrectProperty(result, StructureRegion.CrestWidth.Key, "5.000");
                AssertCorrectProperty(result, StructureRegion.GateLowerEdgeLevel.Key, "11.000");
                AssertCorrectProperty(result, StructureRegion.CorrectionCoeff.Key, "0.630");
                AssertCorrectProperty(result, StructureRegion.UseVelocityHeight.Key, "true");
            }
            var orifice = new Orifice(nameof(Orifice), true);
            yield return GenerateTestCaseData(orifice, AssertCorrectOrifice);

            void AssertCorrectPump(IniSection result)
            {
                Assert.That(result.Properties.Count, Is.EqualTo(11));
                AssertCorrectProperty(result, StructureRegion.Orientation.Key, "positive");
                AssertCorrectProperty(result, "controlSide", "suctionSide");
                AssertCorrectProperty(result, "numStages", "1");
                AssertCorrectProperty(result, StructureRegion.Capacity.Key, "1.0000");
                AssertCorrectProperty(result, StructureRegion.StartLevelSuctionSide.Key, "3.000");
                AssertCorrectProperty(result, StructureRegion.StopLevelSuctionSide.Key, "2.000");
                AssertCorrectProperty(result, StructureRegion.StartLevelDeliverySide.Key, "0.000");
                AssertCorrectProperty(result, StructureRegion.StopLevelDeliverySide.Key, "0.000");
                AssertCorrectProperty(result, StructureRegion.ReductionFactorLevels.Key, "0");
                AssertCorrectProperty(result, StructureRegion.Head.Key, "0.0000000e+000");
                AssertCorrectProperty(result, StructureRegion.ReductionFactor.Key, "1.0000000e+000");
            }
            var pump = new Pump(nameof(Pump), true);
            yield return GenerateTestCaseData(pump, AssertCorrectPump);

        }

        [Test]
        [TestCaseSource(nameof(NetworkGenerateStructureData))]
        public void GivenAnHydroNetworkWithAStructure_WhenGeneratingStructures_CorrectIniSectionGenerated(IHydroNetwork network,
                                                                                                        Action<IniSection> assertAction)
        {
            IHydroRegion[] regions = { network };
            IEnumerable<IniSection> iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(regions, referenceTime)
                             .ToArray();
            
            Assert.That(iniSections.Count, Is.EqualTo(1));

            IniSection structureIniSection = iniSections.First();
            Assert.IsNotNull(structureIniSection);
            assertAction(structureIniSection);
        }

        [Test]
        public void GivenAnHydroNetworkWithACompositeStructure_WhenGeneratingStructures_CorrectIniSectionsGenerated()
        {
            var weir = new Weir(nameof(Weir), true);
            var compositeStructure = new CompositeBranchStructure(nameof(CompositeBranchStructure), 0.5);
            compositeStructure.Structures.Add(weir);
            
            Network.Structures.Returns(_ => new[] { compositeStructure });

            IHydroRegion[] regions = { Network };
            IniSection[] iniSections = 
                StructureFile.GenerateStructureIniSectionsFromFmModel(regions, referenceTime)
                             .ToArray();
            
            Assert.That(iniSections.Count, Is.EqualTo(2));
            IniSection weirIniSection = iniSections[0];
            Assert.That(weirIniSection.Properties.Count, Is.EqualTo(5));
            AssertCorrectProperty(weirIniSection, StructureRegion.AllowedFlowDir.Key, "both");
            AssertCorrectProperty(weirIniSection, StructureRegion.CrestLevel.Key, "1.000");
            AssertCorrectProperty(weirIniSection, StructureRegion.CrestWidth.Key, "5.000");
            AssertCorrectProperty(weirIniSection, StructureRegion.CorrectionCoeff.Key, "1.000");
            AssertCorrectProperty(weirIniSection, StructureRegion.UseVelocityHeight.Key, "true");

            IniSection compositeIniSection = iniSections[1];
            Assert.That(compositeIniSection.Properties.Count, Is.EqualTo(2));
            AssertCorrectProperty(compositeIniSection, StructureRegion.NumberOfCompoundStructures.Key, "1");
            AssertCorrectProperty(compositeIniSection, StructureRegion.StructureIds.Key, weir.Name);
        }

        private static void CheckCommon2DIniProperties(IniSection structureIniSection, string structureName, string expectedType)
        {
            CheckKeyValuePair(structureIniSection, StructureRegion.Id.Key, structureName);
            CheckKeyValuePair(structureIniSection, StructureRegion.DefinitionType.Key, expectedType);
        }

        private static void CheckKeyValuePair(IniSection structureIniSection, string key, string expectedValue)
        {
            IniProperty property = structureIniSection.Properties.FirstOrDefault(p => p.Key == key);
            Assert.That(property?.Value, Is.EqualTo(expectedValue));
        }

        private static void CheckKeyValuePair(IniSection structureIniSection, string key, double expectedValue)
        {
            IniProperty property = structureIniSection.Properties.FirstOrDefault(p => p.Key == key);
            Assert.That(property, Is.Not.Null,
                        $"The requested property with name \"{key}\" was not present in the IniSection.");
            double valueAsDouble = double.Parse(property.Value, CultureInfo.InvariantCulture);
            Assert.That(valueAsDouble, Is.EqualTo(expectedValue));
        }
    }
}