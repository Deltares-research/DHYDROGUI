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
    public class SobekRetentionImporterTest
    {
        [Test]
        public void ImportRetentions()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork,
                                                                                 new IPartialSobekImporter[]
                                                                                     {
                                                                                         new SobekBranchesImporter(),
                                                                                         new SobekLateralSourcesImporter(),
                                                                                         new SobekRetentionImporter()
                                                                                     });

            importer.Import();

            Assert.AreEqual(30, hydroNetwork.Retentions.Count());

            var retention = hydroNetwork.Retentions.First();
            var retentionBranch = retention.Branch;
            Assert.AreEqual(100, retentionBranch.Length);
            Assert.AreEqual(0, retention.Chainage);
            Assert.AreEqual(0, retentionBranch.Source.IncomingBranches.Count);
            Assert.AreEqual(1, retentionBranch.Source.OutgoingBranches.Count);
            Assert.AreEqual(2, retentionBranch.Target.IncomingBranches.Count);
            Assert.AreEqual(1, retentionBranch.Target.OutgoingBranches.Count);

            Assert.AreEqual("Ret-114_Latkw2__in", retention.LongName);

            var cutBranch = hydroNetwork.Branches.First(n => n.Name.Equals("010_A"));
            Assert.AreEqual(1150.0, cutBranch.Length, 0.001);

            var retentionCompositeStructure = (CompositeBranchStructure)hydroNetwork.BranchFeatures.OfType<BranchStructure>().First(bf => bf.Branch.Name.Contains("retb"));
            Assert.AreEqual("Latkw2__in", retentionCompositeStructure.Structures[0].LongName);
        }

        [Test]
        //To complex for updating (specially from RE)
        public void NoUpdateExistingRetentions()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekRetentionImporter() });

            importer.Import();

            Assert.AreEqual(30, hydroNetwork.Retentions.Count());

            var firstRetention = hydroNetwork.Retentions.First();
            var offset = firstRetention.Chainage;

            firstRetention.Chainage += 1;

            Assert.AreNotEqual(offset, firstRetention.Chainage);

            importer.Import();

            Assert.AreEqual(30, hydroNetwork.Retentions.Count());
            Assert.AreNotEqual(offset, firstRetention.Chainage);
        }

        [Test]
        public void RemoveMarkerLateralsAfterAddingRetention()
        {
            //issue 5344
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\JAMM2010.sbk\40\DEFTOP.1";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekRetentionImporter() });

            importer.Import();

            var markerLateral = hydroNetwork.LateralSources.FirstOrDefault(l => l.LongName == "Ret-114_Latkw2__in");
            Assert.IsNull(markerLateral);
        }

        [Test]
        public void ImportLateralSourcesAndRetentions()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\263_000.lit\1\NETWORK.TP";

            var hydroNetwork = new HydroNetwork();
            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekLateralSourcesImporter(), new SobekRetentionImporter() });

            importer.Import();

            Assert.AreEqual(6, hydroNetwork.LateralSources.Count()); //checked in sobek
            Assert.AreEqual(5, hydroNetwork.Retentions.Count()); //checked in sobek

        }
    }
}
