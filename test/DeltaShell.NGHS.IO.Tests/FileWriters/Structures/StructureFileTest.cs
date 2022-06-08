using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class StructureFileTest
    {
        private const string expectedCategoryName = "Structure";
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

            Pump2D pump2D = CreateDefaultPump2D();
            pump2D.Capacity = expectedCapacity;

            Area.Pumps.Add(pump2D);

            // Action
            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();

            // Assert
            Assert.That(categories.Count, Is.EqualTo(1));

            DelftIniCategory structureCategory = categories.First();
            Assert.That(structureCategory, Is.Not.Null);
            Assert.That(structureCategory.Properties.Count, Is.EqualTo(6));

            CheckCommon2DDelftIniProperties(structureCategory, pump2D.Name, expectedType);
            CheckKeyValuePair(structureCategory, StructureRegion.Capacity.Key, expectedCapacity);
        }

        [Test]
        public void GivenAnHydroAreaWithPump2DThatHasATimeSeriesForCapacity_WhenGeneratingStructures_ThenThePumpCategoryHasTheCorrectCapacityValue()
        {
            Pump2D pump2D = CreateDefaultPump2D();
            pump2D.UseCapacityTimeSeries = true;

            const string expectedType = "pump";
            var expectedCapacityString = $"{pump2D.Name}_{StructureRegion.Capacity.Key}.tim";
            
            Area.Pumps.Add(pump2D);

            // Action
            IEnumerable<DelftIniCategory> categories =
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(categories.Count, Is.EqualTo(1));

            DelftIniCategory structureCategory = categories.First();
            Assert.IsNotNull(structureCategory);
            Assert.That(structureCategory.Properties.Count, Is.EqualTo(6));

            CheckCommon2DDelftIniProperties(structureCategory, pump2D.Name, expectedType);
            CheckKeyValuePair(structureCategory, StructureRegion.Capacity.Key, expectedCapacityString);
        }

        private static Pump2D CreateDefaultPump2D() =>
            new Pump2D(nameof(Pump2D), true)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(2, 2)
                })
            };

        [Test]
        public void GivenAnHydroAreaWithWeirThatHasATimeSeriesForCrestLevel_WhenGeneratingStructures_ThenWeirIsBeingWrittenToFileWithTimeSeriesFileNameInIniFile()
        {
            const string expectedType = "weir";
            const double expectedCrestWidth = 2.58;
            const double expectedCorrectionCoeff = 0.34;

            Weir2D weir2D = CreateDefaultWeir2D();
            weir2D.CrestWidth = expectedCrestWidth;
            weir2D.WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = expectedCorrectionCoeff };
            weir2D.UseCrestLevelTimeSeries = true;
            Area.Weirs.Add(weir2D);

            var expectedCrestLevelString = $"{weir2D.Name}_crest_level.tim";

            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(categories.Count, Is.EqualTo(1));

            DelftIniCategory structureCategory = categories.First();
            Assert.IsNotNull(structureCategory);
            Assert.That(structureCategory.Properties.Count, Is.EqualTo(9));

            CheckCommon2DDelftIniProperties(structureCategory, weir2D.Name, expectedType);
            CheckKeyValuePair(structureCategory, StructureRegion.CrestLevel.Key, expectedCrestLevelString);
            CheckKeyValuePair(structureCategory, StructureRegion.CrestWidth.Key, expectedCrestWidth);
            CheckKeyValuePair(structureCategory, StructureRegion.CorrectionCoeff.Key, expectedCorrectionCoeff);
        }

        [Test]
        public void GivenAnHydroAreaWithWeir_WhenGeneratingStructures_ThenWeirIsBeingCorrectlyWrittenToIniFile()
        {
            const string expectedType = "weir";
            const double expectedCrestLevel = 1.12;
            const double expectedCrestWidth = 2.58;
            const double expectedCorrectionCoef = 0.34;

            Weir2D weir2D = CreateDefaultWeir2D();
            weir2D.CrestLevel = expectedCrestLevel;
            weir2D.CrestWidth = expectedCrestWidth;
            weir2D.WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = expectedCorrectionCoef };
            Area.Weirs.Add(weir2D);

            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(categories.Count, Is.EqualTo(1));

            DelftIniCategory structureCategory = categories.First();
            Assert.IsNotNull(structureCategory);
            Assert.That(structureCategory.Properties.Count, Is.EqualTo(9));

            CheckCommon2DDelftIniProperties(structureCategory, weir2D.Name, expectedType);
            CheckKeyValuePair(structureCategory, StructureRegion.CrestLevel.Key, expectedCrestLevel);
            CheckKeyValuePair(structureCategory, StructureRegion.CrestWidth.Key, expectedCrestWidth);
            CheckKeyValuePair(structureCategory, StructureRegion.CorrectionCoeff.Key, expectedCorrectionCoef);
        }

        [Test]
        public void GivenAnHydroAreaWithGeneralStructure_WhenGeneratingStructures_ThenGeneralStructureIsBeingCorrectlyWrittenToIniFile()
        {
            const string expectedType = "generalstructure";
            const double expectedCrestLevel = 1.12;
            const double expectedCrestWidth = 2.58;

            Weir2D weir2D = CreateDefaultWeir2D();
            weir2D.CrestLevel = expectedCrestLevel;
            weir2D.CrestWidth = expectedCrestWidth;
            weir2D.WeirFormula = new GeneralStructureWeirFormula();
            
            Area.Weirs.Add(weir2D);

            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(categories.Count, Is.EqualTo(1));

            DelftIniCategory structureCategory = categories.First();
            Assert.IsNotNull(structureCategory);
            Assert.That(structureCategory.Properties.Count, Is.EqualTo(32));

            CheckCommon2DDelftIniProperties(structureCategory, weir2D.Name, expectedType);
        }

        private static Weir2D CreateDefaultWeir2D() =>
            new Weir2D(nameof(Weir2D), true)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(2, 2)
                })
            };

        [Test]
        public void GivenAnHydroAreaWithGate_WhenGeneratingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            const string expectedType = "gate";
            const double expectedSillLevel = 1.12;
            const double expectedSillWidth = 1.23;
            const double expectedLowerEdgeLevel = 0.01;
            const double expectedOpeningWidth = 5.11;
            const string expectedHorizontalOpeningDirection = "fromRight";

            Gate2D gate2D = CreateDefaultGate2D();
            gate2D.SillLevel = expectedSillLevel;
            gate2D.SillWidth = expectedSillWidth;
            gate2D.LowerEdgeLevel = expectedLowerEdgeLevel;
            gate2D.OpeningWidth = expectedOpeningWidth;
            gate2D.HorizontalOpeningDirection = GateOpeningDirection.FromRight;
            
            Area.Gates.Add(gate2D);

            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();

            Assert.That(categories.Count, Is.EqualTo(1));
            DelftIniCategory structureCategory = categories.First();
            Assert.IsNotNull(structureCategory);
            Assert.That(structureCategory.Properties.Count, Is.EqualTo(11));

            CheckCommon2DDelftIniProperties(structureCategory, gate2D.Name, expectedType);
            CheckKeyValuePair(structureCategory, StructureRegion.GateCrestLevel.Key, expectedSillLevel);
            CheckKeyValuePair(structureCategory, StructureRegion.GateCrestWidth.Key, expectedSillWidth);
            CheckKeyValuePair(structureCategory, StructureRegion.GateLowerEdgeLevel.Key, expectedLowerEdgeLevel);
            CheckKeyValuePair(structureCategory, StructureRegion.GateOpeningWidth.Key, expectedOpeningWidth);
            CheckKeyValuePair(structureCategory, StructureRegion.GateHorizontalOpeningDirection.Key, expectedHorizontalOpeningDirection);
        }

        [Test]
        public void GivenAnHydroAreaWithGateThatHasSillLevelTimeSeries_WhenGeneratingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            Gate2D gate2D = CreateDefaultGate2D();
            gate2D.UseSillLevelTimeSeries = true;

            var timFileName = $"{gate2D.Name}_crestLevel.tim";
            
            Area.Gates.Add(gate2D);

            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();

            DelftIniCategory structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
            CheckKeyValuePair(structureCategory, StructureRegion.GateCrestLevel.Key, timFileName);
        }

        [Test]
        public void GivenAnHydroAreaWithGateThatHasLowerEdgeLevelTimeSeries_WhenGeneratingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            Gate2D gate2D = CreateDefaultGate2D();
            gate2D.UseLowerEdgeLevelTimeSeries = true;

            var timFileName = $"{gate2D.Name}_{StructureRegion.GateLowerEdgeLevel.Key}.tim";

            Area.Gates.Add(gate2D);

            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();

            DelftIniCategory structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
            CheckKeyValuePair(structureCategory, StructureRegion.GateLowerEdgeLevel.Key, timFileName);
        }

        [Test]
        public void GivenAnHydroAreaWithGateThatHasOpeningWidthTimeSeries_WhenGeneratingStructures_ThenGateIsBeingCorrectlyWrittenToIniFile()
        {
            const string gateName = nameof(Gate2D);
            var timFileName = $"{gateName}_{StructureRegion.GateOpeningWidth.Key}.tim";

            Gate2D gate2D = CreateDefaultGate2D();
            gate2D.UseOpeningWidthTimeSeries = true;
            Area.Gates.Add(gate2D);

            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();

            DelftIniCategory structureCategory = categories.FirstOrDefault(c => c.Name == expectedCategoryName);
            CheckKeyValuePair(structureCategory, StructureRegion.GateOpeningWidth.Key, timFileName);
        }

        private static Gate2D CreateDefaultGate2D() =>
            new Gate2D(nameof(Gate2D))
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(2, 2)
                })
            };

        private static LeveeBreach CreateDefaultLeveeBreach() =>
            new LeveeBreach
            {
                Name = nameof(LeveeBreach),
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(2, 2)
                }),
            };

        [Test]
        public void GivenAnHydroAreaWithLeveeBreach_WhenGeneratingStructures_ThenLeveeBreachIsBeingCorrectlyWrittenToIniFile()
        {
            const double expectedBreachLocationX = 1.1;
            const double expectedBreachLocationY = 1.1;
            const int expectedStartTimeBreachGrowth = 7200;
            const string expectedBreachGrowthActivated = "0";

            LeveeBreach leveeBreach = CreateDefaultLeveeBreach();
            leveeBreach.BreachLocationX = expectedBreachLocationX;
            leveeBreach.BreachLocationY = expectedBreachLocationY;
            leveeBreach.SetBaseLeveeBreachSettings(referenceTime.AddHours(2.0), false);
            
            Area.LeveeBreaches.Add(leveeBreach);

            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();

            Assert.That(categories.Count, Is.EqualTo(1));

            DelftIniCategory structureCategory = categories.First();
            Assert.IsNotNull(structureCategory);
            Assert.That(structureCategory.Properties.Count, Is.EqualTo(9));

            CheckKeyValuePair(structureCategory, StructureRegion.BreachLocationX.Key, expectedBreachLocationX);
            CheckKeyValuePair(structureCategory, StructureRegion.BreachLocationY.Key, expectedBreachLocationY);
            CheckKeyValuePair(structureCategory, StructureRegion.StartTimeBreachGrowth.Key, expectedStartTimeBreachGrowth);
            CheckKeyValuePair(structureCategory, StructureRegion.BreachGrowthActivated.Key, expectedBreachGrowthActivated);
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

            LeveeBreach leveeBreach = CreateDefaultLeveeBreach();
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

            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(categories.Count, Is.EqualTo(1));

            DelftIniCategory structureCategory = categories.First();
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

        [Test]
        public void GivenAnHydroAreaWithLeveeBreachThatHasUserDefinedFormula_WhenGeneratingStructures_ThenLeveeBreachIsBeingCorrectlyWrittenToIniFile()
        {
            const int expectedAlgorithmValue = (int)LeveeBreachGrowthFormula.UserDefinedBreach;

            LeveeBreach leveeBreach = CreateDefaultLeveeBreach();
            leveeBreach.LeveeBreachFormula = LeveeBreachGrowthFormula.UserDefinedBreach;
            leveeBreach.SetBaseLeveeBreachSettings(referenceTime.AddHours(2.0), true);

            var timeSeriesFileName = $"{leveeBreach.Name}.tim";

            Area.LeveeBreaches.Add(leveeBreach);

            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(Regions, referenceTime)
                             .ToArray();
            Assert.That(categories.Count, Is.EqualTo(1));

            DelftIniCategory structureCategory = categories.First();
            Assert.IsNotNull(structureCategory);
            Assert.That(structureCategory.Properties.Count, Is.EqualTo(11));

            CheckKeyValuePair(structureCategory, StructureRegion.Algorithm.Key, expectedAlgorithmValue);
            CheckKeyValuePair(structureCategory, StructureRegion.TimeFileName.Key, timeSeriesFileName);
        }

        private static void AssertCorrectProperty(IDelftIniCategory category, string key, string value)
        {
            DelftIniProperty property = category.Properties.FirstOrDefault(p => p.Name == key);
            Assert.That(property, Is.Not.Null, $"{key} should not be null.");
            Assert.That(property.Value, Is.EqualTo(value), $"{key} should be {value}");
        }

        private static IEnumerable<TestCaseData> NetworkGenerateStructureData()
        {
            TestCaseData GenerateTestCaseData(IStructure1D structure,
                                              Action<IDelftIniCategory> assertAction)
            {
                var network = Substitute.For<IHydroNetwork>();
                network.Structures.Returns(_ => new[] { structure });
                return new TestCaseData(network, assertAction).SetName(structure.Name);
            }

            var weir = new Weir(nameof(Weir), true);
            void AssertCorrectWeir(IDelftIniCategory result)
            {
                Assert.That(result.Properties.Count, Is.EqualTo(5));
                AssertCorrectProperty(result, StructureRegion.AllowedFlowDir.Key, "both");
                AssertCorrectProperty(result, StructureRegion.CrestLevel.Key, "1.000");
                AssertCorrectProperty(result, StructureRegion.CrestWidth.Key, "5.000");
                AssertCorrectProperty(result, StructureRegion.CorrectionCoeff.Key, "1.000");
                AssertCorrectProperty(result, StructureRegion.UseVelocityHeight.Key, "true");
            }

            yield return GenerateTestCaseData(weir, AssertCorrectWeir);

            void AssertCorrectBridge(IDelftIniCategory result)
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

            void AssertCorrectCulvert(IDelftIniCategory result)
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

            void AssertCorrectOrifice(IDelftIniCategory result)
            {
                Assert.That(result.Properties.Count, Is.EqualTo(6));
                AssertCorrectProperty(result, StructureRegion.AllowedFlowDir.Key, "both");
                AssertCorrectProperty(result, StructureRegion.CrestLevel.Key, "1.000");
                AssertCorrectProperty(result, StructureRegion.CrestWidth.Key, "5.000");
                AssertCorrectProperty(result, StructureRegion.GateLowerEdgeLevel.Key, "2.000");
                AssertCorrectProperty(result, StructureRegion.CorrectionCoeff.Key, "0.630");
                AssertCorrectProperty(result, StructureRegion.UseVelocityHeight.Key, "true");
            }
            var orifice = new Orifice(nameof(Orifice), true);
            yield return GenerateTestCaseData(orifice, AssertCorrectOrifice);

            void AssertCorrectPump(IDelftIniCategory result)
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
        public void GivenAnHydroNetworkWithAStructure_WhenGeneratingStructures_CorrectCategoryGenerated(IHydroNetwork network,
                                                                                                        Action<IDelftIniCategory> assertAction)
        {
            IHydroRegion[] regions = { network };
            IEnumerable<DelftIniCategory> categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(regions, referenceTime)
                             .ToArray();
            
            Assert.That(categories.Count, Is.EqualTo(1));

            DelftIniCategory structureCategory = categories.First();
            Assert.IsNotNull(structureCategory);
            assertAction(structureCategory);
        }

        [Test]
        public void GivenAnHydroNetworkWithACompositeStructure_WhenGeneratingStructures_CorrectCategoriesGenerated()
        {
            var weir = new Weir(nameof(Weir), true);
            var compositeStructure = new CompositeBranchStructure(nameof(CompositeBranchStructure), 0.5);
            compositeStructure.Structures.Add(weir);
            
            Network.Structures.Returns(_ => new[] { compositeStructure });

            IHydroRegion[] regions = { Network };
            DelftIniCategory[] categories = 
                StructureFile.GenerateStructureCategoriesFromFmModel(regions, referenceTime)
                             .ToArray();
            
            Assert.That(categories.Count, Is.EqualTo(2));
            DelftIniCategory weirCategory = categories[0];
            Assert.That(weirCategory.Properties.Count, Is.EqualTo(5));
            AssertCorrectProperty(weirCategory, StructureRegion.AllowedFlowDir.Key, "both");
            AssertCorrectProperty(weirCategory, StructureRegion.CrestLevel.Key, "1.000");
            AssertCorrectProperty(weirCategory, StructureRegion.CrestWidth.Key, "5.000");
            AssertCorrectProperty(weirCategory, StructureRegion.CorrectionCoeff.Key, "1.000");
            AssertCorrectProperty(weirCategory, StructureRegion.UseVelocityHeight.Key, "true");

            DelftIniCategory compositeCategory = categories[1];
            Assert.That(compositeCategory.Properties.Count, Is.EqualTo(2));
            AssertCorrectProperty(compositeCategory, StructureRegion.NumberOfCompoundStructures.Key, "1");
            AssertCorrectProperty(compositeCategory, StructureRegion.StructureIds.Key, weir.Name);
        }

        private static IEnumerable<TestCaseData> TimeSeriesData()
        {
            TestCaseData GenerateTestData(IStructure structure, 
                                          string fileName,
                                          Action<IHydroNetwork, HydroArea, IStructure> configureAction,
                                          bool expectedResult,
                                          string propertyName,
                                          bool? hasTimeSeries=null)
            {
                var testName = $"{structure.Name}.Use{propertyName}: {hasTimeSeries ?? expectedResult}";

                return new TestCaseData(structure, fileName, configureAction, expectedResult)
                    .SetName(testName);
            }

            // Structure2D
            void ConfigurePump2D(IHydroNetwork network, HydroArea area, IStructure pump) =>
                area.Pumps.Add((Pump2D)pump);

            Pump2D pump2DWithTimeSeries = CreateDefaultPump2D();
            pump2DWithTimeSeries.UseCapacityTimeSeries = true;

            var pumpTimFileName = $"{pump2DWithTimeSeries.Name}_{StructureRegion.Capacity.Key}.tim";
            yield return GenerateTestData(pump2DWithTimeSeries, 
                                          pumpTimFileName, 
                                          ConfigurePump2D, 
                                          true, 
                                          nameof(Pump2D.Capacity));

            Pump2D pump2DWithoutTimeSeries = CreateDefaultPump2D();
            pump2DWithoutTimeSeries.UseCapacityTimeSeries = false;

            yield return GenerateTestData(pump2DWithoutTimeSeries, 
                                          pumpTimFileName, 
                                          ConfigurePump2D, 
                                          false,
                                          nameof(Pump2D.Capacity));

            void ConfigureWeir2D(IHydroNetwork network, HydroArea area, IStructure weir) =>
                area.Weirs.Add((Weir2D)weir);

            Weir2D weir2DWithTimeSeries = CreateDefaultWeir2D();
            weir2DWithTimeSeries.CrestWidth = 3.0;
            weir2DWithTimeSeries.WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = 2.0 };
            weir2DWithTimeSeries.UseCrestLevelTimeSeries = true;

            var weirTimFileName = $"{weir2DWithTimeSeries.Name}_crest_level.tim";
            yield return GenerateTestData(weir2DWithTimeSeries, 
                                          weirTimFileName, 
                                          ConfigureWeir2D, 
                                          true,
                                          nameof(Weir2D.CrestLevel));

            Weir2D weir2DWithoutTimeSeries = CreateDefaultWeir2D();
            weir2DWithoutTimeSeries.CrestWidth = 3.0;
            weir2DWithoutTimeSeries.WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = 2.0 };
            weir2DWithoutTimeSeries.UseCrestLevelTimeSeries = false;

            yield return GenerateTestData(weir2DWithoutTimeSeries, 
                                          weirTimFileName, 
                                          ConfigureWeir2D, 
                                          false,
                                          nameof(Weir2D.CrestLevel));

            void ConfigureGate2D(IHydroNetwork network, HydroArea area, IStructure gate) =>
                area.Gates.Add((Gate2D)gate);

            Gate2D gate2DWithCrestLevelTimeSeries = CreateDefaultGate2D();
            gate2DWithCrestLevelTimeSeries.UseSillLevelTimeSeries = true;
            var gateCrestLevelTimFileName = $"{gate2DWithCrestLevelTimeSeries}_{KnownStructureProperties.GateCrestLevel}.tim";
            yield return GenerateTestData(gate2DWithCrestLevelTimeSeries, 
                                          gateCrestLevelTimFileName, 
                                          ConfigureGate2D, 
                                          false,
                                          nameof(Gate2D.SillLevel), 
                                          true);

            Gate2D gate2DWithoutCrestLevelTimeSeries = CreateDefaultGate2D();
            gate2DWithoutCrestLevelTimeSeries.UseSillLevelTimeSeries = false;
            yield return GenerateTestData(gate2DWithoutCrestLevelTimeSeries, 
                                          gateCrestLevelTimFileName, 
                                          ConfigureGate2D, false,
                                          nameof(Gate2D.SillLevel));

            Gate2D gate2DWithLowerEdgeLevelTimeSeries = CreateDefaultGate2D();
            gate2DWithLowerEdgeLevelTimeSeries.UseSillLevelTimeSeries = true;
            var gateLowerEdgeLevelTimFileName = $"{gate2DWithCrestLevelTimeSeries}_{KnownStructureProperties.GateLowerEdgeLevel}.tim";
            yield return GenerateTestData(gate2DWithLowerEdgeLevelTimeSeries, 
                                          gateLowerEdgeLevelTimFileName, 
                                          ConfigureGate2D, 
                                          false,
                                          nameof(Gate2D.LowerEdgeLevel), 
                                          true);

            Gate2D gate2DWithoutLowerEdgeLevelTimeSeries = CreateDefaultGate2D();
            gate2DWithoutLowerEdgeLevelTimeSeries.UseSillLevelTimeSeries = false;
            yield return GenerateTestData(gate2DWithoutLowerEdgeLevelTimeSeries, 
                                          gateLowerEdgeLevelTimFileName, 
                                          ConfigureGate2D, 
                                          false,
                                          nameof(Gate2D.LowerEdgeLevel));

            Gate2D gate2DWithGateOpeningWidthTimeSeries = CreateDefaultGate2D();
            gate2DWithGateOpeningWidthTimeSeries.UseSillLevelTimeSeries = true;
            var gateOpeningWidthTimFileName = $"{gate2DWithCrestLevelTimeSeries}_{KnownStructureProperties.GateLowerEdgeLevel}.tim";
            yield return GenerateTestData(gate2DWithGateOpeningWidthTimeSeries, 
                                          gateOpeningWidthTimFileName, 
                                          ConfigureGate2D, 
                                          false,
                                          nameof(Gate2D.OpeningWidth), 
                                          true);

            Gate2D gate2DWithoutGateOpeningWidthTimeSeries = CreateDefaultGate2D();
            gate2DWithoutGateOpeningWidthTimeSeries.UseSillLevelTimeSeries = false;
            yield return GenerateTestData(gate2DWithoutGateOpeningWidthTimeSeries, 
                                          gateOpeningWidthTimFileName, 
                                          ConfigureGate2D, 
                                          false,
                                          nameof(Gate2D.OpeningWidth));

            void ConfigureLeveeBreach(IHydroNetwork network, HydroArea area, IStructure leveeBreach) =>
                area.LeveeBreaches.Add((LeveeBreach)leveeBreach);

            LeveeBreach leveeBreachUserDefinedFormula = CreateDefaultLeveeBreach();
            leveeBreachUserDefinedFormula.LeveeBreachFormula = LeveeBreachGrowthFormula.UserDefinedBreach;
            leveeBreachUserDefinedFormula.SetBaseLeveeBreachSettings(referenceTime.AddHours(2.0), true);

            var settings = leveeBreachUserDefinedFormula.GetActiveLeveeBreachSettings() as UserDefinedBreachSettings;
            settings?.ManualBreachGrowthSettings.Add(new BreachGrowthSetting
            {
                Width = 2.0,
                Height = 3.0,
                TimeSpan = new TimeSpan(0, 1, 0, 0)
            });

            var leveeBreachTimeSeriesFileName = $"{leveeBreachUserDefinedFormula.Name}.tim";
            yield return GenerateTestData(leveeBreachUserDefinedFormula,
                                          leveeBreachTimeSeriesFileName,
                                          ConfigureLeveeBreach,
                                          true,
                                          $"{nameof(LeveeBreach.LeveeBreachFormula)}.{nameof(LeveeBreachGrowthFormula.UserDefinedBreach)}");

            // Structure1D
            void ConfigureStructure1D(IHydroNetwork network, HydroArea area, IStructure structure)
            {
                IStructure1D[] structures = { (IStructure1D)structure };
                network.Structures.Returns(structures);
            }

            var weir1DWithTimeSeries = new Weir(nameof(Weir), true) { 
                CrestWidth = 3.0,
                WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = 2.0 },
                UseCrestLevelTimeSeries = true
            };

            var weir1DTimFileName = $"{weir1DWithTimeSeries.Name}_crest_level.tim";
            yield return GenerateTestData(weir1DWithTimeSeries, 
                                          weir1DTimFileName, 
                                          ConfigureStructure1D, 
                                          true,
                                          nameof(Weir.CrestLevel));

            var weir1DWithoutTimeSeries = new Weir(nameof(Weir), true) { 
                CrestWidth = 3.0,
                WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = 2.0 },
                UseCrestLevelTimeSeries = false
            };

            yield return GenerateTestData(weir1DWithoutTimeSeries, 
                                          weir1DTimFileName, 
                                          ConfigureStructure1D, 
                                          false,
                                          nameof(Weir.CrestLevel));

            var pump1DWithTimeSeries = new Pump(nameof(Pump), true) { 
                UseCapacityTimeSeries = true
            };

            var pump1DTimFileName = $"{pump1DWithTimeSeries.Name}_capacity.tim";
            yield return GenerateTestData(pump1DWithTimeSeries, 
                                          pump1DTimFileName, 
                                          ConfigureStructure1D, 
                                          true,
                                          nameof(Pump.Capacity));

            var pump1DWithoutTimeSeries = new Pump(nameof(Pump), true) { 
                UseCapacityTimeSeries = false
            };

            yield return GenerateTestData(pump1DWithoutTimeSeries, 
                                          pump1DTimFileName, 
                                          ConfigureStructure1D, 
                                          false,
                                          nameof(Pump.Capacity));

            var orifice1DWithTimeSeries = new Orifice(nameof(Orifice), true) { 
                CrestWidth = 3.0,
                WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = 2.0 },
                UseCrestLevelTimeSeries = true
            };

            var orifice1DTimFileName = $"{orifice1DWithTimeSeries.Name}_crest_level.tim";
            yield return GenerateTestData(orifice1DWithTimeSeries, 
                                          orifice1DTimFileName, 
                                          ConfigureStructure1D, 
                                          true,
                                          nameof(Orifice.CrestLevel));

            var orifice1DWithoutTimeSeries = new Orifice(nameof(Orifice), true) { 
                CrestWidth = 3.0,
                WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = 2.0 },
                UseCrestLevelTimeSeries = false
            };

            yield return GenerateTestData(orifice1DWithoutTimeSeries, 
                                          orifice1DTimFileName, 
                                          ConfigureStructure1D, 
                                          false,
                                          nameof(Orifice.CrestLevel));

            var orifice1DWithLowerEdgeLevelTimeSeries = new Orifice(nameof(Orifice), true);
            ((GatedWeirFormula) orifice1DWithLowerEdgeLevelTimeSeries.WeirFormula).UseLowerEdgeLevelTimeSeries = true;

            var orifice1DLowerEdgeLevelTimFileName = $"{orifice1DWithTimeSeries.Name}_gate_opening.tim";
            yield return GenerateTestData(orifice1DWithLowerEdgeLevelTimeSeries,
                                          orifice1DLowerEdgeLevelTimFileName,
                                          ConfigureStructure1D,
                                          true,
                                          nameof(GatedWeirFormula.LowerEdgeLevel));

            var orifice1DWithoutLowerEdgeLevelTimeSeries = new Orifice(nameof(Orifice), true);
            ((GatedWeirFormula) orifice1DWithoutLowerEdgeLevelTimeSeries.WeirFormula).UseLowerEdgeLevelTimeSeries = false;

            yield return GenerateTestData(orifice1DWithoutLowerEdgeLevelTimeSeries, 
                                          orifice1DLowerEdgeLevelTimFileName, 
                                          ConfigureStructure1D, 
                                          false,
                                          nameof(GatedWeirFormula.LowerEdgeLevel));

            var culvert1DWithTimeSeries = new Culvert(nameof(Culvert))
            {
                UseGateInitialOpeningTimeSeries = true,
            };

            var culvert1DTimFileName = $"{culvert1DWithTimeSeries.Name}_valve_opening_height.tim";
            yield return GenerateTestData(culvert1DWithTimeSeries,
                                          culvert1DTimFileName,
                                          ConfigureStructure1D,
                                          true,
                                          nameof(Culvert.GateInitialOpening));

            var culvert1DWithoutTimeSeries = new Culvert(nameof(Culvert))
            {
                UseGateInitialOpeningTimeSeries = false,
            };
            yield return GenerateTestData(culvert1DWithoutTimeSeries,
                                          culvert1DTimFileName,
                                          ConfigureStructure1D,
                                          false,
                                          nameof(Culvert.GateInitialOpening));

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(TimeSeriesData))]
        public void WriteStructureTimFiles_ProvidedStructure_ExpectedResults(IStructure structure, 
                                                                             string timFileName, 
                                                                             Action<IHydroNetwork, HydroArea, IStructure> configureStructure, 
                                                                             bool expectedResult)
        {
            using (var tempDir = new TemporaryDirectory())
            {
                string mduFilePath = Path.Combine(tempDir.Path, "FlowFM.mdu");
                string expectedTimFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(mduFilePath, 
                                                                                          timFileName);
                configureStructure(Network, Area, structure);

                StructureFile.WriteStructureTimFiles(Regions, mduFilePath, referenceTime);
                Assert.That(File.Exists(expectedTimFilePath), Is.EqualTo(expectedResult));
            }
        }

        private static void CheckCommon2DDelftIniProperties(IDelftIniCategory structureCategory, string structureName, string expectedType)
        {
            CheckKeyValuePair(structureCategory, StructureRegion.Id.Key, structureName);
            CheckKeyValuePair(structureCategory, StructureRegion.DefinitionType.Key, expectedType);
        }

        private static void CheckKeyValuePair(IDelftIniCategory structureCategory, string key, string expectedValue)
        {
            DelftIniProperty property = structureCategory.Properties.FirstOrDefault(p => p.Name == key);
            Assert.That(property?.Value, Is.EqualTo(expectedValue));
        }

        private static void CheckKeyValuePair(IDelftIniCategory structureCategory, string key, double expectedValue)
        {
            DelftIniProperty property = structureCategory.Properties.FirstOrDefault(p => p.Name == key);
            Assert.That(property, Is.Not.Null,
                        $"The requested property with name \"{key}\" was not present in the DelftIniCategory.");
            double valueAsDouble = double.Parse(property.Value, CultureInfo.InvariantCulture);
            Assert.That(valueAsDouble, Is.EqualTo(expectedValue));
        }
    }
}