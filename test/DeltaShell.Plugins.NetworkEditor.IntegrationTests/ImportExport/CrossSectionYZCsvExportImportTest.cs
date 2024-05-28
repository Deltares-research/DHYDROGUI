using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.ImportExport
{
    [TestFixture]
    public class CrossSectionYZCsvExportImportTest
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

            var branch1 = new Branch("ab", node1, node2, 100);
            var branch2 = new Branch("bc", node2, node3, 100);
            var branch3 = new Branch("cd", node3, node4, 100);
            HydroNetwork.Branches.AddRange(new[] {branch1, branch2, branch3});

            var csd1 = CrossSectionDefinitionYZ.CreateDefault("csd1");
            var cs1 = new CrossSection(csd1) {Name = "cs1"};
            var csd2 = CrossSectionDefinitionYZ.CreateDefault("csd2");
            foreach (var coordinate in csd2.GetProfile())
            {
                coordinate.Y = coordinate.Y + 1/(coordinate.X*coordinate.X + 1);
            }
            var cs2 = new CrossSection(csd2) { Name = "cs2"};
            var csd3 = CrossSectionDefinitionYZ.CreateDefault("csd3");
            foreach (var coordinate in csd3.GetProfile())
            {
                coordinate.Y = coordinate.Y - 1 / (coordinate.X * coordinate.X + 4);
            }
            var cs3 = new CrossSection(csd3) {Name = "cs3"};

            NetworkHelper.AddBranchFeatureToBranch(cs1, branch1, 50);
            NetworkHelper.AddBranchFeatureToBranch(cs2, branch2, 50); 
            NetworkHelper.AddBranchFeatureToBranch(cs3, branch3, 50);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportImportYZCrossSections()
        {
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            
            var exporter = new CrossSectionYZToCsvFileExporter();

            exporter.Export(HydroNetwork.CrossSections.ToList(), path);

            var nCrossSections = HydroNetwork.CrossSections.Count();

            Assert.AreNotEqual(0, nCrossSections);

            foreach (var cs in HydroNetwork.CrossSections.ToList())
            {
                cs.Branch.BranchFeatures.Remove(cs);
            }

            Assert.AreEqual(0, HydroNetwork.CrossSections.Count());

            // import using invariant culture
            using (CultureUtils.SwitchToInvariantCulture())
            {
                var importer = new CrossSectionYZFromCsvFileImporter { FilePath = path};
                importer.ImportItem(null, HydroNetwork);
            }
            Assert.AreEqual(HydroNetwork.CrossSections.Count(), nCrossSections);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportImportYZCrossSectionsZWShouldBeSkipped()
        {
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            
            var exporter = new CrossSectionYZToCsvFileExporter();

            var nCrossSectionBefore = HydroNetwork.CrossSections.Count();

            var branch1 = HydroNetwork.Branches[0];

            NetworkHelper.AddBranchFeatureToBranch(new CrossSection(CrossSectionDefinitionZW.CreateDefault()), branch1, 10);

            exporter.Export(HydroNetwork.CrossSections.ToList(), path);


            var nCrossSectionsAfter = HydroNetwork.CrossSections.Count();

            Assert.AreNotEqual(0, nCrossSectionsAfter);

            foreach (var cs in HydroNetwork.CrossSections.ToList())
            {
                cs.Branch.BranchFeatures.Remove(cs);
            }

            Assert.AreEqual(0, HydroNetwork.CrossSections.Count());


            var importer = new CrossSectionYZFromCsvFileImporter { FilePath = path};
            importer.ImportItem(null, HydroNetwork);

            Assert.AreEqual(nCrossSectionBefore, HydroNetwork.CrossSections.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportChangeCultureImportYZCrossSections()
        {
            using (CultureUtils.SwitchToCulture("nl-NL"))
            {
                var path = TestHelper.GetCurrentMethodName() + ".csv";
                var exporter = new CrossSectionYZToCsvFileExporter();
                var crossSection = HydroNetwork.CrossSections.FirstOrDefault();
                double chainage = crossSection.Chainage;
                exporter.Export(HydroNetwork.CrossSections.ToList(), path);

                crossSection.Chainage += 0.5;

                var nCrossSections = HydroNetwork.CrossSections.Count();

                Assert.AreNotEqual(0, nCrossSections);

                foreach (var cs in HydroNetwork.CrossSections.ToList())
                {
                    cs.Branch.BranchFeatures.Remove(cs);
                }

                Assert.AreEqual(0, HydroNetwork.CrossSections.Count());

                // import using invariant culture
                using (CultureUtils.SwitchToInvariantCulture())
                {
                    var importer = new CrossSectionYZFromCsvFileImporter { FilePath = path};
                    importer.ImportItem(null, HydroNetwork);
                }

                Assert.AreEqual(nCrossSections, HydroNetwork.CrossSections.Count());
                crossSection = HydroNetwork.CrossSections.FirstOrDefault();
                Assert.AreEqual(chainage, crossSection.Chainage, 0.001);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportImportPlainCsvCheckStorage()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var branch = network.Branches[0];

            var def1 = CrossSectionDefinitionYZ.CreateDefault();
            var cs1 = new CrossSection(def1) { Name = "cs1" };

            def1.YZDataTable.Clear();
            def1.YZDataTable.AddCrossSectionYZRow(10, 20);

            NetworkHelper.AddBranchFeatureToBranch(cs1,branch, 5);

            //export
            var exporter = new CrossSectionYZToCsvFileExporter();
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            exporter.Export(network.CrossSections, path);

            //import
            var csvFileImporter = new CrossSectionYZFromCsvFileImporter { FilePath = path };
            csvFileImporter.ImportItem(null, network);

            var retrievedDef1 = (CrossSectionDefinitionYZ)network.CrossSections.First(cs => cs.Name == "cs1").Definition;
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportYZPlainCsvModifyAndImport()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4); //contains xyz cs's
            var branch = network.Branches[1];

            var def = CrossSectionDefinitionYZ.CreateDefault();

            var cs1 = new CrossSection(CrossSectionDefinitionZW.CreateDefault()) { Name = "cs1" };
            var cs2 = new CrossSection(CrossSectionDefinitionZW.CreateDefault()) { Name = "cs2" };
            var cs3 = new CrossSection(def) { Name = "cs3" };

            const double storageWidth0 = 22.22;

            def.YZDataTable[0].DeltaZStorage = storageWidth0;
            def.YZDataTable.AddCrossSectionYZRow(999, 999);

            const int originalThalweg = 66;
            def.Thalweg = originalThalweg;

            var cs2copy = (ICrossSection)cs2.Clone();
            cs2copy.Name = "cs2_copy";

            NetworkHelper.AddBranchFeatureToBranch(cs1, branch, 5);
            NetworkHelper.AddBranchFeatureToBranch(cs2, branch, 10);
            NetworkHelper.AddBranchFeatureToBranch(cs2copy, branch, 15);
            NetworkHelper.AddBranchFeatureToBranch(cs3, branch, 20);

            //export
            var exporter = new CrossSectionYZToCsvFileExporter();
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            exporter.Export(network.CrossSections, path);

            //open & modify file
            var fileContents = File.ReadAllText(path);
            var newContents = fileContents.Replace("999", "888");
            //additional cross section
            newContents += "cs4,branch2,30,-50,0,0\n" +
                           "cs4,branch2,30,-16.6666666666667,-10,0\n" +
                           "cs4,branch2,30,16.6666666666667,-10,0\n" +
                           "cs4,branch2,30,50,0,0";
            File.WriteAllText(path, newContents);
            //end

            //import
            var csvFileImporter = new CrossSectionYZFromCsvFileImporter {FilePath = path};
            csvFileImporter.ImportItem(null, network);

            var retrievedCsDef = (CrossSectionDefinitionYZ)network.CrossSections.First(cs => cs.Name == "cs3").Definition;
            var cs4 = network.CrossSections.First(cs => cs.Name == "cs4");

            Assert.AreEqual(CrossSectionType.YZ, cs3.CrossSectionType);
            Assert.AreEqual(CrossSectionType.YZ, cs4.CrossSectionType);

            //YZ
            Assert.AreEqual(originalThalweg, retrievedCsDef.Thalweg);
            Assert.AreEqual(888, retrievedCsDef.YZDataTable[6].Yq);                      //yzdata updated from file
            Assert.AreEqual(888, retrievedCsDef.YZDataTable[6].Z);                       //..
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportPlainCsvModifyAndImportWithAProxy()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4); //contains xyz cs's
            var branch = network.Branches[1];

            var def1 = CrossSectionDefinitionYZ.CreateDefault();
            var def2 = CrossSectionDefinitionZW.CreateDefault();
            var cs1 = new CrossSection(new CrossSectionDefinitionProxy(def1)) { Name = "cs1" };
            var cs2 = new CrossSection(new CrossSectionDefinitionProxy(def2)) { Name = "cs2" };

            const int savedOffsetYZ = 50;
            const int originalOffsetZW = 75;
            NetworkHelper.AddBranchFeatureToBranch(cs1,branch, savedOffsetYZ);
            NetworkHelper.AddBranchFeatureToBranch(cs2,branch, originalOffsetZW);

            //export & import
            var exporter = new CrossSectionYZToCsvFileExporter();
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            exporter.Export(network.CrossSections, path);
            //end export

            //modify live cross section
            cs1.Chainage = 60; //to be undone
            cs2.Chainage = 80; //to be undone
            //modify definition
            def1.YZDataTable.AddCrossSectionYZRow(9, 9);
            def2.ZWDataTable.AddCrossSectionZWRow(9, 9, 9);
            //end modify
            var newProfileYZ = cs1.Definition.GetProfile().ToList();
            var newProfileZW = cs2.Definition.GetProfile().ToList();

            //import
            var csvFileImporter = new CrossSectionYZFromCsvFileImporter { FilePath = path };
            csvFileImporter.ImportItem(path, network);

            //end import

            //make sure cross section changes have been undone by import
            Assert.AreEqual(savedOffsetYZ, cs1.Chainage);
            //not for ZW
            Assert.AreNotEqual(originalOffsetZW, cs2.Chainage);

            //make sure definition changes are overwritten by import (yz)
            Assert.AreNotEqual(newProfileYZ, cs1.Definition.GetProfile().ToList());
            Assert.AreEqual(newProfileZW, cs2.Definition.GetProfile().ToList());
        }
    }
}
