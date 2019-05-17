using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using Point = NetTopologySuite.Geometries.Point;


namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructureFactoryTest
    {
        private MockRepository mocks;
        private string structuresPath;
        private DateTime refDate;
        private Dictionary<string, double> constValLookUpTable;
        private Dictionary<string, string> timeSeriesLookUpTable;
        private Dictionary<string, Dictionary<string, string>> propertyNameMap;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            constValLookUpTable = new Dictionary<string, double>()
            {
                [KnownStructureProperties.CrestLevel] = 1.0,
                [KnownStructureProperties.CrestWidth] = 2.0,
                [KnownStructureProperties.LateralContractionCoefficient] = 4.0,
                [KnownStructureProperties.GateOpeningWidth] = 8.0,
                [KnownStructureProperties.GateLowerEdgeLevel] = 16.0,
                [KnownStructureProperties.GateHeight] = 32.0,

                [KnownGeneralStructureProperties.Upstream2Width.GetDescription()]    = 64.0,
                [KnownGeneralStructureProperties.Upstream1Width.GetDescription()]  = 128.0,
                [KnownGeneralStructureProperties.Downstream1Width.GetDescription()] = 256.0,
                [KnownGeneralStructureProperties.Downstream2Width.GetDescription()]   = 512.0,

                [KnownGeneralStructureProperties.Upstream2Level.GetDescription()]   = 1024.0,
                [KnownGeneralStructureProperties.Upstream1Level.GetDescription()]  = 2048.0,
                [KnownGeneralStructureProperties.Downstream1Level.GetDescription()] = 4096.0,
                [KnownGeneralStructureProperties.Downstream2Level.GetDescription()]  = 8192.0,

                [KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient.GetDescription()]  = 16384.0,
                [KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient.GetDescription()] = 32768.0,
                [KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient.GetDescription()]  = 65536.0,
                [KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient.GetDescription()] = 131072.0,

                [KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient.GetDescription()]  = 262144.0,
                [KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient.GetDescription()] = 524288.0,
                [KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient.GetDescription()]  = 1048576.0,
                [KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient.GetDescription()] = 2097152.0,

                [KnownGeneralStructureProperties.ExtraResistance.GetDescription()] = 41927.0,
            };

            timeSeriesLookUpTable = new Dictionary<string, string>()
            {
                [KnownStructureProperties.CrestLevel] = $"weir_{KnownStructureProperties.CrestLevel}.tim",
                [KnownStructureProperties.GateLowerEdgeLevel] = $"Gate02_{KnownStructureProperties.GateLowerEdgeLevel}.tim",
                [KnownStructureProperties.GateOpeningWidth] = $"Gate02_{KnownStructureProperties.GateOpeningWidth}.tim",
            };

            propertyNameMap = new Dictionary<string, Dictionary<string, string>>()
            {
                [StructureRegion.StructureTypeName.Weir] = new Dictionary<string, string>()
                {
                    [KnownStructureProperties.CrestLevel] = KnownStructureProperties.CrestLevel,
                    [KnownStructureProperties.CrestWidth] = KnownStructureProperties.CrestWidth,
                },
                [StructureRegion.StructureTypeName.Gate] = new Dictionary<string, string>()
                {
                    [KnownStructureProperties.CrestLevel]         = KnownStructureProperties.CrestLevel,
                    [KnownStructureProperties.CrestWidth]         = KnownStructureProperties.CrestWidth,
                    [KnownStructureProperties.GateOpeningWidth]   = KnownStructureProperties.GateOpeningWidth,
                    [KnownStructureProperties.GateLowerEdgeLevel] = KnownStructureProperties.GateLowerEdgeLevel,
                    [KnownStructureProperties.GateHeight]     = KnownStructureProperties.GateHeight,
                    [KnownStructureProperties.GateOpeningHorizontalDirection] = 
                        KnownStructureProperties.GateOpeningHorizontalDirection,
                },
                [StructureRegion.StructureTypeName.GeneralStructure] = new Dictionary<string, string>()
                {

                    [KnownStructureProperties.CrestLevel]         = KnownGeneralStructureProperties.CrestLevel.GetDescription(),
                    [KnownStructureProperties.CrestWidth]         = KnownGeneralStructureProperties.CrestWidth.GetDescription(),
                    [KnownStructureProperties.GateOpeningWidth]   = KnownGeneralStructureProperties.GateOpeningWidth.GetDescription(),
                    [KnownStructureProperties.GateLowerEdgeLevel] = KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription(),
                    [KnownStructureProperties.GateHeight]     = KnownGeneralStructureProperties.GateHeight.GetDescription(),
                    [KnownStructureProperties.GateOpeningHorizontalDirection] = KnownGeneralStructureProperties.GateOpeningHorizontalDirection.GetDescription(),
                }
            };

            refDate = DateTime.MinValue;
            structuresPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");
        }

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        #region Pump

        [Test]
        public void CreatePumpWithConstantCapacityTest()
        {
            var structure = new Structure2D("pump");
            structure.AddProperty(KnownStructureProperties.Type, typeof(string), "pump");
            structure.AddProperty(KnownStructureProperties.Name, typeof(string), "pump01");
            structure.AddProperty(KnownStructureProperties.X, typeof(double), "680");
            structure.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            structure.AddProperty(KnownStructureProperties.Capacity, typeof(Steerable), "2");

            var dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            var pump = StructureFactory.CreatePump(structure, dummyPath, new DateTime());
            Assert.AreEqual("pump01", pump.Name);
            Assert.IsTrue(pump.CanBeTimedependent);
            Assert.IsNull(pump.LongName);
            Assert.IsNull(pump.Branch);
            Assert.IsNaN(pump.Chainage);
            Assert.AreEqual(new Point(680, 360), pump.Geometry);
            Assert.IsFalse(pump.UseCapacityTimeSeries);
            Assert.AreEqual(2.0, pump.Capacity);
            Assert.AreEqual(0, pump.CapacityTimeSeries.Time.Values.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CreatePumpWithCapacityTimeSeriesTest()
        {
            var structure = new Structure2D("pump");
            structure.AddProperty(KnownStructureProperties.Type, typeof(string), "pump");
            structure.AddProperty(KnownStructureProperties.Name, typeof(string), "pump05");
            structure.AddProperty(KnownStructureProperties.PolylineFile, typeof(string), "pump05.pli");
            structure.AddProperty(KnownStructureProperties.Capacity, typeof(Steerable), "pump05_capacity.tim");

            var dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            var pump = StructureFactory.CreatePump(structure, dummyPath, new DateTime(2013, 1, 1));
            Assert.AreEqual("pump05", pump.Name);
            Assert.IsTrue(pump.CanBeTimedependent);
            Assert.IsNull(pump.LongName);
            Assert.IsNull(pump.Branch);
            Assert.IsNaN(pump.Chainage);
            Assert.AreEqual(new LineString(new [] { new Coordinate(1, 2), new Coordinate(3, 4), new Coordinate(6, 7) }), pump.Geometry);

            Assert.IsTrue(pump.UseCapacityTimeSeries);
            Assert.AreEqual(1.0, pump.Capacity);
            Assert.AreEqual(2, pump.CapacityTimeSeries.Time.Values.Count);
            Assert.AreEqual(5.6, pump.CapacityTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(11.12, pump.CapacityTimeSeries[new DateTime(2013, 1, 1, 1, 2, 0)]);
        }

        [Test]
        public void CreatePumpFromIncorrectType()
        {
            var structure = new Structure2D("test");
            Assert.Throws<FormatException>(() => StructureFactory.CreatePump(structure, null, new DateTime()));
        }

        #endregion

        #region General Structure

        [Test]
        public void GivenGeneralStructureAsStructure2D_WhenCreatingStructure_ThenTypeIsWeir()
        {
            var generalStructure = new Structure2D(StructureRegion.StructureTypeName.GeneralStructure);
            var result = StructureFactory.CreateStructure(generalStructure, null, new DateTime());
            Assert.IsTrue(result is IWeir);
        }

        [Test]
        public void GivenGeneralStructureAsStructure2D_WhenCreatingStructure_ThenWeirFormulaIsAGeneralStructureWeirFormula()
        {
            var generalStructure = new Structure2D(StructureRegion.StructureTypeName.GeneralStructure);
            var result = StructureFactory.CreateStructure(generalStructure, null, new DateTime());
            Assert.IsTrue(result is IWeir);

            var weir = (Weir) result;
            Assert.That(weir.WeirFormula is GeneralStructureWeirFormula); 
        }

        [Test]
        public void GivenGeneralStructureAsStructure2DWithKnownPropertyUnequalToZero_WhenCreatingStructure_ThenWeirAdaptsProperty()
        {
            var knownProperties = Enum.GetValues(typeof(KnownGeneralStructureProperties));
            foreach (var knownProperty in knownProperties)
            {
                var property = (KnownGeneralStructureProperties) knownProperty;

                var generalStructure = new Structure2D(StructureRegion.StructureTypeName.GeneralStructure);

                if (property == KnownGeneralStructureProperties.GateOpeningWidth ||
                    property == KnownGeneralStructureProperties.GateLowerEdgeLevel ||
                    property == KnownGeneralStructureProperties.CrestLevel)
                {
                    generalStructure.AddProperty(property.GetDescription(), typeof(Steerable), "12.34");
                }
                else if (property == KnownGeneralStructureProperties.GateOpeningHorizontalDirection)
                {
                    continue;
                }
                else 
                {
                    generalStructure.AddProperty(property.GetDescription(), typeof(double), "12.34");
                }
                
                var resultingStructure = StructureFactory.CreateStructure(generalStructure, null, new DateTime());
                var weir = resultingStructure as Weir;
                Assert.NotNull(weir);
            
                var weirFormulaValueDictionary = ConstructWeirFormulaValueDictionary(weir);
                Assert.That(weirFormulaValueDictionary[property], Is.EqualTo(12.34), property.GetDescription());
            }
        }

        [Test]
        public void GivenGeneralStructureAsStructure2DWithExtraResistanceEqualToZero_WhenCreatingStructure_ThenUseExtraResistanceIsFalse()
        {
            var generalStructure = new Structure2D(StructureRegion.StructureTypeName.GeneralStructure);
            generalStructure.AddProperty(KnownGeneralStructureProperties.ExtraResistance.GetDescription(), typeof(double), "0.0");

            var resultingStructure = StructureFactory.CreateStructure(generalStructure, null, new DateTime());
            var weir = resultingStructure as Weir;
            Assert.NotNull(weir);

            var weirFormula = weir.WeirFormula as GeneralStructureWeirFormula;
            Assert.NotNull(weirFormula);
            Assert.IsFalse(weirFormula.UseExtraResistance);
        }

        private static Dictionary<KnownGeneralStructureProperties, object> ConstructWeirFormulaValueDictionary(IWeir weir)
        {
            var weirFormula = weir.WeirFormula as GeneralStructureWeirFormula;
            Assert.NotNull(weirFormula);

            var dictionary = new Dictionary<KnownGeneralStructureProperties, object>
            {
                {KnownGeneralStructureProperties.Upstream2Width, weirFormula.WidthLeftSideOfStructure},
                {KnownGeneralStructureProperties.Upstream1Width, weirFormula.WidthStructureLeftSide},
                {KnownGeneralStructureProperties.CrestWidth, weirFormula.WidthStructureCentre },
                {KnownGeneralStructureProperties.Downstream1Width, weirFormula.WidthStructureRightSide},
                {KnownGeneralStructureProperties.Downstream2Width, weirFormula.WidthRightSideOfStructure},
                {KnownGeneralStructureProperties.Upstream2Level, weirFormula.BedLevelLeftSideOfStructure},
                {KnownGeneralStructureProperties.Upstream1Level, weirFormula.BedLevelLeftSideStructure },
                {KnownGeneralStructureProperties.CrestLevel, weirFormula.BedLevelStructureCentre },
                {KnownGeneralStructureProperties.Downstream1Level, weirFormula.BedLevelRightSideStructure },
                {KnownGeneralStructureProperties.Downstream2Level, weirFormula.BedLevelRightSideOfStructure },
                {KnownGeneralStructureProperties.GateHeight, weirFormula.DoorHeight},
                {KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient, weirFormula.PositiveFreeGateFlow},
                {KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient, weirFormula.PositiveDrownedGateFlow},
                {KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient, weirFormula.PositiveFreeWeirFlow},
                {KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient, weirFormula.PositiveDrownedWeirFlow},
                {KnownGeneralStructureProperties.PositiveContractionCoefficientFreeGate, weirFormula.PositiveContractionCoefficient},
                {KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient, weirFormula.NegativeFreeGateFlow},
                {KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient, weirFormula.NegativeDrownedGateFlow},
                {KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient, weirFormula.NegativeFreeWeirFlow},
                {KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient, weirFormula.NegativeDrownedWeirFlow},
                {KnownGeneralStructureProperties.NegativeContractionCoefficientFreeGate, weirFormula.NegativeContractionCoefficient},
                {KnownGeneralStructureProperties.ExtraResistance, weirFormula.ExtraResistance},
                {KnownGeneralStructureProperties.GateOpeningWidth, weirFormula.HorizontalDoorOpeningWidth},
                {KnownGeneralStructureProperties.GateLowerEdgeLevel, weirFormula.LowerEdgeLevel}
            };
            return dictionary;
        }

        #endregion

        #region Weir

        [Test]
        public void CreateWeirFromIncorrectType()
        {
            var structure = new Structure2D("test");
            Assert.Throws<FormatException>(() => StructureFactory.CreateWeir(structure, null, new DateTime()));
        }

        [Test]
        public void CreateWeirWithConstantCrestLevel()
        {
            var structure = new Structure2D("weir");
            structure.AddProperty(KnownStructureProperties.Type, typeof(string), "weir");
            structure.AddProperty(KnownStructureProperties.Name, typeof(string), "Weir_down");
            structure.AddProperty(KnownStructureProperties.X, typeof(double), "680");
            structure.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            structure.AddProperty(KnownStructureProperties.CrestLevel, typeof(Steerable), "2");
            structure.AddProperty(KnownStructureProperties.LateralContractionCoefficient, typeof(double), "0.7");

            var dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            var weir = StructureFactory.CreateWeir(structure, dummyPath, new DateTime());
            Assert.AreEqual("Weir_down", weir.Name);
            Assert.IsTrue(weir.CanBeTimedependent);
            Assert.IsNull(weir.LongName);
            Assert.IsNull(weir.Branch);
            Assert.IsNaN(weir.Chainage);
            Assert.AreEqual(new Point(680, 360), weir.Geometry);
            Assert.IsFalse(weir.UseCrestLevelTimeSeries);
            Assert.AreEqual(2.0, weir.CrestLevel);
            Assert.AreEqual(double.NaN, weir.CrestWidth);
            Assert.AreEqual(0, weir.CrestLevelTimeSeries.Time.Values.Count);
            Assert.IsInstanceOf<SimpleWeirFormula>(weir.WeirFormula);
            var simpleWeirFormula = (SimpleWeirFormula) weir.WeirFormula;
            Assert.AreEqual(0.7, simpleWeirFormula.LateralContraction);
        }


        [Test]
        public void CreateWeirWithCrestLevelTimeSeries()
        {
            var structure = new Structure2D("weir");
            structure.AddProperty(KnownStructureProperties.Type, typeof(string), "weir");
            structure.AddProperty(KnownStructureProperties.Name, typeof(string), "Weir_moving");
            structure.AddProperty(KnownStructureProperties.X, typeof(double), "680");
            structure.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            structure.AddProperty(KnownStructureProperties.CrestLevel, typeof(Steerable), "weir_CrestLevel.tim");
            structure.AddProperty(KnownStructureProperties.CrestWidth, typeof (double), "23.5");
            structure.AddProperty(KnownStructureProperties.LateralContractionCoefficient, typeof(double), "0.7");

            var dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            var weir = StructureFactory.CreateWeir(structure, dummyPath, new DateTime(2013, 1, 1));
            Assert.AreEqual("Weir_moving", weir.Name);
            Assert.IsTrue(weir.CanBeTimedependent);
            Assert.IsNull(weir.LongName);
            Assert.IsNull(weir.Branch);
            Assert.IsNaN(weir.Chainage);
            Assert.AreEqual(new Point(680, 360), weir.Geometry);
            Assert.AreEqual(23.5, weir.CrestWidth);
            Assert.IsTrue(weir.UseCrestLevelTimeSeries);
            Assert.AreEqual(2, weir.CrestLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(5.6, weir.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(11.12, weir.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 1, 2, 0)]);
            Assert.IsInstanceOf<SimpleWeirFormula>(weir.WeirFormula);
            var simpleWeirFormula = (SimpleWeirFormula)weir.WeirFormula;
            Assert.AreEqual(0.7, simpleWeirFormula.LateralContraction);
        }

        #endregion
        #region Gate

        [Test]
        public void CreateGateWithConstantPropertiesTest()
        {
            var schema = new StructureSchemaCsvFile().ReadStructureSchema(StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);
            var openingDirectionDefinition = schema.GetDefinition("gate", KnownGeneralStructureProperties.GateOpeningHorizontalDirection.GetDescription());
            
            var structure = new Structure2D("gate");
            structure.AddProperty(KnownStructureProperties.Type, typeof(string), "gate");
            structure.AddProperty(KnownStructureProperties.Name, typeof(string), "Gate01");
            structure.AddProperty(KnownStructureProperties.X, typeof(double), "500");
            structure.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            structure.AddProperty(KnownStructureProperties.CrestLevel, typeof(Steerable), "2");
            structure.AddProperty(KnownStructureProperties.CrestWidth,typeof(double),"55.7");
            structure.AddProperty(KnownStructureProperties.GateOpeningWidth, typeof(Steerable), "1");
            structure.AddProperty(KnownStructureProperties.GateLowerEdgeLevel, typeof(Steerable), "2.8");
            structure.AddProperty(KnownStructureProperties.GateHeight, typeof(double), "10");
            structure.AddProperty(KnownStructureProperties.GateOpeningHorizontalDirection, openingDirectionDefinition.DataType, "from_right");

            var dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            var gate = StructureFactory.CreateGate(structure, dummyPath, new DateTime());
            var gateWeirFormula = gate.WeirFormula as IGatedWeirFormula;

            Assert.NotNull(gateWeirFormula);

            Assert.AreEqual("Gate01", gate.Name);
            Assert.IsNull(gate.LongName);
            Assert.IsNull(gate.Branch);
            Assert.IsNaN(gate.Chainage);
            Assert.AreEqual(new Point(500, 360), gate.Geometry);
            Assert.IsFalse(gate.UseCrestLevelTimeSeries);
            Assert.AreEqual(2.0, gate.CrestLevel);
            Assert.AreEqual(55.7, gate.CrestWidth);
            Assert.IsFalse(gateWeirFormula.UseLowerEdgeLevelTimeSeries);
            Assert.AreEqual(2.8, gateWeirFormula.LowerEdgeLevel);
            Assert.AreEqual(0, gateWeirFormula.LowerEdgeLevelTimeSeries.Time.Values.Count);
            Assert.IsFalse(gateWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries);
            Assert.AreEqual(1.0, gateWeirFormula.HorizontalDoorOpeningWidth);
            Assert.AreEqual(0, gateWeirFormula.HorizontalDoorOpeningWidthTimeSeries.Time.Values.Count);
            Assert.AreEqual(GateOpeningDirection.FromRight, gateWeirFormula.HorizontalDoorOpeningDirection);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CreateGateWithTimeSeriesTest()
        {
            var schema =
                new StructureSchemaCsvFile().ReadStructureSchema(
                    StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);
            var openingDirectionDefinition = schema.GetDefinition("gate", KnownGeneralStructureProperties.GateOpeningHorizontalDirection.GetDescription());

            var structure = new Structure2D("gate");
            structure.AddProperty(KnownStructureProperties.Type, typeof(string), "gate");
            structure.AddProperty(KnownStructureProperties.Name, typeof(string), "Gate02");
            structure.AddProperty(KnownStructureProperties.PolylineFile, typeof(string), "pump05.pli");
            structure.AddProperty(KnownStructureProperties.CrestLevel, typeof(Steerable),
                                  $"Gate02_{KnownStructureProperties.GateLowerEdgeLevel}.tim");
            structure.AddProperty(KnownStructureProperties.GateLowerEdgeLevel, typeof(Steerable),
                                  $"Gate02_{KnownStructureProperties.GateLowerEdgeLevel}.tim");
            structure.AddProperty(KnownStructureProperties.GateOpeningWidth, typeof(Steerable),
                                  $"Gate02_{KnownStructureProperties.GateOpeningWidth}.tim");
            structure.AddProperty(KnownStructureProperties.GateHeight, typeof(double), "10");
            structure.AddProperty(KnownStructureProperties.GateOpeningHorizontalDirection,
                                  openingDirectionDefinition.DataType, "from_right");

            var dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            var gate = StructureFactory.CreateGate(structure, dummyPath, new DateTime(2013, 1, 1));
            var gateWeirFormula = gate.WeirFormula as IGatedWeirFormula;

            Assert.NotNull(gateWeirFormula);
            Assert.AreEqual("Gate02", gate.Name);
            Assert.IsNull(gate.LongName);
            Assert.IsNull(gate.Branch);
            Assert.IsNaN(gate.Chainage);
            Assert.AreEqual(new LineString(new[] {new Coordinate(1, 2), new Coordinate(3, 4), new Coordinate(6, 7)}),
                            gate.Geometry);
            Assert.IsTrue(gate.UseCrestLevelTimeSeries);
            Assert.AreEqual(2, gate.CrestLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, gate.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, gate.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);
            Assert.AreEqual(double.NaN, gate.CrestWidth);

            Assert.IsTrue(gateWeirFormula.UseLowerEdgeLevelTimeSeries);
            Assert.AreEqual(2, gateWeirFormula.LowerEdgeLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, gateWeirFormula.LowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, gateWeirFormula.LowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);

            Assert.IsTrue(gateWeirFormula.UseHorizontalDoorOpeningWidthTimeSeries);
            Assert.AreEqual(2, gateWeirFormula.HorizontalDoorOpeningWidthTimeSeries.Time.Values.Count);
            Assert.AreEqual(
                5.6, gateWeirFormula.HorizontalDoorOpeningWidthTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(
                7.8, gateWeirFormula.HorizontalDoorOpeningWidthTimeSeries[new DateTime(2013, 1, 1, 2, 3, 0)]);

            Assert.AreEqual(GateOpeningDirection.FromRight, gateWeirFormula.HorizontalDoorOpeningDirection);
        }

        #endregion Gate

        #region CreateStructureWeir
        /// <summary>
        /// GIVEN a simple weir Structure2D
        /// WHEN CreateStructure is called
        /// THEN the corresponding simple weir is returned
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void GivenASimpleWeirStructure2D_WhenCreateStructureIsCalled_ThenTheCorrespondingSimpleWeirIsReturned(bool isConstCrestLevel)
        {
            // Given
            var simpleWeirPrecursor = ComposeSimpleWeir(isConstCrestLevel);

            // When
            var result = StructureFactory.CreateStructure(simpleWeirPrecursor, structuresPath, refDate);

            // Then
            VerifySimpleWeir(result, isConstCrestLevel);
        }

        /// <summary>
        /// GIVEN a gated weir Structure2D
        /// WHEN CreateStructure is called
        /// THEN the corresponding gated weir is returned
        /// </summary>
        [TestCase(false, false, false)]
        [TestCase(true,  false, false)]
        [TestCase(false, true,  false)]
        [TestCase(true,  true,  false)]
        [TestCase(false, false, true)]
        [TestCase(true,  false, true)]
        [TestCase(false, true,  true)]
        [TestCase(true,  true,  true)]
        [Category(TestCategory.DataAccess)]
        public void GivenAGatedWeirStructure2D_WhenCreateStructureIsCalled_ThenTheCorrespondingGatedWeirIsReturned(bool isConstCrestLevel, 
                                                                                                                   bool isConstLowerEdgeLevel,
                                                                                                                   bool isConstHorizontalOpeningWidth)
        {
            // Given
            var gatedWeirPrecursor = ComposeGatedWeir(isConstCrestLevel,
                                                      isConstLowerEdgeLevel, 
                                                      isConstHorizontalOpeningWidth);

            // When
            var result = StructureFactory.CreateStructure(gatedWeirPrecursor, structuresPath, refDate);

            // Then
            VerifyGatedWeir(result, isConstCrestLevel, isConstLowerEdgeLevel, isConstHorizontalOpeningWidth);
        }

        /// <summary>
        /// GIVEN a general structure Structure2D
        /// WHEN CreateStructure is called
        /// THEN the corresponding general structure is returned
        /// </summary>
        [TestCase(false, false, false)]
        [TestCase(true,  false, false)]
        [TestCase(false, true,  false)]
        [TestCase(true,  true,  false)]
        [TestCase(false, false, true)]
        [TestCase(true,  false, true)]
        [TestCase(false, true,  true)]
        [TestCase(true,  true,  true)]
        [Category(TestCategory.DataAccess)]
        public void GivenAGeneralStructureStructure2D_WhenCreateStructureIsCalled_ThenTheCorrespondingGeneralStructureIsReturned(bool isConstCrestLevel,
                                                                                                                                 bool isConstLowerEdgeLevel,
                                                                                                                                 bool isConstHorizontalOpeningWidth)
        {
            // Given
            var generalStructurePrecursor = ComposeGeneralStructure(isConstCrestLevel,
                                                                    isConstLowerEdgeLevel,
                                                                    isConstHorizontalOpeningWidth);

            // When
            var result = StructureFactory.CreateStructure(generalStructurePrecursor, structuresPath, refDate);

            // Then
            VerifyGeneralStructure(result, isConstCrestLevel, isConstLowerEdgeLevel, isConstHorizontalOpeningWidth);
        }
        #endregion

        #region CreateStructure
        private Structure2D ComposeSimpleWeir(bool isConstCrestLevel)
        {
            const string t = StructureRegion.StructureTypeName.Weir;
            var result = ComposeCommon(t, isConstCrestLevel);

            // SimpleWeir specific
            const string contractionCoefficientProperty = KnownStructureProperties.LateralContractionCoefficient;
            var contractionCoefficientValue =
                constValLookUpTable[KnownStructureProperties.LateralContractionCoefficient].ToString();

            result.AddProperty(contractionCoefficientProperty, typeof(double), contractionCoefficientValue);

            return result;
        }

        private Structure2D ComposeGatedWeir(bool isConstCrestLevel,
                                             bool isConstLowerEdgeLevel,
                                             bool isConstHorizontalOpeningWidth)
        {
            const string t = StructureRegion.StructureTypeName.Gate;
            var result = ComposeCommon(t, isConstCrestLevel);
            AddGatedProperties(result, t, isConstLowerEdgeLevel, isConstHorizontalOpeningWidth);

            return result;
        }

        private Structure2D ComposeGeneralStructure(bool isConstCrestLevel,
                                                    bool isConstLowerEdgeLevel,
                                                    bool isConstHorizontalOpeningWidth)
        {
            const string t = StructureRegion.StructureTypeName.GeneralStructure;
            var result = ComposeCommon(t, isConstCrestLevel);
            AddGatedProperties(result, t, isConstLowerEdgeLevel, isConstHorizontalOpeningWidth);

            // GeneralStructure specific
            var generalStructureProperties = new List<string>()
            {
                KnownGeneralStructureProperties.Upstream2Width.GetDescription(),
                KnownGeneralStructureProperties.Upstream1Width.GetDescription(),
                KnownGeneralStructureProperties.Downstream1Width.GetDescription(),
                KnownGeneralStructureProperties.Downstream2Width.GetDescription(),

                KnownGeneralStructureProperties.Upstream2Level.GetDescription(),
                KnownGeneralStructureProperties.Upstream1Level.GetDescription(),
                KnownGeneralStructureProperties.Downstream1Level.GetDescription(),
                KnownGeneralStructureProperties.Downstream2Level.GetDescription(),

                KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient.GetDescription(),
                KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient.GetDescription(),
                KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient.GetDescription(),
                KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient.GetDescription(),

                KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient.GetDescription(),
                KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient.GetDescription(),
                KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient.GetDescription(),
                KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient.GetDescription(),

                KnownGeneralStructureProperties.ExtraResistance.GetDescription(),
            };

            foreach (var generalStructureProperty in generalStructureProperties)
            {
                result.AddProperty(generalStructureProperty, typeof(double), constValLookUpTable[generalStructureProperty].ToString());
            }

            return result;
        }

        private Structure2D ComposeCommon(string structureType, bool isConstCrestLevel)
        {
            var result = new Structure2D(structureType);

            result.AddProperty(KnownStructureProperties.Type, typeof(string), structureType);
            result.AddProperty(KnownStructureProperties.Name, typeof(string), "SomeName");

            // Position
            result.AddProperty(KnownStructureProperties.X, typeof(double), "500");
            result.AddProperty(KnownStructureProperties.Y, typeof(double), "360");

            // Crest level
            var crestLevelProperty =
                propertyNameMap[structureType][KnownStructureProperties.CrestLevel];
            var crestLevelValue =
                isConstCrestLevel ? constValLookUpTable[KnownStructureProperties.CrestLevel].ToString()
                    : timeSeriesLookUpTable[KnownStructureProperties.CrestLevel];

            result.AddProperty(crestLevelProperty, typeof(Steerable), crestLevelValue);

            // Crest width
            var crestWidthProperty =
                propertyNameMap[structureType][KnownStructureProperties.CrestWidth];
            var crestWidthValue = constValLookUpTable[KnownStructureProperties.CrestWidth].ToString();

            result.AddProperty(crestWidthProperty, typeof(double), crestWidthValue);

            return result;
        }

        private void AddGatedProperties(Structure2D result,
                                        string structureType,
                                        bool isConstLowerEdgeLevel,
                                        bool isConstHorizontalOpeningWidth)
        {
            // LowerEdgeLevel
            var lowerEdgeLevelProperty =
                propertyNameMap[structureType][KnownStructureProperties.GateLowerEdgeLevel];
            var lowerEdgeLevelValue =
                isConstLowerEdgeLevel
                    ? constValLookUpTable[KnownStructureProperties.GateLowerEdgeLevel].ToString()
                    : timeSeriesLookUpTable[KnownStructureProperties.GateLowerEdgeLevel];

            result.AddProperty(lowerEdgeLevelProperty, typeof(Steerable), lowerEdgeLevelValue);

            // Horizontal door opening width
            var horizontalDoorOpeningWidthProperty =
                propertyNameMap[structureType][KnownStructureProperties.GateOpeningWidth];
            var horizontalDoorOpeningWidthValue =
                isConstHorizontalOpeningWidth
                    ? constValLookUpTable[KnownStructureProperties.GateOpeningWidth].ToString()
                    : timeSeriesLookUpTable[KnownStructureProperties.GateOpeningWidth];

            result.AddProperty(horizontalDoorOpeningWidthProperty,
                               typeof(Steerable),
                               horizontalDoorOpeningWidthValue);

            // Opening direction
            var schema = new StructureSchemaCsvFile().ReadStructureSchema(StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);
            var openingDirectionDefinition = schema.GetDefinition(structureType, KnownGeneralStructureProperties.GateOpeningHorizontalDirection.GetDescription());
            result.AddProperty(KnownStructureProperties.GateOpeningHorizontalDirection, openingDirectionDefinition.DataType, "symmetric");

            // door height
            var gateDoorHeightProperty =
                propertyNameMap[structureType][KnownStructureProperties.GateHeight];
            var gateDoorHeightValue =
                constValLookUpTable[KnownStructureProperties.GateHeight].ToString();
            result.AddProperty(gateDoorHeightProperty, typeof(double), gateDoorHeightValue);
        }
        #endregion


        #region VerifyStructure
        private void VerifySimpleWeir(IStructure1D structure, bool isConstCrestLevel)
        {
            var weir = structure as Weir2D;

            VerifyCommon(weir, isConstCrestLevel);

            // Verify SimpleWeir
            Assert.That(weir.WeirFormula, Is.Not.Null, "Expected a weir formula:");
            var weirFormula = weir.WeirFormula as SimpleWeirFormula;
            Assert.That(weirFormula, Is.Not.Null, "Expected the weir formula to be a simple weir");
            Assert.That(weirFormula.LateralContraction, Is.EqualTo(constValLookUpTable[KnownStructureProperties.LateralContractionCoefficient]));
        }

        private void VerifyGatedWeir(IStructure1D structure,
                                     bool isConstCrestLevel,
                                     bool isConstLowerEdgeLevel,
                                     bool isConstHorizontalOpeningWidth)
        {
            var weir = structure as Weir2D;

            VerifyCommon(weir, isConstCrestLevel);
            VerifyGated(weir, isConstLowerEdgeLevel, isConstHorizontalOpeningWidth);

        }

        private void VerifyGeneralStructure(IStructure1D structure,
                                            bool isConstCrestLevel,
                                            bool isConstLowerEdgeLevel,
                                            bool isConstHorizontalOpeningWidth)
        {
            var weir = structure as Weir2D;

            VerifyCommon(weir, isConstCrestLevel);
            VerifyGated(weir, isConstLowerEdgeLevel, isConstHorizontalOpeningWidth);

            // Verify GeneralStructure
            var generalStructureFormula = weir.WeirFormula as GeneralStructureWeirFormula;
            Assert.That(generalStructureFormula, Is.Not.Null, "Expected the weir formula to be a general structure:");

            Assert.That(generalStructureFormula.WidthLeftSideOfStructure,  Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Upstream2Width.GetDescription()]    ));
            Assert.That(generalStructureFormula.WidthStructureLeftSide,    Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Upstream1Width.GetDescription()]  ));
            Assert.That(generalStructureFormula.WidthStructureRightSide,   Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Downstream1Width.GetDescription()] ));
            Assert.That(generalStructureFormula.WidthRightSideOfStructure, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Downstream2Width.GetDescription()]   ));

            Assert.That(generalStructureFormula.BedLevelLeftSideOfStructure,  Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Upstream2Level.GetDescription()]   ));
            Assert.That(generalStructureFormula.BedLevelLeftSideStructure,    Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Upstream1Level.GetDescription()]  ));
            Assert.That(generalStructureFormula.BedLevelRightSideStructure,   Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Downstream1Level.GetDescription()] ));
            Assert.That(generalStructureFormula.BedLevelRightSideOfStructure, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Downstream2Level.GetDescription()]  ));

            Assert.That(generalStructureFormula.PositiveFreeGateFlow,    Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient.GetDescription()]   ));
            Assert.That(generalStructureFormula.PositiveDrownedGateFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient.GetDescription()]  ));
            if (constValLookUpTable.ContainsKey(KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient.GetDescription()))
                Assert.That(generalStructureFormula.PositiveFreeWeirFlow,    Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient.GetDescription()]   ));
            Assert.That(generalStructureFormula.PositiveDrownedWeirFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient.GetDescription()]  ));

            Assert.That(generalStructureFormula.NegativeFreeGateFlow,    Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient.GetDescription()]   ));
            Assert.That(generalStructureFormula.NegativeDrownedGateFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient.GetDescription()]  ));
            Assert.That(generalStructureFormula.NegativeFreeWeirFlow,    Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient.GetDescription()]   ));
            Assert.That(generalStructureFormula.NegativeDrownedWeirFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient.GetDescription()]  ));

            Assert.That(generalStructureFormula.ExtraResistance, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.ExtraResistance.GetDescription()]));
        }

        private void VerifyCommon(IWeir weir, bool isConstCrestLevel)
        {
            Assert.That(weir, Is.Not.Null, "Expected the structure to not be null");

            // Name
            Assert.That(weir.Name, Is.EqualTo("SomeName"));

            // Crest level
            if (isConstCrestLevel)
            {
                Assert.That(weir.UseCrestLevelTimeSeries, Is.False, "Expected use crest level time series to be false.");
                Assert.That(weir.CrestLevel, Is.EqualTo(constValLookUpTable[KnownStructureProperties.CrestLevel]), "Expected a different crest level:");
            }
            else
            {
                VerifyTimeSeries(KnownStructureProperties.CrestLevel, weir.UseCrestLevelTimeSeries, weir.CrestLevelTimeSeries);
            }

            // Crest width
            Assert.That(weir.CrestWidth, Is.EqualTo(constValLookUpTable[KnownStructureProperties.CrestWidth]), "Expected a different crest width:");
        }

        private void VerifyGated(IWeir weir, bool isConstLowerEdgeLevel, bool isConstHorizontalOpeningWidth)
        {
            Assert.That(weir.WeirFormula, Is.Not.Null, "Expected a weir formula:");
            var weirFormula = weir.WeirFormula as IGatedWeirFormula;
            Assert.That(weirFormula, Is.Not.Null, "Expected the weir formula to be a gated weir");

            if (isConstLowerEdgeLevel)
            {
                Assert.That(weirFormula.UseLowerEdgeLevelTimeSeries, Is.False, "Expected lower edge level time-series to be false.");
                Assert.That(weirFormula.LowerEdgeLevel, Is.EqualTo(constValLookUpTable[KnownStructureProperties.GateLowerEdgeLevel]), "Expected lower edge level to be a different value:");
            }
            else
            {
                VerifyTimeSeries(KnownStructureProperties.GateLowerEdgeLevel,
                                 weirFormula.UseLowerEdgeLevelTimeSeries,
                                 weirFormula.LowerEdgeLevelTimeSeries);
            }

            if (isConstHorizontalOpeningWidth)
            {
                Assert.That(weirFormula.UseHorizontalDoorOpeningWidthTimeSeries, Is.False, "Expected horizontal door opening width time-series time-series to be false.");
                Assert.That(weirFormula.HorizontalDoorOpeningWidth, Is.EqualTo(constValLookUpTable[KnownStructureProperties.GateOpeningWidth]), "Expected horizontal door opening width to be a different value:");
            }
            else
            {
                VerifyTimeSeries(KnownStructureProperties.GateOpeningWidth,
                                 weirFormula.UseHorizontalDoorOpeningWidthTimeSeries,
                                 weirFormula.HorizontalDoorOpeningWidthTimeSeries);
            }

            Assert.That(weirFormula.DoorHeight, Is.EqualTo(constValLookUpTable[KnownStructureProperties.GateHeight]), "Expected door height to be a different value:");
            Assert.That(weirFormula.HorizontalDoorOpeningDirection, Is.EqualTo(GateOpeningDirection.Symmetric));
        }

        private void VerifyTimeSeries(string timeSeriesName, bool isActive, TimeSeries timeSeries)
        {
            Assert.That(isActive, Is.True, $"Expected use {timeSeriesName} to be true.");
            Assert.That(timeSeries, Is.Not.Null, $"Expected {timeSeriesName} to not be null:");

            Assert.That(timeSeries.Arguments.Count,  Is.EqualTo(1), $"Expected a single argument in the {timeSeriesName}");
            Assert.That(timeSeries.Components.Count, Is.EqualTo(1), $"Expected a single component in the {timeSeriesName}");

            var reader = new TimFile();
            var refTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries("argument", "component", "unit");

            reader.Read(TestHelper.GetTestFilePath($"structures/{timeSeriesLookUpTable[timeSeriesName]}"), refTimeSeries, refDate);

            var argumentValues = timeSeries.Arguments[0];

            Assert.That(argumentValues, Is.Not.Null, "Expected argument values not to be null:");
            Assert.That(argumentValues.Values.Count, Is.EqualTo(refTimeSeries.Arguments[0].Values.Count), "Expected the number of argument values to be different:");
            for (var i = 0; i < argumentValues.Values.Count; i++)
                Assert.That(argumentValues.Values[i], Is.EqualTo(refTimeSeries.Arguments[0].Values[i]), $"Expected argument {i} of the {timeSeriesName} to be different:");

            var componentValues = timeSeries.Components[0];

            Assert.That(componentValues, Is.Not.Null, "Expected argument values not to be null:");
            Assert.That(componentValues.Values.Count, Is.EqualTo(refTimeSeries.Components[0].Values.Count), "Expected the number of argument values to be different:");
            for (var i = 0; i < componentValues.Values.Count; i++)
                Assert.That(componentValues.Values[i], Is.EqualTo(refTimeSeries.Components[0].Values[i]), $"Expected components {i} of the {timeSeriesName} to be different:");
        }
        #endregion
    }
}