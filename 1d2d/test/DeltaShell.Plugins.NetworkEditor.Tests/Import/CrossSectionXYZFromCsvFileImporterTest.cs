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
    class CrossSectionXYZFromCsvFileImporterTest
    {
        private IHydroNetwork HydroNetwork { get; set; }

        [SetUp]
        public void SetUp()
        {
            HydroNetwork = new HydroNetwork { Name = "network" };

            var node1 = new Node { Name = "a", Geometry = new Point(0, 0) };
            var node2 = new Node { Name = "b", Geometry = new Point(100, 0) };
            var node3 = new Node { Name = "c", Geometry = new Point(100, 100) };
            var node4 = new Node { Name = "d", Geometry = new Point(200, 100) };
            HydroNetwork.Nodes.AddRange(new[] { node1, node2, node3, node4 });

            var branch1 = new Branch("branch1", node1, node2, 4000);
            var branch2 = new Branch("branch2", node2, node3, 4000);
            var branch3 = new Branch("branch3", node3, node4, 4000);
            HydroNetwork.Branches.AddRange(new[] { branch1, branch2, branch3 });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportXYZCrossSections()
        {
            var path = TestHelper.GetTestFilePath("testcsXYZ.csv");
            var importer = new CrossSectionXYZFromCsvFileImporter {FilePath = path};
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();
            Assert.AreEqual(3, crossSections.Count);

            var chainages = HydroNetwork.CrossSections.Select(cs => cs.Chainage).Distinct().ToList();
            Assert.AreEqual(1, chainages.Count);
            Assert.AreEqual(500, chainages[0]);

            var cs2 = HydroNetwork.CrossSections.First(cs => cs.Name == "CrossSection2");
            var geometry = cs2.Definition.GetGeometry(cs2).Coordinates;
            
            var xValues = geometry.Select(c => c.X).ToList();
            Assert.AreEqual(12, xValues.Count);
            Assert.AreEqual(752.662115933089, xValues[0], 1e-06);
            Assert.AreEqual(803.139235130418, xValues[1], 1e-06);
            Assert.AreEqual(848.568642408014, xValues[2], 1e-06);
            Assert.AreEqual(893.99804968561 , xValues[3], 1e-06);
            Assert.AreEqual(979.809152321069, xValues[4], 1e-06);
            Assert.AreEqual(1030.2862715184 , xValues[5], 1e-06);
            Assert.AreEqual(1090.85881455519, xValues[6], 1e-06);
            Assert.AreEqual(1146.38364567225, xValues[7], 1e-06);
            Assert.AreEqual(1201.90847678932, xValues[8], 1e-06);
            Assert.AreEqual(1257.43330790638, xValues[9], 1e-06);
            Assert.AreEqual(1297.81500326424, xValues[10], 1e-06);
            Assert.AreEqual(1312.95813902344, xValues[11], 1e-06);

            var yValues = geometry.Select(c => c.Y).ToList();
            Assert.AreEqual(12, yValues.Count);
            Assert.AreEqual(982.333008280935, yValues[0], 1e-06);
            Assert.AreEqual(957.09444868227 , yValues[1], 1e-06);
            Assert.AreEqual(931.855889083606, yValues[2], 1e-06);
            Assert.AreEqual(906.617329484942, yValues[3], 1e-06);
            Assert.AreEqual(886.42648180601 , yValues[4], 1e-06);
            Assert.AreEqual(876.331057966544, yValues[5], 1e-06);
            Assert.AreEqual(866.235634127079, yValues[6], 1e-06);
            Assert.AreEqual(861.187922207346, yValues[7], 1e-06);
            Assert.AreEqual(851.09249836788 , yValues[8], 1e-06);
            Assert.AreEqual(830.901650688948, yValues[9], 1e-06);
            Assert.AreEqual(800.615379170551, yValues[10], 1e-06);
            Assert.AreEqual(790.519955331085, yValues[11], 1e-06);

            var zValues = geometry.Select(c => c.Z).ToList();
            Assert.AreEqual(12, zValues.Count);
            Assert.AreEqual(10                , zValues[0], 1e-06);
            Assert.AreEqual(8.12308631674878  , zValues[1], 1e-06);
            Assert.AreEqual(6.39469439988775  , zValues[2], 1e-06);
            Assert.AreEqual(4.66630248302672  , zValues[3], 1e-06);
            Assert.AreEqual(1.7344695857632   , zValues[4], 1e-06);
            Assert.AreEqual(0.0224609004229173, zValues[5], 1e-06);
            Assert.AreEqual(2.01984198839175  , zValues[6], 1e-06);
            Assert.AreEqual(3.87409592326438  , zValues[7], 1e-06);
            Assert.AreEqual(5.75100960651559  , zValues[8], 1e-06);
            Assert.AreEqual(7.7159508911131   , zValues[9], 1e-06);
            Assert.AreEqual(9.39471352457281  , zValues[10], 1e-06);
            Assert.AreEqual(10                , zValues[11], 1e-06);

            int i = 0;
            var storage = cs2.Definition.FlowProfile.Select(f => f.Y - zValues[i++]).Where(z => z > 1e-06).ToList();
            Assert.AreEqual(3, storage.Count);
            Assert.AreEqual(2.1003047616623, storage[0], 1e-06);
            Assert.AreEqual(3.59638163066846, storage[1], 1e-06);
            Assert.AreEqual(1.59900054269962, storage[2], 1e-06);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportYZCrossSectionsWrongHeader()
        {
            var path = TestHelper.GetTestFilePath("testcsXYZWrongHeaders.csv");
            var importer = new CrossSectionXYZFromCsvFileImporter { FilePath = path };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(0, crossSections.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportYZCrossSectionsMissingColumn()
        {
            var path = TestHelper.GetTestFilePath("testcsXYZMissingColumns.csv");
            var importer = new CrossSectionXYZFromCsvFileImporter { FilePath = path };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(0, crossSections.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportYZCrossSectionsWrongField()
        {
            var path = TestHelper.GetTestFilePath("testcsXYZWrongFields.csv");
            var importer = new CrossSectionXYZFromCsvFileImporter { FilePath = path };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(2, crossSections.Count);
            Assert.AreEqual(0, crossSections.Count(cs => cs.Name == "CrossSection2"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateSingleXYZCrossSectionUpdateChainage()
        {
            var cs1 = new CrossSection(CrossSectionDefinitionZW.CreateDefault()) { Name = "CrossSection1", Chainage = 1000 };
            var cs2 = new CrossSection(CrossSectionDefinitionXYZ.CreateDefault()) { Name = "CrossSection2", Chainage = 1000 };

            var branch1 = HydroNetwork.Branches.FirstOrDefault(b => b.Name == "branch1");
            branch1.BranchFeatures.Add(cs1);
            var branch2 = HydroNetwork.Branches.FirstOrDefault(b => b.Name == "branch2");
            branch2.BranchFeatures.Add(cs2);

            var path = TestHelper.GetTestFilePath("testcsXYZ.csv");
            var importer = new CrossSectionXYZFromCsvFileImporter
            {
                FilePath = path,
                CrossSectionImportSettings = { CreateIfNotFound = false, ImportChainages = true }
            };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(2, crossSections.Count);

            var importedCrossSections = crossSections.Where(cs => cs.Definition is CrossSectionDefinitionXYZ).ToList();

            Assert.AreEqual(1, importedCrossSections.Count);

            var importedCrossSection = importedCrossSections[0];

            Assert.AreEqual("CrossSection2", importedCrossSection.Name);
            Assert.AreEqual(500, importedCrossSection.Chainage, 1e-06);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void UpdateSingleXYZCrossSectionNoUpdateChainage()
        {
            var cs1 = new CrossSection(CrossSectionDefinitionZW.CreateDefault()) { Name = "CrossSection1", Chainage = 1000 };
            var cs2 = new CrossSection(CrossSectionDefinitionXYZ.CreateDefault()) { Name = "CrossSection2", Chainage = 1000 };

            var branch1 = HydroNetwork.Branches.FirstOrDefault(b => b.Name == "branch1");
            branch1.BranchFeatures.Add(cs1);
            var branch2 = HydroNetwork.Branches.FirstOrDefault(b => b.Name == "branch2");
            branch2.BranchFeatures.Add(cs2);

            var path = TestHelper.GetTestFilePath("testcsXYZ.csv");
            var importer = new CrossSectionXYZFromCsvFileImporter
            {
                FilePath = path,
                CrossSectionImportSettings = { CreateIfNotFound = false, ImportChainages = false }
            };
            importer.ImportItem(null, HydroNetwork);
            var crossSections = HydroNetwork.CrossSections.ToList();

            Assert.AreEqual(2, crossSections.Count);

            var importedCrossSections = crossSections.Where(cs => cs.Definition is CrossSectionDefinitionXYZ).ToList();

            Assert.AreEqual(1, importedCrossSections.Count);

            var importedCrossSection = importedCrossSections[0];

            Assert.AreEqual("CrossSection2", importedCrossSection.Name);
            Assert.AreEqual(1000, importedCrossSection.Chainage, 1e-06);
        }
    }
}
