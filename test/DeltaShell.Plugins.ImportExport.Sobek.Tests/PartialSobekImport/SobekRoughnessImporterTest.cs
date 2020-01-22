using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.Tests.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekRoughnessImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportRoughness()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel("water flow 1d");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter(), new SobekRoughnessImporter() });

            importer.Import();

            var network = waterFlowFmModel.Network;

            Assert.IsFalse(waterFlowFmModel.UseReverseRoughness);

            Assert.IsNotNull(waterFlowFmModel.RoughnessSections);
            Assert.Greater(waterFlowFmModel.RoughnessSections.Count,0);
            Assert.AreEqual(network.CrossSectionSectionTypes.Count, waterFlowFmModel.RoughnessSections.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportReverseRoughness()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\REVERSE.sbk\3\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel("water flow 1d");

            Assert.IsFalse(waterFlowFmModel.UseReverseRoughness);

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel,
                                                                                 new IPartialSobekImporter[]
                                                                                     {
                                                                                         new SobekBranchesImporter(),
                                                                                         new SobekCrossSectionsImporter(),
                                                                                         new SobekRoughnessImporter()
                                                                                     });

            importer.Import();

            Assert.IsTrue(waterFlowFmModel.UseReverseRoughness);

            var network = waterFlowFmModel.Network;
            var channelA = network.Branches[0];
            var channelB = network.Branches[1];
            var channelC = network.Branches[2];
            var channelD = network.Branches[3];
            var channelE = network.Branches[4];
            var sections = waterFlowFmModel.RoughnessSections;
            var reverseMain = (ReverseRoughnessSection)sections.GetApplicableReverseRoughnessSection(sections.GetMainRoughnessSection());
            var reverseFp1 = (ReverseRoughnessSection)sections.GetApplicableReverseRoughnessSection(sections.GetFloodplain1());
            
            //see readme.txt in model for details
            var locationA = new NetworkLocation(channelA, 10);
            Assert.AreEqual(2.5, reverseMain.EvaluateRoughnessValue(locationA));
            Assert.AreEqual(RoughnessType.Chezy, reverseMain.EvaluateRoughnessType(locationA));
            Assert.AreEqual(0.35, reverseFp1.EvaluateRoughnessValue(locationA));
            Assert.AreEqual(RoughnessType.Manning, reverseFp1.EvaluateRoughnessType(locationA));

            var locationB = new NetworkLocation(channelB, 10);
            Assert.AreEqual(2.5, reverseMain.EvaluateRoughnessValue(locationB));
            Assert.AreEqual(RoughnessType.Chezy, reverseMain.EvaluateRoughnessType(locationB));
            Assert.AreEqual(0.35, reverseFp1.EvaluateRoughnessValue(locationB));
            Assert.AreEqual(RoughnessType.Manning, reverseFp1.EvaluateRoughnessType(locationB));

            var locationC1 = new NetworkLocation(channelC, 0);
            Assert.AreEqual(5, reverseMain.EvaluateRoughnessValue(locationC1));
            Assert.AreEqual(RoughnessType.Chezy, reverseMain.EvaluateRoughnessType(locationC1));
            Assert.AreEqual(5, reverseFp1.EvaluateRoughnessValue(locationC1));
            Assert.AreEqual(RoughnessType.Chezy, reverseFp1.EvaluateRoughnessType(locationC1));

            var locationC2 = new NetworkLocation(channelC, channelC.Length);
            Assert.AreEqual(2.5, reverseMain.EvaluateRoughnessValue(locationC2));
            Assert.AreEqual(RoughnessType.Chezy, reverseMain.EvaluateRoughnessType(locationC2));
            Assert.AreEqual(2.5, reverseFp1.EvaluateRoughnessValue(locationC2));
            Assert.AreEqual(RoughnessType.Chezy, reverseFp1.EvaluateRoughnessType(locationC2));

            var locationD = new NetworkLocation(channelD, 0);
            Assert.AreEqual(30, reverseMain.EvaluateRoughnessValue(locationD));
            Assert.AreEqual(RoughnessType.Chezy, reverseMain.EvaluateRoughnessType(locationD));
            Assert.AreEqual(0.02, reverseFp1.EvaluateRoughnessValue(locationD));
            Assert.AreEqual(RoughnessType.Manning, reverseFp1.EvaluateRoughnessType(locationD));

            var main = sections.GetMainRoughnessSection();
            var floodplain1 = sections.GetFloodplain1();

            var locationE = new NetworkLocation(channelE, 0); //reverse leading
            Assert.AreEqual(30, main.EvaluateRoughnessValue(locationE));
            Assert.AreEqual(RoughnessType.Chezy, reverseMain.EvaluateRoughnessType(locationE));
            Assert.AreEqual(30, floodplain1.EvaluateRoughnessValue(locationE));
            Assert.AreEqual(RoughnessType.Chezy, reverseFp1.EvaluateRoughnessType(locationE));

            Assert.AreEqual(30, reverseMain.EvaluateRoughnessValue(locationE));
            Assert.AreEqual(RoughnessType.Chezy, reverseMain.EvaluateRoughnessType(locationE));
            Assert.AreEqual(30, reverseFp1.EvaluateRoughnessValue(locationE));
            Assert.AreEqual(RoughnessType.Chezy, reverseFp1.EvaluateRoughnessType(locationE));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportReverseRoughnessNDB()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\20110331_NDB.sbk\6\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel("water flow 1d");

            Assert.IsFalse(waterFlowFmModel.UseReverseRoughness);

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork,
                                                                                 waterFlowFmModel,
                                                                                 new IPartialSobekImporter[]
                                                                                     {
                                                                                         new SobekBranchesImporter(),
                                                                                         new SobekCrossSectionsImporter(),
                                                                                         new SobekRoughnessImporter()
                                                                                     });

            importer.Import();

            var network = waterFlowFmModel.Network;
            var sections = waterFlowFmModel.RoughnessSections;
            var reverseMain = (ReverseRoughnessSection) sections.GetApplicableReverseRoughnessSection(sections.GetMainRoughnessSection());
            var reverseFp1 = (ReverseRoughnessSection) sections.GetApplicableReverseRoughnessSection(sections.GetFloodplain1());
            var reverseFp2 = (ReverseRoughnessSection) sections.GetApplicableReverseRoughnessSection(sections.GetFloodplain2());

            Assert.IsTrue(waterFlowFmModel.UseReverseRoughness);
            Assert.IsNotNull(sections);
            Assert.Greater(sections.Count, 0);
            Assert.AreEqual(network.CrossSectionSectionTypes.Count*2, sections.Count);

            Assert.IsFalse(reverseMain.UseNormalRoughness); //reverse roughness is only defined on main in this model
            Assert.IsFalse(reverseFp1.UseNormalRoughness); //copy of main
            Assert.IsFalse(reverseFp2.UseNormalRoughness); //copy of main

            //check values and reverse values differ. 
            var main = sections.GetMainRoughnessSection();
            var reverseValues =
                ((IMultiDimensionalArray<double>) reverseMain.RoughnessNetworkCoverage.Components[0].Values).Skip(10).
                    Take(5).ToArray();
            
            var values =
                ((IMultiDimensionalArray<double>)main.RoughnessNetworkCoverage.Components[0].Values).Skip(10).
                    Take(5).ToArray();
            Assert.AreEqual(new[] { 0.014, 0.014, 0.014, 0.028, 0.028 }, reverseValues);
            Assert.AreEqual(new[] { 0.022, 0.016, 0.029, 0.022, 0.029 }, values);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportRoughnessWhereYZCrossSectionHasNoFrictionDataShouldTakeTheMainValueOfTheBranch() //Review Witteveen en Bos
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Twentekanaal.lit\3\Network.TP";
            var waterFlowFmModel = new WaterFlowFMModel("water flow 1d");

            Assert.IsFalse(waterFlowFmModel.UseReverseRoughness);

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel,
                                                                                 new IPartialSobekImporter[]
                                                                                     {
                                                                                         new SobekBranchesImporter(),
                                                                                         new SobekCrossSectionsImporter(),
                                                                                         new SobekRoughnessImporter()
                                                                                     });

            importer.Import();

            var network = waterFlowFmModel.Network;
            var crossSection = network.CrossSections.FirstOrDefault(cd => cd.Name == "cross_H_35600");
            var roughnessSection = waterFlowFmModel.RoughnessSections.First(rs => rs.Name == "Main");

            Assert.IsNotNull(crossSection);

            var roughnessValues = roughnessSection.RoughnessNetworkCoverage.GetValues(new VariableValueFilter<NetworkLocation>(roughnessSection.RoughnessNetworkCoverage.Locations,new NetworkLocation(crossSection.Branch, crossSection.Chainage)));

            Assert.AreEqual(0.04, (double)roughnessValues[0], 0.001);
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportRoughnessShouldBeEfficient()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\LSM1_0.lit\12\network.tp";

            var flowModel = new WaterFlowFMModel("water flow 1d");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, flowModel,
                                                                                 new IPartialSobekImporter[]
                                                                                     {
                                                                                         new SobekBranchesImporter(),
                                                                                         new SobekRoughnessImporter(), 
                                                                                     });
            
            // since upgrade to framework 1.2, takes approx. 15 secs locally - much longer on build server
            TestHelper.AssertIsFasterThan(38000, importer.Import); //on my pc: 13sec, was 75sec.. more to gain though

            Assert.AreEqual(10960, flowModel.RoughnessSections.GetMainRoughnessSection().RoughnessNetworkCoverage.Locations.Values.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportRoughnessUrban()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Groesbeek.lit\Network.TP";

            var flowModel = new WaterFlowFMModel();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, flowModel,
                new IPartialSobekImporter[]
                {
                    new SobekBranchesImporter(),
                    new SobekRoughnessImporter()
                });
            importer.Import();

            Assert.IsNotNull(flowModel.RoughnessSections.GetMainRoughnessSection());

            var sewerRoughness =
                flowModel.RoughnessSections.First(rs => rs.Name.Equals(RoughnessDataSet.SewerSectionTypeName));
            Assert.IsNotNull(sewerRoughness);
            Assert.AreEqual(sewerRoughness.GetDefaultRoughnessType(), RoughnessType.WhiteColebrook);
            Assert.AreEqual(sewerRoughness.GetDefaultRoughnessValue(), 0.003);
            Assert.Greater(sewerRoughness.RoughnessNetworkCoverage.GetValues<double>().Count, 0);


            var pipeToCheck = flowModel.Network.Pipes.FirstOrDefault(p => p.Name.Equals("1"));
            var nRoughnessLocationPipeToChecks = sewerRoughness.RoughnessNetworkCoverage.Locations.AllValues.Count(l => pipeToCheck != null && l.Branch.Equals(pipeToCheck));
            Assert.AreEqual(1, nRoughnessLocationPipeToChecks);

            //BDFR id '1' ci '1' mf 4 mt cp 0 0.004 0 mr cp 0 0.004 0 s1 6 s2 6 bdfr//
            var roughnessLocation = sewerRoughness.RoughnessNetworkCoverage.Locations.AllValues.First(l => pipeToCheck != null && l.Branch.Equals(pipeToCheck));
            Assert.AreEqual(0.004, sewerRoughness.RoughnessNetworkCoverage[roughnessLocation]);

        }
    }
}
