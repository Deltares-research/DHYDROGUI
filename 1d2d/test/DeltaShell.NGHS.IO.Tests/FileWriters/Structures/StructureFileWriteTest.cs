using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.LeveeBreachFormula;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.Structures
{
    [TestFixture]
    public class StructureFileWriteTest
    {
        private const string structureBcFileName = "FlowFM_structures.bc";
        private static readonly DateTime referenceTime = new DateTime(2018, 8, 25);
        private IEnumerable<IHydroRegion> Regions => new IHydroRegion[] { Network, Area };

        [SetUp]
        public void SetUp()
        {
            Area = new HydroArea();
            Network = Substitute.For<IHydroNetwork>();
        }

        private HydroArea Area { get; set; }
        private IHydroNetwork Network { get; set; }
        private static IEnumerable<TestCaseData> TimeSeriesData()
        {
            TestCaseData GenerateTestData(IStructure structure, 
                                          string fileName,
                                          Action<IHydroNetwork, HydroArea, IStructure> configureAction,
                                          string propertyName,
                                          bool? hasTimeSeries=null)
            {
                var testName = $"{structure.Name}.Use{propertyName}";

                return new TestCaseData(structure, fileName, configureAction)
                    .SetName(testName);
            }

            // Structure2D
            void ConfigurePump2D(IHydroNetwork network, HydroArea area, IStructure pump) =>
                area.Pumps.Add((Pump2D)pump);

            Pump2D pump2DWithTimeSeries =StructureFileTestHelper.CreateDefaultPump2D();
            pump2DWithTimeSeries.UseCapacityTimeSeries = true;
            
            yield return GenerateTestData(pump2DWithTimeSeries, 
                                          structureBcFileName, 
                                          ConfigurePump2D,
                                          nameof(Pump2D.Capacity));

            void ConfigureWeir2D(IHydroNetwork network, HydroArea area, IStructure weir) =>
                area.Weirs.Add((Weir2D)weir);

            Weir2D weir2DWithTimeSeries =StructureFileTestHelper.CreateDefaultWeir2D();
            weir2DWithTimeSeries.CrestWidth = 3.0;
            weir2DWithTimeSeries.WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = 2.0 };
            weir2DWithTimeSeries.UseCrestLevelTimeSeries = true;
            
            yield return GenerateTestData(weir2DWithTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureWeir2D,
                                          nameof(Weir2D.CrestLevel));

            void ConfigureGate2D(IHydroNetwork network, HydroArea area, IStructure gate) =>
                area.Gates.Add((Gate2D)gate);

            Gate2D gate2DWithCrestLevelTimeSeries =StructureFileTestHelper.CreateDefaultGate2D();
            gate2DWithCrestLevelTimeSeries.UseSillLevelTimeSeries = true;
            yield return GenerateTestData(gate2DWithCrestLevelTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureGate2D,
                                          nameof(Gate2D.SillLevel), 
                                          true);

            Gate2D gate2DWithLowerEdgeLevelTimeSeries =StructureFileTestHelper.CreateDefaultGate2D();
            gate2DWithLowerEdgeLevelTimeSeries.UseSillLevelTimeSeries = true;
            yield return GenerateTestData(gate2DWithLowerEdgeLevelTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureGate2D,
                                          nameof(Gate2D.LowerEdgeLevel), 
                                          true);

            Gate2D gate2DWithGateOpeningWidthTimeSeries =StructureFileTestHelper.CreateDefaultGate2D();
            gate2DWithGateOpeningWidthTimeSeries.UseSillLevelTimeSeries = true;
            yield return GenerateTestData(gate2DWithGateOpeningWidthTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureGate2D,
                                          nameof(Gate2D.OpeningWidth), 
                                          true);

            void ConfigureLeveeBreach(IHydroNetwork network, HydroArea area, IStructure leveeBreach) =>
                area.LeveeBreaches.Add((LeveeBreach)leveeBreach);

            LeveeBreach leveeBreachUserDefinedFormula =StructureFileTestHelper.CreateDefaultLeveeBreach();
            leveeBreachUserDefinedFormula.LeveeBreachFormula = LeveeBreachGrowthFormula.UserDefinedBreach;
            leveeBreachUserDefinedFormula.SetBaseLeveeBreachSettings(referenceTime.AddHours(2.0), true);

            var settings = leveeBreachUserDefinedFormula.GetActiveLeveeBreachSettings() as UserDefinedBreachSettings;
            settings?.ManualBreachGrowthSettings.Add(new BreachGrowthSetting
            {
                Width = 2.0,
                Height = 3.0,
                TimeSpan = new TimeSpan(0, 1, 0, 0)
            });
            
            yield return GenerateTestData(leveeBreachUserDefinedFormula,
                                          structureBcFileName,
                                          ConfigureLeveeBreach,
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
            
            yield return GenerateTestData(weir1DWithTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureStructure1D,
                                          nameof(Weir.CrestLevel));

            var pump1DWithTimeSeries = new Pump(nameof(Pump), true) { 
                UseCapacityTimeSeries = true
            };
            
            yield return GenerateTestData(pump1DWithTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureStructure1D,
                                          nameof(Pump.Capacity));

            var orifice1DWithTimeSeries = new Orifice(nameof(Orifice), true) { 
                CrestWidth = 3.0,
                WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = 2.0 },
                UseCrestLevelTimeSeries = true
            };
            
            yield return GenerateTestData(orifice1DWithTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureStructure1D,
                                          nameof(Orifice.CrestLevel));

            var orifice1DWithLowerEdgeLevelTimeSeries = new Orifice(nameof(Orifice), true);
            ((GatedWeirFormula) orifice1DWithLowerEdgeLevelTimeSeries.WeirFormula).UseLowerEdgeLevelTimeSeries = true;
            
            yield return GenerateTestData(orifice1DWithLowerEdgeLevelTimeSeries,
                                          structureBcFileName,
                                          ConfigureStructure1D,
                                          nameof(GatedWeirFormula.LowerEdgeLevel));

            var culvert1DWithTimeSeries = new Culvert(nameof(Culvert))
            {
                UseGateInitialOpeningTimeSeries = true,
            };
            
            yield return GenerateTestData(culvert1DWithTimeSeries,
                                          structureBcFileName,
                                          ConfigureStructure1D,
                                          nameof(Culvert.GateInitialOpening));
        }
        
                private static IEnumerable<TestCaseData> WithoutTimeSeriesData()
        {
            TestCaseData GenerateTestData(IStructure structure, 
                                          string fileName,
                                          Action<IHydroNetwork, HydroArea, IStructure> configureAction,
                                          string propertyName,
                                          bool? hasTimeSeries=null)
            {
                var testName = $"{structure.Name}.Use{propertyName}";

                return new TestCaseData(structure, fileName, configureAction)
                    .SetName(testName);
            }

            // Structure2D
            void ConfigurePump2D(IHydroNetwork network, HydroArea area, IStructure pump) =>
                area.Pumps.Add((Pump2D)pump);

            Pump2D pump2DWithoutTimeSeries =StructureFileTestHelper.CreateDefaultPump2D();
            pump2DWithoutTimeSeries.UseCapacityTimeSeries = false;

            yield return GenerateTestData(pump2DWithoutTimeSeries, 
                                          structureBcFileName, 
                                          ConfigurePump2D,
                                          nameof(Pump2D.Capacity));

            void ConfigureWeir2D(IHydroNetwork network, HydroArea area, IStructure weir) =>
                area.Weirs.Add((Weir2D)weir);
            

            Weir2D weir2DWithoutTimeSeries =StructureFileTestHelper.CreateDefaultWeir2D();
            weir2DWithoutTimeSeries.CrestWidth = 3.0;
            weir2DWithoutTimeSeries.WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = 2.0 };
            weir2DWithoutTimeSeries.UseCrestLevelTimeSeries = false;

            yield return GenerateTestData(weir2DWithoutTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureWeir2D,
                                          nameof(Weir2D.CrestLevel));

            void ConfigureGate2D(IHydroNetwork network, HydroArea area, IStructure gate) =>
                area.Gates.Add((Gate2D)gate);
            

            Gate2D gate2DWithoutCrestLevelTimeSeries =StructureFileTestHelper.CreateDefaultGate2D();
            gate2DWithoutCrestLevelTimeSeries.UseSillLevelTimeSeries = false;
            yield return GenerateTestData(gate2DWithoutCrestLevelTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureGate2D,
                                          nameof(Gate2D.SillLevel));
            

            Gate2D gate2DWithoutLowerEdgeLevelTimeSeries =StructureFileTestHelper.CreateDefaultGate2D();
            gate2DWithoutLowerEdgeLevelTimeSeries.UseSillLevelTimeSeries = false;
            yield return GenerateTestData(gate2DWithoutLowerEdgeLevelTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureGate2D,
                                          nameof(Gate2D.LowerEdgeLevel));
            

            Gate2D gate2DWithoutGateOpeningWidthTimeSeries =StructureFileTestHelper.CreateDefaultGate2D();
            gate2DWithoutGateOpeningWidthTimeSeries.UseSillLevelTimeSeries = false;
            yield return GenerateTestData(gate2DWithoutGateOpeningWidthTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureGate2D,
                                          nameof(Gate2D.OpeningWidth));

            // Structure1D
            void ConfigureStructure1D(IHydroNetwork network, HydroArea area, IStructure structure)
            {
                IStructure1D[] structures = { (IStructure1D)structure };
                network.Structures.Returns(structures);
            }

            var weir1DWithoutTimeSeries = new Weir(nameof(Weir), true) { 
                CrestWidth = 3.0,
                WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = 2.0 },
                UseCrestLevelTimeSeries = false
            };

            yield return GenerateTestData(weir1DWithoutTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureStructure1D,
                                          nameof(Weir.CrestLevel));

            var pump1DWithoutTimeSeries = new Pump(nameof(Pump), true) { 
                UseCapacityTimeSeries = false
            };

            yield return GenerateTestData(pump1DWithoutTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureStructure1D,
                                          nameof(Pump.Capacity));

            var orifice1DWithoutTimeSeries = new Orifice(nameof(Orifice), true) { 
                CrestWidth = 3.0,
                WeirFormula = new SimpleWeirFormula { CorrectionCoefficient = 2.0 },
                UseCrestLevelTimeSeries = false
            };

            yield return GenerateTestData(orifice1DWithoutTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureStructure1D,
                                          nameof(Orifice.CrestLevel));

            var orifice1DWithoutLowerEdgeLevelTimeSeries = new Orifice(nameof(Orifice), true);
            ((GatedWeirFormula) orifice1DWithoutLowerEdgeLevelTimeSeries.WeirFormula).UseLowerEdgeLevelTimeSeries = false;

            yield return GenerateTestData(orifice1DWithoutLowerEdgeLevelTimeSeries, 
                                          structureBcFileName, 
                                          ConfigureStructure1D,
                                          nameof(GatedWeirFormula.LowerEdgeLevel));

            var culvert1DWithoutTimeSeries = new Culvert(nameof(Culvert))
            {
                UseGateInitialOpeningTimeSeries = false,
            };
            yield return GenerateTestData(culvert1DWithoutTimeSeries,
                                          structureBcFileName,
                                          ConfigureStructure1D,
                                          nameof(Culvert.GateInitialOpening));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCaseSource(nameof(TimeSeriesData))]
        [TestCaseSource(nameof(WithoutTimeSeriesData))]
        public void WriteStructureBcFiles_ProvidedStructureWithOrWithoutTimeSeries_ExpectedResultsBcFileIsCreated(IStructure structure, 
                                                                                                  string fileName, 
                                                                                                  Action<IHydroNetwork, HydroArea, IStructure> configureStructure)
        {
            using (var tempDir = new TemporaryDirectory())
            {
                string mduFilePath = Path.Combine(tempDir.Path, "FlowFM.mdu");
                string expectedBcFilePath = NGHSFileBase.GetOtherFilePathInSameDirectory(mduFilePath, 
                                                                                         fileName);
                configureStructure(Network, Area, structure);

                StructureFile.WriteStructureFiles(Regions, mduFilePath, referenceTime);
                Assert.That(File.Exists(expectedBcFilePath), Is.True);
            }
        }
    }
}