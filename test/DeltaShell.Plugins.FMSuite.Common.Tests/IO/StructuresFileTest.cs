using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.Structure;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class StructuresFileTest
    {
        private StructureSchema<ModelPropertyDefinition> schema;

        [SetUp]
        public void Setup()
        {
            schema = new StructureSchemaCsvFile().ReadStructureSchema(StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresUsingExampleFile()
        {
            var path = TestHelper.GetTestFilePath(@"structures\example-structures.imp");

            var structureFile = new StructuresFile { StructureSchema = schema };
            var structures = structureFile.ReadStructures2D(path).ToList();

            Assert.AreEqual(8, structures.Count);
            Assert.AreEqual(2, structures.Count(s => s.Structure2DType == Structure2DType.Weir));
            Assert.AreEqual(3, structures.Count(s => s.Structure2DType == Structure2DType.Pump));
            Assert.AreEqual(1, structures.Count(s => s.Structure2DType == Structure2DType.Gate));
            Assert.AreEqual(1, structures.Count(s => s.Structure2DType == Structure2DType.GeneralStructure));
            Assert.AreEqual(1, structures.Count(s => s.Structure2DType == Structure2DType.LeveeBreach));

            var weirDown = structures.First(s => s.Name == "Weir_down");
            Assert.AreEqual(8, weirDown.Properties.Count);
            Assert.AreEqual("47.9853 -260.3466", weirDown.GetProperty(StructureRegion.XCoordinates.Key).GetValueAsString());
            Assert.AreEqual("1700.8989 1119.2005", weirDown.GetProperty(StructureRegion.YCoordinates.Key).GetValueAsString());
            Assert.AreEqual("2", weirDown.GetProperty(StructureRegion.CrestLevel.Key).GetValueAsString());
            Assert.AreEqual("1", weirDown.GetProperty(StructureRegion.CorrectionCoeff.Key).GetValueAsString());
            Assert.AreEqual("1", weirDown.GetProperty(StructureRegion.UseVelocityHeight.Key).GetValueAsString());

            var generalStructure = structures.First(s => s.Name == "gs_01");
            Assert.That(generalStructure.Properties.Count, Is.EqualTo(32));
            Assert.AreEqual("2", generalStructure.GetProperty(StructureRegion.NumberOfCoordinates.Key).GetValueAsString());
            Assert.AreEqual("1111.2222 3333.4444", generalStructure.GetProperty(StructureRegion.XCoordinates.Key).GetValueAsString());
            Assert.AreEqual("2222.1111 4444.3333", generalStructure.GetProperty(StructureRegion.YCoordinates.Key).GetValueAsString());
            Assert.AreEqual("6", generalStructure.GetProperty(StructureRegion.Upstream1Level.Key).GetValueAsString());
            Assert.AreEqual("1", generalStructure.GetProperty(StructureRegion.Upstream1Width.Key).GetValueAsString());
            Assert.AreEqual("7", generalStructure.GetProperty(StructureRegion.Upstream2Level.Key).GetValueAsString());
            Assert.AreEqual("2", generalStructure.GetProperty(StructureRegion.Upstream2Width.Key).GetValueAsString());
            Assert.AreEqual("3", generalStructure.GetProperty(StructureRegion.CrestWidth.Key).GetValueAsString());
            Assert.AreEqual("9", generalStructure.GetProperty(StructureRegion.Downstream1Level.Key).GetValueAsString());
            Assert.AreEqual("4", generalStructure.GetProperty(StructureRegion.Downstream1Width.Key).GetValueAsString());
            Assert.AreEqual("10", generalStructure.GetProperty(StructureRegion.Downstream2Level.Key).GetValueAsString());
            Assert.AreEqual("5", generalStructure.GetProperty(StructureRegion.Downstream2Width.Key).GetValueAsString());
            Assert.AreEqual("8", generalStructure.GetProperty(StructureRegion.CrestLevel.Key).GetValueAsString());
            Assert.AreEqual("11", generalStructure.GetProperty(StructureRegion.GateLowerEdgeLevel.Key).GetValueAsString());
            Assert.AreEqual("12", generalStructure.GetProperty(StructureRegion.PosFreeGateFlowCoeff.Key).GetValueAsString());
            Assert.AreEqual("13", generalStructure.GetProperty(StructureRegion.PosDrownGateFlowCoeff.Key).GetValueAsString());
            Assert.AreEqual("14", generalStructure.GetProperty(StructureRegion.PosFreeWeirFlowCoeff.Key).GetValueAsString());
            Assert.AreEqual("15", generalStructure.GetProperty(StructureRegion.PosDrownWeirFlowCoeff.Key).GetValueAsString());
            Assert.AreEqual("16", generalStructure.GetProperty(StructureRegion.PosContrCoefFreeGate.Key).GetValueAsString());
            Assert.AreEqual("17", generalStructure.GetProperty(StructureRegion.NegFreeGateFlowCoeff.Key).GetValueAsString());
            Assert.AreEqual("18", generalStructure.GetProperty(StructureRegion.NegDrownGateFlowCoeff.Key).GetValueAsString());
            Assert.AreEqual("19", generalStructure.GetProperty(StructureRegion.NegFreeWeirFlowCoeff.Key).GetValueAsString());
            Assert.AreEqual("20", generalStructure.GetProperty(StructureRegion.NegDrownWeirFlowCoeff.Key).GetValueAsString());
            Assert.AreEqual("21", generalStructure.GetProperty(StructureRegion.NegContrCoefFreeGate.Key).GetValueAsString());
            Assert.AreEqual("22", generalStructure.GetProperty(StructureRegion.CrestLength.Key).GetValueAsString());
            Assert.AreEqual("1", generalStructure.GetProperty(StructureRegion.UseVelocityHeight.Key).GetValueAsString());
            Assert.AreEqual("23", generalStructure.GetProperty(StructureRegion.ExtraResistance.Key).GetValueAsString());
            Assert.AreEqual("24", generalStructure.GetProperty(StructureRegion.GateHeight.Key).GetValueAsString());
            Assert.AreEqual("25", generalStructure.GetProperty(StructureRegion.GateOpeningWidth.Key).GetValueAsString());
            Assert.AreEqual("symmetric", generalStructure.GetProperty(StructureRegion.GateHorizontalOpeningDirection.Key).GetValueAsString());


            var leveeBreach = structures.FirstOrDefault(s => s.Name == "lb_01");
            Assert.NotNull(leveeBreach);
            Assert.AreEqual(17, leveeBreach.Properties.Count);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresInvalidFileFormat()
        {
            var path = TestHelper.GetTestFilePath(@"structures\invalidFormat.imp");
            var structuresFile = new StructuresFile { StructureSchema = schema };
            var structures = structuresFile.ReadStructures2D(path);
            Assert.AreEqual(0, structures.Count(), "Nothing should have been read.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresWithUnsupportedSections()
        {
            var path = TestHelper.GetTestFilePath(@"structures\mixedFile.imp");
            var schema =
                new StructureSchemaCsvFile().ReadStructureSchema(
                    StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);

            IList<Structure2D> structures = null;
            var structuresFile = new StructuresFile { StructureSchema = schema };
            TestHelper.AssertLogMessageIsGenerated(
                () => structures = structuresFile.ReadStructures2D(path).ToList(),
                "Section [test] not supported for structures and is skipped.");
            Assert.AreEqual(1, structures.Count, "Only one structure section in file.");

            var w = structures[0];
            Assert.AreEqual("w", w.GetProperty(KnownStructureProperties.Name).GetValueAsString());
            Assert.IsNull(w.GetProperty("dummy"), "Should not accidentally take key from [test] section.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresWithKeysNotInSchema()
        {
            var path = TestHelper.GetTestFilePath(@"structures\keyNotInSchema.imp");
            var schema =
                new StructureSchemaCsvFile().ReadStructureSchema(
                    StructureSchemaCsvFileTest.ApplicationStructuresSchemaCsvFilePath);

            IList<Structure2D> structures = null;
            var structuresFile = new StructuresFile { StructureSchema = schema };
            TestHelper.AssertLogMessageIsGenerated(
                () => structures = structuresFile.ReadStructures2D(path).ToList(),
                String.Format(
                    "Property 'Im_a_nonexistent_property' not supported for structures of type 'weir' and is skipped. (Line 12 of file {0})",
                    path));
            Assert.AreEqual(1, structures.Count, "Only one structure section in file.");

            var w = structures[0];
            Assert.AreEqual("w", w.GetProperty(KnownStructureProperties.Name).GetValueAsString());
            Assert.IsNull(w.GetProperty("Im_a_nonexistent_property"), "Should not add keys outside of schema.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadStructuresWithMissingTypeProperty()
        {
            var path = TestHelper.GetTestFilePath(@"structures\missingTypeProperty.imp");

            IList<Structure2D> structures = null;
            var structuresFile = new StructuresFile { StructureSchema = schema };
            TestHelper.AssertLogMessageIsGenerated(() => structures = structuresFile.ReadStructures2D(path).ToList(),
                "Obligated property 'type' expected but is missing; Structure is skipped.");
            Assert.AreEqual(0, structures.Count, "No valid structures in file.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteStructures()
        {
            var exportFilePath = TestHelper.GetCurrentMethodName() + ".imp";
            if (File.Exists(exportFilePath)) File.Delete(exportFilePath);

            var weir = new Structure2D("weir");
            weir.AddProperty(KnownStructureProperties.Type, typeof(string), "weir");
            weir.AddProperty(KnownStructureProperties.Name, typeof(string), "Weir_down");
            weir.AddProperty(KnownStructureProperties.X, typeof(double), "680");
            weir.AddProperty(KnownStructureProperties.Y, typeof(double), "360");
            weir.AddProperty(KnownStructureProperties.CrestLevel, typeof(double), "2");
            weir.AddProperty(KnownStructureProperties.LateralContractionCoefficient, typeof(double), "1");

            var structures = new[]
                {
                    weir
                };
            StructuresFile.WriteStructures2D(exportFilePath, structures);

            var fileContents = File.ReadAllText(exportFilePath);
            Assert.AreEqual(
                "[structure]" + Environment.NewLine +
                "    type                  = weir                " + Environment.NewLine +
                "    id                    = Weir_down           " + Environment.NewLine +
                "    x                     = 680                 " + Environment.NewLine +
                "    y                     = 360                 " + Environment.NewLine +
                "    crest_level           = 2                   " + Environment.NewLine +
                "    lat_contr_coeff       = 1                   " + Environment.NewLine, fileContents);

            File.Delete(exportFilePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteLeveeBreach()
        {
            var mocks = new MockRepository();
            var leveeBreach = mocks.StrictMock<ILeveeBreach>();
            leveeBreach.Expect(lb => lb.Name).Return("lb_01").Repeat.Any();
            leveeBreach.Expect(lb => lb.BreachLocationX).Return(125).Repeat.Any();
            leveeBreach.Expect(lb => lb.BreachLocationY).Return(250).Repeat.Any();
            leveeBreach.Expect(lb => lb.WaterLevelUpstreamLocationX).Return(125).Repeat.Any();
            leveeBreach.Expect(lb => lb.WaterLevelUpstreamLocationY).Return(250).Repeat.Any();
            leveeBreach.Expect(lb => lb.WaterLevelDownstreamLocationX).Return(125).Repeat.Any();
            leveeBreach.Expect(lb => lb.WaterLevelDownstreamLocationY).Return(250).Repeat.Any();
            leveeBreach.Expect(lb => lb.WaterLevelFlowLocationsActive).Return(false).Repeat.Any();
            
            leveeBreach.Expect(lb => lb.Geometry).Return(new LineString(new[] { new Coordinate(0, 0), new Coordinate(10, 10) })).Repeat.Any();
            leveeBreach.Expect(lb => lb.LeveeBreachFormula).Return(LeveeBreachGrowthFormula.VerheijvdKnaap2002).Repeat.Any();

            var settings = new VerheijVdKnaap2002BreachSettings
            {
                StartTimeBreachGrowth = new DateTime(2001, 01, 01),
                BreachGrowthActive = true,
                InitialCrestLevel = 2.25,
                MinimumCrestLevel = 0.69,
                InitialBreachWidth = 2.38,
                PeriodToReachZmin = new TimeSpan(1, 11, 11),
                Factor1Alfa = 0.88,
                Factor2Beta = 0.73,
                CriticalFlowVelocity = 1.22
            };

            leveeBreach.Expect(lb => lb.GetActiveLeveeBreachSettings()).Return(settings).Repeat.Any();

            mocks.ReplayAll();

            var iniFilePath = TestHelper.GetCurrentMethodName() + ".ini";
            if (File.Exists(iniFilePath)) File.Delete(iniFilePath);

            try
            {
                var structuresFile = new StructuresFile
                {
                    StructureSchema = schema,
                    ReferenceDate = new DateTime(2000, 1, 1)
                };
                structuresFile.Write(iniFilePath, new[] { leveeBreach });
                Assert.That(File.Exists(iniFilePath));

                // This is a temporary addition until the reader for levee breaches is finished.
                var text = File.ReadAllText(iniFilePath);
                var expectedText =
                 "[structure]" + Environment.NewLine +
                 "    type                  = dambreak            # Type of structure" + Environment.NewLine +
                 "    id                    = lb_01               # Unique structure id." + Environment.NewLine +
                 "    polylinefile          = lb_01.pli           # *.pli" + Environment.NewLine +
                 "    StartLocationX        = 125                 # X-position of the breach growth" + Environment.NewLine +
                 "    StartLocationY        = 250                 # Y-position of the breach growth" + Environment.NewLine +
                 "    T0                    = 31622400            # Start time of the breach (in seconds) [s]" + Environment.NewLine +
                 "    State                 = 1                   # 0 = off 1 = on (typically set via BMI)" + Environment.NewLine +
                 "    waterLevelUpstreamLocationX= 125                 # X-position of the upstream point of the water level stream to the breach point" + Environment.NewLine +
                 "    waterLevelUpstreamLocationY= 250                 # Y-position of the upstream point of the water level stream to the breach point" + Environment.NewLine +
                 "    waterLevelDownstreamLocationX= 125                 # X-position of the downstream point of the water level stream from the breach point" + Environment.NewLine +
                 "    waterLevelDownstreamLocationY= 250                 # Y-position of the downstream point of the water level stream from the breach point" + Environment.NewLine +
                 "    Algorithm             = 2                   # 0 = unknown 2 = Verheij - vd Knaap (2002) 3 = User defined" + Environment.NewLine +
                 "    CrestLevelIni         = 2.25                # Initial crest level [m]" + Environment.NewLine +
                 "    CrestLevelMin         = 0.69                # Minimum crest level [m]" + Environment.NewLine +
                 "    BreachWidthIni        = 2.38                # Initial breach width [m]" + Environment.NewLine +
                 "    TimeToBreachToMaximumDepth= 4271                # Time to reach maximum breach depth (in seconds) [s]" + Environment.NewLine +
                 "    F1                    = 0.88                # Factor 1 Alfa [-]" + Environment.NewLine +
                 "    F2                    = 0.73                # Factor 2 Beta [-]" + Environment.NewLine +
                 "    Ucrit                 = 1.22                # Critical flow velocity [m/s]" + Environment.NewLine;

                Assert.AreEqual(expectedText, text);

            }
            finally
            {
                if (File.Exists(iniFilePath)) File.Delete(iniFilePath);
            }
            
            mocks.VerifyAll();
        }
        
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category("Quarantine")] // StructuresFile.Write() does not write a General section which is required for properly Reading back the structures.
        public void CanRepeatedlyReadAndWrite()
        {
            var path = TestHelper.GetTestFilePath(@"structures\example-structures.imp");
            Assert.True(File.Exists(path), $"Expected file '{path}' does not exist");
            var structureFile = new StructuresFile { StructureSchema = schema };
            var structures = structureFile.ReadStructures2D(path).ToList();

            var exportFilePath = TestHelper.GetCurrentMethodName() + ".imp";
            var newStructuresFile = new StructuresFile { StructureSchema = schema };
            StructuresFile.WriteStructures2D(exportFilePath, structures);

            CompareStructureIniFiles(path, exportFilePath); // Note: Comments in user file can differ from schema!

            var newStructures = newStructuresFile.ReadStructures2D(exportFilePath).ToList();

            CompareStructures(structures, newStructures);

            File.Delete(exportFilePath);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category("Quarantine")] // StructuresFile.Write() does not write a General section which is required for properly Reading back the structures.
        public void GivenGeneralStructureWhenWritingToFileAndReadingFromThatFileThenResultingStructuresAreTheSame()
        {
            var iniFilePath = TestHelper.GetTestFilePath(@"structures\temp_file.ini");
            if (File.Exists(iniFilePath)) File.Delete(iniFilePath);

            var initialFormula = new GeneralStructureWeirFormula
            {
                PositiveFreeGateFlow = 1,
                PositiveDrownedGateFlow = 2,
                PositiveFreeWeirFlow = 3,
                PositiveDrownedWeirFlow = 4,
                PositiveContractionCoefficient = 5,
                NegativeFreeGateFlow = 6,
                NegativeDrownedGateFlow = 7,
                NegativeFreeWeirFlow = 8,
                NegativeDrownedWeirFlow = 9,
                NegativeContractionCoefficient = 10,
                WidthLeftSideOfStructure = 11,
                WidthStructureLeftSide = 12,
                WidthStructureCentre = 13,
                WidthStructureRightSide = 14,
                WidthRightSideOfStructure = 15,
                BedLevelLeftSideOfStructure = 16,
                BedLevelLeftSideStructure = 17,
                BedLevelStructureCentre = 18,
                BedLevelRightSideStructure = 19,
                BedLevelRightSideOfStructure = 20,
                UseExtraResistance = true,
                ExtraResistance = 40,
                GateOpening = 60
            };
            var initialGeneralStructure = new Weir("gs01")
            {
                Geometry = new LineString(new[] { new Coordinate(4, 5), new Coordinate(6, 7) }),
                WeirFormula = initialFormula
            };
            var structures = new IStructure1D[] { initialGeneralStructure };
            var structuresFile = new StructuresFile
            {
                StructureSchema = schema,
                ReferenceDate = new DateTime(2013, 1, 1, 0, 0, 0)
            };

            structuresFile.Write(iniFilePath, structures);
            var readResult = structuresFile.Read(iniFilePath);
            Assert.That(readResult.Count, Is.EqualTo(1));

            var resultingGeneralStructure = readResult.FirstOrDefault() as Weir;
            Assert.IsNotNull(resultingGeneralStructure);
            var resultingFormula = resultingGeneralStructure.WeirFormula as GeneralStructureWeirFormula;
            Assert.IsNotNull(resultingFormula);

            var weirPropertiesAreEqual = initialGeneralStructure.CanBeTimedependent == resultingGeneralStructure.CanBeTimedependent &&
                                         initialGeneralStructure.IsGated == resultingGeneralStructure.IsGated &&
                                         initialGeneralStructure.AllowNegativeFlow == resultingGeneralStructure.AllowNegativeFlow &&
                                         initialGeneralStructure.AllowPositiveFlow == resultingGeneralStructure.AllowPositiveFlow &&
                                         initialGeneralStructure.IsRectangle == resultingGeneralStructure.IsRectangle &&
                                         initialGeneralStructure.FormulaName == resultingGeneralStructure.FormulaName;

            var formulasAreEqual = (initialFormula.Name == resultingFormula.Name &&
                                    initialFormula.PositiveContractionCoefficient == resultingFormula.PositiveContractionCoefficient &&
                                    initialFormula.PositiveContractionCoefficient == resultingFormula.PositiveContractionCoefficient &&
                                    initialFormula.PositiveDrownedGateFlow == resultingFormula.PositiveDrownedGateFlow &&
                                    initialFormula.PositiveDrownedWeirFlow == resultingFormula.PositiveDrownedWeirFlow &&
                                    initialFormula.PositiveFreeWeirFlow == resultingFormula.PositiveFreeWeirFlow &&

                                    initialFormula.NegativeContractionCoefficient == resultingFormula.NegativeContractionCoefficient &&
                                    initialFormula.NegativeDrownedGateFlow == resultingFormula.NegativeDrownedGateFlow &&
                                    initialFormula.NegativeDrownedWeirFlow == resultingFormula.NegativeDrownedWeirFlow &&
                                    initialFormula.NegativeFreeGateFlow == resultingFormula.NegativeFreeGateFlow &&
                                    initialFormula.NegativeFreeWeirFlow == resultingFormula.NegativeFreeWeirFlow &&

                                    initialFormula.BedLevelLeftSideOfStructure == resultingFormula.BedLevelLeftSideOfStructure &&
                                    initialFormula.BedLevelLeftSideStructure == resultingFormula.BedLevelLeftSideStructure &&
                                    initialFormula.BedLevelStructureCentre == resultingFormula.BedLevelStructureCentre &&
                                    initialFormula.BedLevelRightSideStructure == resultingFormula.BedLevelRightSideStructure &&
                                    initialFormula.BedLevelRightSideOfStructure == resultingFormula.BedLevelRightSideOfStructure &&

                                    initialFormula.WidthLeftSideOfStructure == resultingFormula.WidthLeftSideOfStructure &&
                                    initialFormula.WidthStructureLeftSide == resultingFormula.WidthStructureLeftSide &&
                                    initialFormula.WidthStructureCentre == resultingFormula.WidthStructureCentre &&
                                    initialFormula.WidthStructureRightSide == resultingFormula.WidthStructureRightSide &&
                                    initialFormula.WidthRightSideOfStructure == resultingFormula.WidthRightSideOfStructure &&

                                    initialFormula.UseExtraResistance == resultingFormula.UseExtraResistance &&
                                    initialFormula.ExtraResistance == resultingFormula.ExtraResistance &&
                                    initialFormula.GateOpening == resultingFormula.GateOpening);

            Assert.IsTrue(weirPropertiesAreEqual && formulasAreEqual);

            //cleanup
            if (File.Exists(iniFilePath)) File.Delete(iniFilePath);
        }

        [Test]
        public void ReadThrowsForInvalidFilePath()
        {
            var structureFile = new StructuresFile { StructureSchema = new StructureSchema<ModelPropertyDefinition>() };
            Assert.Throws<FileNotFoundException>(() => structureFile.ReadStructures2D("I do not exist").ToList());
        }

        #region Sobek Structures

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadAsSobekStructuresTest()
        {
            var path = TestHelper.GetTestFilePath(@"structures\example-structures-sobek.imp");

            var structureFile = new StructuresFile { StructureSchema = schema, ReferenceDate = new DateTime() };

            var structures = structureFile.Read(path).ToList();

            Assert.AreEqual(3, structures.Count); // There are 4 pumps in the file
            Assert.AreEqual(0, structures.OfType<IWeir>().Count());
            Assert.AreEqual(3, structures.OfType<IPump>().Count());

            var pump = structures.OfType<IPump>().First();
            Assert.AreEqual("pump01", pump.Name);
            Assert.IsNull(pump.LongName);
            Assert.IsNull(pump.Branch);
            Assert.IsNaN(pump.Chainage);
            //Assert.AreEqual(new Point(500, 360), pump.Geometry);
            Assert.That(new LineString(new[] { new Coordinate(1, 3), new Coordinate(2, 4) }), Is.EqualTo(pump.Geometry));
            Assert.AreEqual(3.0, pump.Capacity);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTimeDependentSobekStructuresTest()
        {
            var path = TestHelper.GetTestFilePath(@"structures\time_dependent_structures.ini");

            var structureFile = new StructuresFile { StructureSchema = schema, ReferenceDate = new DateTime(2013, 1, 1) };

            var structures = structureFile.Read(path).ToList();

            Assert.AreEqual(3, structures.Count);
            Assert.AreEqual(1, structures.OfType<IWeir>().Count());
            Assert.AreEqual(1, structures.OfType<IPump>().Count());
            Assert.AreEqual(1, structures.OfType<IGate>().Count());

            var pump = structures.OfType<IPump>().First();
            Assert.AreEqual(2, pump.CapacityTimeSeries.Time.Values.Count);
            Assert.AreEqual(5.6, pump.CapacityTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(11.12, pump.CapacityTimeSeries[new DateTime(2013, 1, 1, 1, 2, 0)]);

            var weir = structures.OfType<IWeir>().First(w => w.WeirFormula is SimpleWeirFormula);
            Assert.AreEqual(2, weir.CrestLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, weir.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, weir.CrestLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);

            var gate = structures.OfType<IGate>().First();
            Assert.AreEqual(2, gate.LowerEdgeLevelTimeSeries.Time.Values.Count);
            Assert.AreEqual(1.2, gate.LowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(3.4, gate.LowerEdgeLevelTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)]);
            Assert.AreEqual(2, gate.OpeningWidthTimeSeries.Time.Values.Count);
            Assert.AreEqual(5.6, gate.OpeningWidthTimeSeries[new DateTime(2013, 1, 1, 0, 0, 0)]);
            Assert.AreEqual(7.8, gate.OpeningWidthTimeSeries[new DateTime(2013, 1, 1, 2, 3, 0)]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteSobekStructuresTest()
        {
            var exportFilePath = TestHelper.GetCurrentMethodName() + ".imp";

            #region Clean up left overs:

            if (File.Exists(exportFilePath))
            {
                File.Delete(exportFilePath);
            }

            var expectedFileNames = new[] { "pump1.pli", "weir1.pli", "gate1.pli" };
            foreach (var expectedFileName in expectedFileNames)
            {
                var pliFile = FMSuiteFileBase.GetOtherFilePathInSameDirectory(exportFilePath, expectedFileName);
                if (File.Exists(pliFile))
                {
                    File.Delete(pliFile);
                }
            }

            #endregion

            var pump = new Pump("pump1", true)
            {
                Branch = null,
                Chainage = double.NaN,
                UseCapacityTimeSeries = false,
                Capacity = 3.0,
                ControlDirection = PumpControlDirection.DeliverySideControl,
                StartDelivery = 3.4,
                StopDelivery = 2.3,
                Geometry = new LineString(new[] { new Coordinate(1, 2), new Coordinate(2, 3) })
            };
            var simpleWeir = new Weir("weir1", true)
            {
                Branch = null,
                Chainage = double.NaN,
                UseCrestLevelTimeSeries = false,
                CrestLevel = 2.0,
                CrestWidth = 25,
                WeirFormula = new SimpleWeirFormula
                {
                    CorrectionCoefficient = 0.7
                },
                Geometry = new LineString(new[] { new Coordinate(4, 5), new Coordinate(6, 7) })
            };
            var simpleGate = new Gate("gate1")
            {
                Branch = null,
                Chainage = double.NaN,
                UseLowerEdgeLevelTimeSeries = false,
                LowerEdgeLevel = 4.0,
                UseOpeningWidthTimeSeries = false,
                OpeningWidth = 12.0,
                DoorHeight = 3.0,
                HorizontalOpeningDirection = GateOpeningDirection.FromLeft,
                UseSillLevelTimeSeries = false,
                SillLevel = 1.0,
                Geometry = new LineString(new[] { new Coordinate(8, 9), new Coordinate(10, 11) })
            };

            var structures = new IStructure1D[]
                {
                    pump, simpleWeir, simpleGate
                };
            var structuresFile = new StructuresFile
            {
                StructureSchema = schema,
                ReferenceDate = new DateTime(2013, 1, 1, 0, 0, 0)
            };
            structuresFile.Write(exportFilePath, structures);

            var fileContents = File.ReadAllText(exportFilePath);
            Assert.AreEqual(
                "[structure]" + Environment.NewLine +
                "    type                  = pump                # \"Structure type must read pump.\"" + Environment.NewLine +
                "    id                    = pump1               # Unique structure id." + Environment.NewLine +
                "    polylinefile          = pump1.pli           # *.pli" + Environment.NewLine +
                "    capacity              = 3                   # Pump capacity (m�/s). (number of values = max(1 numStages))." + Environment.NewLine +
                Environment.NewLine +
                "[structure]" + Environment.NewLine +
                "    type                  = weir                # \"Structure type must read weir.\"" + Environment.NewLine +
                "    id                    = weir1               # Unique structure id." + Environment.NewLine +
                "    polylinefile          = weir1.pli           # *.pli" + Environment.NewLine +
                "    crestLevel            = 2                   # Crest level of weir (m AD)." + Environment.NewLine +
                "    crestWidth            = 25                  # (optional) Width of weir (m)." + Environment.NewLine +
                "    corrCoeff             = 0.7                 # Correction coefficient (-)." + Environment.NewLine +
                Environment.NewLine +
                "[structure]" + Environment.NewLine +
                "    type                  = gate                # \"Structure type must read gate.\"" + Environment.NewLine +
                "    id                    = gate1               # Unique structure id." + Environment.NewLine +
                "    polylinefile          = gate1.pli           # *.pli" + Environment.NewLine +
                "    crestLevel            = 1                   # Crest level (m AD)." + Environment.NewLine +
                "    gateLowerEdgeLevel    = 4                   # Position of gate door's lower edge (m AD)." + Environment.NewLine +
                "    gateOpeningWidth      = 12                  # Opening width between gate doors should be smaller than (or equal to) crestWidth. Use 0.0 for a vertical door. (m)." + Environment.NewLine +
                "    gateHeight            = 3                   # Height of gate door. Needed for possible overflow across door (m)." + Environment.NewLine +
                "    gateOpeningHorizontalDirection= from_left           # Horizontal direction of the opening doors" + Environment.NewLine, fileContents);

            foreach (var expectedFileName in expectedFileNames)
            {
                var filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(exportFilePath, expectedFileName);
                Assert.IsTrue(File.Exists(filePath), $"File '{filePath}' expected to exist.");
                File.Delete(filePath);
            }

            File.Delete(exportFilePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteTimeDependentSobekStructuresTest()
        {
            var exportFilePath = TestHelper.GetCurrentMethodName() + ".imp";

            #region Clean up left overs:

            if (File.Exists(exportFilePath))
            {
                File.Delete(exportFilePath);
            }

            var expectedFileNames = new[]
                {
                    "pump1.pli", "weir1.pli", "gate1.pli",
                    "pump1_capacity.tim",
                    "weir1_crestLevel.tim",
                    "gate1_gateLowerEdgeLevel.tim", "gate1_gateOpeningWidth.tim"
                };
            foreach (var expectedFileName in expectedFileNames)
            {
                var pliFile = FMSuiteFileBase.GetOtherFilePathInSameDirectory(exportFilePath, expectedFileName);
                if (File.Exists(pliFile))
                {
                    File.Delete(pliFile);
                }
            }

            #endregion

            var pump = new Pump("pump1", true)
            {
                Branch = null,
                Chainage = double.NaN,
                UseCapacityTimeSeries = true,
                Capacity = 3.0,
                ControlDirection = PumpControlDirection.DeliverySideControl,
                StartDelivery = 3.4,
                StopDelivery = 2.3,
                Geometry = new LineString(new[] { new Coordinate(1, 2), new Coordinate(2, 3) })
            };
            pump.CapacityTimeSeries[new DateTime(2013, 1, 2, 3, 4, 0)] = 5.6;
            pump.CapacityTimeSeries[new DateTime(2013, 7, 8, 9, 10, 0)] = 11.12;

            var simpleWeir = new Weir("weir1", true)
            {
                Branch = null,
                Chainage = double.NaN,
                UseCrestLevelTimeSeries = true,
                CrestLevel = 2.0,
                CrestWidth = 0,
                WeirFormula = new SimpleWeirFormula
                {
                    CorrectionCoefficient = 0.7
                },
                Geometry = new LineString(new[] { new Coordinate(4, 5), new Coordinate(6, 7) })
            };
            simpleWeir.CrestLevelTimeSeries[new DateTime(2013, 1, 2, 3, 4, 0)] = 6.5;
            simpleWeir.CrestLevelTimeSeries[new DateTime(2013, 7, 8, 9, 10, 0)] = 12.11;

            var simpleGate = new Gate("gate1")
            {
                Branch = null,
                Chainage = double.NaN,
                UseLowerEdgeLevelTimeSeries = true,
                LowerEdgeLevel = 4.0,
                UseOpeningWidthTimeSeries = true,
                OpeningWidth = 12.0,
                DoorHeight = 3.0,
                HorizontalOpeningDirection = GateOpeningDirection.Symmetric,
                UseSillLevelTimeSeries = true,
                SillLevel = 1.0,
                SillWidth = 15.5,
                Geometry = new LineString(new[] { new Coordinate(8, 9), new Coordinate(10, 11) })
            };

            simpleGate.SillLevelTimeSeries[new DateTime(2013, 6, 5, 4, 3, 2)] = 1.0;
            simpleGate.SillLevelTimeSeries[new DateTime(2013, 1, 2, 3, 4, 5)] = 6.7;

            simpleGate.LowerEdgeLevelTimeSeries[new DateTime(2013, 4, 3, 2, 1, 0)] = 4.3;
            simpleGate.LowerEdgeLevelTimeSeries[new DateTime(2013, 8, 7, 6, 5, 0)] = 2.1;

            simpleGate.OpeningWidthTimeSeries[new DateTime(2013, 1, 1, 1, 1, 0)] = 8.7;
            simpleGate.OpeningWidthTimeSeries[new DateTime(2013, 2, 3, 4, 5, 0)] = 6.5;

            var structures = new IStructure1D[]
                {
                    pump, simpleWeir, simpleGate
                };
            var structuresFile = new StructuresFile { StructureSchema = schema, ReferenceDate = new DateTime(2013, 1, 1, 0, 0, 0) };
            structuresFile.Write(exportFilePath, structures);

            var fileContents = File.ReadAllText(exportFilePath);
            Assert.AreEqual(
                "[structure]" + Environment.NewLine +
                "    type                  = pump                # \"Structure type must read pump.\"" + Environment.NewLine +
                "    id                    = pump1               # Unique structure id." + Environment.NewLine +
                "    polylinefile          = pump1.pli           # *.pli" + Environment.NewLine +
                "    capacity              = pump1_capacity.tim  # Pump capacity (m�/s). (number of values = max(1 numStages))." + Environment.NewLine +
                Environment.NewLine +
                "[structure]" + Environment.NewLine +
                "    type                  = weir                # \"Structure type must read weir.\"" + Environment.NewLine +
                "    id                    = weir1               # Unique structure id." + Environment.NewLine +
                "    polylinefile          = weir1.pli           # *.pli" + Environment.NewLine +
                "    crestLevel            = weir1_crestLevel.tim# Crest level of weir (m AD)." + Environment.NewLine +
                "    corrCoeff             = 0.7                 # Correction coefficient (-)." + Environment.NewLine +
                Environment.NewLine +
                "[structure]" + Environment.NewLine +
                "    type                  = gate                # \"Structure type must read gate.\"" + Environment.NewLine +
                "    id                    = gate1               # Unique structure id." + Environment.NewLine +
                "    polylinefile          = gate1.pli           # *.pli" + Environment.NewLine +
                "    crestLevel            = gate1_crestLevel.tim# Crest level (m AD)." + Environment.NewLine +
                "    gateLowerEdgeLevel    = gate1_gateLowerEdgeLevel.tim# Position of gate door's lower edge (m AD)." + Environment.NewLine +
                "    gateOpeningWidth      = gate1_gateOpeningWidth.tim# Opening width between gate doors should be smaller than (or equal to) crestWidth. Use 0.0 for a vertical door. (m)." + Environment.NewLine +
                "    gateHeight            = 3                   # Height of gate door. Needed for possible overflow across door (m)." + Environment.NewLine +
                "    gateOpeningHorizontalDirection= symmetric           # Horizontal direction of the opening doors" + Environment.NewLine +
                "    crestWidth            = 15.5                # Crest width (m)." + Environment.NewLine, fileContents);

            foreach (var expectedFileName in expectedFileNames)
            {
                var filePath = NGHSFileBase.GetOtherFilePathInSameDirectory(exportFilePath, expectedFileName);
                Assert.IsTrue(File.Exists(filePath), $"File '{filePath}' expected to exist.");
                File.Delete(filePath);
            }

            File.Delete(exportFilePath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category("Quarantine")] // StructuresFile.Write() does not write a General section which is required for properly Reading back the structures.
        public void WriteReadLeveeBreachVerheij_StructureBeforeWritingAndAfterLoading_ShouldBeEqual()
        {
            var iniFilePath = TestHelper.GetCurrentMethodName() + ".ini";
            if (File.Exists(iniFilePath)) File.Delete(iniFilePath);

            try
            {
                var name = "lb_01";
                var leveeBreach = new LeveeBreach
                {
                    Name = name,
                    BreachLocationX = 125,
                    BreachLocationY = 250,
                    Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(10, 10) }),
                    LeveeBreachFormula = LeveeBreachGrowthFormula.VerheijvdKnaap2002,

                };

                var settings = leveeBreach.GetActiveLeveeBreachSettings() as VerheijVdKnaap2002BreachSettings;
                Assert.NotNull(settings);

                settings.BreachGrowthActive = true;
                settings.StartTimeBreachGrowth = new DateTime(2018, 5, 29, 18, 19, 0);
                settings.InitialCrestLevel = 2.25;
                settings.MinimumCrestLevel = 0.69;
                settings.InitialBreachWidth = 2.38;
                settings.PeriodToReachZmin = new TimeSpan(1, 11, 11);
                settings.Factor1Alfa = 0.88;
                settings.Factor2Beta = 0.73;
                settings.CriticalFlowVelocity = 1.22;

                var structuresFile = new StructuresFile
                {
                    StructureSchema = schema,
                    ReferenceDate = new DateTime(2000, 1, 1)
                };
                structuresFile.Write(iniFilePath, new[] { leveeBreach });
                Assert.That(File.Exists(iniFilePath));

                var importedStructures = structuresFile.Read(iniFilePath);
                Assert.IsNotEmpty(importedStructures);
                var importedLeveeBreach = importedStructures.FirstOrDefault(s => s.Name == name) as LeveeBreach;
                Assert.NotNull(importedLeveeBreach);

                CompareLeveeBreaches(leveeBreach, importedLeveeBreach);
                CompareLeveeBreachSettingsBase(leveeBreach.GetActiveLeveeBreachSettings(), importedLeveeBreach.GetActiveLeveeBreachSettings());

                var importedSettings = importedLeveeBreach.GetActiveLeveeBreachSettings() as VerheijVdKnaap2002BreachSettings;
                Assert.NotNull(importedSettings);
                CompareLeveeBreachVerheijSettings(settings, importedSettings);

            }
            finally
            {
                if (File.Exists(iniFilePath)) File.Delete(iniFilePath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category("Quarantine")] // StructuresFile.Write() does not write a General section which is required for properly Reading back the structures.
        public void WriteReadLeveeBreachUserDefined_StructureBeforeWritingAndAfterLoading_ShouldBeEqual()
        {
            var iniFilePath = TestHelper.GetCurrentMethodName() + ".ini";
            if (File.Exists(iniFilePath)) File.Delete(iniFilePath);

            try
            {
                var name = "lb_02";
                var leveeBreach = new LeveeBreach
                {
                    Name = name,
                    BreachLocationX = 250,
                    BreachLocationY = 375,
                    Geometry = new LineString(new[] { new Coordinate(10, 10), new Coordinate(20, 20) }),
                    LeveeBreachFormula = LeveeBreachGrowthFormula.UserDefinedBreach
                };

                var settings = leveeBreach.GetActiveLeveeBreachSettings() as UserDefinedBreachSettings;
                Assert.NotNull(settings);

                settings.BreachGrowthActive = true;
                settings.StartTimeBreachGrowth = new DateTime(2018, 5, 31, 14, 30, 00);
                settings.ManualBreachGrowthSettings = new EventedList<BreachGrowthSetting>
                {
                    new BreachGrowthSetting {TimeSpan = new TimeSpan(0,10,0), Height = 1, Width = 2},
                    new BreachGrowthSetting {TimeSpan = new TimeSpan(0,20,0), Height = 3, Width = 4},
                    new BreachGrowthSetting {TimeSpan = new TimeSpan(0,30,0), Height = 5, Width = 6},
                    new BreachGrowthSetting {TimeSpan = new TimeSpan(0,40,0), Height = 7, Width = 8},
                    new BreachGrowthSetting {TimeSpan = new TimeSpan(0,50,0), Height = 9, Width = 10},
                    new BreachGrowthSetting {TimeSpan = new TimeSpan(1,00,0), Height = 11, Width = 12},
                    new BreachGrowthSetting {TimeSpan = new TimeSpan(1,10,0), Height = 13, Width = 14}
                };

                var structuresFile = new StructuresFile
                {
                    StructureSchema = schema,
                    ReferenceDate = new DateTime(2000, 1, 1)
                };
                structuresFile.Write(iniFilePath, new[] { leveeBreach });
                Assert.That(File.Exists(iniFilePath));

                var importedStructures = structuresFile.Read(iniFilePath);
                Assert.IsNotEmpty(importedStructures);
                var importedLeveeBreach = importedStructures.FirstOrDefault(s => s.Name == name) as LeveeBreach;
                Assert.NotNull(importedLeveeBreach);

                CompareLeveeBreaches(leveeBreach, importedLeveeBreach);
                CompareLeveeBreachSettingsBase(leveeBreach.GetActiveLeveeBreachSettings(), importedLeveeBreach.GetActiveLeveeBreachSettings());

                var importedSettings = importedLeveeBreach.GetActiveLeveeBreachSettings() as UserDefinedBreachSettings;
                Assert.NotNull(importedSettings);
                CompareLeveeBreachUserDefinedSettings(settings, importedSettings);

            }
            finally
            {
                if (File.Exists(iniFilePath)) File.Delete(iniFilePath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category("Quarantine")] // StructuresFile.Write() does not write a General section which is required for properly Reading back the structures.
        public void WriteReadLeveeBreachWithInactiveBreachGrowth_ExportedAndImportedStructuresShouldBeEqual()
        {
            var iniFilePath = TestHelper.GetCurrentMethodName() + ".ini";
            if (File.Exists(iniFilePath)) File.Delete(iniFilePath);

            try
            {
                var name = "lb_02";
                var leveeBreach = new LeveeBreach
                {
                    Name = name,
                    BreachLocationX = 375,
                    BreachLocationY = 500,
                    Geometry = new LineString(new[] { new Coordinate(20, 20), new Coordinate(30, 30) }),
                    LeveeBreachFormula = LeveeBreachGrowthFormula.VerheijvdKnaap2002
                };

                leveeBreach.GetActiveLeveeBreachSettings().BreachGrowthActive = false;
                
                var structuresFile = new StructuresFile
                {
                    StructureSchema = schema,
                    ReferenceDate = new DateTime(2000, 1, 1)
                };
                structuresFile.Write(iniFilePath, new[] { leveeBreach });
                Assert.That(File.Exists(iniFilePath));

                var importedStructures = structuresFile.Read(iniFilePath);
                Assert.IsNotEmpty(importedStructures);
                var importedLeveeBreach = importedStructures.FirstOrDefault(s => s.Name == name) as LeveeBreach;
                Assert.NotNull(importedLeveeBreach);

                CompareLeveeBreaches(leveeBreach, importedLeveeBreach);
                CompareLeveeBreachSettingsBase(leveeBreach.GetActiveLeveeBreachSettings(), importedLeveeBreach.GetActiveLeveeBreachSettings());
            }
            finally
            {
                if (File.Exists(iniFilePath)) File.Delete(iniFilePath);
            }
        }

        #region Comparison helper methods for levee breaches
        
        private static void CompareLeveeBreaches(ILeveeBreach expected, ILeveeBreach actual)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.BreachLocationX, actual.BreachLocationX);
            Assert.AreEqual(expected.BreachLocationY, actual.BreachLocationY);
            Assert.AreEqual(expected.Geometry, actual.Geometry);
            Assert.AreEqual(expected.LeveeBreachFormula, actual.LeveeBreachFormula);
        }

        private static void CompareLeveeBreachSettingsBase(LeveeBreachSettings expected, LeveeBreachSettings actual)
        {
            Assert.AreEqual(expected.BreachGrowthActive, actual.BreachGrowthActive);
            Assert.AreEqual(expected.GrowthFormula, actual.GrowthFormula);
            Assert.AreEqual(expected.StartTimeBreachGrowth, actual.StartTimeBreachGrowth);
        }

        private static void CompareLeveeBreachVerheijSettings(VerheijVdKnaap2002BreachSettings expected, VerheijVdKnaap2002BreachSettings actual)
        {
            Assert.AreEqual(expected.InitialCrestLevel, actual.InitialCrestLevel);
            Assert.AreEqual(expected.MinimumCrestLevel, actual.MinimumCrestLevel);
            Assert.AreEqual(expected.InitialBreachWidth, actual.InitialBreachWidth);
            Assert.AreEqual(expected.PeriodToReachZmin, actual.PeriodToReachZmin);
            Assert.AreEqual(expected.Factor1Alfa, actual.Factor1Alfa);
            Assert.AreEqual(expected.Factor2Beta, actual.Factor2Beta);
            Assert.AreEqual(expected.CriticalFlowVelocity, actual.CriticalFlowVelocity);
        }

        private static void CompareLeveeBreachUserDefinedSettings(UserDefinedBreachSettings expected, UserDefinedBreachSettings actual)
        {
            Assert.AreEqual(expected.ManualBreachGrowthSettings.Count, actual.ManualBreachGrowthSettings.Count);

            var actualSettings = actual.ManualBreachGrowthSettings;
            var expectedSettings = expected.ManualBreachGrowthSettings;
            for (var i = 0; i < actualSettings.Count; i++)
            {
                Assert.AreEqual(actualSettings[i].TimeSpan, expectedSettings[i].TimeSpan);
                Assert.AreEqual(actualSettings[i].Height, expectedSettings[i].Height);
                Assert.AreEqual(actualSettings[i].Width, expectedSettings[i].Width);
                Assert.AreEqual(actualSettings[i].Area, expectedSettings[i].Area);
            }
        }

        #endregion

        #endregion

        #region Comparison helper methods for .ini files:

        private static void CompareStructureIniFiles(string iniFilePathA, string iniFilePathB)
        {
            var iniSectionsA = new IniReader().ReadIniFile(iniFilePathA);
            var iniSectionsB = new IniReader().ReadIniFile(iniFilePathB);

            CompareSections(iniSectionsA, iniSectionsB);
        }

        private static void CompareSections(IList<IniSection> iniSectionsA, IList<IniSection> iniSectionsB)
        {
            Assert.AreEqual(iniSectionsA.Count, iniSectionsB.Count, "Expected the same number of INI sections.");
            for (var i = 0; i < iniSectionsA.Count; i++)
            {
                Assert.AreEqual(iniSectionsA[i].Name, iniSectionsB[i].Name, String.Format("Names are not the same at index = {0}.", i));
                CompareProperties(iniSectionsA[i].Properties.ToList(), iniSectionsB[i].Properties.ToList());
            }
        }

        private static void CompareProperties(IList<IniProperty> propertiesA, IList<IniProperty> propertiesB)
        {
            Assert.AreEqual(propertiesA.Count, propertiesB.Count, "Expected the same number of properties.");
            for (var i = 0; i < propertiesA.Count; i++)
            {
                Assert.AreEqual(propertiesA[i].Key.ToLower(), propertiesB[i].Key.ToLower(), String.Format("Names are not the same at index = {0}.", i));
                Assert.AreEqual(propertiesA[i].Value, propertiesB[i].Value, String.Format("Values are not the same at index = {0}.", i));
                // Don't care about comments
            }
        }

        #endregion

        #region Comparison helper methods for structure collections:

        private static void CompareStructures(IList<Structure2D> structures, IList<Structure2D> newStructures)
        {
            Assert.AreEqual(structures.Count, newStructures.Count, "Expected the same number of structures.");
            for (int i = 0; i < structures.Count; i++)
            {
                Assert.AreEqual(structures[i].Structure2DType, newStructures[i].Structure2DType,
                    String.Format("Expected same types at index {0}", i));
                CompareStructureProperties(structures[i].Properties, newStructures[i].Properties);
            }
        }

        private static void CompareStructureProperties(IList<ModelProperty> propertiesA, IList<ModelProperty> propertiesB)
        {
            Assert.AreEqual(propertiesA.Count, propertiesB.Count);
            for (int i = 0; i < propertiesA.Count; i++)
            {
                Assert.AreEqual(propertiesA[i].GetValueAsString(), propertiesB[i].GetValueAsString());

                Assert.AreEqual(propertiesA[i].PropertyDefinition.Caption, propertiesB[i].PropertyDefinition.Caption);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.Category, propertiesB[i].PropertyDefinition.Category);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.DataType, propertiesB[i].PropertyDefinition.DataType);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.DefaultValueAsString, propertiesB[i].PropertyDefinition.DefaultValueAsString);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.Description, propertiesB[i].PropertyDefinition.Description);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.FilePropertyKey, propertiesB[i].PropertyDefinition.FilePropertyKey);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.IsDefinedInSchema, propertiesB[i].PropertyDefinition.IsDefinedInSchema);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.ModelFileOnly, propertiesB[i].PropertyDefinition.ModelFileOnly);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.MaxValueAsString, propertiesB[i].PropertyDefinition.MaxValueAsString);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.MinValueAsString, propertiesB[i].PropertyDefinition.MinValueAsString);
                Assert.AreEqual(propertiesA[i].PropertyDefinition.IsFile, propertiesB[i].PropertyDefinition.IsFile);
            }
        }

        #endregion
    }
}