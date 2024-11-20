using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.PartialSobekImport
{
    [TestFixture]
    public class SobekMeasurementStationsImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportMeasurementStations()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\J_10BANK.sbk\4\DEFTOP.1";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekMeasurementStationsImporter() });

            importer.Import();

            Assert.AreEqual(21, hydroNetwork.ObservationPoints.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateExistingMeasurementStations()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\J_10BANK.sbk\4\DEFTOP.1";
            var hydroNetwork = new HydroNetwork();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekMeasurementStationsImporter() });

            importer.Import();

            Assert.AreEqual(21, hydroNetwork.ObservationPoints.Count());

            var firstMeasurementStation = hydroNetwork.ObservationPoints.First();
            var offset = firstMeasurementStation.Chainage;

            firstMeasurementStation.Chainage += 1;

            Assert.AreNotEqual(offset, firstMeasurementStation.Chainage);

            importer.Import();

            Assert.AreEqual(21, hydroNetwork.ObservationPoints.Count());
            Assert.AreEqual(offset, firstMeasurementStation.Chainage);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateExistingMeasurementStationOnAnotherBranch()
        {
            string pathToSobekNetwork = TestHelper.GetTestDataDirectory() + @"\ReModels\J_10BANK.sbk\4\DEFTOP.1";

            var hydroNetwork = PartialSobekImporterTestHelper.GetTestNetwork();
            var branch = hydroNetwork.Branches.First();
            var nFeatures = branch.BranchFeatures.Count();

            var importer = PartialSobekImporterBuilder.BuildPartialSobekImporter(pathToSobekNetwork, hydroNetwork, new IPartialSobekImporter[] { new SobekBranchesImporter(), new SobekMeasurementStationsImporter() });

            importer.Import();

            Assert.AreEqual(21, hydroNetwork.ObservationPoints.Count());

            var firstMeasurementStation = hydroNetwork.ObservationPoints.First();
            var orgBranch = firstMeasurementStation.Branch;
            orgBranch.BranchFeatures.Remove(firstMeasurementStation);

            firstMeasurementStation.Branch = branch;
            branch.BranchFeatures.Add(firstMeasurementStation);

            Assert.AreNotSame(orgBranch, firstMeasurementStation.Branch);
            Assert.AreEqual(nFeatures + 1, branch.BranchFeatures.Count());

            importer.Import();

            Assert.AreEqual(21, hydroNetwork.ObservationPoints.Count());
            Assert.AreSame(orgBranch, firstMeasurementStation.Branch);
            Assert.AreEqual(nFeatures, branch.BranchFeatures.Count());
        }
    }
}
