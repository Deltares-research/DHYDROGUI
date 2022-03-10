using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Import
{
    [TestFixture]
    class CrossSectionYZFromCsvFileImporterTest
    {
        private IHydroNetwork HydroNetwork { get; set; }

        [SetUp]
        public void SetUp()
        {
            HydroNetwork = new HydroNetwork {Name = "network"};
            
            var node1 = new Node {Name = "a", Geometry = new Point(0, 0) };
            var node2 = new Node { Name = "b", Geometry = new Point(100, 0) };
            var node3 = new Node { Name = "c", Geometry = new Point(100, 100) };
            var node4 = new Node { Name = "d", Geometry = new Point(200, 100) };
            HydroNetwork.Nodes.AddRange(new[] {node1, node2, node3, node4});

            var branch1 = new Branch("branch1", node1, node2, 4000);
            var branch2 = new Branch("branch2", node2, node3, 4000);
            var branch3 = new Branch("branch3", node3, node4, 4000);
            HydroNetwork.Branches.AddRange(new[] {branch1, branch2, branch3});
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportYZCrossSections()
        {
            var path = TestHelper.GetTestFilePath("testcsYZ.csv");
            var importer = new CrossSectionYZFromCsvFileImporter { FilePath = path };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(5, crossSections.Count);
            
            var chainages = HydroNetwork.CrossSections.Select(cs => cs.Chainage).ToList();
            Assert.AreEqual(760.5080321, chainages[0], 1e-06);
            Assert.AreEqual(2402.403816, chainages[1], 1e-06);
            Assert.AreEqual(807.8987695, chainages[2], 1e-06);
            Assert.AreEqual(785.5002577, chainages[3], 1e-06);
            Assert.AreEqual(2200.894605, chainages[4], 1e-06);

            var profile = HydroNetwork.CrossSections.First(cs => cs.Name == "CrossSection3").Definition.GetProfile().ToList();
            Assert.AreEqual(7, profile.Count);
            Assert.AreEqual(0, profile[0].X, 1e-06);
            Assert.AreEqual(12.18181818, profile[1].X, 1e-06);
            Assert.AreEqual(33.33333333, profile[2].X, 1e-06);
            Assert.AreEqual(66.66666667, profile[3].X, 1e-06);
            Assert.AreEqual(77.77777778, profile[4].X, 1e-06);
            Assert.AreEqual(87.81818182, profile[5].X, 1e-06);
            Assert.AreEqual(100, profile[6].X, 1e-06);

            Assert.AreEqual(0, profile[0].Y, 1e-06);
            Assert.AreEqual(-1.604947274, profile[1].Y, 1e-06);
            Assert.AreEqual(-10, profile[2].Y, 1e-06);
            Assert.AreEqual(-10, profile[3].Y, 1e-06);
            Assert.AreEqual(0, profile[4].Y, 1e-06);
            Assert.AreEqual(-2.192828665, profile[5].Y, 1e-06);
            Assert.AreEqual(0, profile[6].Y, 1e-06);

            var flowProfile = HydroNetwork.CrossSections.First(cs => cs.Name == "CrossSection3").Definition.FlowProfile.ToList();

            Assert.AreEqual(7, flowProfile.Count);
            Assert.AreEqual(0, flowProfile[0].X, 1e-06);
            Assert.AreEqual(12.18181818, flowProfile[1].X, 1e-06);
            Assert.AreEqual(33.33333333, flowProfile[2].X, 1e-06);
            Assert.AreEqual(66.66666667, flowProfile[3].X, 1e-06);
            Assert.AreEqual(77.77777778, flowProfile[4].X, 1e-06);
            Assert.AreEqual(87.81818182, flowProfile[5].X, 1e-06);
            Assert.AreEqual(100, flowProfile[6].X, 1e-06);

            Assert.AreEqual(0, flowProfile[0].Y, 1e-06);
            Assert.AreEqual(-1.604947274, flowProfile[1].Y, 1e-06);
            Assert.AreEqual(-10, flowProfile[2].Y, 1e-06);
            Assert.AreEqual(-10, flowProfile[3].Y, 1e-06);
            Assert.AreEqual(0, flowProfile[4].Y, 1e-06);
            Assert.AreEqual(-2.192828665, flowProfile[5].Y, 1e-06);
            Assert.AreEqual(0, flowProfile[6].Y, 1e-06);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportYZCrossSectionsWrongHeader()
        {
            var path = TestHelper.GetTestFilePath("testcsYZWrongHeaders.csv");
            var importer = new CrossSectionYZFromCsvFileImporter { FilePath = path };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(0, crossSections.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportYZCrossSectionsMissingColumn()
        {
            var path = TestHelper.GetTestFilePath("testcsYZMissingColumns.csv");
            var importer = new CrossSectionYZFromCsvFileImporter { FilePath = path };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(0, crossSections.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportYZCrossSectionsWrongField()
        {
            var path = TestHelper.GetTestFilePath("testcsYZWrongFields.csv");
            var importer = new CrossSectionYZFromCsvFileImporter { FilePath = path };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(4, crossSections.Count);
            Assert.AreEqual(0, crossSections.Count(cs => cs.Name == "CrossSection3"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateSingleYZCrossSectionUpdateChainage()
        {
            var cs1 = new CrossSection(CrossSectionDefinitionZW.CreateDefault()) {Name = "CrossSection1", Chainage = 500};
            var cs2 = new CrossSection(CrossSectionDefinitionYZ.CreateDefault()) {Name = "CrossSection2", Chainage = 500};

            var branch1 = HydroNetwork.Branches.FirstOrDefault(b => b.Name == "branch1");
            branch1.BranchFeatures.Add(cs1);
            branch1.BranchFeatures.Add(cs2);

            var path = TestHelper.GetTestFilePath("testcsYZ.csv");
            var importer = new CrossSectionYZFromCsvFileImporter
                {
                    FilePath = path,
                    CrossSectionImportSettings = {CreateIfNotFound = false, ImportChainages = true}
                };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(2, crossSections.Count);

            var importedCrossSections = crossSections.Where(cs => cs.Definition is CrossSectionDefinitionYZ).ToList();

            Assert.AreEqual(1, importedCrossSections.Count);

            var importedCrossSection = importedCrossSections[0];

            Assert.AreEqual("CrossSection2", importedCrossSection.Name);
            Assert.AreEqual(2402.403816, importedCrossSection.Chainage, 1e-06);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateSingleYZCrossSectionNoUpdateChainage()
        {
            var cs1 = new CrossSection(CrossSectionDefinitionZW.CreateDefault()) { Name = "CrossSection1", Chainage = 500 };
            var cs2 = new CrossSection(CrossSectionDefinitionYZ.CreateDefault()) { Name = "CrossSection2", Chainage = 500 };

            var branch1 = HydroNetwork.Branches.FirstOrDefault(b => b.Name == "branch1");
            branch1.BranchFeatures.Add(cs1);
            var branch2 = HydroNetwork.Branches.FirstOrDefault(b => b.Name == "branch2");
            branch2.BranchFeatures.Add(cs2);

            var path = TestHelper.GetTestFilePath("testcsYZ.csv");
            var importer = new CrossSectionYZFromCsvFileImporter
            {
                FilePath = path,
                CrossSectionImportSettings = { CreateIfNotFound = false, ImportChainages = false }
            };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(2, crossSections.Count);

            var importedCrossSections = crossSections.Where(cs => cs.Definition is CrossSectionDefinitionYZ).ToList();

            Assert.AreEqual(1, importedCrossSections.Count);

            var importedCrossSection = importedCrossSections[0];

            Assert.AreEqual("CrossSection2", importedCrossSection.Name);
            Assert.AreEqual(500, importedCrossSection.Chainage, 1e-06);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateSingleYZCrossSectionNoUpdateBranchChainage()
        {
            var cs1 = new CrossSection(CrossSectionDefinitionYZ.CreateDefault()) { Name = "CrossSection1", Chainage = 500 };
            var cs2 = new CrossSection(CrossSectionDefinitionYZ.CreateDefault()) { Name = "CrossSection2", Chainage = 500 };

            var branch1 = HydroNetwork.Branches.FirstOrDefault(b => b.Name == "branch1");
            branch1.BranchFeatures.Add(cs1);
            var branch2 = HydroNetwork.Branches.FirstOrDefault(b => b.Name == "branch2");
            branch2.BranchFeatures.Add(cs2);

            var path = TestHelper.GetTestFilePath("testcsYZNoBranchChainage.csv");
            var importer = new CrossSectionYZFromCsvFileImporter
            {
                FilePath = path,
                CrossSectionImportSettings = { CreateIfNotFound = false, ImportChainages = false }
            };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(2, crossSections.Count);

            var importedCrossSections = crossSections.Where(cs => cs.Definition is CrossSectionDefinitionYZ).ToList();

            Assert.AreEqual(2, importedCrossSections.Count);

            var importedCrossSection = importedCrossSections[1];

            Assert.AreEqual("CrossSection2", importedCrossSection.Name);
            Assert.AreEqual(500, importedCrossSection.Chainage, 1e-06);
            var profile = importedCrossSection.Definition.FlowProfile.ToList();
            Assert.AreEqual(6, profile.Count);
            Assert.AreEqual(0, profile[0].X, 1e-06);
            Assert.AreEqual(22.22222222, profile[1].X, 1e-06);
            Assert.AreEqual(33.33333333, profile[2].X, 1e-06);
            Assert.AreEqual(66.66666667, profile[3].X, 1e-06);
            Assert.AreEqual(77.77777778, profile[4].X, 1e-06);
            Assert.AreEqual(100, profile[5].X, 1e-06);
        }
    }
}
