using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    public class CrossSectionZWFromCsvFileImporterTest
    {
        private IHydroNetwork HydroNetwork;

        [SetUp]
        public void SetUp()
        {
            HydroNetwork = new HydroNetwork();

            var node1 = new Node { Name = "node1", Geometry = new Point(0, 0) };
            var node2 = new Node { Name = "node2", Geometry = new Point(1000, 0) };
            var node3 = new Node { Name = "node3", Geometry = new Point(1000, 1000) };

            HydroNetwork.Nodes.AddRange(new[] {node1, node2, node3});

            var branch1 = new Branch("branch1", node1, node2, 1000);
            var branch2 = new Branch("branch2", node2, node3, 1000);

            HydroNetwork.Branches.AddRange(new[] {branch1, branch2});

            HydroNetwork.CrossSectionSectionTypes.Clear();
            HydroNetwork.CrossSectionSectionTypes.AddRange(new[]
                {
                    new CrossSectionSectionType {Name = CrossSectionZWCsvImportExportSettings.MainSectionName},
                    new CrossSectionSectionType {Name = CrossSectionZWCsvImportExportSettings.FloodPlain1SectionName},
                    new CrossSectionSectionType {Name = CrossSectionZWCsvImportExportSettings.FloodPlain2SectionName}
                });
        }

        [Test]
        [System.ComponentModel.Category(TestCategory.DataAccess)]
        public void TestWaq2ProfCsvRecordsParseRecords()
        {
            var path = TestHelper.GetTestFilePath("Waq2Prof.csv");
            var importer = new CrossSectionZWFromCsvFileImporter { FilePath = path };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();
            
            Assert.AreEqual(2, crossSections.Count());

            var firstCrossSection = crossSections.First();
            Assert.AreEqual(CrossSectionType.ZW, firstCrossSection.CrossSectionType);
            
            var secondCrossSection = crossSections.Last();
            Assert.AreEqual("P_1379", secondCrossSection.Name);
            Assert.AreEqual("Getijms1__204.03", secondCrossSection.LongName);

            var mainSection =
                secondCrossSection.Definition.Sections.First(
                    s => s.SectionType.Name == CrossSectionZWCsvImportExportSettings.MainSectionName);

            Assert.AreEqual(140.0, 2*(mainSection.MaxY - mainSection.MinY), 1.0e-6);

            var floodPlain1Section =
                secondCrossSection.Definition.Sections.First(
                    s => s.SectionType.Name == CrossSectionZWCsvImportExportSettings.FloodPlain1SectionName);

            Assert.AreEqual(646.0, 2*(floodPlain1Section.MaxY - floodPlain1Section.MinY), 1.0e-6);
            
            var floodPlain2Section =
                secondCrossSection.Definition.Sections.First(
                    s => s.SectionType.Name == CrossSectionZWCsvImportExportSettings.FloodPlain2SectionName);

            Assert.AreEqual(0.0, 2*(floodPlain2Section.MaxY - floodPlain2Section.MinY), 1.0e-6);

            Assert.IsTrue(secondCrossSection.Definition is CrossSectionDefinitionZW);

            var definition = (CrossSectionDefinitionZW) secondCrossSection.Definition;
            var summerDike = definition.SummerDike;
            Assert.IsTrue(summerDike != null);
            Assert.AreEqual(4.01, summerDike.CrestLevel, 1.0e-6);
            Assert.AreEqual(2.51, summerDike.FloodPlainLevel, 1.0e-6);
            Assert.AreEqual(96.0, summerDike.FloodSurface, 1.0e-6);
            Assert.AreEqual(1123.0, summerDike.TotalSurface, 1.0e-6);
        }

        [Test]
        [System.ComponentModel.Category(TestCategory.DataAccess)]
        public void TestWaq2ProfWithWrongColumnHeaders()
        {
            var path = TestHelper.GetTestFilePath("Waq2ProfWithWrongColumns.csv");
            var importer = new CrossSectionZWFromCsvFileImporter { FilePath = path};
            importer.ImportItem(null, HydroNetwork);
            Assert.AreEqual(0, HydroNetwork.CrossSections.Count());
        }

        [Test]
        [System.ComponentModel.Category(TestCategory.DataAccess)]
        public void TestWaq2ProfWithWrongEnumValue()
        {
            var path = TestHelper.GetTestFilePath("Waq2ProfWithWrongEnumValue.csv");
            var importer = new CrossSectionZWFromCsvFileImporter { FilePath = path};
            importer.ImportItem(null, HydroNetwork);
            Assert.AreEqual(0, HydroNetwork.CrossSections.Count());
        }

        [Test]
        [System.ComponentModel.Category(TestCategory.DataAccess)]
        public void TestWaq2ProfWithWrongType()
        {
            var path = TestHelper.GetTestFilePath("Waq2ProfWithWrongType.csv");
            var importer = new CrossSectionZWFromCsvFileImporter { FilePath = path };
            importer.ImportItem(null, HydroNetwork);
            Assert.AreEqual(0, HydroNetwork.CrossSections.Count());
        }

        [Test]
        [System.ComponentModel.Category(TestCategory.DataAccess)]
        public void TestWaq2ProfWithMissingColumns()
        {
            var path = TestHelper.GetTestFilePath("Waq2ProfWithMissingColumns.csv");
            var importer = new CrossSectionZWFromCsvFileImporter { FilePath = path};
            importer.ImportItem(null, HydroNetwork);
            Assert.AreEqual(0, HydroNetwork.CrossSections.Count());
        }

        IHydroNetwork CreateDonauImportNetwork()
        {
            const int branches = 8;
            const double length = 30000;

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(branches);
            int counter = 0;
            foreach (var branch in network.Branches)
            {
                counter++;
                branch.IsLengthCustom = true;
                branch.Length = length;
                branch.Name = "Branch00" + counter;
            }
            return network;
        }

        [Test]
        public void TestImportDonauZWCrossSections()
        {
            var path = TestHelper.GetTestFilePath("DonauZWcrosssections.csv");
            var network = CreateDonauImportNetwork();
            var importer = new CrossSectionZWFromCsvFileImporter{ FilePath = path};
            importer.ImportItem(null, network);
            Assert.AreEqual(115, network.CrossSections.Count());
        }

        [Test]
        public void TestImportDonauWithChainageUpdate()
        {
            var path = TestHelper.GetTestFilePath("DonauZWcrosssections.csv");
            var network = CreateDonauImportNetwork();
            var cs = new CrossSection(new CrossSectionDefinitionZW())
                {
                    Name = "CrossSection001",
                };
            NetworkHelper.AddBranchFeatureToBranch(cs, network.Branches.First(), 100);
            var importer = new CrossSectionZWFromCsvFileImporter
                {
                    FilePath = path,
                    CrossSectionImportSettings = {ImportChainages = true}
                };
            importer.ImportItem(null, network);
            Assert.AreNotEqual(100, cs.Chainage);
        }

        [Test]
        public void TestImportDonauWithoutChainageUpdate()
        {
            var path = TestHelper.GetTestFilePath("DonauZWcrosssections.csv");
            var network = CreateDonauImportNetwork();
            var cs = new CrossSection(new CrossSectionDefinitionZW())
                {
                    Name = "CrossSection001",
                };
            NetworkHelper.AddBranchFeatureToBranch(cs, network.Branches.First(), 100);
            var importer = new CrossSectionZWFromCsvFileImporter
                {
                    FilePath = path,
                    CrossSectionImportSettings = {ImportChainages = false}
                };
            importer.ImportItem(null, network);
            Assert.AreEqual(100, cs.Chainage);
        }

        [Test]
        public void TestImportDonauWithoutCreatingNew()
        {
            var path = TestHelper.GetTestFilePath("DonauZWcrosssections.csv");
            var network = CreateDonauImportNetwork();
            var cs = new CrossSection(new CrossSectionDefinitionZW())
            {
                Name = "CrossSection001",
            };
            NetworkHelper.AddBranchFeatureToBranch(cs, network.Branches.First(), 100);
            var importer = new CrossSectionZWFromCsvFileImporter
            {
                FilePath = path,
                CrossSectionImportSettings = { ImportChainages = true, CreateIfNotFound = false}
            };
            importer.ImportItem(null, network);
            Assert.AreEqual(1, network.CrossSections.Count());
            Assert.AreEqual(cs, network.CrossSections.First());
        }
    }
}
