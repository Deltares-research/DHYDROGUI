using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.ImportExport.Sobek.Builders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class WeirBuilderTest
    {
        private WeirBuilder builder;

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            builder = new WeirBuilder(new Dictionary<string, SobekCrossSectionDefinition>());
        }

        [Test]
        public void WeirHasSameCrestLevelAsInImportObject()
        {
            const float expectedCrestLevel = 10;

            var structure = new SobekWeir
                                {
                                    CrestLevel = expectedCrestLevel,
                                    
                                };

            var structureDefinition = new SobekStructureDefinition
            {
                Definition = structure,
                Type = (int)SobekStructureType.weir
            };

            Weir actualWeir = builder.GetBranchStructures(structureDefinition).FirstOrDefault();

            Assert.AreEqual(expectedCrestLevel, actualWeir.CrestLevel);
        }

        [Test]
        public void WeirHasSameCrestWidthAsInImportObject()
        {
            const float expectedCrestWidth = 10;

            var structure = new SobekWeir() { CrestWidth = expectedCrestWidth };
            var structureDefinition = new SobekStructureDefinition
            {
                Name = "Weir",
                Definition = structure,
                Type = (int)SobekStructureType.weir
            };

            Weir actualWeir = builder.GetBranchStructures(structureDefinition).FirstOrDefault();

            Assert.AreEqual(expectedCrestWidth, actualWeir.CrestWidth);
        }
        
        [Test]
        public void UseAsBranchStructureBuilder()
        {
            IBranchStructureBuilder builder = new WeirBuilder(null);
            var structs = builder.GetBranchStructures(new SobekStructureDefinition{ Type = 66 });
            Assert.AreEqual(0,structs.Count());
        }

        [Test]
        public void WeirToSimpleWeirConversionWorksCorrectly()
        {
            var structure = new SobekWeir()
            {
                CrestLevel = 12, 
                CrestWidth = 13, 
                DischargeCoefficient = 14, 
                LateralContractionCoefficient = 1.1f
            };

            var structureDefinition = new SobekStructureDefinition
            {
                Definition = structure,
                Type = (int)SobekStructureType.weir
            };

            var weirs = builder.GetBranchStructures(structureDefinition);
            Assert.AreEqual(1, weirs.Count());

            Weir actualWeir = weirs.FirstOrDefault();
            Assert.IsNotNull(actualWeir);

            // Test common weir property values
            Assert.AreEqual(structure.CrestLevel, actualWeir.CrestLevel);
            Assert.AreEqual(structure.CrestWidth, actualWeir.CrestWidth);

            var actualFormula = actualWeir.WeirFormula as SimpleWeirFormula;

            Assert.IsNotNull(actualFormula);
            Assert.AreEqual(structure.DischargeCoefficient * structure.LateralContractionCoefficient, actualFormula.CorrectionCoefficient);           
        }

        [Test]
        public void CanBuildGatedWeirFromOrifice()
        {
            var orifice = new SobekOrifice();
            orifice.ContractionCoefficient = 0.1f;
            orifice.CrestLevel = 0.2f;
            orifice.CrestWidth = 0.3f;
            orifice.FlowDirection = 1;
            orifice.GateHeight = 1.7f;
            orifice.LateralContractionCoefficient = 0.6f;
            orifice.MaximumFlowNeg = 0.7f;
            orifice.MaximumFlowPos = 0.8f;

            var structureDefinition = new SobekStructureDefinition
                                          {
                                              Definition = orifice,
                                              Type = (int) SobekStructureType.orifice
                                          };

            builder = new WeirBuilder(null);
            var weirs = builder.GetBranchStructures(structureDefinition);
            Assert.AreEqual(1, weirs.Count());

            Weir weir = weirs.FirstOrDefault();
            //check weir
            Assert.AreEqual(0.2f, weir.CrestLevel);
            Assert.AreEqual(0.3f, weir.CrestWidth);
            //check formula
            var formula = (GatedWeirFormula) weir.WeirFormula;
            Assert.AreEqual(FlowDirection.Positive, weir.FlowDirection);
            Assert.AreEqual(0.06, formula.ContractionCoefficient, 0.001);
            Assert.AreEqual(1.5f, formula.GateOpening, 0.001);
            Assert.AreEqual(1.0f, formula.LateralContraction);
            Assert.AreEqual(0.7f, formula.MaxFlowNeg);
            Assert.AreEqual(0.8f, formula.MaxFlowPos);
        }

        [Test]
        public void CanBuildFreeFormWeir()
        {
            // hack; when tests are run insequence builder is broken
            builder = new WeirBuilder(new Dictionary<string, SobekCrossSectionDefinition>());
            //var structure = new SobekWeir();
            var structure = new SobekUniversalWeir {CrossSectionId = "0"};
            var structureDefinition = new SobekStructureDefinition
            {
                Definition = structure,
                Type = (int)SobekStructureType.universalWeir
            };
            var weirs = builder.GetBranchStructures(structureDefinition);
            Assert.AreEqual(1, weirs.Count());

            Weir actualWeir = weirs.FirstOrDefault(); 
            
            IWeirFormula actualWeirFormula = actualWeir.WeirFormula;
            Type expectedType = typeof(FreeFormWeirFormula);
            Assert.IsInstanceOf(expectedType, actualWeirFormula);
        }

        [Test]
        public void CanBuildFreeFormWeirWithCrossSection()
        {
            // hack; when tests are run in sequence builder is broken
            builder = new WeirBuilder(new Dictionary<string, SobekCrossSectionDefinition>());
            //var structure = new SobekWeir();
            var structure = new SobekUniversalWeir { CrossSectionId = "0" };
            var structureDefinition = new SobekStructureDefinition
            {
                Definition = structure,
                Type = (int)SobekStructureType.universalWeir
            };

            var sobekCrossSectionDefinition = new SobekCrossSectionDefinition();
            sobekCrossSectionDefinition.YZ.Add(new Coordinate(0.0, 0.3));
            sobekCrossSectionDefinition.YZ.Add(new Coordinate(1.0, 0.3));

            builder.SobekCrossSectionDefinitions["0"] = sobekCrossSectionDefinition;
            
            var weirs = builder.GetBranchStructures(structureDefinition);
            Assert.AreEqual(1, weirs.Count());

            Weir actualWeir = weirs.FirstOrDefault();

            FreeFormWeirFormula freeFormWeirFormula = (FreeFormWeirFormula)actualWeir.WeirFormula;
            int count = sobekCrossSectionDefinition.YZ.Count;
            Assert.AreEqual(2, count);
            Assert.AreEqual(count, freeFormWeirFormula.Y.Count());
            Assert.AreEqual(count, freeFormWeirFormula.Z.Count());
        }

        [Test]
        public void CanBuildFreeFormWeirWithCrossSectionAndCrestLevelShift()
        {
            // hack; when tests are run in sequence builder is broken
            builder = new WeirBuilder(new Dictionary<string, SobekCrossSectionDefinition>());
            //var structure = new SobekWeir();
            var structure = new SobekUniversalWeir { CrossSectionId = "0", CrestLevelShift = 1.7F };
            var structureDefinition = new SobekStructureDefinition
            {
                Definition = structure,
                Type = (int)SobekStructureType.universalWeir
            };

            var sobekCrossSectionDefinition = new SobekCrossSectionDefinition();
            sobekCrossSectionDefinition.YZ.Add(new Coordinate(0.0, 0.3));
            sobekCrossSectionDefinition.YZ.Add(new Coordinate(1.0, -0.3));

            builder.SobekCrossSectionDefinitions["0"] = sobekCrossSectionDefinition;
            var weirs = builder.GetBranchStructures(structureDefinition);
            var weir = weirs.FirstOrDefault();
            var freeFormWeirFormula = (FreeFormWeirFormula)weir.WeirFormula;
            var zetjes = freeFormWeirFormula.Z.ToArray();
            Assert.AreEqual(2.0, zetjes[0], 1.0e-6);
            Assert.AreEqual(1.4, zetjes[1], 1.0e-6);
            // (for freeformweir crestlevel is the actual lowest Z -> 1.7 + -0.3 = 1.4)
            // Not any more. FM crest level shift is the Sobek2 crest level shift (FM1D2D-1023)
            // It is now again (FM1D2D-1170)
            Assert.AreEqual(1.4, weir.CrestLevel, 1.0e-6);
        }

        [Test]
        public void CanBuildSimpleWeir()
        {
            var structure = new SobekWeir();
            var structureDefinition = new SobekStructureDefinition
            {
                Name = "SimpleFormWeir (Weir)",
                Definition = structure,
                Type = (int)SobekStructureType.weir
            };

            var weirs = builder.GetBranchStructures(structureDefinition);
            Assert.AreEqual(1, weirs.Count());

            Weir actualWeir = weirs.FirstOrDefault(); 
            IWeirFormula actualWeirFormula = actualWeir.WeirFormula;
            Type expectedType = typeof(SimpleWeirFormula);
            Assert.IsInstanceOf(expectedType, actualWeirFormula);
        }

        [Test]
        public void BuildGeneralStructure()
        {
            var structure = new SobekGeneralStructure();
            
            var structureDefinition = new SobekStructureDefinition
            {
                Definition = structure,
                Type = (int)SobekStructureType.generalStructure
            };
            var weirs = builder.GetBranchStructures(structureDefinition);
            Assert.AreEqual(1, weirs.Count());


        }

        [Test]
        public void ReturnsEmptyEnumerableForUnknownType()
        {
            var structure = new SobekWeir();
            var structureDefinition = new SobekStructureDefinition
            {
                Definition = structure,
                Type = 100
            };

            builder = new WeirBuilder(null);
            IEnumerable<Weir> structures = builder.GetBranchStructures(structureDefinition);
            Assert.AreEqual(0,structures.Count());
            
        }

        [Test]
        public void GateOpeningGeneralStructureFromRE()
        {

            var structure = new SobekGeneralStructure();
            structure.GateHeight = (float) 10.0;
            structure.BedLevelStructureCentre = (float)4.0;
            structure.ImportFromRE = true;


            var structureDefinition = new SobekStructureDefinition
                                          {
                                              Definition = structure,
                                              Type = (int) SobekStructureType.generalStructure,
                                          };
            var weirs = builder.GetBranchStructures(structureDefinition);

            Assert.AreEqual(1, weirs.Count());

            var generalStructureWeirFormula = (GeneralStructureWeirFormula) weirs.First().WeirFormula;
            Assert.AreEqual(10.0, generalStructureWeirFormula.GateOpening);
        }

        [Test]
        public void GateOpeningGeneralStructureFrom212()
        {

            var structure = new SobekGeneralStructure();
            structure.GateHeight = (float)10.0;
            structure.BedLevelStructureCentre = (float)4.0;
            structure.ImportFromRE = false;


            var structureDefinition = new SobekStructureDefinition
            {
                Definition = structure,
                Type = (int)SobekStructureType.generalStructure,
            };
            var weirs = builder.GetBranchStructures(structureDefinition);

            Assert.IsFalse(structure.ImportFromRE);
            Assert.AreEqual(1, weirs.Count());

            var generalStructureWeirFormula = (GeneralStructureWeirFormula)weirs.First().WeirFormula;
            Assert.AreEqual(6.0, generalStructureWeirFormula.GateOpening);
        }



    }
}