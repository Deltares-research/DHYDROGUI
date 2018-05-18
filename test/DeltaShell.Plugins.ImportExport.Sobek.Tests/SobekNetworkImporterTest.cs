using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using DeltaShell.Sobek.Readers;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Coverages;
using log4net;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpTestsEx;
using BridgeType = DelftTools.Hydro.Structures.BridgeType;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    //TODO: clean out this class. Move modelreadertest to SobekWaterFlowModelReaderTest. This should be about the networkfilereader.
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class SobekNetworkImporterTest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (SobekNetworkImporterTest));

        [SetUp]
        public void SetUp()
        {
            LogHelper.ResetLogging(); // NOTE: set it back before commit! Otherwise HUGE log files are generated
        }

        [Test]
        public void ReadTabulatedCrossSectionDefinition()
        {
            string definitionFile = TestHelper.GetDataDir() + @"\CrossSection\tabulated.def";
            
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
            Assert.AreEqual(17,sobekCrossSectionDefinition.TabulatedProfile.Count);
 
        }

        [Test]
        public void ReadCSWithAmpersand()
        {
            string mappingFile = TestHelper.GetDataDir() + @"\CrossSection\crosssectionswithampersand.cr";
            IList<SobekCrossSectionMapping> mappings = new SobekProfileDatFileReader().Read(mappingFile).ToList();

            Assert.AreEqual(16, mappings.Count());
            Assert.IsTrue(mappings[11].LocationId.Contains(@"&"));
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadTabulatedCSWithSummerdike()
        {
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\REModels\JAMM2010.sbk\40\Deftop.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

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
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\network1\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

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
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\SW_max_1.lit\3\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(17, network.Nodes.Count);
            Assert.AreEqual(18, network.Branches.Count);
            Assert.AreEqual(20, network.LateralSources.Count());
        
            Assert.IsTrue(network.Nodes.All(n=>n.Network == network));
            Assert.IsTrue(network.Branches.All(n => n.Network == network));
        }
        
        /// <summary>
        /// Tests if data from cross section is correctly imported
        /// Update: cross sections of unsuprted typoes are still read but will not be added to the network.
        /// </summary>
        [Test]
        public void ReadCrossSectionTypes()
        {
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\network2\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            //ICrossSection trapeziumProfile = network.CrossSections.Where(cs => cs.Name == "4").First();
            //// trapzium not supported; will be default GeometryBased cross section
            //Assert.AreEqual(CrossSectionType.GeometryBased, trapeziumProfile.CrossSectionType);

            //ICrossSection circleProfile = network.CrossSections.Where(cs => cs.Name == "30").First();
            //// circle not supported; will be default GeometryBased cross section
            //Assert.AreEqual(CrossSectionType.GeometryBased, circleProfile.CrossSectionType);

            var yzProfile = network.CrossSections.Where(cs => cs.Name == "33").First();
            // yz profile supported; will be default cross section
            Assert.AreEqual(CrossSectionType.YZ, yzProfile.CrossSectionType);

            //ICrossSection tabulatedProfile = network.CrossSections.Where(cs => cs.Name == "32").First();
            // definition name is r_Default -> closed rectangle in disguise
            //// tabulated profile supported; will be default cross section
            //Assert.AreEqual(CrossSectionType.ZW, tabulatedProfile.CrossSectionType);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadNetworkWithRoughnessSections()
        {
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\SW_max_1.lit\3\NETWORK.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(4, network.CrossSectionSectionTypes.Count());
            // first 3 sections are reserved for tabulated
            var mainSection = network.CrossSectionSectionTypes[0];
            //network.CrossSections.All(cs => Assert.AreEqual(mainSection, cs.CrossSectionType));

            var tabulatedProfiles =
                network.CrossSections.Where(cs => cs.CrossSectionType == CrossSectionType.ZW);
            foreach (var crossSection in tabulatedProfiles)
            {
                Assert.AreEqual(mainSection.Name, crossSection.Definition.Sections[0].SectionType.Name);
            }
            var firstYzSection = network.CrossSectionSectionTypes[3];
            var yzProfiles = network.CrossSections.Where(cs => cs.CrossSectionType == CrossSectionType.YZ);
            foreach (var crossSection in yzProfiles)
            {
                Assert.IsTrue(firstYzSection.Name == crossSection.Definition.Sections[0].SectionType.Name || mainSection.Name == crossSection.Definition.Sections[0].SectionType.Name);
                //main section will be used if yz cross-section has not been declared in Friction.Dat as CRFR
            }
        }

        [Test]
        public void ReadRiverProfileWithMainChannelAnFloodplain1()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\ReModels\20110331_BfgRhein.sbk\1\DEFTOP.1";
            var importer = new SobekWaterFlowModel1DImporter();
            var rtcMmodel = ((ICompositeActivity) importer.ImportItem(pathToSobekNetwork)).Activities.OfType<RealTimeControlModel>().First();
            var model = rtcMmodel.ControlledModels.OfType<WaterFlowModel1D>().First();
            var network = model.Network;
            var heightFlowStorageWidthCrossSections = network.CrossSections.Where(cs => cs.CrossSectionType == CrossSectionType.ZW);
            var crossSection = heightFlowStorageWidthCrossSections.First(cs => cs.Name == "5573755");

            // DEFCRS.1 cross sectiojn name -> carrier id = channel
            // CRSN id '5573822' nm 'RUHR_LIP_780.62' ci 'AL2_06' lc 618 crsn
            // DEFCRS.3 cross sectiojn name -> definition id
            // CRSN id '5573822' di '1309' rl 0 us 1e+010 ds 1e+010 crsn
            // DEFCRS.2
            // CRDS id '1309' nm 'RUHR_LIP_780.62' ty 0 wm 287 w1 638 w2 0 sw 287 bl 9.9999e+009 lt lw 'Level Width' PDIN 0 0 '' pdin CLTT 'Level [m]' 'Tot. Width [m]' 'Flow width [m]' cltt CLID '(null)' '(null)' '(null)' clid TBLE 
            //- width main channel = 287
            //- width floodplain1 = 638
            //- width floodplain2 = 0
            //- h  total flow
            // 14.33 112 112 < 
            // 14.71 262 249 < 
            // 14.97 307 265 < 
            // 15.22 2809 271 < 
            // 16.48 2825 287 < 
            // 17.73 2835 297 < 
            // 18.05 2837 299 < 
            // 23.74 2869 433 < 
            // 24.03 3464 441 < 
            // 27.72 4680 877 < 
            // 27.93 5401 877 < 
            // 28.54 5519 877 < 
            // 29.06 5987 877 < 
            // 29.79 6938 924 < 
            // 30.03 6938 925 < 
            // tble
            //  dk 1 dc 24.31 db 22.81 df 132 dt 801 bw 9.9999e+009 bs 9.9999e+009 aw 9.9999e+009 rd 9.9999e+009 ll 9.9999e+009 rl 9.9999e+009 lw 9.9999e+009 rw 9.9999e+009 crds
            //
            // friction is given as BDFR in channel AL2_06
            // DEFFRC.1
            // BDFR id '5574774' nm '(null)' ci 'AL2_06' em 1 er 0 e1 1 e2 0 e3 0 e4 0 mf 3 mt fq 4 26 9.9999e+009 'Coefficient Discharge' PDIN 0 0 '' 
            //        -> pdin CLTT 'Q' '0' '7000' '12000' '34524' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE 
            // - main channel mt fq 4 = function of location and q (discharge)
            // 700 42 40 42 41 < 
            // 850 40 37 37.5 40 < 
            // 1400 36 36 36 40 < 
            // 2000 33 36 37 40 < 
            // 2160 33 34 36.5 37 < 
            // 2800 33.5 34 37.5 37 < 
            // 3800 34 34 40 37 < 
            // 5300 34 33 42 37 < 
            // 5600 34 33 42 38 < 
            // 7400 34.5 35 42 41 < 
            // 10150 35 36 42 42 < 
            // 10390 36 36 41.5 42 < 
            // 11300 36 36 40 42 < 
            // 11600 36 36 40 43 < 
            // 11850 36 36 40 44 < 
            // 12000 36 36 40 45 < 

            // 3 sections : floodplain1 main floodplain1
            Assert.AreEqual(2, crossSection.Definition.Sections.Count);
            var branch = network.Branches.Where(b => b.Name == "AL2_06").FirstOrDefault();
            var roughnessSections = model.RoughnessSections;
            var main = roughnessSections.Where(r => r.Name == "Main").FirstOrDefault();
            var mainFunctionofQ = main.FunctionOfQ(branch);
            Assert.AreEqual(4, mainFunctionofQ.Arguments[0].Values.Count);
            Assert.AreEqual(16, mainFunctionofQ.Arguments[1].Values.Count);
            Assert.AreEqual(42.0, (double)mainFunctionofQ[0.0, 700.0], 1.0e-6);
            Assert.AreEqual(45.0, (double)mainFunctionofQ[34524.0, 12000.0], 1.0e-6);

            // continued...
            // tble
            //  mr fq 4 26 9.9999e+009 'Coefficient Discharge' PDIN 0 0 '' pdin CLTT 'Q' '0' '7000' '12000' '34524' cltt CLID '(null)' '(null)' '(null)' '(null)' '(null)' clid TBLE 
            // 700 42 40 42 41 < 
            // 850 40 37 37.5 40 < 
            // 1400 36 36 36 40 < 
            // 2000 33 36 37 40 < 
            // 2160 33 34 36.5 37 < 
            // 2800 33.5 34 37.5 37 < 
            // 3800 34 34 40 37 < 
            // 5300 34 33 42 37 < 
            // 5600 34 33 42 38 < 
            // 7400 34.5 35 42 41 < 
            // 10150 35 36 42 42 < 
            // 10390 36 36 41.5 42 < 
            // 11300 36 36 40 42 < 
            // 11600 36 36 40 43 < 
            // 11850 36 36 40 44 < 
            // 12000 36 36 40 45 < 
            // tble

            var fp1 = roughnessSections.Where(r => r.Name == "FloodPlain1").FirstOrDefault();
            Assert.AreEqual(RoughnessFunction.Constant, fp1.GetRoughnessFunctionType(branch));
            Assert.AreEqual(1.4, fp1.RoughnessNetworkCoverage.EvaluateRoughnessValue(new NetworkLocation(branch, 0.0)));
            Assert.AreEqual(RoughnessType.WhiteColebrook, fp1.RoughnessNetworkCoverage.EvaluateRoughnessType(new NetworkLocation(branch, 0.0)));

            //  s1 4 c1 cp 2 9.9999e+009 9.9999e+009 'Nikuradse Coefficient' PDIN 1 0 '' pdin CLTT 'Feature' 'Coefficient' cltt CLID '(null)' '(null)' clid TBLE 
            // - flood plain 1: s1 4 c1 cp 2 = function of location only (cp 0 is constant)
            // 0 1.4 < 
            // 2729 0.5 < 
            // 5457 0.5 < 
            // 8186 0.8 < 
            // 10914 0.6 < 
            // 13643 0.5 < 
            // 16371 3 < 
            // 19100 1.7 < 
            // 21828 1.4 < 
            // 24557 1.8 < 
            // 27285 2.5 < 
            // 32742 1.5 < 
            // tble
            //  r1 cp 2 9.9999e+009 9.9999e+009 'Nikuradse Coefficient' PDIN 1 0 '' pdin CLTT 'Feature' 'Coefficient' cltt CLID '(null)' '(null)' clid TBLE 
            // 0 1.4 < 
            // 2729 0.5 < 
            // 5457 0.5 < 
            // 8186 0.8 < 
            // 10914 0.6 < 
            // 13643 0.5 < 
            // 16371 3 < 
            // 19100 1.7 < 
            // 21828 1.4 < 
            // 24557 1.8 < 
            // 27285 2.5 < 
            // 32742 1.5 < 
            // tble
            // - flood plain 2 = irrelevant and same as main
            //  s2  6 c2 cp 0 9.9999e+009 9.9999e+009 r2 cp 0 9.9999e+009 9.9999e+009 d9 f9 0 9.9999e+009 9.9999e+009 bdfr

            var fp2 = roughnessSections.Where(r => r.Name == "FloodPlain2").FirstOrDefault();

            var fp2FunctionofQ = fp2.FunctionOfQ(branch);
            Assert.AreEqual(4, fp2FunctionofQ.Arguments[0].Values.Count);
            Assert.AreEqual(16, fp2FunctionofQ.Arguments[1].Values.Count);
            Assert.AreEqual(42.0, (double)fp2FunctionofQ[0.0, 700.0], 1.0e-6);
            Assert.AreEqual(45.0, (double)fp2FunctionofQ[34524.0, 12000.0], 1.0e-6);
        }

        [Test]
        public void ReadCrossSectionZW()
        {
            // offset for branchfeature are always relative to the geometry of the branch
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\ReModels\20110331_BfgRhein.sbk\1\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            var crossSectionZW = network.CrossSections.First(cs => cs.CrossSectionType == CrossSectionType.ZW);
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
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\ReModels\20110331_BfgRhein.sbk\1\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(0, network.CrossSections.Where(cs => cs.Chainage > cs.Branch.Length).Count());
        }

        [Test]
        public void AllStructureChainagesShouldFitChannelGeometry()
        {
            // offset for branchfeature are always relative to the geometry of the branch
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\ReModels\20110331_NDB.sbk\6\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(0, network.Structures.Where(s => s.Chainage > s.Branch.Length).Count());
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadNetworkWithBridgesAndStructureFrictions()
        {
            //STFR id 'brug_27' ci 'brug_27' mf 3 mt cp 0 30.000 30.000 mr cp 0 30.000 30.000 s1 6 s2 6 sf 3 st cp 0 30.000 30.000 sr cp 0 30.000 stfr
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\SW_max_1.lit\3\NETWORK.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            var bridge = network.Bridges.First(br => br.Name == "brug_27");
            Assert.AreEqual(StructureFrictionType.StricklerKs, (StructureFrictionType)bridge.FrictionType);
            Assert.AreEqual(30, bridge.Friction);

        }

        [Test]
        public void ReadNetworkWithRetentions212()
        {
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\Retent.lit\2\NETWORK.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(1, network.Retentions.Count());
            Assert.AreEqual(1, network.LateralSources.Count());
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadNetworkWithRetentionsRE()
        {
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\REModels\JAMM2010.sbk\5\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(30, network.Retentions.Count());

            foreach (IRetention retention in network.Retentions)
            {
                Assert.AreEqual(100, retention.Branch.Length, 1E-6, "New branch");
                Assert.AreEqual(0, retention.Branch.Source.IncomingBranches.Count, "Incoming new branch");
                Assert.AreEqual(1, retention.Branch.Source.OutgoingBranches.Count, "Outgoing on new branch");
                Assert.Greater(retention.Branch.BranchFeatures.Count, 2, "Structures added on new branch");
            }

            //check the names are correct
            var atom = "ABCDEFGHIJKLM".ToCharArray();
            var names = atom.Select(c => "004_" + c);
            var longNames = atom.Select(c => "Grensms2_" + c);
            
            var branchNames = network.Branches.Select(b => b.Name);
            var branchLongNames = network.Channels.Select(c => c.LongName);

            var a = branchNames.ToList();
            var bb = longNames.ToList();
            
            Assert.IsTrue(names.All(n=>branchNames.Contains(n)),"Branch 004_A , 004_B ...to M should exist");
            Assert.IsTrue(longNames.All(n => branchLongNames.Contains(n)), "Grensms3b_A , Grensms3b_B ...to M should exist");
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadingNetworkWithRetentionsAddsCrossSectionToNewBranchWhenTheBranchWasSplitDueToRetentionAndThereWasACrossSectionAtTheSplitNode()
        {
            //pretty coarse test...jist import a network with that crossections at retententions locations.
            //TODO: make this code better UNIT testable by giving it a list of Sobek entities ...read files -> Sobek Entities -> DS Entities
            //so we can test with predefined sobek entities
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\REModels\J_10BANK_v2.sbk\5\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            //assert crossections are added to newly generated branches
            var createdCrossSection = network.CrossSections.FirstOrDefault(c => c.Branch.Name == "008_B" && c.Chainage == 0.0d);
            Assert.IsNotNull(createdCrossSection);
            Assert.AreEqual("1154-retcs",createdCrossSection.Name);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadNetworkWithExtraResistanceRe()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\REModels\JAMM2010.sbk\5\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            Assert.AreEqual(5, network.ExtraResistances.Count());
            var extraResistance = network.ExtraResistances.Where(er =>er.Name == "5580754").FirstOrDefault();
            Assert.AreEqual("vak3_4", extraResistance.LongName);
            Assert.AreEqual("001_A", extraResistance.Branch.Name);
            Assert.AreEqual(1325, extraResistance.Chainage, 1.0e-6);
            Assert.AreEqual(5, extraResistance.FrictionTable.Arguments[0].Values.Count);
            Assert.AreEqual(0.0, (double)extraResistance.FrictionTable[50.21], 1.0e-6);
        }

        /// <summary>
        /// Tests if the name of the strcuture is correctly imported from the structure.dat file
        /// </summary>
        [Test]
        public void ReadStructureNames()
        {
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\NetworkWithStructures\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            var pump = network.Structures.Where(s => s.Name == "158").First(); // river pump id != def id
            Assert.IsInstanceOfType(typeof(Pump), pump);
            var weir = network.Structures.Where(s => s.Name == "123").First(); // river weir id != def id
            Assert.IsInstanceOfType(typeof(Weir), weir);
            var bridge = network.Structures.Where(s => s.Name == "65").First(); // bridge == def id
            Assert.IsInstanceOfType(typeof(Bridge), bridge);
        }

        /// <summary>
        /// Tests if the name of the strcuture is correctly imported from the structure.dat file
        /// </summary>
        [Test]
        public void ReadExtraResistances()
        {
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\TestXRST.lit\4\network.tp";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            var extraResistances = network.Structures.Where(s => s is IExtraResistance);
            Assert.AreEqual(1, extraResistances.Count());
            ExtraResistance extraResistance = (ExtraResistance)extraResistances.FirstOrDefault();
            Assert.AreEqual(5, extraResistance.FrictionTable.Arguments[0].Values.Count);
            Assert.AreEqual(0.000001, (double)extraResistance.FrictionTable[0.0], 1.0e-6);
            Assert.AreEqual(2.000001, (double)extraResistance.FrictionTable[5.0], 1.0e-6);
            Assert.AreEqual(0.000001, (double)extraResistance.FrictionTable[10.0], 1.0e-6);
            Assert.AreEqual(4.000001, (double)extraResistance.FrictionTable[15.0], 1.0e-6);
            Assert.AreEqual(0.000001, (double)extraResistance.FrictionTable[20.0], 1.0e-6);
        }
        
        [Test]
        public void ReadStructures()
        {
            var structureLocationFile = TestHelper.GetDataDir() + @"\vsa.lit\network.st";
            var structures = new SobekNetworkStructureReader().Read(structureLocationFile);
            Assert.AreEqual(62, structures.Count());

            string structureMappingFile = TestHelper.GetDataDir() + @"\vsa.lit\struct.dat";
            var structureMappings = new SobekStructureDatFileReader().Read(structureMappingFile).ToList();
            Assert.AreEqual(62, structureMappings.Count);
            //Assert.Fail();
            string structureDefinitionFile = TestHelper.GetDataDir() + @"\vsa.lit\struct.def";

            var reader = new SobekStructureDefFileReader(SobekType.Sobek212);

            //regular sobek weir
            IEnumerable<SobekStructureDefinition> definitions = reader.Read(structureDefinitionFile);

            // nb not all the structures are active
            Assert.AreEqual(23, definitions.Where(d => d.Type == (int)SobekStructureType.weir).Count());
            Assert.AreEqual(1, definitions.Where(d => d.Type == (int)SobekStructureType.universalWeir).Count());
            Assert.AreEqual(1, definitions.Where(d => d.Type == (int)SobekStructureType.pump).Count());
        }

        [Test]
        public void ReadCompoundStructures()
        {
            var structureLocationFile = TestHelper.GetDataDir() + @"\compound.lit\2\network.st";
            var structures = new SobekNetworkStructureReader().Read(structureLocationFile);

            // 1 compound structure with 2 sub structures and 2 'normal' structures
            Assert.AreEqual(3, structures.Count());

            var structureMappingFile = TestHelper.GetDataDir() + @"\compound.lit\2\struct.dat";
            var structureMappings = new SobekStructureDatFileReader().Read(structureMappingFile).ToList();
            Assert.AreEqual(4, structureMappings.Count);

            var structureDefinitionFile = TestHelper.GetDataDir() + @"\compound.lit\2\struct.def";

            var reader = new SobekStructureDefFileReader(SobekType.Sobek212);

            var definitions = reader.Read(structureDefinitionFile);
            Assert.AreEqual(4, definitions.Count());

            Assert.AreEqual(1, definitions.Where(d => d.Type == (int)SobekStructureType.riverWeir).Count());
            Assert.AreEqual(1, definitions.Where(d => d.Type == (int)SobekStructureType.riverPump).Count());
        }

        [Test]
        public void ReadNetworkWithCompoundStructures()
        {
            var networkTopologyFile = TestHelper.GetDataDir() + @"\compound.lit\2\network.tp";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(networkTopologyFile);

            Assert.AreEqual(2, network.Nodes.Count);
            Assert.AreEqual(1, network.Branches.Count);
            Assert.AreEqual(7, network.Structures.Count());
            Assert.AreEqual(3, network.Structures.Where(s => s is ICompositeBranchStructure).Count());
            ICompositeBranchStructure compound = (ICompositeBranchStructure) network.Structures.Where(
                    s => s is ICompositeBranchStructure && ((ICompositeBranchStructure) s).Structures.Count() > 1)
                                                                                 .FirstOrDefault();
            Assert.IsNotNull(compound);
            Assert.AreEqual(2, compound.Structures.Count);
        }

        [Test]
        public void TestReadNetworkWithCompoundStructures_CompositeStructureNamesAreUnique()
        {
            // we use this test-data since it contains composite structures with non-unique names
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\ReModels\20110331_NDB.sbk\6\DEFTOP.1";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);
            var compositeStructures = network.CompositeBranchStructures.ToList();

            Assert.IsTrue(compositeStructures.Count > 1);
            Assert.IsTrue(compositeStructures.Select(cbs => cbs.Name).HasUniqueValues());
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ReadLargeNetwork()
        {
            var t = DateTime.Now;
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\eindhoven\network.tp";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            var dt = DateTime.Now - t;

            Assert.AreEqual(20297, network.Nodes.Count);
            Assert.AreEqual(23248, network.Branches.Count);

            //ensure each branch has start and endnodes
            foreach (var branch in network.Branches)
            {
                Assert.IsNotNull(branch.Source);
                Assert.IsNotNull(branch.Target);
            }

            Console.WriteLine("Total time required for reading network: {0}", dt);
            log.DebugFormat("Total time required for reading network: {0}", dt);
            
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
        public void ImportCalculationGridWithNetworkInDutchCulture()
        {
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            //for a dutch culture to demonstrate the import works ok on NL machines
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL"); 

            // import model and network.
            string pathToSobekModel = TestHelper.GetTestFilePath(@"LinkageNodes\network.tp");

            //import existing network and model + default boundary conditions
            var importer = new SobekWaterFlowModel1DImporter();
            var flowModel1D = (WaterFlowModel1D)importer.ImportItem(pathToSobekModel);
            var network = flowModel1D.Network;

            Assert.IsNotNull(flowModel1D.NetworkDiscretization);
            Assert.AreEqual(SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered, flowModel1D.NetworkDiscretization.SegmentGenerationMethod);

            var branchCount = network.Branches.Count;

            Assert.AreEqual(10, branchCount);

            Thread.CurrentThread.CurrentCulture = oldCulture;
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateCalcGridForLinkageNodes()
        {
            // import model and network.
            var pathToSobekModel = TestHelper.GetDataDir() + @"\LinkageNodes\network.tp";

            var importer = new SobekWaterFlowModel1DImporter();
            var flowModel1D = (WaterFlowModel1D)importer.ImportItem(pathToSobekModel);
            var network = flowModel1D.Network;

            Assert.IsNotNull(flowModel1D.NetworkDiscretization);
            Assert.AreEqual(SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered, flowModel1D.NetworkDiscretization.SegmentGenerationMethod);

            int branchCount = network.Branches.Count;

            Assert.AreEqual(10, branchCount);
            // are gridpoints generated for all the branches?
            Assert.AreEqual(branchCount, flowModel1D.NetworkDiscretization.Locations.Values.Select(bl => bl.Branch).Distinct().Count());
            // are segmensts generated for all the branches?
            Assert.AreEqual(branchCount, flowModel1D.NetworkDiscretization.Segments.Values.Select(s => s.Branch).Distinct().Count());

            // have all branches a gridpoint at start and end?
            foreach (var branch in network.Branches)
            {
                var first = flowModel1D.NetworkDiscretization.Locations.Values.Where(nl => nl.Branch == branch).Min(nl => nl.Chainage);
                Assert.AreEqual(0.0, first, 1.0e-5);
                var last = flowModel1D.NetworkDiscretization.Locations.Values.Where(nl => nl.Branch == branch).Max(nl => nl.Chainage);
                Assert.AreEqual(branch.Length, last, 1.0e-5);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadNotSupportedEngelundAndSetToDefault()
        {
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\120_001.lit\3\network.tp";
            var importer = new SobekWaterFlowModel1DImporter();
            var model = (WaterFlowModel1D)importer.ImportItem(pathToSobekNetwork);

            var mainRoughnessSection = model.RoughnessSections.FirstOrDefault(s => s.Name.ToLower() == "main");

            Assert.IsNotNull(mainRoughnessSection);
            var firstNetworkLocation = mainRoughnessSection.RoughnessNetworkCoverage.Locations.Values[0];

            Assert.AreEqual(45.0, mainRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(firstNetworkLocation));
            Assert.AreEqual(RoughnessType.Chezy, mainRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessType(firstNetworkLocation));

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadComplexNetwork()
        {
            string pathToSobekModel = TestHelper.GetDataDir() + @"\network2\network.tp";
            var importer = new SobekWaterFlowModel1DImporter();
            var flowModel1D = (WaterFlowModel1D)importer.ImportItem(pathToSobekModel);

            var network = flowModel1D.Network;

            Assert.AreEqual(flowModel1D.Network, network);
            Assert.AreEqual(flowModel1D.NetworkDiscretization.Network, network);

            // for reach "2" a BDFR record is defined; expect this friction to be set to cross section in this branch
            // BDFR id '2' ci '2' mf 4 mt cp 0 0.003 0 mr cp 0 0.003 0 s1 6 s2 6 sf 4 st cp 0 0.003 0 sr cp 0 0.003 bdfr

            var crossSections = network.CrossSections.Where(cs => cs.Branch.Name == "2");

            // 3 cross section on branch 2: 
            // dat id   def id
            // '33' -> '4' = ty 10; process as yz
            // '32' -> '2' = ty 0 name r_Default; not ignored
            // '31' -> 'Egg Shape .25 m' = ty 6; ignore wait for implementation closed branch
            Assert.AreEqual(2, crossSections.Count());

            //CRFR id '4' nm 'Friction' cs '4'
            //lt ys
            //TBLE
            //0 40 <
            //40 70 <
            //70 100 <
            //tble
            //ft ys
            //TBLE
            //1 0.03 <
            //0 45 <
            //1 0.02 <
            //tble
            //fr ys
            //TBLE
            //1 0.03 <
            //0 45 <
            //1 0.02 <
            //tble crfr
            var crossSection = crossSections.First();
            Assert.AreEqual(3, crossSection.Definition.Sections.Count);
            var firstSectionType = crossSection.Definition.Sections[0].SectionType;
            var secondSectionType = crossSection.Definition.Sections[1].SectionType;
            var thirdSectionType = crossSection.Definition.Sections[2].SectionType;
            var firstRoughnessSection = flowModel1D.RoughnessSections.Where(rs => rs.CrossSectionSectionType == firstSectionType).FirstOrDefault();
            var secondRoughnessSection = flowModel1D.RoughnessSections.Where(rs => rs.CrossSectionSectionType == secondSectionType).FirstOrDefault();
            var thirdRoughnessSection = flowModel1D.RoughnessSections.Where(rs => rs.CrossSectionSectionType == thirdSectionType).FirstOrDefault();

            Assert.AreEqual(RoughnessType.Manning,
                            firstRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessType(
                                new NetworkLocation(crossSection.Branch, crossSection.Chainage)));
            Assert.AreEqual(0.03,
                            firstRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(
                                new NetworkLocation(crossSection.Branch, crossSection.Chainage)), 1.0e-6);
            Assert.AreEqual(RoughnessType.Chezy,
                            secondRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessType(
                                new NetworkLocation(crossSection.Branch, crossSection.Chainage)));
            Assert.AreEqual(45.0,
                            secondRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(
                                new NetworkLocation(crossSection.Branch, crossSection.Chainage)), 1.0e-6);
            Assert.AreEqual(RoughnessType.Manning,
                            thirdRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessType(
                                new NetworkLocation(crossSection.Branch, crossSection.Chainage)));
            Assert.AreEqual(0.02,
                            thirdRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(
                                new NetworkLocation(crossSection.Branch, crossSection.Chainage)), 1.0e-6);
        }

        private void InitializeSobekLicense()
        {
            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void RunZwolleModel()
        {
            InitializeSobekLicense();

            var modelImporter = new SobekWaterFlowModel1DImporter();
            var pathToSobekNetwork = TestHelper.GetTestFilePath(@"SW_max_1.lit\3\network.tp");
            var importedModel = (WaterFlowModel1D) modelImporter.ImportItem(pathToSobekNetwork);
            RunModel(importedModel);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportTwenteModelAndCheckInitialConditions()
        {
            var modelImporter = new SobekHydroModelImporter(false);
            string pathToSobekNetwork = TestHelper.GetTestFilePath(@"TwenteKanaal.lit\3\network.tp");
            var hydroModel = (HydroModel)modelImporter.ImportItem(pathToSobekNetwork);

            var flowModel = hydroModel.Activities.OfType<WaterFlowModel1D>().First();

            //not a requirement per-se, but otherwise our other asserts will fail
            Assert.AreEqual(InitialConditionsType.Depth, flowModel.InitialConditionsType);

            var bedLevelCoverage = BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(flowModel.Network);
            var initialDepth = flowModel.InitialConditions;
            var branch = flowModel.Network.Branches[0];

            var beginningOfBranch = new NetworkLocation(branch, 0);
            var centerOfBranch = new NetworkLocation(branch, branch.Length / 2.0);
            var endOfBranch = new NetworkLocation(branch, branch.Length);

            var bedLevel1 = bedLevelCoverage.Evaluate(beginningOfBranch);
            var depth1 = initialDepth.Evaluate(beginningOfBranch);
            var bedLevel2 = bedLevelCoverage.Evaluate(centerOfBranch);
            var depth2 = initialDepth.Evaluate(centerOfBranch);
            var bedLevel3 = bedLevelCoverage.Evaluate(endOfBranch);
            var depth3 = initialDepth.Evaluate(endOfBranch);

            //assert we have a constant water level but a non-constant water depth!
            Assert.AreNotEqual(depth1, depth2);
            Assert.AreEqual(25.0, depth1 + bedLevel1);
            Assert.AreEqual(25.0, depth2 + bedLevel2);
            Assert.AreEqual(25.0, depth3 + bedLevel3);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void ImportZwolleModelAndCheckLateralSources()
        {
            //runs the test with a altered version of the sw_max model.
            //in this version the crossection on branch '4' is replaced by a YZ-crossection (this was a rectangle)
            var modelImporter = new SobekWaterFlowModel1DImporter();
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\SW_zRect.lit\3\network.tp";
            var importedModel = (WaterFlowModel1D)modelImporter.ImportItem(pathToSobekNetwork);
            Assert.AreEqual(importedModel.LateralSourceData.Count, importedModel.Network.LateralSources.Count());
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void RunZwolleModelWithAddedCrossSection()
        {
            InitializeSobekLicense();

            //runs the test with a altered version of the sw_max model.
            //in this version the crossection on branch '4' is replaced by a YZ-crossection (this was a rectangle)
            var modelImporter = new SobekWaterFlowModel1DImporter();
            string pathToSobekNetwork = TestHelper.GetTestFilePath(@"SW_zRect.lit\3\network.tp");
            var importedModel = (WaterFlowModel1D)modelImporter.ImportItem(pathToSobekNetwork);
            RunModel(importedModel);
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        [Category(TestCategory.Integration)]
        public void ImportVancouverNetwork_GenerateGridPointsAtCrossSectionsAndRunModel()
        {
            ImportVancouverNetworkAndRun(false);
        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        [Category(TestCategory.Integration)]
        public void ImportVancouverNetwork_GenerateGridAtFixedChainagesAndRunModel()
        {
            ImportVancouverNetworkAndRun(true);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadNetworkWithChineseNames()
        {
            string path = TestHelper.GetDataDir() + @"\Tanshui.lit\3\network.tp";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(path);
        }


        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ReadPoNetwork()
        {
            var pathToSobekNetwork = TestHelper.GetDataDir() + @"\POup_GV.lit\7\network.tp";
            var importer = new SobekNetworkImporter();

            HydroNetwork network = null;

            TestHelper.AssertIsFasterThan(28000, () => network = (HydroNetwork) importer.ImportItem(pathToSobekNetwork));

            var channelsWithoutCrossSections = network.Channels.Where(c => !c.CrossSections.Any());
            
            //linkagenode -> branches with same order number: interpolation/extrapolation cross-sections over node
            foreach (var channelWithoutCrossSection in channelsWithoutCrossSections)
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
            string path = TestHelper.GetDataDir() + @"\JKFL2009.LIT\31\NETWORK.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(path);
        }

        //[Test]
        //[Category(TestCategory.Performance)]
        //public void ImportElbeNetwork()
        //{
        //    string path = TestHelper.GetDataDir() + @"\elbe\NETWORK.TP";

        //    //import existing network and model + default boundary conditions
        //    SobekNetworkBranchFileReader reader = new SobekNetworkBranchFileReader(path);
        //    reader.Read();
        //}

        [Test]
        [Category(TestCategory.Integration)]
        public void ReadReferenceNetworkAndClone()
        {
            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\SW_max_1.lit\3\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            IHydroNetwork clonedHydroNetwork = (IHydroNetwork)network.Clone();
            Assert.AreEqual(network.CrossSections.First().Name, clonedHydroNetwork.CrossSections.First().Name);
            clonedHydroNetwork.Nodes.Count.Should().Be.EqualTo(network.Nodes.Count);
            clonedHydroNetwork.Branches.Count.Should().Be.EqualTo(network.Branches.Count);
        }

        
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadNetworkWithBridges()
        {
            string path = TestHelper.GetDataDir() + @"\BrugRect.lit\1\NETWORK.TP";

            //import existing network and model + default boundary conditions
            IHydroNetwork network = GetNetwork(path);

            var bridges = network.Bridges.ToList();
            Assert.AreEqual(3,bridges.Count);
            var rectangleBridge = bridges[1];
            Assert.AreEqual(BridgeType.Rectangle,rectangleBridge.BridgeType);
            Assert.AreEqual(21, rectangleBridge.Width);
            Assert.AreEqual(5, rectangleBridge.Height);
            Assert.AreEqual(0, rectangleBridge.BottomLevel);
            Assert.AreEqual(FlowDirection.Negative, rectangleBridge.FlowDirection);
            
            //check the other bridges' directions
            Assert.AreEqual(FlowDirection.Positive, bridges[2].FlowDirection);
            Assert.AreEqual(FlowDirection.Both, bridges[0].FlowDirection);
        }

        private IHydroNetwork GetNetwork(string path)
        {
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(path);
            return network;
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadNetworkWithAsymTrapezium()
        {
            string path = TestHelper.GetDataDir() + @"\AsymTrap.lit\1\NETWORK.TP";
            var network = GetNetwork(path);

            Assert.AreEqual(2, network.CrossSections.Count());
            Assert.IsTrue(network.CrossSections.All(c => c.CrossSectionType == CrossSectionType.YZ));

            var firstCrossSection = network.CrossSections.First();
            double[] yValues = ((CrossSectionDefinitionYZ) firstCrossSection.Definition).YZDataTable.Select(r => r.Yq).ToArray();
            double[] zValues = ((CrossSectionDefinitionYZ) firstCrossSection.Definition).YZDataTable.Select(r => r.Z).ToArray();

            Assert.AreEqual(new[] {-4.5, -3.5, -2.5, -1.5, -0.5, 0.5, 1.5, 2.5, 3.5, 4.5},
                            yValues);

            Assert.AreEqual(new[] {2, 2, 1, 1, 0, 0, 1, 1, 2, 2},
                            zValues);

            //this one is really asymetrical
            var lastCrossSection = network.CrossSections.Last();
            yValues = ((CrossSectionDefinitionYZ)lastCrossSection.Definition).YZDataTable.Select(r => r.Yq).ToArray();
            zValues = ((CrossSectionDefinitionYZ)lastCrossSection.Definition).YZDataTable.Select(r => r.Z).ToArray();

            Assert.AreEqual(new[] { -9.5, -8.5, -7.5, -6.5, -5.5, -4.5, -3.5, -2.5, -1.5, -0.5 ,5.5,9.5},
                            yValues);
            Assert.AreEqual(new[] { 2, 2, 1,1,0,0,1,1,2,2,5,5},
                                        zValues);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadNetworkWithCulverts()
        {
            string path = TestHelper.GetDataDir() + @"\Culverts.lit\1\NETWORK.TP";
            var network = GetNetwork(path);

            var culverts = network.Culverts.ToList();
            Assert.AreEqual(16, culverts.Count);
            
            //first the 'normal culvert'
            var simpleCulvert = culverts[0];
            Assert.AreEqual(5, simpleCulvert.InletLevel);
            Assert.AreEqual(10, simpleCulvert.OutletLevel);
            Assert.AreEqual(10, simpleCulvert.Length);
            Assert.IsFalse(simpleCulvert.IsGated);
            Assert.IsFalse(simpleCulvert.CulvertType.Equals(DelftTools.Hydro.CulvertType.Siphon));
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
            var valvedCulvertWithRectangleCrossSection = culverts[1];
            Assert.IsTrue(valvedCulvertWithRectangleCrossSection.IsGated);
            Assert.AreEqual(2.0, valvedCulvertWithRectangleCrossSection.GateInitialOpening);
            
            //check the geometry of the culvert..
            Assert.AreEqual(CulvertGeometryType.Rectangle,valvedCulvertWithRectangleCrossSection.GeometryType);
            Assert.AreEqual(5.0, valvedCulvertWithRectangleCrossSection.Height);
            Assert.AreEqual(10.0, valvedCulvertWithRectangleCrossSection.Width);
            Assert.AreEqual(3.1f,valvedCulvertWithRectangleCrossSection.InletLevel);
            
            //check the gate-opening reduction function
            Assert.AreEqual(11,valvedCulvertWithRectangleCrossSection.GateOpeningLossCoefficientFunction.Arguments[0].Values.Count);

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

            // this should be an eggie
            var eitje = culverts[5];
            Assert.AreEqual(0.5, eitje.Width, 1.0e-6);
            Assert.AreEqual(0.75, eitje.Height, 1.0e-6);

            //number 5..the inverted siphon
            //notice this is NOT a siphon
            var circle = culverts[4];
            Assert.AreEqual(0.1, circle.Diameter, 1.0e-6); // Diameter = 2 * 0.05
        }

        /// <summary>
        /// 
        ///                             cs=5 (tabulated, constant friction strickler ks 85)
        ///  0--------------------------------------------------------0
        /// 
        ///                             cs=6 (river, main friction strickler ks 33
        ///                                           fp1 manning 66
        ///                                           fp2 White-Colebrook 99
        ///  0--------------------------------------------------------0
        /// 
        ///     cs=9         cs=10         cs=11              cs=12
        ///   0-50 m 0.23   0-15 m 0.23  0- 5 bb 0.1       0- 5 bb 100
        ///                15-35 c  45   5-10 m  0.2       5-10 bb 101
        ///                35-50 m 0.23 10-15 c    3      10-15 bb 102
        ///                             15-35 m    4      15-35 bb 103
        ///                             35-40 Skn 0.5     35-40 bb 104
        ///                             40-45 Sks  6      40-45 bb 105
        ///                             45-50 WC  0.5     45-50 bb 106
        ///  0--------------------------------------------------------0
        ///  =>   rs00       rs00         rs02               rs02
        ///                  rs01         rs03               rs09
        ///                  rs00         rs04               rs10
        ///                               rs05               rs11
        ///                               rs06               rs12
        ///                               rs07               rs13
        ///                               rs08               rs14
        /// total number 18 (main, fp01, fp02, rs00..rs14)
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadNetworkWithRiverFriction()
        {
            var path = TestHelper.GetDataDir() + @"\Friction.lit\1\NETWORK.TP";
            var importer = new SobekWaterFlowModel1DImporter();
            var model = (WaterFlowModel1D)importer.ImportItem(path);
            var network = model.Network;

            var crossSection = network.CrossSections.Where(cs => cs.Name == "6").First();
            var crossSectionDef = crossSection.Definition;
            Assert.IsNotNull(crossSection);
            // friction river cross section is given as
            //              flow width      type             value
            // Main            60        Strickler(ks)        33
            // FP1             20          Manning            66 
            // FP2             40        White-Colebrook      99
            // which should translate to
            //     FP2       FP1            Main              FP1      FP2
            // [------------|----|--------------------------|----|---------------]
            //      WC        M                Sks            M        WC
            //      99        66                33            66       99
            Assert.AreEqual(CrossSectionType.ZW, crossSection.CrossSectionType);
            Assert.AreEqual(3, crossSectionDef.Sections.Count());

            var crossSectionSection = crossSectionDef.Sections[0];
            Assert.AreEqual(0, crossSectionSection.MinY);
            Assert.AreEqual(30, crossSectionSection.MaxY);
            CheckRoughnessInCoverage(model, crossSection, crossSectionSection, RoughnessType.StricklerKs, 33.0);
            Assert.AreEqual("Main", crossSectionSection.SectionType.Name);

            crossSectionSection = crossSectionDef.Sections[1];
            Assert.AreEqual(30, crossSectionSection.MinY);
            Assert.AreEqual(40, crossSectionSection.MaxY);
            CheckRoughnessInCoverage(model, crossSection, crossSectionSection, RoughnessType.Manning, 66.0);
            Assert.AreEqual("FloodPlain1", crossSectionSection.SectionType.Name);

            crossSectionSection = crossSectionDef.Sections[2];
            Assert.AreEqual(40, crossSectionSection.MinY);
            Assert.AreEqual(60, crossSectionSection.MaxY);
            CheckRoughnessInCoverage(model, crossSection, crossSectionSection, RoughnessType.WhiteColebrook, 99.0);
            Assert.AreEqual("FloodPlain2", crossSectionSection.SectionType.Name);
        }

        private void CheckRoughnessInCoverage(WaterFlowModel1D model, ICrossSection crossSection, CrossSectionSection crossSectionSection, 
            RoughnessType roughnessType, double roughness)
        {
            var roughnessSection =
                model.RoughnessSections.Where(rs => rs.CrossSectionSectionType == crossSectionSection.SectionType).
                    FirstOrDefault();
            Assert.AreEqual(roughnessType,
                            roughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessType(new NetworkLocation(crossSection.Branch, crossSection.Chainage)));
            Assert.AreEqual(roughness,
                            roughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(new NetworkLocation(crossSection.Branch, crossSection.Chainage)), 1.0e-6);
        }

        /// <summary>
        /// see comments for ReadNetworkWithRiverFriction
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadNetworkWithYzFriction()
        {
            var path = TestHelper.GetDataDir() + @"\Friction.lit\1\NETWORK.TP";
            var importer = new SobekWaterFlowModel1DImporter();
            var flowModel1D = (WaterFlowModel1D)importer.ImportItem(path);

            Assert.AreEqual(18, flowModel1D.RoughnessSections.Count);
            var crossSection = flowModel1D.Network.CrossSections.Where(cs => cs.Name == "9").First();
            var crossSectionDef = crossSection.Definition;
            Assert.AreEqual(1, crossSectionDef.Sections.Count);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 0), crossSectionDef.Sections[0].SectionType.Name);

            crossSection = flowModel1D.Network.CrossSections.Where(cs => cs.Name == "10").First();
            crossSectionDef = crossSection.Definition;
            Assert.AreEqual(3, crossSectionDef.Sections.Count);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 0), crossSectionDef.Sections[0].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 1), crossSectionDef.Sections[1].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 0), crossSectionDef.Sections[2].SectionType.Name);

            crossSection = flowModel1D.Network.CrossSections.Where(cs => cs.Name == "11").First();
            crossSectionDef = crossSection.Definition;
            Assert.AreEqual(7, crossSectionDef.Sections.Count);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 2), crossSectionDef.Sections[0].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 3), crossSectionDef.Sections[1].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 4), crossSectionDef.Sections[2].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 5), crossSectionDef.Sections[3].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 6), crossSectionDef.Sections[4].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 7), crossSectionDef.Sections[5].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 8), crossSectionDef.Sections[6].SectionType.Name);

            crossSection = flowModel1D.Network.CrossSections.Where(cs => cs.Name == "12").First();
            crossSectionDef = crossSection.Definition;
            Assert.AreEqual(7, crossSectionDef.Sections.Count);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 2), crossSectionDef.Sections[0].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 9), crossSectionDef.Sections[1].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 10), crossSectionDef.Sections[2].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 11), crossSectionDef.Sections[3].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 12), crossSectionDef.Sections[4].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 13), crossSectionDef.Sections[5].SectionType.Name);
            Assert.AreEqual(string.Format(HydroNetwork.CrossSectionSectionFormat, 14), crossSectionDef.Sections[6].SectionType.Name);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void IgnoreOrphanedBDFRrecords()
        {
            var path = TestHelper.GetDataDir() + @"\profshft.lit\1\friction.dat";
            var defFileText = File.ReadAllText(path, Encoding.Default);
            var sobekFriction = SobekFrictionDatFileReader.GetSobekFriction(defFileText);
            Assert.AreEqual(2, sobekFriction.SobekBedFrictionList.Count); // 2 BDFR records

            path = TestHelper.GetDataDir() + @"\profshft.lit\1\network.tp";
            var importer = new SobekWaterFlowModel1DImporter();
            var flowModel1D = (WaterFlowModel1D)importer.ImportItem(path);

            var main = flowModel1D.RoughnessSections.FirstOrDefault(rs => rs.Name.ToUpper().Contains("MAIN"));
            // no mainchannel because no tabulated profiles
            Assert.IsNull(main);
        }

        [Test]
        public void ReadNetworkWithGeneralStructure()
        {
            var network = GetNetwork(TestHelper.GetDataDir() + @"\GenaStru.lit\1\NETWORK.TP");
            var weir = network.Structures.OfType<Weir>().First(w => w.WeirFormula is GeneralStructureWeirFormula);
            
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

            Assert.AreEqual(0.13, formula.PositiveFreeGateFlow,0.001);
            Assert.AreEqual(0.14, formula.PositiveDrownedGateFlow,0.001);
            Assert.AreEqual(0.15, formula.PositiveFreeWeirFlow,0.001);
            Assert.AreEqual(0.16, formula.PositiveDrownedWeirFlow,0.001);
            Assert.AreEqual(0.17, formula.PositiveContractionCoefficient,0.001);
            Assert.AreEqual(0.18, formula.NegativeFreeGateFlow,0.001);
            Assert.AreEqual(0.19, formula.NegativeDrownedGateFlow,0.001);
            Assert.AreEqual(0.2, formula.NegativeFreeWeirFlow,0.001);
            Assert.AreEqual(0.21, formula.NegativeDrownedWeirFlow,0.001);
            Assert.AreEqual(0.22, formula.NegativeContractionCoefficient,0.001);
            Assert.AreEqual(0.23, formula.ExtraResistance,0.001);
            
        }

        /// <summary>
        /// DelftShel.Plugins.DelftModels.Tests.FlowModel.DelftFlowModel1DTest
        /// </summary>
        private static void ImportVancouverNetworkAndRun(bool fixedSegments)
        {
            // import model and network.
            var path = TestHelper.GetTestFilePath(@"Coquitlam-Vancouver-Canada\Network\NETWORK.TP");

            //import existing network and model + default boundary conditions
            var importer = new SobekWaterFlowModel1DImporter();
            var flowModel1D = (WaterFlowModel1D)importer.ImportItem(path);

            // set initial conditions
            flowModel1D.DefaultInitialWaterLevel = 0.1;
            flowModel1D.DefaultInitialDepth = 0.1;

            // generate grid cells (on cross-sections) using HydroNetworkHelper
            flowModel1D.NetworkDiscretization = new Discretization
            {
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered,
                Network = flowModel1D.Network
            };
            foreach (var branch in flowModel1D.Network.Channels)
            {
                NetworkHelper.ClearLocations(flowModel1D.NetworkDiscretization, branch);
                if (fixedSegments)
                {
                    HydroNetworkHelper.GenerateDiscretization(flowModel1D.NetworkDiscretization, branch, 0, true, 0.5, true, false, true, 500);
                }
                else
                {
                    IList<double> offsets = new List<double> { 0 };

                    foreach (var crossSection in branch.CrossSections)
                    {
                        offsets.Add(crossSection.Chainage);
                    }
                    offsets.Add(branch.Geometry.Length);
                    HydroNetworkHelper.GenerateDiscretization(flowModel1D.NetworkDiscretization, branch, offsets);
                }
            }

            Assert.IsTrue(File.Exists(WaterFlowModel1D.TemplateDataZipFile), "Cannot find template dir");
            // WaterFlowModel1D.ServerExecutablePath = @"DelftModelServer.exe";
            //flowModel1D.RunInSeparateProcess = true;

            var t = DateTime.Now;

            flowModel1D.StartTime = t;
            flowModel1D.StopTime = t.AddMinutes(60);
            flowModel1D.TimeStep = new TimeSpan(0, 10, 0);

            flowModel1D.OutputTimeStep = flowModel1D.TimeStep;

            RunModel(flowModel1D);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void IgnoreRectangularCrossSectionDefinition()
        {
            string definitionFile = TestHelper.GetDataDir() + @"\SW_max_1.lit\3\profile.def";

            IList<SobekCrossSectionDefinition> sobekCrossSectionDefinitions =
                new CrossSectionDefinitionReader().Read(definitionFile).ToList();

            //IList<SobekCrossSectionDefinition> sobekCrossSectionDefinitions = ProfileDefFileScanner.ReadCrossSectionLayer(definitionFile);

            string pathToSobekNetwork = TestHelper.GetDataDir() + @"\SW_max_1.lit\3\Network.TP";
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(pathToSobekNetwork);

            // profile.def 17 has name that starts with r_; it should be skipped
            Assert.AreEqual(1, sobekCrossSectionDefinitions.Count(cs => cs.ID == "17"));
            var profileDefinition = sobekCrossSectionDefinitions.First(cs => cs.ID == "17");
            Assert.AreEqual("r_DuikerLemmelerveld", profileDefinition.Name);
            // profile.def 17 = profile.dat 18

            //Ignored since tests of the sobek testbank are based on rectangle cross-section
            //Assert.AreEqual(0, network.CrossSections.Where(cs => cs.Name == "18").Count());

            // check if other cross section definitions are processed.
            Assert.AreEqual(1, sobekCrossSectionDefinitions.Count(cs => cs.ID == "prof_D20061011-DP-144"));
            Assert.AreEqual(1, network.CrossSections.Count(cs => cs.Name == "prof_D20061011-DP-144"));
        }

        private static void RunModel(WaterFlowModel1D flowModel1D)
        {
            // fix for added validation (cross section definition sections total width should not be less than total cross section width
            flowModel1D.Network.CrossSections.Select(cs => cs.Definition)
                .OfType<CrossSectionDefinition>()
                .Union
                (
                    flowModel1D.Network.CrossSections.Select(cs => cs.Definition)
                    .OfType<CrossSectionDefinitionProxy>()
                    .Select(csdp => csdp.InnerDefinition)
                    .OfType<CrossSectionDefinition>()
                )
                .ForEach(csd => csd.RefreshSectionsWidths());

            flowModel1D.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, flowModel1D.Status, "Model should be in initialized state after it is created.");

            while (flowModel1D.Status != ActivityStatus.Done)
            {
                flowModel1D.Execute();

                // get values from model for the last time step
                IList<double> values = flowModel1D.OutputWaterLevel.GetValues<double>(
                    new VariableValueFilter<DateTime>(flowModel1D.OutputWaterLevel.Arguments[0], flowModel1D.CurrentTime)
                    );

                log.Debug(new List<double>(values).ToArray());

                if (flowModel1D.Status == ActivityStatus.Failed)
                {
                    Assert.Fail("Model run has failed");
                }
            }

            flowModel1D.Finish();

            if (flowModel1D.Status == ActivityStatus.Failed)
            {
                Assert.Fail("Model run has failed");
            }

            flowModel1D.Cleanup();

            if (flowModel1D.Status == ActivityStatus.Failed)
            {
                Assert.Fail("Model run has failed");
            }

            Assert.AreEqual(ActivityStatus.Cleaned, flowModel1D.Status);
        }
    }
}

