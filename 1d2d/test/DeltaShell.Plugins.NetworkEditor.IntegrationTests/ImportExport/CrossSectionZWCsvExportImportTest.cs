using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.ImportExport
{
    [TestFixture]
    public class CrossSectionZWCsvExportImportTest
    {
        private static bool CoordinatesEqual(IList<Coordinate> one, IList<Coordinate> two)
        {
            const double delta = 0.0001;

            Assert.AreEqual(one.Count, two.Count);
            for(int i = 0; i < one.Count; i++)
            {
                Assert.AreEqual(one[i].X, two[i].X, delta);
                Assert.AreEqual(one[i].Y, two[i].Y, delta);
                Assert.AreEqual(one[i].Z, two[i].Z, delta);
            }
            return true;
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportWaqCsvModifyAndImport()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4); //contains xyz cs's
            var branch = network.Branches[1];

            var def1 = CrossSectionDefinitionYZ.CreateDefault();
            var def2 = CrossSectionDefinitionZW.CreateDefault();
            var cs1 = new CrossSection(def1) { Name = "cs1" };
            var cs2 = new CrossSection(def2) { Name = "cs2" };

            NetworkHelper.AddBranchFeatureToBranch(cs1,branch, 50);
            NetworkHelper.AddBranchFeatureToBranch(cs2,branch, 75);

            def2.SummerDike.Active = false;

            //export & import
            var exporter = new CrossSectionZWToCsvFileExporter();
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            exporter.Export(network.CrossSections, path);
            //end export

            var oldProfileZW = cs2.Definition.GetProfile().ToList();
            //modify
            def2.SummerDike.Active = true;
            def2.ZWDataTable.AddCrossSectionZWRow(9, 9, 9);

            //import
            var csvFileImporter = new CrossSectionZWFromCsvFileImporter();
            csvFileImporter.ImportItem(path, network);

            //make sure definition changes are reverted by import
            Assert.AreEqual(false, def2.SummerDike.Active);

            Assert.IsTrue(CoordinatesEqual(oldProfileZW, def2.GetProfile().ToList()));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportWaqCsvModifyAndImportWithAProxy()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4); //contains xyz cs's
            var branch = network.Branches[1];

            var def1 = CrossSectionDefinitionYZ.CreateDefault();
            var def2 = CrossSectionDefinitionZW.CreateDefault();
            var cs1 = new CrossSection(new CrossSectionDefinitionProxy(def1)) { Name = "cs1" };
            var cs2 = new CrossSection(new CrossSectionDefinitionProxy(def2)) { Name = "cs2" };

            NetworkHelper.AddBranchFeatureToBranch(cs1, branch, 50);
            NetworkHelper.AddBranchFeatureToBranch(cs2, branch, 75);

            def2.SummerDike.Active = false;

            //export & import
            var exporter = new CrossSectionZWToCsvFileExporter();
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            exporter.Export(network.CrossSections, path);
            //end export

            //modify
            def2.SummerDike.Active = true;
            def2.ZWDataTable.AddCrossSectionZWRow(9, 9, 9);
            var newProfileZW = cs2.Definition.GetProfile().ToList();

            //import
            var csvFileImporter = new CrossSectionZWFromCsvFileImporter();
            csvFileImporter.ImportItem(path, network);

            //end import
            
            //make sure definition changes are overwritten by import
            Assert.AreEqual(false, def2.SummerDike.Active);
            Assert.AreNotEqual(newProfileZW.Count, def2.GetProfile().Count());
        }
   
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteAndReadDemoModelWithTabulatedCrossSectionsParseAndMerge()
        {
            // export test data 
            var path = TestHelper.GetCurrentMethodName() + ".csv";

            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4); //contains xyz cs's
            var branch = network.Branches.First();

            var crossSectionDefinition = new CrossSectionDefinitionZW {Name = "JanDef"};
            crossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(0, 100, 1);
            crossSectionDefinition.ZWDataTable.AddCrossSectionZWRow(-5, 0, 0);
            
            //we have to have three sections with the 'special' names..
            var floodPlain2 = new CrossSectionSectionType{Name = CrossSectionZWCsvImportExportSettings.FloodPlain2SectionName};
            var floodPlain1 = new CrossSectionSectionType { Name = CrossSectionZWCsvImportExportSettings.FloodPlain1SectionName };
            
            network.CrossSectionSectionTypes.Add(floodPlain1);
            network.CrossSectionSectionTypes.Add(floodPlain2);

            //|FP2|FP1|Main|FP1|FP2
            crossSectionDefinition.Sections.Add(new CrossSectionSection
                {
                    MinY = 0,
                    MaxY = 40,
                    SectionType =
                        network.CrossSectionSectionTypes.FirstOrDefault(
                            csst => csst.Name == CrossSectionZWCsvImportExportSettings.MainSectionName)
                });
            crossSectionDefinition.Sections.Add(new CrossSectionSection { MinY = 40, MaxY = 45, SectionType = floodPlain1 });
            crossSectionDefinition.Sections.Add(new CrossSectionSection { MinY = 45, MaxY = 50, SectionType = floodPlain2 });

            var nSections = crossSectionDefinition.Sections.Count;

            var crossSection = new CrossSection(crossSectionDefinition)
                                   {
                                       Name = "Jan",
                                       Branch = branch
                                   };
            NetworkHelper.AddBranchFeatureToBranch(crossSection, branch, 50);

            var crossSections = new List<ICrossSection> { crossSection };


            var exporter = new CrossSectionZWToCsvFileExporter();
            exporter.Export(crossSections, path);

            crossSectionDefinition.Sections.Clear();

            Assert.AreEqual(0, crossSection.Definition.Sections.Count);


            // import and check for changes
            var importer = new CrossSectionZWFromCsvFileImporter();
            importer.ImportItem(path, network);

            crossSection = network.CrossSections.FirstOrDefault(cs => cs.Name == "Jan") as CrossSection;

            Assert.IsNotNull(crossSection);

            Assert.AreEqual(nSections, crossSection.Definition.Sections.Count);

            Assert.AreEqual(CrossSectionZWCsvImportExportSettings.MainSectionName, crossSection.Definition.Sections[0].SectionType.Name);
            Assert.AreEqual(0.0, crossSection.Definition.Sections[0].MinY, 1.0e-6);
            Assert.AreEqual(40.0, crossSection.Definition.Sections[0].MaxY, 1.0e-6);

            Assert.AreEqual(CrossSectionZWCsvImportExportSettings.FloodPlain1SectionName, crossSection.Definition.Sections[1].SectionType.Name);
            Assert.AreEqual(40.0, crossSection.Definition.Sections[1].MinY, 1.0e-6);
            Assert.AreEqual(45.0, crossSection.Definition.Sections[1].MaxY, 1.0e-6);

            Assert.AreEqual(CrossSectionZWCsvImportExportSettings.FloodPlain2SectionName, crossSection.Definition.Sections[2].SectionType.Name);
            Assert.AreEqual(45.0, crossSection.Definition.Sections[2].MinY, 1.0e-6);
            Assert.AreEqual(50.0, crossSection.Definition.Sections[2].MaxY, 1.0e-6);
        }
    
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportExportZWCrossSectionWithSharedDefinition()
        {
            var network = new HydroNetwork();
            var from = new HydroNode{ Geometry = new Point(0,0) };
            var to = new HydroNode { Geometry = new Point(0, 100) };
            network.Nodes.Add(from);
            network.Nodes.Add(to);
            var channel = new Channel { Source = from, Target = to, Length = 100.0 };
            network.Branches.Add(channel);

            //add definition
            var crossSectionDefinitionZw = new CrossSectionDefinitionZW("zwCSDef") { IsClosed = true };
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(0, 0, 0);
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(5, 10, 2);
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(10, 12, 0);

            var proxy = new CrossSectionDefinitionProxy(crossSectionDefinitionZw);

            network.SharedCrossSectionDefinitions.Add(crossSectionDefinitionZw);


            var cs1 = new CrossSection(proxy) {Name = "cs1"};
            var cs2 = new CrossSection(proxy) { Name = "cs2" };

            NetworkHelper.AddBranchFeatureToBranch(cs1,channel, 10.0);
            NetworkHelper.AddBranchFeatureToBranch(cs2, channel, 90.0);

            var exporter = new CrossSectionZWToCsvFileExporter();
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            exporter.Export(network.CrossSections, path);
            //end export

            //modify shared definition
            var nDefinitionRows = crossSectionDefinitionZw.ZWDataTable.Count;
            crossSectionDefinitionZw.ZWDataTable.AddCrossSectionZWRow(15, 15, 4);


            Assert.AreEqual(nDefinitionRows + 1, ((CrossSectionDefinitionZW)((CrossSectionDefinitionProxy) cs1.Definition).InnerDefinition).ZWDataTable.Count);
            Assert.AreEqual(nDefinitionRows + 1, ((CrossSectionDefinitionZW)((CrossSectionDefinitionProxy) cs2.Definition).InnerDefinition).ZWDataTable.Count);


            //import
            var csvFileImporter = new CrossSectionZWFromCsvFileImporter();
            csvFileImporter.ImportItem(path, network);

            Assert.AreEqual(nDefinitionRows, crossSectionDefinitionZw.ZWDataTable.Count);
            Assert.AreEqual(nDefinitionRows, ((CrossSectionDefinitionZW)((CrossSectionDefinitionProxy)cs1.Definition).InnerDefinition).ZWDataTable.Count);
            Assert.AreEqual(nDefinitionRows, ((CrossSectionDefinitionZW)((CrossSectionDefinitionProxy)cs2.Definition).InnerDefinition).ZWDataTable.Count);
        }
    }
}
