using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekLateralSourcesImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportLateralSources()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\J_10BANK.sbk\4\DEFTOP.1";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter() });

            importer.Import();

            Assert.AreEqual(20, hydroNetwork.LateralSources.Count());
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportSobek2NetworkWithDiffuseLateralSource()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\diffLate.lit\1\NETWORK.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter() });

            importer.Import();

            Assert.AreEqual(3, hydroNetwork.LateralSources.Count());
            var diffuseLateral = hydroNetwork.LateralSources.Where(ls => ls.IsDiffuse).First();
            Assert.AreEqual(diffuseLateral.Branch.Length,diffuseLateral.Length);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportSobekRENetworkWithDiffuseLateralSource()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\LATERALS.sbk\2\DEFTOP.1";
            var waterFlowFmModel = new WaterFlowFMModel();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, waterFlowFmModel, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekLateralSourcesDataImporter() });

            importer.Import();

            Assert.AreEqual(3,waterFlowFmModel.Network.LateralSources.Count());
            var diffuseLateral = waterFlowFmModel.Network.LateralSources.Where(ls => ls.IsDiffuse).First();
            Assert.AreEqual(1800.0, diffuseLateral.Length);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateExistingLateralSources()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\J_10BANK.sbk\4\DEFTOP.1";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter() });

            importer.Import();

            Assert.AreEqual(20, hydroNetwork.LateralSources.Count());

            var firstLateralSource = hydroNetwork.LateralSources.First();
            var offset = firstLateralSource.Chainage;

            firstLateralSource.Chainage += 1;

            Assert.AreNotEqual(offset, firstLateralSource.Chainage);

            importer.Import();

            Assert.AreEqual(20, hydroNetwork.LateralSources.Count());
            Assert.AreEqual(offset, firstLateralSource.Chainage);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateExistingLateralSourceOnAnotherBranch()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\J_10BANK.sbk\4\DEFTOP.1";

            var hydroNetwork = PartialSobekImporterTestHelper.GetTestNetwork();
            var branch = hydroNetwork.Branches.First();
            var nFeatures = branch.BranchFeatures.Count();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter() });

            importer.Import();

            Assert.AreEqual(20, hydroNetwork.LateralSources.Count());

            var firstLateralSource = hydroNetwork.LateralSources.First();
            var orgBranch = firstLateralSource.Branch;
            orgBranch.BranchFeatures.Remove(firstLateralSource);

            firstLateralSource.Branch = branch;
            branch.BranchFeatures.Add(firstLateralSource);

            Assert.AreNotSame(orgBranch, firstLateralSource.Branch);
            Assert.AreEqual(nFeatures + 1, branch.BranchFeatures.Count());

            importer.Import();

            Assert.AreEqual(20, hydroNetwork.LateralSources.Count());
            Assert.AreSame(orgBranch, firstLateralSource.Branch);
            Assert.AreEqual(nFeatures, branch.BranchFeatures.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportLateralSourcesData_Testbench_272()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\272_000.lit\NETWORK.TP";

            var network = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, network, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter() });

            importer.Import();

            Assert.AreEqual(2, network.LateralSources.Where(ls => ls.IsDiffuse == false).Count());
            Assert.AreEqual(11, network.LateralSources.Where(ls => ls.IsDiffuse == true).Count());  //All diffuse lateralsources are merged to one diffuse lateralsource per branch
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportUrbanLateralsToManholes()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Groesbeek.lit\Network.TP";

            var flowModel = new WaterFlowFMModel();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, flowModel,
                new IPartialSobekImporter[]
                {
                    new SobekBranchesImporter(),
                    new SobekLateralSourcesImporter()
                });
            importer.Import();

            Assert.IsNotNull(flowModel);
            Assert.IsNotNull(flowModel.Network);
            Assert.IsNotNull(flowModel.Network.LateralSources);
            Assert.Greater(flowModel.Network.LateralSources.Count(),0);

            //FLBX id 'l_D00230-D00231' nm '' ci '1' lc 6.72681202353685 flbx
            //"1","","D00230","D00231",0,2,192954.8,421288.9,192964.8,421297.9,13.4536240470737,0,0,0
            //In sewer systems we want to have laterals on the node. Move the lateral to the end of the node so it will be treated as a lateral on the node.
            var pipeToCheck = flowModel.Network.Pipes.FirstOrDefault(p => p.Name.Equals("1"));
            Assert.AreEqual(1, flowModel.Network.LateralSources.Count(ls => ls.Branch.Equals(pipeToCheck) & ls.Chainage.IsEqualTo(pipeToCheck.Length, 0.001) ));


        }
    }
}
