using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.KnownProperties;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

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

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [OneTimeSetUp]
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
                [KnownGeneralStructureProperties.Upstream2Width.GetDescription()] = 64.0,
                [KnownGeneralStructureProperties.Upstream1Width.GetDescription()] = 128.0,
                [KnownGeneralStructureProperties.Downstream1Width.GetDescription()] = 256.0,
                [KnownGeneralStructureProperties.Downstream2Width.GetDescription()] = 512.0,
                [KnownGeneralStructureProperties.Upstream2Level.GetDescription()] = 1024.0,
                [KnownGeneralStructureProperties.Upstream1Level.GetDescription()] = 2048.0,
                [KnownGeneralStructureProperties.Downstream1Level.GetDescription()] = 4096.0,
                [KnownGeneralStructureProperties.Downstream2Level.GetDescription()] = 8192.0,
                [KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient.GetDescription()] = 16384.0,
                [KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient.GetDescription()] = 32768.0,
                [KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient.GetDescription()] = 65536.0,
                [KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient.GetDescription()] = 131072.0,
                [KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient.GetDescription()] = 262144.0,
                [KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient.GetDescription()] = 524288.0,
                [KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient.GetDescription()] = 1048576.0,
                [KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient.GetDescription()] = 2097152.0,
                [KnownGeneralStructureProperties.ExtraResistance.GetDescription()] = 41927.0
            };

            timeSeriesLookUpTable = new Dictionary<string, string>()
            {
                [KnownStructureProperties.CrestLevel] = $"weir_{KnownStructureProperties.CrestLevel}.tim",
                [KnownStructureProperties.GateLowerEdgeLevel] = $"Gate02_{KnownStructureProperties.GateLowerEdgeLevel}.tim",
                [KnownStructureProperties.GateOpeningWidth] = $"Gate02_{KnownStructureProperties.GateOpeningWidth}.tim"
            };

            propertyNameMap = new Dictionary<string, Dictionary<string, string>>()
            {
                [StructureRegion.StructureTypeName.Weir] = new Dictionary<string, string>()
                {
                    [KnownStructureProperties.CrestLevel] = KnownStructureProperties.CrestLevel,
                    [KnownStructureProperties.CrestWidth] = KnownStructureProperties.CrestWidth
                },
                [StructureRegion.StructureTypeName.Gate] = new Dictionary<string, string>()
                {
                    [KnownStructureProperties.CrestLevel] = KnownStructureProperties.CrestLevel,
                    [KnownStructureProperties.CrestWidth] = KnownStructureProperties.CrestWidth,
                    [KnownStructureProperties.GateOpeningWidth] = KnownStructureProperties.GateOpeningWidth,
                    [KnownStructureProperties.GateLowerEdgeLevel] = KnownStructureProperties.GateLowerEdgeLevel,
                    [KnownStructureProperties.GateHeight] = KnownStructureProperties.GateHeight,
                    [KnownStructureProperties.GateOpeningHorizontalDirection] =
                        KnownStructureProperties.GateOpeningHorizontalDirection
                },
                [StructureRegion.StructureTypeName.GeneralStructure] = new Dictionary<string, string>()
                {
                    [KnownStructureProperties.CrestLevel] = KnownGeneralStructureProperties.CrestLevel.GetDescription(),
                    [KnownStructureProperties.CrestWidth] = KnownGeneralStructureProperties.CrestWidth.GetDescription(),
                    [KnownStructureProperties.GateOpeningWidth] = KnownGeneralStructureProperties.GateOpeningWidth.GetDescription(),
                    [KnownStructureProperties.GateLowerEdgeLevel] = KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription(),
                    [KnownStructureProperties.GateHeight] = KnownGeneralStructureProperties.GateHeight.GetDescription(),
                    [KnownStructureProperties.GateOpeningHorizontalDirection] = KnownGeneralStructureProperties.GateOpeningHorizontalDirection.GetDescription()
                }
            };

            refDate = DateTime.MinValue;
            structuresPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");
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
            var structureDataAccessObject = new StructureDAO("pump");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Type, typeof(string), "pump");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Name, typeof(string), "pump01");
            structureDataAccessObject.AddProperty(KnownStructureProperties.X, typeof(double), "680");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Capacity, typeof(Steerable), "2");

            string dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            IPump pump = StructureFactory.CreatePump(structureDataAccessObject, dummyPath, new DateTime());
            Assert.AreEqual("pump01", pump.Name);
            Assert.AreEqual(new Point(680, 360), pump.Geometry);
            Assert.IsFalse(pump.UseCapacityTimeSeries);
            Assert.AreEqual(2.0, pump.Capacity);
            Assert.AreEqual(0, pump.CapacityTimeSeries.Time.Values.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CreatePumpWithCapacityTimeSeriesTest()
        {
            // Setup
            var structureDataAccessObject = new StructureDAO("pump");

            structureDataAccessObject.AddProperty(KnownStructureProperties.Type, typeof(string), "pump");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Name, typeof(string), "pump05");
            structureDataAccessObject.AddProperty(KnownStructureProperties.PolylineFile, typeof(string), "pump05.pli");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Capacity, typeof(Steerable), "pump05_capacity.tim");

            string dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            // Call
            IPump pump = StructureFactory.CreatePump(structureDataAccessObject, dummyPath, new DateTime(2013, 1, 1));

            // Assert
            Assert.AreEqual("pump05", pump.Name);
            Assert.AreEqual(new LineString(new[]
            {
                new Coordinate(1, 2),
                new Coordinate(3, 4),
                new Coordinate(6, 7)
            }), pump.Geometry);

            Assert.IsTrue(pump.UseCapacityTimeSeries);
            Assert.AreEqual(1.0, pump.Capacity);
            Assert.AreEqual(2, pump.CapacityTimeSeries.Time.Values.Count);
            Assert.AreEqual(5.6, pump.CapacityTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(11.12, pump.CapacityTimeSeries[new DateTime(2013, 1, 1, 1, 2, 0)]);
        }

        [Test]
        public void CreatePumpFromIncorrectType()
        {
            var structure = new StructureDAO("test");
            Assert.Throws<FormatException>(() => StructureFactory.CreatePump(structure, null, new DateTime()));
        }

        #endregion

        #region General Structure

        [Test]
        public void GivenGeneralStructureAsStructure2D_WhenCreatingStructure_ThenTypeIsWeir()
        {
            var generalStructureDataAccessObject = new StructureDAO(StructureRegion.StructureTypeName.GeneralStructure);
            IStructureObject result = StructureFactory.CreateStructure(generalStructureDataAccessObject, null, new DateTime());
            Assert.IsTrue(result is IStructure);
        }

        [Test]
        public void GivenGeneralStructureAsStructure2D_WhenCreatingStructure_ThenWeirFormulaIsAGeneralStructureWeirFormula()
        {
            var generalStructureDataAccessObject = new StructureDAO(StructureRegion.StructureTypeName.GeneralStructure);
            IStructureObject result = StructureFactory.CreateStructure(generalStructureDataAccessObject, null, new DateTime());
            Assert.IsTrue(result is IStructure);

            var weir = (IStructure) result;
            Assert.That(weir.Formula is GeneralStructureFormula);
        }

        public static IEnumerable<TestCaseData> GetKnownGeneralStructureProperties() =>
            Enum.GetValues(typeof(KnownGeneralStructureProperties))
                .Cast<KnownGeneralStructureProperties>()
                .Where(x => x != KnownGeneralStructureProperties.GateOpeningHorizontalDirection)
                .Select(x => new TestCaseData(x));

        [Test]
        [TestCaseSource(nameof(GetKnownGeneralStructureProperties))]
        public void GivenGeneralStructureAsStructure2DWithKnownPropertyUnequalToZero_WhenCreatingStructure_ThenWeirAdaptsProperty(KnownGeneralStructureProperties property)
        {
            // Setup
            var generalStructureDataAccessObject = new StructureDAO(StructureRegion.StructureTypeName.GeneralStructure);

            if (property == KnownGeneralStructureProperties.GateOpeningWidth ||
                property == KnownGeneralStructureProperties.GateLowerEdgeLevel ||
                property == KnownGeneralStructureProperties.CrestLevel)
            {
                generalStructureDataAccessObject.AddProperty(property.GetDescription(), typeof(Steerable), "12.34");
            }
            else
            {
                generalStructureDataAccessObject.AddProperty(property.GetDescription(), typeof(double), "12.34");
            }

            // Call
            IStructureObject resultingStructure = 
                StructureFactory.CreateStructure(generalStructureDataAccessObject, null, new DateTime());

            // Assert
            var weir = resultingStructure as IStructure;
            Assert.That(weir, Is.Not.Null);

            Dictionary<KnownGeneralStructureProperties, object> weirFormulaValueDictionary = ConstructWeirFormulaValueDictionary(weir);
            Assert.That(weirFormulaValueDictionary[property], Is.EqualTo(12.34), property.GetDescription());
        }

        [Test]
        public void GivenGeneralStructureAsStructure2DWithExtraResistanceEqualToZero_WhenCreatingStructure_ThenUseExtraResistanceIsFalse()
        {
            var generalStructureDataAccessObject = new StructureDAO(StructureRegion.StructureTypeName.GeneralStructure);
            generalStructureDataAccessObject.AddProperty(KnownGeneralStructureProperties.ExtraResistance.GetDescription(), typeof(double), "0.0");

            IStructureObject resultingStructure = StructureFactory.CreateStructure(generalStructureDataAccessObject, null, new DateTime());
            var weir = resultingStructure as IStructure;
            Assert.NotNull(weir);

            var weirFormula = weir.Formula as GeneralStructureFormula;
            Assert.NotNull(weirFormula);
            Assert.IsFalse(weirFormula.UseExtraResistance);
        }

        private static Dictionary<KnownGeneralStructureProperties, object> ConstructWeirFormulaValueDictionary(IStructure weir)
        {
            var weirFormula = weir.Formula as GeneralStructureFormula;
            Assert.NotNull(weirFormula);

            var dictionary = new Dictionary<KnownGeneralStructureProperties, object>
            {
                {KnownGeneralStructureProperties.Upstream1Width, weirFormula.Upstream1Width},
                {KnownGeneralStructureProperties.Upstream2Width, weirFormula.Upstream2Width},
                {KnownGeneralStructureProperties.CrestWidth, weirFormula.CrestWidth},
                {KnownGeneralStructureProperties.Downstream1Width, weirFormula.Downstream1Width},
                {KnownGeneralStructureProperties.Downstream2Width, weirFormula.Downstream2Width},
                {KnownGeneralStructureProperties.Upstream1Level, weirFormula.Upstream1Level},
                {KnownGeneralStructureProperties.Upstream2Level, weirFormula.Upstream2Level},
                {KnownGeneralStructureProperties.CrestLevel, weirFormula.CrestLevel},
                {KnownGeneralStructureProperties.Downstream1Level, weirFormula.Downstream1Level},
                {KnownGeneralStructureProperties.Downstream2Level, weirFormula.Downstream2Level},
                {KnownGeneralStructureProperties.GateHeight, weirFormula.GateHeight},
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
                {KnownGeneralStructureProperties.GateOpeningWidth, weirFormula.HorizontalGateOpeningWidth},
                {KnownGeneralStructureProperties.GateLowerEdgeLevel, weirFormula.GateLowerEdgeLevel}
            };
            return dictionary;
        }

        #endregion

        #region Weir

        [Test]
        public void CreateWeirFromIncorrectType()
        {
            var structureDataAccessObject = new StructureDAO("test");
            Assert.Throws<FormatException>(() => StructureFactory.CreateWeir(structureDataAccessObject, null, new DateTime()));
        }

        [Test]
        public void CreateWeirWithConstantCrestLevel()
        {
            var structureDataAccessObject = new StructureDAO("weir");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Type, typeof(string), "weir");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Name, typeof(string), "Weir_down");
            structureDataAccessObject.AddProperty(KnownStructureProperties.X, typeof(double), "680");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            structureDataAccessObject.AddProperty(KnownStructureProperties.CrestLevel, typeof(Steerable), "2");
            structureDataAccessObject.AddProperty(KnownStructureProperties.LateralContractionCoefficient, typeof(double), "0.7");

            string dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            IStructure weir = StructureFactory.CreateWeir(structureDataAccessObject, dummyPath, new DateTime());
            Assert.AreEqual("Weir_down", weir.Name);
            Assert.AreEqual(new Point(680, 360), weir.Geometry);
            Assert.IsFalse(weir.UseCrestLevelTimeSeries);
            Assert.AreEqual(2.0, weir.CrestLevel);
            Assert.AreEqual(double.NaN, weir.CrestWidth);
            Assert.AreEqual(0, weir.CrestLevelTimeSeries.Time.Values.Count);
            Assert.IsInstanceOf<SimpleWeirFormula>(weir.Formula);
            var simpleWeirFormula = (SimpleWeirFormula) weir.Formula;
            Assert.AreEqual(0.7, simpleWeirFormula.LateralContraction);
        }

        [Test]
        public void CreateWeirWithCrestLevelTimeSeries()
        {
            var structureDataAccessObject = new StructureDAO("weir");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Type, typeof(string), "weir");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Name, typeof(string), "Weir_moving");
            structureDataAccessObject.AddProperty(KnownStructureProperties.X, typeof(double), "680");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            structureDataAccessObject.AddProperty(KnownStructureProperties.CrestLevel, typeof(Steerable), "weir_CrestLevel.tim");
            structureDataAccessObject.AddProperty(KnownStructureProperties.CrestWidth, typeof(double), "23.5");
            structureDataAccessObject.AddProperty(KnownStructureProperties.LateralContractionCoefficient, typeof(double), "0.7");

            string dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            IStructure weir = StructureFactory.CreateWeir(structureDataAccessObject, dummyPath, new DateTime(2013, 1, 1));
            Assert.AreEqual("Weir_moving", weir.Name);
            Assert.AreEqual(new Point(680, 360), weir.Geometry);
            Assert.AreEqual(23.5, weir.CrestWidth);
            Assert.IsTrue(weir.UseCrestLevelTimeSeries);
            Assert.AreEqual(2, weir.CrestLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(5.6, weir.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(11.12, weir.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 1, 2, 0)]);
            Assert.IsInstanceOf<SimpleWeirFormula>(weir.Formula);
            var simpleWeirFormula = (SimpleWeirFormula) weir.Formula;
            Assert.AreEqual(0.7, simpleWeirFormula.LateralContraction);
        }

        #endregion

        #region Gate

        [Test]
        public void CreateGateWithConstantPropertiesTest()
        {
            StructureSchema<ModelPropertyDefinition> schema = new StructureSchemaCsvFile().ReadStructureSchema(StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);
            ModelPropertyDefinition openingDirectionDefinition = schema.GetDefinition("gate", KnownGeneralStructureProperties.GateOpeningHorizontalDirection.GetDescription());

            var structureDataAccessObject = new StructureDAO("gate");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Type, typeof(string), "gate");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Name, typeof(string), "Gate01");
            structureDataAccessObject.AddProperty(KnownStructureProperties.X, typeof(double), "500");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            structureDataAccessObject.AddProperty(KnownStructureProperties.CrestLevel, typeof(Steerable), "2");
            structureDataAccessObject.AddProperty(KnownStructureProperties.CrestWidth, typeof(double), "55.7");
            structureDataAccessObject.AddProperty(KnownStructureProperties.GateOpeningWidth, typeof(Steerable), "1");
            structureDataAccessObject.AddProperty(KnownStructureProperties.GateLowerEdgeLevel, typeof(Steerable), "2.8");
            structureDataAccessObject.AddProperty(KnownStructureProperties.GateHeight, typeof(double), "10");
            structureDataAccessObject.AddProperty(KnownStructureProperties.GateOpeningHorizontalDirection, openingDirectionDefinition.DataType, "from_right");

            string dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            IStructure gate = StructureFactory.CreateGate(structureDataAccessObject, dummyPath, new DateTime());
            var gateWeirFormula = gate.Formula as IGatedStructureFormula;

            Assert.NotNull(gateWeirFormula);

            Assert.AreEqual("Gate01", gate.Name);
            Assert.AreEqual(new Point(500, 360), gate.Geometry);
            Assert.IsFalse(gate.UseCrestLevelTimeSeries);
            Assert.AreEqual(2.0, gate.CrestLevel);
            Assert.AreEqual(55.7, gate.CrestWidth);
            Assert.IsFalse(gateWeirFormula.UseGateLowerEdgeLevelTimeSeries);
            Assert.AreEqual(2.8, gateWeirFormula.GateLowerEdgeLevel);
            Assert.AreEqual(0, gateWeirFormula.GateLowerEdgeLevelTimeSeries.Time.Values.Count);
            Assert.IsFalse(gateWeirFormula.UseHorizontalGateOpeningWidthTimeSeries);
            Assert.AreEqual(1.0, gateWeirFormula.HorizontalGateOpeningWidth);
            Assert.AreEqual(0, gateWeirFormula.HorizontalGateOpeningWidthTimeSeries.Time.Values.Count);
            Assert.AreEqual(GateOpeningDirection.FromRight, gateWeirFormula.GateOpeningHorizontalDirection);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CreateGateWithTimeSeriesTest()
        {
            StructureSchema<ModelPropertyDefinition> schema =
                new StructureSchemaCsvFile().ReadStructureSchema(
                    StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);
            ModelPropertyDefinition openingDirectionDefinition = schema.GetDefinition("gate", KnownGeneralStructureProperties.GateOpeningHorizontalDirection.GetDescription());

            var structureDataAccessObject = new StructureDAO("gate");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Type, typeof(string), "gate");
            structureDataAccessObject.AddProperty(KnownStructureProperties.Name, typeof(string), "Gate02");
            structureDataAccessObject.AddProperty(KnownStructureProperties.PolylineFile, typeof(string), "pump05.pli");
            structureDataAccessObject.AddProperty(KnownStructureProperties.CrestLevel, typeof(Steerable),
                                  $"Gate02_{KnownStructureProperties.GateLowerEdgeLevel}.tim");
            structureDataAccessObject.AddProperty(KnownStructureProperties.GateLowerEdgeLevel, typeof(Steerable),
                                  $"Gate02_{KnownStructureProperties.GateLowerEdgeLevel}.tim");
            structureDataAccessObject.AddProperty(KnownStructureProperties.GateOpeningWidth, typeof(Steerable),
                                  $"Gate02_{KnownStructureProperties.GateOpeningWidth}.tim");
            structureDataAccessObject.AddProperty(KnownStructureProperties.GateHeight, typeof(double), "10");
            structureDataAccessObject.AddProperty(KnownStructureProperties.GateOpeningHorizontalDirection,
                                  openingDirectionDefinition.DataType, "from_right");

            string dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            IStructure gate = StructureFactory.CreateGate(structureDataAccessObject, dummyPath, new DateTime(2013, 1, 1));
            var gateWeirFormula = gate.Formula as IGatedStructureFormula;

            Assert.NotNull(gateWeirFormula);
            Assert.AreEqual("Gate02", gate.Name);
            Assert.AreEqual(new LineString(new[]
                            {
                                new Coordinate(1, 2),
                                new Coordinate(3, 4),
                                new Coordinate(6, 7)
                            }),
                            gate.Geometry);
            Assert.IsTrue(gate.UseCrestLevelTimeSeries);
            Assert.AreEqual(2, gate.CrestLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, gate.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, gate.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);
            Assert.AreEqual(double.NaN, gate.CrestWidth);

            Assert.IsTrue(gateWeirFormula.UseGateLowerEdgeLevelTimeSeries);
            Assert.AreEqual(2, gateWeirFormula.GateLowerEdgeLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, gateWeirFormula.GateLowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, gateWeirFormula.GateLowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);

            Assert.IsTrue(gateWeirFormula.UseHorizontalGateOpeningWidthTimeSeries);
            Assert.AreEqual(2, gateWeirFormula.HorizontalGateOpeningWidthTimeSeries.Time.Values.Count);
            Assert.AreEqual(
                5.6, gateWeirFormula.HorizontalGateOpeningWidthTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(
                7.8, gateWeirFormula.HorizontalGateOpeningWidthTimeSeries[new DateTime(2013, 1, 1, 2, 3, 0)]);

            Assert.AreEqual(GateOpeningDirection.FromRight, gateWeirFormula.GateOpeningHorizontalDirection);
        }

        #endregion Gate

        #region CreateStructureWeir

        /// <summary>
        /// GIVEN a simple weir StructureDAO
        /// WHEN CreateStructure is called
        /// THEN the corresponding simple weir is returned
        /// </summary>
        [TestCase(true)]
        [TestCase(false)]
        [Category(TestCategory.DataAccess)]
        public void GivenASimpleWeirStructureDAO_WhenCreateStructureIsCalled_ThenTheCorrespondingSimpleWeirIsReturned(bool isConstCrestLevel)
        {
            // Given
            StructureDAO simpleWeirPrecursor = ComposeSimpleWeir(isConstCrestLevel);

            // When
            IStructureObject result = StructureFactory.CreateStructure(simpleWeirPrecursor, structuresPath, refDate);

            // Then
            VerifySimpleWeir(result, isConstCrestLevel);
        }

        /// <summary>
        /// GIVEN a gated weir StructureDAO
        /// WHEN CreateStructure is called
        /// THEN the corresponding gated weir is returned
        /// </summary>
        [TestCase(false, false, false)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, true, false)]
        [TestCase(false, false, true)]
        [TestCase(true, false, true)]
        [TestCase(false, true, true)]
        [TestCase(true, true, true)]
        [Category(TestCategory.DataAccess)]
        public void GivenAGatedWeirStructureDAO_WhenCreateStructureIsCalled_ThenTheCorrespondingGatedWeirIsReturned(bool isConstCrestLevel,
                                                                                                                   bool isConstLowerEdgeLevel,
                                                                                                                   bool isConstHorizontalOpeningWidth)
        {
            // Given
            StructureDAO gatedWeirPrecursor = ComposeGatedWeir(isConstCrestLevel,
                                                              isConstLowerEdgeLevel,
                                                              isConstHorizontalOpeningWidth);

            // When
            IStructureObject result = StructureFactory.CreateStructure(gatedWeirPrecursor, structuresPath, refDate);

            // Then
            VerifyGatedWeir(result, isConstCrestLevel, isConstLowerEdgeLevel, isConstHorizontalOpeningWidth);
        }

        [TestCase(KnownStructureProperties.Name)]
        [TestCase(KnownStructureProperties.Type)]
        [TestCase(KnownStructureProperties.CrestLevel)]
        [TestCase(KnownStructureProperties.CrestWidth)]
        [TestCase(KnownStructureProperties.GateHeight)]
        [TestCase(KnownStructureProperties.GateLowerEdgeLevel)]
        [TestCase(KnownStructureProperties.GateOpeningWidth)]
        [TestCase(KnownStructureProperties.GateOpeningHorizontalDirection)]
        [TestCase(KnownStructureProperties.LateralContractionCoefficient)]
        [Category(TestCategory.DataAccess)]
        public void CreateStructure_WhenAGatePropertyIsMissing_ThenNoExceptionIsThrown(string propertyName)
        {
            // Set-up
            StructureDAO gatedWeirPrecursor = ComposeGatedWeir(true, true, true);
            ModelProperty property = gatedWeirPrecursor.GetProperty(propertyName);
            gatedWeirPrecursor.Properties.Remove(property);

            // Action
            void TestAction()
            {
                StructureFactory.CreateStructure(gatedWeirPrecursor, structuresPath, refDate);
            }

            // Assert
            Assert.DoesNotThrow(TestAction,
                                $"When property {propertyName} is missing, no exception should be thrown.");
        }

        /// <summary>
        /// GIVEN a general structure StructureDAO
        /// WHEN CreateStructure is called
        /// THEN the corresponding general structure is returned
        /// </summary>
        [TestCase(false, false, false)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(true, true, false)]
        [TestCase(false, false, true)]
        [TestCase(true, false, true)]
        [TestCase(false, true, true)]
        [TestCase(true, true, true)]
        [Category(TestCategory.DataAccess)]
        public void GivenAGeneralStructureStructure2D_WhenCreateStructureIsCalled_ThenTheCorrespondingGeneralStructureIsReturned(bool isConstCrestLevel,
                                                                                                                                 bool isConstLowerEdgeLevel,
                                                                                                                                 bool isConstHorizontalOpeningWidth)
        {
            // Given
            StructureDAO generalStructurePrecursor = ComposeGeneralStructure(isConstCrestLevel,
                                                                            isConstLowerEdgeLevel,
                                                                            isConstHorizontalOpeningWidth);

            // When
            IStructureObject result = StructureFactory.CreateStructure(generalStructurePrecursor, structuresPath, refDate);

            // Then
            VerifyGeneralStructure(result, isConstCrestLevel, isConstLowerEdgeLevel, isConstHorizontalOpeningWidth);
        }

        #endregion

        #region CreateStructure

        private StructureDAO ComposeSimpleWeir(bool isConstCrestLevel)
        {
            const string t = StructureRegion.StructureTypeName.Weir;
            StructureDAO result = ComposeCommon(t, isConstCrestLevel);

            // SimpleWeir specific
            const string contractionCoefficientProperty = KnownStructureProperties.LateralContractionCoefficient;
            var contractionCoefficientValue =
                constValLookUpTable[KnownStructureProperties.LateralContractionCoefficient].ToString();

            result.AddProperty(contractionCoefficientProperty, typeof(double), contractionCoefficientValue);

            return result;
        }

        private StructureDAO ComposeGatedWeir(bool isConstCrestLevel,
                                             bool isConstLowerEdgeLevel,
                                             bool isConstHorizontalOpeningWidth)
        {
            const string t = StructureRegion.StructureTypeName.Gate;
            StructureDAO result = ComposeCommon(t, isConstCrestLevel);
            AddGatedProperties(result, t, isConstLowerEdgeLevel, isConstHorizontalOpeningWidth);

            return result;
        }

        private StructureDAO ComposeGeneralStructure(bool isConstCrestLevel,
                                                    bool isConstLowerEdgeLevel,
                                                    bool isConstHorizontalOpeningWidth)
        {
            const string t = StructureRegion.StructureTypeName.GeneralStructure;
            StructureDAO result = ComposeCommon(t, isConstCrestLevel);
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
                KnownGeneralStructureProperties.ExtraResistance.GetDescription()
            };

            foreach (string generalStructureProperty in generalStructureProperties)
            {
                result.AddProperty(generalStructureProperty, typeof(double), constValLookUpTable[generalStructureProperty].ToString());
            }

            return result;
        }

        private StructureDAO ComposeCommon(string structureType, bool isConstCrestLevel)
        {
            var result = new StructureDAO(structureType);

            result.AddProperty(KnownStructureProperties.Type, typeof(string), structureType);
            result.AddProperty(KnownStructureProperties.Name, typeof(string), "SomeName");

            // Position
            result.AddProperty(KnownStructureProperties.X, typeof(double), "500");
            result.AddProperty(KnownStructureProperties.Y, typeof(double), "360");

            // Crest level
            string crestLevelProperty =
                propertyNameMap[structureType][KnownStructureProperties.CrestLevel];
            string crestLevelValue =
                isConstCrestLevel
                    ? constValLookUpTable[KnownStructureProperties.CrestLevel].ToString()
                    : timeSeriesLookUpTable[KnownStructureProperties.CrestLevel];

            result.AddProperty(crestLevelProperty, typeof(Steerable), crestLevelValue);

            // Crest width
            string crestWidthProperty =
                propertyNameMap[structureType][KnownStructureProperties.CrestWidth];
            var crestWidthValue = constValLookUpTable[KnownStructureProperties.CrestWidth].ToString();

            result.AddProperty(crestWidthProperty, typeof(double), crestWidthValue);

            return result;
        }

        private void AddGatedProperties(StructureDAO result,
                                        string structureType,
                                        bool isConstLowerEdgeLevel,
                                        bool isConstHorizontalOpeningWidth)
        {
            // GateLowerEdgeLevel
            string gateLowerEdgeLevelProperty =
                propertyNameMap[structureType][KnownStructureProperties.GateLowerEdgeLevel];
            string lowerEdgeLevelValue =
                isConstLowerEdgeLevel
                    ? constValLookUpTable[KnownStructureProperties.GateLowerEdgeLevel].ToString()
                    : timeSeriesLookUpTable[KnownStructureProperties.GateLowerEdgeLevel];

            result.AddProperty(gateLowerEdgeLevelProperty, typeof(Steerable), lowerEdgeLevelValue);

            // Horizontal gate opening width
            string horizontalGateOpeningWidthProperty =
                propertyNameMap[structureType][KnownStructureProperties.GateOpeningWidth];
            string horizontalGateOpeningWidthValue =
                isConstHorizontalOpeningWidth
                    ? constValLookUpTable[KnownStructureProperties.GateOpeningWidth].ToString()
                    : timeSeriesLookUpTable[KnownStructureProperties.GateOpeningWidth];

            result.AddProperty(horizontalGateOpeningWidthProperty,
                               typeof(Steerable),
                               horizontalGateOpeningWidthValue);

            // Opening direction
            StructureSchema<ModelPropertyDefinition> schema = new StructureSchemaCsvFile().ReadStructureSchema(StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);
            ModelPropertyDefinition openingDirectionDefinition = schema.GetDefinition(structureType, KnownGeneralStructureProperties.GateOpeningHorizontalDirection.GetDescription());
            result.AddProperty(KnownStructureProperties.GateOpeningHorizontalDirection, openingDirectionDefinition.DataType, "symmetric");

            // gate height
            string gateHeight =
                propertyNameMap[structureType][KnownStructureProperties.GateHeight];
            var gateHeightValue =
                constValLookUpTable[KnownStructureProperties.GateHeight].ToString();
            result.AddProperty(gateHeight, typeof(double), gateHeightValue);
        }

        #endregion

        #region VerifyStructure

        private void VerifySimpleWeir(IStructureObject structure, bool isConstCrestLevel)
        {
            var weir = structure as IStructure;

            VerifyCommon(weir, isConstCrestLevel);

            // Verify SimpleWeir
            Assert.That(weir.Formula, Is.Not.Null, "Expected a weir formula:");
            var weirFormula = weir.Formula as SimpleWeirFormula;
            Assert.That(weirFormula, Is.Not.Null, "Expected the weir formula to be a simple weir");
            Assert.That(weirFormula.LateralContraction, Is.EqualTo(constValLookUpTable[KnownStructureProperties.LateralContractionCoefficient]));
        }

        private void VerifyGatedWeir(IStructureObject structure,
                                     bool isConstCrestLevel,
                                     bool isConstLowerEdgeLevel,
                                     bool isConstHorizontalOpeningWidth)
        {
            var weir = structure as IStructure;

            VerifyCommon(weir, isConstCrestLevel);
            VerifyGated(weir, isConstLowerEdgeLevel, isConstHorizontalOpeningWidth);
        }

        private void VerifyGeneralStructure(IStructureObject structure,
                                            bool isConstCrestLevel,
                                            bool isConstLowerEdgeLevel,
                                            bool isConstHorizontalOpeningWidth)
        {
            var weir = structure as IStructure;

            VerifyCommon(weir, isConstCrestLevel);
            VerifyGated(weir, isConstLowerEdgeLevel, isConstHorizontalOpeningWidth);

            // Verify GeneralStructure
            var generalStructureFormula = weir.Formula as GeneralStructureFormula;
            Assert.That(generalStructureFormula, Is.Not.Null, "Expected the weir formula to be a general structure:");

            Assert.That(generalStructureFormula.Upstream1Width, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Upstream1Width.GetDescription()]));
            Assert.That(generalStructureFormula.Upstream2Width, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Upstream2Width.GetDescription()]));
            Assert.That(generalStructureFormula.Downstream1Width, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Downstream1Width.GetDescription()]));
            Assert.That(generalStructureFormula.Downstream2Width, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Downstream2Width.GetDescription()]));

            Assert.That(generalStructureFormula.Upstream1Level, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Upstream1Level.GetDescription()]));
            Assert.That(generalStructureFormula.Upstream2Level, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Upstream2Level.GetDescription()]));
            Assert.That(generalStructureFormula.Downstream1Level, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Downstream1Level.GetDescription()]));
            Assert.That(generalStructureFormula.Downstream2Level, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.Downstream2Level.GetDescription()]));

            Assert.That(generalStructureFormula.PositiveFreeGateFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.PositiveFreeGateFlowCoefficient.GetDescription()]));
            Assert.That(generalStructureFormula.PositiveDrownedGateFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.PositiveDrownGateFlowCoefficient.GetDescription()]));
            if (constValLookUpTable.ContainsKey(KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient.GetDescription()))
            {
                Assert.That(generalStructureFormula.PositiveFreeWeirFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.PositiveFreeWeirFlowCoefficient.GetDescription()]));
            }

            Assert.That(generalStructureFormula.PositiveDrownedWeirFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.PositiveDrownWeirFlowCoefficient.GetDescription()]));

            Assert.That(generalStructureFormula.NegativeFreeGateFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.NegativeFreeGateFlowCoefficient.GetDescription()]));
            Assert.That(generalStructureFormula.NegativeDrownedGateFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.NegativeDrownGateFlowCoefficient.GetDescription()]));
            Assert.That(generalStructureFormula.NegativeFreeWeirFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.NegativeFreeWeirFlowCoefficient.GetDescription()]));
            Assert.That(generalStructureFormula.NegativeDrownedWeirFlow, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.NegativeDrownWeirFlowCoefficient.GetDescription()]));

            Assert.That(generalStructureFormula.ExtraResistance, Is.EqualTo(constValLookUpTable[KnownGeneralStructureProperties.ExtraResistance.GetDescription()]));
        }

        private void VerifyCommon(IStructure weir, bool isConstCrestLevel)
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

        private void VerifyGated(IStructure weir, bool isConstLowerEdgeLevel, bool isConstHorizontalOpeningWidth)
        {
            Assert.That(weir.Formula, Is.Not.Null, "Expected a weir formula:");
            var weirFormula = weir.Formula as IGatedStructureFormula;
            Assert.That(weirFormula, Is.Not.Null, "Expected the weir formula to be a gated weir");

            if (isConstLowerEdgeLevel)
            {
                Assert.That(weirFormula.UseGateLowerEdgeLevelTimeSeries, Is.False, "Expected gate lower edge level time-series to be false.");
                Assert.That(weirFormula.GateLowerEdgeLevel, Is.EqualTo(constValLookUpTable[KnownStructureProperties.GateLowerEdgeLevel]), "Expected lower edge level to be a different value:");
            }
            else
            {
                VerifyTimeSeries(KnownStructureProperties.GateLowerEdgeLevel,
                                 weirFormula.UseGateLowerEdgeLevelTimeSeries,
                                 weirFormula.GateLowerEdgeLevelTimeSeries);
            }

            if (isConstHorizontalOpeningWidth)
            {
                Assert.That(weirFormula.UseHorizontalGateOpeningWidthTimeSeries, Is.False, "Expected horizontal gate opening width time-series time-series to be false.");
                Assert.That(weirFormula.HorizontalGateOpeningWidth, Is.EqualTo(constValLookUpTable[KnownStructureProperties.GateOpeningWidth]), "Expected horizontal gate opening width to be a different value:");
            }
            else
            {
                VerifyTimeSeries(KnownStructureProperties.GateOpeningWidth,
                                 weirFormula.UseHorizontalGateOpeningWidthTimeSeries,
                                 weirFormula.HorizontalGateOpeningWidthTimeSeries);
            }

            Assert.That(weirFormula.GateHeight, Is.EqualTo(constValLookUpTable[KnownStructureProperties.GateHeight]), "Expected gate height to be a different value:");
            Assert.That(weirFormula.GateOpeningHorizontalDirection, Is.EqualTo(GateOpeningDirection.Symmetric));
        }

        private void VerifyTimeSeries(string timeSeriesName, bool isActive, TimeSeries timeSeries)
        {
            Assert.That(isActive, Is.True, $"Expected use {timeSeriesName} to be true.");
            Assert.That(timeSeries, Is.Not.Null, $"Expected {timeSeriesName} to not be null:");

            Assert.That(timeSeries.Arguments.Count, Is.EqualTo(1), $"Expected a single argument in the {timeSeriesName}");
            Assert.That(timeSeries.Components.Count, Is.EqualTo(1), $"Expected a single component in the {timeSeriesName}");

            var reader = new TimFile();
            TimeSeries refTimeSeries = HydroTimeSeriesFactory.CreateTimeSeries("argument", "component", "unit");

            reader.Read(TestHelper.GetTestFilePath($"structures/{timeSeriesLookUpTable[timeSeriesName]}"), refTimeSeries, refDate);

            IVariable argumentValues = timeSeries.Arguments[0];

            Assert.That(argumentValues, Is.Not.Null, "Expected argument values not to be null:");
            Assert.That(argumentValues.Values.Count, Is.EqualTo(refTimeSeries.Arguments[0].Values.Count), "Expected the number of argument values to be different:");
            for (var i = 0; i < argumentValues.Values.Count; i++)
            {
                Assert.That(argumentValues.Values[i], Is.EqualTo(refTimeSeries.Arguments[0].Values[i]), $"Expected argument {i} of the {timeSeriesName} to be different:");
            }

            IVariable componentValues = timeSeries.Components[0];

            Assert.That(componentValues, Is.Not.Null, "Expected argument values not to be null:");
            Assert.That(componentValues.Values.Count, Is.EqualTo(refTimeSeries.Components[0].Values.Count), "Expected the number of argument values to be different:");
            for (var i = 0; i < componentValues.Values.Count; i++)
            {
                Assert.That(componentValues.Values[i], Is.EqualTo(refTimeSeries.Components[0].Values[i]), $"Expected components {i} of the {timeSeriesName} to be different:");
            }
        }

        #endregion
    }
}