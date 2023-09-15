using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekCrossSectionsImporterTest
    {

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportCrossSections()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork,hydroNetwork, new IPartialSobekImporter[] {new SobekBranchesImporter(),new SobekCrossSectionsImporter()});

            importer.Import();

            Assert.AreEqual(52, hydroNetwork.CrossSections.Count());
            Assert.AreEqual("P_CoqR_21730", hydroNetwork.CrossSections.First().Name);
            Assert.AreEqual(0, hydroNetwork.SharedCrossSectionDefinitions.Count());
            Assert.IsTrue(hydroNetwork.CrossSections.All(cs => !cs.Definition.IsProxy));
            Assert.AreEqual(0, hydroNetwork.CrossSectionSectionTypes.Count());
            //no friction file in \network1\
            //var cs = hydroNetwork.CrossSections.First(); 
            //Assert.AreEqual(1, cs.Sections.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Jira)]
        public void ImportCrossSectionsWithScientificNotationTools9637()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekCrossSectionsImporterTest).Assembly, @"TOOLS963.lit\1\NETWORK.TP");
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(2, hydroNetwork.CrossSections.Count());
            var crossSectionWithScientificNotation = hydroNetwork.CrossSections.First(cs => cs.Name == "3");
            var crossSectionWithNormalNotation = hydroNetwork.CrossSections.First(cs => cs.Name == "11");

            Assert.AreEqual(7.69938720957774E-05, crossSectionWithScientificNotation.Chainage, 1e-9);
            Assert.AreEqual(5437.30926751243, crossSectionWithNormalNotation.Chainage, 1e-9);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportCrossSectionsWithStrangeSymbolsInNames()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\FHM2011F.lit\1\Network.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(2551, hydroNetwork.CrossSections.Count());
            Assert.AreEqual(38, hydroNetwork.SharedCrossSectionDefinitions.Count());
            Assert.AreEqual("KA45", hydroNetwork.CrossSections.First().Name);
            Assert.IsTrue(hydroNetwork.CrossSections.Any(cs => cs.Name == "bPlu_11.5*_14"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportCrossSectionsOfPo()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\POup_GV.lit\7\Network.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(4153, hydroNetwork.CrossSections.Count());
            Assert.AreEqual(15, hydroNetwork.SharedCrossSectionDefinitions.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportCrossSectionsToExistingNetwork()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";
            var hydroNetwork = PartialSobekImporterTestHelper.GetTestNetwork();

            var csName = hydroNetwork.CrossSections.First().Name;

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();


            Assert.IsNotNull(hydroNetwork.CrossSections.FirstOrDefault(cs => cs.Name == csName));

            Assert.AreEqual(53, hydroNetwork.CrossSections.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateExistingCrossSectionOnAnotherBranch()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";

            var hydroNetwork = PartialSobekImporterTestHelper.GetTestNetwork();
            var branch = hydroNetwork.Branches.First();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(53, hydroNetwork.CrossSections.Count());

            var firstCrossSection = hydroNetwork.CrossSections.Last(); //the first cs is part of  GetTestNetwork

            var branchOfCS = firstCrossSection.Branch;
            firstCrossSection.Branch = branch;

            Assert.AreNotSame(branchOfCS, firstCrossSection.Branch);

            importer.Import();

            Assert.AreEqual(53, hydroNetwork.CrossSections.Count());
            Assert.AreSame(branchOfCS, hydroNetwork.CrossSections.Last().Branch);
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateExistingCrossSection()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\network1\Network.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(52, hydroNetwork.CrossSections.Count());

            var firstCrossSection = hydroNetwork.CrossSections.First();

            var longName = firstCrossSection.LongName;

            firstCrossSection.LongName = "changedCHANGED";

            importer.Import();

            Assert.AreEqual(52, hydroNetwork.CrossSections.Count());
            Assert.AreEqual(longName, hydroNetwork.CrossSections.First().LongName);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportAllCrossSectionTypes()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\StandardCrossSections.lit\10\Network.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });

            importer.Import();

            Assert.AreEqual(22, hydroNetwork.CrossSections.Count()); 
            //needs to be 12 now instead of 11 because we will retrieve height and width of a closed rectangular (id=19) from the table (5x10).
            Assert.AreEqual(12, hydroNetwork.CrossSections.Where(t => t.CrossSectionType == CrossSectionType.Standard).Count());


            var definition = hydroNetwork.CrossSections.Where(
                cs => cs.CrossSectionType == CrossSectionType.ZW && !cs.Definition.IsProxy).Select(
                    cs => cs.Definition as CrossSectionDefinitionZW).First();

            Assert.AreEqual(0.0, definition.Sections[0].MinY);
            Assert.AreEqual(2.0, definition.Sections[0].MaxY);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category("Quarantine")]
        public void ImportPipeProfilesTest()
        {
            var pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\Groesbeek.lit\Network.TP";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekCrossSectionsImporter() });
            importer.Import();

            //global check
            Assert.AreEqual(912, hydroNetwork.Pipes.Count());

            //cross-section check
            Assert.AreEqual(29, hydroNetwork.SharedCrossSectionDefinitions.Count());

            //each pipe profile should be in shared cross-section definitions

            foreach (var pipe in hydroNetwork.Pipes)
            {
                Assert.IsFalse(string.IsNullOrEmpty(pipe.CrossSectionDefinitionName));
                Assert.IsNotNull(pipe.CrossSection?.Definition);
            }

            //CRDS id 'Round 1000 mm' nm 'Round 1000 mm' ty 4 bl 0 rd  .5 crds//
            //CRSN id 'l_D00230-D00231' di 'Round 1000 mm'  rl  26.25 ll  26.23 crsn//
            //CRSN id 'l_D00230-D00231' nm '' ci '1' lc 6.72681202353685 crsn//
            //BDFR id '1' ci '1' mf 4 mt cp 0 0.004 0 mr cp 0 0.004 0 s1 6 s2 6 bdfr//

            var pipeToCheck = hydroNetwork.Pipes.FirstOrDefault(p => p.Name.Equals("1"));
            Assert.IsNotNull(pipeToCheck);
            Assert.True(pipeToCheck.CrossSectionDefinitionName.Equals("Round 1000 mm"));
            Assert.True(pipeToCheck.CrossSection?.Definition.CrossSectionType  == CrossSectionType.Standard);
            Assert.AreEqual(26.23, pipeToCheck.LevelSource, 0.001);
            Assert.AreEqual(26.25, pipeToCheck.LevelTarget, 0.001);
            Assert.AreEqual(pipeToCheck.Material,SewerProfileMapping.SewerProfileMaterial.Unknown);
        }
    }
}
