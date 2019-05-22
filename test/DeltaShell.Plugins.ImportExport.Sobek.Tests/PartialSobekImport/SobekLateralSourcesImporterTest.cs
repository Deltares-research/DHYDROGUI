using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
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
            var flowModel1D = new WaterFlowModel1D();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, flowModel1D, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekLateralSourcesDataImporter() });

            importer.Import();

            Assert.AreEqual(3,flowModel1D.Network.LateralSources.Count());
            var diffuseLateral = flowModel1D.Network.LateralSources.Where(ls => ls.IsDiffuse).First();
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
        [Category(TestCategory.Slow)]
        public void ImportModelShouldNotResultInMultipleLateralsWithIdenticalId_Tools8812()
        {
            var zipPath = Path.Combine(TestHelper.GetTestDataDirectory(), "LSM1_0.lit", "3.zip");

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Export value to zip.
                ZipFileUtils.Extract(zipPath, tempDir);
                var pathToSobekNetwork = Path.Combine(tempDir, "3", "NETWORK.TP");


                var network = new HydroNetwork();

                var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, network,
                                                                                     new IPartialSobekImporter[]
                                                                                     {
                                                                                         new SobekBranchesImporter(),
                                                                                         new SobekLateralSourcesImporter
                                                                                             ()
                                                                                     });
                importer.Import();

                Assert.AreEqual(3, network.LateralSources.Count());
                Assert.DoesNotThrow(() =>
                {
                    network.LateralSources.ToDictionary(ls => ls.Name, ls => ls);
                });
            });
        }
    }
}
