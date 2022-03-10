using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.ImportExport.Sobek.Builders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
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
            

            var bridges = builder.GetBranchStructures(structureDefinition);
            Assert.AreEqual(1, bridges.Count());

            Bridge actualBridge = bridges.FirstOrDefault();

            Assert.AreEqual(CrossSectionType.ZW, actualBridge.EffectiveCrossSectionDefinition.CrossSectionType);
            Assert.AreEqual(BridgeType.Tabulated,actualBridge.BridgeType);

            int count = actualBridge.EffectiveCrossSectionDefinition.ZWDataTable.Count;

            Assert.AreEqual(2, count);
            Assert.AreEqual(1.0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[0].Z, 1.0e-6);
            Assert.AreEqual(21.0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[0].Width, 1.0e-6);
            Assert.AreEqual(10.0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[0].StorageWidth, 1.0e-6);
            Assert.AreEqual(0.0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[1].Z, 1.0e-6);
            Assert.AreEqual(20.0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[1].Width, 1.0e-6);
            Assert.AreEqual(10.0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[1].StorageWidth, 1.0e-6);
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
            

            var bridges = builder.GetBranchStructures(structureDefinition);
            Assert.AreEqual(1, bridges.Count());

            Bridge actualBridge = bridges.FirstOrDefault();

            //this is actually always tabulated
            Assert.AreEqual(CrossSectionType.ZW, actualBridge.EffectiveCrossSectionDefinition.CrossSectionType);

            Assert.AreEqual(BridgeType.Rectangle, actualBridge.BridgeType);

            int count = actualBridge.EffectiveCrossSectionDefinition.ZWDataTable.Count;

            Assert.AreEqual(2, count);
            Assert.AreEqual(5.0, actualBridge.Height, 1.0e-6);
            Assert.AreEqual(21.0, actualBridge.Width, 1.0e-6);
            Assert.AreEqual(0.0, actualBridge.Shift, 1.0e-6);

            //check the bridge geometry
            Assert.AreEqual(2, count);
            Assert.AreEqual(5.0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[0].Z, 1.0e-6);
            Assert.AreEqual(21.0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[0].Width, 1.0e-6);
            Assert.AreEqual(0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[0].StorageWidth, 1.0e-6);
            Assert.AreEqual(0.0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[1].Z, 1.0e-6);
            Assert.AreEqual(21.0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[1].Width, 1.0e-6);
            Assert.AreEqual(0, actualBridge.EffectiveCrossSectionDefinition.ZWDataTable[1].StorageWidth, 1.0e-6);

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

            var bridges = builder.GetBranchStructures(structureDefinition);
            Assert.AreEqual(1, bridges.Count());

            Bridge bridge = bridges.First();

            Assert.IsNotNull(bridge);
            Assert.AreEqual(BridgeType.Rectangle, bridge.BridgeType);

            //check the bridge geometry (+ -1 of BottemLevel)
            Assert.AreEqual(4.0, bridge.EffectiveCrossSectionDefinition.ZWDataTable[0].Z, 1.0e-6);
            Assert.AreEqual(21.0, bridge.EffectiveCrossSectionDefinition.ZWDataTable[0].Width, 1.0e-6);
            Assert.AreEqual(0, bridge.EffectiveCrossSectionDefinition.ZWDataTable[0].StorageWidth, 1.0e-6);
            Assert.AreEqual(-1.0, bridge.EffectiveCrossSectionDefinition.ZWDataTable[1].Z, 1.0e-6);
            Assert.AreEqual(21.0, bridge.EffectiveCrossSectionDefinition.ZWDataTable[1].Width, 1.0e-6);
            Assert.AreEqual(0, bridge.EffectiveCrossSectionDefinition.ZWDataTable[1].StorageWidth, 1.0e-6);
        }

        [Test]
        [Ignore("not yet implemented in the kernel")]
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

            var bridges = builder.GetBranchStructures(structureDefinition);
            Assert.AreEqual(1, bridges.Count());

            Bridge pillarBridge = bridges.FirstOrDefault();

            Assert.IsNotNull(pillarBridge);
            Assert.IsTrue(pillarBridge.IsPillar);
            Assert.AreEqual(123.45f, pillarBridge.PillarWidth);
            Assert.AreEqual(0.123f, pillarBridge.ShapeFactor);
        }
    }
}
