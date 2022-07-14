using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class SobekStructureImporterTest
    {
        [Test]
        public void ImportStructures()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\J_10BANK.sbk\4\DEFTOP.1";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekStructuresImporter() });

            importer.Import();

            Assert.AreEqual(36, hydroNetwork.Structures.Count());
        }

        [Test]
        public void UpdateExistingStructures()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\J_10BANK.sbk\4\DEFTOP.1";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekStructuresImporter() });

            importer.Import();

            Assert.AreEqual(36, hydroNetwork.Structures.Count());

            var firstStructure = hydroNetwork.Structures.Where(s => s.ParentStructure != null).First();
            var longName = firstStructure.LongName;

            firstStructure.LongName = "haha";

            Assert.AreNotEqual(longName, firstStructure.LongName);

            importer.Import();

            Assert.AreEqual(36, hydroNetwork.Structures.Count());
            Assert.AreEqual(longName, firstStructure.LongName);
        }

        [Test]
        public void UpdateExistingStructureOnAnotherBranch()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\J_10BANK.sbk\4\DEFTOP.1";

            var hydroNetwork = PartialSobekImporterTestHelper.GetTestNetwork();
            var branch = hydroNetwork.Branches.First();
            var nFeatures = branch.BranchFeatures.Count();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekStructuresImporter() });

            importer.Import();

            Assert.AreEqual(36, hydroNetwork.Structures.Count());

            var firstStructure = hydroNetwork.Structures.Where(s => s.ParentStructure != null).First();
            var orgBranch = firstStructure.Branch;
            orgBranch.BranchFeatures.Remove(firstStructure);
            firstStructure.ParentStructure.Structures.Remove(firstStructure);

            firstStructure.Branch = branch;
            branch.BranchFeatures.Add(firstStructure);

            Assert.AreNotSame(orgBranch, firstStructure.Branch);
            Assert.AreEqual(nFeatures + 1, branch.BranchFeatures.Count());

            importer.Import();

            Assert.AreEqual(36, hydroNetwork.Structures.Count());
            Assert.AreSame(orgBranch, firstStructure.Branch);
            Assert.AreEqual(nFeatures, branch.BranchFeatures.Count());
        }

        [Test]
        public void ImportPumpsWithDifferentSuctionLevelsJakartaModel()
        {

            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\FHM2012A.lit\1\NETWORK.TP";

            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekStructuresImporter() });

            importer.Import();

            //pump with first level
            var wadukSunterUtaraPump1 = hydroNetwork.Structures.FirstOrDefault(s => s.Name == "WadukSunterUtaraPump") as IPump;
            Assert.IsNotNull(wadukSunterUtaraPump1);
            Assert.IsNotNull(wadukSunterUtaraPump1.ParentStructure, "Pump should be in a compound structure");
            Assert.AreEqual("54",wadukSunterUtaraPump1.Branch.Name);
            Assert.AreEqual("54", wadukSunterUtaraPump1.ParentStructure.Branch.Name);

            Assert.IsTrue(wadukSunterUtaraPump1.Branch.BranchFeatures.Contains(wadukSunterUtaraPump1));
            Assert.IsTrue(wadukSunterUtaraPump1.Branch.BranchFeatures.Contains(wadukSunterUtaraPump1.ParentStructure));

            Assert.AreEqual(6.6, wadukSunterUtaraPump1.Capacity,0.001);
            Assert.AreEqual(-1.4, wadukSunterUtaraPump1.StartSuction);
            Assert.AreEqual(-1.5, wadukSunterUtaraPump1.StopSuction);
            Assert.AreEqual(0.0, wadukSunterUtaraPump1.StartDelivery);
            Assert.AreEqual(0.0, wadukSunterUtaraPump1.StopDelivery);

            var compoundStructure = wadukSunterUtaraPump1.ParentStructure;

            //pump with second level
            var wadukSunterUtaraPump2 = hydroNetwork.Structures.FirstOrDefault(s => s.Name == "WadukSunterUtaraPump2") as IPump;
            Assert.IsNotNull(wadukSunterUtaraPump2);
            Assert.AreSame(wadukSunterUtaraPump2.ParentStructure, compoundStructure);
            Assert.AreEqual("54", wadukSunterUtaraPump2.Branch.Name);
            Assert.IsTrue(wadukSunterUtaraPump2.Branch.BranchFeatures.Contains(wadukSunterUtaraPump2));

            Assert.AreEqual(3.3, wadukSunterUtaraPump2.Capacity, 0.001);
            Assert.AreEqual(-1.3, wadukSunterUtaraPump2.StartSuction);
            Assert.AreEqual(-1.49, wadukSunterUtaraPump2.StopSuction);
            Assert.AreEqual(0.0, wadukSunterUtaraPump2.StartDelivery);
            Assert.AreEqual(0.0, wadukSunterUtaraPump2.StopDelivery);

            //pump with third level
            var wadukSunterUtaraPump3 = hydroNetwork.Structures.FirstOrDefault(s => s.Name == "WadukSunterUtaraPump3") as IPump;
            Assert.IsNotNull(wadukSunterUtaraPump3);
            Assert.AreSame(wadukSunterUtaraPump3.ParentStructure, compoundStructure);
            Assert.AreEqual("54", wadukSunterUtaraPump3.Branch.Name);
            Assert.IsTrue(wadukSunterUtaraPump3.Branch.BranchFeatures.Contains(wadukSunterUtaraPump3));

            Assert.AreEqual(10.0, wadukSunterUtaraPump3.Capacity, 0.001);
            Assert.AreEqual(-1.1, wadukSunterUtaraPump3.StartSuction);
            Assert.AreEqual(-1.48, wadukSunterUtaraPump3.StopSuction);
            Assert.AreEqual(0.0, wadukSunterUtaraPump3.StartDelivery);
            Assert.AreEqual(0.0, wadukSunterUtaraPump3.StopDelivery);

        }

        [Test]
        public void CheckStructureNamesAfterImportStructuresFromSobek212()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\StructureNames\NETWORK.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekStructuresImporter() });

            importer.Import();

            Assert.AreEqual(1, hydroNetwork.Structures.OfType<IWeir>().Count()); // was 4 but -1 rivier weir && 2 adv weir
            Assert.AreEqual("Weir 1", hydroNetwork.Weirs.Where(s => s.Name == "wr1").FirstOrDefault().LongName);
            /*
            // River weir and Advanced weir are not yet implemented in the kernel
            Assert.AreEqual("River Weir 1", hydroNetwork.Weirs.Where(s => s.Name == "rwr1").FirstOrDefault().LongName);
            Assert.AreEqual("mem 1", hydroNetwork.Weirs.Where(s => s.Name == "cs1~~1").FirstOrDefault().LongName);
            Assert.AreEqual("mem 2", hydroNetwork.Weirs.Where(s => s.Name == "cs1~~2").FirstOrDefault().LongName);
            */
            //name for (real) composite structure is also imported:
            Assert.AreEqual("Weir 1", hydroNetwork.CompositeBranchStructures.FirstOrDefault(s => s.Name == "wr1 [compound]").LongName);
        }

        [Test]
        public void CheckStructureNamesAfterImportStructuresFromSobekRE()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\J_10BANK.sbk\26\DEFTOP.1";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekStructuresImporter() });

            importer.Import();

            Assert.AreEqual(19, hydroNetwork.Structures.OfType<IWeir>().Count()); // was 32 but -13 riverweirs
            Assert.AreEqual("stuw_Borgh_zom", hydroNetwork.Weirs.Where(s => s.Name == "01").FirstOrDefault().LongName);
            Assert.AreEqual("sluis___Limmel", hydroNetwork.Weirs.Where(s => s.Name == "04").FirstOrDefault().LongName);

            //name for composite structure is also imported:
            Assert.AreEqual("stuw_Borg_comp", hydroNetwork.CompositeBranchStructures.First(s => s.Name == "01 [compound]").LongName);
        }

    }
}
