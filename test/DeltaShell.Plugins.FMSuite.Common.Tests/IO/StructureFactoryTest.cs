using System;
using System.Collections.Generic;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructureFactoryTest
    {
        private MockRepository mocks;

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
                if (property == KnownGeneralStructureProperties.GateHeight) continue;

                var generalStructure = new Structure2D(StructureRegion.StructureTypeName.GeneralStructure);
                generalStructure.AddProperty(property.GetDescription(), typeof(double), "12.34");

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
                {KnownGeneralStructureProperties.WidthLeftW1, weirFormula.WidthLeftSideOfStructure},
                {KnownGeneralStructureProperties.WidthLeftWsdl, weirFormula.WidthStructureLeftSide},
                {KnownGeneralStructureProperties.WidthCenter, weirFormula.WidthStructureCentre },
                {KnownGeneralStructureProperties.WidthRightWsdr, weirFormula.WidthStructureRightSide},
                {KnownGeneralStructureProperties.WidthRightW2, weirFormula.WidthRightSideOfStructure},
                {KnownGeneralStructureProperties.LevelLeftZb1, weirFormula.BedLevelLeftSideOfStructure},
                {KnownGeneralStructureProperties.LevelLeftZbsl, weirFormula.BedLevelLeftSideStructure },
                {KnownGeneralStructureProperties.LevelCenter, weirFormula.BedLevelStructureCentre },
                {KnownGeneralStructureProperties.LevelRightZbsr, weirFormula.BedLevelRightSideStructure },
                {KnownGeneralStructureProperties.LevelRightZb2, weirFormula.BedLevelRightSideOfStructure },
                {KnownGeneralStructureProperties.GateDoorHeightGeneralStructure, weirFormula.GateOpening},
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
                {KnownGeneralStructureProperties.ExtraResistance, weirFormula.ExtraResistance}
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
            Assert.AreEqual(0.0, weir.CrestWidth);
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
            structure.AddProperty(KnownStructureProperties.CrestLevel, typeof(Steerable), "weir_crest_level.tim");
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
            var openingDirectionDefinition = schema.GetDefinition("gate", "horizontal_opening_direction");
            
            var structure = new Structure2D("gate");
            structure.AddProperty(KnownStructureProperties.Type, typeof(string), "gate");
            structure.AddProperty(KnownStructureProperties.Name, typeof(string), "Gate01");
            structure.AddProperty(KnownStructureProperties.X, typeof(double), "500");
            structure.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            structure.AddProperty(KnownStructureProperties.GateSillLevel, typeof(Steerable), "2");
            structure.AddProperty(KnownStructureProperties.GateSillWidth,typeof(double),"55.7");
            structure.AddProperty(KnownStructureProperties.GateOpeningWidth, typeof(Steerable), "1");
            structure.AddProperty(KnownStructureProperties.GateLowerEdgeLevel, typeof(Steerable), "2.8");
            structure.AddProperty(KnownStructureProperties.GateDoorHeight, typeof(double), "10");
            structure.AddProperty(KnownStructureProperties.GateHorizontalOpeningDirection, openingDirectionDefinition.DataType, "from_right");

            var dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            var gate = StructureFactory.CreateGate(structure, dummyPath, new DateTime());
            Assert.AreEqual("Gate01", gate.Name);
            Assert.IsNull(gate.LongName);
            Assert.IsNull(gate.Branch);
            Assert.IsNaN(gate.Chainage);
            Assert.AreEqual(new Point(500, 360), gate.Geometry);
            Assert.IsFalse(gate.UseSillLevelTimeSeries);
            Assert.AreEqual(2.0, gate.SillLevel);
            Assert.AreEqual(55.7, gate.SillWidth);
            Assert.IsFalse(gate.UseLowerEdgeLevelTimeSeries);
            Assert.AreEqual(2.8, gate.LowerEdgeLevel);
            Assert.AreEqual(0, gate.LowerEdgeLevelTimeSeries.Time.Values.Count);
            Assert.IsFalse(gate.UseOpeningWidthTimeSeries);
            Assert.AreEqual(1.0, gate.OpeningWidth);
            Assert.AreEqual(0, gate.OpeningWidthTimeSeries.Time.Values.Count);
            Assert.AreEqual(GateOpeningDirection.FromRight, gate.HorizontalOpeningDirection);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void CreateGateWithTimeSeriesTest()
        {
            var schema = new StructureSchemaCsvFile().ReadStructureSchema(StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);
            var openingDirectionDefinition = schema.GetDefinition("gate", "horizontal_opening_direction");

            var structure = new Structure2D("gate");
            structure.AddProperty(KnownStructureProperties.Type, typeof(string), "gate");
            structure.AddProperty(KnownStructureProperties.Name, typeof(string), "Gate02");
            structure.AddProperty(KnownStructureProperties.PolylineFile, typeof(string), "pump05.pli");
            structure.AddProperty(KnownStructureProperties.GateSillLevel, typeof(Steerable), "Gate02_lower_edge_level.tim");
            structure.AddProperty(KnownStructureProperties.GateLowerEdgeLevel, typeof(Steerable), "Gate02_lower_edge_level.tim");
            structure.AddProperty(KnownStructureProperties.GateOpeningWidth, typeof(Steerable), "Gate02_opening_width.tim");
            structure.AddProperty(KnownStructureProperties.GateDoorHeight, typeof(double), "10");
            structure.AddProperty(KnownStructureProperties.GateHorizontalOpeningDirection, openingDirectionDefinition.DataType, "from_right");

            var dummyPath = TestHelper.GetTestFilePath(@"structures/nonExistentFile_structures.ini");

            var gate = StructureFactory.CreateGate(structure, dummyPath, new DateTime(2013,1,1));
            Assert.AreEqual("Gate02", gate.Name);
            Assert.IsNull(gate.LongName);
            Assert.IsNull(gate.Branch);
            Assert.IsNaN(gate.Chainage);
            Assert.AreEqual(new LineString(new [] { new Coordinate(1, 2), new Coordinate(3, 4), new Coordinate(6, 7) }), gate.Geometry);
            Assert.IsTrue(gate.UseSillLevelTimeSeries);
            Assert.AreEqual(2, gate.SillLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, gate.SillLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, gate.SillLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);
            Assert.AreEqual(0.0, gate.SillWidth);
            Assert.IsTrue(gate.UseLowerEdgeLevelTimeSeries);
            Assert.AreEqual(2, gate.LowerEdgeLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, gate.LowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, gate.LowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);

            Assert.IsTrue(gate.UseOpeningWidthTimeSeries);
            Assert.AreEqual(2, gate.OpeningWidthTimeSeries.Time.Values.Count);
            Assert.AreEqual(5.6, gate.OpeningWidthTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(7.8, gate.OpeningWidthTimeSeries[new DateTime(2013, 1, 1, 2, 3, 0)]);

            Assert.AreEqual(GateOpeningDirection.FromRight, gate.HorizontalOpeningDirection);
        }

        #endregion Gate
    }
}