using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpTestsEx;
using BridgeType = DelftTools.Hydro.Structures.BridgeType;
using CulvertType = DelftTools.Hydro.CulvertType;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    //TODO: clean out this class. Move modelreadertest to SobekWaterFlowModelReaderTest. This should be about the networkfilereader.
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class SobekNetworkImporterTest
    {
        [Test]
        public void ReadTabulatedCrossSectionDefinition()
        {
            string definitionFile = TestHelper.GetTestDataDirectory() + @"\CrossSection\tabulated.def";

            var crossSectionDefinitionReader = new CrossSectionDefinitionReader();
            IList<SobekCrossSectionDefinition> cs =
                crossSectionDefinitionReader.Read(definitionFile).ToArray();

            Assert.AreEqual(3, cs.Count);

            SobekCrossSectionDefinition sobekCrossSectionDefinition = cs[0];

            //EXAMPLE OK CRDS id 'prof_stadswater2' nm 'stadswater2' ty 0 wm 93 w1 0 w2 0 sw 0 gl 0 gu 0 lt lw
            //CRDS id 'P_NDB_1934' nm 'NDB_MAMO_2' ty 0 wm 1038.16 w1 0 w2 0 sw 9999900000 lt lw PDIN 0 0 '' pdin CLTT 'Level [m]' 'Tot. Width [m]' 'Flow width [m]' cltt CLID '(null)' '(null)' '(null)' clid 
            //TBLE 
            //-30 0.07 0.07 <
            //-25 6.87 6.87 <
            //-24 335.57 335.57 <
            //-23.5 367.53 367.31 <
            //-21.5 390.53 388.74 <
            //-17.5 513.25 507.84 <
            //-16.5 585.67 578.4 <
            //-15 796.72 785.78 <
            //-14.5 825.43 812.96 <
            //-8.5 960.01 923.29 <
            //-7.5 994.84 953.13 <
            //-2.5 1060.09 971.13 <
            //-1.5 1081.87 972.6 <
            //0 1104.59 973.88 <
            //4.5 1150.49 977.05 <
            //5 1223.93 977.05 <
            //5.5 1223.93 1038.16 <
            //tble  gl 0 gu 0 crds

            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, sobekCrossSectionDefinition.Type);
            Assert.AreEqual(17, sobekCrossSectionDefinition.TabulatedProfile.Count);
        }

        [Test]
        public void ReadCSWithAmpersand()
        {
            string mappingFile = TestHelper.GetTestDataDirectory() + @"\CrossSection\crosssectionswithampersand.cr";
            IList<SobekCrossSectionMapping> mappings = new SobekProfileDatFileReader().Read(mappingFile).ToList();

            Assert.AreEqual(16, mappings.Count());
            Assert.IsTrue(mappings[11].LocationId.Contains(@"&"));
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadTabulatedCSWithSummerdike()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\REModels\JAMM2010.sbk\40\Deftop.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            //CRDS id '1001' nm 'Kalkmas1____2.70' ty 0 wm 142 w1 229 w2 0 sw 142 bl 9.9999e+009 lt lw 'Level Width' PDIN 0 0 '' pdin CLTT 'Level [m]' 'Tot. Width [m]' 'Flow width [m]' cltt CLID '(null)' '(null)' '(null)' clid TBLE 
            //40.84 4 4 < 
            //41.71 24 24 < 
            //43.44 105 105 < 
            //43.98 124 120 < 
            //45.27 149 142 < 
            //46.92 149 148 < 
            //47.49 172 150 < 
            //47.99 187 153 < 
            //48.49 196 164 < 
            //49.02 196 175 < 
            //49.66 218 187 < 
            //50.42 325 211 < 
            //51.17 451 291 < 
            //51.74 470 364 < 
            //51.99 473 371 < 
            //tble
            // dk 1 dc 51.17 db 49.67 df 50 dt 0 bw 9.9999e+009 bs 9.9999e+009 aw 9.9999e+009 rd 9.9999e+009 ll 9.9999e+009 rl 9.9999e+009 lw 9.9999e+009 rw 9.9999e+009 crds
            //CRDS id '1002' nm 'Kalkmas1____3.11' ty 0 wm 117 w1 372 w2 0 sw 117 bl 9.9999e+009 lt lw 'Level Width' PDIN 0 0 '' pdin CLTT 'Level [m]' 'Tot. Width [m]' 'Flow width [m]' cltt CLID '(null)' '(null)' '(null)' clid TBLE 
            //39.3 2 2 < 
            //40.58 21 21 < 
            //42.3 63 63 < 
            //43.54 112 110 < 
            //44.12 123 117 < 
            //46.22 132 123 < 
            //46.75 140 125 < 
            //47.23 144 129 < 
            //47.72 149 135 < 
            //48.75 155 144 < 
            //49.37 176 148 < 
            //50.1 370 185 < 
            //50.86 602 335 < 
            //51.28 662 449 < 
            //51.79 670 489 < 
            //tble
            // dk 1 dc 50.86 db 49.36 df 85 dt 5 bw 9.9999e+009 bs 9.9999e+009 aw 9.9999e+009 rd 9.9999e+009 ll 9.9999e+009 rl 9.9999e+009 lw 9.9999e+009 rw 9.9999e+009 crds

            //etc...
        }

        [Test]
        public void ReadSimpleNetwork()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(2, network.Nodes.Count);
            Assert.AreEqual(1, network.Branches.Count);
            // NB last cp is at endnode
            Assert.AreEqual(130, network.Branches[0].Geometry.Coordinates.Length);
            Assert.AreEqual(52, network.CrossSections.Count());
        }

        /// <summary>
        /// Reads the reference
        /// </summary>
        [Test]
        public void ReadReferenceNetwork()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\SW_max_1.lit\3\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(17, network.Nodes.Count);
            Assert.AreEqual(18, network.Branches.Count);
            Assert.AreEqual(20, network.LateralSources.Count());

            Assert.IsTrue(network.Nodes.All(n => n.Network == network));
            Assert.IsTrue(network.Branches.All(n => n.Network == network));
        }

        /// <summary>
        /// Tests if data from cross section is correctly imported
        /// Update: cross sections of unsuprted typoes are still read but will not be added to the network.
        /// </summary>
        [Test]
        public void ReadCrossSectionTypes()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network2\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            ICrossSection yzProfile = network.CrossSections.Where(cs => cs.Name == "33").First();
            // yz profile supported; will be default cross section
            Assert.AreEqual(CrossSectionType.YZ, yzProfile.CrossSectionType);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadNetworkWithRoughnessSections()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\SW_max_1.lit\3\NETWORK.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(4, network.CrossSectionSectionTypes.Count());
            // first 3 sections are reserved for tabulated
            CrossSectionSectionType mainSection = network.CrossSectionSectionTypes[0];

            IEnumerable<ICrossSection> tabulatedProfiles =
                network.CrossSections.Where(cs => cs.CrossSectionType == CrossSectionType.ZW);
            foreach (ICrossSection crossSection in tabulatedProfiles)
            {
                Assert.AreEqual(mainSection.Name, crossSection.Definition.Sections[0].SectionType.Name);
            }

            CrossSectionSectionType firstYzSection = network.CrossSectionSectionTypes[3];
            IEnumerable<ICrossSection> yzProfiles = network.CrossSections.Where(cs => cs.CrossSectionType == CrossSectionType.YZ);
            foreach (ICrossSection crossSection in yzProfiles)
            {
                Assert.IsTrue(firstYzSection.Name == crossSection.Definition.Sections[0].SectionType.Name || mainSection.Name == crossSection.Definition.Sections[0].SectionType.Name);
                //main section will be used if yz cross-section has not been declared in Friction.Dat as CRFR
            }
        }

        [Test]
        public void ReadCrossSectionZW()
        {
            // offset for branchfeature are always relative to the geometry of the branch
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_BfgRhein.sbk\1\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            ICrossSection crossSectionZW = network.CrossSections.First(cs => cs.CrossSectionType == CrossSectionType.ZW);
            var crossSectionZWDef = crossSectionZW.Definition as CrossSectionDefinitionZW;

            Assert.AreEqual("5573755", crossSectionZW.Name);
            Assert.AreEqual(true, crossSectionZWDef.SummerDike.Active);
            Assert.AreEqual(34.8, crossSectionZWDef.SummerDike.FloodPlainLevel);
            Assert.AreEqual(2, crossSectionZWDef.Sections.Count);
            Assert.AreEqual(158.0, crossSectionZWDef.Sections[0].MaxY);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void AllProfileChainagesShouldFitChannelGeometry()
        {
            // offset for branchfeature are always relative to the geometry of the branch
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_BfgRhein.sbk\1\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(0, network.CrossSections.Where(cs => cs.Chainage > cs.Branch.Length).Count());
        }

        [Test]
        public void AllStructureChainagesShouldFitChannelGeometry()
        {
            // offset for branchfeature are always relative to the geometry of the branch
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_NDB.sbk\6\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(0, network.Structures.Where(s => s.Chainage > s.Branch.Length).Count());
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadNetworkWithBridgesAndStructureFrictions()
        {
            //STFR id 'brug_27' ci 'brug_27' mf 3 mt cp 0 30.000 30.000 mr cp 0 30.000 30.000 s1 6 s2 6 sf 3 st cp 0 30.000 30.000 sr cp 0 30.000 stfr
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\SW_max_1.lit\3\NETWORK.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            IBridge bridge = network.Bridges.First(br => br.Name == "brug_27");
            Assert.AreEqual(BridgeFrictionType.StricklerKs, bridge.FrictionType);
            Assert.AreEqual(30, bridge.Friction);
        }

        [Test]
        public void ReadNetworkWithRetentions212()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Retent.lit\2\NETWORK.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(1, network.Retentions.Count());
            Assert.AreEqual(1, network.LateralSources.Count());
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadNetworkWithRetentionsRE()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\REModels\JAMM2010.sbk\5\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(30, network.Retentions.Count());

            foreach (IRetention retention in network.Retentions)
            {
                Assert.AreEqual(100, retention.Branch.Length, 1E-6, "New branch");
                Assert.AreEqual(0, retention.Branch.Source.IncomingBranches.Count, "Incoming new branch");
                Assert.AreEqual(1, retention.Branch.Source.OutgoingBranches.Count, "Outgoing on new branch");
                Assert.Greater(retention.Branch.BranchFeatures.Count, 2, "Structures added on new branch");
            }

            //check the names are correct
            char[] atom = "ABCDEFGHIJKLM".ToCharArray();
            IEnumerable<string> names = atom.Select(c => "004_" + c);
            IEnumerable<string> longNames = atom.Select(c => "Grensms2_" + c);

            IEnumerable<string> branchNames = network.Branches.Select(b => b.Name);
            IEnumerable<string> branchLongNames = network.Channels.Select(c => c.LongName);

            List<string> a = branchNames.ToList();
            List<string> bb = longNames.ToList();

            Assert.IsTrue(names.All(n => branchNames.Contains(n)), "Branch 004_A , 004_B ...to M should exist");
            Assert.IsTrue(longNames.All(n => branchLongNames.Contains(n)), "Grensms3b_A , Grensms3b_B ...to M should exist");
        }

        /// <summary>
        /// Tests if the name of the strcuture is correctly imported from the structure.dat file
        /// </summary>
        [Test]
        public void ReadStructureNames()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\NetworkWithStructures\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            IStructure1D pump = network.Structures.Where(s => s.Name == "158").First(); // river pump id != def id
            Assert.IsInstanceOf<Pump>(pump);

            /*
             river weirs are not yet implemented in kernel
            var weir = network.Structures.Where(s => s.Name == "123").First(); // river weir id != def id
            Assert.IsInstanceOfType(typeof(Weir), weir);*/
            IStructure1D bridge = network.Structures.Where(s => s.Name == "65").First(); // bridge == def id
            Assert.IsInstanceOf<Bridge>(bridge);
        }

        [Test]
        public void ReadStructures()
        {
            string structureLocationFile = TestHelper.GetTestDataDirectory() + @"\vsa.lit\network.st";
            IEnumerable<SobekStructureLocation> structures = new SobekNetworkStructureReader().Read(structureLocationFile);
            Assert.AreEqual(62, structures.Count());

            string structureMappingFile = TestHelper.GetTestDataDirectory() + @"\vsa.lit\struct.dat";
            List<SobekStructureMapping> structureMappings = new SobekStructureDatFileReader().Read(structureMappingFile).ToList();
            Assert.AreEqual(62, structureMappings.Count);
            string structureDefinitionFile = TestHelper.GetTestDataDirectory() + @"\vsa.lit\struct.def";

            var reader = new SobekStructureDefFileReader(SobekType.Sobek212);

            //regular sobek weir
            IEnumerable<SobekStructureDefinition> definitions = reader.Read(structureDefinitionFile);

            // nb not all the structures are active
            Assert.AreEqual(23, definitions.Where(d => d.Type == (int) SobekStructureType.weir).Count());
            Assert.AreEqual(1, definitions.Where(d => d.Type == (int) SobekStructureType.universalWeir).Count());
            Assert.AreEqual(1, definitions.Where(d => d.Type == (int) SobekStructureType.pump).Count());
        }

        [Test]
        public void ReadCompoundStructures()
        {
            string structureLocationFile = TestHelper.GetTestDataDirectory() + @"\compound.lit\2\network.st";
            IEnumerable<SobekStructureLocation> structures = new SobekNetworkStructureReader().Read(structureLocationFile);

            // 1 compound structure with 2 sub structures and 2 'normal' structures
            Assert.AreEqual(3, structures.Count());

            string structureMappingFile = TestHelper.GetTestDataDirectory() + @"\compound.lit\2\struct.dat";
            List<SobekStructureMapping> structureMappings = new SobekStructureDatFileReader().Read(structureMappingFile).ToList();
            Assert.AreEqual(4, structureMappings.Count);

            string structureDefinitionFile = TestHelper.GetTestDataDirectory() + @"\compound.lit\2\struct.def";

            var reader = new SobekStructureDefFileReader(SobekType.Sobek212);

            IEnumerable<SobekStructureDefinition> definitions = reader.Read(structureDefinitionFile);
            Assert.AreEqual(4, definitions.Count());

            Assert.AreEqual(1, definitions.Where(d => d.Type == (int) SobekStructureType.riverWeir).Count());
            Assert.AreEqual(1, definitions.Where(d => d.Type == (int) SobekStructureType.riverPump).Count());
        }

        [Test]
        public void ReadNetworkWithCompoundStructures()
        {
            string networkTopologyFile = TestHelper.GetTestDataDirectory() + @"\compound.lit\2\network.tp";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(networkTopologyFile);

            Assert.AreEqual(2, network.Nodes.Count);
            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(6, network.Structures.Count()); // not reading river weirs yet, not available in the kernel
            Assert.AreEqual(3, network.Structures.Where(s => s is ICompositeBranchStructure).Count());
        }

        [Test]
        public void TestReadNetworkWithCompoundStructures_CompositeStructureNamesAreUnique()
        {
            // we use this test-data since it contains composite structures with non-unique names
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_NDB.sbk\6\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);
            List<ICompositeBranchStructure> compositeStructures = network.CompositeBranchStructures.ToList();

            Assert.IsTrue(compositeStructures.Count > 1);
            Assert.IsTrue(compositeStructures.Select(cbs => cbs.Name).HasUniqueValues());
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadLargeNetwork()
        {
            DateTime t = DateTime.Now;
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\eindhoven\network.tp";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            TimeSpan dt = DateTime.Now - t;

            Assert.AreEqual(20297, network.Nodes.Count);
            Assert.AreEqual(23248, network.Branches.Count);

            //ensure each branch has start and endnodes
            foreach (IBranch branch in network.Branches)
            {
                Assert.IsNotNull(branch.Source);
                Assert.IsNotNull(branch.Target);
            }

            Console.WriteLine("Total time required for reading network: {0}", dt);

            Assert.Less(dt.TotalSeconds, 35, "time to read all (sec)");

            Assert.AreEqual("0-NOORD1", network.Nodes[0].Name);
            Assert.AreEqual(164371.1, ((Point) network.Nodes[0].Geometry).X, 1e-8);

            Assert.AreEqual("1", network.Branches[0].Name);
            Assert.AreEqual(2561.613, network.Branches[0].Length, 0.001);

            Assert.AreEqual("tmp0-EFFLUENT", network.Branches[23247].Name);
            Assert.AreEqual(1, network.Branches[23247].Length, 1e-8);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadNetworkWithChineseNames()
        {
            string path = TestHelper.GetTestDataDirectory() + @"\Tanshui.lit\3\network.tp";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(path);
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ReadPoNetwork()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\POup_GV.lit\7\network.tp";
            var importer = new SobekNetworkImporter();

            HydroNetwork network = null;

            TestHelper.AssertIsFasterThan(28000, () => network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork));

            IEnumerable<IChannel> channelsWithoutCrossSections = network.Channels.Where(c => !c.CrossSections.Any());

            //linkagenode -> branches with same order number: interpolation/extrapolation cross-sections over node
            foreach (IChannel channelWithoutCrossSection in channelsWithoutCrossSections)
            {
                Assert.Greater(
                    network.Channels.Count(c => c.OrderNumber == channelWithoutCrossSection.OrderNumber),
                    1,
                    string.Format("Channel {0} should have the same order number as another branch for extra/interpolation of cross-section", channelWithoutCrossSection.Name)
                );
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadNetworkJkfl2009()
        {
            string path = TestHelper.GetTestDataDirectory() + @"\JKFL2009.LIT\31\NETWORK.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(path);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ReadReferenceNetworkAndClone()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\SW_max_1.lit\3\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            var clonedHydroNetwork = (IHydroNetwork) network.Clone();
            Assert.AreEqual(network.CrossSections.First().Name, clonedHydroNetwork.CrossSections.First().Name);
            clonedHydroNetwork.Nodes.Count.Should().Be.EqualTo(network.Nodes.Count);
            clonedHydroNetwork.Branches.Count.Should().Be.EqualTo(network.Branches.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadNetworkWithBridges()
        {
            string path = TestHelper.GetTestDataDirectory() + @"\BrugRect.lit\1\NETWORK.TP";

            //import existing network and model + default boundary conditions
            IHydroNetwork network = GetNetwork(path);

            List<IBridge> bridges = network.Bridges.ToList();
            Assert.AreEqual(3, bridges.Count);
            IBridge rectangleBridge = bridges[1];
            Assert.AreEqual(BridgeType.Rectangle, rectangleBridge.BridgeType);
            Assert.AreEqual(21, rectangleBridge.Width);
            Assert.AreEqual(5, rectangleBridge.Height);
            Assert.AreEqual(0, rectangleBridge.Shift);
            Assert.AreEqual(FlowDirection.Negative, rectangleBridge.FlowDirection);

            //check the other bridges' directions
            Assert.AreEqual(FlowDirection.Positive, bridges[2].FlowDirection);
            Assert.AreEqual(FlowDirection.Both, bridges[0].FlowDirection);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadNetworkWithAsymTrapezium()
        {
            string path = TestHelper.GetTestDataDirectory() + @"\AsymTrap.lit\1\NETWORK.TP";
            IHydroNetwork network = GetNetwork(path);

            Assert.AreEqual(2, network.CrossSections.Count());
            Assert.IsTrue(network.CrossSections.All(c => c.CrossSectionType == CrossSectionType.YZ));

            ICrossSection firstCrossSection = network.CrossSections.First();
            double[] yValues = ((CrossSectionDefinitionYZ) firstCrossSection.Definition).YZDataTable.Select(r => r.Yq).ToArray();
            double[] zValues = ((CrossSectionDefinitionYZ) firstCrossSection.Definition).YZDataTable.Select(r => r.Z).ToArray();

            Assert.AreEqual(new[]
                            {
                                -4.5,
                                -3.5,
                                -2.5,
                                -1.5,
                                -0.5,
                                0.5,
                                1.5,
                                2.5,
                                3.5,
                                4.5
                            },
                            yValues);

            Assert.AreEqual(new[]
                            {
                                2,
                                2,
                                1,
                                1,
                                0,
                                0,
                                1,
                                1,
                                2,
                                2
                            },
                            zValues);

            //this one is really asymetrical
            ICrossSection lastCrossSection = network.CrossSections.Last();
            yValues = ((CrossSectionDefinitionYZ) lastCrossSection.Definition).YZDataTable.Select(r => r.Yq).ToArray();
            zValues = ((CrossSectionDefinitionYZ) lastCrossSection.Definition).YZDataTable.Select(r => r.Z).ToArray();

            Assert.AreEqual(new[]
                            {
                                -9.5,
                                -8.5,
                                -7.5,
                                -6.5,
                                -5.5,
                                -4.5,
                                -3.5,
                                -2.5,
                                -1.5,
                                -0.5,
                                5.5,
                                9.5
                            },
                            yValues);
            Assert.AreEqual(new[]
                            {
                                2,
                                2,
                                1,
                                1,
                                0,
                                0,
                                1,
                                1,
                                2,
                                2,
                                5,
                                5
                            },
                            zValues);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadNetworkWithCulverts()
        {
            string path = TestHelper.GetTestDataDirectory() + @"\Culverts.lit\1\NETWORK.TP";
            IHydroNetwork network = GetNetwork(path);

            List<ICulvert> culverts = network.Culverts.ToList();
            Assert.AreEqual(15, culverts.Count); // siphon and inverted siphon are not yet implemented in the kernel

            //first the 'normal culvert'
            ICulvert simpleCulvert = culverts[0];
            Assert.AreEqual(5, simpleCulvert.InletLevel);
            Assert.AreEqual(10, simpleCulvert.OutletLevel);
            Assert.AreEqual(10, simpleCulvert.Length);
            Assert.IsFalse(simpleCulvert.IsGated);
            Assert.IsTrue(simpleCulvert.CulvertType.Equals(CulvertType.Culvert));
            Assert.AreEqual(0.7f, simpleCulvert.InletLossCoefficient);
            Assert.AreEqual(1.0, simpleCulvert.OutletLossCoefficient);
            Assert.AreEqual(FlowDirection.Both, simpleCulvert.FlowDirection);
            Assert.AreEqual(CulvertFrictionType.WhiteColebrook, simpleCulvert.FrictionType);
            Assert.AreEqual(0.003, simpleCulvert.Friction);

            //check the crossection-geometry
            Assert.AreEqual(CulvertGeometryType.Tabulated, simpleCulvert.GeometryType);
            Assert.AreEqual(43, simpleCulvert.TabulatedCrossSectionDefinition.ZWDataTable.Count);
            Assert.AreEqual(5.887142, simpleCulvert.TabulatedCrossSectionDefinition.ZWDataTable[3].Z);

            //next a rectangular culvert with a gate.
            ICulvert valvedCulvertWithRectangleCrossSection = culverts[1];
            Assert.IsTrue(valvedCulvertWithRectangleCrossSection.IsGated);
            Assert.AreEqual(2.0, valvedCulvertWithRectangleCrossSection.GateInitialOpening);

            //check the geometry of the culvert..
            Assert.AreEqual(CulvertGeometryType.Rectangle, valvedCulvertWithRectangleCrossSection.GeometryType);
            Assert.AreEqual(5.0, valvedCulvertWithRectangleCrossSection.Height);
            Assert.AreEqual(10.0, valvedCulvertWithRectangleCrossSection.Width);
            Assert.AreEqual(3.1f, valvedCulvertWithRectangleCrossSection.InletLevel);

            //check the gate-opening reduction function
            Assert.AreEqual(11, valvedCulvertWithRectangleCrossSection.GateOpeningLossCoefficientFunction.Arguments[0].Values.Count);
            /*
             // siphon and inverted siphon are not yet implemented in the kernel
            //number 3..the siphon
            var siphon = culverts[2];
            Assert.IsTrue(siphon.CulvertType.Equals(DelftTools.Hydro.CulvertType.Siphon));
            Assert.AreEqual(1.0,siphon.SiphonOnLevel);
            Assert.AreEqual(1.2f, siphon.SiphonOffLevel);
            Assert.AreEqual(0.3f, siphon.BendLossCoefficient);
            
            //number 4..the inverted siphon
            //notice this is NOT a siphon
            var invertedSiphon = culverts[3];
            Assert.IsFalse(invertedSiphon.CulvertType.Equals(DelftTools.Hydro.CulvertType.Siphon));
            Assert.AreEqual(FlowDirection.Negative, invertedSiphon.FlowDirection);
            Assert.AreEqual(3.0, invertedSiphon.BendLossCoefficient);
            */
            // this should be an eggie

            //number 5..the inverted siphon
            //notice this is NOT a siphon
            ICulvert circle = culverts[3];
            Assert.AreEqual(0.1, circle.Diameter, 1.0e-6); // Diameter = 2 * 0.05
        }

        [Test]
        public void ReadNetworkWithGeneralStructure()
        {
            IHydroNetwork network = GetNetwork(TestHelper.GetTestDataDirectory() + @"\GenaStru.lit\1\NETWORK.TP");
            Weir weir = network.Structures.OfType<Weir>().First(w => w.WeirFormula is GeneralStructureWeirFormula);

            //check the parameters of the forumale
            var formula = (GeneralStructureWeirFormula) weir.WeirFormula;
            Assert.AreEqual(1, formula.WidthLeftSideOfStructure);
            Assert.AreEqual(2, formula.BedLevelLeftSideOfStructure);
            Assert.AreEqual(3, formula.WidthStructureLeftSide);
            Assert.AreEqual(4, formula.BedLevelLeftSideStructure);
            Assert.AreEqual(5, formula.WidthStructureCentre);
            Assert.AreEqual(6, formula.BedLevelStructureCentre);
            Assert.AreEqual(7, formula.WidthStructureRightSide);
            Assert.AreEqual(8, formula.BedLevelRightSideStructure);
            Assert.AreEqual(9, formula.WidthRightSideOfStructure);
            Assert.AreEqual(10, formula.BedLevelRightSideOfStructure);
            //gate opening is gateheight minus bedlevel at centre
            Assert.AreEqual(6, formula.GateOpening);

            Assert.AreEqual(0.13, formula.PositiveFreeGateFlow, 0.001);
            Assert.AreEqual(0.14, formula.PositiveDrownedGateFlow, 0.001);
            Assert.AreEqual(0.15, formula.PositiveFreeWeirFlow, 0.001);
            Assert.AreEqual(0.16, formula.PositiveDrownedWeirFlow, 0.001);
            Assert.AreEqual(0.17, formula.PositiveContractionCoefficient, 0.001);
            Assert.AreEqual(0.18, formula.NegativeFreeGateFlow, 0.001);
            Assert.AreEqual(0.19, formula.NegativeDrownedGateFlow, 0.001);
            Assert.AreEqual(0.2, formula.NegativeFreeWeirFlow, 0.001);
            Assert.AreEqual(0.21, formula.NegativeDrownedWeirFlow, 0.001);
            Assert.AreEqual(0.22, formula.NegativeContractionCoefficient, 0.001);
            Assert.AreEqual(0.23, formula.ExtraResistance, 0.001);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void IgnoreRectangularCrossSectionDefinition()
        {
            string definitionFile = TestHelper.GetTestDataDirectory() + @"\SW_max_1.lit\3\profile.def";

            IList<SobekCrossSectionDefinition> sobekCrossSectionDefinitions =
                new CrossSectionDefinitionReader().Read(definitionFile).ToList();

            //IList<SobekCrossSectionDefinition> sobekCrossSectionDefinitions = ProfileDefFileScanner.ReadCrossSectionLayer(definitionFile);

            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\SW_max_1.lit\3\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork);

            // profile.def 17 has name that starts with r_; it should be skipped
            Assert.AreEqual(1, sobekCrossSectionDefinitions.Count(cs => cs.ID == "17"));
            SobekCrossSectionDefinition profileDefinition = sobekCrossSectionDefinitions.First(cs => cs.ID == "17");
            Assert.AreEqual("r_DuikerLemmelerveld", profileDefinition.Name);

            // check if other cross section definitions are processed.
            Assert.AreEqual(1, sobekCrossSectionDefinitions.Count(cs => cs.ID == "prof_D20061011-DP-144"));
            Assert.AreEqual(1, network.CrossSections.Count(cs => cs.Name == "prof_D20061011-DP-144"));
        }

        private IHydroNetwork GetNetwork(string path)
        {
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork) importer.ImportItem(path);
            return network;
        }
    }
}