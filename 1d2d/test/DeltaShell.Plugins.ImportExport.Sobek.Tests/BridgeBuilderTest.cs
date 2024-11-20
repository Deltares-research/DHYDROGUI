using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.ImportExport.Sobek.Builders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Geometries;
using NUnit.Framework;
using BridgeType = DelftTools.Hydro.Structures.BridgeType;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class BridgeBuilderTest
    {
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            BuildBridgeWithBedLevelTest206b(); // remove overhead from test timings
        }

        [Test]
        public void BuilderCreatesBridge()
        {
            var sobekBridge = new SobekBridge();
            // todo set properties to sobekBridge
            var structure = new SobekStructureDefinition
            {
                Name = "bruggetje",
                Definition = sobekBridge,
                Type = 12, // Bridge
            };

            var builder = new BridgeBuilder(new Dictionary<string, SobekCrossSectionDefinition>());
            var bridges = builder.GetBranchStructures(structure);
            Assert.AreEqual(1, bridges.Count());
            Assert.AreEqual("bruggetje", bridges.First().Name);

            // todo test if properties are correctly converted to bridge
        }

        [Test]
        public void BuildBridgeWithTabulatedCrossSection()
        {
            
            var sobekBridge = new SobekBridge {CrossSectionId = "0"};
            // todo set properties to sobekBridge
            var structureDefinition = new SobekStructureDefinition
            {
                Name = "bruggetje",
                Definition = sobekBridge,
                Type = 12, // Bridge
            };

            var sobekCrossSectionDefinition = new SobekCrossSectionDefinition();
            sobekCrossSectionDefinition.AddTableRow(0.0, 20.0, 10.0);
            sobekCrossSectionDefinition.AddTableRow(1.0, 21.0, 11.0);

            
            var crossSectionDefinitions = new Dictionary<string, SobekCrossSectionDefinition>();
            crossSectionDefinitions["0"] = sobekCrossSectionDefinition;
            var builder = new BridgeBuilder(crossSectionDefinitions);
            

            var bridges = builder.GetBranchStructures(structureDefinition).ToList();
            Assert.AreEqual(1, bridges.Count);

            Bridge actualBridge = bridges.FirstOrDefault();
            
            Assert.IsNotNull(actualBridge);
            Assert.AreEqual(BridgeType.Tabulated, actualBridge.BridgeType);
            Assert.AreEqual(3.0, actualBridge.Height, 1.0e-6);
            Assert.AreEqual(50.0, actualBridge.Width, 1.0e-6);
            Assert.AreEqual(0.0, actualBridge.Shift, 1.0e-6);
            
            ICrossSectionDefinition crossSectionDefinition = actualBridge.GetShiftedCrossSectionDefinition();
            List<Coordinate> flowProfile = crossSectionDefinition.FlowProfile.ToList();

            Assert.AreEqual(CrossSectionType.ZW, crossSectionDefinition.CrossSectionType);
            Assert.AreEqual(21.0, crossSectionDefinition.Width);
            Assert.AreEqual(4, flowProfile.Count);
            Assert.AreEqual(new Coordinate(-5.5, 1.0), flowProfile[0]);
            Assert.AreEqual(new Coordinate(-5.0, 0.0), flowProfile[1]);
            Assert.AreEqual(new Coordinate(5.0, 0.0), flowProfile[2]);
            Assert.AreEqual(new Coordinate(5.5, 1.0), flowProfile[3]);
        }

        [Test]
        public void BuildBridgeWithRectangleCrossSection()
        {
            var sobekBridge = new SobekBridge {CrossSectionId = "0"};
            // todo set properties to sobekBridge
            var structureDefinition = new SobekStructureDefinition
                                          {
                                              Name = "bruggetje",
                                              Definition = sobekBridge,
                                              Type = 12 // Bridge
                                          };

            var sobekCrossSectionDefinition = new SobekCrossSectionDefinition();

            sobekCrossSectionDefinition.AddTableRow(0.0, 21.0, 21.0);
            sobekCrossSectionDefinition.AddTableRow(5.0, 21.0, 21.0);
            //add the fake row for rectangle
            sobekCrossSectionDefinition.AddTableRow(5.0001, 0.0001, 0.0001);
            sobekCrossSectionDefinition.Name = "r_width=20";


            var crossSectionDefinitions = new Dictionary<string, SobekCrossSectionDefinition>();
            crossSectionDefinitions["0"] = sobekCrossSectionDefinition;
            var builder = new BridgeBuilder(crossSectionDefinitions);
            

            var bridges = builder.GetBranchStructures(structureDefinition).ToList();
            Assert.AreEqual(1, bridges.Count);
            
            Bridge actualBridge = bridges.FirstOrDefault();
            
            Assert.IsNotNull(actualBridge);
            Assert.AreEqual(BridgeType.Rectangle, actualBridge.BridgeType);
            Assert.AreEqual(5.0, actualBridge.Height, 1.0e-6);
            Assert.AreEqual(21.0, actualBridge.Width, 1.0e-6);
            Assert.AreEqual(0.0, actualBridge.Shift, 1.0e-6);

            ICrossSectionDefinition crossSectionDefinition = actualBridge.GetShiftedCrossSectionDefinition();
            List<Coordinate> flowProfile = crossSectionDefinition.FlowProfile.ToList();
            
            Assert.AreEqual(CrossSectionType.Standard, crossSectionDefinition.CrossSectionType);
            Assert.AreEqual(21.0, crossSectionDefinition.Width);
            Assert.AreEqual(4, flowProfile.Count);
            Assert.AreEqual(new Coordinate(-10.5, 5.0), flowProfile[0]);
            Assert.AreEqual(new Coordinate(-10.5, 0.0), flowProfile[1]);
            Assert.AreEqual(new Coordinate(10.5, 0.0), flowProfile[2]);
            Assert.AreEqual(new Coordinate(10.5, 5.0), flowProfile[3]);
        }

        [Test]
        public void BuildBridgeWithBedLevelTest206b()
        {
            var sobekBridge = new SobekBridge
                                  {
                                      CrossSectionId = "0",
                                      BridgeType = DeltaShell.Sobek.Readers.SobekDataObjects.BridgeType.SoilBed,
                                      BedLevel = -1.0f
            };
            // todo set properties to sobekBridge
            var structureDefinition = new SobekStructureDefinition
            {
                Name = "bruggetje",
                Definition = sobekBridge,
                Type = 12
            };


            var sobekCrossSectionDefinition = new SobekCrossSectionDefinition();

            sobekCrossSectionDefinition.AddTableRow(0.0, 21.0, 21.0);
            sobekCrossSectionDefinition.AddTableRow(5.0, 21.0, 21.0);
            //add the fake row for rectangle
            sobekCrossSectionDefinition.AddTableRow(5.0001, 0.0001, 0.0001);
            sobekCrossSectionDefinition.Name = "r_width=20";


            var crossSectionDefinitions = new Dictionary<string, SobekCrossSectionDefinition>();
            crossSectionDefinitions["0"] = sobekCrossSectionDefinition;
            var builder = new BridgeBuilder(crossSectionDefinitions);

            var bridges = builder.GetBranchStructures(structureDefinition).ToList();
            Assert.AreEqual(1, bridges.Count);

            Bridge actualBridge = bridges.FirstOrDefault();

            Assert.IsNotNull(actualBridge);
            Assert.AreEqual(BridgeType.Rectangle, actualBridge.BridgeType);
            Assert.AreEqual(5.0, actualBridge.Height, 1.0e-6);
            Assert.AreEqual(21.0, actualBridge.Width, 1.0e-6);
            Assert.AreEqual(-1.0, actualBridge.Shift, 1.0e-6);
            
            ICrossSectionDefinition crossSectionDefinition = actualBridge.GetShiftedCrossSectionDefinition();
            List<Coordinate> flowProfile = crossSectionDefinition.FlowProfile.ToList();
            
            Assert.AreEqual(CrossSectionType.Standard, crossSectionDefinition.CrossSectionType);
            Assert.AreEqual(21.0, crossSectionDefinition.Width);
            Assert.AreEqual(4, flowProfile.Count);
            Assert.AreEqual(new Coordinate(-10.5, 4.0), flowProfile[0]);
            Assert.AreEqual(new Coordinate(-10.5, -1.0), flowProfile[1]);
            Assert.AreEqual(new Coordinate(10.5, -1.0), flowProfile[2]);
            Assert.AreEqual(new Coordinate(10.5, 4.0), flowProfile[3]);
        }

        [Test]
        public void BuildPillarBridge()
        {
            var sobekBridge = new SobekBridge
                                  {
                                      BridgeType = DeltaShell.Sobek.Readers.SobekDataObjects.BridgeType.PillarBridge, 
                                      TotalPillarWidth = 123.45f,
                                      FormFactor = 0.123f
                                  };
            // todo set properties to sobekBridge
            var structureDefinition = new SobekStructureDefinition
            {
                Name = "bruggetje",
                Definition = sobekBridge,
                Type = 12 // Bridge
            };


            var crossSectionDefinitions = new Dictionary<string, SobekCrossSectionDefinition>();
            var builder = new BridgeBuilder(crossSectionDefinitions);

            var bridges = builder.GetBranchStructures(structureDefinition).ToList();
            Assert.AreEqual(1, bridges.Count());

            Bridge pillarBridge = bridges.FirstOrDefault();

            Assert.IsNotNull(pillarBridge);
            Assert.IsTrue(pillarBridge.IsPillar);
            Assert.AreEqual(123.45f, pillarBridge.PillarWidth);
            Assert.AreEqual(0.123f, pillarBridge.ShapeFactor);
        }
    }
}
