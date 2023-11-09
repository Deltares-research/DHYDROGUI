using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.TestUtils;
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
            Assert.Greater(waterFlowFmModel.RoughnessSections.Count, 0);
            Assert.AreEqual(network.CrossSectionSectionTypes.Count, waterFlowFmModel.RoughnessSections.Count);
        }

        [Test]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ImportRoughnessShouldBeEfficient()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\POup_GV.lit\7\NETWORK.TP";

            var flowModel = new WaterFlowFMModel("water flow 1d");

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, flowModel,
                                                                                 new IPartialSobekImporter[]
                                                                                     {
                                                                                         new SobekBranchesImporter(),
                                                                                         new SobekRoughnessImporter()
                                                                                     });

            TestHelper.AssertIsFasterThan(38000, importer.Import);
            var locationCount = flowModel.RoughnessSections.SelectMany(rs => rs.RoughnessNetworkCoverage.Locations.Values).Count();
            Assert.AreEqual(8737, locationCount);
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

            Assert.IsNotNull(GetMainRoughnessSection(flowModel.RoughnessSections));

            var sewerRoughness =
                flowModel.RoughnessSections.First(rs => rs.Name.Equals(RoughnessDataSet.SewerSectionTypeName));
            Assert.IsNotNull(sewerRoughness);
            Assert.AreEqual(sewerRoughness.GetDefaultRoughnessType(), RoughnessType.WhiteColebrook);
            Assert.AreEqual(sewerRoughness.GetDefaultRoughnessValue(), 0.2);
            Assert.Greater(sewerRoughness.RoughnessNetworkCoverage.GetValues<double>().Count, 0);


            var pipeToCheck = flowModel.Network.Pipes.FirstOrDefault(p => p.Name.Equals("1"));
            var nRoughnessLocationPipeToChecks = sewerRoughness.RoughnessNetworkCoverage.Locations.AllValues.Count(l => pipeToCheck != null && l.Branch.Equals(pipeToCheck));
            Assert.AreEqual(1, nRoughnessLocationPipeToChecks);

            //BDFR id '1' ci '1' mf 4 mt cp 0 0.004 0 mr cp 0 0.004 0 s1 6 s2 6 bdfr//
            var roughnessLocation = sewerRoughness.RoughnessNetworkCoverage.Locations.AllValues.First(l => pipeToCheck != null && l.Branch.Equals(pipeToCheck));
            Assert.AreEqual(0.004, sewerRoughness.RoughnessNetworkCoverage[roughnessLocation]);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportRoughnessWhereYZCrossSectionHasNoFrictionDataShouldTakeTheMainValueOfTheBranch() //Review Witteveen en Bos
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Twentekanaal.lit\3\Network.TP";
            var waterFlowFmModel = new WaterFlowFMModel("waterflow1d");

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
            var crossSection = network.CrossSections.FirstOrDefault(cd => cd.Name == "180");
            var roughnessSection = waterFlowFmModel.RoughnessSections.First(rs => rs.Name == "Main");

            Assert.IsNotNull(crossSection);

            var roughnessValues = roughnessSection.RoughnessNetworkCoverage.GetValues(new VariableValueFilter<NetworkLocation>(roughnessSection.RoughnessNetworkCoverage.Locations, new NetworkLocation(crossSection.Branch, crossSection.Chainage)));

            Assert.AreEqual(0.04, (double)roughnessValues[0], 0.001);
        }

        private static RoughnessSection GetMainRoughnessSection(IEnumerable<RoughnessSection> sections)
        {
            return sections.FirstOrDefault(s => s.Name == RoughnessDataSet.MainSectionTypeName);
        }
    }
}
